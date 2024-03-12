using System;
using Harmony;
using Il2Cpp;
using Il2CppDMM;
using Il2CppInterop.Runtime.Injection;
using Il2CppItemFiltering;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[assembly: MelonInfo(typeof(kg_LastEpoch_FilterIcons_Melon.kg_LastEpoch_FilterIcons_Melon), "kg.LastEpoch.FilterIcons", "1.3.0", "KG")]
namespace kg_LastEpoch_FilterIcons_Melon;

public class kg_LastEpoch_FilterIcons_Melon : MelonMod
{
    private static kg_LastEpoch_FilterIcons_Melon _thistype; 
    private static MelonPreferences_Category FilterIconsMod; 
    private static MelonPreferences_Entry<bool> OverrideShow;
    private static MelonPreferences_Entry<bool> AffixShowRoll;
    private static GameObject CustomMapIcon; 

    private static void CreateCustomMapIcon()
    {
        ClassInjector.RegisterTypeInIl2Cpp<CustomIconProcessor>();
        CustomMapIcon = new GameObject("kg_CustomMapIcon") { hideFlags = HideFlags.HideAndDontSave };
        CustomMapIcon.SetActive(false);
        var iconChild = new GameObject("Icon");
        iconChild.transform.SetParent(CustomMapIcon.transform);
        iconChild.transform.localPosition = Vector3.zero;
        iconChild.transform.localScale = Vector3.one;
        var itemIcon = iconChild.AddComponent<Image>();
        itemIcon.rectTransform.sizeDelta = new Vector2(24, 24);
        var backgroundIcon = CustomMapIcon.AddComponent<Image>();
        backgroundIcon.rectTransform.sizeDelta = new Vector2(24, 24);
        var canvasGroup = CustomMapIcon.AddComponent<CanvasGroup>();
        canvasGroup.ignoreParentGroups = true;
        GameObject textChild = new("Text");
        textChild.transform.SetParent(CustomMapIcon.transform);
        textChild.transform.localPosition = Vector3.zero;
        var textComponent = textChild.AddComponent<Text>();
        textComponent.fontSize = 15;
        textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComponent.alignment = TextAnchor.MiddleLeft;
        textComponent.rectTransform.anchoredPosition = new Vector2(64, 0);
        textComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
        textComponent.verticalOverflow = VerticalWrapMode.Overflow;
        var outline = textComponent.AddComponent<Outline>();
        outline.effectColor = Color.black;
        CustomMapIcon.AddComponent<CustomIconProcessor>();
    }

    public override void OnInitializeMelon()
    {
        _thistype = this;   
        FilterIconsMod = MelonPreferences.CreateCategory("kg_FilterIcons");
        OverrideShow = FilterIconsMod.CreateEntry("Show Override", false, "Show Override", "Show each filter rule on map");
        AffixShowRoll = FilterIconsMod.CreateEntry("Show Affix Roll", true, "Show Affix Roll", "Show each affix roll on item");
        FilterIconsMod.SetFilePath("UserData/kg_LastEpoch_FilterIcons.cfg", autoload: true);
        CreateCustomMapIcon();
    }
    
    private static Color GetColorForItemRarity(ItemDataUnpacked item)
    {
        if (item.isUnique()) return new Color(1f, 0.38f, 0f); 
        if (item.isSet()) return Color.green;
        if (item.isUniqueSetOrLegendary()) return Color.red;
        if (item.isExaltedItem()) return Color.magenta;
        if (item.isRare()) return Color.yellow;
        if (item.isMagicOrRare()) return Color.blue;

        return Color.white;
    }

    [HarmonyPatch(typeof(TooltipItemManager), nameof(TooltipItemManager.AffixFormatter))]
    private static class TooltipItemManager_AffixFormatter_Patch
    {
        private static void Postfix(
            ItemAffix affix,
            ref string __result)
        {
            if (affix == null || !AffixShowRoll.Value) return;
            string toInsert =  $" (<color=yellow>{Math.Round(affix.getRollFloat(), 3)}</color>)";
            int lastNewLine = __result.LastIndexOf("\n", StringComparison.Ordinal);
            if (lastNewLine == -1)
                __result += toInsert;
            else
                __result = __result.Insert(lastNewLine, toInsert);
        }
    }

    [HarmonyPatch(typeof(Rule), nameof(Rule.Match))]
    private static class Rule_Match_Patch
    {
        private static void Postfix(Rule __instance, ItemDataUnpacked data, ref bool __result)
        {
            if (!__instance.isEnabled) return;

            string ruleNameToLower = __instance.nameOverride.ToLower();
            if (string.IsNullOrWhiteSpace(ruleNameToLower)) return;
            int indexOf = ruleNameToLower.IndexOf("lpmin:", StringComparison.Ordinal);
            if (indexOf == -1) return;
            char number = ruleNameToLower[indexOf + 6];
            if (int.TryParse(number.ToString(), out int lpmin)) __result &= data.legendaryPotential >= lpmin;
        }
    }

    [HarmonyPatch(typeof(GroundItemVisuals), nameof(GroundItemVisuals.initialise), typeof(ItemDataUnpacked), typeof(uint), typeof(GroundItemLabel), typeof(GroundItemRarityVisuals), typeof(bool))]
    private static class GroundItemVisuals_initialise_Patch2
    {
        private static void Postfix(GroundItemVisuals __instance, ItemDataUnpacked itemData, GroundItemLabel label)
        {
            var filter = ItemFilterManager.Instance.Filter;
            if (filter != null)
            {
                foreach (Rule rule in filter.rules)
                {
                    if (!rule.isEnabled || rule.type == Rule.RuleOutcome.HIDE) continue;
                    if (!rule.nameOverride.Contains("@show") && !OverrideShow.Value) continue;
                    if (rule.Match(itemData))
                    {
                        var customMapIcon = Object.Instantiate(CustomMapIcon, DMMap.Instance.iconContainer.transform);
                        customMapIcon.SetActive(true);
                        customMapIcon.GetComponent<CustomIconProcessor>().Init(__instance.gameObject, label);
                        string path = ItemList.instance.GetBaseTypeName(itemData.itemType).Replace(" ", "_").ToLower();
                        string itemName = itemData.BaseNameForTooltipSprite;
                        if (itemData.isUniqueSetOrLegendary())
                        {
                            customMapIcon.GetComponent<CustomIconProcessor>().ShowLegendaryPotential(itemData.legendaryPotential, itemData.weaversWill);
                            if (UniqueList.instance.uniques.Count > itemData.uniqueID && UniqueList.instance.uniques.get(itemData.uniqueID) is { } entry)
                            {
                                path = "uniques";
                                itemName = entry.name.Replace(" ", "_");
                            }
                        }

                        var icon = Resources.Load<Sprite>($"gear/{path}/{itemName}");
                        customMapIcon.GetComponent<Image>().sprite = ItemList.instance.defaultItemBackgroundSprite;
                        customMapIcon.GetComponent<Image>().color = GetColorForItemRarity(itemData);
                        customMapIcon.transform.GetChild(0).GetComponent<Image>().sprite = icon;
                        return;
                    }
                }
            }
        }
    }

    private class CustomIconProcessor : MonoBehaviour
    {
        public GameObject _trackable;
        private Text _text;
        private RectTransform thisTransform;
        private GroundItemLabel _label;

        private void Awake()
        {
            _text = transform.GetChild(1).GetComponent<Text>();
        }

        public void Init(GameObject toTrack, GroundItemLabel label)
        {
            thisTransform = transform.GetComponent<RectTransform>();
            transform.localPosition = DMMap.Instance.WorldtoUI(toTrack.transform.position);
            _trackable = toTrack;
            _label = label;
        }

        public void ShowLegendaryPotential(int lp, int ww)
        {
            if (lp > 0)
            {
                _text.text += $"{lp}";
                _text.color = new Color(1f, 0.5f, 0f);
            }
            else if (ww > 0)
            {
                _text.text += $"{ww}";
                _text.color = new Color(1f, 0.05f, 0.77f);
            }
        }

        private static CustomIconProcessor showingAffix;

        private void PointerEnter()
        {
            if (_label != null && _label && _label.tooltipItem) _label.tooltipItem.OnPointerEnter(null);
        }

        private void PointerExit()
        {
            if (_label != null && _label && _label.tooltipItem) _label.tooltipItem.OnPointerExit(null);
        }

        private void FixedUpdate()
        {
            if (!_trackable || !_trackable.activeSelf)
            {
                Destroy(gameObject);
                return;
            }

            if (showingAffix == this)
            {
                bool isMouseInside = RectTransformUtility.RectangleContainsScreenPoint(thisTransform, Input.mousePosition);
                if (!isMouseInside || !Input.GetKey(KeyCode.LeftShift))
                {
                    showingAffix = null;
                    PointerExit();
                }
            }

            if ((!showingAffix || showingAffix == this) && Input.GetKey(KeyCode.LeftShift))
            {
                bool isMouseInside = RectTransformUtility.RectangleContainsScreenPoint(thisTransform, Input.mousePosition);
                if (isMouseInside) 
                {
                    showingAffix = this;
                    PointerEnter();
                }
            }

            transform.localPosition = DMMap.Instance.WorldtoUI(_trackable.transform.position);
        }
    }

    [HarmonyPatch(typeof(SettingsPanelTabNavigable), nameof(SettingsPanelTabNavigable.Awake))]
    private static class SettingsPanelTabNavigable_Awake_Patch
    {
        private static void Postfix(SettingsPanelTabNavigable __instance) 
        {
            __instance.CreateNewOption("<color=green>Map Filter Show Override</color>", OverrideShow, (tf) =>
            {
                OverrideShow.Value = tf;
                FilterIconsMod.SaveToFile();
            });
            __instance.CreateNewOption("<color=green>Affix Show Roll</color>", AffixShowRoll, (tf) =>
            {
                AffixShowRoll.Value = tf;
                FilterIconsMod.SaveToFile();
            });
        }
        
    }
}


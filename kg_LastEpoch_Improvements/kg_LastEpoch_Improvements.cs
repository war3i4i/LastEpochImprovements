#define FOGCAMERAVERSION 
using Il2CppDMM;
using Il2CppInterop.Common;
using Il2CppInterop.Runtime.Injection;
using Il2CppItemFiltering;
using Il2CppLE.Telemetry;
using Il2CppLE.UI;
using Il2CppSystem.Net;
using Il2CppTMPro;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

[assembly: MelonInfo(typeof(kg_LastEpoch_Improvements.kg_LastEpoch_Improvements), "kg.LastEpoch.Improvements", "1.3.7", "KG", "https://www.nexusmods.com/lastepoch/mods/8")]

namespace kg_LastEpoch_Improvements;

public class kg_LastEpoch_Improvements : MelonMod
{
    private static kg_LastEpoch_Improvements _thistype; 
    private static MelonPreferences_Category ImprovementsModCategory; 
    private static MelonPreferences_Entry<bool> ShowAll;
    private static MelonPreferences_Entry<DisplayAffixType> AffixShowRoll;
    public static MelonPreferences_Entry<DisplayAffixType_GroundLabel> ShowAffixOnLabel; 
#if FOGCAMERAVERSION
    private static MelonPreferences_Entry<bool> FogOfWar; 
    private static MelonPreferences_Entry<bool> EnhancedCamera; 
#endif
    private static MelonPreferences_Entry<bool> AutoStoreCraftMaterials;
    private static GameObject CustomMapIcon;

    private enum DisplayAffixType { None, Old_Style, New_Style, Letter_Style };
    public enum DisplayAffixType_GroundLabel { None, Without_Tier, Without_Tier_Filter_Only, With_Tier, With_Tier_Filter_Only, Letter_Without_Tier, Letter_Without_Tier_Filter_Only, Letter_With_Tier, Letter_With_Tier_Filter_Only }
    private void CreateCustomMapIcon()
    {
        ClassInjector.RegisterTypeInIl2Cpp<CustomIconProcessor>();
        CustomMapIcon = new GameObject("kg_CustomMapIcon") { hideFlags = HideFlags.HideAndDontSave };
        CustomMapIcon.SetActive(false);
        GameObject iconChild = new GameObject("Icon");
        iconChild.transform.SetParent(CustomMapIcon.transform);
        iconChild.transform.localPosition = Vector3.zero;
        iconChild.transform.localScale = Vector3.one;
        Image itemIcon = iconChild.AddComponent<Image>();
        itemIcon.rectTransform.sizeDelta = new Vector2(24, 24);
        Image backgroundIcon = CustomMapIcon.AddComponent<Image>();
        backgroundIcon.rectTransform.sizeDelta = new Vector2(24, 24);
        CanvasGroup canvasGroup = CustomMapIcon.AddComponent<CanvasGroup>();
        canvasGroup.ignoreParentGroups = true;
        GameObject textChild = new("Text");
        textChild.transform.SetParent(CustomMapIcon.transform);
        textChild.transform.localPosition = Vector3.zero;
        Text textComponent = textChild.AddComponent<Text>();
        textComponent.fontSize = 15;
        textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComponent.alignment = TextAnchor.MiddleLeft;
        textComponent.rectTransform.anchoredPosition = new Vector2(64, 0);
        textComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
        textComponent.verticalOverflow = VerticalWrapMode.Overflow;
        Outline outline = textComponent.AddComponent<Outline>(); 
        outline.effectColor = Color.black;
        CustomMapIcon.AddComponent<CustomIconProcessor>();
    }
    
    public override void OnInitializeMelon()
    { 
        _thistype = this;
        ImprovementsModCategory = MelonPreferences.CreateCategory("kg_Improvements");
        ShowAll = ImprovementsModCategory.CreateEntry("Show Override", false, "Show Override", "Show each filter rule on map");
        AffixShowRoll = ImprovementsModCategory.CreateEntry("Show Affix Roll New", DisplayAffixType.None, "Show Affix Roll New", "Show each affix roll on item");
        ShowAffixOnLabel = ImprovementsModCategory.CreateEntry("Show Affix On Label", DisplayAffixType_GroundLabel.None, "Show Affix On Label Type", "Show each affix roll on item label (ground)");
        AutoStoreCraftMaterials = ImprovementsModCategory.CreateEntry("AutoStoreCraftMaterials", false, "Auto storage craft materials", "Automatic storage of craft materials from the inventory");
#if FOGCAMERAVERSION
        FogOfWar = ImprovementsModCategory.CreateEntry("Fog of war", false, "Clear fog on map on start", "Clear fog of war when you 1th enter on map");
        EnhancedCamera = ImprovementsModCategory.CreateEntry("Enhanced Camera", false, "Enhanced camera", "Enhanced camera angles and zoom");
#endif
        ImprovementsModCategory.SetFilePath("UserData/kg_LastEpoch_Improvements.cfg", autoload: true);
        CreateCustomMapIcon();
        CustomDropSounds.Load();
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
        private static void Postfix(ItemDataUnpacked item, ItemAffix affix, ref string __result)
        {
            if (item == null || affix == null || AffixShowRoll.Value is DisplayAffixType.None) return;
            __result = AffixShowRoll.Value switch
            {
                DisplayAffixType.Old_Style => __result.Style1_AffixRoll(affix),
                DisplayAffixType.New_Style => __result.Style2_AffixRoll(affix), 
                DisplayAffixType.Letter_Style => __result.Letter_Style_AffixRoll(affix), 
                _ => __result 
            };
        }
    }  
 
    [HarmonyPatch(typeof(TooltipItemManager), nameof(TooltipItemManager.UniqueBasicModFormatter))]
    private static class TooltipItemManager_FormatUniqueModAffixString_Patch 
    {
        private static void Postfix(ItemDataUnpacked item, ref string __result, int uniqueModIndex, float modifierValue)
        {
            if (item == null || AffixShowRoll.Value is DisplayAffixType.None || item.isSet()) return;
            __result = AffixShowRoll.Value switch
            {
                DisplayAffixType.Old_Style => __result.Style1_AffixRoll_Unique(item, uniqueModIndex, modifierValue),
                DisplayAffixType.New_Style => __result.Style2_AffixRoll_Unique(item, uniqueModIndex, modifierValue),
                DisplayAffixType.Letter_Style => __result.Letter_Style_AffixRoll_Unique(item, uniqueModIndex, modifierValue),
                _ => __result
            };
        }
    }
    
    [HarmonyPatch(typeof(TooltipItemManager),nameof(TooltipItemManager.ImplicitFormatter))]
    private static class TooltipItemManager_FormatMod_Patch
    {
        private static void Postfix(ItemDataUnpacked item, int implicitNumber, ref string __result, bool isComparsionItem)
        {
            if (item == null || AffixShowRoll.Value is DisplayAffixType.None || item.isSet()) return;
            __result = AffixShowRoll.Value switch
            {
                DisplayAffixType.Old_Style => __result.Style1_Implicit(item, implicitNumber),
                DisplayAffixType.New_Style => __result.Style2_Implicit(item, implicitNumber),
                DisplayAffixType.Letter_Style => __result.Letter_Style_Implicit(item, implicitNumber),
                _ => __result
            }; 
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
    private static class GroundItemVisuals_initialise_Patch
    {
        private static bool ShouldShow(Rule rule)
        {
            if (!rule.isEnabled || rule.type is Rule.RuleOutcome.HIDE) return false;
            if (ShowAll.Value) return true;
            return rule.emphasized;
        }

        private static void Prefix(GroundItemVisuals __instance, ItemDataUnpacked itemData, GroundItemLabel label, GroundItemRarityVisuals groundItemRarityVisuals)
        {
            ItemFilter filter = ItemFilterManager.Instance.Filter;
            if (filter == null) return;
            foreach (Rule rule in filter.rules)
            {
                if (!ShouldShow(rule)) continue;
                 
                if (rule.Match(itemData) || itemData.rarity == 9) 
                {
                    if (CustomDropSounds.RuleHasCustomSound(rule, out string sName))
                    {
                        PlayOneShotSound oneShotComp = groundItemRarityVisuals?.GetComponent<PlayOneShotSound>();
                        bool customSound = CustomDropSounds.TryPlaySoundDelayed(sName, oneShotComp?.delayDuration ?? 0f, 1f);
                        if (oneShotComp && customSound) oneShotComp.StopAllCoroutines();
                    }
                    
                    GameObject customMapIcon = Object.Instantiate(CustomMapIcon, DMMap.Instance.iconContainer.transform);
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

                    Sprite icon = Resources.Load<Sprite>($"gear/{path}/{itemName}");
                    customMapIcon.GetComponent<Image>().sprite = ItemList.instance.defaultItemBackgroundSprite;
                    customMapIcon.GetComponent<Image>().color = GetColorForItemRarity(itemData);
                    customMapIcon.transform.GetChild(0).GetComponent<Image>().sprite = icon;
                    
                    return;
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

            transform.localPosition = DMMap.Instance.WorldtoUI(_trackable.transform.position);
            
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
        }
    } 
    
    [HarmonyPatch(typeof(SettingsPanelTabNavigable), nameof(SettingsPanelTabNavigable.Awake))]
    private static class SettingsPanelTabNavigable_Awake_Patch
    {
        private static void Postfix(SettingsPanelTabNavigable __instance)
        {
            const string CategoryName = "KG Improvements";
#if FOGCAMERAVERSION
            __instance.CreateNewOption_Toggle(CategoryName, "<color=green>Clear fog on map on start</color>", FogOfWar, (tf) =>
            {
                FogOfWar.Value = tf;
                ImprovementsModCategory.SaveToFile();
            }); 
            __instance.CreateNewOption_Toggle(CategoryName, "<color=green>Enhanced camera</color>", EnhancedCamera, (tf) =>
            {
                EnhancedCamera.Value = tf;
                ImprovementsModCategory.SaveToFile(); 
                CameraManager_Start_Patch.Switch();  
            }); 
#endif
            __instance.CreateNewOption_EnumDropdown(CategoryName, "<color=green>Affix Show Roll (Tooltip)</color>", "Show affix roll on tooltip text", AffixShowRoll, (i) =>
            {
                AffixShowRoll.Value = (DisplayAffixType)i;
                ImprovementsModCategory.SaveToFile();
            });
            __instance.CreateNewOption_EnumDropdown(CategoryName, "<color=green>Affix Show Roll (Ground)</color>", "Show affix roll on ground text", ShowAffixOnLabel, (i) =>
            {
                ShowAffixOnLabel.Value = (DisplayAffixType_GroundLabel)i;
                ImprovementsModCategory.SaveToFile();
            });
            __instance.CreateNewOption_Toggle(CategoryName, "<color=green>Map Filter Show All</color>", ShowAll, (tf) =>
            {
                ShowAll.Value = tf;
                ImprovementsModCategory.SaveToFile();
            });
            __instance.CreateNewOption_Toggle(CategoryName, "<color=green>Auto storage craft materials</color>", AutoStoreCraftMaterials, (ascm) =>
            {
                AutoStoreCraftMaterials.Value = ascm;
                ImprovementsModCategory.SaveToFile();
            });
        }
    }

#if FOGCAMERAVERSION
    [HarmonyPatch(typeof(MinimapFogOfWar), nameof(MinimapFogOfWar.Initialize))]
    private static class MinimapFogOfWar_Initialize_Patch
    {
        private const float fullDiscovery = 10000f;
        private static void Prefix(MinimapFogOfWar __instance, out float __state) 
        {
            __state = __instance.discoveryDistance;
            if (FogOfWar.Value) __instance.discoveryDistance = fullDiscovery;
        }
        private static void Postfix(MinimapFogOfWar __instance, float __state) => __instance.discoveryDistance = __state;
    }
    
    [HarmonyPatch(typeof(CameraManager),nameof(CameraManager.Start))] 
    private static class CameraManager_Start_Patch
    {
        private static float LE_cameraAngleDefault;
        private static float LE_cameraAngleMax;
        private static float LE_cameraAngleMin;
        private static float LE_zoomMin;
        
        private const float newAngles = 55f;
        private const float newZoomMin = -40f;
        
        public static void Switch()
        {
            if (!CameraManager.instance) return;
            if (EnhancedCamera.Value)
            {
                CameraManager.instance.cameraAngleDefault = newAngles;
                CameraManager.instance.cameraAngleMax = newAngles;
                CameraManager.instance.cameraAngleMin = newAngles;
                CameraManager.instance.zoomMin = newZoomMin;
            }
            else
            {
                CameraManager.instance.cameraAngleDefault = LE_cameraAngleDefault;
                CameraManager.instance.cameraAngleMax = LE_cameraAngleMax;
                CameraManager.instance.cameraAngleMin = LE_cameraAngleMin;
                CameraManager.instance.zoomMin = LE_zoomMin;
            }
        }
        private static void Postfix(CameraManager __instance)
        {
            LE_cameraAngleDefault = __instance.cameraAngleDefault;
            LE_cameraAngleMax = __instance.cameraAngleMax;
            LE_cameraAngleMin = __instance.cameraAngleMin;
            LE_zoomMin = __instance.zoomMin;
            Switch();
        }
    }
#endif 

    
    //OpenInventoryPanel invokes at loadingscreen after InventoryPanelUI.Awake, we cannot call StoreMaterialsButtonPress() at this moment
    //cause it throws an exception (probably some stuff didn't load yet)
    [HarmonyPatch(typeof(InventoryPanelUI),nameof(InventoryPanelUI.Awake))]
    private static class InventoryPanelUI_Awake_Patch
    {
        public static int AwakeFrame;
        private static void Postfix(InventoryPanelUI __instance) => AwakeFrame = Time.frameCount;
    }
    [HarmonyPatch(typeof(InventoryPanelUI), nameof(InventoryPanelUI.OpenInventoryPanel))]
    private static class InventoryPanelUI_OpenInventoryPanel_Patch
    {
        private static void Postfix(InventoryPanelUI __instance)
        {
            if (AutoStoreCraftMaterials.Value && Time.frameCount != InventoryPanelUI_Awake_Patch.AwakeFrame)
                __instance.StoreMaterialsButtonPress();
        }
    }

}

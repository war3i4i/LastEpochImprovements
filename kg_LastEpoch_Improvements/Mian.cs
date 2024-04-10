using System.ComponentModel;
using Il2CppDMM;
using Il2CppInterop.Runtime.Injection;
using Il2CppItemFiltering;
using MelonLoader;
using Object = UnityEngine.Object;


[assembly: MelonInfo(typeof(kg_LastEpoch_Improvements.Kg_LastEpoch_Improvements), "kg.LastEpoch.Improvements", "1.3.5", "KG", "https://www.nexusmods.com/lastepoch/mods/8")]

namespace kg_LastEpoch_Improvements;

public class Kg_LastEpoch_Improvements : MelonMod
{
    private static Kg_LastEpoch_Improvements _thistype;
    private static MelonPreferences_Category ImprovementsModCategory;
    private static MelonPreferences_Entry<bool> ShowOverride;
    private static MelonPreferences_Entry<DisplayAffixType> ShowAffixRollNew;
    public static MelonPreferences_Entry<DisplayAffixType_GroundLabel> ShowAffixOnLabel;
    public static MelonPreferences_Entry<bool> ShowAffixPSOnLabel;
    public static MelonPreferences_Entry<bool> AutoClickOnline;
    private static MelonPreferences_Entry<bool> AutoStoreCraftMaterials;

#if CHEATVERSION
    private static MelonPreferences_Entry<bool> WaypointUnlock;
    private static MelonPreferences_Entry<bool> CheatEnhancedCamera;
    private static MelonPreferences_Entry<bool> CheatFogOfWar;
#endif

    private static GameObject CustomMapIcon;
    public enum ROLL_INVALID { NO_ROLL = -1, NO_RANGE = 255 };

    private enum DisplayAffixType
    {
        [Description("关闭")] None,
        [Description("老旧样式")] Style_1,
        [Description("T级 + ROLL值百分比")] Style_2,
        [Description("T级评分 + ROLL值百分比 ")] Style_3,
        [Description("超详细模式")] Style_4
    };

    public enum DisplayAffixType_GroundLabel
    {
        [Description("关闭")] None,
        [Description("只显示T级")] Ontier,
        [Description("ROLL值百分比")] Without_Tier,
        [Description("T级 + ROLL值百分比")] With_Tier,
        [Description("ROLL值评分")] Letter_Without_Tier,
        [Description("T级 + ROLL值评分")] Letter_With_Tier

    }
    private static void CreateCustomMapIcon()
    {
        ClassInjector.RegisterTypeInIl2Cpp<CustomIconProcessor>();
        CustomMapIcon = new GameObject("kg_CustomMapIcon") { hideFlags = HideFlags.HideAndDontSave };
        CustomMapIcon.SetActive(false);
        GameObject iconChild = new("Icon");
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
        ShowOverride = ImprovementsModCategory.CreateEntry("ShowOverride", true, "Show Override", "Show each filter rule on map");
        ShowAffixRollNew = ImprovementsModCategory.CreateEntry("ShowAffixRollNew", DisplayAffixType.None, "Show Affix Roll New", "Show each affix roll on item");
        ShowAffixOnLabel = ImprovementsModCategory.CreateEntry("ShowAffixOnLabel", DisplayAffixType_GroundLabel.None, "Show Affix On Label Type", "Show each affix roll on item label (ground)");
        ShowAffixPSOnLabel = ImprovementsModCategory.CreateEntry("ShowAffixPSOnLabel", false, "Show Affix On Label Type", "Show each affix roll on item label (ground)");

        AutoStoreCraftMaterials = ImprovementsModCategory.CreateEntry("AutoStoreCraftMaterials", true, "Auto storage craft materials", "Automatic storage of craft materials from the inventory");
        AutoClickOnline = ImprovementsModCategory.CreateEntry("AutoClickOnline", true, "Auto Click Online", "Automatic click on online");
#if CHEATVERSION
        CheatFogOfWar = ImprovementsModCategory.CreateEntry("CheatFogOfWar", false, "Clear fog on map on start", "Clear fog of war when you first enter on map");
        WaypointUnlock = ImprovementsModCategory.CreateEntry("WaypointUnlock", false, "Unlock Portal", "Unlock Portal");
        CheatEnhancedCamera = ImprovementsModCategory.CreateEntry("CheatEnhancedCamera", false, "Enhanced camera", "Enhanced camera angles and zoom");
#endif



        ImprovementsModCategory.SetFilePath("UserData/kg_LastEpoch_Improvements.cfg", autoload: true);
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





    [HarmonyPatch(typeof(TooltipItemManager), "AffixFormatter")]
    public class AltTextTypePatch
    {
        [HarmonyPrefix]
        public static void Prefix(ref byte altTextType)
        {
            if (ShowAffixRollNew.Value is DisplayAffixType.Style_4)
            {
                bool altPressed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
                bool bothPressed = altPressed && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));

                if (!altPressed && !bothPressed)
                {
                    altTextType = 2;
                }
            }
        }
    }

    [HarmonyPatch(typeof(TooltipItemManager), nameof(TooltipItemManager.AffixFormatter))]
    private static class TooltipItemManager_AffixFormatter_Patch
    {
        private static void Postfix(ref string __result, ItemDataUnpacked item, ItemAffix affix, ref float modifierValue)

        {
            if (item == null || affix == null || ShowAffixRollNew.Value is DisplayAffixType.None) return;
            double trueRoll = -1;
            int display_value = -1;
            int display_minRoll_value = -1;
            int display_maxRoll_value = -1;
            float roll = -1;
            float minRoll = -1;
            float maxRoll = -1;
            float affixEffectModifier = -1;
            Affixfix.GetAffixRollValues(item, affix, modifierValue, ref trueRoll, ref display_value, ref display_minRoll_value, ref display_maxRoll_value, ref roll, ref minRoll, ref maxRoll, ref affixEffectModifier);
            __result = ShowAffixRollNew.Value switch
            {
                DisplayAffixType.Style_1 => __result.Style1_AffixRoll(trueRoll),
                DisplayAffixType.Style_2 => __result.Style2_AffixRoll(affix, trueRoll),
                DisplayAffixType.Style_3 => __result.Style3_AffixRoll(affix, trueRoll),
                _ => __result
            };
        }
    }

    [HarmonyPatch(typeof(TooltipItemManager), nameof(TooltipItemManager.UniqueBasicModFormatter))]
    private static class TooltipItemManager_FormatUniqueModAffixString_Patch
    {
        private static void Postfix(ItemDataUnpacked item, ref string __result, int uniqueModIndex, float modifierValue, SP modProperty)
        {
            if (item == null || ShowAffixRollNew.Value is DisplayAffixType.None || item.isSet()) return;
            __result = ShowAffixRollNew.Value switch
            {
                DisplayAffixType.Style_1 => __result.Style1_AffixRoll_Unique(item, uniqueModIndex, modifierValue, modProperty),
                DisplayAffixType.Style_2 => __result.Style2_AffixRoll_Unique(item, uniqueModIndex, modifierValue, modProperty),
                DisplayAffixType.Style_3 => __result.Style3_AffixRoll_Unique(item, uniqueModIndex, modifierValue, modProperty),
                _ => __result
            };
        }
    }

    [HarmonyPatch(typeof(TooltipItemManager), nameof(TooltipItemManager.ImplicitFormatter))]
    private static class TooltipItemManager_FormatMod_Patch
    {
        private static void Postfix(ItemDataUnpacked item, int implicitNumber, ref string __result, bool isComparsionItem, float modifierValue)
        {
            if (item == null || ShowAffixRollNew.Value is DisplayAffixType.None || item.isSet()) return;
            ItemDataUnpacked itemToUse = item;
            if (isComparsionItem)
            {
                if (TooltipItemManager.instance.equipedItem == null) return;
                itemToUse = TooltipItemManager.instance.equipedItem;
            }
            double trueRoll = -1;
            int display_value = -1;
            int display_minRoll_value = -1;
            int display_maxRoll_value = -1;
            float roll = -1;
            float minRoll = -1;
            float maxRoll = -1;
            Affixfix.GetImplicitRollValues(itemToUse, implicitNumber, modifierValue, ref trueRoll, ref display_value, ref display_minRoll_value, ref display_maxRoll_value, ref roll, ref minRoll, ref maxRoll);
            __result = ShowAffixRollNew.Value switch
            {
                DisplayAffixType.Style_1 => __result.Style1_AffixRoll_Implicit(trueRoll),
                DisplayAffixType.Style_2 => __result.Style2_AffixRoll_Implicit(trueRoll),
                DisplayAffixType.Style_3 => __result.Style3_AffixRoll_Implicit(trueRoll),
                _ => __result
            };
        }
    }



    [HarmonyPatch(typeof(GroundItemVisuals), nameof(GroundItemVisuals.initialise), typeof(ItemDataUnpacked), typeof(uint), typeof(GroundItemLabel), typeof(GroundItemRarityVisuals), typeof(bool))]
    private static class GroundItemVisuals_initialise_Patch2
    {
        private static bool ShouldShow(Rule rule)
        {
            if (!rule.isEnabled || rule.type is Rule.RuleOutcome.HIDE) return false;
            if (ShowOverride.Value) return true;
            return false;
        }

        private static void Postfix(GroundItemVisuals __instance, ItemDataUnpacked itemData, GroundItemLabel label)
        {
            ItemFilter filter = ItemFilterManager.Instance.Filter;
            if (filter == null || !ShowOverride.Value) return;
            foreach (Rule rule in filter.rules)
            {
                if (!ShouldShow(rule)) continue;
                if (rule.Match(itemData))
                {
                    GameObject customMapIcon = Object.Instantiate(CustomMapIcon, DMMap.Instance.iconContainer.transform);
                    customMapIcon.SetActive(true);
                    customMapIcon.GetComponent<CustomIconProcessor>().Init(__instance.gameObject, label);
                    string path = ItemList.instance.GetBaseTypeName(itemData.itemType).Replace(" ", "_").ToLower();
                    string itemName = itemData.BaseNameForTooltipSprite;
                    if (itemData.isUniqueSetOrLegendary())
                    {
                        customMapIcon.GetComponent<CustomIconProcessor>().ShowLegendaryPotential(itemData.legendaryPotential, itemData.weaversWill);
                        if (UniqueList.instance.uniques.Count > itemData.uniqueID && UniqueList.instance.uniques.Get(itemData.uniqueID) is { } entry)

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
                _text.color = new Color(0f, 1f, 0f);
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
                if (!isMouseInside)
                {
                    showingAffix = null;
                    PointerExit();
                }
            }

            if (!showingAffix || showingAffix == this)
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

#if CHEATVERSION

            __instance.CreateNewOption(CategoryName, "<color=red>[警告] 万能传送点</color>\n 所有未解锁图标允许传送(小号按C)",WaypointUnlock, (tf) =>
            {
                WaypointUnlock.Value = tf;
                ImprovementsModCategory.SaveToFile();
            });
            __instance.CreateNewOption(CategoryName, "<color=red>[慎用] 小地图全开</color> \n 打开后切图", CheatFogOfWar, (tf) =>
            {
                CheatFogOfWar.Value = tf;
                ImprovementsModCategory.SaveToFile();
            });
            __instance.CreateNewOption(CategoryName, "<color=red>[慎用] 超大视距</color>\n 真的很大", CheatEnhancedCamera, (tf) =>
            {
                CheatEnhancedCamera.Value = tf;
                ImprovementsModCategory.SaveToFile();
                CameraManager_Start_Patch.Switch();
            });
#endif
            __instance.CreateNewOption_EnumDropdown(CategoryName, "<color=green>显示风格(背包)</color>", "背包物品样式", ShowAffixRollNew, (i) =>
            {
                ShowAffixRollNew.Value = (DisplayAffixType)i;
                ImprovementsModCategory.SaveToFile();
            });
            __instance.CreateNewOption_EnumDropdown(CategoryName, "<color=green>显示风格(地面)</color>", "地面物品样式", ShowAffixOnLabel, (i) =>
            {
                ShowAffixOnLabel.Value = (DisplayAffixType_GroundLabel)i;
                ImprovementsModCategory.SaveToFile();
            });
            __instance.CreateNewOption(CategoryName, "<color=green>显示风格(地面) 区分前后缀</color>", ShowAffixPSOnLabel, (tf) =>
           {
               ShowAffixPSOnLabel.Value = tf;
               ImprovementsModCategory.SaveToFile();
           });
            __instance.CreateNewOption(CategoryName, "<color=green>地图显示装备图标</color>\n 过滤规则中启用了的装备图标", ShowOverride, (tf) =>
            {
                ShowOverride.Value = tf;
                ImprovementsModCategory.SaveToFile();
            });
            __instance.CreateNewOption(CategoryName, "<color=green>自动转移工艺材料</color>\n 打开背包时自动转移", AutoStoreCraftMaterials, (tf) =>
            {
                AutoStoreCraftMaterials.Value = tf;
                ImprovementsModCategory.SaveToFile();
            });
            __instance.CreateNewOption(CategoryName, "<color=green>自动登录</color>\n 打开游戏自动进在线模式", AutoClickOnline, (tf) =>
            {
                AutoClickOnline.Value = tf;
                ImprovementsModCategory.SaveToFile();
            });
        }
    }




#if CHEATVERSION

    //视距
    [HarmonyPatch(typeof(MinimapFogOfWar), nameof(MinimapFogOfWar.Initialize))]
    private static class MinimapFogOfWar_Initialize_Patch
    {
        private const float cheatDiscovery = 10000f;
        private static void Prefix(MinimapFogOfWar __instance, out float __state)
        {
            __state = __instance.discoveryDistance;
            if (CheatFogOfWar.Value) __instance.discoveryDistance = cheatDiscovery;
        }
        private static void Postfix(MinimapFogOfWar __instance, float __state) => __instance.discoveryDistance = __state;
    }

    [HarmonyPatch(typeof(CameraManager), nameof(CameraManager.Start))]
    private static class CameraManager_Start_Patch
    {
        private static float LE_cameraAngleDefault;
        private static float LE_cameraAngleMax;
        private static float LE_cameraAngleMin;
        private static float LE_zoomMin;

        private const float cheatAngles = 60f;
        private const float cheatZoomMin = -80f;

        public static void Switch()
        {
            if (!CameraManager.instance) return;
            if (CheatEnhancedCamera.Value)
            {
                CameraManager.instance.cameraAngleDefault = cheatAngles;
                CameraManager.instance.cameraAngleMax = cheatAngles;
                CameraManager.instance.cameraAngleMin = cheatAngles;
                CameraManager.instance.zoomMin = cheatZoomMin;
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

    // 地图传送点
    [HarmonyPatch(typeof(UIWaypointStandard), "OnPointerEnter")]
    public class UIWaypointStandard_OnPointerEnter
    {
        [HarmonyPrefix]
        static void Prefix(UIWaypointStandard __instance, UnityEngine.EventSystems.PointerEventData __0)
        {
            if (WaypointUnlock.Value)
            {
                __instance.isActive = true;
                __instance.noWaypointInScene = false;
            }
        }
    }

    //解锁职业
    public override void OnUpdate()
    {
        base.OnUpdate();
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (!IsMastered() && WaypointUnlock.Value)
            {
                Choose();
            }
        }
    }

    public static bool IsMastered()
    {
        bool result = false;
        try
        {
            if (PlayerFinder.getLocalTreeData().chosenMastery > 0) { result = true; }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error in IsMastered(): " + ex.Message);
        }
        return result;
    }

    public static void Choose()
    {
        UIBase.instance.openMasteryPanel(true);
    }

#endif



    // 自动转移工艺材料
    [HarmonyPatch(typeof(InventoryPanelUI), "OnEnable")]
    public class InventoryPanelUI_OnEnable
    {
        [HarmonyPostfix]
        static void Postfix(ref InventoryPanelUI __instance)
        {
            if (AutoStoreCraftMaterials.Value)
            {
                __instance.StoreMaterialsButtonPress();
            }

        }
    }
}








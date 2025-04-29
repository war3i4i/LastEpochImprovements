﻿using System.Reflection;
using Il2CppLE.Data;
using Il2CppLE.Services.Bazaar;
using Il2CppLE.UI.Controls;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine.Localization.Components;
using AccessTools = HarmonyLib.AccessTools;

namespace kg_LastEpoch_Improvements;

public static class Utils
{
    //idk why but for some weird reason IL2CPP libs doesn't allow me to use indexer in List<>. So this is a reflection workaround
    private static readonly Dictionary<Type, MethodInfo> _cachedMethods = new Dictionary<Type, MethodInfo>();
    public static T get<T>(this Il2CppSystem.Collections.Generic.List<T> list, int index)
    {
        Type type = typeof(T);
        if (_cachedMethods.TryGetValue(type, out MethodInfo method)) return (T)method.Invoke(list, new object[] { index }); 
        method = AccessTools.Method(typeof(Il2CppSystem.Collections.Generic.List<T>), "get_Item", new[] { typeof(int) });
        _cachedMethods[type] = method;
        return (T)method.Invoke(list, new object[] { index });
    }
    public static void set<T>(this Il2CppSystem.Collections.Generic.List<T> list, int index, T value)
    {
        Type type = typeof(T);
        if (!_cachedMethods.TryGetValue(type, out MethodInfo method))
        {
            method = AccessTools.Method(typeof(Il2CppSystem.Collections.Generic.List<T>), "set_Item", new[] { typeof(int), typeof(T) });
            _cachedMethods[type] = method;
        }
        method.Invoke(list, new object[] { index, value });
    }
    //adds an option to vanilla last epoch UI settings
    private static int CreateCategoryIfNeeded(SettingsPanelTabNavigable settings, string Category)
    {
        Transform findExisting = settings.transform.GetChild(0).GetChild(0).Find($"ModsCategory - {Category}");
        if (findExisting) return findExisting.GetSiblingIndex();
        Transform headerInterface = settings.transform.GetChild(0).GetChild(0).Find("Header-Interface");
        if (!headerInterface) return 0;
        Transform newCategory = UnityEngine.Object.Instantiate(headerInterface, headerInterface.parent);
        newCategory.name = $"ModsCategory - {Category}";
        newCategory.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = Category;
        newCategory.GetChild(0).GetChild(0).GetComponent<TMP_Text>().color = Color.green;
        UnityEngine.Object.DestroyImmediate(newCategory.GetChild(0).GetChild(0).GetComponent<LocalizeStringEvent>());
        newCategory.transform.SetSiblingIndex(headerInterface.GetSiblingIndex());
        return newCategory.GetSiblingIndex();
    }
    public static void CreateNewOption_Toggle(this SettingsPanelTabNavigable settings, string Category, string Name, MelonPreferences_Entry<bool> option, Action<bool> a)
    {
        Transform optionsTransform = settings.transform.GetChild(0).GetChild(0).Find("Option - Minion Health Bars");
        if (!optionsTransform) return;
        int orderIndex = CreateCategoryIfNeeded(settings, Category);
        Transform newButton = UnityEngine.Object.Instantiate(optionsTransform, optionsTransform.parent);
        newButton.name = Name;
        newButton.SetSiblingIndex(orderIndex + 1); 
        Toggle toggle = newButton.GetChild(0).GetComponent<Toggle>();
        toggle.isOn = option.Value;
        toggle.onValueChanged.AddListener(new Action<bool>(_ => a(toggle.isOn)));
        UnityEngine.Object.DestroyImmediate(newButton.GetChild(1).GetChild(0).GetComponent<LocalizeStringEvent>());
        newButton.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = Name; 
    }
    public static GameObject CopyFrom_Dropdown;
    public static void CreateNewOption_EnumDropdown<T>(this SettingsPanelTabNavigable settings, string Category, string Name, string Description, MelonPreferences_Entry<T> option, Action<int> a) where T : Enum
    {
        Transform optionsTransform = settings.transform.GetChild(0).GetChild(0).Find("Dropdown - Language Selection");
        if (!optionsTransform) return;
        int orderIndex = CreateCategoryIfNeeded(settings, Category);
        Transform newDropdown = UnityEngine.Object.Instantiate(optionsTransform, optionsTransform.parent);
        UnityEngine.Object.DestroyImmediate(newDropdown.GetComponent<LocalizationSettingsPanelUI>());
        UnityEngine.Object.DestroyImmediate(newDropdown.GetComponent<LootFilterSettingsPanelUI>());
        newDropdown.name = Name;
        newDropdown.SetSiblingIndex(orderIndex + 1);
        
        newDropdown.GetChild(0).GetComponent<TMP_Text>().text = Name;
        UnityEngine.Object.DestroyImmediate(newDropdown.GetChild(0).GetComponent<LocalizeStringEvent>());
        newDropdown.GetChild(1).GetComponent<TMP_Text>().text = Description;
        UnityEngine.Object.DestroyImmediate(newDropdown.GetChild(1).GetComponent<LocalizeStringEvent>());
        if (!CopyFrom_Dropdown)
        {
            GameObject _disabled = new GameObject("disabled.copydropdown") { hideFlags = HideFlags.HideAndDontSave };
            _disabled.SetActive(false);
            CopyFrom_Dropdown = UnityEngine.Object.Instantiate(newDropdown.gameObject, _disabled.transform);
        }
        ColoredIconDropdown dropdown = newDropdown.GetChild(3).GetComponent<ColoredIconDropdown>();
        dropdown.onValueChanged.RemoveAllListeners();
        dropdown.ClearOptions();
        Il2CppSystem.Collections.Generic.List<string> options  = new();
        foreach (string enumName in Enum.GetNames(typeof(T))) options.Add(enumName.Replace("_"," "));
        dropdown.AddOptions(options);
        dropdown.value = (int)(object)option.Value;
        dropdown.onValueChanged.AddListener(new Action<int>(_ => a(dropdown.value)));
    }
    public static int CharToIntFast(this char c) => c - '0';
    public static Sprite ToSprite(this string base64)
    {
        byte[] bytes = Convert.FromBase64String(base64);
        Texture2D texture = new Texture2D(1, 1);
        texture.LoadImage(bytes); 
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
    private static BazaarStallType? IdolStall(ItemData item) => item.getAsUnpacked()?.classReq switch
    {
        ItemList.ClassRequirement.None => BazaarStallType.GENERIC_IDOL,
        ItemList.ClassRequirement.Primalist => BazaarStallType.PRIMALIST_IDOL,
        ItemList.ClassRequirement.Mage => BazaarStallType.MAGE_IDOL,
        ItemList.ClassRequirement.Sentinel => BazaarStallType.SENTINEL_IDOL,
        ItemList.ClassRequirement.Acolyte => BazaarStallType.ACOLYTE_IDOL,
        ItemList.ClassRequirement.Rogue => BazaarStallType.ROGUE_IDOL,
        ItemList.ClassRequirement.Any => BazaarStallType.GENERIC_IDOL,
        _ => null
    };
    public static BazaarStallType? ToStall(this ItemData item)
    {
        BazaarItemType type = (BazaarItemType)item.itemType;
        if (item.itemType.IsIdol()) return IdolStall(item);
        return type switch
        {
            BazaarItemType.Helmet => BazaarStallType.HELMET,
            BazaarItemType.BodyArmor => BazaarStallType.BODY_ARMOR,
            BazaarItemType.Belt => BazaarStallType.BELT,
            BazaarItemType.Boots => BazaarStallType.BOOTS,
            BazaarItemType.Gloves => BazaarStallType.GLOVES,
            BazaarItemType.OneHandedAxe => BazaarStallType.ONE_HANDED_AXE,
            BazaarItemType.OneHandedDagger => BazaarStallType.ONE_HANDED_DAGGER,
            BazaarItemType.OneHandedMaces => BazaarStallType.ONE_HANDED_MACES,
            BazaarItemType.OneHandedSceptre => BazaarStallType.ONE_HANDED_SCEPTRE,
            BazaarItemType.OneHandedSword => BazaarStallType.ONE_HANDED_SWORD,
            BazaarItemType.Wand => BazaarStallType.WAND,
            BazaarItemType.TwoHandedAxe => BazaarStallType.TWO_HANDED_AXE,
            BazaarItemType.TwoHandedMace => BazaarStallType.TWO_HANDED_MACE,
            BazaarItemType.TwoHandedSpear => BazaarStallType.TWO_HANDED_SPEAR,
            BazaarItemType.TwoHandedStaff => BazaarStallType.TWO_HANDED_STAFF,
            BazaarItemType.TwoHandedSword => BazaarStallType.TWO_HANDED_SWORD,
            BazaarItemType.Quiver => BazaarStallType.QUIVER,
            BazaarItemType.Shield => BazaarStallType.SHIELD,
            BazaarItemType.Catalyst => BazaarStallType.CATALYST,
            BazaarItemType.Amulet => BazaarStallType.AMULET,
            BazaarItemType.Ring => BazaarStallType.RING,
            BazaarItemType.Relic => BazaarStallType.RELIC,
            BazaarItemType.Bow => BazaarStallType.BOW,
            _ => null
        };
    }
    public static bool IsIdol(this byte itemType)
    {
        BazaarItemType type = (BazaarItemType)itemType;
        return type is BazaarItemType.Idol1X2 or BazaarItemType.Idol1X3 or BazaarItemType.Idol1X4 
            or BazaarItemType.Idol2X1 or BazaarItemType.Idol2X2 or BazaarItemType.Idol3X1 
            or BazaarItemType.Idol4X1 or BazaarItemType.Idol1X1Eterra or BazaarItemType.Idol1X1Lagon;
    }
}
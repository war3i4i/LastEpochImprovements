using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using Il2CppLE.UI.Controls;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
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
}
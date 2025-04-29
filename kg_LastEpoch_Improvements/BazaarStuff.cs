using Il2CppLE.Services.Bazaar;
using Il2CppLE.UI.Bazaar;
using Il2CppLE.UI.MultiPicker;
using MelonLoader;
using UnityEngine.SceneManagement;
using State = Il2CppLE.UI.MultiPicker.State;

namespace kg_LastEpoch_Improvements;

public static class BazaarStuff
{
    private static object LastSearchPressRoutine;
    public static void Update()
    {
        if (!Input.GetKey(KeyCode.LeftShift) || !Input.GetKeyDown(KeyCode.Mouse2) || TooltipItemManager.instance?.activeParameters?.Item is not { } currentItem) return;
        BazaarStallType? type = currentItem.ToStall();
        if (type == null) return;
        if (SceneManager.GetActiveScene().name != "Bazaar") return;
        UIBase.instance.closeInventory();
        BazaarUI bazaarUI = UIBase.instance.BazaarMenu;
        bazaarUI.FilterUI.ResetUI();
        UIBase.instance.openBazaar(new Il2CppSystem.Nullable<BazaarStallType>(type.Value));
        if (LastSearchPressRoutine != null) MelonCoroutines.Stop(LastSearchPressRoutine); 
        LastSearchPressRoutine = MelonCoroutines.Start(PressSearchAfterLoadDone(currentItem));
    }
    private static void IncludeModsInSearch(List<ItemAffix> mods)
    {
        if (mods == null || mods.Count == 0) return;
        BazaarUI bazaarUI = UIBase.instance.BazaarMenu;
        bazaarUI.filterUI.advancedButton.onClick.Invoke();
        bazaarUI.filterUI.affixesPicker.multiPickerOpener.openPickerButton.onClick.Invoke();
        State state = bazaarUI.FilterUI.affixesPicker.multiPickerOpener.multipicker.CurrentState;
        foreach (ItemAffix mod in mods)
        {
            if (!state.Entries.TryGetValue(mod.affixId, out StatefulEntry val)) continue;
            val.selected = true;
            val.data = new AffixData() { tier = mod.DisplayTier };
        }
        bazaarUI.filterUI.affixesPicker.multiPickerOpener.multipicker.confirmButton.onClick.Invoke();
        bazaarUI.filterUI.advancedButton.onClick.Invoke();
    }
    
    private static IEnumerator PressSearchAfterLoadDone(ItemDataUnpacked item)
    {
        yield return null; yield return null; yield return null;
        while (true)
        {
            if (!UIBase.instance.BazaarMenu || !UIBase.instance.BazaarMenu.gameObject.activeSelf || item == null) 
                yield break;
            if (!UIBase.instance.BazaarMenu.IsLoadingIndicatorActive)
            {
                yield return new WaitForSeconds(0.5f);
                BazaarUI bazaarUI = UIBase.instance.BazaarMenu;
                bazaarUI.filterUI.clearFilterButton.onClick.Invoke();
                if (item.isUniqueSetOrLegendary()) bazaarUI.FilterUI.uniquesPicker.SelectedUniques = new(1) { [0] = item.uniqueID };
                bazaarUI.FilterUI.legendaryPotentialRange.min.text = item.legendaryPotential.ToString();
                bazaarUI.FilterUI.legendaryPotentialRange.MinValue = new(item.legendaryPotential);
                bazaarUI.FilterUI.sortingSelection.dropdown.value = 1;
                bazaarUI.FilterUI.sortingSelection.SelectedSorting = SortingSelection.GOLD_LOW_FIRST;
                bazaarUI.FilterUI.raritySelection.dropdown.value = (int)BazaarItemRarityHelper.GetBazaarItemRarity(item) + 1;
                bazaarUI.FilterUI.raritySelection.SelectedRarity = new(BazaarItemRarityHelper.GetBazaarItemRarity(item));

                if (item.isExaltedItem())
                { 
                    List<ItemAffix> _6TierPlusMods = [];
                    foreach (ItemAffix affix in item.affixes) if (affix.DisplayTier >= 6) _6TierPlusMods.Add(affix);
                    IncludeModsInSearch(_6TierPlusMods);
                }

                if (item.itemType.IsIdol())
                {
                    List<ItemAffix> _allMods = [];
                    foreach (ItemAffix affix in item.affixes) if (affix.DisplayTier > 0) _allMods.Add(affix);
                    IncludeModsInSearch(_allMods);
                }
                
                yield return new WaitForSeconds(0.5f);
                bazaarUI.SearchPress();
                yield break;  
            }
            yield return null; 
        }
    }
    
    [HarmonyPatch(typeof(TooltipItemManager),nameof(TooltipItemManager.OpenItemTooltip))]
    private static class TooltipItemManager_OpenItemTooltip_Patch
    {
        private static void Prefix(ItemDataUnpacked data, out string __state)
        {
            __state = null;
            if (data?.ToStall() == null || data.LoreText == null || SceneManager.GetActiveScene().name != "Bazaar") return;
            __state = data.LoreText;
            data.LoreText += $"\n<color=yellow><size=18>Shift+Middle Mouse to search for this item in the Bazaar</size></color>\n" +
                             $"<color=#808080><size=12>This also will include current item rarity , legendary potential and unique item name (if its unique or legendary)</size></color>";
        }
        private static void Finalizer(ItemDataUnpacked data, string __state)
        {
            if (__state != null) data.LoreText = __state;
        }
    }
}
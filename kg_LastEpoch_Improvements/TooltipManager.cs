using System.Text;
using Il2CppItemFiltering;
using MelonLoader;


namespace Fallen_LE_Mods.Features
{

    public class TooltipManager : MelonMod
    {
        private static void HandleTooltipUpdate(UITooltipItem.ItemTooltipInfo ttInfo, ItemDataUnpacked item)
        {
            try
            {
                
                ItemFilter filter = ItemFilterManager.Instance.Filter;
                if (filter == null) return;
                Rule match = FallenUtils.MatchFilterRule(item);
                if (match != null && (ItemList.isEquipment(item.itemType) || ItemList.isIdol(item.itemType)))
                {
                    AppendTooltipText(ttInfo, match.GetRuleDescription(), true);
                }

                ItemDataUnpacked itemData = FallenUtils.FindSimilarUniqueItemInStash(item);
                if (itemData != null)
                {
                    string description = GetMatchedItemDescription(itemData, item);
                    AppendTooltipText(ttInfo, description, false);
                }
                else if (item.isUniqueSetOrLegendary())
                {
                    AppendTooltipText(ttInfo, "仓库里<color=red>没有</color>该装备", false);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception in HandleTooltipUpdate: {ex.Message}");
            }
        }

        private static void AppendTooltipText(UITooltipItem.ItemTooltipInfo ttInfo, string description, bool isFilterText)
        {
            StringBuilder loreTextBuilder = new(ttInfo.loreText);
            if (string.IsNullOrEmpty(loreTextBuilder.ToString()))
            {
                loreTextBuilder.Append(description);
            }
            else
            {
                if (isFilterText)
                {
                    loreTextBuilder.AppendLine("");
                }
                loreTextBuilder.AppendLine($"\n{description}");
            }
            ttInfo.loreText = loreTextBuilder.ToString();
        }

        private static string GetMatchedItemDescription(ItemDataUnpacked itemData, ItemDataUnpacked item)
        {
            int ownedLP = itemData.legendaryPotential;
            int ownedWW = itemData.weaversWill;

            if (ownedLP != item.legendaryPotential || ownedWW != item.weaversWill)
            {
                string LPdescription = (ownedLP > item.legendaryPotential) ? $"仓库里有<color=green>更高</color>的潜能值 : <color=green><b>{ownedLP}</b></color>" : $"仓库里有<color=red>更低</color>的潜能值 : <color=red><b>{ownedLP}</b></color>";
                string WWdescription = (ownedWW > item.weaversWill) ? $"仓库里有<color=green>更高</color>的编织者 : <color=green><b>{ownedWW}</b></color>" : $"仓库里有<color=red>更低</color>的编织者 : <color=red><b>{ownedWW}</b></color>";

                if (ownedLP != item.legendaryPotential)
                {
                    return LPdescription;
                }
                else if (ownedWW != item.weaversWill)
                {
                    return WWdescription;
                }
                
            }
            return itemData.Equals(item) ? "<color=#ED3800>存放在仓库里的装备</color>" : "仓库里有<color=#B0D600>相同</color>的装备";
        }

        [HarmonyPatch(typeof(UITooltipItem), "OpenTooltip", new Type[] { typeof(UITooltipItem.ItemTooltipInfo), typeof(UnityEngine.Vector2), typeof(UnityEngine.GameObject), typeof(ItemDataUnpacked), typeof(TooltipItemManager.SlotType) })]
        public class UITooltipItemPatch
        {
            public static void Prefix(ref UITooltipItem __instance, ref UITooltipItem.ItemTooltipInfo ttInfo, ref Vector2 position, ref GameObject targetSlot, ref ItemDataUnpacked _item, ref TooltipItemManager.SlotType slotType)
            {
                HandleTooltipUpdate(ttInfo, _item);
            }
        }

        [HarmonyPatch(typeof(UITooltipItem), "OpenGroundTooltip", new Type[] { typeof(UITooltipItem.ItemTooltipInfo), typeof(UnityEngine.Vector2), typeof(UnityEngine.GameObject), typeof(ItemDataUnpacked) })]
        public class GroundUITooltipItemPatch
        {
            public static void Prefix(ref UITooltipItem __instance, ref UITooltipItem.ItemTooltipInfo ttInfo, ref Vector2 position, ref GameObject targetSlot, ref ItemDataUnpacked _item)
            {
                HandleTooltipUpdate(ttInfo, _item);
            }
        }
    }
}
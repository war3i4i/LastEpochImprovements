using Harmony;
using Il2Cpp;
using Il2CppItemFiltering;
using Il2CppTMPro;
using MelonLoader;
using System.Collections;
using static kg_LastEpoch_FilterIcons_Melon.kg_LastEpoch_FilterIcons_Melon;

namespace kg_LastEpoch_FilterIcons_Melon;

public class Experimental
{
    //For some reason EHG TextMeshPro on item tooltip literally has different value than displayed text itself. I didn't manage to find a way to set it directly in some patch
    //so as a workaround (even not most optimized) we just use coroutine to skip one frame yield return null and then set the text. Important thing is that we need to first
    //set text to empty string and then to the desired value. Otherwise it won't work (maybe some internal OnTextChanged event or smth).
    //I would even assume that for (maybe) some optimization purposes they're changing TMP Text not via .text = "something" but by directly changing TMP Vertex buffer or smth
    //(that's why we need to set it to empty string first, so it internally updates verticies and then we can set it to the desired value)
    //^ or maybe its all bullshit and im just dumb, who knows
    [HarmonyPatch(typeof(GroundItemLabel), nameof(GroundItemLabel.SetGroundTooltipText), typeof(bool))]
    private static class GroundItemLabel_Show_Patch
    {
        private static void Postfix(GroundItemLabel __instance)
        {
            if (ShowAffixOnLabel.Value is DisplayAffixType_GroundLabel.None) return;
            MelonCoroutines.Start(DelayRoutine(__instance));
        }

        private static bool IsFilter(ItemDataUnpacked item)
        {
            if (ItemFilterManager.Instance.Filter == null) return false;
            foreach (var rule in ItemFilterManager.Instance.Filter.rules)
            {
                if (!rule.isEnabled || rule.type is Rule.RuleOutcome.HIDE) continue;
                bool result = rule.Match(item);
                if (result) return true;
            }
            return false;
        }

        private static IEnumerator DelayRoutine(GroundItemLabel item)
        {
            yield return null;
            if (item == null || !item || item.getItemData() == null) yield break;
            ItemDataUnpacked itemData = item.getItemData();
            TextMeshProUGUI tmp = item.itemText;
            if (!tmp) yield break;

            bool isFiltered = ShowAffixOnLabel.Value is (DisplayAffixType_GroundLabel.With_Tier_Filter_Only or DisplayAffixType_GroundLabel.Without_Tier_Filter_Only or DisplayAffixType_GroundLabel.Letter_With_Tier_Filter_Only or DisplayAffixType_GroundLabel.Letter_Without_Tier_Filter_Only);
            bool isLetter = ShowAffixOnLabel.Value is (DisplayAffixType_GroundLabel.Letter_With_Tier or DisplayAffixType_GroundLabel.Letter_Without_Tier or DisplayAffixType_GroundLabel.Letter_With_Tier_Filter_Only or DisplayAffixType_GroundLabel.Letter_Without_Tier_Filter_Only);
            if (isFiltered)
                if (!IsFilter(itemData)) yield break;

            string itemName = itemData.FullName;
            if (itemData.isUnique() && itemData.affixes.Count == 0)
            {
                if (itemData.weaversWill > 0)
                    itemName += $" <color=#FF0000>[WW: {itemData.weaversWill}]</color>";
                else
                    itemName += $" <color=#FF0000>[LP: {itemData.legendaryPotential}]</color>";
            }
            if (itemData.affixes.Count > 0)
            {
                if (isLetter) { itemName += " ["; }
                foreach (ItemAffix affix in itemData.affixes)
                {
                    double roll = Math.Round(affix.getRollFloat() * 100.0, 1);
                    int tier = affix.DisplayTier;
                    string rollColor = AffixRolls.GetItemRollRarityColor(roll);
                    string tierColor = AffixRolls.GetItemTierColor(tier);
                    string letter = AffixRolls.GetItemRollRarityLetter(roll);
                    string letterColor = AffixRolls.GetItemRollRarityColorLetter(roll);
                    string letterTier = tier > 0 ? $"<color={tierColor}>{tier}</color>" : "";
                    
                    itemName += ShowAffixOnLabel.Value switch
                    {
                        DisplayAffixType_GroundLabel.With_Tier or DisplayAffixType_GroundLabel.With_Tier_Filter_Only => $" [<color={tierColor}>T{tier}</color> <color={rollColor}>{roll}%</color>]",
                        DisplayAffixType_GroundLabel.Without_Tier or DisplayAffixType_GroundLabel.Without_Tier_Filter_Only => $" [<color={rollColor}>{roll}%</color>]",
                        DisplayAffixType_GroundLabel.Letter_With_Tier or DisplayAffixType_GroundLabel.Letter_With_Tier_Filter_Only => $" {letterTier}<color={letterColor}>{letter}</color>",
                        DisplayAffixType_GroundLabel.Letter_Without_Tier or DisplayAffixType_GroundLabel.Letter_Without_Tier_Filter_Only => $" <color={letterColor}>{letter}</color>",
                        _ => ""
                    };
                }
                if (isLetter) { itemName += " ]"; }
            }
            tmp.text = "";
            tmp.text = item.emphasized ? itemName.ToUpper() : itemName;
        }
    }


}
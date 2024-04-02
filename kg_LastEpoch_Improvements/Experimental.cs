using Il2CppTMPro;
using MelonLoader;
using static kg_LastEpoch_Improvements.Kg_LastEpoch_Improvements;

namespace kg_LastEpoch_Improvements;

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


        private static IEnumerator DelayRoutine(GroundItemLabel item)
        {
            yield return null;
            if (item == null || !item || item.getItemData() is not { } itemData) yield break;
            TextMeshProUGUI tmp = item.itemText;
            if (!tmp) yield break;

            bool isKG = ShowAffixOnLabel.Value is (DisplayAffixType_GroundLabel.Letter_With_Tier or DisplayAffixType_GroundLabel.Letter_Without_Tier or DisplayAffixType_GroundLabel.Without_Tier or DisplayAffixType_GroundLabel.With_Tier);
            bool isDD = ShowAffixOnLabel.Value is (DisplayAffixType_GroundLabel.DD_Tier);
            string itemName = itemData.FullName;
            {
                if (itemData.isUniqueSetOrLegendary() && itemData.affixes.Count == 0)
                {
                    if (itemData.weaversWill > 0)
                        itemName += $" <color=#FF0AC4>[WW: {itemData.weaversWill}]</color>";
                    else if (itemData.legendaryPotential > 0)
                        itemName += $" <color=#00FF00>[LP: {itemData.legendaryPotential}]</color>";
                }
                tmp.text = "";
                tmp.text = item.emphasized ? itemName.ToUpper() : itemName;
            }

            if (itemData.affixes.Count > 0)
            {

                if (isKG)
                {
                    itemName += " [";

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
                            DisplayAffixType_GroundLabel.With_Tier => $" [<color={tierColor}>T{tier}</color> <color={rollColor}>{roll}%</color>]",
                            DisplayAffixType_GroundLabel.Without_Tier => $" [<color={rollColor}>{roll}%</color>]",
                            DisplayAffixType_GroundLabel.Letter_With_Tier => $" {letterTier}<color={letterColor}>{letter}</color>",
                            DisplayAffixType_GroundLabel.Letter_Without_Tier => $" <color={letterColor}>{letter}</color>",
                            _ => ""

                        };
                    }
                    itemName += " ]";
                    tmp.text = "";
                    tmp.text = item.emphasized ? itemName.ToUpper() : itemName;
                }




                if (isDD)
                {
                    List<string> prefixes = new();
                    List<string> suffixes = new();
                    List<string> SealedAffixes = new();
                    foreach (ItemAffix affix in itemData.affixes)
                    {
                        if (affix.affixType == AffixList.AffixType.PREFIX)
                        {
                            if (affix.isSealedAffix)
                            {
                                SealedAffixes.Add($"T{affix.DisplayTier}");
                            }
                            else if (!affix.isSealedAffix && affix.specialAffixType == AffixList.SpecialAffixType.Experimental)
                            {
                                int prefixIndex = prefixes.Count + 1;
                                string experimentalAffix = $"<color=#00FF00>•T{affix.DisplayTier}</color>";
                                if (prefixIndex == 1)
                                {
                                    experimentalAffix = $"{experimentalAffix}";
                                }
                                else if (prefixIndex == 2)
                                {
                                    experimentalAffix = $"{experimentalAffix}";
                                }
                                affix.affixName = experimentalAffix;
                                prefixes.Add(affix.affixName);
                            }
                            else
                            {
                                prefixes.Add($"T{affix.DisplayTier}");
                            }
                        }
                        else if (affix.affixType == AffixList.AffixType.SUFFIX)
                        {
                            if (affix.isSealedAffix)
                            {
                                SealedAffixes.Add($"T{affix.DisplayTier}");
                            }
                            else if (!affix.isSealedAffix && affix.specialAffixType == AffixList.SpecialAffixType.Experimental)
                            {
                                int suffixIndex = suffixes.Count + 1;
                                string experimentalAffix = $"<color=#00FF00>•T{affix.DisplayTier}</color>";
                                if (suffixIndex == 1)
                                {
                                    experimentalAffix = $"{experimentalAffix}";
                                }
                                else if (suffixIndex == 2)
                                {
                                    experimentalAffix = $"{experimentalAffix}";
                                }
                                affix.affixName = experimentalAffix;
                                suffixes.Add(affix.affixName);
                            }
                            else
                            {
                                suffixes.Add($"T{affix.DisplayTier}");
                            }
                        }
                    }

                    string prefix = string.Join(" ", prefixes);
                    string suffix = string.Join(" ", suffixes);
                    string sealedAffix = string.Join(" ", SealedAffixes);

                    bool hasPrefix = prefixes.Count > 0;
                    bool hasSuffix = suffixes.Count > 0;
                    bool hasSealedAffix = SealedAffixes.Count > 0;

                    string finalItemName;
                    if (hasPrefix && hasSuffix && hasSealedAffix)
                    {
                        finalItemName = $"[{prefix}] {itemName} [{suffix}] <color=#FF0000>•[{sealedAffix}]</color>";
                    }
                    else if (hasPrefix && hasSuffix)
                    {
                        finalItemName = $"[{prefix}] {itemName} [{suffix}]";
                    }
                    else if (hasPrefix)
                    {
                        finalItemName = $"[{prefix}] {itemName}";
                    }
                    else if (hasSuffix)
                    {
                        finalItemName = $"{itemName} [{suffix}]";
                    }
                    else if (hasSealedAffix)
                    {
                        finalItemName = $"{itemName} <color=#FF0000>•[{sealedAffix}]</color>";
                    }
                    else
                    {
                        finalItemName = itemName;
                    }
                    tmp.text = "";
                    tmp.text = item.emphasized ? finalItemName.ToUpper() : finalItemName;
                }

            }
        }
    }
}

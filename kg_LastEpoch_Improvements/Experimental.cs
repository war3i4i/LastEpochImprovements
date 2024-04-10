using System.Text;
using Il2CppTMPro;
using MelonLoader;
using static kg_LastEpoch_Improvements.Kg_LastEpoch_Improvements;

namespace kg_LastEpoch_Improvements;

public class Experimental
{
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
            if (item == null) yield break;
            var itemData = item.getItemData();
            if (itemData == null || itemData.affixes == null || item.itemText == null) yield break;
            TextMeshProUGUI tmp = item.itemText;
            bool isLetter = ShowAffixOnLabel.Value is DisplayAffixType_GroundLabel.Ontier or DisplayAffixType_GroundLabel.Letter_With_Tier or DisplayAffixType_GroundLabel.Letter_Without_Tier or DisplayAffixType_GroundLabel.Without_Tier or DisplayAffixType_GroundLabel.With_Tier;
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
                tmp.text = itemName;
            }


            if (itemData.affixes != null && itemData.affixes.Count > 0)
            {
                StringBuilder prefixBuilder = new();
                StringBuilder suffixBuilder = new();
                StringBuilder SealedBuilder = new();
                StringBuilder NopsBuilder = new();
                int prefixIndex = 0;
                int suffixIndex = 0;
                int NopsIndex = 0;

                foreach (ItemAffix affix in itemData.affixes)
                {
                    string affixEntry = getAffixOnLabelEntry(affix, itemData);

                    if (isLetter && ShowAffixPSOnLabel.Value)
                    {
                        if (affix.affixType == AffixList.AffixType.PREFIX)
                        {
                            if (affix.isSealedAffix)
                            {
                                SealedBuilder.Append(affixEntry);
                            }
                            else if (!affix.isSealedAffix && affix.specialAffixType == AffixList.SpecialAffixType.Experimental)
                            {
                                if (prefixIndex > 0)
                                    prefixBuilder.Append(' ');
                                prefixBuilder.Append($"<color=#FFFFFF>•</color>{affixEntry}");
                                prefixIndex++;
                            }
                            else
                            {
                                if (prefixIndex > 0)
                                    prefixBuilder.Append(' ');
                                prefixBuilder.Append(affixEntry);
                                prefixIndex++;
                            }
                        }
                        else if (affix.affixType == AffixList.AffixType.SUFFIX)
                        {
                            if (affix.isSealedAffix)
                            {
                                SealedBuilder.Append(affixEntry);
                            }
                            else if (!affix.isSealedAffix && affix.specialAffixType == AffixList.SpecialAffixType.Experimental)
                            {
                                if (suffixIndex > 0)
                                    suffixBuilder.Append(' ');
                                suffixBuilder.Append($"<color=#FFFFFF>•</color>{affixEntry}");
                                suffixIndex++;
                            }
                            else
                            {
                                if (suffixIndex > 0)
                                    suffixBuilder.Append(' ');
                                suffixBuilder.Append(affixEntry);
                                suffixIndex++;
                            }
                        }
                    }
                    else
                    {
                        if (affix.isSealedAffix)
                        {
                            SealedBuilder.Append(affixEntry);
                        }
                        else if (!affix.isSealedAffix && affix.specialAffixType == AffixList.SpecialAffixType.Experimental)
                        {
                            if (NopsIndex > 0)
                                NopsBuilder.Append(' ');
                            NopsBuilder.Append($"<color=#FFFFFF>•</color>{affixEntry}");
                            NopsIndex++;
                        }
                        else
                        {
                            if (NopsIndex > 0)
                                NopsBuilder.Append(' ');
                            NopsBuilder.Append(affixEntry);
                            NopsIndex++;
                        }
                    }
                }

                string finalPrefixes = prefixBuilder.Length > 0 ? $"[{prefixBuilder}]" : ""; // 前
                string finalSuffixes = suffixBuilder.Length > 0 ? $"[{suffixBuilder}]" : ""; // 后
                string finalSealed = SealedBuilder.Length > 0 ? $"<color=red>[•{SealedBuilder}]</color>" : ""; // 封印
                string finalNops = NopsBuilder.Length > 0 ? $"[{NopsBuilder}]" : ""; // 不区分格式
                string finalItemName = $"{finalPrefixes} {itemName} {finalSuffixes} {finalNops} {finalSealed}";
                tmp.text = "";
                tmp.text = item.emphasized ? $"<u>{finalItemName}</u>" : finalItemName;
            }




            static string getAffixOnLabelEntry(ItemAffix affix, ItemDataUnpacked itemData)
            {
                if (itemData == null || affix == null) return "?";

                int tier = affix.DisplayTier;

                double trueRoll = Affixfix.GetAffixTrueRoll(itemData, affix);

                float true_roll_value = (float)(trueRoll / 100);

                return getAffixOnLabel(tier, true_roll_value);

            }

            static string getAffixOnLabel(int tier, double true_roll_value)
            {

                string tierColor = AffixRolls.GetItemTierColor_Style2(tier);
                string Tierclass = tier > 0 ? $"<color={tierColor}>{tier}</color>" : "";
                double rollValue = Math.Round(true_roll_value * 100.0, 1);
                string rollValueColor = AffixRolls.GetItemRollRarityColor_Style2(rollValue);

                double style2_roll_value = Math.Round(true_roll_value * 100.0, 1);
                string style2_roll_string = "" + style2_roll_value + "%";

                string style3_roll_value = AffixRolls.GetItemRollRarity_Style3(rollValue, false);
                string style3_roll_Color = AffixRolls.GetItemRollRarityColor_Style3(rollValue);



                if (true_roll_value > 1)
                {
                    style2_roll_string = " -";
                }
                else if (true_roll_value < 0)
                {
                    style2_roll_string = "";
                }
                return ShowAffixOnLabel.Value switch
                {
                    DisplayAffixType_GroundLabel.Ontier => $"<color={tierColor}>T{tier}</color>",
                    DisplayAffixType_GroundLabel.With_Tier => $"<color={tierColor}>T{tier}</color>(<color={rollValueColor}>{style2_roll_string}</color>)",
                    DisplayAffixType_GroundLabel.Without_Tier => $"<color={rollValueColor}>{style2_roll_string}</color>",
                    DisplayAffixType_GroundLabel.Letter_With_Tier => $"{Tierclass}<color={style3_roll_Color}>{style3_roll_value}</color>",
                    DisplayAffixType_GroundLabel.Letter_Without_Tier => $"<color={style3_roll_Color}>{style3_roll_value}</color>",




                    _ => ""
                };
            }

        }
    }
}
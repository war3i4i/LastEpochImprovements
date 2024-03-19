using System.Collections;
using System.Reflection;
using Harmony;
using Il2Cpp;
using Il2CppTMPro;
using JetBrains.Annotations;
using MelonLoader;
using UnityEngine;
using AccessTools = HarmonyLib.AccessTools;
using Random = UnityEngine.Random;

namespace kg_LastEpoch_FilterIcons_Melon;

public class Experimental
{
    [HarmonyPatch(typeof(GroundItemLabel),nameof(GroundItemLabel.SetGroundTooltipText), typeof(bool))]
    private static class GroundItemLabel_Show_Patch4
    {
        private static void Postfix(GroundItemLabel __instance)
        {
            if (kg_LastEpoch_FilterIcons_Melon.ShowAffixOnLabel.Value is kg_LastEpoch_FilterIcons_Melon.DisplayAffixType_GroundLabel.None) return;
            MelonCoroutines.Start(DelayRoutine(__instance));
        }

        private static IEnumerator DelayRoutine(GroundItemLabel item)
        {
            yield return null;
            if (item == null || !item || item.getItemData() == null) yield break;
            ItemDataUnpacked itemData = item.getItemData(); 
            TextMeshProUGUI tmp = item.itemText;
            if (!tmp) yield break;
            string itemName = itemData.FullName;
            if (itemData.isUnique() && itemData.affixes.Count == 0)
            {
                if (itemData.weaversWill > 0)
                    itemName += $" <color=red>[WW: {itemData.weaversWill}]</color>";
                else
                    itemName += $" <color=red>[LP: {itemData.legendaryPotential}]</color>";
            }

            if (itemData.affixes.Count > 0)
            { 
                foreach (ItemAffix affix in itemData.affixes) 
                {
                    double roll = Math.Round(affix.getRollFloat() * 100.0, 1);
                    int tier = affix.DisplayTier;
                    string rollColor = AffixRolls.GetItemRollRarityColor(roll);
                    string tierColor = AffixRolls.GetItemTierColor(tier);
                    itemName += kg_LastEpoch_FilterIcons_Melon.ShowAffixOnLabel.Value switch
                    {
                        kg_LastEpoch_FilterIcons_Melon.DisplayAffixType_GroundLabel.With_Tier => $" [<color={tierColor}>T{tier}</color> <color={rollColor}>{roll}%</color>]",
                        kg_LastEpoch_FilterIcons_Melon.DisplayAffixType_GroundLabel.Without_Tier => $" [<color={rollColor}>{roll}%</color>]",
                        _ => ""
                    };
                    
                }
            }
            tmp.text = "";
            if (item.emphasized) itemName = $"{itemName.ToUpper()}";
            tmp.text = itemName;
        }
    }
    
  
}
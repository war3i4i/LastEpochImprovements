namespace kg_LastEpoch_Improvements

{
    public class Affixfix
    {

        public static double GetAffixTrueRoll(ItemDataUnpacked item, ItemAffix affix)
        {
            if (item == null || affix == null) return -1;
            double trueRoll = -1;
            int display_roll_value = -1;
            int display_minRoll_value = -1;
            int display_maxRoll_value = -1;
            float roll = -1;
            float minRoll = -1;
            float maxRoll = -1;
            float affixEffectModifier = -1;
            GetAffixRollValues(item, affix, -1, ref trueRoll, ref display_roll_value, ref display_minRoll_value, ref display_maxRoll_value, ref roll, ref minRoll, ref maxRoll, ref affixEffectModifier);
            return trueRoll;
        }

        private static bool IsIntNumber(float num)
        {
            int int_num = (int)Math.Round(num);
            return int_num == num;
        }

        public static void GetAffixRollValues(ItemDataUnpacked item, ItemAffix affix, float modifierValue,
            ref double display_trueRoll, // 实际计算出的Roll值(取一位小数,无百分号)
            ref int display_value, // 实际取值(已取整,百分比也只取百分号前的部分,下同)
            ref int display_minRoll_value,  // 实际范围的最小值(已取整,百分比时取百分号前的部分)
            ref int display_maxRoll_value,  // 实际范围的最大值(已取整,百分比时取百分号前的部分)
            ref float roll, // 内部Roll值
            ref float minRoll, // 基准范围最小值
            ref float maxRoll, // 基准范围最大值
            ref float affixEffectModifier // 装备类型调整因子 (1 + factor) * base
            )
        {

            if (item == null || affix == null) return;
            var afl = TooltipItemManager.instance.affixList;
            var af = afl.GetAffix(affix.affixId);
            int tierIdx = affix.DisplayTier - 1;
            var tier = af.tiers[tierIdx];
            minRoll = tier.minRoll;
            maxRoll = tier.maxRoll;
            roll = affix.getRollFloat();

            affixEffectModifier = ItemList.instance.getAffixEffectModifier(item.itemType);

            float scaled_minRoll = minRoll * (1 + affixEffectModifier);
            float scaled_maxRoll = maxRoll * (1 + affixEffectModifier);
            float scaled_value = scaled_minRoll + (scaled_maxRoll - scaled_minRoll) * roll;

            if (
                IsIntNumber(modifierValue) &&
                IsIntNumber(maxRoll) &&
                IsIntNumber(minRoll)
                ) //说明是不带百分号的数值类型
            {
                display_value = (int)Math.Round(scaled_value);
                display_minRoll_value = (int)Math.Round(scaled_minRoll);
                display_maxRoll_value = (int)Math.Round(scaled_maxRoll);
            }
            else
            {
                display_value = (int)Math.Round(scaled_value * 100);
                display_minRoll_value = (int)Math.Round(scaled_minRoll * 100);
                display_maxRoll_value = (int)Math.Round(scaled_maxRoll * 100);
            }

            if (display_maxRoll_value == display_minRoll_value)
            {
                display_trueRoll = 255; //254 代表最大值和最小值相同, 评分无意义
            }
            else
            {
                display_trueRoll = Math.Round((display_value - display_minRoll_value) * 100.0 / (display_maxRoll_value - display_minRoll_value), 1);
            }
        }

        public static void GetImplicitRollValues(ItemDataUnpacked item, int implicitNumber, float modifierValue,
            ref double display_trueRoll, // 实际计算出的Roll值(取一位小数,无百分号)
            ref int display_value, // 实际取值(已取整,百分比也只取百分号前的部分,下同)
            ref int display_minRoll_value,  // 实际范围的最小值(已取整,百分比时取百分号前的部分)
            ref int display_maxRoll_value,  // 实际范围的最大值(已取整,百分比时取百分号前的部分)
            ref float roll, // 内部Roll值
            ref float minRoll, // 基准范围最小值
            ref float maxRoll // 基准范围最大值
            )
        {
            var implicit_obj = item.Implicits[implicitNumber];
            //float value = implicit_obj.implicitValue;

            maxRoll = implicit_obj.implicitMaxValue;
            var minValues = ItemList.instance.GetImplicitMinimumValues(item.itemType, item.subType);
            minRoll = minValues[implicitNumber];

            roll = item.getImplictRollFloat((byte)implicitNumber);

            int int_round_modifierValue = (int)Math.Round(modifierValue);

            if (
                IsIntNumber(modifierValue) &&
                IsIntNumber(maxRoll) &&
                IsIntNumber(minRoll)
                ) //说明是不带百分号的数值类型
            {
                display_value = int_round_modifierValue;
                display_minRoll_value = (int)Math.Round(minRoll);
                display_maxRoll_value = (int)Math.Round(maxRoll);
            }
            else
            {
                display_value = (int)Math.Round(modifierValue * 100);
                display_minRoll_value = (int)Math.Round(minRoll * 100);
                display_maxRoll_value = (int)Math.Round(maxRoll * 100);
            }

            if (display_maxRoll_value == display_minRoll_value)
            {
                display_trueRoll = 255; //254 代表最大值和最小值相同, 评分无意义
            }
            else
            {
                display_trueRoll = Math.Round((display_value - display_minRoll_value) * 100.0 / (display_maxRoll_value - display_minRoll_value), 1);
            }

        }
    }
}
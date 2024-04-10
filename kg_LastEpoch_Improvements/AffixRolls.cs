using static kg_LastEpoch_Improvements.Kg_LastEpoch_Improvements;


namespace kg_LastEpoch_Improvements;

public static class AffixRolls
{

    //style1
    public static float GetUniqueAffixRoll(ItemDataUnpacked item, int uniqueModIndex, float value, SP modProperty)
    {

        if (item.uniqueID > UniqueList.instance.uniques.Count) return (float)ROLL_INVALID.NO_ROLL;
        if (UniqueList.instance.uniques.Get(item.uniqueID) is not { } uniqueEntry) return (float)ROLL_INVALID.NO_ROLL;
        float roll;
        UniqueItemMod uniqueMod = uniqueEntry.mods.Get(uniqueModIndex);
        float min = uniqueMod.value;
        float max = uniqueMod.maxValue;
        bool noRange;
        noRange =
            (min == max) ||
            (max == 0 && min == value) ||
            (max < 0 && min == value && value > 0) ||
            (max > 0 && min > 0 && value > max);

        if (value < 0)
        {
            if (item.uniqueID == 142 && modProperty is SP.ManaCost)
            {
                noRange = true;
            }
        }
        bool negtive = false;



        if (item.uniqueID == 125 && modProperty is SP.DamagePerStackOfAilment)
        {
            roll = 255;
            return roll;
        }




        if (value < 0 && min < 0 && max < 0)
        {
            if (modProperty is SP.AbilityProperty || modProperty is SP.PlayerProperty ||
                modProperty is SP.DamageTaken || modProperty is SP.DamageTakenWhileMoving ||
                modProperty is SP.DamageTakenFromNearbyEnemies ||
                modProperty is SP.DamageTakenAsCold || modProperty is SP.DamageTakenAsFire ||
                modProperty is SP.DamageTakenAsLightning || modProperty is SP.DamageTakenAsNecrotic ||
                modProperty is SP.DamageTakenAsPhysical || modProperty is SP.DamageTakenAsPoison ||
                modProperty is SP.DamageTakenAsVoid
                )
            {
                negtive = true;
            }
        }
        if (noRange)
        {
            roll = (int)ROLL_INVALID.NO_RANGE / 100;
            return roll;
        }


        if (negtive)
        {
            (min, max) = (max, min);
        }

        roll = (value - min) / (max - min);

        return roll;
    }

    public static string Style1_Custom(string finishedString, float trueRoll)
    {
        double true_roll_value = Math.Round(trueRoll, 3);
        string true_roll_string = "" + true_roll_value;

        if (trueRoll > 1)
        {
            true_roll_string = " - ";
        }
        else if (trueRoll < 0)
        {
            true_roll_string = "";
        }

        string toInsert = $" (<color=yellow>{true_roll_string}</color>)";

        int lastNewLine = finishedString.LastIndexOf("\n", StringComparison.Ordinal);
        if (lastNewLine == -1) return finishedString + toInsert;

        return finishedString.Insert(lastNewLine, toInsert);
    }


    public static string Style1_AffixRoll(this string affixStr, double trueRoll)
    {
        float true_roll = (float)(trueRoll / 100);
        return Style1_Custom(affixStr, true_roll);
    }
    public static string Style1_AffixRoll_Unique(this string affixStr, ItemDataUnpacked item, int uniqueModIndex, float modifierValue, SP modProperty)
    {
        float trueRoll = GetUniqueAffixRoll(item, uniqueModIndex, modifierValue, modProperty);
        if (trueRoll == -1) return affixStr;
        return Style1_Custom(affixStr, trueRoll);
    }
    public static string Style1_AffixRoll_Implicit(this string implicitStr, double trueRoll)
    {
        float true_roll = (float)(trueRoll / 100);
        return Style1_Custom(implicitStr, true_roll);
    }








    //style2 
    public static string GetItemRollRarityColor_Style2(double roll)
        => roll switch
        {
            < 20 => "#D2D2D2",
            < 40 => "#E1E1E1",
            < 60 => "#16FF0E",
            < 70 => "#77ACFF",
            < 80 => "#A807FF",
            < 95 => "#FA9E3D",
            <= 100 => "#F9002E",
            _ => "#FFFFFF"
        };
    public static string GetItemTierColor_Style2(int tier)
        => tier switch
        {
            1 => "#D2D2D2",
            2 => "#E1E1E1",
            3 => "#16FF0E",
            4 => "#77ACFF",
            5 => "#A807FF",
            6 => "#FA9E3D",
            7 => "#FA9E3D",
            _ => "#FFFFFF"
        };

    private static string Style2_Custom(int tier, string finishedString, float trueRoll)
    {

        double true_roll_value = Math.Round(trueRoll * 100.0, 1);
        string true_color = GetItemRollRarityColor_Style2(true_roll_value);
        string true_roll_string = "" + true_roll_value + "%";
        if (trueRoll > 1)
        {
            true_roll_string = " - ";
        }

        string tierStr = tier > 0 ? $"<color={GetItemTierColor_Style2(tier)}>[T{tier}]</color> " : "";
        string toInsert = $" {tierStr}<color={true_color}>[{true_roll_string}]</color>";

        int lastNewLine = finishedString.LastIndexOf("\n", StringComparison.Ordinal);
        if (lastNewLine == -1) return finishedString + toInsert;

        return finishedString.Insert(lastNewLine, toInsert);
    }

    public static string Style2_AffixRoll(this string affixStr, ItemAffix affix, double trueRoll)
    {
        float true_roll = (float)(trueRoll / 100);
        return Style2_Custom(affix.DisplayTier, affixStr, true_roll);
    }

    public static string Style2_AffixRoll_Unique(this string affixStr, ItemDataUnpacked item, int uniqueModIndex, float modifierValue, SP modProperty)
    {

        float trueRoll = GetUniqueAffixRoll(item, uniqueModIndex, modifierValue, modProperty);
        if (trueRoll == -1) return affixStr;
        return Style2_Custom(-1, affixStr, trueRoll);
    }

    public static string Style2_AffixRoll_Implicit(this string implicitStr, double trueRoll)
    {
        float true_roll = (float)(trueRoll / 100);
        return Style2_Custom(-1, implicitStr, true_roll);
    }


    //Style3
    public static string GetItemRollRarity_Style3(double roll, bool dotFlag)
    => roll switch
    {
        < 40 => "F",
        < 60 => "D",
        < 70 => "C",
        < 80 => "B",
        < 95 => "A",
        <= 100 => "S",
        _ => dotFlag ? "·" : "-" //unkown
    };
    public static string GetItemRollRarityColor_Style3(double roll)
    => roll switch
    {
        < 40 => "#D2D2D2",
        < 60 => "#1CA600",
        < 70 => "#77ACFF",
        < 80 => "#A807FF",
        < 95 => "#FA9E3D",
        <= 100 => "#FA9E3D",
        _ => "#FFFFFF"
    };



    private static string Style3_Custom(int tier, string finishedString, float trueRoll)
    {

        double true_roll_value = Math.Round(trueRoll * 100.0, 1);
        string true_color = GetItemRollRarityColor_Style2(true_roll_value);
        string true_roll_string = "" + true_roll_value + "%";
        if (trueRoll > 1)
        {
            true_roll_string = " - ";
        }

        string tierStr = tier > 0 ? $"<color={GetItemTierColor_Style2(tier)}>{tier}</color>" : "";
        string toInsert = $" <color={true_color}>[{true_roll_string}]</color>";
        string letterText = $"{tierStr}<color={GetItemRollRarityColor_Style3(true_roll_value)}>{GetItemRollRarity_Style3(true_roll_value, true)}</color>   ";

        int lastNewLine = finishedString.LastIndexOf("\n", StringComparison.Ordinal);
        if (lastNewLine == -1)
        {
            return letterText + finishedString + toInsert;
        }
        return letterText + finishedString.Insert(lastNewLine, toInsert);
    }

    public static string Style3_AffixRoll(this string affixStr, ItemAffix affix, double trueRoll)
    {
        float true_roll = (float)(trueRoll / 100);
        return Style3_Custom(affix.DisplayTier, affixStr, true_roll);
    }

    public static string Style3_AffixRoll_Unique(this string affixStr, ItemDataUnpacked item, int uniqueModIndex, float modifierValue, SP modProperty)
    {
        float trueRoll = GetUniqueAffixRoll(item, uniqueModIndex, modifierValue, modProperty);
        if (trueRoll == -1) return affixStr;
        return Style3_Custom(-1, affixStr, trueRoll);
    }

    public static string Style3_AffixRoll_Implicit(this string implicitStr, double trueRoll)
    {
        float true_roll = (float)(trueRoll / 100);
        return Style3_Custom(-1, implicitStr, true_roll);
    }






    //DD_Style
    private static string DD_Style(int tier, string finishedString)
    {
        string tierStr = tier > 0 ? $"  T{tier}" : "";
        int lastNewLine = finishedString.LastIndexOf("\n");
        if (lastNewLine == -1)
        {
            return $"{finishedString}{tierStr}";
        }
        return $"{finishedString.Insert(lastNewLine, $"{tierStr}")}";
    }

    public static string DD_Style_AffixRoll(this string affixStr, ItemAffix affix)
    {
        return DD_Style(affix.DisplayTier, affixStr);
    }

}

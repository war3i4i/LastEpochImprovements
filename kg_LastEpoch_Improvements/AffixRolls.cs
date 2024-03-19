using Il2Cpp;

namespace kg_LastEpoch_Improvements;

public static class AffixRolls
{
    //style 1
    public static string Style1_AffixRoll(this string affixStr, ItemAffix affix)
    {
        float roll = affix.getRollFloat();
        string toInsert = $" (<color=yellow>{Math.Round(roll, 3)}</color>)";
        int lastNewLine = affixStr.LastIndexOf("\n", StringComparison.Ordinal);
        if (lastNewLine == -1)
            affixStr += toInsert;
        else
            affixStr = affixStr.Insert(lastNewLine, toInsert);
        return affixStr;
    }

    public static string Style1_AffixRoll_Unique(this string affixStr, ItemDataUnpacked item, int uniqueModIndex, float modifierValue)
    {
        if (item.uniqueID > UniqueList.instance.uniques.Count) return affixStr;
        if (UniqueList.instance.uniques.get(item.uniqueID) is not { } uniqueEntry) return affixStr;
        UniqueItemMod uniqueMod = uniqueEntry.mods.get(uniqueModIndex);
        float min = uniqueMod.value;
        float max = uniqueMod.maxValue;
        float roll = min == max || modifierValue > max ? 1 : (modifierValue - min) / (max - min);
        string toInsert = $" (<color=yellow>{Math.Round(roll, 3)}</color>)";
        int lastNewLine = affixStr.LastIndexOf("\n", StringComparison.Ordinal);
        if (lastNewLine == -1)
            affixStr += toInsert;
        else
            affixStr = affixStr.Insert(lastNewLine, toInsert);
        return affixStr;
    }

    public static string Style1_Implicit(this string implicitStr, ItemDataUnpacked item, int implicitNumber)
    {
        float roll = item.getImplictRollFloat((byte)implicitNumber);
        string toInsert = $" (<color=yellow>{Math.Round(roll, 3)}</color>)";
        int lastNewLine = implicitStr.LastIndexOf("\n", StringComparison.Ordinal);
        if (lastNewLine == -1)
            implicitStr += toInsert;
        else
            implicitStr = implicitStr.Insert(lastNewLine, toInsert);
        return implicitStr;
    }

    //style2 
    public static string GetItemRollRarityColor(double roll)
        => roll switch
        {
            < 20 => "#D2D2D2", //poor
            < 40 => "#E1E1E1", //common
            < 60 => "#16FF0E", //uncommon
            < 70 => "#77ACFF", //rare
            < 80 => "#A807FF", //epic
            < 95 => "#FA9E3D", //legendary
            _ => "#FA9E3D" //artifact
        };
    public static string GetItemTierColor(int tier)
        => tier switch
        {
            1 => "#D2D2D2", //poor
            2 => "#E1E1E1", //common
            3 => "#16FF0E", //uncommon
            4 => "#77ACFF", //rare
            5 => "#A807FF", //epic
            6 => "#FA9E3D", //legendary
            _ => "#FA9E3D" //artifact
        };

    private static string Modify_Custom(float roll, int tier, string finishedString)
    {
        double value = Math.Round(roll * 100.0, 1);
        string color = GetItemRollRarityColor(value);
        string tierStr = tier > 0 ? $"<color={GetItemTierColor(tier)}>[T{tier}]</color> " : "";
        string toInsert = $" {tierStr}<color={color}>[{value}%]</color>";

        int lastNewLine = finishedString.LastIndexOf("\n", StringComparison.Ordinal);
        if (lastNewLine == -1) return finishedString + toInsert;

        return finishedString.Insert(lastNewLine, toInsert);
    }

    public static string Style2_AffixRoll(this string affixStr, ItemAffix affix)
    {
        float roll = affix.getRollFloat();
        return Modify_Custom(roll, affix.DisplayTier, affixStr);
    }

    public static string Style2_AffixRoll_Unique(this string affixStr, ItemDataUnpacked item, int uniqueModIndex, float modifierValue)
    {
        if (item.uniqueID > UniqueList.instance.uniques.Count) return affixStr;
        if (UniqueList.instance.uniques.get(item.uniqueID) is not { } uniqueEntry) return affixStr;
        UniqueItemMod uniqueMod = uniqueEntry.mods.get(uniqueModIndex);
        float min = uniqueMod.value;
        float max = uniqueMod.maxValue;
        float roll = min == max || modifierValue > max ? 1 : (modifierValue - min) / (max - min);
        return Modify_Custom(roll, -1, affixStr);
    }

    public static string Style2_Implicit(this string implicitStr, ItemDataUnpacked item, int implicitNumber)
    {
        float roll = item.getImplictRollFloat((byte)implicitNumber);
        return Modify_Custom(roll, -1, implicitStr);
    }

    //Letter_Style
    public static string GetItemRollRarityLetter(double roll)
    => roll switch
    {
        < 50 => "F", //poor
        < 70 => "C", //rare
        < 80 => "B", //epic
        < 95 => "A", //legendary
        _ => "S" //artifact
    };
    public static string GetItemRollRarityColorLetter(double roll)
    => roll switch
    {
        < 50 => "#D2D2D2", //poor
        < 70 => "#77ACFF", //rare
        < 80 => "#A807FF", //epic
        < 95 => "#FA9E3D", //legendary
        _ => "#FA9E3D" //artifact
    };

    private static string Modify_Letter(float roll, int tier, string finishedString)
    {
        double value = Math.Round(roll * 100.0, 1);
        string color = GetItemRollRarityColor(value);
        string tierStr = tier > 0 ? $"<color={GetItemTierColor(tier)}>{tier}</color>" : "";
        string toInsert = $" <color={color}>[{value}%]</color>";
        string letterText = $"[{tierStr}<color={GetItemRollRarityColorLetter(value)}>{GetItemRollRarityLetter(value)}</color>] ";

        int lastNewLine = finishedString.LastIndexOf("\n", StringComparison.Ordinal);
        if (lastNewLine == -1) 
        { 
            return letterText + finishedString + toInsert; 
        }
        return letterText + finishedString.Insert(lastNewLine, toInsert);
    }

    public static string Letter_Style_AffixRoll(this string affixStr, ItemAffix affix)
    {
        float roll = affix.getRollFloat();
        return Modify_Letter(roll, affix.DisplayTier, affixStr);
    }

    public static string Letter_Style_AffixRoll_Unique(this string affixStr, ItemDataUnpacked item, int uniqueModIndex, float modifierValue)
    {
        if (item.uniqueID > UniqueList.instance.uniques.Count) return affixStr;
        if (UniqueList.instance.uniques.get(item.uniqueID) is not { } uniqueEntry) return affixStr;
        UniqueItemMod uniqueMod = uniqueEntry.mods.get(uniqueModIndex);
        float min = uniqueMod.value;
        float max = uniqueMod.maxValue;
        float roll = min == max || modifierValue > max ? 1 : (modifierValue - min) / (max - min);
        return Modify_Letter(roll, -1, affixStr);
    }

    public static string Letter_Style_Implicit(this string implicitStr, ItemDataUnpacked item, int implicitNumber)
    {
        float roll = item.getImplictRollFloat((byte)implicitNumber);
        return Modify_Letter(roll, -1, implicitStr);
    }
}
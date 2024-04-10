
using Il2CppItemFiltering;


namespace Fallen_LE_Mods
{
    //Maybe this should be split, idk..
    public static class FallenUtils
    {


        public static void IncrementOrInitialize(Dictionary<string, int> dict, string key)
        {
            // Try to get the value for the key
            if (dict.TryGetValue(key, out int value))
            {
                // Key exists, increment the value
                dict[key]++;
            }
            else
            {
                // Key doesn't exist, initialize it to 1
                dict[key] = 1;
            }
        }

        /// <summary>
        /// find ItemFilterManager
        /// </summary>
        /// <returns>ItemFilterManager</returns>
        public static ItemFilterManager GetFilterManager
        {
            get
            {
                WorldOverlayCanvas WOC = WorldOverlayCanvas.instance;
                GameObject WOcanvas = WOC.gameObject;
                ItemFilterManager myManager = WOcanvas.GetComponent<ItemFilterManager>();
                return myManager;
            }
        }

        public static Rule MatchFilterRule(ItemDataUnpacked _item)
        {
            if (ThingsKeeper.myManager == null) { return null; }

            for (int i = ThingsKeeper.myManager.Filter.rules.Count - 1; i >= 0; i--)
            {
                Rule rule = ThingsKeeper.myManager.Filter.rules[i];

                if (!rule.isEnabled)
                {
                    continue;
                }

                if (rule.Match(_item))
                {
                    return rule;
                }
            }

            return null;
        }


        public static ItemDataUnpacked FindSimilarUniqueItemInStash(ItemDataUnpacked _item)
        {
            if (!_item.isUniqueSetOrLegendary()) { return null; };
            if (ThingsKeeper.myStash == null) { return null; }
            ItemDataUnpacked highestLPmatch = null;
            foreach (ItemContainer stashtab in ThingsKeeper.myStash.Containers)
            {
                foreach (ItemContainerEntry itemEntry in stashtab.content)
                {
                    //uniqueID 0 for non unique/sets
                    var data = itemEntry.data;
                    if (data.isUniqueSetOrLegendary() && (_item.uniqueID == data.uniqueID))
                    {
                        if (highestLPmatch == null)
                        {
                            highestLPmatch = data.getAsUnpacked();

                        }
                        else
                        {
                            if (data.legendaryPotential > highestLPmatch.legendaryPotential)
                            {
                                highestLPmatch = data.getAsUnpacked();
                            }

                        }
                    }
                }

            }
            return highestLPmatch;
        }

        /// <summary>
        /// Make a rectransform start from top left
        /// </summary>
        /// <param name="rectTransform"></param>
        public static void SetDefaultRectTransformProperties(RectTransform rectTransform)
        {
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.anchoredPosition = Vector2.zero;
        }

    }

}

using Il2CppItemFiltering;
using Il2CppLE.UI;

namespace Fallen_LE_Mods
{
    //Probably no need to refresh these on each load but idk...
    [HarmonyPatch(typeof(LoadingScreen), "Disable")]
    public class ThingsKeeper
    {
        public static ItemFilterManager myManager;
        public static ItemContainersManager myItemContainer;
        public static TabbedItemContainer myStash;
        public static void Postfix(ref LoadingScreen __instance)
        {
            myManager = FallenUtils.GetFilterManager;
            myItemContainer = ItemContainersManager.instance;
            myStash = myItemContainer.stash;
        }
    }
}

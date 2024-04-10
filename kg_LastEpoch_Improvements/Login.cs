



namespace kg_LastEpoch_Improvements
{
    public class Login
    {
        public class Functions
        {
            public static void AutoClickOnline(LE.UI.Login.UnityUI.LandingZonePanel __instance)
            {
                __instance.OnPlayOnlineClicked();
            }
        }
        public class Hooks
        {
            [HarmonyPatch(typeof(LE.UI.Login.UnityUI.LandingZonePanel), "OnOnEnable")]
            public class LandingZonePanel_OnOnEnable
            {
                [HarmonyPostfix]
                static void Postfix(ref LE.UI.Login.UnityUI.LandingZonePanel __instance)
                { 
                    if(Kg_LastEpoch_Improvements.AutoClickOnline.Value)
                    Functions.AutoClickOnline(__instance);
                }
            }
        }
    }
}
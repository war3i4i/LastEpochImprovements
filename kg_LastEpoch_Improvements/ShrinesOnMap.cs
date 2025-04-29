#if SPECIALVERSION
using Il2CppDMM;
using MelonLoader;
using Object = UnityEngine.Object;
namespace kg_LastEpoch_Improvements;
public static class ShrinesOnMap
{
    private const string Icon_Base64 = "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";
    private static Sprite icon;
    private static Sprite GetIcon()
    {
        if (icon) return icon;
        Texture2D texture = new Texture2D(1, 1);
        texture.LoadImage(Convert.FromBase64String(Icon_Base64));
        texture.Apply();
        icon = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        icon.name = "ShrinesOnMap";
        return icon;
    } 
    
    [HarmonyPatch(typeof(ShrineSync),nameof(ShrineSync.PlaceClientShrine))]
    private static class ShrineSync_MessageSyncRarit 
    { 
        private static void Postfix(ShrineSync __instance)
        {
            try 
            { 
                if (!kg_LastEpoch_Improvements.ShowShrinesOnMap.Value) return;
                GameObject customMapIcon = Object.Instantiate(kg_LastEpoch_Improvements.CustomMapIcon, DMMap.Instance.iconContainer.transform);
                customMapIcon.SetActive(true);
                customMapIcon.GetComponent<kg_LastEpoch_Improvements.CustomIconProcessor>().Init(__instance.ShrineObject.gameObject, null);
                if (__instance.ShrineObject?.GetComponent<DisplayInformation>() is {} info) 
                    customMapIcon.GetComponent<kg_LastEpoch_Improvements.CustomIconProcessor>().SetCustomText(info.description, Color.green, 12);
                customMapIcon.GetComponent<Image>().enabled = false;
                customMapIcon.transform.GetChild(0).GetComponent<Image>().sprite = GetIcon();
                customMapIcon.name = $"shrine_{__instance.gameObject.name}";
            }
            catch (Exception ex)
            {
            }
        }
    }
}
#endif
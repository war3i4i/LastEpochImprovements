using Il2CppItemFiltering;
using Il2CppLE.UI.Controls;
using Il2CppTMPro;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace kg_LastEpoch_Improvements;

public static class CustomDropSounds
{
    private static readonly string CustomSoundMapperPath = Path.Combine(MelonEnvironment.UserDataDirectory, "CustomDropSounds", "CustomSoundMapper.json");
    private static Dictionary<string, string> RuleToSound = [];
    private static readonly Dictionary<string, AudioSource> Sounds  = [];
    private static AudioSource CreateAudioSource(AudioClip clip)
    {
        GameObject audioSource = new GameObject("kg_AudioSource") { hideFlags = HideFlags.HideAndDontSave };
        AudioSource source = audioSource.AddComponent<AudioSource>();
        source.reverbZoneMix = 0;
        source.spatialBlend = 0;
        source.bypassListenerEffects = true;
        source.bypassEffects = true;
        source.volume = 1f;
        source.clip = clip;
        return source;
    }
    private static void LoadSoundsSync()
    {
        string AudioFilesPath = Path.Combine(MelonEnvironment.UserDataDirectory, "CustomDropSounds");
        if (!Directory.Exists(AudioFilesPath)) Directory.CreateDirectory(AudioFilesPath);
        MelonCoroutines.Start(LoadCustomDropSounds(AudioFilesPath));
    }
    private static IEnumerator LoadCustomDropSounds(string path)
    {
        if (Sounds.Count > 0)
        {
            foreach (KeyValuePair<string, AudioSource> kvp in Sounds)
                Object.Destroy(kvp.Value.gameObject);
            Sounds.Clear(); 
        }
        string[] files = Directory.GetFiles(path, "*.mp3", SearchOption.AllDirectories)
                 .Concat(Directory.GetFiles(path, "*.wav", SearchOption.AllDirectories))
                 .Concat(Directory.GetFiles(path, "*.ogg", SearchOption.AllDirectories)).ToArray();
        for(int i = 0; i < files.Length; i++)
        {
            string fNameNoExt = Path.GetFileNameWithoutExtension(files[i]);
            if (fNameNoExt.Contains(' ') || fNameNoExt.Contains(':'))
            {
                MelonLogger.Warning($"Custom drop sound file name cannot contain spaces or colons: {fNameNoExt}");
                continue;
            }
            UnityWebRequest www = UnityWebRequest.Get($"file://{files[i]}");
            var request = www.SendWebRequest();
            while (!request.isDone) { }
            if (www.isNetworkError || www.isHttpError) continue;
            AudioClip clip = WebRequestWWW.InternalCreateAudioClipUsingDH(www.downloadHandler, www.url, false, true, AudioType.UNKNOWN);
            if (clip)
            {
                clip.name = Path.GetFileNameWithoutExtension(files[i]);
                Sounds[fNameNoExt] = CreateAudioSource(clip);
            }
        }
        MelonLogger.Msg($"Loaded {Sounds.Count} custom drop sounds");
        yield break;
    }
    public static bool TryPlaySoundDelayed(string name, float delay, float volume)
    {
        if (Sounds.Count == 0 || !Sounds.TryGetValue(name, out var sound)) return false;
        MelonCoroutines.Start(DelaySound(sound, delay, volume));
        return true;
    }
    private static IEnumerator DelaySound(AudioSource source, float delay, float volume)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        source.PlayOneShot(source.clip, volume);
    }
    public static void Load()
    { 
        fastJSON.JSON.Parameters = new fastJSON.JSONParameters
        {
            UseExtensions = false,
            SerializeNullValues = false,
            UseOptimizedDatasetSchema = true,
            UseValuesOfEnums = true,
        };
        LoadSoundsSync();
        if (File.Exists(CustomSoundMapperPath))
        {
            try
            {
                string json = File.ReadAllText(CustomSoundMapperPath);
                RuleToSound = fastJSON.JSON.ToObject<Dictionary<string, string>>(json);
            }
            catch (Exception) { RuleToSound = []; }
        }
    }
    [HarmonyPatch(typeof(RuleUI),nameof(RuleUI.Awake))]
    private static class RuleUI_Awake_Patch
    {
        public static KeyValuePair<GameObject, ColoredIconDropdown> Dropdown;
        public static List<string> LastOrdered  = [];
        private static void Postfix(RuleUI __instance)
        {  
            if (Utils.CopyFrom_Dropdown)
            {
                Transform targetParent = __instance.transform.Find("Content/LeftRulePanel");
                GameObject copyFromDropdown = Object.Instantiate(Utils.CopyFrom_Dropdown, targetParent);
                copyFromDropdown.name = "CopyFromDropdown";
                copyFromDropdown.transform.SetAsLastSibling();
                Object.DestroyImmediate(copyFromDropdown.transform.GetChild(2).gameObject);
                Object.DestroyImmediate(copyFromDropdown.transform.GetChild(0).gameObject);
                ColoredIconDropdown dropdown = copyFromDropdown.transform.GetChild(1).GetComponent<ColoredIconDropdown>();
                dropdown.onValueChanged.RemoveAllListeners();
                dropdown.ClearOptions();
                
                RectTransform rect = copyFromDropdown.GetComponent<RectTransform>(); 
                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(0, 0);
                rect.pivot = new Vector2(0, 0); 
                rect.anchoredPosition = new Vector2(10f, -80f);
                rect.GetComponent<VerticalLayoutGroup>().spacing = -10f; 
                copyFromDropdown.transform.GetChild(0).GetComponent<TMP_Text>().fontSize = 16;
                copyFromDropdown.transform.GetChild(0).GetComponent<TMP_Text>().color = Color.green;
                copyFromDropdown.transform.GetChild(0).GetComponent<TMP_Text>().text = "Custom Sound";
                Dropdown = new KeyValuePair<GameObject, ColoredIconDropdown>(copyFromDropdown, dropdown);
                LastOrdered = new List<string>(Sounds.Keys);
                LastOrdered.Sort((x, y) => string.Compare(x, y, StringComparison.InvariantCultureIgnoreCase));
                LastOrdered.Insert(0, "None");
                Il2CppSystem.Collections.Generic.List<string> options  = new();
                foreach (var sound in LastOrdered) options.Add(sound);
                dropdown.AddOptions(options);
                dropdown.value = 0;
            }
            else
            {
                MelonLogger.Error("Copyfrom dropdown is null");
            }
        }
    }
    [HarmonyPatch(typeof(RuleUI),nameof(RuleUI.Init))]
    private static class RuleUI_Init_Patch
    {
        private static void Postfix(Rule rule)
        {
            if (rule == null) return;
            if (!RuleUI_Awake_Patch.Dropdown.Value) return;
            if (Sounds.Count == 0)
            {
                RuleUI_Awake_Patch.Dropdown.Key.SetActive(false);
                return;
            }
            RuleUI_Awake_Patch.Dropdown.Key.SetActive(true);
            ColoredIconDropdown dropdown = RuleUI_Awake_Patch.Dropdown.Value;
            if (RuleHasCustomSound(rule, out string sName))
            {
                int index = RuleUI_Awake_Patch.LastOrdered.IndexOf(sName);
                dropdown.value = index != -1 ? index : 0;
            }
            else dropdown.value = 0;
        }
    }
    [HarmonyPatch(typeof(RuleUI),nameof(RuleUI.CloseApplyButton))]
    private static class RuleUI_UpdateConfirmButtonLabel_Patch
    {
        private static void Prefix(RuleUI __instance)
        { 
            if (__instance.rule == null || !RuleUI_Awake_Patch.Dropdown.Value) return;
            string ruleName = __instance.rule.nameOverride.Trim();
            if (string.IsNullOrWhiteSpace(ruleName)) return;
            int index = RuleUI_Awake_Patch.Dropdown.Value.value;
            if (index < 0 || index >= RuleUI_Awake_Patch.LastOrdered.Count) return;
            if (RuleUI_Awake_Patch.Dropdown.Value.value == 0)
            {
                if (RuleToSound.ContainsKey(ruleName)) RuleToSound.Remove(ruleName);
            }
            else
            {
                string soundName = RuleUI_Awake_Patch.LastOrdered[index];
                RuleToSound[ruleName] = soundName;
            }
            string json = fastJSON.JSON.ToNiceJSON(RuleToSound);
            try
            {
                Task.Run(() => File.WriteAllText(CustomSoundMapperPath, json));
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Failed to write custom sound mapper file: {e}");
            }
        }
    }
    public static bool RuleHasCustomSound(Rule rule, out string sName) => RuleToSound.TryGetValue(rule.nameOverride.Trim(), out sName);
}
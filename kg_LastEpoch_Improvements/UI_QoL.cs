#if SPECIALVERSION
using Il2CppLE.Services.Bazaar;
using Il2CppLE.UI.Bazaar;
using Il2CppLE.UI.MultiPicker;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;
using State = Il2CppLE.UI.MultiPicker.State;

namespace kg_LastEpoch_Improvements;
 
public static class UI_QoL
{
    [HarmonyPatch(typeof(EnableWovenEchoesTabIfRelevant),nameof(EnableWovenEchoesTabIfRelevant.Awake))]
    private static class EnableWovenEchoesTabIfRelevant_Awake_Patch
    {
        private const string container_base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAABHNCSVQICAgIfAhkiAAAAAlwSFlzAAAAdgAAAHYBTnsmCAAAABl0RVh0U29mdHdhcmUAd3d3Lmlua3NjYXBlLm9yZ5vuPBoAAAI6SURBVDiNpZNbS9RRFMV/55z/TRrN0ZkGJjWjSSIi6EEJfAuCqF40X+qxb9FX8HPUq8+V+CZkkaBipZEXVJiYdK7N5X855/TgJIkkQRs27A17sxZrrw3/GeJ3YZemh8oH8cXSdidz3kJ61C3nRoKKuDt3AODYpekhrJ1beXM4vvWxjtH2XMRKR5PJB9h3Ux/QekqCna0Uw/Ht5TrCgpLi3MQKivsdtlYbEyg1K0l7TyIrkFIindM5UBghczN/0msAY3Cx7H1pAdx38KXnpBxMYk5RLTy6x42ZhwjW2Zn/zOrL95RbmsQey9asJgBZYbeeWpSA9OCxptUySbGJzD+gri/j9+ziJ98x5U1USiEGMhDH2P0jRKhxWClBjwt9bZASak2cegileT6Vb5EdrjJWWkMGCnwFvR2INeKoBbHBIaVYX6yyuVLEWgtCMPM8Te3qYyYmBUosE1XG8NY2ePvqiEZdkySWoSsek3c8JPmLeBc8Atelx/PQXS32FhdQrEIrYuP1DwCCwCFwXVyliDvH53bodXAChZBdZ3VtYHa+sfBil04jQiQht5/1o5RASBACJGB1jOQwgkgDEGuD6ZpTAHG5TdKIzpgp6bLUtQqS2EAXNWzrM8PtMPmjs1hrCcMETIJp1pBUGhatiWJN59QwtNrxCRotAwn8bMYYCzYKQQgcYjGUSonspWGvkI5dzw/cJsZZTw/GA7l8UJBC+LmsNGi1N3otyPX2metKaj/dL0oy53/9y8v8e/wC6K7/egpop1wAAAAASUVORK5CYII=";
        private const string trader_base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAABHNCSVQICAgIfAhkiAAAAAlwSFlzAAAAdgAAAHYBTnsmCAAAABl0RVh0U29mdHdhcmUAd3d3Lmlua3NjYXBlLm9yZ5vuPBoAAANJSURBVDiNjZJdTFsFGIbf89Oe/p620gIFSllbKOtgBQpjAjq26DQYM2XonDom2ZUxi/EKdDfELHqxXSz7idnFsmRZNGTJ2MT5N53ZyCLidEXcRihYELCU9pT+nvaUnnO82YjRbfG9e7/kfd7k+z4C/1/KClfJvnJH2e5cRjBP3LzbASD/uIDVbDZuf2Bqm5yfnbj6oTiydFbufvv5AAAKAMhHhOnWnQ3Dbw3uGfZsqRkEgEQsPZ6Kp9cAIMGllgGIeED5t9xNjo97+3t6RkfGIweP9m2fv7dQWrfV/SafzuWGjl2ejHPpm5FF7ruHVpvNevf+D3oWntnz1G8MwzhbdzbeOnXtsNx/+h3OVWc/8J/AplbXgMNr775vieYd9dcPnTkYr3SXv3J/ZvB11l+obXENPKyQ8rRUn9zR3bY3urwKk4Vt7x3o2Xfh5JdXLUJhMwU0brAaXt9URde1bNS3e2qL+iiSsPy1nLmxDuBTmTGKUnQ998a2Ltao8wYmgtHRz8cP9T3b+pG3yuRzeDQN5fWNxTHJZdCoVSV7X7RsnQ0mV0PhzM8AQEaW4v6frvm7rl/8cd7mLi+aHAscB+CfWgxnVTpS3f6klVjVdKC3/xh2vXcGk7MKdbWTfWn9XACQCCeCoyPj2+76Zzu5Be5yh6fqk0xWyH11a47e2OZltEYTAIBhVBAKJERJVq0D9nfXBJwVbFlaEuOSJIlff0seffXpZmdNqRZcPIaZ6Sgi4iV8Ovc7ctEZ7GoXxZFLian1HRh1zPs2q1YVWuFps1Gt4rNysUjmSZHP4ht/EJUGI6R8CD5nTCLWVpYHj0wo9ILGW6nXNyylkhdph02vrLazTCKZ17R5LWhuLkEuK+LGlTBYtRIVOgN8hjIMfzHNH7/yy+6momLngZoN5zwV9pcHRr8/T1MkKJOeobRqGiZWhYKagCCIyAhrSPJ55AsiGAUFu5mlrCbd5l+5ldPFC9pOEkQvq1RWU0+Y1O/mChJ9eypKyrJMTAVSxJ07MehkBq+1eVBq1AIAbGZWEU3yvsk/I2dnkvEhTshOT4cWD9MvdNruyRI8gaDCJIqgCwVZFkXwP9yez84tpVb++XV/hFdDABIAMBYJDwHA3x7WUGiKHpxbAAAAAElFTkSuQmCC";
        private const string sellall_base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAABHNCSVQICAgIfAhkiAAAAAlwSFlzAAAAdgAAAHYBTnsmCAAAABl0RVh0U29mdHdhcmUAd3d3Lmlua3NjYXBlLm9yZ5vuPBoAAAJVSURBVDiNlVBPSBRhHH3zzezO7mw607bquqSDmpj5hyXwUFEdopCyxAgCgw55qig8dZBAIoIoOkSIQYc8eAjEmx7SCCJChEzDBWtxl23B1dZdnXV3x92Zb2Y6hLKaf+h3+vjeH957TKNT/HhcEKvwn2dacIRzmSTnF8TqnvI6uS8ehp0QdJfWYGh1AVFNhUA4dLoPY5lq8AsiAGBEWUKb5AW1LNwLfZc4Alivl8N44K3FmkkxrMQQ1VT0+uoBADNqChFN3TSYUlfRJnkBAAwAAgBnijzojc3hfeo3Ot0VWKE6HsXmMLy6sG8VDgBE1oYXFU2YzK7gbeIX3JxtS4J9DSJ5FSPKEjImRYfkQzCXwUAiCjth0OAoxqf0MhSq4+QBN5JUx0AiCoGwf2u0imWhftlfnTYoWIaBQFgoho41g2LMSqGRc8FncAAAD2dHzjSQMQ3AtPA0FswwpTx/vozwl7dH0y2Tv3LjXPvE1x9ZJbg4uh13MVxx0qBDXDyfH48jP75DvYNXefslzsWraolrNhRdfLPTBuwe+zhPtzTcEZy87D9WdSKTXT+bp/mJXI4qhSSyi9i+8ZCKXbYaubyk/8ndNu8hz+S11lNbknDblS3NtS+b6uX26UB4HoRhBBsPh8NOxCIBXdcvlAaCkYt7GpimKXXfavfVVHrldHYdZR5pE6uuLMfth31b+P9UmAqEujrvP3/34ctMqlC82+20AQ38jN7seTY4+G12XisELMuCrlNa+MfsYc62NB8ZbTwq1xHCgrFAcprmjCdSj8c+T7/aIP0BVmTkKWrbqVEAAAAASUVORK5CYII=";
        private static void CreateButton(GameObject copy, string name, Sprite icon, Color backgroundColor, Action onPress)
        {
            GameObject newObj = UnityEngine.Object.Instantiate(copy, copy.transform.parent);
            newObj.name = name; 
            UnityEngine.Object.DestroyImmediate(newObj.GetComponent<SortInventoryButton>());
            newObj.transform.GetChild(0).GetComponent<Image>().color = backgroundColor;
            newObj.transform.GetChild(1).GetComponent<Image>().sprite = icon;
            LayoutElement layoutElement = newObj.transform.GetChild(1).GetComponent<LayoutElement>();
            layoutElement.preferredWidth = layoutElement.preferredHeight;
            UnityEngine.Object.DestroyImmediate(newObj.transform.GetChild(2).GetComponent<LocalizeStringEvent>());
            newObj.transform.GetChild(2).GetComponent<TMP_Text>().text = name;
            Button button = newObj.GetComponent<Button>();
            button.onClick.RemoveAllListeners(); 
            button.onClick.AddListener(onPress);
        }

        private static void InsertEmptySpace(Transform parent, int width)
        {
            GameObject emptySpace = new GameObject("EmptySpace");
            emptySpace.transform.SetParent(parent);
            emptySpace.AddComponent<LayoutElement>().preferredWidth = width;
        }
        
        private static void Postfix(EnableWovenEchoesTabIfRelevant __instance)
        {
            GameObject copy = __instance.transform.Find("Tab Contents/Items Tab/Inventory Footer/Left_Buttons_Container/Sort").gameObject;
            CreateButton(copy, "Stash", container_base64.ToSprite(), Color.green, () =>
            {
                if (UIBase.instance.stashPanel.instance && UIBase.instance.stashPanel.instance.active) UIBase.instance.closeStash(true);
                else UIBase.instance.openStash(true);
            });
            CreateButton(copy, "Trader", trader_base64.ToSprite(), Color.yellow, () =>
            {
                if (UIBase.instance.shop.instance && UIBase.instance.shop.instance.active) UIBase.instance.closeShop();
                else UIBase.instance.openShop(true);
            }); 
            InsertEmptySpace(copy.transform.parent, 20);
            CreateButton(copy, "Sell All", sellall_base64.ToSprite(), Color.red, () =>
            {
                MelonCoroutines.Start(SellAll());
            });
        }
    }  
    private static IEnumerator SellAll()
    {
        if (ItemContainersManager.Instance.inventory == null) yield break;
        if (!UIBase.instance.shop.instance.active)
        {
            UIBase.instance.openShop();
            UIBase.instance.closeShop();
        }
        Vector2Int[] positions = ItemContainersManager.Instance.inventory.content.ToArray().Select(i => i.Position).ToArray();
        for (int i = 0; i < positions.Length; i++)
        {
            ItemContainersManager.Instance.tryManualQuickMove(ContainerID.INVENTORY, ContainerID.SHOP, positions[i], false);
            yield return null; yield return null; yield return null;
        }
    }
}
#endif
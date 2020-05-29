using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.UserInterfaceWindows;

public class LGSRegisterTextWindows : MonoBehaviour
{
    static Mod mod;

    public void Awake()
    {
        mod.IsReady = true;
    }

    public void Start()
    {
        UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Trade, typeof(LGSTradeWindowText));
        UIWindowFactory.RegisterCustomUIWindow(UIWindowType.MerchantRepairPopup, typeof(LGSMerchantTradeRepairPopupWindowReplace));
        UIWindowFactory.RegisterCustomUIWindow(UIWindowType.MerchantServicePopup, typeof(LGSMerchantTradeServicePopupWindowReplace));
        Debug.Log("registered windows");
    }
    
    [Invoke(StateManager.StateTypes.Start, 0)]
    public static void Init(InitParams initParams)
    {
        mod = initParams.Mod;
        var go = new GameObject(mod.Title);
        go.AddComponent<LGSRegisterTextWindows>();
    }
}

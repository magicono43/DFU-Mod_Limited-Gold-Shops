using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.UserInterfaceWindows;

using DaggerfallWorkshop.Utility.AssetInjection;
using System.IO;

public class RegisterInventoryWindow : MonoBehaviour
{
    static Mod mod;

    public void Awake()
    {
        mod.IsReady = true;
    }

    // Alright, so a lot of information to absorb today. Tomorrow I will try and use convinent clock as an example of how to add a "loose" texture. With that I will attempt to replace the merchant repair services pop-up box with something else and see how that goes, then go from there.

    public void Start()
    {
        UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Inventory, typeof(AsesinoInventoryWindow));
        UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Trade, typeof(AsesinoTradeWindow));
        
        UIWindowFactory.RegisterCustomUIWindow(UIWindowType.MerchantRepairPopup, typeof(KirkMerchantTradeRepairPopupWindow));
        //UIWindowFactory.RegisterCustomUIWindow(UIWindowType.MerchantServicePopup, typeof(KirkMerchantTradePopupWindow));
        Debug.Log("registered windows");
    }
    
    [Invoke(StateManager.StateTypes.Start, 0)]
    public static void Init(InitParams initParams)
    {
        mod = initParams.Mod;
        var go = new GameObject(mod.Title);
        go.AddComponent<RegisterInventoryWindow>();
    }
}

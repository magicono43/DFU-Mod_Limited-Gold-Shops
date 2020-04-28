// Project:         SaveDataTestingScript1 mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2020 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Version:			v.1.10
// Created On: 	    4/21/2020, 7:35 PM
// Last Edit:		3/26/2020, 1:15 AM
// Modifier:		

using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using DaggerfallWorkshop.Game.Serialization;
using FullSerializer;

namespace SaveDataTestingScript1
{
    [FullSerializer.fsObject("v1")]
    public class TestingSaveData
    {
        public Dictionary<int, ShopData> ShopBuildingData;
    }

    public class ShopData {
        public int BuildingKey;
        public int BuildingQuality;
        public int CurrentGoldSupply;
    }

    public class SaveDataTestingScript1 : MonoBehaviour, IHasModSaveData
    {
        static Mod mod;
        static SaveDataTestingScript1 instance;

        public static Dictionary<int, ShopData> ShopBuildingData;

        public static int FlexCurrentGoldSupply { get; set; }
        public static int StupidTest { get; set; }

        public Type SaveDataType
        {
            get { return typeof(TestingSaveData); }
        }

        public object NewSaveData()
        {
            return new TestingSaveData
            {
                ShopBuildingData = new Dictionary<int, ShopData>()
            };
        }

        public object GetSaveData()
        {
            return new TestingSaveData
            {
                ShopBuildingData = ShopBuildingData
            };
        }

        public void RestoreSaveData(object saveData)
        {
            var testingSaveData = (TestingSaveData)saveData;
            Debug.Log("Restoring save data");
            ShopBuildingData = testingSaveData.ShopBuildingData;
            Debug.Log("Save data restored");
        }

        // Variables?

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            var go = new GameObject("SaveDataTestingScript1");
            //go.AddComponent<SaveDataTestingScript1>();
            instance = go.AddComponent<SaveDataTestingScript1>();
            mod.SaveDataInterface = instance;
            if (ShopBuildingData == null) 
            {
                ShopBuildingData = new Dictionary<int, ShopData>();
            }
            PlayerEnterExit.OnTransitionInterior += Testing_OnTransitionInterior;

            GameManager.Instance.PlayerEffectManager.OnNewReadySpell += Testing_OnNewReadySpell;

            IUserInterfaceManager uiManager = DaggerfallUI.UIManager;
            uiManager.OnWindowChange += WindowChanged; // Now I need to figure out how I can look at whatever file is being created, if any.
            mod.IsReady = true;
        }

        void Awake()
        {
            InitMod();
            //ShopBuildingData = new Dictionary<int, ShopData>();
        }

        static private int buildingKey = 0;
        static private int mapID = 0;
        static private int currentGoldSupply = 0;

        private static void InitMod()
        {
            Debug.Log("Begin mod init: SaveDataTestingScript1");
            
            Debug.Log("Finished mod init: SaveDataTestingScript1");
        }

        private static void Testing_OnNewReadySpell(EntityEffectBundle spell)
        {
            Debug.Log("You just made a spell ready.");
            return;
        }

        public static void WindowChanged(object sender, EventArgs e) // Look about possibly optimizing this somewhat, if needed, etc. Figure out how to change a variable on the trade event, based on how much gold was given or recieved from a sale.
        {
            UserInterfaceManager uiManager = sender as UserInterfaceManager;
            DaggerfallTradeWindow window = uiManager.TopWindow as DaggerfallTradeWindow;
            if (window != null)
            {
                Type windowType = window.GetType();
                Debug.Log("Custom activation - Window of type: " + windowType.Name); // Hmm, I may have to think of a more "clever" way to deduct/increase the amount of gold that the shop currently has in supply, instead of from the trade event. Having to check for window changes is pretty costly.
                window.OnTrade += Trading; // Perhaps if I can differenciate between the pop-up windows, I can specify to only run my "gold tracking" code when the "Yes" button is clicked for the trade confirmation pop-up.
                if (StupidTest == 1)
                {
                    window.OnTrade -= Trading; // Alright, so it seems to be working better this time. However, if I buy (possibly sell as well?) multiple items in one transaction, it causes the issue again, so there might be something going on with the "basket" variables or something, most likely it is being caused by the "loop" that happens to calculate multiple items at once, will have to look at that and see, hopefully can think of a way to get around this as well.
                    StupidTest = 0; // Alright, I see now, it's not due to the amount of items in the basket like I thought, it's actually due to more windows/pop-ups from coming up during the transaction. This counts the pop-up that says "you are forbidden from using the material/weapon, etc." So now I have to see how I can get around that, i'm glad this might be easier to get around.
                }
            }
        }

        private static void Trading(DaggerfallTradeWindow.WindowModes mode, int numItems, int value)
        {
            int newValue;
            Debug.LogFormat("Sale Value = {0}", value); // Perfect, "value" gives the value of the final transaction, just what I need.
            if (mode == DaggerfallTradeWindow.WindowModes.Buy)
                newValue = FlexCurrentGoldSupply + value;
            else
                newValue = FlexCurrentGoldSupply - value;
            Debug.LogFormat("Num of Items in Cart/Basket = {0}", numItems);
            Debug.LogFormat("Shop Gold Supply After Sale = {0}", newValue);
            FlexCurrentGoldSupply = newValue; // Math is working now, but issue is now that if you stay in the same trade window and make multiple different purchases, this parts of the script starts running multiple times based on how many transactions you made in that session, for whatever reason.
            StupidTest = 1;
            return;
        }

        private static void Testing_OnTransitionInterior(PlayerEnterExit.TransitionEventArgs args)
        {
            int buildingQuality = 0;
            int buildingKey = 0;
            int mapID = 0;
            int currentGoldSupply = 0;
            
            buildingQuality = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.quality;
            buildingKey = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingKey;
            mapID = GameManager.Instance.PlayerGPS.CurrentLocation.MapTableData.MapId;

            if (GameManager.Instance.PlayerEnterExit.IsPlayerInsideOpenShop)
            {
                Debug.Log("You Just Entered An Open Shop, Good Job!...");
                currentGoldSupply = GoldSupplyAmountGenerator(buildingQuality);
                FlexCurrentGoldSupply = currentGoldSupply; // On transition, sets the shops gold supply to this "global variable" for later uses.
                if (!ShopBuildingData.ContainsKey(buildingKey)) {
                    ShopData testbuildKey = new ShopData {
                        BuildingKey = buildingKey,
                        BuildingQuality = buildingQuality,
                        CurrentGoldSupply = currentGoldSupply
                    };
                    Debug.Log("Adding building " + buildingKey.ToString() + " - quality: " + buildingQuality.ToString());
                    ShopBuildingData.Add(buildingKey, testbuildKey);
                } else {
                    ShopData sd;
                    if(ShopBuildingData.TryGetValue(buildingKey, out sd)) {
                        Debug.Log("Building " + buildingKey.ToString() + " is present - quality = " + sd.BuildingQuality.ToString());
                    }
                }
            }
            else
                Debug.Log("You Just Entered Something Other Than An Open Shop...");

            //Debug.LogFormat("mapID = {1} and buildingKey = {0}. The Quality of the building is {2}. currentGoldSupply = {3}.", buildingKey, mapID, buildingQuality, currentGoldSupply); // Higher Quality value = better shop quality.
            //Debug.LogFormat("shopBuildingData Dictionary Data = {0}.", shopBuildingData);
        } // So MapID and BuildingKey seem to be what is used to identify a specific "Scene" or building interior. So these would likely be needed to track different shops.

        /*void OnDestroy()
        {
            PlayerEnterExit.OnTransitionInterior -= Testing_OnTransitionInterior;
            GameManager.Instance.PlayerEffectManager.OnNewReadySpell -= Testing_OnNewReadySpell;

            IUserInterfaceManager uiManager = DaggerfallUI.UIManager;
            uiManager.OnWindowChange -= WindowChanged;
            DaggerfallTradeWindow window = uiManager.TopWindow as DaggerfallTradeWindow;
            window.OnTrade -= Trading;

            /*UserInterfaceManager uiManager = DaggerfallUI.Instance.UserInterfaceManager;
            DaggerfallTradeWindow testing = new DaggerfallTradeWindow(uiManager, null, DaggerfallTradeWindow.WindowModes.Buy, null);
            uiManager.PushWindow(testing);
            testing.OnTrade -= Testing_OnTrade;*/
        /*}*/

        public static int GoldSupplyAmountGenerator(int buildingQuality)
        {
            int currentRegionIndex = GameManager.Instance.PlayerGPS.CurrentRegionIndex;
            int regionCostAdjust = GameManager.Instance.PlayerEntity.RegionData[currentRegionIndex].PriceAdjustment / 100;
            float varianceMaker = (UnityEngine.Random.Range(1.001f, 2.001f));

            return (int)Mathf.Ceil(((buildingQuality * 2) + (regionCostAdjust + 1)) * (float)(500 * varianceMaker));
        }
    }
}
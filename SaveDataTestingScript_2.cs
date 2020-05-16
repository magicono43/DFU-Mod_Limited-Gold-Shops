// Project:         SaveDataTestingScript1 mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2020 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Version:			v.1.10
// Created On: 	    4/21/2020, 7:35 PM
// Last Edit:		5/5/2020, 1:00 PM
// Modifier:
// Special Thanks:  BadLuckBurt, Hazelnut, Ralzar, LypyL, Pango

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
        public ulong CreationTime;
        public bool InvestedIn;
        public uint AmountInvested;
        public int ShopAttitude;
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

            WorldTime.OnNewDay += Testing_OnNewDay;

            IUserInterfaceManager uiManager = DaggerfallUI.UIManager;
            uiManager.OnWindowChange += WindowChanged;
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
            ulong creationTime = 0;
            int buildingQuality = 0;
            bool investedIn = false;
            uint amountInvested = 0;
            int shopAttitude = 0;
            int buildingKey = 0;
            int currentGoldSupply = 0;
            int currentBuildingID = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingKey;

            ShopData sd;
            if(ShopBuildingData.TryGetValue(currentBuildingID, out sd))
            {
                buildingKey = currentBuildingID;
                creationTime = sd.CreationTime;
                investedIn = sd.InvestedIn; // Try out putting my own action box area over the sell button and see what happens, i'm going to get both trigger, but will have to see.
                amountInvested = sd.AmountInvested; // Could possibly sum the total gold value of a trade and disallow this from running until the final "stack" of items is done looping, as to reduce performance running this multiple times. // Not Enough Gold message, and possibly Yes/No pop-up about accepting what they have left. // So the values seem to be updating "properly" now, still need to have the UI value change as the window is still open, but that's for later. I would like a different "cleaner" and faster way to solve this value update issue, but this seems to work fine for the time being at least.
                shopAttitude = sd.ShopAttitude;
                buildingQuality = sd.BuildingQuality;
            }
            Debug.LogFormat("Sale Value = {0}", value); // Perfect, "value" gives the value of the final transaction, just what I need.
            if (mode == DaggerfallTradeWindow.WindowModes.Buy)
            {
                currentGoldSupply = sd.CurrentGoldSupply + value;
                ShopBuildingData.Remove(buildingKey);

                ShopData testbuildKey = new ShopData
                {
                    CreationTime = creationTime,
                    InvestedIn = investedIn,
                    AmountInvested = amountInvested,
                    ShopAttitude = shopAttitude,
                    BuildingQuality = buildingQuality,
                    CurrentGoldSupply = currentGoldSupply
                };
                ShopBuildingData.Add(buildingKey, testbuildKey);
            }
            else
            {
                currentGoldSupply = sd.CurrentGoldSupply - value;
                ShopBuildingData.Remove(buildingKey);

                ShopData testbuildKey = new ShopData
                {
                    CreationTime = creationTime,
                    InvestedIn = investedIn,
                    AmountInvested = amountInvested,
                    ShopAttitude = shopAttitude,
                    BuildingQuality = buildingQuality,
                    CurrentGoldSupply = currentGoldSupply
                };
                ShopBuildingData.Add(buildingKey, testbuildKey);
            }
            Debug.LogFormat("Num of Items in Cart/Basket = {0}", numItems);
            Debug.LogFormat("Shop Gold Supply After Sale = {0}", currentGoldSupply);
            FlexCurrentGoldSupply = currentGoldSupply; // Math is working now, but issue is now that if you stay in the same trade window and make multiple different purchases, this parts of the script starts running multiple times based on how many transactions you made in that session, for whatever reason.
            StupidTest = 1;
            return;
        }

        public static void Testing_OnNewDay()
        {
            List<int> expiredKeys = new List<int>();
            ulong creationTime = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToSeconds();

            foreach (KeyValuePair<int, ShopData> kvp in ShopBuildingData)
            {
                ulong timeLastVisited = creationTime - kvp.Value.CreationTime;
                if (!kvp.Value.InvestedIn) // Only deletes expired entries with "OnNewDay" if the "InvestedIn" value inside ShopBuildingData is "false", if the shop has been invested in though, it will not be deleted in this way, only will it be deleted/refreshed when the player enters the expired shop in question, which will generate the gold value based on the amount invested in store, etc.
                {
                    if (timeLastVisited >= 1296000) // 15 * 86400 = Number of seconds in 15 days.
                    {
                        Debug.Log("Expired Building Key " + kvp.Key.ToString() + " Detected, Adding to List");
                        expiredKeys.Add(kvp.Key);
                    }
                }
            }

            foreach (int expired in expiredKeys) // Sweet it's working perfectly with this "2-step" approach, now to refine this and work with the other features as needed, this was a pretty big one though. Likely make forum post about this for the possible benefit of other modders.
            {
                Debug.Log("Removed Expired Building Key " + expired.ToString());
                ShopBuildingData.Remove(expired);
            }
        }

        private static void Testing_OnTransitionInterior(PlayerEnterExit.TransitionEventArgs args)
        {
            ulong creationTime = 0;
            int buildingQuality = 0;
            bool investedIn = false;
            uint amountInvested = 0;
            int shopAttitude = 0;
            int buildingKey = 0;
            int mapID = 0;
            int currentGoldSupply = 0;

            creationTime = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToSeconds();
            amountInvested = 0; // Will change this to be set whenever the player does the "invest" feature that will be added to the store menu, for right now just have it set to 0 until then.
            buildingQuality = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.quality;
            buildingKey = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingKey;
            mapID = GameManager.Instance.PlayerGPS.CurrentLocation.MapTableData.MapId;

            if (GameManager.Instance.PlayerEnterExit.IsPlayerInsideOpenShop)
            {
                Debug.Log("You Just Entered An Open Shop, Good Job!...");
                currentGoldSupply = GoldSupplyAmountGenerator(buildingQuality);
                shopAttitude = UnityEngine.Random.Range(0, 2); // Mostly have it as an int instead of a bool, just incase I want to add more "attitude types" to this later on.
                Debug.Log("Building Attitude Rolled: " + shopAttitude.ToString());
                FlexCurrentGoldSupply = currentGoldSupply; // On transition, sets the shops gold supply to this "global variable" for later uses.

                if (!ShopBuildingData.ContainsKey(buildingKey)) {
                    ShopData testbuildKey = new ShopData {
                        CreationTime = creationTime,
                        InvestedIn = investedIn,
                        AmountInvested = amountInvested,
                        ShopAttitude = shopAttitude,
                        BuildingQuality = buildingQuality,
                        CurrentGoldSupply = currentGoldSupply
                    };
                    Debug.Log("Adding building " + buildingKey.ToString() + " - quality: " + buildingQuality.ToString());
                    ShopBuildingData.Add(buildingKey, testbuildKey);
                } else {
                    ShopData sd;
                    if(ShopBuildingData.TryGetValue(buildingKey, out sd)) {
                        ulong timeLastVisited = creationTime - sd.CreationTime;
                        if(timeLastVisited >= 1296000) { // 15 * 86400 = Number of seconds in 15 days.
                            ShopBuildingData.Remove(buildingKey);
                            Debug.Log("Removed Expired Building Key " + buildingKey.ToString() + " Generating New Properties");
                            ShopData testbuildKey = new ShopData {
                                CreationTime = creationTime,
                                InvestedIn = investedIn,
                                AmountInvested = amountInvested,
                                ShopAttitude = shopAttitude,
                                BuildingQuality = buildingQuality,
                                CurrentGoldSupply = currentGoldSupply
                            };
                            Debug.Log("Adding building " + buildingKey.ToString() + " - quality: " + buildingQuality.ToString());
                            ShopBuildingData.Add(buildingKey, testbuildKey);
                        }
                        Debug.Log("Building " + buildingKey.ToString() + " is present - quality = " + sd.BuildingQuality.ToString());
                    }
                }
            }
            else
                Debug.Log("You Just Entered Something Other Than An Open Shop...");

            //Debug.LogFormat("mapID = {1} and buildingKey = {0}. The Quality of the building is {2}. currentGoldSupply = {3}.", buildingKey, mapID, buildingQuality, currentGoldSupply); // Higher Quality value = better shop quality.
        }

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
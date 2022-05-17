// Project:         LimitedGoldShops mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2020 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Version:			v.1.00
// Created On: 	    4/21/2020, 7:35 PM
// Last Edit:		5/28/2020, 11:40 PM
// Modifier:
// Special Thanks:  BadLuckBurt, Hazelnut, Ralzar, LypyL, Pango, TheLacus, Baler, Macadaynu, Kab the Bird Ranger

using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using UnityEngine;
using System;
using System.Collections.Generic;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;

namespace LimitedGoldShops
{
    [FullSerializer.fsObject("v1")]
    public class LGSaveData
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

    public class LimitedGoldShopsMain : MonoBehaviour, IHasModSaveData
    {
        static LimitedGoldShopsMain instance;

        public static LimitedGoldShopsMain Instance
        {
            get { return instance ?? (instance = FindObjectOfType<LimitedGoldShopsMain>()); }
        }

        static Mod mod;

        public static Dictionary<int, ShopData> ShopBuildingData;

        public static int FlexCurrentGoldSupply { get; set; }

        public Type SaveDataType
        {
            get { return typeof(LGSaveData); }
        }

        public object NewSaveData()
        {
            return new LGSaveData
            {
                ShopBuildingData = new Dictionary<int, ShopData>()
            };
        }

        public object GetSaveData()
        {
            return new LGSaveData
            {
                ShopBuildingData = ShopBuildingData
            };
        }

        public void RestoreSaveData(object saveData)
        {
            var LGSaveData = (LGSaveData)saveData;
            Debug.Log("Restoring save data");
            ShopBuildingData = LGSaveData.ShopBuildingData;
            Debug.Log("Save data restored");
        }    

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            instance = new GameObject("LimitedGoldShops").AddComponent<LimitedGoldShopsMain>(); // Add script to the scene.
            mod.SaveDataInterface = instance;
            if (ShopBuildingData == null) 
            {
                ShopBuildingData = new Dictionary<int, ShopData>();
            }

            mod.LoadSettingsCallback = LoadSettings; // To enable use of the "live settings changes" feature in-game.

            // initiates mod message handler
            mod.MessageReceiver = DFModMessageReceiver;

            mod.IsReady = true;
        }

        private void Start()
        {
            Debug.Log("Begin mod init: LimitedGoldShops");

            UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Trade, typeof(LGSTradeWindowText));
            UIWindowFactory.RegisterCustomUIWindow(UIWindowType.MerchantRepairPopup, typeof(LGSMerchantTradeRepairPopupWindowReplace));
            UIWindowFactory.RegisterCustomUIWindow(UIWindowType.MerchantServicePopup, typeof(LGSMerchantTradeServicePopupWindowReplace));
            Debug.Log("LimitedGoldShops Registered It's Windows");

            mod.LoadSettings();

            PlayerEnterExit.OnTransitionInterior += GenerateShop_OnTransitionInterior;

            WorldTime.OnNewDay += RemoveExpiredShops_OnNewDay;

            Debug.Log("Finished mod init: LimitedGoldShops");
        }

        static void LoadSettings(ModSettings modSettings, ModSettingsChange change)
        {
            ShopGoldSettingModifier = mod.GetSettings().GetValue<float>("Options", "ShopGoldModifier");
            ShopStandardsSetting = mod.GetSettings().GetValue<bool>("Options", "ShopStandards");
        }

        public static float ShopGoldSettingModifier { get; set; }
        public static bool ShopStandardsSetting { get; set; }

        public static float GetShopGoldSettingModifier()
        {
            return ShopGoldSettingModifier;
        }

        public static bool GetShopStandardsSetting()
        {
            return ShopStandardsSetting;
        }

        public static void TradeUpdateShopGold(DaggerfallTradeWindow.WindowModes mode, int value)
        {
            ulong creationTime = 0;
            int buildingQuality = 0;
            bool investedIn = false;
            uint amountInvested = 0;
            int shopAttitude = 0;
            int buildingKey = 0;
            int currentGoldSupply = 0;
            int currentBuildingID = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingKey;
            PlayerEnterExit playerEnterExit = GameManager.Instance.PlayerEnterExit;

            try
            {
                ShopData sd;
                if (ShopBuildingData.TryGetValue(currentBuildingID, out sd))
                {
                    buildingKey = currentBuildingID;
                    creationTime = sd.CreationTime;
                    investedIn = sd.InvestedIn;
                    amountInvested = sd.AmountInvested;
                    shopAttitude = sd.ShopAttitude;
                    buildingQuality = sd.BuildingQuality;
                }
                //Debug.LogFormat("Sale Value = {0}", value);
                if (mode == DaggerfallTradeWindow.WindowModes.Buy && (playerEnterExit.BuildingDiscoveryData.buildingType == DFLocation.BuildingTypes.Temple || playerEnterExit.BuildingDiscoveryData.buildingType == DFLocation.BuildingTypes.GuildHall))
                {
                    //Debug.Log("You are buying inside a Temple or a Guild-Hall.");
                }
                else if (mode == DaggerfallTradeWindow.WindowModes.Buy)
                {
                    currentGoldSupply = sd.CurrentGoldSupply + value;
                    ShopBuildingData.Remove(buildingKey);

                    ShopData currentBuildKey = new ShopData
                    {
                        CreationTime = creationTime,
                        InvestedIn = investedIn,
                        AmountInvested = amountInvested,
                        ShopAttitude = shopAttitude,
                        BuildingQuality = buildingQuality,
                        CurrentGoldSupply = currentGoldSupply
                    };
                    ShopBuildingData.Add(buildingKey, currentBuildKey);
                }
                else
                {
                    currentGoldSupply = sd.CurrentGoldSupply - value;
                    ShopBuildingData.Remove(buildingKey);

                    ShopData currentBuildKey = new ShopData
                    {
                        CreationTime = creationTime,
                        InvestedIn = investedIn,
                        AmountInvested = amountInvested,
                        ShopAttitude = shopAttitude,
                        BuildingQuality = buildingQuality,
                        CurrentGoldSupply = currentGoldSupply
                    };
                    ShopBuildingData.Add(buildingKey, currentBuildKey);
                }
                //Debug.LogFormat("Shop Gold Supply After Sale = {0}", currentGoldSupply);
                FlexCurrentGoldSupply = currentGoldSupply;
            }
            catch
            {
                //Debug.Log("You are buying inside a Temple or a Guild-Hall, as your first entered building since launching the game!");
            }
        }

        public static void UpdateInvestAmount(int investOffer = 0) // This is responsible for updating investment data in the save-data for a building etc.
        {
            uint investedOffer = Convert.ToUInt32(investOffer);
            ulong creationTime = 0;
            int buildingQuality = 0;
            bool investedIn = false;
            uint amountInvested = 0;
            int shopAttitude = 0;
            int buildingKey = 0;
            int currentGoldSupply = 0;
            int currentBuildingID = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingKey;
            PlayerEnterExit playerEnterExit = GameManager.Instance.PlayerEnterExit;

            ShopData sd;
            if (ShopBuildingData.TryGetValue(currentBuildingID, out sd))
            {
                buildingKey = currentBuildingID;
                creationTime = sd.CreationTime;
                investedIn = sd.InvestedIn;
                amountInvested = sd.AmountInvested;
                shopAttitude = sd.ShopAttitude;
                buildingQuality = sd.BuildingQuality;
                currentGoldSupply = sd.CurrentGoldSupply;
            }
            //Debug.LogFormat("Offered Investment Amount = {0}", investedOffer);

            investedIn = true;
            amountInvested = sd.AmountInvested + investedOffer;
            ShopBuildingData.Remove(buildingKey);

            ShopData currentBuildKey = new ShopData
            {
                CreationTime = creationTime,
                InvestedIn = investedIn,
                AmountInvested = amountInvested,
                ShopAttitude = shopAttitude,
                BuildingQuality = buildingQuality,
                CurrentGoldSupply = currentGoldSupply
            };
            ShopBuildingData.Add(buildingKey, currentBuildKey);
            //Debug.LogFormat("Total Investment Amount After This Addition = {0}", amountInvested);
        }

        protected static void RemoveExpiredShops_OnNewDay()
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
                        //Debug.Log("Expired Building Key " + kvp.Key.ToString() + " Detected, Adding to List");
                        expiredKeys.Add(kvp.Key);
                    }
                }
            }

            foreach (int expired in expiredKeys)
            {
                //Debug.Log("Removed Expired Building Key " + expired.ToString());
                ShopBuildingData.Remove(expired);
            }
        }

        protected static void GenerateShop_OnTransitionInterior(PlayerEnterExit.TransitionEventArgs args)
        {
            ulong creationTime = 0;
            int buildingQuality = 0;
            bool investedIn = false;
            uint amountInvested = 0;
            int shopAttitude = 0;
            int buildingKey = 0;
            int currentGoldSupply = 0;

            creationTime = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToSeconds();
            buildingQuality = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.quality;
            buildingKey = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingKey;

            if (GameManager.Instance.PlayerEnterExit.IsPlayerInsideOpenShop)
            {
                //Debug.Log("You Just Entered An Open Shop, Good Job!...");
                ShopData sd;
                currentGoldSupply = GoldSupplyAmountGenerator(buildingQuality);
                shopAttitude = UnityEngine.Random.Range(0, 2); // Mostly have it as an int instead of a bool, just incase I want to add more "attitude types" to this later on.
                //Debug.Log("Building Attitude Rolled: " + shopAttitude.ToString());
                FlexCurrentGoldSupply = currentGoldSupply; // On transition, sets the shops gold supply to this "global variable" for later uses.

                if (!ShopBuildingData.ContainsKey(buildingKey))
                {
                    ShopData currentBuildKey = new ShopData
                    {
                        CreationTime = creationTime,
                        InvestedIn = investedIn,
                        AmountInvested = amountInvested,
                        ShopAttitude = shopAttitude,
                        BuildingQuality = buildingQuality,
                        CurrentGoldSupply = currentGoldSupply
                    };
                    //Debug.Log("Adding building " + buildingKey.ToString() + " - quality: " + buildingQuality.ToString());
                    ShopBuildingData.Add(buildingKey, currentBuildKey);
                }
                else
                {
                    if (ShopBuildingData.TryGetValue(buildingKey, out sd))
                    {
                        ulong timeLastVisited = creationTime - sd.CreationTime;
                        investedIn = sd.InvestedIn;
                        if (timeLastVisited >= 1296000)
                        { // 15 * 86400 = Number of seconds in 15 days.
                            if (investedIn)
                            {
                                investedIn = sd.InvestedIn;
                                amountInvested = sd.AmountInvested;
                                shopAttitude = sd.ShopAttitude;
                                buildingQuality = sd.BuildingQuality;
                                currentGoldSupply = InvestedGoldSupplyAmountGenerator(buildingQuality, amountInvested, shopAttitude);

                                ShopBuildingData.Remove(buildingKey);
                                //Debug.Log("Removing Expired Invested Shop Gold & Time: " + buildingKey.ToString());
                                ShopData currentBuildKey = new ShopData
                                {
                                    CreationTime = creationTime,
                                    InvestedIn = investedIn,
                                    AmountInvested = amountInvested,
                                    ShopAttitude = shopAttitude,
                                    BuildingQuality = buildingQuality,
                                    CurrentGoldSupply = currentGoldSupply
                                };
                                //Debug.Log("Refreshing building " + buildingKey.ToString() + " - quality: " + buildingQuality.ToString());
                                ShopBuildingData.Add(buildingKey, currentBuildKey);
                            }
                            else
                            {
                                ShopBuildingData.Remove(buildingKey);
                                //Debug.Log("Removed Expired Building Key " + buildingKey.ToString() + " Generating New Properties");
                                ShopData currentBuildKey = new ShopData
                                {
                                    CreationTime = creationTime,
                                    InvestedIn = investedIn,
                                    AmountInvested = amountInvested,
                                    ShopAttitude = shopAttitude,
                                    BuildingQuality = buildingQuality,
                                    CurrentGoldSupply = currentGoldSupply
                                };
                                //Debug.Log("Adding building " + buildingKey.ToString() + " - quality: " + buildingQuality.ToString());
                                ShopBuildingData.Add(buildingKey, currentBuildKey);
                            }
                        }
                        //Debug.Log("Building " + buildingKey.ToString() + " is present - quality = " + sd.BuildingQuality.ToString());
                    }
                }
            }
            else
                return;
                //Debug.Log("You Just Entered Something Other Than An Open Shop...");
        }

        public static int GoldSupplyAmountGenerator(int buildingQuality)
        {
            //Debug.Log("A shop without investment was just generated.");
            PlayerEntity player = GameManager.Instance.PlayerEntity;
            int playerLuck = player.Stats.LiveLuck - 50;
            int currentRegionIndex = GameManager.Instance.PlayerGPS.CurrentRegionIndex;
            int regionCostAdjust = GameManager.Instance.PlayerEntity.RegionData[currentRegionIndex].PriceAdjustment / 100;
            float varianceMaker = (UnityEngine.Random.Range(1.001f, 2.751f));

            return (int)Mathf.Ceil(((buildingQuality * 10) + (regionCostAdjust + 1)) * ((135 + playerLuck) * varianceMaker) * ShopGoldSettingModifier);
        }

        public static int InvestedGoldSupplyAmountGenerator(int buildingQuality, uint amountInvested, int shopAttitude)
        {
            //Debug.Log("A shop that was invested in refreshed its gold supply, investment taken into account.");
            PlayerEntity player = GameManager.Instance.PlayerEntity;
            int playerLuck = player.Stats.LiveLuck - 50;

            float attitudeMulti = 1.00f;
            if (shopAttitude == 1)
                attitudeMulti = .90f;
            else
                attitudeMulti = 1.00f;
            int mercSkill = player.Skills.GetLiveSkillValue(DFCareer.Skills.Mercantile);
            float investmentMulti = (mercSkill * .01f) + 1;
            uint investmentPostMulti = (uint)Mathf.Floor(amountInvested * investmentMulti);
            float qualityVarianceMod = (UnityEngine.Random.Range(0.2f, 1.1f));
            float qualityInvestMulti = (buildingQuality * (0.03f * qualityVarianceMod) * attitudeMulti);
            uint investmentPostQuality = (uint)Mathf.Floor(investmentPostMulti * qualityInvestMulti);
            //Debug.Log("Player Has: " + amountInvested.ToString() + " Gold Invested Here.");
            //Debug.Log("Invested Amount Has Added: " + investmentPostQuality.ToString() + " Gold To Shop Stock This Cycle.");

            int currentRegionIndex = GameManager.Instance.PlayerGPS.CurrentRegionIndex;
            int regionCostAdjust = GameManager.Instance.PlayerEntity.RegionData[currentRegionIndex].PriceAdjustment / 100;
            float varianceMaker = (UnityEngine.Random.Range(1.201f, 2.751f));

            return (int)Mathf.Ceil((((buildingQuality * 10) + (regionCostAdjust + 1)) * ((135 + playerLuck) * varianceMaker) + investmentPostQuality) * ShopGoldSettingModifier);
        }

        static void DFModMessageReceiver(string message, object data, DFModMessageCallback callBack)
        {
            switch (message)
            {
                case "GetShopGoldSupply":
                    callBack?.Invoke(message, FlexCurrentGoldSupply);
                    break;
                default:
                    callBack?.Invoke(message, "Unknown message");
                    break;
            }
        }
    }
}
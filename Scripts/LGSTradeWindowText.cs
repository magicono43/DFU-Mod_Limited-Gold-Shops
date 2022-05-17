using UnityEngine;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Banking;
using DaggerfallWorkshop.Game.Guilds;
using LimitedGoldShops;
using System.Collections.Generic;

namespace DaggerfallWorkshop.Game.UserInterfaceWindows
{
    public partial class LGSTradeWindowText : DaggerfallTradeWindow, IMacroContextProvider
    {
        protected static TextLabel localTextLabelOne;
        protected static TextLabel localTextLabelTwo;

        public LGSTradeWindowText(IUserInterfaceManager uiManager, DaggerfallBaseWindow previous = null) // Added to attempt to circumvent DFU v0.10.25 issue, working now.
            : base(uiManager, previous, WindowModes.Sell, null)
        {

        }

        public LGSTradeWindowText(IUserInterfaceManager uiManager, DaggerfallBaseWindow previous, WindowModes windowMode, IGuild guild = null)
            : base(uiManager, previous, windowMode, guild)
        {

        }

        List<ItemGroups> itemTypesAccepted = storeBuysItemType[DFLocation.BuildingTypes.GeneralStore];

        static Dictionary<DFLocation.BuildingTypes, List<ItemGroups>> storeBuysItemType = new Dictionary<DFLocation.BuildingTypes, List<ItemGroups>>()
        {
            { DFLocation.BuildingTypes.Alchemist, new List<ItemGroups>()
                { ItemGroups.Gems, ItemGroups.CreatureIngredients1, ItemGroups.CreatureIngredients2, ItemGroups.CreatureIngredients3, ItemGroups.PlantIngredients1, ItemGroups.PlantIngredients2, ItemGroups.MiscellaneousIngredients1, ItemGroups.MiscellaneousIngredients2, ItemGroups.MetalIngredients } },
            { DFLocation.BuildingTypes.Armorer, new List<ItemGroups>()
                { ItemGroups.Armor, ItemGroups.Weapons } },
            { DFLocation.BuildingTypes.Bookseller, new List<ItemGroups>()
                { ItemGroups.Books } },
            { DFLocation.BuildingTypes.ClothingStore, new List<ItemGroups>()
                { ItemGroups.MensClothing, ItemGroups.WomensClothing } },
            { DFLocation.BuildingTypes.FurnitureStore, new List<ItemGroups>()
                { ItemGroups.Furniture } },
            { DFLocation.BuildingTypes.GemStore, new List<ItemGroups>()
                { ItemGroups.Gems, ItemGroups.Jewellery } },
            { DFLocation.BuildingTypes.GeneralStore, new List<ItemGroups>()
                { ItemGroups.Books, ItemGroups.MensClothing, ItemGroups.WomensClothing, ItemGroups.Transportation, ItemGroups.Jewellery, ItemGroups.Weapons, ItemGroups.UselessItems2 } },
            { DFLocation.BuildingTypes.PawnShop, new List<ItemGroups>()
                { ItemGroups.Armor, ItemGroups.Books, ItemGroups.MensClothing, ItemGroups.WomensClothing, ItemGroups.Gems, ItemGroups.Jewellery, ItemGroups.ReligiousItems, ItemGroups.Weapons, ItemGroups.UselessItems2, ItemGroups.Paintings } },
            { DFLocation.BuildingTypes.WeaponSmith, new List<ItemGroups>()
                { ItemGroups.Armor, ItemGroups.Weapons } },
        };

        protected override void FilterLocalItems()
        {
            localItemsFiltered.Clear();

            // Add any basket items to filtered list first, if not using wagon
            if (WindowMode == WindowModes.Buy && !UsingWagon && basketItems != null)
            {
                for (int i = 0; i < basketItems.Count; i++)
                {
                    DaggerfallUnityItem item = basketItems.GetItem(i);
                    // Add if not equipped
                    if (!item.IsEquipped)
                        AddLocalItem(item);
                }
            }
            // Add local items to filtered list
            if (localItems != null)
            {
                for (int i = 0; i < localItems.Count; i++)
                {
                    // Add if not equipped & accepted for selling
                    DaggerfallUnityItem item = localItems.GetItem(i);
                    if (!item.IsEquipped && (
                            (WindowMode != WindowModes.Sell && WindowMode != WindowModes.SellMagic) ||
                            (WindowMode == WindowModes.Sell && itemTypesAccepted.Contains(item.ItemGroup)) ||
                            (WindowMode == WindowModes.SellMagic && item.IsEnchanted)))
                    {
                        if (!LimitedGoldShopsMain.GetShopStandardsSetting())
                        {
                            AddLocalItem(item); // Should test this feature in-game tomorrow, not really feeling like doing it more tonight.
                            continue;
                        }

                        if (LimitedGoldShopsMain.GetShopStandardsSetting() && IsItemWithinShopStandards(item))
                        {
                            AddLocalItem(item);
                        }
                    }
                }
            }
        }

        public bool IsItemWithinShopStandards(DaggerfallUnityItem itemChecked)
        {
            int material = -1;
            int baseGoldValue = itemChecked.value;

            if (itemChecked.ItemGroup == ItemGroups.Weapons || itemChecked.ItemGroup == ItemGroups.Armor)
            {
                material = GetMaterialTypeInt(itemChecked.NativeMaterialValue);

                if (itemChecked.IsArtifact)
                    material = 11;

                switch (GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.quality)
                {
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                        if (material <= 4)
                            return false;
                        else
                            return true;
                    case 14:
                    case 15:
                    case 16:
                    case 17:
                        if (material <= 6)
                            return false;
                        else
                            return true;
                    case 18:
                    case 19:
                    case 20:
                        if (material <= 8)
                            return false;
                        else
                            return true;
                    default:
                        return true;
                }
            }
            else if (itemChecked.ItemGroup == ItemGroups.MensClothing || itemChecked.ItemGroup == ItemGroups.WomensClothing)
            {
                switch (GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.quality)
                {
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                        if (baseGoldValue <= 9)
                            return false;
                        else
                            return true;
                    case 14:
                    case 15:
                    case 16:
                    case 17:
                        if (baseGoldValue <= 24)
                            return false;
                        else
                            return true;
                    case 18:
                    case 19:
                    case 20:
                        if (baseGoldValue <= 99)
                            return false;
                        else
                            return true;
                    default:
                        return true;
                }
            }
            else if (itemChecked.ItemGroup == ItemGroups.ReligiousItems)
            {
                switch (GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.quality)
                {
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                        if (baseGoldValue <= 15)
                            return false;
                        else
                            return true;
                    case 14:
                    case 15:
                    case 16:
                    case 17:
                        if (baseGoldValue <= 40)
                            return false;
                        else
                            return true;
                    case 18:
                    case 19:
                    case 20:
                        if (baseGoldValue <= 99)
                            return false;
                        else
                            return true;
                    default:
                        return true;
                }
            }
            else if (itemChecked.IsIngredient)
            {
                switch (GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.quality)
                {
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                        if (baseGoldValue <= 7)
                            return false;
                        else
                            return true;
                    case 14:
                    case 15:
                    case 16:
                    case 17:
                        if (baseGoldValue <= 19)
                            return false;
                        else
                            return true;
                    case 18:
                    case 19:
                    case 20:
                        if (baseGoldValue <= 29)
                            return false;
                        else
                            return true;
                    default:
                        return true;
                }
            }
            else
                return true;
        }

        public int GetMaterialTypeInt (int nativeMatValue) // -1 = Unknown, 0 = Leather, 1 = Chain, 2 = Iron, 3 = Steel, 4 = Silver, 5 = Elven, 6 = Dwarven, 7 = Mithril, 8 = Adamantium, 9 = Ebony, 10 = Orcish, 11 = Daedric.
        {
            if (nativeMatValue <= 9 && nativeMatValue >= 0) // Checks if the item material is for weapons, and leather armor.
            {
                switch (nativeMatValue)
                {
                    case (int)WeaponMaterialTypes.Iron:
                        return 2;
                    case (int)WeaponMaterialTypes.Steel:
                        return 3;
                    case (int)WeaponMaterialTypes.Silver:
                        return 4;
                    case (int)WeaponMaterialTypes.Elven:
                        return 5;
                    case (int)WeaponMaterialTypes.Dwarven:
                        return 6;
                    case (int)WeaponMaterialTypes.Mithril:
                        return 7;
                    case (int)WeaponMaterialTypes.Adamantium:
                        return 8;
                    case (int)WeaponMaterialTypes.Ebony:
                        return 9;
                    case (int)WeaponMaterialTypes.Orcish:
                        return 10;
                    case (int)WeaponMaterialTypes.Daedric:
                        return 11;
                    default:
                        return 0; // Leather should default to this.
                }
            }
            else if (nativeMatValue <= 521 && nativeMatValue >= 256) // Checks if the item material is for armors.
            {
                switch (nativeMatValue)
                {
                    case (int)ArmorMaterialTypes.Chain:
                    case (int)ArmorMaterialTypes.Chain2:
                        return 1;
                    case (int)ArmorMaterialTypes.Iron:
                        return 2;
                    case (int)ArmorMaterialTypes.Steel:
                        return 3;
                    case (int)ArmorMaterialTypes.Silver:
                        return 4;
                    case (int)ArmorMaterialTypes.Elven:
                        return 5;
                    case (int)ArmorMaterialTypes.Dwarven:
                        return 6;
                    case (int)ArmorMaterialTypes.Mithril:
                        return 7;
                    case (int)ArmorMaterialTypes.Adamantium:
                        return 8;
                    case (int)ArmorMaterialTypes.Ebony:
                        return 9;
                    case (int)ArmorMaterialTypes.Orcish:
                        return 10;
                    case (int)ArmorMaterialTypes.Daedric:
                        return 11;
                    default:
                        return -1;
                }
            }
            else
                return -1;
        }

        protected override void Setup()
        {
            base.Setup();
            SetupShopGoldInfoText();
        }

        protected override void ShowTradePopup()
        {
            const int tradeMessageBaseId = 260;
            const int notEnoughGoldId = 454;
            int msgOffset = 0;
            int tradePrice = GetTradePrice();

            int currentBuildingID = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingKey;
            int goldSupply = 0;
            int shopAttitude = 0; // For now, 0 = Nice Attitude and 1 = Bad Attitude.
            LimitedGoldShops.ShopData sd;
            if (LimitedGoldShops.LimitedGoldShopsMain.ShopBuildingData.TryGetValue(currentBuildingID, out sd))
            {
                goldSupply = sd.CurrentGoldSupply;
                shopAttitude = sd.ShopAttitude;
            }

            if (WindowMode == WindowModes.Sell && goldSupply <= 0)
            {
                int buildQual = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.quality;
                TextFile.Token[] tokens = LGSTextTokenHolder.ShopTextTokensNice(4);
                if (buildQual <= 3) // 01 - 03
                {
                    if (shopAttitude == 0) // Possibly could add the "shop type" to the save-data as well, and use that to determine some other things.
                        tokens = LGSTextTokenHolder.ShopTextTokensNice(5);
                    else
                        tokens = LGSTextTokenHolder.ShopTextTokensMean(4);
                }
                else if (buildQual <= 7) // 04 - 07
                {
                    if (shopAttitude == 0)
                        tokens = LGSTextTokenHolder.ShopTextTokensNice(6);
                    else
                        tokens = LGSTextTokenHolder.ShopTextTokensMean(5);
                }
                else if (buildQual <= 13) // 08 - 13
                {
                    if (shopAttitude == 0)
                        tokens = LGSTextTokenHolder.ShopTextTokensNice(4);
                    else
                        tokens = LGSTextTokenHolder.ShopTextTokensMean(6);
                }
                else if (buildQual <= 17) // 14 - 17
                {
                    if (shopAttitude == 0)
                        tokens = LGSTextTokenHolder.ShopTextTokensNice(7);
                    else
                        tokens = LGSTextTokenHolder.ShopTextTokensMean(7);
                }
                else                      // 18 - 20
                {
                    if (shopAttitude == 0)
                        tokens = LGSTextTokenHolder.ShopTextTokensNice(8);
                    else
                        tokens = LGSTextTokenHolder.ShopTextTokensMean(8);
                }

                DaggerfallMessageBox noGoldShop = new DaggerfallMessageBox(DaggerfallUI.UIManager, this);
                noGoldShop.SetTextTokens(tokens, this);
                noGoldShop.ClickAnywhereToClose = true;
                uiManager.PushWindow(noGoldShop);
            }
            else if (WindowMode == WindowModes.Sell && goldSupply < tradePrice)
            {
                TextFile.Token[] tokens = DaggerfallUnity.Instance.TextProvider.CreateTokens(
                    TextFile.Formatting.JustifyCenter,
                    "I can't afford your offer.",
                    "Would you be willing to settle",
                    "for the " + goldSupply.ToString() + " gold that I have left?");

                DaggerfallMessageBox messageBox = new DaggerfallMessageBox(uiManager, this);
                messageBox.SetTextTokens(tokens, this);
                messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.Yes);
                messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.No);
                messageBox.OnButtonClick += ConfirmPoorTrade_OnButtonClick;
                uiManager.PushWindow(messageBox);
            }
            else if (WindowMode != WindowModes.Sell && WindowMode != WindowModes.SellMagic && PlayerEntity.GetGoldAmount() < tradePrice)
            {
                DaggerfallUI.MessageBox(notEnoughGoldId);
            }
            else
            {
                if (cost >> 1 <= tradePrice)
                {
                    if (cost - (cost >> 2) <= tradePrice)
                        msgOffset = 2;
                    else
                        msgOffset = 1;
                }
                if (WindowMode == WindowModes.Sell || WindowMode == WindowModes.SellMagic)
                    msgOffset += 3;

                DaggerfallMessageBox messageBox = new DaggerfallMessageBox(uiManager, this);
                TextFile.Token[] tokens = DaggerfallUnity.Instance.TextProvider.GetRandomTokens(tradeMessageBaseId + msgOffset);
                messageBox.SetTextTokens(tokens, this);
                messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.Yes);
                messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.No);
                messageBox.OnButtonClick += ConfirmTrade_OnButtonClick;
                uiManager.PushWindow(messageBox);
            }
        }

        protected void SetupShopGoldInfoText()
        {
            PlayerEnterExit playerEnterExit = GameManager.Instance.PlayerEnterExit;
            int currentBuildingID = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingKey;
            LimitedGoldShops.ShopData sd;
            int goldSupply = 0;
            uint investedAmount = 0;
            if (LimitedGoldShops.LimitedGoldShopsMain.ShopBuildingData.TryGetValue(currentBuildingID, out sd))
            {
                goldSupply = sd.CurrentGoldSupply;
                investedAmount = sd.AmountInvested;
            }

            if (!(playerEnterExit.BuildingDiscoveryData.buildingType == DFLocation.BuildingTypes.Temple) && !(playerEnterExit.BuildingDiscoveryData.buildingType == DFLocation.BuildingTypes.GuildHall))
            {
                localTextLabelOne = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(263, 34), string.Empty, NativePanel);
                localTextLabelOne.TextScale = 0.85f;
                localTextLabelOne.Text = "Invested: " + investedAmount.ToString();

                localTextLabelTwo = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(263, 41), string.Empty, NativePanel);
                localTextLabelTwo.TextScale = 0.85f;
                localTextLabelTwo.Text = "Shop Gold: " + goldSupply.ToString();
            }
            UpdateShopGoldDisplay();
        }

        protected override void ConfirmTrade_OnButtonClick(DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton)
        {
            bool receivedLetterOfCredit = false;

            if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.Yes)
            {
                // Proceed with trade.
                int tradePrice = GetTradePrice();
                switch (WindowMode)
                {
                    case WindowModes.Sell:
                    case WindowModes.SellMagic:
                        float goldWeight = tradePrice * DaggerfallBankManager.goldUnitWeightInKg;
                        if (PlayerEntity.CarriedWeight + goldWeight <= PlayerEntity.MaxEncumbrance)
                        {
                            PlayerEntity.GoldPieces += tradePrice;
                        }
                        else
                        {
                            DaggerfallUnityItem loc = ItemBuilder.CreateItem(ItemGroups.MiscItems, (int)MiscItems.Letter_of_credit);
                            loc.value = tradePrice;
                            GameManager.Instance.PlayerEntity.Items.AddItem(loc, Items.ItemCollection.AddPosition.Front);
                            receivedLetterOfCredit = true;
                        }
                        RaiseOnTradeHandler(remoteItems.GetNumItems(), tradePrice);
                        remoteItems.Clear();
                        break;

                    case WindowModes.Buy:
                        PlayerEntity.DeductGoldAmount(tradePrice);
                        RaiseOnTradeHandler(basketItems.GetNumItems(), tradePrice);
                        PlayerEntity.Items.TransferAll(basketItems);
                        break;

                    case WindowModes.Repair:
                        PlayerEntity.DeductGoldAmount(tradePrice);
                        if (DaggerfallUnity.Settings.InstantRepairs)
                        {
                            foreach (DaggerfallUnityItem item in remoteItemsFiltered)
                                item.currentCondition = item.maxCondition;
                        }
                        else
                        {
                            UpdateRepairTimes(true);
                        }
                        RaiseOnTradeHandler(remoteItems.GetNumItems(), tradePrice);
                        break;

                    case WindowModes.Identify:
                        PlayerEntity.DeductGoldAmount(tradePrice);
                        for (int i = 0; i < remoteItems.Count; i++)
                        {
                            DaggerfallUnityItem item = remoteItems.GetItem(i);
                            item.IdentifyItem();
                        }
                        RaiseOnTradeHandler(remoteItems.GetNumItems(), tradePrice);
                        break;
                }
                if (receivedLetterOfCredit)
                    DaggerfallUI.Instance.PlayOneShot(SoundClips.ParchmentScratching);
                else
                    DaggerfallUI.Instance.PlayOneShot(SoundClips.GoldPieces);
                PlayerEntity.TallySkill(DFCareer.Skills.Mercantile, 1);
                LimitedGoldShops.LimitedGoldShopsMain.TradeUpdateShopGold(WindowMode, tradePrice);
                UpdateShopGoldDisplay();
				Refresh();
            }
            CloseWindow();
            if (receivedLetterOfCredit)
                DaggerfallUI.MessageBox(TextManager.Instance.GetLocalizedText("letterOfCredit"));
        }

        protected void ConfirmPoorTrade_OnButtonClick(DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton)
        {
            bool receivedLetterOfCredit = false;
            int currentBuildingID = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingKey;
            LimitedGoldShops.ShopData sd;
            int goldSupply = 0;
            if (LimitedGoldShops.LimitedGoldShopsMain.ShopBuildingData.TryGetValue(currentBuildingID, out sd))
                goldSupply = sd.CurrentGoldSupply;

            if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.Yes)
            {
                float goldWeight = goldSupply * DaggerfallBankManager.goldUnitWeightInKg;
                if (PlayerEntity.CarriedWeight + goldWeight <= PlayerEntity.MaxEncumbrance)
                {
                    PlayerEntity.GoldPieces += goldSupply;
                }
                else
                {
                    DaggerfallUnityItem loc = ItemBuilder.CreateItem(ItemGroups.MiscItems, (int)MiscItems.Letter_of_credit);
                    loc.value = goldSupply;
                    GameManager.Instance.PlayerEntity.Items.AddItem(loc, Items.ItemCollection.AddPosition.Front);
                    receivedLetterOfCredit = true;
                }
                RaiseOnTradeHandler(remoteItems.GetNumItems(), goldSupply);
                remoteItems.Clear();

                if (receivedLetterOfCredit)
                    DaggerfallUI.Instance.PlayOneShot(SoundClips.ParchmentScratching);
                else
                    DaggerfallUI.Instance.PlayOneShot(SoundClips.GoldPieces);
                PlayerEntity.TallySkill(DFCareer.Skills.Mercantile, 1);
                LimitedGoldShops.LimitedGoldShopsMain.TradeUpdateShopGold(WindowMode, goldSupply);
                UpdateShopGoldDisplay();
				Refresh();
            }
            CloseWindow();
            if (receivedLetterOfCredit)
                DaggerfallUI.MessageBox(TextManager.Instance.GetLocalizedText("letterOfCredit"));
        }

        public void UpdateShopGoldDisplay()
        {
            try // This is here to "resolve" whenever a building that does not count as a shop, has the trade-window opened up inside of it if the game has been loaded and no "valid" shop buildings have been entered. Essentially it threw an exception because the dictionary had nothing to give, so this is there to catch that in that situation.
            {
                int currentBuildingID = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingKey;
                LimitedGoldShops.ShopData sd;
                int goldSupply = 0;
                uint investedAmount = 0;
                if (LimitedGoldShops.LimitedGoldShopsMain.ShopBuildingData.TryGetValue(currentBuildingID, out sd))
                {
                    goldSupply = sd.CurrentGoldSupply;
                    investedAmount = sd.AmountInvested;
                }

                localTextLabelOne.Text = "Invested: " + investedAmount.ToString();
                localTextLabelTwo.Text = "Shop Gold: " + goldSupply.ToString();
            }
            catch
            {
                //Debug.Log("You opened the trade menu inside a Temple or a Guild-Hall, as your first entered building since launching the game.");
            }
        }
    }
}

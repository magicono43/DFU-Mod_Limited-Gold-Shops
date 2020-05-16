// Project:         Daggerfall Tools For Unity
// Copyright:       Copyright (C) 2009-2019 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Hazelnut
// Contributors:    Pango
// Extended by:     Asesino, Pango
// Notes:
//
using UnityEngine;
using System;
using System.Collections.Generic;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallConnect.Utility;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Banking;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.Guilds;
using DaggerfallWorkshop.Game.Utility;

namespace DaggerfallWorkshop.Game.UserInterfaceWindows
{
    /// <summary>
    /// Implements trade windows, based on inventory window.
    /// </summary>
    public partial class AsesinoTradeWindow : DaggerfallTradeWindow, IMacroContextProvider
    {
        public static int StupidTest { get; set; }

        protected Button localCloseFilterButton;
        protected bool filterButtonNeedUpdate;
        protected static Button localFilterButton;
        protected static TextBox localFilterTextBox;
        protected static TextLabel localTextLabelOne;
        protected static TextLabel localTextLabelTwo;
        protected Button localTestButtonOne;
        protected static string filterString = null;
        protected static string[] itemGroupNames = new string[]
        {
            "drugs",
            "uselessitems1",
            "armor",
            "weapons",
            "magicitems",
            "artifacts",
            "mensclothing",
            "books",
            "furniture",
            "uselessitems2",
            "religiousitems",
            "maps",
            "womensclothing",
            "paintings",
            "gems",
            "plantingredients1",
            "plantingredients2",
            "creatureingredients1",
            "creatureingredients2",
            "creatureingredients3",
            "miscellaneousingredients1",
            "metalingredients",
            "miscellaneousingredients2",
            "transportation",
            "deeds",
            "jewellery",
            "questitems",
            "miscitems",
            "currency"
        };

        #region Constructors

        public AsesinoTradeWindow(IUserInterfaceManager uiManager, DaggerfallBaseWindow previous = null, WindowModes windowMode = WindowModes.Sell, IGuild guild = null)
            : base(uiManager, previous, windowMode, guild)
        {
            
        }

        protected override void Setup()
        {
            base.Setup();
            SetupTargetIconPanelFilterBox();
        }

        public string GetHonoric()
        {
            int buildQual = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.quality;

            if (GameManager.Instance.PlayerEntity.Gender == Genders.Male)
            {
                if (buildQual <= 7)       // 01 - 07
                    return "%ra";
                else if (buildQual <= 17) // 08 - 17
                    return "sir";
                else                      // 18 - 20
                    return "m'lord";
            }
            else
            {
                if (buildQual <= 7)       // 01 - 07
                    return "%ra";
                else if (buildQual <= 17) // 08 - 17
                    return "ma'am";
                else                      // 18 - 20
                    return "madam";
            }
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
            foreach (KeyValuePair<int, SaveDataTestingScript1.ShopData> kvp in SaveDataTestingScript1.SaveDataTestingScript1.ShopBuildingData)
            {
                if (kvp.Key == currentBuildingID)
                {
                    goldSupply = kvp.Value.CurrentGoldSupply;
                    shopAttitude = kvp.Value.ShopAttitude;
                }
            }

            if (WindowMode == WindowModes.Sell && goldSupply <= 0)
            {
                int buildQual = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.quality;
                TextFile.Token[] tokens = DaggerfallUnity.Instance.TextProvider.CreateTokens(
                    TextFile.Formatting.JustifyCenter,
                    "Sorry " + GetHonoric() + ", I'm all out of gold.",
                    "You emptied my coffers clean.",
                    "You'll have to buy something,",
                    "or come back in a few days when",
                    "I've been able to visit the bank.");

                if (buildQual <= 3) // 01 - 03
                {
                    if (shopAttitude == 0) // Possibly could add the "shop type" to the save-data as well, and use that to determine some other things.
                    {
                        tokens = DaggerfallUnity.Instance.TextProvider.CreateTokens(
                            TextFile.Formatting.JustifyCenter,
                            "I may not be the best with",
                            "finances, " + GetHonoric() + ". But, I do know",
                            "when my purse is so light I can't",
                            "even buy a watered down ale.",
                            "Come back some other time.");
                    }
                    else
                    {
                        tokens = DaggerfallUnity.Instance.TextProvider.CreateTokens(
                            TextFile.Formatting.JustifyCenter,
                            "Are ya blind or just dumb, " + GetHonoric() + "?",
                            "Can't ya see my purse is as empty",
                            "as that space between your ears?",
                            "Now unless you're gonna buy somethin',",
                            "stop wastin' my damn time!");
                    }
                }
                else if (buildQual <= 7) // 04 - 07
                {
                    if (shopAttitude == 0)
                    {
						tokens = DaggerfallUnity.Instance.TextProvider.CreateTokens(
                            TextFile.Formatting.JustifyCenter,
                            "I'll level with you, " + GetHonoric() + ".",
                            "I don't have a pot to piss in.",
                            "Return in a few days and maybe",
                            "my situation will be different.");
                    }
                    else
                    {
                        tokens = DaggerfallUnity.Instance.TextProvider.CreateTokens(
                            TextFile.Formatting.JustifyCenter,
                            "Look, " + GetHonoric() + ", I think it's",
                            "pretty clear that I don't have a rat's",
                            "turd left to offer you. So why do you",
                            "mock me with a pointless offer?");
                    }
                }
                else if (buildQual <= 13) // 08 - 13
                {
                    if (shopAttitude == 0)
                    {
                        tokens = DaggerfallUnity.Instance.TextProvider.CreateTokens(
                            TextFile.Formatting.JustifyCenter,
                            "Sorry " + GetHonoric() + ", I'm all out of gold.",
                            "You emptied my coffers clean.",
                            "You'll have to buy something,",
                            "or come back in a few days when",
                            "I've been able to visit the bank.");
                    }
                    else
                    {
						tokens = DaggerfallUnity.Instance.TextProvider.CreateTokens(
                            TextFile.Formatting.JustifyCenter,
                            "Well, \"" + GetHonoric() + "\" do you plan on",
                            "giving this as a gift? Because if you",
                            "opened your damn eyes, I think it's",
                            "pretty obvious I can't afford your junk.");
                    }
                }
                else if (buildQual <= 17) // 14 - 17
                {
                    if (shopAttitude == 0)
                    {
                        tokens = DaggerfallUnity.Instance.TextProvider.CreateTokens(
                            TextFile.Formatting.JustifyCenter,
                            "Terribly sorry " + GetHonoric() + ", it appears",
                            "that my funds have run dry.",
                            "Return in about a fortnight and",
                            "I should have a sack of newly",
                            "minted coin to barter with.");
                    }
                    else
                    {
						tokens = DaggerfallUnity.Instance.TextProvider.CreateTokens(
                            TextFile.Formatting.JustifyCenter,
                            "My father did not build this shop",
                            "to serve mouth-breathers that can't",
                            "even tell when a respectable merchant",
                            "is out of coin. NOW SOD OFF!");
                    }
                }
                else                      // 18 - 20
                {
                    if (shopAttitude == 0)
                    {
                        tokens = DaggerfallUnity.Instance.TextProvider.CreateTokens(
                            TextFile.Formatting.JustifyCenter,
                            "My most sincere apologies " + GetHonoric() + ",",
                            "I have foolishly not stocked",
                            "myself with ample enough coin",
                            "to satisfy your needs. Give",
                            "me fifteen days at most and",
                            "we can continue our transaction.");
                    }
                    else
                    {
						tokens = DaggerfallUnity.Instance.TextProvider.CreateTokens(
                            TextFile.Formatting.JustifyCenter,
                            "I did not get to where I am",
                            "by wasting my time with peasants",
                            "that don't know the difference between",
                            "an empty coin purse and a gelded stallion's",
                            "scrotum. Now, leave my sight, plebeian.");
                    }
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

                // Seems to be working, now I want to make this a yes/no box like a normal trade, but the "yes" would be to trade whatever you a offering for whatever the shop current still has left in gold, "no" would close the window/offer. Also, have to figure out how to not make the OnTrade event fuck up how many times gold is counted in a trade, but that's another problem.
                // Add yes/no box here instead. Also, want to fix when the gold amount of shop is updated in the UI screen, have to test that too.
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

        /*protected override void ShowTradePopup()
        {
            const int notEnoughGoldId = 454;

            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            DaggerfallUI.MessageBox(notEnoughGoldId); // Override seems to have worked as expected without issues for now. Will want to test a bit more to see if I can get full intended functionality, then submit a PR request.
            filterButtonNeedUpdate = true;
        }*/

        protected  void SetupTargetIconPanelFilterBox()
        {
            string toolTipText = string.Empty;
            toolTipText = "Press Filter Button to Open Filter Text Box.\rAnything typed into text box will autofilter.\rFor negative filter, type '-' in front.\rFor example, -steel weapon will find all weapons not made of steel.";

            int currentBuildingID = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingKey;
            int goldSupply = 0;
            foreach (KeyValuePair<int, SaveDataTestingScript1.ShopData> kvp in SaveDataTestingScript1.SaveDataTestingScript1.ShopBuildingData)
            {
                if (kvp.Key == currentBuildingID)
                    goldSupply = kvp.Value.CurrentGoldSupply;
            }

            localTestButtonOne = DaggerfallUI.AddButton(new Rect(230, 130, 50, 50), NativePanel);
            localTestButtonOne.Label.Text = "Testing";
            localTestButtonOne.Label.TextScale = 0.75f;
            localTestButtonOne.Label.ShadowColor = Color.black;
            localTestButtonOne.BackgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.75f);
            localTestButtonOne.OnMouseClick += LocalTestButtonOne_OnMouseClick;

            localTextLabelOne = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(263, 34), string.Empty, NativePanel);
            localTextLabelOne.TextScale = 0.85f;
            localTextLabelOne.Text = "Invested:   1234567890";
			
            localTextLabelTwo = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(263, 41), string.Empty, NativePanel);
            localTextLabelTwo.TextScale = 0.85f;                                // Figure out how to get the ShopGold display value to update whenever a trade is complete. As I have it, now it just keeps layering new text labels over one another, not sure how to "remove" the last one and just update it or something while still in the window. May have to ask the forums, but will experiment more tomorrow with this and how it works in "DaggerfallTradeWindow.cs".
            localTextLabelTwo.Text = "Shop Gold: " + goldSupply.ToString();

            localFilterTextBox = DaggerfallUI.AddTextBoxWithFocus(new Rect(new Vector2(1, 24), new Vector2(47, 8)), "filter pattern", localTargetIconPanel);
            localFilterTextBox.VerticalAlignment = VerticalAlignment.Bottom;
            localFilterTextBox.OnType += LocalFilterTextBox_OnType;
            localFilterTextBox.OverridesHotkeySequences = true;

            localFilterButton = DaggerfallUI.AddButton(new Rect(40, 25, 15, 8), localTargetIconPanel);
            localFilterButton.Label.Text = "Filter";
            localFilterButton.ToolTip = defaultToolTip;
            localFilterButton.ToolTipText = toolTipText;

            localFilterButton.Label.TextScale = 0.75f;
            localFilterButton.Label.ShadowColor = Color.black;
            localFilterButton.VerticalAlignment = VerticalAlignment.Bottom;
            localFilterButton.HorizontalAlignment = HorizontalAlignment.Left;
            localFilterButton.BackgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.75f);
            localFilterButton.OnMouseClick += LocalFilterButton_OnMouseClick;

            localCloseFilterButton = DaggerfallUI.AddButton(new Rect(47, 25, 8, 8), localTargetIconPanel);
            localCloseFilterButton.Label.Text = "x";
            localCloseFilterButton.Label.TextScale = 0.75f;
            localCloseFilterButton.Label.ShadowColor = Color.black;
            localCloseFilterButton.VerticalAlignment = VerticalAlignment.Bottom;
            localCloseFilterButton.HorizontalAlignment = HorizontalAlignment.Right;
            localCloseFilterButton.BackgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.75f);
            localCloseFilterButton.OnMouseClick += LocalCloseFilterButton_OnMouseClick;

            filterButtonNeedUpdate = true;
            Refresh(false);
        }

        #endregion

        public override void OnPop()
        {
            ClearFilterFields();
            base.OnPop();
        }

        private void UpdateFilterButton()
        {
            if (filterString != null && localFilterButton.Enabled)
            {
                localFilterButton.Enabled = false;
                localFilterTextBox.Enabled = true;
                localCloseFilterButton.Enabled = true;
            }
            else
            {
                localFilterButton.Enabled = true;
                localFilterTextBox.Enabled = false;
                localCloseFilterButton.Enabled = false;
            }
        }

        public override void Update()
        {
            base.Update();

            if (localFilterTextBox.HasFocus() && Input.GetKeyDown(KeyCode.Return))
                SetFocus(null);

            if (filterButtonNeedUpdate)
            {
                filterButtonNeedUpdate = false;
                UpdateFilterButton();
            }
       }


        protected new void SelectTabPage(TabPages tabPage)
        {
            // Select new tab page
            base.SelectTabPage(tabPage);

            //ClearFilterFields();
            FilterRemoteItems();
            remoteItemListScroller.Items = remoteItemsFiltered;
        }

        protected override void FilterLocalItems()
        {
            localItemsFiltered.Clear();

            // Add any basket items to filtered list first, if not using wagon
            if (WindowMode == WindowModes.Buy && !UsingWagon && BasketItems != null)
            {
                for (int i = 0; i < BasketItems.Count; i++)
                {
                    DaggerfallUnityItem item = BasketItems.GetItem(i);
                    // Add if not equipped
                    if (!item.IsEquipped)
                    {
                        AddLocalItem(item);
                    }

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
                            (WindowMode == WindowModes.Sell && ItemTypesAccepted.Contains(item.ItemGroup)) ||
                            (WindowMode == WindowModes.SellMagic && item.IsEnchanted)))
                    {
                        if (ItemPassesFilter(item))
                            AddLocalItem(item);
                    }
                }
            }
        }

        protected override void FilterRemoteItems()
        {
            if (WindowMode == WindowModes.Repair)
            {
                // Clear current references
                remoteItemsFiltered.Clear();

                // Add items to list if they are not being repaired or are being repaired here. 
                if (remoteItems != null)
                {
                    for (int i = 0; i < remoteItems.Count; i++)
                    {
                        DaggerfallUnityItem item = remoteItems.GetItem(i);
                        if (!item.RepairData.IsBeingRepaired() || item.RepairData.IsBeingRepairedHere())
                            remoteItemsFiltered.Add(item);
                        if (item.RepairData.IsRepairFinished())
                            item.currentCondition = item.maxCondition;
                    }
                }
                UpdateRepairTimes(false);
            }
            else
                FilterRemoteItemsWithoutExtraFilter();
        }

        protected  void FilterRemoteItemsWithoutExtraFilter()
        {
            // Clear current references
            remoteItemsFiltered.Clear();

            // Add items to list
            if (remoteItems != null)
                for (int i = 0; i < remoteItems.Count; i++)
                   remoteItemsFiltered.Add(remoteItems.GetItem(i));
        }

        protected bool ItemPassesFilter(DaggerfallUnityItem item)
        {
            bool iterationPass = false;

            if (String.IsNullOrEmpty(filterString))
                return true;

            foreach (string word in filterString.Split(' '))
            {
                if (word.Trim().Length > 0)
                {
                    if (word[0] == '-')
                    {
                        string wordLessFirstChar = word.Remove(0, 1);
                        iterationPass = true;
                        if (item.LongName.IndexOf(wordLessFirstChar, StringComparison.OrdinalIgnoreCase) != -1)
                            iterationPass = false;
                        else if (itemGroupNames[(int)item.ItemGroup].IndexOf(wordLessFirstChar, StringComparison.OrdinalIgnoreCase) != -1)
                            iterationPass = false;
                    }
                    else
                    {
                        iterationPass = false;
                        if (item.LongName.IndexOf(word, StringComparison.OrdinalIgnoreCase) != -1)
                            iterationPass = true;
                        else if (itemGroupNames[(int)item.ItemGroup].IndexOf(word, StringComparison.OrdinalIgnoreCase) != -1)
                            iterationPass = true;
                    }

                    if (!iterationPass)
                        return false;
                }
            }

            return true;
        }

        private void ClearFilterFields()
        {
            filterString = null;
            localFilterTextBox.Text = string.Empty;
            UpdateFilterButton();
        }

        protected override void ConfirmTrade_OnButtonClick(DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton)
        {
            bool receivedLetterOfCredit = false;
            int currentBuildingID = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingKey;
            int goldSupply = 0;
            foreach (KeyValuePair<int, SaveDataTestingScript1.ShopData> kvp in SaveDataTestingScript1.SaveDataTestingScript1.ShopBuildingData)
            {
                if (kvp.Key == currentBuildingID)
                    goldSupply = kvp.Value.CurrentGoldSupply;
            }

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
                Refresh();
            }
            CloseWindow();
            if (receivedLetterOfCredit)
                DaggerfallUI.MessageBox(TextManager.Instance.GetText(textDatabase, "letterOfCredit"));
        }

        protected void ConfirmPoorTrade_OnButtonClick(DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton)
        {
            bool receivedLetterOfCredit = false;
            int currentBuildingID = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingKey;
            int goldSupply = 0;
            foreach (KeyValuePair<int, SaveDataTestingScript1.ShopData> kvp in SaveDataTestingScript1.SaveDataTestingScript1.ShopBuildingData)
            {
                if (kvp.Key == currentBuildingID)
                    goldSupply = kvp.Value.CurrentGoldSupply;
            }

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
                Refresh();
            }
            CloseWindow();
            if (receivedLetterOfCredit)
                DaggerfallUI.MessageBox(TextManager.Instance.GetText(textDatabase, "letterOfCredit"));
        }

        private void LocalTestButtonOne_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            const int notEnoughGoldId = 454;

            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            DaggerfallUI.MessageBox(notEnoughGoldId);
            filterButtonNeedUpdate = true;
        }

        public void UpdateShopGoldDisplay()
        {
            int currentBuildingID = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingKey;
            int goldSupply = 0;
            foreach (KeyValuePair<int, SaveDataTestingScript1.ShopData> kvp in SaveDataTestingScript1.SaveDataTestingScript1.ShopBuildingData)
            {
                if (kvp.Key == currentBuildingID)
                    goldSupply = kvp.Value.CurrentGoldSupply;
            }

            localTextLabelTwo = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(263, 41), string.Empty, NativePanel);
            localTextLabelTwo.TextScale = 0.85f;                                // Figure out how to get the ShopGold display value to update whenever a trade is complete. As I have it, now it just keeps layering new text labels over one another, not sure how to "remove" the last one and just update it or something while still in the window. May have to ask the forums, but will experiment more tomorrow with this and how it works in "DaggerfallTradeWindow.cs".
            localTextLabelTwo.Text = "Shop Gold: " + goldSupply.ToString();
        }

        public override void Refresh(bool refreshPaperDoll = true)
        {
            if (!IsSetup)
                return;

            base.Refresh(refreshPaperDoll);

            UpdateRepairTimes(false);
            UpdateCostAndGold();
            UpdateShopGoldDisplay();
        }

        private void LocalFilterButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            filterString = "";
            localFilterTextBox.SetFocus();
            filterButtonNeedUpdate = true;
        }

        private void LocalCloseFilterButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            ClearFilterFields();
            Refresh(false);
            filterButtonNeedUpdate = true;
        }


        private void LocalFilterTextBox_OnType()
        {
            filterString = localFilterTextBox.Text.ToLower();
            Refresh(false);
        }
    }

}

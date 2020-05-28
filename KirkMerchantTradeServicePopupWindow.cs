using UnityEngine;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Utility.AssetInjection;
using System.IO;
using SaveDataTestingScript1;

namespace DaggerfallWorkshop.Game.UserInterfaceWindows
{
    public class KirkMerchantTradeServicePopupWindow : DaggerfallMerchantServicePopupWindow
    {
        internal const string TexturesFolder = "Test_Textures";
        TextFile.Token[] tokens = null;
        Entity.PlayerEntity player = GameManager.Instance.PlayerEntity;
        int buildQual = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.quality;
        int currentBuildingID = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingKey;
        ShopData sd;
        bool investedFlag = false;
        int shopAttitude = 0;
        int investOffer = 0;


        #region UI Rects

        new Rect talkButtonRect = new Rect(5, 5, 120, 7);
        Rect investButtonRect = new Rect(5, 23, 120, 7);
        new Rect serviceButtonRect = new Rect(5, 14, 120, 7);
        new Rect exitButtonRect = new Rect(44, 24, 43, 15);

        #endregion

        #region UI Controls

        Button investButton = new Button();

        #endregion

        #region Fields

        new const string baseTextureName = "TEST_2";      // Talk / Sell

        #endregion

        #region Constructors

        public KirkMerchantTradeServicePopupWindow(IUserInterfaceManager uiManager, StaticNPC npc, Services service)
            : base(uiManager, npc, service)
        {

        }

        #endregion

        

        protected override void Setup()
        {
            // Load all textures
            LoadTextures();

            // Create interface panel
            mainPanel.HorizontalAlignment = HorizontalAlignment.Center;
            mainPanel.VerticalAlignment = VerticalAlignment.Middle;
            mainPanel.BackgroundTexture = baseTexture;
            mainPanel.Position = new Vector2(0, 50);
            mainPanel.Size = new Vector2(130, 51);

            // Talk button
            talkButton = DaggerfallUI.AddButton(talkButtonRect, mainPanel);
            talkButton.BackgroundColor = new Color(0.9f, 0.1f, 0.5f, 0.75f);
            talkButton.OnMouseClick += TalkButton_OnMouseClick;

            // Invest button
            investButton = DaggerfallUI.AddButton(investButtonRect, mainPanel);
            investButton.BackgroundColor = new Color(0.9f, 0.1f, 0.5f, 0.75f);
            investButton.OnMouseClick += InvestButton_OnMouseClick;

            // Service button
            serviceLabel.Position = new Vector2(0, 1);
            serviceLabel.ShadowPosition = Vector2.zero;
            serviceLabel.HorizontalAlignment = HorizontalAlignment.Center;
            serviceLabel.Text = GetServiceLabelText();
            serviceButton = DaggerfallUI.AddButton(serviceButtonRect, mainPanel);
            serviceButton.BackgroundColor = new Color(0.9f, 0.1f, 0.5f, 0.75f);
            serviceButton.Components.Add(serviceLabel);
            serviceButton.OnMouseClick += ServiceButton_OnMouseClick;

            // Exit button
            exitButton = DaggerfallUI.AddButton(exitButtonRect, mainPanel);
            exitButton.BackgroundColor = new Color(0.9f, 0.1f, 0.5f, 0.75f);
            exitButton.OnMouseClick += ExitButton_OnMouseClick;

            NativePanel.Components.Add(mainPanel);
        }

        protected override void LoadTextures()
        {
            Debug.Log("FUCK THIS BULLSHIT!");

            Texture2D tex;
			
            //TextureReplacement.TryImportTextureFromLooseFiles(Path.Combine(TexturesFolder, baseTextureName), false, false, true, out tex);
            TextureReplacement.TryImportTexture(baseTextureName, true, out tex);

            Debug.Log("Texture is:" + tex.ToString());

            if (tex == null)
                Debug.Log("Not Working!");
			else
				Debug.Log("This May Be Working!");

            baseTexture = tex;
        }

        #region Event Handlers

        protected void InvestButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            int mercSkill = player.Skills.GetLiveSkillValue(DFCareer.Skills.Mercantile);
            int playerIntell = player.Stats.LiveIntelligence;
            if (SaveDataTestingScript1.SaveDataTestingScript1.ShopBuildingData.TryGetValue(currentBuildingID, out sd))
            {
                investedFlag = sd.InvestedIn;
                shopAttitude = sd.ShopAttitude;
            }

            if (playerIntell >= 30 && mercSkill >= 40)
            {
                DaggerfallInputMessageBox investMessageBox = new DaggerfallInputMessageBox(uiManager, this);
                if (buildQual <= 3) // 01 - 03
                {
                    if (shopAttitude == 0)
                        tokens = KirkTextTokenHolder.ShopTextTokensNice(10);
                    else
                        tokens = KirkTextTokenHolder.ShopTextTokensMean(13);
                }
                else if (buildQual <= 7) // 04 - 07
                {
                    if (shopAttitude == 0)
                        tokens = KirkTextTokenHolder.ShopTextTokensNice(1);
                    else
                        tokens = KirkTextTokenHolder.ShopTextTokensMean(9);
                }
                else if (buildQual <= 17) // 08 - 17
                {
                    if (shopAttitude == 0)
                        tokens = KirkTextTokenHolder.ShopTextTokensNice(14);
                    else
                        tokens = KirkTextTokenHolder.ShopTextTokensMean(17);
                }
                else                      // 18 - 20
                {
                    if (shopAttitude == 0)
                        tokens = KirkTextTokenHolder.ShopTextTokensNice(18);
                    else
                        tokens = KirkTextTokenHolder.ShopTextTokensMean(21);
                }

                if (investedFlag)
                {
                    if (buildQual <= 3) // 01 - 03
                    {
                        if (shopAttitude == 0)
                            tokens = KirkTextTokenHolder.ShopTextTokensNice(12);
                        else
                            tokens = KirkTextTokenHolder.ShopTextTokensMean(15);
                    }
                    else if (buildQual <= 7) // 04 - 07
                    {
                        if (shopAttitude == 0)
                            tokens = KirkTextTokenHolder.ShopTextTokensNice(2);
                        else
                            tokens = KirkTextTokenHolder.ShopTextTokensMean(11);
                    }
                    else if (buildQual <= 17) // 08 - 17
                    {
                        if (shopAttitude == 0)
                            tokens = KirkTextTokenHolder.ShopTextTokensNice(16);
                        else
                            tokens = KirkTextTokenHolder.ShopTextTokensMean(19);
                    }
                    else                      // 18 - 20
                    {
                        if (shopAttitude == 0)
                            tokens = KirkTextTokenHolder.ShopTextTokensNice(20);
                        else
                            tokens = KirkTextTokenHolder.ShopTextTokensMean(23);
                    }
                }
                investMessageBox.SetTextTokens(tokens);
                investMessageBox.TextPanelDistanceY = 0;
                investMessageBox.InputDistanceX = 24;
                investMessageBox.InputDistanceY = 2;
                investMessageBox.TextBox.Numeric = true;
                investMessageBox.TextBox.MaxCharacters = 9;
                investMessageBox.TextBox.Text = "1";
                investMessageBox.OnGotUserInput += InvestMessageBox_OnGotUserInput;
                investMessageBox.Show();
            }
            else if (shopAttitude == 1 && buildQual <= 7 && playerIntell < 30)
            {
                DaggerfallInputMessageBox scamMessageBox = new DaggerfallInputMessageBox(uiManager, this);
                tokens = KirkTextTokenHolder.ShopTextTokensMean(1);
				scamMessageBox.InputDistanceY = 2;
                if (investedFlag)
                //if (test == 0)
                {
                    tokens = KirkTextTokenHolder.ShopTextTokensMean(2);
					scamMessageBox.InputDistanceY = 9;
                }
                scamMessageBox.SetTextTokens(tokens);
                scamMessageBox.TextPanelDistanceY = 0;
                scamMessageBox.InputDistanceX = 24;
                scamMessageBox.TextBox.Numeric = true;
                scamMessageBox.TextBox.MaxCharacters = 9;
                scamMessageBox.TextBox.Text = "1";
                scamMessageBox.OnGotUserInput += ScamMessageBox_OnGotUserInput;
                scamMessageBox.Show();
            }
            else if (playerIntell < 30)
            {
                tokens = KirkTextTokenHolder.ShopTextTokensNeutral(1);
                DaggerfallUI.MessageBox(tokens);
            }
            else
            {
                tokens = KirkTextTokenHolder.ShopTextTokensNeutral(2);
                DaggerfallUI.MessageBox(tokens);
            }
        }

        protected void InvestMessageBox_OnGotUserInput(DaggerfallInputMessageBox sender, string input)
        {
            int mercSkill = player.Skills.GetLiveSkillValue(DFCareer.Skills.Mercantile);
            int playerIntell = player.Stats.LiveIntelligence;
            if (SaveDataTestingScript1.SaveDataTestingScript1.ShopBuildingData.TryGetValue(currentBuildingID, out sd))
            {
                investedFlag = sd.InvestedIn;
                shopAttitude = sd.ShopAttitude;
            }

            bool result = int.TryParse(input, out investOffer);
            if (!result || investOffer < 1000)
            {
                tokens = KirkTextTokenHolder.ShopTextTokensNeutral(5);
                DaggerfallUI.MessageBox(tokens);
				return;
            }

            DaggerfallMessageBox investConfimBox = new DaggerfallMessageBox(uiManager, this);
            if (buildQual <= 3) // 01 - 03
            {
                if (shopAttitude == 0)
                    tokens = KirkTextTokenHolder.ShopTextTokensNice(11, investOffer);
                else
                    tokens = KirkTextTokenHolder.ShopTextTokensMean(14, investOffer);
            }
            else if (buildQual <= 7) // 04 - 07
            {
                if (shopAttitude == 0)
                    tokens = KirkTextTokenHolder.ShopTextTokensNice(3, investOffer);
                else
                    tokens = KirkTextTokenHolder.ShopTextTokensMean(10, investOffer);
            }
            else if (buildQual <= 17) // 08 - 17
            {
                if (shopAttitude == 0)
                    tokens = KirkTextTokenHolder.ShopTextTokensNice(15, investOffer);
                else
                    tokens = KirkTextTokenHolder.ShopTextTokensMean(18, investOffer);
            }
            else                      // 18 - 20
            {
                if (shopAttitude == 0)
                    tokens = KirkTextTokenHolder.ShopTextTokensNice(19, investOffer);
                else
                    tokens = KirkTextTokenHolder.ShopTextTokensMean(22, investOffer);
            }
            if (investedFlag)
            {
                if (buildQual <= 3) // 01 - 03
                {
                    if (shopAttitude == 0)
                        tokens = KirkTextTokenHolder.ShopTextTokensNice(13, investOffer);
                    else
                        tokens = KirkTextTokenHolder.ShopTextTokensMean(16, investOffer);
                }
                else if (buildQual <= 7) // 04 - 07
                {
                    if (shopAttitude == 0)
                        tokens = KirkTextTokenHolder.ShopTextTokensNice(9, investOffer);
                    else
                        tokens = KirkTextTokenHolder.ShopTextTokensMean(12, investOffer);
                }
                else if (buildQual <= 17) // 08 - 17
                {
                    if (shopAttitude == 0)
                        tokens = KirkTextTokenHolder.ShopTextTokensNice(17, investOffer);
                    else
                        tokens = KirkTextTokenHolder.ShopTextTokensMean(20, investOffer);
                }
                else                      // 18 - 20
                {
                    if (shopAttitude == 0)
                        tokens = KirkTextTokenHolder.ShopTextTokensNice(21, investOffer);
                    else
                        tokens = KirkTextTokenHolder.ShopTextTokensMean(24, investOffer);
                }
            }
            investConfimBox.SetTextTokens(tokens);
            investConfimBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.Yes);
            investConfimBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.No);
            investConfimBox.OnButtonClick += ConfirmInvestment_OnButtonClick;
            uiManager.PushWindow(investConfimBox);
        }

        protected void ScamMessageBox_OnGotUserInput(DaggerfallInputMessageBox sender, string input)
        {
            int mercSkill = player.Skills.GetLiveSkillValue(DFCareer.Skills.Mercantile);
            int playerIntell = player.Stats.LiveIntelligence;
            if (SaveDataTestingScript1.SaveDataTestingScript1.ShopBuildingData.TryGetValue(currentBuildingID, out sd))
            {
                investedFlag = sd.InvestedIn;
                shopAttitude = sd.ShopAttitude;
            }

            bool result = int.TryParse(input, out investOffer);
            if (!result || investOffer < 1000)
            {
                tokens = KirkTextTokenHolder.ShopTextTokensMean(25);
                DaggerfallUI.MessageBox(tokens);
				return;
            }

            DaggerfallMessageBox scamConfimBox = new DaggerfallMessageBox(uiManager, this);
            tokens = KirkTextTokenHolder.ShopTextTokensMean(3);
            scamConfimBox.SetTextTokens(tokens);
            scamConfimBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.Yes);
            scamConfimBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.No);
            scamConfimBox.OnButtonClick += ConfirmGettingScammed_OnButtonClick;
            uiManager.PushWindow(scamConfimBox);
        }

        protected void ConfirmInvestment_OnButtonClick(DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton)
        {
            int mercSkill = player.Skills.GetLiveSkillValue(DFCareer.Skills.Mercantile);
            int playerIntell = player.Stats.LiveIntelligence;
            if (SaveDataTestingScript1.SaveDataTestingScript1.ShopBuildingData.TryGetValue(currentBuildingID, out sd))
            {
                investedFlag = sd.InvestedIn;
                shopAttitude = sd.ShopAttitude;
            }

            CloseWindow();
            if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.Yes)
            {
                Entity.PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
                if (playerEntity.GetGoldAmount() >= investOffer)
                {
                    playerEntity.DeductGoldAmount(investOffer);
                    DaggerfallUI.Instance.PlayOneShot(SoundClips.GoldPieces);
                    if (buildQual <= 3) // 01 - 03
                    {
                        if (shopAttitude == 0)
                            DaggerfallUI.MessageBox("I'll do my best, partner.");
                        else
                            DaggerfallUI.MessageBox("Thanks for the gold, ya nutter.");
                    }
                    else if (buildQual <= 7) // 04 - 07
                    {
                        if (shopAttitude == 0)
                            DaggerfallUI.MessageBox("Ya won't be disappointed, boss.");
                        else
                            DaggerfallUI.MessageBox("Really care for those fish, huh?");
                    }
                    else if (buildQual <= 17) // 08 - 17
                    {
                        if (shopAttitude == 0)
                            DaggerfallUI.MessageBox("Not a bad signature, do you practice?");
                        else
                            DaggerfallUI.MessageBox("Your signature is atrocious, by the way.");
                    }
                    else if (buildQual >= 18) // 18 - 20
                    {
                        if (shopAttitude == 0)
                            DaggerfallUI.MessageBox("I never disappoint when it comes to turning a profit.");
                        else
                            DaggerfallUI.MessageBox("Wipe your feet next time, you're tracking sludge in, lamprey.");
                    }
                    SaveDataTestingScript1.SaveDataTestingScript1.UpdateInvestAmount(investOffer); // Likely have investment method call here that actually does the main sava-data changes and such.
                }
                else
                {
                    if (buildQual <= 3) // 01 - 03
                    {
                        if (shopAttitude == 0)
                            DaggerfallUI.MessageBox("I think you counted wrong, you don't have that much.");
                        else
                            DaggerfallUI.MessageBox("I know you don't have that much, stop lyin' ya idiot!");
                    }
                    else if (buildQual <= 7) // 04 - 07
                    {
                        if (shopAttitude == 0)
                            DaggerfallUI.MessageBox("Might want to recount, boss, cause I don't see that amount.");
                        else
                            DaggerfallUI.MessageBox("Who ya tryna impress, huh? I know you don't have that much.");
                    }
                    else if (buildQual <= 17) // 08 - 17
                    {
                        if (shopAttitude == 0)
                            DaggerfallUI.MessageBox("Sorry, but it appears your funds are less than you suggest.");
                        else
                            DaggerfallUI.MessageBox("Quit wasting my damn time, you dolt, you don't have that much!");
                    }
                    else if (buildQual >= 18) // 18 - 20
                    {
                        if (shopAttitude == 0)
                            DaggerfallUI.MessageBox("I think you need to hire a new accountant, because you don't have that.");
                        else
                            DaggerfallUI.MessageBox("I need to charge more for my time, so leeches like you won't waste it so often!");
                    }
                }
            }
            else if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.No)
            {
                if (buildQual <= 3) // 01 - 03
                {
                    if (shopAttitude == 0)
                        DaggerfallUI.MessageBox("I understand your reluctance, just look around, haha!");
                    else
                        DaggerfallUI.MessageBox("Why you gettin' my hopes up, just to dash them down?");
                }
                else if (buildQual <= 7) // 04 - 07
                {
                    if (shopAttitude == 0)
                        DaggerfallUI.MessageBox("That's fine, I would not take such a gamble myself either.");
                    else
                        DaggerfallUI.MessageBox("Why you tryna bait me in just to pull away the line?");
                }
                else if (buildQual <= 17) // 08 - 17
                {
                    if (shopAttitude == 0)
                        DaggerfallUI.MessageBox("Very well, thankfully parchment is cheap around here.");
                    else
                        DaggerfallUI.MessageBox("What's with the indecisive always wasting my time in this town?");
                }
                else if (buildQual >= 18) // 18 - 20
                {
                    if (shopAttitude == 0)
                        DaggerfallUI.MessageBox("So be it, ask again when you are ready to commit.");
                    else
                        DaggerfallUI.MessageBox("The parasite sucks my precious time away once again, a pity.");
                }
            }
        }

        protected void ConfirmGettingScammed_OnButtonClick(DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton)
        {
            int mercSkill = player.Skills.GetLiveSkillValue(DFCareer.Skills.Mercantile);
            int playerIntell = player.Stats.LiveIntelligence;
            if (SaveDataTestingScript1.SaveDataTestingScript1.ShopBuildingData.TryGetValue(currentBuildingID, out sd))
            {
                investedFlag = sd.InvestedIn;
                shopAttitude = sd.ShopAttitude;
            }

            CloseWindow();
            if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.Yes)
            {
                Entity.PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
                if (playerEntity.GetGoldAmount() >= investOffer)
                {
                    playerEntity.DeductGoldAmount(investOffer);
                    DaggerfallUI.Instance.PlayOneShot(SoundClips.GoldPieces);
                    tokens = KirkTextTokenHolder.ShopTextTokensNeutral(3);
                    if (investedFlag)
                    {
                        tokens = KirkTextTokenHolder.ShopTextTokensNeutral(4);
                    }
                    DaggerfallUI.MessageBox(tokens);
                    investOffer = 0;
                    SaveDataTestingScript1.SaveDataTestingScript1.UpdateInvestAmount(investOffer); // Likely have investment method call here that actually does the main sava-data changes and such. In this case, don't increase investment amount, just give it the "flag" that this store was invested in at some point.
                }
                else
                    DaggerfallUI.MessageBox("Good joke there, you really got me there, ya jerk..."); // Textbox message if you don't have enough gold to cover what you have offered. Possibly try and make letters of credit a viable offer.
            }
            else if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.No)
                DaggerfallUI.MessageBox("Yeah, I was just joking as well, haha...");
        }
        #endregion
    }
}
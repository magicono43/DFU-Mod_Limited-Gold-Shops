using UnityEngine;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Utility.AssetInjection;
using System.IO;

namespace DaggerfallWorkshop.Game.UserInterfaceWindows
{
    public class KirkMerchantTradeRepairPopupWindow : DaggerfallMerchantRepairPopupWindow
    {
        internal const string TexturesFolder = "Test_Textures";

        #region UI Rects

        new Rect repairButtonRect = new Rect(5, 5, 120, 7);
        new Rect talkButtonRect = new Rect(5, 14, 120, 7);
        new Rect sellButtonRect = new Rect(5, 23, 120, 7);
        new Rect exitButtonRect = new Rect(44, 33, 43, 15);

        #endregion

        #region Fields

        new const string baseTextureName = "TEST_1";      // Repair / Talk / Sell
		//new const string baseTextureName = "Bitch_Ass_2";      // Repair / Talk / Sell
        new Texture2D baseTexture;

        #endregion

        #region Constructors

        public KirkMerchantTradeRepairPopupWindow(IUserInterfaceManager uiManager, StaticNPC npc)
            : base(uiManager, npc)
        {
            
        }

        protected override void LoadTextures()
        {
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


        #endregion
    }
}
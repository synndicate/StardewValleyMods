using System.Collections.Generic;
using System.Linq;
using Harmony;
using StardewValley;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace Synndicate.Stardew.CustomProfitBreakdown
{
    public class ModEntry : Mod, IAssetEditor
    {
        /*****************
        ** Fields
        *****************/

        private ModConfig Config;

        /*****************
        ** Public Methods
        *****************/

        public override void Entry(IModHelper helper)
        {
            var config = helper.ReadConfig<ModConfig>();
            config.Validate(Monitor);

            helper.Events.Input.ButtonPressed += InputButtonPressed;

            Harmony.Initialize(config);
            HarmonyInstance.Create(ModManifest.UniqueID).Patch(
                original: AccessTools.Method(typeof(StardewValley.Menus.ShippingMenu), nameof(StardewValley.Menus.ShippingMenu.getCategoryIndexForObject)),
                postfix: new HarmonyMethod(typeof(Harmony), nameof(Harmony.GetCategoryIndex_Postfix)));

            Config = config;
        }

        /*****************
        ** IAssetEditor
        *****************/

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Strings\\StringsFromCSFiles");
        }

        public void Edit<T>(IAssetData asset)
        {
            IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;

            data["ShippingMenu.cs.11389"] = " " + Config.Section1.Name;
            data["ShippingMenu.cs.11390"] = " " + Config.Section2.Name;
            data["ShippingMenu.cs.11391"] = " " + Config.Section3.Name;
            data["ShippingMenu.cs.11392"] = " " + Config.Section4.Name;
            data["ShippingMenu.cs.11393"] = " " + "Other";
            data["ShippingMenu.cs.11394"] = " " + "Total";
        }

        /*****************
        ** Event Handlers
        *****************/

        private void InputButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady) { return; }

            if (e.Button == Config.BinTotalKey)
            {
                var binTotal = Game1.getFarm().getShippingBin(Game1.player)
                    .Aggregate(0, (acc, x) => acc + Utility.getSellToStorePriceOfItem(x));

                Game1.addHUDMessage(new HUDMessage($"Bin Total: {binTotal}", 2));
            }
        }
    }
}

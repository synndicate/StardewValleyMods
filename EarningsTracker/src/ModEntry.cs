using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;


namespace EarningsTracker
{
    public sealed class ModEntry : Mod
    {
        /******************
        ** Private Fields
        ******************/

        private ModConfig Config;
        private EarningsTracker EarningsTracker;
        private bool InShopMenu = false;


        /******************
        ** Entry Point
        ******************/

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            EarningsTracker = new EarningsTracker(Game1.MasterPlayer, Config, Monitor);

            helper.Events.Display.MenuChanged += DisplayMenuChanged;
            helper.Events.GameLoop.DayStarted += GameLoopDayStarted;
            helper.Events.GameLoop.DayEnding += GameLoopDayEnding;
            helper.Events.GameLoop.SaveLoaded += GameLoopSaveLoaded;
            helper.Events.GameLoop.Saving += GameLoopSaving;
            helper.Events.Input.ButtonPressed += InputButtonPressed;
            helper.Events.Player.InventoryChanged += PlayerInventoryChanged;

            HarmonyCategoryPatch.Initialize(Config);

            var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Menus.ShippingMenu), nameof(StardewValley.Menus.ShippingMenu.getCategoryIndexForObject)),
                postfix: new HarmonyMethod(typeof(HarmonyCategoryPatch), nameof(HarmonyCategoryPatch.GetCategoryIndex_Postfix)));
        }

        /******************
        ** Event Handlers
        ******************/

        private void DisplayMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (!Context.IsWorldReady) { return; }
                
            if (e.NewMenu is StardewValley.Menus.ShopMenu)
            {
                InShopMenu = true;
            }
            else if (e.OldMenu is StardewValley.Menus.ShopMenu)
            {
                InShopMenu = false;
            }

            if (Game1.player.totalMoneyEarned <= EarningsTracker.TotalTrackedEarnings) { return; }

            if (e.OldMenu is StardewValley.Menus.GameMenu) { EarningsTracker.AddTrashEarning(Game1.player); }
            else if (e.OldMenu is StardewValley.Menus.QuestLog) { EarningsTracker.AddQuestEarning(Game1.player); }
            else if (e.OldMenu is StardewValley.Menus.AnimalQueryMenu) { EarningsTracker.AddAnimalEarning(Game1.player); }
            else if (e.NewMenu is StardewValley.Menus.LetterViewerMenu) { EarningsTracker.AddMailEarning(Game1.player); }
            else { EarningsTracker.AddUnknownEarning(Game1.player); }
        }

        private void GameLoopDayStarted(object sender, DayStartedEventArgs e)
        {
            EarningsTracker.UpdateTrackedEarnings(Game1.player);
        }

        private void GameLoopDayEnding(object sender, DayEndingEventArgs e)
        {
            // Game clears shipping bin if done later
            Helper.Data.WriteJsonFile(ModData.JsonPath(StardewModdingAPI.Constants.SaveFolderName), EarningsTracker.PackageDayData(Game1.player));
        }

        private void GameLoopSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            var data = Helper.Data.ReadSaveData<ModData>(ModData.DataKey);
            if (data != null)
            {
                Monitor.Log("====================", LogLevel.Warn);
                Monitor.Log("Save Data Loaded", LogLevel.Warn);
                Monitor.Log("====================", LogLevel.Warn);
                Monitor.Log($"Save name: {data.SaveName}", LogLevel.Warn);
                // pass to data manager in future
            }
        }

        private void GameLoopSaving(object sender, SavingEventArgs e)
        {
            var data = new ModData(StardewModdingAPI.Constants.SaveFolderName); // get this from data manager in future
            Helper.Data.WriteSaveData(ModData.DataKey, data);
        }

        private void InputButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady) { return; }

            if (e.Button == SButton.G)
            {
                var binTotal = Game1.getFarm().getShippingBin(Game1.player)
                    .Aggregate(0, (acc, x) => acc + Utility.getSellToStorePriceOfItem(x));

                Game1.addHUDMessage(new HUDMessage($"Bin Total: {binTotal}", 2));
            }
        }

        private void PlayerInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            if (!e.IsLocalPlayer) { return; }
            if (!InShopMenu) { return; }
            if (Game1.player.totalMoneyEarned <= EarningsTracker.TotalTrackedEarnings) { return; }

            if (e.Removed.Count() > 0) { EarningsTracker.AddItemSoldEvent(Game1.player, e.Removed); }
            else if (e.QuantityChanged.Count() > 0) { EarningsTracker.AddItemSoldEvent(Game1.player, e.QuantityChanged); }
            else { Monitor.Log("Unaccounted earnings from an item removed from inventory", LogLevel.Error); }
        }
    }
}

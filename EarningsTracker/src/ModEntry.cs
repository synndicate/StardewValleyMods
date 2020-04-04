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

        private DataManager DataManager;

        private bool InShopMenu = false;

        /******************
        ** Entry Point
        ******************/

        public override void Entry(IModHelper helper)
        {
            var config = Helper.ReadConfig<ModConfig>();
            Validate(config);

            DataManager = new DataManager(config, Monitor);

            helper.Events.Display.MenuChanged += DisplayMenuChanged;
            helper.Events.GameLoop.DayStarted += GameLoopDayStarted;
            helper.Events.GameLoop.DayEnding += GameLoopDayEnding;
            helper.Events.GameLoop.GameLaunched += GameLoopGameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoopSaveLoaded;
            helper.Events.GameLoop.Saving += GameLoopSaving;
            helper.Events.Input.ButtonPressed += InputButtonPressed;
            helper.Events.Player.InventoryChanged += PlayerInventoryChanged;

            HarmonyCategoryPatch.Initialize(config);

            var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Menus.ShippingMenu), nameof(StardewValley.Menus.ShippingMenu.getCategoryIndexForObject)),
                postfix: new HarmonyMethod(typeof(HarmonyCategoryPatch), nameof(HarmonyCategoryPatch.GetCategoryIndex_Postfix)));
        }

        private void Validate(ModConfig config)
        {
            try
            {
                config.Validate();
            }
            catch (InvalidOperationException)
            {
                Monitor.Log($"Failed to load config.json", LogLevel.Error);
                throw;
            }

            if (config.UseCustomCategories)
            {
                Monitor.Log($"Loaded config.json with {config.CategoryNames().Count()} custom categories", LogLevel.Info);
            }
            else
            {
                Monitor.Log($"Loaded config.json without custom categories", LogLevel.Info);
            }
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

            if (Game1.player.totalMoneyEarned <= DataManager.TotalTrackedEarnings()) { return; }

            if (e.OldMenu is StardewValley.Menus.GameMenu) { DataManager.AddTrashEarning(); }
            else if (e.OldMenu is StardewValley.Menus.QuestLog) { DataManager.AddQuestEarning(); }
            else if (e.OldMenu is StardewValley.Menus.AnimalQueryMenu) { DataManager.AddAnimalEarning(); }
            else if (e.NewMenu is StardewValley.Menus.LetterViewerMenu) { DataManager.AddMailEarning(); }
            else { DataManager.AddUnknownEarning(); }
        }

        private void GameLoopDayStarted(object sender, DayStartedEventArgs e)
        {
            DataManager.UpdateAllPlayerEarnings();
        }

        private void GameLoopDayEnding(object sender, DayEndingEventArgs e)
        {
            var data = DataManager.PackageEarningsData();
            Helper.Data.WriteJsonFile(ModData.JsonPath(StardewModdingAPI.Constants.SaveFolderName), data);
        }

        private void GameLoopGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            DataManager.Register(Game1.MasterPlayer);
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
            if (Game1.player.totalMoneyEarned <= DataManager.TotalTrackedEarnings()) { return; }

            if (e.Removed.Count() > 0) { DataManager.AddItemSoldEvent(e.Removed); }
            else if (e.QuantityChanged.Count() > 0) { DataManager.AddItemSoldEvent(e.QuantityChanged); }
            else { Monitor.Log("Unaccounted earnings from an item removed from inventory", LogLevel.Error); }
        }
    }
}

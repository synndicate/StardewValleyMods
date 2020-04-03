using System;
using System.Collections.Generic;
using System.Linq;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace EarningsTracker
{
    using Category = String;

    public sealed class EarningsTracker : Mod
    {
        /******************
        ** Private Fields
        ******************/

        private ModConfig Config;
        private DataManager DataManager;
        private bool InShopMenu = false;


        /******************
        ** Public Methods
        ******************/

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            DataManager = new DataManager(Game1.MasterPlayer, Config, Monitor);

            helper.Events.Display.MenuChanged += DisplayMenuChanged;
            helper.Events.GameLoop.DayStarted += GameLoopDayStarted;
            helper.Events.GameLoop.DayEnding += GameLoopDayEnding;
            helper.Events.GameLoop.SaveLoaded += GameLoopSaveLoaded;
            helper.Events.GameLoop.Saving += GameLoopSaving;
            helper.Events.Player.InventoryChanged += PlayerInventoryChanged;

            helper.Events.Input.ButtonPressed += InputButtonPressed;
        }

        /******************
        ** temp debugging methods
        ******************/

        private void InputButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (e.Button == SButton.J)
            {
                NetCollection<Item> bin = Game1.getFarm().getShippingBin(Game1.player);
                int binTotal = 0;

                foreach (Item item in bin)
                {
                    binTotal += Utility.getSellToStorePriceOfItem(item);
                }

                Monitor.Log($"Current Bin Total: {binTotal}", LogLevel.Warn);
                Game1.addHUDMessage(new HUDMessage($"Bin Total: {binTotal}", 2));
            }
        }

        /******************
        ** Event Handlers
        ******************/

        private void DisplayMenuChanged(object sender, MenuChangedEventArgs e)
        {
            /*Monitor.Log("====================", LogLevel.Warn);
            Monitor.Log("Display Menu Changed", LogLevel.Warn);
            Monitor.Log("====================", LogLevel.Warn);
            Monitor.Log($"\tOld menu: {e.OldMenu?.GetType()?.ToString() ?? "null"}", LogLevel.Warn);
            Monitor.Log($"\tNew menu: {e.NewMenu?.GetType()?.ToString() ?? "null"}", LogLevel.Warn);*/

            if (!Context.IsWorldReady) { return; }
                
            if (e.NewMenu is StardewValley.Menus.ShopMenu)
            {
                InShopMenu = true;
            }
            else if (e.OldMenu is StardewValley.Menus.ShopMenu)
            {
                InShopMenu = false;
            }

            if (Game1.player.totalMoneyEarned <= DataManager.TotalTrackedEarnings) { return; }

            if (e.OldMenu is StardewValley.Menus.GameMenu) { DataManager.AddTrashEarning(Game1.player); }
            else if (e.OldMenu is StardewValley.Menus.QuestLog) { DataManager.AddQuestEarning(Game1.player); }
            else if (e.OldMenu is StardewValley.Menus.AnimalQueryMenu) { DataManager.AddAnimalEarning(Game1.player); }
            else if (e.NewMenu is StardewValley.Menus.LetterViewerMenu) { DataManager.AddMailEarning(Game1.player); }
            else { DataManager.AddUnknownEarning(Game1.player); }
        }

        private void GameLoopDayStarted(object sender, DayStartedEventArgs e)
        {
            Monitor.Log("====================", LogLevel.Warn);
            Monitor.Log($"Day Started - {TodayAsString()}", LogLevel.Warn);
            Monitor.Log($"\tCurrent Earnings: {Game1.player.totalMoneyEarned}", LogLevel.Warn);
            Monitor.Log("====================", LogLevel.Warn);

            DataManager.UpdateTrackedEarnings(Game1.player);
        }

        private void GameLoopDayEnding(object sender, DayEndingEventArgs e)
        {
            // Game clears shipping bin if done later
            Helper.Data.WriteJsonFile(ModData.JsonPath(StardewModdingAPI.Constants.SaveFolderName), DataManager.PackageDayData(Game1.player));
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

        private void PlayerInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            if (!e.IsLocalPlayer) { return; }
            if (!InShopMenu) { return; }
            if (Game1.player.totalMoneyEarned <= DataManager.TotalTrackedEarnings) { return; }

            if (e.Removed.Count() > 0) { DataManager.AddItemSoldEvent(Game1.player, e.Removed); }
            else if (e.QuantityChanged.Count() > 0) { DataManager.AddItemSoldEvent(Game1.player, e.QuantityChanged); }
            else { Monitor.Log("Unaccounted earnings from an item removed from inventory", LogLevel.Error); }
        }

        /******************
        ** Utility Methods
        ******************/

        private string TodayAsString()
        {
            // Y1 Summer - Day 1 (Monday)
            SDate today = SDate.Now();
            return $"Y{today.Year} {today.Season.First().ToString().ToUpper() + today.Season.Substring(1)} - Day {today.Day} ({today.DayOfWeek})";
        }

    }
}

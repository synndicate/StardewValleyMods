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
    public class EarningsTracker : Mod
    {
        public override void Entry(IModHelper helper)
        {
            helper.Events.Display.MenuChanged += DisplayMenuChanged;
            helper.Events.GameLoop.DayStarted += GameLoopDayStarted;
            helper.Events.GameLoop.DayEnding += GameLoopDayEnding;
            helper.Events.GameLoop.SaveLoaded += GameLoopSaveLoaded;
            helper.Events.GameLoop.ReturnedToTitle += GameLoopReturnedToTitle;
            helper.Events.Player.InventoryChanged += PlayerInventoryChanged;
            helper.Events.Player.Warped += PlayerWarped;

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
        ** Private Fields
        ******************/

        private uint CurrentEarnings = 0;
        private bool HasSaveLoaded = false;
        private bool InShopMenu = false;

        /******************
        ** Event Handlers
        ******************/

        private void DisplayMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (!HasSaveLoaded) { return; }

            if (e.NewMenu is StardewValley.Menus.ShopMenu)
            {
                InShopMenu = true;
            }
            else if (e.OldMenu is StardewValley.Menus.ShopMenu)
            {
                InShopMenu = false;
            }

            if (e.OldMenu is StardewValley.Menus.GameMenu)
            {
                if (Game1.player.totalMoneyEarned > CurrentEarnings)
                {
                    Monitor.Log($"Earned {Game1.player.totalMoneyEarned - CurrentEarnings}g from trashing", LogLevel.Warn);
                    CurrentEarnings = Game1.player.totalMoneyEarned;
                }
            }
            else if (e.OldMenu is StardewValley.Menus.QuestLog)
            {
                if (Game1.player.totalMoneyEarned > CurrentEarnings)
                {
                    Monitor.Log($"Earned {Game1.player.totalMoneyEarned - CurrentEarnings}g from quest reward", LogLevel.Warn);
                    CurrentEarnings = Game1.player.totalMoneyEarned;
                }
            }
            else if (e.OldMenu is StardewValley.Menus.AnimalQueryMenu)
            {
                if (Game1.player.totalMoneyEarned > CurrentEarnings)
                {
                    Monitor.Log($"Earned {Game1.player.totalMoneyEarned - CurrentEarnings}g from selling animals", LogLevel.Warn);
                    CurrentEarnings = Game1.player.totalMoneyEarned;
                }
            }
            else if (e.NewMenu is StardewValley.Menus.LetterViewerMenu || e.OldMenu is StardewValley.Menus.LetterViewerMenu)
            {
                if (Game1.player.totalMoneyEarned > CurrentEarnings)
                {
                    Monitor.Log($"Earned {Game1.player.totalMoneyEarned - CurrentEarnings}g from mail", LogLevel.Warn);
                    CurrentEarnings = Game1.player.totalMoneyEarned;
                }
            }
            else
            {
                if (Game1.player.totalMoneyEarned > CurrentEarnings)
                {
                    Monitor.Log($"Earned {Game1.player.totalMoneyEarned - CurrentEarnings}g from unaccounted source", LogLevel.Error);
                    Monitor.Log($"CTE: {CurrentEarnings}, player: {Game1.player.totalMoneyEarned}", LogLevel.Error);

                    CurrentEarnings = Game1.player.totalMoneyEarned;
                }
            }
        }

        private void GameLoopDayStarted(object sender, DayStartedEventArgs e)
        {
            CurrentEarnings = Game1.player.totalMoneyEarned;
            Monitor.Log($"{TodayAsString()} Current Earnings: {CurrentEarnings}", LogLevel.Warn);
        }

        private void GameLoopDayEnding(object sender, DayEndingEventArgs e)
        {
            NetCollection<Item> bin = Game1.getFarm().getShippingBin(Game1.player);
            int binTotal = 0;

            foreach (Item item in bin)
            {
                binTotal += Utility.getSellToStorePriceOfItem(item);
            }

            CurrentEarnings += (uint)binTotal;
            Monitor.Log($"{TodayAsString()} Today's Earnings from Shipping: {binTotal}, Total Earnings: {CurrentEarnings}", LogLevel.Warn);
        }

        private void GameLoopReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            HasSaveLoaded = false;
        }

        private void GameLoopSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            HasSaveLoaded = true;
        }

        private void PlayerInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            if (!e.IsLocalPlayer) { return; }

            ParseInventoryChangedEvent(e);
        }

        // dont need but doesn't hurt as a margin of safety?
        private void PlayerWarped(object sender, WarpedEventArgs e)
        {
            CurrentEarnings = Game1.player.totalMoneyEarned;
        }

        /******************
        ** Private Methods
        ******************/

        private void ParseInventoryChangedEvent(InventoryChangedEventArgs e)
        {
            if (!InShopMenu) { return; }

            uint NewTotalEarnings = Game1.player.totalMoneyEarned;
            int checksum = 0;

            if (NewTotalEarnings <= CurrentEarnings) { return; }
            else if (e.Removed.Count() > 0)
            {
                foreach (Item item in e.Removed)
                {
                    var salePrice = Utility.getSellToStorePriceOfItem(item);

                    if (item is StardewValley.Object)
                    {
                        var obj = item as StardewValley.Object;

                        Monitor.Log($"\tSold: {item.Stack} {item.DisplayName} ({QualityIntToString(obj.quality)})", LogLevel.Warn);
                        Monitor.Log($"\tSale Price: {salePrice}", LogLevel.Warn);
                    }
                    else
                    {
                        Monitor.Log($"\tSold: {item.Stack} {item.DisplayName}", LogLevel.Warn);
                        Monitor.Log($"\tSale Price: {salePrice}", LogLevel.Warn);
                    }

                    checksum += salePrice;
                }

                if (checksum != NewTotalEarnings - CurrentEarnings)
                {
                    Monitor.Log("Earning failed checksum", LogLevel.Error);
                    Monitor.Log($"Change in earnings = {NewTotalEarnings - CurrentEarnings}", LogLevel.Error);
                    Monitor.Log($"Checksum = {checksum}", LogLevel.Error);
                }

                CurrentEarnings = NewTotalEarnings;
            }
            else if (e.QuantityChanged.Count() > 0)
            {
                foreach (ItemStackSizeChange change in e.QuantityChanged)
                {
                    var obj = change.Item as StardewValley.Object;
                    var sizeChange = change.OldSize - change.NewSize;
                    var salePrice = Utility.getSellToStorePriceOfItem(change.Item, false) * sizeChange;

                    Monitor.Log($"\tSold: {sizeChange} {change.Item.DisplayName} ({QualityIntToString(obj.quality)})", LogLevel.Warn);
                    Monitor.Log($"\tSale Price: {salePrice}", LogLevel.Warn);

                    checksum += salePrice;
                }

                if (checksum != NewTotalEarnings - CurrentEarnings)
                {
                    Monitor.Log("Earning failed checksum", LogLevel.Error);
                    Monitor.Log($"Change in earnings = {NewTotalEarnings - CurrentEarnings}", LogLevel.Error);
                    Monitor.Log($"Checksum = {checksum}", LogLevel.Error);
                }

                CurrentEarnings = NewTotalEarnings;
            }
            else
            {
                Monitor.Log("Unaccounted earnings from an item removed from inventory", LogLevel.Error);
            }
        }

        /******************
        ** Utility Methods
        ******************/
        private string QualityIntToString(Netcode.NetInt quality)
        {
            switch (quality)
            {
                case 0:
                    return "regular";
                case 1:
                    return "silver";
                case 2:
                    return "gold";
                case 3:
                    return "iridium";
                default:
                    return "unknown";
            }
        }

        private string TodayAsString()
        {
            // Y1 Summer - Day 1 (Monday)
            SDate today = SDate.Now();
            return $"Y{today.Year} {today.Season.First().ToString().ToUpper() + today.Season.Substring(1)} - Day {today.Day} ({today.DayOfWeek})";
        }

    }
}

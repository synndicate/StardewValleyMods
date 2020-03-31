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
            helper.Events.GameLoop.DayStarted += GameLoopDayStarted;
            helper.Events.GameLoop.DayEnding += GameLoopDayEnding;
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

                Monitor.Log($"bin total: {binTotal}", LogLevel.Debug);
                Game1.addHUDMessage(new HUDMessage($"Bin Total: {binTotal}", 2));
            }
        }

        /******************
        ** Private Fields
        ******************/

        private uint CurrentTotalEarnings = 0;


        /******************
        ** Event Handlers
        ******************/
        private void GameLoopDayStarted(object sender, DayStartedEventArgs e)
        {
            CurrentTotalEarnings = Game1.player.totalMoneyEarned;
            Monitor.Log($"total earnings: {CurrentTotalEarnings}", LogLevel.Debug);
        }

        private void GameLoopDayEnding(object sender, DayEndingEventArgs e)
        {
            NetCollection<Item> bin = Game1.getFarm().getShippingBin(Game1.player);
            int binTotal = 0;

            foreach (Item item in bin)
            {
                binTotal += Utility.getSellToStorePriceOfItem(item);
            }

            CurrentTotalEarnings += (uint)binTotal;
            Monitor.Log($"\n{TodayAsString()}\nTotal Earnings: {CurrentTotalEarnings}", LogLevel.Debug);
        }

        private void PlayerInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            if (!e.IsLocalPlayer) { return; }

            ParseInventoryChangedEvent(e);
        }

        /******************
        ** Private Methods
        ******************/

        private void ParseInventoryChangedEvent(InventoryChangedEventArgs e)
        {
            uint NewTotalEarnings = Game1.player.totalMoneyEarned;
            int checksum = 0;

            if (NewTotalEarnings <= CurrentTotalEarnings) { return; }
            else if (e.Removed.Count() > 0)
            {
                foreach (Item item in e.Removed)
                {
                    var obj = item as StardewValley.Object;
                    var salePrice = Utility.getSellToStorePriceOfItem(item);

                    Monitor.Log($"\tSold: {item.Stack} {item.DisplayName} ({QualityIntToString(obj.quality)})", LogLevel.Debug);
                    Monitor.Log($"\tSale Price: {salePrice}", LogLevel.Debug);

                    checksum += salePrice;
                }

                if (checksum != NewTotalEarnings - CurrentTotalEarnings)
                {
                    Monitor.Log("Earning failed checksum", LogLevel.Warn);
                    Monitor.Log($"Change in earnings = {NewTotalEarnings - CurrentTotalEarnings}", LogLevel.Warn);
                    Monitor.Log($"Checksum = {checksum}", LogLevel.Warn);
                }

                CurrentTotalEarnings = NewTotalEarnings;
            }
            else if (e.QuantityChanged.Count() > 0)
            {
                foreach (ItemStackSizeChange change in e.QuantityChanged)
                {
                    var obj = change.Item as StardewValley.Object;
                    var sizeChange = change.OldSize - change.NewSize;
                    var salePrice = Utility.getSellToStorePriceOfItem(change.Item, false) * sizeChange;

                    Monitor.Log($"\tSold: {sizeChange} {change.Item.DisplayName} ({QualityIntToString(obj.quality)})", LogLevel.Debug);
                    Monitor.Log($"\tSale Price: {salePrice}", LogLevel.Debug);

                    checksum += salePrice;
                }

                if (checksum != NewTotalEarnings - CurrentTotalEarnings)
                {
                    Monitor.Log("Earning failed checksum", LogLevel.Warn);
                    Monitor.Log($"Change in earnings = {NewTotalEarnings - CurrentTotalEarnings}", LogLevel.Warn);
                    Monitor.Log($"Checksum = {checksum}", LogLevel.Warn);
                }

                CurrentTotalEarnings = NewTotalEarnings;
            }
            else
            {
                Monitor.Log("Unaccounted earnings", LogLevel.Warn);
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

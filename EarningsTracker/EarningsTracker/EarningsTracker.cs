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
            helper.Events.GameLoop.ReturnedToTitle += GameLoopReturnedToTitle;
            helper.Events.GameLoop.SaveLoaded += GameLoopSaveLoaded;
            helper.Events.GameLoop.Saving += GameLoopSaving;
            helper.Events.Player.InventoryChanged += PlayerInventoryChanged;
            helper.Events.Player.Warped += PlayerWarped;

            helper.Events.Input.ButtonPressed += InputButtonPressed;

            for (int i = 0; i < CategoryNames.Length; i++)
            {
                ItemsSold.Add(CategoryNames[i], new List<JsonItem>());
            }
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

        private uint TotalTrackedEarnings = 0;
        private int AnimalEarnings = 0;
        private int MailEarnings = 0;
        private int QuestEarnings = 0;
        private int TrashEarnings = 0;
        private int UnknownEarnings = 0;

        private bool HasSaveLoaded = false;
        private bool InShopMenu = false;




        // array must be of length 5
        private readonly string[] CategoryNames = { "Farming", "Foraging", "Fishing", "Mining", "Other" };

        // we use jsonItem because sometimes we want to change the stack/qty value stored in item
        private Dictionary<string, List<JsonItem>> ItemsSold = new Dictionary<string, List<JsonItem>>();

        /******************
        ** Event Handlers
        ******************/

        private void DisplayMenuChanged(object sender, MenuChangedEventArgs e)
        {
            /*Monitor.Log("====================", LogLevel.Warn);
            Monitor.Log("Display Menu Changed", LogLevel.Warn);
            Monitor.Log("====================", LogLevel.Warn);
            Monitor.Log($"Old menu: {e.OldMenu?.GetType()?.ToString() ?? "null"}", LogLevel.Warn);
            Monitor.Log($"New menu: {e.NewMenu?.GetType()?.ToString() ?? "null"}", LogLevel.Warn);*/


            if (!HasSaveLoaded) { return; }
            
            if (e.NewMenu is StardewValley.Menus.ShopMenu)
            {
                InShopMenu = true;
            }
            else if (e.OldMenu is StardewValley.Menus.ShopMenu)
            {
                InShopMenu = false;
            }

            if (Game1.player.totalMoneyEarned <= TotalTrackedEarnings) { return; }
            int newEarnings = Convert.ToInt32(Game1.player.totalMoneyEarned - TotalTrackedEarnings);

            if (e.OldMenu is StardewValley.Menus.GameMenu)
            {
                Monitor.Log($"Earned {Game1.player.totalMoneyEarned - TotalTrackedEarnings}g from trashing", LogLevel.Warn);
                TrashEarnings += newEarnings;
            }
            else if (e.OldMenu is StardewValley.Menus.QuestLog)
            {
                Monitor.Log($"Earned {Game1.player.totalMoneyEarned - TotalTrackedEarnings}g from quest reward", LogLevel.Warn);
                QuestEarnings += newEarnings;
            }
            else if (e.OldMenu is StardewValley.Menus.AnimalQueryMenu)
            {
                Monitor.Log($"Earned {Game1.player.totalMoneyEarned - TotalTrackedEarnings}g from selling animals", LogLevel.Warn);
                AnimalEarnings += newEarnings;
            }
            else if (e.NewMenu is StardewValley.Menus.LetterViewerMenu || e.OldMenu is StardewValley.Menus.LetterViewerMenu)
            {
                Monitor.Log($"Earned {Game1.player.totalMoneyEarned - TotalTrackedEarnings}g from mail", LogLevel.Warn);
                MailEarnings += newEarnings;
            }
            else
            {
                Monitor.Log($"Earned {Game1.player.totalMoneyEarned - TotalTrackedEarnings}g from unaccounted source", LogLevel.Error);
                Monitor.Log($"CTE: {TotalTrackedEarnings}, player: {Game1.player.totalMoneyEarned}", LogLevel.Error);
                UnknownEarnings += newEarnings;
            }

            TotalTrackedEarnings = Game1.player.totalMoneyEarned;
        }

        private void GameLoopDayStarted(object sender, DayStartedEventArgs e)
        {
            TotalTrackedEarnings = Game1.player.totalMoneyEarned;
            Monitor.Log($"{TodayAsString()} Current Earnings: {TotalTrackedEarnings}", LogLevel.Warn);
        }

        private void GameLoopDayEnding(object sender, DayEndingEventArgs e)
        {
            // Game clears shipping bin if done later

            var jsonDay = PackageDayData();
            Helper.Data.WriteJsonFile(ModData.JsonPath(StardewModdingAPI.Constants.SaveFolderName), jsonDay);
        }

        private void GameLoopReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            HasSaveLoaded = false;
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
            }

            HasSaveLoaded = true;
        }

        private void GameLoopSaving(object sender, SavingEventArgs e)
        {
            var data = new ModData(StardewModdingAPI.Constants.SaveFolderName);
            Helper.Data.WriteSaveData(ModData.DataKey, data);
        }

        private void PlayerInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            if (!e.IsLocalPlayer) { return; }

            ParseInventoryChangedEvent(e);
        }

        // dont need but doesn't hurt as a margin of safety?
        private void PlayerWarped(object sender, WarpedEventArgs e)
        {
            TotalTrackedEarnings = Game1.player.totalMoneyEarned;
        }

        /******************
        ** Private Methods
        ******************/
        private JsonCategoryMap ParseShippingBin()
        {
            var items = Game1.getFarm().getShippingBin(Game1.player);
            Utility.consolidateStacks(items);

            var listMap = new Dictionary<string, List<JsonItem>>();
            var categoryMap = new Dictionary<string, JsonItemList>();

            for (int i = 0; i < CategoryNames.Length; i++)
            {
                listMap.Add(CategoryNames[i], new List<JsonItem>());
            }

            foreach (Item item in (IEnumerable<Item>)items)
            {
                JsonItem jsonItem = new JsonItem(FullItemName(item), item.Stack, Utility.getSellToStorePriceOfItem(item));
                listMap[GetCategoryNameForItem(item)].Add(jsonItem);
            }

            for (int i = 0; i < CategoryNames.Length; i++)
            {
                categoryMap.Add(CategoryNames[i], new JsonItemList(listMap[CategoryNames[i]]));
            }

            return new JsonCategoryMap(categoryMap);
        }

        private JsonCategoryMap ParseStore()
        {
            var categoryMap = new Dictionary<string, JsonItemList>();

            for (int i = 0; i < CategoryNames.Length; i++)
            {
                categoryMap.Add(CategoryNames[i], new JsonItemList(ItemsSold[CategoryNames[i]]));
            }

            return new JsonCategoryMap(categoryMap);
        }


        private JsonDay PackageDayData()
        {
            var shippingJson = ParseShippingBin();
            var storeJson = ParseStore();

            return new JsonDay(shippingJson, storeJson, AnimalEarnings, MailEarnings, QuestEarnings, TrashEarnings, UnknownEarnings);
        }

        private string GetCategoryNameForItem(Item item)
        {
            switch ((int)(NetFieldBase<int, NetInt>)item.parentSheetIndex)
            {
                case 296:
                case 396:
                case 402:
                case 406:
                case 410:
                case 414:
                case 418:
                    return CategoryNames[1];
                default:
                    switch (item.Category)
                    {
                        case -81:
                        case -27:
                        case -23:
                            return CategoryNames[1];
                        case -80:
                        case -79:
                        case -75:
                        case -26:
                        case -14:
                        case -6:
                        case -5:
                            return CategoryNames[0];
                        case -20:
                        case -4:
                            return CategoryNames[2];
                        case -15:
                        case -12:
                        case -2:
                            return CategoryNames[3];
                        default:
                            return CategoryNames[4];
                    }
            }
        }

        private string GetCustomCategoryNameForItem(Item item)
        {
            /* user provides a json that will be parsed into a 
             * Dictionary<string (category name), List<string> (names of all the items that belong)>
             * 
             * code will generate a custom trie that stores the category name at each final node for more efficient lookup
             * trie will be generated when we first parse the config.json
             * 
             * this function will just use the trie to return the category name
             */

            return "";
        }

        private void ParseInventoryChangedEvent(InventoryChangedEventArgs e)
        {
            if (!InShopMenu) { return; }
            if (Game1.player.totalMoneyEarned <= TotalTrackedEarnings) { return; }

            uint NewTotalEarnings = Game1.player.totalMoneyEarned;
            int checksum = 0;

            if (e.Removed.Count() > 0)
            {
                foreach (Item item in e.Removed)
                {
                    var salePrice = Utility.getSellToStorePriceOfItem(item);

                    if (item is StardewValley.Object)
                    {
                        var obj = item as StardewValley.Object;

                        Monitor.Log($"\tSold: {item.Stack} {FullItemName(obj)}", LogLevel.Warn);
                        Monitor.Log($"\tSale Price: {salePrice}", LogLevel.Warn);

                        JsonItem jsonItem = new JsonItem(FullItemName(obj), item.Stack, salePrice);
                        ItemsSold[GetCategoryNameForItem(item)].Add(jsonItem);
                    }
                    else
                    {
                        Monitor.Log($"\tSold: {item.Stack} {FullItemName(item)}", LogLevel.Warn);
                        Monitor.Log($"\tSale Price: {salePrice}", LogLevel.Warn);

                        JsonItem jsonItem = new JsonItem(FullItemName(item), item.Stack, salePrice);
                        ItemsSold[GetCategoryNameForItem(item)].Add(jsonItem);
                    }

                    checksum += salePrice;
                }

                if (checksum != NewTotalEarnings - TotalTrackedEarnings)
                {
                    Monitor.Log("Earning failed checksum", LogLevel.Error);
                    Monitor.Log($"Change in earnings = {NewTotalEarnings - TotalTrackedEarnings}", LogLevel.Error);
                    Monitor.Log($"Checksum = {checksum}", LogLevel.Error);
                }

                TotalTrackedEarnings = NewTotalEarnings;
            }
            else if (e.QuantityChanged.Count() > 0)
            {
                foreach (ItemStackSizeChange change in e.QuantityChanged)
                {
                    var obj = change.Item as StardewValley.Object;
                    var sizeChange = change.OldSize - change.NewSize;
                    var salePrice = Utility.getSellToStorePriceOfItem(change.Item, false) * sizeChange;

                    Monitor.Log($"\tSold: {sizeChange} {FullItemName(obj)}", LogLevel.Warn);
                    Monitor.Log($"\tSale Price: {salePrice}", LogLevel.Warn);

                    JsonItem jsonItem = new JsonItem(obj.Name, sizeChange, salePrice);
                    ItemsSold[GetCategoryNameForItem(obj)].Add(jsonItem);

                    checksum += salePrice;
                }

                if (checksum != NewTotalEarnings - TotalTrackedEarnings)
                {
                    Monitor.Log("Earning failed checksum", LogLevel.Error);
                    Monitor.Log($"Change in earnings = {NewTotalEarnings - TotalTrackedEarnings}", LogLevel.Error);
                    Monitor.Log($"Checksum = {checksum}", LogLevel.Error);
                }

                TotalTrackedEarnings = NewTotalEarnings;
            }
            else
            {
                Monitor.Log("Unaccounted earnings from an item removed from inventory", LogLevel.Error);
            }
        }

        /******************
        ** Utility Methods
        ******************/

        private string FullItemName(Item item)
        {
            if (item is StardewValley.Object)
            {
                var obj = item as StardewValley.Object;

                switch (obj.quality.Value)
                {
                    case 0:
                        return obj.DisplayName;
                    case 1:
                        return obj.DisplayName + " (silver)";
                    case 2:
                        return obj.DisplayName + " (gold)";
                    case 3:
                        return obj.DisplayName + " (iridium)"; // test this
                    default:
                        return obj.DisplayName + " (unknown)"; // test this
                }
            }
            else
            {
                return item.DisplayName;
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

using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace EarningsTracker
{
    using Category = String;

    public class DataManager
    {
        /******************
        ** Public Fields
        ******************/
        public uint TotalTrackedEarnings { get; private set; }

        /******************
        ** Private Fields
        ******************/

        // Currently hardcoded during development but will later read from a config.json
        private readonly Category[] CategoryNames = { "Farming", "Foraging", "Fishing", "Mining", "Other" };
        private readonly Dictionary<Category, List<JsonItem>> ItemsSold = new Dictionary<Category, List<JsonItem>>();
       
        private readonly IMonitor Monitor; // for logging

        private int AnimalEarnings = 0;
        private int MailEarnings = 0;
        private int QuestEarnings = 0;
        private int TrashEarnings = 0;
        private int UnknownEarnings = 0;


        /******************
        ** Constructor
        ******************/

        public DataManager(IMonitor monitor) // pass in config.json in constructor in future
        {
            TotalTrackedEarnings = 0;
            Monitor = monitor;

            // setup CategoryNames here in the future

            for (int i = 0; i < CategoryNames.Length; i++)
            {
                ItemsSold.Add(CategoryNames[i], new List<JsonItem>());
            }
        }

        /******************
        ** Public Methods
        ******************/

        public void AddItemSoldEvent(Farmer player, IEnumerable<Item> itemsSold)
        {
            int checksum = 0;

            foreach (Item item in itemsSold)
            {
                var salePrice = Utility.getSellToStorePriceOfItem(item);

                Monitor.Log($"\tSold: {item.Stack} {FullItemName(item)}", LogLevel.Warn);
                Monitor.Log($"\tSale Price: {salePrice}", LogLevel.Warn);

                JsonItem jsonItem = new JsonItem(FullItemName(item), item.Stack, salePrice);
                ItemsSold[GetCategoryNameForItem(item)].Add(jsonItem);

                checksum += salePrice;
            }

            if (checksum != player.totalMoneyEarned - TotalTrackedEarnings)
            {
                Monitor.Log("Item sold failed checksum", LogLevel.Error);
                Monitor.Log($"Change in earnings = {player.totalMoneyEarned - TotalTrackedEarnings}", LogLevel.Error);
                Monitor.Log($"Checksum = {checksum}", LogLevel.Error);
            }

            UpdateTrackedEarnings(player);
        }

        public void AddItemSoldEvent(Farmer player, IEnumerable<ItemStackSizeChange> itemsSold)
        {
            AddItemSoldEvent(player, itemsSold
                .Select(change => { change.Item.Stack = change.OldSize - change.NewSize; return change.Item; }));
        }

        public void AddAnimalEarning(Farmer player)
        {
            Monitor.Log($"Earned {player.totalMoneyEarned - TotalTrackedEarnings}g from selling an animal", LogLevel.Warn);
            AnimalEarnings += Convert.ToInt32(player.totalMoneyEarned - TotalTrackedEarnings);
            UpdateTrackedEarnings(player);
        }
        public void AddMailEarning(Farmer player)
        {
            Monitor.Log($"Earned {player.totalMoneyEarned - TotalTrackedEarnings}g from mail attachment", LogLevel.Warn);
            MailEarnings += Convert.ToInt32(player.totalMoneyEarned - TotalTrackedEarnings);
            UpdateTrackedEarnings(player);
        }
        public void AddQuestEarning(Farmer player)
        {
            Monitor.Log($"Earned {player.totalMoneyEarned - TotalTrackedEarnings}g from quest reward", LogLevel.Warn);
            QuestEarnings += Convert.ToInt32(player.totalMoneyEarned - TotalTrackedEarnings);
            UpdateTrackedEarnings(player);
        }
        public void AddTrashEarning(Farmer player)
        {
            Monitor.Log($"Earned {player.totalMoneyEarned - TotalTrackedEarnings}g from reclaiming trash", LogLevel.Warn);
            TrashEarnings += Convert.ToInt32(player.totalMoneyEarned - TotalTrackedEarnings);
            UpdateTrackedEarnings(player);
        }
        public void AddUnknownEarning(Farmer player)
        {
            Monitor.Log($"Earned {player.totalMoneyEarned - TotalTrackedEarnings}g from unknown source", LogLevel.Error);
            UnknownEarnings += Convert.ToInt32(player.totalMoneyEarned - TotalTrackedEarnings);
            UpdateTrackedEarnings(player);
        }
        public void UpdateTrackedEarnings(Farmer player) 
        {
            TotalTrackedEarnings = player.totalMoneyEarned;
        }

        public JsonDay PackageDayData(Farmer player)
        {
            return new JsonDay(new JsonCategoryMap(ParseShippingBin(player)), 
                               new JsonCategoryMap(ItemsSold), 
                               AnimalEarnings, MailEarnings, QuestEarnings, 
                               TrashEarnings, UnknownEarnings);
        }

        /******************
        ** Private Methods
        ******************/

        private Dictionary<Category, List<JsonItem>> ParseShippingBin(Farmer player)
        {
            var items = Game1.getFarm().getShippingBin(player);
            Utility.consolidateStacks(items);

            var listMap = new Dictionary<Category, List<JsonItem>>();

            for (int i = 0; i < CategoryNames.Length; i++)
            {
                listMap.Add(CategoryNames[i], new List<JsonItem>());
            }

            foreach (Item item in (IEnumerable<Item>)items)
            {
                JsonItem jsonItem = new JsonItem(FullItemName(item), item.Stack, Utility.getSellToStorePriceOfItem(item));
                listMap[GetCategoryNameForItem(item)].Add(jsonItem);
            }

            return listMap;
        }

        // swap this out to use config file in the future
        private Category GetCategoryNameForItem(Item item)
        {
            switch (item.parentSheetIndex.Value)
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

        private Category GetCustomCategoryNameForItem(Item item)
        {
            /* user provides a json that will be parsed into a 
             * Dictionary<Category, List<item.name> (names of all the items that belong)>
             * 
             * code will generate a custom trie that stores the category name at each final node for more efficient lookup
             * trie will be generated when we first parse the config.json
             * 
             * this function will just use the trie to return the category name
             */

            return "";
        }

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
                    case 4:
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
    }
}

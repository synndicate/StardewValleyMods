using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace EarningsTracker
{
    using Category = String;
    using UserID = String;

    public class DataManager
    {
        /******************
        ** Public Fields
        ******************/
        public uint TotalTrackedEarnings { get; private set; }

        /******************
        ** Private Fields
        ******************/

        private readonly IMonitor Monitor; // for logging

        private readonly List<Category> Categories;
        private readonly Dictionary<int, Category> SdvIdMap;
        private readonly Dictionary<int, Category> SdvCategoryMap;

        private readonly List<Item> ItemsSold;

        private int AnimalEarnings = 0;
        private int MailEarnings = 0;
        private int QuestEarnings = 0;
        private int TrashEarnings = 0;
        private int UnknownEarnings = 0;


        /******************
        ** Constructor
        ******************/

        public DataManager(Farmer mainPlayer, ModConfig config, IMonitor monitor) // pass in config.json in constructor in future
        {
            TotalTrackedEarnings = 0;
            Monitor = monitor;

            Categories = new List<Category>();
            SdvIdMap = new Dictionary<int, Category>();
            SdvCategoryMap = new Dictionary<int, Category>();

            foreach (KeyValuePair<Category, Dictionary<string, List<int>>> definition in config.CustomCategories)
            {
                var categoryName = definition.Key;
                var itemIDs = definition.Value["itemIDs"];
                var objectCategories = definition.Value["objectCategories"];

                Categories.Add(categoryName);

                foreach (int id in itemIDs)
                {
                    if (!SdvIdMap.ContainsKey(id))
                    {
                        SdvIdMap.Add(id, categoryName);

                    }
                }

                foreach (int oc in objectCategories)
                {
                    if (!SdvCategoryMap.ContainsKey(oc))
                    {
                        SdvCategoryMap.Add(oc, categoryName);
                    }
                }
            }

            Categories.Add("Other");
            ItemsSold = new List<Item>();
        }

        /******************
        ** Public Methods
        ******************/

        public void AddItemSoldEvent(Farmer player, IEnumerable<Item> items)
        {
            ItemsSold.AddRange(items);
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
            var shippingBin = Game1.getFarm().getShippingBin(player);

            Utility.consolidateStacks(shippingBin);
            Utility.consolidateStacks(ItemsSold);

            return new JsonDay(new JsonCategoryMap(CategorizeItems(shippingBin)), 
                               new JsonCategoryMap(CategorizeItems(ItemsSold)), 
                               AnimalEarnings, MailEarnings, QuestEarnings, 
                               TrashEarnings, UnknownEarnings);
        }

        /******************
        ** Private Methods
        ******************/

        private Dictionary<Category, List<JsonItem>> CategorizeItems(IEnumerable<Item> items)
        {
            var map = new Dictionary<Category, List<JsonItem>>();

            for (int i = 0; i < Categories.Count; i++)
            {
                map.Add(Categories[i], new List<JsonItem>());
            }

            foreach (Item item in items)
            {
                JsonItem jsonItem = new JsonItem(FullItemName(item), item.Stack, Utility.getSellToStorePriceOfItem(item));
                map[GetCategoryForItem(item)].Add(jsonItem);
            }

            return map;
        }

        private Category GetCategoryForItem(Item item)
        {
            if (SdvIdMap.ContainsKey(item.ParentSheetIndex))
            {
                return SdvIdMap[item.ParentSheetIndex];
            }
            else if (SdvCategoryMap.ContainsKey(item.Category))
            {
                return SdvCategoryMap[item.Category];
            }
            else
            {
                return "Other";
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

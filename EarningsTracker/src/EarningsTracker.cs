using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace EarningsTracker
{
    using SItem = StardewValley.Item;

    public class EarningsTracker
    {
        /******************
        ** Public Fields
        ******************/
        public uint TotalTrackedEarnings { get; private set; }

        /******************
        ** Private Fields
        ******************/

        private readonly IMonitor Monitor; // for logging

        private readonly List<string> Categories;
        private readonly Dictionary<int, string> SIdMap;
        private readonly Dictionary<int, string> SCategoryMap;

        private readonly List<SItem> ItemsSold;

        private int AnimalEarnings = 0;
        private int MailEarnings = 0;
        private int QuestEarnings = 0;
        private int TrashEarnings = 0;
        private int UnknownEarnings = 0;


        /******************
        ** Constructor
        ******************/

        public EarningsTracker(Farmer mainPlayer, ModConfig config, IMonitor monitor)
        {
            Monitor = monitor;

            var categoryDefines = config.UseCustomCategories ? config.CustomCategories : config.VanillaCategories;

            Categories = categoryDefines.Keys.ToList();

            SIdMap = categoryDefines
                .Select(d => new KeyValuePair<string, List<int>>(d.Key, d.Value?["itemIDs"] ?? new List<int>()))
                .SelectMany(p => p.Value.Select(i => new Tuple<int, string>(i, p.Key)))
                .ToDictionary(t => t.Item1, t => t.Item2);

            SCategoryMap = categoryDefines
                .Select(d => new KeyValuePair<string, List<int>>(d.Key, d.Value?["objectCategories"] ?? new List<int>()))
                .SelectMany(f => f.Value.Select(s => new Tuple<int, string>(s, f.Key)))
                .ToDictionary(t => t.Item1, t => t.Item2);

            TotalTrackedEarnings = 0;
            ItemsSold = new List<SItem>();
        }

        /******************
        ** Public Methods
        ******************/

        public void AddItemSoldEvent(Farmer player, IEnumerable<SItem> items)
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

            return new JsonDay(new Day(TodayAsString(), TodayAsIndex(), CategorizeItems(shippingBin), CategorizeItems(ItemsSold), 
                                        AnimalEarnings, MailEarnings, QuestEarnings, TrashEarnings, UnknownEarnings));
        }

        /******************
        ** Private Methods
        ******************/

        private Dictionary<string, IEnumerable<Item>> CategorizeItems(IEnumerable<SItem> items)
        {
            return items
                .Select(i => new KeyValuePair<string, Item>(GetCategoryForItem(i), new Item(FullItemName(i), i.Stack, Utility.getSellToStorePriceOfItem(i))))
                .GroupBy(p => p.Key, p => p.Value)
                .ToDictionary(g => g.Key, g => (IEnumerable<Item>)g);
        }

        private string GetCategoryForItem(SItem item)
        {
            if (SIdMap.ContainsKey(item.ParentSheetIndex))
            {
                return SIdMap[item.ParentSheetIndex];
            }
            else if (SCategoryMap.ContainsKey(item.Category))
            {
                return SCategoryMap[item.Category];
            }
            else
            {
                return "Other";
            }
        }

        private string GetCustomCategoryNameForItem(SItem item)
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

        private string FullItemName(SItem item)
        {
            if (item is StardewValley.Object)
            {
                var obj = item as StardewValley.Object;

                switch (obj.quality.Value)
                {
                    case 0:
                        return obj.DisplayName;
                    case 1:
                        return obj.DisplayName + " (Silver)";
                    case 2:
                        return obj.DisplayName + " (Gold)";
                    case 4:
                        return obj.DisplayName + " (Iridium)";
                    default:
                        return obj.DisplayName + " (Unknown)";
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
            var today = SDate.Now();
            return $"Y{today.Year} {today.Season.First().ToString().ToUpper() + today.Season.Substring(1)} - Day {today.Day} ({today.DayOfWeek})";
        }

        private int TodayAsIndex()
        {
            var today = SDate.Now();
            var seasonIndex = new Dictionary<string, int> 
            {
                { "spring", 0 },
                { "summer", 1 },
                { "fall"  , 2 },
                { "winter", 3 }
            };

            return (today.Year - 1) * (28 * 4) + seasonIndex[today.Season] * 28 + today.Day;
        }
    }
}

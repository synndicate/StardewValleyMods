using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly ModConfig Config;
        private readonly Farmer Player;

        private readonly List<SItem> ItemsSold;

        private int AnimalEarnings = 0;
        private int MailEarnings = 0;
        private int QuestEarnings = 0;
        private int TrashEarnings = 0;
        private int UnknownEarnings = 0;


        /******************
        ** Constructor
        ******************/

        public EarningsTracker(Farmer player, ModConfig config)
        {
            Config = config;
            Player = player;

            TotalTrackedEarnings = 0;
            ItemsSold = new List<SItem>();
        }

        /******************
        ** Public Methods
        ******************/

        public void AddItemSoldEvent(IEnumerable<SItem> items)
        {
            ItemsSold.AddRange(items);
            UpdateEarnings();
        }

        public void AddAnimalEarning()
        {
            AnimalEarnings += Convert.ToInt32(Player.totalMoneyEarned - TotalTrackedEarnings);
            UpdateEarnings();
        }
        public void AddMailEarning()
        {
            MailEarnings += Convert.ToInt32(Player.totalMoneyEarned - TotalTrackedEarnings);
            UpdateEarnings();
        }
        public void AddQuestEarning()
        {
            QuestEarnings += Convert.ToInt32(Player.totalMoneyEarned - TotalTrackedEarnings);
            UpdateEarnings();
        }
        public void AddTrashEarning()
        {
            TrashEarnings += Convert.ToInt32(Player.totalMoneyEarned - TotalTrackedEarnings);
            UpdateEarnings();
        }
        public void AddUnknownEarning()
        {
            UnknownEarnings += Convert.ToInt32(Player.totalMoneyEarned - TotalTrackedEarnings);
            UpdateEarnings();
        }
        public void UpdateEarnings() 
        {
            TotalTrackedEarnings = Player.totalMoneyEarned;
        }

        public Day PackageDayData()
        {
            var shippingBin = Game1.getFarm().getShippingBin(Player);

            Utility.consolidateStacks(shippingBin);
            Utility.consolidateStacks(ItemsSold);

            return new Day(SDate.Now(), CategorizeItems(shippingBin), CategorizeItems(ItemsSold),
                AnimalEarnings, MailEarnings, QuestEarnings, TrashEarnings, UnknownEarnings);
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
            var sIDMap = Config.ItemIDMap();
            var sCategoryMap = Config.ObjectCategoryMap();

            if (sIDMap.ContainsKey(item.ParentSheetIndex))
            {
                return sIDMap[item.ParentSheetIndex];
            }
            else if (sCategoryMap.ContainsKey(item.Category))
            {
                return sCategoryMap[item.Category];
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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace EarningsTracker
{
    using Category = String;

    public sealed class ModData
    {
        public const string DataKey = "ModData";
        public readonly string SaveName;

        private ModData() { }

        public ModData(string saveName)
        {
            SaveName = saveName;
        }

        public static string JsonPath(string saveFolderName)
        {
            return "data/EarningsData+" + saveFolderName + ".json";
        }
    }

    public sealed class JsonItem
    {
        public readonly string Name;
        public readonly int Qty;
        public readonly int Value;

        private JsonItem() { }

        public JsonItem(string name, int qty, int value) 
        {
            Name = name;
            Qty = qty;
            Value = value;
        }
    }

    public sealed class JsonItemList // list + total
    {
        public readonly int Total;
        public readonly List<JsonItem> Items;

        private JsonItemList() { }

        public JsonItemList(List<JsonItem> items)
        {
            Items = items;
            Total = items.Aggregate(0, (acc, x) => acc + x.Value);
        }
    }

    public sealed class JsonCategoryMap // maps categories 
    {
        public readonly int Total;
        public readonly Dictionary<Category, JsonItemList> Categories;

        private JsonCategoryMap() { }

        public JsonCategoryMap(Dictionary<Category, JsonItemList> categories)
        {
            Categories = categories;
            Total = Categories.Values.Aggregate(0, (acc, x) => acc + x.Total);
        }

        public JsonCategoryMap(Dictionary<Category, List<JsonItem>> categories)
        {
            Categories = categories
                .Select(i => new KeyValuePair<Category, JsonItemList>(i.Key, new JsonItemList(i.Value)))
                .ToDictionary(i => i.Key, i => i.Value);
            Total = Categories.Values.Aggregate(0, (acc, x) => acc + x.Total);
        }
    }

    public sealed class JsonDay
    {
        public readonly int Total;
        public readonly JsonCategoryMap;
        public readonly JsonCategoryMap;
        public readonly int Animals;
        public readonly int Mail;
        public readonly int Quest;
        public readonly int Trash;
        public readonly int Unknown;

        private JsonDay() { }

        public JsonDay(JsonCategoryMap shipped, JsonCategoryMap store, int animals, int mail, int quest, int trash, int unknown)
        {
            Shipped = shipped;
            Store = store;
            Animals = animals;
            Mail = mail;
            Quest = quest;
            Trash = trash;
            Unknown = unknown;

            Total = shipped.Total + store.Total + animals + mail + quest + trash + unknown;
        }
    }
}

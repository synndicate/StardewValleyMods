using System;
using System.Collections.Generic;
using System.Linq;

namespace EarningsTracker
{
    public sealed class ModData
    {
        public static string DataKey = "ModData";

        public static string JsonPath(string saveFolderName)
        {
            return "data/EarningsData+" + saveFolderName + ".json";
        }


        public string SaveName { get; set; }

        private ModData() { }

        public ModData(string saveName)
        {
            SaveName = saveName;
        }
    }

    public sealed class JsonItem
    {
        public string Name { get; set; }
        public int Qty { get; set; }
        public int Value { get; set; }

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
        public int Total { get; set; }
        public List<JsonItem> Items { get; set; }

        private JsonItemList() { }

        public JsonItemList(List<JsonItem> items)
        {
            Items = items;
            Total = items.Aggregate(0, (acc, x) => acc + x.Value);
        }
    }

    public sealed class JsonCategoryMap // dictionary + total
    {
        public int Total { get; set; }

        public Dictionary<string, JsonItemList> Categories { get; set; }

        private JsonCategoryMap() { }

        public JsonCategoryMap(Dictionary<string, JsonItemList> categories)
        {
            Categories = categories;
            Total = categories.Values.Aggregate(0, (acc, x) => acc + x.Total);
        }
    }

    // Note: The keys for shipped and store should always match
    public sealed class JsonDay
    {
        public int Total { get; set; }
        public JsonCategoryMap Shipped { get; set; }
        public JsonCategoryMap Store { get; set; }
        public int Animals { get; set; }
        public int Mail { get; set; }
        public int Quest { get; set; }
        public int Trash { get; set; }
        public int Unknown { get; set; }

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

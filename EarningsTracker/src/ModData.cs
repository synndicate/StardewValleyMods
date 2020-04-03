using System;
using System.Collections.Generic;
using System.Linq;

namespace EarningsTracker
{
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

    public sealed class JsonCategorizedItems
    {
        public readonly int Total;
        public readonly Dictionary<string, IEnumerable<Item>> ItemsByCategory;

        private JsonCategorizedItems() { }

        public JsonCategorizedItems(Dictionary<string, IEnumerable<Item>> itemsByCategory)
        {
            ItemsByCategory = itemsByCategory;
            Total = ItemsByCategory.Values
                .SelectMany(x => x)
                .Aggregate(0, (acc, x) => acc + x.Value);
        }

        public static JsonCategorizedItems operator +(JsonCategorizedItems l, JsonCategorizedItems r)
            => new JsonCategorizedItems(l.ItemsByCategory
                .Concat(r.ItemsByCategory)
                .GroupBy(p => p.Key, p => p.Value)
                .ToDictionary(k => k.Key, v => v.Aggregate((acc, x) => acc.Concat(x))));
    }

    public sealed class JsonDay
    {
        public readonly string Date;
        public readonly int Total;
        public readonly JsonCategorizedItems Shipped;
        public readonly JsonCategorizedItems Store;
        public readonly int Animals;
        public readonly int Mail;
        public readonly int Quests;
        public readonly int Trash;
        public readonly int Unknown;

        private JsonDay() { }

        public JsonDay(Day day)
        {
            Date = day.Date;
            Shipped = new JsonCategorizedItems(day.Shipped);
            Store = new JsonCategorizedItems(day.Store);
            Animals = day.Animals;
            Mail = day.Mail;
            Quests = day.Quests;
            Trash = day.Trash;
            Unknown = day.Unknown;

            var shippedTotal = day.Shipped.Values
                .SelectMany(x => x)
                .Aggregate(0, (acc, x) => acc + x.Value);

            var storeTotal = day.Store.Values
                .SelectMany(x => x)
                .Aggregate(0, (acc, x) => acc + x.Value);

            Total = shippedTotal + storeTotal + Animals + Mail + Quests + Trash + Unknown;
        }
    }

    public sealed class JsonWeek
    {
        public readonly int Total;
        public readonly JsonCategorizedItems Shipped;
        public readonly JsonCategorizedItems Store;
        public readonly int Animals;
        public readonly int Mail;
        public readonly int Quests;
        public readonly int Trash;
        public readonly int Unknown;
        public readonly Dictionary<int, JsonDay> Days;

        public JsonWeek() { } // change to private after testing

        public JsonWeek(Dictionary<int, JsonDay> days)
        {
            var dayValues = days.Select(d => d.Value);

            Shipped = dayValues
                .Select(d => d.Shipped)
                .Aggregate((acc, x) => acc + x);

            Store = dayValues
                .Select(d => d.Store)
                .Aggregate((acc, x) => acc + x);

            Animals = dayValues.Aggregate(0, (acc, d) => acc + d.Animals);
            Mail = dayValues.Aggregate(0, (acc, d) => acc + d.Mail);
            Quests = dayValues.Aggregate(0, (acc, d) => acc + d.Quests);
            Trash = dayValues.Aggregate(0, (acc, d) => acc + d.Trash);
            Unknown = dayValues.Aggregate(0, (acc, d) => acc + d.Unknown);

            Total = Shipped.Total + Store.Total + Animals + Mail + Quests + Trash + Unknown;
        }
    }
}

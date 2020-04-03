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
        public readonly int Stack;
        public readonly int Value;

        private JsonItem() { }

        public JsonItem(string name, int stack, int value) 
        {
            Name = name;
            Stack = stack;
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
            Total = Items.Aggregate(0, (acc, x) => acc + x.Value);
        }
    }

    public sealed class JsonCategoryMap // maps categories 
    {
        public readonly int Total;
        public readonly Dictionary<Category, JsonItemList> Categories;

        public JsonCategoryMap() { }

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
        public readonly JsonCategoryMap Shipped;
        public readonly JsonCategoryMap Store;
        public readonly int Animals;
        public readonly int Mail;
        public readonly int Quests;
        public readonly int Trash;
        public readonly int Unknown;

        private JsonDay() { }

        public JsonDay(JsonCategoryMap shipped, JsonCategoryMap store, int animals, int mail, int quests, int trash, int unknown)
        {
            Shipped = shipped;
            Store = store;
            Animals = animals;
            Mail = mail;
            Quests = quests;
            Trash = trash;
            Unknown = unknown;

            Total = Shipped.Total + Store.Total + Animals + Mail + Quests + Trash + Unknown;
        }
    }

    public sealed class JsonWeek
    {
        public readonly int Total;
        public readonly JsonCategoryMap Shipped;
        public readonly JsonCategoryMap Store;
        public readonly int Animals;
        public readonly int Mail;
        public readonly int Quests;
        public readonly int Trash;
        public readonly int Unknown;
        public readonly List<JsonDay> Days;

        public JsonWeek() { } // change to private 

        public JsonWeek(List<JsonDay> days)
        {
            var totalShipped = Days
                .Select(d => d.Shipped.Categories)
                .Aggregate((acc, d) => 
                {
                    foreach (Category c in d.Keys)
                    {
                        if (acc.ContainsKey(c))
                        {
                            acc[c].Items.AddRange(d[c].Items);
                        }
                        else
                        {
                            acc.Add(c, d[c]);
                        }
                    }

                    return acc;
                });
            Shipped = new JsonCategoryMap(totalShipped);

            var totalStore = Days
                .Select(d => d.Shipped.Categories)
                .Aggregate((acc, d) =>
                {
                    foreach (Category c in d.Keys)
                    {
                        if (acc.ContainsKey(c))
                        {
                            acc[c].Items.AddRange(d[c].Items);
                        }
                        else
                        {
                            acc.Add(c, d[c]);
                        }
                    }

                    return acc;
                });
            Store = new JsonCategoryMap(totalStore);

            Animals = days.Aggregate(0, (acc, d) => acc + d.Animals);
            Mail = days.Aggregate(0, (acc, d) => acc + d.Mail);
            Quests = days.Aggregate(0, (acc, d) => acc + d.Quests);
            Trash = days.Aggregate(0, (acc, d) => acc + d.Trash);
            Unknown = days.Aggregate(0, (acc, d) => acc + d.Unknown);

            Total = Shipped.Total + Store.Total + Animals + Mail + Quests + Trash + Unknown;
        }
    }
}

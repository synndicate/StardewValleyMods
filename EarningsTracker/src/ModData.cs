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

    public sealed class JsonCategory
    {
        public readonly int Total;
        public readonly IEnumerable<Item> Items;

        private JsonCategory() { }

        public JsonCategory(IEnumerable<Item> items)
        {
            Items = items;
            Total = items.Aggregate(0, (acc, x) => acc + x.Value);
        }

        public static JsonCategory operator +(JsonCategory l, JsonCategory r)
            => new JsonCategory(l.Items.Concat(r.Items));
    }


    public sealed class JsonCategoryList
    {
        public readonly int Total;
        public readonly Dictionary<string, JsonCategory> Categories;

        private JsonCategoryList() { }

        public JsonCategoryList(Dictionary<string, IEnumerable<Item>> categories)
        {
            Categories = categories
                .ToDictionary(p => p.Key, p => new JsonCategory(p.Value));
            Total = categories.Values
                .SelectMany(x => x)
                .Aggregate(0, (acc, x) => acc + x.Value);
        }

        public static JsonCategoryList operator +(JsonCategoryList l, JsonCategoryList r)
            => new JsonCategoryList(l.Categories
                .Concat(r.Categories)
                .GroupBy(p => p.Key, p => p.Value)
                .ToDictionary(g => g.Key, g => g.Aggregate((acc, x) => acc + x).Items));

    }

    public sealed class JsonDay
    {
        public readonly int Total;
        public readonly JsonCategoryList Shipped;
        public readonly JsonCategoryList Store;
        public readonly int Animals;
        public readonly int Mail;
        public readonly int Quests;
        public readonly int Trash;
        public readonly int Unknown;

        private readonly string _Label;

        private JsonDay() { }

        public JsonDay(string label, Day day)
        {
            Shipped = new JsonCategoryList(day.Shipped);
            Store = new JsonCategoryList(day.Store);
            Animals = day.Animals;
            Mail = day.Mail;
            Quests = day.Quests;
            Trash = day.Trash;
            Unknown = day.Unknown;

            Total = Shipped.Total + Store.Total + Animals + Mail + Quests + Trash + Unknown;
            _Label = label;
        }

        public string Label()
        {
            return _Label;
        }
    }

    public sealed class JsonWeek
    {
        public readonly int Total;
        public readonly JsonCategoryList Shipped;
        public readonly JsonCategoryList Store;
        public readonly int Animals;
        public readonly int Mail;
        public readonly int Quests;
        public readonly int Trash;
        public readonly int Unknown;
        public readonly Dictionary<string, JsonDay> Days;

        private readonly string _Label;

        private JsonWeek() { }

        public JsonWeek(string label, List<JsonDay> days)
        {
            Shipped = days
                .Select(d => d.Shipped)
                .Aggregate((acc, d) => acc + d);

            Store = days
                .Select(d => d.Store)
                .Aggregate((acc, d) => acc + d);

            Animals = days.Aggregate(0, (acc, d) => acc + d.Animals);
            Mail    = days.Aggregate(0, (acc, d) => acc + d.Mail);
            Quests  = days.Aggregate(0, (acc, d) => acc + d.Quests);
            Trash   = days.Aggregate(0, (acc, d) => acc + d.Trash);
            Unknown = days.Aggregate(0, (acc, d) => acc + d.Unknown);

            Total = Shipped.Total + Store.Total + Animals + Mail + Quests + Trash + Unknown;
            Days = days.ToDictionary(d => d.Label(), d => d);
            _Label = label;
        }

        public string Label()
        {
            return _Label;
        }
    }

    public sealed class JsonSeason
    {
        public readonly int Total;
        public readonly JsonCategoryList Shipped;
        public readonly JsonCategoryList Store;
        public readonly int Animals;
        public readonly int Mail;
        public readonly int Quests;
        public readonly int Trash;
        public readonly int Unknown;
        public readonly Dictionary<string, JsonWeek> Weeks;

        private readonly string _Label;

        private JsonSeason() { }

        public JsonSeason(string label, List<JsonWeek> weeks)
        {
            Shipped = weeks
                .Select(w => w.Shipped)
                .Aggregate((acc, w) => acc + w);

            Store = weeks
                .Select(w => w.Store)
                .Aggregate((acc, w) => acc + w);

            Animals = weeks.Aggregate(0, (acc, w) => acc + w.Animals);
            Mail    = weeks.Aggregate(0, (acc, w) => acc + w.Mail);
            Quests  = weeks.Aggregate(0, (acc, w) => acc + w.Quests);
            Trash   = weeks.Aggregate(0, (acc, w) => acc + w.Trash);
            Unknown = weeks.Aggregate(0, (acc, w) => acc + w.Unknown);

            Total = Shipped.Total + Store.Total + Animals + Mail + Quests + Trash + Unknown;
            Weeks = weeks.ToDictionary(w => w.Label(), w => w);
            _Label = label;
        }

        public string Label()
        {
            return _Label;
        }
    }

    public sealed class JsonYear
    {
        public readonly int Total;
        public readonly JsonCategoryList Shipped;
        public readonly JsonCategoryList Store;
        public readonly int Animals;
        public readonly int Mail;
        public readonly int Quests;
        public readonly int Trash;
        public readonly int Unknown;
        public readonly Dictionary<string, JsonSeason> Seasons;

        private readonly string _Label;

        private JsonYear() { }

        public JsonYear(string label, List<JsonSeason> seasons)
        {
            Shipped = seasons
                .Select(s => s.Shipped)
                .Aggregate((acc, x) => acc + x);

            Store = seasons
                .Select(s => s.Store)
                .Aggregate((acc, x) => acc + x);

            Animals = seasons.Aggregate(0, (acc, s) => acc + s.Animals);
            Mail    = seasons.Aggregate(0, (acc, s) => acc + s.Mail);
            Quests  = seasons.Aggregate(0, (acc, s) => acc + s.Quests);
            Trash   = seasons.Aggregate(0, (acc, s) => acc + s.Trash);
            Unknown = seasons.Aggregate(0, (acc, s) => acc + s.Unknown);

            Total = Shipped.Total + Store.Total + Animals + Mail + Quests + Trash + Unknown;
            Seasons = seasons.ToDictionary(s => s.Label(), s => s);
            _Label = label;
        }

        public string Label()
        {
            return _Label;
        }
    }

    public sealed class JsonTotal
    {
        public readonly int Total;
        public readonly JsonCategoryList Shipped;
        public readonly JsonCategoryList Store;
        public readonly int Animals;
        public readonly int Mail;
        public readonly int Quests;
        public readonly int Trash;
        public readonly int Unknown;
        public readonly Dictionary<string, JsonYear> Years;

        private JsonTotal() { }

        public JsonTotal(List<JsonYear> years)
        {
            Shipped = years
                .Select(y => y.Shipped)
                .Aggregate((acc, y) => acc + y);

            Store = years
                .Select(y => y.Store)
                .Aggregate((acc, y) => acc + y);

            Animals = years.Aggregate(0, (acc, y) => acc + y.Animals);
            Mail    = years.Aggregate(0, (acc, y) => acc + y.Mail);
            Quests  = years.Aggregate(0, (acc, y) => acc + y.Quests);
            Trash   = years.Aggregate(0, (acc, y) => acc + y.Trash);
            Unknown = years.Aggregate(0, (acc, y) => acc + y.Unknown);

            Total = Shipped.Total + Store.Total + Animals + Mail + Quests + Trash + Unknown;
            Years = years.ToDictionary(y => y.Label(), y => y);
        }
    }

}

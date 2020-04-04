using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI.Utilities;

namespace EarningsTracker
{
    public struct Date
    {
        public readonly string Year;
        public readonly string Season;
        public readonly string Week;
        public readonly string Day;

        public Date(SDate date)
        {
            Year = $"Year {date.Year}";
            Season = $"{date.Season.First().ToString().ToUpper() + date.Season.Substring(1)}";
            Week = $"Week {date.Day / 7 + 1}";
            Day = $"Day {date.Day} ({date.DayOfWeek})";
        }
    }


    public sealed class Item
    {
        public readonly string Name;
        public readonly int Stack;
        public readonly int Value;

        private Item() { }

        public Item(string name, int stack, int value)
        {
            Name = name;
            Stack = stack;
            Value = value;
        }
    }

    public sealed class Day
    {
        public readonly Date Date;
        public readonly int Animals;
        public readonly int Mail;
        public readonly int Quests;
        public readonly int Trash;
        public readonly int Unknown;
        public readonly Dictionary<string, IEnumerable<Item>> Shipped;
        public readonly Dictionary<string, IEnumerable<Item>> Store;

        private Day() { }

        public Day(SDate date, Dictionary<string, IEnumerable<Item>> shipped, Dictionary<string, IEnumerable<Item>> store, int animals, int mail, int quests, int trash, int unknown)
        {
            Date = new Date(date);
            Shipped = shipped;
            Store = store;
            Animals = animals;
            Mail = mail;
            Quests = quests;
            Trash = trash;
            Unknown = unknown;
        }
    }

}

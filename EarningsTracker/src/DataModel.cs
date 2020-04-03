using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarningsTracker
{
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
        public readonly string Date;
        public readonly int Index;
        public readonly int Animals;
        public readonly int Mail;
        public readonly int Quests;
        public readonly int Trash;
        public readonly int Unknown;
        public readonly Dictionary<string, IEnumerable<Item>> Shipped;
        public readonly Dictionary<string, IEnumerable<Item>> Store;

        private Day() { }

        public Day(string date, int index, Dictionary<string, IEnumerable<Item>> shipped, Dictionary<string, IEnumerable<Item>> store, int animals, int mail, int quests, int trash, int unknown)
        {
            Date = date;
            Index = index;
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

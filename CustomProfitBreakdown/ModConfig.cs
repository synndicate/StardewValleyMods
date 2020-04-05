using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace Synndicate.Stardew.CustomProfitBreakdown
{
    public class ModConfig
    {
        public SButton BinTotalKey  { get; set; }
        public JsonSection Section1 { get; set; }
        public JsonSection Section2 { get; set; }
        public JsonSection Section3 { get; set; }
        public JsonSection Section4 { get; set; }
        public JsonSection Other { get; set; }

        public ModConfig()
        {
            BinTotalKey = SButton.B;

            Section1 = new JsonSection("Farming",
                new List<int>(), 
                new List<int> { -80, -79, -75, -26, -14, -6, -5 });
            Section2 = new JsonSection("Foraging",
                new List<int> { 296, 396, 402, 406, 410, 414, 418 },
                new List<int> { -81, -27, -23 });
            Section3 = new JsonSection("Fishing",
                new List<int>(),
                new List<int> { -20, -4 });
            Section4 = new JsonSection("Mining",
                new List<int>(),
                new List<int> { -15, -12, -2 });
            Other = new JsonSection("Other",
                new List<int>(),
                new List<int>());
        }

        public void Validate(IMonitor monitor = null)
        {
            var sections = new List<JsonSection> { Section1, Section2, Section3, Section4, Other };

            var itemDuplicates = sections
                .SelectMany(s => s.Items)
                .GroupBy(i => i)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            if (itemDuplicates.Count() > 0)
            {
                itemDuplicates.ToList()
                    .ForEach(i => monitor?.Log($"config.json: Item ({i}) is listed in more than one section", LogLevel.Error));
                throw new InvalidOperationException("Failed to load config.json");
            }

            var categoryDuplicates = sections
                .SelectMany(s => s.Categories)
                .GroupBy(i => i)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            if (categoryDuplicates.Count() > 0)
            {
                categoryDuplicates.ToList()
                    .ForEach(i => monitor?.Log($"config.json: Category ({i}) is listed in more than one section", LogLevel.Error));
                throw new InvalidOperationException("Failed to load config.json");
            }
        }
    }

    public class JsonSection
    {
        public string Name;
        public List<int> Items;
        public List<int> Categories;

        private JsonSection() { }

        public JsonSection(string name, List<int> items, List<int> categories)
        {
            Name = name;
            Items = items;
            Categories = categories;
        }
    }
}

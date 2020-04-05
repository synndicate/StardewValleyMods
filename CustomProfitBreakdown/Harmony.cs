using System;
using System.Collections.Generic;
using System.Linq;

namespace Synndicate.Stardew.CustomProfitBreakdown
{
    public class Harmony
    {
        private static Dictionary<int, int> ItemMap;
        private static Dictionary<int, int> CategoryMap;

        public static void Initialize(ModConfig config)
        {
            var sections = new List<JsonSection> { config.Section1, config.Section2, config.Section3, config.Section4, config.Other };

            ItemMap = sections
                .SelectMany((s, idx) => s.Items.Select(item => new { item, idx }))
                .ToDictionary(a => a.item, a => a.idx);

            CategoryMap = sections
                .SelectMany((s, idx) => s.Categories.Select(category => new { category, idx }))
                .ToDictionary(a => a.category, a => a.idx);
        }

        public static void GetCategoryIndex_Postfix(StardewValley.Object o, ref int __result)
        {
            if (Harmony.ItemMap.ContainsKey(o.ParentSheetIndex))
            {
                __result = Harmony.ItemMap[o.ParentSheetIndex];
            }
            else if (Harmony.CategoryMap.ContainsKey(o.Category))
            {
                __result = Harmony.CategoryMap[o.Category];
            }
            else
            {
                __result = 4;
            }
        }
    }
}

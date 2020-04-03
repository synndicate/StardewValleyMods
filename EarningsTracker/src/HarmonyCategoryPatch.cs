using System;
using System.Collections.Generic;
using System.Linq;

namespace EarningsTracker
{
    public class HarmonyCategoryPatch
    {
        private static Dictionary<int, int> IdMap;
        private static Dictionary<int, int> CategoryMap;

        public static void Initialize(ModConfig config)
        {
            var vanillaCategoryIndexes = new Dictionary<string, int>
            {
                { "Farming" , 0 },
                { "Foraging", 1 },
                { "Fishing" , 2 },
                { "Mining"  , 3 },
                { "Other"   , 4 }
            };

            IdMap = config.VanillaCategories
                .Select(d => new KeyValuePair<string, List<int>>(d.Key, d.Value?["itemIDs"] ?? new List<int>()))
                .SelectMany(p => p.Value.Select(i => new Tuple<int, int>(i, vanillaCategoryIndexes[p.Key])))
                .ToDictionary(t => t.Item1, t => t.Item2);

            CategoryMap = config.VanillaCategories
                .Select(d => new KeyValuePair<string, List<int>>(d.Key, d.Value?["objectCategories"] ?? new List<int>()))
                .SelectMany(p => p.Value.Select(i => new Tuple<int, int>(i, vanillaCategoryIndexes[p.Key])))
                .ToDictionary(t => t.Item1, t => t.Item2);
        }

        public static void GetCategoryIndex_Postfix(StardewValley.Object o, ref int __result)
        {
            if (HarmonyCategoryPatch.IdMap.ContainsKey(o.ParentSheetIndex))
            {
                __result = HarmonyCategoryPatch.IdMap[o.ParentSheetIndex];
            }
            else if (HarmonyCategoryPatch.CategoryMap.ContainsKey(o.Category))
            {
                __result = HarmonyCategoryPatch.CategoryMap[o.Category];
            }
            else
            {
                __result = 4;
            }
        }
    }
}

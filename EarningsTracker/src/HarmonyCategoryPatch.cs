using System;
using System.Collections.Generic;
using System.Linq;

namespace EarningsTracker
{
    public class HarmonyCategoryPatch
    {
        private static readonly Dictionary<int, int> IdMap = new Dictionary<int, int>();
        private static readonly Dictionary<int, int> CategoryMap = new Dictionary<int, int>();

        public static void Initialize(ModConfig config)
        {
            var vanillaNameMap = new Dictionary<string, int>
            {
                { "Farming" , 0 },
                { "Foraging", 1 },
                { "Fishing" , 2 },
                { "Mining"  , 3 },
                { "Other"   , 4 }
            };

            foreach (KeyValuePair<string, Dictionary<string, List<int>>> definition in config.VanillaCategories)
            {
                var categoryIndex = vanillaNameMap[definition.Key];
                var itemIDs = definition.Value["itemIDs"];
                var objectCategories = definition.Value["objectCategories"];

                foreach (int id in itemIDs)
                {
                    if (!IdMap.ContainsKey(id))
                    {
                        IdMap.Add(id, categoryIndex);
                    }
                }

                foreach (int oc in objectCategories)
                {
                    if (!CategoryMap.ContainsKey(oc))
                    {
                        CategoryMap.Add(oc, categoryIndex);
                    }
                }
            }
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

using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;

namespace EarningsTracker
{
    public class CategoryPatch
    {
        private static IMonitor Monitor;
        private static readonly Dictionary<int, int> IdMap = new Dictionary<int, int>();
        private static readonly Dictionary<int, int> CategoryMap = new Dictionary<int, int>();

        public static void Initialize(ModConfig config, IMonitor monitor)
        {
            Monitor = monitor;

            var vanillaNameMap = new Dictionary<string, int>
            {
                { "Farming", 0 },
                { "Foraging", 1 },
                { "Fishing", 2 },
                { "Mining", 3 },
                { "Other", 4 }
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

        public static void MyHarmony_Postfix(StardewValley.Object o, ref int __result)
        {
            if (CategoryPatch.IdMap.ContainsKey(o.ParentSheetIndex))
            {
                __result = CategoryPatch.IdMap[o.ParentSheetIndex];
                return;
            }
            else if (CategoryPatch.CategoryMap.ContainsKey(o.Category))
            {
                __result = CategoryPatch.CategoryMap[o.Category];
                return;
            }
            else
            {
                __result = 4;
                return;
            }

/*

            switch (o.parentSheetIndex.Value)
            {
                case 107:           // Dinosaur Egg
                case 395:           // Coffee
                case 417:           // Sweet Gem Berry
                    __result = 0;
                    return;
                case 388:           // Wood
                case 709:           // Hardwood
                case 771:           // Fiber
                case 309:           // Acorn
                case 310:           // Maple Seed
                case 311:           // Pine Cone
                case 403:           // Field Snack
                case 296:           // Salmonberry
                case 396:           // Spice Berry
                case 402:           // Sweet Pea
                case 406:           // Wild Plum
                case 410:           // Blackberry
                case 414:           // Crystal Fruit
                case 418:           // Crocus
                case 430:           // Truffle
                case 495:           // Spring Seeds
                case 496:           // Summer Seeds
                case 497:           // Fall Seeds
                case 498:           // Winter Seeds
                    __result = 1;
                    return;
                case 152:           // Seaweed
                    __result = 2;
                    return;
                case 330:           // Clay
                case 390:           // Stone
                case 535:           // Geode
                case 536:           // Frozen Geode
                case 537:           // Magma Geode
                case 749:           // Omni Geode
                    __result = 3;
                    return;
                default:
                    switch (o.Category)
                    { 
                        case -80:
                        case -79:
                        case -75:
                        case -74:
                        case -26:
                        case -19:
                        case -18:
                        case -14:
                        case -6:
                        case -5:
                            __result = 0;
                            return;
                        case -81:
                        case -27:
                            __result = 1;
                            return;
                        case -23:
                        case -22:
                        case -21:
                        case -4:
                            __result = 2;
                            return;
                        case -28:
                        case -15:
                        case -12:
                        case -2:
                            __result = 3;
                            return;
                        default:
                            __result = 4;
                            return;
                    }
            }*/
        }
    }
}

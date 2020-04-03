﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley.Menus;
using Harmony;

namespace EarningsTracker
{
    public class CategoryPatch
    {
        private static IMonitor Monitor;
        //private static Dictionary<int, int> CustomCategoryIndex;

        public static void Initialize(ModConfig Config, IMonitor monitor)
        {
            Monitor = monitor;

        }

        public static void MyHarmony_Postfix(StardewValley.Object o, ref int __result)
        {
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
            }
        }
    }
}
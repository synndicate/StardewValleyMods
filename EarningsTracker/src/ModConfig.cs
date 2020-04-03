using System;
using System.Collections.Generic;
using System.Linq;

namespace EarningsTracker
{
    public sealed class ModConfig
    {
        public bool UseCustomCategories = false;
        public Dictionary<string, Dictionary<string, List<int>>> VanillaCategories;
        public Dictionary<string, Dictionary<string, List<int>>> CustomCategories;

        public ModConfig()
        {
            VanillaCategories = new Dictionary<string, Dictionary<string, List<int>>>
            { 
                { "Farming",  new Dictionary<string, List<int>>
                    {   { "itemIDs", new List<int>() },
                        { "objectCategories", new List<int> { -80, -79, -75, -26, -14, -6, -5 } } } 
                },
                { "Foraging", new Dictionary<string, List<int>>
                    {   { "itemIDs", new List<int> { 296, 396, 402, 406, 410, 414, 418 } },
                        { "objectCategories", new List<int> { -81, -27, -23 } } }
                },
                { "Fishing",  new Dictionary<string, List<int>>
                    {   { "itemIDs", new List<int>() },
                        { "objectCategories", new List<int> { -20, -4 } } }
                },
                { "Mining", new Dictionary<string, List<int>>
                    {   { "itemIDs", new List<int>() },
                        { "objectCategories", new List<int> { -15, -12, -2 } } }
                },
                { "Other", new Dictionary<string, List<int>>
                    {   { "itemIDs", new List<int>() },
                        { "objectCategories", new List<int>() } }
                }
            };
            CustomCategories = new Dictionary<string, Dictionary<string, List<int>>>
            {
                { "Artisan Goods",  new Dictionary<string, List<int>>
                    {   { "itemIDs", new List<int> { 395 } },
                        { "objectCategories", new List<int> { -26 } } }
                },
                { "Animal Products", new Dictionary<string, List<int>>
                    {   { "itemIDs", new List<int> { 107, 440, 444, 446 } },
                        { "objectCategories", new List<int> { -18, -14, -6, -5 } } }
                },
                { "Cooking",  new Dictionary<string, List<int>>
                    {   { "itemIDs", new List<int> { 245, 246, 247, 419, 423 } },
                        { "objectCategories", new List<int> { -25, -7 } } }
                },
                { "Crops", new Dictionary<string, List<int>>
                    {   { "itemIDs", new List<int> { 417 } },
                        { "objectCategories", new List<int> { -80, -79, -75, -74, -19 } } }
                },
                { "Foraging", new Dictionary<string, List<int>>
                    {   { "itemIDs", new List<int> { 88, 90, 259, 296, 396, 402, 406, 410, 414, 418, 430, 309, 310, 311, 403, 495, 496, 497, 498 } },
                        { "objectCategories", new List<int> { -81, -27 } } }
                },
                { "Fishing",  new Dictionary<string, List<int>>
                    {   { "itemIDs", new List<int> { 152, 797 } },
                        { "objectCategories", new List<int> { -23, -22, -21, -4 } } }
                },
                { "Mining", new Dictionary<string, List<int>>
                    {   { "itemIDs", new List<int> { 535, 536, 537, 749, 96, 97, 98, 99, 100, 101, 103, 104, 105, 106, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 579, 580, 581, 582, 583, 584, 585, 586, 587, 588, 589, 275 } },
                        { "objectCategories", new List<int> { -12, -2 } } }
                },
                { "Monster Loot",  new Dictionary<string, List<int>>
                    {   { "itemIDs", new List<int> { 413, 437, 439, 680 } },
                        { "objectCategories", new List<int> { -28 } } }
                },
                { "Equipment", new Dictionary<string, List<int>>
                    {   { "itemIDs", new List<int> { 441 } },
                        { "objectCategories", new List<int> { -99, -98, -96, -95, -29 } } }
                },
                { "Resources", new Dictionary<string, List<int>>
                    {   { "itemIDs", new List<int>() },
                        { "objectCategories", new List<int> { -16, -15 } } }
                },
                { "Trash",  new Dictionary<string, List<int>>
                    {   { "itemIDs", new List<int>() },
                        { "objectCategories", new List<int> { -20 } } }
                },
                { "Other", new Dictionary<string, List<int>>
                    {   { "itemIDs", new List<int>() },
                        { "objectCategories", new List<int>() } }
                }
            };
        }
    }
}

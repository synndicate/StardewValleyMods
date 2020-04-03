using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarningsTracker
{
    public sealed class ModConfig
    {
        public Dictionary<string, Dictionary<string, List<int>>> CustomCategories;

        public ModConfig()
        {
            CustomCategories = new Dictionary<string, Dictionary<string, List<int>>>
            { 

                { "Farming",  new Dictionary<string, List<int>>
                    {   { "itemIDs", new List<int> { 395 } },
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
            };
        }
    }
}

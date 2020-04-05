using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace Synndicate.Stardew.CustomProfitBreakdown
{
    public class ModEntry : Mod
    {
        /*****************
        ** Fields
        *****************/

        /*****************
        ** Public Methods
        *****************/

        public override void Entry(IModHelper helper)
        {
            var config = helper.ReadConfig<ModConfig>();
            config.Validate(Monitor);

        }

        /*****************
        ** Private Methods
        *****************/
    }
}

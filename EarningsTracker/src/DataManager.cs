using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace EarningsTracker
{
    using SItem = StardewValley.Item;

    public class DataManager
    {
        private readonly ModConfig Config;

        private EarningsTracker Main;
        private long MainPlayerId;
        private readonly IMonitor Monitor;

        public DataManager(ModConfig config, IMonitor monitor)
        {
            Config = config;
            Monitor = monitor;

            // load saved data
        }

        public void Register(Farmer player)
        {
            // Until multiplayer migration, no need to have multiple EarningsTrackers
            Main = new EarningsTracker(player, Config);
            MainPlayerId = player.uniqueMultiplayerID;
        }

        /******************
        ** Public Methods
        * 
        *  Remove default parameter values when migrating to multiplayer support
        *  or keep and add if (playerId == 0)  use MainPlayerId
        *  
        ******************/

        public void AddItemSoldEvent(IEnumerable<SItem> items, long playerId = 0)
        {
            Main.AddItemSoldEvent(items);
        }

        public void AddItemSoldEvent(IEnumerable<ItemStackSizeChange> itemsSold, long playerId = 0)
        {
            AddItemSoldEvent(itemsSold
                .Select(change => { change.Item.Stack = change.OldSize - change.NewSize; return change.Item; }), playerId);
        }

        public void AddAnimalEarning(long playerId = 0)
        {
            Main.AddAnimalEarning();
        }
        public void AddMailEarning(long playerId = 0)
        {
            Main.AddMailEarning();
        }
        public void AddQuestEarning(long playerId = 0)
        {
            Main.AddQuestEarning();
            
        }
        public void AddTrashEarning(long playerId = 0)
        {
            Main.AddTrashEarning();
        }
        public void AddUnknownEarning(long playerId = 0)
        {
            Main.AddUnknownEarning();
        }
        public void UpdateEarnings(long playerId = 0)
        {
            Main.UpdateEarnings();
        }
        public void UpdateAllPlayerEarnings()
        {
            Main.UpdateEarnings();
        }
        public uint TotalTrackedEarnings(long playerId = 0)
        {
            return Main.TotalTrackedEarnings;
        }

        public JsonTotal PackageEarningsData()
        {
            var day = Main.PackageDayData();

            // if no save loaded, create tree
            var jsonDay    = new JsonDay(day.Date.Day, day);
            var jsonWeek   = new JsonWeek(day.Date.Week, new List<JsonDay> { { jsonDay } });
            var jsonSeason = new JsonSeason(day.Date.Season, new List<JsonWeek> { jsonWeek });
            var jsonYear   = new JsonYear(day.Date.Year, new List<JsonSeason> { jsonSeason });

            return new JsonTotal(new List<JsonYear> { jsonYear });

            // else
            //      append to tree
        }
    }
}

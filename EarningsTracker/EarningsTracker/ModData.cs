
namespace EarningsTracker
{
    public sealed class ModData
    {
        public static string DataKey = "ModData";

        public string SaveName { get; set; }

        private ModData() { }

        public ModData(string saveName)
        {
            SaveName = saveName;
        }
    }
}

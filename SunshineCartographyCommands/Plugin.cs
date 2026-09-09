using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;


namespace SunshineCartographyCommands
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class Plugin : BaseUnityPlugin
    {
        internal const string ModName = "LimitCartographyPins";
        internal const string ModVersion = "1.2.0";
        internal const string Author = "Weiler";
        private const string ModGUID = Author + "." + ModName;

        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource ItemManagerModTemplateLogger =
            BepInEx.Logging.Logger.CreateLogSource(ModName);

        /* VALHEIMS NEW BEHAVIOR
         * When clicking Read on map table:
         * - Merge explored map into yours
         * - Delete others pins from your map
         * - Import pins from table
         * - Skip importing pins close to existing pins
         * - Skip importing pins created by yourself
         * 
         * When clicking Write on map table:
         * - Read map table into your own map as if you click read (inlcuding delete others pins from your own map)
         * - Merge with your map
         * - Read map again from table
         * - Merge your map with table map into package
         * - Add your pins into package
         * - Send to Owner/Host over RPC
         */

        /* HOW WE DO IT:
         * When reading map:
         * - We will skip deleting others pins from your map
         * - We will get all pins from the map table, including those you have made yourself
         * 
         * ->> Slash commands allows you to delete pins from your map.
         * ->> Map table can be rebuild if you want to erase all records.
         * 
         * When writing to map:
         * - We will skip the read operation and bypass most of the original merge code
         * - We will read the tables data, and merge explored map with our own.
         * - We will read all pins on the table, merge with our own pins.
         * - Player Pins (the pins players can make themselves), will be skipped by default.
         * - Duplicate pins will be skipped.
         * - The merged data will be sent by RPC to Owner/Area Host
         *  
         * ->> Slash command allows player to add their own Player Pins to the map
         *  
         *  
         *  WHY WE WANT IT LIKE THIS:
         *  - We have a public map that everyone can read from, with a designated character adding pins for various world locations.
         *  - Teams have their own map, can share pins as they wish, and can read pins from public table without removing the teams pins and vice versa.
         *  - Players might want their own map table to share between alts.
         */

        public void Awake()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
        }
    }
}
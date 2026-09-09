using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using static Minimap;

namespace SunshineCartographyCommands.Patches
{
    [HarmonyPatch(typeof(MapTable))]
    public class MapTable_Patches
    {
        private static bool allowRead = true;

        [HarmonyPrefix]
        [HarmonyPatch("OnWrite")]
        static void OnWrite_PrefixPatch()
        {
            //disable read function on map, as this is called from OnWrite
            allowRead = false;
        }
        [HarmonyPostfix]
        [HarmonyPatch("OnWrite")]
        static void OnWrite_PostfixPatch()
        {
            //re-enable read function on map
            allowRead = true;
        }
        [HarmonyPrefix]
        [HarmonyPatch("OnRead", typeof(Switch), typeof(Humanoid), typeof(ItemDrop.ItemData), typeof(bool))]
        static bool OnRead_PrefixPatch()
        {
            return allowRead;
        }
    }
}

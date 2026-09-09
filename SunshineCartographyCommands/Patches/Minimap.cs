using HarmonyLib;
using Splatform;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static Minimap;

namespace SunshineCartographyCommands.Patches
{
    [HarmonyPatch(typeof(Minimap))]
    class Minimap_Patches
    {
        private static bool allowRemovePin = true;
        public static bool writePinDataOnce = false;

        [HarmonyPrefix]
        [HarmonyPatch("AddSharedMapData")]
        static void AddSharedMapData_PrefixPatch()
        {
            allowRemovePin = false;
        }
        [HarmonyPostfix]
        [HarmonyPatch("AddSharedMapData")]
        static void AddSharedMapData_PostfixPatch(Minimap __instance)
        {
            allowRemovePin = true;
            foreach (Minimap.PinData pin in __instance.m_pins)
            {
                if (pin.m_shouldDelete) pin.m_shouldDelete = false;
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch("RemovePin", typeof(Minimap.PinData))]
        static bool RemovePin_PrefixPatch()
        {
            return allowRemovePin;
        }

        //transpiler to skip the pin owner check when reading pins off a map table
        [HarmonyPatch(typeof(Minimap))]
        [HarmonyPatch("AddSharedMapData")]
        public class AddSharedMapDataPatch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);

                // Log the instructions before modifying
                //Debug.Log("Instructions before transpiler:");
                //foreach (var code in codes)
                //{
                //    Debug.Log($"{code.opcode} {code.operand}");
                //}


                for (int i = 0; i < codes.Count; i++)
                {
                    // Look for the sequence: Stfld -> Br -> Ldloc_S -> Ldloc_S -> Beq
                    if (codes[i].opcode == OpCodes.Stfld &&
                        codes[i + 1].opcode == OpCodes.Br &&
                        codes[i + 2].opcode == OpCodes.Ldloc_S &&
                        codes[i + 3].opcode == OpCodes.Ldloc_S &&
                        codes[i + 4].opcode == OpCodes.Beq)
                    {
                        // Replace the `Beq` instruction with two `Pop` instructions and a `Nop`
                        codes[i + 4] = new CodeInstruction(OpCodes.Pop); // Remove first value
                        codes.Insert(i + 5, new CodeInstruction(OpCodes.Pop)); // Remove second value
                        codes.Insert(i + 6, new CodeInstruction(OpCodes.Nop)); // Add `Nop` for safety
                        break; // Exit after modifying the first match
                    }
                }

                // Log the instructions after modifying
                //Debug.Log("Instructions after transpiler:");
                //foreach (var code in codes)
                //{
                //    Debug.Log($"{code.opcode} {code.operand}");
                //}

                return codes;
            }
        }

        /*
         * 
         * New merger method, bypasses original code
         * 
         */

        [HarmonyPrefix]
        [HarmonyPatch("GetSharedMapData")]
        static bool GetSharedMapData_PrefixPatch(Minimap __instance, byte[] oldMapData, ref byte[] __result)
        {
            //table data
            byte[] table_Data = oldMapData;// __instance.m_nview.GetZDO().GetByteArray(ZDOVars.s_data);
            List<bool> table_Explored = new List<bool>();
            ZPackage table_ZPackage = null;
            int table_Version = 0;
            if (table_Data != null)
            {
                //table_Data = Utils.Decompress(table_Data);
                table_ZPackage = new ZPackage(table_Data);
                table_Version = table_ZPackage.ReadInt();
                int table_ExploredSize = table_ZPackage.ReadInt();
                //Debug.Log($"table_ExploredSize {table_ExploredSize}");
                for (int i = 0; i < table_ExploredSize; i++)
                {
                    table_Explored.Add(table_ZPackage.ReadBool());
                }
            }
            //merged data array
            ZPackage new_ZPackage = new ZPackage();
            new_ZPackage.Write(3);
            new_ZPackage.Write(__instance.m_explored.Length);

            for (int i = 0; i < __instance.m_explored.Length; i++)
            {
                bool explored = __instance.m_explored[i] || __instance.m_exploredOthers[i];
                if (table_Explored.Count > i) explored |= table_Explored[i];
                new_ZPackage.Write(explored);
            }

            //table pins
            //Debug.Log("table pins:");
            List<Minimap.PinData> table_Pins = new List<Minimap.PinData>();
            if (table_ZPackage != null)
            {
                int table_PinSize = table_ZPackage.ReadInt();
                for (int i = 0; i < table_PinSize; i++)
                {
                    PinData pinData = new PinData();
                    pinData.m_ownerID = table_ZPackage.ReadLong();
                    pinData.m_name = table_ZPackage.ReadString();
                    pinData.m_pos = table_ZPackage.ReadVector3();
                    pinData.m_type = (PinType)table_ZPackage.ReadInt();
                    pinData.m_checked = table_ZPackage.ReadBool();
                    if (table_Version >= 3)
                    {
                        if (Splatform.PlatformUserID.TryParse(table_ZPackage.ReadString(), out PlatformUserID platformUserID2))
                        {
                            pinData.m_author = platformUserID2;
                        }
                    }
                    //pinData.m_author = ((table_Version >= 3) ? table_ZPackage.ReadString() : "");
                    pinData.m_save = true;
                    table_Pins.Add(pinData);
                    //Debug.Log($"m_type {pinData.m_type} {pinData.m_pos}");
                }
            }

            //merge pins
            //Debug.Log("add table pins:");
            foreach (Minimap.PinData add_Pin in __instance.m_pins)
            {
                if (IsTemporaryPin(add_Pin)) continue; //skip temporary, no-save pins, like pings
                if (add_Pin.m_type == PinType.Death) continue; //skip death markers
                if (IsPlayerPin(add_Pin.m_type) && !writePinDataOnce) continue; //skip playerpins unless writePinData is enabled

                //pins too close will be skipped
                //only applies to Player Pins or if same type
                bool add_PinIsPlayerPin = false;
                if (IsPlayerPin(add_Pin.m_type)) add_PinIsPlayerPin = true;

                Minimap.PinData table_PinNear = null;
                foreach (Minimap.PinData table_Pin in table_Pins)
                {
                    if (!IsTemporaryPin(table_Pin) && Utils.DistanceXZ(add_Pin.m_pos, table_Pin.m_pos) < 1f)
                    {
                        bool table_PinIsPlayerPin = false;
                        if (IsPlayerPin(table_Pin.m_type)) table_PinIsPlayerPin = true;
                        if (add_PinIsPlayerPin == true && table_PinIsPlayerPin || add_Pin.m_type == table_Pin.m_type)
                        {
                            table_PinNear = table_Pin;
                            break;
                        }
                    }
                }
                //add to table pins
                if (table_PinNear == null)
                {
                    //Debug.Log($"table_Pins.Add");
                    table_Pins.Add(add_Pin);
                }
            }

            //append pins to package
            long playerID = Player.m_localPlayer.GetPlayerID();
            //string networkUserId = PrivilegeManager.GetNetworkUserId();
            PlatformUserID platformUserID = PlatformManager.DistributionPlatform.LocalUser.PlatformUserID;
            new_ZPackage.Write(table_Pins.Count);
            foreach (PinData new_Pin in table_Pins)
            {
                long ownerID = ((new_Pin.m_ownerID != 0L) ? new_Pin.m_ownerID : playerID);
                //string author = ((string.IsNullOrEmpty(new_Pin.m_author) && ownerID == playerID) ? networkUserId : new_Pin.m_author);
                PlatformUserID platformUserID2 = (ownerID == playerID ? platformUserID : new_Pin.m_author);
                new_ZPackage.Write(ownerID);
                new_ZPackage.Write(new_Pin.m_name);
                new_ZPackage.Write(new_Pin.m_pos);
                new_ZPackage.Write((int)new_Pin.m_type);
                new_ZPackage.Write(new_Pin.m_checked);
                if (platformUserID2 != null)
                {
                    new_ZPackage.Write(platformUserID2.ToString());
                }
                else
                {
                    new_ZPackage.Write("");
                }
            }

            writePinDataOnce = false;
            __result = new_ZPackage.GetArray();
            return false;
        }
        public static bool IsPlayerPin(Minimap.PinType pinType)
        {
            return (pinType == PinType.Icon0
                || pinType == PinType.Icon1
                || pinType == PinType.Icon2
                || pinType == PinType.Icon3
                || pinType == PinType.Icon4
                );
        }
        public static bool IsTemporaryPin(Minimap.PinData pinData)
        {
            return !pinData.m_save;
        }
    }
}
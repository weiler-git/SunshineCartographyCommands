using HarmonyLib;
using System.Collections.Generic;
using static Terminal;

namespace SunshineCartographyCommands.Patches
{

    [HarmonyPatch(typeof(Terminal))]
    class Terminal_Patches
    {


        [HarmonyPostfix]
        [HarmonyPatch("InitTerminal")]
        static void InitTerminal_Patch()
        {

            new Terminal.ConsoleCommand("removemypins", "Removes all personal pins from map", delegate (ConsoleEventArgs args)
            {
                RemoveMyPins(args.Context, args.Args);
            });
            new Terminal.ConsoleCommand("removeotherspins", "Removes all pins from others from map", delegate (ConsoleEventArgs args)
            {
                RemoveOthersPins(args.Context, args.Args);
            });
            new Terminal.ConsoleCommand("removeallpins", "Removes allpins from map", delegate (ConsoleEventArgs args)
            {
                RemoveAllPins(args.Context, args.Args);
            });
            new Terminal.ConsoleCommand("writepindata", "writepindata", delegate (ConsoleEventArgs args)
            {
                WritePinData(args.Context, args.Args);
            });
        }



        public static void RemoveMyPins(Terminal context, string[] args)
        {
            if (Player.m_localPlayer == null)
            {
                return;
            }
            List<Minimap.PinData> pinsToDelete = new List<Minimap.PinData>();
            foreach (Minimap.PinData pin in Minimap.instance.m_pins)
            {
                if (Minimap_Patches.IsTemporaryPin(pin)) continue;
                if (Minimap_Patches.IsPlayerPin(pin.m_type) && pin.m_ownerID == 0L) pinsToDelete.Add(pin);
            }
            int pinsDeleted = 0;
            foreach (Minimap.PinData pin in pinsToDelete)
            {
                Minimap.instance.m_pins.Remove(pin);
                pinsDeleted++;
            }
            context.AddString($"{pinsDeleted} pins deleted");
        }
        public static void RemoveOthersPins(Terminal context, string[] args)
        {
            if (Player.m_localPlayer == null)
            {
                return;
            }
            List<Minimap.PinData> pinsToDelete = new List<Minimap.PinData>();
            foreach (Minimap.PinData pin in Minimap.instance.m_pins)
            {
                if (Minimap_Patches.IsTemporaryPin(pin)) continue;
                if (Minimap_Patches.IsPlayerPin(pin.m_type) && pin.m_ownerID != 0L) pinsToDelete.Add(pin);
            }
            int pinsDeleted = 0;
            foreach (Minimap.PinData pin in pinsToDelete)
            {
                Minimap.instance.m_pins.Remove(pin);
                pinsDeleted++;
            }
            context.AddString($"{pinsDeleted} pins deleted");
        }
        public static void RemoveAllPins(Terminal context, string[] args)
        {
            if (Player.m_localPlayer == null)
            {
                return;
            }
            List<Minimap.PinData> pinsToDelete = new List<Minimap.PinData>();
            foreach (Minimap.PinData pin in Minimap.instance.m_pins)
            {
                if (Minimap_Patches.IsTemporaryPin(pin)) continue;
                if (Minimap_Patches.IsPlayerPin(pin.m_type)) pinsToDelete.Add(pin);
            }
            int pinsDeleted = 0;
            foreach (Minimap.PinData pin in pinsToDelete)
            {
                Minimap.instance.m_pins.Remove(pin);
                pinsDeleted++;
            }
            context.AddString($"{pinsDeleted} pins deleted");
        }
        public static void WritePinData(Terminal context, string[] args)
        {
            if (Player.m_localPlayer == null)
            {
                return;
            }

            Minimap_Patches.writePinDataOnce = true;
            context.AddString($"Pins will be written once on interact with cartography table");
        }
    }
}


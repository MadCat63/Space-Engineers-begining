using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using VRageMath;
using VRage.Game;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Ingame;
using Sandbox.Game.EntityComponents;
using VRage.Game.Components;
using VRage.Collections;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
public sealed class Program : MyGridProgram
{
    IMyTextPanel TP1, TP2;

    Program()
    {
        TP1 = (IMyTextPanel)GridTerminalSystem.GetBlockWithName("TP1");
        TP2 = (IMyTextPanel)GridTerminalSystem.GetBlockWithName("TP2");
        Runtime.UpdateFrequency = UpdateFrequency.Update100;
    }

    void Main()
    {
        var Bat = new List<IMyTerminalBlock>();
        GridTerminalSystem.SearchBlocksOfName("Battery", Bat);
        TP1.WriteText("", false);
        foreach (IMyBatteryBlock Batt in Bat)
        {
                TP1.WriteText(Batt.DisplayNameText + "\n" + " Заряда осталось: " + Batt.CurrentStoredPower + "\n", true);
        }

        
        var Cargo1 = new List<IMyTerminalBlock>();
        GridTerminalSystem.SearchBlocksOfName("Cargo", Cargo1);

        TP2.WriteText("", false);
        foreach (IMyCargoContainer CargoContainer in Cargo1)
        {
            var Items = new List<MyInventoryItem>();
            CargoContainer.GetInventory(0).GetItems(Items, null);
            TP2.WriteText(CargoContainer.DisplayNameText + "\n",true);
            int id = 0;
            foreach (MyInventoryItem item in Items)
            {
                id++;
                if (item.Type.SubtypeId != null)
                {
                    TP2.WriteText(id + ") " + item.Type.SubtypeId + ": " + item.Amount + "\n", true);
                }
            }
        }
    }
}
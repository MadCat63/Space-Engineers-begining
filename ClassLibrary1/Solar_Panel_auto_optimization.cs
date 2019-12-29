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

    IMyTextPanel TP;
    IMyRemoteControl RemCon;
    IMySolarPanel SolarPanel;
    int InverseCounter;
    bool Stop;
    float GyroMult = 1f;

    Vector3D AxisV = new Vector3D(0, -1, 0);

    double SolarOutput, PrevSolarOutput;
    int RollDirection = 1;


    void Main(string argument)
    {
        if (TP == null)
            TP = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel;
        if (RemCon == null)
            RemCon = GridTerminalSystem.GetBlockWithName("RemCon") as IMyRemoteControl;
        if (SolarPanel == null)
            SolarPanel = GridTerminalSystem.GetBlockWithName("SolarPanel") as IMySolarPanel;
        switch (argument)
        {
            case "Start":
                {
                    Stop = false;
                    break;
                }
            case "Stop":
                {
                    Stop = true;
                    break;
                }
            default:
                break;
        }
        if (!Stop)
        {
            PrevSolarOutput = SolarOutput;
            SolarOutput = SolarPanel.MaxOutput;
                TP.WriteText("\n" + SolarOutput + ";", true);
                  Vector3D GyrAng = GetNavAngles(AxisV);
                  SetMotorOverride(GyrAng * GyroMult);
            Runtime.UpdateFrequency = UpdateFrequency.Update10;

        }
        else
        {
            SetMotorOverride(new Vector3(0, 0, 0));
        }
    }

    Vector3D GetNavAngles(Vector3D Target)
    {
        Vector3D V3Dup = RemCon.WorldMatrix.Up;
        Vector3D V3Dleft = RemCon.WorldMatrix.Left;

        double TargetPitch = Math.Asin(Vector3D.Dot(V3Dup, Vector3D.Normalize(Vector3D.Reject(Target, V3Dleft))));
        double TargetYaw = Math.Asin(Vector3D.Dot(V3Dleft, Vector3D.Normalize(Vector3D.Reject(Target, V3Dup))));

        double TargetRoll = 0;
        if (TargetPitch + TargetYaw < 0.01)
        {
            InverseCounter--;
            if ((SolarOutput < PrevSolarOutput) && (InverseCounter < 0))
            {
                InverseCounter = 5;
                RollDirection *= -1;
            }
            TargetRoll = RollDirection * (0.16 - SolarOutput) * 50;
        }

        TP.WriteText("Yaw: " + Math.Round(TargetYaw, 5) + "\n Pitch: " + Math.Round(TargetPitch, 5) + "\n Roll: " + Math.Round(TargetRoll, 5));
        TP.WriteText("\n Previous: " + PrevSolarOutput, true);
        TP.WriteText("\n Current:  " + SolarOutput, true);
        return new Vector3D(TargetYaw, -TargetPitch, TargetRoll);
    }

    void SetMotorOverride(Vector3 settings)
    {
        var Motors = new List<IMyTerminalBlock>();
        GridTerminalSystem.SearchBlocksOfName("Rotor", Motors);
        foreach (IMyMotorAdvancedStator Motor in Motors)
        {
            if (Motor != null)
            {
                if (Motor.CustomName == "RotorYaw")
                    Motor.SetValue("Velocity", settings.GetDim(0));
                else if (Motor.CustomName == "RotorPitch")
                    Motor.SetValue("Velocity", settings.GetDim(1));
                else if (Motor.CustomName == "RotorRoll")
                    Motor.SetValue("Velocity", settings.GetDim(2));
            }
        }
    }
}
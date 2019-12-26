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
    bool Stop, Log;
    float GyroMult = 1f;

    Vector3D AxisV = new Vector3D(0, -1, 0);
    Vector3D SolarV1 = new Vector3D(0, 0, 0);
    Vector3D SolarV2 = new Vector3D(0, 0, 0);

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
            case "Log":
                {
                    Log = !Log;
                    break;
                }
            case "Cross":
                {
                    Stop = true;
                    FindAxis();
                    break;
                }
            default:
                break;
        }
        if (!Stop)
        {
            PrevSolarOutput = SolarOutput;
            SolarOutput = SolarPanel.MaxOutput;
            //if (Log)
            //{
                TP.WriteText("\n" + SolarOutput + ";", true);
                SetGyroOverride(true, new Vector3(0, 0, 0));
            //}
            //else
            //{
            //    Vector3D GyrAng = GetNavAngles(AxisV);
            //    SetMotorOverride(GyrAng * GyroMult);
            //}

            Runtime.UpdateFrequency = UpdateFrequency.Update10;

        }
        else
        {
            SetGyroOverride(false, new Vector3(0, 0, 0));
        }
    }

    //void FindAxis()
    //{
    //    SolarV1 = SolarV2;
    //    SolarV2 = RemCon.WorldMatrix.Forward;
    //    AxisV = Vector3D.Normalize(SolarV2.Cross(SolarV1));
    //    TP.WriteText(" X: " + Math.Round(AxisV.GetDim(0), 5) +
    //                     "\n Y: " + Math.Round(AxisV.GetDim(1), 5) +
    //                     "\n Z: " + Math.Round(AxisV.GetDim(2), 5));
    //}

    Vector3D GetNavAngles(Vector3D Target)
    {
        //Vector3D V3Dfow = RemCon.WorldMatrix.Forward;
        Vector3D V3Dup = RemCon.WorldMatrix.Up;
        Vector3D V3Dleft = RemCon.WorldMatrix.Left;

        double TargetPitch = Vector3D.Dot(V3Dup, Vector3D.Normalize(Vector3D.Reject(Target, V3Dleft)));
        TargetPitch = Math.Acos(TargetPitch) - Math.PI / 2;
        double TargetYaw = Vector3D.Dot(V3Dleft, Vector3D.Normalize(Vector3D.Reject(Target, V3Dup)));
        TargetYaw = Math.Acos(TargetYaw) - Math.PI / 2;

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
        TP.WriteText("\n p: " + PrevSolarOutput, true);
        TP.WriteText("\n n: " + SolarOutput + " \n", true);
        return new Vector3D(-TargetYaw, -TargetPitch, TargetRoll);
    }

    void SetGyroOverride(bool OverrideOnOff, Vector3 settings, float Power = 1)
    {
        var Gyros = new List<IMyTerminalBlock>();
        GridTerminalSystem.SearchBlocksOfName("Gyro", Gyros);
        for (int i = 0; i < Gyros.Count; i++)
        {
            IMyGyro Gyro = Gyros[i] as IMyGyro;
            if (Gyro != null)
            {
                if ((!Gyro.GyroOverride && OverrideOnOff) || (Gyro.GyroOverride && !OverrideOnOff))
                    Gyro.ApplyAction("Override");
                Gyro.SetValue("Power", Power);
                Gyro.SetValue("Yaw", settings.GetDim(0));
                Gyro.SetValue("Pitch", settings.GetDim(1));
                Gyro.SetValue("Roll", settings.GetDim(2));
            }
        }
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
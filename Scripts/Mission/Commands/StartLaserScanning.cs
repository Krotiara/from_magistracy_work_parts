using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CableWalker.Simulator.LaserScanning;
using CableWalker.Simulator.Model;
using CableWalker.Simulator.Modules;
using UnityEngine;

namespace CableWalker.Simulator.Mission.Commands
{
    public class StartLaserScanning :  Command
    {
        public int LaserScanIteration { get; }
       
        private LaserScanner laserScaner;
       

        public StartLaserScanning(int laserScanIteration)
        {
            LaserScanIteration = laserScanIteration;
            Status = ConsoleCommandStatus.WaitingInLine;
            var alias = CommandManager.GetDescriptor(this).Aliases.ToList()[0];
            Name = $"{alias}({laserScanIteration})";
        }

        public override IEnumerator DebugExecute(CableWalkerApi cableWalkerApi)
        {

            if (cableWalkerApi.WheelModule.Cable == null)
            {
                Status = ConsoleCommandStatus.NeedToSitOnCable;
                yield break;
            }
            Status = ConsoleCommandStatus.Running;
            laserScaner = cableWalkerApi.GetComponentInChildren<LaserScanner>();
            laserScaner.StartScanning();
            for (int i=0; i<LaserScanIteration; i++)
            {
                laserScaner.Scann();
                cableWalkerApi.FlightModule.Up(10);
                yield return null;
            }
            laserScaner.EndScanning();
            yield return null;
            Status = ConsoleCommandStatus.Success;
        }

        

        public override Message GetMessageToSend()
        {
            throw new NotSupportedCommandForMessageException();
        }

        public override void SetParams(CableWalkerApi cableWalkerApi)
        {
            return;
        }

        public new IEnumerator Execute(CableWalkerApi cableWalkerApi, CableWalkerClient cableWalkerClient)
        {
            throw new NotImplementedException();
        }

       
    }
}
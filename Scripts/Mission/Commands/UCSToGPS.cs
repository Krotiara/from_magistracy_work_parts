using CableWalker.Simulator;
using CableWalker.Simulator.Mission.Commands;
using CableWalker.Simulator.Modules;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace CableWalker.Simulator.Mission.Commands
{
    public class UCSToGPS : Command
    {
        public float X { get; }
        public float Z { get; }
     

      

        public UCSToGPS(float x, float z)
        {
            X = x;
            Z = z;
            Status = ConsoleCommandStatus.WaitingInLine;
            var alias = CommandManager.GetDescriptor(this).Aliases.First();
            Name = $"{alias}({x},{z})";
        }

        public override IEnumerator DebugExecute(CableWalkerApi cableWalkerApi)
        {
            Status = ConsoleCommandStatus.Running;
            var point = new Vector3(X, 0, Z);
            var gps = GPSEncoder.USCToGPS(point);
            Debug.Log($"{gps[0]},{gps[1]}");
            Status = ConsoleCommandStatus.Success;
            yield break;
        }

       

        public override Message GetMessageToSend()
        {
            throw new NotSupportedCommandForMessageException();
        }

        public override void SetParams(CableWalkerApi cableWalkerApi)
        {
            return;
        }

        public new  IEnumerator Execute(CableWalkerApi cableWalkerApi, CableWalkerClient cableWalkerClient)
        {
            throw new System.NotImplementedException();
        }

       
    }
}

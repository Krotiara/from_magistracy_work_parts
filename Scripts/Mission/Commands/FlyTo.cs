using CableWalker.Simulator.Modules;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CableWalker.Simulator.Mission.Commands
{
    public class FlyTo : Command
    {
        private const float Precision = 0.05f;
        
        public float X { get; }
        public float Y { get; }
        public float Z { get; }
       

        public FlyTo(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
            Status = ConsoleCommandStatus.WaitingInLine;
            var alias = CommandManager.GetDescriptor(this).Aliases.First();
            Name = $"{alias}({x},{y},{z})";
        }

        public override IEnumerator DebugExecute(CableWalkerApi cableWalkerApi)
        {
            Status = ConsoleCommandStatus.Running;
            var target = new Vector3(X, Y, Z);
            
            while (Vector3.Distance(cableWalkerApi.transform.position, target) > Precision)
            {
                var power = (target - cableWalkerApi.transform.position).normalized;
                var localPower = cableWalkerApi.transform.InverseTransformVector(power);
                localPower.y = power.y;
                localPower = localPower.normalized;
                
                if (localPower.y > 0)
                    cableWalkerApi.FlightModule.Up(localPower.y);
                else
                    cableWalkerApi.FlightModule.Down(-localPower.y);
                
                if (localPower.z > 0)
                    cableWalkerApi.FlightModule.Forward(localPower.z);
                else
                    cableWalkerApi.FlightModule.Backward(-localPower.z);
                
                if (localPower.x > 0)
                    cableWalkerApi.FlightModule.RollRight(localPower.x);
                else
                    cableWalkerApi.FlightModule.RollLeft(-localPower.x);
                
                yield return null;
            }
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
            throw new System.NotImplementedException();
        }

    }
}

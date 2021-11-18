using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CableWalker.Simulator.Modules;
using UnityEngine;

namespace CableWalker.Simulator.Mission.Commands
{
    public class RotateTo : Command
    {
        private const float Precision = 4.0f;
      
        public float X { get; }
        public float Y { get; }
        public float Z { get; }
       

        public RotateTo(float x, float y, float z)
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

            var direction = target - cableWalkerApi.transform.position;
            direction.y = 0;
            var currentAngle = Vector3.SignedAngle(cableWalkerApi.transform.forward, direction, Vector3.up);
            while (Mathf.Abs(currentAngle) > Precision)
            {
                if (currentAngle > 0)
                    cableWalkerApi.FlightModule.YawRight(1);
                else
                    cableWalkerApi.FlightModule.YawLeft(1);
                direction = target - cableWalkerApi.transform.position;
                direction.y = 0;
                currentAngle = Vector3.SignedAngle(cableWalkerApi.transform.forward, direction, Vector3.up);
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

        public new  IEnumerator Execute(CableWalkerApi cableWalkerApi, CableWalkerClient cableWalkerClient)
        {
            throw new System.NotImplementedException();
        }

    }
}

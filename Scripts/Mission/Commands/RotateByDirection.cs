
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CableWalker.Simulator.Modules;
using UnityEngine;


namespace CableWalker.Simulator.Mission.Commands
{
    public class RotateByDirection : Command
    {
        private const float Precision = 1.0f;
        public Vector3 Direction { get; set; }
       
        private Vector3 direction;
       
        public RotateByDirection(Vector3 direction)
        {
            Direction = new Vector3(direction.x, 0, direction.z);
            Status = ConsoleCommandStatus.WaitingInLine;
            var alias = CommandManager.GetDescriptor(this).Aliases.First();
            Name = $"{alias}({direction.ToString()})";
        }


        public override IEnumerator DebugExecute(CableWalkerApi cableWalkerApi)
        {
            Status = ConsoleCommandStatus.Running;
            var currentAngle = Vector3.SignedAngle(cableWalkerApi.transform.forward, Direction, Vector3.up);
            float force = 1;
            while (Mathf.Abs(currentAngle) >= Precision)
            {
                if (Mathf.Abs(currentAngle) < 2)
                    force = 0.3f;
                if (currentAngle > 0)
                    cableWalkerApi.FlightModule.YawRight(force);
                else
                    cableWalkerApi.FlightModule.YawLeft(force);      
                yield return null;
                currentAngle = Vector3.SignedAngle(cableWalkerApi.transform.forward, Direction, Vector3.up);
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

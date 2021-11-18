using CableWalker.Simulator.Mission.Commands;
using CableWalker.Simulator.Modules;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace CableWalker.Simulator.Mission.Commands
{
    public class WheelModuleMoveTo : Command
    {
        private const float Precision = 0.001f;

        public float DistanceFromTower1 { get; private set; }
        public float Speed { get; }
      
      
        public WheelModuleMoveTo(float distanceFromTower1,float speedMps)
        {
            DistanceFromTower1 = distanceFromTower1;
            Speed = speedMps;
            Status = ConsoleCommandStatus.WaitingInLine;
            var alias = CommandManager.GetDescriptor(this).Aliases.First();
            Name = $"{alias}({distanceFromTower1},{speedMps})";
        }

        public override IEnumerator DebugExecute(CableWalkerApi cableWalkerApi)
        {
            if (cableWalkerApi.WheelModule == null)
            {
                Status = ConsoleCommandStatus.OnDroneNoWheelModule;
                yield break;
            }

            Status = ConsoleCommandStatus.Running;
            if (DistanceFromTower1 <= 0)
            {
                Status = ConsoleCommandStatus.DistanceArgumentMustBePositive;
                yield break;
            }

            if (cableWalkerApi.FlightModule.IsEnginesEnabled)
            {
                Status = ConsoleCommandStatus.NeedToTurnOffTheEngines;
                yield break;
            }

            var targetPoint = cableWalkerApi.WheelModule.Cable.GetTByDistance(DistanceFromTower1);
            bool isForwardDirection = cableWalkerApi.WheelModule.Point < targetPoint;
            cableWalkerApi.WheelModule.StartMoving(Speed, isForwardDirection); 
            while (Mathf.Abs(cableWalkerApi.WheelModule.Point - targetPoint) > Precision)
                yield return null;
            SetParams(cableWalkerApi);
            Status = ConsoleCommandStatus.Success;
        }

       

        public override Message GetMessageToSend()
        {
            throw new NotSupportedCommandForMessageException();
        }

        public override void SetParams(CableWalkerApi cableWalkerApi)
        {
            cableWalkerApi.WheelModule.StopMoving();
        }

        public new IEnumerator Execute(CableWalkerApi cableWalkerApi, CableWalkerClient cableWalkerClient)
        {
            throw new System.NotImplementedException();
        }

       
    }
}

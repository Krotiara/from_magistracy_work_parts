using CableWalker.Simulator.Modules;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CableWalker.Simulator.Mission.Commands
{
    public class WheelModuleMove : Command
    {
        private const float Precision = 0.01f;
       

        public float Distance { get; private set; }
        public float Speed { get; private set; }
        public bool IsForwardDirection { get; }

        public float SpeedInPercents { get; private set; }
        public bool IsBackward { get; }



        public WheelModuleMove(float distanceM, float speedMps, int forwardDirectionInt)
        {
            Distance = distanceM;
            Speed = speedMps;
            Status = ConsoleCommandStatus.WaitingInLine;
            var alias = CommandManager.GetDescriptor(this).Aliases.First();
            Name = $"{alias}({distanceM},{speedMps},{forwardDirectionInt})";
            IsForwardDirection = forwardDirectionInt == 1;
            if (forwardDirectionInt != 0 && forwardDirectionInt != 1)
                Status = ConsoleCommandStatus.IncorrectDirectionIntArgument;
        }

        public WheelModuleMove(int speedInPercents, bool isBackward)
        {
            SpeedInPercents = speedInPercents;
            IsBackward = isBackward;
        }

        public override IEnumerator DebugExecute(CableWalkerApi cableWalkerApi)
        {

            if(Status != ConsoleCommandStatus.WaitingInLine)
            {
                yield break;
            }

            if(cableWalkerApi.WheelModule == null)
            {
                Status = ConsoleCommandStatus.OnDroneNoWheelModule;
                yield break;
            }
            Status = ConsoleCommandStatus.Running;
            if (Distance <= 0)
            {
                Status = ConsoleCommandStatus.DistanceArgumentMustBePositive;
                yield break;
            }

            if (cableWalkerApi.FlightModule.IsEnginesEnabled)
            {
                Status = ConsoleCommandStatus.NeedToTurnOffTheEngines;
                yield break;
            }

            if (cableWalkerApi.WheelModule.PassedDistance != 0)
                Distance += cableWalkerApi.WheelModule.PassedDistance;

            cableWalkerApi.WheelModule.StartMoving(Speed, IsForwardDirection);

            while (Distance - cableWalkerApi.WheelModule.PassedDistance > Precision)
                yield return null;

            cableWalkerApi.WheelModule.StopMoving();
            SetParams(cableWalkerApi);
            Status = ConsoleCommandStatus.Success;
        }

        public override Message GetMessageToSend()
        {
            if (IsBackward)
                SpeedInPercents = -SpeedInPercents;
            Dictionary<string, dynamic> cmd = new Dictionary<string, dynamic>()
            {
                ["cmd_id"] = Number,
                ["cmd"] = "cw.move|" + SpeedInPercents.ToString()
            };
            return new Message("cmd", cmd);
        }

        public override void SetParams(CableWalkerApi cableWalkerApi)
        {
            cableWalkerApi.SetNewCablePoint();
        }

       
    }
}

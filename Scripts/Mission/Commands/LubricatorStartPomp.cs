using CableWalker.Simulator.Modules;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CableWalker.Simulator.Mission.Commands
{
    public class LubricatorStartPomp : Command
    {
        public float Consumption { get; }
        

       
        public LubricatorStartPomp(float consumptionMlpm)
        {
            Consumption = consumptionMlpm;
            Status = ConsoleCommandStatus.WaitingInLine;
            var alias = CommandManager.GetDescriptor(this).Aliases.ToList()[0];
            Name = $"{alias}({consumptionMlpm.ToString()})";
        }

        public LubricatorStartPomp() : this(0.02f)
        {
           
        }

        public override IEnumerator DebugExecute(CableWalkerApi cableWalkerApi)
        {
            if (cableWalkerApi.LubricatorModule == null)
            {
                Status = ConsoleCommandStatus.OnDroneNoLubricatorModule;
                yield break;
            }
            if (cableWalkerApi.WheelModule == null)
            {
                Status = ConsoleCommandStatus.OnDroneNoWheelModule;
                yield break;
            }

            if (cableWalkerApi.WheelModule.Cable == null)
            {
                Status = ConsoleCommandStatus.NeedToSitOnCable;
                yield break;
            }

            if (!cableWalkerApi.LubricatorModule.IsClamped)
            {
                Status = ConsoleCommandStatus.NeedToClamp;
                yield break;
            }
            Status = ConsoleCommandStatus.Running;
            cableWalkerApi.LubricatorModule.StartPomp(Consumption);
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


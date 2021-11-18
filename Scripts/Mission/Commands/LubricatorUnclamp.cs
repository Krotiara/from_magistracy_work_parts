using CableWalker.Simulator.Modules;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CableWalker.Simulator.Mission.Commands
{
    public class LubricatorUnclamp : Command 
    {
        
       
        public LubricatorUnclamp()
        {
            Status = ConsoleCommandStatus.WaitingInLine;
            var alias = CommandManager.GetDescriptor(this).Aliases.First();
            Name = $"{alias}()";
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
            // TODO: Нужно дождаться, пока анимация закончится.
            Status = ConsoleCommandStatus.Running;
            cableWalkerApi.LubricatorModule.Unclamp();
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

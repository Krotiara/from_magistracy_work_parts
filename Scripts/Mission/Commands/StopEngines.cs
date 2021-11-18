using CableWalker.Simulator.Modules;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CableWalker.Simulator.Mission.Commands
{
	public class StopEngines : Command
	{
        

        public StopEngines()
        {
            Status = ConsoleCommandStatus.WaitingInLine;
            var alias = CommandManager.GetDescriptor(this).Aliases.First();
            Name = $"{alias}()";
        }

       
        public override IEnumerator DebugExecute(CableWalkerApi cableWalkerApi)
		{
            Status = ConsoleCommandStatus.Running;
            SetParams(cableWalkerApi);
            Status = ConsoleCommandStatus.Success;
			yield break;
		}


        public override Message GetMessageToSend()
        {
            throw new NotSupportedCommandForMessageException();
        }

        public override void SetParams(CableWalkerApi cableWalkerApi)
        {
            cableWalkerApi.FlightModule.Disable();
        }

        public new  IEnumerator Execute(CableWalkerApi cableWalkerApi, CableWalkerClient cableWalkerClient)
        {
            throw new System.NotImplementedException();
        }

        
    }
}

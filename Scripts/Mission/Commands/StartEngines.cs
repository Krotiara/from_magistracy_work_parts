using CableWalker.Simulator.Modules;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CableWalker.Simulator.Mission.Commands
{
	public class StartEngines : Command
	{
       
      

        public StartEngines()
        {
            Status = ConsoleCommandStatus.WaitingInLine;
            var alias = CommandManager.GetDescriptor(this).Aliases.First();
            Name = $"{alias}()";
        }

       

        public List<Command> GetSubCommands()
        {
            return new List<Command>();
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
            throw new System.NotImplementedException();
        }

        public override void SetParams(CableWalkerApi cableWalkerApi)
        {
            cableWalkerApi.FlightModule.Enable();
        }

        public new  IEnumerator Execute(CableWalkerApi cableWalkerApi, CableWalkerClient cableWalkerClient)
        {
            throw new System.NotImplementedException();
        }

       
    }
}

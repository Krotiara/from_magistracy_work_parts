using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CableWalker.Simulator.Modules;

namespace CableWalker.Simulator.Mission.Commands
{
    public class StopMagnetScanning : Command
    {

        public StopMagnetScanning()
        {
            Status = ConsoleCommandStatus.WaitingInLine;
            var alias = CommandManager.GetDescriptor(this).Aliases.First();
            Name = $"{alias}()";
        }

      

        public override IEnumerator DebugExecute(CableWalkerApi cableWalkerApi)
        {

            if (cableWalkerApi.WheelModule.Cable == null)
            {
                Status = ConsoleCommandStatus.NeedToSitOnCable;
                yield break;
            }
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
            cableWalkerApi.MagnetScannerModule.StopScanning();
        }

        public new IEnumerator Execute(CableWalkerApi cableWalkerApi, CableWalkerClient cableWalkerClient)
        {
            throw new System.NotImplementedException();
        }

     
    }
}

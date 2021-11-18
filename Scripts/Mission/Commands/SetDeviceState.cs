using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CableWalker.Simulator.Modules;
using UnityEngine;

namespace CableWalker.Simulator.Mission.Commands
{
    public class SetDeviceState : Command
    {
        private bool isEnable;
        private string deviceName;


        public SetDeviceState(string deviceName, bool isEnable)
        {
            Status = ConsoleCommandStatus.WaitingInLine;
            var alias = CommandManager.GetDescriptor(this).Aliases.First();
            Name = $"{alias}()";
            this.deviceName = deviceName;
            this.isEnable = isEnable;
        }

        public override Message GetMessageToSend()
        {
            string enableArg = isEnable ? ".enable" : ".disable";
            Dictionary<string, dynamic> cmd = new Dictionary<string, dynamic>()
            {
                ["cmd_id"] = Number,
                ["cmd"] = "cw.devices." + deviceName + enableArg
            };

            return new Message("cmd", cmd);
        }

       
        public override IEnumerator DebugExecute(CableWalkerApi cableWalkerApi)
        {
            SetParams(cableWalkerApi);
            yield break;
        }

        public override void SetParams(CableWalkerApi cableWalkerApi)
        {
            cableWalkerApi.SetModuleState(deviceName, isEnable);
        }

       
    }
}

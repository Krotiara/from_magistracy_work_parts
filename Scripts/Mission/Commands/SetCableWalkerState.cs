using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CableWalker.Simulator.Modules;
using UnityEngine;

namespace CableWalker.Simulator.Mission.Commands
{
    public class SetCableWalkerState : Command
    {
        private bool isOn;

        public SetCableWalkerState(bool isOn)
        {
            Status = ConsoleCommandStatus.WaitingInLine;
            var alias = CommandManager.GetDescriptor(this).Aliases.First();
            Name = $"{alias}()";
            this.isOn = isOn;
        }

        public override IEnumerator DebugExecute(CableWalkerApi cableWalkerApi)
        {
            SetParams(cableWalkerApi);
            yield break;
        }


        public override Message GetMessageToSend()
        {
            string enableArg = isOn ? ".enable" : ".disable";
            Dictionary<string, dynamic> cmd = new Dictionary<string, dynamic>()
            {
                ["cmd_id"] = Number,
                ["cmd"] = "cw" + enableArg
            };
            return new Message("cmd", cmd);
        }

        public override void SetParams(CableWalkerApi cableWalkerApi)
        {
            cableWalkerApi.SetCableWalkerOnFlag(isOn);
        }

    }
}

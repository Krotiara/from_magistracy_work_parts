using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CableWalker.Simulator.Modules;
using UnityEngine;

namespace CableWalker.Simulator.Mission.Commands
{
    public class SetRecorderFlag : Command
    {

        private bool isRecorder;

        public SetRecorderFlag(bool isRecorder)
        {
            Status = ConsoleCommandStatus.WaitingInLine;
            var alias = CommandManager.GetDescriptor(this).Aliases.First();
            Name = $"{alias}()";  
            this.isRecorder = isRecorder;
        }

        public override Message GetMessageToSend()
        {
            string enableArg = isRecorder ? ".enable" : ".disable";
            Dictionary<string, dynamic> cmd = new Dictionary<string, dynamic>()
            {
                ["cmd_id"] = Number,
                ["cmd"] = "cw.submodules.recorder" + enableArg
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
            cableWalkerApi.SetIsRecordingFlag(isRecorder);
        }

       
    }
}

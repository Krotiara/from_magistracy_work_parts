using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CableWalker.Simulator.Modules;
using UnityEngine;


namespace CableWalker.Simulator.Mission.Commands
{
    public class SetPhase : Command
    {
        private string phase;

        public SetPhase(string phase)
        {
            Status = ConsoleCommandStatus.WaitingInLine;
            var alias = CommandManager.GetDescriptor(this).Aliases.First();
            Name = $"{alias}()";
            //Добавить проверку через infoHolder

        }

        public override IEnumerator DebugExecute(CableWalkerApi cableWalkerApi)
        {
            throw new System.NotImplementedException();
        }

        
        public override Message GetMessageToSend()
        {
            Dictionary<string, dynamic> cmd = new Dictionary<string, dynamic>()
            {
                ["cmd_id"] = Number,
                ["cmd"] = "cw.set_param|" + $"phase,{phase}"
            };
            return new Message("cmd", cmd);
        }

        public override void SetParams(CableWalkerApi cableWalkerApi)
        {
            return;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CableWalker.Simulator.Model;
using CableWalker.Simulator.Modules;
using UnityEngine;

namespace CableWalker.Simulator.Mission.Commands
{
    public class VideoCameraGetVectorTo : Command
    {
        
        private InformationHolder infoHolder;
        public string InsulatorNumber { get; }
       
        public VideoCameraGetVectorTo(string number)
        {
            Status = ConsoleCommandStatus.WaitingInLine;
            var alias = CommandManager.GetDescriptor(this).Aliases.First();
            Name = $"{alias}()";
            infoHolder = GameObject.FindGameObjectWithTag("InfoHolder").GetComponent<InformationHolder>();
            InsulatorNumber = number;
        }

        public override Message GetMessageToSend()
        {
            throw new NotSupportedCommandForMessageException();
        }

        
  

        public override IEnumerator DebugExecute(CableWalkerApi cableWalkerApi)
        {
            Status = ConsoleCommandStatus.Running;
            var target = infoHolder.Get<InsulatorString>(InsulatorNumber);
            var vector = cableWalkerApi.VideoCameraModule.GetVectorTo(target.ObjectOnScene);
            Status = ConsoleCommandStatus.Success;
            yield break;

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

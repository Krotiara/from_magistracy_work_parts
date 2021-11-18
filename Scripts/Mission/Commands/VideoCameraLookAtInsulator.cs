using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CableWalker.Simulator.Model;
using CableWalker.Simulator.Modules;
using UnityEngine;

namespace CableWalker.Simulator.Mission.Commands
{
    public class VideoCameraLookAtInsulator : Command
    {
        public string Insulator { get; }
       

        private InformationHolder infoHolder;
        

        public VideoCameraLookAtInsulator(string name)
        {
            Insulator = name;
            infoHolder = GameObject.FindGameObjectWithTag("InfoHolder").GetComponent<InformationHolder>();
            Status = ConsoleCommandStatus.WaitingInLine;
            var alias = CommandManager.GetDescriptor(this).Aliases.First();
            Name = $"{alias}({name})";
        }

        public override IEnumerator DebugExecute(CableWalkerApi cableWalkerApi)
        {
            Status = ConsoleCommandStatus.Running;
            var target = infoHolder.Get<InsulatorString>(Insulator);
            cableWalkerApi.VideoCameraModule.LookAt(target.ObjectOnScene);
            while (cableWalkerApi.VideoCameraModule.InMotion)
                yield return null;
            Status = ConsoleCommandStatus.Success;
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

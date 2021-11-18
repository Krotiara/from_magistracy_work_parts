using CableWalker.Simulator.Model;
using CableWalker.Simulator.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace CableWalker.Simulator.Mission.Commands
{
    public class SetCable : Command
    {
        private string spanNumber;
        private string phaseNumber;
        private string cableNumber => spanNumber + "." + phaseNumber;
        private InformationHolder infoHolder;
        private Cable currentCable;


        public SetCable(string spanNumber, string phaseNumber)
        {
            this.spanNumber = spanNumber;
            this.phaseNumber = phaseNumber;
            infoHolder = GameObject.FindGameObjectWithTag("InfoHolder").GetComponent<InformationHolder>();
            Status = ConsoleCommandStatus.WaitingInLine;
            var alias = CommandManager.GetDescriptor(this).Aliases.First();
            Name = $"{alias}()";
            //Добавить проверку через infoHolder
            
            Span span = infoHolder.Get<Span>(spanNumber);
            if (span == null)
                throw new MissingMethodException();

            Cable cable = infoHolder.Get<Cable>(cableNumber);
            if(cable == null)
                throw new MissingMethodException();
            currentCable = cable;
            SubCommands = new List<Command> { new SetSpan(spanNumber), new SetPhase(phaseNumber) };
            CurSubCommandIndex = 0;


        }

        public override IEnumerator DebugExecute(CableWalkerApi cableWalkerApi)
        {
            throw new NotImplementedException();
        }

        public new IEnumerator Execute(CableWalkerApi cableWalkerApi, CableWalkerClient cableWalkerClient)
        {
            Status = ConsoleCommandStatus.Running;
            for(int i = 0; i < SubCommands.Count;i++)
            {
                CurSubCommandIndex = i;
                yield return SubCommands[i].Execute(cableWalkerApi, cableWalkerClient);
            }
            Status = ConsoleCommandStatus.Success;
            SetParams(cableWalkerApi);
            yield break;
        }

        public override void SetParams(CableWalkerApi cableWalkerApi)
        {
            cableWalkerApi.SetCurrentCable(currentCable);
        }

        public override Message GetMessageToSend()
        {
            return null;
        }
    }
}

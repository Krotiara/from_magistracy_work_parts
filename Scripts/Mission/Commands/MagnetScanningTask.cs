using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CableWalker.Simulator.Mission.Generator.Jobs;
using CableWalker.Simulator.Model;
using CableWalker.Simulator.Modules;
using UnityEngine;


namespace CableWalker.Simulator.Mission.Commands
{
    public class MagnetScanningTask : Command
    {

        private InformationHolder infoHolder;
        private Cable cable;
        
        private MagnetDiagnosticJob job;
        private List<Command> subCommands;
       
        

        public MagnetScanningTask(int firstTowerNumber, int secondTowerNumber, string phase)
            : this($"{firstTowerNumber}-{secondTowerNumber}.{phase}")
        {

        }

        public MagnetScanningTask(string firstTowerNumber, string secondTowerNumber, string phase)
            :this($"{firstTowerNumber}-{secondTowerNumber}.{phase}")
        {

        }

        public MagnetScanningTask(string cableNumber)
        {
            infoHolder = GameObject.FindGameObjectWithTag("InfoHolder").GetComponent<InformationHolder>();
            cable = infoHolder.Get<Cable>(cableNumber);
            Status = cable == null? ConsoleCommandStatus.IncorrectCableNumberArgument: ConsoleCommandStatus.WaitingInLine;
            if (cable != null)
            {
                job = new MagnetDiagnosticJob(cable);
                subCommands = job.Commands;
            }
            else subCommands = new List<Command>();
            var alias = CommandManager.GetDescriptor(this).Aliases.First();
            Name = $"{alias}(\"{cableNumber}\")";
        }

        public override IEnumerator DebugExecute(CableWalkerApi cableWalkerApi)
        {
            if (Status != ConsoleCommandStatus.WaitingInLine)
                yield break;

            if(cableWalkerApi.MagnetScannerModule == null)
            {
                Status = ConsoleCommandStatus.OnDroneNoMagnetScannerModule;
                yield break;
            }

            Status = ConsoleCommandStatus.Running;
            foreach (var command in subCommands)
                yield return command.DebugExecute(cableWalkerApi);
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

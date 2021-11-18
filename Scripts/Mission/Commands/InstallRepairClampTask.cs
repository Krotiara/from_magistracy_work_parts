using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CableWalker.Simulator.Mission.Generator.Jobs;
using CableWalker.Simulator.Model;
using CableWalker.Simulator.Modules;
using UnityEngine;


namespace CableWalker.Simulator.Mission.Commands
{
    public class InstallRepairClampTask : Command
    {

        private InformationHolder infoHolder;
        private Cable cable;
        private float distanceFromTower1;
       
       
        private InstallRepairClampJob job;
       
        

        public InstallRepairClampTask(int firstTowerNumber, int secondTowerNumber, string phase, float distanceFromTower1InSpan)
          : this($"{firstTowerNumber}-{secondTowerNumber}.{phase}", distanceFromTower1InSpan)
        {

        }

        public InstallRepairClampTask(string firstTowerNumber, string secondTowerNumber, string phase, float distanceFromTower1InSpan)
            :this($"{firstTowerNumber}-{secondTowerNumber}.{phase}", distanceFromTower1InSpan)
        {
     
        }

        public InstallRepairClampTask(string cableNumber, float distanceFromTower1InSpan)
        {
            infoHolder = GameObject.FindGameObjectWithTag("InfoHolder").GetComponent<InformationHolder>();
            cable = infoHolder.Get<Cable>(cableNumber);
            this.distanceFromTower1 = distanceFromTower1InSpan;
            Status = ConsoleCommandStatus.WaitingInLine;
            if(cable != null)
            {
                Status = ConsoleCommandStatus.WaitingInLine;
                job = new InstallRepairClampJob(cable, distanceFromTower1);
                

            }
            else
            {
                Status = ConsoleCommandStatus.IncorrectCableNumberArgument;
                
                job = null;
            }
            var alias = CommandManager.GetDescriptor(this).Aliases.First();
            Name = $"{alias}(\"{cableNumber}\",{distanceFromTower1})";
        }

        public override IEnumerator DebugExecute(CableWalkerApi cableWalkerApi)
        {
            if (Status != ConsoleCommandStatus.WaitingInLine)
                yield break;
            Status = ConsoleCommandStatus.Running;
            
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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CableWalker.Simulator.Model;
using CableWalker.Simulator.Modules;
using UnityEngine;

namespace CableWalker.Simulator.Mission.Commands
{
    

    public class FlyToTower : Command
    {
        public string TowerNumber { get; }
        public Vector3 TargetPoint { get; }
        

        private InformationHolder infoHolder;
       


        public FlyToTower(string towerNumber)
        {
            infoHolder = GameObject.FindGameObjectWithTag("InfoHolder").GetComponent<InformationHolder>();
            TowerNumber = towerNumber;
            Status = ConsoleCommandStatus.WaitingInLine;
            var alias = CommandManager.GetDescriptor(this).Aliases.ToList()[0];
            Name = $"{alias}(\"{towerNumber}\")";
        }

        public FlyToTower(int towerNumber):this(towerNumber.ToString())
        {
           
        }

        public override IEnumerator DebugExecute(CableWalkerApi cableWalkerApi)
        {
            if (!cableWalkerApi.FlightModule.IsEnginesEnabled)
            {
                Status = ConsoleCommandStatus.NeedToTurnOnTheEngines;
                yield break;
            }
            infoHolder.SetActiveObstacleColliders(true);
            Status = ConsoleCommandStatus.Running;
            var tower = infoHolder.Get<Tower>(TowerNumber);
            if (tower == null)
            {
                Status = ConsoleCommandStatus.IncorrectTowerNumberArgument;
                yield break;
            }
            var boxCollider = tower.ObjectOnScene.GetComponent<BoxCollider>();
            var point = tower.ObjectOnScene.transform.TransformPoint(boxCollider.center + new Vector3(0, boxCollider.size.y, 0) * 0.5f + new Vector3(0, 1, 0));
            var flyByPath = new FlyByPath(cableWalkerApi.CurrentPosition, point);
            yield return flyByPath.DebugExecute(cableWalkerApi);
            Status = flyByPath.Status == ConsoleCommandStatus.Success ? ConsoleCommandStatus.Success : flyByPath.Status;
            infoHolder.SetActiveObstacleColliders(false);
        }

        

        public override Message GetMessageToSend()
        {
            throw new NotSupportedCommandForMessageException();
        }

        public override void SetParams(CableWalkerApi cableWalkerApi)
        {
            return;
        }

        public new IEnumerator Execute(CableWalkerApi cableWalkerApi, CableWalkerClient cableWalkerClient)
        {
            throw new System.NotImplementedException();
        }

    }
}

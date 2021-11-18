using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CableWalker.Simulator.Model;
using CableWalker.Simulator.Modules;
using UnityEngine;

namespace CableWalker.Simulator.Mission.Commands
{
    public class SitOnCable : Command
    {
        private const float Precision = 0.01f;
        public string CableNumber { get; }
        public float DistanceFromTower1InSpan { get; }
        public bool IsForwardDirection { get; }
        
        private InformationHolder infoHolder;
        private int layerMask;
       
        private Cable cable;
        private Vector3 target;

        public SitOnCable(string firstTowerNumber, string secondTowerNumber, string phase, float distanceFromTower1InSpan, int directionInt): this($"{firstTowerNumber}-{secondTowerNumber}.{phase}", distanceFromTower1InSpan, directionInt)
        {
          
        }

        public SitOnCable(int firstTowerNumber, int secondTowerNumber, string phase, float distanceFromTower1InSpan, int directionInt) : this($"{firstTowerNumber}-{secondTowerNumber}.{phase}", distanceFromTower1InSpan, directionInt)
        {
            
        }

        public SitOnCable(string cableNumber, float distanceFromTower1InSpan, int directionInt)
        {
            
            IsForwardDirection = directionInt == 1;
            CableNumber = cableNumber;
            DistanceFromTower1InSpan = distanceFromTower1InSpan;
            infoHolder = GameObject.FindGameObjectWithTag("InfoHolder").GetComponent<InformationHolder>();
            layerMask = 1 << LayerMask.NameToLayer("PowerLineObjects")
           | 1 << LayerMask.NameToLayer("Borders")
           | 1 << LayerMask.NameToLayer("Ground")
           | 1 << LayerMask.NameToLayer("Objects");
            Status = ConsoleCommandStatus.WaitingInLine;
            var alias = CommandManager.GetDescriptor(this).Aliases.First();
            Name = $"{alias}(\"{cableNumber}\",{distanceFromTower1InSpan},{directionInt})";
            if (directionInt != 0 && directionInt != 1)
                Status = ConsoleCommandStatus.IncorrectDirectionIntArgument;
        }


        private Vector3 GetPointAboveTarget(Cable cable, Vector3 targetCablePoint, LandingType landingType)
        {
            float adjustmentCount = 0;
            Vector3 adjustmentVector = landingType == LandingType.Top ? Vector3.up : Vector3.down;
            var cableColliderSizeY = cable.ObjectOnScene.transform.Find("CableObstacle").GetComponent<BoxCollider>().size.y;
            //TODO Нужно добавить итоговую проверку рэйкастом
            while (Physics.OverlapSphere(targetCablePoint + adjustmentVector * adjustmentCount, 0.1f,layerMask).Length > 0 && adjustmentCount != cableColliderSizeY)
            {
                adjustmentCount++;
            }
            if (adjustmentCount == cableColliderSizeY)
                return Vector3.zero;
            return targetCablePoint + adjustmentVector * adjustmentCount;
        }

        public override IEnumerator DebugExecute(CableWalkerApi cableWalkerApi)
        {
            if(Status != ConsoleCommandStatus.WaitingInLine)
            {
                yield break;
            }
            Status = ConsoleCommandStatus.Running;
            if (!cableWalkerApi.FlightModule.IsEnginesEnabled)
            {
                Status = ConsoleCommandStatus.NeedToTurnOnTheEngines;
                yield break;
            }
            
            cable = infoHolder.Get<Cable>(CableNumber);
            if(cable == null)
            {
                Status = ConsoleCommandStatus.IncorrectCableNumberArgument;
                yield break;
            }
            infoHolder.SetActiveObstacleColliders(true);

            target = cable.GetPointByDistance(DistanceFromTower1InSpan);

            var pointAboveTarget = GetPointAboveTarget(cable, target, cableWalkerApi.DroneLandingType);
            if(pointAboveTarget.Equals(Vector3.zero))
            {
                Status = ConsoleCommandStatus.PointAboveCablePointsIsNotSafe;
                yield break;
                //TODO добавить попытку поиска ближайшей безопасной точки? Вывести в консоль дистанцию первой безопасной точки?
            }
            var landingPoint = cableWalkerApi.transform.Find("LandingPoint");
            var flyByPath = new FlyByPath(cableWalkerApi.transform.position, pointAboveTarget);
            yield return flyByPath.DebugExecute(cableWalkerApi);
            if(flyByPath.Status != ConsoleCommandStatus.Success)
            {
                Status = flyByPath.Status;
                yield break;
            }
            Vector3 direction = IsForwardDirection ? cable.End.CablePoint.position - cable.Start.CablePoint.position : cable.Start.CablePoint.position - cable.End.CablePoint.position;
            yield return new RotateByDirection(direction).DebugExecute(cableWalkerApi);
            ;
               // yield return new FlyTo(landingPoint.position.x, landingPoint.position.y, landingPoint.position.z).DebugRun(cableWalkerApi);
            //yield return new FlyByPath(cableWalkerApi.transform.position, landingPoint.position).DebugRun(cableWalkerApi);    
            var adjustment = cableWalkerApi.DroneLandingType == LandingType.Top ? -1 : 1;
            var deltaY = Mathf.Abs(landingPoint.position.y - target.y);
            while (deltaY > Precision)
            {
                //
                if(deltaY < 0.9f)
                    cableWalkerApi.transform.position = Vector3.MoveTowards(cableWalkerApi.transform.position, target + new Vector3(0, adjustment, 0), deltaY * Time.deltaTime);
                else
                    cableWalkerApi.transform.position = Vector3.MoveTowards(cableWalkerApi.transform.position, target + new Vector3(0, adjustment,0), 0.9f * Time.deltaTime);
                deltaY = Mathf.Abs(landingPoint.position.y - target.y);
                yield return null;
            }

            cableWalkerApi.transform.position = new Vector3(target.x + landingPoint.localPosition.x, cableWalkerApi.transform.position.y, target.z + landingPoint.localPosition.z);

            
            Status = ConsoleCommandStatus.Success;
            infoHolder.SetActiveObstacleColliders(false);
           
            
        }

        

        public override Message GetMessageToSend()
        {
            throw new NotSupportedCommandForMessageException();
        }

        public override void SetParams(CableWalkerApi cableWalkerApi)
        {
            if (cableWalkerApi.WheelModule != null)
                cableWalkerApi.WheelModule.SetCable(cable, cable.GetTByPoint(target));
            cableWalkerApi.SetCable(cable, target);
        }

        public new  IEnumerator Execute(CableWalkerApi cableWalkerApi, CableWalkerClient cableWalkerClient)
        {
            throw new System.NotImplementedException();
        }

        
    }
}

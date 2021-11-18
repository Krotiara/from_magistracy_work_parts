using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CableWalker.Simulator.Model;
using CableWalker.Simulator.Modules;
using UnityEngine;



namespace CableWalker.Simulator.Mission.Commands
{
    public class TakeOffCable : Command
    {
        private const float Precision = 0.01f;
        private int layerMask;
       
        private InformationHolder infoHolder;

        

        public TakeOffCable()
        {
            layerMask = 1 << LayerMask.NameToLayer("PowerLineObjects")
            | 1 << LayerMask.NameToLayer("Borders")
            | 1 << LayerMask.NameToLayer("Ground")
            | 1 << LayerMask.NameToLayer("Objects");
            Status = ConsoleCommandStatus.WaitingInLine;
            var alias = CommandManager.GetDescriptor(this).Aliases.First();
            Name = $"{alias}()";
            infoHolder = GameObject.FindGameObjectWithTag("InfoHolder").GetComponent<InformationHolder>();
        }

       

        public List<Command> GetSubCommands()
        {
            return new List<Command>();
        }

        public override IEnumerator DebugExecute(CableWalkerApi cableWalkerApi)
        {
            Status = ConsoleCommandStatus.Running;
            if (!cableWalkerApi.FlightModule.IsEnginesEnabled)
            {
                Status = ConsoleCommandStatus.NeedToTurnOnTheEngines;
                yield break;
            }
            infoHolder.SetActiveObstacleColliders(true);
            var target = GetSavedTarget(cableWalkerApi.transform.position, cableWalkerApi.DroneLandingType, cableWalkerApi.CurrentCable.Number);
            infoHolder.SetActiveObstacleColliders(false);
            if (target.Equals(Vector3.zero))
            {
                Status = ConsoleCommandStatus.TakeOffIsNotSafe;
                yield break;
            }

            float deltaY = Mathf.Abs(target.y - cableWalkerApi.transform.position.y);

            while (deltaY > Precision)
            {
                //
                if (deltaY < 0.9f)
                    cableWalkerApi.transform.position = Vector3.MoveTowards(cableWalkerApi.transform.position, target, deltaY * Time.deltaTime);
                else
                    cableWalkerApi.transform.position = Vector3.MoveTowards(cableWalkerApi.transform.position, target, 0.9f * Time.deltaTime);
                deltaY = deltaY = Mathf.Abs(target.y - cableWalkerApi.transform.position.y);
                yield return null;
            }

            SetParams(cableWalkerApi);
            Status = ConsoleCommandStatus.Success;
        }

        private Vector3 GetSavedTarget(Vector3 copterPosition, LandingType landingType, string cableName)
        {
            float metersCount = 0;
            float precision = 0.1f;
            float maxMetersDistance = 10;
            Vector3 adjustmentVector = landingType == LandingType.Top ? Vector3.up : Vector3.down;
            while (Physics.OverlapSphere(copterPosition + adjustmentVector * metersCount, 0.1f, layerMask).Length > 0 && metersCount != maxMetersDistance)
            {
                metersCount++;
            }
            if (metersCount == maxMetersDistance)
                return Vector3.zero;

            var position = copterPosition + adjustmentVector * (metersCount + precision);
            var hits = Physics.RaycastAll(position, -adjustmentVector, (metersCount + precision), layerMask);
            foreach(var hit in hits)
            {
                GameObject hitObj = hit.transform.gameObject;
                if(hit.transform.gameObject.tag == "ObstacleBoxCollider")
                {
                    hitObj = hit.transform.parent.gameObject;
                }
                var indexHolder = hitObj.GetComponent<IndexHolder>();

                if (indexHolder == null || indexHolder.name != cableName)
                    return Vector3.zero;
            }
            return position;
        }

       

        public override Message GetMessageToSend()
        {
            throw new NotSupportedCommandForMessageException();
        }

        public override void SetParams(CableWalkerApi cableWalkerApi)
        {
            if (cableWalkerApi.WheelModule != null)
                cableWalkerApi.WheelModule.DropCable();
            cableWalkerApi.DropCable();
        }

        public new  IEnumerator Execute(CableWalkerApi cableWalkerApi, CableWalkerClient cableWalkerClient)
        {
            throw new System.NotImplementedException();
        }

       
    }
}

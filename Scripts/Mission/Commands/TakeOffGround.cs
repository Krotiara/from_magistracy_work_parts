using CableWalker.Simulator.Model;
using CableWalker.Simulator.Modules;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CableWalker.Simulator.Mission.Commands
{
    public class TakeOffGround : Command
    {
        private const float Precision = 0.01f;
        private int layerMask;
        private InformationHolder infoHolder;

        public float DistanceUp { get; private set; }
       

        public TakeOffGround()
        {
            layerMask = 1 << LayerMask.NameToLayer("PowerLineObjects")
            | 1 << LayerMask.NameToLayer("Borders")
            | 1 << LayerMask.NameToLayer("Ground")
            | 1 << LayerMask.NameToLayer("Objects");
            Status = ConsoleCommandStatus.WaitingInLine;
            var alias = CommandManager.GetDescriptor(this).Aliases.ToList()[0];
            Name = $"{alias}()";
            infoHolder = GameObject.FindGameObjectWithTag("InfoHolder").GetComponent<InformationHolder>();
        }

        public override IEnumerator DebugExecute(CableWalkerApi cableWalkerApi)
        {
            Status = ConsoleCommandStatus.Running;
            infoHolder.SetActiveObstacleColliders(true);
            var target = GetSavedTarget(cableWalkerApi.transform.position);
            infoHolder.SetActiveObstacleColliders(false);
            if (target.Equals(Vector3.zero))
            {
                Status = ConsoleCommandStatus.TakeOffIsNotSafe;
                yield break;
            }
            if (!cableWalkerApi.FlightModule.IsEnginesEnabled)
            {
                Status = ConsoleCommandStatus.NeedToTurnOnTheEngines;
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
            Status = ConsoleCommandStatus.Success;
        }

        private Vector3 GetSavedTarget(Vector3 copterPosition)
        {
            float upperCount = 0;
            float maxUpDistance = 10;
            float precision = 0.1f;
            while (Physics.OverlapSphere(copterPosition + Vector3.up * upperCount, 0.1f, layerMask).Length > 0 && upperCount != maxUpDistance)
            {
                upperCount++;
            }
            if (upperCount == maxUpDistance)
                return Vector3.zero;
            var position = copterPosition + Vector3.up * (upperCount+precision);

            var hits = Physics.RaycastAll(position, Vector3.down, layerMask);
            foreach (var hit in hits)
                if (hit.transform.gameObject.tag != "Ground")
                    return Vector3.zero;

            DistanceUp = upperCount;
            return position;
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

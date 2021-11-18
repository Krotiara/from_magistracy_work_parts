using CableWalker.Simulator.Mission.Commands;
using CableWalker.Simulator.Modules;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace CableWalker.Simulator.Mission.Commands
{
    public class FlyByPath : Command
    {
        public Vector3 Start { get; }
        public Vector3 Target { get; }
        

        public FlyByPath(Vector3 start, Vector3 target)
        {
            Start = start;
            Target = target;
            Status = ConsoleCommandStatus.WaitingInLine;
            var alias = CommandManager.GetDescriptor(this).Aliases.ToList()[0];
            Name = $"{alias}({start.ToString()},{target.ToString()})";
        }


        public override IEnumerator DebugExecute(CableWalkerApi cableWalkerApi)
        {
            Status = ConsoleCommandStatus.Running;
            var findPath = new FindPath(Start, Target);
            yield return findPath.DebugExecute(cableWalkerApi);
            var path = findPath.PathFinder.Solution;
            if(path == null)
            {
                Status = ConsoleCommandStatus.PathNotFound;
                yield break;
            }
            if (Mathf.Abs(cableWalkerApi.CurrentPosition.x - Target.x) > 0.5f || Mathf.Abs(cableWalkerApi.CurrentPosition.y - Target.y) > 0.5f || Mathf.Abs(cableWalkerApi.CurrentPosition.y - Target.y) > 0.5f)
                yield return new RotateTo(Target.x, Target.y, Target.z).DebugExecute(cableWalkerApi);
            foreach (var point in path)
            {
                //if(Mathf.Abs(cableWalkerApi.CurrentPosition.x - point.x) > 0.5f || Mathf.Abs(cableWalkerApi.CurrentPosition.y - point.y) > 0.5f || Mathf.Abs(cableWalkerApi.CurrentPosition.y - point.y)>0.5f)
                //    yield return new RotateTo(point.x, point.y, point.z).DebugRun(cableWalkerApi);
                yield return new FlyTo(point.x, point.y, point.z).DebugExecute(cableWalkerApi);
            }
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

        public new IEnumerator Execute(CableWalkerApi cableWalkerApi, CableWalkerClient cableWalkerClient)
        {
            throw new System.NotImplementedException();
        }

    }
}

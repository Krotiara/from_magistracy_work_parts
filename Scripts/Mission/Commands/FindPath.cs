using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CableWalker.Simulator.Modules;
using CableWalker.Simulator.PathFinding;
using UnityEngine;


namespace CableWalker.Simulator.Mission.Commands
{
    public class FindPath : Command
    {
        Vector3 StartPoint { get; }
        Vector3 EndPoint { get; }
        private VisibilityGraphBuilder visibilityGraphBuilder;
        public PathFinder PathFinder { get; }

        private InformationHolder infoHolder;
        
       

        public FindPath(Vector3 start, Vector3 target)
        {
            StartPoint = start;
            EndPoint = target;
            PathFinder = new PathFinder(new AStar<BinaryHeap<Vertex>>());
            infoHolder = GameObject.FindGameObjectWithTag("InfoHolder").GetComponent<InformationHolder>();
            Status = ConsoleCommandStatus.WaitingInLine;
            var alias = CommandManager.GetDescriptor(this).Aliases.First();
            Name = $"{alias}({start.ToString()},{target.ToString()})";
        }

        public override IEnumerator DebugExecute(CableWalkerApi cableWalkerApi)
        {
            infoHolder.SetActiveObstacleColliders(true);
            Status = ConsoleCommandStatus.Running;
            yield return PathFinder.FindPath(StartPoint, EndPoint);
            Status = PathFinder.Solution == null? ConsoleCommandStatus.PathNotFound: ConsoleCommandStatus.Success;
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
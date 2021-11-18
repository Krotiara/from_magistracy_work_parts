using System.Collections.Generic;
using CableWalker.Simulator.Mission.Commands;
using UnityEngine;

namespace CableWalker.Simulator.Mission.Generator
{
    public class GenerationResult
    {
        public GenerationStatus Status { get; }
        public IEnumerable<Command> CommandList { get; }
        public (Vector3, Vector3)? BadPath { get; }
        
        public GenerationResult(GenerationStatus status, IEnumerable<Command> commandList, (Vector3, Vector3)? badPath)
        {
            Status = status;
            CommandList = commandList;
            BadPath = badPath;
        }
    }

    public enum GenerationStatus
    {
        Success,
        PathNotFound,
        CombinationNotFound
    }
}

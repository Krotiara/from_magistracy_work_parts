using System.Collections.Generic;
using CableWalker.Simulator.Mission.Commands;
using CableWalker.Simulator.Mission.Generator.Jobs;
using UnityEngine;

namespace CableWalker.Simulator.Mission.Generator
{
    public class RepairProcedureGenerator
    {
        public IEnumerable<Command> Generate(InstallRepairClampJob job)
        {
            var commands = new List<Command>();

            var currentCable = job.Target;
            var currentSpanCenter = currentCable.Start.Position + (currentCable.End.Position - currentCable.Start.Position) / 2;
            
            commands.Add(new SetStart(new Vector2(currentSpanCenter.x, currentSpanCenter.z)));
            commands.Add(new StartEngines());
            commands.Add(new TakeOffGround());
            commands.Add(new FlyTo(currentSpanCenter.x, TerrainUtils.GetSampleHeight(currentSpanCenter) + 5, currentSpanCenter.z));
            //commands.Add(new SitOnCable(int.Parse(currentCable.Start.Tower.Number), int.Parse(currentCable.Start.Tower.Number) + 1, currentCable.Phase, job.Point));
            commands.Add(new StopEngines());
            
            //commands.Add(new InstallRepairClamp());

            commands.Add(new StartEngines());
            commands.Add(new TakeOffGround());
            commands.Add(new FlyTo(currentSpanCenter.x, TerrainUtils.GetSampleHeight(currentSpanCenter) + 5, currentSpanCenter.z));
            commands.Add(new FlyTo(currentSpanCenter.x, TerrainUtils.GetSampleHeight(currentSpanCenter) + 0.2f, currentSpanCenter.z));
            commands.Add(new StopEngines());
                

            return commands;
        }
    }
}

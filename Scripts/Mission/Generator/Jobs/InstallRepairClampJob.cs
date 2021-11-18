using System.Collections.Generic;
using CableWalker.Simulator.Mission.Commands;
using CableWalker.Simulator.Model;
using UnityEngine;

namespace CableWalker.Simulator.Mission.Generator.Jobs
{
    /// <summary>
    /// Задание на установку ремонтного зажима.
    /// </summary>
    public class InstallRepairClampJob : IAtomicJob<Cable>
    {
        public Cable Target { get; }
        public float DistancefromTower1InSpan { get; }

        public List<Command> Commands => GetCommands();


        public InstallRepairClampJob(Cable target, float distancefromTower1InSpan)
        {
            Target = target;
            DistancefromTower1InSpan = distancefromTower1InSpan;
        }

        private List<Command> GetCommands()
        {
            var result = new List<Command>();
            result.Add(new SitOnCable(Target.Span.FirstTower.Number, Target.Span.SecondTower.Number, Target.Phase, DistancefromTower1InSpan,1));
            result.Add(new InstallRepairClamp());
            result.Add(new Wait(1));
            result.Add(new StartEngines());
            result.Add(new Wait(1));
            result.Add(new TakeOffCable());
            return result;
        }


    }
}

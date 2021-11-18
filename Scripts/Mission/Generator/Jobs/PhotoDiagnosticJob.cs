using System.Collections.Generic;
using CableWalker.Simulator.Mission.Commands;

namespace CableWalker.Simulator.Mission.Generator.Jobs
{
    /// <summary>
    /// Задание на фотографирование объекта.
    /// </summary>
    public class PhotoDiagnosticJob : IAtomicJob<Model.Model>
    {
        public Model.Model Target { get; }
        public float PolarAngle { get; }
        public float ZenithAngle { get; }

        public List<Command> Commands => throw new System.NotImplementedException(); //TODO

        public PhotoDiagnosticJob(Model.Model target, float polarAngle, float zenithAngle)
        {
            Target = target;
            PolarAngle = polarAngle;
            ZenithAngle = zenithAngle;
        }

    }
}

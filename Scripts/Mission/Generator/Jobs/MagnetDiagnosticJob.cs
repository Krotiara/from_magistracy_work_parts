using System.Collections.Generic;
using CableWalker.Simulator.Mission.Commands;
using CableWalker.Simulator.Model;

namespace CableWalker.Simulator.Mission.Generator.Jobs
{
    /// <summary>
    /// Задание на выполнение магнитной диагностики кабеля.
    /// </summary>
    public class MagnetDiagnosticJob : IAtomicJob<Cable>
    {
        public Cable Target { get; }

        public List<Command> Commands => GetCommands();

        public MagnetDiagnosticJob(Cable target)
        {
            Target = target;
        }

        private List<Command> GetCommands()
        {
            var offset = 10;
            var result = new List<Command>();
            result.Add(new SitOnCable(Target.Span.FirstTower.Number, Target.Span.SecondTower.Number, Target.Phase, offset,1)); ///10 надо заменить на первую точку безопасной зоны провода
            result.Add(new Wait(1));
            result.Add(new StartMagnetScanning());
            result.Add(new Wait(1));
            result.Add(new WheelModuleMoveTo(Target.SpanLength - offset, 1)); ///100 надо заменить на последнюю точку безопасной зоны провода
            result.Add(new Wait(1));
            result.Add(new StopMagnetScanning());
            result.Add(new Wait(1));
            result.Add(new StartEngines());
            result.Add(new Wait(1));
            result.Add(new TakeOffCable()); //TODO добавить возможность взлета и авто расчет взлета до безопасной зоны. Если нельзя взлететь. найти точку на проводе, где можно взлететь.
            return result;

        }
    }
}

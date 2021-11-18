using System.Collections.Generic;
using System.Linq;
using CableWalker.Simulator.Mission.Commands;
using CableWalker.Simulator.Model;
using UnityEngine;

namespace CableWalker.Simulator.Mission
{
    /// <summary>
    /// Позволяет определить состояние системы ВЛ-Канатоход в любой момент выполнения миссии без необходимости её запуска.
    /// 25.02 - не работает, нужны правки или вообще удалить, так как сейчас отслеживается состояние канатохода в отдельном классе
    /// </summary>
    public class MissionTimeline
    {
        private Command[] commands;
    
        public MissionTimeline(IEnumerable<Command> mission)
        {
            commands = mission.ToArray();
        }

        // TODO-nexusasx10: сделать определение положения непрерывным от прогресса выполнения миссии.
        /// <summary>
        /// Получить положение Канатохода в момент начала выполнения команды.
        /// </summary>
        /// <param name="commandNumber">Номер команды</param>
        /// <returns></returns>
        public IEnumerable<Vector3> GetPositions(int commandNumber)
        {
            var positions = new List<Vector3>();
            Cable cable = null;
            var position = Vector3.zero;

            var infoHolder = GameObject.FindGameObjectWithTag("InfoHolder").GetComponent<InformationHolder>();

            for (var i = 0; i <= commandNumber; i++)
            {
                var command = commands[i];

                if (command is SetStart setStart)
                    position = new Vector3(setStart.X, TerrainUtils.GetSampleHeight(new Vector3(setStart.X, 0, setStart.Z)), setStart.Z);
                else if (command is FlyTo flyTo)
                    position = new Vector3(flyTo.X, flyTo.Y, flyTo.Z);
                else if (command is TakeOffGround takeOff)
                {
                    cable = null;
                    position += Vector3.up * takeOff.DistanceUp;
                }
                else if (command is SitOnCable sitOnCable)
                {
                    cable = infoHolder.Get<Cable>(sitOnCable.CableNumber);
                    //position = cable.GetPointByDistance(sitOnCable.Distance);
                }
                else if (command is WheelModuleMove wheelModuleMove)
                {
                    // TODO-nexusasx10: Работает только для поездки по всему кабелю.
                    if (i == commandNumber)
                        for (var t = 0.0f; t < 1.0f; t += 0.01f)
                            positions.Add(cable.GetPoint(t));
                    position = cable.GetPoint(1);
                }
            }
            
            positions.Add(position);

            return positions;
        }
    }
}

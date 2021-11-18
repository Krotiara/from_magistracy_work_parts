using CableWalker.Simulator.Mission.Generator.Jobs;
using CableWalker.Simulator.Model;
using TMPro;
using UnityEngine;

namespace CableWalker.Simulator.Mission.Generator
{
    public class RepairProcedureGeneratorComponent : MonoBehaviour
    {
        public TMP_InputField OutputField;

        public void OnButtonPress(Model.Model model)
        {
            var deffect = (CableDefect)model;
            var cable = (Cable)deffect.Model;
            var point = cable.GetPointByDistance(deffect.DistanceFromTower1);
            var nearestPointIndex = -1;
            for (var i = 0; i < cable.Points.Count; i++)
                if (nearestPointIndex == -1 || Vector3.Distance(cable.Points[i], point) < Vector3.Distance(cable.Points[nearestPointIndex], point))
                    nearestPointIndex = i;
            var t = cable.GetTByIndex(nearestPointIndex);
            var job = new InstallRepairClampJob(cable, t);
            var commands = new RepairProcedureGenerator().Generate(job);
            OutputField.text = commands.Format();
            FindObjectOfType<MissionVisualizer>().Visualize();
        }
    }
}

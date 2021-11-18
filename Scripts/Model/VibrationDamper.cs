using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CableWalker.Simulator.Model
{
    public class VibrationDamper : Model
    {
        public string LocalNumber => Number.Split('d').Last();
        public string TextNumber => string.Format("Vibration damper # {0} on cable {1}", LocalNumber, Cable.Number);

        public VibrationDamper()
        {

        }

        public VibrationDamper(string number, string name,Cable cable)
        {
            Number = number;
            Name = name;
            Cable = cable;
        }

        public Cable Cable { get; }
        

        public override void CalculateCondition()
        {
            throw new System.NotImplementedException();
        }

        public override Model Create(List<string> args, List<string> typeArgs, InformationHolder infoHolder, bool isEditorMode)
        {
            throw new System.NotImplementedException();
        }

        public override List<string> GetCellsNamesToTable()
        {
            throw new System.NotImplementedException();
        }

        public override List<(string, string)> GetInfo()
        {
            throw new System.NotImplementedException();
        }

        public override List<string> GetInfoForTable()
        {
            throw new System.NotImplementedException();
        }

        public override GameObject Instantiate()
        {
            throw new System.NotImplementedException();
        }

        public override GameObject Instantiate2D()
        {
            throw new System.NotImplementedException();
        }

        public override void SetObjectOnSceneParams()
        {
            return;
        }
    }
}

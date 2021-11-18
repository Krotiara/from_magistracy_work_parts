using System.Collections.Generic;

namespace CableWalker.Simulator.Model
{
    public abstract class Defect : Model
    {
        public  string Description { get; set; }

        public  string DescriptionByType { get; set; }
       

        public string TypeFromDataBase { get; set; }

        public List<string> ArgsFromFiles { get; set; } = new List<string>();

      //  public abstract List<string> GetCellsNamesToTable();

      //  public abstract List<string> GetInfoForTable();

        /// <summary>
        /// Объект, к которому относится дефект.
        /// </summary>
        public  Model Model { get; set; }
        public float Criticality = 1;
    }
}


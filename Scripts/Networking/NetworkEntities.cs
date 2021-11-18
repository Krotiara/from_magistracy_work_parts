using System;
using System.Collections.Generic;

namespace CableWalker.Simulator.Networking
{
    [Serializable]
    public class User
    {
        public string token;
        public string user_role;
    }

    [Serializable]
    public class DefectType
    {
        public int Number;
        public string rusDescription;
        public string engDescription;
    }

    [Serializable]
    public class Defect
    {
        public string TowerNumber;
        public int Type;
        public string Description;
        public string PhotoPath;
    }

    [Serializable]
    public class DefectsList
    {
        public List<DefectType> DefectTypes;
        public List<Defect> Defects;
    }
}

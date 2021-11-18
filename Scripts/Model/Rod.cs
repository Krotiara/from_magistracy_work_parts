using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CableWalker.Simulator.Model
{
    public class Rod
    {
        public int Number { get; }
        public float DistanceFromTower1 { get; }
        /// <summary>
        /// Высота относительно низжей точки провода
        /// </summary>
        public float Height { get; }
        public Simulator.RodType Type { get; }
       

       public Rod(int number, float distanceFromTower1, float height, Simulator.RodType type)
        {
            Number = number;
            DistanceFromTower1 = distanceFromTower1;
            Height = height;
            Type = type;
        }

        public override bool Equals(object obj)
        {
            return obj is Rod rod &&
                   Number == rod.Number &&
                   DistanceFromTower1 == rod.DistanceFromTower1 &&
                   Type == rod.Type;
        }

        public override int GetHashCode()
        {
            int hashCode = -457024762;
            hashCode = hashCode * -1521134295 + Number.GetHashCode();
            hashCode = hashCode * -1521134295 + DistanceFromTower1.GetHashCode();
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            return hashCode;
        }
    }
}

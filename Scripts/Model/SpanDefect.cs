using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CableWalker.Simulator.Model
{
    public class SpanDefect : Defect
    {

        //public float ActualValue { get; set; }
        public string DescriptionByTypeRus { get; set; }
        public Tower Tower1 { get; set; }
        public Tower Tower2 { get; set; }

        public SpanDefect()
        {

        }

        public SpanDefect(Tower first, Tower second, string number, string typeNumberFromDataBase, string descriptionByTypeRus, string descriptionByTypeEn, List<string> args)
        {
            Number = number;
            Tower1 = first;
            Tower2 = second;
            
            TypeFromDataBase = typeNumberFromDataBase;
            DescriptionByTypeRus = descriptionByTypeRus;
            DescriptionByType = descriptionByTypeEn;
            ArgsFromFiles = args;
           

        }

       

        public override void CalculateCondition()
        {
            throw new System.NotImplementedException();
        }

        public override Model Create(List<string> args, List<string> typeArgs, InformationHolder infoHolder, bool isEditorMode)
        {
            string number = args[0];
            string firstTowerNum = args[1];
            string secondTowerNum = args[2];
          
            string descriptionByTypeRus = typeArgs[0];
            string descriptionByTypeEn = typeArgs[1];
            string typeNumberFromDataBase = typeArgs[2];
            Tower firstTower = infoHolder.Get<Tower>(firstTowerNum);
            Tower secondTower = infoHolder.Get<Tower>(secondTowerNum);
            if (firstTower == null || secondTower == null)
               
                throw new Exception("Incorrect SpanDefects config. Towers numbers are wrong");
           
            return new SpanDefect(firstTower,secondTower, number, typeNumberFromDataBase, descriptionByTypeRus, descriptionByTypeEn, args.Skip(3).ToList()); 


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
            return null;
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

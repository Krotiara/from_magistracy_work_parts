using CableWalker.Simulator;
using CableWalker.Simulator.Mission.Commands;
using CableWalker.Simulator.Model;
using CableWalker.Simulator.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace CableWalker.Simulator.Mission.Commands
{
    public class SetSpan : Command
    {
       
        private string firstTower;
        private string secondTower;

        private InformationHolder infoHolder;

        private Span currentSpan;

        
        public SetSpan(string spanNumber): this(spanNumber.Split('-')[0], spanNumber.Split('-')[1])
        {
        }

        public SetSpan(string firstTower, string secondTower)
        {
            infoHolder = GameObject.FindGameObjectWithTag("InfoHolder").GetComponent<InformationHolder>();
            Status = ConsoleCommandStatus.WaitingInLine;
            var alias = CommandManager.GetDescriptor(this).Aliases.First();
            Name = $"{alias}()";
            //Добавить проверку через infoHolder
            this.firstTower = firstTower;
            this.secondTower = secondTower;

            Tower tower1 = infoHolder.Get<Tower>(firstTower);
            Tower tower2 = infoHolder.Get<Tower>(secondTower);
            if (tower1 == null || tower2 == null)
                throw new MissingMethodException();
            Span span = infoHolder.Get<Span>($"{firstTower}-{secondTower}");
            if (span == null)
                throw new MissingMethodException();
            currentSpan = span;
        }

       

        public override IEnumerator DebugExecute(CableWalkerApi cableWalkerApi)
        {
            SetParams(cableWalkerApi);
            yield break;
        }

       

        public override Message GetMessageToSend()
        {
            Dictionary<string, dynamic> cmd = new Dictionary<string, dynamic>()
            {
                ["cmd_id"] = Number,
                ["cmd"] = "cw.set_param|" + $"span,{firstTower},{secondTower}" 
            };
            return new Message("cmd", cmd);
        }

        

        public override void SetParams(CableWalkerApi cableWalkerApi)
        {
            cableWalkerApi.SetCurrentSpan(currentSpan);
        }

    }
}

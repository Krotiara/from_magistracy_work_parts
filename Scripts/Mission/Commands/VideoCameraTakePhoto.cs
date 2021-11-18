using CableWalker.Simulator.Modules;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CableWalker.Simulator.Mission.Commands
{
    public class VideoCameraTakePhoto : Command
    {
        private const float Delay = 2.0f;
      

       

        public VideoCameraTakePhoto()
        {
            Status = ConsoleCommandStatus.WaitingInLine;
            var alias = CommandManager.GetDescriptor(this).Aliases.First();
            Name = $"{alias}()";
        }

        

       

        public override IEnumerator DebugExecute(CableWalkerApi cableWalkerApi)
        {
            Status = ConsoleCommandStatus.Running;
            cableWalkerApi.VideoCameraModule.TakePhoto();
            Status = ConsoleCommandStatus.Success;
            yield return new WaitForSecondsRealtime(Delay);
        }

       

        public override Message GetMessageToSend()
        {
            throw new NotSupportedCommandForMessageException();
        }

        public override void SetParams(CableWalkerApi cableWalkerApi)
        {
            return;
        }

        public new  IEnumerator Execute(CableWalkerApi cableWalkerApi, CableWalkerClient cableWalkerClient)
        {
            throw new System.NotImplementedException();
        }

       
    }
}

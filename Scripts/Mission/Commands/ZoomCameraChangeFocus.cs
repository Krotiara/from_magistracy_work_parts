using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CableWalker.Simulator.Modules;
using UnityEngine;


namespace CableWalker.Simulator.Mission.Commands
{
    public class ZoomCameraChangeFocus : Command
    {

        private float value;

        public ZoomCameraChangeFocus(float value)
        {
            var alias = CommandManager.GetDescriptor(this).Aliases.First();
            Name = $"{alias}()";
            this.value = value;
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
                ["cmd"] = $"cw.devices.zoomcamera.change_focus|{value}"
            };
            return new Message("cmd", cmd);
        }

        public override void SetParams(CableWalkerApi cableWalkerApi)
        {
            cableWalkerApi.VideoCameraModule.Focus += value;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CableWalker.Simulator.Model;
using CableWalker.Simulator.Modules;
using UnityEngine;

namespace CableWalker.Simulator.Mission.Commands
{
    public class InstallRepairClamp : Command
    {

        private InformationHolder infoHolder;
        public Cable Cable { get; set; }
        public Vector3 RepairClampPosition { get; set; }
        public Vector3 RepairClampRotation { get; set; }

        public RepairClampDevice RepairClampDeviceApi { get; private set; }
       

       

        public InstallRepairClamp()
        {
            infoHolder = GameObject.FindGameObjectWithTag("InfoHolder").GetComponent<InformationHolder>();
            Status = ConsoleCommandStatus.WaitingInLine;
            var alias = CommandManager.GetDescriptor(this).Aliases.ToList()[0];
            Name = $"{alias}()";
        }

        public override IEnumerator DebugExecute(CableWalkerApi cableWalkerApi)
        {

            Status = ConsoleCommandStatus.Running;
            Cable = cableWalkerApi.CurrentCable;
            if(Cable == null)
            {
                Status = ConsoleCommandStatus.NeedToSitOnCable;
                yield break;
            }
            RepairClampPosition = Cable.GetPoint(cableWalkerApi.CurrentCablePointT);
            //канатоход уже повернут параллельно проводу
            RepairClampRotation = cableWalkerApi.transform.rotation.eulerAngles;
            RepairClampDeviceApi = cableWalkerApi.transform.GetComponentInChildren<RepairClampDevice>();
            if(RepairClampDeviceApi == null)
            {
                Status = ConsoleCommandStatus.ThereIsNoRepairClampDeviceOnDrone;
                yield break;
            }
            // подвешиваем устройство на провод
            RepairClampDeviceApi.InstallDevice(Cable, RepairClampPosition,RepairClampRotation, infoHolder);
            //RepairClampDeviceApi.SaveDevice(); TODO
            Status = ConsoleCommandStatus.Success;
        }

        

        public override Message GetMessageToSend()
        {
            throw new NotSupportedCommandForMessageException();
        }

        public override void SetParams(CableWalkerApi cableWalkerApi)
        {
            return;
        }

        public new IEnumerator Execute(CableWalkerApi cableWalkerApi, CableWalkerClient cableWalkerClient)
        {
            throw new System.NotImplementedException();
        }

       
    }
}

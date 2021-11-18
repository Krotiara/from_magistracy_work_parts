using System;
using System.Collections.Generic;
using CableWalker.Simulator.Mission.Commands;
using UnityEngine;

namespace CableWalker.Simulator.Mission
{
    public static class CommandManager
    {
        private static readonly Dictionary<Type, CommandDescriptor> CommandDescriptors = new Dictionary<Type, CommandDescriptor>();
        private static readonly Dictionary<string, Type> Aliases = new Dictionary<string, Type>();

        static CommandManager()
        {
            CommandDescriptor
                .Build<Wait>()
                .AddAlias("Wait")
                .Register();
            CommandDescriptor
                .Build<SetStart>()
                .AddAlias("SetStart")
                .Register();
            CommandDescriptor
                .Build<FlyTo>()
                .AddAlias("FlyTo")
                .Register();
            CommandDescriptor
                .Build<TakeOffGround>()
                .AddAlias("TakeOffGround")
                .Register();
            CommandDescriptor
               .Build<TakeOffCable>()
               .AddAlias("TakeOffCable")
               .Register();
            CommandDescriptor
                .Build<StartEngines>()
                .AddAlias("StartEngines")
                .Register();
            CommandDescriptor
                .Build<StopEngines>()
                .AddAlias("StopEngines")
                .Register();
            CommandDescriptor
                .Build<SitOnCable>()
                .AddAlias("SitOnCable")
                .Register();
            CommandDescriptor
                .Build<RotateTo>()
                .AddAlias("Rotate")
                .Register();
            CommandDescriptor
                .Build<RotateByDirection>()
                .AddAlias("RotateByDirection")
                .Register();
            CommandDescriptor
                .Build<WheelModuleMove>()
                .AddAlias("MoveOnCable")
                .Register();
            CommandDescriptor
                .Build<WheelModuleMoveTo>()
                .AddAlias("MoveOnCableTo")
                .Register();
            CommandDescriptor
                .Build<VideoCameraTakePhoto>()
                .AddAlias("TakePhoto")
                .Register();
            CommandDescriptor
                .Build<VideoCameraLookAtInsulator>()
                .AddAlias("LookAt")
                .Register();
            CommandDescriptor
                .Build<VideoCameraGetVectorTo>()
                .AddAlias("GetVectorTo")
                .Register();
            CommandDescriptor
                .Build<InstallDevice>()
                .AddAlias("InstallDevice")
                .Register();
            CommandDescriptor
                .Build<InstallRepairClamp>()
                .AddAlias("InstallRepairClamp")
                .Register();
            CommandDescriptor
                .Build<StartLaserScanning>()
                .AddAlias("StartLaserScanning")
                .Register();
            CommandDescriptor
                .Build<StartMagnetScanning>()
                .AddAlias("StartMagnetScanning")
                .Register();
            CommandDescriptor
                .Build<StopMagnetScanning>()
                .AddAlias("StopMagnetScanning")
                .Register();
            CommandDescriptor
                .Build<LubricatorClamp>()
                .AddAlias("LubricatorClamp")
                .Register();
            CommandDescriptor
                .Build<LubricatorUnclamp>()
                .AddAlias("LubricatorUnclamp")
                .Register();
            CommandDescriptor
                .Build<LubricatorStartPomp>()
                .AddAlias("LubricatorStartPomp")
                .Register();
            CommandDescriptor
                .Build<LubricatorStopPomp>()
                .AddAlias("LubricatorStopPomp")
                .Register();
            CommandDescriptor
                .Build<UCSToGPS>()
                .AddAlias("UCSToGPS")
                .Register();
            CommandDescriptor
                .Build<FlyToTower>()
                .AddAlias("FlyToTower")
                .Register();
            CommandDescriptor
                .Build<FindPath>()
                .AddAlias("FindPath")
                .Register();
            CommandDescriptor
                .Build<InstallRepairClampTask>()
                .AddAlias("InstallRepairClampTask")
                .Register();
            CommandDescriptor
                .Build<MagnetScanningTask>()
                .AddAlias("MagnetScanningTask")
                .Register();
            CommandDescriptor
               .Build<FlyByPath>()
               .AddAlias("FlyByPath")
               .Register();
            CommandDescriptor
               .Build<SetDeviceState>()
               .AddAlias("SetDeviceState")
               .Register();
            CommandDescriptor
               .Build<SetRecorderFlag>()
               .AddAlias("SetRecorderFlag")
               .Register();
            CommandDescriptor
              .Build<SetSpan>()
              .AddAlias("SetSpan")
              .Register();
            CommandDescriptor
              .Build<SetPhase>()
              .AddAlias("SetPhase")
              .Register();
            CommandDescriptor
              .Build<SetCable>()
              .AddAlias("SetCable")
              .Register();
            CommandDescriptor
              .Build<SetCableWalkerState>()
              .AddAlias("SetCableWalkerState")
              .Register();
            CommandDescriptor
              .Build<ZoomCameraAutoFocus>()
              .AddAlias("ZoomCameraAutoFocus")
              .Register();
            CommandDescriptor
              .Build<ZoomCameraChangeFocus>()
              .AddAlias("ZoomCameraChangeFocus")
              .Register();
            CommandDescriptor
              .Build<ZoomCameraSetFocus>()
              .AddAlias("ZoomCameraSetFocus")
              .Register();
            CommandDescriptor
              .Build<ZoomCameraSetZoom>()
              .AddAlias("ZoomCameraSetZoom")
              .Register();

        }

        private static void Register(this CommandDescriptor descriptor)
        {
            foreach (var alias in descriptor.Aliases)
            {
                if (Aliases.ContainsKey(alias))
                {
                    Debug.LogError($"Команда с алиасом {alias} уже существует.");
                    return;
                }
            }

            if (CommandDescriptors.ContainsKey(descriptor.CommandType))
            {
                Debug.LogError($"Команда {descriptor.CommandType} уже зарегистрирована.");
                return;
            }
            
            foreach (var alias in descriptor.Aliases)
                Aliases.Add(alias, descriptor.CommandType);

            CommandDescriptors.Add(descriptor.CommandType, descriptor);
        }

        public static CommandDescriptor GetDescriptor<TCom>(TCom command) where TCom : Command
        {
            CommandDescriptors.TryGetValue(command.GetType(), out var descriptor);
            return descriptor;
        }

        public static Type GetCommandTypeByAlias(string alias)
        {
            Aliases.TryGetValue(alias, out var type);
            return type;
        }
    }
}

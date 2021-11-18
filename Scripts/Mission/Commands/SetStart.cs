using CableWalker.Simulator.Modules;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CableWalker.Simulator.Mission.Commands
{
    public class SetStart : Command
    {
        public float X { get; }
        public float Z { get; }
        public double Latitude { get; }
        public double Longitude { get; }
       
        private Spectator spectatorCamera;
        

        public SetStart(Vector2 unityPoint)
        {
            X = unityPoint.x;
            Z = unityPoint.y;
            Status = ConsoleCommandStatus.WaitingInLine;
        }

        public SetStart(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
            var point = GPSEncoder.GPSToUCS(new double[2] { Latitude, Longitude });
            X = point.x;
            Z = point.z;
            spectatorCamera = GameObject.FindGameObjectWithTag("SpectatorCamera").GetComponent<Spectator>();
            Status = ConsoleCommandStatus.WaitingInLine;
            var alias = CommandManager.GetDescriptor(this).Aliases.First();
            Name = $"{alias}({latitude},{longitude})";
        }

        public override IEnumerator DebugExecute(CableWalkerApi cableWalkerApi)
        {
            Status = ConsoleCommandStatus.Running;
            cableWalkerApi.TakeOffPlatform.transform.position = new Vector3(X, TerrainUtils.GetSampleHeight(new Vector3(X, 0, Z)), Z);
            cableWalkerApi.transform.position += new Vector3(0, cableWalkerApi.LocalYOnTakeOffPlatform, 0); 
            spectatorCamera.Follow(cableWalkerApi.transform);
            Status = ConsoleCommandStatus.Success;
            SetParams(cableWalkerApi);
            yield break;
        }        

        public override Message GetMessageToSend()
        {
            throw new NotSupportedCommandForMessageException();
        }

        public override void SetParams(CableWalkerApi cableWalkerApi)
        {
            cableWalkerApi.SetStartPosition((Latitude, Longitude), cableWalkerApi.transform.position.y);
        }

        public new IEnumerator Execute(CableWalkerApi cableWalkerApi, CableWalkerClient cableWalkerClient)
        {
            yield return DebugExecute(cableWalkerApi);
        }

       
    }
}

using CableWalker.Simulator.Model;
using CableWalker.Simulator.Modules;
using CableWalker.Simulator.Modules.VideoCamera;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CableWalker.Simulator
{
    public class CableWalkerCondition
    {

        public bool VideoCameraInMotion { get; }

        #region WheelModuleFields
        public Cable CurrectCable { get; }
        public float CurrectCablePoint { get; }
        public float PassedDistanceOnCable { get; }
        public float SpeedOnCable { get; }
        public float TitltAngleOnCable { get; }
        #endregion

        #region LubricatorModuleFields
        public float Capacity { get; }
        public float CurrentCount { get; }
        public float Consumption { get; }
        public bool PompEnabled { get; }
        public bool IsClamped { get; }
        #endregion

       // public Vector3 Position { get; }
        public Vector3 Rotation { get; }
        public Vector3 TakeOffPlatformPosition { get; }
        public Vector3 LocalPosOnTakeOffPlatform { get; }

        public bool IsEnginesEnabled { get; }

        public (double, double) StartGPSPosition { get; }
        public float LocatStartUnityHeight { get;  } //Нужно для вычисления высот относительно точки старта для передавания в команды консоли
        public double[] CurrentGPS { get; }
        public bool IsCableWalkerOn { get; }
        public Span CurrentSpan { get;}
        //Запись данных со всех включенных устройств
        public bool IsRecording { get; }

        public float Charge { get; }


        public CableWalkerCondition(CableWalkerApi cableWalkerApi)
        {
            //Position = cableWalkerApi.transform.position;
            Rotation = cableWalkerApi.transform.rotation.eulerAngles;
            TakeOffPlatformPosition = cableWalkerApi.transform.parent.position;
            LocalPosOnTakeOffPlatform = cableWalkerApi.transform.localPosition;

            //if (cableWalkerApi.VideoCameraModule != null)
            //{
            //    var cameraApi = cableWalkerApi.VideoCameraModule.CameraApi;
            //    VideoCameraInMotion = cameraApi.InMotion;
            //}
            if (cableWalkerApi.FlightModule != null)
            {
                IsEnginesEnabled = cableWalkerApi.FlightModule.IsEnginesEnabled;
            }
            if (cableWalkerApi.WheelModule != null)
            {
                CurrectCable = cableWalkerApi.WheelModule.Cable;
                CurrectCablePoint = cableWalkerApi.WheelModule.Point;
                PassedDistanceOnCable = cableWalkerApi.WheelModule.PassedDistance;
                SpeedOnCable = cableWalkerApi.WheelModule.Speed;
                TitltAngleOnCable = cableWalkerApi.WheelModule.TiltAngle;
            }
            if (cableWalkerApi.LubricatorModule != null)
            {
                Capacity = cableWalkerApi.LubricatorModule.Capacity;
                CurrentCount = cableWalkerApi.LubricatorModule.CurrentCount;
                Consumption = cableWalkerApi.LubricatorModule.Consumption;
                PompEnabled = cableWalkerApi.LubricatorModule.PompEnabled;
                IsClamped = cableWalkerApi.LubricatorModule.IsClamped;
            }
            if (cableWalkerApi.MagnetScannerModule != null)
            {

            }
            if (cableWalkerApi.WireCameraModule!= null)
            {

            }
            if (cableWalkerApi.LidarModule!= null)
            {

            }

        }

        /// <summary>
        /// Нужен для дебажного режима консоли
        /// </summary>
        /// <param name="cableWalkerApi"></param>
        public void SetConditionTo(CableWalkerApi cableWalkerApi)
        {
            //cableWalkerApi.transform.position = Position;
            cableWalkerApi.transform.eulerAngles = Rotation;
            cableWalkerApi.transform.localPosition = LocalPosOnTakeOffPlatform;
            cableWalkerApi.transform.parent.position = TakeOffPlatformPosition;     
            //if (cableWalkerApi.VideoCameraModule != null)
            //{
            //    cableWalkerApi.VideoCameraModule.CameraApi.InMotion = VideoCameraInMotion;
            //}
            if (cableWalkerApi.FlightModule != null)
            {
                if (IsEnginesEnabled) cableWalkerApi.FlightModule.Enable();
                else cableWalkerApi.FlightModule.Disable();
            }
            if (cableWalkerApi.WheelModule != null)
            {
                cableWalkerApi.WheelModule.Cable = CurrectCable;
                cableWalkerApi.WheelModule.Point = CurrectCablePoint;
                cableWalkerApi.WheelModule.PassedDistance = PassedDistanceOnCable;
                cableWalkerApi.WheelModule.Speed = SpeedOnCable;
                cableWalkerApi.WheelModule.TiltAngle = TitltAngleOnCable;
            }
            if (cableWalkerApi.LubricatorModule != null)
            {
                cableWalkerApi.LubricatorModule.Capacity = Capacity;
                cableWalkerApi.LubricatorModule.CurrentCount = CurrentCount;
                cableWalkerApi.LubricatorModule.Consumption = Consumption;
                cableWalkerApi.LubricatorModule.PompEnabled = PompEnabled;
                cableWalkerApi.LubricatorModule.IsClamped = IsClamped;
            }
            if (cableWalkerApi.MagnetScannerModule != null)
            {

            }


        }
    }
}
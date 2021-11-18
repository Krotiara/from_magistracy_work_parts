using CableWalker.Simulator.Model;
using CableWalker.Simulator.Modules.Flight;
using CableWalker.Simulator.Modules.Lubricator;
using CableWalker.Simulator.Modules.MagnetScanner;
using CableWalker.Simulator.Modules.VideoCamera;
using CableWalker.Simulator.Modules.Wheels;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CableWalker.Simulator.Modules
{
    [Serializable()]
    public class CableWalkerApi : MonoBehaviour
    {
        public VideoCameraModule VideoCameraModule { get; private set; }
        public FlightModule FlightModule { get; private set; }
        public WheelModule WheelModule { get; private set; }
        public LubricatorModule LubricatorModule { get; private set; }
        public MagnetScannerModule MagnetScannerModule { get; private set; }
        public WireCameraModule WireCameraModule { get; private set; }
        public LidarModule LidarModule { get; private set; }
        public CourseCameraModule CourseCameraModule { get; private set; }
        


        public Vector3 CurrentPosition => transform.position;   
        public float CurrentCablePointT { get; set; }       
        public LandingType DroneLandingType;
        public CableWalkerCondition Condition => new CableWalkerCondition(this);
        public GameObject TakeOffPlatform { get; private set; }
        public float LocalYOnTakeOffPlatform { get; private set; }


        #region params
        public Model.Cable CurrentCable { get; private set; }
        public (double, double) StartGPSPosition { get; private set; }
        public float LocatStartUnityHeight { get; private set; } //Нужно для вычисления высот относительно точки старта для передавания в команды консоли
        public Span CurrentSpan { get; private set; }
        public double[] CurrentGPS { get; set; }
        public float Charge { get; private set; }
        public float Speed { get; private set; }


        public string Status { get; set; }
        public string HardwareStatus { get; set; }
        public string DevicesStatus { get; set; }


        #endregion

        #region bool params
        public bool IsCableWalkerOn { get; private set; }
        //Запись данных со всех включенных устройств
        public bool IsRecording { get; private set; }

        #endregion

        public void Start()
        {
            VideoCameraModule = GetComponent<VideoCameraModule>();
            FlightModule = GetComponent<FlightModule>();
            WheelModule = GetComponent<WheelModule>();
            LubricatorModule = GetComponent<LubricatorModule>();
            MagnetScannerModule = GetComponent<MagnetScannerModule>();
           
        }

        
        private void Update()
        {
            //Будут проблемы про connection, надо думать
            if (WheelModule != null)
                CurrentCablePointT = WheelModule.Point;
        }
        
        public void SetCharge(float charge)
        {
            Charge = charge;
        }

        public void SetCurrentSpan(Span span)
        {
            CurrentSpan = span;
        }

        public void SetCurrentCable(Cable cable)
        {
            CurrentCable = cable;
        }

        public void SetIsRecordingFlag(bool isRecording)
        {
            IsRecording = isRecording;
        }

        public void SetCableWalkerOnFlag(bool isOn)
        {
            IsCableWalkerOn = isOn;
        }

        public void SetStartPosition((double, double) gpsStart, float locatStartUnityHeight)
        {
            StartGPSPosition = gpsStart;
            LocatStartUnityHeight = locatStartUnityHeight;
        }

        public void SetModuleState(string moduleName, bool state)
        {
            switch (moduleName)
            {
                case "wirecamera":
                    WireCameraModule.IsOn = state;
                    break;
                case "zoomcamera":
                    VideoCameraModule.IsOn = state;
                    break;
                case "lidar":
                    LidarModule.IsOn = state;
                    break;
                case "magneticscanner":
                    MagnetScannerModule.IsOn = state;
                    break;
                case "coursecamera":
                    CourseCameraModule.IsOn = state;
                    break;
            }

        }

        /// <summary>
        /// Этот метод подписывается на событие первого приема статуса от канатохода.
        /// </summary>
        /// <param name="status"></param>
        public void OnConnectionSetStatus(Dictionary<string, dynamic> status)
        {
            foreach (JProperty x in status["devices"])
            {
                //Надо протестить
                SetModuleState(x.Name, (bool)x.Value);


            }

            //Set start gps и localStartWorldHeight
            //Set span
            //Set phase
            //Set cable

        }

        /// <summary>
        /// этот метод подписывается на событие приема статуса от канатохода.
        /// </summary>
        /// <param name="status"></param>
        ///  <param name="isNeedFirstSet">если true, то происходит установка параметров при первом подключении</param>
        public void RefreshStatus(Dictionary<string, dynamic> status)
        {
            string hardwareResult = "";
            foreach (JProperty x in status["hardware"])
            {
                hardwareResult += x.Name + ": " + x.Value + "\r\n";
                
            }

            string deviceResult = "";
            foreach (JProperty x in status["devices"])
            {
                deviceResult += x.Name + ": " + x.Value + "\r\n";
            }


            string result = $"Active:{status["active"]}\n" +
                $"Operator:{status["operator"]}\n" +
                $"Gps:{status["gps"]}\n" +
                $"Span:{status["span"]}\n" +
                $"Phase:{status["phase"]}\n" +
                $"Crd:{status["crd"]}\n"+
                $"Speed:{status["speed"]}\n" +
                $"Charge:{status["charge"]}\n" +
                $"Recording:{status["recording"]}\n"+
                $"Rec_speed:{status["rec_speed"]} KB/sec\n" +
                $"Charge:{status["charge"]}\n"+
                $"m_clb:{status["m_clb"][0]};{status["m_clb"][1]}\n"
                ;

            Status = result;
            HardwareStatus = hardwareResult;
            DevicesStatus = deviceResult;

            //SetDinamicParams
            //SetCharge(status["charge"]);
            //double[] gps = status["gps"];
            //CableWalkerApi.RefreshCableWalkerPosition(gps, localStartWorldHeight);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="localStartWorldHeight">Высота коптера относительно точки взлета</param>
        public void RefreshCableWalkerPosition(double[] gps, float localStartWorldHeight)
        {
            float height = LocatStartUnityHeight + localStartWorldHeight;
            Vector3 pos = GPSEncoder.GPSToUCS(gps);
            pos.y = height;
            transform.position = pos;
            CurrentGPS = gps;
            //Чтоб не вылетело при отладке
            //if (CurrentCable != null)
            //    CurrentCablePointT = CurrentCable.GetNearestCablePointTTo(pos);
        }

        //public void StopAllActions()
        //{
        //    FlightModule.Disable();
        //    if (WheelModule != null)
        //        WheelModule.StopMoving();
        //    if(LubricatorModule != null)
        //        LubricatorModule.Disable();
        //    if (MagnetScannerModule != null)
        //        MagnetScannerModule.StopScanning();
        //    gameObject.transform.position = Vector3.zero;
        //    gameObject.transform.eulerAngles = Vector3.zero;
        //}

        public void SetCable(Model.Cable cable, Vector3 cablePoint)
        {
            CurrentCable = cable;
            CurrentCablePointT = cable.GetTByPoint(cablePoint);
            CurrentCable.RegenerateInHighDetail();
        }

        public void SetNewCablePoint()
        {
            //по gps и высоте
            //CurrentCable.GetNearestCablePointTo
            //throw new NotImplementedException();
            return;
        }

        public void DropCable()
        {
            if(CurrentCable != null)
                CurrentCable.Regenerate(CableDisplayMode.LineRenderer);
            CurrentCable = null;
            CurrentCablePointT = -1;
        }

        public void SetTakeOffPlatform(GameObject p, float localYOnTakeOffPlatform)
        {
            TakeOffPlatform = p;
            LocalYOnTakeOffPlatform = localYOnTakeOffPlatform;
        }


    }
}

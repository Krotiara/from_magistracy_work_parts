using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CableWalker.Simulator.Model;
using CableWalker.Simulator.Modules;
using UnityEngine;

namespace CableWalker.Simulator.Mission.Commands
{
    public class InstallDevice : Command
    {
        private const float Precision = 0.01f;
        private const float Speed = 2.0f;
        private Cable cable;

        public float PointOnCable { get; }
        public string CableName { get; }
        

        /// <summary>
        /// координаты точки на проводе для установки
        /// </summary>
        public Vector3 pointOnCable;
        Vector3 cableDirection;
        int additionCablePoint;
        private CableSuspensionDevice installerDeviceApi;
        private Vector3 target;
        private InformationHolder infoHolder;
       

        public InstallDevice(string cableName, float pointOnCable)
        {
            CableName = cableName;
            PointOnCable = pointOnCable;
            infoHolder = GameObject.FindGameObjectWithTag("InfoHolder").GetComponent<InformationHolder>();
            Status = ConsoleCommandStatus.WaitingInLine;
            var alias = CommandManager.GetDescriptor(this).Aliases.First();
            Name = $"{alias}(\"{cableName}\",{pointOnCable})";
        }

        public override IEnumerator DebugExecute(CableWalkerApi cableWalkerApi)
        {
            Status = ConsoleCommandStatus.Running;
            cable = infoHolder.Get<Cable>(CableName);
            if(cable == null)
            {
                Status = ConsoleCommandStatus.IncorrectCableNumberArgument;
                yield break;
            }
            var arrow = (cable.End.Position - cable.Start.Position).normalized;
            var targetPoint = arrow * PointOnCable;
            installerDeviceApi = cableWalkerApi.transform.GetComponentInChildren<CableSuspensionDevice>();
            
            var minDistance = float.PositiveInfinity;
            var pointIndex =(int)(PointOnCable);

            // Извини, Андрей, но оно чего то не то
            //var pointIndex = 0;
            //for (var i = 0; i < cable.Points.Length; i++)
            //{
            //    var point = cable.Points[i];
            //    if ((targetPoint - point).magnitude < minDistance)
            //    {
            //        minDistance = (targetPoint - point).magnitude;
            //        pointOnCable = point;
            //        pointIndex = i;
            //    }
            //}


            var startEngines = new StartEngines();
            yield return startEngines.DebugExecute(cableWalkerApi);

            additionCablePoint = SetAdditionPoint(pointIndex, cable.Points.Count);
            if (additionCablePoint > PointOnCable)
                cableDirection = cable.Points[additionCablePoint] - cable.Points[pointIndex];
            else
                cableDirection = cable.Points[pointIndex] - cable.Points[additionCablePoint];

            target = cable.Points[pointIndex];
            target = new Vector3(target.x, target.y  - installerDeviceApi.DeviceHeight, target.z);

            while ((target - cableWalkerApi.transform.position).magnitude > Precision)
            {
               
                cableWalkerApi.transform.position = Vector3.MoveTowards(cableWalkerApi.transform.position, target, .9f * Time.deltaTime);
                yield return null;
            }

            // пока не работает чего т
            //var flyTo = new FlyTo(target.x, target.y, target.z);
            //yield return flyTo.DebugRun(cableWalkerApi);


            // устанавливаем коптер параллельно проводу
            var rotateTo = new RotateTo(cable.End.Position.x, cable.End.Position.y, cable.End.Position.z);
            yield return rotateTo.DebugExecute(cableWalkerApi);

            // Наклоняет коптер под провод по вертикале
            // угловой коэффициент 
            float tg = (cable.Points[pointIndex].z - cable.Points[additionCablePoint].z)
                / (cable.Points[pointIndex].x - cable.Points[additionCablePoint].x);
            // угол наклона провода на отрезке установки устройства
            float balanceAngle = (float)Math.Atan(tg);

            while (balanceAngle > Precision)
            {
                var angleLerped = Mathf.LerpAngle(balanceAngle, 0.0f, Speed * Time.deltaTime);
                cableWalkerApi.transform.Rotate(new Vector3(balanceAngle - angleLerped, 0.0f, 0.0f));
                balanceAngle = angleLerped;
                yield return null;
            }

            target = new Vector3(target.x, target.y + installerDeviceApi.UpAfterRotate, target.z);

            while ((target - cableWalkerApi.transform.position).magnitude > Precision)
            {

                cableWalkerApi.transform.position = Vector3.MoveTowards(cableWalkerApi.transform.position, target, .9f * Time.deltaTime);
                yield return null;
            }

            Install(cableWalkerApi);
            Status = ConsoleCommandStatus.Success;
            yield return null;
        }

        /// <summary>
        /// Установка подвеса на провод
        /// </summary>
        private void Install(CableWalkerApi cableWalkerApi)
        {
            var installerDeviceApi = cableWalkerApi.transform.GetComponentInChildren<CableSuspensionDevice>();
            // подвешиваем устройство на провод
            //installerDeviceApi.InstallDevice();
            //installerDeviceApi.SaveDevice();
        }

        /// <summary>
        /// Вычислить дополнительную точку на проводе
        /// </summary>
        /// <param name="point">заданная точка</param>
        /// <param name="pointsCount">количество точек на проводе</param>
        /// <returns></returns>
        private int SetAdditionPoint(int point, int pointsCount)
        {
            if (point > pointsCount / 2)
                return point - 10;
            return point + 10;
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
            throw new NotImplementedException();
        }

    }
}

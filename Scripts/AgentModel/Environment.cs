using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CableWalker.Simulator
{
    public class Environment
    {
        /// <summary>
        /// Температура окружающей среды [Цельсий].
        /// </summary>
        public float Temperature { get; private set; }

        public float TemperatureInKelvin => Temperature + 273.15f;
        /// <summary>
        /// Атмосферное давление [мм.рт.ст.].
        /// </summary>
        public float P { get; private set; }
        /// <summary>
        /// Суммарная среднемесячная мощность солнечной радиации [Вт/(см*см)].
        /// </summary>
        public float Q { get; private set; }
        /// <summary>
        /// Скорость ветра [м/с].
        /// </summary>
        public float WindSpeed { get; private set; }
        /// <summary>
        /// Влажность воздуха [%].
        /// </summary>
        public float Humidity { get; private set; }
        /// <summary>
        /// Время влажности [ч].
        /// </summary>
        public float TDH { get; private set; }

        public WindDirection WindDirection { get; private set; }

        public WindSpeedAreas WindSpeedArea { get;}
        public float AreaWindSpeedValue => (int)WindSpeedArea;
        public float AreaWindPressureValue => (int)Enum.Parse(typeof(WindPressureAreas), WindSpeedArea.ToString());

        public IceThicknessAreas IceThicknessArea { get;}
        public float AreaIceThicknessValue => (int)IceThicknessArea;


        public Environment(WindSpeedAreas windSpeedArea, IceThicknessAreas iceThicknessArea)
        {
            WindSpeedArea = windSpeedArea;
            IceThicknessArea = iceThicknessArea;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t">Температура окружающей среды [Цельсий].</param>
        /// <param name="w">Скорость ветра [м/с].</param>
        /// <param name="h">Влажность воздуха [%].</param>
        /// <param name="p">Атмосферное давление [мм.рт.ст.]</param>
        /// <param name="q">Суммарная среднемесячная мощность солнечной радиации [Вт/(см*см)]</param>
        public void SetParams(float t, float w, float h, float p, float q, WindDirection windDirection)
        {
            //TODO (09.04.2020) присвоить q по-умолчанию. 
            Temperature = t;
            WindSpeed = w;
            Humidity = h;
            P = p;
            Q = q;
            WindDirection = windDirection;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t">Температура окружающей среды [Цельсий].</param>
        /// <param name="w">Скорость ветра [м/с].</param>
        /// <param name="h">Влажность воздуха [%].</param>
        /// <param name="p">Атмосферное давление [мм.рт.ст.]</param>
        /// <param name="q">Суммарная среднемесячная мощность солнечной радиации [Вт/(см*см)]</param>
        public void SetParams(float t, float w, float h, float p, WindDirection windDirection)
        {
            SetParams(t, w, h, p, 0.07f, windDirection);
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }


    }
}

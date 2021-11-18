using UnityEngine;

public class Battery : MonoBehaviour
{
    //full charge of the battery in W*c
    public double FullCharge;

    // Current charge of the battery in W*c
    public double CurrentCharge;

    public double FullVoltage;
    public double CurrentlyVoltage;

    public double SumСonsumption = 0;
    public double Tprov;

    public void Start()
    {
        FullCharge = 1200000;
        FullVoltage = 25.2;
        CurrentCharge = FullCharge;
        CurrentlyVoltage = FullVoltage;
    }

    public void Energy(int number, double time)
    {
        double consumption = 0;
        if (number == 1)
            consumption = Movement(time * 10);
        if (number == 2)
            consumption = Fly(time * 10);
        if (number == 3)
            consumption = Camera(time);
        if (number == 4)
            consumption = IRCamera(time);
        if (number == 5)
            consumption = UVCamera(time);
        if (number == 6)
            consumption = LaserScanner(time);
        if (number == 7)
            consumption = MagnetScanner(time);
        SumСonsumption += consumption + ConstDevices(time);
        CurrentCharge = FullCharge - SumСonsumption;
        CurrentlyVoltage = FullVoltage - SumСonsumption / 180000 * 0.3;
        if (CurrentCharge <= FullCharge)
            BatteryException.LowBattery.Invoke();
    }
    public void Information(int number)
    {
        double Akkum = FullCharge / 2; // 1278720-для 1600 мАч
        int Nakkum = 2;
        double I = 200;
        double SumAkkum = Akkum * Nakkum;
        double Pgen = (0.0004 * I * I + 0.0138 * I - 0.3435);
        double Wgen = Pgen * Tprov;
        double def = SumСonsumption - SumAkkum - Wgen;
        int n1 = (int)(SumСonsumption / Akkum) + 1;
        if (n1 == 1)
            n1 = 2;
        int n2 = (int)((SumСonsumption - Wgen) / Akkum) + 1;
        if (n2 == 1) n2 = 2;
        if (Wgen > SumСonsumption) n2 = 2;

        if (number == 1)
            Debug.Log(string.Format("Емкость аккумуляторов={0}", SumAkkum));
        if (number == 2)
            Debug.Log(string.Format("Потребляемая энергия {0}", SumСonsumption));
        if (number == 3)
            Debug.Log(string.Format("Подзарядка от линии {0}", Wgen));
        if (number == 4)
            Debug.Log(string.Format("Дефецит={0}", def));
        if (number == 5)
            if (n1 > 2 || n2 > 2)
                Debug.Log("Без смены аккумуляторов выполнить задачу нельзя!");
        if (number == 6)
            if (n1 == n2)
                Debug.Log("Установка ЗУ нецелесообразна");
            else
                Debug.Log("Установка ЗУ целесообразна");
        if (number == 7)
            Debug.Log(string.Format("Кол-во аккумуляторов без применения ЗУ {0}, дополнительно трбуется {1}", n1, n1 - 2));
        if (number == 8)
            Debug.Log(string.Format("Кол-во аккумуляторов при использовании ЗУ {0}, дополнительно трбуется {1}", n2, n2 - 2));
        if (number == 9)
            if (def > 0)
            {
                double Tdef = Tprov;
                while (def > 100)
                {
                    Tdef = Tdef + 10;
                    Wgen = Pgen * Tdef;
                    def = SumСonsumption - SumAkkum - Wgen;
                }
                Debug.Log(string.Format("Для устранения дефецита повисеть на линии {0} сек", Tdef));
            }
    }
    
    public double Fly(double timeSec)
    {
        return 3000 * timeSec;
    }
    
    public double Camera(double timeSec)
    {
        return 8 * timeSec;
    }
    
    public double IRCamera(double cek)
    {
        return 2.1 * cek;
    }
    
    public double UVCamera(double cek)
    {
        return 1.6 * cek;
    }
    
    public double LaserScanner(double distance)
    {
        return 8 * distance / 0.5;
    }
    
    public double MagnetScanner(double distance)
    {
        return 0.1 * distance / 0.5;
    }
    
    public double Movement(double t)
    {
        Tprov = t / 0.5;
        return 16 * t;
    }
    
    public double ConstDevices(double timeSec)
    {
        return 10 * timeSec;
    }
}

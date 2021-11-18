using CableWalker.Simulator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


public class SpanAreaTSVData
{

    public Dictionary<AreasCalculatorType, AreaCalculator> AreasCalculators { get; private set; }
    public AreaCalculator GostFormatCalculator => AreasCalculators[AreasCalculatorType.Gost];

    public SpanAreaTSVData(SpanData spanData, GameObject areaPrefab)
    {
        AreasCalculators = new Dictionary<AreasCalculatorType, AreaCalculator>();
        (Dictionary<string, float>, Dictionary<string, Color>) notGost = InitializeAreasDictByType(AreasCalculatorType.NotGost);
        var notGostCalculator = new AreaCalculator(notGost.Item1);
        // InitializeAreasLinesBy(notGostCalculator, notGost.Item2);
        (Dictionary<string, float>, Dictionary<string, Color>) gost = InitializeAreasDictByType(AreasCalculatorType.Gost);
        var gostCalculator = new AreaCalculator(gost.Item1);
        AreasCalculators[AreasCalculatorType.NotGost] = notGostCalculator;
        AreasCalculators[AreasCalculatorType.Gost] = gostCalculator;

        int currentAreaNumber = 0;
        ClearCalculators();
        foreach (string areaData in spanData.DKRAreasData)
        {
            string[] split = areaData.Split(';').ToArray();
            AreaScript area = GameObject.Instantiate(areaPrefab).GetComponent<AreaScript>();
            //area.p1.position = new Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2])); //Тут надо корректировать позиции по облаку точек
            //area.p2.position = new Vector3(float.Parse(split[3]), float.Parse(split[4]), float.Parse(split[5]));
            //area.p3.position = new Vector3(float.Parse(split[6]), float.Parse(split[7]), float.Parse(split[8]));
            //area.p4.position = new Vector3(float.Parse(split[9]), float.Parse(split[10]), float.Parse(split[11]));

            string areaType = GetAreaType(split);
            area.AreaType = areaType;
            area.Number = currentAreaNumber;
            area.StartSquare = area.GetSquare();
            area.IsSummed = true;
            currentAreaNumber += 1;
            AreasCalculatorType areasCalculatorType = GetAreaCalculatorType(split);
            if (areasCalculatorType != AreasCalculatorType.Object)
                AreasCalculators[areasCalculatorType].AddArea(area, areaType, area.GetSquare()); //Просто суммируем начальную площадь, аля флаг, что есть растительность. Пока так

            //if (areaType != "")
            //{
            //    try
            //    {
            //        area.SetColor(AreasCalculatorsColors[areasCalculatorType][areaType]);
            //    }
            //    catch (KeyNotFoundException e)
            //    {
            //        Debug.Log(string.Format("No key {0} in areas calculator", areaType));
            //        return;
            //    }
            //}
        }


    }

    public SpanAreaTSVData(FileInfo file, GameObject areaPrefab)
    {

        AreasCalculators = new Dictionary<AreasCalculatorType, AreaCalculator>();
        (Dictionary<string, float>, Dictionary<string, Color>) notGost = InitializeAreasDictByType(AreasCalculatorType.NotGost);
        var notGostCalculator = new AreaCalculator(notGost.Item1);
        // InitializeAreasLinesBy(notGostCalculator, notGost.Item2);
        (Dictionary<string, float>, Dictionary<string, Color>) gost = InitializeAreasDictByType(AreasCalculatorType.Gost);
        var gostCalculator = new AreaCalculator(gost.Item1);
        AreasCalculators[AreasCalculatorType.NotGost] = notGostCalculator;
        AreasCalculators[AreasCalculatorType.Gost] = gostCalculator;

        using (StreamReader sr = file.OpenText())
        {
            string lines = sr.ReadToEnd();
            SpanData spanData = DataLoader<SpanData>.LoadJson(lines);
            int currentAreaNumber = 0;
            ClearCalculators();
            foreach (string areaData in spanData.DKRAreasData)
            {
                string[] split = areaData.Split(';').ToArray();
                AreaScript area = GameObject.Instantiate(areaPrefab).GetComponent<AreaScript>();
                //area.p1.position = new Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2])); //Тут надо корректировать позиции по облаку точек
                //area.p2.position = new Vector3(float.Parse(split[3]), float.Parse(split[4]), float.Parse(split[5]));
                //area.p3.position = new Vector3(float.Parse(split[6]), float.Parse(split[7]), float.Parse(split[8]));
                //area.p4.position = new Vector3(float.Parse(split[9]), float.Parse(split[10]), float.Parse(split[11]));

                string areaType = GetAreaType(split);
                area.AreaType = areaType;
                area.Number = currentAreaNumber;
                area.StartSquare = area.GetSquare();
                area.IsSummed = true;
                currentAreaNumber += 1;
                AreasCalculatorType areasCalculatorType = GetAreaCalculatorType(split);
                if (areasCalculatorType != AreasCalculatorType.Object)
                    AreasCalculators[areasCalculatorType].AddArea(area, areaType, area.GetSquare()); //Просто суммируем начальную площадь, аля флаг, что есть растительность. Пока так

                //if (areaType != "")
                //{
                //    try
                //    {
                //        area.SetColor(AreasCalculatorsColors[areasCalculatorType][areaType]);
                //    }
                //    catch (KeyNotFoundException e)
                //    {
                //        Debug.Log(string.Format("No key {0} in areas calculator", areaType));
                //        return;
                //    }
                //}
            }

        }
    }

    public void ClearCalculators()
    {
        foreach (AreaCalculator areaCalculator in AreasCalculators.Values)
            areaCalculator.Clear();
    }

    private string GetAreaType(string[] split)
    {
        try
        {
            return split[12];
        }
        catch (IndexOutOfRangeException e)
        {
            Debug.Log(e);
            return "";
        }
    }

    private AreasCalculatorType GetAreaCalculatorType(string[] split)
    {
        try
        {
            //string aCT = split[14];
            string aCT = split[17];
           
            if (aCT == AreasCalculatorType.NotGost.ToString())
                return AreasCalculatorType.NotGost;
            else if (aCT == AreasCalculatorType.Gost.ToString())
                return AreasCalculatorType.Gost;
            else
                return AreasCalculatorType.Object;
        }
        catch (IndexOutOfRangeException e)
        {
            return AreasCalculatorType.NotGost;
        }
    }


    private (Dictionary<string, float>, Dictionary<string, Color>) InitializeAreasDictByType(AreasCalculatorType areasCalculatorType)
    {
        Dictionary<string, float> squaresDict = new Dictionary<string, float>();
        Dictionary<string, Color> colorsDict = new Dictionary<string, Color>();

        switch (areasCalculatorType)
        {
            case AreasCalculatorType.NotGost:
                squaresDict = new Dictionary<string, float>()
                {
                    {"01s", 0 },
                    {"01r", 0 },
                    {"13s", 0 },
                    {"13r", 0 },
                    {"34s", 0 },
                    {"34r", 0 },
                    {"46s", 0 },
                    {"46r", 0 },
                    {"6+s", 0 },
                    {"6+r", 0 },
                    {"threating", 0 }
                };
                colorsDict = new Dictionary<string, Color>()
                {
                    {"01s", Color.white },
                    {"01r", Color.white },
                    {"13s", Color.green },
                    {"13r", Color.green },
                    {"34s", new Color(166f/255,46f/255,123f/255) },
                    {"34r", new Color(166f/255,46f/255,123f/255) },
                    {"46s",  new Color(75f/255,87f/255,219f/255) },
                    {"46r", new Color(75f/255,87f/255,219f/255) },
                    {"6+s", new Color(229f/255,135f/255,0) },
                    {"6+r", new Color(229f/255,135f/255,0) },
                    {"threating", Color.red }
                };
                break;
            case AreasCalculatorType.Gost:
                squaresDict = new Dictionary<string, float>()
                {
                    {"1_4", 0 },
                    {"4_10", 0 },
                    {"10+", 0 },
                    {"threating", 0 }
                };
                colorsDict = new Dictionary<string, Color>()
                {
                    {"1_4", Color.green },
                    {"4_10", new Color(166f/255,46f/255,123f/255)},
                    {"10+", new Color(229f/255,135f/255,0) },
                    {"threating", Color.red }
                };
                break;
        }
        return (squaresDict, colorsDict);
    }

}


public class TSVAreasRecalculatorByPointClouds
{

  
    public TSVAreasRecalculatorByPointClouds()
    {
    }

    public Dictionary<string, SpanAreaTSVData> LoadData(List<GameObject> pointClouds, Dictionary<string, SpanData> spansData,
            InformationHolder infoHolder)
    {

        Dictionary<string, SpanAreaTSVData> areasData = new Dictionary<string, SpanAreaTSVData>();

        foreach(var entry in spansData)
        {
            areasData[entry.Key] = new SpanAreaTSVData(entry.Value, infoHolder.AreaPrefab);
        }

        //DirectoryInfo dinfo = new DirectoryInfo(pointCloudsTxtsPath);
        //FileInfo[] Files = dinfo.GetFiles("*.txt");
        //foreach (FileInfo file in Files)
        //{
        //    string[] split = file.Name.Split('_');
        //    string number = string.Format("{0}_{1}_{2}", split[0], split[1], split[2]);
        //    areasData[number] = new SpanAreaTSVData(file, infoHolder.AreaPrefab);
        //}
        return areasData;
    }



    

}

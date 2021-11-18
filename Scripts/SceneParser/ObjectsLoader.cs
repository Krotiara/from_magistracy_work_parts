using CableWalker.Simulator;
using CableWalker.Simulator.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

public class ObjectsLoader : MonoBehaviour
{

    public InformationHolder infoHolder;
    public static readonly string pathToAreasFile = Path.Combine(GetStreamingAssetsPath(), "Configs\\Areas.csv").Replace('\\', '/'); //Path.Combine(@"Assets/Resources/Configs", "Areas.csv");
    public static readonly string pathToConfigs = Path.Combine(GetStreamingAssetsPath(), "Configs").Replace('\\', '/');

    public ObjectsLoader() { }

    public bool LoadObjects(InformationHolder infoHolder, bool isEditorMode)
    {
        
            var configName = GameObject.FindGameObjectWithTag("savedDataName").name;
            var pathToConfig = Path.Combine(pathToConfigs, configName).Replace('\\', '/');
            if (!LoadConfig(infoHolder, configName, pathToConfig,isEditorMode))
                return false;
            FindObjectsOnScene();
            
            return true;
       
    }

    public string GetPointCloudsDataPath()
    {
        var configName = GameObject.FindGameObjectWithTag("savedDataName").name;
        var pathToConfig = Path.Combine(pathToConfigs, configName).Replace('\\', '/');
        //return new string[2] { Path.Combine(pathToConfig, "SpansTXTs"), Path.Combine(pathToConfig, "CablesTXTs") };
        return Path.Combine(pathToConfig, "PointCloudsTxts");
    }



    private static string GetStreamingAssetsPath()
    {
        return Application.platform == RuntimePlatform.Android ?
            Application.dataPath + "!assets/" : Application.streamingAssetsPath;
    }

    public bool LoadConfig(InformationHolder infoHolder, bool isEditorMode)
    {
        var configName = GameObject.FindGameObjectWithTag("savedDataName").name;
        var pathToConfig = Path.Combine(pathToConfigs, configName).Replace('\\', '/');
        return LoadConfig(infoHolder, configName, pathToConfig, isEditorMode);
    }

    public bool LoadConfig(InformationHolder infoHolder, string configName, string pathToConfig, bool isEditorMode)
    {
        //try
        //{
        this.infoHolder = infoHolder;
        string pathtoEnvironmentTypes = Path.Combine(pathToConfig, "EnvironmentTypes\\EnvironmentTypes.csv");
        using (var parser = new StreamReader(pathtoEnvironmentTypes))
        {
            parser.ReadLine(); //пропустить заголовки
            var args = parser.ReadLine().Split(';');
            WindSpeedAreas windArea = (WindSpeedAreas)Enum.Parse(typeof(WindSpeedAreas), args[1]);
            IceThicknessAreas iceArea = (IceThicknessAreas)Enum.Parse(typeof(IceThicknessAreas), args[2]);
            infoHolder.InitializeEnvironment(windArea, iceArea);
        }
        string pathToPowerLineInfo = Path.Combine(pathToConfig, "PowerLineInfo.csv");
        Dictionary<string, float> normativeCablesDimensions = new Dictionary<string, float>();
        using (var parser = new StreamReader(pathToPowerLineInfo))
        {
            parser.ReadLine(); //пропустить заголовки
            while (!parser.EndOfStream)
            {
                var line = parser.ReadLine().Split(';');
                switch (line[0])
                {
                    case "securedDistance":
                        infoHolder.SetSecuredDistance(int.Parse(line[1]));
                        break;
                    case "i":
                        infoHolder.SetPowerLineI(int.Parse(line[1]));
                        break;
                    case "gladeWidthNormativeValue":
                        infoHolder.SetGladeWidthNormativeValue(float.Parse(line[1]));
                        break;
                    case "ground":
                        normativeCablesDimensions["ground"] = float.Parse(line[1]);
                        break;
                    case "green":
                        normativeCablesDimensions["green"] = float.Parse(line[1]);
                        break;
                    case "crossedCables":
                        normativeCablesDimensions["crossedCables"] = float.Parse(line[1]);
                        break;
                    case "roads":
                        normativeCablesDimensions["roads"] = float.Parse(line[1]);
                        break;
                    case "buildings":
                        normativeCablesDimensions["buildings"] = float.Parse(line[1]);
                        break;
                }
            }
        }
        infoHolder.NormativeCablesDimensions = normativeCablesDimensions;

        return LoadConfigData(configName, pathToAreasFile, pathToConfig, infoHolder,isEditorMode);
        //}
        //catch (Exception e)
        //{
        //    Debug.Log(e);
        //    return false;
        //}

        //using (var parser = new StreamReader(pathToAreasFile))
        //{
        //    parser.ReadLine(); //пропустить заголовки
        //    while (!parser.EndOfStream)
        //    {
        //        var args = parser.ReadLine().Split(';');
        //        if (args[0].Equals(configName))
        //        {
        //            args[1] = args[1].Replace(',', '.');
        //            args[2] = args[2].Replace(',', '.');
        //            LoadConfigData(args[0], double.Parse(args[1], CultureInfo.InvariantCulture),
        //                double.Parse(args[2], CultureInfo.InvariantCulture), pathToConfig, double.Parse(args[4], CultureInfo.InvariantCulture));
        //            return;
        //        }
        //    }
        //}
    }

    public bool LoadConfigData(string configName, string pathToAreasFile, string pathToConfig, InformationHolder infoHolder, bool isEditorMode)
    {
        //try
        //{
            using (var parser = new StreamReader(pathToAreasFile))
            {
                parser.ReadLine(); //пропустить заголовки
                while (!parser.EndOfStream)
                {
                    var args = parser.ReadLine().Split(';');
                    if (args[0].Equals(configName))
                    {
                        args[1] = args[1].Replace(',', '.');
                        args[2] = args[2].Replace(',', '.');
                        LoadConfigData(args[0], double.Parse(args[1], CultureInfo.InvariantCulture),
                            double.Parse(args[2], CultureInfo.InvariantCulture), pathToConfig, double.Parse(args[4], CultureInfo.InvariantCulture), infoHolder,isEditorMode);
                        return true;
                    }
                }
            }
            return true;
        //}
        //catch (Exception e)
        //{
        //    Debug.Log(e);
        //    return false;
        //}
    }


    public void FindObjectsOnScene()
    {
        var objects = infoHolder.GetAll();
        foreach (var type in objects.Keys)
        {
            if (type.Equals(typeof(Span)))
                continue; //Костыль, span не генерится на сцене пока что
            var tag = infoHolder.GetTag(type);
            var objectsWithTag = GameObject.FindGameObjectsWithTag(tag); //Сделать единичную загрузку по тегам 09.03.2021
            if (objectsWithTag.Length == 0) return;
            foreach (var model in objects[type].Values)
            {
                GameObject obj = objectsWithTag.First(x => x.name == model.Number);
                if (obj == null)
                    Debug.Log($"Cant find object with type {type.ToString()}, tag {tag} and number {model.Number}");
                else
                    model.SetObjectOnScene(obj);
            }
        }

       

    }

    public void LoadDefectsData(string pathToDefectsHolder, InformationHolder infoHolder, bool isEditorMode)
    {
        var preloadedTypesData = PreloadConfigTypesData(pathToDefectsHolder);
        foreach (var objectsDataFolder in infoHolder.defectsLoadOrder)
        {
            var folderDataPath = Path.Combine(pathToDefectsHolder, objectsDataFolder);
            var type = infoHolder.GetTypeByDispatch(objectsDataFolder);
            if (!Directory.Exists(folderDataPath))
                continue;
            using (var parser = new StreamReader(Path.Combine(folderDataPath, objectsDataFolder + ".csv"), encoding: System.Text.Encoding.GetEncoding(1251)))
            {
                parser.ReadLine();
                while (!parser.EndOfStream)
                {
                    var line = parser.ReadLine().Split(';').ToArray();
                    var typeNum = line[1];
                    List<string> args = line.Where((val, idx) => idx != 1).ToList();
                    for (int i = 0; i < args.Count; i++)
                    {
                        args[i] = args[i].Replace(',', '.');
                    }

                    List<string> typeArgs = GetObjectTypeDataByTypeNumber(preloadedTypesData[objectsDataFolder], typeNum, objectsDataFolder);

                    typeArgs.Add(typeNum.ToString()); //Костыль для прокидки числового значения типа дефекта

                    var model = (Model)Activator
                        .CreateInstance(type); // вызывается конструктор соответствующего типа
                    var created = model.Create(args, typeArgs, infoHolder,isEditorMode);
                    if (created != null)
                        infoHolder.Set(created);
                }
            }
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="configName"></param>
    /// <param name="centerLatitude"></param>
    /// <param name="centerLongitude"></param>
    /// <param name="configPath">Путь до конфига относительно проекта Unity</param>
    /// <param name="centerHeightInWorld"></param>
    private void LoadConfigData(string configName, double centerLatitude, double centerLongitude, string configPath, double centerHeightInWorld, InformationHolder infoHolder, bool isEditorMode)
    {
        infoHolder.SetAreaCenterGPS(new double[2] { centerLatitude, centerLongitude });
        infoHolder.SetTerrainCenterHeight(centerHeightInWorld);
        //configPath = Path.Combine(Application.dataPath, configPath);// путь до конфига относительно файловой системы
        var preloadedTypesData = PreloadConfigTypesData(configPath);

        foreach (var objectsDataFolder in infoHolder.preloadOrder)
        {
            var folderDataPath = Path.Combine(configPath, objectsDataFolder);
            var type = infoHolder.GetTypeByDispatch(objectsDataFolder);
            if (!Directory.Exists(folderDataPath))
                continue;
            using (var parser = new StreamReader(Path.Combine(folderDataPath, objectsDataFolder + ".csv"), encoding: System.Text.Encoding.GetEncoding(1251)))
            {
                parser.ReadLine();
                while (!parser.EndOfStream)
                {
                    var line = parser.ReadLine().Split(';').ToArray();
                    var typeNum = line[1];
                    List<string> args = line.Where((val, idx) => idx != 1).ToList();
                    for (int i = 0; i < args.Count; i++)
                    {
                        args[i] = args[i].Replace(',', '.');
                    }

                    List<string> typeArgs = GetObjectTypeDataByTypeNumber(preloadedTypesData[objectsDataFolder], typeNum, objectsDataFolder);

                    typeArgs.Add(typeNum.ToString()); //Костыль для прокидки числового значения типа дефекта

                    var model = (Model)Activator
                        .CreateInstance(type); // вызывается конструктор соответствующего типа
                    var created = model.Create(args, typeArgs, infoHolder, isEditorMode);
                    if (created != null)
                        infoHolder.Set(created);
                }
            }

        }

        SetRelativeTowers();
        SetRelativeInsStrings();
        infoHolder.SetSpans();

    }

    private void SetRelativeTowers()
    {
        var towers = infoHolder.GetList<Tower>();
        foreach (Tower tower in towers)
            tower.SetPrevNextTowers(infoHolder);
    }

    private void SetRelativeInsStrings()
    {
        var strings = infoHolder.GetDict<InsulatorString>();
        foreach (InsulatorString insulatorString in strings.Values)
        {
            if (insulatorString.InsStringRelativeNumber == ((int)CablesNumbers.PortalNumber).ToString())
                continue;
            var relativeInsulatorString = (InsulatorString)strings[insulatorString.InsStringRelativeNumber];
            insulatorString.SetRelative(relativeInsulatorString);
        }
    }


    

    /// <summary>
    /// Получить информацию, связанную с типом объекта
    /// </summary>
    /// <param name="typesData"></param>
    /// <param name="typeNumber"></param>
    /// <param name="objectsDataFolder"></param>
    /// <returns></returns>
    private List<string> GetObjectTypeDataByTypeNumber(List<string[]> typesData, string typeNumber, string objectsDataFolder)
    {
        if (typesData.Count == 0)
            return new List<string>();
        foreach (var data in typesData)
        {
            if (data[0] == typeNumber)
            {
                if (infoHolder.objectsFolderWhichDontNeedPrefabLoading.Contains(objectsDataFolder))
                    return data.Skip(1).ToList();
                var replacedLast = data.Last().Replace("Models", "Assets/Resources/Prefabs/Models");
                replacedLast = Path.ChangeExtension(replacedLast, ".prefab");
                var dataResult = data.Take(data.Count() - 1).ToList();
                dataResult.Add(replacedLast);
                return dataResult.Skip(1).ToList();
            }
        }
        throw new Exception($"No data of folder {objectsDataFolder} in typeData where number is {typeNumber}");       
    }

    /// <summary>
    /// Возвращает предзагруженную информацию типов объектов из конфига в виде словаря, где ключ - тип объекта, значение - содержимое файла Types.csv
    /// </summary>
    /// <param name="configPath"></param>
    /// <returns></returns>
    private Dictionary<string, List<string[]>> PreloadConfigTypesData(string configPath)
    {
        var preloadedTypes = new Dictionary<string, List<string[]>>();
        foreach(var objectsDataFolderName in infoHolder.preloadOrder)
            preloadedTypes[objectsDataFolderName] = new List<string[]>();
       
        //Preload config files
        foreach(var configDirName in infoHolder.preloadOrder)
        {
            var configFilesPath = Path.Combine(configPath, configDirName); // путь до конфига + конфиг объектов
            var type = infoHolder.GetTypeByDispatch(configDirName);
            try
            {
                using (var parser = new StreamReader(Path.Combine(configFilesPath, "Types.csv"), encoding: System.Text.Encoding.GetEncoding(1251)))
                {
                    parser.ReadLine(); // Skip first line
                    while (!parser.EndOfStream)
                    {
                        var args = parser.ReadLine().Split(';').ToArray();
                        preloadedTypes[configDirName].Add(args);
                    }
                }
            }
            catch(Exception e)
            {
                Debug.Log(e);
                continue;
            }
        }
        return preloadedTypes;
    }
}

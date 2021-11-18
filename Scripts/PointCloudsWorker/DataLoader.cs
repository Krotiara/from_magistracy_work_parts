using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;


public static class DataLoader<T>
{
 
    public static void Save<T>(T data, string path)
    {
        string json = JsonUtility.ToJson(data);
        StreamWriter writer = new StreamWriter(path, false, Encoding.GetEncoding("Windows-1251"));
        writer.Write(json);
        writer.Close();
    }

    public static T Load(string path)
    {
        using (StreamReader sr = new StreamReader(path, Encoding.GetEncoding("Windows-1251")))
        {
            string json = sr.ReadToEnd();
            T data = JsonUtility.FromJson<T>(json);
            return data;
        }
    }

    public static T LoadJson(string json)
    {
        T data = JsonUtility.FromJson<T>(json);
        return data;
    }

}

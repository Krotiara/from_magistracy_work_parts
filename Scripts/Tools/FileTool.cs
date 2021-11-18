using System;
using System.IO;
using UnityEngine;

public static class FileTool 
{
    public static string[] GetFilesList(string directory, string pattern)
    {
        return Directory.GetFiles(directory, pattern);
    }

    public static string GetFileName(string path)
    {
        return Path.GetFileNameWithoutExtension(path);
    }

    public static string GetDirectoryName(string directory)
    {
        return new DirectoryInfo(directory).Name;
    }

    public static string[] GetDirectoriesList(string directory)
    {
        return Directory.GetDirectories(directory);
    }

    public static void DeleteFolder(string directory)
    {
        Directory.Delete(directory, true);
    }

    public static void CheckDirectoryExists(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public static void DeleteFilesWithName(string path)
    {
        var filePatern = GetFileName(path);

        var filesToDelete = GetFilesList(Path.GetDirectoryName(path), filePatern+ ".*");

        foreach (var file in filesToDelete)
        {
            File.Delete(file);
        }
    }

    public static void CopyFilesWithName(string path, string newPath)
    {
        var filePatern = GetFileName(path);

        var filesToDelete = GetFilesList(Path.GetDirectoryName(path), filePatern + ".*");

        foreach (var file in filesToDelete)
        {
            File.Copy(file, newPath + Path.GetFileName(file));
        }
    }

    public static void MoveFiles(string oldPath, string newPath)
    {
        var filePatern = GetFileName(oldPath);

        var filesToDelete = GetFilesList(Path.GetDirectoryName(oldPath), filePatern + ".*");

        foreach (var file in filesToDelete)
        {
            File.Move(file, newPath + Path.GetFileName(file)); // костыль будто линия уехала на серввер
        }
    }

}

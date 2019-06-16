﻿using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

using System;
using System.IO;

static class EditorMenus
{
    [MenuItem("Tools/Update Sources")]
    static void LoadServerWaitingScenes()
    {
        if(EditorUtility.DisplayDialog("Update Sources", " Would you like to update sources?", "Yes", "No"))
        {
            string sourcePath = Path.Combine(Application.dataPath, "../../Snowball/");
            string targetPath = Path.Combine(Application.dataPath, "./Snowball/Scripts/Snowball/");

            CopyFilesInDirectory(sourcePath, targetPath);
        }
    }

    public static void CopyFilesInDirectory(string sourceDirectoryPath, string destinationDirectoryPath)
    {
        CopyFilesInDirectoryInternal(sourceDirectoryPath, destinationDirectoryPath);
        AssetDatabase.Refresh();
    }

    public static void CopyFilesInDirectoryInternal(string sourceDirectoryPath, string destinationDirectoryPath)
    {
            DirectoryInfo sDir = new DirectoryInfo(sourceDirectoryPath);
        DirectoryInfo dDir = new DirectoryInfo(destinationDirectoryPath);

        if (dDir.Exists == false)
        {
            dDir.Create();
            dDir.Attributes = sDir.Attributes;
        }

        foreach (FileInfo fInfo in sDir.GetFiles())
        {
            if(fInfo.Extension == ".cs")
            {
                fInfo.CopyTo(dDir.FullName + "/" + fInfo.Name, true);
            }
        }

        /*
        //Process Recursive
        foreach (DirectoryInfo dInfo in sDir.GetDirectories())
        {
            CopyFilesInDirectoryInternal(dInfo.FullName, dDir.FullName + "/" + dInfo.Name);
        }
        */
    }

}
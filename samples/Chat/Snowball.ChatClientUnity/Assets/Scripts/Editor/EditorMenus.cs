using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

using System;
using System.IO;

static class EditorMenus
{
	[MenuItem("Tools/Update Sources", false, 1)]
    static void UpdateSources()
    {
        if(EditorUtility.DisplayDialog("Update Sources", " Would you like to update sources?", "Yes", "No"))
        {
            string sourcePath = Path.Combine(Application.dataPath, "../../../../src/Snowball.Unity/Assets/Snowball");
            string targetPath = Path.Combine(Application.dataPath, "./Snowball/");

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

        //Process Recursive
        foreach (DirectoryInfo dInfo in sDir.GetDirectories())
        {
            if (dInfo.Name == "bin") continue;
            else if (dInfo.Name == "obj") continue;
            CopyFilesInDirectoryInternal(dInfo.FullName, dDir.FullName + "/" + dInfo.Name);
        }

    }

}
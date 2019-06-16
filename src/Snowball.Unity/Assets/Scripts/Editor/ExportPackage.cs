using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public static class UnityPackageExporter
{
	[MenuItem("Tools/Export Package", false, 2)]
	static public void ExportUnityPackage()
	{
		var exportRootPath = Path.GetDirectoryName(Application.dataPath);

		var exportDatas = Directory.GetDirectories(Application.dataPath + "/", "Snowball")
		.Select(dir =>
		{
			var inputFilePath = "Assets" + dir.Substring(Application.dataPath.Length);
			var outputFilePath = Path.Combine(exportRootPath, Path.GetFileName(inputFilePath) + ".unitypackage");
			return new ExportData { InputFilePath = inputFilePath, OutputFilePath = outputFilePath };
		})
		.ToList();
		
		foreach (var data in exportDatas)
		{
			EditorUtility.DisplayProgressBar("Export Unitypackages", data.FolderName, 0f);
			AssetDatabase.ExportPackage(data.InputFilePath, data.OutputFilePath, ExportPackageOptions.Recurse);
		}

		EditorUtility.ClearProgressBar();
		EditorUtility.DisplayDialog("Export Unitypackages", "Complete", "OK");

		System.Diagnostics.Process.Start(exportRootPath);
	}


	private class ExportData
	{
		public string FolderName { get { return Path.GetFileName(InputFilePath); } }
		public string InputFilePath;
		public string OutputFilePath;
	}
}
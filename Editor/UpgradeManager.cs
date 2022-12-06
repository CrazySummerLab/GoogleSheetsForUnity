using System.IO;
using GoogleSheetsForUnity.Runtime;
using UnityEditor;
using UnityEngine;

namespace GoogleSheetsForUnity.Editor
{
    [InitializeOnLoad]
    public class UpgradeManager 
    {
        [MenuItem("Tools/GoogleSheetsForUnity/ConnectionData.asset", false, 0)]
        public static void OpenGlobalSource()
        {
            CreateConnectionSources();
            ConnectionData GO = Resources.Load<ConnectionData>(DriveSpreadsheet.GlobalSources[0]);
            if (GO == null)
                Debug.Log("Unable to find Global Language at Assets/Resources/" + DriveSpreadsheet.GlobalSources[0] + ".asset");

            Selection.activeObject = GO;
        }

        public static void CreateConnectionSources()
        {
            if (DriveSpreadsheet.GlobalSources == null || DriveSpreadsheet.GlobalSources.Length == 0)
                return;

            Object GlobalSource = Resources.Load(DriveSpreadsheet.GlobalSources[0]);
            string sourcePath = null;
            
            if (GlobalSource != null)
            {
                if (GlobalSource is GameObject)
                {
                    // ConnectionData was a prefab before 2018.3, it should be converted to an ScriptableObject
                    sourcePath = AssetDatabase.GetAssetPath(GlobalSource);
                }
                else
                {
                    return;
                }
            }
            ConnectionData asset = ScriptableObject.CreateInstance<ConnectionData>();
            if (string.IsNullOrEmpty(sourcePath))
            {
                string ResourcesFolder = "Assets/Resources";//PluginPath.Substring(0, PluginPath.Length-"/ConnectionData".Length) + "/Resources";

                string fullresFolder = Application.dataPath + ResourcesFolder.Replace("Assets", "");
                if (!Directory.Exists(fullresFolder))
                    Directory.CreateDirectory(fullresFolder);

                sourcePath = ResourcesFolder + "/" + DriveSpreadsheet.GlobalSources[0] + ".asset";
            }
            else
            {
                sourcePath = sourcePath.Replace(".prefab", ".asset");
            }

            AssetDatabase.CreateAsset(asset, sourcePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}

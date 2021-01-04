using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MPCore
{
    public class FindProblematicAsset
    {
        [MenuItem("AssetDatabase/ZeroGUID")]
        public static void GetZeroGUID()
        {
            var allFiles = Directory.EnumerateFiles("Assets", "*").ToArray();
            var allAssets = new HashSet<string>(AssetDatabase.GetAllAssetPaths());

            for (int i = 0; i < allFiles.Length; ++i)
            {
                //Make sure we have forward slashes only
                var curFile = allFiles[i].Replace(@"\", "/");

                //Ignore .meta files as they're not part of AssetDatabase.GetAllAssetPaths();
                if (!curFile.EndsWith(".meta") && !allAssets.Contains(curFile))
                {
                    Debug.Log($"File path is in project, but not found inside AssetDatabase: {curFile}");
                }
            }
        }
    }
}

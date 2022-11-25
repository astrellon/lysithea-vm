#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

using System.IO;

namespace LysitheaVM.Unity
{
    [UnityEditor.AssetImporters.ScriptedImporter(1, "lys")]
    public class LysitheaTextImporter : UnityEditor.AssetImporters.ScriptedImporter
    {
        public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx)
        {
            TextAsset subAsset = new TextAsset(File.ReadAllText(ctx.assetPath));
            ctx.AddObjectToAsset("text", subAsset);
            ctx.SetMainObject(subAsset);
        }
    }
}
#endif
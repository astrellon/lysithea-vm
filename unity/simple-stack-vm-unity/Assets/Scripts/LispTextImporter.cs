using UnityEngine;
using UnityEditor;

using System.IO;

namespace SimpleStackVM.Unity
{
    [UnityEditor.AssetImporters.ScriptedImporter(1, "lisp")]
    public class LispTextImporter : UnityEditor.AssetImporters.ScriptedImporter
    {
        public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx)
        {
            TextAsset subAsset = new TextAsset(File.ReadAllText(ctx.assetPath));
            ctx.AddObjectToAsset("text", subAsset);
            ctx.SetMainObject(subAsset);
        }
    }
}
#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.UI;

using UnityEditor;
using System.IO;
using WorldAsSupport.WorldAPI;

namespace WorldAsSupport {
  [UnityEditor.AssetImporters.ScriptedImporter(1, "wasversion")]
  public class WorldVersionImporter : UnityEditor.AssetImporters.ScriptedImporter {
    public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx) {
      WorldVersion worldVersion;

      // read WorldVersion JSON into WorldVersionData
      using (StreamReader versionReader = new StreamReader(ctx.assetPath)) {
          string versionJson = versionReader.ReadToEnd();
          worldVersion = new WorldVersion(JsonUtility.FromJson<WorldVersionData>(versionJson));
      }

      // create Prefab parent
      string prefabName = ctx.assetPath.Replace(".wasversion", "");
      GameObject prefab = new GameObject(prefabName);
      
      // Add anchors as cubes
      foreach (WorldDocAnchorData anchorData in worldVersion.Data.anchors) {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        DestroyImmediate(cube.GetComponent<BoxCollider>());
        cube.AddComponent<Image>();
        cube.transform.SetParent(prefab.transform);
        cube.name = anchorData.item.itemId;
        cube.transform.position = anchorData.item.position;
      }

      ctx.AddObjectToAsset(prefabName, prefab);
      ctx.SetMainObject(prefab);

    }
  }
  [CustomEditor(typeof(WorldVersionImporter))]
  public class WorldVersionImporterEditor: UnityEditor.AssetImporters.ScriptedImporterEditor {
      public override void OnInspectorGUI() {
          base.ApplyRevertGUI();
      }
  }
}

#endif
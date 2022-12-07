#if UNITY_EDITOR
using UnityEditor;
using System.IO;

public class LoadPlaceables : EditorWindow {
    public const string PlaceableResourceDir = "Assets/Demos/Resources/Placeable/";

    [MenuItem("World-as-Support/Load Placeables")]
    static void Apply() {
        string path = EditorUtility.OpenFolderPanel("Demo Placeables Directory", "", "");
        string[] files = Directory.GetFiles(path);

        if (Directory.Exists(PlaceableResourceDir)) {
            Directory.Delete(PlaceableResourceDir, true);
        }
        Directory.CreateDirectory(PlaceableResourceDir);

        foreach (string file in files) {
            string fileName = Path.GetFileName(file);
            if (fileName.EndsWith(".prefab")) {
                File.Copy(file, PlaceableResourceDir + fileName);
            }
        }

        AssetDatabase.Refresh();
    }
}
#endif
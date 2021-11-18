#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class GetInsulatorStringsCoordinates : EditorWindow
{
    string towerNumber;
    string towerTag = "Tower";
    string stringTag = "String";
    string layerMask = "PowerLineObjects";
    int scaleFactor = 1000; // TODO: брать у каждой модели опоры

    [MenuItem("Window/Strings Coordinates")]
    static void ShowWindow()
    {
        var window = GetWindow<GetInsulatorStringsCoordinates>("Strings Coordinates");
        window.maxSize = new Vector2(400f, 200f);
        window.minSize = window.maxSize;
        window.Show();
    }

    void OnGUI()
    {
        CreateCSVWithInsStrCoorditates();
    }

    private void CreateCSVWithInsStrCoorditates()
    {
        towerNumber = EditorGUILayout.TextField("tower number:", towerNumber);
        EditorGUILayout.LabelField("Create CSV", EditorStyles.boldLabel);
        if (GUILayout.Button("Create CSV"))
        {
            WriteCoordinatesToCSV(towerNumber);
        }
        EditorGUILayout.LabelField("(Result will be in Console)");
    }

    private void WriteCoordinatesToCSV(string towerNumber)
    {
        var towers = GameObject.FindGameObjectsWithTag(towerTag);
        GameObject tower = null;
        foreach (var go in towers)
            if (go.name == towerNumber  && go.layer == LayerMask.NameToLayer(layerMask))
                tower = go;
        if (tower == null)
        {
            Debug.Log($"There is no game object with tag {towerTag} and named {towerNumber} on layer {layerMask}");
            return;
        }

        string result = "num;X;Y;Z\n";
        for (int i = 0; i< tower.transform.childCount; i++)
            {
                var child = tower.transform.GetChild(i);
                if (child.tag == stringTag)
                {

                    string log = child.name + ";"
                        + (child.transform.localPosition.x * scaleFactor).ToString() + ";"
                        + (child.transform.localPosition.y * scaleFactor).ToString() + ";"
                        + (child.transform.localPosition.z * scaleFactor).ToString();
                result += log + "\n";
                }
            }
        string localPath = Application.dataPath + "/StreamingAssets/Configs/"+towerNumber+"_str.csv";
        CableWalker.Simulator.Networking.Network.SaveData(result, localPath);
        Debug.Log($"Local Strings Coordinates of {towerNumber} saved to: \n{localPath}");
    }
}
#endif

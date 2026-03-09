using System.IO;
using UnityEditor;
using UnityEngine;

public static class AutoPrefabCreator
{
    private const string PrefabsFolderPath = "Assets/Prefabs";

    [MenuItem("Tools/Smart Prefab Creator")]
    public static void CreatePrefabsFromSelection()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            Debug.LogWarning("No GameObject selected. Please select at least one GameObject in the Hierarchy to create a prefab.");
            return;
        }

        if (!AssetDatabase.IsValidFolder(PrefabsFolderPath))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        foreach (GameObject go in selectedObjects)
        {
            string prefabPath = $"{PrefabsFolderPath}/{go.name}.prefab";

            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                Debug.LogWarning($"Prefab already exists at {prefabPath}. Skipping {go.name} to avoid overwrite.");
                continue;
            }

            bool prefabSuccess;
            PrefabUtility.SaveAsPrefabAssetAndConnect(go, prefabPath, InteractionMode.AutomatedAction, out prefabSuccess);

            if (prefabSuccess)
            {
                Debug.Log($"Successfully created and connected Prefab for '{go.name}' at {prefabPath}");
            }
            else
            {
                Debug.LogError($"Failed to create Prefab for '{go.name}' at {prefabPath}");
            }
        }
    }
}

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

[AddComponentMenu("")]
public class RuntimeSceneInjector : MonoBehaviour
{
    [Tooltip("Scenes to add temporarily to Build Settings for runtime loading.")]
    public List<SceneAsset> scenesToAdd;

    private static EditorBuildSettingsScene[] _originalScenes;

    [InitializeOnLoadMethod]
    private static void Init()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            #pragma warning disable CS0618
            // Find all RuntimeSceneInjector instances in open scenes
            var injectors = GameObject.FindObjectsOfType<RuntimeSceneInjector>();
            if (injectors.Length == 0) return;

            // Save original Build Settings
            _originalScenes = EditorBuildSettings.scenes;

            var scenePaths = injectors
                .SelectMany(i => i.scenesToAdd)
                .Where(s => s != null)
                .Select(s => AssetDatabase.GetAssetPath(s))
                .Distinct()
                .ToArray();

            var currentPaths = _originalScenes.Select(s => s.path).ToHashSet();

            var tempScenes = scenePaths
                .Where(p => !currentPaths.Contains(p))
                .Select(p => new EditorBuildSettingsScene(p, true))
                .ToArray();

            EditorBuildSettings.scenes = _originalScenes.Concat(tempScenes).ToArray();
            Debug.Log($"[RuntimeSceneInjector] Added {tempScenes.Length} temporary scenes to Build Settings.");
        }

        if (state == PlayModeStateChange.EnteredEditMode && _originalScenes != null)
        {
            EditorBuildSettings.scenes = _originalScenes;
            _originalScenes = null;
            Debug.Log("[RuntimeSceneInjector] Restored original Build Settings scenes.");
        }
    }
}
#endif

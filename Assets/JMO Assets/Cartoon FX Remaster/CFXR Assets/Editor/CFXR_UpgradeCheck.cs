using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class CFXR_UpgradeCheck
{
    const string THIS_GUID = "eab363b4799cd574f9124f77712c9ad9"; // this script's GUID

    [InitializeOnLoadMethod]
    static void DoCheck()
    {
        string skipCheckFilePath = AssetDatabase.GUIDToAssetPath(THIS_GUID);
        if (string.IsNullOrEmpty(skipCheckFilePath))
        {
            return;
        }
        skipCheckFilePath = Path.Combine(Path.GetDirectoryName(skipCheckFilePath), "skip_upgrade_check.txt");
        bool skip = File.Exists(skipCheckFilePath);

        if (skip)
        {
            return;
        }

        // Upgrade to new .cfxrshader file format
        string[] guidsToCheck =
        {
            "9f1e1c035a8fc0b4cab1d282789b282b", // CFXR Particle Distortion.shader
            "714e760b98304b44292e04841e05f258", // CFXR Particle Glow.shader
            "3dda44a7d9cec71439b5b8e20fcbccfb", // CFXR Particle Procedural Ring.shader
            "e198026df304af84a85675be636192ea", // CFXR Particle Ubershader.shader
            "00a9608d23679304bbbcf63f569012f8", // CFXR_BuildShaderPreprocessor.cs
        };

        foreach (string guid in guidsToCheck)
        {
            bool exists = !string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(guid));
            if (exists)
            {
                goto OLD_SHADER_EXIST;
            }
        }

        DeleteThisScript();
        return;

        OLD_SHADER_EXIST:
        int choice = EditorUtility.DisplayDialogComplex(
            "Cartoon FX Upgrade",
            "The latest version of Cartoon FX Remaster has changed how shaders are compiled based on the current active render pipeline to prevent issues in builds.\nThis change requires to delete the old shader files and the CFXR_BuildShaderPreprocessor script from the project.\n\nMake sure you have a backup of your project before proceeding, just in case.\n\nProceed to delete the files?", "Yes", "No", "No and don't show again"
        );

        switch (choice)
        {
            case 0: // Yes
                goto DO_REMOVE_FILES;
            case 2: // Don't show
                File.WriteAllText(skipCheckFilePath, "This file indicates that Cartoon FX Remaster shouldn't perform any check to delete files for the shader file format upgrade.");
                break;
        }

        return;

        DO_REMOVE_FILES:

        var deletedFiles = new List<string>();
        bool anyException = false;
        foreach (string guid in guidsToCheck)
        {
            string file = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(file)) continue;
            string filename = Path.GetFileName(file);

            try
            {
                AssetDatabase.DeleteAsset(file);
                deletedFiles.Add(file);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Cartoon FX Remaster] Couldn't delete the file: {filename}\nError returned: {exception}");
                anyException = true;
            }
        }

        if (!anyException)
        {
            // All files deleted successfully: delete this script too to not bloat the InitializeOnLoadMethod callback
            string thisPath = AssetDatabase.GUIDToAssetPath(THIS_GUID);
            if (DeleteThisScript())
            {
                deletedFiles.Add(thisPath);
            }
        }

        if (deletedFiles.Count > 0)
        {
            Debug.Log("[Cartoon FX Remaster] Successfully deleted the following files:\n- " + string.Join("\n- ", deletedFiles));
        }
    }

    static bool DeleteThisScript()
    {
        if (EditorPrefs.GetBool("CFXR_DontAutoDeleteUpgradeScript"))
        {
            return false;
        }

        string thisPath = AssetDatabase.GUIDToAssetPath(THIS_GUID);
        return !string.IsNullOrEmpty(thisPath) && AssetDatabase.DeleteAsset(thisPath);
    }
}

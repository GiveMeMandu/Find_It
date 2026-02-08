using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SpriteMetadataCopier : EditorWindow
{
    private Texture2D sourceSprite;
    private Texture2D targetSprite;

    [MenuItem("Tools/스프라이트 메타데이터 복사기")]
    static void Init()
    {
        var window = GetWindow<SpriteMetadataCopier>();
        window.Show();
    }

    void OnGUI()
    {
        sourceSprite = (Texture2D)EditorGUILayout.ObjectField("Source Sprite", sourceSprite, typeof(Texture2D), false);
        targetSprite = (Texture2D)EditorGUILayout.ObjectField("Target Sprite", targetSprite, typeof(Texture2D), false);

        if (GUILayout.Button("Copy Metadata"))
        {
            if (sourceSprite != null && targetSprite != null)
            {
                sourceSprite.CopyMetadata(targetSprite);
            }
        }
    }
}

public static class Texture2DExtensions
{
    public static void CopyMetadata(this Texture2D source, Texture2D destination)
    {
        // Create Data Provider for destination texture
        var destinationFactory = new SpriteDataProviderFactories();
        destinationFactory.Init();
        var destinationDataProvider = destinationFactory.GetSpriteEditorDataProviderFromObject(destination);
        destinationDataProvider.InitSpriteEditorDataProvider();

        // Create Data Provider for source texture
        var sourceFactory = new SpriteDataProviderFactories();
        sourceFactory.Init();
        var sourceDataProvider = sourceFactory.GetSpriteEditorDataProviderFromObject(source);
        sourceDataProvider.InitSpriteEditorDataProvider();

        // Get sprite rects of the source
        SpriteRect[] sourceSpriteRects = sourceDataProvider.GetSpriteRects();

        // Create a list of indices being used
        List<int> indices = new List<int>();
        foreach (var spriteRect in sourceSpriteRects)
        {
            Regex rx = new Regex(@".*_(?<suffix>\d*)$");
            Match match = rx.Match(spriteRect.name);
            if (match.Success)
            {
                indices.Add(int.Parse(match.Groups["suffix"].Value));
            }
        }

        // Create new SpriteRects that copy the source
        List<SpriteRect> newSpriteRects = new List<SpriteRect>();
        List<SpriteNameFileIdPair> newPairs = new List<SpriteNameFileIdPair>();
        foreach (var spriteRect in sourceSpriteRects)
        {
            Regex rx = new Regex(@"(?<prefix>.*)_(?<suffix>\d*)$");
            Match match = rx.Match(spriteRect.name);
            string newName;
            if (match.Success)
            {
                newName = $"{destination.name}_{match.Groups["suffix"].Value}";
            }
            else
            {
                int index = Enumerable.Range(0, indices.Max() + 2).Except(indices).Min();
                newName = $"{destination.name}_{index}";
                indices.Add(index);
            }

            // Create a new pair
            SpriteNameFileIdPair newPair = new SpriteNameFileIdPair();

            // Make a copy of the spriteRect
            SpriteRect newSpriteRect = spriteRect.Copy();

            // Update pair name and GUID
            newPair.name = newName;
            newPair.SetFileGUID(newSpriteRect.spriteID);

            // Add to the new lists
            newPairs.Add(newPair);
            newSpriteRects.Add(newSpriteRect);
        }

        // Set sprite rects
        destinationDataProvider.SetSpriteRects(newSpriteRects.ToArray());

        // Set name file id pairs
        var textureNameFileIdDataProvider = destinationDataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();
        textureNameFileIdDataProvider.SetNameFileIdPairs(newPairs);

        // Apply and save
        destinationDataProvider.Apply();
        var assetImporter = destinationDataProvider.targetObject as AssetImporter;
        assetImporter.SaveAndReimport();
    }
}

#endif
public static class SpriteRectExtensions
{
    public static SpriteRect Copy(this SpriteRect original, string newName = null)
    {
        SpriteRect copy = new SpriteRect
        {
            name = newName ?? original.name, // Используйте новое имя, если оно предоставлено, иначе используйте оригинальное имя
            rect = original.rect,
            alignment = original.alignment,
            border = original.border,
            pivot = original.pivot,
            spriteID = original.spriteID // Ensure this is handled correctly
        };

        return copy;
    }
}
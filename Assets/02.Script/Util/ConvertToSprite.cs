#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.IO;

public static class ConvertToSprite
{
    public static Texture2D TextureFromSprite(Sprite sprite)
    {
        if (sprite.rect.width != sprite.texture.width)
        {
            int texWid = (int)sprite.textureRect.width;
            int texHei = (int)sprite.textureRect.height;
            Texture2D newTex = new Texture2D(texWid, texHei);
            Color[] pixels = sprite.texture.GetPixels((int)sprite.textureRect.x, (int)sprite.textureRect.y, texWid, texHei);

            newTex.SetPixels(pixels);
            newTex.Apply();
            return newTex;
        }
        else
        {
            return sprite.texture;
        }
    }

    [MenuItem("Assets/선택한 Sprite를 Texture2D로 변환")]
    public static void ConvertSelectedSpriteToTexture2D()
    {
        // Get the selected sprite in the editor
        Sprite selectedSprite = Selection.activeObject as Sprite;
        if (selectedSprite == null)
        {
            Debug.LogError("Please select a sprite!");
            return;
        }

        // Convert sprite to texture
        Texture2D texture = TextureFromSprite(selectedSprite);

        // Create a path to save the texture
        string path = EditorUtility.SaveFilePanelInProject("Save Texture", selectedSprite.name + ".png", "png", "Save the texture as PNG");

        if (string.IsNullOrEmpty(path))
            return;

        // Convert texture to PNG data
        byte[] pngData = texture.EncodeToPNG();
        if (pngData != null)
        {
            File.WriteAllBytes(path, pngData);
            AssetDatabase.Refresh();
            Debug.Log("Texture saved to " + path);
        }
        else
        {
            Debug.LogError("Failed to convert texture to PNG!");
        }
    }
}
#endif

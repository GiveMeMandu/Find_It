using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public static class InputBindingUtility
{
    // InputAction에서 바인딩 정보 읽기
    public static string GetBindingDisplayString(InputAction action, bool isGamepad)
    {
        if (action == null)
        {
            Debug.LogError("InputAction이 null입니다.");
            return string.Empty;
        }

        return action.GetBindingDisplayString(
            isGamepad ? 0 : 1,
            InputBinding.DisplayStringOptions.DontIncludeInteractions
        );
    }

    // 스프라이트 이름 생성 로직
    public static string GenerateSpriteName(string bindingText, bool isGamepad)
    {
        string normalizedText = bindingText.Replace("/", "_").Replace(" ", "_");
        string prefix = isGamepad ? "Gamepad_" : "";
        return $"{prefix}{normalizedText}";
    }

    // TMP_SpriteAsset에서 스프라이트 존재 확인
    public static bool HasSprite(string spriteName)
    {
        TMP_SpriteAsset spriteAsset = TMP_Settings.defaultSpriteAsset;

#if UNITY_EDITOR
        // 에디터에서는 TMP Settings 에셋을 직접 로드
        if (spriteAsset == null)
        {
            spriteAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(
                "Assets/NormalResource/Fonts/InputPrompts.asset");
        }
#endif

        if (spriteAsset == null || spriteAsset.spriteGlyphTable == null)
            return false;

        return spriteAsset.spriteGlyphTable.Any(glyph =>
            glyph.sprite != null && glyph.sprite.name == spriteName);
    }

    // InputAction에서 최종 입력 텍스트 생성 (스프라이트 태그 또는 텍스트)
    public static string GetInputText(InputAction action, bool isGamepad)
    {
        string bindingDisplayString = GetBindingDisplayString(action, isGamepad);
        int index = isGamepad ? 0 : 1;
        var binding = action.bindings.ElementAtOrDefault(index);
        if (binding.isComposite)
        {
            string compositeText = "";
            foreach (var childDisplayString in bindingDisplayString.Split('/'))
            {
                string childSpriteName = GenerateSpriteName(childDisplayString, isGamepad);

                if (HasSprite(childSpriteName))
                {
                    compositeText += $"<sprite name=\"{childSpriteName}\">";
                }
                else
                {
                    compositeText += childDisplayString;
                }
            }
            return compositeText.TrimEnd();
        }
        if (string.IsNullOrEmpty(bindingDisplayString))
            return string.Empty;

        string spriteName = GenerateSpriteName(bindingDisplayString, isGamepad);

        if (HasSprite(spriteName))
        {
            return $"<sprite name=\"{spriteName}\">";
        }

        return bindingDisplayString;
    }

    // 특정 바인딩 인덱스에 대한 텍스트 생성
    public static string GetInputText(InputAction action, int bindingIndex, bool isGamepad)
    {
        string bindingDisplayString = action.GetBindingDisplayString(bindingIndex, InputBinding.DisplayStringOptions.DontIncludeInteractions);

        if (string.IsNullOrEmpty(bindingDisplayString))
            return string.Empty;

        string spriteName = GenerateSpriteName(bindingDisplayString, isGamepad);

        if (HasSprite(spriteName))
        {
            return $"<sprite name=\"{spriteName}\">";
        }

        return bindingDisplayString;
    }
}

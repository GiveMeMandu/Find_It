using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Small compatibility helper to use the new Input System APIs in places
/// that previously used UnityEngine.Input (legacy). Returns safe defaults
/// if the new Input System devices are not available.
/// Placed in Plugins so early-compiled assemblies can reference it.
/// </summary>
public static class InputCompatibility
{
    private static Vector3? _forcedMousePosition;

    public static void SetForcedMousePosition(Vector3? pos) => _forcedMousePosition = pos;

    public static Vector3 MousePosition()
    {
        if (_forcedMousePosition.HasValue) return _forcedMousePosition.Value;
        if (Mouse.current != null)
        {
            var p = Mouse.current.position.ReadValue();
            return new Vector3(p.x, p.y, 0f);
        }
        return Vector3.zero;
    }

    public static Vector2 MousePosition2D()
    {
        if (_forcedMousePosition.HasValue) return (Vector2)_forcedMousePosition.Value;
        if (Mouse.current != null)
            return Mouse.current.position.ReadValue();
        return Vector2.zero;
    }

    public static bool GetMouseButton(int button)
    {
        if (Mouse.current == null) return false;
        switch (button)
        {
            case 0: return Mouse.current.leftButton.isPressed;
            case 1: return Mouse.current.rightButton.isPressed;
            case 2: return Mouse.current.middleButton.isPressed;
            default: return false;
        }
    }

    public static bool GetMouseButtonDown(int button)
    {
        if (Mouse.current == null) return false;
        switch (button)
        {
            case 0: return Mouse.current.leftButton.wasPressedThisFrame;
            case 1: return Mouse.current.rightButton.wasPressedThisFrame;
            case 2: return Mouse.current.middleButton.wasPressedThisFrame;
            default: return false;
        }
    }

    public static bool GetMouseButtonUp(int button)
    {
        if (Mouse.current == null) return false;
        switch (button)
        {
            case 0: return Mouse.current.leftButton.wasReleasedThisFrame;
            case 1: return Mouse.current.rightButton.wasReleasedThisFrame;
            case 2: return Mouse.current.middleButton.wasReleasedThisFrame;
            default: return false;
        }
    }

    public static float GetAxis(string axisName)
    {
        if (Mouse.current == null) return 0f;
        if (string.Equals(axisName, "Mouse X", StringComparison.OrdinalIgnoreCase))
            return Mouse.current.delta.ReadValue().x;
        if (string.Equals(axisName, "Mouse Y", StringComparison.OrdinalIgnoreCase))
            return Mouse.current.delta.ReadValue().y;
        if (axisName != null && axisName.IndexOf("Scroll", StringComparison.OrdinalIgnoreCase) >= 0)
            return Mouse.current.scroll.ReadValue().y;
        return 0f;
    }

    public static bool GetKeyDown(KeyCode keyCode)
    {
        if (Keyboard.current == null) return false;

        // Try to map common KeyCode names to InputSystem Key names
        string name = keyCode.ToString();
        string lower = name.ToLowerInvariant();
        if (lower.StartsWith("alpha"))
            lower = "digit" + lower.Substring(5);
        else if (lower.StartsWith("keypad"))
            lower = "numpad" + lower.Substring(6);

        try
        {
            if (Enum.TryParse<UnityEngine.InputSystem.Key>(lower, true, out var k))
            {
                var control = Keyboard.current[k];
                if (control != null)
                    return control.wasPressedThisFrame;
            }
        }
        catch { }

        // Fallback common keys
        switch (keyCode)
        {
            case KeyCode.Space:
                return Keyboard.current.spaceKey.wasPressedThisFrame;
            case KeyCode.Return:
            case KeyCode.KeypadEnter:
                return Keyboard.current.enterKey.wasPressedThisFrame;
            default:
                break;
        }

        return false;
    }

    public static bool GetKey(KeyCode keyCode)
    {
        if (Keyboard.current == null) return false;

        string name = keyCode.ToString();
        string lower = name.ToLowerInvariant();
        if (lower.StartsWith("alpha"))
            lower = "digit" + lower.Substring(5);
        else if (lower.StartsWith("keypad"))
            lower = "numpad" + lower.Substring(6);

        try
        {
            if (Enum.TryParse<UnityEngine.InputSystem.Key>(lower, true, out var k))
            {
                var control = Keyboard.current[k];
                if (control != null)
                    return control.isPressed;
            }
        }
        catch { }

        switch (keyCode)
        {
            case KeyCode.Space:
                return Keyboard.current.spaceKey.isPressed;
            default:
                break;
        }

        return false;
    }

    public static bool GetKeyUp(KeyCode keyCode)
    {
        if (Keyboard.current == null) return false;

        string name = keyCode.ToString();
        string lower = name.ToLowerInvariant();
        if (lower.StartsWith("alpha"))
            lower = "digit" + lower.Substring(5);
        else if (lower.StartsWith("keypad"))
            lower = "numpad" + lower.Substring(6);

        try
        {
            if (Enum.TryParse<UnityEngine.InputSystem.Key>(lower, true, out var k))
            {
                var control = Keyboard.current[k];
                if (control != null)
                    return control.wasReleasedThisFrame;
            }
        }
        catch { }

        switch (keyCode)
        {
            case KeyCode.Space:
                return Keyboard.current.spaceKey.wasReleasedThisFrame;
            default:
                break;
        }

        return false;
    }
}

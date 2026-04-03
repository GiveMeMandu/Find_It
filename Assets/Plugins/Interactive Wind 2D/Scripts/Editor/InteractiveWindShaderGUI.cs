using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace InteractiveWind2D
{
    public class InteractiveWindShaderGUI : ShaderGUI
    {
        static bool textureSetup;
        static bool windSetup;
        static bool interactionSetup;
        static bool otherInfo;

        static bool showMainTexture;
        static bool showBonus;
        static bool showLit;

        bool isOpen;

        static Shader currentDefaultShader;
        static float[] defaultFloats;
        static Vector4[] defaultVectors;
        static Color[] defaultColors;

        float lastParallax;
        float lastLocalWind;

        public static bool showUpgradeInfo;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            //Initialize:
            Material mat = ((Material)materialEditor.target);
            Shader shader = mat.shader;
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.richText = true;

            #region Reset Prep
            if (currentDefaultShader != shader || defaultFloats == null)
            {
                currentDefaultShader = shader;

                int propCount = shader.GetPropertyCount();

                defaultFloats = new float[propCount];
                defaultVectors = new Vector4[propCount];
                defaultColors = new Color[propCount];

                Material defaultMaterial = new Material(shader);

                for (int n = 0; n < propCount; n++)
                {
                    string propName = shader.GetPropertyName(n);

                    switch (shader.GetPropertyType(n))
                    {
                        case (ShaderPropertyType.Float):
                            defaultFloats[n] = defaultMaterial.GetFloat(propName);
                            break;
                        case (ShaderPropertyType.Range):
                            defaultFloats[n] = defaultMaterial.GetFloat(propName);
                            break;
                        case (ShaderPropertyType.Vector):
                            defaultVectors[n] = defaultMaterial.GetVector(propName);
                            break;
                        case (ShaderPropertyType.Color):
                            defaultColors[n] = defaultMaterial.GetColor(propName);
                            break;
                        default:
                            break;
                    }
                }
            }
            #endregion

            //Header:
            EditorGUILayout.BeginVertical("Helpbox");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("<size=17><b>Interactive Wind 2D</b></size>", style);
            if(shader.name.EndsWith("Lit"))
            {
                if (GUILayout.Button("Switch to Unlit"))
                {
                    mat.shader = Shader.Find("Interactive Wind 2D/Wind Uber Unlit");
                    return;
                }
            }
            else
            {
                if (GUILayout.Button("Switch to 2D Lit"))
                {
                    mat.shader = Shader.Find("Interactive Wind 2D/Wind Uber Lit");
                    return;
                }
            }

            string colorHex = EditorGUIUtility.isProSkin ? "#FFFFFF77" : "#00000077"; //For faded text.

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField(" ", GUILayout.Height(1));
            InteractiveWindShaderGUI.UpgradeInformation();
            EditorGUILayout.LabelField(" ", GUILayout.Height(1));
            InteractiveWind2DEditor.DisplayHints(ref textureSetup, "1. Texture Setup",
                "- Do <b>not</b> use <b>Sprite Sheets</b> or <b>Atlases</b>.",
                "- Set the texture's <b>Mesh Type</b> to <b>Full Rect</b>.",
                " ",
                "<b>How to fix pixel clipping:</b>",
                "The sprite needs more mesh-space to bend.",
                "Either <b>expand</b> the texture horizontally with empty pixels.",
                "Or enable <b>UV Scale</b> and shrink the sprite."
                );
            EditorGUILayout.LabelField(" ", GUILayout.Height(1));
            InteractiveWind2DEditor.DisplayHints(ref windSetup, "2. Wind Setup",
                "<b>1.</b> Make sure to check the <b>Texture Setup</b> above.",
                "<b>2.</b> Have a single <b>WindManager2D</b> component in your scene.",
                " ",
                "The WindManager updates the global wind settings.",
                "Only have a single one active at a time."
                );
            EditorGUILayout.LabelField(" ", GUILayout.Height(1));

            InteractiveWind2DEditor.DisplayHints(ref interactionSetup, "3. Interaction Setup <color=" + colorHex + ">(optional)</color>",
                "<b>1.</b> Add the <b>InteractiveWind2D</b> component to the sprite.",
                "<b>2.</b> Adjust the box collider which will detect physical interaction."
                );
            EditorGUILayout.LabelField(" ", GUILayout.Height(1));

            InteractiveWind2DEditor.DisplayHints(ref otherInfo, "Other Information",
                " ",
                "<b>Fixing Parallax:</b>",
                "- Add the <b>WindParallax</b> component to the sprite.",
                "- Enable <b>Is Parallax</b> in the material.",
                " ",
                "<b>Note:</b>",
                "If you do not have any <b>scripts</b> attached to your sprite renderer.",
                "Attach the <b>ShaderInstancer</b> script to it."
                );
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            for(int n = 0; n < properties.Length; n++)
            {
                //Get Prop:
                MaterialProperty prop = properties[n];

                //Skip:
                if (prop.name == "_AlphaTex" || prop.name == "_texcoord") continue;
                if (prop.name == "_WindXPosition" && lastParallax < 0.5f) continue;
                if(lastLocalWind < 0.5f)
                {
                    switch(prop.name)
                    {
                        case ("_WindNoiseScale"):
                            continue;
                        case ("_WindNoiseSpeed"):
                            continue;
                        case ("_WindMinIntensity"):
                            continue;
                        case ("_WindMaxIntensity"):
                            continue;
                    }
                }

                //Categories:
                switch(prop.name)
                {
                    case ("_MainTex"):
                        EditorGUILayout.BeginVertical("Helpbox");
                        ToggleCategory("Main Texture", ref showMainTexture, ref style);
                        break;
                    case ("_Color"):
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space();
                        EditorGUILayout.BeginVertical("Helpbox");
                        ToggleCategory("Bonus", ref showBonus, ref style);
                        break;
                    case ("_MaskMap"):
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space();
                        EditorGUILayout.BeginVertical("Helpbox");
                        ToggleCategory("2D Lit Settings", ref showLit, ref style);
                        break;
                }

                if (!isOpen && prop.name.StartsWith("_Enable") == false) continue;

                //Display:
                string displayName = prop.displayName;

                //Remove Prefix:
                string[] prefixSplit = displayName.Split(':');
                if (prefixSplit.Length > 1)
                {
                    displayName = prefixSplit[1].Substring(1);
                }

                GUIContent displayContent = new GUIContent(displayName, prop.name + "  (C#)");

                //Prop Type:
                ShaderPropertyType propType = ShaderPropertyType.Texture;
                if (shader.GetPropertyCount() >= properties.Length && prop == properties[n])
                {
                    propType = shader.GetPropertyType(n);
                }

                //Display:
                EditorGUILayout.BeginHorizontal();
                if (displayName == "Flip")
                {
                    EditorGUILayout.PrefixLabel(displayContent);
                    prop.floatValue = EditorGUILayout.IntPopup((int)(prop.floatValue), new string[] { "Default", "Flipped" }, new int[] { 0, -1 });
                }
                else if (propType == ShaderPropertyType.Vector)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(displayContent);

                    Vector4 value = prop.vectorValue;
                    float oldWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 15f;
                    value.x = EditorGUILayout.FloatField(" X", value.x);
                    value.y = EditorGUILayout.FloatField(" Y", value.y);
                    value.z = value.w = 0;
                    EditorGUILayout.EndHorizontal();
                    EditorGUIUtility.labelWidth = oldWidth;

                    prop.vectorValue = value;
                }
                else if (propType == ShaderPropertyType.Texture)
                {
                    prop.textureValue = (Texture)EditorGUILayout.ObjectField(displayContent, prop.textureValue, typeof(Texture), false);
                }else if (prop.name.StartsWith("_Enable"))
                {
                    EditorGUILayout.EndHorizontal();
                    ToggleShader(materialEditor, prop, style);
                    EditorGUILayout.BeginHorizontal();
                }
                else
                {
                    materialEditor.ShaderProperty(prop, displayContent);
                }

                #region Reset
                if (propType != ShaderPropertyType.Texture && displayName != "Shader Space" && prop.name.StartsWith("_Enable") == false && prop.name != "_WindIsParallax" && prop.name != "_WindLocalWind" && prop.name != "_WindHighQualityNoise")
                {
                    GUIContent resetButton = new GUIContent();
                    resetButton.text = "R";
                    resetButton.tooltip = "Resets the property.";

                    if (propType == ShaderPropertyType.Vector) //Vector:
                    {
                        Vector4 defaultValue = defaultVectors[n];

                        if (prop.vectorValue == defaultValue)
                        {
                            GUI.color = new Color(1, 1, 1, 0.5f);
                            GUILayout.Toolbar(0, new GUIContent[] { resetButton }, GUILayout.Width(20));
                        }
                        else
                        {
                            if (GUILayout.Button(resetButton, GUILayout.Width(20)))
                            {
                                prop.vectorValue = defaultValue;
                            }
                        }
                    }
                    else if (propType == ShaderPropertyType.Color) //Color:
                    {
                        Color defaultValue = defaultColors[n];

                        if (prop.colorValue == defaultValue)
                        {
                            GUI.color = new Color(1, 1, 1, 0.5f);
                            GUILayout.Toolbar(0, new GUIContent[] { resetButton }, GUILayout.Width(20));
                        }
                        else
                        {
                            if (GUILayout.Button(resetButton, GUILayout.Width(20)))
                            {
                                prop.colorValue = defaultValue;
                            }
                        }
                    }
                    else
                    {
                        float defaultValue = defaultFloats[n];

                        if (prop.floatValue == defaultValue)
                        {
                            GUI.color = new Color(1, 1, 1, 0.5f);
                            GUILayout.Toolbar(0, new GUIContent[] { resetButton }, GUILayout.Width(20));
                        }
                        else
                        {
                            if (GUILayout.Button(resetButton, GUILayout.Width(20)))
                            {
                                prop.floatValue = defaultValue;
                            }
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                #endregion

                //Suffix & Lines:
                if (isOpen)
                {
                    GUI.color = new Color(1, 1, 1, 0.7f);
                    switch (prop.name)
                    {
                        case ("_EnableUVScale"):
                            EditorGUILayout.LabelField("- Shrinks the sprite's <b>uv</b> size.", style);
                            EditorGUILayout.LabelField("- Can be used to fix pixel <b>clipping</b>.", style);
                            Lines();
                            break;
                        case ("_EnableBrightness"):
                            EditorGUILayout.LabelField("- Adjusts the sprite's <b>brightness</b>.", style);
                            Lines();
                            break;
                        case ("_EnableContrast"):
                            EditorGUILayout.LabelField("- Adjusts the sprite's <b>contrast</b>.", style);
                            Lines();
                            break;
                        case ("_EnableSaturation"):
                            EditorGUILayout.LabelField("- Adjusts the sprite's <b>saturation</b>.", style);
                            Lines();
                            break;
                        case ("_EnableHue"):
                            EditorGUILayout.LabelField("- Adjusts the sprite's <b>hue</b>.", style);
                            Lines();
                            break;
                        case ("_EnableWind"):
                            EditorGUILayout.LabelField("- <b>Bends</b> and <b>squishes</b> the sprite.", style);
                            Lines();
                            break;
                        case ("_WindRotationWindFactor"):
                            Lines();
                            break;
                        case ("_WindSquishWindFactor"):
                            Lines();
                            break;
                        case ("_WindMaxIntensity"):
                            EditorGUILayout.LabelField("- If enabled uses <b>local</b> settings for <b>wind</b>.", style);
                            EditorGUILayout.LabelField("- If disabled uses <b>Wind Manager</b> settings for <b>wind</b>.", style);
                            Lines();
                            break;
                        case ("_WindFlip"):
                            if(prop.floatValue < -0.5f)
                            {
                                EditorGUILayout.LabelField("- Use <b>Flipped</b> for <b>hanging</b> objects.", style);
                            }
                            break;
                        case ("_WindXPosition"):
                            EditorGUILayout.LabelField("- This fixes <b>parallax</b> issues for back- and foreground objects.", style);
                            EditorGUILayout.LabelField("- Requires the <b>WindParallax</b> component attached to this sprite.", style);
                            Lines();
                            break;
                        case ("_WindIsParallax"):
                            lastParallax = prop.floatValue;
                            if(lastParallax < 0.5f)
                            {
                                Lines();
                            }
                            break;
                        case ("_WindLocalWind"):
                            lastLocalWind = prop.floatValue;
                            if (lastLocalWind < 0.5f)
                            {
                                Lines();
                            }
                            break;
                        case ("_WindHighQualityNoise"):
                            if (prop.floatValue > 0.5f)
                            {
                                EditorGUILayout.LabelField("- Costs more <b>gpu performance</b> but is less repetitive.", style);
                                EditorGUILayout.LabelField("- Not recommended on low-end mobile devices.", style);
                            }
                            Lines();
                            break;
                        case ("_EnableUVDistort"):
                            EditorGUILayout.LabelField("- <b>Distorts</b> the sprite with a scrolling noise texture.", style);
                            EditorGUILayout.LabelField("- Works well in combination with the <b>wind</b> shader.", style);
                            Lines();
                            break;
                        case ("_ShaderSpace"):
                            EditorGUILayout.LabelField("- Set to <b>world</b> to blend the distortion of multiple sprites.", style);
                            Lines();
                            break;
                        case ("_UVDistortFade"):
                            Lines();
                            break;
                        case ("_UVDistortShaderMask"):
                            EditorGUILayout.LabelField("- Use a mask to fade distortion where it makes sense.", style);
                            EditorGUILayout.LabelField("- You can use this to prevent grass <b>sliding</b> on the ground.", style);
                            Lines();
                            break;
                        case ("_UVDistortTo"):
                            Lines();
                            break;
                    }
                    GUI.color = Color.white;
                }
            }
            EditorGUILayout.EndVertical();

            DisplayFinalInformation();
        }

        void ToggleCategory(string title, ref bool toggleVariable, ref GUIStyle style)
        {
            GUIStyle button = new GUIStyle(GUI.skin.button);
            button.richText = true;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("<b><size=14>" + title + "</size></b>", style);
            if (GUILayout.Button("<size=10>" + (toggleVariable ? "▼" : "▲") + "</size>", button, GUILayout.Width(20)))
            {
                toggleVariable = !toggleVariable;
            }
            EditorGUILayout.EndHorizontal();

            isOpen = toggleVariable;
        }
        void ToggleShader(MaterialEditor materialEditor,MaterialProperty prop, GUIStyle style)
        {
            float brightness = prop.floatValue > 0.5f ? (EditorGUIUtility.isProSkin ? 0f : (prop.floatValue == 1.1f ? 0.3f : 0.5f)) : 1f;
            GUI.color = new Color(brightness, brightness, brightness, 1);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("Helpbox");

            GUI.color = new Color(1, 1, 1, prop.floatValue > 0.5f ? 1f : 0.8f);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("<b><size=14>" + prop.displayName.Replace("Enable ","") + "</size></b>", style, style);
            materialEditor.ShaderProperty(prop, GUIContent.none);

            bool toggleVariable = false;
            if (prop.floatValue > 0.5f)
            {
                GUIStyle button = new GUIStyle(GUI.skin.button);
                button.richText = true;

                toggleVariable = prop.floatValue < 1.05f;

                if (GUILayout.Button("<size=10>" + (toggleVariable ? "▼" : "▲") + "</size>", button, GUILayout.Width(20)))
                {
                    toggleVariable = !toggleVariable;

                    prop.floatValue = toggleVariable ? 1f : 1.1f;
                }
            }
            EditorGUILayout.EndHorizontal();
            isOpen = toggleVariable;
        }

        public static void DisplayFinalInformation()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("Helpbox");

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.richText = true;
            style.alignment = TextAnchor.MiddleLeft;

            GUIStyle linkStyle = new GUIStyle(GUI.skin.label);
            linkStyle.richText = true;
            linkStyle.alignment = TextAnchor.MiddleLeft;
            linkStyle.normal.textColor = linkStyle.focused.textColor = linkStyle.hover.textColor = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.9f, 1f, 1) : new Color(0.1f, 0.2f, 0.4f, 1);
            linkStyle.active.textColor = EditorGUIUtility.isProSkin ? new Color(0.6f, 0.8f, 1f, 1) : new Color(0.15f, 0.4f, 0.6f, 1);

            EditorGUILayout.BeginHorizontal();
            GUI.color = new Color(1, 1, 1, 1);
            EditorGUILayout.LabelField("<b>Contact:</b>", style, GUILayout.Width(100));
            EditorGUILayout.SelectableLabel("<b>ekincantascontact@gmail.com</b>", linkStyle, GUILayout.Height(16));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUI.color = new Color(1, 1, 1, 1);
            EditorGUILayout.LabelField("<b>Documentation:</b>", style, GUILayout.Width(100));
            if (GUILayout.Button("<b><size=11>https://ekincantas.com/interactive-wind-2d/</size></b>", linkStyle))
            {
                Application.OpenURL("https://ekincantas.com/interactive-wind-2d/");
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            GUI.color = new Color(1, 1, 1, 0.75f);
            EditorGUILayout.LabelField("<b>Thank you for using my asset.</b>", style);
            EditorGUILayout.LabelField("<b>Reviews are appreciated.</b>", style);
            EditorGUILayout.EndVertical();
            GUI.color = new Color(1, 1, 1, 1);
        }

        public static void Lines()
        {
            GUI.color = new Color(1, 1, 1, 0.5f);
            EditorGUILayout.LabelField("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - ", GUILayout.Height(9));
            GUI.color = new Color(1, 1, 1, 1);
        }

        public static void UpgradeInformation()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.richText = true;

            GUIStyle button = new GUIStyle(GUI.skin.button);
            button.richText = true;

            if (showUpgradeInfo)
            {
                GUI.color = new Color(0.8f, 1f, 0.9f, 0.7f);
            }
            else
            {
                GUI.color = new Color(0.8f, 1f, 0.9f, 0.5f);
            }

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.BeginHorizontal();
            GUI.color = new Color(0.8f,1f,0.9f);
            EditorGUILayout.LabelField("<b>Discount</b> on <b>Sprite Shaders Ultimate</b>", style);
            if (GUILayout.Button("<size=10>" + (showUpgradeInfo ? "▼" : "▲") + "</size>", button, GUILayout.Width(20)))
            {
                showUpgradeInfo = !showUpgradeInfo;
            }
            EditorGUILayout.EndHorizontal();


            if (showUpgradeInfo == true)
            {
                GUIStyle linkStyle = new GUIStyle(GUI.skin.label);
                linkStyle.richText = true;
                linkStyle.alignment = TextAnchor.MiddleLeft;
                linkStyle.normal.textColor = linkStyle.focused.textColor = linkStyle.hover.textColor = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.9f, 1f, 1) : new Color(0.1f, 0.2f, 0.4f, 1);
                linkStyle.active.textColor = EditorGUIUtility.isProSkin ? new Color(0.6f, 0.8f, 1f, 1) : new Color(0.15f, 0.4f, 0.6f, 1);


                GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, 0.7f);
                EditorGUILayout.LabelField("Unlock <b>lots</b> of additional <b>shaders</b> and <b>features</b>.", style);
                EditorGUILayout.LabelField("Owning this asset grants you a <b>discount</b> on Sprite Shaders Ultimate.", style);
                EditorGUILayout.LabelField("", style);
                if (GUILayout.Button("<size=14>View <b>Sprite Shaders Ultimate</b></size>", linkStyle))
                {
                    Application.OpenURL("https://assetstore.unity.com/packages/slug/158988");
                }
            }

            GUI.color = Color.white;
            EditorGUILayout.EndVertical();
        }
    }
}
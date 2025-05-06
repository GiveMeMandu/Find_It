/*
*	Copyright (c) RainyRizzle Inc. All rights reserved
*	Contact to : www.rainyrizzle.com , contactrainyrizzle@gmail.com
*
*	This file is part of [AnyPortrait].
*
*	AnyPortrait can not be copied and/or distributed without
*	the express permission of [Seungjik Lee] of [RainyRizzle team].
*
*	It is illegal to download files from other than the Unity Asset Store and RainyRizzle homepage.
*	In that case, the act could be subject to legal sanctions.
*/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Collections.Generic;

using AnyPortrait;

namespace AnyPortrait
{
	public class apDialog_SelectShaderProp : EditorWindow
	{
		// Members
		//--------------------------------------------------------------
		public delegate void FUNC_SELECT_SHADER_PROP(bool isSuccess, object loadKey, List<PropInfo> props, apMaterialSet calledMaterialSet);

		private static apDialog_SelectShaderProp s_window = null;

		private apEditor _editor = null;
		private object _loadKey = null;
		private FUNC_SELECT_SHADER_PROP _funcResult = null;

		private apMaterialSet _matSet = null;

		//프로퍼티 정보
		public class PropInfo
		{
			public string _name = "";
			public apMaterialSet.SHADER_PROP_TYPE _type = apMaterialSet.SHADER_PROP_TYPE.Int;

			public PropInfo(string name, apMaterialSet.SHADER_PROP_TYPE type)
			{
				_name = name;
				_type = type;
			}
		}

		private List<PropInfo> _propInfos = null;
		private List<PropInfo> _selectedInfos = null;

		private Vector2 _scrollList = new Vector2();

		// Show Window
		//--------------------------------------------------------------
		public static object ShowDialog(apEditor editor, apMaterialSet materialSet, FUNC_SELECT_SHADER_PROP funcResult)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null || editor._portrait._controller == null)
			{
				return null;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_SelectShaderProp), true, "Select Property", true);
			apDialog_SelectShaderProp curTool = curWindow as apDialog_SelectShaderProp;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 350;
				int height = 400;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.Init(editor, loadKey, materialSet, funcResult);

				return loadKey;
			}
			else
			{
				return null;
			}

		}

		private static void CloseDialog()
		{
			if (s_window != null)
			{
				try
				{
					s_window.Close();
				}
				catch (Exception ex)
				{
					Debug.LogError("Close Exception : " + ex);

				}

				s_window = null;
			}
		}

		// Init
		//--------------------------------------------------------------
		public void Init(apEditor editor, object loadKey, apMaterialSet materialSet, FUNC_SELECT_SHADER_PROP funcResult)
		{
			_editor = editor;
			_loadKey = loadKey;
			_funcResult = funcResult;

			_matSet = materialSet;

			_scrollList = Vector2.zero;

			

			HashSet<string> propNames = new HashSet<string>();
			_propInfos = new List<PropInfo>();
			_selectedInfos = new List<PropInfo>();

			//기존에 추가된 프로퍼티들은 보여주지 말자
			int nMatSetProps = _matSet._propertySets != null ? _matSet._propertySets.Count : 0;
			if(nMatSetProps > 0)
			{
				apMaterialSet.PropertySet propSet = null;
				for (int i = 0; i < nMatSetProps; i++)
				{
					propSet = _matSet._propertySets[i];
					
					if(!propNames.Contains(propSet._name))
					{
						propNames.Add(propSet._name);
					}
				}
			}

			//이제 쉐이더별로 프로퍼티를 찾아서 등록하자
			FindProperties(_matSet._shader_Normal_AlphaBlend, _propInfos, propNames);
			FindProperties(_matSet._shader_Normal_Additive, _propInfos, propNames);
			FindProperties(_matSet._shader_Normal_SoftAdditive, _propInfos, propNames);
			FindProperties(_matSet._shader_Normal_Multiplicative, _propInfos, propNames);
			FindProperties(_matSet._shader_Clipped_AlphaBlend, _propInfos, propNames);
			FindProperties(_matSet._shader_Clipped_Additive, _propInfos, propNames);
			FindProperties(_matSet._shader_Clipped_SoftAdditive, _propInfos, propNames);
			FindProperties(_matSet._shader_Clipped_Multiplicative, _propInfos, propNames);
			FindProperties(_matSet._shader_L_Normal_AlphaBlend, _propInfos, propNames);
			FindProperties(_matSet._shader_L_Normal_Additive, _propInfos, propNames);
			FindProperties(_matSet._shader_L_Normal_SoftAdditive, _propInfos, propNames);
			FindProperties(_matSet._shader_L_Normal_Multiplicative, _propInfos, propNames);
			FindProperties(_matSet._shader_L_Clipped_AlphaBlend, _propInfos, propNames);
			FindProperties(_matSet._shader_L_Clipped_Additive, _propInfos, propNames);
			FindProperties(_matSet._shader_L_Clipped_SoftAdditive, _propInfos, propNames);
			FindProperties(_matSet._shader_L_Clipped_Multiplicative, _propInfos, propNames);
			FindProperties(_matSet._shader_AlphaMask, _propInfos, propNames);

#if UNITY_2021_2_OR_NEWER
			//프로퍼티에 이어서 키워드도 추가
			FindKeywords(_matSet._shader_Normal_AlphaBlend, _propInfos, propNames);
			FindKeywords(_matSet._shader_Normal_Additive, _propInfos, propNames);
			FindKeywords(_matSet._shader_Normal_SoftAdditive, _propInfos, propNames);
			FindKeywords(_matSet._shader_Normal_Multiplicative, _propInfos, propNames);
			FindKeywords(_matSet._shader_Clipped_AlphaBlend, _propInfos, propNames);
			FindKeywords(_matSet._shader_Clipped_Additive, _propInfos, propNames);
			FindKeywords(_matSet._shader_Clipped_SoftAdditive, _propInfos, propNames);
			FindKeywords(_matSet._shader_Clipped_Multiplicative, _propInfos, propNames);
			FindKeywords(_matSet._shader_L_Normal_AlphaBlend, _propInfos, propNames);
			FindKeywords(_matSet._shader_L_Normal_Additive, _propInfos, propNames);
			FindKeywords(_matSet._shader_L_Normal_SoftAdditive, _propInfos, propNames);
			FindKeywords(_matSet._shader_L_Normal_Multiplicative, _propInfos, propNames);
			FindKeywords(_matSet._shader_L_Clipped_AlphaBlend, _propInfos, propNames);
			FindKeywords(_matSet._shader_L_Clipped_Additive, _propInfos, propNames);
			FindKeywords(_matSet._shader_L_Clipped_SoftAdditive, _propInfos, propNames);
			FindKeywords(_matSet._shader_L_Clipped_Multiplicative, _propInfos, propNames);
			FindKeywords(_matSet._shader_AlphaMask, _propInfos, propNames);
#endif
		}

		private void FindProperties(Shader shader, List<PropInfo> result, HashSet<string> propNames)
		{
			if(shader == null)
			{
				return;
			}

			int nProps = ShaderUtil.GetPropertyCount(shader);
			if(nProps == 0)
			{
				return;
			}

			for (int i = 0; i < nProps; i++)
			{
				string strPropName = ShaderUtil.GetPropertyName(shader, i);
				apMaterialSet.SHADER_PROP_TYPE propType = apMaterialSet.SHADER_PROP_TYPE.Float;
				switch (ShaderUtil.GetPropertyType(shader, i))
				{
#if UNITY_2021_1_OR_NEWER
					case ShaderUtil.ShaderPropertyType.Int:
						propType = apMaterialSet.SHADER_PROP_TYPE.Int;
						break;
#endif

					case ShaderUtil.ShaderPropertyType.Float:
					case ShaderUtil.ShaderPropertyType.Range:
						propType = apMaterialSet.SHADER_PROP_TYPE.Float;
						break;

					case ShaderUtil.ShaderPropertyType.TexEnv:
						propType = apMaterialSet.SHADER_PROP_TYPE.Texture;
						break;

					case ShaderUtil.ShaderPropertyType.Vector:
						propType = apMaterialSet.SHADER_PROP_TYPE.Vector;
						break;

					case ShaderUtil.ShaderPropertyType.Color:
						propType = apMaterialSet.SHADER_PROP_TYPE.Color;
						break;
				}

				if(propNames.Contains(strPropName))
				{
					//이미 등록되었다면
					continue;
				}

				//리스트에 추가
				result.Add(new PropInfo(strPropName, propType));

				propNames.Add(strPropName);
			}
		}

#if UNITY_2021_2_OR_NEWER
		private void FindKeywords(Shader shader, List<PropInfo> result, HashSet<string> propNames)
		{
			if(shader == null)
			{
				return;
			}

			string[] keywords = shader.keywordSpace.keywordNames;
			int nKeywords = keywords != null ? keywords.Length : 0;
			if(nKeywords == 0)
			{
				return;
			}

			for (int i = 0; i < nKeywords; i++)
			{
				string strKeyword = keywords[i];

				if(propNames.Contains(strKeyword))
				{
					//이미 등록되었다면
					continue;
				}

				//리스트에 추가 (키워드 타입으로)
				result.Add(new PropInfo(strKeyword, apMaterialSet.SHADER_PROP_TYPE.Keyword));

				propNames.Add(strKeyword);
			}
		}
#endif
		// GUI
		//--------------------------------------------------------------
		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			if (_editor == null || _funcResult == null)
			{
				return;
			}

			Color prevColor = GUI.backgroundColor;
			GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);
			GUI.Box(new Rect(0, 35, width, height - 90), "");
			GUI.backgroundColor = prevColor;

			EditorGUILayout.BeginVertical();

			Texture2D iconImageCategory = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown);

			GUIStyle guiStyle_None = new GUIStyle(GUIStyle.none);
			guiStyle_None.normal.textColor = GUI.skin.label.normal.textColor;
			guiStyle_None.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_Selected = new GUIStyle(GUIStyle.none);
			if(EditorGUIUtility.isProSkin)
			{
				guiStyle_Selected.normal.textColor = Color.cyan;
			}
			else
			{
				guiStyle_Selected.normal.textColor = Color.white;
			}
			guiStyle_Selected.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_Center = new GUIStyle(GUIStyle.none);
			guiStyle_Center.normal.textColor = GUI.skin.label.normal.textColor;
			guiStyle_Center.alignment = TextAnchor.MiddleCenter;

			GUILayout.Space(10);
			GUILayout.Button(_editor.GetText(TEXT.SelectPropertiesToAdd), guiStyle_Center, GUILayout.Width(width), GUILayout.Height(15));//<투명 버튼//"Select Control Param"
			GUILayout.Space(10);

//Ctrl키나 Shift키를 누르면 여러개를 선택할 수 있다.
			bool isCtrlOrShift = false;

			if(Event.current.shift
#if UNITY_EDITOR_OSX
				|| Event.current.command
#else
				|| Event.current.control
#endif		
				)
			{
				isCtrlOrShift = true;
			}

			_scrollList = EditorGUILayout.BeginScrollView(_scrollList, GUILayout.Width(width), GUILayout.Height(height - 90));

			
			GUILayout.Button(new GUIContent(_editor.GetText(TEXT.Properties), iconImageCategory), guiStyle_None, GUILayout.Height(20));//<투명 버튼

			int nPropInfos = _propInfos != null ? _propInfos.Count : 0;
			if(nPropInfos > 0)
			{
				PropInfo curInfo = null;
				for (int i = 0; i < nPropInfos; i++)
				{
					curInfo = _propInfos[i];

					GUIStyle curGUIStyle = guiStyle_None;
					bool isSelected = _selectedInfos.Contains(curInfo);
					if (isSelected)
					{
						Rect lastRect = GUILayoutUtility.GetLastRect();

						//변경 v1.4.2
						apEditorUtil.DrawListUnitBG(lastRect.x + 1, lastRect.y + 20, width - 2, 20, apEditorUtil.UNIT_BG_STYLE.Main);

						curGUIStyle = guiStyle_Selected;
					}

					EditorGUILayout.BeginHorizontal(GUILayout.Width(width - 50));
					GUILayout.Space(15);
					if (GUILayout.Button(new GUIContent(" " + curInfo._name + " (" + curInfo._type.ToString() + ")"), curGUIStyle, GUILayout.Width(width - 35), GUILayout.Height(20)))
					{
						if(!isCtrlOrShift)
						{
							_selectedInfos.Clear();							
						}

						if(isSelected)
						{
							_selectedInfos.Remove(curInfo);
						}
						else
						{
							_selectedInfos.Add(curInfo);
						}
					}

					EditorGUILayout.EndHorizontal();
				}
			}
			

			GUILayout.Space(height);

			EditorGUILayout.EndScrollView();

			EditorGUILayout.EndVertical();

			GUILayout.Space(10);
			EditorGUILayout.BeginHorizontal();
			bool isClose = false;
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Select), GUILayout.Height(30)))//"Select"
			{
				int nSelected = _selectedInfos != null ? _selectedInfos.Count : 0;
				if(nSelected > 0)
				{
					_funcResult(true, _loadKey, _selectedInfos, _matSet);
				}
				else
				{
					_funcResult(false, _loadKey, null, _matSet);
				}
				
				isClose = true;
			}
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Close), GUILayout.Height(30)))//"Close"
			{
				isClose = true;
			}
			EditorGUILayout.EndHorizontal();

			if (isClose)
			{
				CloseDialog();
			}


		}
	}
}
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
	/// <summary>
	/// v1.4.8에서 추가된 
	/// </summary>
	public class apDialog_SelectGameObjectInScene : EditorWindow
	{
		public delegate void FUNC_SELECT_GAMEOBJECT_RESULT(bool isSuccess, object loadKey, Transform resultTransform);

		public enum REQUEST_GAMEOBJECT
		{
			/// <summary>
			/// 이 apPortrait의 부모 객체들을 선택할 수 있다. 본인 제외
			/// </summary>
			ParentsOfPortrait,
		}
		private REQUEST_GAMEOBJECT _requestType = REQUEST_GAMEOBJECT.ParentsOfPortrait;


		private static apDialog_SelectGameObjectInScene s_window = null;

		private apEditor _editor = null;
		private apPortrait _portrait = null;
		private object _loadKey = null;

		private FUNC_SELECT_GAMEOBJECT_RESULT _funcResult;


		private Vector2 _scrollList = new Vector2();
		private int _iCurSelected = -1;

		private List<Transform> _transformList = null;

		private GUIStyle _guiStyle_None = null;
		private GUIStyle _guiStyle_Selected = null;
		private GUIStyle _guiStyle_Center = null;

		private apGUIContentWrapper _guiContent_Transforms = null;
		private apGUIContentWrapper _guiContent_Item = null;
		private Texture2D _imageTransform = null;


		// Show Window
		//--------------------------------------------------------------
		public static object ShowDialog(	apEditor editor,
											apPortrait targetPortrait,
											REQUEST_GAMEOBJECT requestType,
											FUNC_SELECT_GAMEOBJECT_RESULT funcResult)
		{
			CloseDialog();

			if (editor == null || targetPortrait == null)
			{
				return null;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_SelectGameObjectInScene), true, "Select Transform", true);
			apDialog_SelectGameObjectInScene curTool = curWindow as apDialog_SelectGameObjectInScene;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 250;
				int height = 400;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.Init(editor, loadKey, targetPortrait, requestType, funcResult);

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
		public void Init(	apEditor editor, object loadKey, apPortrait targetPortrait,
							REQUEST_GAMEOBJECT requestType,
							FUNC_SELECT_GAMEOBJECT_RESULT funcResult)
		{
			_editor = editor;
			_loadKey = loadKey;
			_portrait = targetPortrait;
			_funcResult = funcResult;
			_requestType = requestType;

			_iCurSelected = -1;

			//요청에 따라 리스트를 만들자
			_transformList = new List<Transform>();

			

			switch (_requestType)
			{
				case REQUEST_GAMEOBJECT.ParentsOfPortrait:
					MakeList_Parents();
					break;
			}

		}

		private void MakeList_Parents()
		{
			if(_portrait == null)
			{
				return;
			}

			//부모 리스트를 순차적으로 검토한다.
			Transform curParent = _portrait.transform.parent;

			while (true)
			{
				if (curParent == null)
				{
					break;
				}

				_transformList.Add(curParent);

				curParent = curParent.parent;
			}
			

			//첫번째는 항상 Null을 넣는다. ("None")
			//Reverse 직전에 마지막에 넣어야 맨 위로 올라온다.
			_transformList.Add(null);

			if(_transformList.Count > 0)
			{
				//순서를 바꾸자
				_transformList.Reverse();
			}

		}


		// GUI
		//----------------------------------------------------------------------
		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			if (_editor == null || _funcResult == null)
			{
				CloseDialog();
				return;
			}

			if(_guiStyle_None == null)
			{
				_guiStyle_None = new GUIStyle(GUIStyle.none);
				_guiStyle_None.normal.textColor = GUI.skin.label.normal.textColor;
				_guiStyle_None.alignment = TextAnchor.MiddleLeft;
			}

			if (_guiStyle_Selected == null)
			{
				_guiStyle_Selected = new GUIStyle(GUIStyle.none);
				if (EditorGUIUtility.isProSkin)
				{
					_guiStyle_Selected.normal.textColor = Color.cyan;
				}
				else
				{
					_guiStyle_Selected.normal.textColor = Color.white;
				}
				_guiStyle_Selected.alignment = TextAnchor.MiddleLeft;
			}


			if (_guiStyle_Center == null)
			{
				_guiStyle_Center = new GUIStyle(GUIStyle.none);
				_guiStyle_Center.normal.textColor = GUI.skin.label.normal.textColor;
				_guiStyle_Center.alignment = TextAnchor.MiddleCenter;
			}
			

			if(_guiContent_Transforms == null)
			{
				Texture2D iconImageCategory = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown);
				_guiContent_Transforms = apGUIContentWrapper.Make(_editor.GetText(TEXT.TransformObjects), false, iconImageCategory);
			}

			if(_guiContent_Item == null)
			{
				_guiContent_Item = new apGUIContentWrapper();
			}

			if(_imageTransform == null)
			{
				_imageTransform = _editor.ImageSet.Get(apImageSet.PRESET.Unity_Transform);
			}

			Color prevColor = GUI.backgroundColor;
			GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);
			GUI.Box(new Rect(0, 9, width, height - (55 - 1)), "");
			GUI.backgroundColor = prevColor;

			EditorGUILayout.BeginVertical();

			GUILayout.Space(10);

			_scrollList = EditorGUILayout.BeginScrollView(_scrollList, GUILayout.Width(width), GUILayout.Height(height - 55));

			GUILayout.Button(_guiContent_Transforms.Content, _guiStyle_None, GUILayout.Height(20));//<투명 버튼

			GUIStyle curGUIStyle = null;
			Transform curTransform = null;
			for (int i = 0; i < _transformList.Count; i++)
			{
				curGUIStyle = _guiStyle_None;
				curTransform = _transformList[i];

				if(_iCurSelected == i)
				{
					Rect lastRect = GUILayoutUtility.GetLastRect();

					//변경 v1.4.2
					apEditorUtil.DrawListUnitBG(lastRect.x + 1, lastRect.y + 20, width - 2, 20, apEditorUtil.UNIT_BG_STYLE.Main);


					curGUIStyle = _guiStyle_Selected;
				}

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width - 50));
				GUILayout.Space(15);

				_guiContent_Item.ClearAll();
				if(curTransform == null)
				{
					_guiContent_Item.AppendText("< None >", true);
					_guiContent_Item.SetImage(_imageTransform);
				}
				else
				{
					_guiContent_Item.AppendSpaceText(1, false);
					_guiContent_Item.AppendText(curTransform.gameObject.name, true);
					_guiContent_Item.SetImage(_imageTransform);
				}

				
				if (GUILayout.Button(_guiContent_Item.Content, curGUIStyle, GUILayout.Width(width - 35), GUILayout.Height(20)))
				{
					_iCurSelected = i;
				}

				EditorGUILayout.EndHorizontal();
			}

			GUILayout.Space(height + 100);


			EditorGUILayout.EndScrollView();

			EditorGUILayout.EndVertical();

			GUILayout.Space(5);

			EditorGUILayout.BeginHorizontal();


			bool isClose = false;
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Select), GUILayout.Height(30)))//"Select"
			{
				Transform resultTransform = null;
				if(_iCurSelected >= 0 || _iCurSelected < _transformList.Count)
				{
					resultTransform = _transformList[_iCurSelected];
				}

				_funcResult(true, _loadKey, resultTransform);
				isClose = true;
			}
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Close), GUILayout.Height(30)))//"Close"
			{
				_funcResult(false, _loadKey, null);
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
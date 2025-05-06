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
	/// 애니메이션 이벤트에서 "새로운 카테고리 추가" 용도로 사용되는 팝업.
	/// Rename과 거의 비슷하다.
	/// </summary>
	public class apDialog_AnimEventNewCategory : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		private static apDialog_AnimEventNewCategory s_window = null;

		private apEditor _editor = null;
		private object _targetObject = null;
		private object _loadKey = null;
		
		public delegate void FUNC_NEW_CATEGORY(bool isSuccess, object loadKey, object targetObject, string newName);
		private FUNC_NEW_CATEGORY _funcResult = null;

		private string _newName = "";
		private bool _isInitFocused = false;


		// Show Window
		//------------------------------------------------------------------
		public static object ShowDialog(apEditor editor, object targetObject, FUNC_NEW_CATEGORY funcResult)
		{
			CloseDialog();

			if (editor == null)
			{
				return null;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_AnimEventNewCategory), true, "New Category", true);
			apDialog_AnimEventNewCategory curTool = curWindow as apDialog_AnimEventNewCategory;

			object loadKey = new object();
			if (curTool != null)
			{
				int width = 300;
				int height = 85;
				
				//[v1.4.5] 맥에서는 세로가 너무 짧으면 안된다.
#if UNITY_EDITOR_OSX
				height += 30;
#endif
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);

				s_window.Init(editor, loadKey, targetObject, funcResult);

				return loadKey;
			}
			else
			{
				return null;
			}
		}

		public static void CloseDialog()
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
		//------------------------------------------------------------------
		public void Init(apEditor editor, object loadKey, object targetObject, FUNC_NEW_CATEGORY funcResult)
		{
			_editor = editor;
			_loadKey = loadKey;

			_targetObject = targetObject;
			_funcResult = funcResult;

			_newName = "";
			_isInitFocused = false;
		}


		// GUI
		//------------------------------------------------------------------
		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			if (_editor == null || _funcResult == null)
			{
				CloseDialog();
				return;
			}

			//만약 Portriat가 바뀌었거나 Editor가 리셋되면 닫자
			if (_editor != apEditor.CurrentEditor)
			{
				CloseDialog();
				return;
			}

			
			width -= 10;

			bool isPressedEnter = false;
			if(Event.current != null && _isInitFocused)
			{
				if(Event.current.type == EventType.KeyUp)
				{
					if(Event.current.keyCode == KeyCode.Return)
					{
						isPressedEnter = true;
						Event.current.Use();
						apEditorUtil.ReleaseGUIFocus();
					}
				}
			}

			//이름
			EditorGUILayout.LabelField(_editor.GetUIWord(UIWORD.Name), GUILayout.Width(width));
			
			GUILayout.Space(10);

			apEditorUtil.SetNextGUIID(apStringFactory.I.GUI_ID__AnimEventNewCategory);
			_newName = EditorGUILayout.TextField(_newName, GUILayout.Width(width));

			GUILayout.Space(10);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			int width_Btn = ((width - 10) / 2) - 4;
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Add), GUILayout.Width(width_Btn), GUILayout.Height(30)))
			{
				if(_funcResult != null)
				{
					_funcResult(true, _loadKey, _targetObject, _newName);
				}
				
				CloseDialog();
			}
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Cancel), GUILayout.Width(width_Btn), GUILayout.Height(30)))//"Cancel"
			{
				if(_funcResult != null)
				{
					_funcResult(false, _loadKey, _targetObject, null);
				}
				
				CloseDialog();
			}
			EditorGUILayout.EndHorizontal();


			//추가 21.6.13 : 다이얼로그를 열면 자동으로 이름 텍스트 필드에 포커스를 설정하자
			if(!_isInitFocused)
			{
				_isInitFocused = true;
				apEditorUtil.SetGUIFocus_TextField(apStringFactory.I.GUI_ID__AnimEventNewCategory);
			}

			if(isPressedEnter)
			{
				if(_funcResult != null)
				{
					_funcResult(true, _loadKey, _targetObject, _newName);
				}
				CloseDialog();
			}
		}
	}
}
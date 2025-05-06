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
	/// 컨트롤 파라미터의 스냅샷을 저장하는 다이얼로그에서 호출하는 서브 다이얼로그.
	/// 이름을 설정하고 컨트롤 파라미터들을 선택한다.
	/// </summary>
	public class apDialog_ControlParamNewSnapshot : EditorWindow
	{
		// Static Members
		//-------------------------------------------------------------
		private static apDialog_ControlParamNewSnapshot s_window = null;

		// Members
		//-------------------------------------------------------------
		//결과
		public delegate void FUNC_NEW_SNAPSHOT(bool isSuccess, object loadKey, apPortrait portrait, string name, List<apControlParam> targetControlParams);

		private apEditor _editor = null;
		private apPortrait _portrait = null;
		private object _loadKey = null;

		private FUNC_NEW_SNAPSHOT _funcResult = null;

		//현재 값
		public class ControlParamInfo
		{
			public apControlParam _linkedControlParam = null;
			public bool _isSelected = false;
		}

		private List<ControlParamInfo> _infos = null;

		private Vector2 _scroll = Vector2.zero;
		private string _name = "";

		private Texture2D _img_Icon = null;
		private Texture2D _img_FoldDown = null;

		private bool _isFirstFocus = false;
		private const string SNAPSHOT_NAME = "AP_CONTROLPARAM_SNAPSHOT_NAME";

		// Show Window
		//---------------------------------------------------------------
		public static object ShowDialog(apEditor editor, apPortrait portrait, FUNC_NEW_SNAPSHOT funcResult)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null || portrait == null || funcResult == null)
			{
				return null;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_ControlParamNewSnapshot), true, "New Snapshot", true);
			apDialog_ControlParamNewSnapshot curTool = curWindow as apDialog_ControlParamNewSnapshot;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 350;
				int height = 590;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.Init(editor, loadKey, portrait, funcResult);

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
		//---------------------------------------------------------------
		public void Init(apEditor editor, object loadKey, apPortrait portrait, FUNC_NEW_SNAPSHOT funcResult)
		{
			_editor = editor;
			_portrait = portrait;
			_loadKey = loadKey;

			_funcResult = funcResult;

			_scroll = Vector2.zero;
			_name = "(New Snapshot)";


			_infos = new List<ControlParamInfo>();
			List<apControlParam> controlParams = null;
			if(portrait._controller != null)
			{
				controlParams = portrait._controller._controlParams;
			}

			int nControlParams = controlParams != null ? controlParams.Count : 0;
			if(nControlParams > 0)
			{
				apControlParam curParam = null;
				for (int i = 0; i < nControlParams; i++)
				{
					curParam = controlParams[i];

					ControlParamInfo newInfo = new ControlParamInfo();
					newInfo._linkedControlParam = curParam;
					newInfo._isSelected = true;//전부 선택한게 기본

					_infos.Add(newInfo);
				}
			}

			_img_Icon = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Param);
			_img_FoldDown = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown);

			_isFirstFocus = false;
		}

		// GUI
		//-------------------------------------------------------------------------------
		private void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			if (_editor == null || _portrait == null || _funcResult == null)
			{
				return;
			}

			if(_editor._portrait == null 
				|| _editor._portrait != _portrait)
			{
				return;
			}


			//레이아웃
			//- 이름
			//- 컨트롤 파라미터 리스트
			//- 전체 선택/전체 선택 해제
			//- Yes / Cancel

			int height_List = height - 160;

			Color prevColor = GUI.backgroundColor;
			GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);
			GUI.Box(new Rect(0, 55, width, height_List), "");
			GUI.backgroundColor = prevColor;


			bool isClose = false;
			EditorGUILayout.BeginVertical();

			GUILayout.Space(5);
			
			//이름
			EditorGUILayout.LabelField(_editor.GetUIWord(UIWORD.Name));
			GUI.SetNextControlName(SNAPSHOT_NAME);
			_name = EditorGUILayout.TextField(_name);

			GUILayout.Space(10);

			//리스트
			GUIStyle guiStyle_None = new GUIStyle(GUIStyle.none);
			GUIStyle guiStyle_Selected = new GUIStyle(GUIStyle.none);

			if (EditorGUIUtility.isProSkin)
			{
				guiStyle_Selected.normal.textColor = Color.cyan;
				guiStyle_None.normal.textColor = GUI.skin.label.normal.textColor;
			}
			else
			{
				guiStyle_Selected.normal.textColor = Color.white;
				guiStyle_None.normal.textColor = Color.black;
			}

			int height_ListItem = 25;
			

			GUIStyle guiStyle_ItemLabelBtn = new GUIStyle(GUI.skin.label);
			guiStyle_ItemLabelBtn.alignment = TextAnchor.MiddleLeft;
			
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

			_scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Width(width), GUILayout.Height(height_List));
			EditorGUILayout.BeginVertical();

			GUILayout.Button(new GUIContent(_editor.GetUIWord(UIWORD.ControlParameters), _img_FoldDown), guiStyle_None, GUILayout.Height(height_ListItem));//<투명 버튼

			//리스트를 출력한다.
			int nInfo = _infos != null ? _infos.Count : 0;
			if(nInfo > 0)
			{
				ControlParamInfo curInfo = null;
				for (int i = 0; i < nInfo; i++)
				{
					curInfo = _infos[i];

					bool isClick = DrawItem(curInfo, _img_Icon, guiStyle_None, guiStyle_Selected, i, width, height_ListItem, _scroll.x);
					if(isClick)
					{
						if(!isCtrlOrShift)
						{
							//Ctrl/Shift가 눌렸다면
							//다른 선택을 일괄 해제
							DeselectAll();
						}

						curInfo._isSelected = !curInfo._isSelected;
					}

					GUILayout.Space(2);
				}
			}



			GUILayout.Space(height_List + 20);
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();

			GUILayout.Space(10);

			//Select All / Deselect All

			int width_BtnHalf = (width - 10) / 2 - 2;

			EditorGUILayout.BeginHorizontal(GUILayout.Height(22));
			GUILayout.Space(5);
			if(GUILayout.Button(_editor.GetText(TEXT.DLG_SelectAll), GUILayout.Width(width_BtnHalf), GUILayout.Height(22)))
			{
				//Select All
				SelectAll();
			}
			if(GUILayout.Button(_editor.GetText(TEXT.DLG_DeselectAll), GUILayout.Width(width_BtnHalf), GUILayout.Height(22)))
			{
				//Deselect All
				DeselectAll();
			}

			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);

			EditorGUILayout.BeginHorizontal(GUILayout.Height(30));
			GUILayout.Space(5);

			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Select), GUILayout.Width(width_BtnHalf), GUILayout.Height(30)))
			{
				//탭에 따라서 선택된게 다르다.
				if(string.IsNullOrEmpty(_name))
				{
					//"스냅샷 생성 실패"
					//"스냅샷의 이름이 빈칸입니다."
					EditorUtility.DisplayDialog(	_editor.GetText(TEXT.DLG_CPSnapshotFailed_Title),
													_editor.GetText(TEXT.DLG_CPSnapshotFailed_NoName_Body),
													_editor.GetText(TEXT.Okay));
				}
				else
				{
					List<apControlParam> selectedParams = GetSelectedControlParams();
					int nSelectedParams = selectedParams != null ? selectedParams.Count : 0;
					if(nSelectedParams == 0)
					{
						//"스냅샷 생성 실패"
						//"컨트롤 파라미터가 선택되지 않았습니다."
						EditorUtility.DisplayDialog(	_editor.GetText(TEXT.DLG_CPSnapshotFailed_Title),
														_editor.GetText(TEXT.DLG_CPSnapshotFailed_NoCP_Body),
														_editor.GetText(TEXT.Okay));
					}
					else
					{
						//완료. 정보를 리턴하자
						_funcResult(true, _loadKey, _portrait, _name, selectedParams);
						isClose = true;
					}
				}
			}
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Close), GUILayout.Width(width_BtnHalf), GUILayout.Height(30)))//"Close"
			{
				_funcResult(false, _loadKey, _portrait, "", null);
				isClose = true;
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();


			if(isClose)
			{
				CloseDialog();
			}

			if(!_isFirstFocus)
			{
				_isFirstFocus = true;
				GUI.FocusControl(SNAPSHOT_NAME);
			}
		}

		private bool DrawItem(ControlParamInfo info, Texture2D imgIcon, GUIStyle guiStyle_None, GUIStyle guiStyle_Selected, int index, int width, int height, float scrollX)
		{
			Rect lastRect = GUILayoutUtility.GetLastRect();

			if (info._isSelected)
			{
				int yOffset = 0;
				if(index == 0)
				{
					yOffset = height - 2;
				}

				//변경 v1.4.2
				apEditorUtil.DrawListUnitBG(lastRect.x + scrollX + 1, lastRect.y + yOffset , width + 10 - 2, height + 3, apEditorUtil.UNIT_BG_STYLE.Main);
			}

			EditorGUILayout.BeginHorizontal(GUILayout.Height(height));
			GUILayout.Space(5);

			bool isClick = false;

			if(GUILayout.Button(new GUIContent(" " + info._linkedControlParam._keyName, imgIcon), (info._isSelected ? guiStyle_Selected : guiStyle_None), GUILayout.Height(height)))
			{
				isClick = true;
			}

			EditorGUILayout.EndHorizontal();

			return isClick;
		}


		private void DeselectAll()
		{
			int nInfo = _infos != null ? _infos.Count : 0;
			if (nInfo > 0)
			{
				ControlParamInfo curInfo = null;
				for (int i = 0; i < nInfo; i++)
				{
					curInfo = _infos[i];
					curInfo._isSelected = false;
				}
			}
		}

		private void SelectAll()
		{
			int nInfo = _infos != null ? _infos.Count : 0;
			if (nInfo > 0)
			{
				ControlParamInfo curInfo = null;
				for (int i = 0; i < nInfo; i++)
				{
					curInfo = _infos[i];
					curInfo._isSelected = true;
				}
			}
		}

		private List<apControlParam> GetSelectedControlParams()
		{
			List<apControlParam> result = new List<apControlParam>();
			int nInfo = _infos != null ? _infos.Count : 0;
			if (nInfo > 0)
			{
				ControlParamInfo curInfo = null;
				for (int i = 0; i < nInfo; i++)
				{
					curInfo = _infos[i];
					if(curInfo._isSelected)
					{
						result.Add(curInfo._linkedControlParam);
					}
				}
			}
			return result;
		}


	}
}
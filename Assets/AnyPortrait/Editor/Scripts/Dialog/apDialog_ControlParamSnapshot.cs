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
    //컨트롤 파라미터의 값을 저장했다가 적용하는 스냅샷 다이얼로그
    public class apDialog_ControlParamSnapshot : EditorWindow
    {
        // Static Members
        //--------------------------------------------------------------
        private static apDialog_ControlParamSnapshot s_window = null;

        private apEditor _editor = null;
        private apPortrait _portrait = null;

        private Vector2 _scroll = Vector2.zero;
        private apControlParamValueSnapShot.SnapShotSet _selectedSnapShot = null;

		private object _loadKey_NewSnapshot = null;

		//적용 방식 (애니메이션의 경우 한정)
		public enum SNAPSHOT_ADAPT_METHOD : int
		{
			Preview = 0,
			AddAndSetKeyframe = 1
		}

		private SNAPSHOT_ADAPT_METHOD _adaptMethod = SNAPSHOT_ADAPT_METHOD.Preview;

		private const string ADAPT_METHOD_PREF = "AnyPortrait_CPSnapshot_Method";
		

        // Show Window
		//--------------------------------------------------------------
		public static void ShowDialog(apEditor editor)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null)
			{
				return;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_ControlParamSnapshot), true, "Snapshots", true);
			apDialog_ControlParamSnapshot curTool = curWindow as apDialog_ControlParamSnapshot;

			if (curTool != null && curTool != s_window)
			{
				int width = 300;
				int height = 400;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.Init(editor);
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
        //----------------------------------------------------
        public apDialog_ControlParamSnapshot()
        {

        }

        public void Init(apEditor editor)
        {
            _editor = editor;
            _portrait = _editor._portrait;

            _selectedSnapShot = null;
            _scroll = Vector2.zero;

			_adaptMethod = (SNAPSHOT_ADAPT_METHOD)EditorPrefs.GetInt(ADAPT_METHOD_PREF, (int)SNAPSHOT_ADAPT_METHOD.Preview);

			_loadKey_NewSnapshot = null;
        }

        // OnGUI
        //----------------------------------------------------
        void OnGUI()
        {
            int width = (int)position.width;
            int height = (int)position.height;
            if (_editor == null
				|| _editor._portrait == null
				|| _editor._portrait != _portrait)
			{
				return;
			}

			GUILayout.Space(5);
			//레이아웃
			//+추가 버튼
			//--------
			//- 스냅샷 리스트
			//- 체크박스 : 키에 적용하기 여부
			//- 적용
			//- 선택한 스냅샷 삭제
			//- 닫기
			//_editor.ImageSet.Get(apImageSet.PRESET.Controller_Snapshot)

			int height_List = height - 185;

			Color prevColor = GUI.backgroundColor;
			GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);
			GUI.Box(new Rect(0, 46, width, height_List + 2), "");
			GUI.backgroundColor = prevColor;
			
			Texture2D icon_Snapshot = _editor.ImageSet.Get(apImageSet.PRESET.Controller_Snapshot);
			Texture2D icon_FoldDown = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown);
			
			GUIStyle guiStyle_Center = new GUIStyle(GUIStyle.none);
			guiStyle_Center.normal.textColor = GUI.skin.label.normal.textColor;
			guiStyle_Center.alignment = TextAnchor.MiddleCenter;

			GUIStyle guiStyle_None = new GUIStyle(GUIStyle.none);
			guiStyle_None.normal.textColor = GUI.skin.label.normal.textColor;

			GUIStyle guiStyle_Selected = new GUIStyle(GUIStyle.none);
			if(EditorGUIUtility.isProSkin)
			{
				guiStyle_Selected.normal.textColor = Color.cyan;
			}
			else
			{
				guiStyle_Selected.normal.textColor = Color.white;
			}


			List<apControlParamValueSnapShot.SnapShotSet> snapshots = _portrait.ControlParamSnapShot._snapShots;
			int nSnapshots = snapshots != null ? snapshots.Count : 0;

			bool isClose = false;
			EditorGUILayout.BeginVertical();

			//1. 스냅샷 추가 버튼
			//"현재 상태를 스냅샷으로 저장"
			if(GUILayout.Button(_editor.GetText(TEXT.SaveCPAsSnapshot), apGUILOFactory.I.Height(30)))
			{
				_loadKey_NewSnapshot = apDialog_ControlParamNewSnapshot.ShowDialog(_editor, _portrait, OnNewSnapshot);
			}

			GUILayout.Space(10);

			//2. 리스트
			
			_scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Width(width), GUILayout.Height(height_List));

			int itemHeight = 25;
			//"Snapshots"
			GUILayout.Button(new GUIContent(_editor.GetUIWord(UIWORD.Snapshot), icon_FoldDown), guiStyle_None, GUILayout.Height(itemHeight));//<투명 버튼

			if(nSnapshots > 0)
			{
				
				apControlParamValueSnapShot.SnapShotSet curSnapshot = null;
				for (int i = 0; i < nSnapshots; i++)
				{
					curSnapshot = snapshots[i];

					GUIStyle curGUIStyle = guiStyle_None;
					if(curSnapshot == _selectedSnapShot)
					{
						//선택 중인 스냅샷이라면
						Rect lastRect = GUILayoutUtility.GetLastRect();
						apEditorUtil.DrawListUnitBG(lastRect.x + 1, lastRect.y + 25, width - 2, itemHeight, apEditorUtil.UNIT_BG_STYLE.Main);
						curGUIStyle = guiStyle_Selected;
					}

					EditorGUILayout.BeginHorizontal(GUILayout.Width(width - 50), GUILayout.Height(itemHeight));
					GUILayout.Space(15);
					if (GUILayout.Button(new GUIContent(" " + curSnapshot._name, icon_Snapshot), curGUIStyle, GUILayout.Width(width - 35), GUILayout.Height(itemHeight)))
					{
						_selectedSnapShot = curSnapshot;
					}

					EditorGUILayout.EndHorizontal();
				}
			}

			GUILayout.Space(height_List + 50);

			EditorGUILayout.EndScrollView();

			GUILayout.Space(10);

			//3. 방식 : 키를 생성할 것인가
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
			GUILayout.Space(4);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.Method), GUILayout.Width(100));
			SNAPSHOT_ADAPT_METHOD nextMethod = (SNAPSHOT_ADAPT_METHOD)EditorGUILayout.EnumPopup(_adaptMethod, GUILayout.Width(width - (100 + 12)));
			if (nextMethod != _adaptMethod)
			{
				_adaptMethod = nextMethod;

				if (_adaptMethod == SNAPSHOT_ADAPT_METHOD.Preview)
				{
					//기본값(Preview)인 경우
					EditorPrefs.DeleteKey(ADAPT_METHOD_PREF);
				}
				else
				{
					//변경된 값 저장
					EditorPrefs.SetInt(ADAPT_METHOD_PREF, (int)_adaptMethod);
				}
			}

			

			EditorGUILayout.EndHorizontal();

			//4. 적용 버튼
			if(apEditorUtil.ToggledButton_2Side(_editor.GetText(TEXT.DLG_Apply), false, _selectedSnapShot != null, width - 6, 30))
			{
				_editor.Controller.OnControlParamSnapshotAdapt(true, _portrait, _selectedSnapShot, _adaptMethod);
				//적용한다고 해서 없어지지 않는다.
			}

			//5. 선택한거 삭제 버튼
			//"선택된 스냅샷 삭제하기"
			if(apEditorUtil.ToggledButton_2Side(_editor.GetText(TEXT.RemoveCPSnapshot), false, _selectedSnapShot != null, width - 6, 20))
			{
				if(_selectedSnapShot != null)
				{
					//"스냅샷 삭제", "선택된 스냅샷을 삭제하시겠습니까?"
					bool result = EditorUtility.DisplayDialog(	_editor.GetText(TEXT.DLG_CPSnapshotRemove_Title),
																_editor.GetText(TEXT.DLG_CPSnapshotRemove_Body),
																_editor.GetText(TEXT.Remove),
																_editor.GetText(TEXT.Cancel));

					if(result)
					{
						apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Portrait_SettingChanged,
															_editor,
															_portrait,
															false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

						if(_portrait.ControlParamSnapShot._snapShots != null)
						{
							_portrait.ControlParamSnapShot._snapShots.Remove(_selectedSnapShot);
						}
						_selectedSnapShot = null;

						Repaint();
					}
				}
				
			}

			//6. 닫기
			GUILayout.Space(5);
			if(GUILayout.Button(_editor.GetText(TEXT.Close), apGUILOFactory.I.Height(30)))
			{
				isClose = true;
			}

			EditorGUILayout.EndVertical();


			if(isClose)
			{
				CloseDialog();
			}


        }


		//스냅샷 생성 결과
		private void OnNewSnapshot(bool isSuccess, object loadKey, apPortrait portrait, string name, List<apControlParam> targetControlParams)
		{
			if(!isSuccess
			|| loadKey == null
			|| _loadKey_NewSnapshot != loadKey
			|| _portrait != portrait)
			{
				_loadKey_NewSnapshot = null;
				return;
			}

			_loadKey_NewSnapshot = null;

			if(string.IsNullOrEmpty(name))
			{
				return;
			}

			int nCPs = targetControlParams != null ? targetControlParams.Count : 0;
			if(nCPs == 0)
			{
				return;
			}

			apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Portrait_SettingChanged,
												_editor,
												_portrait,
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

			if(_portrait.ControlParamSnapShot._snapShots == null)
			{
				_portrait.ControlParamSnapShot._snapShots = new List<apControlParamValueSnapShot.SnapShotSet>();				
			}

			apControlParamValueSnapShot.SnapShotSet newSet = new apControlParamValueSnapShot.SnapShotSet();

			//이름 지정
			newSet._name = name;

			if(newSet._valueList == null)
			{
				newSet._valueList = new List<apControlParamValueSnapShot.ControlParamValue>();				
			}

			//컨트롤 파라미터 하나씩 입력하기
			apControlParam curCP = null;
			for (int i = 0; i < nCPs; i++)
			{
				curCP = targetControlParams[i];

				apControlParamValueSnapShot.ControlParamValue newCPValue = new apControlParamValueSnapShot.ControlParamValue();
				newCPValue._controlParamID = curCP._uniqueID;

				newCPValue._savedType = curCP._valueType;
				newCPValue._value_Int = 0;
				newCPValue._value_Float = 0.0f;
				newCPValue._value_Vector = Vector2.zero;
				
				switch(curCP._valueType)
				{
					case apControlParam.TYPE.Int:
						newCPValue._value_Int = curCP._int_Cur;
						break;

					case apControlParam.TYPE.Float:
						newCPValue._value_Float = curCP._float_Cur;
						break;

					case apControlParam.TYPE.Vector2:
						newCPValue._value_Vector = curCP._vec2_Cur;
						break;
				}

				//컨트롤 파라미터 정보 추가
				newSet._valueList.Add(newCPValue);
			}


			//리스트에 넣기.
			_portrait.ControlParamSnapShot._snapShots.Add(newSet);

			//선택
			_selectedSnapShot = newSet;


			Repaint();
		}
    }
}
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
	//v1.5.0
	//애니메이션의 선택된 키프레임들의 위치를 일괄 변경하는 다이얼로그
	public class apDialog_RescaleKeyframes : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		private static apDialog_RescaleKeyframes s_window = null;

		private apEditor _editor = null;
		private object _loadKey = null;

		private apAnimClip _animClip = null;
		private List<apAnimKeyframe> _keyframes = null;
		private int _nKeyframes = 0;

		//소스 정보
		private int _srcFrame_Start = 0;
		private int _srcFrame_Last = 0;

		//변경된 값
		private int _dstFrame_Start = 0;
		private int _dstFrame_Last = 0;

		//길이에 대한 속성
		//- 길이는 Last - Start (프레임이 모두 같다면 Length는 0이다.)
		//- 길이가 1 이상 (Start/Last가 다르다)이라면 Length를 조절할 수 있다.
		//- 길이가 0이라면 Length를 조절할 수 없으며, Start/Last 대신 Frame만 나온다.
		private int _srcLength = 0;
		private int _dstLength = 0;
		private bool _isValidLength = false;
		public bool IsValidLength { get { return _isValidLength; } }

		//AnimClip 상의 프레임
		private int _minFrame = 0;
		private int _maxFrame = 0;


		//UI에 보여지는 텍스트
		private string _strInfo = "";

		public delegate void FUNC_RESCALE_KEYFRAMES(	bool isSuccess,
														object loadKey,
														apAnimClip animClip,
														List<apAnimKeyframe> keyframes,
														int srcFrame_Start, int srcFrame_Last,
														int dstFrame_Start, int dstFrame_Last);
		private FUNC_RESCALE_KEYFRAMES _funcResult = null;

		// Show Window
		//------------------------------------------------------------------
		public static object ShowDialog(	apEditor editor,
											apAnimClip animClip,
											List<apAnimKeyframe> selectedKeyframes,
											FUNC_RESCALE_KEYFRAMES funcResult)
		{
			CloseDialog();

			if (editor == null)
			{
				return null;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_RescaleKeyframes), true, "Stretch Keyframes", true);
			apDialog_RescaleKeyframes curTool = curWindow as apDialog_RescaleKeyframes;

			object loadKey = new object();
			if (curTool != null)
			{
				int width = 300;

				
				
				s_window = curTool;

				//값에 따라서 Height가 다르므로 Init를 먼저 한다.
				s_window.Init(editor, loadKey, animClip, selectedKeyframes, funcResult);


				int height = 0;
				if(s_window.IsValidLength)
				{
					//Length 속성이 있다면
					//UI가 좀 더 있어서 Height가 길다.
					//- Start / End / Length가 있다.
					height = 150;
				}
				else
				{
					//Length 속성이 없다면
					//UI가 줄어서 Height가 짧다.
					//- Frame만 있다.
					height = 130;
				}

				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);

				

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
		public void Init(	apEditor editor,
							object loadKey,
							apAnimClip animClip,
							List<apAnimKeyframe> selectedKeyframes,
							FUNC_RESCALE_KEYFRAMES funcResult)
		{
			_editor = editor;
			_loadKey = loadKey;
			_funcResult = funcResult;

			_animClip = animClip;

			//리스트를 돌면서 최소/최대 프레임 위치를 찾자
			bool isAnyKeyframe = false;

			//소스 정보
			_srcFrame_Start = 0;
			_srcFrame_Last = 0;

			//변경된 값
			_dstFrame_Start = 0;
			_dstFrame_Last = 0;

			_keyframes = new List<apAnimKeyframe>();//리스트는 복제하기
			int nSrcKeyframes = selectedKeyframes != null ? selectedKeyframes.Count : 0;
			if(nSrcKeyframes > 0)
			{
				apAnimKeyframe curKeyframe = null;
				for (int i = 0; i < nSrcKeyframes; i++)
				{
					curKeyframe = selectedKeyframes[i];
					if(curKeyframe == null)
					{
						continue;
					}

					if(!isAnyKeyframe)
					{
						isAnyKeyframe = true;
						_srcFrame_Start = curKeyframe._frameIndex;
						_srcFrame_Last = curKeyframe._frameIndex;
					}
					else
					{
						_srcFrame_Start = Mathf.Min(_srcFrame_Start, curKeyframe._frameIndex);
						_srcFrame_Last = Mathf.Max(_srcFrame_Last, curKeyframe._frameIndex);
					}

					//키프레임도 복사
					_keyframes.Add(curKeyframe);
				}
			}

			_nKeyframes = _keyframes.Count;

			_dstFrame_Start = _srcFrame_Start;
			_dstFrame_Last = _srcFrame_Last;


			//길이에 대한 속성
			//- 길이는 Last - Start (프레임이 모두 같다면 Length는 0이다.)
			//- 길이가 1 이상 (Start/Last가 다르다)이라면 Length를 조절할 수 있다.
			//- 길이가 0이라면 Length를 조절할 수 없으며, Start/Last 대신 Frame만 나온다.
			_srcLength = _srcFrame_Last - _srcFrame_Start;
			_dstLength = _srcLength;
			if(_srcLength > 0)
			{
				//유효한 길이
				_isValidLength = true;
			}
			else
			{
				//모든 키프레임이 동일한 위치에 있다.
				_isValidLength = false;
			}

			//AnimClip 상의 프레임
			_minFrame = _animClip.StartFrame;
			_maxFrame = _animClip.EndFrame;

			//보여지는 정보
			if(_isValidLength)
			{
				//길이를 설정할 수 있다면
				_strInfo = _editor.GetUIWord(UIWORD.Range) + " : " + _srcFrame_Start + " ~ " + _srcFrame_Last + " (" + _srcLength + ")\n" 
							+ _editor.GetUIWord(UIWORD.Keyframes) + " : " + _nKeyframes;
			}
			else
			{
				//길이를 설정할 수 없다면
				_strInfo = _editor.GetUIWord(UIWORD.Frame) + " : " + _srcFrame_Start + "\n" 
							+ _editor.GetUIWord(UIWORD.Keyframes) + " : " + _nKeyframes;
			}
			
		}

		// GUI
		//-------------------------------------------------------------------------------
		private void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			if (_editor == null || _funcResult == null || _animClip == null)
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

			GUILayout.Space(5);

			//UI 구조
			//<박스 1 : 파란색>
			//원래의 키프레임 범위 Start ~ Last (길이)
			//키프레임 개수

			//값 설정 + 적용 버튼
			//해당 값을 변경하면 나머지 옵션의 값이 자동으로 변경된다.
			//Start / End 영역

			//Apply , Cancel 버튼

			//TODO
			//키프레임이 2개 이상이라도, 모두 같은 프레임에 있으면 길이는 0이다.
			//길이가 0이면 스케일 안된다는 문구를 띄워야 한다.


			Color prevGUIColor = GUI.backgroundColor;
			
			GUI.backgroundColor = new Color(0.2f, 0.8f, 0.8f, 1.0f);

			//int srcLength = (_srcFrame_Last - _srcFrame_Start);
			GUILayout.Box(	_strInfo,
							apGUIStyleWrapper.I.Box_MiddleCenter_LabelMargin_WhiteColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(40));

			GUI.backgroundColor = prevGUIColor;
			GUILayout.Space(5);


			int width_Label = 100;
			int width_Value_1 = width - (width_Label + 8);
			int width_Value_2 = (width_Value_1 / 2) - 15;

			//길이에 따라서 UI가 바뀐다.
			if(_isValidLength)
			{
				// [ 길이가 1 이상인 경우 ]
				//- Start / End
				//- Length
				
				//1. Start / End(Last)
				
				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
				GUILayout.Space(4);
				EditorGUILayout.LabelField(_editor.GetUIWord(UIWORD.Range), apGUILOFactory.I.Width(width_Label));
				EditorGUI.BeginChangeCheck();
				int nextStartFrame = EditorGUILayout.IntField(_dstFrame_Start, apGUILOFactory.I.Width(width_Value_2));
				if(EditorGUI.EndChangeCheck())
				{
					_dstFrame_Start = nextStartFrame;

					//길이를 변경하자
					_dstLength = Mathf.Max(_dstFrame_Last - _dstFrame_Start, 0);
				}

				EditorGUILayout.LabelField(apStringFactory.I.Tilde, apGUIStyleWrapper.I.Label_MiddleCenter, apGUILOFactory.I.Width(24));
				EditorGUI.BeginChangeCheck();
				int nextLastFrame = EditorGUILayout.IntField(_dstFrame_Last, apGUILOFactory.I.Width(width_Value_2));
				if(EditorGUI.EndChangeCheck())
				{
					_dstFrame_Last = nextLastFrame;
					//길이를 변경하자
					_dstLength = Mathf.Max(_dstFrame_Last - _dstFrame_Start, 0);
				}

				EditorGUILayout.EndHorizontal();

				//if(EditorGUI.EndChangeCheck())
				//{
				//	//Start / Last 값이 바뀌었다.
				//	//Delayed가 아니므로, 값 보정은 하지 말자
				//	//- Anim Clip 범위 안에 넣자
				//	//nextStartFrame = Mathf.Clamp(nextStartFrame, _minFrame, _maxFrame);
				//	//nextLastFrame = Mathf.Clamp(nextLastFrame, _minFrame, _maxFrame);

				//	//_dstFrame_Start = Mathf.Min(nextStartFrame, nextLastFrame);
				//	//_dstFrame_Last = Mathf.Max(nextStartFrame, nextLastFrame);

				//	//길이를 변경하자
				//	//_dstLength = _dstFrame_Last - _dstFrame_Start;
				//}

				//2. Length
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
				GUILayout.Space(4);
				EditorGUILayout.LabelField(_editor.GetUIWord(UIWORD.Length), apGUILOFactory.I.Width(width_Label));
				int nextLength = EditorGUILayout.IntField(_dstLength, apGUILOFactory.I.Width(width_Value_1));
				EditorGUILayout.EndHorizontal();
				
				if(EditorGUI.EndChangeCheck())
				{
					//Delayed가 아니므로 보정은 없다.
					//Last를 바로 변경하되 그 이상의 보정은 하지 않는다.
					_dstLength = nextLength;
					_dstFrame_Last = _dstFrame_Start + _dstLength;

					////Length가 변경이 되었다.
					//if(nextLength < 0)
					//{
					//	nextLength = 0;//0으로 설정할 수 있다.
					//}

					////Last Frame을 변경한다.
					//_dstFrame_Last = _dstFrame_Start + nextLength;

					////Start / Last 범위를 체크한 후 nextLength를 변경한다.
					//int frame_A = Mathf.Clamp(_dstFrame_Start, _minFrame, _maxFrame);
					//int frame_B = Mathf.Clamp(_dstFrame_Last, _minFrame, _maxFrame);

					//_dstFrame_Start = Mathf.Min(frame_A, frame_B);
					//_dstFrame_Last = Mathf.Max(frame_A, frame_B);

					////길이를 다시 정한다.
					//nextLength = _dstFrame_Last - _dstFrame_Start;
					//_dstLength = nextLength;
				}
				
			}
			else
			{
				// [ 길이가 0인 경우 ]
				//- Frame
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
				GUILayout.Space(4);
				EditorGUILayout.LabelField(_editor.GetUIWord(UIWORD.Frame), apGUILOFactory.I.Width(width_Label));
				int nextStartFrame = EditorGUILayout.IntField(_dstFrame_Start, apGUILOFactory.I.Width(width_Value_1));
				EditorGUILayout.EndHorizontal();

				if(EditorGUI.EndChangeCheck())
				{
					//Delayed가 아니므로 보정은 하지 않는다.
					_dstFrame_Start = nextStartFrame;
					_dstFrame_Last = nextStartFrame;

					////Start / Last 값이 바뀌었다.
					////- Anim Clip 범위 안에 넣자
					//nextStartFrame = Mathf.Clamp(nextStartFrame, _minFrame, _maxFrame);

					//_dstFrame_Start = nextStartFrame;
					//_dstFrame_Last = nextStartFrame;

					////길이를 변경하자
					//_dstLength = _dstFrame_Last - _dstFrame_Start;
				}
			}

			GUILayout.Space(10);

			//Apply / Cancel
			bool isClose = false;
			int btnWidth = (width / 2);

			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Height(30));
			GUILayout.Space(4);
			if(GUILayout.Button(_editor.GetText(TEXT.DLG_Apply), apGUILOFactory.I.Width(btnWidth), apGUILOFactory.I.Height(30)))
			{
				if(_funcResult != null)
				{
					//여기서 Dst 보정을 해두자
					_dstFrame_Start = Mathf.Clamp(_dstFrame_Start, _minFrame, _maxFrame);
					_dstFrame_Last = Mathf.Clamp(_dstFrame_Last, _minFrame, _maxFrame);
					if(_dstFrame_Last < _dstFrame_Start)
					{
						_dstFrame_Last = _dstFrame_Start;
					}


					_funcResult(	true, _loadKey, _animClip, _keyframes, 
									_srcFrame_Start, _srcFrame_Last,
									_dstFrame_Start, _dstFrame_Last);
				}

				isClose = true;
			}
			if(GUILayout.Button(_editor.GetText(TEXT.DLG_Cancel), apGUILOFactory.I.Width(btnWidth), apGUILOFactory.I.Height(30)))
			{
				if(_funcResult != null)
				{
					_funcResult(false, _loadKey, _animClip, _keyframes, 0, 0, 0, 0);
				}
				isClose = true;
			}
			EditorGUILayout.EndHorizontal();

			if(isClose)
			{
				CloseDialog();
			}
		}

	}
}
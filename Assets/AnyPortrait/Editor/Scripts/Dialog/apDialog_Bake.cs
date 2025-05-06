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
using System.CodeDom.Compiler;

namespace AnyPortrait
{

	public class apDialog_Bake : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		private static apDialog_Bake s_window = null;

		private apEditor _editor = null;
		private apPortrait _targetPortrait = null;
		//private object _loadKey = null;

		private string[] _colorSpaceNames = new string[] { "Gamma", "Linear" };

		
//#if UNITY_2019_1_OR_NEWER
		private string[] _renderPipelineNames = new string[] { "Default", "Scriptable Render Pipeline" };
//#endif

		private string[] _sortingLayerNames = null;
		private int[] _sortingLayerIDs = null;

		private bool _isSortingLayerInit = false;

		private string[] _billboardTypeNames = new string[] { "None", "Billboard", "Billboard with fixed Up Vector" };
		private string[] _billboardParentOptionNames = new string[] { "Ignore", "Pitch Yaw Roll (Local)", "Sync Up Vector (World)" };

		private string[] _vrSupportModeLabel = new string[] { "None", "Single Camera and Eye Textures (Unity VR)", "Multiple Cameras" };

		private string[] _cameraCheckModeLabel = new string[] { "Current Camera Mainly", "All Scene Cameras" };

		private string[] _flippedMeshOptionLabel = new string[] { "Check excluding Rigged Meshes", "Check All"};

		private string[] _rootBoneScaleOptionLabel = new string[] { "Default", "Non-Uniform Scale"};
		
#if UNITY_2019_1_OR_NEWER
		private string[] _vrRTSizeLabel = new string[] { "By Mesh Setting", "By Eye Texture Setting" };
#else
		private string[] _vrRTSizeLabel = new string[] { "By Mesh Setting", "By Eye Texture Setting (Not Supported)" };
#endif


		//v1.4.7
		private string[] _meshUpdateFrequencyLabel = new string[]
		{
			"Every Frame", "Per Time (Not Sync)", "Per Time (Sync)"
		};

		//v1.4.8
		private string[] _mainLogicEventLabel = new string[]
		{
			"LateUpdate (Default)", "Update"
		};

		//v1.5.0
		private string[] _invisibleMeshUpdateLabel = new string[]
		{
			"Not Update (Default)", "Update"
		};


#if UNITY_2017_1_OR_NEWER
		//v1.5.1
		private string[] _clippingMeshUpdateLabel = new string[]
		{
			"In Update (Default)",
			"Before Rendering",
		};
#endif


		private string[] _rootMotionModeLabel = new string[]
		{
			"None", "Lock to Center", "Move Parent Transform"
		};

		public string[] _IKMethodNames = new string[] { "FABRIK", "CCD (Legacy)" };

		//추가 : 탭으로 분류하자
		//private Vector2 _scroll_Bake = Vector2.zero;
		private Vector2 _scroll_Options = Vector2.zero;

		private enum TAB
		{
			Bake,
			Options
		}
		private TAB _tab = TAB.Bake;


		// 추가 19.11.20 : GUIContent
		private apGUIContentWrapper _guiContent_Setting_IsImportant = null;
		private apGUIContentWrapper _guiContent_Setting_FPS = null;

		private apGUIContentWrapper _guiContent_Setting_MeshUpdateFrequency = null;
		private apGUIContentWrapper _guiContent_Setting_MeshFreqFPS = null;
		private apGUIContentWrapper _guiContent_Setting_MeshFreqFPSScaleOpt = null;

		private apGUIContentWrapper _guiContent_Setting_InvisibleMesh = null;//v1.5.0
#if UNITY_2017_1_OR_NEWER
		private apGUIContentWrapper _guiContent_Setting_ClippingUpdate = null;//v1.5.1
#endif

		private apGUIContentWrapper _guiContent_Setting_ProcessEvent = null;

		


		private apGUIContentWrapper _guiContent_BakeBtn_Normal = null;
		private apGUIContentWrapper _guiContent_BakeBtn_Optimized = null;

		private GUIStyle _guiStyle_Category = null;
		private GUIStyle _guiStyle_SyncBtn = null;

		private GUIStyle _guiStyle_Label_Default = null;
		private GUIStyle _guiStyle_Label_Changed = null;

		private GUIStyle _guiStyle_LabelWrapText = null;
		private GUIStyle _guiStyle_LabelWrapText_Changed = null;

		private GUIStyle _guiStyle_BoxMessage = null;
		private int _layoutLeftLabelWidth = 170;


		// Show Window
		//------------------------------------------------------------------
		public static object ShowDialog(apEditor editor, apPortrait portrait)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null || editor._portrait._controller == null)
			{
				return null;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_Bake), true, "Bake", true);
			apDialog_Bake curTool = curWindow as apDialog_Bake;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 370;
				int height = 380;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);

				s_window.Init(editor, portrait, loadKey);

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
		public void Init(apEditor editor, apPortrait portrait, object loadKey)
		{
			_editor = editor;
			//_loadKey = loadKey;
			_targetPortrait = portrait;

			_isSortingLayerInit = false;


		}

		// GUI
		//------------------------------------------------------------------
		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;

			_layoutLeftLabelWidth = Mathf.Max((width / 2) - 5, 50);

			if (_editor == null || _targetPortrait == null)
			{
				CloseDialog();
				return;
			}

			//만약 Portriat가 바뀌었거나 Editor가 리셋되면 닫자
			if (_editor != apEditor.CurrentEditor || _targetPortrait != apEditor.CurrentEditor._portrait)
			{
				CloseDialog();
				return;
			}




			//Sorting Layer를 추가하자
			if (!_isSortingLayerInit)
			{
				if (_sortingLayerNames == null || _sortingLayerIDs == null)
				{
					_sortingLayerNames = new string[SortingLayer.layers.Length];
					_sortingLayerIDs = new int[SortingLayer.layers.Length];
				}
				else if (_sortingLayerNames.Length != SortingLayer.layers.Length
					|| _sortingLayerIDs.Length != SortingLayer.layers.Length)
				{
					_sortingLayerNames = new string[SortingLayer.layers.Length];
					_sortingLayerIDs = new int[SortingLayer.layers.Length];
				}

				for (int i = 0; i < SortingLayer.layers.Length; i++)
				{
					_sortingLayerNames[i] = SortingLayer.layers[i].name;
					_sortingLayerIDs[i] = SortingLayer.layers[i].id;
				}

				_isSortingLayerInit = true;
			}
			
			if(_guiStyle_Category == null)
			{
				_guiStyle_Category = new GUIStyle(GUI.skin.box);
				_guiStyle_Category.normal.background = apEditorUtil.WhiteTexture;
				_guiStyle_Category.normal.textColor = Color.white;
				_guiStyle_Category.alignment = TextAnchor.MiddleCenter;
			}
			if(_guiStyle_SyncBtn == null)
			{
				_guiStyle_SyncBtn = new GUIStyle(GUI.skin.button);
				_guiStyle_SyncBtn.margin = new RectOffset(0, 0, 0, 0);
				_guiStyle_SyncBtn.padding = new RectOffset(0, 0, 0, 0);
			}


			if(_guiStyle_Label_Default == null)
			{
				_guiStyle_Label_Default = new GUIStyle(GUI.skin.label);
				_guiStyle_Label_Default.alignment = TextAnchor.UpperLeft;
			}
			
			if(_guiStyle_Label_Changed == null)
			{
				_guiStyle_Label_Changed = new GUIStyle(GUI.skin.label);
				_guiStyle_Label_Changed.alignment = TextAnchor.UpperLeft;
				if (EditorGUIUtility.isProSkin)
				{
					//어두운 Pro 버전에선 노란색 Label
					_guiStyle_Label_Changed.normal.textColor = Color.yellow;
				}
				else
				{
					//밝은 색이면 진한 보라색
					_guiStyle_Label_Changed.normal.textColor = new Color(1.0f, 0.0f, 0.5f, 1.0f);
				}
			}

			if(_guiStyle_LabelWrapText == null)
			{
				_guiStyle_LabelWrapText = new GUIStyle(GUI.skin.label);
				_guiStyle_LabelWrapText.wordWrap = true;
				_guiStyle_LabelWrapText.alignment = TextAnchor.UpperLeft;
			}

			if(_guiStyle_LabelWrapText_Changed == null)
			{
				_guiStyle_LabelWrapText_Changed = new GUIStyle(GUI.skin.label);
				_guiStyle_LabelWrapText_Changed.wordWrap = true;
				_guiStyle_LabelWrapText_Changed.alignment = TextAnchor.UpperLeft;

				if (EditorGUIUtility.isProSkin)
				{
					//어두운 Pro 버전에선 노란색 Label
					_guiStyle_LabelWrapText_Changed.normal.textColor = Color.yellow;
				}
				else
				{
					//밝은 색이면 진한 보라색
					_guiStyle_LabelWrapText_Changed.normal.textColor = new Color(1.0f, 0.0f, 0.5f, 1.0f);
				}
			}
			
			if(_guiStyle_BoxMessage == null)
			{
				_guiStyle_BoxMessage = new GUIStyle(GUI.skin.box);
				_guiStyle_BoxMessage.alignment = TextAnchor.MiddleCenter;
				_guiStyle_BoxMessage.wordWrap = true;
			}


			int width_2Btn = (width - 14) / 2;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
			GUILayout.Space(5);
			if(apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_Bake), _tab == TAB.Bake, width_2Btn, 25))
			{
				_tab = TAB.Bake;
			}
			if(apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_Setting), _tab == TAB.Options, width_2Btn, 25))
			{
				_tab = TAB.Options;
			}
			EditorGUILayout.EndHorizontal();


			if (_tab == TAB.Bake)
			{
				Draw_BakeTab(width, height);
			}
			else
			{
				Draw_SettingTab(width, height);
			}

			GUILayout.Space(5);
		}





		// 1. Bake에 대한 UI
		private void Draw_BakeTab(int width, int height)
		{
			GUILayout.Space(5);

			if (_guiContent_BakeBtn_Normal == null)
			{
				_guiContent_BakeBtn_Normal = apGUIContentWrapper.Make(4, _editor.GetText(TEXT.DLG_Bake), _editor.ImageSet.Get(apImageSet.PRESET.BakeBtn_Normal));
			}
			if (_guiContent_BakeBtn_Optimized == null)
			{
				_guiContent_BakeBtn_Optimized = apGUIContentWrapper.Make(4, _editor.GetText(TEXT.DLG_OptimizedBakeTo), _editor.ImageSet.Get(apImageSet.PRESET.BakeBtn_Optimized));
			}

			//Bake 설정
			//EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_BakeSetting));//"Bake Setting"
			//GUILayout.Space(5);

			EditorGUILayout.ObjectField(_editor.GetText(TEXT.DLG_Portrait), _targetPortrait, typeof(apPortrait), true);//"Portait"

			GUILayout.Space(5);

			//"Bake Scale"
			float prevBakeScale = _targetPortrait._bakeScale;
			_targetPortrait._bakeScale = EditorGUILayout.FloatField(_editor.GetText(TEXT.DLG_BakeScale), _targetPortrait._bakeScale);

			//"Z Per Depth"
			float prevBakeZSize = _targetPortrait._bakeZSize;
			_targetPortrait._bakeZSize = EditorGUILayout.FloatField(_editor.GetText(TEXT.DLG_ZPerDepth), _targetPortrait._bakeZSize);

			if (_targetPortrait._bakeZSize < 0.5f)
			{
				_targetPortrait._bakeZSize = 0.5f;
			}

			if (prevBakeScale != _targetPortrait._bakeScale ||
				prevBakeZSize != _targetPortrait._bakeZSize)
			{
				apEditorUtil.SetDirty(_editor);
			}


			//Bake 버튼
			GUILayout.Space(10);


			//[ Bake ] 기능 실행
			if (GUILayout.Button(_guiContent_BakeBtn_Normal.Content, GUILayout.Height(45)))//"Bake"//_editor.GetText(TEXT.DLG_Bake)
			{
				GUI.FocusControl(null);

				//CheckChangedProperties(nextRootScale, nextZScale);
				apEditorUtil.SetDirty(_editor);


				//추가 22.1.7 : SRP 옵션이 적절한지 물어보고 자동으로 변경한다.
				CheckSRPOption();


				//-------------------------------------
				// Bake 함수를 실행한다. << 중요오오오오
				//-------------------------------------

				apBakeResult bakeResult = _editor.Controller.Bake();

				if (bakeResult != null)
				{
					_editor.Notification("[" + _targetPortrait.name + "] is Baked", false, false);

					if (bakeResult.NumUnlinkedExternalObject > 0)
					{
						EditorUtility.DisplayDialog(_editor.GetText(TEXT.BakeWarning_Title),
							_editor.GetTextFormat(TEXT.BakeWarning_Body, bakeResult.NumUnlinkedExternalObject),
							_editor.GetText(TEXT.Okay));
					}

					//추가 3.29 : Bake 후에 Ambient를 체크하자
					CheckAmbientAndCorrection();

					//추가 22.1.6 : Bake 후에 URP 설정 체크를 하자 > 일단 보류. 문제가 확인되지 않는다.
					//CheckURP2022SettingsIfAnyClippedMeshes();

					//추가 22.5.16 : Bake가 끝난 후에 자동으로 선택하자
					Selection.activeGameObject = _targetPortrait.gameObject;

					//자동 저장
					if(_editor._isSaveProjectWhenBaked)
					{
						apEditorUtil.SaveUnityProject();
					}


					//[v1.5.0]
					//Bake 직후에 애니메이션 편집 중 / 루트 유닛 상태에서
					//애니메이션 업데이트 중 Control Param 업데이트를 다시 해준다.
					//컨트롤 파라미터 값이 원래대로 돌아가는 문제가 있다.
					if (_editor.Select.SelectionType == apSelection.SELECTION_TYPE.Overall)
					{
						if (_editor.Select.RootUnitAnimClip != null)
						{
							_editor.Select.RootUnitAnimClip.UpdateControlParam_Editor();
						}
					}
					if (_editor.Select.SelectionType == apSelection.SELECTION_TYPE.Animation)
					{
						if (_editor.Select.AnimClip != null)
						{
							_editor.Select.AnimClip.UpdateControlParam_Editor();
						}
					}

					_editor.SetRepaint();
				}
				else
				{
					//추가 20.11.7 : Bake가 실패한 경우
					_editor.Notification("Bake is canceled", false, false);
				}
			}


			DrawDelimeter(width - 5);


			//최적화 Bake
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_OptimizedBaking));//"Optimized Baking"

			//"Target"
			apPortrait nextOptPortrait = (apPortrait)EditorGUILayout.ObjectField(_editor.GetText(TEXT.DLG_Target), _targetPortrait._bakeTargetOptPortrait, typeof(apPortrait), true);

			if (nextOptPortrait != _targetPortrait._bakeTargetOptPortrait)
			{
				//타겟을 바꾸었다.
				bool isChanged = false;
				if (nextOptPortrait != null)
				{
					//1. 다른 Portrait를 선택했다.
					if (!nextOptPortrait._isOptimizedPortrait)
					{
						//1-1. 최적화된 객체가 아니다.
						EditorUtility.DisplayDialog(_editor.GetText(TEXT.OptBakeError_Title),
													_editor.GetText(TEXT.OptBakeError_NotOptTarget_Body),
													_editor.GetText(TEXT.Close));
					}
					else if (nextOptPortrait._bakeSrcEditablePortrait != _targetPortrait)
					{
						//1-2. 다른 대상으로부터 Bake된 Portrait같다. (물어보고 계속)
						bool isResult = EditorUtility.DisplayDialog(_editor.GetText(TEXT.OptBakeError_Title),
													_editor.GetText(TEXT.OptBakeError_SrcMatchError_Body),
													_editor.GetText(TEXT.Okay),
													_editor.GetText(TEXT.Cancel));

						if (isResult)
						{
							//뭐 선택하겠다는데요 뭐..
							isChanged = true;

						}
					}
					else
					{
						//1-3. 오케이. 변경 가능
						isChanged = true;
					}
				}
				else
				{
					//2. 선택을 해제했다.
					isChanged = true;
				}

				if (isChanged)
				{
					//Target을 변경한다.
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
														_editor,
														_targetPortrait,
														//null, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

					_targetPortrait._bakeTargetOptPortrait = nextOptPortrait;
				}

			}

			string optBtnText = "";
			if (_targetPortrait._bakeTargetOptPortrait != null)
			{
				optBtnText = string.Format("{0}\n    [{1}]", _editor.GetText(TEXT.DLG_OptimizedBakeTo), _targetPortrait._bakeTargetOptPortrait.gameObject.name);
			}
			else
			{
				optBtnText = _editor.GetText(TEXT.DLG_OptimizedBakeMakeNew);
			}

			_guiContent_BakeBtn_Optimized.SetText(4, optBtnText);
			GUILayout.Space(10);

			Color prevColor = GUI.backgroundColor;

			if(apVersion.I.IsDemo)
			{
				//데모에선 해당 버튼 비활성
				GUI.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);
			}
			
			// [ Optimized Bake ] 기능 실행
			if (GUILayout.Button(_guiContent_BakeBtn_Optimized.Content, GUILayout.Height(45)))
			{
				GUI.FocusControl(null);

				if (apVersion.I.IsDemo)
				{
					//데모 제한 다이얼로그
					EditorUtility.DisplayDialog(_editor.GetText(TEXT.DemoLimitation_Title),
													_editor.GetText(TEXT.DemoLimitation_Body),
													_editor.GetText(TEXT.Okay));
				}
				else
				{
					// < 풀버전 전용 코드 >
					#region ONLY_FULLVERSION

					//CheckChangedProperties(nextRootScale, nextZScale);

					//추가 22.1.7 : SRP 옵션이 적절한지 물어보고 자동으로 변경한다.
					CheckSRPOption();


					//Optimized Bake를 하자
					apBakeResult bakeResult = _editor.Controller.Bake_Optimized(_targetPortrait, _targetPortrait._bakeTargetOptPortrait);

					if (bakeResult != null)
					{
						if (bakeResult.NumUnlinkedExternalObject > 0)
						{
							EditorUtility.DisplayDialog(_editor.GetText(TEXT.BakeWarning_Title),
								_editor.GetTextFormat(TEXT.BakeWarning_Body, bakeResult.NumUnlinkedExternalObject),
								_editor.GetText(TEXT.Okay));
						}

						_editor.Notification("[" + _targetPortrait.name + "] is Baked (Optimized)", false, false);

						//추가 3.29 : Bake 후에 Ambient를 체크하자
						CheckAmbientAndCorrection();

						//추가 22.1.6 : Bake 후에 URP 설정 체크를 하자 > 일단 보류. 문제가 확인되지 않는다.
						//CheckURP2022SettingsIfAnyClippedMeshes();

						if (_targetPortrait._bakeTargetOptPortrait != null)
						{
							//추가 22.5.16 : Bake가 끝난 후에 자동으로 선택하자
							Selection.activeGameObject = _targetPortrait._bakeTargetOptPortrait.gameObject;
						}

						//자동 저장
						if(_editor._isSaveProjectWhenBaked)
						{
							apEditorUtil.SaveUnityProject();
						}

					}
					else
					{
						//Bake가 취소되었다. (20.11.7)
						_editor.Notification("Bake is canceled (Optimized)", false, false);
					}

					#endregion
				}

				
			}

			GUI.backgroundColor = prevColor;

			DrawDelimeter(width - 5);

			//TODO : 언어
			bool isPrevSaveOption = _editor._isSaveProjectWhenBaked;
			//"Bake 후 자동 저장"
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			GUILayout.Space(2);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_SaveScenesAfterBake), GUILayout.Width(width - 40));
			_editor._isSaveProjectWhenBaked = EditorGUILayout.Toggle(_editor._isSaveProjectWhenBaked, GUILayout.Width(30));
			EditorGUILayout.EndHorizontal();
			

			if(_editor._isSaveProjectWhenBaked != isPrevSaveOption)
			{
				_editor.SaveEditorPref();
			}
		}


		// 2. Setting에 대한 UI
		private void Draw_SettingTab(int width, int height)
		{
			//Vector2 curScroll = (_tab == TAB.Bake) ? _scroll_Bake : _scroll_Options;

			Color prevColor = GUI.backgroundColor;

			_scroll_Options = EditorGUILayout.BeginScrollView(_scroll_Options, false, true, GUILayout.Width(width), GUILayout.Height(height - 30));

			EditorGUILayout.BeginVertical(GUILayout.Width(width - 24));
			GUILayout.Space(5);

			width -= 24;

			// [ 프로젝트 설정 ]
			DrawCategoryTitle(_editor.GetText(TEXT.DLG_Project), width, 0);//카테고리 타이틀



			//1. Color Space [Gamma / Linear]
			//bool prevBakeGamma = _editor._isBakeColorSpaceToGamma;//이전
			bool prevBakeGamma = _editor.ProjectSettingData.Project_IsColorSpaceGamma;//변경 v1.4.2

			int iPrevColorSpace = prevBakeGamma ? 0 : 1;
			//int iNextColorSpace = EditorGUILayout.Popup(_editor.GetUIWord(UIWORD.ColorSpace), iPrevColorSpace, _colorSpaceNames);
			int iNextColorSpace = Layout_Popup(_editor.GetUIWord(UIWORD.ColorSpace), iPrevColorSpace, _colorSpaceNames, width);
			if (iNextColorSpace != iPrevColorSpace)
			{
				apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
													_editor,
													_targetPortrait,
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				if (iNextColorSpace == 0)
				{
					//Gamma
					//_editor._isBakeColorSpaceToGamma = true;
					_editor.ProjectSettingData.SetColorSpaceGamma(true);//변경 v1.4.2
				}
				else
				{
					//Linear
					//_editor._isBakeColorSpaceToGamma = false;
					_editor.ProjectSettingData.SetColorSpaceGamma(false);//변경 v1.4.2
				}
			}

			

			//7. LWRP / URP
			//LWRP 쉐이더를 쓸지 여부와 다시 강제로 생성하기 버튼을 만들자. : 이건 2019부터 적용 (그 전에는 SRP용 처리가 안된다.)
//#if UNITY_2019_1_OR_NEWER
			
			//DrawDelimeter(width);
			//GUILayout.Space(10);

			//bool prevUseLWRP = _editor._isUseSRP;//이전
			bool prevUseLWRP = _editor.ProjectSettingData.Project_IsUseSRP;//변경 [v1.4.2]

			int iPrevUseLWRP = prevUseLWRP ? 1 : 0;
			int iNextUseLWRP = Layout_Popup(_editor.GetText(TEXT.RenderPipeline), iPrevUseLWRP, _renderPipelineNames, width);//"Render Pipeline"
			if (iNextUseLWRP != iPrevUseLWRP)
			{
				//이건 Undo 대상이 아니다.
				//apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Portrait_SettingChanged, 
				//									_editor, 
				//									_targetPortrait, 
				//									false, 
				//									apEditorUtil.UNDO_STRUCT.ValueOnly);
				if (iNextUseLWRP == 0)
				{
					//사용 안함
					//_editor._isUseSRP = false;
					_editor.ProjectSettingData.SetUseSRP(false);
				}
				else
				{
					//LWRP 사용함
					//_editor._isUseSRP = true;
					_editor.ProjectSettingData.SetUseSRP(true);//URP 활성
				}

				_editor.SaveEditorPref();
			}

//#endif

			GUILayout.Space(10);

			//11.7 추가 : Ambient Light를 검은색으로 만든다.
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_AmbientToBlack), GUILayout.Height(24)))
			{
				MakeAmbientLightToBlack();
			}


			GUILayout.Space(25);



			// [ 포트레이트 설정 ]
			DrawCategoryTitle(_editor.GetText(TEXT.DLG_Portrait), width, 1);//카테고리 타이틀

			//사용자가 기본값으로서 저장된 값이 있는지 확인한다.
			bool isDefaultSaved = _editor.ProjectSettingData.IsCommonSettingSaved;
			

			//2. Sorting Layer
			int curSortingLayerIndex = FindSortingLayerIndex(_targetPortrait._sortingLayerID);
			int commonSortingLayerIndex = -1;

			if(isDefaultSaved)
			{
				commonSortingLayerIndex = FindSortingLayerIndex(_editor.ProjectSettingData.Common_SortingLayerID);
			}
			
			if (curSortingLayerIndex < 0)
			{
				apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
													_editor,
													_targetPortrait,
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				//어라 레이어가 없는데용..
				//초기화해야겠다.
				_targetPortrait._sortingLayerID = -1;
				if (SortingLayer.layers.Length > 0)
				{
					_targetPortrait._sortingLayerID = SortingLayer.layers[0].id;
					curSortingLayerIndex = 0;
				}

				apEditorUtil.SetDirty(_editor);
			}
			//Sorting Layer
			int nextIndex = Layout_Popup(	_editor.GetText(TEXT.SortingLayer),
											curSortingLayerIndex,
											_sortingLayerNames,
											width,
											isDefaultSaved && commonSortingLayerIndex >= 0,
											commonSortingLayerIndex);

			if (nextIndex != curSortingLayerIndex)
			{
				apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
													_editor,
													_targetPortrait,
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
				//레이어가 변경되었다.
				if (nextIndex >= 0 && nextIndex < SortingLayer.layers.Length)
				{
					//LayerID 변경
					_targetPortrait._sortingLayerID = SortingLayer.layers[nextIndex].id;
				}

				apEditorUtil.SetDirty(_editor);
			}

			//추가 19.8.18 : Sorting Order를 지정하는 방식을 3가지 + 미적용 1가지로 더 세분화
			apPortrait.SORTING_ORDER_OPTION nextSortingLayerOption = (apPortrait.SORTING_ORDER_OPTION)Layout_EnumPopup(	_editor.GetText(TEXT.SortingOrderOption),
																														_targetPortrait._sortingOrderOption,
																														width,
																														isDefaultSaved,
																														_editor.ProjectSettingData.Common_SortingLayerOption);
			if (nextSortingLayerOption != _targetPortrait._sortingOrderOption)
			{
				apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
													_editor,
													_targetPortrait,
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				_targetPortrait._sortingOrderOption = nextSortingLayerOption;
				apEditorUtil.SetDirty(_editor);
			}

			if (_targetPortrait._sortingOrderOption == apPortrait.SORTING_ORDER_OPTION.SetOrder)
			{
				//Set Order인 경우에만 한정
				int nextOrder = Layout_Int(	_editor.GetText(TEXT.SortingOrder),
											_targetPortrait._sortingOrder,
											width,
											isDefaultSaved,
											_editor.ProjectSettingData.Common_SortingOrder);

				if (nextOrder != _targetPortrait._sortingOrder)
				{
					apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Portrait_SettingChanged,
														_editor,
														_targetPortrait,
														//null, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

					_targetPortrait._sortingOrder = nextOrder;
					apEditorUtil.SetDirty(_editor);
				}
			}
			else if (_targetPortrait._sortingOrderOption == apPortrait.SORTING_ORDER_OPTION.DepthToOrder
				|| _targetPortrait._sortingOrderOption == apPortrait.SORTING_ORDER_OPTION.ReverseDepthToOrder)
			{
				//추가 21.1.31 : Depth To Order일때, 1씩만 증가하는게 아닌 더 큰값으로 증가할 수도 있게 만들자
				int nextOrderPerDepth = Layout_Int(	_editor.GetText(TEXT.OrderPerDepth),
													_targetPortrait._sortingOrderPerDepth,
													width,
													isDefaultSaved,
													_editor.ProjectSettingData.Common_SortingOrderPerDepth);


				if (nextOrderPerDepth != _targetPortrait._sortingOrderPerDepth)
				{
					if (nextOrderPerDepth < 1)
					{
						nextOrderPerDepth = 1;
					}

					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
														_editor,
														_targetPortrait,
														//null, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

					_targetPortrait._sortingOrderPerDepth = nextOrderPerDepth;
					apEditorUtil.SetDirty(_editor);
				}
			}

			DrawDelimeter(width);


			//3. 메카님 사용 여부

			//변경 [v1.4.2]
			bool nextIsUseMecanim = Layout_Toggle(	_editor.GetText(TEXT.IsMecanimAnimation),
																_targetPortrait._isUsingMecanim,
																width,
																isDefaultSaved,
																_editor.ProjectSettingData.Common_IsUsingMecanim);

			if(nextIsUseMecanim != _targetPortrait._isUsingMecanim)
			{
				apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Portrait_BakeOptionChanged,
													_editor,
													_targetPortrait,
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				_targetPortrait._isUsingMecanim = nextIsUseMecanim;
				apEditorUtil.SetDirty(_editor);
			}
			
			if (_targetPortrait._isUsingMecanim)
			{
				EditorGUILayout.LabelField(_editor.GetText(TEXT.AnimationClipExportPath));//"Animation Clip Export Path"
				GUIStyle guiStyle_ChangeBtn = new GUIStyle(GUI.skin.button);
				guiStyle_ChangeBtn.margin = GUI.skin.textField.margin;
				guiStyle_ChangeBtn.border = GUI.skin.textField.border;

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(20));
				GUILayout.Space(5);

				if(string.IsNullOrEmpty(_targetPortrait._mecanimAnimClipResourcePath))
				{
					GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
				}

				EditorGUILayout.TextField(_targetPortrait._mecanimAnimClipResourcePath, GUILayout.Width(width - (70 + 9)));

				GUI.backgroundColor = prevColor;

				if (GUILayout.Button(_editor.GetText(TEXT.DLG_Change), guiStyle_ChangeBtn, GUILayout.Width(70), GUILayout.Height(18)))
				{
					string nextPath = EditorUtility.SaveFolderPanel("Select to export animation clips", "", "");
					if (!string.IsNullOrEmpty(nextPath))
					{
						//추가 21.7.3 : 경로 입력 변경 (Escape 문자 삭제 %20 같은거)
						nextPath = apUtil.ConvertEscapeToPlainText(nextPath);

						if (apEditorUtil.IsInAssetsFolder(nextPath))
						{
							//유효한 폴더인 경우
							//중요 : 경로가 절대 경로로 찍힌다.
							//상대 경로로 바꾸자
							apEditorUtil.PATH_INFO_TYPE pathInfoType = apEditorUtil.GetPathInfo(nextPath);
							if (pathInfoType == apEditorUtil.PATH_INFO_TYPE.Absolute_InAssetFolder)
							{
								//절대 경로 + Asset 폴더 안쪽이라면
								//Debug.LogError("절대 경로가 리턴 되었다. : " + nextPath);
								nextPath = apEditorUtil.AbsolutePath2RelativePath(nextPath);
								//Debug.LogError(">> 상대 경로로 변경 : " + nextPath);
							}

							apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_BakeOptionChanged,
																_editor,
																_targetPortrait,
																false,
																apEditorUtil.UNDO_STRUCT.ValueOnly);

							_targetPortrait._mecanimAnimClipResourcePath = nextPath;
							apEditorUtil.SetDirty(_editor);
						}
						else
						{
							//유효한 폴더가 아닌 경우
							//EditorUtility.DisplayDialog("Invalid Folder Path", "Invalid Clip Path", "Close");
							EditorUtility.DisplayDialog(
								_editor.GetText(TEXT.DLG_AnimClipSavePathValidationError_Title),
								_editor.GetText(TEXT.DLG_AnimClipSavePathResetError_Body),
								_editor.GetText(TEXT.Close));
						}
					}

					GUI.FocusControl(null);

				}

				
				EditorGUILayout.EndHorizontal();
			}
			

			DrawDelimeter(width);

			//업데이트 옵션 포괄
			EditorGUILayout.LabelField(_editor.GetText(TEXT.UpdateOption));
			GUILayout.Space(5);

			if (_guiContent_Setting_IsImportant == null)
			{
				_guiContent_Setting_IsImportant = apGUIContentWrapper.Make(_editor.GetText(TEXT.DLG_Setting_IsImportant), false, "When this setting is on, it always updates and the physics effect works.");
			}
			if (_guiContent_Setting_FPS == null)
			{
				_guiContent_Setting_FPS = apGUIContentWrapper.Make(_editor.GetText(TEXT.DLG_Setting_FPS), false, "This setting is used when <Important> is off");
			}

			if(_guiContent_Setting_ProcessEvent == null)
			{
				_guiContent_Setting_ProcessEvent = apGUIContentWrapper.Make(_editor.GetText(TEXT.MainLogicEvent), false, "Unity’s event function where the main logic of apPortrait is executed.");
			}
			//Process Event
			apPortrait.PROCESS_EVENT_ON nextProcessEvent = (apPortrait.PROCESS_EVENT_ON)Layout_Popup(_guiContent_Setting_ProcessEvent.Content,
																										(int)_targetPortrait._mainProcessEvent,
																										_mainLogicEventLabel,
																										width,
																										isDefaultSaved,
																										(int)_editor.ProjectSettingData.Common_MainProcessEvent);
			if(nextProcessEvent != _targetPortrait._mainProcessEvent)
			{
				apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
													_editor,
													_targetPortrait,
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				_targetPortrait._mainProcessEvent = nextProcessEvent;
				apEditorUtil.SetDirty(_editor);
			}

			GUILayout.Space(5);

			//Important 먼저


			//4. Important
			bool nextImportant = Layout_Toggle(	_guiContent_Setting_IsImportant.Content,
												_targetPortrait._isImportant,
												width,
												isDefaultSaved,
												_editor.ProjectSettingData.Common_IsImportant);

			if (nextImportant != _targetPortrait._isImportant)
			{
				apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
													_editor,
													_targetPortrait,
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				_targetPortrait._isImportant = nextImportant;
				apEditorUtil.SetDirty(_editor);
			}

			//"FPS (Important Off)"
			if (!_targetPortrait._isImportant)
			{
				//[1.4.2 변경] : Important가 꺼질 때에만 FPS가 보이도록 한다.
				
				//변경 [v1.4.2]
				int nextFPS = Layout_DelayedInt(	_guiContent_Setting_FPS.Content,
													_targetPortrait._FPS,
													width,
													isDefaultSaved,
													_editor.ProjectSettingData.Common_FPS);

				if (_targetPortrait._FPS != nextFPS)
				{
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
														_editor,
														_targetPortrait,
														//null, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);
					if (nextFPS < 10)
					{
						nextFPS = 10;
					}
					_targetPortrait._FPS = nextFPS;
					apEditorUtil.SetDirty(_editor);
					GUI.FocusControl(null);
				}

				GUILayout.Space(5);
			}

			GUILayout.Space(5);

			//v1.4.7 : 메시 갱신 옵션 
			if (_guiContent_Setting_MeshUpdateFrequency == null)
			{
				_guiContent_Setting_MeshUpdateFrequency = apGUIContentWrapper.Make(_editor.GetText(TEXT.MeshUpdateFrequency), false, "This option determines whether meshes are updated every frame or at regular intervals.");
			}
			if (_guiContent_Setting_MeshFreqFPS == null)
			{
				_guiContent_Setting_MeshFreqFPS = apGUIContentWrapper.Make(_editor.GetText(TEXT.DLG_Setting_FPS), false, "This is the FPS option when the mesh is updated at regular intervals. (1~30)");
			}
			if(_guiContent_Setting_MeshFreqFPSScaleOpt == null)
			{
				_guiContent_Setting_MeshFreqFPSScaleOpt = apGUIContentWrapper.Make(_editor.GetText(TEXT.FPSScaleOption), false, "This is an option to change FPS according to timeScale.");
			}




			int nextMeshUpdateFreqOption = Layout_Popup(	_guiContent_Setting_MeshUpdateFrequency.Content,
															(int)_targetPortrait._meshRefreshRateOption,
															_meshUpdateFrequencyLabel,
															width,
															isDefaultSaved,
															(int)_editor.ProjectSettingData.Common_MeshUpdateFrequency);

			if (nextMeshUpdateFreqOption != (int)_targetPortrait._meshRefreshRateOption)
			{
				apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
													_editor,
													_targetPortrait,
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				_targetPortrait._meshRefreshRateOption = (apPortrait.MESH_UPDATE_FREQUENCY)nextMeshUpdateFreqOption;
				apEditorUtil.SetDirty(_editor);
			}


			if(_targetPortrait._meshRefreshRateOption != apPortrait.MESH_UPDATE_FREQUENCY.EveryFrames)
			{
				//일정 시간마다 업데이트 하는 경우

				//변경 [v1.4.2]
				int nextMeshUpdateFPS = Layout_DelayedInt(	_guiContent_Setting_MeshFreqFPS.Content,
															_targetPortrait._meshRefreshRateFPS,
															width,
															isDefaultSaved,
															_editor.ProjectSettingData.Common_MeshUpdateFPS);

				if (_targetPortrait._meshRefreshRateFPS != nextMeshUpdateFPS)
				{
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
														_editor,
														_targetPortrait,
														//null, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);
					if(nextMeshUpdateFPS < 1)
					{
						nextMeshUpdateFPS = 1;
					}
					else if(nextMeshUpdateFPS > 30)
					{
						nextMeshUpdateFPS = 30;
					}
					_targetPortrait._meshRefreshRateFPS = nextMeshUpdateFPS;
					apEditorUtil.SetDirty(_editor);
					GUI.FocusControl(null);
				}


				if (_targetPortrait._meshRefreshRateOption == apPortrait.MESH_UPDATE_FREQUENCY.FixedFrames_NotSync)
				{
					//추가 v1.4.9 : 메시 업데이트 옵션 중 FPS의 가변 여부 (Sync가 아닌 경우에만)
					apPortrait.FIXED_UPDATE_FPS_SCALE_OPTION nextFPSScaleOption = (apPortrait.FIXED_UPDATE_FPS_SCALE_OPTION)Layout_EnumPopup(
																					_guiContent_Setting_MeshFreqFPSScaleOpt.Content,
																					_targetPortrait._meshRefreshFPSScaleOption,
																					width,
																					isDefaultSaved,
																					_editor.ProjectSettingData.Common_MeshUpdateFPSScaled);

					if (nextFPSScaleOption != _targetPortrait._meshRefreshFPSScaleOption)
					{
						apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
															_editor,
															_targetPortrait,
															//null, 
															false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

						_targetPortrait._meshRefreshFPSScaleOption = nextFPSScaleOption;
						apEditorUtil.SetDirty(_editor);
						GUI.FocusControl(null);
					}
				}
				
			}

			GUILayout.Space(5);

			//v1.5.0 : 안보이는 메시의 업데이트 여부
			if(_guiContent_Setting_InvisibleMesh == null)
			{
				_guiContent_Setting_InvisibleMesh = apGUIContentWrapper.Make(_editor.GetText(TEXT.Setting_InvisibleMeshes), false, "This option determines whether to apply the results of the modifier to invisible meshes.");
			}

			int nextInvisibleMeshUpdate = Layout_Popup(	_guiContent_Setting_InvisibleMesh.Content,
														(int)_targetPortrait._invisibleMeshUpdate,
														_invisibleMeshUpdateLabel,
														width,
														isDefaultSaved,
														(int)_editor.ProjectSettingData.Common_InvisibleMeshUpdate);

			if(nextInvisibleMeshUpdate != (int)_targetPortrait._invisibleMeshUpdate)
			{
				apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
													_editor,
													_targetPortrait,
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				_targetPortrait._invisibleMeshUpdate = (apPortrait.INVISIBLE_MESH_UPDATE)nextInvisibleMeshUpdate;
				apEditorUtil.SetDirty(_editor);
			}

#if UNITY_2017_1_OR_NEWER
			GUILayout.Space(5);

			//v1.5.1 : 클리핑 메시의 처리 타이밍 (Unity 2017부터)
			if(_guiContent_Setting_ClippingUpdate == null)
			{
				_guiContent_Setting_ClippingUpdate = apGUIContentWrapper.Make(_editor.GetText(TEXT.Setting_ClippingProcess), false, "This option determines when processing of clipping meshes will be performed.");
			}

			int nextClippingUpdate = Layout_Popup(	_guiContent_Setting_ClippingUpdate.Content,
														(int)_targetPortrait._clippingMeshUpdate,
														_clippingMeshUpdateLabel,
														width,
														isDefaultSaved,
														(int)_editor.ProjectSettingData.Common_ClippingUpdate);

			if(nextClippingUpdate != (int)_targetPortrait._clippingMeshUpdate)
			{
				apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
													_editor,
													_targetPortrait,
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				_targetPortrait._clippingMeshUpdate = (apPortrait.CLIPPING_MESH_UPDATE)nextClippingUpdate;
				apEditorUtil.SetDirty(_editor);
			}

#endif


			DrawDelimeter(width);

			//5. Billboard + Perspective

			//변경 [v1.4.2]
			apPortrait.BILLBOARD_TYPE nextBillboardType = (apPortrait.BILLBOARD_TYPE)Layout_Popup(	_editor.GetText(TEXT.DLG_Billboard),
																									(int)_targetPortrait._billboardType,
																									_billboardTypeNames,
																									width,
																									isDefaultSaved,
																									(int)_editor.ProjectSettingData.Common_BillboardOption);
			if (nextBillboardType != _targetPortrait._billboardType)
			{
				apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
													_editor,
													_targetPortrait,
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				_targetPortrait._billboardType = nextBillboardType;
				apEditorUtil.SetDirty(_editor);
			}

			
			//추가 19.9.24 : Billboard인 경우 카메라의 SortMode를 OrthoGraphic으로 강제할지 여부
			if (_targetPortrait._billboardType != apPortrait.BILLBOARD_TYPE.None)
			{
				GUILayout.Space(2);

				//변경 [1.4.2]
				bool nextForceSortModeToOrtho = Layout_Toggle_WrapLabel(	_editor.GetText(TEXT.SetSortMode2Orthographic),
																			_targetPortrait._isForceCamSortModeToOrthographic,
																			width,
																			isDefaultSaved,
																			_editor.ProjectSettingData.Common_IsForceCamSortModeToOrthographic);

				if (nextForceSortModeToOrtho != _targetPortrait._isForceCamSortModeToOrthographic)
				{
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
														_editor,
														_targetPortrait,
														//null, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

					_targetPortrait._isForceCamSortModeToOrthographic = nextForceSortModeToOrtho;
					apEditorUtil.SetDirty(_editor);
				}

				GUILayout.Space(2);

				//추가 v1.5.0: 부모의 Rotation을 적용하는 옵션
				apPortrait.BILLBOARD_PARENT_ROTATION nextBillboardParentOption = (apPortrait.BILLBOARD_PARENT_ROTATION)Layout_Popup(	_editor.GetText(TEXT.DLG_ParentRotation),
																									(int)_targetPortrait._billboardParentRotation,
																									_billboardParentOptionNames,
																									width,
																									isDefaultSaved,
																									(int)_editor.ProjectSettingData.Common_BillboardParentRotation);
				if (nextBillboardParentOption != _targetPortrait._billboardParentRotation)
				{
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
														_editor,
														_targetPortrait,
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

					_targetPortrait._billboardParentRotation = nextBillboardParentOption;
					apEditorUtil.SetDirty(_editor);
				}
				
			}




			DrawDelimeter(width);

			//6. Shadow

			//변경 1.4.2
			apPortrait.SHADOW_CASTING_MODE nextChastShadows = (apPortrait.SHADOW_CASTING_MODE)Layout_EnumPopup(	_editor.GetUIWord(UIWORD.CastShadows),
																												_targetPortrait._meshShadowCastingMode,
																												width,
																												isDefaultSaved,
																												_editor.ProjectSettingData.Common_CastShadows);
			
			bool nextReceiveShaodw = Layout_Toggle(	_editor.GetUIWord(UIWORD.ReceiveShadows),
													_targetPortrait._meshReceiveShadow,
													width,
													isDefaultSaved,
													_editor.ProjectSettingData.Common_IsReceiveShadows);

			//추가 v1.5.0 : Light Probe / Reflection Probe
			GUILayout.Space(2);
			apPortrait.LIGHT_PROBE_USAGE nextLightProbeUsage = (apPortrait.LIGHT_PROBE_USAGE)Layout_EnumPopup(	_editor.GetUIWord(UIWORD.LightProbes),
																												_targetPortrait._meshLightProbeUsage,
																												width,
																												isDefaultSaved,
																												_editor.ProjectSettingData.Common_LightProbeUsage);

			apPortrait.REFLECTION_PROBE_USAGE nextReflectionProbeUsage = (apPortrait.REFLECTION_PROBE_USAGE)Layout_EnumPopup(
																												_editor.GetUIWord(UIWORD.ReflectionProbes),
																												_targetPortrait._meshReflectionProbeUsage,
																												width,
																												isDefaultSaved,
																												_editor.ProjectSettingData.Common_ReflectionProbeUsage);




			if (nextChastShadows != _targetPortrait._meshShadowCastingMode
				|| nextReceiveShaodw != _targetPortrait._meshReceiveShadow
				|| nextLightProbeUsage != _targetPortrait._meshLightProbeUsage
				|| nextReflectionProbeUsage != _targetPortrait._meshReflectionProbeUsage)
			{
				apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
													_editor,
													_targetPortrait,
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				_targetPortrait._meshShadowCastingMode = nextChastShadows;
				_targetPortrait._meshReceiveShadow = nextReceiveShaodw;
				_targetPortrait._meshLightProbeUsage = nextLightProbeUsage;
				_targetPortrait._meshReflectionProbeUsage = nextReflectionProbeUsage;

				apEditorUtil.SetDirty(_editor);
			}

			

			DrawDelimeter(width);

			//8. VR Supported 19.9.24 추가

			//VR Supported
			//변경 1.4.2
			int iNextVRSupported = Layout_Popup(	_editor.GetText(TEXT.VROption),
													(int)_targetPortrait._vrSupportMode,
													_vrSupportModeLabel,
													width,
													isDefaultSaved,
													(int)_editor.ProjectSettingData.Common_VRSupported);

			if (iNextVRSupported != (int)_targetPortrait._vrSupportMode)
			{
				apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
													_editor,
													_targetPortrait,
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				_targetPortrait._vrSupportMode = (apPortrait.VR_SUPPORT_MODE)iNextVRSupported;

				apEditorUtil.SetDirty(_editor);
			}

			

			if (_targetPortrait._vrSupportMode == apPortrait.VR_SUPPORT_MODE.SingleCamera)
			{
				//Single Camera인 경우, Clipping Mask의 크기를 결정해야한다.

				//변경 1.4.2
				int iNextVRRTSize = Layout_Popup(	_editor.GetUIWord(UIWORD.MaskTextureSize),
													(int)_targetPortrait._vrRenderTextureSize,
													_vrRTSizeLabel,
													width,
													isDefaultSaved,
													(int)_editor.ProjectSettingData.Common_VRRenterTextureSize
													);


				if (iNextVRRTSize != (int)_targetPortrait._vrRenderTextureSize)
				{
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
														_editor,
														_targetPortrait,
														//null, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

					_targetPortrait._vrRenderTextureSize = (apPortrait.VR_RT_SIZE)iNextVRRTSize;

					apEditorUtil.SetDirty(_editor);
				}
			}

			//[v1.5.0] 카메라 자동 감지 방식
			int iNextCameraCheckMode = Layout_Popup(	_editor.GetText(TEXT.Setting_CheckCameras),
														(int)_targetPortrait._cameraCheckMode,
														_cameraCheckModeLabel,
														width,
														isDefaultSaved,
														(int)_editor.ProjectSettingData.Common_CameraCheckMode);

			if(iNextCameraCheckMode != (int)_targetPortrait._cameraCheckMode)
			{
				apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
													_editor,
													_targetPortrait,
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				_targetPortrait._cameraCheckMode = (apPortrait.CAMERA_CHECK_MODE)iNextCameraCheckMode;
				apEditorUtil.SetDirty(_editor);
			}



			DrawDelimeter(width);






			//추가 20.8.11 : Flipped Mesh를 체크하는 방법을 정하도록 만들자
			//변경 1.4.2
			int iNextFlippedMeshOption = Layout_Popup(	_editor.GetText(TEXT.Setting_FlippedMesh),
														(int)_targetPortrait._flippedMeshOption,
														_flippedMeshOptionLabel,
														width,
														isDefaultSaved,
														(int)_editor.ProjectSettingData.Common_FlippedMeshOption
														);


			if (iNextFlippedMeshOption != (int)_targetPortrait._flippedMeshOption)
			{
				apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
													_editor,
													_targetPortrait,
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				_targetPortrait._flippedMeshOption = (apPortrait.FLIPPED_MESH_CHECK)iNextFlippedMeshOption;

				apEditorUtil.SetDirty(_editor);
			}

			
			DrawDelimeter(width);


			//추가 20.8.14 : 루트 본의 스케일 옵션을 직접 정한다. [Skew 문제]
			//변경
			int iNextRootBoneScaleOption = Layout_Popup(	_editor.GetText(TEXT.Setting_ScaleOfRootBone),
															(int)_targetPortrait._rootBoneScaleMethod,
															_rootBoneScaleOptionLabel,
															width,
															isDefaultSaved,
															(int)_editor.ProjectSettingData.Common_RootBoneScaleMethod);
			if (iNextRootBoneScaleOption != (int)_targetPortrait._rootBoneScaleMethod)
			{
				apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
													_editor,
													_targetPortrait,
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				_targetPortrait._rootBoneScaleMethod = (apPortrait.ROOT_BONE_SCALE_METHOD)iNextRootBoneScaleOption;

				//모든 본에 대해서 ScaleOption을 적용해야한다.
				_editor.Controller.RefreshBoneScaleMethod_Editor();

				apEditorUtil.SetDirty(_editor);
			}

			DrawDelimeter(width);


			//추가 22.5.15 : 다음 애니메이션에서 컨트롤 파라미터가 지정되지 않은 경우, 이전의 컨트롤 파라미터 값을 어떻게 처리할 것인가
			//"애니메이션 전환시 지정되지 않은 값의 처리"
			EditorGUILayout.LabelField(_editor.GetText(TEXT.Setting_UnspecifiedValueInAnimTransition));
			GUILayout.Space(5);

			apPortrait.UNSPECIFIED_ANIM_CONTROL_PARAM nextUnspecifiedAnimControlParam =
				(apPortrait.UNSPECIFIED_ANIM_CONTROL_PARAM)Layout_EnumPopup(	_editor.GetUIWord(UIWORD.ControlParameter),
																				_targetPortrait._unspecifiedAnimControlParamOption,
																				width,
																				isDefaultSaved,
																				_editor.ProjectSettingData.Common_UnspecifiedAnimControlParam);

			if (nextUnspecifiedAnimControlParam != _targetPortrait._unspecifiedAnimControlParamOption)
			{
				apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
													_editor,
													_targetPortrait,
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				_targetPortrait._unspecifiedAnimControlParamOption = nextUnspecifiedAnimControlParam;

				apEditorUtil.SetDirty(_editor);
			}

			DrawDelimeter(width);


			//추가 22.7.7 : 텔레포트가 발생한 경우
			//"물리 텔레포트 인식"
			bool nextTeleportOption = Layout_Toggle(	_editor.GetText(TEXT.TeleportCalibration),
														_targetPortrait._isTeleportCorrectionOption,
														width,
														isDefaultSaved,
														_editor.ProjectSettingData.Common_TeleportCorrection);

			if (nextTeleportOption != _targetPortrait._isTeleportCorrectionOption)
			{
				apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
													_editor,
													_targetPortrait,
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				_targetPortrait._isTeleportCorrectionOption = nextTeleportOption;

				apEditorUtil.SetDirty(_editor);
			}


			if (_targetPortrait._isTeleportCorrectionOption)
			{
				//변경 v1.5.0 : 텔레포트 옵션이 1개의 세부 옵션에서 3개(위치, 회전, 크기)의 세부 옵션으로 더 나뉘었다.
				

				//1. 위치 (Position)
				bool isGUIChanged_Position = false;
				bool nextTeleport_PositionEnabled = _targetPortrait._teleportPositionEnabled;
				float nextTeleport_MoveDist = _targetPortrait._teleportMovementDist;

				Layout_ToggleAndFloat(	_editor.GetUIWord(UIWORD.Position),
										_targetPortrait._teleportPositionEnabled,
										_targetPortrait._teleportMovementDist,
										width,
										isDefaultSaved,
										_editor.ProjectSettingData.Common_TeleportPositionEnabled,
										_editor.ProjectSettingData.Common_TeleportDist,
										out nextTeleport_PositionEnabled,
										out nextTeleport_MoveDist,
										out isGUIChanged_Position);

				if (isGUIChanged_Position)
				{
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
													_editor,
													_targetPortrait,
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

					_targetPortrait._teleportPositionEnabled = nextTeleport_PositionEnabled;
					_targetPortrait._teleportMovementDist = nextTeleport_MoveDist;

					if (_targetPortrait._teleportMovementDist < 0.1f)
					{
						_targetPortrait._teleportMovementDist = 0.1f;
					}

					apEditorUtil.SetDirty(_editor);
					apEditorUtil.ReleaseGUIFocus();
				}

				//2. 회전 (Rotation)
				bool isGUIChanged_Rotation = false;
				bool nextTeleport_RotationEnabled = _targetPortrait._teleportRotationEnabled;
				float nextTeleport_RotationOffset = _targetPortrait._teleportRotationOffset;

				Layout_ToggleAndFloat(	_editor.GetUIWord(UIWORD.Rotation),
										_targetPortrait._teleportRotationEnabled,
										_targetPortrait._teleportRotationOffset,
										width,
										isDefaultSaved,
										_editor.ProjectSettingData.Common_TeleportRotationEnabled,
										_editor.ProjectSettingData.Common_TeleportRotationOffset,
										out nextTeleport_RotationEnabled,
										out nextTeleport_RotationOffset,
										out isGUIChanged_Rotation);

				if(isGUIChanged_Rotation)
				{
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
													_editor,
													_targetPortrait,
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

					_targetPortrait._teleportRotationEnabled = nextTeleport_RotationEnabled;
					_targetPortrait._teleportRotationOffset = nextTeleport_RotationOffset;

					if (_targetPortrait._teleportRotationOffset < 0.1f)
					{
						_targetPortrait._teleportRotationOffset = 0.1f;
					}

					apEditorUtil.SetDirty(_editor);
					apEditorUtil.ReleaseGUIFocus();
				}

				//3. 크기 (Scale)
				bool isGUIChanged_Scale = false;
				bool nextTeleport_ScaleEnabled = _targetPortrait._teleportScaleEnabled;
				float nextTeleport_ScaleOffset = _targetPortrait._teleportScaleOffset;

				Layout_ToggleAndFloat(	_editor.GetUIWord(UIWORD.Scaling),
										_targetPortrait._teleportScaleEnabled,
										_targetPortrait._teleportScaleOffset,
										width,
										isDefaultSaved,
										_editor.ProjectSettingData.Common_TeleportScaleEnabled,
										_editor.ProjectSettingData.Common_TeleportScaleOffset,
										out nextTeleport_ScaleEnabled,
										out nextTeleport_ScaleOffset,
										out isGUIChanged_Scale);

				if(isGUIChanged_Scale)
				{
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
													_editor,
													_targetPortrait,
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

					_targetPortrait._teleportScaleEnabled = nextTeleport_ScaleEnabled;
					_targetPortrait._teleportScaleOffset = nextTeleport_ScaleOffset;

					if (_targetPortrait._teleportScaleOffset < 0.01f)
					{
						_targetPortrait._teleportScaleOffset = 0.01f;
					}

					apEditorUtil.SetDirty(_editor);
					apEditorUtil.ReleaseGUIFocus();
				}
			}


			DrawDelimeter(width);

			//v1.4.8 : 루트 모션 옵션
			EditorGUILayout.LabelField(_editor.GetText(TEXT.RootMotionOptions));
			GUILayout.Space(5);

			apPortrait.ROOT_MOTION_MODE nextRootMotionMode = (apPortrait.ROOT_MOTION_MODE)Layout_Popup(	_editor.GetText(TEXT.RootMotionMethod), 
																										(int)_targetPortrait._rootMotionModeOption,
																										_rootMotionModeLabel,
																										width);

			if(nextRootMotionMode != _targetPortrait._rootMotionModeOption)
			{
				apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
													_editor,
													_targetPortrait,
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				_targetPortrait._rootMotionModeOption = nextRootMotionMode;

				apEditorUtil.SetDirty(_editor);
				apEditorUtil.ReleaseGUIFocus();
			}
			if(_targetPortrait._rootMotionModeOption != apPortrait.ROOT_MOTION_MODE.None)
			{
				//루트 모션이 유효한 값을 가진다면
				//축별 옵션을 보여준다.
				GUILayout.Space(5);
				EditorGUILayout.LabelField(_editor.GetText(TEXT.RootMotionPerAxis));
				EditorGUI.BeginChangeCheck();

				//X축 옵션
				apPortrait.ROOT_MOTION_MOVE_TYPE_PER_AXIS nextOptX = (apPortrait.ROOT_MOTION_MOVE_TYPE_PER_AXIS)Layout_EnumPopup(	_editor.GetText(TEXT.RootMotionXOption ),
																																	_targetPortrait._rootMotionAxisOption_X,
																																	width);
				//Y축 옵션
				apPortrait.ROOT_MOTION_MOVE_TYPE_PER_AXIS nextOptY = (apPortrait.ROOT_MOTION_MOVE_TYPE_PER_AXIS)Layout_EnumPopup(	_editor.GetText(TEXT.RootMotionYOption ),
																																	_targetPortrait._rootMotionAxisOption_Y,
																																	width);


				if(EditorGUI.EndChangeCheck())
				{
					if(nextOptX != _targetPortrait._rootMotionAxisOption_X
						|| nextOptY != _targetPortrait._rootMotionAxisOption_Y)
					{
						//축별 옵션을 바꾼다.
						apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
														_editor,
														_targetPortrait,
														//null, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

						_targetPortrait._rootMotionAxisOption_X = nextOptX;
						_targetPortrait._rootMotionAxisOption_Y = nextOptY;

						apEditorUtil.SetDirty(_editor);
						apEditorUtil.ReleaseGUIFocus();
					}
				}
			}


			//Move Parent Transform의 경우엔 Parent를 지정해야한다.
			if(_targetPortrait._rootMotionModeOption == apPortrait.ROOT_MOTION_MODE.MoveParentTransform)
			{
				GUILayout.Space(5);

				//루트 모션 옵션이 설정되었다면
				//1. 타겟 Transform을 일반 Parent / 지정된 Transform 중에서 선택할 수 있다.
				EditorGUI.BeginChangeCheck();
				//Parent Transform 종류
				apPortrait.ROOT_MOTION_TARGET_TRANSFORM nextRMTargetType = (apPortrait.ROOT_MOTION_TARGET_TRANSFORM)Layout_EnumPopup(	_editor.GetText(TEXT.RootMotion_ParentTransformType),
																																		_targetPortrait._rootMotionTargetTransformType,
																																		width);
				if(EditorGUI.EndChangeCheck())
				{
					if(nextRMTargetType != _targetPortrait._rootMotionTargetTransformType)
					{
						apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
														_editor,
														_targetPortrait,
														//null, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

						_targetPortrait._rootMotionTargetTransformType = nextRMTargetType;

						apEditorUtil.SetDirty(_editor);
						apEditorUtil.ReleaseGUIFocus();
					}
				}

				if (_targetPortrait._rootMotionTargetTransformType == apPortrait.ROOT_MOTION_TARGET_TRANSFORM.SpecifiedTransform)
				{
					//특정 Transform을 지정
					EditorGUI.BeginChangeCheck();
					Transform nextRootMotionParentTransform = (Transform)Layout_Object<Transform>(	_editor.GetText(TEXT.RootMotion_SpecifiedParentTransform),
																									_targetPortrait._rootMotionSpecifiedParentTransform,
																									true, width);

					if (EditorGUI.EndChangeCheck())
					{
						if (nextRootMotionParentTransform != _targetPortrait._rootMotionSpecifiedParentTransform)
						{
							//유효성을 테스트하자
							if (nextRootMotionParentTransform == null)
							{
								_targetPortrait._rootMotionSpecifiedParentTransform = null;
							}
							else
							{
								if (!_targetPortrait.transform.IsChildOf(nextRootMotionParentTransform))
								{
									//"루트 모션의 기준이 되는 Transform은 부모여야 합니다."
									EditorUtility.DisplayDialog(_editor.GetText(TEXT.DLG_RootMotionTransformFailed_Title),
																_editor.GetText(TEXT.DLG_RootMotionTransformFailed_Body),
																_editor.GetText(TEXT.Okay));
								}
								else
								{
									//유효하다.

									apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
														_editor,
														_targetPortrait,
														//null, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

									_targetPortrait._rootMotionSpecifiedParentTransform = nextRootMotionParentTransform;

									apEditorUtil.SetDirty(_editor);
									apEditorUtil.ReleaseGUIFocus();
								}
							}
						}
					}

					if(Layout_RightButton(_editor.GetText(TEXT.DLG_Change), width))
					{
						_loadKey_RootMotionTranform = apDialog_SelectGameObjectInScene.ShowDialog(	_editor,
																									_targetPortrait,
																									apDialog_SelectGameObjectInScene.REQUEST_GAMEOBJECT.ParentsOfPortrait,
																									OnSelectRootMotionTransform);
					}

				}
				
			}

			
			DrawDelimeter(width);

			//[v1.5.0] IK 업데이트 방식
			apPortrait.IK_METHOD nextIKMethod = (apPortrait.IK_METHOD)Layout_Popup(	_editor.GetText(TEXT.IKMethod),
																					(int)_targetPortrait._IKMethod, _IKMethodNames,
																					width,
																					isDefaultSaved,
																					(int)_editor.ProjectSettingData.Common_IKMethod
																					);
			if(nextIKMethod != _targetPortrait._IKMethod)
			{
				apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
												_editor,
												_targetPortrait,
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

				_targetPortrait._IKMethod = nextIKMethod;

				apEditorUtil.SetDirty(_editor);
				apEditorUtil.ReleaseGUIFocus();
			}



			//유효성을 테스트해서 메시지로 남긴다.

			DrawDelimeter(width);

			//현재 설정을 파일로 내보내기 [v1.4.2]
			if(GUILayout.Button(_editor.GetText(TEXT.DLG_Bake_SaveSettingsAsDefault), GUILayout.Height(20)))
			{
				apProjectSettingData.SAVE_RESULT saveResult = _editor.ProjectSettingData.SetPortraitCommonSettings(_targetPortrait);

				if(saveResult == apProjectSettingData.SAVE_RESULT.FileSaved)
				{
					//파일이 저장되었다.
					EditorUtility.DisplayDialog(	_editor.GetText(TEXT.DLG_Bake_SaveSettingsSuccess_Title),
													_editor.GetText(TEXT.DLG_Bake_SaveSettingsSuccess_Body),
													_editor.GetText(TEXT.Okay));
				}
				else if(saveResult == apProjectSettingData.SAVE_RESULT.Failed)
				{
					//파일 저장에 실패했다.
					EditorUtility.DisplayDialog(	_editor.GetText(TEXT.DLG_Bake_SaveSettingsFailed_Title),
													_editor.GetText(TEXT.DLG_Bake_SaveSettingsFailed_Body),
													_editor.GetText(TEXT.Okay));
				}
			}

			//내보낸 설정이 있다면 메시지/삭제 버튼을 보여주자
			if(_editor.ProjectSettingData.IsCommonSettingSaved)
			{
				GUI.backgroundColor = new Color(	GUI.backgroundColor.r * 0.8f,
													GUI.backgroundColor.g * 1.2f,
													GUI.backgroundColor.b * 1.5f,
													1.0f);

				GUILayout.Box(_editor.GetText(TEXT.DLG_Bake_SettingSavedMessage), _guiStyle_BoxMessage, GUILayout.Width(width), GUILayout.Height(30));

				GUI.backgroundColor = prevColor;

				//기본 설정 삭제하기
				if(GUILayout.Button(_editor.GetText(TEXT.DLG_Bake_RemoveDefaultSettings), GUILayout.Height(20)))
				{
					int iBtn = EditorUtility.DisplayDialogComplex(	_editor.GetText(TEXT.DLG_Bake_RemoveSettings_Title),
																	_editor.GetText(TEXT.DLG_Bake_RemoveSettings_Body),
																	_editor.GetText(TEXT.DLG_Bake_RemoveSettings_Btn_OnlyFileInfo),
																	_editor.GetText(TEXT.DLG_Bake_RemoveSettings_Btn_RemoveAll),
																	_editor.GetText(TEXT.DLG_Cancel));

					if (iBtn == 0)
					{
						//파일 기록만 삭제
						_editor.ProjectSettingData.ClearCommonSettingsAndSave();
					}
					else if (iBtn == 1)
					{
						//파일 기록 삭제 후 초기화
						_editor.ProjectSettingData.ClearCommonSettingsAndSave();

						apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
													_editor,
													_targetPortrait,
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

						_editor.ProjectSettingData.ResetPortraitBakeSettings(_targetPortrait);
					}
				}
			}



			GUILayout.Space(height + 500);
			EditorGUILayout.EndVertical();

			EditorGUILayout.EndScrollView();
		}





		// Functions
		//------------------------------------------------------------------------------
		private void MakeAmbientLightToBlack()
		{	
			RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
			RenderSettings.ambientLight = Color.black;
			apEditorUtil.SetAllSceneDirty();
		}


		private void CheckAmbientAndCorrection()
		{
			if(_editor == null)
			{
				return;
			}
			if(!_editor._isAmbientCorrectionOption)
			{
				//Ambient 보정 옵션이 False이면 처리를 안함
				return;
			}

			//현재 Ambient 색상과 모드를 확인하자
			UnityEngine.Rendering.AmbientMode ambientMode = RenderSettings.ambientMode;
			Color ambientColor = RenderSettings.ambientLight;
			if(ambientMode == UnityEngine.Rendering.AmbientMode.Flat &&
				ambientColor.r <= 0.001f &&
				ambientColor.g <= 0.001f &&
				ambientColor.b <= 0.001f)
			{
				//Ambient가 검은색이다.
				return;
			}
			//이전
			////Ambient 색상을 바꿀 것인지 물어보자
#region [미사용 코드]
			//int iBtn = EditorUtility.DisplayDialogComplex(
			//										_editor.GetText(TEXT.DLG_AmbientColorCorrection_Title),
			//										_editor.GetText(TEXT.DLG_AmbientColorCorrection_Body),
			//										_editor.GetText(TEXT.Okay),
			//										_editor.GetText(TEXT.DLG_AmbientColorCorrection_Ignore),
			//										_editor.GetText(TEXT.Cancel)
			//										);

			//if(iBtn == 0)
			//{
			//	//색상을 바꾸자
			//	MakeAmbientLightToBlack();
			//}
			//else if(iBtn == 1)
			//{
			//	//무시하자
			//	_editor._isAmbientCorrectionOption = false;
			//	_editor.SaveEditorPref();
			//} 
#endregion

			//조건문 추가 19.6.22 : 현재의 기본 Material Set이 Ambient Color 색상이 검은색을 필요로할 경우
			if(_targetPortrait != null)
			{
				apMaterialSet defaultMatSet = _targetPortrait.GetDefaultMaterialSet();
				if(defaultMatSet != null)
				{
					if(!defaultMatSet._isNeedToSetBlackColoredAmbient)
					{
						//현재의 Default MatSet이 검은색을 필요로 하지 않는 경우
						return;
					}
				}
			}
			//이후 : 별도의 다이얼로그 표시
			apDialog_AmbientCorrection.ShowDialog(_editor, (int)position.x, (int)position.y);
		}

		


		/// <summary>
		/// 추가 22.1.6 : Bake후에 URP 설정을 사용하고 있고 클리핑 메시를 사용하고 있었다면 안내 메시지를 보여주자
		/// </summary>
		private void CheckURP2022SettingsIfAnyClippedMeshes()
		{
			if(_editor == null || _targetPortrait == null)
			{
				return;
			}
			if(!_editor._isShowURPWarningMsg)
			{
				//메시지 안보기로 했다면
				return;
			}

			//유니티 2022부터 체크
			if(!apEditorUtil.IsUseURPRenderPipeline2022())
			{
				//URP를 사용하지 않는다면 당연히 그냥 넘어감
				return;
			}

			//Bake된 메시들 중에서
			bool isAnyClippedMeshes = false;
			int nOptMeshes = _targetPortrait._optMeshes != null ? _targetPortrait._optMeshes.Count : 0;
			apOptMesh optMesh = null;
			for (int i = 0; i < nOptMeshes; i++)
			{
				optMesh = _targetPortrait._optMeshes[i];
				if(optMesh._isMaskChild)
				{
					isAnyClippedMeshes = true;
					break;
				}
			}

			if(!isAnyClippedMeshes)
			{
				//클리핑 메시가 없다면
				return;
			}

			//다이얼로그 표시
			apDialog_URPSettingNotice.ShowDialog(_editor, (int)position.x, (int)position.y);
		}

		/// <summary>
		/// SRP 옵션이 제대로 설정되었는지 물어보고 설정한다.
		/// </summary>
		private void CheckSRPOption()
		{
			if (_editor == null || _targetPortrait == null)
			{
				return;
			}

			if (!_editor._option_CheckSRPWhenBake)
			{
				//확인하지 않기로 했다면
				return;
			}

			//이 옵션은 2020_1부터 체크한다. 그 전에는 URP를 정상적으로 확인하기가 어렵다.
//#if UNITY_2020_1_OR_NEWER

			//옵션을 체크한다.
			apEditorUtil.RENDER_PIPELINE_ENV_RESULT renderPipelineResult = apEditorUtil.CheckUseURPRenderPipeline();
			
			if(renderPipelineResult == apEditorUtil.RENDER_PIPELINE_ENV_RESULT.Unknown)
			{
				//모르는 거면 패스
				return;
			}


			

			//서로 다르다.
			//Default를 URP로 바꿔야 하는 경우
			//bool isCurURPUsed = _editor._isUseSRP;//이전
			bool isCurURPUsed = _editor.ProjectSettingData.Project_IsUseSRP;//변경 v1.4.2

			if(!isCurURPUsed 
				&& renderPipelineResult == apEditorUtil.RENDER_PIPELINE_ENV_RESULT.URP)
			{
				//Default > SRP 물어보기
				int iBtn = EditorUtility.DisplayDialogComplex(
										_editor.GetText(TEXT.RenderPipelineOptionUnmatch_Title),
										_editor.GetText(TEXT.RenderPipelineOptionUnmatch_ToURP_Body),
										_editor.GetText(TEXT.ChangeNow),//변경										
										_editor.GetText(TEXT.Ignore),//무시하기
										_editor.GetText(TEXT.DLG_DoNotShowThisMessage)//메시지 숨기기
										);

				if(iBtn == 0)
				{
					//변경한다. (-> SRP)

					//이전
					//_editor._isUseSRP = true;
					//_editor.SaveEditorPref();

					//변경 v1.4.2
					_editor.ProjectSettingData.SetUseSRP(true);
				}
				else if(iBtn == 2)
				{
					//(옵션 끈다)
					_editor._option_CheckSRPWhenBake = false;
					_editor.SaveEditorPref();
				}
			}
			else if(isCurURPUsed && renderPipelineResult == apEditorUtil.RENDER_PIPELINE_ENV_RESULT.BuiltIn)
			{
				//URP를 Default로 바꿔야 하는 경우
				//SRP > Default 물어보기
				int iBtn = EditorUtility.DisplayDialogComplex(
										_editor.GetText(TEXT.RenderPipelineOptionUnmatch_Title),
										_editor.GetText(TEXT.RenderPipelineOptionUnmatch_ToDefault_Body),
										_editor.GetText(TEXT.ChangeNow),										
										_editor.GetText(TEXT.Ignore),
										_editor.GetText(TEXT.DLG_DoNotShowThisMessage)
										);

				if(iBtn == 0)
				{
					//변경한다. (-> Default)
					
					//이전
					//_editor._isUseSRP = false;
					//_editor.SaveEditorPref();

					//변경 v1.4.2
					_editor.ProjectSettingData.SetUseSRP(false);
				}
				else if(iBtn == 2)
				{
					//옵션 끈다
					_editor._option_CheckSRPWhenBake = false;
					_editor.SaveEditorPref();
				}
			}
//#endif
			
			
		}

		private void DrawDelimeter(int width)
		{
			GUILayout.Space(15);

			apEditorUtil.GUI_DelimeterBoxH(width);

			GUILayout.Space(15);
		}


		private void DrawCategoryTitle(string strTitle, int width, int colorIndex)
		{

			Color prevColor = GUI.backgroundColor;

			//파란색/녹색 계열 (진한색)
			if (EditorGUIUtility.isProSkin)
			{
				if(colorIndex == 0)
				{
					GUI.backgroundColor = new Color(0.3f, 0.5f, 0.8f, 1.0f);
				}
				else
				{
					GUI.backgroundColor = new Color(0.3f, 0.8f, 0.5f, 1.0f);
				}
				
			}
			else
			{
				if(colorIndex == 0)
				{
					GUI.backgroundColor = new Color(0.2f, 0.4f, 0.7f, 1.0f);
				}
				else
				{
					GUI.backgroundColor = new Color(0.2f, 0.7f, 0.4f, 1.0f);
				}
				
			}

			//GUILayout.Space(10);
			GUILayout.Box(strTitle, _guiStyle_Category, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(20));			
			GUILayout.Space(10);

			GUI.backgroundColor = prevColor;
		}

		private bool DrawSyncButton(int width)
		{
			GUILayout.Space(4);

			GUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(20));
			GUILayout.Space(width - 32);

			bool isBtn = GUILayout.Button(_editor.ImageSet.Get(apImageSet.PRESET.SyncSettingToFile16px), _guiStyle_SyncBtn, GUILayout.Width(32), GUILayout.Height(16));

			GUILayout.EndHorizontal();

			return isBtn;
		}



		private int FindSortingLayerIndex(int targetLayerIndex)
		{	
			int nLayers = SortingLayer.layers != null ? SortingLayer.layers.Length : 0;
			if(nLayers == 0)
			{
				return -1;
			}
			
			for (int i = 0; i < SortingLayer.layers.Length; i++)
			{
				SortingLayer sortingLayer = SortingLayer.layers[i];
				if (sortingLayer.id == targetLayerIndex)
				{
					//찾았다.
					return i;
				}
			}

			//못찾았당.. ㅜㅜ
			return -1;
		}


		private object _loadKey_RootMotionTranform = null;
		private void OnSelectRootMotionTransform(bool isSuccess, object loadKey, Transform resultTransform)
		{
			if(!isSuccess
				|| _loadKey_RootMotionTranform != loadKey
				|| _targetPortrait == null)
			{
				_loadKey_RootMotionTranform = null;
				return;
			}
			_loadKey_RootMotionTranform = null;

			apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Portrait_SettingChanged,
												_editor,
												_targetPortrait,
												//null, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

			_targetPortrait._rootMotionSpecifiedParentTransform = resultTransform;
			apEditorUtil.SetDirty(_editor);
			apEditorUtil.ReleaseGUIFocus();

			Repaint();
			Focus();
		}



		// UI 코드 래핑
		//------------------------------------------------------------------
		private const int LEFT_MARGIN = 5;
		//private const int LABEL_WIDTH = 175;//이거 동적으로 만들 수 있나
		private int LABEL_WIDTH { get { return _layoutLeftLabelWidth; } }
		
		private const int LAYOUT_HEIGHT = 22;
		private const int SYNC_BTN_WIDTH = 18;
		private const int SYNC_BTN_HEIGHT = 18;

		private int Layout_Popup(string strLabel, int curPopupIndex, string[] names, int width)
		{
			int valueWidth = width - (LEFT_MARGIN * 2 + LABEL_WIDTH);

			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT), GUILayout.Width(width));
			GUILayout.Space(LEFT_MARGIN);
			EditorGUILayout.LabelField(strLabel, _guiStyle_Label_Default, GUILayout.Width(LABEL_WIDTH));
			int result = EditorGUILayout.Popup(curPopupIndex, names, GUILayout.Width(valueWidth));
			EditorGUILayout.EndHorizontal();

			return result;
		}

		private int Layout_Popup(GUIContent guiLabel, int curPopupIndex, string[] names, int width)
		{
			int valueWidth = width - (LEFT_MARGIN * 2 + LABEL_WIDTH);

			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT), GUILayout.Width(width));
			GUILayout.Space(LEFT_MARGIN);
			EditorGUILayout.LabelField(guiLabel, _guiStyle_Label_Default, GUILayout.Width(LABEL_WIDTH));
			int result = EditorGUILayout.Popup(curPopupIndex, names, GUILayout.Width(valueWidth));
			EditorGUILayout.EndHorizontal();

			return result;
		}


		//파일로 저장된 별도의 기본값이 있는 경우
		private int Layout_Popup(string strLabel, int curPopupIndex, string[] names, int width, bool isDefaultSaved, int defaultIndex)
		{
			if (!isDefaultSaved)
			{
				return Layout_Popup(strLabel, curPopupIndex, names, width);
			}
			
			int valueWidth = width - (LEFT_MARGIN * 2 + LABEL_WIDTH);
			bool isShowSyncBtn = false;
			if(isDefaultSaved && curPopupIndex != defaultIndex)
			{
				isShowSyncBtn = true;
				valueWidth -= SYNC_BTN_WIDTH + 3;
			}

			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT), GUILayout.Width(width));
			GUILayout.Space(LEFT_MARGIN);
			EditorGUILayout.LabelField(	strLabel, 
										(defaultIndex == curPopupIndex) ? _guiStyle_Label_Default : _guiStyle_Label_Changed,
										GUILayout.Width(LABEL_WIDTH));
			
			int result = EditorGUILayout.Popup(curPopupIndex, names, GUILayout.Width(valueWidth));

			if(isShowSyncBtn)
			{
				if(GUILayout.Button(_editor.ImageSet.Get(apImageSet.PRESET.SyncSettingToFile12px), _guiStyle_SyncBtn, GUILayout.Width(SYNC_BTN_WIDTH), GUILayout.Height(SYNC_BTN_HEIGHT)))
				{
					result = defaultIndex;
				}
			}

			EditorGUILayout.EndHorizontal();

			return result;
		}


		private int Layout_Popup(GUIContent guiLabel, int curPopupIndex, string[] names, int width, bool isDefaultSaved, int defaultIndex)
		{
			if (!isDefaultSaved)
			{
				return Layout_Popup(guiLabel, curPopupIndex, names, width);
			}
			
			int valueWidth = width - (LEFT_MARGIN * 2 + LABEL_WIDTH);
			bool isShowSyncBtn = false;
			if(isDefaultSaved && curPopupIndex != defaultIndex)
			{
				isShowSyncBtn = true;
				valueWidth -= SYNC_BTN_WIDTH + 3;
			}

			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT), GUILayout.Width(width));
			GUILayout.Space(LEFT_MARGIN);
			EditorGUILayout.LabelField(	guiLabel, 
										(defaultIndex == curPopupIndex) ? _guiStyle_Label_Default : _guiStyle_Label_Changed,
										GUILayout.Width(LABEL_WIDTH));
			
			int result = EditorGUILayout.Popup(curPopupIndex, names, GUILayout.Width(valueWidth));

			if(isShowSyncBtn)
			{
				if(GUILayout.Button(_editor.ImageSet.Get(apImageSet.PRESET.SyncSettingToFile12px), _guiStyle_SyncBtn, GUILayout.Width(SYNC_BTN_WIDTH), GUILayout.Height(SYNC_BTN_HEIGHT)))
				{
					result = defaultIndex;
				}
			}

			EditorGUILayout.EndHorizontal();

			return result;
		}



		private Enum Layout_EnumPopup(string strLabel, Enum curEnumValue, int width)
		{
			int valueWidth = width - (LEFT_MARGIN * 2 + LABEL_WIDTH);

			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT), GUILayout.Width(width));
			GUILayout.Space(LEFT_MARGIN);
			EditorGUILayout.LabelField(strLabel, _guiStyle_Label_Default, GUILayout.Width(LABEL_WIDTH));
			Enum result = EditorGUILayout.EnumPopup(curEnumValue, GUILayout.Width(valueWidth));
			EditorGUILayout.EndHorizontal();

			return result;
		}

		private Enum Layout_EnumPopup(GUIContent guiLabel, Enum curEnumValue, int width)
		{
			int valueWidth = width - (LEFT_MARGIN * 2 + LABEL_WIDTH);

			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT), GUILayout.Width(width));
			GUILayout.Space(LEFT_MARGIN);
			EditorGUILayout.LabelField(guiLabel, _guiStyle_Label_Default, GUILayout.Width(LABEL_WIDTH));
			Enum result = EditorGUILayout.EnumPopup(curEnumValue, GUILayout.Width(valueWidth));
			EditorGUILayout.EndHorizontal();

			return result;
		}

		private Enum Layout_EnumPopup(string strLabel, Enum curEnumValue, int width, bool isDefaultSaved, Enum defaultValue)
		{
			if(!isDefaultSaved)
			{
				return Layout_EnumPopup(strLabel, curEnumValue, width);
			}

			int valueWidth = width - (LEFT_MARGIN * 2 + LABEL_WIDTH);
			bool isShowSyncBtn = false;
			if(isDefaultSaved && !Enum.Equals(defaultValue, curEnumValue))
			{
				isShowSyncBtn = true;
				valueWidth -= SYNC_BTN_WIDTH + 3;
			}


			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT), GUILayout.Width(width));
			GUILayout.Space(LEFT_MARGIN);

			EditorGUILayout.LabelField(	strLabel,
										Enum.Equals(defaultValue, curEnumValue) ? _guiStyle_Label_Default : _guiStyle_Label_Changed,
										GUILayout.Width(LABEL_WIDTH));

			Enum result = EditorGUILayout.EnumPopup(curEnumValue, GUILayout.Width(valueWidth));

			if(isShowSyncBtn)
			{
				if(GUILayout.Button(_editor.ImageSet.Get(apImageSet.PRESET.SyncSettingToFile12px), _guiStyle_SyncBtn, GUILayout.Width(SYNC_BTN_WIDTH), GUILayout.Height(SYNC_BTN_HEIGHT)))
				{
					result = defaultValue;
				}
			}

			EditorGUILayout.EndHorizontal();

			return result;
		}

		private Enum Layout_EnumPopup(GUIContent guiLabel, Enum curEnumValue, int width, bool isDefaultSaved, Enum defaultValue)
		{
			if(!isDefaultSaved)
			{
				return Layout_EnumPopup(guiLabel, curEnumValue, width);
			}

			int valueWidth = width - (LEFT_MARGIN * 2 + LABEL_WIDTH);
			bool isShowSyncBtn = false;
			if(isDefaultSaved && !Enum.Equals(defaultValue, curEnumValue))
			{
				isShowSyncBtn = true;
				valueWidth -= SYNC_BTN_WIDTH + 3;
			}


			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT), GUILayout.Width(width));
			GUILayout.Space(LEFT_MARGIN);

			EditorGUILayout.LabelField(	guiLabel,
										Enum.Equals(defaultValue, curEnumValue) ? _guiStyle_Label_Default : _guiStyle_Label_Changed,
										GUILayout.Width(LABEL_WIDTH));

			Enum result = EditorGUILayout.EnumPopup(curEnumValue, GUILayout.Width(valueWidth));

			if(isShowSyncBtn)
			{
				if(GUILayout.Button(_editor.ImageSet.Get(apImageSet.PRESET.SyncSettingToFile12px), _guiStyle_SyncBtn, GUILayout.Width(SYNC_BTN_WIDTH), GUILayout.Height(SYNC_BTN_HEIGHT)))
				{
					result = defaultValue;
				}
			}

			EditorGUILayout.EndHorizontal();

			return result;
		}



		private int Layout_Int(	string strLabel,
								int curValue,
								int width,
								bool isDefaultSaved,
								int defaultValue)
		{
			int valueWidth = width - (LEFT_MARGIN * 2 + LABEL_WIDTH);
			bool isShowSyncBtn = false;
			if(isDefaultSaved && curValue != defaultValue)
			{
				isShowSyncBtn = true;
				valueWidth -= SYNC_BTN_WIDTH + 3;
			}

			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT), GUILayout.Width(width));
			GUILayout.Space(LEFT_MARGIN);
			if(isDefaultSaved)
			{
				EditorGUILayout.LabelField(	strLabel,
											(curValue == defaultValue) ? _guiStyle_Label_Default : _guiStyle_Label_Changed,
											GUILayout.Width(LABEL_WIDTH));
			}
			else
			{
				EditorGUILayout.LabelField(	strLabel,
											_guiStyle_Label_Default,
											GUILayout.Width(LABEL_WIDTH));
			}
			
			int result = EditorGUILayout.IntField(curValue, GUILayout.Width(valueWidth));

			if(isShowSyncBtn)
			{
				if(GUILayout.Button(_editor.ImageSet.Get(apImageSet.PRESET.SyncSettingToFile12px), _guiStyle_SyncBtn, GUILayout.Width(SYNC_BTN_WIDTH), GUILayout.Height(SYNC_BTN_HEIGHT)))
				{
					result = defaultValue;
					apEditorUtil.ReleaseGUIFocus();
				}
			}

			EditorGUILayout.EndHorizontal();

			return result;
		}


		private int Layout_DelayedInt(	GUIContent labelContent,
										int curValue,
										int width,
										bool isDefaultSaved,
										int defaultValue)
		{
			int valueWidth = width - (LEFT_MARGIN * 2 + LABEL_WIDTH);
			bool isShowSyncBtn = false;
			if(isDefaultSaved && curValue != defaultValue)
			{
				isShowSyncBtn = true;
				valueWidth -= SYNC_BTN_WIDTH + 3;
			}


			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT), GUILayout.Width(width));
			GUILayout.Space(LEFT_MARGIN);
			if(isDefaultSaved)
			{
				EditorGUILayout.LabelField(	labelContent,
											(curValue == defaultValue) ? _guiStyle_Label_Default : _guiStyle_Label_Changed,
											GUILayout.Width(LABEL_WIDTH));
			}
			else
			{
				EditorGUILayout.LabelField(	labelContent,
											_guiStyle_Label_Default,
											GUILayout.Width(LABEL_WIDTH));
			}
			
			int result = EditorGUILayout.DelayedIntField(curValue, GUILayout.Width(valueWidth));

			if(isShowSyncBtn)
			{
				if(GUILayout.Button(_editor.ImageSet.Get(apImageSet.PRESET.SyncSettingToFile12px), _guiStyle_SyncBtn, GUILayout.Width(SYNC_BTN_WIDTH), GUILayout.Height(SYNC_BTN_HEIGHT)))
				{
					result = defaultValue;
					apEditorUtil.ReleaseGUIFocus();
				}
			}

			EditorGUILayout.EndHorizontal();

			return result;
		}


		private float Layout_DelayedFloat(	string strLabel,
											float curValue,
											int width,
											bool isDefaultSaved,
											float defaultValue,
											out bool isGUIChanged)
		{
			float bias = 0.001f;

			int valueWidth = width - (LEFT_MARGIN * 2 + LABEL_WIDTH);
			bool isShowSyncBtn = false;
			if(isDefaultSaved && Mathf.Abs(curValue - defaultValue) > bias)
			{
				isShowSyncBtn = true;
				valueWidth -= SYNC_BTN_WIDTH + 3;
			}

			isGUIChanged = false;

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT), GUILayout.Width(width));
			GUILayout.Space(LEFT_MARGIN);
			if(isDefaultSaved)
			{
				EditorGUILayout.LabelField(	strLabel,
											(Mathf.Abs(curValue - defaultValue) < bias) ? _guiStyle_Label_Default : _guiStyle_Label_Changed,
											GUILayout.Width(LABEL_WIDTH));
			}
			else
			{
				EditorGUILayout.LabelField(	strLabel,
											_guiStyle_Label_Default,
											GUILayout.Width(LABEL_WIDTH));
			}
			
			float result = EditorGUILayout.DelayedFloatField(curValue, GUILayout.Width(valueWidth));

			isGUIChanged = EditorGUI.EndChangeCheck();

			if(isShowSyncBtn)
			{
				if(GUILayout.Button(_editor.ImageSet.Get(apImageSet.PRESET.SyncSettingToFile12px), _guiStyle_SyncBtn, GUILayout.Width(SYNC_BTN_WIDTH), GUILayout.Height(SYNC_BTN_HEIGHT)))
				{
					result = defaultValue;
					isGUIChanged = true;
					apEditorUtil.ReleaseGUIFocus();
				}
			}

			EditorGUILayout.EndHorizontal();

			return result;
		}



		private bool Layout_Toggle(	string strLabel,
									bool curValue,
									int width,
									bool isDefaultSaved,
									bool defaultValue)
		{
			int valueWidth = width - (LEFT_MARGIN * 2 + LABEL_WIDTH);
			bool isShowSyncBtn = false;
			if(isDefaultSaved && curValue != defaultValue)
			{
				isShowSyncBtn = true;
				valueWidth -= SYNC_BTN_WIDTH + 3;
			}


			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT), GUILayout.Width(width));
			GUILayout.Space(LEFT_MARGIN);
			if(isDefaultSaved)
			{
				EditorGUILayout.LabelField(	strLabel,
											(curValue == defaultValue) ? _guiStyle_Label_Default : _guiStyle_Label_Changed,
											GUILayout.Width(LABEL_WIDTH));
			}
			else
			{
				EditorGUILayout.LabelField(	strLabel,
											_guiStyle_Label_Default,
											GUILayout.Width(LABEL_WIDTH));
			}
			
			bool result = EditorGUILayout.Toggle(curValue, GUILayout.Width(valueWidth));

			if(isShowSyncBtn)
			{
				if(GUILayout.Button(_editor.ImageSet.Get(apImageSet.PRESET.SyncSettingToFile12px), _guiStyle_SyncBtn, GUILayout.Width(SYNC_BTN_WIDTH), GUILayout.Height(SYNC_BTN_HEIGHT)))
				{
					result = defaultValue;
				}
			}

			EditorGUILayout.EndHorizontal();

			return result;
		}


		private bool Layout_Toggle(	GUIContent labelContent,
									bool curValue,
									int width,
									bool isDefaultSaved,
									bool defaultValue)
		{
			int valueWidth = width - (LEFT_MARGIN * 2 + LABEL_WIDTH);
			bool isShowSyncBtn = false;
			if(isDefaultSaved && curValue != defaultValue)
			{
				isShowSyncBtn = true;
				valueWidth -= SYNC_BTN_WIDTH + 3;
			}


			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT), GUILayout.Width(width));
			GUILayout.Space(LEFT_MARGIN);
			if(isDefaultSaved)
			{
				EditorGUILayout.LabelField(	labelContent,
											(curValue == defaultValue) ? _guiStyle_Label_Default : _guiStyle_Label_Changed,
											GUILayout.Width(LABEL_WIDTH));
			}
			else
			{
				EditorGUILayout.LabelField(	labelContent,
											_guiStyle_Label_Default,
											GUILayout.Width(LABEL_WIDTH));
			}
			
			bool result = EditorGUILayout.Toggle(curValue, GUILayout.Width(valueWidth));

			if(isShowSyncBtn)
			{
				if(GUILayout.Button(_editor.ImageSet.Get(apImageSet.PRESET.SyncSettingToFile12px), _guiStyle_SyncBtn, GUILayout.Width(SYNC_BTN_WIDTH), GUILayout.Height(SYNC_BTN_HEIGHT)))
				{
					result = defaultValue;
				}
			}


			EditorGUILayout.EndHorizontal();

			return result;
		}


		private bool Layout_Toggle_WrapLabel(	string strLabel,
												bool curValue,
												int width,
												bool isDefaultSaved,
												bool defaultValue)
		{
			int valueWidth = width - (LEFT_MARGIN * 2 + LABEL_WIDTH);
			bool isShowSyncBtn = false;
			if(isDefaultSaved && curValue != defaultValue)
			{
				isShowSyncBtn = true;
				valueWidth -= SYNC_BTN_WIDTH + 3;
			}


			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT), GUILayout.Width(width));
			GUILayout.Space(LEFT_MARGIN);
			if(isDefaultSaved)
			{	
				EditorGUILayout.LabelField(	strLabel,
											(curValue == defaultValue) ? _guiStyle_LabelWrapText : _guiStyle_LabelWrapText_Changed,
											GUILayout.Width(LABEL_WIDTH));
			}
			else
			{
				EditorGUILayout.LabelField(	strLabel,
											_guiStyle_LabelWrapText,
											GUILayout.Width(LABEL_WIDTH));
			}
			
			bool result = EditorGUILayout.Toggle(curValue, GUILayout.Width(valueWidth));

			if(isShowSyncBtn)
			{
				if(GUILayout.Button(_editor.ImageSet.Get(apImageSet.PRESET.SyncSettingToFile12px), _guiStyle_SyncBtn, GUILayout.Width(SYNC_BTN_WIDTH), GUILayout.Height(SYNC_BTN_HEIGHT)))
				{
					result = defaultValue;
				}
			}


			EditorGUILayout.EndHorizontal();

			return result;
		}



		private void Layout_ToggleAndFloat(	string strLabel,
											bool curValue_Bool,
											float curValue_Float,
											int width,
											bool isDefaultSaved,
											bool defaultValue_Bool,
											float defaultValue_Float,
											out bool resultValue_Bool,
											out float resultValue_Float,
											out bool isGUIChanged)
		{
			resultValue_Bool = curValue_Bool;
			resultValue_Float = curValue_Float;

			int valueWidth_Toggle = 25;
			int valueWidth_Float = width - (LEFT_MARGIN * 2 + LABEL_WIDTH + valueWidth_Toggle + 4);
			bool isShowSyncBtn = false;

			//기본값과 같은지 확인
			bool isSameDefault = false;
			if(curValue_Bool == defaultValue_Bool
				&& Mathf.Abs(curValue_Float - defaultValue_Float) < 0.001f)
			{
				isSameDefault = true;
			}
			if(isDefaultSaved && !isSameDefault)
			{
				//저장된 기본값과 다른 상태
				isShowSyncBtn = true;
				valueWidth_Float -= SYNC_BTN_WIDTH + 3;
			}

			isGUIChanged = false;

			

			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT), GUILayout.Width(width));
			GUILayout.Space(LEFT_MARGIN);
			if(isDefaultSaved)
			{
				EditorGUILayout.LabelField(	strLabel,
											isSameDefault ? _guiStyle_Label_Default : _guiStyle_Label_Changed,
											GUILayout.Width(LABEL_WIDTH));
			}
			else
			{
				EditorGUILayout.LabelField(	strLabel,
											_guiStyle_Label_Default,
											GUILayout.Width(LABEL_WIDTH));
			}
			
			//1. 토글 옵션
			EditorGUI.BeginChangeCheck();
			resultValue_Bool = EditorGUILayout.Toggle(curValue_Bool, GUILayout.Width(valueWidth_Toggle));

			//2. Float 옵션
			resultValue_Float = EditorGUILayout.DelayedFloatField(curValue_Float, GUILayout.Width(valueWidth_Float));
			if(EditorGUI.EndChangeCheck() || resultValue_Bool != curValue_Bool)
			{
				
				isGUIChanged = true;
			}

			if(isShowSyncBtn)
			{
				if(GUILayout.Button(_editor.ImageSet.Get(apImageSet.PRESET.SyncSettingToFile12px), _guiStyle_SyncBtn, GUILayout.Width(SYNC_BTN_WIDTH), GUILayout.Height(SYNC_BTN_HEIGHT)))
				{
					resultValue_Bool = defaultValue_Bool;
					resultValue_Float = defaultValue_Float;
					isGUIChanged = true;
					apEditorUtil.ReleaseGUIFocus();
				}
			}

			EditorGUILayout.EndHorizontal();
		}

		private UnityEngine.Object Layout_Object<T>(	string strLabel,
														UnityEngine.Object curValue,
														bool isInScene,
														int width
														)
		{
			int valueWidth = width - (LEFT_MARGIN * 2 + LABEL_WIDTH);
			

			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT), GUILayout.Width(width));
			GUILayout.Space(LEFT_MARGIN);
			EditorGUILayout.LabelField(	strLabel,
										_guiStyle_Label_Default,
										GUILayout.Width(LABEL_WIDTH));
			
			UnityEngine.Object result = EditorGUILayout.ObjectField(curValue, typeof(T), isInScene, GUILayout.Width(valueWidth));

			EditorGUILayout.EndHorizontal();

			return result;
		}

		private bool Layout_RightButton(string strLabel, int width)
		{
			int valueWidth = width - (LEFT_MARGIN * 2 + LABEL_WIDTH);
			
			bool isClick = false;
			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT), GUILayout.Width(width));
			GUILayout.Space(LEFT_MARGIN + LABEL_WIDTH + LEFT_MARGIN);
			if(GUILayout.Button(strLabel, GUILayout.Width(valueWidth)))
			{
				isClick = true;
			}
			EditorGUILayout.EndHorizontal();

			return isClick;
		}

	}

}
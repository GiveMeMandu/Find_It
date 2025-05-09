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
using System.Collections;
using System.Collections.Generic;
using System;

using AnyPortrait;

namespace AnyPortrait
{

	/// <summary>
	/// Modifier에서 ParamSet 중 비슷한 것끼리 묶어서 처리하는 클래스
	/// Editor에서 - 작업용 기능을 제공하고, UI의 편의성을 높이는 기능을 제공한다.
	/// Realtime에서 - matrix 계산시 도와주도록 한다.
	/// Serailize는 안되는 참조용 클래스 > 수정 : Serialize가 되며 레이어 값을 가진다.
	/// </summary>
	[Serializable]
	public class apModifierParamSetGroup
	{
		// Members
		//-------------------------------------------------
		[NonSerialized]
		public apPortrait _portrait = null;

		[NonSerialized]
		public apModifierBase _parentModifier = null;

		//어떤 값에 의해서 영향을 받는가
		public enum SYNC_TARGET
		{
			/// <summary>고정값. 세팅한 값이 그대로 적용된다. Input이 없어서 1개의 ParamSetGroup만 사용한다. (Rigging 등)</summary>
			Static = 0,
			/// <summary>컨트롤러에 의해 적용 수준이 결정된다. 복수개의 ParamSet이 있어야 보간이 된다.</summary>
			Controller = 1,
			/// <summary>키프레임에 의해 적용 수준이 결정된다. 이 값은 모션 데이터의 KeyFrame에 저장되며, Modifier와 연결된다.</summary>
			KeyFrame = 2,
			/// <summary>컨트롤러에 의해 적용 수준이 결정되지만 값은 Static하게 고정되며 자동으로 보간이 이루어진다. [키 추가 불가]</summary>
			ControllerWithoutKey = 3,
			/// <summary>본에 연동된다. TF류 모디파이어와 유사하게 작동됨</summary>
			Bones = 4,
		}

		//Sync 값은 하위의 모든 ParamSet과 동일하다.
		public SYNC_TARGET _syncTarget = SYNC_TARGET.Static;

		//타겟의 어떤 값에 연동할 것인가
		//1. None.. 없다. 그냥 고정값

		//2. Controller -> Controller Param
		//Controller 방식일때
		//public string _keyControlParamName = "";
		public int _keyControlParamID = -1;

		[NonSerialized]
		public apControlParam _keyControlParam = null;

		//3. KeyFrame으로 정의될 때
		//ParamSetGroup은 AnimClip / AnimTimeline / AnimTimelineLayer까지 저장한다. (여기까지가 전부 Key다)
		//ParamSet에서 AnimKeyframe을 저장
		public int _keyAnimClipID = -1;
		public int _keyAnimTimelineID = -1;
		public int _keyAnimTimelineLayerID = -1;

		[NonSerialized]
		public apAnimClip _keyAnimClip = null;

		[NonSerialized]
		public apAnimTimeline _keyAnimTimeline = null;

		[NonSerialized]
		public apAnimTimelineLayer _keyAnimTimelineLayer = null;



		[SerializeField]
		public List<apModifierParamSet> _paramSetList = new List<apModifierParamSet>();


		//해당 파라미터가 적용 중인지 체크한다.
		//변경 : 이 값은 임시 값이므로 NonSerialized로 바꾼다.
		[NonSerialized]
		public bool _isEnabled = true;

		//추가 3.22 : ExclusiveEnabled 처리 타입이 조금 더 상세해진다.
		//3가지 타입으로 바뀌며,
		//Transform / Color로 나뉜다.
		public enum MOD_EX_CALCULATE
		{
			/// <summary>Mod 계산 비활성</summary>
			Disabled,
			/// <summary>Mod 계산이 허용</summary>
			Enabled,
			///// <summary>선택된 "다른 Mod"에 속한 "객체"가 "아닌!" 경우에만 계산이 허용</summary>
			//SubExEnabled,//삭제 21.2.15 : 구조상 이게 불가
		}

		//[NonSerialized]
		//public bool _isEnabledExclusive = true;//<<이건 전버전

		[NonSerialized]
		public MOD_EX_CALCULATE _modExType_Transform = MOD_EX_CALCULATE.Enabled;

		[NonSerialized]
		public MOD_EX_CALCULATE _modExType_Color = MOD_EX_CALCULATE.Enabled;

		//일단 이거 생략
		public bool IsCalculateEnabled
		{
			get
			{
				//return _isEnabled && _isEnabledExclusive;
				return _isEnabled && 
					(_modExType_Transform != MOD_EX_CALCULATE.Disabled || _modExType_Color != MOD_EX_CALCULATE.Disabled);
			}
		}

		public bool IsExCalculatable_Transform { get { return _modExType_Transform != MOD_EX_CALCULATE.Disabled; } }
		public bool IsExCalculatable_Color {  get { return _modExType_Color != MOD_EX_CALCULATE.Disabled; } }

		
		public bool IsAnimEnabledInEditor
		{
			get
			{
				return _keyAnimClip != null && _keyAnimClip._isSelectedInEditor;//툴에서는 애니메이션 클립이 선택된 상태여야 한다.
			}
		}




		// 추가 - 직렬화 + 레이어
		// [이 레이어값은 "Animated가 아닌" 모디파이어에서만 적용된다]
		public int _layerIndex = 0;//레이어 값이 낮을 수록 먼저 계산된다.
		public float _layerWeight = 0.0f;

		public enum BLEND_METHOD
		{
			/// <summary>기존 값을 유지하면서 변화값을 덮어 씌운다.</summary>
			Additive = 0,
			/// <summary>기존 값과 선형 보간을 하며 덮어씌운다.</summary>
			Interpolation = 1
		}

		public BLEND_METHOD _blendMethod = BLEND_METHOD.Interpolation;

		/// <summary>
		/// Color/Visible을 제외하는 Modifier라 할지라도 ParamSetGroup에서 색상 옵션이 꺼져있으면 색상이 계산되지 않는다.
		/// </summary>
		[SerializeField]
		public bool _isColorPropertyEnabled = true;

		//추가 20.2.22 : 알파 변화 없이 Hide <-> Show만 토글되는 경우, Alpha Blending이 아닌 Weight 0.5를 기점으로 아예 토글되어야 한다.
		//기본값은 false
		//조건은 오직 Hide [0.0] <-> Show 일때만이며,
		//Hide대신 Show [0.0]인 경우는 적용되지 않는다.
		[SerializeField]
		public bool _isToggleShowHideWithoutBlend = false;

		//중요!
		//TODO : Animated Modifier에서는 리얼타임으로 "AnimClip을 호출될 때의 Blend + Layer 정보"를 그대로 받아서 쓴다.


		//추가 : Vertex Work Weight
		//Transform-Mesh에서 작업한 VertMorph 내용이 100% 적용되는건 아니다
		//전체 중에서 "일부 Vertex만 적용"할 수 있도록 별도의 VertexWeight를 저장한다. (Layer가 0이 아닐 경우 적용)
		//값은 저장되며, 리스트 순서는 Vertex ID와 Index를 기준으로 배열 순서를 잘 맞춘다.
		//정렬은 자동이다.


		//현재 파라미터들에 공통적으로 적용된 Transform들을 저장한다.
		//뭔가 변동사항이 생기면 Refresh하자

		[NonSerialized] public List<apTransform_Mesh> _syncTransform_Mesh = new List<apTransform_Mesh>();
		[NonSerialized] public List<apTransform_MeshGroup> _syncTransform_MeshGroup = new List<apTransform_MeshGroup>();
		[NonSerialized] public List<apBone> _syncBone = new List<apBone>();

		//추가 v1.5.0 : Sync 데이터의 확장 버전
		[NonSerialized] private Dictionary<apTransform_Mesh, List<apModifierParamSet>> _syncSets_MeshTF = new Dictionary<apTransform_Mesh, List<apModifierParamSet>>();
		[NonSerialized] private Dictionary<apTransform_MeshGroup, List<apModifierParamSet>> _syncSets_MeshGroupTF = new Dictionary<apTransform_MeshGroup, List<apModifierParamSet>>();
		[NonSerialized] private Dictionary<apBone, List<apModifierParamSet>> _syncSets_Bone = new Dictionary<apBone, List<apModifierParamSet>>();


		//추가 21.9.1 : 회전 옵션 (컨트롤 파라미터만 해당)
		public enum TF_ROTATION_LERP_METHOD : int
		{
			/// <summary>기본값. 값 그대로 보간하기</summary>
			Default = 0,
			/// <summary>최소 범위로 회전</summary>
			RotationByVector = 1
		}

		[SerializeField]
		public TF_ROTATION_LERP_METHOD _tfRotationLerpMethod = TF_ROTATION_LERP_METHOD.Default;


		// Init
		//-------------------------------------------------
		/// <summary>
		/// 백업용 생성자. 코드에서는 사용하지 말것
		/// </summary>
		public apModifierParamSetGroup()
		{

		}

		public apModifierParamSetGroup(apPortrait portrait, apModifierBase parentModifier, int layerIndex)
		{
			LinkPortrait(portrait, parentModifier);

			_layerIndex = layerIndex;
			_layerWeight = 1.0f;
			//_blendMethod = BLEND_METHOD.Interpolation;
			_blendMethod = BLEND_METHOD.Additive;//<<기본값을 Additive로 변경

			
		}

		public void LinkPortrait(apPortrait portrait, apModifierBase parentModifier)
		{
			_portrait = portrait;
			_parentModifier = parentModifier;

			//if (_tmpMatrix == null)
			//{
			//	//변경 : apMatrix > apMatrixCal로 변경
			//	//_tmpMatrix = new apMatrix();
			//}
		}



		public void SetStatic()
		{
			_syncTarget = SYNC_TARGET.Static;
		}

		public void SetController(apControlParam controlParam)
		{
			_syncTarget = SYNC_TARGET.Controller;

			_keyControlParam = controlParam;
			//_keyControlParamName = controlParam._keyName;//Name은 사용하지 않는다.
			_keyControlParamID = controlParam._uniqueID;//ID 저장으로 변경

		}

		//TODO : 이걸 호출해야한다.
		public void SetTimeline(apAnimClip animClip, apAnimTimeline timeline, apAnimTimelineLayer timelineLayer)
		{
			_syncTarget = SYNC_TARGET.KeyFrame;
			_keyAnimClipID = animClip._uniqueID;
			_keyAnimClip = animClip;

			_keyAnimTimelineID = timeline._uniqueID;
			_keyAnimTimeline = timeline;

			_keyAnimTimelineLayerID = timelineLayer._uniqueID;
			_keyAnimTimelineLayer = timelineLayer;
		}



		// Functions
		//-------------------------------------------------
		public void RemoveInvalidParamSet()
		{
			for (int i = 0; i < _paramSetList.Count; i++)
			{
				apModifierParamSet paramSet = _paramSetList[i];

				
				//int nRemoveModMesh = paramSet._meshData.RemoveAll(delegate (apModifiedMesh a)
				paramSet._meshData.RemoveAll(delegate (apModifiedMesh a)
				{
					if (a._meshGroupOfModifier == null)
					{
						//Debug.LogError("_meshGroupOfModifier > null");
						return true;
					}
					if (a._meshGroupOfTransform == null)
					{
						//Debug.LogError("_meshGroupOfTransform > null");
						return true;
					}
					if (a._transform_Mesh == null && a._transform_MeshGroup == null)
					{
						//Debug.LogError("_transform_Mesh + _transform_MeshGroup > null");
						return true;
					}
					return false;
				});

				//Debug.LogError("TODO : RemoveInvalidParamSet : 유효하지 않는 Bone을 결정해야한다.");
				paramSet._boneData.RemoveAll(delegate (apModifiedBone a)
				{
					return a._bone == null;//4.4 변경
				});
			}
		}


		public void SortParamSet()
		{
			if (_keyControlParam != null)
			{
				_paramSetList.Sort(delegate (apModifierParamSet a, apModifierParamSet b)
				{
					switch (_keyControlParam._valueType)
					{
					//case apControlParam.TYPE.Bool:
					//	return 0;

					case apControlParam.TYPE.Int:
							return a._conSyncValue_Int - b._conSyncValue_Int;

						case apControlParam.TYPE.Float:
							return (int)((a._conSyncValue_Float - b._conSyncValue_Float) * 1000.0f);

						case apControlParam.TYPE.Vector2:
							if (Mathf.Abs(a._conSyncValue_Vector2.y - b._conSyncValue_Vector2.y) < 0.001f)
							{
								return (int)((a._conSyncValue_Vector2.x - b._conSyncValue_Vector2.x) * 1000.0f);
							}
							else
							{
								return (int)((a._conSyncValue_Vector2.y - b._conSyncValue_Vector2.y) * 1000.0f);
							}
					}
					return 0;

				});

				//추가 : ParamSet의 Dist 보간을 위해서 "보간 영역"을 설정해줘야 한다.
				float rangeBias = 1000.0f;
				for (int iParam = 0; iParam < _paramSetList.Count; iParam++)
				{
					apModifierParamSet paramSet = _paramSetList[iParam];

					switch (_keyControlParam._valueType)
					{
						case apControlParam.TYPE.Int:
							paramSet._conSyncValueRange_Under = new Vector3(_keyControlParam._int_Min, 0.0f, 0.0f);
							paramSet._conSyncValueRange_Over = new Vector3(_keyControlParam._int_Max, 0.0f, 0.0f);
							break;

						case apControlParam.TYPE.Float:
							paramSet._conSyncValueRange_Under = new Vector3(_keyControlParam._float_Min, 0.0f, 0.0f);
							paramSet._conSyncValueRange_Over = new Vector3(_keyControlParam._float_Max, 0.0f, 0.0f);
							break;

						case apControlParam.TYPE.Vector2:
							paramSet._conSyncValueRange_Under = new Vector3(_keyControlParam._vec2_Min.x, _keyControlParam._vec2_Min.y, 0.0f);
							paramSet._conSyncValueRange_Over = new Vector3(_keyControlParam._vec2_Max.x, _keyControlParam._vec2_Max.y, 0.0f);
							break;
					}

					//Bias를 가감해줘야 이후 Weight 계산시 Range 경계에서 적절한 값을 가진다.
					paramSet._conSyncValueRange_Under -= Vector2.one * rangeBias;
					paramSet._conSyncValueRange_Over += Vector2.one * rangeBias;




					//다른 값과 비교하여 Range를 정한다.
					//영역을 축소하는 방향으로
					for (int iNext = 0; iNext < _paramSetList.Count; iNext++)
					{
						apModifierParamSet nextParamSet = _paramSetList[iNext];
						if (nextParamSet == paramSet)
						{
							continue;
						}

						switch (_keyControlParam._valueType)
						{
							case apControlParam.TYPE.Int:
								{
									int nextValue_Int = nextParamSet._conSyncValue_Int;
									if (nextValue_Int <= paramSet._conSyncValue_Int)
									{
										//값이 작은 경우 + 영역보다 값이 큰 경우
										if (nextValue_Int > paramSet._conSyncValueRange_Under.x)
										{
											paramSet._conSyncValueRange_Under.x = nextValue_Int;
										}
									}
									if (nextValue_Int >= paramSet._conSyncValue_Int)
									{
										//값이 큰 경우 + 영역보다 값이 작은 경우
										if (nextValue_Int < paramSet._conSyncValueRange_Over.x)
										{
											paramSet._conSyncValueRange_Over.x = nextValue_Int;
										}
									}
								}
								break;

							case apControlParam.TYPE.Float:
								{
									float nextValue_float = nextParamSet._conSyncValue_Float;
									if (nextValue_float <= paramSet._conSyncValue_Float)
									{
										//값이 작은 경우 + 영역보다 값이 큰 경우
										if (nextValue_float > paramSet._conSyncValueRange_Under.x)
										{
											paramSet._conSyncValueRange_Under.x = nextValue_float;
										}
									}
									if (nextValue_float >= paramSet._conSyncValue_Float)
									{
										//값이 큰 경우 + 영역보다 값이 작은 경우
										if (nextValue_float < paramSet._conSyncValueRange_Over.x)
										{
											paramSet._conSyncValueRange_Over.x = nextValue_float;
										}
									}
								}
								break;

							case apControlParam.TYPE.Vector2:
								{
									Vector2 nextValue_Vec2 = nextParamSet._conSyncValue_Vector2;
									//X, Y에 대해서 값을 각각 처리한다.

									//X에 대해서 영역 처리할 땐 -> Y가 같은 경우에만
									//Y에 대해서 영역 처리할 땐 -> X가 같은 경우에만

									bool isXSame = Mathf.Abs(nextValue_Vec2.x - paramSet._conSyncValue_Vector2.x) < 0.01f;
									bool isYSame = Mathf.Abs(nextValue_Vec2.y - paramSet._conSyncValue_Vector2.y) < 0.01f;

									//Under - X
									if (nextValue_Vec2.x <= paramSet._conSyncValue_Vector2.x && isYSame)
									{
										//값이 작은 경우 + 영역보다 값이 큰 경우
										if (nextValue_Vec2.x > paramSet._conSyncValueRange_Under.x)
										{
											paramSet._conSyncValueRange_Under.x = nextValue_Vec2.x;
										}
									}

									//Under - Y
									if (nextValue_Vec2.y <= paramSet._conSyncValue_Vector2.y && isXSame)
									{
										//값이 작은 경우 + 영역보다 값이 큰 경우
										if (nextValue_Vec2.y > paramSet._conSyncValueRange_Under.y)
										{
											paramSet._conSyncValueRange_Under.y = nextValue_Vec2.y;
										}
									}

									//Over - X
									if (nextValue_Vec2.x >= paramSet._conSyncValue_Vector2.x && isYSame)
									{
										//값이 큰 경우 + 영역보다 값이 작은 경우
										if (nextValue_Vec2.x < paramSet._conSyncValueRange_Over.x)
										{
											paramSet._conSyncValueRange_Over.x = nextValue_Vec2.x;
										}
									}

									//Over - Y
									if (nextValue_Vec2.y >= paramSet._conSyncValue_Vector2.y && isXSame)
									{
										//값이 큰 경우 + 영역보다 값이 작은 경우
										if (nextValue_Vec2.y < paramSet._conSyncValueRange_Over.y)
										{
											paramSet._conSyncValueRange_Over.y = nextValue_Vec2.y;
										}
									}
								}
								break;
						}
					}
				}
			}
		}


		public bool RefreshSync()
		{
			return RefreshSyncWithNewMod(null, null);
		}

		public bool RefreshSyncWithNewMod(Dictionary<apModifierParamSet, List<apModifiedMesh>> createdModMeshes, Dictionary<apModifierParamSet, List<apModifiedBone>> createdModBones)
		{
			if (_syncTransform_Mesh == null) { _syncTransform_Mesh = new List<apTransform_Mesh>(); }
			_syncTransform_Mesh.Clear();

			if (_syncTransform_MeshGroup == null) { _syncTransform_MeshGroup = new List<apTransform_MeshGroup>(); }
			_syncTransform_MeshGroup.Clear();

			if (_syncBone == null) { _syncBone = new List<apBone>(); }
			_syncBone.Clear();

			//추가 v1.5.0 : 객체-ModBone/ModMesh 연결 정보도 저장
			if(_syncSets_MeshTF == null) { _syncSets_MeshTF = new Dictionary<apTransform_Mesh, List<apModifierParamSet>>(); }
			_syncSets_MeshTF.Clear();

			if (_syncSets_MeshGroupTF == null) { _syncSets_MeshGroupTF = new Dictionary<apTransform_MeshGroup, List<apModifierParamSet>>(); }
			_syncSets_MeshGroupTF.Clear();

			if (_syncSets_Bone == null) { _syncSets_Bone = new Dictionary<apBone, List<apModifierParamSet>>(); }
			_syncSets_Bone.Clear();

			List<apModifierParamSet> curSyncSetPS = null;

			//한번이라도 등장한 MeshTransform / MeshGroup Transform을 찾자
			apModifierParamSet paramSet = null;
			int nParamSets = _paramSetList != null ? _paramSetList.Count : 0;
			if (nParamSets > 0)
			{
				for (int i = 0; i < nParamSets; i++)
				{
					paramSet = _paramSetList[i];

					int nModMeshes = paramSet._meshData != null ? paramSet._meshData.Count : 0;
					if (nModMeshes > 0)
					{
						apModifiedMesh modMesh = null;
						for (int iModMesh = 0; iModMesh < nModMeshes; iModMesh++)
						{
							modMesh = paramSet._meshData[iModMesh];

							if (modMesh._transform_Mesh != null
								&& modMesh._transform_Mesh._mesh != null)
							{
								if (!_syncTransform_Mesh.Contains(modMesh._transform_Mesh))
								{
									_syncTransform_Mesh.Add(modMesh._transform_Mesh);
								}

								//추가 v1.5.0 : 빠른 동기화 위한 데이터 생성
								curSyncSetPS = null;
								_syncSets_MeshTF.TryGetValue(modMesh._transform_Mesh, out curSyncSetPS);
								if(curSyncSetPS == null)
								{
									curSyncSetPS = new List<apModifierParamSet>();
									_syncSets_MeshTF.Add(modMesh._transform_Mesh, curSyncSetPS);
								}
								if(!curSyncSetPS.Contains(paramSet))
								{
									curSyncSetPS.Add(paramSet);
								}

							}
							if (modMesh._transform_MeshGroup != null)
							{
								if (!_syncTransform_MeshGroup.Contains(modMesh._transform_MeshGroup))
								{
									_syncTransform_MeshGroup.Add(modMesh._transform_MeshGroup);
								}

								//추가 v1.5.0 : 빠른 동기화 위한 데이터 생성
								curSyncSetPS = null;
								_syncSets_MeshGroupTF.TryGetValue(modMesh._transform_MeshGroup, out curSyncSetPS);
								if(curSyncSetPS == null)
								{
									curSyncSetPS = new List<apModifierParamSet>();
									_syncSets_MeshGroupTF.Add(modMesh._transform_MeshGroup, curSyncSetPS);
								}
								if(!curSyncSetPS.Contains(paramSet))
								{
									curSyncSetPS.Add(paramSet);
								}
							}
						}
					}

					int nModBones = paramSet._boneData != null ? paramSet._boneData.Count : 0;
					if (nModBones > 0)
					{
						apModifiedBone modBone = null;
						for (int iModBone = 0; iModBone < nModBones; iModBone++)
						{
							modBone = paramSet._boneData[iModBone];

							if (modBone._bone != null)
							{
								//>> 이건 Bone Set이 필요없다.
								if (modBone._meshGroup_Bone != null
									&& modBone._meshGroup_Bone._boneList_All != null
									&& modBone._meshGroup_Bone._boneList_All.Contains(modBone._bone)//해당 MeshGroup에 Bone이 존재하는가.
									)
								{
									if (!_syncBone.Contains(modBone._bone))
									{
										_syncBone.Add(modBone._bone);
									}

									//추가 v1.5.0 : 빠른 동기화 위한 데이터 생성
									curSyncSetPS = null;
									_syncSets_Bone.TryGetValue(modBone._bone, out curSyncSetPS);
									if(curSyncSetPS == null)
									{
										curSyncSetPS = new List<apModifierParamSet>();
										_syncSets_Bone.Add(modBone._bone, curSyncSetPS);
									}
									if(!curSyncSetPS.Contains(paramSet))
									{
										curSyncSetPS.Add(paramSet);
									}
								}
							}
						}
					}

				}
			}
			

			//MeshTF, MeshGroupTF, Bone을 Sync 리스트에 넣고 ModMesh/ModBone이 있는지 확인하여 없으면 추가하는 코드
			//근데 이미 생성된 ModMesh/ModBone을 전달하지 않으니 매번 전체 검사를 해야한다. (AddMeshTransformToParamSet 함수 등등에서)
			//그래서 Sync 리스트에 ModMesh/ModBone도 넣어서 전달하던가 해야겠다.

			//동기화 전용 Sync Transform을 모든 ParamSet에 넣자
			bool isAnyChanged = false;

			int nSyncMeshes = _syncTransform_Mesh != null ? _syncTransform_Mesh.Count : 0;
			nParamSets = _paramSetList != null ? _paramSetList.Count : 0;
			
			apModifierParamSet curParamSet = null;

			if (nSyncMeshes > 0 && nParamSets > 0)
			{
				apTransform_Mesh meshTransform = null;

				//이전 (모든 ParamSet을 항상 비교하여 불필요한 처리가 많음)
				//for (int iSync = 0; iSync < nSyncMeshes; iSync++)
				//{
				//	meshTransform = _syncTransform_Mesh[iSync];
					
				//	for (int iParamSet = 0; iParamSet < nParamSets; iParamSet++)
				//	{
				//		curParamSet = _paramSetList[iParamSet];
				//		bool isAdd = AddMeshTransformToParamSet(curParamSet, meshTransform);

				//		if (isAdd)
				//		{
				//			isAnyChanged = true;
				//		}
				//	}
				//}

				//변경 v1.5.0
				foreach (KeyValuePair<apTransform_Mesh, List<apModifierParamSet>> syncSet in _syncSets_MeshTF)
				{
					meshTransform = syncSet.Key;
					curSyncSetPS = syncSet.Value;

					//이 MeshTF에 대한 ParamSet 개수와 PSG의 ParamSet 개수가 다른 경우 누락된게 있다.
					//반대로 개수가 같다면 동기화를 할 필요가 없다. (불필요한 처리 방지)
					if(curSyncSetPS.Count == nParamSets)
					{
						//Debug.Log("이 MeshTF [" + meshTransform._nickName + "]에 대한 Sync가 이미 완료되어 동기화 불필요 (" + nParamSets + ")");
						continue;
					}

					//Debug.LogError("이 MeshTF [" + meshTransform._nickName + "]에 대한 Sync가 이미 완료되지 않아서 동기화 필요 (" + curSyncSetPS.Count + " > " + nParamSets + ")");

					//이 MeshTF를 ModMesh로서 저장하지 못한 ParamSet을 찾아서 ModMesh를 생성해주자
					for (int iParamSet = 0; iParamSet < nParamSets; iParamSet++)
					{
						curParamSet = _paramSetList[iParamSet];
						
						//빠른 PS 체크를 한다.
						if(curSyncSetPS.Contains(curParamSet))
						{
							continue;
						}

						//이 ParamSet은 해당 MeshTF를 가지지 못했다.
						//ModMesh를 생성해주자.
						apModifiedMesh newModMesh = null;
						bool isAdd = AddMeshTransformToParamSet(curParamSet, meshTransform, out newModMesh);

						if (isAdd)
						{
							isAnyChanged = true;

							//요청이 있다면 추가된 ModMesh를 매핑 리스트에 넣어주자
							if(createdModMeshes != null && newModMesh != null)
							{
								List<apModifiedMesh> modMeshList = null;
								createdModMeshes.TryGetValue(curParamSet, out modMeshList);
								if(modMeshList == null)
								{
									modMeshList = new List<apModifiedMesh>();
									createdModMeshes.Add(curParamSet, modMeshList);
								}
								modMeshList.Add(newModMesh);
							}
						}
					}
				}
			}
			
			int nSyncMeshGroups = _syncTransform_MeshGroup != null ? _syncTransform_MeshGroup.Count : 0;
			if (nSyncMeshGroups > 0 && nParamSets > 0)
			{
				apTransform_MeshGroup meshGroupTransform = null;
				
				//이전 (항상 모든 ParamSet에 대해 동기화)
				//for (int iSync = 0; iSync < nSyncMeshGroups; iSync++)
				//{
				//	meshGroupTransform = _syncTransform_MeshGroup[iSync];					
				//	for (int iParamSet = 0; iParamSet < nParamSets; iParamSet++)
				//	{
				//		curParamSet = _paramSetList[iParamSet];
				//		bool isAdd = AddMeshGroupTransformToParamSet(curParamSet, meshGroupTransform);
				//		if (isAdd)
				//		{
				//			isAnyChanged = true;
				//		}
				//	}
				//}

				//변경 v1.5.0
				foreach (KeyValuePair<apTransform_MeshGroup, List<apModifierParamSet>> syncSet in _syncSets_MeshGroupTF)
				{
					meshGroupTransform = syncSet.Key;
					curSyncSetPS = syncSet.Value;

					//이 MeshGroupTF에 대한 ParamSet 개수와 PSG의 ParamSet 개수가 다른 경우 누락된게 있다.
					//반대로 개수가 같다면 동기화를 할 필요가 없다. (불필요한 처리 방지)
					if(curSyncSetPS.Count == nParamSets)
					{
						continue;
					}

					for (int iParamSet = 0; iParamSet < nParamSets; iParamSet++)
					{
						curParamSet = _paramSetList[iParamSet];

						//빠른 PS 체크를 한다.
						if(curSyncSetPS.Contains(curParamSet))
						{
							continue;
						}

						apModifiedMesh newModMesh = null;
						bool isAdd = AddMeshGroupTransformToParamSet(curParamSet, meshGroupTransform, out newModMesh);
						if (isAdd)
						{
							isAnyChanged = true;

							//요청이 있다면 추가된 ModMesh를 매핑 리스트에 넣어주자
							if(createdModMeshes != null && newModMesh != null)
							{
								List<apModifiedMesh> modMeshList = null;
								createdModMeshes.TryGetValue(curParamSet, out modMeshList);
								if(modMeshList == null)
								{
									modMeshList = new List<apModifiedMesh>();
									createdModMeshes.Add(curParamSet, modMeshList);
								}
								modMeshList.Add(newModMesh);
							}
						}
					}
				}
			}
			
			int nSyncBones = _syncBone != null ? _syncBone.Count : 0;
			if (nSyncBones > 0 && nParamSets > 0)
			{
				apBone bone = null;
				apTransform_MeshGroup meshGroupTransform = null;

				//이전 (항상 모든 ParamSet에 대해서 동기화 시도)
				//for (int iSync = 0; iSync < nSyncBones; iSync++)
				//{
				//	bone = _syncBone[iSync];

				//	meshGroupTransform = null;
				//	if (_parentModifier._meshGroup == bone._meshGroup)
				//	{
				//		meshGroupTransform = _parentModifier._meshGroup._rootMeshGroupTransform;
				//	}
				//	else
				//	{
				//		meshGroupTransform = _parentModifier._meshGroup.FindChildMeshGroupTransform(bone._meshGroup);
				//	}

				//	for (int iParamSet = 0; iParamSet < nParamSets; iParamSet++)
				//	{
				//		//이전
				//		//bool isAdd = AddBoneToParamSet(_paramSetList[iParamSet], bone._meshGroup._rootMeshGroupTransform, bone);
						
				//		//변경 : ChildMeshGroup의 Root MGTF로 설정하는 코드는 잘못되었다.
				//		curParamSet = _paramSetList[iParamSet];
				//		bool isAdd = AddBoneToParamSet(curParamSet, meshGroupTransform, bone);
				//		if (isAdd)
				//		{
				//			isAnyChanged = true;
				//		}
				//	}
				//}

				//변경 v1.5.0
				foreach (KeyValuePair<apBone, List<apModifierParamSet>> syncSet in _syncSets_Bone)
				{
					bone = syncSet.Key;
					curSyncSetPS = syncSet.Value;

					meshGroupTransform = null;
					if (_parentModifier._meshGroup == bone._meshGroup)
					{
						meshGroupTransform = _parentModifier._meshGroup._rootMeshGroupTransform;
					}
					else
					{
						meshGroupTransform = _parentModifier._meshGroup.FindChildMeshGroupTransform(bone._meshGroup);
					}

					//이 Bone에 대한 ParamSet 개수와 PSG의 ParamSet 개수가 다른 경우 누락된게 있다.
					//반대로 개수가 같다면 동기화를 할 필요가 없다. (불필요한 처리 방지)
					if(curSyncSetPS.Count == nParamSets)
					{
						//Debug.Log("이 Bone [" + bone._name + "]에 대한 Sync가 이미 완료되어 동기화 불필요 (" + nParamSets + ")");
						continue;
					}

					//Debug.LogError("이 Bone [" + bone._name + "]에 대한 Sync가 이미 완료되지 않아서 동기화 필요 (" + curSyncSetPS.Count + " > " + nParamSets + ")");

					//이 Bone를 ModBone으로 저장하지 못한 ParamSet을 찾아서 ModBone을 생성해주자
					for (int iParamSet = 0; iParamSet < nParamSets; iParamSet++)
					{
						//변경 : ChildMeshGroup의 Root MGTF로 설정하는 코드는 잘못되었다.
						curParamSet = _paramSetList[iParamSet];

						//빠른 PS 체크를 한다.
						if(curSyncSetPS.Contains(curParamSet))
						{
							continue;
						}

						apModifiedBone newModBone = null;
						bool isAdd = AddBoneToParamSet(curParamSet, meshGroupTransform, bone, out newModBone);
						if (isAdd)
						{
							isAnyChanged = true;

							//요청이 있다면 추가된 ModBone을 매핑 리스트에 넣어주자
							if(createdModBones != null && newModBone != null)
							{
								List<apModifiedBone> modBoneList = null;
								createdModBones.TryGetValue(curParamSet, out modBoneList);
								if(modBoneList == null)
								{
									modBoneList = new List<apModifiedBone>();
									createdModBones.Add(curParamSet, modBoneList);
								}
								modBoneList.Add(newModBone);
							}
						}
					}
				}
			}
			


			return isAnyChanged;
		}


		// 중요!
		//Mesh Transform / MeshGroup Transform을 각각의 ParamSet에 넣어준다.
		//Modifier 조건에 맞게 처리한다.
		private bool AddMeshTransformToParamSet(apModifierParamSet paramSet, apTransform_Mesh meshTransform, out apModifiedMesh addedModMesh)
		{
			addedModMesh = null;

			if (!_parentModifier.IsTarget_MeshTransform || meshTransform == null)
			{
				return false;
			}

			if(meshTransform._mesh == null)
			{
				return false;
			}
			
			apMeshGroup parentMeshGroup = _parentModifier._meshGroup;

			//이전 (GC 발생)
			//bool isExist = paramSet._meshData.Exists(delegate (apModifiedMesh a)
			//{
			//	return a.IsContains_MeshTransform(_parentModifier._meshGroup, meshTransform, meshTransform._mesh);
			//});

			s_FindModMesh_MeshGroup = parentMeshGroup;
			s_FindModMesh_MeshTF = meshTransform;
			s_FindModMesh_SrcMesh = meshTransform._mesh;
		
			//변경 v1.5.0
			bool isExist = false;
			if(paramSet._meshData != null)
			{
				isExist = paramSet._meshData.Exists(s_FindModMeshByMeshTF_Func);
			}

			if (!isExist)
			{	
				apRenderUnit targetRenderUnit = parentMeshGroup.GetRenderUnit(meshTransform);
				if (targetRenderUnit != null)
				{
					apMeshGroup parentMeshGroupOfTransform = GetParentMeshGroupOfMeshTransform(meshTransform);
					if (parentMeshGroupOfTransform == null)
					{
						//Parent MeshGroup이 없네염
						return false;
					}

					apModifiedMesh modMesh = new apModifiedMesh();

					modMesh.Init(parentMeshGroup._uniqueID, parentMeshGroupOfTransform._uniqueID, _parentModifier.ModifiedValueType);


					modMesh.SetTarget_MeshTransform(meshTransform._transformUniqueID, meshTransform._mesh._uniqueID, meshTransform._meshColor2X_Default, meshTransform._isVisible_Default);
					modMesh.Link_MeshTransform(parentMeshGroup, parentMeshGroupOfTransform, meshTransform, targetRenderUnit, _portrait);

					if (paramSet._meshData == null)
					{
						paramSet._meshData = new List<apModifiedMesh>();
					}
					paramSet._meshData.Add(modMesh);

					addedModMesh = modMesh;
				}
			}

			return !isExist;
		}

		





		private bool AddMeshGroupTransformToParamSet(apModifierParamSet paramSet, apTransform_MeshGroup meshGroupTransform, out apModifiedMesh addedModMesh)
		{
			addedModMesh = null;

			if (!_parentModifier.IsTarget_MeshGroupTransform)
			{
				return false;
			}

			apMeshGroup parentMeshGroup = _parentModifier._meshGroup;

			//이전
			//bool isExist = paramSet._meshData.Exists(delegate (apModifiedMesh a)
			//{
			//	return a.IsContains_MeshGroupTransform(_parentModifier._meshGroup, meshGroupTransform);
			//});

			//변경 v1.5.0
			s_FindModMesh_MeshGroup = parentMeshGroup;
			s_FindModMesh_MeshGroupTF = meshGroupTransform;
			bool isExist = paramSet._meshData.Exists(s_FindModMeshByMeshGroupTF_Func);


			if (!isExist)
			{
				apRenderUnit targetRenderUnit = parentMeshGroup.GetRenderUnit(meshGroupTransform);
				if (targetRenderUnit != null)
				{
					apMeshGroup parentMeshGroupOfTransform = GetParentMeshGroupOfMeshGroupTransform(meshGroupTransform);
					if (parentMeshGroupOfTransform == null)
					{
						//Parent MeshGroup이 없네염
						return false;
					}

					apModifiedMesh modMesh = new apModifiedMesh();
					modMesh.Init(parentMeshGroup._uniqueID, parentMeshGroupOfTransform._uniqueID, _parentModifier.ModifiedValueType);
					modMesh.SetTarget_MeshGroupTransform(meshGroupTransform._transformUniqueID, meshGroupTransform._meshColor2X_Default, meshGroupTransform._isVisible_Default);

					modMesh.Link_MeshGroupTransform(parentMeshGroup, parentMeshGroupOfTransform, meshGroupTransform, targetRenderUnit);
					paramSet._meshData.Add(modMesh);

					addedModMesh = modMesh;
				}
			}

			return !isExist;
		}

		



		private bool AddBoneToParamSet(apModifierParamSet paramSet, apTransform_MeshGroup meshGroupTransform, apBone bone, out apModifiedBone addedModBone)
		{
			addedModBone = null;

			if (!_parentModifier.IsTarget_Bone)
			{
				return false;
			}

			apMeshGroup parentMeshGroup = _parentModifier._meshGroup;

			//이전
			//bool isExist = paramSet._boneData.Exists(delegate (apModifiedBone a)
			//{
			//	return a._bone == bone;
			//});

			//변경 v1.5.0
			s_FindModBone_Bone = bone;
			bool isExist = paramSet._boneData.Exists(s_FindModBoneByBone_Func);

			if (!isExist)
			{
				apMeshGroup parentMeshGroupOfBone = GetParentMeshGroupOfBone(bone);
				if (parentMeshGroupOfBone == null)
				{
					return false;
				}

				apRenderUnit targetRenderUnit = parentMeshGroup.GetRenderUnit(meshGroupTransform);

				apModifiedBone modBone = new apModifiedBone();
				modBone.Init(parentMeshGroup._uniqueID, parentMeshGroupOfBone._uniqueID, meshGroupTransform._transformUniqueID, bone);
				modBone.Link(parentMeshGroup, parentMeshGroupOfBone, bone, targetRenderUnit, meshGroupTransform);


				paramSet._boneData.Add(modBone);

				addedModBone = modBone;
			}

			return !isExist;
		}


		private static apMeshGroup s_FindModMesh_MeshGroup = null;
		private static apTransform_Mesh s_FindModMesh_MeshTF = null;
		private static apMesh s_FindModMesh_SrcMesh = null;
		private static Predicate<apModifiedMesh> s_FindModMeshByMeshTF_Func = FUNC_FindModMeshByMeshTF;
		private static bool FUNC_FindModMeshByMeshTF(apModifiedMesh a)
		{
			return a.IsContains_MeshTransform(s_FindModMesh_MeshGroup, s_FindModMesh_MeshTF, s_FindModMesh_SrcMesh);
		}

		private static apTransform_MeshGroup s_FindModMesh_MeshGroupTF = null;
		private static Predicate<apModifiedMesh> s_FindModMeshByMeshGroupTF_Func = FUNC_FindModMeshByMeshGroupTF;
		private static bool FUNC_FindModMeshByMeshGroupTF(apModifiedMesh a)
		{
			return a.IsContains_MeshGroupTransform(s_FindModMesh_MeshGroup, s_FindModMesh_MeshGroupTF);
		}

		private static apBone s_FindModBone_Bone = null;
		private static Predicate<apModifiedBone> s_FindModBoneByBone_Func = FUNC_FindModBoneByBone;
		private static bool FUNC_FindModBoneByBone(apModifiedBone a)
		{
			return a._bone == s_FindModBone_Bone;
		}

		/// <summary>
		/// 처리를 도와주는 함수. MeshTransform의 Parent MeshGroup을 검색한다.
		/// </summary>
		/// <param name="meshTransform"></param>
		/// <returns></returns>
		private apMeshGroup GetParentMeshGroupOfMeshTransform(apTransform_Mesh meshTransform)
		{
			int nMeshGroups = _portrait._meshGroups != null ? _portrait._meshGroups.Count : 0;
			if(nMeshGroups == 0)
			{
				return null;
			}

			apMeshGroup curMeshGroup = null;
			for (int i = 0; i < nMeshGroups; i++)
			{
				curMeshGroup = _portrait._meshGroups[i];
				int nMeshTFs = curMeshGroup._childMeshTransforms != null ? curMeshGroup._childMeshTransforms.Count : 0;
				if(nMeshTFs == 0)
				{
					continue;
				}
				if (curMeshGroup._childMeshTransforms.Contains(meshTransform))
				{
					//찾았다!
					return curMeshGroup;
				}
			}
			return null;
		}


		/// <summary>
		/// 처리를 도와주는 함수. MeshTransform의 Parent MeshGroup을 검색한다.
		/// </summary>
		/// <param name="meshTransform"></param>
		/// <returns></returns>
		private apMeshGroup GetParentMeshGroupOfMeshGroupTransform(apTransform_MeshGroup meshGroupTransform)
		{
			int nMeshGroups = _portrait._meshGroups != null ? _portrait._meshGroups.Count : 0;
			if(nMeshGroups == 0)
			{
				return null;
			}

			apMeshGroup curMeshGroup = null;

			for (int i = 0; i < nMeshGroups; i++)
			{
				curMeshGroup = _portrait._meshGroups[i];
				int nMeshGroupTFs = curMeshGroup._childMeshGroupTransforms != null ? curMeshGroup._childMeshGroupTransforms.Count : 0;
				if(nMeshGroupTFs == 0)
				{
					continue;
				}

				if (curMeshGroup._childMeshGroupTransforms.Contains(meshGroupTransform))
				{
					//찾았다!
					return curMeshGroup;
				}
			}
			return null;
		}

		private apMeshGroup GetParentMeshGroupOfBone(apBone bone)
		{
			int nMeshGroups = _portrait._meshGroups != null ? _portrait._meshGroups.Count : 0;
			if(nMeshGroups == 0)
			{
				return null;
			}

			apMeshGroup curMeshGroup = null;

			for (int i = 0; i < nMeshGroups; i++)
			{
				curMeshGroup = _portrait._meshGroups[i];
				int nBones = curMeshGroup._boneList_All != null ? curMeshGroup._boneList_All.Count : 0;
				if(nBones == 0)
				{
					continue;
				}

				if (curMeshGroup._boneList_All.Contains(bone))
				{
					//찾았다!
					return curMeshGroup;
				}
			}
			return null;
		}

		//삭제 v1.5.0
		//public bool IsSubMeshInGroup(apTransform_Mesh subMeshTransform)
		//{
		//	if (subMeshTransform == null)
		//	{ return false; }
		//	return _syncTransform_Mesh.Contains(subMeshTransform);
		//}

		//public bool IsSubMeshGroupInGroup(apTransform_MeshGroup subMeshGroupTransform)
		//{
		//	if (subMeshGroupTransform == null)
		//	{ return false; }
		//	return _syncTransform_MeshGroup.Contains(subMeshGroupTransform);
		//}


		// 추가
		//--------------------------------------------------------------------------------
		/// <summary>
		/// ParamSetGroup 내의 모든 ParamSet에서 MeshTransform을 포함한 ModMesh를 모두 삭제한다.
		/// 주의 메세지를 꼭 출력할 것
		/// </summary>
		/// <param name="meshTransform"></param>
		public void RemoveModifierMeshes(apTransform_Mesh meshTransform)
		{
			apModifierParamSet paramSet = null;
			for (int i = 0; i < _paramSetList.Count; i++)
			{
				paramSet = _paramSetList[i];
				paramSet._meshData.RemoveAll(delegate (apModifiedMesh a)
				{
					return a.IsContains_MeshTransform(_parentModifier._meshGroup, meshTransform, meshTransform._mesh);
				});

				//if(nRemoved > 0)
				//{
				//	//테스트
				//	Debug.LogError("Mesh Data 삭제");
				//}
			}


		}

		/// <summary>
		/// ParamSetGroup 내의 모든 ParamSet에서 MeshGroupTransform을 포함한 ModMesh를 모두 삭제한다.
		/// 주의 메세지를 꼭 출력할 것
		/// </summary>
		/// <param name="meshGroupTransform"></param>
		public void RemoveModifierMeshes(apTransform_MeshGroup meshGroupTransform)
		{
			apModifierParamSet paramSet = null;
			for (int i = 0; i < _paramSetList.Count; i++)
			{
				paramSet = _paramSetList[i];
				paramSet._meshData.RemoveAll(delegate (apModifiedMesh a)
				{
					return a.IsContains_MeshGroupTransform(_parentModifier._meshGroup, meshGroupTransform);
				});

				//if(nRemoved > 0)
				//{
				//	Debug.LogError("테스트 : MeshData 삭제");
				//}
			}
		}

		public void RemoveModifierBones(apBone bone)
		{
			apModifierParamSet paramSet = null;
			for (int i = 0; i < _paramSetList.Count; i++)
			{
				paramSet = _paramSetList[i];
				paramSet._boneData.RemoveAll(delegate (apModifiedBone a)
				{
					return a._bone == bone;
				});
			}
		}

		// Get / Set
		//-------------------------------------------------
		public bool IsMeshTransformContain(apTransform_Mesh meshTransform)
		{
			return _syncTransform_Mesh.Contains(meshTransform);
		}

		public bool IsMeshGroupTransformContain(apTransform_MeshGroup meshGroupTransform)
		{
			return _syncTransform_MeshGroup.Contains(meshGroupTransform);
		}

		// 삭제 19.5.20 : 이 변수는 더이상 사용되지 않는다.
		//public apModifierParamSetGroupVertWeight GetWeightVertexData(apTransform_Mesh meshTransform)
		//{
		//	return _calculatedWeightedVertexList.Find(delegate (apModifierParamSetGroupVertWeight a)
		//		{
		//			return a._meshTransform_ID == meshTransform._transformUniqueID;
		//		});
		//}

		public bool IsBoneContain(apBone bone)
		{
			return _syncBone.Contains(bone);
		}
	}

}
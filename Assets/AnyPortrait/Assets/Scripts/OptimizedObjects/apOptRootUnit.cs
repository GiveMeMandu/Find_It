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
//using UnityEngine.Profiling;
using System.Collections;
using System.Collections.Generic;
using System;


using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// This class is the basic unit for performing updates including meshes.
	/// (You can refer to this in your script, but we do not recommend using it directly.)
	/// </summary>
	public class apOptRootUnit : MonoBehaviour
	{
		// Members
		//------------------------------------------------
		public apPortrait _portrait = null;

		public apOptTransform _rootOptTransform = null;
		
		[HideInInspector]
		public Transform _transform = null;

		//빠른 처리를 위해 OptBone을 리스트로 저장해두자. 오직 참조용
		[SerializeField, HideInInspector]
		private List<apOptBone> _optBones = new List<apOptBone>();

		[SerializeField, HideInInspector]
		private List<apOptTransform> _optTransforms = new List<apOptTransform>();

		//빠른 참조를 위한 Dictionary
		[NonSerialized]
		private Dictionary<string, apOptBone> _optBonesMap = new Dictionary<string, apOptBone>();

		[NonSerialized]
		private Dictionary<string, apOptTransform> _optTransformMap = new Dictionary<string, apOptTransform>();

		public List<apOptTransform> OptTransforms
		{
			get
			{
				return _optTransforms;
			}
		}

		//추가 21.8.21
		public List<apOptBone> OptBones
		{
			get
			{
				return _optBones;
			}
		}

		//추가 12.6 : SortedBuffer
		[SerializeField, NonBackupField]//백업하진 않는다. 다시 Bake하세염
		public apOptSortedRenderBuffer _sortedRenderBuffer = new apOptSortedRenderBuffer();


		//추가 v1.4.8 : 루트 모션을 위한 본. 없을 수도 있다.
		[SerializeField] public int _rootMotionBoneID = -1;
		[NonSerialized] private apOptBone _linkedRootMotionBone = null;
		public apOptBone RootMotionBone { get { return _linkedRootMotionBone; } }



		//추가 2.25 : Flipped 체크
		private bool _isFlipped_X = false;
		private bool _isFlipped_Y = false;
		
		//private bool _isFlipped = false;

		//v1.4.7 : 이전의 Flip 여부가 체크되었는지 여부 + 값을 저장한다.
		//값이 변한다면 메시 갱신에 변화가 있기 때문
		private bool _isFlippedCheckedPrev = false;
		private bool _isFlipped_X_Prev = false;
		private bool _isFlipped_Y_Prev = false;


		private bool _isFirstFlippedCheck = true;
		//private Vector3 _defaultScale = Vector3.one;

		//추가 21.4.3 : 가시성 정보를 멤버로 가진다. 루트유닛 전환시 에러가 발생하는걸 방지하기 위함
		[NonSerialized]
		public bool _isVisible = false;
		

		//추가 v1.4.8 : 루트모션용 변수와 함수 대리자
		private apOptRootMotionData _rootMotionData = null;
		public delegate void FUNC_ROOTMOTION_UPDATE(apOptRootUnit rootUnit);





		// Init
		//------------------------------------------------
		void Awake()
		{
			_transform = transform;

			_isFlipped_X = false;
			_isFlipped_Y = false;
			//_isFlipped_X_Prev = false;
			//_isFlipped_Y_Prev = false;
			//_isFlipped = false;

			_isFirstFlippedCheck = true;

			//v1.4.7
			_isFlippedCheckedPrev = false;
			_isFlipped_X_Prev = false;
			_isFlipped_Y_Prev = false;
		}

		void Start()
		{
			this.enabled = false;//<<업데이트를 하진 않습니다.

			_isFirstFlippedCheck = true;

			
		}

		// Update
		//------------------------------------------------
		void Update()
		{

		}

		void LateUpdate()
		{

		}


		// Bake
		//-----------------------------------------------
		public void ClearChildLinks()
		{
			if(_optBones == null)
			{
				_optBones = new List<apOptBone>();
			}
			_optBones.Clear();

			if(_optTransforms == null)
			{
				_optTransforms = new List<apOptTransform>();
			}
			_optTransforms.Clear();
		}

		public void AddChildBone(apOptBone bone)
		{
			_optBones.Add(bone);
		}

		public void AddChildTransform(apOptTransform optTransform, apSortedRenderBuffer.BufferData srcBufferData)//파라미터 추가
		{
			_optTransforms.Add(optTransform);
			_sortedRenderBuffer.Bake_AddTransform(optTransform, srcBufferData);//<< 부분 추가 12.6
		}

		//추가 12.6
		//Sorted Render Buffer에 대한 Bake
		public void BakeSortedRenderBuffer(apPortrait portrait, apRootUnit srcRootUnit)
		{
			if(_sortedRenderBuffer == null)
			{
				_sortedRenderBuffer = new apOptSortedRenderBuffer();
			}
			_sortedRenderBuffer.Bake_Init(portrait, this, srcRootUnit);
		}

		public void BakeComplete()
		{
			_sortedRenderBuffer.Bake_Complete();
		}

		
		// Link
		//------------------------------------------------
		public void Link(apPortrait portrait)
		{
			_sortedRenderBuffer.Link(portrait, this);

			for (int i = 0; i < _optTransforms.Count; i++)
			{
				_optTransforms[i].SetExtraDepthChangedEvent(OnExtraDepthChanged);
			}
			
			//v1.4.8
			//루트 모션용 본도 체크하자
			_linkedRootMotionBone = null;
			if(_rootMotionBoneID >= 0)
			{
				if(_rootOptTransform != null)
				{
					_linkedRootMotionBone = _rootOptTransform.GetBone(_rootMotionBoneID);
				}
			}

			//v1.4.8 루트 모션 초기화
			if(_rootMotionData == null) { _rootMotionData = new apOptRootMotionData(); }
			_rootMotionData.ResetData();
		}

		//추가 19.5.28 : Async용으로 다시 작성된 함수
		public IEnumerator LinkAsync(apPortrait portrait, apAsyncTimer asyncTimer)
		{
			yield return _sortedRenderBuffer.LinkAsync(portrait, this, asyncTimer);

			for (int i = 0; i < _optTransforms.Count; i++)
			{
				_optTransforms[i].SetExtraDepthChangedEvent(OnExtraDepthChanged);
			}

			//v1.4.8
			//루트 모션용 본도 체크하자
			_linkedRootMotionBone = null;
			if(_rootMotionBoneID >= 0)
			{
				if(_rootOptTransform != null)
				{
					_linkedRootMotionBone = _rootOptTransform.GetBone(_rootMotionBoneID);
				}
			}

			//v1.4.8 루트 모션 초기화
			if(_rootMotionData == null) { _rootMotionData = new apOptRootMotionData(); }
			_rootMotionData.ResetData();

			if(asyncTimer.IsYield())
			{
				yield return asyncTimer.WaitAndRestart();
			}
		}

		// Functions
		//------------------------------------------------
		public void UpdateTransforms(	float tDelta, 
										bool isMeshRefreshFrame,//추가 v1.4.7 : 메시가 간헐적으로 갱신될 수 있기 때문에 실제 갱신 프레임을 알려준다.
										FUNC_ROOTMOTION_UPDATE rootMotionEvent)
		{
			if (_rootOptTransform == null)
			{
				return;
			}

			//추가 : Flipped 체크
			bool isFlipChanged = CheckFlippedTransform();

			//v1.4.7
			if(isFlipChanged)
			{
				//이번 프레임에 Flip 여부가 변경되었다면
				//무조건 메시를 갱신하자
				isMeshRefreshFrame = true;
			}


			//추가 v1.4.8
			//루트 모션이 필요한 경우 업데이트하기
			if(rootMotionEvent != null)
			{
				_rootMotionData.ReadyToUpdate();
			}

			//---------------------------------------------------------

			//추가 12.6
			//Sorted Buffer 업데이트 준비
			_sortedRenderBuffer.ReadyToUpdate();

			//본 업데이트 1단계
			_rootOptTransform.ReadyToUpdateBones();

			//---------------------------------------------------------

			//1. Modifer부터 업데이트 (Pre)
			_rootOptTransform.UpdateModifier_Pre(tDelta);

			//---------------------------------------------------------

			//2. 모디파이어값 적용 (Pre)
			_rootOptTransform.ReadyToUpdate();
			_rootOptTransform.UpdateCalculate_Pre();


			//Extra-Depth Changed 이벤트 있을 경우 처리 - Pre에서 다 계산되었을 것이다.
			_sortedRenderBuffer.UpdateDepthChangedEventAndBuffers();

			//---------------------------------------------------------
			//3. Bone World Matrix 업데이트
			//인자로는 지글본에 적용될 물리 값(시간과 여부)을 넣는다.
			_rootOptTransform.UpdateBonesWorldMatrix(	_portrait.PhysicsDeltaTime, 
														_portrait._isImportant && _portrait._isPhysicsPlay_Opt,
														_portrait._isCurrentTeleporting || _portrait._isCurrentRootUnitChanged//추가 22.7.7 + 추가 v1.4.7
													);

			//------------------------------------------------------------

			//4. Modifier 업데이트 (Post)
			_rootOptTransform.UpdateModifier_Post(tDelta);

			//5. 모디파이어값 적용 (Post)
			_rootOptTransform.UpdateCalculate_Post();//Post Calculate



			//6. 루트 모션
			if(rootMotionEvent != null)
			{
				_rootMotionData.CalculateRootMotion();//루트모션 이동 계산
				rootMotionEvent(this);
			}



			//7. 메시 갱신
			if(isMeshRefreshFrame)
			{
				//현재 메시가 갱신되는 프레임이라면 (기본)
				//추가 20.4.2 : UpdateCalculate_Post() 함수의 일부 코드가 뒤로 빠졌다.
				_rootOptTransform.UpdateMeshes();
			}
			else
			{
				//메시는 갱신되지 않는 프레임이라면, 마스크 메시만 갱신하자
				_rootOptTransform.UpdateMaskMeshes();
			}
			


		}




		/// <summary>
		/// UpdateTransform의 Bake버전
		/// Bone 관련 부분 처리가 조금 다르다.
		/// </summary>
		/// <param name="tDelta"></param>
		public void UpdateTransformsForBake(float tDelta)
		{
			if (_rootOptTransform == null)
			{
				return;
			}
			
			//추가
			//본 업데이트 1단계
			_rootOptTransform.ReadyToUpdateBones();

			//1. Modifer부터 업데이트 (Pre)
			_rootOptTransform.UpdateModifier_Pre(tDelta);
			//---------------------------------------------------------

			_rootOptTransform.ReadyToUpdateBones();

			//2. 실제로 업데이트
			_rootOptTransform.ReadyToUpdate();
			_rootOptTransform.UpdateCalculate_Pre();//Post 작성할 것


			//---------------------------------------------------------


			//Bone World Matrix Update
			_rootOptTransform.UpdateBonesWorldMatrix(0.0f, true, false);
			//_rootOptTransform.UpdateBonesWorldMatrixForBake();//<<이게 다르다


			//------------------------------------------------------------


			//Modifier 업데이트 (Post)
			_rootOptTransform.UpdateModifier_Post(tDelta);

			_rootOptTransform.UpdateCalculate_Post();//Post Calculate

			//추가 20.4.2 : UpdateCalculate_Post() 함수의 일부 코드가 뒤로 빠졌다.
			_rootOptTransform.UpdateMeshes();

		}



		public void UpdateTransformsOnlyMaskMesh()
		{
			if (_rootOptTransform == null)
			{
				return;
			}
			_rootOptTransform.UpdateMaskMeshes();
		}







		/// <summary>
		/// Sync된 Child Portrait의 업데이트 함수. 기존 UpdateTransforms와 조금 다르다.
		/// </summary>
		/// <param name="tDelta"></param>
		/// <param name="isSyncBones"></param>
		public void UpdateTransformsAsSyncChild(float tDelta, bool isSyncBones, bool isMeshRefreshFrame, FUNC_ROOTMOTION_UPDATE rootMotionEvent)
		{
			if (_rootOptTransform == null)
			{
				return;
			}

			//추가 : Flipped 체크
			bool isFlipChanged = CheckFlippedTransform();

			//v1.4.7
			if(isFlipChanged)
			{
				//이번 프레임에 Flip 여부가 변경되었다면
				//무조건 메시를 갱신하자
				isMeshRefreshFrame = true;
			}


			//Sync Child의 업데이트에서는 일부 코드가 바뀐다.

			//추가 v1.4.8
			//루트 모션이 필요한 경우 업데이트하기
			if(rootMotionEvent != null)
			{
				_rootMotionData.ReadyToUpdate();
			}

			//---------------------------------------------------------
			//Sorted Buffer 업데이트 준비
			_sortedRenderBuffer.ReadyToUpdate();

			//본 업데이트 1단계
			_rootOptTransform.ReadyToUpdateBones();
			
			//---------------------------------------------------------

			//1. Modifer부터 업데이트 (Pre)
			_rootOptTransform.UpdateModifier_Pre(tDelta);

			//---------------------------------------------------------

			//2. 실제로 업데이트
			_rootOptTransform.ReadyToUpdate();
			_rootOptTransform.UpdateCalculate_Pre();//Post 작성할 것

			//추가 12.6
			//Extra-Depth Changed 이벤트 있을 경우 처리 - Pre에서 다 계산되었을 것이다.
			_sortedRenderBuffer.UpdateDepthChangedEventAndBuffers();

			//---------------------------------------------------------

			//Bone World Matrix Update
			//인자로는 지글본에 적용될 물리 값(시간과 여부)을 넣는다.
			if(isSyncBones)
			{
				//<중요> 본이 동기화된 경우
				_rootOptTransform.UpdateBonesWorldMatrixAsSyncBones(
														_portrait.PhysicsDeltaTime, 
														_portrait._isImportant && _portrait._isPhysicsPlay_Opt,
														_portrait._isCurrentTeleporting || _portrait._isCurrentRootUnitChanged//추가 22.7.7 + 추가 v1.4.7
														);
			}
			else
			{
				//일반
				_rootOptTransform.UpdateBonesWorldMatrix(	_portrait.PhysicsDeltaTime, 
															_portrait._isImportant && _portrait._isPhysicsPlay_Opt,
															_portrait._isCurrentTeleporting || _portrait._isCurrentRootUnitChanged//추가 22.7.7 + 추가 v1.4.7
															);
			}
			

			//------------------------------------------------------------

			//Modifier 업데이트 (Post)
			_rootOptTransform.UpdateModifier_Post(tDelta);

			//중요 : Sync용 함수를 이용하자
			//_rootOptTransform.UpdateCalculate_Post();//Post Calculate
			_rootOptTransform.UpdateCalculate_Post_AsSyncChild();//[Sync]



			//루트 모션
			if(rootMotionEvent != null)
			{
				_rootMotionData.CalculateRootMotion();
				rootMotionEvent(this);
			}

			//추가 20.4.2 : UpdateCalculate_Post() 함수의 일부 코드가 뒤로 빠졌다.
			//추가 v1.4.7 : 메시 갱신이 간헐적으로 될 수 있다.

			if(isMeshRefreshFrame)
			{
				//현재 메시가 갱신되는 프레임이라면 (기본)
				//추가 20.4.2 : UpdateCalculate_Post() 함수의 일부 코드가 뒤로 빠졌다.
				_rootOptTransform.UpdateMeshes();
			}
			else
			{
				//메시는 갱신되지 않는 프레임이라면, 마스크 메시만 갱신하자
				_rootOptTransform.UpdateMaskMeshes();
			}
		}










		public void Show()
		{
			if (_rootOptTransform == null)
			{
				return;
			}

			_rootOptTransform.Show(true);

			_isVisible = true;//추가 21.4.3


			//v1.4.7 : Flip 체크 여부 초기화
			_isFlippedCheckedPrev = false;

			//v1.4.8 : 루트 모션 데이터
			if(_rootMotionData == null) { _rootMotionData = new apOptRootMotionData(); }
			_rootMotionData.ResetData();//기존까지의 정보는 리셋
		}

		public void ShowWhenBake()
		{
			if (_rootOptTransform == null)
			{
				return;
			}

			_rootOptTransform.ShowWhenBake(true);

			_isVisible = true;//추가 21.4.3

			//v1.4.7 : Flip 체크 여부 초기화
			_isFlippedCheckedPrev = false;

			if(_rootMotionData == null) { _rootMotionData = new apOptRootMotionData(); }
			_rootMotionData.ResetData();//기존까지의 정보는 리셋
		}



		public void Hide()
		{
			if (_rootOptTransform == null)
			{
				return;
			}

			_rootOptTransform.Hide(true);
			
			_isVisible = false;//추가 21.4.3

			//v1.4.7 : Flip 체크 여부 초기화
			_isFlippedCheckedPrev = false;

			if(_rootMotionData == null) { _rootMotionData = new apOptRootMotionData(); }
			_rootMotionData.ResetData();//기존까지의 정보는 리셋

		}

		public void ResetCommandBuffer(bool isRegistToCamera)
		{
			if (_rootOptTransform == null)
			{
				return;
			}

			_rootOptTransform.ResetCommandBuffer(isRegistToCamera);
		}


		// 추가 12.7 : Extra Option 관련 처리
		//------------------------------------------------------------------------------------
		private void OnExtraDepthChanged(apOptTransform optTransform, int deltaDepth)
		{
			if(deltaDepth == 0)
			{
				return;
			}

			_sortedRenderBuffer.OnExtraDepthChanged(optTransform, deltaDepth);
		}


		// 추가 2.25 : Flipped 관련 처리
		//-------------------------------------------------------------

		//v1.4.7 변경 : 이전 프레임과 비교하여 Flip 여부가 변경되었다면 true 리턴
		private bool CheckFlippedTransform()
		{
			if (_isFirstFlippedCheck)
			{
				_transform = transform;
			}

			_isFlipped_X = _portrait._transform.lossyScale.x < 0.0f;
			_isFlipped_Y = _portrait._transform.lossyScale.y < 0.0f;


			bool isChanged = false;
			if (!_isFlippedCheckedPrev || _isFirstFlippedCheck)
			{
				//이전에 비교를 한 적이 없다면
				//무조건 변화된 것으로 체크
				isChanged = true;
			}
			else
			{
				//이전 프레임과 비교해서 Flip 값이 변경되었다면
				if(_isFlipped_X != _isFlipped_X_Prev
					|| _isFlipped_Y != _isFlipped_Y_Prev)
				{
					//값 비교후 변화를 체크
					isChanged = true;
				}

			}

			_isFlippedCheckedPrev = true;//계산이 되었고
			_isFirstFlippedCheck = false;//IsFirst는 지났다.

			//Prev에 저장
			_isFlipped_X_Prev = _isFlipped_X;
			_isFlipped_Y_Prev = _isFlipped_Y;

			return isChanged;
		}
		
		public bool IsFlippedX { get { return _isFlipped_X; } }
		public bool IsFlippedY { get { return _isFlipped_Y; } }


		// 추가 19.8.19 : SortingOption 관련
		//---------------------------------------------------
		public void SetSortingOrderChangedAutomatically(bool isEnabled)
		{
			if(_sortedRenderBuffer == null)
			{
				return;
			}
			_sortedRenderBuffer.SetSortingOrderChangedAutomatically(isEnabled);
		}

		public bool RefreshSortingOrderByDepth()
		{
			if(_sortedRenderBuffer == null)
			{
				return false;
			}
			_sortedRenderBuffer.Link(_portrait, this);

			return _sortedRenderBuffer.RefreshSortingOrderByDepth();
		}

		public void SetSortingOrderOption(apPortrait.SORTING_ORDER_OPTION sortingOrderOption, int sortingOrderPerDepth)
		{
			if(_sortedRenderBuffer == null)
			{
				return;
			}
			_sortedRenderBuffer.SetSortingOrderOption(sortingOrderOption, sortingOrderPerDepth);
		}



		// 추가 v1.4.8 : 루트 모션
		//-------------------------------------------------
		public void AddRootMotionData(apAnimClip animClip, ref Vector2 modDeltaPos, float weight)
		{
			if(_rootMotionData == null || animClip == null)
			{
				return;
			}

			//루트 모션에 데이터를 넣는다.
			_rootMotionData.AddAnimModInfo(animClip, ref modDeltaPos, weight);
		}

		public Vector2 GetRootMotionDeltaPos()
		{
			return _rootMotionData.ResultDeltaPos;
		}

		//애니메이션 시작시, 루트 모션에 의한 움직임 정보를 한번 리셋해야한다.
		public void OnAnimStartAndRootMotionDataReset(apAnimClip animClip)
		{
			if(_rootMotionData == null
				|| animClip == null
				|| _portrait._rootMotionValidatedMode != apPortrait.ROOT_MOTION_MODE.MoveParentTransform)
			{
				return;
			}

			_rootMotionData.ResetAnimModInfo(animClip);
		}

		// Get / Set
		//------------------------------------------------
		public apOptBone GetBone(string name)
		{
			//일단 빠른 검색부터
			if(_optBonesMap.ContainsKey(name))
			{
				return _optBonesMap[name];
			}

			//이전 (GC 발생)
			// apOptBone resultBone = _optBones.Find(delegate (apOptBone a)
			// {
			// 	return string.Equals(a._name, name);
			// });

			//변경 v1.5.0
			s_GetOptBone_Name = name;
			apOptBone resultBone = _optBones.Find(s_GetOptBoneByName_Func);


			if(resultBone == null)
			{
				return null;
			}

			//빠른 검색 리스트에 넣고
			_optBonesMap.Add(name, resultBone);

			return resultBone;
		}

		private static string s_GetOptBone_Name = null;
		private static Predicate<apOptBone> s_GetOptBoneByName_Func = FUNC_GetOptBoneByName;
		private static bool FUNC_GetOptBoneByName(apOptBone a)
		{
			return string.Equals(a._name, s_GetOptBone_Name);
		}



		public apOptTransform GetTransform(string name)
		{
			//일단 빠른 검색부터 (Link에서 하지 않고 여기서 등록)
			apOptTransform resultTransform = null;
			_optTransformMap.TryGetValue(name, out resultTransform);

			if(resultTransform != null)
			{
				return resultTransform;
			}
			
			//처음엔 직접 찾기
			//이전 (GC 발생)
			// resultTransform = _optTransforms.Find(delegate (apOptTransform a)
			// {
			// 	return string.Equals(a._name, name);
			// });

			//변경 v1.5.0
			s_GetOptTransform_Name = name;
			resultTransform = _optTransforms.Find(s_GetOptTransformByName_Func);


			if(resultTransform == null)
			{
				return null;
			}

			//빠른 검색 리스트에 넣고
			_optTransformMap.Add(name, resultTransform);

			return resultTransform;
		}


		private static string s_GetOptTransform_Name = null;
		private static Predicate<apOptTransform> s_GetOptTransformByName_Func = FUNC_GetOptTransformByName;
		private static bool FUNC_GetOptTransformByName(apOptTransform a)
		{
			return string.Equals(a._name, s_GetOptTransform_Name);
		}





		
		public apOptTransform GetTransform(int transformID)
		{
			//이전 (GC 발생)
			// return _optTransforms.Find(delegate(apOptTransform a)
			// {
			// 	return a._transformID == transformID;
			// });

			//변경 (v1.5.0)
			s_GetOptTransform_ID = transformID;
			return _optTransforms.Find(s_GetOptTransformByID_Func);
		}

		private static int s_GetOptTransform_ID = -1;
		private static Predicate<apOptTransform> s_GetOptTransformByID_Func = FUNC_GetOptTransformByID;
		private static bool FUNC_GetOptTransformByID(apOptTransform a)
		{
			return a._transformID == s_GetOptTransform_ID;
		}

		//추가 21.9.18
		public List<apOptBone> GetRootBones()
		{
			//이전 (GC 발생)
			// return _optBones.FindAll(delegate(apOptBone a)
			// {
			// 	return a._parentBone == null;
			// });

			//변경 v1.5.0
			return _optBones.FindAll(s_GetRootBone_Func);
		}

		private static Predicate<apOptBone> s_GetRootBone_Func = FUNC_GetRootBone;
		private static bool FUNC_GetRootBone(apOptBone a)
		{
			return a._parentBone == null;
		}
	}

}
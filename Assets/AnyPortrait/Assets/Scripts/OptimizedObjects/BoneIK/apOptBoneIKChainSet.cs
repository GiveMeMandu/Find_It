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
	/// apBoneIKChainSet의 Opt버전
	/// Serialize가 안되므로 초기화시 Link를 해야한다.
	/// apBoneIKChainSet을 복붙하여 Opt만 추가하자
	/// </summary>
	[Serializable]
	public class apOptBoneIKChainSet
	{
		// Members
		//------------------------------------------------------------
		[SerializeField]
		public apOptBone _bone = null;

		/// <summary>
		/// Tail -> Head로 이어지는 ChainUnit
		/// IK 특성상 이 ChainSet을 소유하고 있는 Bone이 베이스가 되는 ChainUnit은 없다.
		/// </summary>
		[SerializeField] public List<apOptBoneIKChainUnit> _chainUnits = new List<apOptBoneIKChainUnit>();


		//Head/Tail의 체인들.
		//기존과 달리 NonSerialized이다.
		[NonSerialized] private apOptBoneIKChainUnit _headChainUnit = null;
		[NonSerialized] private apOptBoneIKChainUnit _tailChainUnit = null;

		public apOptBone GetHeadBone()
		{
			//초기화 타이밍상 headChainUnit이 없을 수 있다.			
			apOptBoneIKChainUnit headUnit = _headChainUnit;
			if(headUnit == null)
			{
				int nChainUnits = _chainUnits != null ? _chainUnits.Count : 0;
				if(nChainUnits > 0)
				{
					//자식 > 부모 순서의 리스트이므로, 마지막 항목이 부모이다.
					headUnit = _chainUnits[nChainUnits - 1];
				}
			}

			return headUnit != null ? headUnit._baseBone : null;
		}

		[NonSerialized]
		public Vector2 _requestedTargetPosW = Vector2.zero;

		[NonSerialized]
		public Vector2 _requestedBonePosW = Vector2.zero;


		/// <summary>
		/// 계산을 반복하는 Loop의 최대 횟수
		/// </summary>
		public const int MAX_CALCULATE_LOOP_EDITOR = 30;
		public const int MAX_CALCULATE_LOOP_RUNTIME = 20;
		public const int MAX_TOTAL_UNIT_CALCULATE = 100;

		public const float CONTINUOUS_TARGET_POS_BIAS = 5.0f;
		public const float CONTINUOUS_ANGLE_JUMP_LIMIT = 30.0f;

		//FABRIK에서의 루프 횟수 (런타임 - 조금 빠르다.)
		public const int FABRIK_LOOPS_RUNTIME__MAX = 50;
		public const int FABRIK_LOOPS_RUNTIME__ACONST_PREFER = 3;
		public const int FABRIK_LOOPS_RUNTIME__ACONST_WEIGHT01 = 5;
		public const int FABRIK_LOOPS_RUNTIME__ACONST_WEIGHT02 = 10;
		public const int FABRIK_LOOPS_RUNTIME__ACONST_WEIGHT10 = 12;
		public const int FABRIK_LOOPS_RUNTIME__ACONST_SOFTLIMIT_SHRINK = 7;
		public const int FABRIK_LOOPS_RUNTIME__ACONST_SOFTLIMIT_EXPAND = 12;
		public const int FABRIK_LOOPS_RUNTIME__STOPPABLE = 15;
		public const int FABRIK_LOOPS_RUNTIME__STOPPABLE_NOCHANGED = 25;
		public const int FABRIK_LOOPS_RUNTIME__ROTATION = 10;

		/// <summary>
		/// 연산이 종료되는 거리값 오차 허용값
		/// </summary>
		public const float BIAS_TARGET_POS_MATCH = 0.1f * 0.1f;//sqr Distance 값이다.


		private int _nLoop = 0;

		private Vector2 _prevTargetPosW = Vector2.zero;
		private bool _isContinuousPrevPos = false;

		public Vector2 _tailBoneNextPosW = Vector2.zero;


		private int _nUnits = 0;
		private float _totalChainLength = 0.0f;

		
		// Init
		//------------------------------------------------------------
		public apOptBoneIKChainSet(apOptBone bone)
		{
			_bone = bone;
			_nLoop = 0;

			_nUnits = 0;
			_totalChainLength = 0.0f;
		}

		// Functions
		//------------------------------------------------------------
		/// <summary>
		/// Bone Hierarchy에 맞추어서 다시 Chain을 만든다.
		/// </summary>
		public void RefreshChain()
		{
			if(_chainUnits == null)
			{
				_chainUnits = new List<apOptBoneIKChainUnit>();
			}
			_chainUnits.Clear();
			_headChainUnit = null;
			_tailChainUnit = null;

			_nUnits = 0;
			_totalChainLength = 0.0f;

			if (!_bone._isIKTail)
			{
				return;
			}


			//Bone으로부터 Head가 나올때까지 Chain을 추가하자
			//[Parent] .... [Cur Bone] ----> [Target Bone < 이것 부터 시작]
			apOptBone parentBone = null;
			apOptBone curBone = null;
			apOptBone targetBone = _bone;

			//int curLevel = 0;
			while (true)
			{
				curBone = targetBone._parentBone;
				if (curBone == null)
				{
					//? 왜 여기서 끊기지..
					break;
				}

				parentBone = curBone._parentBone;//<<이건 Null 일 수 있다.

				//apBoneIKChainUnit newUnit = new apBoneIKChainUnit(curBone, targetBone, parentBone, curLevel);
				apOptBoneIKChainUnit newUnit = new apOptBoneIKChainUnit(curBone, targetBone, parentBone);

				_chainUnits.Add(newUnit);

				//끝났당
				if (curBone == _bone._IKHeaderBone)
				{
					break;
				}

				//하나씩 위로 탐색하자
				targetBone = curBone;
				//curLevel++;
			}

			int nChainUnits = _chainUnits.Count;
			if (nChainUnits == 0)
			{
				return;
			}
			
			//앞쪽이 Tail이다.
			_tailChainUnit = _chainUnits[0];
			_headChainUnit = _chainUnits[nChainUnits - 1];
			_nUnits = _chainUnits.Count;

			//Chain Unit간의 연결을 한다.
			apOptBoneIKChainUnit curUnit = null;
			for (int i = 0; i < nChainUnits; i++)
			{
				curUnit = _chainUnits[i];

				if (i > 0)
				{
					curUnit.SetChild(_chainUnits[i - 1]);
				}

				if (i < _chainUnits.Count - 1)
				{
					curUnit.SetParent(_chainUnits[i + 1]);
				}

				curUnit.SetTail(_tailChainUnit);

				//Bone Length를 더한다.
				_totalChainLength += curUnit._lengthBoneToTarget;
			}

			if (_nUnits == 0)
			{
				_nLoop = 0;
			}
			else
			{
				// 얼마나 연산을 반복할 것인지 결정 (연산 횟수는 루프 단위로 결정한다)
				_nLoop = MAX_CALCULATE_LOOP_EDITOR;
				if (_nUnits * _nLoop > MAX_TOTAL_UNIT_CALCULATE)
				{
					//전체 계산 횟수 (Unit * Loop)가 제한을 넘겼을 때
					_nLoop = MAX_TOTAL_UNIT_CALCULATE / _nUnits;
					if (_nLoop < 2)
					{
						_nLoop = 2;
					}
				}
			}

			_isContinuousPrevPos = false;
		}



		/// <summary>
		/// v1.5.0 : IK 초기 포즈를 연산하기 위한 플래그를 초기화한다.
		/// </summary>
		public void ResetIKInitPoseFlag()
		{
			if(_headChainUnit == null)
			{
				return;
			}
			_headChainUnit.ResetIKInitPoseFlagRecursive();
		}


		/// <summary>
		/// IK를 시뮬레이션한다.
		/// 요청한 Bone을 Tail로 하여 Head까지 처리한다.
		/// 결과값은 Delta Angle로 나오며, 이 값을 참조하여 결정한다. (Matrix 중 어디에 쓸지는 외부에서 결정)
		/// </summary>
		/// <param name="targetPosW"></param>
		public bool SimulateIK_CCD(Vector2 targetPosW, bool isContinuous)
		{
			if (!_bone._isIKTail)
			{
				//Debug.LogError("Failed 1 - _bone._isIKTail : " + _bone._isIKTail);
				return false;
			}

			if (_chainUnits.Count == 0)
			{
				//Debug.LogError("Failed 2 - _chainUnits.Count : " + _chainUnits.Count);
				return false;
			}

			apOptBoneIKChainUnit chainUnit = null;

			//[Tail] .....[] .... [Head]
			//Tail에 가까운(인덱스가 가장 작은) Constraint가 적용된 Bone을 구한다.
			//Head에 가까운(인덱스가 가장 큰) Constraint가 적용된 Bone을 구한다.

			float lengthTotal = 0.0f;

			//1. Simulate 준비
			//Debug.Log("ReadyToSimulate : " + _chainUnits.Count);
			for (int i = 0; i < _chainUnits.Count; i++)
			{
				chainUnit = _chainUnits[i];
				chainUnit.ReadyToSimulate();

				lengthTotal += chainUnit._lengthBoneToTarget;
			}

			if(_tailChainUnit == null)
			{
				_tailChainUnit = _chainUnits[0];
			}
			if(_headChainUnit == null)
			{
				_headChainUnit = _chainUnits[_chainUnits.Count - 1];
			}

			//1. 길이 확인 후 압축을 해야하는지 적용
			float length2Target = (targetPosW - _headChainUnit._bonePosW).magnitude;
			float length2Tail = (_tailChainUnit._targetPosW - _headChainUnit._bonePosW).magnitude;

			if (length2Tail == 0.0f)
			{
				return false;
			}

			float beforSqrDist = (targetPosW - _tailChainUnit._bonePosW).sqrMagnitude;

			apOptBoneIKChainUnit curBoneUnit = null;

			if (length2Target < lengthTotal)
			{
				//압축을 해야한다.
				//float compressRatio = Mathf.Clamp01(length2Target / lengthTotal);//<<이거 사용하지 않는다.

				for (int i = 0; i < _chainUnits.Count; i++)
				{
					curBoneUnit = _chainUnits[i];
					if (curBoneUnit._isAngleContraint)
					{
						//이전
						//curBoneUnit._angleLocal_Next = curBoneUnit._angleDir_Preferred * (1.0f - compressRatio) + curBoneUnit._angleLocal_Next + compressRatio;

						//변경 : 바로 Preferred를 사용한다.
						//curBoneUnit._angleLocal_Next = curBoneUnit._angleDir_Preferred;

						//변경 20.10.9 : _angleDir_Preferred이 _angleLocal_Next와 360가까이 차이나는걸 막기 위함
						curBoneUnit._angleLocal_Next = apUtil.GetNearestLoopedAngle360(curBoneUnit._angleDir_Preferred, curBoneUnit._angleLocal_Next);

						//Preferred를 적용했다는 것을 알려주자
						curBoneUnit._isPreferredAngleAdapted = true;
					}
				}

				_headChainUnit.CalculateWorldRecursive();
			}
			else if (length2Target > lengthTotal + 1.0f)//Bias 추가해서 플래그 리셋
			{
				for (int i = 0; i < _chainUnits.Count; i++)
				{
					_chainUnits[i]._isPreferredAngleAdapted = false;
				}
			}

			curBoneUnit = null;
			int nCalculate = 1;
			int curIndex = 0;

			for (int i = 0; i < _nLoop; i++)
			{
				curBoneUnit = _tailChainUnit;

				while (true)
				{
					//루프를 돕시다.
					if(i == 0)
					{
						//가중치가 포함된 것으로 돈다.
						curBoneUnit.RequestIK_Weighted(targetPosW, isContinuous, Mathf.Clamp01((float)(curIndex + 1) / (float)_chainUnits.Count) * 0.7f);
						//curBoneUnit.RequestIK(targetPosW, isContinuous);//구형
						curIndex++;
					}
					else
					{
						curBoneUnit.RequestIK(targetPosW, isContinuous);
					}
					

					curBoneUnit.CalculateWorldRecursive();

					if (curBoneUnit._parentChainUnit != null)
					{
						curBoneUnit = curBoneUnit._parentChainUnit;
					}
					else
					{
						break;
					}
				}

				//마지막으로 Tail에서 처리 한번더
				curBoneUnit = _tailChainUnit;
				//curBoneUnit.RequestIK(targetPosW, i, _nLoop);
				curBoneUnit.RequestIK(targetPosW, isContinuous);
				curBoneUnit.CalculateWorldRecursive();

				nCalculate++;
			}


			//만약 Continuous 모드에서 각도가 너무 많이 차이가 나면 실패한 처리다.
			//이전 요청 좌표와 거리가 적은 경우 유효

			if (isContinuous)
			{
				if (_isContinuousPrevPos)
				{
					float distTargetDelta = Vector2.Distance(_prevTargetPosW, targetPosW);
					if (distTargetDelta < CONTINUOUS_TARGET_POS_BIAS)
					{

						//연속된 위치 입력인 경우
						//전체의 각도 크기를 구하자
						float totalDeltaAngle = 0.0f;
						for (int i = 0; i < _chainUnits.Count; i++)
						{
							totalDeltaAngle += Mathf.Abs(_chainUnits[i]._angleLocal_Delta);
						}
						//Debug.Log("Cont Move : " + distTargetDelta + " / Delta Angle : " + totalDeltaAngle);
						if (totalDeltaAngle > CONTINUOUS_ANGLE_JUMP_LIMIT)
						{
							//너무 많이 움직였다.
							_isContinuousPrevPos = true;
							_prevTargetPosW = targetPosW;
							//Debug.LogError("Angle Jump Error : Total Angle : " + totalDeltaAngle + " / Delta Target : " + distTargetDelta);
							return false;
						}
					}
				}
				_isContinuousPrevPos = true;
				_prevTargetPosW = targetPosW;
			}
			else
			{
				_isContinuousPrevPos = false;
			}

			if (isContinuous && length2Target < lengthTotal)
			{
				float afterSqrdist = (_tailChainUnit._targetPosW - targetPosW).sqrMagnitude;
				if (beforSqrDist * 1.2f < afterSqrdist)
				{
					//오히려 더 멀어졌다.
					//Debug.LogError("다시 멀어졌다");
					//Debug.LogError("Failed 4 - length2Target < lengthTotal : " + length2Target + " < " + lengthTotal);
					return false;
				}
			}

			_requestedTargetPosW = _tailChainUnit._targetPosW;
			_requestedBonePosW = _tailChainUnit._bonePosW;

			return true;
		}


		//[v1.5.0] FABRIK 방식
		
		/// <summary>
		/// IK를 "FABRIK" 방식으로 시뮬레이션한다.
		/// 요청한 Bone을 Tail로 하여 Head까지 처리한다.
		/// 결과값은 Delta Angle로 나오며, 이 값을 외부에서 참조해서 사용한다.
		/// </summary>
		/// <returns>실패시 false</returns>
		public bool SimulateIK_FABRIK(Vector2 targetPosW, bool isContinuous, float IKWeight)
		{	
			if (!_bone._isIKTail)
			{
				//FABRIK 실패 : Tail 아님
				return false;
			}

			if (_nUnits == 0)
			{
				//FABRIK 실패 : 유닛 없음
				return false;
			}

			apOptBoneIKChainUnit chainUnit = null;
			
			//리스트는 Tail > Head 순서로 배치되어 있다.
			//[0 : Tail] .....[] .... [n-1 : Head]

			//1. Simulate 준비
			for (int i = 0; i < _nUnits; i++)
			{
				chainUnit = _chainUnits[i];
				chainUnit.ReadyToSimulate();
			}

			//FABRIK의 경우,
			//- Forward, Backward의 스탭 2개를 하나의 세트로 묶어서 연산한다.
			//- 최대 연산 횟수를 넘었거나, 위치가 동일하면 성공
			//- 연산 횟수는 Forward-Backward를 1개의 Loop로 본다.
			//- Forward+Backward 1회 순회 후 End가 Target보다 더 멀어졌다면 거기서 중단한다.
			//- Constraint는 Backward에서 수행한다.
			

			//2. Head의 IK 직전의 시작 위치를 알아야 한다.
			Vector2 headStartPosW = _headChainUnit._bonePosW;

			//옵션에 따라 Prefer나 특정 상태로 회전시킨 후, CCD처럼 회전을 한다. > CCD 회전은 안하는 걸로
			float curIKPoseWeight = 0.0f;

			for (int iBone = _nUnits - 1; iBone >= 0; iBone--)//[부모 > 자식 순서]
			{
				chainUnit = _chainUnits[iBone];

				//이전의 연산으로부터 "Init Pose"가 연산된 만큼, 초기 자세에 대한 IK Weight를 줄여야 한다.
				//기본은 1.0
				curIKPoseWeight = Mathf.Clamp01(1.0f - chainUnit._baseBone._IKInitPoseWeightSum);
					
				//Weight를 여기서 소모한다.
				chainUnit._baseBone._IKInitPoseWeightSum += IKWeight;

				if(curIKPoseWeight < 0.00001f)
				{
					//남은 Weight가 없다.
					continue;
				}

				if(!chainUnit._isAngleContraint)
				{
					continue;
				}

				//초기 타입에 따라 Prefer / 현재값 유지 / 평균 중에서 결정한다.
				bool isPoseChanged = false;
				switch(chainUnit._initPose)
				{
					case apBone.IK_START_POSE.PreferAngle:
						{
							//Prefer Angle을 그대로 사용한다.
							chainUnit._angleLocal_Next = apUtil.AngleSlerp(chainUnit._angleLocal_Prev, chainUnit._angleDir_Preferred, curIKPoseWeight);//Weight만큼 회전
							chainUnit._angleLocal_Delta = apUtil.AngleTo360(chainUnit._angleLocal_Next - chainUnit._angleLocal_Prev);
							chainUnit._angleWorld_Next = chainUnit._angleLocal_Next + chainUnit._angleWorld_Parent;
							isPoseChanged = true;
						}
						break;

					case apBone.IK_START_POSE.KeepCurrent:
						//현재 상태를 유지하여 초기포즈로서 사용한다.
						//FK를 사용하거나 상위 본의 IK 결과를 그대로 사용할 때.
						break;

					case apBone.IK_START_POSE.Average_FK_Prefer:
						{
							//Prefer와 현재 Local 값의 중간을 적용한다.
							float centerAngleL = apUtil.AngleSlerp(chainUnit._angleLocal_Next, chainUnit._angleDir_Preferred, 0.5f);
							chainUnit._angleLocal_Next = apUtil.AngleSlerp(chainUnit._angleLocal_Prev, centerAngleL, curIKPoseWeight);//Weight만큼 회전;
							chainUnit._angleLocal_Delta = apUtil.AngleTo360(chainUnit._angleLocal_Next - chainUnit._angleLocal_Prev);
							chainUnit._angleWorld_Next = chainUnit._angleLocal_Next + chainUnit._angleWorld_Parent;
							isPoseChanged = true;
						}
						break;
				}
				//Prefer를 했을 때의 거리와 현재 거리
					
				if(isPoseChanged)
				{
					//회전한 만큼 각도와 위치를 다시 계산해야한다.
					chainUnit.CalculateWorldRecursive_FABRIK(chainUnit);
				}
			}
			
			//"축소시 처리"를 수행한다.
			bool isShrinked = false;

			//End가 Tail Unit의 Dir의 반대 방향이면 압축(축소) 방향이다.
			Vector2 tailDir = _tailChainUnit._targetPosW - _tailChainUnit._bonePosW;
			Vector2 tailEndToTargetDir = targetPosW - _tailChainUnit._targetPosW;
			
			if(Vector2.Dot(tailDir, tailEndToTargetDir) < 0.0f)
			{	
				isShrinked = true;
			}

			//전체적으로 다시 World를 갱신한다.
			_headChainUnit.CalculateWorldRecursive();			

			// Forward : Tail의 끝을 End에 부착하면서 Parent 방향으로 IK를 수행한다. 결과적으로 End에는 붙지만 Start에서는 떨어짐
			// Backward : Head의 위치를 Start에 부착하면서 Child 방향으로 IK를 수행한다. 결과적으로 Start에는 붙지만 End에서는 떨어짐
			Vector2 chainStartPosW = Vector2.zero;
			Vector2 chainEndPosW = Vector2.zero;
			
			Vector2 curTargetPosW_End = Vector2.zero;//End가 향해야할 곳
			Vector2 curTargetPosW_Start = Vector2.zero;//Start가 향해야할 곳
			float chainLength = 0.0f;

			Vector2 tailEnd = Vector2.zero;
			float distTailEndToTarget = 0.0f;
			float prevDistTailEndToTarget = 0.0f;

			Vector2 nextChainDir_EndToStart = Vector2.zero;
			Vector2 nextChainStartPos = Vector2.zero;
			Vector2 nextChainDir_StartToEnd = Vector2.zero;
			Vector2 nextChainEndPos = Vector2.zero;

			Vector2 constPos_Lower = Vector2.zero;
			Vector2 constPos_Upper = Vector2.zero;

			for (int iLoop = 0; iLoop < FABRIK_LOOPS_RUNTIME__MAX; iLoop++)
			{
				//<0> 종료 조건을 체크한다. Tail 체인 (첫번째 체인)의 End가 이미 도달했다면 종료
				tailEnd = _tailChainUnit._targetPosW;
				distTailEndToTarget = Mathf.Abs(targetPosW.x - tailEnd.x) + Mathf.Abs(targetPosW.y - tailEnd.y);

				if (iLoop > FABRIK_LOOPS_RUNTIME__STOPPABLE)
				{	
					if(distTailEndToTarget < 0.0001f)
					{	
						//Tail End가 목적 위치에 도달했기 때문에 루프를 조기 종료한다.
						break;
					}

					//확장 중에 거리가 벌어지고 특정 Loop를 지났다면 바로 종료 가능
					if(!isShrinked && distTailEndToTarget > 5.0f)
					{	
						//Debug.Log("[" + iLoop + "] 중간 종료 : 확장 중에 거리가 벌어지고 특정 Loop를 지났다면 바로 종료 가능");
						break;
					}
				}

				//F/B 체크 전에 실패를 대비해서 값을 저장한다. > Runtime에서는 이 기능은 동작하지 않는다.
				//_headChainUnit.StoreChainValuesInLoopRecursive();

				//처리 전의 거리를 체크한다.
				prevDistTailEndToTarget = Vector2.Distance(_tailChainUnit._targetPosW, targetPosW);
								
				//<1> Forward를 연산한다.
				//- Tail > Head 방향으로 연산하며, 각 본의 End를 목표에 맞추고 Start를 결정
				//- Forward에서는 각 본의 End 위치를 목표에 위치시키는 것이 목표다.

				//시작은 targetPosW
				curTargetPosW_End = targetPosW;
				
				//Tail > Head는 정방향
				for (int iBone = 0; iBone < _nUnits; iBone++)
				{
					chainUnit = _chainUnits[iBone];

					//현재 체인의 속성을 받자
					chainStartPosW = chainUnit._bonePosW;
					chainEndPosW = chainUnit._targetPosW;
					chainLength = chainUnit._lengthBoneToTarget;

					//End를 부착하고 Start'를 만들기 위해
					//"타겟 End > 체인의 Start"로의 방향을 구하자.
					//End가 목표로 이동, 회전한 후의 체인의 End > Start 방향 (역방향)을 구해서 새로운 Start를 구한다.
					nextChainDir_EndToStart = (chainStartPosW - curTargetPosW_End).normalized;
					nextChainStartPos = curTargetPosW_End + (nextChainDir_EndToStart * chainLength);


					//해당 위치로 체인을 이동시킨다.
					chainUnit._bonePosW = nextChainStartPos;//역으로 추정한 Start 위치 갱신
					chainUnit._targetPosW = curTargetPosW_End;//End는 목표에 바로 부착

					//End 목표를 현재 체인의 Start로 변경한다.
					curTargetPosW_End = chainUnit._bonePosW;

				}

				//<2> Backward를 연산한다.
				//- Head > Tail 방향으로 연산하며, 각 본의 Start를 목표에 맞추고 End를 결정한다.
				//- Backward에서는 Head의 Start 위치를 원래 위치로 보정하는 것이 목표다.
				//- Constraint 연산도 할 겸 Backward 단계에서 각도 계산도 한다.

				//시작은 Head의 원래 Start 위치
				curTargetPosW_Start = headStartPosW;

				//<각도 계산>
				//- Head 부터 계산을 한다.
				//- 원래 "CalculateWorldRecursive"에서 수행해야할 연산을 여기서 한꺼번에 한다.
				float parentWorldAngle = _headChainUnit._angleWorld_Parent;

				//Head > Tail은 역방향 (부모 > 자식)
				for (int iBone = _nUnits - 1; iBone >= 0; iBone--)
				{
					chainUnit = _chainUnits[iBone];

					//현재 체인의 속성을 받자
					chainStartPosW = chainUnit._bonePosW;
					chainEndPosW = chainUnit._targetPosW;
					chainLength = chainUnit._lengthBoneToTarget;

					//Start를 부탁하고 End를 구하기 위해
					//"타겟 Start > 체인의 End"로의 방향을 구하자.
					//Start가 목표로 이동, 회전한 후의 체인의 Start > End 방향을 더해서 새로운 End를 구한다.
					nextChainDir_StartToEnd = (chainEndPosW - curTargetPosW_Start).normalized;
					nextChainEndPos = curTargetPosW_Start + (nextChainDir_StartToEnd * chainLength);

					//해당 위치로 체인을 이동시킨다.
					chainUnit._bonePosW = curTargetPosW_Start;//Start는 목표에 바로 부착
					chainUnit._targetPosW = nextChainEndPos;//End는 연산된 위치로 이동

					//Start 목표를 현재 체인의 End로 변경한다.
					curTargetPosW_Start = chainUnit._targetPosW;

					// < 각도 계산 >
					float nextAngleW = apBoneIKChainUnit.Vector2Angle(chainUnit._targetPosW - chainUnit._bonePosW);
					float nextAngleL = nextAngleW - parentWorldAngle;//180, 360 범위 계산 안한 값
					float deltaAngle = apUtil.AngleTo180(nextAngleL - chainUnit._angleLocal_Prev);
					chainUnit._angleLocal_Next = apUtil.AngleTo360(chainUnit._angleLocal_Prev + deltaAngle);

					
					// < Constraint가 있고, IK Space에서 동작한다면 >
					bool isClamped = false;

					if(chainUnit._isAngleContraint 
						&& iLoop > FABRIK_LOOPS_RUNTIME__ACONST_WEIGHT01)
					{
						//Soft Limit라면 Angle Range가 Loop에 따라 조금씩 달라진다.
						//갑자기 제한이 걸려서 움직임이 튀는 것을 막는다.
						float angleRange_Min = chainUnit._angleDir_Lower;
						float angleRange_Max = chainUnit._angleDir_Upper;

						if (chainUnit._isSoftLimit)
						{
							//완만한 Angle의 경우, 
							float avgRange = (angleRange_Min * 0.5f) + (angleRange_Max * 0.5f);
							float rangeSize_Half = Mathf.Abs(angleRange_Max - angleRange_Min) * 0.5f;
							if (iLoop < FABRIK_LOOPS_RUNTIME__ACONST_SOFTLIMIT_SHRINK)
							{
								//Angle이 축소된다. (미리 막기 위함. 대신 Weight가 작을 때)
								rangeSize_Half *= 0.95f;
								angleRange_Min = avgRange - rangeSize_Half;
								angleRange_Max = avgRange + rangeSize_Half;
							}
							else if (iLoop > FABRIK_LOOPS_RUNTIME__ACONST_SOFTLIMIT_EXPAND)
							{
								//Angle이 확대된다.
								rangeSize_Half *= 1.05f;
								angleRange_Min = avgRange - rangeSize_Half;
								angleRange_Max = avgRange + rangeSize_Half;
							}
						}

						isClamped = apUtil.IsAngleClamped(chainUnit._angleLocal_Next,
																angleRange_Min,
																angleRange_Max);

						if (isClamped)
						{
							float weight = 0.1f;
							if (chainUnit._isSoftLimit)
							{
								//완만한 각도 제한 (가중치가 약하다)
								if (iLoop > FABRIK_LOOPS_RUNTIME__ACONST_WEIGHT10)		{ weight = 0.5f; }
								else if (iLoop > FABRIK_LOOPS_RUNTIME__ACONST_WEIGHT02)	{ weight = 0.12f; }
								else													{ weight = 0.05f; }
							}
							else
							{
								//정확한 각도 제한
								if (iLoop > FABRIK_LOOPS_RUNTIME__ACONST_WEIGHT10)		{ weight = 1.0f; }
								else if (iLoop > FABRIK_LOOPS_RUNTIME__ACONST_WEIGHT02)	{ weight = 0.2f; }
								else													{ weight = 0.1f; }
							}
							


							//각도 제한을 그대로 할게 아니라, Clamp시, Min, Max에서의 End 결과를 비교한다. (nextChainEndPos와 비교)
							float angleConstW_Lower = angleRange_Min + parentWorldAngle;
							float angleConstW_Upper = angleRange_Max + parentWorldAngle;

							constPos_Lower = apOptBoneIKChainUnit.MakeTargetPos(angleConstW_Lower, ref chainUnit._bonePosW, chainUnit._lengthBoneToTarget);
							constPos_Upper = apOptBoneIKChainUnit.MakeTargetPos(angleConstW_Upper, ref chainUnit._bonePosW, chainUnit._lengthBoneToTarget);

							float distToPreResult_Lower = Vector2.SqrMagnitude(nextChainEndPos - constPos_Lower);
							float distToPreResult_Upper = Vector2.SqrMagnitude(nextChainEndPos - constPos_Upper);

							if (distToPreResult_Lower < distToPreResult_Upper)
							{
								//Lower로 제한하는게 덜 이동하는 것이라면
								chainUnit._angleLocal_Next = apUtil.AngleSlerp(chainUnit._angleLocal_Next, angleRange_Min, weight);
							}
							else
							{
								//Upper로 제한하는게 덜 이동하는 것이라면
								chainUnit._angleLocal_Next = apUtil.AngleSlerp(chainUnit._angleLocal_Next, angleRange_Max, weight);
							}

							nextAngleW = apUtil.AngleTo360(chainUnit._angleLocal_Next + parentWorldAngle);
						}
					}

					//Local을 이용해서 각도를 마저 계산하자
					chainUnit._angleLocal_Delta = apUtil.AngleTo360(chainUnit._angleLocal_Next - chainUnit._angleLocal_Prev);
					chainUnit._angleWorld_Next = nextAngleW;
					
					//현재의 World를 Parent World로 변경한다.
					parentWorldAngle = nextAngleW;

					//각도가 Clamp 되었다면, Target Pos를 바꾸고, 자식 Chain Unit의 위치들을 다 갱신해야한다.
					if(isClamped)
					{
						//주의 : FABRIK에서는 임시 계산으로 Recursive를 쓰면 안된다.
						//chainUnit.CalculateWorldRecursive_FABRIK(chainUnit);

						//Recursive가 아닌 현재 World만 변경한다. (자식 본은 Forward 결과를 유지해야한다)
						chainUnit.CalculateWorld_FABRIK();

						//Start 목표를 다시 변경한다.
						curTargetPosW_Start = chainUnit._targetPosW;
					}
				}

				//성공 여부를 판단한다.
				if (iLoop > FABRIK_LOOPS_RUNTIME__STOPPABLE_NOCHANGED)
				{

					//처리 전의 거리를 체크한다.
					float curDistTailEndToTarget = Vector2.Distance(_tailChainUnit._targetPosW, targetPosW);

					if (curDistTailEndToTarget > prevDistTailEndToTarget - 0.0005f)
					{
						//Runtime에서는 "거리에 따른 복구 여부" 기능은 동작하지 않고, 바로 종료한다.
						_headChainUnit.CalculateWorldRecursive();
						break;
					}
				}

				//World를 전체적으로 다시 계산
				_headChainUnit.CalculateWorldRecursive();
			}


			//마지막으로 Tail만 Target 방향을 바라보게 만들자 (마지막 보정)
			Vector2 lookAtDir = (targetPosW - _tailChainUnit._bonePosW).normalized;
			Vector2 correctedTargetPos = _tailChainUnit._bonePosW + (lookAtDir * _tailChainUnit._lengthBoneToTarget);
			
			//Debug.Log("> 위치 : 이전 : " + chainUnit._targetPosW + " > 보정 : " + correctedTargetPos + " / 요청 : " + targetPosW + " / UnitHead : " + chainUnit._bonePosW);
			_tailChainUnit._targetPosW = correctedTargetPos;
			float nextTailAngleW = apBoneIKChainUnit.Vector2Angle(_tailChainUnit._targetPosW - _tailChainUnit._bonePosW);
			float nextTailAngleL = nextTailAngleW - _tailChainUnit._angleWorld_Parent;//180, 360 범위 계산 안한 값
			float deltaTailAngle = apUtil.AngleTo180(nextTailAngleL - _tailChainUnit._angleLocal_Prev);
			_tailChainUnit._angleLocal_Next = apUtil.AngleTo360(_tailChainUnit._angleLocal_Prev + deltaTailAngle);

			if(_tailChainUnit._isAngleContraint)
			{
				bool isTailClamped = apUtil.IsAngleClamped(	_tailChainUnit._angleLocal_Next,
															_tailChainUnit._angleDir_Lower,
															_tailChainUnit._angleDir_Upper);

				if(isTailClamped)
				{
					float angleConstW_Lower = _tailChainUnit._angleDir_Lower + _tailChainUnit._angleWorld_Parent;
					float angleConstW_Upper = _tailChainUnit._angleDir_Upper + _tailChainUnit._angleWorld_Parent;
					constPos_Lower = apBoneIKChainUnit.MakeTargetPos(angleConstW_Lower, _tailChainUnit._bonePosW, _tailChainUnit._lengthBoneToTarget);
					constPos_Upper = apBoneIKChainUnit.MakeTargetPos(angleConstW_Upper, _tailChainUnit._bonePosW, _tailChainUnit._lengthBoneToTarget);

					float distToPreResult_Lower = Vector2.SqrMagnitude(targetPosW - constPos_Lower);
					float distToPreResult_Upper = Vector2.SqrMagnitude(targetPosW - constPos_Upper);

					if (distToPreResult_Lower < distToPreResult_Upper)
					{
						//Lower로 제한하는게 덜 이동하는 것이라면
						_tailChainUnit._angleLocal_Next = _tailChainUnit._angleDir_Lower;
					}
					else
					{
						//Upper로 제한하는게 덜 이동하는 것이라면
						_tailChainUnit._angleLocal_Next = _tailChainUnit._angleDir_Upper;
					}
				}
			}

			_tailChainUnit._angleLocal_Delta = apUtil.AngleTo360(_tailChainUnit._angleLocal_Next - _tailChainUnit._angleLocal_Prev);
			_tailChainUnit._angleWorld_Next = nextTailAngleW;
			_tailChainUnit.CalculateWorld_FABRIK();

			if(isContinuous)
			{
				_isContinuousPrevPos = true;
				_prevTargetPosW = targetPosW;
			}
			else
			{
				_isContinuousPrevPos = false;
			}

			_requestedTargetPosW = _tailChainUnit._targetPosW;
			_requestedBonePosW = _tailChainUnit._bonePosW;
			return true;
		}


		//Limited Simulate는 없애자.


		

		//추가 : LookAt IK
		//추가:
		/// <summary>
		/// LookAt IK를 시뮬레이션한다.
		/// 요청한 Bone을 Tail로 하여 Head까지 처리한다.
		/// 결과값은 Delta Angle로 나오며, 이 값을 참조하여 결정한다. (Matrix 중 어디에 쓸지는 외부에서 결정)
		/// </summary>
		/// <param name="targetPosW"></param>
		public bool SimulateLookAtIK_CCD(Vector2 defaultLookAtPosW, Vector2 lookAtPosW, bool isContinuous)
		{
			//기본적인 계산은 SimulateIK를 이용한다.
			//여기서는 SimulateIK 전후로 추가적인 작업을 한다.
			//계산 전)
			//- 각각의 ChainUnit을 기준으로 Look At Dir를 계산하여 "평균적으로"적절한 targetPosW를 계산한다.
			//계산 후)
			//-이 본의 예상 위치를 계산하여 마지막으로 바라보는 각도를 계산한다. (적용은 나중에)
			
			if (!_bone._isIKTail)
			{
				return false;
			}

			if (_chainUnits.Count == 0)
			{
				return false;
			}

			apOptBoneIKChainUnit chainUnit = null;

			Vector2 targetPosW = Vector2.zero;
			int nCalculated = 0;
			for (int i = 0; i < _chainUnits.Count; i++)
			{
				chainUnit = _chainUnits[i];
				if (chainUnit._baseBone == null)
				{
					continue;
				}


				//이전 방식
				//Vector2 dirBone2LookAt = lookAtPosW - chainUnit._baseBone._worldMatrix._pos;
				//Vector2 dirBone2DefaultLookAt = defaultLookAtPosW - chainUnit._baseBone._worldMatrix._pos;//<<기본 위치 기준
				//float deltaAngle = apUtil.AngleTo180(apOptBoneIKChainUnit.Vector2Angle(dirBone2LookAt) - apOptBoneIKChainUnit.Vector2Angle(dirBone2DefaultLookAt));
				
				//Vector2 dirBone2TailBone = _tailChainUnit._targetBone._worldMatrix._pos - chainUnit._baseBone._worldMatrix._pos;
				//Vector2 dirBone2ExpectedTargetPos = apOptBoneIKChainUnit.RotateAngle(
				//											chainUnit._baseBone._worldMatrix._pos, 
				//											_tailChainUnit._targetBone._worldMatrix._pos, 
				//											apOptBoneIKChainUnit.Vector2Angle(dirBone2TailBone) + deltaAngle);

				//변경 20.8.31 : IKSpace의 값을 이용
				Vector2 dirBone2LookAt = lookAtPosW - chainUnit._baseBone._worldMatrix.Pos_IKSpace;
				Vector2 dirBone2DefaultLookAt = defaultLookAtPosW - chainUnit._baseBone._worldMatrix.Pos_IKSpace;//<<기본 위치 기준
				float deltaAngle = apUtil.AngleTo180(apOptBoneIKChainUnit.Vector2Angle(dirBone2LookAt) - apOptBoneIKChainUnit.Vector2Angle(dirBone2DefaultLookAt));
				
				Vector2 dirBone2TailBone = _tailChainUnit._targetBone._worldMatrix.Pos_IKSpace - chainUnit._baseBone._worldMatrix.Pos_IKSpace;
				Vector2 dirBone2ExpectedTargetPos = apOptBoneIKChainUnit.RotateAngle(
															chainUnit._baseBone._worldMatrix.Pos_IKSpace, 
															_tailChainUnit._targetBone._worldMatrix.Pos_IKSpace, 
															apOptBoneIKChainUnit.Vector2Angle(dirBone2TailBone) + deltaAngle);

				targetPosW += dirBone2ExpectedTargetPos;
				nCalculated++;
			}

			if(nCalculated == 0)
			{
				targetPosW = lookAtPosW;
			}
			else
			{
				targetPosW.x /= nCalculated;
				targetPosW.y /= nCalculated;
			}

			bool result = SimulateIK_CCD(targetPosW, isContinuous);
			if(!result)
			{
				return false;
			}

			_tailBoneNextPosW = _tailChainUnit._targetPosW;

			return true;
		}



		public bool SimulateLookAtIK_FABRIK(	Vector2 defaultLookAtPosW,
												Vector2 lookAtPosW,
												bool isContinuous,
												float IKWeight)
		{
			//기본적인 계산은 SimulateIK를 이용한다.
			//여기서는 SimulateIK 전후로 추가적인 작업을 한다.
			//계산 전)
			//- 각각의 ChainUnit을 기준으로 Look At Dir를 계산하여 "평균적으로"적절한 targetPosW를 계산한다.
			//계산 후)
			//-이 본의 예상 위치를 계산하여 마지막으로 바라보는 각도를 계산한다. (적용은 나중에)
			if (!_bone._isIKTail)
			{
				return false;
			}
			
			if (_nUnits == 0)
			{
				return false;
			}

			apOptBoneIKChainUnit chainUnit = null;

			Vector2 targetPosW = Vector2.zero;
			int nCalculated = 0;
			for (int i = 0; i < _nUnits; i++)
			{
				chainUnit = _chainUnits[i];
				if (chainUnit._baseBone == null)
				{
					continue;
				}

				//변경 20.8.26 : IKSpace의 값을 이용
				Vector2 dirBone2LookAt = lookAtPosW - chainUnit._baseBone._worldMatrix.Pos_IKSpace;
				Vector2 dirBone2DefaultLookAt = defaultLookAtPosW - chainUnit._baseBone._worldMatrix.Pos_IKSpace;//<<기본 위치 기준
				float deltaAngle = apUtil.AngleTo180(apBoneIKChainUnit.Vector2Angle(dirBone2LookAt) - apBoneIKChainUnit.Vector2Angle(dirBone2DefaultLookAt));
				
				Vector2 dirBone2TailBone = _tailChainUnit._targetBone._worldMatrix.Pos_IKSpace - chainUnit._baseBone._worldMatrix.Pos_IKSpace;
				Vector2 dirBone2ExpectedTargetPos = apBoneIKChainUnit.RotateAngle(
															chainUnit._baseBone._worldMatrix.Pos_IKSpace, 
															_tailChainUnit._targetBone._worldMatrix.Pos_IKSpace, 
															apBoneIKChainUnit.Vector2Angle(dirBone2TailBone) + deltaAngle);

				targetPosW += dirBone2ExpectedTargetPos;
				nCalculated++;
			}

			if(nCalculated == 0)
			{
				targetPosW = lookAtPosW;
			}
			else
			{
				targetPosW.x /= nCalculated;
				targetPosW.y /= nCalculated;
			}

			bool result = SimulateIK_FABRIK(targetPosW, isContinuous, IKWeight);//targetPosW > 이건 IKSpace로 계산된 것이다.
			if(!result)
			{
				return false;
			}

			_tailBoneNextPosW = _tailChainUnit._targetPosW;
			return true;
		}






		/// <summary>
		/// IK 결과값을 일단 각 Bone에게 넣어준다.
		/// 적용된 값이 아니라 변수로 저장하는 것이므로, 
		/// 각 Bone에 있는 _IKRequestAngleResult를 참조하자
		/// </summary>
		/// <param name="weight"></param>
		public void AdaptIKResultToBones(float weight)
		{
			//Debug.Log("AdaptIKResultToBones - " + weight);

			apOptBoneIKChainUnit chainUnit = null;
			for (int i = 0; i < _chainUnits.Count; i++)
			{
				chainUnit = _chainUnits[i];

				
				chainUnit._baseBone.AddIKAngle(	chainUnit._angleWorld_Next, 
												chainUnit._angleWorld_Next - chainUnit._angleWorld_Prev,//Delta값도 추가하자 20.9.2
												weight);
			}
		}

		/// <summary>
		/// AdaptIKResultToBones()의 IKController 버전
		/// </summary>
		/// <param name="weight"></param>
		public void AdaptIKResultToBones_ByController(float weight)
		{
			apOptBoneIKChainUnit chainUnit = null;
			int nChainUnits = _chainUnits != null ? _chainUnits.Count : 0;
			if(nChainUnits == 0)
			{
				return;
			}
			for (int i = 0; i < nChainUnits; i++)
			{
				chainUnit = _chainUnits[i];
				chainUnit._baseBone.AddIKAngle_Controlled(chainUnit._angleWorld_Next - chainUnit._angleWorld_Prev, weight);
			}
		}
	}
}
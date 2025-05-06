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

	public class apCalculatedResultParamSubList
	{
		// Members
		//--------------------------------------------------
		public apCalculatedResultParam _parentResultParam = null;

		//이걸 그냥 Modifier의 Key값과 연계하는게 좋을지도
		//public apModifierParamSetGroup.SYNC_TARGET _keySyncType = apModifierParamSetGroup.SYNC_TARGET.Controller;
		public apModifierParamSetGroup _keyParamSetGroup = null;

		//Key
		//1. Control Param인 경우
		//public apControlParam _controlParam = null;

		//2. 키프레임인 경우 : TODO
		//3. Static인 경우 -> 없음 (Input에 따라 구분되지 않는다.)


		public List<apCalculatedResultParam.ParamKeyValueSet> _subParamKeyValues = new List<apCalculatedResultParam.ParamKeyValueSet>();
		//private int _nSubParamKeyValues = 0;//삭제 v1.4.7
		private Dictionary<apAnimKeyframe, apCalculatedResultParam.ParamKeyValueSet> _animKeyframe2ParamKeyValue = null;
		



		//계산용
		public float _totalWeight = 0.0f;

		



		private const float _distBias = 0.01f;

		/// <summary>
		/// Control Param 타입인 경우에, "어떤 Key를 선택하여 Weight를 수행할 지를 결정"하도록 도와주는 클래스
		/// Input Control Param에 대해서
		/// 1) 입력된 1차원 영역 / 2차원 삼각 영역 / 2.5D 삼각 영역에 포함되는지 판단
		/// 2) 포함이 된다면 "이 영역에서 Weight를 같이 계산할 ParamSetKey들"을 리턴
		/// </summary>
		public class ControlParamArea
		{
			public int _iKeyA, _iKeyB;//Int 타입일때
			public Vector2 _keyPosA, _keyPosB, _keyPosC;
			public Vector2 _center;
			public float _radius = 0.0f;

		}

		// 개선된 Control Param 보간
		//Control Param 타입인 경우
		private List<apCalculatedLerpPoint> _cpLerpPoints = null;
		private List<apCalculatedLerpArea> _cpLerpAreas = null;

		//1D Lerp 처리를 위한 참조 변수
		private apCalculatedLerpPoint _cpLerpPoint_A = null;
		private apCalculatedLerpPoint _cpLerpPoint_B = null;

		//2D Lerp 처리를 위한 참조 변수
		private apCalculatedLerpArea _cpLerpAreaLastSelected = null;//빠른 처리를 위해 "이전에 참조된 Area"를 저장하자.


		//개선된 Anim Keyframe 보간 > 보류
		//private apCalculatedAnimKeyLUT _animKeyLUT = null;


		//[v1.4.7 추가]
		//Calculate 함수 최적화 방안.
		//_subParamKeyValues와 동일한 타입이지만, Calculate되는 
		//이전에는 Calculate 함수를 통해서 "적용되는 CalParam_PKV의 isCalculated 속성을 true로 전환"하는 방식을 이용했다.
		//새로운 방식에서는, 그냥 "Calculate되는 CalParam_PKV를 리스트"로 따로 모은다.
		private int _nCalculatedParamKeyValues = 0;
		private List<apCalculatedResultParam.ParamKeyValueSet> _calculatedParamKeyValues = null;
		
		//이미 추가된 PKV인지 여부는 기존의 _isCalculated (중간의 RandKey) 대신, 캐시 변수를 먼저 이용하자.
		//캐시는 총 4개
		private apCalculatedResultParam.ParamKeyValueSet _calculatedPKV_Cache_0 = null;
		private apCalculatedResultParam.ParamKeyValueSet _calculatedPKV_Cache_1 = null;
		private apCalculatedResultParam.ParamKeyValueSet _calculatedPKV_Cache_2 = null;
		private apCalculatedResultParam.ParamKeyValueSet _calculatedPKV_Cache_3 = null;
		private int _nCalculatedPKVCached = 0;
		



		//[v1.4.7 추가]
		//애니메이션 키프레임 조회시 참조하는 캐시.
		//캐시 정보가 현재 상태에 적합하면 재활용을 하고, 그렇지 않다면 새로 캐시를 계산한다.
		private apCalculatedResultParam.ParamKeyValueSet _animCache_KeyframePKV_A = null;
		private apCalculatedResultParam.ParamKeyValueSet _animCache_KeyframePKV_B = null;//Cur 키프레임에 정확히 맞다면 이게 없을 수도 있다.



		// Init
		//--------------------------------------------------
		public apCalculatedResultParamSubList(apCalculatedResultParam parentResultParam)
		{
			_parentResultParam = parentResultParam;

			if(_subParamKeyValues == null)
			{
				_subParamKeyValues = new List<apCalculatedResultParam.ParamKeyValueSet>();
			}
			_subParamKeyValues.Clear();
			//_nSubParamKeyValues = 0;

			//v1.4.7 추가 : 애니메이션 키프레임으로의 매핑을 해서 참조를 더 빠르게 하자
			if(_animKeyframe2ParamKeyValue == null)
			{
				_animKeyframe2ParamKeyValue = new Dictionary<apAnimKeyframe, apCalculatedResultParam.ParamKeyValueSet>();
			}
			_animKeyframe2ParamKeyValue.Clear();


			//v1.4.7 추가 : 최적화
			if(_calculatedParamKeyValues == null)
			{
				_calculatedParamKeyValues = new List<apCalculatedResultParam.ParamKeyValueSet>();
			}
			_calculatedParamKeyValues.Clear();
			_nCalculatedParamKeyValues = 0;
			
			//v1.4.7 : 선택 캐시 초기화
			_calculatedPKV_Cache_0 = null;
			_calculatedPKV_Cache_1 = null;
			_calculatedPKV_Cache_2 = null;
			_calculatedPKV_Cache_3 = null;
			_nCalculatedPKVCached = 0;

			//애니메이션 캐시 초기화
			_animCache_KeyframePKV_A = null;
			_animCache_KeyframePKV_B = null;//Cur 키프레임에 정확히 맞다면 이게 없을 수도 있다.
		}

		public void SetParamSetGroup(apModifierParamSetGroup paramSetGroup)
		{
			_keyParamSetGroup = paramSetGroup;
			//_controlParam = _keyParamSetGroup._keyControlParam;
		}


		// Add / Clear
		//--------------------------------------------------
		public void ClearParams()
		{
			_subParamKeyValues.Clear();
			//_nSubParamKeyValues = 0;
		}

		
		/// <summary>Param Key Value를 리스트에 추가한다.</summary>
		public void AddParamKeyValueSet(apCalculatedResultParam.ParamKeyValueSet paramKeyValue)
		{
			if (_subParamKeyValues.Contains(paramKeyValue))
			{	
				return;
			}
			//Debug.Log("AddParamKeyValueSet");

			//v1.4.7
			//- isCalculated 맵에 매핑하기 위해 PKV의 "리스트 내에서의 인덱스"를 지정해줘야 한다.
			//int curIndex = _subParamKeyValues.Count;
			//paramKeyValue.SetIndexOfSubParamList(curIndex);

			_subParamKeyValues.Add(paramKeyValue);
			//_nSubParamKeyValues = _subParamKeyValues.Count;

			//v1.4.7 : 애니메이션 키프레임 > ParamKeyValue 연결하는 매핑
			if(_keyParamSetGroup._keyAnimTimelineLayer != null)
			{
				if(paramKeyValue._paramSet.SyncKeyframe != null)
				{
					apAnimKeyframe linkedKeyframe = paramKeyValue._paramSet.SyncKeyframe;
					if (!_animKeyframe2ParamKeyValue.ContainsKey(linkedKeyframe))
					{
						_animKeyframe2ParamKeyValue.Add(linkedKeyframe, paramKeyValue);
					}
				}
			}
			
		}


		/// <summary>
		/// 입력된 Param Key Value를 보간하기 위한 보조 데이터를 만들어준다.
		/// Add ParamKeyValueSet을 모두 호출한 이후에 꼭 호출해야한다.
		/// </summary>
		public void MakeMetaData()
		{
			switch (_keyParamSetGroup._syncTarget)
			{
				case apModifierParamSetGroup.SYNC_TARGET.Controller:
					{
						if (_keyParamSetGroup._keyControlParam != null)
						{
							//보간을 위한 Key Point와 Area를 만들자.
							if (_cpLerpPoints == null)	{ _cpLerpPoints = new List<apCalculatedLerpPoint>(); }
							if (_cpLerpAreas == null)	{ _cpLerpAreas = new List<apCalculatedLerpArea>(); }

							_cpLerpPoint_A = null;
							_cpLerpPoint_B = null;

							_cpLerpAreaLastSelected = null;

							MakeControlParamLerpAreas();
						}
					}
					break;

					//일단 이건 보류
				//case apModifierParamSetGroup.SYNC_TARGET.KeyFrame:
				//	{
				//		//v1.4.6 추가
				//		//LUT를 여기서도 이용하자
				//		if(_animKeyLUT == null)
				//		{
				//			_animKeyLUT = new apCalculatedAnimKeyLUT(	_keyParamSetGroup._keyAnimClip,
				//														_keyParamSetGroup._keyAnimTimelineLayer,
				//														this);
				//		}

				//		_animKeyLUT.MakeLUT();//LUT를 갱신한다.
				//	}
				//	break;


			}
		}


		// 계산
		//--------------------------------------------------
		public void InitCalculate()
		{
			_totalWeight = 0.0f;

			//이전
			//int nSubParamKeyValues = _subParamKeyValues != null ? _subParamKeyValues.Count : 0;

			//for (int i = 0; i < nSubParamKeyValues; i++)
			//{
			//	_subParamKeyValues[i].ReadyToCalculate();
			//}

			//v1.4.7 변경. 전체 isCalculated를 초기화하는 대신, 몇개의 캐시로 이를 대체할 수 있다.
			_calculatedParamKeyValues.Clear();
			_nCalculatedParamKeyValues = 0;
			
			_calculatedPKV_Cache_0 = null;
			_calculatedPKV_Cache_1 = null;
			_calculatedPKV_Cache_2 = null;
			_calculatedPKV_Cache_3 = null;
			_nCalculatedPKVCached = 0;

		}
		public void Calculate()
		{
			//이전 : v1.4.7에서 삭제됨
			//int nSubParamKeyValues = _subParamKeyValues != null ? _subParamKeyValues.Count : 0;

			//v1.4.7 : 변경. 계산 결과를 여기에 저장하자
			_calculatedParamKeyValues.Clear();
			_nCalculatedParamKeyValues = 0;

			if (_keyParamSetGroup == null)
			{
				return;
			}

			_totalWeight = 0.0f;
			
			//PKV 초기화
			//이전
			//for (int i = 0; i < nSubParamKeyValues; i++)
			//{
			//	_subParamKeyValues[i].ReadyToCalculate();
			//}

			//v1.4.7 변경. 전체 isCalculated를 초기화하는 대신, 몇개의 캐시로 이를 대체할 수 있다.
			_calculatedPKV_Cache_0 = null;
			_calculatedPKV_Cache_1 = null;
			_calculatedPKV_Cache_2 = null;
			_calculatedPKV_Cache_3 = null;
			_nCalculatedPKVCached = 0;


			//Sync 타입에 따라서 ParamSet의 Weight를 계산한다. [중요!]
			switch (_keyParamSetGroup._syncTarget)
			{
				case apModifierParamSetGroup.SYNC_TARGET.Controller:
					//CalculateWeight_ControlParam();//구버전 Lerp
					CalculateWeight_ControlParam_WithMetaData();//신버전 Bilinear
					break;

				case apModifierParamSetGroup.SYNC_TARGET.KeyFrame:
					CalculateWeight_Keyframe();
					break;

				case apModifierParamSetGroup.SYNC_TARGET.Static:
					CalculateWeight_Static();
					break;
			}

			//v1.4.7 : 계산된 PKV의 개수
			_nCalculatedParamKeyValues = _calculatedParamKeyValues.Count;
		}



		//[v1.4.7 변경] 계산된 결과를 단순히 각각의 PKV의 isCalculated로 구분할 것이 아니라, 리스트에 저장해서 참조할 수 있게 하자.
		/// <summary>
		/// v1.4.7 : 계산된 ParamKeyValue를 결과 리스트에 넣는다.
		/// 이전 코드에서 isCalculated = true와 동일한 효과를 가진다.
		/// </summary>
		/// <param name="calculatedPKV"></param>
		public void OnParamKeyValueCalculated(apCalculatedResultParam.ParamKeyValueSet calculatedPKV, float addedWeight)
		{
			bool isCalculated = false;

			//이미 계산되었는지 여부 체크
			//1. 캐시를 확인한다.
			//2. 4개의 캐시를 모두 체크했다면, 리스트에서 Contains를 하자 (계산된 PKV 자체가 많지 않아서 이거 그렇게 안느리다)
			
			//새로 추가할 때는 캐시 개수가 남았다면 캐시에 추가한다.
			if(_nCalculatedPKVCached > 0)
			{
				if(_calculatedPKV_Cache_0 == calculatedPKV)
				{
					//PKV는 이미 계산되었으며 "캐시 0"에 저장된 상태다
					isCalculated = true;
				}
			}
			if(!isCalculated && _nCalculatedPKVCached > 1)
			{
				if(_calculatedPKV_Cache_1 == calculatedPKV)
				{
					//PKV는 이미 계산되었으며 "캐시 1"에 저장된 상태다
					isCalculated = true;
				}
			}
			if(!isCalculated && _nCalculatedPKVCached > 2)
			{
				if(_calculatedPKV_Cache_2 == calculatedPKV)
				{
					//PKV는 이미 계산되었으며 "캐시 2"에 저장된 상태다
					isCalculated = true;
				}
			}
			if(!isCalculated && _nCalculatedPKVCached > 3)
			{
				if(_calculatedPKV_Cache_3 == calculatedPKV)
				{
					//PKV는 이미 계산되었으며 "캐시 3"에 저장된 상태다
					isCalculated = true;
				}
			}
			//4개 미만이면, 모두 캐시에서 찾을 수 있고, 못찾았다면 신규 값이다.
			//캐시가 최대값이 4개인데도 계산 여부를 알 수 없다면, 리스트에서 직접 계산 여부를 알아내야한다.
			if(!isCalculated && _nCalculatedPKVCached >= 4)
			{
				//만약 4개의 
				//만약 캐시에서 계산 여부를 판정할 수 없다면 리스트에서 직접 찾아야 함
				isCalculated = _calculatedParamKeyValues.Contains(calculatedPKV);

				//Debug.Log("4개의 캐시에서 찾을 수 없었다. [" + _nCalculatedPKVCached + "] - " + isCalculated);
			}

			
			if(isCalculated)
			{
				// [ 이미 계산되었다면 ]
				// "계산된 리스트"에 추가하지 말고, 가중치만 더하기만 하자
				calculatedPKV._weight += addedWeight;
				return;
			}
			

			// [ 새로운 계산된 PKV 라면 ]
			//리스트에 추가하자
			_calculatedParamKeyValues.Add(calculatedPKV);
			_nCalculatedParamKeyValues += 1;

			//- 처음 추가하는 것이므로 가중치는 바로 할당을 한다. (별도의 초기화가 없다)
			calculatedPKV._weight = addedWeight;

			//계산되었음을 저장하자
			//_isCalculatedMap[calculatedPKV._indexOfSubParamList] = true;
			//calculatedPKV._calRandKey = _calculateRandKey;//랜덤키 할당으로 변경

			//캐시에 저장하기
			if(_nCalculatedPKVCached < 4)//최대값 4개
			{
				//기존의 저장된 캐시 개수에 따라 캐시 대상을 결정한다.
				switch (_nCalculatedPKVCached)
				{
					case 0: _calculatedPKV_Cache_0 = calculatedPKV; break;
					case 1: _calculatedPKV_Cache_1 = calculatedPKV; break;
					case 2: _calculatedPKV_Cache_2 = calculatedPKV; break;
					case 3: _calculatedPKV_Cache_3 = calculatedPKV; break;
				}
				//Debug.Log("PKV 저장 > 캐시 : " + _nCalculatedPKVCached);
				_nCalculatedPKVCached += 1;
				
			}
			
		}


		/// <summary>
		/// v1.4.7 : Calculated된 ParamKeyValues (기존의 isCalculated가 true인 ParamKeyValue들의 집합)
		/// </summary>
		public List<apCalculatedResultParam.ParamKeyValueSet> CalculatedParamKeyValues
		{
			get
			{
				return _calculatedParamKeyValues;
			}
		}

		/// <summary>
		/// v1.4.7 : Calcuated된 ParamKeyValues의 개수
		/// </summary>
		public int NumCalculatedParamKeyValues
		{
			get { return _nCalculatedParamKeyValues; }
		}


		//-----------------------------------------------------------------------------------
		// 중요
		//-----------------------------------------------------------------------------------

		#region [미사용 코드] 이전 버전의 Control Param 계산 식
		//		/// <summary>
		//		/// ParamSet간의 Weight를 계산한다. [ControlParam이 입력값인 경우]
		//		/// </summary>
		//		private void CalculateWeight_ControlParam()
		//		{
		//			//if(_controlParam == null)
		//			if (_keyParamSetGroup == null || _keyParamSetGroup._keyControlParam == null)
		//			{
		//				Debug.LogError("Key ParamSet Group is Null / Key Control Param Is null");
		//				return;
		//			}

		//			apControlParam controlParam = _keyParamSetGroup._keyControlParam;

		//			//1. 현재 값에 따라서 Dist 값을 넣자
		//			float minDist = float.MaxValue;
		//			float maxDist = 0.0f;
		//			float dist = 0.0f;
		//			apCalculatedResultParam.ParamKeyValueSet curParamKeyValue = null;
		//			apCalculatedResultParam.ParamKeyValueSet nextParamKeyValue = null;

		//			//int nSubParamKeyValues = _subParamKeyValues.Count;
		//			_totalWeight = 0.0f;

		//			for (int i = 0; i < _subParamKeyValues.Count; i++)
		//			{
		//				curParamKeyValue = _subParamKeyValues[i];
		//				dist = -10.0f;
		//				curParamKeyValue._isCalculated = false;

		//#if UNITY_EDITOR
		//				//if (!curParamKeyValue._isActive_InEditorExclusive)
		//				if (!curParamKeyValue.IsActive)
		//				{
		//					//에디터에서 제한한 Paramkey면
		//					curParamKeyValue._dist = -10.0f;
		//					curParamKeyValue._isCalculated = false;

		//					//bool isKeyNull = false;
		//					//bool isCalculateNotEnabled = false;
		//					//if (curParamKeyValue._keyParamSetGroup == null)
		//					//{
		//					//	isKeyNull = true;
		//					//}
		//					//else if (!curParamKeyValue._keyParamSetGroup.IsCalculateEnabled)
		//					//{
		//					//	//isCalculateNotEnabled = true;
		//					//}
		//					//Debug.LogError("CalResultParamSubList Weight Failed : " + _parentResultParam._targetRenderUnit + " / "
		//					//	+ "ParamSetGroup Is Null : " + (isKeyNull) + " / Calculate Enabled : " + isCalculateNotEnabled);
		//					continue;
		//				}
		//#endif

		//				//수식 1 : IDW 방식 (Inverse Distance Weighting)
		//				//-----------------------------------------------
		//				#region 수식 1 적용
		//				switch (controlParam._valueType)
		//				{
		//					//case apControlParam.TYPE.Bool:
		//					//	if (curParamKeyValue._paramSet._conSyncValue_Bool == controlParam._bool_Cur)
		//					//	{
		//					//		curParamKeyValue._dist = 0.0f;
		//					//		curParamKeyValue._isCalculated = true;
		//					//	}
		//					//	else
		//					//	{
		//					//		curParamKeyValue._dist = -10.0f;
		//					//		curParamKeyValue._isCalculated = false;
		//					//	}
		//					//	break;

		//					case apControlParam.TYPE.Int:
		//						dist = controlParam.GetNormalizedDistance_Int(curParamKeyValue._paramSet._conSyncValue_Int);
		//						break;

		//					case apControlParam.TYPE.Float:
		//						dist = controlParam.GetNormalizedDistance_Float(curParamKeyValue._paramSet._conSyncValue_Float);
		//						break;

		//					case apControlParam.TYPE.Vector2:
		//						dist = controlParam.GetNormalizedDistance_Vector2(curParamKeyValue._paramSet._conSyncValue_Vector2);
		//						break;

		//						//case apControlParam.TYPE.Vector3:
		//						//	dist = controlParam.GetNormalizedDistance_Vector3(curParamKeyValue._paramSet._conSyncValue_Vector3);
		//						//	break;

		//						//case apControlParam.TYPE.Color:
		//						//	break;
		//				}


		//				if (dist < -1.0f)
		//				{
		//					//계산 안함
		//					continue;
		//				}

		//				curParamKeyValue._dist = dist;
		//				curParamKeyValue._isCalculated = true;

		//				if (dist < minDist)
		//				{
		//					minDist = dist;//<<최소 값
		//				}
		//				if (dist > maxDist)
		//				{
		//					maxDist = dist;//최대값 (가장 Weight가 적게 걸리는 값)
		//				}


		//				#endregion
		//				//-----------------------------------------------
		//			}

		//			if (maxDist - minDist < 0.0001f)
		//			{
		//				maxDist = minDist + 0.0001f;
		//			}


		//			_totalWeight = 0.0f;
		//			// 여러개의 키값을 사용할 거라면


		//			#region [미사용 코드] 수식이 중복된다.
		//			//List<float> keepWeightRatios = new List<float>();


		//			//for (int i = 0; i < _subParamKeyValues.Count; i++)
		//			//{
		//			//	curParamKeyValue = _subParamKeyValues[i];
		//			//	if(curParamKeyValue._dist < -1.0f)
		//			//	{
		//			//		continue;
		//			//	}
		//			//	float keepWeightRatio = Mathf.Clamp01((curParamKeyValue._dist - minDist) / (maxDist - minDist));

		//			//	keepWeightRatios.Add(keepWeightRatio);
		//			//}

		//			//keepWeightRatios.Sort(delegate (float a, float b)
		//			//	{
		//			//		return (int)((a * 1000.0f) - (b * 1000.0f));
		//			//	});

		//			//bool isLimitedWeight = false;
		//			//float keepWeightRatio_Min = 0.0f;
		//			//float keepWeightRatio_Min2 = 0.0f;
		//			//float limitWeight = 1.0f;
		//			//if(keepWeightRatios.Count >= 2)
		//			//{
		//			//	isLimitedWeight = true;
		//			//	keepWeightRatio_Min = keepWeightRatios[0];
		//			//	keepWeightRatio_Min2 = keepWeightRatios[1];

		//			//	if(keepWeightRatio_Min2 - keepWeightRatio_Min < 0.0001f)
		//			//	{
		//			//		keepWeightRatio_Min2 = keepWeightRatio_Min + 0.0001f;
		//			//	}
		//			//	limitWeight = Mathf.Clamp01(0.5f - keepWeightRatio_Min) * 2;
		//			//} 
		//			#endregion

		//			#region [미사용 코드] 역 선형 수식이지만 오류가 있다.
		//			//for (int i = 0; i < _subParamKeyValues.Count; i++)
		//			//{
		//			//	curParamKeyValue = _subParamKeyValues[i];

		//			//	if (curParamKeyValue._dist < -1.0f)
		//			//	{
		//			//		curParamKeyValue._weight = 0.0f;
		//			//		curParamKeyValue._isCalculated = false;
		//			//		continue;
		//			//	}

		//			//	//ITP 계산
		//			//	//1 - Dist로 역 선형 보간을 사용한다.
		//			//	//가장 가까운 포인트에서 MinDist를 구한다.
		//			//	//Min Dist가 0일때 = 어느 점에 도달했을때
		//			//	//> 다른 Weight가 0이 되어야 하며, Min Point인 부분은 Weight가 보전되어야 한다.
		//			//	//"보전률" = deltaMinDist가 작을수록 크다 (max를 구해야겠네)
		//			//	//"Mul-Weight" : 보전률에 비례한다. 보전률이 0일땐 MinDist (Normalize)의 값을 가지고, 최대일땐 1의 값을 가진다.

		//			//	float keepWeightRatio = 1.0f - Mathf.Clamp01((curParamKeyValue._dist - minDist) / (maxDist - minDist));

		//			//	// 가까우면 = 1 (감소 없음) / 멀면 minDist (포인트에 근접할 수록 0에 수렴)

		//			//	float multiplyWeight = (1.0f * keepWeightRatio) + minDist * (1.0f - keepWeightRatio);

		//			//	//만약, minDist가 일정 값 이하로 떨어지면 0으로 multiplyWeight가 아예 수렴해야한다.



		//			//	//float revWeight = (maxDist - curParamKeyValue._dist) * multiplyWeight;
		//			//	float revWeight = (2.0f - curParamKeyValue._dist) * multiplyWeight;//<<수정 : MaxDist가 아니라 Normalize 영역 크기(-1 ~ 1 = 2)로 빼야 적절하게 나온다.


		//			//	_totalWeight += revWeight;
		//			//	curParamKeyValue._weight = revWeight;
		//			//	curParamKeyValue._isCalculated = true;
		//			//} 
		//			#endregion

		//			for (int i = 0; i < _subParamKeyValues.Count; i++)
		//			{
		//				curParamKeyValue = _subParamKeyValues[i];

		//				if (curParamKeyValue._dist < -1.0f)
		//				{
		//					curParamKeyValue._weight = 0.0f;
		//					curParamKeyValue._weightBase = 0.0f;
		//					curParamKeyValue._isCalculated = false;
		//					continue;
		//				}

		//				//변경
		//				//Weight 시작값이 기본 1이 아니라, 거리에 따른 가중치로 바뀐다.
		//				curParamKeyValue._weight = 1.0f;

		//				if (_subParamKeyValues.Count <= 2)
		//				//if(true)
		//				{
		//					curParamKeyValue._weightBase = 1.0f;
		//				}
		//				else
		//				{
		//					curParamKeyValue._weightBase = controlParam.GetInterpolationWeight(curParamKeyValue._dist);
		//				}
		//				curParamKeyValue._isCalculated = true;
		//				//_totalWeight += 1.0f;
		//				_totalWeight += curParamKeyValue._weight;//변경!

		//			}

		//			if (_subParamKeyValues.Count >= 2)
		//			{
		//				_totalWeight = 0.0f;

		//				for (int i = 0; i < _subParamKeyValues.Count - 1; i++)
		//				{
		//					curParamKeyValue = _subParamKeyValues[i];
		//					if (!curParamKeyValue._isCalculated)
		//					{
		//						continue;
		//					}
		//					if (curParamKeyValue._weight < 0.00001f)
		//					{
		//						continue;
		//					}
		//					for (int j = i + 1; j < _subParamKeyValues.Count; j++)
		//					{
		//						nextParamKeyValue = _subParamKeyValues[j];
		//						if (!nextParamKeyValue._isCalculated)
		//						{
		//							continue;
		//						}

		//						float sumDist = curParamKeyValue._dist + nextParamKeyValue._dist;
		//						if (sumDist < 0.0001f)
		//						{
		//							curParamKeyValue._weight *= 1.0f;
		//							nextParamKeyValue._weight *= 1.0f;
		//						}
		//						else
		//						{
		//							float itp = Mathf.Clamp01((sumDist - curParamKeyValue._dist) / sumDist);
		//							//float baseWeight = (curParamKeyValue._weightBase + nextParamKeyValue._weightBase) * 0.5f;
		//							float baseWeight = Mathf.Clamp01(curParamKeyValue._weightBase + nextParamKeyValue._weightBase);
		//							//float baseWeight = curParamKeyValue._weightBase * nextParamKeyValue._weightBase;

		//							//float itp = apAnimCurve.GetSmoothInterpolation((sumDist - curParamKeyValue._dist) / sumDist);
		//							//curParamKeyValue._weight *= itp;
		//							//nextParamKeyValue._weight *= (1.0f - itp);
		//							curParamKeyValue._weight = curParamKeyValue._weight * ((1.0f - baseWeight) + itp * baseWeight);
		//							nextParamKeyValue._weight = nextParamKeyValue._weight * ((1.0f - baseWeight) + (1.0f - itp) * baseWeight);


		//							if (itp < 0.00001f)
		//							{
		//								break;
		//							}
		//						}
		//					}
		//				}

		//				for (int i = 0; i < _subParamKeyValues.Count; i++)
		//				{
		//					curParamKeyValue = _subParamKeyValues[i];
		//					if (curParamKeyValue._isCalculated)
		//					{
		//						_totalWeight += curParamKeyValue._weight;
		//					}
		//				}
		//			}

		//			//공통 부분
		//			if (_totalWeight > 0.0f)
		//			{
		//				for (int i = 0; i < _subParamKeyValues.Count; i++)
		//				{
		//					curParamKeyValue = _subParamKeyValues[i];

		//					if (curParamKeyValue._isCalculated)
		//					{
		//						curParamKeyValue._weight /= _totalWeight;
		//					}
		//					else
		//					{
		//						curParamKeyValue._weight = 0.0f;
		//					}
		//				}
		//			}
		//		} 
		#endregion


		private void CalculateWeight_ControlParam_WithMetaData()
		{
			if (_keyParamSetGroup == null || _keyParamSetGroup._keyControlParam == null)
			{
				Debug.LogError("Key ParamSet Group is Null / Key Control Param Is null");
				return;
			}

			apControlParam controlParam = _keyParamSetGroup._keyControlParam;

			//Value 타입에 따라 처리가 달라진다.
			switch (controlParam._valueType)
			{
				case apControlParam.TYPE.Int:
				case apControlParam.TYPE.Float:
					CalculateWeight_ControlParam_1D();
					break;

				case apControlParam.TYPE.Vector2:
					CalculateWeight_ControlParam_2D();
					break;
			}

		}

		private void CalculateWeight_ControlParam_1D()
		{
			if (_keyParamSetGroup == null || _keyParamSetGroup._keyControlParam == null)
			{
				Debug.LogError("Key ParamSet Group is Null / Key Control Param Is null");
				return;
			}

			//int nSubParamKeyValues = _subParamKeyValues != null ? _subParamKeyValues.Count : 0;

			apControlParam controlParam = _keyParamSetGroup._keyControlParam;

			apCalculatedResultParam.ParamKeyValueSet curParamKeyValue = null;

			//v1.4.7 삭제 : 초기화가 필요없다. 이미 CalculateWeight_ControlParam_1D 호출 전에 초기화를 했기 때문
			//for (int i = 0; i < nSubParamKeyValues; i++)
			//{
			//	curParamKeyValue = _subParamKeyValues[i];
			//	curParamKeyValue._weight = 0.0f;//<<이게 중요한데..
			//	curParamKeyValue._isCalculated = false;//<<나중에 이것도 true로 올리자
			//}

			int nLerpPoints = _cpLerpPoints != null ? _cpLerpPoints.Count : 0;

			if (nLerpPoints == 0)
			{
				return;//처리 불가;
			}

			apCalculatedLerpPoint curLerpPoint = null;

			_totalWeight = 0.0f;//v1.4.7 : Weight 계산이 Calcuate 중간에 바로 발생

			if (nLerpPoints == 1)
			{
				curLerpPoint = _cpLerpPoints[0];
				curLerpPoint._calculatedWeight = 1.0f;
				curLerpPoint.CalculateITPWeight(ref _totalWeight);//v1.4.7 : Total Weight 전달
			}
			else
			{

				//1) ITP를 계산할 두개의 Point (A, B)를 잡는다.
				//2) 두개의 포인트를 기준으로 ITP를 계산한다.
				//3) Total Weight 계산 후 적용
				bool isRefreshLerpPointRange = false;
				if (_cpLerpPoint_A == null || _cpLerpPoint_B == null)
				{
					isRefreshLerpPointRange = true;
				}
				else
				{
					if (controlParam._valueType == apControlParam.TYPE.Int)
					{
						if (controlParam._int_Cur < _cpLerpPoint_A._iPos ||
							controlParam._int_Cur > _cpLerpPoint_B._iPos)
						{
							isRefreshLerpPointRange = true;
						}
					}
					else
					{
						if (controlParam._float_Cur < _cpLerpPoint_A._pos.x ||
							controlParam._float_Cur > _cpLerpPoint_B._pos.x)
						{
							isRefreshLerpPointRange = true;
						}
					}
				}

				if (isRefreshLerpPointRange)
				{
					//0..1..2.. [value]..3...4
					int iB = -1;
					if (controlParam._valueType == apControlParam.TYPE.Int)
					{
						for (int i = 0; i < nLerpPoints; i++)
						{
							if (controlParam._int_Cur <= _cpLerpPoints[i]._iPos)
							{
								iB = i;
								break;
							}
						}
					}
					else
					{
						for (int i = 0; i < nLerpPoints; i++)
						{
							if (controlParam._float_Cur <= _cpLerpPoints[i]._pos.x)
							{
								iB = i;
								break;
							}
						}
					}
					if (iB < 0)
					{
						iB = _cpLerpPoints.Count - 1;
					}

					_cpLerpPoint_B = _cpLerpPoints[iB];
					if (iB == 0)
					{
						_cpLerpPoint_A = _cpLerpPoints[0];
					}
					else
					{
						_cpLerpPoint_A = _cpLerpPoints[iB - 1];
					}
				}

				if (_cpLerpPoint_A == null || _cpLerpPoint_B == null)
				{
					return;
				}

				if (_cpLerpPoint_A == _cpLerpPoint_B)
				{
					_cpLerpPoint_A._calculatedWeight = 1.0f;
					_cpLerpPoint_A.CalculateITPWeight(ref _totalWeight);//v1.4.7 : Total Weight 전달 (인자 추가)
				}
				else
				{
					float itp = 0.0f;
					if (controlParam._valueType == apControlParam.TYPE.Int)
					{
						itp = 1.0f - Mathf.Clamp01((float)(controlParam._int_Cur - _cpLerpPoint_A._iPos) / (float)(_cpLerpPoint_B._iPos - _cpLerpPoint_A._iPos));
					}
					else
					{
						itp = 1.0f - Mathf.Clamp01((float)(controlParam._float_Cur - _cpLerpPoint_A._pos.x) / (float)(_cpLerpPoint_B._pos.x - _cpLerpPoint_A._pos.x));
					}

					_cpLerpPoint_A._calculatedWeight = itp;
					_cpLerpPoint_B._calculatedWeight = 1.0f - itp;

					//v1.4.7 : Total Weight 전달 (인자 추가)
					_cpLerpPoint_A.CalculateITPWeight(ref _totalWeight);
					_cpLerpPoint_B.CalculateITPWeight(ref _totalWeight);
				}

				//v1.4.7에서 삭제. Weight 계산은 위에서 바로 계산한다. (CalculateITPWeight)
				//_totalWeight = 0.0f;

				////TODO : 이거 고민해볼것. 다시 루프 안돌고 바로 totalWeight를 계산할 수 있을까?
				//for (int i = 0; i < nSubParamKeyValues; i++)
				//{
				//	curParamKeyValue = _subParamKeyValues[i];

				//	if (!curParamKeyValue._isCalculated)
				//	{
				//		curParamKeyValue._weight = 0.0f;
				//		continue;
				//	}

				//	_totalWeight += curParamKeyValue._weight;
				//}

				if (_totalWeight > 0.0f)
				{
					//이전
					//for (int i = 0; i < nSubParamKeyValues; i++)
					//{
					//	curParamKeyValue = _subParamKeyValues[i];
					//	if (curParamKeyValue._isCalculated)
					//	{
					//		curParamKeyValue._weight /= _totalWeight;
					//	}
					//}

					//변경 v1.4.7 : 모든 ParamKeyValue가 아닌 계산된 리스트만 골라서 나누자 (개수 차이가 크다)
					if(_nCalculatedParamKeyValues > 0)
					{
						for (int i = 0; i < _nCalculatedParamKeyValues; i++)
						{
							curParamKeyValue = _calculatedParamKeyValues[i];
							curParamKeyValue._weight /= _totalWeight;
						}
					}
				}
			}
		}



		private void CalculateWeight_ControlParam_2D()
		{
			if (_keyParamSetGroup == null || _keyParamSetGroup._keyControlParam == null)
			{
				Debug.LogError("Key ParamSet Group is Null / Key Control Param Is null");
				return;
			}

			apControlParam controlParam = _keyParamSetGroup._keyControlParam;


			//int nSubParamKeyValues = _subParamKeyValues != null ? _subParamKeyValues.Count : 0;

			//1. Param의 Weight를 모두 0으로 세팅 (+ 연산으로 Weight를 추가하는 방식)
			//2. 어느 RectArea에 있는지 결정한다.
			//3. Rect 안에서 itp를 계산한다.
			apCalculatedResultParam.ParamKeyValueSet curParamKeyValue = null;
			
			//v1.4.7 삭제 : 이 함수 호출전에 이미 PKV는 초기화가 되었다.
			//for (int i = 0; i < nSubParamKeyValues; i++)
			//{
			//	curParamKeyValue = _subParamKeyValues[i];
			//	curParamKeyValue._weight = 0.0f;
			//	curParamKeyValue._isCalculated = false;//<<나중에 이것도 true로 올리자
			//}

			Vector2 curValue = controlParam._vec2_Cur;


			int nLerpAreas = _cpLerpAreas != null ? _cpLerpAreas.Count : 0;
			apCalculatedLerpArea curLerpArea = null;

			_totalWeight = 0.0f;//v1.4.7 : Weight 계산이 Calcuate 중간에 바로 발생

			if (_cpLerpAreaLastSelected == null || !_cpLerpAreaLastSelected.IsInclude(curValue))
			{
				//TODO : 이부분 성능 최적화 필요
				//여기서 delegate때문에 약간의 메모리 할당이 발생한다.
				//_cpLerpAreaLastSelected = _cpLerpAreas.Find(delegate (apCalculatedLerpArea a)
				//{
				//	return a.IsInclude(curValue);
				//});

				//성능은 떨어지지만 메모리 할당이 발생하지 않는 부분
				for (int i = 0; i < nLerpAreas; i++)
				{
					curLerpArea = _cpLerpAreas[i];
					if(curLerpArea.IsInclude(curValue))
					{
						_cpLerpAreaLastSelected = curLerpArea;
						break;
					}
				}
			}
			if (_cpLerpAreaLastSelected == null)
			{
				//잠깐 끕시더
				//Debug.LogError("No Lerp Area");
				return;//처리가 안되는데요;
			}

			_cpLerpAreaLastSelected.ReadyToCalculate();

			float itpX_L = 0.0f;
			float itpY_B = 0.0f;
			float rectPosX_L = _cpLerpAreaLastSelected._posMin.x;
			float rectPosX_R = _cpLerpAreaLastSelected._posMax.x;
			float rectPosY_T = _cpLerpAreaLastSelected._posMax.y;
			float rectPosY_B = _cpLerpAreaLastSelected._posMin.y;

			itpX_L = 1.0f - Mathf.Clamp01((curValue.x - rectPosX_L) / (rectPosX_R - rectPosX_L));
			itpY_B = 1.0f - Mathf.Clamp01((curValue.y - rectPosY_B) / (rectPosY_T - rectPosY_B));

			_cpLerpAreaLastSelected._pointLT._calculatedWeight = itpX_L * (1.0f - itpY_B);
			_cpLerpAreaLastSelected._pointRT._calculatedWeight = (1.0f - itpX_L) * (1.0f - itpY_B);
			_cpLerpAreaLastSelected._pointLB._calculatedWeight = itpX_L * itpY_B;
			_cpLerpAreaLastSelected._pointRB._calculatedWeight = (1.0f - itpX_L) * itpY_B;


			//v1.4.7 : Total Weight 전달 (인자 추가)
			_cpLerpAreaLastSelected._pointLT.CalculateITPWeight(ref _totalWeight);
			_cpLerpAreaLastSelected._pointRT.CalculateITPWeight(ref _totalWeight);
			_cpLerpAreaLastSelected._pointLB.CalculateITPWeight(ref _totalWeight);
			_cpLerpAreaLastSelected._pointRB.CalculateITPWeight(ref _totalWeight);


			//삭제 v1.4.7 : Weight 계산은 위에서 (CalculateITPWeight) 바로 계산한다.

			//변경 v1.4.7 : 모든 ParamKeyValue가 아닌 계산된 리스트만 골라서 나누자 (개수 차이가 크다)
			if (_totalWeight > 0.0f)
			{
				if (_nCalculatedParamKeyValues > 0)
				{
					for (int i = 0; i < _nCalculatedParamKeyValues; i++)
					{
						curParamKeyValue = _calculatedParamKeyValues[i];
						curParamKeyValue._weight /= _totalWeight;
					}
				}
			}
		}



		/// <summary>
		/// ParamSet간의 Weight를 계산한다. [Keyframe이 입력값인 경우]
		/// </summary>
		private void CalculateWeight_Keyframe()
		{
			if (_keyParamSetGroup == null || _keyParamSetGroup._keyAnimTimelineLayer == null)
			{
				return;
			}

			bool isPlayedAnimClip = false;//<<이 코드가 추가됨
			if (_keyParamSetGroup._keyAnimClip._isSelectedInEditor)
			{
				_keyParamSetGroup._layerWeight = 1.0f;
				isPlayedAnimClip = true;
			}
			else
			{
				_keyParamSetGroup._layerWeight = 0.0f;
				isPlayedAnimClip = false;
			}

			apAnimTimelineLayer timlineLayer = _keyParamSetGroup._keyAnimTimelineLayer;
			apCalculatedResultParam.ParamKeyValueSet curParamKeyValue = null;
			int curFrame = timlineLayer._parentAnimClip.CurFrame;

			bool isLoop = timlineLayer._parentAnimClip.IsLoop;
			

			_totalWeight = 0.0f;

			apAnimKeyframe curKeyframe = null;
			//apAnimKeyframe prevKeyframe = null;
			apAnimKeyframe nextKeyframe = null;

			int lengthFrames = timlineLayer._parentAnimClip.EndFrame - timlineLayer._parentAnimClip.StartFrame;
			int tmpCurFrame = 0;

			int nSubParamKeyValues = _subParamKeyValues != null ? _subParamKeyValues.Count : 0;


			//애니메이션 키프레임 PKV 순회 최적화 전략
			//- 이전에는 매번 모든 키프레임들을 돌면서, isCalculated와 weight를 계산했다.





			#region [미사용 코드] v1.4.7에서 개선됨.
			//for (int i = 0; i < nSubParamKeyValues; i++)
			//{
			//	curParamKeyValue = _subParamKeyValues[i];
			//	curParamKeyValue._dist = -10.0f;
			//	curParamKeyValue._isCalculated = false;

			//	//추가 11.29 : Animation Key 위치 타입이 추가되었다.
			//	curParamKeyValue._animKeyPos = apCalculatedResultParam.AnimKeyPos.NotCalculated;

			//	//유효하지 않은 키프레임이면 처리하지 않는다.
			//	if (curParamKeyValue._paramSet.SyncKeyframe == null ||
			//		!curParamKeyValue._paramSet.SyncKeyframe._isActive ||
			//		//!curParamKeyValue._isActive_InEditorExclusive
			//		!curParamKeyValue.IsActive ||
			//		!isPlayedAnimClip //<<애니메이션 재생 안될때는 여기서 생략이 되어야 하는데, 이게 왜 없었지;;
			//		)
			//	{
			//		//Debug.Log("[" + i + "] Not Active or Null Keyframe");
			//		continue;
			//	}

			//	curKeyframe = curParamKeyValue._paramSet.SyncKeyframe;
			//	prevKeyframe = curParamKeyValue._paramSet.SyncKeyframe._prevLinkedKeyframe;
			//	nextKeyframe = curParamKeyValue._paramSet.SyncKeyframe._nextLinkedKeyframe;


			//	//1. 프레임이 같다. => 100%
			//	if (curFrame == curKeyframe._frameIndex ||
			//		((curKeyframe._isLoopAsStart || curKeyframe._isLoopAsEnd) && curFrame == curKeyframe._loopFrameIndex))
			//	{
			//		curParamKeyValue._dist = 0.0f;
			//		curParamKeyValue._isCalculated = true;
			//		curParamKeyValue._weight = 1.0f;
			//		_totalWeight += 1.0f;

			//		//추가 11.29 : AnimKeyPos - 동일 프레임
			//		curParamKeyValue._animKeyPos = apCalculatedResultParam.AnimKeyPos.ExactKey;
			//	}
			//	//else if(curFrame >= curKeyframe._activeFrameIndexMin &&
			//	//		curFrame < curKeyframe._frameIndex)
			//	else if (curKeyframe.IsFrameIn(curFrame, apAnimKeyframe.LINKED_KEY.Prev))
			//	{
			//		//범위 안에 들었다. [Prev - Cur]
			//		if (prevKeyframe != null)
			//		{
			//			//v1.4.6 : 재생되는 프레임이 Prev 키프레임에 딱맞는 상태라면 보간을 생략한다.
			//			//["Loop + 마지막 키프레임" 버그 해결]
			//			if(prevKeyframe._frameIndex == curFrame) { continue; }

			//			//indexOffsetA = 0;
			//			//indexOffsetB = 0;
			//			//if(prevKeyframe._frameIndex > curKeyframe._frameIndex)
			//			//{
			//			//	//Loop인 경우 Prev가 더 클 수 있다.
			//			//	indexOffsetA = -lengthFrames;
			//			//}

			//			tmpCurFrame = curFrame;
			//			if (tmpCurFrame > curKeyframe._frameIndex)
			//			{
			//				tmpCurFrame -= lengthFrames;
			//			}

			//			//float itp = apAnimCurve.GetCurvedRelativeInterpolation(prevKeyframe._curveKey, curKeyframe._curveKey, curFrame, curKeyframe._curveKey._isPrevKeyUseDummyIndex, false);
			//			//float itp = apAnimCurve.GetCurvedRelativeInterpolation(curKeyframe._curveKey, prevKeyframe._curveKey, tmpCurFrame, true);

			//			//>> 변경
			//			float itp = curKeyframe._curveKey.GetItp_Int(tmpCurFrame, true);

			//			curParamKeyValue._dist = 0.0f;
			//			curParamKeyValue._isCalculated = true;
			//			curParamKeyValue._weight = itp;
			//			_totalWeight += itp;

			//			//추가 : Rotation Bias
			//			//Prev와 연결되었다면 Prev 설정을 적용한다.
			//			if (curKeyframe._prevRotationBiasMode != apAnimKeyframe.ROTATION_BIAS.None)
			//			{
			//				curParamKeyValue.SetAnimRotationBias(curKeyframe._prevRotationBiasMode, curKeyframe._prevRotationBiasCount);
			//			}

			//			//Debug.Log("[" + i + "] [Prev ~ Cur] " + itp);
			//			//Debug.Log("Prev ~ Next : " + itp);

			//			//추가 11.29 : AnimKeyPos - Next 프레임으로서 Prev 프레임과 보간이 된다.
			//			curParamKeyValue._animKeyPos = apCalculatedResultParam.AnimKeyPos.NextKey;
			//		}
			//		else
			//		{
			//			//연결된게 없다면 이게 100% 가중치를 갖는다.
			//			curParamKeyValue._dist = 0.0f;
			//			curParamKeyValue._isCalculated = true;
			//			curParamKeyValue._weight = 1.0f;
			//			_totalWeight += 1.0f;
			//			//Debug.Log("[" + i + "] [Prev ?? ~ Cur] 1.0");

			//			//추가 11.29 : AnimKeyPos - 동일 프레임
			//			curParamKeyValue._animKeyPos = apCalculatedResultParam.AnimKeyPos.ExactKey;
			//		}

			//	}
			//	//else if(curFrame > curKeyframe._frameIndex &&
			//	//		curFrame <= curKeyframe._activeFrameIndexMax)
			//	else if (curKeyframe.IsFrameIn(curFrame, apAnimKeyframe.LINKED_KEY.Next))
			//	{
			//		//범위안에 들었다 [Cur - Next]
			//		if (nextKeyframe != null)
			//		{
			//			//v1.4.6 : 재생되는 프레임이 Next 키프레임에 딱맞는 상태라면 보간을 생략한다.
			//			//["Loop + 마지막 키프레임" 버그 해결]
			//			if(nextKeyframe._frameIndex == curFrame) { continue; }

			//			//indexOffsetA = 0;
			//			//indexOffsetB = 0;
			//			//if(nextKeyframe._frameIndex < curKeyframe._frameIndex)
			//			//{
			//			//	//Loop인 경우 Next가 더 작을 수 있다.
			//			//	indexOffsetB = lengthFrames;
			//			//}

			//			tmpCurFrame = curFrame;
			//			if (tmpCurFrame < curKeyframe._frameIndex)
			//			{
			//				tmpCurFrame += lengthFrames;
			//			}

			//			//float itp = apAnimCurve.GetCurvedRelativeInterpolation(curKeyframe._curveKey, nextKeyframe._curveKey, curFrame, false, curKeyframe._curveKey._isNextKeyUseDummyIndex);
			//			//float itp = apAnimCurve.GetCurvedRelativeInterpolation(curKeyframe._curveKey, nextKeyframe._curveKey, tmpCurFrame, false);

			//			//>> 변경
			//			float itp = curKeyframe._curveKey.GetItp_Int(tmpCurFrame, false);

			//			//itp = 1.0f - itp;//결과가 B에 맞추어지므로 여기서는 Reverse

			//			curParamKeyValue._dist = 0.0f;
			//			curParamKeyValue._isCalculated = true;
			//			curParamKeyValue._weight = itp;
			//			_totalWeight += itp;

			//			//추가 : Rotation Bias
			//			//Next와 연결되었다면 Next 설정을 적용한다.
			//			if (curKeyframe._nextRotationBiasMode != apAnimKeyframe.ROTATION_BIAS.None)
			//			{
			//				curParamKeyValue.SetAnimRotationBias(curKeyframe._nextRotationBiasMode, curKeyframe._nextRotationBiasCount);
			//			}

			//			//추가 11.29 : AnimKeyPos - Prev 프레임으로서 Next 프레임과 보간이 된다.
			//			curParamKeyValue._animKeyPos = apCalculatedResultParam.AnimKeyPos.PrevKey;
			//		}
			//		else
			//		{
			//			//연결된게 없다면 이게 100% 가중치를 갖는다.
			//			curParamKeyValue._dist = 0.0f;
			//			curParamKeyValue._isCalculated = true;
			//			curParamKeyValue._weight = 1.0f;
			//			_totalWeight += 1.0f;

			//			//추가 11.29 : AnimKeyPos - 동일 프레임
			//			curParamKeyValue._animKeyPos = apCalculatedResultParam.AnimKeyPos.ExactKey;
			//		}
			//	}
			//} 
			#endregion


			//[v1.4.7] 애니메이션 조회시 캐시를 이용하여 불필요하게 "전체 순회"를 하지 말자
			bool isNeedFindNewCache = true;//이게 True라면 다시 전체 조회를 해야한다.

			if(!isPlayedAnimClip)
			{
				//v1.4.7에서는 Weight 초기화/isCalculated 등이 밖에서 이루어지므로,
				//애니메이션 재생 중이 아닌 경우 바로 종료 가능하다.
				return;
			}

			//캐시가 재활용 가능한지 체크하자
			//둘다 존재하는 경우 : 구역 체크
			//A 또는 B 하나만 존재하는 경우 : 단일 영역 체크 (루프가 아닌 경우 A없이 B만 존재하는 경우가 있다)
			if (_animCache_KeyframePKV_A != null && _animCache_KeyframePKV_B != null)
			{
				//[A (Cur) ~ B (Next) 둘다 존재한다. > 키프레임 사이에서 보간이 되는 중]
				if ( //[A - Cur]
					_animCache_KeyframePKV_A._paramSet != null
					&& _animCache_KeyframePKV_A._paramSet.SyncKeyframe != null

					//[B - Next]
					&& _animCache_KeyframePKV_B._paramSet != null
					&& _animCache_KeyframePKV_B._paramSet.SyncKeyframe != null)
				{
					//아래에서 이 키프레임들을 그대로 사용한다.
					curKeyframe = _animCache_KeyframePKV_A._paramSet.SyncKeyframe;
					nextKeyframe = _animCache_KeyframePKV_B._paramSet.SyncKeyframe;

					//연결된 키프레임들이 활성화되어 있고, 서로 연결된 상태여야 한다.
					if (curKeyframe._isActive
						&& nextKeyframe._isActive
						&& curKeyframe._nextLinkedKeyframe == nextKeyframe
						&& nextKeyframe._prevLinkedKeyframe == curKeyframe)
					{
						//이제 위치를 체크하자
						if (curKeyframe.IsFrameIn(curFrame, apAnimKeyframe.LINKED_KEY.Next)
							&& nextKeyframe.IsFrameIn(curFrame, apAnimKeyframe.LINKED_KEY.Prev))
						{
							//Cur ~ Next 위치가 맞다.
							isNeedFindNewCache = false;//캐시 재검색을 하지 않고 재활용을 하자
						}
					}
				}
			}
			else if(_animCache_KeyframePKV_A != null && _animCache_KeyframePKV_B == null)
			{
				//PKV가 A만 존재하고 B는 존재하지 않는 경우.
				//루프가 아닌 경우 A의 뒤쪽으로 키프레임이 없을 수 있다.
				if( _animCache_KeyframePKV_A._paramSet != null
					&& _animCache_KeyframePKV_A._paramSet.SyncKeyframe != null
					)
				{
					//아래에서 이 키프레임들을 그대로 사용한다.
					curKeyframe = _animCache_KeyframePKV_A._paramSet.SyncKeyframe;
					nextKeyframe = null;

					//연결된 키프레임들이 활성화되어 있고, 서로 연결된 상태여야 한다.
					if(curKeyframe._isActive)
					{
						//이제 위치를 체크하자
						if (curKeyframe.IsFrameIn(curFrame, apAnimKeyframe.LINKED_KEY.Next))
						{
							//Cur ~~~ 위치가 맞다.

							//만약 Next 키프레임이 있다면, 그것도 캐시로 넣고 연결하자
							if(curKeyframe._nextLinkedKeyframe != null)
							{								
								_animCache_KeyframePKV_B = null;
								_animKeyframe2ParamKeyValue.TryGetValue(curKeyframe._nextLinkedKeyframe, out _animCache_KeyframePKV_B);

								//B도 다시 캐시로서 연결했다.
								if(_animCache_KeyframePKV_B != null)
								{
									nextKeyframe = curKeyframe._nextLinkedKeyframe;	
								}
							}

							isNeedFindNewCache = false;//캐시 재검색을 하지 않고 재활용을 하자
						}
					}
				}
			}
			else if(_animCache_KeyframePKV_A == null && _animCache_KeyframePKV_B != null)
			{
				//PKV가 B (Next)만 존재하고 A (Cur)는 존재하지 않는 경우.
				//루프가 아닌 경우 첫번째 프레임의 경우 B에 해당하는 키프레임만 있을 수 있다.
				if( _animCache_KeyframePKV_B._paramSet != null
					&& _animCache_KeyframePKV_B._paramSet.SyncKeyframe != null
					)
				{
					//아래에서 이 키프레임들을 그대로 사용한다.
					curKeyframe = null;
					nextKeyframe = _animCache_KeyframePKV_B._paramSet.SyncKeyframe;

					//연결된 키프레임들이 활성화되어 있고, 서로 연결된 상태여야 한다.
					if(nextKeyframe._isActive)
					{
						//이제 위치를 체크하자
						if (nextKeyframe.IsFrameIn(curFrame, apAnimKeyframe.LINKED_KEY.Prev))//<- Next를 기준으로 이전 영역을 검색한다.
						{
							//~~~ Next 위치가 맞다.

							//만약 Cur 키프레임이 있다면, 그것도 캐시로 넣고 연결하자
							if(nextKeyframe._prevLinkedKeyframe != null)
							{								
								_animCache_KeyframePKV_A = null;
								_animKeyframe2ParamKeyValue.TryGetValue(nextKeyframe._prevLinkedKeyframe, out _animCache_KeyframePKV_A);

								//A도 다시 캐시로서 연결했다.
								if(_animCache_KeyframePKV_A != null)
								{
									curKeyframe = nextKeyframe._prevLinkedKeyframe;	
								}
							}

							isNeedFindNewCache = false;//캐시 재검색을 하지 않고 재활용을 하자
						}
					}
				}
			}

			//디버그
			//if(isNeedFindNewCache)
			//{
			//	Debug.LogError("캐시 미스");
			//}
			//else
			//{
			//	string strDebug = "[" + curFrame + "] 캐시 적중 : ";
			//	if(_animCache_KeyframePKV_A != null)
			//	{
			//		strDebug += "A : " + _animCache_KeyframePKV_A._paramSet.SyncKeyframe._frameIndex;
			//	}
			//	else
			//	{
			//		strDebug += "A : 없음";
			//	}

			//	if(_animCache_KeyframePKV_B != null)
			//	{
			//		strDebug += " / B : " + _animCache_KeyframePKV_B._paramSet.SyncKeyframe._frameIndex;
			//	}
			//	else
			//	{
			//		strDebug += " / B : 없음";
			//	}
			//	Debug.Log(strDebug);
			//}

			//캐시 미스가 발생하여, 다시 현재 프레임에 맞게 재검색을 해서 캐시를 생성해야한다.
			if(isNeedFindNewCache)
			{
				//만약 캐시된 키프레임이 있었다면, 그 전후로 한번 체크를 한다.
				bool isFindKeyframes = false;

				//- 캐시 A가 있다면, A의 이전을 검색한다.
				if(_animCache_KeyframePKV_A != null
					&& _animCache_KeyframePKV_A._paramSet != null
					&& _animCache_KeyframePKV_A._paramSet.SyncKeyframe != null)
				{
					apAnimKeyframe cacheKey_A = _animCache_KeyframePKV_A._paramSet.SyncKeyframe;
					//이전을 검색하자
					if(cacheKey_A.IsFrameIn(curFrame, apAnimKeyframe.LINKED_KEY.Prev))
					{
						//기존의 KeyA가 B (Next)가 되고, A는 존재하는지 찾자
						apCalculatedResultParam.ParamKeyValueSet cacheExistA = _animCache_KeyframePKV_A;
						_animCache_KeyframePKV_A = null;
						_animCache_KeyframePKV_B = cacheExistA;

						curKeyframe = null;
						nextKeyframe = cacheKey_A;

						if(nextKeyframe._prevLinkedKeyframe != null)
						{
							//이전 키프레임 (Next의 Prev는 Cur)이 있다면 새로운 A를 찾아서 연결
							curKeyframe = nextKeyframe._prevLinkedKeyframe;
							_animKeyframe2ParamKeyValue.TryGetValue(curKeyframe, out _animCache_KeyframePKV_A);
						}

						//Debug.LogWarning(">> 기존의 A의 Prev 구역에서 찾음 : " 
						//	+ (curKeyframe != null ? curKeyframe._frameIndex.ToString() : "[X]")
						//	+ " ~ "
						//	+ (nextKeyframe != null ? nextKeyframe._frameIndex.ToString() : "[X]")
						//	);

						isFindKeyframes = true;
					}
				}

				//- 캐시 B가 있다면, B의 다음을 검색한다.
				if(!isFindKeyframes
					&& _animCache_KeyframePKV_B != null
					&& _animCache_KeyframePKV_B._paramSet != null
					&& _animCache_KeyframePKV_B._paramSet.SyncKeyframe != null)
				{
					apAnimKeyframe cacheKey_B = _animCache_KeyframePKV_B._paramSet.SyncKeyframe;
					//이전을 검색하자
					if(cacheKey_B.IsFrameIn(curFrame, apAnimKeyframe.LINKED_KEY.Next))
					{
						//기존의 KeyB가 A (Cur)가 되고, B는 존재하는지 찾자
						apCalculatedResultParam.ParamKeyValueSet cacheExistB = _animCache_KeyframePKV_B;
						_animCache_KeyframePKV_A = cacheExistB;
						_animCache_KeyframePKV_B = null;

						curKeyframe = cacheKey_B;
						nextKeyframe = null;

						if(curKeyframe._nextLinkedKeyframe != null)
						{
							//다음 키프레임 (Next의 Next)이 있다면 새로운 B를 찾아서 연결
							nextKeyframe = curKeyframe._nextLinkedKeyframe;
							_animKeyframe2ParamKeyValue.TryGetValue(nextKeyframe, out _animCache_KeyframePKV_B);
						}

						//Debug.LogWarning(">> 기존의 B의 Next 구역에서 찾음 : " 
						//	+ (curKeyframe != null ? curKeyframe._frameIndex.ToString() : "[X]")
						//	+ " ~ "
						//	+ (nextKeyframe != null ? nextKeyframe._frameIndex.ToString() : "[X]")
						//	);

						isFindKeyframes = true;
					}
				}

				//- 위에서 찾지 못했다면, 처음부터 전부 검색을 한다.
				if(!isFindKeyframes)
				{
					_animCache_KeyframePKV_A = null;
					_animCache_KeyframePKV_B = null;

					apAnimKeyframe checkKey = null;

					for (int i = 0; i < nSubParamKeyValues; i++)
					{
						curParamKeyValue = _subParamKeyValues[i];
						if (curParamKeyValue._paramSet.SyncKeyframe == null)
						{
							continue;
						}
						checkKey = curParamKeyValue._paramSet.SyncKeyframe;
						if (!checkKey._isActive)
						{
							continue;
						}

						//앞에서부터 하나라도 있다면 된다.
						//Prev가 없다면, Prev 체크를 한다. 그 외에는 Next만 체크한다.
						if (checkKey._prevLinkedKeyframe == null)
						{
							if (checkKey.IsFrameIn(curFrame, apAnimKeyframe.LINKED_KEY.Prev))
							{
								//현재 키프레임의 이전 영역에서 재생중이다. (연결된 프레임 없음)
								//A (Cur)는 없고 B (Next)만 있는 상태이다.
								//루프 없는 첫 키프레임보다 이전에 프레임이 있는 경우
								curKeyframe = null;
								nextKeyframe = checkKey;

								_animKeyframe2ParamKeyValue.TryGetValue(nextKeyframe, out _animCache_KeyframePKV_B);
								isFindKeyframes = true;

								break;//탐색 종료
							}
						}

						//Next를 체크하자
						if (checkKey.IsFrameIn(curFrame, apAnimKeyframe.LINKED_KEY.Next))
						{
							//현재 프레임의 ~Next 영역에 해당한다.
							//A (Cur)는 있고, 연결된 B (Next)가 있는지 확인한다.
							curKeyframe = checkKey;
							nextKeyframe = null;

							_animKeyframe2ParamKeyValue.TryGetValue(curKeyframe, out _animCache_KeyframePKV_A);

							if (checkKey._nextLinkedKeyframe != null)
							{
								//연결된 Next Keyframe이 있다.
								nextKeyframe = checkKey._nextLinkedKeyframe;
								_animKeyframe2ParamKeyValue.TryGetValue(curKeyframe, out _animCache_KeyframePKV_A);

							}

							isFindKeyframes = true;
							break;//탐색 종료
						}
					}

					//if(isFindKeyframes)
					//{
					//	Debug.LogWarning(">> 전체 다시 검색 : " 
					//		+ (curKeyframe != null ? curKeyframe._frameIndex.ToString() : "[X]")
					//		+ " ~ "
					//		+ (nextKeyframe != null ? nextKeyframe._frameIndex.ToString() : "[X]")
					//		);
					//}
					//else
					//{
					//	Debug.LogWarning(">> 전체 다시 검색 : 찾지 못함");
					//}
				}

				
			}


			//이제 현재의 캐시 키프레임과 PKV을 대상으로 가중치와 Calculated를 하자
			
			curKeyframe = null;
			nextKeyframe = null;
			if(_animCache_KeyframePKV_A != null)
			{
				curKeyframe = _animCache_KeyframePKV_A._paramSet.SyncKeyframe;
			}
			if(_animCache_KeyframePKV_B != null)
			{
				nextKeyframe = _animCache_KeyframePKV_B._paramSet.SyncKeyframe;
			}

			//1. 정확한 프레임을 체크한다.
			bool isAnyCalculated = false;
			if(curKeyframe != null)
			{
				if(IsExactKeyFrame(curFrame, curKeyframe))
				{
					//키프레임 A에 프레임이 위치했다. 가중치 100%
					_animCache_KeyframePKV_A._dist = 0.0f;
					_animCache_KeyframePKV_A._weight = 1.0f;

					_totalWeight += 1.0f;

					//AnimKeyPos - 동일 프레임
					_animCache_KeyframePKV_A._animKeyPos = apCalculatedResultParam.AnimKeyPos.ExactKey;

					//계산되었다.
					OnParamKeyValueCalculated(_animCache_KeyframePKV_A, 1.0f);

					isAnyCalculated = true;
				}
			}

			if(!isAnyCalculated && nextKeyframe != null)
			{
				if(IsExactKeyFrame(curFrame, nextKeyframe))
				{
					//키프레임 B에 프레임이 위치했다. 가중치 100%
					_animCache_KeyframePKV_B._dist = 0.0f;
					_animCache_KeyframePKV_B._weight = 1.0f;

					_totalWeight += 1.0f;

					//AnimKeyPos - 동일 프레임
					_animCache_KeyframePKV_B._animKeyPos = apCalculatedResultParam.AnimKeyPos.ExactKey;

					//계산되었다.
					OnParamKeyValueCalculated(_animCache_KeyframePKV_B, 1.0f);

					isAnyCalculated = true;
				}
			}

			//2. 보간을 하자
			//다음의 순서로 체크
			//> Cur의 Prev
			//> Cur의 Next (Next Frame이 있다면 사이값)
			//> Next의 Next


			if(!isAnyCalculated && curKeyframe != null)
			{
				//> Cur의 Prev를 체크한다.
				if (curKeyframe.IsFrameIn(curFrame, apAnimKeyframe.LINKED_KEY.Prev))
				{
					//범위 안에 들었다. [Prev - Cur]
					//- 사실 이 조건은 캐시 처리에 의해서 발생하지 않는게 정상. 캐시 특성상 Prev - Cur를 앞으로 옮겨서 다시 Cur - Next로 만들 것
					//- 다만, 타임라인/키프레임 생성 등의 처리 과정에서 이 상태가 특수하게 나타날 수가 있다.

					//Cur Keyframe의 Prev가 있는지 확인한다.
					apAnimKeyframe prevKeyframe = curKeyframe._prevLinkedKeyframe;
					if (prevKeyframe != null)
					{
						//[ Prev - Cur ]에 들었다면						

						//Prev PKV를 찾자
						apCalculatedResultParam.ParamKeyValueSet prevPKVSet = null;
						_animKeyframe2ParamKeyValue.TryGetValue(prevKeyframe, out prevPKVSet);

						if (prevPKVSet != null)
						{
							//Prev PKV도 유효하다.
							tmpCurFrame = curFrame;
							if (tmpCurFrame > curKeyframe._frameIndex)
							{
								//루프된 상태라면 보간을 위해 프레임 변경
								tmpCurFrame -= lengthFrames;
							}

							//Lerp를 넣자
							float curLerp = curKeyframe._curveKey.GetItp_Int(tmpCurFrame, true);//True : Prev 키와 비교
							float prevLerp = 1.0f - curLerp;

							//[Prev]
							prevPKVSet._dist = 0.0f;
							prevPKVSet._weight = prevLerp;

							//[Cur]
							_animCache_KeyframePKV_A._dist = 0.0f;
							_animCache_KeyframePKV_A._weight = curLerp;

							_totalWeight += 1.0f;//Prev + Cur = 1

							//AnimKeyPos
							prevPKVSet._animKeyPos = apCalculatedResultParam.AnimKeyPos.PrevKey;
							_animCache_KeyframePKV_A._animKeyPos = apCalculatedResultParam.AnimKeyPos.NextKey;

							//계산되었다.
							OnParamKeyValueCalculated(prevPKVSet, prevLerp);
							OnParamKeyValueCalculated(_animCache_KeyframePKV_A, curLerp);

							//Debug.LogError("에러 : Prev - Cur 보간이 되었지만, 캐시 검색에서는 이 경우는 나와선 안된다.");

							isAnyCalculated = true;
						}
					}

					if(!isAnyCalculated)
					{
						//Prev-Keyframe이 없거나 Prev-PKV가 없다.
						//주로 Loop가 아닌 경우의 첫 키프레임의 경우
						
						_animCache_KeyframePKV_A._dist = 0.0f;
						_animCache_KeyframePKV_A._weight = 1.0f;

						_totalWeight += 1.0f;

						//AnimKeyPos - Cur는 Next 키가 된다.
						_animCache_KeyframePKV_A._animKeyPos = apCalculatedResultParam.AnimKeyPos.NextKey;

						//계산되었다.
						OnParamKeyValueCalculated(_animCache_KeyframePKV_A, 1.0f);
						isAnyCalculated = true;
					}
				}
				else if (curKeyframe.IsFrameIn(curFrame, apAnimKeyframe.LINKED_KEY.Next))
				{
					//> Cur의 Next를 체크한다.
					//Next Keyframe은 PKV-B일 것이다.
					if(nextKeyframe != null)
					{
						tmpCurFrame = curFrame;
						if (tmpCurFrame < curKeyframe._frameIndex)
						{
							tmpCurFrame += lengthFrames;
						}

						//Prev PKV도 유효하다.
						//Lerp를 넣자
						float curLerp = curKeyframe._curveKey.GetItp_Int(tmpCurFrame, false);//False : Next 키와 비교
						float nextLerp = 1.0f - curLerp;

						//[Cur - A]
						_animCache_KeyframePKV_A._dist = 0.0f;
						_animCache_KeyframePKV_A._weight = curLerp;

						//[Next - B]
						_animCache_KeyframePKV_B._dist = 0.0f;
						_animCache_KeyframePKV_B._weight = nextLerp;

						_totalWeight += 1.0f;//Cur + Next = 1

						//AnimKeyPos						
						_animCache_KeyframePKV_A._animKeyPos = apCalculatedResultParam.AnimKeyPos.PrevKey;
						_animCache_KeyframePKV_B._animKeyPos = apCalculatedResultParam.AnimKeyPos.NextKey;

						//계산되었다.						
						OnParamKeyValueCalculated(_animCache_KeyframePKV_A, curLerp);
						OnParamKeyValueCalculated(_animCache_KeyframePKV_B, nextLerp);
						isAnyCalculated = true;
					}
					else
					{
						//Next가 없다면 Cur에 100%가 적용된다.
						_animCache_KeyframePKV_A._dist = 0.0f;
						_animCache_KeyframePKV_A._weight = 1.0f;

						_totalWeight += 1.0f;

						//AnimKeyPos - Cur는 Prev 키가 된다.
						_animCache_KeyframePKV_A._animKeyPos = apCalculatedResultParam.AnimKeyPos.PrevKey;

						//계산되었다.
						OnParamKeyValueCalculated(_animCache_KeyframePKV_A, 1.0f);
						isAnyCalculated = true;
					}
				}
			}

			//Cur Keyframe에서의 보간이 안되었다면, Next Keyframe (PKV-B)에서 보간을 체크하자.
			if(!isAnyCalculated && nextKeyframe != null)
			{				
				if (nextKeyframe.IsFrameIn(curFrame, apAnimKeyframe.LINKED_KEY.Prev))
				{
					//v1.4.8 버그 수정
					//> Next의 Prev를 비교한다. (Loop가 아닌 경우 첫 키프레임보다 앞인 경우엔 null--NextKey인 경우가 존재한다.

					//범위 안에 들었다. [Prev - Cur]
					//- 사실 이 조건은 캐시 처리에 의해서 발생하지 않는게 정상. 캐시 특성상 Prev - Cur를 앞으로 옮겨서 다시 Cur - Next로 만들 것
					//- 다만, 타임라인/키프레임 생성 등의 처리 과정에서 이 상태가 특수하게 나타날 수가 있다.

					//Cur Keyframe의 Prev가 있는지 확인한다.
					apAnimKeyframe prevKeyframe = nextKeyframe._prevLinkedKeyframe;
					if (prevKeyframe != null)
					{
						//[ Prev - Next ]에 들었다면

						//Prev PKV를 찾자
						apCalculatedResultParam.ParamKeyValueSet prevPKVSet = null;
						_animKeyframe2ParamKeyValue.TryGetValue(prevKeyframe, out prevPKVSet);

						if (prevPKVSet != null)
						{
							//Prev PKV도 유효하다.
							tmpCurFrame = curFrame;
							if (tmpCurFrame > nextKeyframe._frameIndex)
							{
								//루프된 상태라면 보간을 위해 프레임 변경
								tmpCurFrame -= lengthFrames;
							}

							//Lerp를 넣자
							float curLerp = nextKeyframe._curveKey.GetItp_Int(tmpCurFrame, true);//True : Prev 키와 비교
							float prevLerp = 1.0f - curLerp;

							//[Prev]
							prevPKVSet._dist = 0.0f;
							prevPKVSet._weight = prevLerp;

							//[Next]
							_animCache_KeyframePKV_B._dist = 0.0f;
							_animCache_KeyframePKV_B._weight = curLerp;

							_totalWeight += 1.0f;//Prev + Cur = 1

							//AnimKeyPos
							prevPKVSet._animKeyPos = apCalculatedResultParam.AnimKeyPos.PrevKey;
							_animCache_KeyframePKV_B._animKeyPos = apCalculatedResultParam.AnimKeyPos.NextKey;

							//계산되었다.
							OnParamKeyValueCalculated(prevPKVSet, prevLerp);
							OnParamKeyValueCalculated(_animCache_KeyframePKV_B, curLerp);

							//Debug.LogError("에러 : Prev - Cur 보간이 되었지만, 캐시 검색에서는 이 경우는 나와선 안된다.");

							isAnyCalculated = true;
						}
					}

					if(!isAnyCalculated)
					{
						//Prev-Keyframe이 없거나 Prev-PKV가 없다.
						//주로 Loop가 아닌 경우의 첫 키프레임의 경우
						
						_animCache_KeyframePKV_B._dist = 0.0f;
						_animCache_KeyframePKV_B._weight = 1.0f;

						_totalWeight += 1.0f;

						//AnimKeyPos - Cur는 Next 키가 된다.
						_animCache_KeyframePKV_B._animKeyPos = apCalculatedResultParam.AnimKeyPos.NextKey;

						//계산되었다.
						OnParamKeyValueCalculated(_animCache_KeyframePKV_B, 1.0f);
						isAnyCalculated = true;
					}
				}
				else if (nextKeyframe.IsFrameIn(curFrame, apAnimKeyframe.LINKED_KEY.Next))
				{
					//> Next의 Next를 비교한다.
					//[Next - Next2의 영역에서 보간된다.]
					//에러 : 보간은 되었지만 캐시 검색에서는 이 경우는 나와서는 안된다.

					//Next Keyframe의 Next(2)가 있는지 확인한다.
					apAnimKeyframe next2Keyframe = nextKeyframe._nextLinkedKeyframe;
					if(next2Keyframe != null)
					{
						// [Next - Next2 ]에 들었다면

						//Next2 PKV를 찾자
						apCalculatedResultParam.ParamKeyValueSet next2PKVSet = null;
						_animKeyframe2ParamKeyValue.TryGetValue(next2Keyframe, out next2PKVSet);

						if (next2PKVSet != null)
						{
							//Next2 PKV도 유효하다.
							tmpCurFrame = curFrame;
							if (tmpCurFrame < nextKeyframe._frameIndex)
							{
								//루프된 상태라면 보간을 위해 프레임 변경
								tmpCurFrame += lengthFrames;
							}

							//Lerp를 넣자
							float nextLerp = nextKeyframe._curveKey.GetItp_Int(tmpCurFrame, false);//False : Next 키와 비교
							float next2Lerp = 1.0f - nextLerp;

							//[Next - B]
							_animCache_KeyframePKV_B._dist = 0.0f;
							_animCache_KeyframePKV_B._weight = nextLerp;

							//[Next2]
							next2PKVSet._dist = 0.0f;
							next2PKVSet._weight = next2Lerp;

							_totalWeight += 1.0f;//Next + Next2 = 1

							//AnimKeyPos
							_animCache_KeyframePKV_B._animKeyPos = apCalculatedResultParam.AnimKeyPos.PrevKey;
							next2PKVSet._animKeyPos = apCalculatedResultParam.AnimKeyPos.NextKey;

							//계산되었다.
							OnParamKeyValueCalculated(_animCache_KeyframePKV_B, nextLerp);
							OnParamKeyValueCalculated(next2PKVSet, next2Lerp);

							//Debug.LogError("에러 : Next - Next2 보간이 되었지만, 캐시 검색에서는 이 경우는 나와선 안된다.");

							isAnyCalculated = true;
						}
					}

					if(!isAnyCalculated)
					{
						//Next2-Keyframe이 없거나 Next2-PKV가 없다.
						//주로 Loop가 아닌 경우의 마지막 키프레임의 경우
						
						_animCache_KeyframePKV_B._dist = 0.0f;
						_animCache_KeyframePKV_B._weight = 1.0f;

						_totalWeight += 1.0f;

						//AnimKeyPos - Next는 Prev 키가 된다.
						_animCache_KeyframePKV_B._animKeyPos = apCalculatedResultParam.AnimKeyPos.PrevKey;

						//계산되었다.
						OnParamKeyValueCalculated(_animCache_KeyframePKV_B, 1.0f);
						isAnyCalculated = true;
					}
				}
			}
			
			//Total Weight는 나누지 않아도 된다.
			//if (_totalWeight > 0.0f)
			//{
			//	//Debug.Log("Result --------------------------------");
			//	//float prevWeight = 0.0f;
			//	for (int i = 0; i < nSubParamKeyValues; i++)
			//	{
			//		curParamKeyValue = _subParamKeyValues[i];

			//		if (curParamKeyValue._isCalculated)
			//		{
			//			curParamKeyValue._weight /= _totalWeight;
			//		}
			//		else
			//		{
			//			curParamKeyValue._weight = 0.0f;
			//		}
			//	}
			//	//Debug.Log("-------------------------------------");
			//}
		}


		/// <summary>
		/// 현재 재생중인 프레임 인덱스에 해당 키프레임이 위치하는가.
		/// </summary>
		private bool IsExactKeyFrame(int curFrameIndex, apAnimKeyframe targetKeyframe)
		{
			return (curFrameIndex == targetKeyframe._frameIndex)
				|| ((targetKeyframe._isLoopAsStart || targetKeyframe._isLoopAsEnd) && curFrameIndex == targetKeyframe._loopFrameIndex);
		}

		private void CalculateWeight_Static()
		{
			//계산할 필요가 없는데용...

			//[삭제 v1.4.7 : 필요없는 계산이다. ]
			//if (_keyParamSetGroup == null)
			//{
			//	Debug.LogError("Key ParamSet Group is Null / Key Control Param Is null");
			//	return;
			//}

			//apCalculatedResultParam.ParamKeyValueSet curParamKeyValue = null;
			//int nSubParamKeyValues = _subParamKeyValues != null ? _subParamKeyValues.Count : 0;
			//for (int i = 0; i < nSubParamKeyValues; i++)
			//{
			//	curParamKeyValue = _subParamKeyValues[i];
			//	curParamKeyValue._weight = 1.0f;
			//	curParamKeyValue._isCalculated = true;//<<나중에 이것도 true로 올리자
			//}
		}

		



		//Control Param 보간 관련
		//--------------------------------------------------------------------------------------
		private void MakeControlParamLerpAreas()
		{
			//1. ParamSetKeyValue => Point를 만든다.

			_cpLerpAreas.Clear();
			_cpLerpPoints.Clear();

			apControlParam controlParam = _keyParamSetGroup._keyControlParam;
			if (controlParam == null)
			{
				return;
			}

			List<float> fPosXList = new List<float>();
			List<float> fPosYList = new List<float>();

			float bias = 0.001f;

			if (controlParam._valueType == apControlParam.TYPE.Float)
			{
				bias = Mathf.Abs((controlParam._float_Max - controlParam._float_Min) * 0.05f);
				bias = Mathf.Clamp(bias, 0.0001f, 0.1f);
			}
			else if (controlParam._valueType == apControlParam.TYPE.Vector2)
			{
				bias = Mathf.Min(Mathf.Abs((controlParam._vec2_Max.x - controlParam._vec2_Min.x) * 0.05f),
									Mathf.Abs((controlParam._vec2_Max.y - controlParam._vec2_Min.y) * 0.05f));
				bias = Mathf.Clamp(bias, 0.0001f, 0.1f);
			}


			for (int i = 0; i < _subParamKeyValues.Count; i++)
			{
				apCalculatedResultParam.ParamKeyValueSet keyValueSet = _subParamKeyValues[i];

				if (keyValueSet._paramSet == null)
				{ continue; }

				apCalculatedLerpPoint newPoint = null;
				switch (controlParam._valueType)
				{
					case apControlParam.TYPE.Int:
						{
							int iPos = keyValueSet._paramSet._conSyncValue_Int;
							newPoint = new apCalculatedLerpPoint(iPos, true, this);//v1.4.7 : 생성자 인자 변경
						}
						break;

					case apControlParam.TYPE.Float:
						{
							float fPos = keyValueSet._paramSet._conSyncValue_Float;
							newPoint = new apCalculatedLerpPoint(fPos, true, this);//v1.4.7 : 생성자 인자 변경
						}
						break;

					case apControlParam.TYPE.Vector2:
						{
							Vector2 vPos = keyValueSet._paramSet._conSyncValue_Vector2;
							newPoint = new apCalculatedLerpPoint(vPos, true, this);//v1.4.7 : 생성자 인자 변경

							//위치를 저장해둔다.
							AddLerpPos(vPos, fPosXList, fPosYList, bias);
						}
						break;
				}

				newPoint.AddPoint(keyValueSet, 1.0f);//실제 키는 Weight가 1이다.
				_cpLerpPoints.Add(newPoint);

			}


			//2-1 1차원 값이면 오름차순 정렬하는 걸로 끝
			switch (controlParam._valueType)
			{
				case apControlParam.TYPE.Int:
					_cpLerpPoints.Sort(delegate (apCalculatedLerpPoint a, apCalculatedLerpPoint b)
					{
						return a._iPos - b._iPos;
					});
					break;

				case apControlParam.TYPE.Float:
					_cpLerpPoints.Sort(delegate (apCalculatedLerpPoint a, apCalculatedLerpPoint b)
					{
						return (int)((a._pos.x - b._pos.x) * (1.0f / bias) * 100.0f);
					});
					break;
			}


			//2-2. (Vector2인 경우) Rect Area를 만들자.
			if (controlParam._valueType == apControlParam.TYPE.Vector2)
			{
				//1) Min, Max 위치에 대해서 확인 후 가상 포인트를 추가하자
				//2) X, Y 값에 대해서 정렬
				//3) X, Y 좌표를 순회하면서 "포인트가 없다면" 가상 포인트를 추가하자
				//4) X, Y 좌표 순회하면서 RectArea를 만들자.

				//1)
				float minX = controlParam._vec2_Min.x;
				float minY = controlParam._vec2_Min.y;
				float maxX = controlParam._vec2_Max.x;
				float maxY = controlParam._vec2_Max.y;

				MakeVirtualLerpPoint(new Vector2(minX, minY), bias);
				MakeVirtualLerpPoint(new Vector2(minX, minY), bias);
				MakeVirtualLerpPoint(new Vector2(minX, minY), bias);
				MakeVirtualLerpPoint(new Vector2(minX, minY), bias);

				//Min/Max 위치를 추가로 저장해둔다.
				AddLerpPos(new Vector2(minX, minY), fPosXList, fPosYList, bias);
				AddLerpPos(new Vector2(minX, maxY), fPosXList, fPosYList, bias);
				AddLerpPos(new Vector2(maxX, minY), fPosXList, fPosYList, bias);
				AddLerpPos(new Vector2(maxX, maxY), fPosXList, fPosYList, bias);

				//2) 위치 정렬
				//이전
				//fPosXList.Sort(delegate (float a, float b)
				//{
				//	return (int)((a - b) * (1.0f / bias) * 1000.0f);
				//});

				//fPosYList.Sort(delegate (float a, float b)
				//{
				//	return (int)((a - b) * (1.0f / bias) * 1000.0f);
				//});

				//변경 v1.5.0
				s_SortAreaAxis_Bias = bias;
				fPosXList.Sort(s_SortAreaAxisWithBias_Func);
				fPosYList.Sort(s_SortAreaAxisWithBias_Func);


				//3) 좌표 순회하면서 포인트 추가
				int nPosXList = fPosXList.Count;
				int nPosYList = fPosYList.Count;
				for (int iX = 0; iX < nPosXList; iX++)
				{
					for (int iY = 0; iY < nPosYList; iY++)
					{
						MakeVirtualLerpPoint(new Vector2(fPosXList[iX], fPosYList[iY]), bias);
					}
				}

				
				apCalculatedLerpPoint pointLB = null;
				apCalculatedLerpPoint pointRB = null;
				apCalculatedLerpPoint pointLT = null;
				apCalculatedLerpPoint pointRT = null;

				bool isLimit_L = false;
				bool isLimit_R = false;
				bool isLimit_T = false;
				bool isLimit_B = false;

				float posL = 0.0f;
				float posR = 0.0f;
				float posB = 0.0f;
				float posT = 0.0f;

				//4) 좌표 순회하면서 RectArea 만들기
				for (int iX = 0; iX < nPosXList - 1; iX++)
				{
					posL = fPosXList[iX];
					posR = fPosXList[iX + 1];
					
					isLimit_L = (iX == 0);
					isLimit_R = (iX == (nPosXList - 2));//iX + 1 == nPosXList - 1

					for (int iY = 0; iY < nPosYList - 1; iY++)
					{
						posB = fPosYList[iY];
						posT = fPosYList[iY + 1];
						
						isLimit_B = (iY == 0);
						isLimit_T = (iY == (nPosYList - 2));//iX + 1 == nPosXList - 1

						//pointLT = GetLerpPoint(new Vector2(fPosXList[iX], fPosYList[iY]), bias);
						//pointRT = GetLerpPoint(new Vector2(fPosXList[iX + 1], fPosYList[iY]), bias);
						//pointLB = GetLerpPoint(new Vector2(fPosXList[iX], fPosYList[iY + 1]), bias);
						//pointRB = GetLerpPoint(new Vector2(fPosXList[iX + 1], fPosYList[iY + 1]), bias);
						
						pointLB = GetLerpPoint(new Vector2(posL, posB), bias);
						pointRB = GetLerpPoint(new Vector2(posR, posB), bias);
						pointLT = GetLerpPoint(new Vector2(posL, posT), bias);
						pointRT = GetLerpPoint(new Vector2(posR, posT), bias);

						apCalculatedLerpArea lerpArea = new apCalculatedLerpArea(
							pointLT, pointRT, pointLB, pointRB,
							isLimit_L, isLimit_R, isLimit_T, isLimit_B);

						_cpLerpAreas.Add(lerpArea);
					}
				}

			}
		}

		private static float s_SortAreaAxis_Bias = 0.0f;
		private static Comparison<float> s_SortAreaAxisWithBias_Func = FUNC_SortAreaAxisWithBias;
		private static int FUNC_SortAreaAxisWithBias(float a, float b)
		{
			return (int)((a - b) * (1.0f / s_SortAreaAxis_Bias) * 1000.0f);
		}

		//삭제 v1.5.0 (미사용)
		//private apCalculatedLerpPoint GetLerpPoint(int iPos)
		//{
		//	return _cpLerpPoints.Find(delegate (apCalculatedLerpPoint a)
		//	{
		//		return a._iPos == iPos;
		//	});
		//}

		//private apCalculatedLerpPoint GetLerpPoint(float fPos, float bias)
		//{
		//	return _cpLerpPoints.Find(delegate (apCalculatedLerpPoint a)
		//	{
		//		return Mathf.Abs(a._pos.x - fPos) < bias;
		//	});
		//}

		private apCalculatedLerpPoint GetLerpPoint(Vector2 vPos, float bias)
		{
			//이전 (GC 발생)
			//return _cpLerpPoints.Find(delegate (apCalculatedLerpPoint a)
			//{
			//	return Mathf.Abs(a._pos.x - vPos.x) < bias &&
			//			Mathf.Abs(a._pos.y - vPos.y) < bias;
			//});

			//변경 v1.5.0
			s_FindLerpPoint_Vector = vPos;
			s_FindLerpPoint_Bias = bias;
			return _cpLerpPoints.Find(s_FindLerpPointByVector_Func);
		}

		private static Vector2 s_FindLerpPoint_Vector = Vector2.zero;
		private static float s_FindLerpPoint_Bias = 0.0f;
		private static Predicate<apCalculatedLerpPoint> s_FindLerpPointByVector_Func = FUNC_FindLerpPointByVector;
		private static bool FUNC_FindLerpPointByVector(apCalculatedLerpPoint a)
		{
			return Mathf.Abs(a._pos.x - s_FindLerpPoint_Vector.x) < s_FindLerpPoint_Bias &&
					Mathf.Abs(a._pos.y - s_FindLerpPoint_Vector.y) < s_FindLerpPoint_Bias;
		}


		private void AddLerpPos(Vector2 pos, List<float> posXList, List<float> posYList, float bias)
		{
			//이전 (GC 발생)
			//bool isExistPosX = posXList.Exists(delegate (float a)
			//{
			//	return Mathf.Abs(a - pos.x) < bias;
			//});

			//변경 (v1.5.0)
			s_ExistLerpPos_Point = pos.x;//X 부터 체크
			s_ExistLerpPos_Bias = bias;
			bool isExistPosX = posXList.Exists(s_ExistLerpPos_Func);

			if (!isExistPosX)
			{
				posXList.Add(pos.x);
			}

			//이전 (GC 발생)
			//bool isExistPosY = posYList.Exists(delegate (float a)
			//{
			//	return Mathf.Abs(a - pos.y) < bias;
			//});

			//변경 v1.5.0
			s_ExistLerpPos_Point = pos.y;//Y 체크 (Bias는 생략)
			bool isExistPosY = posYList.Exists(s_ExistLerpPos_Func);

			if (!isExistPosY)
			{
				posYList.Add(pos.y);
			}
		}

		private static float s_ExistLerpPos_Point = 0.0f;
		private static float s_ExistLerpPos_Bias = 0.0f;
		private static Predicate<float> s_ExistLerpPos_Func = FUNC_ExistLerpPos;
		private static bool FUNC_ExistLerpPos(float a)
		{
			return Mathf.Abs(a - s_ExistLerpPos_Point) < s_ExistLerpPos_Bias;
		}

		/// <summary>
		/// 가상의 Lerp Point (Vector2)를 만든다.
		/// bias값을 이용하여 기존에 생성된 값이 있는지 확인한다.
		/// 기존에 생성된 값이나 새로 만든 값을 리턴한다.
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="bias"></param>
		/// <returns></returns>
		private apCalculatedLerpPoint MakeVirtualLerpPoint(Vector2 pos, float bias)
		{
			apCalculatedLerpPoint existLerpPoint = GetLerpPoint(pos, bias);
			if (existLerpPoint != null)
			{
				return existLerpPoint;
			}
			apCalculatedLerpPoint newPoint = new apCalculatedLerpPoint(pos, false, this);//v1.4.7 : 생성자 인자 변경
			_cpLerpPoints.Add(newPoint);

			//실제 Control Param Key를 입력해야한다.

			//이전 (GC 발생)
			//List<apCalculatedLerpPoint> realLerpPoints = _cpLerpPoints.FindAll(delegate (apCalculatedLerpPoint a)
			//{
			//	return a._isRealPoint;
			//});

			//변경 v1.5.0
			List<apCalculatedLerpPoint> realLerpPoints = _cpLerpPoints.FindAll(s_FindRealPoint_Func);

			if (realLerpPoints.Count == 0)
			{
				return newPoint;
			}

			if (realLerpPoints.Count == 1)
			{
				newPoint.AddPoints(realLerpPoints[0], 1.0f);
				newPoint.NormalizeWeights();
				return newPoint;
			}


			//Pos를 기준으로 Lerp의 합을 계산한다.
			//전체 거리의 평균을 잡고, 그 평균 이내의 Point만 계산한다.

			List<float> distList = new List<float>();
			List<float> weightList = new List<float>();
			//float totalDist = 0.0f;
			float totalWeight = 0.0f;

			//Real 포인트들에 대해서 2개씩 짝을 지은 모든 경우의 수에 대해
			//A <-> B의 거리비 (역수)를 입력하여 Weight를 점차 감산하는 식으로 계산한다.
			//처음엔 Weight가 1로 시작하다가 거리 비에 의하여 줄어들고, 전체 Weight를 연산하여 Normalize한다.

			apCalculatedLerpPoint lerpPoint = null;
			for (int i = 0; i < realLerpPoints.Count; i++)
			{
				lerpPoint = realLerpPoints[i];
				float dist = Vector2.Distance(pos, lerpPoint._pos);

				distList.Add(dist);
				weightList.Add(1.0f);
			}

			apCalculatedLerpPoint curPoint = null;
			apCalculatedLerpPoint nextpoint = null;

			for (int iCur = 0; iCur < realLerpPoints.Count - 1; iCur++)
			{
				curPoint = realLerpPoints[iCur];

				if (weightList[iCur] <= 0.0f)
				{
					continue;
				}

				float distCur = distList[iCur];

				for (int iNext = iCur + 1; iNext < realLerpPoints.Count; iNext++)
				{
					nextpoint = realLerpPoints[iNext];
					if (weightList[iNext] <= 0.0f)
					{
						continue;
					}

					float distNext = distList[iNext];

					float distSum = distCur + distNext;

					if (distSum <= 0.0f)
					{
						continue;
					}

					float itp = 1.0f - (distCur / distSum);
					weightList[iCur] *= itp;
					weightList[iNext] *= 1.0f - itp;
				}
			}


			for (int i = 0; i < realLerpPoints.Count - 1; i++)
			{
				if (weightList[i] < 0.0f)
				{
					weightList[i] = 0.0f;
				}
				else
				{
					totalWeight += weightList[i];
				}
			}

			if (totalWeight > 0.0f)
			{
				for (int i = 0; i < realLerpPoints.Count; i++)
				{
					lerpPoint = realLerpPoints[i];
					if (weightList[i] > 0.0f)
					{
						float pointWeight = weightList[i] / totalWeight;

						newPoint.AddPoints(lerpPoint, pointWeight);
					}
				}
			}

			newPoint.NormalizeWeights();//추가 v1.5.0
			return newPoint;
		}

		private static Predicate<apCalculatedLerpPoint> s_FindRealPoint_Func = FUNC_FindRealPoint;
		private static bool FUNC_FindRealPoint(apCalculatedLerpPoint a)
		{
			return a._isRealPoint;
		}
	}

}
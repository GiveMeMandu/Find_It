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
	/// AnimClip 데이터 계산 후, 어떤 Control Param을 컨트롤하여 어떤 값을 가지게 할지 결정하는 데이터
	/// Keyframe 보간 계산 결과값이 들어간다.
	/// 멤버는 Sort시 미리 만든다.
	/// </summary>
	public class apAnimControlParamResult
	{
		// Members
		//--------------------------------------------------------------------
		public apControlParam _targetControlParam = null;
		public bool _isCalculated = false;
		public float _value_Int = 0;//변경 : Int지만 보간을 위해 Float형
		public float _value_Float = 0.0f;
		public Vector2 _value_Vec2 = Vector2.zero;

		//추가 v1.4.5
		//인덱스형 타입의 Int 파라미터라면, 보간이 아니라, KeyA, KeyB에서의 Int값과 Weight를 받는다.
		private bool _isIndexValueAssigned_A = false;
		private int _value_Index_A = -1;
		private float _weight_Index_A = -1.0f;

		private bool _isIndexValueAssigned_B = false;
		private int _value_Index_B = -1;
		private float _weight_Index_B = -1.0f;

		
		//추가 20.4.19
		//애니메이션의 정보를 추가한다.
		public apAnimClip _parentAnimClip = null;
		public int _animPlayOrder = 0;
		public float _animWeight = 0.0f;
		public apAnimPlayUnit.BLEND_METHOD _animBlendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation;
		
		public enum RESULT_KEY
		{
			KeyA, KeyB
		}

		// Init
		//--------------------------------------------------------------------
		public apAnimControlParamResult(apControlParam targetControlParam, apAnimClip parentAnimClip)
		{
			_targetControlParam = targetControlParam;
			_parentAnimClip = parentAnimClip;
		}


		//--------------------------------------------------------------------
		public void Init()
		{
			//_weight = 0.0f;
			_isCalculated = false;
			_value_Int = 0.0f;
			_value_Float = 0.0f;
			_value_Vec2 = Vector2.zero;

			//추가 v1.4.5 : 인덱스 값
			_isIndexValueAssigned_A = false;
			_value_Index_A = -1;
			_weight_Index_A = -1.0f;

			_isIndexValueAssigned_B = false;
			_value_Index_B = -1;
			_weight_Index_B = -1.0f;
		}


		// Set Calculated Value
		//--------------------------------------------------------------------
		public void SetKeyframeResult(apAnimKeyframe keyframe, float weight, RESULT_KEY keyType)
		{
			//_weight = Mathf.Clamp01(_weight + weight);


			switch (_targetControlParam._valueType)
			{
				case apControlParam.TYPE.Int:
					{
						//_value_Int += (int)(keyframe._conSyncValue_Int * weight + 0.5f);//기존 : 버그 있다.
						//일단 이게 없어야 함 + Float형으로 바뀌었다.

						//이전 : 버그
						//_value_Int += (int)(keyframe._conSyncValue_Int * weight);

						//버그 수정 : 20.4.15
						_value_Int += (float)keyframe._conSyncValue_Int * weight;
						
						//인덱스인 경우도 감안해서 할당한다.

						if(keyType == RESULT_KEY.KeyA)
						{
							//Key A (Prev)인 경우
							_isIndexValueAssigned_A = true;
							_value_Index_A = keyframe._conSyncValue_Int;
							_weight_Index_A = weight;
						}
						else
						{
							//Key B (Next)인 경우
							_isIndexValueAssigned_B = true;
							_value_Index_B = keyframe._conSyncValue_Int;
							_weight_Index_B = weight;
						}
					}
					break;


				case apControlParam.TYPE.Float:
					_value_Float += keyframe._conSyncValue_Float * weight;
					break;

				case apControlParam.TYPE.Vector2:
					_value_Vec2 += keyframe._conSyncValue_Vector2 * weight;
					break;
			}

			_isCalculated = true;
		}


		/// <summary>
		/// 1개의 키프레임만 선택되어 적용되는 경우 (가중치가 1이므로 따로 입력받지 않는다.)
		/// </summary>
		/// <param name="keyframe"></param>
		public void SetKeyframeResult_SingleKey(apAnimKeyframe keyframe)
		{
			switch (_targetControlParam._valueType)
			{
				case apControlParam.TYPE.Int:
					{
						_value_Int = (float)keyframe._conSyncValue_Int;

						//인덱스인 경우도 처리
						//A, B를 같게 만들고, A의 값을 가지도록 할당
						_isIndexValueAssigned_A = true;
						_value_Index_A = keyframe._conSyncValue_Int;
						_weight_Index_A = 1.0f;

						_isIndexValueAssigned_B = true;
						_value_Index_B = _value_Index_A;
						_weight_Index_B = 0.0f;
					}
					
					break;

				case apControlParam.TYPE.Float:
					_value_Float = keyframe._conSyncValue_Float;
					break;

				case apControlParam.TYPE.Vector2:
					_value_Vec2 = keyframe._conSyncValue_Vector2;
					break;
			}

			_isCalculated = true;
		}








		public void AdaptToControlParam()
		{
			if (!_isCalculated)
			{
				return;
			}

			switch (_targetControlParam._valueType)
			{
				case apControlParam.TYPE.Int:
					{
						//Int의 경우 보간 여부가 옵션에 따라 다르다 (v1.4.5)
						if(_targetControlParam._isNonInterpolatedIndex)
						{
							//인덱스 타입이라면, 보간이 되지 않는다.
							//A와 B 중에서 계산된 값 중 Weight가 큰 값을 선택한다.
							if(_isIndexValueAssigned_A)
							{
								//Key A가 할당되었다면
								if(_isIndexValueAssigned_B)
								{
									//Key B도 할당되었다.
									//Weight가 더 큰 값을 선택한다.
									//둘다 같을 경우는 B를 선택한다.
									if(_weight_Index_A > _weight_Index_B)
									{
										//A가 더 큰 경우 > A
										_targetControlParam._int_Cur = _value_Index_A;
									}
									else
									{
										//B가 더 크거나 같을 경우 > B
										_targetControlParam._int_Cur = _value_Index_B;
									}
								}
								else
								{
									//Key B는 할당되지 않았다.
									_targetControlParam._int_Cur = _value_Index_A;//A를 할당
								}
							}
							else
							{
								if(_isIndexValueAssigned_B)
								{
									//Key B만 할당되었다면
									_targetControlParam._int_Cur = _value_Index_B;//B를 할당
								}
								else
								{
									//둘다 할당되지 않았다. (에러 상황)
									//임의로 보간 값을 할당한다.
									_targetControlParam._int_Cur = Mathf.RoundToInt(_value_Int);//버그 수정 20.4.15
								}
							}
						}
						else
						{
							//일반적인 보간식의 결과를 적용
							_targetControlParam._int_Cur = Mathf.RoundToInt(_value_Int);//버그 수정 20.4.15
						}
					}
					
					break;

				case apControlParam.TYPE.Float:
					_targetControlParam._float_Cur = _value_Float;
					break;

				case apControlParam.TYPE.Vector2:
					_targetControlParam._vec2_Cur = _value_Vec2;
					break;
			}

		}


		public void AdaptToControlParam_Opt(float weight, int layer, int playOrder, apAnimPlayUnit.BLEND_METHOD blendMethod)
		{
			if (!_isCalculated)
			{
				//Debug.LogError("계산되지 않았다. [" + _targetControlParam._keyName + "]");
				return;
			}


			//이전방식 (레이어가 없다.)

			//변경된 방식 (20.4.19 : 레이어 처리를 위해 아예 이 인스턴스를 넘겨주자)
			if(playOrder < 0 || weight <= 0.0f)
			{
				//Debug.LogError("Order/Weight 이상 [" + _targetControlParam._keyName + "] / Play Order : " + playOrder + " / Weight : " + weight);
				return;
			}
			_animPlayOrder = playOrder;
			_animWeight = weight;
			_animBlendMethod = blendMethod;
			_targetControlParam.AddCalculated_Request(this, layer);
		}


		/// <summary>
		/// 추가 v1.4.5 : Index 타입의 Int형 컨트롤 파라미터가 애니메이션 상에서 키프레임 사이에 위치했을 때의 결과값을 리턴한다.
		/// </summary>
		/// <returns></returns>
		public int GetIndexValueOfIntParam()
		{
			if(_isIndexValueAssigned_A)
			{
				//Key A가 할당되었다면
				if(_isIndexValueAssigned_B)
				{
					//Key B도 할당되었다.
					//Weight가 더 큰 값을 선택한다.
					//둘다 같을 경우는 B를 선택한다.
					if(_weight_Index_A > _weight_Index_B)
					{
						//A가 더 큰 경우 > A
						return _value_Index_A;
					}
					else
					{
						//B가 더 크거나 같을 경우 > B
						return _value_Index_B;
					}
				}
				else
				{
					//Key B는 할당되지 않았다.
					return _value_Index_A;//A를 할당
				}
			}

			//Key A가 할당되지 않았다.

			if(_isIndexValueAssigned_B)
			{
				//Key B만 할당되었다면
				return _value_Index_B;//B를 할당
			}

			//둘다 할당되지 않았다. (에러 상황)
			//임의로 보간 값을 할당한다.
			return Mathf.RoundToInt(_value_Int);//버그 수정 20.4.15
		}
	}

}
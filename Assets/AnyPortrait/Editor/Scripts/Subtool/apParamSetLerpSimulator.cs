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
	/// v1.5.0 : 컨트롤 파라미터 모디파이어에 키를 추가할 때, 보간된 Mod Mesh/Bone을 만들기 위해 사용되는 시뮬레이터.
	/// 인접한 다른 ParamSet으로의 가중치를 구해서 보간을 할 수 있도록 도와준다.
	/// 필요할 때 생성하여 데이터를 입력하면 결과를 도출해준다.
	/// apCalculatedResultParam의 Calculate 함수의 로직을 기반으로 작성되었다.
	/// </summary>
	public class apParamSetLerpSimulator
	{
		// 메인 함수.
		/// <summary>
		/// 새로운 ParamSet(컨트롤 파라미터 키)을 추가했을 경우, 해당 위치가 주변 ParamSet들로 부터 어떻게 보간되는지 참조하기 위한 리스트를 리턴한다.
		/// </summary>
		/// <param name="modifier">대상 모디파이어</param>
		/// <param name="paramSetGroup">컨트롤 파라미터 타입의 ParamSetGroup</param>
		/// <param name="newParamSet">새로 추가된 ParamSet. 해당 ParamSet의 컨트롤 파라미터 위치를 기반으로 Weight 리스트를 작성한다.</param>
		/// <param name="controlParam">대상 컨트롤 파라미터</param>
		/// <returns></returns>
		public static Dictionary<apModifierParamSet, float> GetSimulatedWeightParamSets(	apModifierBase modifier,
																							apModifierParamSetGroup paramSetGroup,
																							apModifierParamSet newParamSet,
																							apControlParam controlParam)
		{
			if(modifier == null
				|| paramSetGroup == null
				|| newParamSet == null
				|| controlParam == null)
			{
				return null;
			}

			int nParamSets = paramSetGroup._paramSetList != null ? paramSetGroup._paramSetList.Count : 0;
			if(nParamSets == 0)
			{
				return null;
			}

			apModifierParamSet curPS = null;
			Dictionary<apModifierParamSet, float> result = null;//유효한 경우에만 생성하여 리턴

			switch (controlParam._valueType)
			{
				case apControlParam.TYPE.Int:
					{
						// [ Int 타입 ]
						int curValue = newParamSet._conSyncValue_Int;

						apModifierParamSet adjPS_A = null;//대상보다 작은 값 중 가장 큰 값
						int adjValue_A = -1;

						apModifierParamSet adjPS_B = null;//대상보다 큰 값 중 가장 작은 값
						int adjValue_B = -1;

						for (int i = 0; i < nParamSets; i++)
						{
							curPS = paramSetGroup._paramSetList[i];

							if(curPS == newParamSet) { continue; }//대상 제외

							if(curPS._conSyncValue_Int < curValue)
							{
								//작은 PS 중에서 가장 큰 값을 찾자
								if(adjPS_A == null || adjValue_A < curPS._conSyncValue_Int)
								{
									adjPS_A = curPS;
									adjValue_A = curPS._conSyncValue_Int;
								}
							}
							else
							{
								//큰 PS 중에서 가장 작은 값을 찾자
								if(adjPS_B == null || curPS._conSyncValue_Int < adjValue_B)
								{
									adjPS_B = curPS;
									adjValue_B = curPS._conSyncValue_Int;
								}
							}
						}

						//찾은 A, B 포인트에 따라 결과를 리턴한다.
						if(adjPS_A == null && adjPS_B == null)
						{
							//둘다 없다면 패스
							break;
						}

						if(adjPS_A != null && adjPS_B == null)
						{
							//A만 존재하는 경우 (100%)
							result = new Dictionary<apModifierParamSet, float>();
							result.Add(adjPS_A, 1.0f);
						}
						else if(adjPS_A == null && adjPS_B != null)
						{
							//B만 존재하는 경우 (100%)
							result = new Dictionary<apModifierParamSet, float>();
							result.Add(adjPS_B, 1.0f);
						}
						else if(adjPS_A != null && adjPS_B != null)
						{
							//A, B가 있다면,
							//거리의 역수배 (거리가 멀다면 Weight가 작아진다. = A의 거리비는 B의 가중치다)
							float distFromA = Mathf.Abs((float)curValue - (float)adjPS_A._conSyncValue_Int);
							float distFromB = Mathf.Abs((float)curValue - (float)adjPS_B._conSyncValue_Int);
							float distTotal = distFromA + distFromB;

							float weightA = 0.0f;
							float weightB = 0.0f;
							if(distTotal < 0.001f)
							{
								//양쪽 거리가 0이라고?
								//반씩 넣자
								weightA = 0.5f;
								weightB = 0.5f;
							}
							else
							{
								//유효한 거리에 있다.
								weightA = Mathf.Clamp01(distFromB / distTotal);//B와의 거리 비 = A의 가중치 (B로부터 멀다면 A 가중치가 높아진다)
								weightB = Mathf.Clamp01(1.0f - weightA);
							}

							result = new Dictionary<apModifierParamSet, float>();
							result.Add(adjPS_A, weightA);
							result.Add(adjPS_B, weightB);
						}
					}
					break;

				case apControlParam.TYPE.Float:
					{
						// [ Float 타입 ]
						float curValue = newParamSet._conSyncValue_Float;

						apModifierParamSet adjPS_A = null;//대상보다 작은 값 중 가장 큰 값
						float adjValue_A = -1.0f;

						apModifierParamSet adjPS_B = null;//대상보다 큰 값 중 가장 작은 값
						float adjValue_B = -1.0f;

						for (int i = 0; i < nParamSets; i++)
						{
							curPS = paramSetGroup._paramSetList[i];

							if(curPS == newParamSet) { continue; }//대상 제외

							if(curPS._conSyncValue_Float < curValue)
							{
								//작은 PS 중에서 가장 큰 값을 찾자
								if(adjPS_A == null || adjValue_A < curPS._conSyncValue_Float)
								{
									adjPS_A = curPS;
									adjValue_A = curPS._conSyncValue_Float;
								}
							}
							else
							{
								//큰 PS 중에서 가장 작은 값을 찾자
								if(adjPS_B == null || curPS._conSyncValue_Float < adjValue_B)
								{
									adjPS_B = curPS;
									adjValue_B = curPS._conSyncValue_Float;
								}
							}
						}

						//찾은 A, B 포인트에 따라 결과를 리턴한다.
						if(adjPS_A == null && adjPS_B == null)
						{
							//둘다 없다면
							break;
						}

						if(adjPS_A != null && adjPS_B == null)
						{
							//A만 존재하는 경우 (100%)
							result = new Dictionary<apModifierParamSet, float>();
							result.Add(adjPS_A, 1.0f);
						}
						else if(adjPS_A == null && adjPS_B != null)
						{
							//B만 존재하는 경우 (100%)
							result = new Dictionary<apModifierParamSet, float>();
							result.Add(adjPS_B, 1.0f);
						}
						else if(adjPS_A != null && adjPS_B != null)
						{
							//A, B가 있다면,
							//거리의 역수배 (거리가 멀다면 Weight가 작아진다. = A의 거리비는 B의 가중치다)
							float distFromA = Mathf.Abs(curValue - adjPS_A._conSyncValue_Float);
							float distFromB = Mathf.Abs(curValue - adjPS_B._conSyncValue_Float);
							float distTotal = distFromA + distFromB;

							float weightA = 0.0f;
							float weightB = 0.0f;
							if(distTotal < 0.001f)
							{
								//양쪽 거리가 0이라고?
								//반씩 넣자
								weightA = 0.5f;
								weightB = 0.5f;
							}
							else
							{
								//유효한 거리에 있다.
								weightA = Mathf.Clamp01(distFromB / distTotal);//B와의 거리 비 = A의 가중치 (B로부터 멀다면 A 가중치가 높아진다)
								weightB = Mathf.Clamp01(1.0f - weightA);
							}

							result = new Dictionary<apModifierParamSet, float>();
							result.Add(adjPS_A, weightA);
							result.Add(adjPS_B, weightB);
						}
					}
					break;

				case apControlParam.TYPE.Vector2:
					{
						//[ Vector 2 타입 ]
						//사각+Virtual Point 방식
						//이건 연산 전에 Rect 타입으로 데이터를 정리해야한다. (newParamSet 제외)
						Vector2 curValue = newParamSet._conSyncValue_Vector2;

						//동일 여부 체크하는 Bias
						//Bias의 크기를 구하는 식은 apCalculatedResultParamSubList의 MakeControlParamLerAreas() 함수 참조
						float bias = 0.001f;
						bias = Mathf.Min(Mathf.Abs((controlParam._vec2_Max.x - controlParam._vec2_Min.x) * 0.05f),
									Mathf.Abs((controlParam._vec2_Max.y - controlParam._vec2_Min.y) * 0.05f));
						bias = Mathf.Clamp(bias, 0.0001f, 0.1f);
						

						//X, Y축에 대해서 각각 A, B 포인트의 위치를 구한다. (포인트가 아닌 위치만)
						//축 선상에 위치할 수 있으므로, 만약 bias 이내에 X, Y 포인트가 위치한다면 그 값을 이용한다.
						//만약 축상에서 "A, B"가 동일해진다면, 나중에 보간할때 한쪽에 100%를 줘야한다. (동일한 나머지 위치는 더미)
						bool isAdjPosX_A = false;
						float adjPosX_A = -1.0f;

						bool isAdjPosX_B = false;
						float adjPosX_B = -1.0f;

						bool isAdjPosY_A = false;
						float adjPosY_A = -1.0f;

						bool isAdjPosY_B = false;
						float adjPosY_B = -1.0f;

						for (int i = 0; i < nParamSets; i++)
						{
							curPS = paramSetGroup._paramSetList[i];

							if (curPS == newParamSet) { continue; }//대상 제외

							//X 비교
							float curX = curPS._conSyncValue_Vector2.x;

							if(curX < curValue.x - bias)
							{
								//작은 것 중에 가장 큰 값을 찾자
								if(!isAdjPosX_A || adjPosX_A < curX)
								{
									isAdjPosX_A = true;
									adjPosX_A = curX;
								}
							}
							else if(curX < curValue.x + bias)
							{
								//동일하다 이건 A, B를 한번에 할당한다.
								isAdjPosX_A = true;
								isAdjPosX_B = true;
								adjPosX_A = curX;								
								adjPosX_B = curX;
							}
							else
							{
								//큰 것 중에서 가장 작은 값을 찾자
								if(!isAdjPosX_B || curX < adjPosX_B)
								{
									isAdjPosX_B = true;
									adjPosX_B = curX;
								}
							}

							//Y 비교
							float curY = curPS._conSyncValue_Vector2.y;

							if(curY < curValue.y - bias)
							{
								//작은 것 중에 가장 큰 값을 찾자
								if(!isAdjPosY_A || adjPosY_A < curY)
								{
									isAdjPosY_A = true;
									adjPosY_A = curY;
								}
							}
							else if(curY < curValue.y + bias)
							{
								//동일하다 이건 A, B를 한번에 할당한다.
								isAdjPosY_A = true;
								isAdjPosY_B = true;
								adjPosY_A = curY;
								adjPosY_B = curY;
							}
							else
							{
								//큰 것 중에서 가장 작은 값을 찾자
								if(!isAdjPosY_B || curY < adjPosY_B)
								{
									isAdjPosY_B = true;
									adjPosY_B = curY;
								}
							}
						}

						//만약 값을 찾지 못했다면 가상의 포인트 생성을 위해 영역을 만들자
						if(!isAdjPosX_A)
						{
							isAdjPosX_A = true;
							adjPosX_A = curPS._conSyncValue_Vector2.x;
						}

						if(!isAdjPosX_B)
						{
							isAdjPosX_B = true;
							adjPosX_B = curPS._conSyncValue_Vector2.x;
						}

						if(!isAdjPosY_A)
						{
							isAdjPosY_A = true;
							adjPosY_A = curPS._conSyncValue_Vector2.y;
						}

						if(!isAdjPosY_B)
						{
							isAdjPosY_B = true;
							adjPosY_B = curPS._conSyncValue_Vector2.y;
						}

						//이제 4개의 포인트를 만들자
						LerpPoint point_LB = new LerpPoint(adjPosX_A, adjPosY_A);
						LerpPoint point_LT = new LerpPoint(adjPosX_A, adjPosY_B);
						LerpPoint point_RT = new LerpPoint(adjPosX_B, adjPosY_B);
						LerpPoint point_RB = new LerpPoint(adjPosX_B, adjPosY_A);
						List<LerpPoint> points = new List<LerpPoint>();//가상 포인트 생성을 편하게 하기 위해 리스트 생성
						points.Add(point_LB);
						points.Add(point_LT);
						points.Add(point_RB);
						points.Add(point_RT);

						//각 포인트에 해당하는 ParamSet을 찾아서 입력한다.
						for (int i = 0; i < nParamSets; i++)
						{
							curPS = paramSetGroup._paramSetList[i];

							if (curPS == newParamSet) { continue; }//대상 제외

							Vector2 curPos = curPS._conSyncValue_Vector2;

							bool isX_A = false;
							bool isX_B = false;
							bool isY_A = false;
							bool isY_B = false;
							isX_A = (Mathf.Abs(curPos.x - adjPosX_A) < bias);
							isX_B = (Mathf.Abs(curPos.x - adjPosX_B) < bias);
							isY_A = (Mathf.Abs(curPos.y - adjPosY_A) < bias);
							isY_B = (Mathf.Abs(curPos.y - adjPosY_B) < bias);

							//위치가 겹칠 수 있기 때문에 else if를 하지 않는다.
							if(isX_A && isY_A)//AA = LB
							{	
								point_LB.AddParamSet(curPS, 1.0f);
							}
							if(isX_A && isY_B)//AB = LT
							{	
								point_LT.AddParamSet(curPS, 1.0f);
							}
							if(isX_B && isY_A)//BA = RB
							{	
								point_RB.AddParamSet(curPS, 1.0f);
							}
							if(isX_B && isY_B)//BB = RT
							{	
								point_RT.AddParamSet(curPS, 1.0f);
							}
						}

						//비어있는 포인트를 찾아서 가상 포인트로 만든다.
						//해당 방식은 apCalculatedResultParamSubList의 MakeVirtuallerpPoint 함수를 참조한다.
						for (int iPoint = 0; iPoint < points.Count; iPoint++)
						{
							LerpPoint curLerpPoint = points[iPoint];
							if(curLerpPoint.NumParamSets > 0)
							{
								//이 포인트는 유효한 ParamSet이 입력되었다. > 가상 포인트로 만들 필요가 없다.
								continue;
							}

							//가상 포인트로 만들자
							Vector2 pos = curLerpPoint._pos;

							//모든 ParamSet들에 대해서 두개씩 짝지어서 거리비 역수로 Weight 곱을 구하자
							List<apModifierParamSet> lerpParamSets = new List<apModifierParamSet>();//여기에 해당하는 ParamSet은 별도로 구분
							List<float> dists = new List<float>();
							List<float> weights = new List<float>();

							for (int iPS = 0; iPS < nParamSets; iPS++)
							{
								curPS = paramSetGroup._paramSetList[iPS];

								if (curPS == newParamSet) { continue; }//대상 제외

								float dist = Vector2.Distance(pos, curPS._conSyncValue_Vector2);
								lerpParamSets.Add(curPS);
								dists.Add(dist);
								weights.Add(1.0f);//시작 가중치는 1
							}

							apModifierParamSet psA = null;
							apModifierParamSet psB = null;

							if(lerpParamSets.Count == 0)
							{
								//이 LerpPoint는 더 참조할 ParamSet이 없다.
								continue;
							}

							if(lerpParamSets.Count == 1)
							{
								//1개라면 그냥 그걸 넣으면 된다.
								curLerpPoint.AddParamSet(lerpParamSets[0], 1.0f);
								continue;
							}

							//이제 모든 경우의 수에 따라 거리비 역수를 가중치에 가한다.
							for (int iA = 0; iA < lerpParamSets.Count - 1; iA++)
							{
								psA = lerpParamSets[iA];
								
								float weightA = weights[iA];
								if(weightA <= 0.0f)
								{
									//더 비교할 필요가 없다.
									continue;
								}

								float distA = dists[iA];

								for (int iB = iA + 1; iB < lerpParamSets.Count; iB++)
								{
									psB = lerpParamSets[iB];
									
									float weightB = weights[iB];
									if(weightB <= 0.0f)
									{
										continue;
									}

									float distB = dists[iB];

									float distSum = distA + distB;
									if(distSum <= 0.0f)
									{
										continue;
									}

									float itpA = Mathf.Clamp01(1.0f - (distA / distSum));
									weights[iA] *= itpA;
									weights[iB] *= 1.0f - itpA;
								}
							}

							//이제 전체 가중치를 계산하자
							float totalWeight = 0.0f;

							for (int iLPS = 0; iLPS < lerpParamSets.Count; iLPS++)
							{
								curPS = lerpParamSets[iLPS];
								float curWeight = weights[iLPS];
								if(curWeight > 0.0f)
								{
									totalWeight += curWeight;
								}
							}

							if(totalWeight > 0.0f)
							{
								for (int iLPS = 0; iLPS < lerpParamSets.Count; iLPS++)
								{
									curPS = lerpParamSets[iLPS];
									float curWeight = weights[iLPS];
									if (curWeight > 0.0f)
									{
										curLerpPoint.AddParamSet(curPS, curWeight / totalWeight);//Normalized Weight 계산
									}
								}
							}
						}

						float weightX_A = 0.0f;
						if(Mathf.Abs(adjPosX_B - adjPosX_A) < bias)
						{
							//X축이 겹쳐있다면
							weightX_A = 1.0f;
						}
						else
						{
							weightX_A = 1.0f - Mathf.Clamp01((curValue.x - adjPosX_A) / Mathf.Abs(adjPosX_B - adjPosX_A));
						}

						float weightY_A = 0.0f;
						if(Mathf.Abs(adjPosY_B - adjPosY_A) < bias)
						{
							//Y축이 겹쳐있다면
							weightY_A = 1.0f;
						}
						else
						{
							weightY_A = 1.0f - Mathf.Clamp01((curValue.y - adjPosY_A) / Mathf.Abs(adjPosY_B - adjPosY_A));
						}

						point_LB._weight = weightX_A * weightY_A;
						point_LT._weight = weightX_A * (1.0f - weightY_A);
						point_RB._weight = (1.0f - weightX_A) * weightY_A;
						point_RT._weight = (1.0f - weightX_A) * (1.0f - weightY_A);

						//이제 4개의 포인트에 포함된 ParamSet들의 Weight를 하나씩 넣고, 가중치의 합을 구해서 Normalize하자
						
						List<apModifierParamSet> totalRefPSs = new List<apModifierParamSet>();
						List<float> totalRefWeights = new List<float>();
						float totalRefWeight = 0.0f;

						for (int iPoint = 0; iPoint < points.Count; iPoint++)
						{
							LerpPoint curLerpPoint = points[iPoint];

							int nRefs = curLerpPoint.NumParamSets;
							if (nRefs == 0 || curLerpPoint._weight <= 0.0f)
							{
								continue;
							}

							for (int iRef = 0; iRef < nRefs; iRef++)
							{
								apModifierParamSet refPS = curLerpPoint._refParamSets[iRef];
								float refWeight = curLerpPoint._refWeights[iRef] * curLerpPoint._weight;

								if(refWeight <= 0.0f)
								{
									continue;
								}
								int iResult = totalRefPSs.IndexOf(refPS);
								
								if(iResult < 0)
								{
									//아직 등록되지 않았다. 새로 추가
									totalRefPSs.Add(refPS);
									totalRefWeights.Add(refWeight);
								}
								else
								{
									//이미 등록되었다. 가중치 추가
									totalRefWeights[iResult] = totalRefWeights[iResult] + refWeight;
								}

								totalRefWeight += refWeight;
							}
						}

						//전체 가중치를 다시 나누자 (Normalize)
						if(totalRefWeight > 0.0f)
						{
							for (int iRef = 0; iRef < totalRefWeights.Count; iRef++)
							{
								totalRefWeights[iRef] = totalRefWeights[iRef] / totalRefWeight;
							}
						}

						result = new Dictionary<apModifierParamSet, float>();
						for (int iResultRef = 0; iResultRef < totalRefPSs.Count; iResultRef++)
						{
							result.Add(totalRefPSs[iResultRef], totalRefWeights[iResultRef]);
						}
					}
					break;
			}

			return result;
		}

		/// <summary>
		/// apCalculatedLerpPoint를 모방한 시뮬레이션 데이터
		/// </summary>
		public class LerpPoint
		{
			public Vector2 _pos = Vector2.zero;
			public List<apModifierParamSet> _refParamSets = null;
			public List<float> _refWeights = null;

			public float _weight = 0.0f;

			public LerpPoint(float posX, float posY)
			{
				_pos = new Vector2(posX, posY);
				_refParamSets = new List<apModifierParamSet>();
				_refWeights = new List<float>();

				_weight = 0.0f;
			}

			public int NumParamSets
			{
				get { return _refParamSets.Count; }
			}

			public void AddParamSet(apModifierParamSet paramSet, float weight)
			{
				_refParamSets.Add(paramSet);
				_refWeights.Add(weight);
			}

			
		}
	}
}
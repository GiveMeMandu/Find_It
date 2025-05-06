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
	/// 추가 v1.4.6 : 애니메이션 타임라인에 의해서 모디파이어 파라미터 (키프레임)을 보간할 때 빠른 탐색을 할 수 있게 만드는 LUT.
	/// 먼저 만들어졌던 apAnimControlKeyLUT를 참고했다.
	/// apAnimControlKeyLUT가 모디파이어와 별개로 동작하여 AnimClip에 속했던 것과 달리,
	/// 이건 모디파이어 전용이어서 apCalculatedResultParamSubList 또는 apOptCalculatedResultParamSubList에 속한다.
	/// </summary>
	public class apCalculatedAnimKeyLUT
	{
		// Members
		//------------------------------------------------------
		public apAnimClip _linkedAnimClip = null;
		public apAnimTimelineLayer _linkedTimelineLayer = null;

		public apCalculatedResultParamSubList _linked_CalResultParamSubList = null;
		
		public class LUTUnit
		{
			public apAnimKeyframe _keyframe_Cur = null;
			public apAnimKeyframe _keyframe_Next = null;
			
			public apCalculatedResultParam.ParamKeyValueSet _linked_PKV_Editor = null;
			
			//먼저 Prev 위주로 생성
			public LUTUnit(apAnimKeyframe keyframe, apCalculatedResultParam.ParamKeyValueSet linkedPKV)
			{
				_keyframe_Cur = keyframe;
				_keyframe_Next = null;
				_linked_PKV_Editor = linkedPKV;
			}


			public void SetNextKeyframe(apAnimKeyframe keyframe_Next)
			{	
				_keyframe_Next = keyframe_Next;
			}
		}

		private LUTUnit[] _LUT = null;//배열로 만들어야 빠르다.
		private int _startFrame = 0;//애니메이션의 시작 프레임을 알아야 Offset을 구한다.
		private int _endFrame = 0;//애니메이션의 시작 프레임을 알아야 Offset을 구한다.
		private int _lutLength = 0;
		private bool _isLoop = false;
		
		private bool _isLUTAvailable = false;//데이터가 없을 수 있다.

		// Init
		//---------------------------------------------------
		//에디터에서의 초기화
		public apCalculatedAnimKeyLUT(apAnimClip animClip,
										apAnimTimelineLayer timelineLayer,
										apCalculatedResultParamSubList calResultParamSubList)
		{
			_linkedAnimClip = animClip;
			_linkedTimelineLayer = timelineLayer;

			_linked_CalResultParamSubList = calResultParamSubList;
		}


		// Functions
		//---------------------------------------------------
		// Make LUT
		public void MakeLUT()
		{
			//LUT 배열을 만들자 (일단 비어있을 것)
			_startFrame = _linkedAnimClip.StartFrame;
			_endFrame = _linkedAnimClip.EndFrame;
			_isLoop = _linkedAnimClip.IsLoop;

			_lutLength = (_endFrame - _startFrame) + 1;

			_LUT = new LUTUnit[_lutLength];


			int nKeyframes = _linkedTimelineLayer._keyframes != null ? _linkedTimelineLayer._keyframes.Count : 0;
			if(nKeyframes == 0)
			{
				_isLUTAvailable = false;
				return;
			}

			_isLUTAvailable = true;

			//Keyframe > SubParamKeyValue를 연결해야한다.
			Dictionary<apAnimKeyframe, apCalculatedResultParam.ParamKeyValueSet> animKeyframe2PKV = new Dictionary<apAnimKeyframe, apCalculatedResultParam.ParamKeyValueSet>();
			int nPKVs = _linked_CalResultParamSubList._subParamKeyValues != null ? _linked_CalResultParamSubList._subParamKeyValues.Count : 0;

			if(nPKVs > 0)
			{
				apCalculatedResultParam.ParamKeyValueSet curPKV = null;
				apAnimKeyframe syncAnimKey = null;
				for (int i = 0; i < nPKVs; i++)
				{
					curPKV = _linked_CalResultParamSubList._subParamKeyValues[i];

					syncAnimKey = curPKV._paramSet.SyncKeyframe;

					if(syncAnimKey == null)
					{
						continue;
					}

					if(animKeyframe2PKV.ContainsKey(syncAnimKey))
					{
						continue;
					}

					animKeyframe2PKV.Add(syncAnimKey, curPKV);//Anim Keyframe > ParamKeyValue를 연결하자
				}
			}



			//일단 subList의 ParamSetValue를 모두 넣자
			List<LUTUnit> unitList = new List<LUTUnit>();

			apAnimKeyframe curKeyframe = null;
			apCalculatedResultParam.ParamKeyValueSet targetPKV = null;
			for (int iKeyframe = 0; iKeyframe < nKeyframes; iKeyframe++)
			{
				curKeyframe = _linkedTimelineLayer._keyframes[iKeyframe];
				if(!curKeyframe._isActive)
				{
					continue;
				}

				//이 Keyframe에 맞는 PKV가 있어야 한다.
				targetPKV = null;
				animKeyframe2PKV.TryGetValue(curKeyframe, out targetPKV);

				LUTUnit newUnit = new LUTUnit(curKeyframe, targetPKV);

				unitList.Add(newUnit);
			}

			int nUnits = unitList.Count;

			if(nUnits == 0)
			{
				//Debug.Log("LUT가 없다.");
				return;
			}

			//정렬을 한다. (키프레임 위치에 따라 오름차순)
			unitList.Sort(delegate (LUTUnit a, LUTUnit b)
			{
				return a._keyframe_Cur._frameIndex - b._keyframe_Cur._frameIndex;
			});


			//일단 앞뒤로 연결을 하자.
			LUTUnit curUnit = null;
			LUTUnit nextUnit = null;
			for (int i = 0; i < nUnits - 1; i++)
			{
				curUnit = unitList[i];
				nextUnit = unitList[i + 1];

				curUnit.SetNextKeyframe(nextUnit._keyframe_Cur);
			}

			
			if (_isLoop)
			{
				//루프라면 Last > First로 묶기
				curUnit = unitList[unitList.Count - 1];
				nextUnit = unitList[0];

				curUnit.SetNextKeyframe(nextUnit._keyframe_Cur);
			}
			else
			{
				//루프가 아니면 Last 혼자서 Prev~Next 처리
				curUnit = unitList[unitList.Count - 1];
				curUnit.SetNextKeyframe(curUnit._keyframe_Cur);
			}

			//이제 배열에 LUT를 넣자
			
			int iLUT_Start = 0;
			int iLUT_End = 0;

			//일단 마지막 전 LUT Unit까지 체크
			for (int i = 0; i < nUnits - 1; i++)
			{
				curUnit = unitList[i];
				iLUT_Start = curUnit._keyframe_Cur._frameIndex - _startFrame;
				iLUT_End = curUnit._keyframe_Next._frameIndex - _startFrame;

				for (int iLUT = iLUT_Start; iLUT < iLUT_End; iLUT++)
				{
					if (iLUT < 0 || iLUT >= _lutLength)
					{
						continue;
					}

					_LUT[iLUT] = curUnit;
				}
			}

			//이제 "<~첫 키프레임" / "마지막 키프레임~>"을 계산하자
			LUTUnit lastUnit = unitList[nUnits - 1];
			LUTUnit firstUnit = unitList[0];

			int iListFirstLUT = firstUnit._keyframe_Cur._frameIndex - _startFrame;
			int iListLastLUT = lastUnit._keyframe_Cur._frameIndex - _startFrame;
			
			if (_isLoop)
			{
				//루프라면, "첫 키프레임 이전"과 "마지막 키프레임 이후"에 마지막 LUT Unit을 넣는다.
				for (int iLUT = 0; iLUT < iListFirstLUT; iLUT++)
				{
					if (iLUT >= _lutLength) { continue; }

					_LUT[iLUT] = lastUnit;
				}

				for (int iLUT = iListLastLUT; iLUT < _lutLength; iLUT++)
				{
					if (iLUT < 0 || iLUT >= _lutLength) { continue; }

					_LUT[iLUT] = lastUnit;
				}
			}
			else
			{
				//루프가 아니라면
				//"첫 키프레임 이전"에는 "첫 키프레임"으로만 구성된 LUT Unit 생성 후 대입
				//"마지막 키프레임 이후"에는 마지막 LUT Unit을 넣는다.

				apCalculatedResultParam.ParamKeyValueSet firstPKV = null;
				animKeyframe2PKV.TryGetValue(firstUnit._keyframe_Cur, out firstPKV);//PKV도 가져온다.

				LUTUnit preUnit = new LUTUnit(firstUnit._keyframe_Cur, firstPKV);

				preUnit.SetNextKeyframe(firstUnit._keyframe_Cur);//자기 자신

				for (int iLUT = 0; iLUT < iListFirstLUT; iLUT++)
				{
					if (iLUT >= _lutLength) { continue; }

					_LUT[iLUT] = preUnit;
				}

				for (int iLUT = iListLastLUT; iLUT < _lutLength; iLUT++)
				{
					if (iLUT < 0 || iLUT >= _lutLength) { continue; }

					_LUT[iLUT] = lastUnit;
				}
			}
		}


		// Get
		//---------------------------------------------------
		/// <summary>
		/// LUT를 리턴한다. 매우! 빠름! (순회따위.. ㅂㄷㅂㄷ)
		/// </summary>
		/// <param name="curAnimFrame"></param>
		/// <returns></returns>
		public LUTUnit GetLUT(int curAnimFrame)
		{
			if(!_isLUTAvailable)
			{
				return null;
			}

			return _LUT[(curAnimFrame - _startFrame) % _lutLength];
		}
	}
}
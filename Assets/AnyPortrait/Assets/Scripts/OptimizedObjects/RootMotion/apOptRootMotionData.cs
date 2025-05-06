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
	/// OptRootUnit에 속한 객체로서, 루트 모션을 위한 모디파이어 데이터를 받는 역할을 한다.
	/// 기록된 데이터의 변화량을 체크해서 ParentTransform의 이동을 계산한다.
	/// </summary>
	public class apOptRootMotionData
	{
		// Members
		//----------------------------------------------------

		//Key : AnimPlayUnit (활성 여부)
		//Data : Pos (Prev > Cur) >> Delta
		//Weight

		public enum DATA_STORED
		{
			/// <summary>저장된 데이터가 없다.</summary>
			None,
			/// <summary>이전 데이터가 없이 오직 "현재 데이터"만 있다.</summary>
			OnlyCurrent,
			/// <summary>이전 프레임의 데이터와 현재 프레임의 데이터가 모두 있다. 이제 적용 가능</summary>
			PrevAndCur,
		}

		/// <summary>
		/// 애니메이션에 의해서 루트 본의 이동 정도를 저장한다.
		/// 애니메이션별로 키값을 두고, Order를 지정한다.
		/// </summary>
		public class AnimMoveInfo
		{
			public apAnimClip _animClip = null;
			
			public DATA_STORED _storedType = DATA_STORED.None;
			public Vector2 _deltaPos_Prev = Vector2.zero;
			public Vector2 _deltaPos_Cur = Vector2.zero;

			public float _weight = 0.0f;
			public bool _isUpdated = false;

			public AnimMoveInfo()
			{
				Clear();
			}

			private void Clear()
			{
				_animClip = null;
				_storedType = DATA_STORED.None;
				_deltaPos_Prev = Vector2.zero;
				_deltaPos_Cur = Vector2.zero;
				_weight = 0.0f;
				_isUpdated = false;
			}

			public void Reset()
			{
				_storedType = DATA_STORED.None;
				_deltaPos_Prev = Vector2.zero;
				_deltaPos_Cur = Vector2.zero;
				_weight = 0.0f;
				_isUpdated = false;
			}

			public void SetAnimClip(apAnimClip animClip)
			{
				_animClip = animClip;
			}

			public void OnUpdated(Vector2 deltaPos, float weight)
			{
				if (_storedType == DATA_STORED.None)
				{
					//이전 프레임은 없으며 현재 프레임에 정보만 가짐
					_storedType = DATA_STORED.OnlyCurrent;
				}
				else if(_storedType == DATA_STORED.OnlyCurrent)
				{
					//이전 프레임 + 현재 프레임 정보를 모두 가짐
					_storedType = DATA_STORED.PrevAndCur;
				}

				_deltaPos_Prev = _deltaPos_Cur;
				_deltaPos_Cur = deltaPos;

				_weight = weight;
				_isUpdated = true;
			}

			public void ReadyToUpdate()
			{
				_isUpdated = false;
			}
		}


		//풀 데이터를 이용한다.
		

		//현재 저장중인 애니메이션 클립 정보
		private List<AnimMoveInfo> _infos_All = null;//한번이라도 등록된 모든 정보(AnimClip 기준)는 여기에 속한다.
		private Dictionary<apAnimClip, AnimMoveInfo> _animClip2Info = null;
		private List<AnimMoveInfo> _infos_Live = null;//이번 프레임에서 사용된 정보는 여기에 저장된다.
		private List<AnimMoveInfo> _infos_Cur = null;
		
		private Vector2 _deltaPosResult = Vector2.zero;
		public Vector2 ResultDeltaPos { get { return _deltaPosResult; } }

		

		// Init
		//----------------------------------------------------
		public apOptRootMotionData()
		{	
			Clear();
		}



		private void Clear()
		{
			if (_infos_All == null)			{ _infos_All = new List<AnimMoveInfo>(); }
			if (_animClip2Info == null)		{ _animClip2Info = new Dictionary<apAnimClip, AnimMoveInfo>(); }
			if (_infos_Live == null)		{ _infos_Live = new List<AnimMoveInfo>(); }
			_infos_All.Clear();
			_animClip2Info.Clear();
			_infos_Live.Clear();

			if(_infos_Cur == null) { _infos_Cur = new List<AnimMoveInfo>(); }
			_infos_Cur.Clear();
		}


		public void ResetData()
		{
			//모든 Live 데이터를 초기화한다.
			int nInfos = _infos_All.Count;
			for (int i = 0; i < nInfos; i++)
			{
				//이전까지의 정보를 모두 리셋한다.
				_infos_All[i].Reset();
			}
			_infos_Live.Clear();
			_infos_Cur.Clear();
		}


		public void ReadyToUpdate()
		{
			int nLive = _infos_Live.Count;
			if(nLive > 0)
			{
				for (int i = 0; i < nLive; i++)
				{
					_infos_Live[i].ReadyToUpdate();
				}
			}

			_infos_Cur.Clear();
		}

		public void AddAnimModInfo(apAnimClip animClip, ref Vector2 modDeltaPos, float weight)
		{
			AnimMoveInfo targetInfo = null;

			//일단 Info를 찾자
			_animClip2Info.TryGetValue(animClip, out targetInfo);
			if(targetInfo == null)
			{
				//이 AnimClip에 대한 정보가 없었다면
				targetInfo = new AnimMoveInfo();
				targetInfo.SetAnimClip(animClip);
				targetInfo.OnUpdated(modDeltaPos, weight);//현재 정보 업데이트

				//현재 프레임에 저장
				_infos_Cur.Add(targetInfo);
				_infos_Live.Add(targetInfo);

				//전체 리스트에도 저장
				_infos_All.Add(targetInfo);
				_animClip2Info.Add(animClip, targetInfo);
			}
			else
			{
				//이 AnimClip에 대한 정보가 이미 있다면
				targetInfo.OnUpdated(modDeltaPos, weight);

				//현재 프레임에 저장
				_infos_Cur.Add(targetInfo);
				if(!_infos_Live.Contains(targetInfo))
				{
					//Live가 아니었다면
					targetInfo._storedType = DATA_STORED.OnlyCurrent;//이전 정보는 없는 것으로 강제로 다운한다.
					_infos_Live.Add(targetInfo);
				}
			}
		}

		/// <summary>
		/// 새로 재생되는 애니메이션의 경우는 기존의 데이터가 있다 하더라도 리셋을 해야한다.
		/// "재생 중인 애니메이션을 다시 재생"하는 경우에 "프레임간 Delta Pos"를 무효로 만들어야하기 때문
		/// 안그러면 재시작한 애니메이션을 "연속 재생"으로 알고 "위치 변화"를 계산해버리고 만다.
		/// </summary>
		/// <param name="animClip">새로 재생을 하고자 하는 애니메이션 클릭</param>
		public void ResetAnimModInfo(apAnimClip animClip)
		{
			AnimMoveInfo targetInfo = null;

			//기존에 등록되었던 애니메이션이 있는지 확인한다.
			_animClip2Info.TryGetValue(animClip, out targetInfo);

			if(targetInfo != null)
			{
				//기존의 애니메이션 정보가 있다면 저장된 데이터를 일단 리셋한다.
				targetInfo.Reset();
			}
		}

		/// <summary>
		/// 모디파이어 업데이트 이후에 호출한다.
		/// 불필요한 업데이트 정보는 삭제하고, 이동 거리를 계산한다.
		/// </summary>
		public void CalculateRootMotion()
		{
			int nCur = _infos_Cur.Count;
			int nLive = _infos_Live.Count;
			//Debug.Log("루트 모션 데이터 수집 완료 : " + nCur + " (이전의 Live 상태 : " + nLive + ")");
			
			_deltaPosResult = Vector2.zero;
			AnimMoveInfo curInfo = null;

			if(nCur > 0)
			{
				for (int i = 0; i < nCur; i++)
				{
					curInfo = _infos_Cur[i];
					if(curInfo._storedType == DATA_STORED.PrevAndCur)
					{
						//Prev > Cur이 모두 있는 경우에만 유효
						Vector2 curDeltaData = curInfo._deltaPos_Cur - curInfo._deltaPos_Prev;
						if (i == 0)
						{
							_deltaPosResult = curDeltaData;
						}
						else
						{
							float weight = Mathf.Clamp01(curInfo._weight);

							//선형 보간을 한다.
							_deltaPosResult = (_deltaPosResult * (1.0f - weight)) + (curDeltaData * weight);
						}
						
					}
				}
			}

			//Debug.Log("결과 : Delta의 합 : " + _deltaPosResult);


			//Live 중에 Update 안된것은 타입 변경하고 삭제한다.
			if(nLive > 0)
			{
				List<AnimMoveInfo> removedList = null;
				bool isAnyRemovable = false;

				for (int i = 0; i < nLive; i++)
				{
					curInfo = _infos_Live[i];
					if(!curInfo._isUpdated)
					{
						//업데이트 되지 않은게 발견되었다면
						curInfo.Reset();//해당 데이터는 리셋

						isAnyRemovable = true;
						if(removedList == null)
						{
							removedList = new List<AnimMoveInfo>();
						}
						//이건 Live에서 제외하자
						removedList.Add(curInfo);
					}
				}

				if (isAnyRemovable)
				{
					int nRemoved = removedList.Count;
					//Debug.Log("[" + nRemoved + "]개의 기존 Live 데이터는 리셋 및 Live에서 삭제");
					if(nRemoved > 0)
					{
						for (int i = 0; i < nRemoved; i++)
						{
							_infos_Live.Remove(removedList[i]);
						}
					}
				}
			}
		}

		
	}
}
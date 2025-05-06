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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//#if UNITY_EDITOR
//using UnityEngine.Profiling;
//#endif
using System;

using AnyPortrait;


namespace AnyPortrait
{
	/// <summary>
	/// Fixed Frame에서 업데이트하기 위한 카운터. (동기화용)
	/// Update/LateUpdate에서 호출이 되면서 마치 GameObject가 실존하는 것처럼 업데이트된다.
	/// </summary>
	public class apOptFixedFrameChecker
	{
		// 싱글톤
		//----------------------------------------------------------------------------------
		private static apOptFixedFrameChecker s_instance = new apOptFixedFrameChecker();
		public static apOptFixedFrameChecker I { get {  return s_instance; } }

		// Members
		//----------------------------------------------------------------------------------
		private enum STATE
		{
			Ready, Update, LateUpdate,
		}		

		//요청된 FPS별로 업데이트를 해서 현재 프레임에서 업데이트할지 결정한다.
		public class FPSCounter
		{
			private STATE _state = STATE.Ready;

			public int _FPS = 0;
			public float _secPerFrame = 0.0f;
			public float _tTimer = 0.0f;
			public bool _isCurUpdateFrame = false;//현재 업데이트할 프레임인가
			
			public FPSCounter(int FPS)
			{
				_FPS = FPS;
				_secPerFrame = 1.0f / (float)_FPS;
				_tTimer = 0.0f;
				_isCurUpdateFrame = true;
				_state = STATE.Ready;
			}

			public void OnUpdate(float tDelta)
			{
				if(_state == STATE.Update)
				{
					return;
				}
				_state = STATE.Update;

				_tTimer += tDelta;
				_isCurUpdateFrame = false;

				//특정 프레임마다 메시를 갱신할 수 있다.
				if (_tTimer > _secPerFrame)
				{
					_isCurUpdateFrame = true;

					_tTimer -= _secPerFrame;

					//만약 계속 더 크다면 줄이자
					while (_tTimer > _secPerFrame)
					{
						_tTimer -= _secPerFrame;
					}
				}
			}

			public void OnLateUpdate()
			{
				if (_state != STATE.LateUpdate)
				{
					_state = STATE.LateUpdate;
				}
			}

			public bool IsUpdatable { get { return _isCurUpdateFrame; } }
		}

		private FPSCounter[] _fpsCounters = null;
		private const int MIN_FPS = 1;
		private const int MAX_FPS = 30;//<<인덱싱은 요청 (FPS - 1)이다.



		// Init
		//-------------------------------------------------------------------------
		private apOptFixedFrameChecker()
		{
			Init();
		}

		public void Init()
		{
			//카운터 배열을 FPS만큼 만들자
			_fpsCounters = new FPSCounter[MAX_FPS];
			for (int i = 0; i < MAX_FPS; i++)
			{
				_fpsCounters[i] = new FPSCounter(i + 1);//FPS는 (인덱스 + 1)이다. (0~29 > 1 ~ 30)
			}
		}

#if UNITY_2020_1_OR_NEWER
		//[v1.5.0] Reload Domain 옵션 비활성화시 static 필드 초기화가 안되는데, 이 기능에 영향을 줘버린다.
		//Reload Domain 비활성화 + 게임 시작시 강제로 초기화를 호출해야한다.
		// 참조 : https://docs.unity3d.com/kr/2022.3/Manual/DomainReloading.html
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void InitDomain()
		{	
			if(s_instance != null)
			{
				s_instance.Init();//초기화 다시
			}
		}
#endif

		/// <summary>
		/// FPS를 계산하도록 요청하자. 이 함수는 Update에서 호출되며, LateUpdate에서 업데이트 가능 여부를 받자
		/// </summary>
		/// <param name="FPS"></param>
		public void OnUpdate(int FPS)
		{
			if(FPS < MIN_FPS) { FPS = MIN_FPS; }
			else if(FPS > MAX_FPS) { FPS = MAX_FPS; }

			_fpsCounters[FPS - 1].OnUpdate(Time.unscaledDeltaTime);
		}

		/// <summary>
		/// 현재 프레임에서 업데이트가 가능한지 받자
		/// Update에서 "UpdateFPS" 함수를 호출했어야 한다.
		/// </summary>
		/// <param name="FPS"></param>
		/// <returns></returns>
		public void OnLateUpdate(int FPS)
		{
			if(FPS < MIN_FPS) { FPS = MIN_FPS; }
			else if(FPS > MAX_FPS) { FPS = MAX_FPS; }

			_fpsCounters[FPS - 1].OnLateUpdate();
		}

		public bool IsUpdatable(int FPS)
		{
			if(FPS < MIN_FPS) { FPS = MIN_FPS; }
			else if(FPS > MAX_FPS) { FPS = MAX_FPS; }

			return _fpsCounters[FPS - 1].IsUpdatable;
		}

	}
}
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
using UnityEditor;

namespace AnyPortrait
{
	/// <summary>
	/// apEditor에 속하는 객체로서, 특정 시점을 기준으로 GC 메모리가 증가했다면 Collect를 실행한다.
	/// </summary>
	public class apEditorMemUtil
	{
		// Members
		//---------------------------------------------------------
		//메모리가 체크 되었는가
		private bool _isMemChecked = false;
		private long _prevMemSizeMB = 0;


		//최대 GC 메모리 (MB 단위) (사실 여기에 도달하면 에러에 가까움)
		private const int MAX_GC_MEMSIZE_MB = 768;//768MB = 512 * 1.5

		//이전 기록으로 부터 메모리 차이
		private const int DELTA_GC_MEMSIZE_MB = 256;



		// Init
		//---------------------------------------------------------
		public apEditorMemUtil()
		{
			Reset();
		}

		public void Reset()
		{
			_isMemChecked = false;
			_prevMemSizeMB = 0;
		}

		/// <summary>
		/// 메모리 상태를 체크하여 이전과 비교하여 큰 차이가 있다면 Collect를 시도한다.
		/// </summary>
		public void CheckAndCollect()
		{
			//현재 메모리 상태
			long curMemSize_Byte = GC.GetTotalMemory(false);
			long curMemSize_KB = curMemSize_Byte / 1024;
			int curMemSize_MB = (int)(curMemSize_KB / 1024);

			bool isCollect = false;
			if (curMemSize_MB > MAX_GC_MEMSIZE_MB)
			{
				//1. 최대치를 넘어가면 Collect
				isCollect = true;
			}
			else if (_isMemChecked && (curMemSize_MB - _prevMemSizeMB) > DELTA_GC_MEMSIZE_MB)
			{
				//2. 이전 기록과 비교하여 차이가 크면 Collect
				isCollect = true;
			}

			if(isCollect)
			{
				//GC.Collect를 수행한다.
				//이건 전체 메모리 참조 그래프를 순회하므로 Unity 내장 GC보다 느리다.
				GC.Collect();

				//Collect 수행시 메모리를 다시 체크한다.
				curMemSize_Byte = GC.GetTotalMemory(false);
				curMemSize_KB = curMemSize_Byte / 1024;
				curMemSize_MB = (int)(curMemSize_KB / 1024);
			}

			//현재 정보 갱신
			_isMemChecked = true;
			_prevMemSizeMB = curMemSize_MB;
		}
	}
}
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
	/// 보간을 위한 영역 값
	/// 2차원의 값을 가진다. (1차원은 굳이 영역 없이도 처리 가능하다)
	/// LT : XY의 Min 값, RT : XY의 Max값
	/// </summary>
	public class apCalculatedLerpArea
	{
		// Members
		//-----------------------------------------------
		public apCalculatedLerpPoint _pointLT, _pointRT, _pointLB, _pointRB;
		public Vector2 _posMin = Vector2.zero, _posMax = Vector2.zero;

		//v1.5.0 추가
		//만약 해당 위치(LTRB)가 Control Param의 영역 가장자리라면, Area 비교시 일부는 생략한다.
		//잘못하면 float 오차에 의해서 가장자리에 위치할 때 영역 밖이라고 판단하면 실패할 수 있다.
		private bool _isLimit_L = false;
		private bool _isLimit_R = false;
		private bool _isLimit_T = false;
		private bool _isLimit_B = false;

		// Init
		//-----------------------------------------------
		public apCalculatedLerpArea(apCalculatedLerpPoint pointLT,
									apCalculatedLerpPoint pointRT,
									apCalculatedLerpPoint pointLB,
									apCalculatedLerpPoint pointRB,
									bool isLimit_L,
									bool isLimit_R,
									bool isLimit_T,
									bool isLimit_B)
		{
			_pointLT = pointLT;
			_pointRT = pointRT;
			_pointLB = pointLB;
			_pointRB = pointRB;

			_posMin = new Vector2(	Mathf.Min(_pointLB._pos.x, _pointRT._pos.x),
									Mathf.Min(_pointLB._pos.y, _pointRT._pos.y));
			_posMax = new Vector2(	Mathf.Max(_pointLB._pos.x, _pointRT._pos.x),
									Mathf.Max(_pointLB._pos.y, _pointRT._pos.y));

			_isLimit_L = isLimit_L;
			_isLimit_R = isLimit_R;
			_isLimit_T = isLimit_T;
			_isLimit_B = isLimit_B;
		}


		// Functions
		//-----------------------------------------------
		public void ReadyToCalculate()
		{
			_pointLT._calculatedWeight = 0.0f;
			_pointRT._calculatedWeight = 0.0f;
			_pointLB._calculatedWeight = 0.0f;
			_pointRB._calculatedWeight = 0.0f;
		}



		// Get / Set
		//-----------------------------------------------
		public bool IsInclude(Vector2 pos)
		{
			//이전
			//if (pos.x < _posLT.x || pos.x > _posRB.x ||
			//	pos.y < _posLT.y || pos.y > _posRB.y)
			//{
			//	return false;
			//}

			//변경 v1.5.0 : Limit Range 고려한다.
			//예 : 왼쪽 가장자리에 위치한 영역이라면, L 밖으로 나간건 실패로 간주하지 않는다.
			if (!_isLimit_L && pos.x < _posMin.x)//왼쪽 가장자리가 아닌데 왼쪽 밖으로 나갔다. - 실패
			{
				return false;
			}

			if(!_isLimit_R && pos.x > _posMax.x)
			{
				return false;
			}

			if(!_isLimit_T && pos.y > _posMax.y)
			{
				return false;
			}

			if(!_isLimit_B && pos.y < _posMin.y)
			{
				return false;
			}

			return true;
		}

	}

}
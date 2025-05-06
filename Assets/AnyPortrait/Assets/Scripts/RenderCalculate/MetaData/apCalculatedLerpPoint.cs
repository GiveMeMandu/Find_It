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
	/// Control Param의 보간을 위한 데이터
	/// 가상의 격자 형태에 배치되므로 위치 인덱스를 가진다.
	/// 2D 공간에 위치한다.
	/// </summary>
	public class apCalculatedLerpPoint
	{
		// Members
		//-----------------------------------------
		public Vector2 _pos = Vector2.zero;//Float / Vector2 값을 사용하는 경우
		public int _iPos = 0;//Int 값을 사용하는 경우

		//True이면 실제 데이터가 있는 것이며, False이면 미리 다른 키로부터 보간된 가상 Point이다.
		public bool _isRealPoint = true;

		//보간 처리를 위해 Param을 저장한다. 여러개 저장 가능 (가상 Point인 경우)
		public List<apCalculatedResultParam.ParamKeyValueSet> _refParams = new List<apCalculatedResultParam.ParamKeyValueSet>();
		public List<float> _refWeights = new List<float>();
		private int _nRefPoints = 0;

		public float _calculatedWeight = 0.0f;

		private apCalculatedResultParam.ParamKeyValueSet _cal_CurRefParam = null;
		private float _cal_CurRefWeight = 0.0f;


		//v1.4.7 : 연결된 SubParamList. 이 LerpPoint를 가지고 있는 객체이다.
		private apCalculatedResultParamSubList _parentCalParamSubList = null;

		// Init
		//-----------------------------------------
		public apCalculatedLerpPoint(	Vector2 vPos, bool isRealPoint,
										apCalculatedResultParamSubList parentCalParamSubList)
		{
			_pos = vPos;
			_isRealPoint = isRealPoint;

			if(_refParams == null) { _refParams = new List<apCalculatedResultParam.ParamKeyValueSet>(); }
			if(_refWeights == null) { _refWeights = new List<float>(); }
			_refParams.Clear();
			_refWeights.Clear();
			_nRefPoints = 0;

			_parentCalParamSubList = parentCalParamSubList;//v1.4.7
		}

		public apCalculatedLerpPoint(	float fPos, bool isRealPoint,
										apCalculatedResultParamSubList parentCalParamSubList)
		{
			_pos.x = fPos;
			_isRealPoint = isRealPoint;

			if(_refParams == null) { _refParams = new List<apCalculatedResultParam.ParamKeyValueSet>(); }
			if(_refWeights == null) { _refWeights = new List<float>(); }

			_refParams.Clear();
			_refWeights.Clear();
			_nRefPoints = 0;

			_parentCalParamSubList = parentCalParamSubList;//v1.4.7
		}

		public apCalculatedLerpPoint(	int iPos, bool isRealPoint,
										apCalculatedResultParamSubList parentCalParamSubList)
		{
			_iPos = iPos;
			_isRealPoint = isRealPoint;

			if(_refParams == null) { _refParams = new List<apCalculatedResultParam.ParamKeyValueSet>(); }
			if(_refWeights == null) { _refWeights = new List<float>(); }
			_refParams.Clear();
			_refWeights.Clear();
			_nRefPoints = 0;

			_parentCalParamSubList = parentCalParamSubList;//v1.4.7
		}


		// Functions
		//-----------------------------------------
		public void AddPoint(apCalculatedResultParam.ParamKeyValueSet point, float weight)
		{
			//이전
			// _refParams.Add(point);
			// _refWeights.Add(weight);

			//변경 v1.5.0
			int iRef = _refParams.IndexOf(point);
			if(iRef >= 0)
			{
				//이미 있는 값이다.
				_refWeights[iRef] = _refWeights[iRef] + weight;
			}
			else
			{
				//없는 값이다.
				_refParams.Add(point);
				_refWeights.Add(weight);
			}

			_nRefPoints = _refParams.Count;
		}

		public void AddPoints(apCalculatedLerpPoint lerpPoint, float weight)
		{
			for (int i = 0; i < lerpPoint._refParams.Count; i++)
			{
				AddPoint(lerpPoint._refParams[i], lerpPoint._refWeights[i] * weight);
			}
		}

		//추가 v1.5.0
		//이게 없어서 Virtual Lerp Point가 에러를 유발한다.
		public void NormalizeWeights()
		{
			_nRefPoints = _refParams.Count;
			if(_nRefPoints == 0)
			{
				return;
			}
			float totalWeights = 0.0f;
			for (int i = 0; i < _nRefPoints; i++)
			{
				totalWeights += _refWeights[i];
			}

			if(totalWeights > 0.0f)
			{
				for (int i = 0; i < _nRefPoints; i++)
				{
					_refWeights[i] /= totalWeights;
				}				
			}
		}



		public void CalculateITPWeight(ref float totalWeight)//v1.4.7 : totalWeight 추가
		{	
			float curWeight = 0.0f;
			
			for (int i = 0; i < _nRefPoints; i++)
			{
				//v1.4.6 변경
				_cal_CurRefParam = _refParams[i];
				_cal_CurRefWeight = _refWeights[i];

				//이전
				//_cal_CurRefParam._isCalculated = true;
				//_cal_CurRefParam._weight += _cal_CurRefWeight * _calculatedWeight;

				//v1.4.7 : Weight를 ref Total 결과에 넣고, 계산된 PKV를 리스트에 넣는다.
				curWeight = _cal_CurRefWeight * _calculatedWeight;
				totalWeight += curWeight;
				_parentCalParamSubList.OnParamKeyValueCalculated(_cal_CurRefParam, curWeight);
			}
		}


		// Get / Set
		//-----------------------------------------
	}
}
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
using System;
using System.Collections.Generic;


using AnyPortrait;

namespace AnyPortrait
{
    /// <summary>
    /// v1.5.0 추가 : 현재 컨트롤 파라미터의 값들을 저장해서 작업에 사용할 수 있다.
    /// 키프레임에 지정할 수도 있다.
    /// </summary>
    [Serializable]
    public class apControlParamValueSnapShot
    {
        // Sub Classes
        //-------------------------------------------
        /// <summary>
        /// 컨트롤 파라미터 값들이 저장된 세트. 이름도 저장할 수 있다.
        /// </summary>
        [Serializable]
        public class SnapShotSet
        {
            [SerializeField] public string _name = "";
            [SerializeField] public List<ControlParamValue> _valueList = null;

            public SnapShotSet()
            {

            }
        }

        [Serializable]
        public class ControlParamValue
        {
            [SerializeField] public int _controlParamID = -1;

            //타입
            [SerializeField] public apControlParam.TYPE _savedType = apControlParam.TYPE.Int;//타입이 맞아야 한다.

            //값들
            [SerializeField] public int _value_Int = 0;
            [SerializeField] public float _value_Float = 0.0f;
            [SerializeField] public Vector2 _value_Vector = Vector2.zero;

            

            public ControlParamValue()
            {

            }
        }

        // Members
        //-------------------------------------------
        [SerializeField]
        public List<SnapShotSet> _snapShots = null;

        // Init
        //-------------------------------------------
        public apControlParamValueSnapShot()
        {

        }

        
    }
}
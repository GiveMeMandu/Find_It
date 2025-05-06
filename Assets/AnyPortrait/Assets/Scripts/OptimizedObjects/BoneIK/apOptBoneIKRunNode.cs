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
    /// IK 연산 순서를 결정하는 노드 래퍼 클래스의 OPT 버전
    /// </summary>
    public class apOptBoneIKRunNode
    {
        // Members
        //----------------------------------
        public apOptBone _linkedBone = null;
        public apOptBone _linkedChainRootBone = null;//IK가 연산되는 Chain의 Root 본

        //실행 순서 관련
        //Depth에 무관한 실행 순서 (부모-자식 순서로 등록)
        public int _index = 0;

        //속성으로 지정된 "Order" 옵션. 
        //이 값이 클 수록 실행이 뒤로 미뤄진다.
        public int _depth = 0;

        public apOptBoneIKRunNode(apOptBone bone, int index, apOptBone chainRootBone)
        {
            _linkedBone = bone;
            _linkedChainRootBone = chainRootBone;
            if(_linkedChainRootBone == null)
            {
                //Debug.LogError("Chain Root Bone이 없다. [" + _linkedBone._name + "]");
                _linkedChainRootBone = _linkedBone;
            }

            _index = index;
            _depth = bone._IKDepth;//추가된 옵션을 실행한다.

            
        }


		/// <summary>
		/// IK의 초기 포즈 연산 여부를 초기화한다.
		/// </summary>
		public void ResetInitPoseFlag()
		{
			ResetInitPoseFlagRecursive(_linkedChainRootBone);
		}

		private void ResetInitPoseFlagRecursive(apOptBone bone)
		{
			bone._IKInitPoseWeightSum = 0.0f;//연산된 가중치 = 0
			
			int nChild = bone._childBones != null ? bone._childBones.Length : 0;
			if(nChild == 0)
			{
				return;
			}

			apOptBone childBone = null;
			for (int i = 0; i < nChild; i++)
			{
				childBone = bone._childBones[i];
				if(childBone == null || childBone == bone)
				{
					continue;
				}
				ResetInitPoseFlagRecursive(childBone);
			}
		}



        /// <summary>
        /// IK를 계산한다. Recursive하지 않으며, 이 노드의 본의 Chain만 연산한다.
        /// </summary>
        public bool CalculateIK(apPortrait.IK_METHOD IKMethod)
        {
            return _linkedBone.CalculateIK(IKMethod, false);//Recursive > false
        }


        /// <summary>
        /// IK를 연산한 결과를 입력한다.
        /// </summary>
        public void MakeMatrixIK()
        {
            _linkedChainRootBone.MakeWorldMatrixForIK_ByRunNode(true, false);
        }

        /// <summary>
        /// IK를 연산한 결과를 입력한다. (Sync용)
        /// </summary>
        public void MakeMatrixIKAsSync()
        {
            _linkedChainRootBone.MakeWorldMatrixForIKAsSyncBones_ByRunNode(true, false);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Helper
{
    public static class LayerManager
    {
        public static int DefaultLayer = 0;
        public static int TransparentFXLayer = 1;
        public static int IgnoreRaycastLayer = 2;
        public static int HiddenObjectLayer = 3;
        public static int OverHiddenObjectLayer = 7;
        public static int WaterLayer = 4;
        public static int UILayer = 5;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            DefaultLayer = 0;
            TransparentFXLayer = 1;
            IgnoreRaycastLayer = 2;
            HiddenObjectLayer = 3;
            WaterLayer = 4;
            UILayer = 5;
            OverHiddenObjectLayer = 7;
        }

        /// <summary>
        /// 주어진 단일 레이어 인덱스에 해당하는 LayerMask를 반환합니다.
        /// </summary>
        /// <param name="layerIndex">LayerMask에 포함할 레이어의 인덱스입니다.</param>
        /// <returns>지정된 레이어만 포함하는 LayerMask입니다.</returns>
        public static LayerMask GetLayerMask(int layerIndex)
        {
            // LayerMask 구조체는 암시적으로 int에서 변환될 수 있습니다.
            return 1 << layerIndex;
        }

        //* 레이어마스크가 레이어에 있는지 체크
        public static bool LayerInLayerMask(int layer, LayerMask layerMask)
        {
            // LayerMask는 비트 플래그이므로 비트 연산을 사용합니다.
            return ((1 << layer) & layerMask.value) != 0;
        }
    }
}

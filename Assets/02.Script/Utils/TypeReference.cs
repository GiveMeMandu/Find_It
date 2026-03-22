using System;
using UnityEngine;

namespace Utils
{
    /// <summary>
    /// Type을 인스펙터에 노출하기 위한 직렬화 래퍼 클래스입니다.
    /// 문자열로 클래스 이름을 저장하여 유지합니다.
    /// </summary>
    [Serializable]
    public class TypeReference : ISerializationCallbackReceiver
    {
        [SerializeField]
        private string typeName;

        private Type type;

        public Type Type
        {
            get
            {
                if (type == null && !string.IsNullOrEmpty(typeName))
                {
                    // 어셈블리 내에서 직접 타입 탐색 (Unity 기본 및 사용자 스크립트)
                    type = Type.GetType(typeName);
                    
                    if (type == null)
                    {
                        // Assembly-CSharp 내의 타입을 찾는 우회 방법
                        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                        foreach (var assembly in assemblies)
                        {
                            type = assembly.GetType(typeName);
                            if (type != null) break;
                        }
                    }
                }
                return type;
            }
            set
            {
                type = value;
                typeName = value?.AssemblyQualifiedName;
            }
        }

        public void OnBeforeSerialize()
        {
            if (type != null)
            {
                typeName = type.AssemblyQualifiedName;
            }
        }

        public void OnAfterDeserialize()
        {
            type = null; // 요청 시에 다시 파싱하도록 null로 초기화
        }
    }
}

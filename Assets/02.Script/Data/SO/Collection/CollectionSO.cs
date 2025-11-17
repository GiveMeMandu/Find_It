using System.Collections;
using System.Collections.Generic;
using Data;
using I2.Loc;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
namespace SO
{
    [CreateAssetMenu(fileName = "Collection", menuName = "Collection/Collection Info")]
    public class CollectionSO : ScriptableObject
    {
        [LabelText("이미지")] public Sprite collectionImage;
        [LabelText("이름")] [TermsPopup("Collection/Name/")] 
        public string collectionName;
        [LabelText("해당 씬")] public SceneName scene;

        [Button("자동으로 컬렉션 정보 설정")]
        public void SetCollectionInfo(){
            // 에셋 이름에서 기본 텍스트 키를 자동으로 생성
            string assetName = this.name;
            collectionName = "Collection/Name/" + assetName;

            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }
    }
}
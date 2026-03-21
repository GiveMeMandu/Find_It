using System.Collections;
using System.Collections.Generic;
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
        [LabelText("챕터 인덱스")] 
        [Tooltip("StageManager의 chapters 리스트 인덱스 (0부터 시작)")]
        public int chapterIndex;

        [LabelText("인게임 매칭 이미지들")]
        [Tooltip("이 컬렉션으로 인정될 인게임 스프라이트들입니다. (자동 매핑 가능)")]
        public List<Sprite> inGameSprites = new List<Sprite>();

        [LabelText("매핑 키워드")]
        [Tooltip("자동 매핑 시 사용할 키워드(쉼표로 구분). 비워두면 에셋 이름을 사용합니다.")]
        public string mappingKeywords;

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
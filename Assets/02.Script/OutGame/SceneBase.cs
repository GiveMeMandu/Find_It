using System.Collections;
using System.Collections.Generic;
using Manager;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OutGame
{
    public class SceneBase : MonoBehaviour
    {
        [BoxGroup("음악 설정")]
        public Data.BGMEnum bGMEnum;
        [BoxGroup("음악 설정")] [LabelText("무한 반복 재생")]
        public bool isLoop;
        [BoxGroup("음악 설정")] [LabelText("음악 소리")]  [Range(0, 1)]
        public float volume = 0.5f;
        protected virtual void Awake()
        {
            if(Global.Instance == null) 
            {
                LoadingSceneManager.LoadScene(); 
                return;
            }
            if (Global.Instance != null)
            {
                Global.CurrentScene = this;
            }
        }
        protected virtual void Start()
        {
            Global.SoundManager.PlayMusic(bGMEnum, volume, isLoop:true);
        }
        
        protected virtual void OnDestroy()
        {
            if (Global.UIManager != null)
            {
                Global.UIManager.CloseAllPages();
            }
        }
    }
}
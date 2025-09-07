using Manager;
using UnityEngine;

public class ClickTouchSound : MonoBehaviour
{
    [Header("사운드 설정")]
    [SerializeField] private Data.SFXEnum clickSoundType = Data.SFXEnum.ClickStretch;
    [SerializeField] private bool playOnAwake = false;
    [SerializeField] private bool useRandomSound = true;
    
    private void Awake()
    {
        if (playOnAwake)
        {
            PlayClickSound();
        }
    }
    
    /// <summary>
    /// 클릭 사운드를 재생합니다.
    /// </summary>
    public void PlayClickSound()
    {
        if (Global.SoundManager != null)
        {
            Data.SFXEnum soundToPlay = useRandomSound ? GetRandomClickSound() : clickSoundType;
            Global.SoundManager.PlaySFX(soundToPlay);
        }
        else
        {
            Debug.LogWarning("SoundManager가 초기화되지 않았습니다.");
        }
    }
    
    /// <summary>
    /// ClickStretch와 ClickStretch2 중 랜덤으로 하나를 선택합니다.
    /// </summary>
    /// <returns>랜덤으로 선택된 사운드 타입</returns>
    private Data.SFXEnum GetRandomClickSound()
    {
        return Random.Range(0, 2) == 0 ? Data.SFXEnum.ClickStretch : Data.SFXEnum.ClickStretch2;
    }
    
    /// <summary>
    /// 사운드 타입을 변경합니다.
    /// </summary>
    /// <param name="newSoundType">새로운 사운드 타입</param>
    public void SetSoundType(Data.SFXEnum newSoundType)
    {
        clickSoundType = newSoundType;
    }
}

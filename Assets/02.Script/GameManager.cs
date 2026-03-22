using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeDOTween()
    {
        // DOTween 초기화 및 캐퍼시티 확장 (IndexOutOfRangeException 방지)
        // Awake 단계보다 먼저 실행되어 기본 Capacity(200, 50)로 초기화되는 것을 막습니다.
        DOTween.Init(true, true, LogBehaviour.Default).SetCapacity(4000, 1000);
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (!PlayerPrefs.HasKey("IsTutorial"))
        {
            PlayerPrefs.SetInt("IsTutorial", 1);
            PlayerPrefs.Save();
        }
        SetResolution();
        Application.targetFrameRate = 60;
    }
    
    /* 해상도 설정하는 함수 */
    public static void SetResolution()
    {
        int setWidth = 2160; // 사용자 설정 너비
        int setHeight = 1080; // 사용자 설정 높이

        int deviceWidth = Screen.width; // 기기 너비 저장
        int deviceHeight = Screen.height; // 기기 높이 저장
        if(deviceWidth / deviceHeight > 2) {
            Screen.SetResolution(setWidth, (int)(((float)deviceHeight / deviceWidth) * setWidth), true); // SetResolution 함수 제대로 사용하기

            if ((float)setWidth / setHeight < (float)deviceWidth / deviceHeight) // 기기의 해상도 비가 더 큰 경우
            {
                float newWidth = ((float)setWidth / setHeight) / ((float)deviceWidth / deviceHeight); // 새로운 너비
                Camera.main.rect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f); // 새로운 Rect 적용
            }
            else // 게임의 해상도 비가 더 큰 경우
            {
                float newHeight = ((float)deviceWidth / deviceHeight) / ((float)setWidth / setHeight); // 새로운 높이
                Camera.main.rect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight); // 새로운 Rect 적용
            }

        }

    }
    public void RestAllData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        SceneManager.LoadScene(0);
    }
}

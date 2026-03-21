using UnityEngine;

public class OptionManager
{
    private const string KeyScreenMode = "Option_ScreenMode";
    private const string KeyResolutionIndex = "Option_ResolutionIndex";
    private const string KeyVSync = "Option_VSync";
    private const string KeyTargetFrameRate = "Option_TargetFrameRate";
    private const string KeyQualityLevel = "QualitySettings";

    public void Init()
    {
        // 저장된 설정 불러오기 (기본값: 전체 창모드 = 1)
        // 0: 전체화면, 1: 전체 창모드, 2: 창모드
        int savedScreenMode = PlayerPrefs.GetInt(KeyScreenMode, 1);
        SetScreenMode(savedScreenMode);

        // 저장된 해상도 불러오기
        if (PlayerPrefs.HasKey(KeyResolutionIndex))
        {
            int savedResIndex = PlayerPrefs.GetInt(KeyResolutionIndex);
            SetResolution(savedResIndex);
        }

        // 수직동기화 불러오기 (기본값: On = 1)
        bool savedVSync = PlayerPrefs.GetInt(KeyVSync, 1) == 1;
        SetVSync(savedVSync);

        // 프레임 제한 불러오기 (기본값: 60)
        int savedFPS = PlayerPrefs.GetInt(KeyTargetFrameRate, 60);
        SetTargetFrameRate(savedFPS);

        // 그래픽 품질 불러오기
        if (PlayerPrefs.HasKey(KeyQualityLevel))
        {
            int savedQuality = PlayerPrefs.GetInt(KeyQualityLevel);
            SetQualityLevel(savedQuality);
        }
    }

    public int GetScreenModeIndex()
    {
        return PlayerPrefs.GetInt(KeyScreenMode, 1);
    }

    public void SetScreenMode(int index)
    {
        FullScreenMode mode = index switch
        {
            0 => FullScreenMode.ExclusiveFullScreen,
            1 => FullScreenMode.FullScreenWindow,
            2 => FullScreenMode.Windowed,
            _ => FullScreenMode.FullScreenWindow
        };

        Screen.fullScreenMode = mode;

        // 설정 저장
        PlayerPrefs.SetInt(KeyScreenMode, index);
        PlayerPrefs.Save();
    }

    public int GetResolutionIndex()
    {
        var resolutions = Screen.resolutions;
        int savedIndex = PlayerPrefs.GetInt(KeyResolutionIndex, -1);

        if (savedIndex >= 0 && savedIndex < resolutions.Length)
        {
            return savedIndex;
        }

        // 저장된 값이 없으면 현재 해상도와 일치하는 인덱스 찾기
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
            {
                return i;
            }
        }

        return resolutions.Length - 1; // 기본값은 가장 높은 해상도
    }

    public void SetResolution(int index)
    {
        var resolutions = Screen.resolutions;
        if (index >= 0 && index < resolutions.Length)
        {
            var res = resolutions[index];
            Screen.SetResolution(res.width, res.height, Screen.fullScreenMode, res.refreshRateRatio);

            PlayerPrefs.SetInt(KeyResolutionIndex, index);
            PlayerPrefs.Save();
        }
    }

    public void SetResolution(int width, int height, RefreshRate refreshRate)
    {
        var resolutions = Screen.resolutions;
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == width &&
                resolutions[i].height == height &&
                resolutions[i].refreshRateRatio.value == refreshRate.value)
            {
                SetResolution(i);
                return;
            }
        }

        // 일치하는 것을 못 찾으면 그냥 설정
        Screen.SetResolution(width, height, Screen.fullScreenMode, refreshRate);
    }

    public bool GetVSync()
    {
        return PlayerPrefs.GetInt(KeyVSync, 1) == 1;
    }

    public void SetVSync(bool isOn)
    {
        QualitySettings.vSyncCount = isOn ? 1 : 0;
        PlayerPrefs.SetInt(KeyVSync, isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public int GetTargetFrameRate()
    {
        return PlayerPrefs.GetInt(KeyTargetFrameRate, 60);
    }

    public void SetTargetFrameRate(int fps)
    {
        Application.targetFrameRate = fps;
        PlayerPrefs.SetInt(KeyTargetFrameRate, fps);
        PlayerPrefs.Save();
    }

    public int GetQualityLevel()
    {
        return QualitySettings.GetQualityLevel();
    }

    public void SetQualityLevel(int level)
    {
        if (level >= 0 && level < QualitySettings.names.Length)
        {
            QualitySettings.SetQualityLevel(level);
            PlayerPrefs.SetInt(KeyQualityLevel, level);
            PlayerPrefs.Save();
        }
    }
}

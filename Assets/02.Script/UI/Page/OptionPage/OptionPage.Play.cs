using System.Collections.Generic;
using UnityEngine;
using OptionPageNamespace;
using SnowRabbit;
using Manager;

namespace UI
{
    public partial class OptionPage
    {
        // Play 탭에 있던 옵션 그룹 생성 메서드는 Options 탭으로 통합되었으므로, 해당 메서드의 선언은 지우거나 사용하지 않습니다.
        
        private void CreateLanguageOption()
        {
            var optionGroup = CreateOptionGroup("UI/Option/Language");

            // I2 Localization에서 사용 가능한 언어 목록 가져오기
            var languageList = new List<string>();
            var currentLanguageIndex = 0;

            if (I2.Loc.LocalizationManager.Sources.Count > 0)
            {
                var source = I2.Loc.LocalizationManager.Sources[0];
                for (int i = 0; i < source.mLanguages.Count; ++i)
                {
                    if (source.mLanguages[i].IsEnabled())
                    {
                        languageList.Add(source.mLanguages[i].Name);
                        if (source.mLanguages[i].Name == I2.Loc.LocalizationManager.CurrentLanguage)
                        {
                            currentLanguageIndex = languageList.Count - 1;
                        }
                    }
                }
            }

            // 드롭다운 생성 및 초기화
            var languageDropdown = optionGroup.CreateOptionDropdown();
            languageDropdown.Init(
                "UI/Option/Language",
                languageList,
                currentLanguageIndex,
                (index) =>
                {
                    if (index >= 0 && index < languageList.Count)
                    {
                        I2.Loc.LocalizationManager.CurrentLanguage = languageList[index];
                    }
                }
            );
        }

        private void CreateScreenOption()
        {
            var screenGroup = CreateOptionGroup("UI/Option/ScreenSettings");

            // 화면 모드 드롭다운
            var screenModeList = new List<string> { 
                "UI/Option/ExclusiveFullScreen", 
                "UI/Option/FullScreenWindow", 
                "UI/Option/Windowed" 
            };
            var currentScreenModeIndex = Global.OptionManager.GetScreenModeIndex();

            var screenDropdown = screenGroup.CreateOptionDropdown();
            screenDropdown.Init(
                "UI/Option/ScreenMode",
                screenModeList,
                currentScreenModeIndex,
                (index) =>
                {
                    Global.OptionManager.SetScreenMode(index);
                },
                true
            );

            // 해상도 및 주사율 분리
            var allResolutions = Screen.resolutions;
            if (allResolutions.Length == 0) return;

            // 1. 유니크한 해상도(가로x세로) 목록 생성
            var uniqueResolutions = new List<(int width, int height)>();
            foreach (var res in allResolutions)
            {
                if (!uniqueResolutions.Exists(r => r.width == res.width && r.height == res.height))
                {
                    uniqueResolutions.Add((res.width, res.height));
                }
            }
            // 해상도 내림차순 정렬 (큰 해상도가 위로)
            uniqueResolutions.Sort((a, b) => (b.width * b.height).CompareTo(a.width * a.height));

            var resolutionStrings = uniqueResolutions.ConvertAll(r => $"{r.width} x {r.height}");

            // 현재 설정된 해상도 정보 가져오기
            var currentResIndex = Global.OptionManager.GetResolutionIndex();
            var currentRes = allResolutions[currentResIndex];
            
            int selectedResIndex = uniqueResolutions.FindIndex(r => r.width == currentRes.width && r.height == currentRes.height);
            if (selectedResIndex == -1) selectedResIndex = 0;

            var resolutionDropdown = screenGroup.CreateOptionDropdown();
            var fpsDropdown = screenGroup.CreateOptionDropdown();

            // 프레임 제한 리스트 (사용자 요청 리스트: 30, 60, 75, 90, 120, 144, 165, 240, 300)
            var fpsValues = new List<int> { 30, 60, 75, 90, 120, 144, 165, 240, 300 };
            var fpsStrings = fpsValues.ConvertAll(fps => $"{fps}Hz");

            // 무제한 옵션 추가 (선택 사항이지만 보통 포함됨)
            fpsValues.Add(-1);
            fpsStrings.Add("UI/Option/FPS_Unlimited");

            int currentFPS = Global.OptionManager.GetTargetFrameRate();
            int selectedFPSIndex = fpsValues.IndexOf(currentFPS);
            if (selectedFPSIndex == -1) selectedFPSIndex = 1; // 기본값 60Hz

            fpsDropdown.Init(
                "UI/Option/FrameRateLimit",
                fpsStrings,
                selectedFPSIndex,
                (index) =>
                {
                    Global.OptionManager.SetTargetFrameRate(fpsValues[index]);
                },
                true // "UI/Option/FPS_Unlimited"와 라벨 로컬라이징을 위해 true 유지
            );

            resolutionDropdown.Init(
                "UI/Option/Resolution",
                resolutionStrings,
                selectedResIndex,
                (index) =>
                {
                    var targetRes = uniqueResolutions[index];
                    // 해상도 변경 시 현재 모니터의 기본 주사율 사용 (프레임 제한은 별도 유지)
                    Global.OptionManager.SetResolution(targetRes.width, targetRes.height, Screen.currentResolution.refreshRateRatio);
                },
                false
            );

            // 수직동기화 토글
            var vsyncToggle = screenGroup.CreateOptionToggle();
            vsyncToggle.Init(
                "UI/Option/VSync",
                Global.OptionManager.GetVSync(),
                (isOn) =>
                {
                    Global.OptionManager.SetVSync(isOn);
                }
            );
        }
    }
}

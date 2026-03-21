using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using OptionPageNamespace;
using SnowRabbit;
using Manager;

namespace UI
{
    public partial class OptionPage
    {
        private void CreateGraphicsOptionGroup()
        {
            var graphicsGroup = CreateOptionGroup("UI/Option/GraphicsSettings");

            // 그래픽 품질 드롭다운
            var qualityNames = QualitySettings.names.ToList();
            var currentQualityIndex = Global.OptionManager.GetQualityLevel();

            var qualityDropdown = graphicsGroup.CreateOptionDropdown();
            
            // 품질 이름들을 로컬라이징 키로 사용할 수 있도록 "UI/Option/Quality_" 접두사를 붙여서 시도해볼 수 있음.
            // 하지만 일단은 기본 이름을 사용하고, 필요시 로컬라이징 처리를 하도록 함.
            qualityDropdown.Init(
                "UI/Option/QualityLevel",
                qualityNames,
                currentQualityIndex,
                (index) =>
                {
                    Global.OptionManager.SetQualityLevel(index);
                },
                false
            );
        }
    }
}

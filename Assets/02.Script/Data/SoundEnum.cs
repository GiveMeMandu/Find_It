using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


namespace Data
{
    [LabelText("음악 이름")]
    public enum BGMEnum
    {
        NONE,
        Title,
        Stage1,
        Stage2,
    }
    [LabelText("효과음 이름")]
    public enum SFXEnum
    {
        NONE,
        ClickStretch,
        ClickStretch2,
        ClickUI,

    }
}
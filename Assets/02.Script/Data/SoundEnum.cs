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
        Huh,
        AirHead,
        FunOuting,
        Chatter,
        Silly
    }
    [LabelText("효과음 이름")]
    public enum SFXEnum
    {
        NONE,
        EAT,
        COOK,
        COOK_DONE,
        CheckOut,
    }
}
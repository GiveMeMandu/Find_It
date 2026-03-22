using Manager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityWeld.Binding;
using System;
using System.Numerics;

namespace UI.Page
{
    [Binding]
    public class GuidePage : PageViewModel
    {
        public override bool BlockEscape => true; // 가이드 페이지에서는 Escape 키 입력 차단
        public override void Init(params object[] parameters)
        {
            
        }
    }
}
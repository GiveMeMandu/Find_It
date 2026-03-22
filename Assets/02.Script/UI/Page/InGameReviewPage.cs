using System;
using Manager;
using OutGame;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityWeld.Binding;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;
using System.Threading;
using Data;

namespace UI.Page
{
    [Binding]
    public class InGameReviewPage : PageViewModel
    {
        public override void Init(params object[] parameters)
        {
        }

        [Binding]
        public void OpenSteamPage()
        {
            Application.OpenURL("https://store.steampowered.com/app/4324730/Find_Bunny__A_Little_Hidden_Journey/");
        }
        [Binding]
        public void OpenFeedbackPage()
        {
            Application.OpenURL("https://www.reddit.com/r/Findbunny/");
        }
    }
}
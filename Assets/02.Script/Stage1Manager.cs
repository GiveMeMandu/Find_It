using System.Collections;
using System.Collections.Generic;
using DeskCat.FindIt.Scripts.Core.Main.System;
using UnityEngine;
using UnityEngine.Playables;

public class Stage1Manager : MonoBehaviour, IStageManager
{
    [SerializeField] private LevelManager _levelManager;
    [SerializeField] private PlayableDirector _playableDirector;

    private void Start() {
        _playableDirector.enabled = false;
        StartStage();
    }

    public void StartStage() {
        bool isTutorial = PlayerPrefs.GetInt("IsTutorial") == 1;
        if(isTutorial) {
            PlayerPrefs.SetInt("IsTutorial", 0);
            _levelManager.gameObject.SetActive(false);
            _playableDirector.initialTime = 0;
            _playableDirector.enabled = true;
        }
        else {
            _levelManager.gameObject.SetActive(true);
        }
    }

    public void ClearStage() {
        PlayerPrefs.Save();
    }
}
    

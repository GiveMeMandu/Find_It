using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TutorialControllerManager : MonoBehaviour
{
    public List<TutorialController> TutorialControllers;
    void Start()
    {
        if(TutorialControllers.Count < 0)
        {
            TutorialControllers = GetComponentsInChildren<TutorialController>().ToList();
        }
    }

    public TutorialController StartTutorial(string tutorialName = "")
    {
        foreach (var tutorialController in TutorialControllers)
        {
            if (tutorialController.TutorialName == tutorialName)
            {
                tutorialController.SetNextTutorial();
                return tutorialController;
            }
        }
        return null;
    }
    
}

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialController : MonoBehaviour
{
	public string TutorialName = "";
	public Action OnCompletedAllTutorials;
	[SerializeField]
	private	List<TutorialBase>	tutorials;
	[SerializeField]
	private	string				nextSceneName = "";

	private TutorialBase		currentTutorial = null;
	public bool                 IsPlaying => currentTutorial != null;
	public	int					currentIndex = -1;

	[SerializeField] private bool autoStart = false;
	[SerializeField] private bool autoSet = false;

	private void Start()
	{
		if (autoStart)
		{
			SetNextTutorial();
		}
		if (autoSet)
		{
			AutoFillTutorials();
		}
	}

	private void Update()
	{
		if ( currentTutorial != null )
		{
			currentTutorial.Execute(this);
		}
	}

	public void SetNextTutorial()
	{
		Debug.Log($"TutorialController: SetNextTutorial called. Current Index: {currentIndex}");
		// 현재 튜토리얼의 Exit() 메소드 호출
		if ( currentTutorial != null )
		{
			currentTutorial.Exit();
		}

		// 마지막 튜토리얼을 진행했다면 CompletedAllTutorials() 메소드 호출
		if ( currentIndex >= tutorials.Count-1 )
		{
			CompletedAllTutorials();
			return;
		}

		// 다음 튜토리얼 과정을 currentTutorial로 등록
		currentIndex ++;
		currentTutorial = tutorials[currentIndex];

		// 새로 바뀐 튜토리얼의 Enter() 메소드 호출
		Debug.Log($"TutorialController: SetNextTutorial - {currentTutorial.GetType().Name}");
		currentTutorial.Enter();
	}

	public void CompletedAllTutorials()
	{
		currentTutorial = null;

		// 행동 양식이 여러 종류가 되었을 때 코드 추가 작성
		// 현재는 씬 전환

		Debug.Log("Complete All");

		if ( !nextSceneName.Equals("") )
		{
			SceneManager.LoadScene(nextSceneName);
		}
		OnCompletedAllTutorials?.Invoke();
	}
	[Button("튜토리얼 자동 할당")]
	public void AutoFillTutorials()
	{
		tutorials.Clear();
		var tutorialObjects = GetComponentsInChildren<TutorialBase>(true);
		foreach (var tutorial in tutorialObjects)
		{
			tutorials.Add(tutorial);
		}
	}

#if UNITY_EDITOR
	[TitleGroup("튜토리얼 객체 동적 생성")]
	[ValueDropdown("GetTutorialTypes")]
	[LabelText("튜토리얼 종류")]
	public System.Type tutorialTypeToCreate;

	[TitleGroup("튜토리얼 객체 동적 생성")]
	[Button("선택한 튜토리얼 자식 객체로 생성", ButtonSizes.Large)]
	public void CreateSelectedTutorial()
	{
		if (tutorialTypeToCreate == null)
		{
			Debug.LogWarning("생성할 튜토리얼 타입을 선택해주세요.");
			return;
		}

		GameObject go = new GameObject(tutorialTypeToCreate.Name);
		go.transform.SetParent(this.transform);
		go.transform.localPosition = Vector3.zero;
		go.transform.localScale = Vector3.one;
		
		UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create " + tutorialTypeToCreate.Name);
		go.AddComponent(tutorialTypeToCreate);

		UnityEditor.Selection.activeGameObject = go;
		
		// 생성 후 자동으로 리스트 갱신
		AutoFillTutorials();
	}

	private System.Collections.IEnumerable GetTutorialTypes()
	{
		var list = new Sirenix.OdinInspector.ValueDropdownList<System.Type>();
		var baseType = typeof(TutorialBase);
		
		foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
		{
			foreach (var type in assembly.GetTypes())
			{
				if (baseType.IsAssignableFrom(type) && !type.IsAbstract && type.IsClass && type != baseType)
				{
					list.Add(type.Name, type);
				}
			}
		}
		return list;
	}
#endif
}


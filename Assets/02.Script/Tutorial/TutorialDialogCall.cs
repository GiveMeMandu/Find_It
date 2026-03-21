using Manager;
using UI.Page;
using UnityEngine;

public class TutorialDialogCall : TutorialBase
{
	[SerializeField] 
	private string tutorialName;
	private TutorialController tutorialController;
	private bool isCompleted = false;
	public override void Enter()
	{
		tutorialController.OnCompletedAllTutorials += OnCompletedAllTutorials;
	}

	private void OnCompletedAllTutorials()
	{
		isCompleted = true;
	}

	public override void Execute(TutorialController controller)
	{
		// 현재 분기의 대사 진행이 완료되면
		if ( isCompleted == true )
		{
			tutorialController.OnCompletedAllTutorials -= OnCompletedAllTutorials;
			Global.UIManager.ClosePage();
			// 다음 튜토리얼로 이동
			controller.SetNextTutorial();
		}
	}

	public override void Exit()
	{
	}
}


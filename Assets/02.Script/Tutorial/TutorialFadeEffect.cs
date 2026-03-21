using UnityEngine;

public class TutorialFadeEffect : TutorialBase
{
	[SerializeField]
	private	FadeScript	fadeEffect;
	[SerializeField]
	private	bool		isFadeIn = false;
	private	bool		isCompleted = false;

	public override void Enter()
	{
		if ( isFadeIn == true )
		{
			fadeEffect.FadeIn(OnAfterFadeEffect);
		}
		else
		{
			fadeEffect.FadeOut(OnAfterFadeEffect);
		}
	}

	private void OnAfterFadeEffect()
	{
		isCompleted = true;
	}

	public override void Execute(TutorialController controller)
	{
		if ( isCompleted == true )
		{
			controller.SetNextTutorial();
		}
	}

	public override void Exit()
	{
	}
}


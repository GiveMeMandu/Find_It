using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialScaleDown : TutorialBase
{
	[SerializeField]
	private	RectTransform	scaleTransform;
	private	bool			isCompleted = false;

	public override void Enter()
	{
		scaleTransform.gameObject.SetActive(true);

		StartCoroutine("TimeCheck");
	}

	public override void Execute(TutorialController controller)
	{
		scaleTransform.sizeDelta -= 20 * Vector2.one * Time.deltaTime;

		if ( isCompleted == true )
		{
			controller.SetNextTutorial();
		}
	}

	private IEnumerator TimeCheck()
	{
		yield return new WaitForSeconds(1.5f);

		isCompleted = true;
	}

	public override void Exit()
	{
		//scaleTransform.gameObject.SetActive(false);
	}
}

using UnityEngine;

[System.Serializable]
public struct SceneVisibleObject
{
	public GameObject visibleObject;
	public bool		  visible;
}

public class TutorialVisible : TutorialBase
{
	[SerializeField]
	private	SceneVisibleObject[] SceneObjects;
	public void SetVisible(bool visible)
	{
		foreach (var sceneObject in SceneObjects)
		{
			sceneObject.visibleObject.SetActive(visible);
		}
	}

	public override void Enter()
	{
		for ( int i = 0; i < SceneObjects.Length; ++ i )
		{
			SceneObjects[i].visibleObject.SetActive(SceneObjects[i].visible);
		}
	}

	public override void Execute(TutorialController controller)
	{
		controller.SetNextTutorial();
	}

	public override void Exit()
	{
	}
}


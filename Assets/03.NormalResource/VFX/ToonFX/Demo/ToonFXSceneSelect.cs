using UnityEngine;
using UnityEngine.SceneManagement;

namespace ToonFX
{

public class ToonFXSceneSelect : MonoBehaviour
{
	
	public bool GUIHide = false;
	
    public void LoadSceneDemo1()
    {
        SceneManager.LoadScene("ToonFXScene1");
    }
    public void LoadSceneDemo2()
    {
        SceneManager.LoadScene("ToonFXScene2");
    }
    public void LoadSceneDemo3()
    {
        SceneManager.LoadScene("ToonFXScene14");
    }
    public void LoadSceneDemo4()
    {
        SceneManager.LoadScene("ToonFXScene4");
    }
    public void LoadSceneDemo5()
    {
        SceneManager.LoadScene("ToonFXScene5");
    }
    public void LoadSceneDemo6()
    {
        SceneManager.LoadScene("ToonFXScene6");
    }
    public void LoadSceneDemo7()
    {
        SceneManager.LoadScene("ToonFXScene7");
    }
    public void LoadSceneDemo8()
    {
        SceneManager.LoadScene("ToonFXScene8");
    }
    public void LoadSceneDemo9()
    {
        SceneManager.LoadScene("ToonFXScene9");
    }
    public void LoadSceneDemo10()
    {
        SceneManager.LoadScene("ToonFXScene10");
    }
	public void LoadSceneDemo11()
    {
        SceneManager.LoadScene("ToonFXScene11");
    }
	public void LoadSceneDemo12()
    {
        SceneManager.LoadScene("ToonFXScene12");
    }
	public void LoadSceneDemo13()
    {
        SceneManager.LoadScene("ToonFXScene13");
    }
	public void LoadSceneDemo14()
    {
        SceneManager.LoadScene("ToonFXScene3");
    }
	public void LoadSceneDemo15()
    {
        SceneManager.LoadScene("ToonFXScene15");
    }

void Update ()
{
 
     if(Input.GetKeyDown(KeyCode.J))
	 {
         GUIHide = !GUIHide;
     
         if (GUIHide)
		 {
             GameObject.Find("SceneSelectCanvas").GetComponent<Canvas> ().enabled = false;
			 GameObject.Find("Canvas").GetComponent<Canvas> ().enabled = false;
         }
		 else
		 {
             GameObject.Find("SceneSelectCanvas").GetComponent<Canvas> ().enabled = true;
			 GameObject.Find("Canvas").GetComponent<Canvas> ().enabled = true;
         }
     }
}
}
}
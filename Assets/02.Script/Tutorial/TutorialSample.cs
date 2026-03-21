using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TutorialSample : MonoBehaviour
{
	[Header("Tutorial01 - Fade Effect")]
	[SerializeField]
	private	FadeScript		fadeImage;								// 페이드 효과 재생을 위한 이미지

	[Header("Tutorial02 - Dialog")]
	[SerializeField]
	private	TextMeshProUGUI	textName;								// 화자 이름
	[SerializeField]
	private	TextMeshProUGUI	textDialog;								// 대사
	[SerializeField]
	[TextArea(0, 1)]
	private	string			stringDialog = "대사오류";		// 대사
	[SerializeField]
	private	float			typingSpeed = 0.5f;						// 대사 타이핑 속도
	[SerializeField]
	private	GameObject		ojbectArrow;							// 커서 이미지
	[SerializeField]
	private	float			waitTime = 2.0f;						// 대기 시간

	[Header("Tutorial03 - SFX")]
	[SerializeField]
	private	AudioSource		audioBoom;								// 폭발음 재생

	[SerializeField]
	private	string			nextSceneName;							// 다음 씬 이름

	private void Start()
	{
		// 화면이 점점 밝아지도록. 화면이 완전히 밝아지면 OnDialog() 메소드 호출
		fadeImage.FadeIn(OnDialog);
	}

	private void OnDialog()
	{
		// 화자 이름 설정 및 활성화
		textName.text = "연구원A";
		textName.gameObject.SetActive(true);

		// 대사 활성화 및 타이핑 효과 재생
		textDialog.gameObject.SetActive(true);
		StartCoroutine(nameof(TypingTextAndWaitTime));
	}

	private IEnumerator TypingTextAndWaitTime()
	{
		int index = 0;

		while ( index < stringDialog.Length )
		{
			textDialog.text = stringDialog.Substring(0, index);

			index ++;

			yield return new WaitForSeconds(typingSpeed);
		}

		// 대사가 완료되었을 때 출력되는 커서 활성화
		ojbectArrow.SetActive(true);

		// 2초 대기
		yield return new WaitForSeconds(waitTime);

		// 화면을 점점 어둡게. 완전히 어두워지면 PlaySoundAndChangeScene() 메소드 호출
		fadeImage.FadeOut(PlaySoundAndChangeScene);
	}

	private void PlaySoundAndChangeScene()
	{
		// 폭발 사운드 재생
		StartCoroutine(nameof(OnPlaySoundAndChangeScene));
	}

	private IEnumerator OnPlaySoundAndChangeScene()
	{
		// 사운드 재생
		audioBoom.Play();

		while ( true )
		{
			// 사운드 재생이 완료되면
			if ( audioBoom.isPlaying == false )
			{
				// 씬 전환
				SceneManager.LoadScene(nextSceneName);
			}

			yield return null;
		}
	}
}


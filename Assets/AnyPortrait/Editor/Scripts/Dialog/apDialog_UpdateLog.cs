/*
*	Copyright (c) RainyRizzle Inc. All rights reserved
*	Contact to : www.rainyrizzle.com , contactrainyrizzle@gmail.com
*
*	This file is part of [AnyPortrait].
*
*	AnyPortrait can not be copied and/or distributed without
*	the express permission of [Seungjik Lee] of [RainyRizzle team].
*
*	It is illegal to download files from other than the Unity Asset Store and RainyRizzle homepage.
*	In that case, the act could be subject to legal sanctions.
*/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Text;
using System.Collections.Generic;
using AnyPortrait;

namespace AnyPortrait
{
	//업데이트 로그를 출력하고 알려준다.
	public class apDialog_UpdateLog : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		private static apDialog_UpdateLog s_window = null;

		private apEditor.LANGUAGE _language = apEditor.LANGUAGE.English;
		//내용은 그냥 코드로 적자

		private Vector2 _scroll = Vector2.zero;
		private string _info = "";
		private int _nUpdateImages = 0;
		private List<Texture> _updateImages = new List<Texture>();
		

		private string _str_GotoHomepage = "";
		private string _str_OpenAssetStore = "";
		private string _str_Close = "";

		private string _str_Next = "";

		private GUIStyle _guiStyle_Text = null;
		private GUIStyle _guiStyle_TextCenter = null;
		private int _iPage = 0;
		private int _nPages = 0;

		private const int WIDTH_WINDOW = 800;
		private const int HEIGHT_WINDOW_LOG = 700;
		private const int HEIGHT_WINDOW_IMAGE = 480;


		// Show Window
		//------------------------------------------------------------------
		[MenuItem("Window/AnyPortrait/Update Log", false, 801)]
		public static void ShowDialog()
		{
			ShowDialog(null);
		}
		
		public static void ShowDialog(apEditor editor)
		{
			
			CloseDialog();

			
			
			
			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_UpdateLog), true, "Update Log", true);
			apDialog_UpdateLog curTool = curWindow as apDialog_UpdateLog;

			//object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = WIDTH_WINDOW;
				int height = HEIGHT_WINDOW_IMAGE;
				s_window = curTool;
				if (editor != null)
				{
					s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
													(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
													width, height);
				}
				else
				{
					s_window.position = new Rect(50, 50,
												width, height);
				}
				s_window.Init();
			}
		}

		public static void CloseDialog()
		{
			if (s_window != null)
			{
				try
				{
					s_window.Close();
				}
				catch (Exception ex)
				{
					Debug.LogError("Close Exception : " + ex);
				}
				s_window = null;
			}
		}

		// Init
		//------------------------------------------------------------------
		public void Init()
		{
			_language = (apEditor.LANGUAGE)EditorPrefs.GetInt("AnyPortrait_Language", (int)apEditor.LANGUAGE.English);

			_guiStyle_Text = null;
			_guiStyle_TextCenter = null;

			GetText();
		}

		// GUI
		//------------------------------------------------------------------
		void OnGUI()
		{
			if(_iPage >= _nPages - 1)
			{
				//업데이트 로그 설명 페이지인 경우
				position = new Rect(position.x, position.y, WIDTH_WINDOW, HEIGHT_WINDOW_LOG);
				
			}
			else
			{
				//이미지 페이지인 경우
				position = new Rect(position.x, position.y, WIDTH_WINDOW, HEIGHT_WINDOW_IMAGE);

			}

			int width = (int)position.width;
			int height = (int)position.height;
			width -= 10;

			if(_guiStyle_TextCenter == null)
			{
				_guiStyle_TextCenter = new GUIStyle(GUI.skin.label);
				_guiStyle_TextCenter.alignment = TextAnchor.MiddleCenter;
			}

			
			if(_iPage >= _nPages - 1)
			{
				//업데이트 로그 설명 페이지인 경우
				DrawLogPage(width, height);
				
			}
			else
			{
				//이미지 페이지인 경우
				DrawImagePage(_iPage, width, height);
			}
		}

		private void DrawImagePage(int iImage, int width, int height)
		{	
			int imageWidth = width;
			int imageHeight = imageWidth / 2;//2:1 비율임

			GUILayout.Space(10);

			EditorGUILayout.LabelField(new GUIContent(_updateImages[iImage]), _guiStyle_TextCenter, GUILayout.Width(imageWidth), GUILayout.Height(imageHeight));

			EditorGUILayout.LabelField(GetPageText(_iPage, _nPages), _guiStyle_TextCenter, GUILayout.Width(width));
			
			GUILayout.Space(10);

			if(GUILayout.Button(_str_Next, GUILayout.Height(30)))
			{
				_iPage++;
			}

		}

		private void DrawLogPage(int width, int height)
		{
			if (_guiStyle_Text == null)
			{
				_guiStyle_Text = new GUIStyle(GUI.skin.label);
				_guiStyle_Text.richText = true;
				_guiStyle_Text.wordWrap = true;
				_guiStyle_Text.alignment = TextAnchor.UpperLeft;
			}

			int height_Log = height - 90;
			EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(height));
			_scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Width(width + 10), GUILayout.Height(height_Log));
			EditorGUILayout.BeginVertical(GUILayout.Width(width - 15));
			
			
			EditorGUILayout.TextArea(_info, _guiStyle_Text, GUILayout.Width(width - 25));
			GUILayout.Space(height + 100);
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();


			bool isClose = false;
			
			if(GUILayout.Button(_str_GotoHomepage, GUILayout.Height(25)))
			{
				//홈페이지를 엽시다.
				if(_language == apEditor.LANGUAGE.Korean)
				{
					//Application.OpenURL("https://www.rainyrizzle.com/anyportrait-updatenote-kor");
					Application.OpenURL("https://www.rainyrizzle.com/ap-updatenotelist-kor");
				}
				else if(_language == apEditor.LANGUAGE.Japanese)
				{
					//Application.OpenURL("https://www.rainyrizzle.com/anyportrait-updatenote-jp");
					Application.OpenURL("https://www.rainyrizzle.com/ap-updatenotelist-jp");
				}
				else
				{
					//Application.OpenURL("https://www.rainyrizzle.com/anyportrait-updatenote-eng");
					Application.OpenURL("https://www.rainyrizzle.com/ap-updatenotelist-eng");
				}
				isClose = true;
			}
			if(GUILayout.Button(_str_OpenAssetStore, GUILayout.Height(25)))
			{
				//에셋 스토어 페이지를 엽니다.
				apEditorUtil.OpenAssetStorePage();
				isClose = true;
			}
			if(GUILayout.Button(_str_Close, GUILayout.Height(25)))
			{
				isClose = true;
			}

			EditorGUILayout.EndVertical();

			if (isClose)
			{
				CloseDialog();
			}
		}


		private string GetPageText(int iPage, int nPage)
		{
			string resultText = "";
			for (int i = 0; i < nPage; i++)
			{
				if(i == iPage)
				{
					resultText += "●";
				}
				else
				{
					resultText += "○";
				}
			}
			return resultText;
		}

		private void GetText()
		{
			_info = "";
			_nUpdateImages = 0;
			if(_updateImages == null)
			{
				_updateImages = new List<Texture>();
			}
			_updateImages.Clear();

			//TextAsset textAsset_Dialog = AssetDatabase.LoadAssetAtPath<TextAsset>(apEditorUtil.ResourcePath_Text + "apUpdateLog.txt");
			TextAsset textAsset_Dialog = AssetDatabase.LoadAssetAtPath<TextAsset>(apEditorUtil.MakePath_Text("apUpdateLog.txt"));
			string strDelimeter = "-----------------------------------------";
			string[] strInfoPerLanguages = textAsset_Dialog.text.Split(new string[] { strDelimeter }, StringSplitOptions.None);

			if (strInfoPerLanguages.Length < (int)apEditor.LANGUAGE.Polish + 1)
			{
				//개수가 부족한데염
				Debug.Log("UpdateLog 개수 부족");
				return;
			}

			//첫줄은 이미지 개수
			string strUpdateImages = strInfoPerLanguages[0];
			strUpdateImages = strUpdateImages.Replace("\r\n", "\n");
			string[] strUpdateImages_Arr = strUpdateImages.Split(new string[] {"\n"}, StringSplitOptions.None);
			try
			{
				if(strUpdateImages_Arr.Length >= 2)
				{
					_nUpdateImages = int.Parse(strUpdateImages_Arr[1]);

					if(_nUpdateImages > 0)
					{
						//이미지를 가져오자
						for (int i = 0; i < _nUpdateImages; i++)
						{
							Texture updateImage = AssetDatabase.LoadAssetAtPath<Texture>(apEditorUtil.MakePath_Icon("Update/UpdateKeyChanges_" + (i + 1), false));
							if(updateImage != null)
							{
								_updateImages.Add(updateImage);
							}
						}

						_nUpdateImages = _updateImages.Count;//유효한 이미지 개수만 받자
						
					}
				}
				
			}
			catch(Exception)
			{

			}
			

			_iPage = 0;
			_nPages = _nUpdateImages + 1;

			//본문
			_info = strInfoPerLanguages[(int)_language + 1];
			_info = _info.Replace("\r\n", "\n");

			//만약 일본어라면
			//이건 전각이라서 한 줄에 전각 68글자 (반각 포함해도 넉넉히 60글자를 넘어가면) 무조건 한줄 내려야 한다.
			//자체적으로 Wrapping을 하자
			if (_language == apEditor.LANGUAGE.Japanese)
			{
				string[] strJPLines = _info.Split(new char[] {'\n'}, StringSplitOptions.None);
				int nStrJPLines = strJPLines != null ? strJPLines.Length : 0;
				
				List<string> resultJPLines = new List<string>();

				for (int iJPLine = 0; iJPLine < nStrJPLines; iJPLine++)
				{
					string curJPLine = strJPLines[iJPLine];

					//첫 두줄(타이틀/날짜)은 글자 개수를 생각하지 않는다.
					if(iJPLine < 2)
					{
						resultJPLines.Add(curJPLine);
						continue;
					}

					int iNextSplit = -1;
					bool isLengthOver = IsJPTextOverLength(ref curJPLine, 60, ref iNextSplit);
					
					if(!isLengthOver)
					{
						//문장이 짧다. 괜춘함
						resultJPLines.Add(curJPLine);
					}
					else
					{
						//문장이 너무 길다.
						//60줄이 넘어가면, 반복적으로 앞에서 끊는다.
						//단, 분할된 두번째 줄부터는 앞에 띄어쓰기 두개를 붙이자

						int cntSplit = 0;
						string curSplitTargetText = curJPLine;
						while(true)
						{
							if(cntSplit > 0)
							{
								curSplitTargetText = "    " + curSplitTargetText;
							}

							iNextSplit = -1;
							isLengthOver = IsJPTextOverLength(ref curSplitTargetText, 60, ref iNextSplit);
							if(!isLengthOver || iNextSplit < 0)
							{
								//60보다 짧다
								//더이상 분할하지 않고 종료
								resultJPLines.Add(curSplitTargetText);
								break;
							}

							//앞에서 60줄만 넣고, 나머지는 분리한다.
							resultJPLines.Add(curSplitTargetText.Substring(0, iNextSplit));
							curSplitTargetText = curSplitTargetText.Substring(iNextSplit);

							cntSplit += 1;
						}
					}
				}

				//결과값을 넣자
				int nNewJPLines = resultJPLines.Count;
				_info = "";
				for (int iJPResult = 0; iJPResult < nNewJPLines; iJPResult++)
				{
					_info += resultJPLines[iJPResult] + "\n";
				}
			}


			//디버그 경고 (혹시 모르니)
			if(_info.Contains("\t"))
			{
				Debug.LogError("Update Log 텍스트 확인 필요 [Tab] [" + _language + "]");
				int iText = _info.IndexOf("\t");
				if(iText >= 0 && iText < _info.Length)
				{
					int iStart = Mathf.Max(iText - 10, 0);
					int iEnd = Mathf.Min(iText + 10, _info.Length - 1);
					Debug.LogError("[" + _info.Substring(iStart, iEnd - iStart + 1) + "]");
				}
			}

			if(_info.Contains("<color = blue>"))
			{
				Debug.LogError("Update Log 텍스트 확인 필요 [Color=Blue] [" + _language + "]");
			}

			if(_info.Contains("<color = red>"))
			{
				Debug.LogError("Update Log 텍스트 확인 필요 [Color=Red] [" + _language + "]");
			}

			if(_info.Contains("</ color>"))
			{
				Debug.LogError("Update Log 텍스트 확인 필요 [/Color] [" + _language + "]");
			}

			_info = _info.Replace("\t", "    ");
			_info = _info.Replace("<color = blue>", "<color=blue>");
			_info = _info.Replace("<color = red>", "<color=red>");
			_info = _info.Replace("</ color>", "</color>");
			
			
			if(EditorGUIUtility.isProSkin)
			{
				_info = _info.Replace("<color=blue>", "<color=yellow>");
				_info = _info.Replace("<color=red>", "<color=lime>");
			}
			if (_info.Length > 0)
			{
				if (_info.Substring(0, 1) == "\n")
				{
					_info = _info.Substring(1);
				}
			}

			//첫줄을 삭제한다. (언어 이름이 써있다.)
			int firstCR = _info.IndexOf("\n");
			_info = _info.Substring(firstCR);

			
			//_str_Next = "▶";			
			switch (_language)
			{
				case apEditor.LANGUAGE.English:
					_str_GotoHomepage = "Go to Homepage";
					_str_OpenAssetStore = "Open Asset Store";
					_str_Close = "Close";
					_str_Next = "Next";
					break;

				case apEditor.LANGUAGE.Korean:
					_str_GotoHomepage = "홈페이지로 가기";
					_str_OpenAssetStore = "에셋 스토어 열기";
					_str_Close = "닫기";
					_str_Next = "다음";
					break;

				case apEditor.LANGUAGE.French:
					_str_GotoHomepage = "Aller à la page d'accueil";
					_str_OpenAssetStore = "Ouvrir Asset Store";
					_str_Close = "Fermer";
					_str_Next = "Suivant";
					break;

				case apEditor.LANGUAGE.German:
					_str_GotoHomepage = "Gehe zur Startseite";
					_str_OpenAssetStore = "Asset Store öffnen";
					_str_Close = "Schließen";
					_str_Next = "Nächste Seite";
					break;

				case apEditor.LANGUAGE.Spanish:
					_str_GotoHomepage = "Ir a la página de inicio";
					_str_OpenAssetStore = "Abrir Asset Store";
					_str_Close = "Cerca";
					_str_Next = "Siguiente";
					break;

				case apEditor.LANGUAGE.Italian:
					_str_GotoHomepage = "Vai alla pagina principale";
					_str_OpenAssetStore = "Apri Asset Store";
					_str_Close = "Vicino";
					_str_Next = "Successivo";
					break;

				case apEditor.LANGUAGE.Danish:
					_str_GotoHomepage = "Gå til Hjemmeside";
					_str_OpenAssetStore = "Åbn Asset Store";
					_str_Close = "Tæt";
					_str_Next = "Næste";
					break;

				case apEditor.LANGUAGE.Japanese:
					_str_GotoHomepage = "ホームページへ";
					_str_OpenAssetStore = "アセットストアを開く";
					_str_Close = "閉じる";
					_str_Next = "次のページ";
					break;

				case apEditor.LANGUAGE.Chinese_Traditional:
					_str_GotoHomepage = "去首頁";
					_str_OpenAssetStore = "打開[資產商店]";
					_str_Close = "關";
					_str_Next = "下一頁";
					break;

				case apEditor.LANGUAGE.Chinese_Simplified:
					_str_GotoHomepage = "去首页";
					_str_OpenAssetStore = "打开[资产商店]";
					_str_Close = "关";
					_str_Next = "下一页";
					break;

				case apEditor.LANGUAGE.Polish:
					_str_GotoHomepage = "Wróć do strony głównej";
					_str_OpenAssetStore = "Otwórz Asset Store";
					_str_Close = "Blisko";
					_str_Next = "Następna";
					break;
			}

			
		}



		//일본어의 경우, 전각이 많아서 Wrapping이 제대로 되지 않는다.
		//글자 길이를 리턴한다. (영어, 문장 부호의 경우 0.6으로 계산)
		private bool IsJPTextOverLength(ref string srcText, int splitLength, ref int nextSplitIndex)
		{
			nextSplitIndex = -1;
			int nText = srcText.Length;
			if(nText == 0)
			{
				return false;
			}

			float result = 0.0f;
			char curWord = ' ';
			bool isAnyOver = false;

			for (int i = 0; i < nText; i++)
			{
				curWord = srcText[i];

				switch (curWord)
				{
					case 'a':	case 'b':	case 'c':	case 'd':	case 'e':
					case 'f':	case 'g':	case 'h':	case 'i':	case 'j':
					case 'k':	case 'l':	case 'm':	case 'n':	case 'o':
					case 'p':	case 'q':	case 'r':	case 's':	case 't':
					case 'u':	case 'v':	case 'w':	case 'y':	case 'x':
					case 'z':

					case 'A':	case 'B':	case 'C':	case 'D':	case 'E':
					case 'F':	case 'G':	case 'H':	case 'I':	case 'J':
					case 'K':	case 'L':	case 'M':	case 'N':	case 'O':
					case 'P':	case 'Q':	case 'R':	case 'S':	case 'T':
					case 'U':	case 'V':	case 'W':	case 'Y':	case 'X':
					case 'Z':

					case '0':	case '1':	case '2':	case '3':	case '4':
					case '5':	case '6':	case '7':	case '8':	case '9':

					case '(':	case ')':	case '<':	case '>':	case '=':
					case ',':	case '.':	case ' ':	case '-':	case '_':
					case ':':	case ';':	case '?':	case '!':	case '*':
					case '/':	case '[':	case ']':	case '\'':	case '\"':
					case '~':	case '+':
						result += 0.6f;
						break;

					default:
						result += 1.0f;
						break;
				}

				//이 쯤에서 목표 텍스트를 초과했는지 확인
				if (!isAnyOver)
				{
					if ((int)(result + 0.5f) > splitLength)
					{
						nextSplitIndex = i;
						isAnyOver = true;
					}
				}
			}

			if (nextSplitIndex < nText - 1)
			{
				return isAnyOver;
			}
			else
			{
				return false;//Split이 마지막에 발생하면 무시
			}

		}
	}
}
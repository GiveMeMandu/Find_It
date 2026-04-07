
using System;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace Manager
{
    public partial class UIManager
    {
        public Canvas uiCanvas;
        public const string PAGE_ROOT_PATH = "Prefabs/UI/Page/";

        [SerializeField]
        private CanvasGroup _pageGroup = null;
        
        [SerializeField] private GameObject _toastObject;
        
        // Stack-based page tracking
        private Stack<PageViewModel> _pageStack = new Stack<PageViewModel>();
        
        public CanvasGroup PageGroup => _pageGroup;
        public int PageCount => _pageStack.Count;
        
        public PageViewModel CurrentPage => _pageStack.Count > 0 ? _pageStack.Peek() : null;
        public EventHandler<PageViewModel> OnClosePage;
        
        public bool EscapePressedThisFrame = false;
        public bool IgnoreEscapeThisFrame = false;

        private void Update()
        {
            if (_toastObject != null && _toastObject.transform.parent != null && _toastObject.transform.parent.childCount > 1)
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(_toastObject.transform.parent as RectTransform);
            }

            if (Keyboard.current != null)
            {
                EscapePressedThisFrame |= Keyboard.current.escapeKey.wasPressedThisFrame;
            }
            if (Gamepad.current != null)
            {
                EscapePressedThisFrame |= Gamepad.current.buttonEast.wasPressedThisFrame;
            }

            if (IgnoreEscapeThisFrame)
            {
                EscapePressedThisFrame = false;
            }

            if (EscapePressedThisFrame)
            {
                // 로딩 씬에서는 옵션창 등 ESC 동작을 막습니다.
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex == (int)Data.SceneNum.LOADING || UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex == (int)Data.SceneNum.BOOTSTRAP)
                {
                    IgnoreEscapeThisFrame = false;
                    EscapePressedThisFrame = false;
                    return;
                }

                if (_pageStack.Count > 0)
                {
                    if (!CurrentPage.BlockEscape)
                    {
                        CurrentPage.OnEscapePressed();
                    }
                }
                else
                {
                    OpenPage<OptionPage>();
                }
            }

            IgnoreEscapeThisFrame = false;
            EscapePressedThisFrame = false;
        }

        public PageViewModel GetCurrentPage()
        {
            return CurrentPage;
        }
        
        public List<T> GetPages<T>() where T : PageViewModel
        {
            List<T> pages = new List<T>();
            foreach (var page in _pageStack)
            {
                if (page is T tPage)
                {
                    pages.Add(tPage);
                }
            }
            return pages;
        }
        
        public T OpenPage<T>(params object[] parameters) where T : PageViewModel
        {
            // 애니메이션 초기화 방지를 위해 SetActive를 끄지 않습니다.
            // if (_pageStack.Count > 0)
            // {
            //     CurrentPage.gameObject.SetActive(false);
            // }
            
            T pagePrefab = FindPage<T>();
            T pageInstance = Instantiate(pagePrefab, _pageGroup.transform);
            
            _pageStack.Push(pageInstance);
            pageInstance.Init(parameters);
            pageInstance.Focus();
            
            // If your PageViewModel has an Init or Focus method, call it here
            // pageInstance.Init();
            // pageInstance.Focus();
            
            return pageInstance;
        }

        private T FindPage<T>() where T : PageViewModel
        {
            T page = Resources.Load<T>($"{PAGE_ROOT_PATH}{typeof(T).Name}");
            if (page == null)
            {
                Debug.LogError($"[UIManager] Page not found: {typeof(T).Name}");
                return null;
            }
            return page;
        }

        public void ClosePage()
        {
            CloseCurrentPage(false);
        }

        public void CloseCurrentPage(bool skipFade = false)
        {
            if (_pageStack.Count <= 0)
            {
                Debug.LogWarning("No page to close");
                return;
            }

            var page = _pageStack.Pop();
            
            // If your PageViewModel has an OnClose method, call it here
            page.OnClose();
            
            OnClosePage?.Invoke(this, page);

            if (page != null && page.gameObject != null)
            {
                if (skipFade)
                {
                    Destroy(page.gameObject);
                }
                else
                {
                    // You can replace this with DOTween Fade logic as in the provided script
                    Destroy(page.gameObject); 
                }
            }

            if (_pageStack.Count > 0)
            {
                // CurrentPage.gameObject.SetActive(true);
                // CurrentPage.Focus();
            }
        }

        public void ClosePage(PageViewModel pageToClose)
        {
            if (pageToClose == null) return;
            
            // If it's the top page, use normal close
            if (_pageStack.Count > 0 && _pageStack.Peek() == pageToClose)
            {
                CloseCurrentPage(true);
                return;
            }

            // Remove specific item from stack by rebuilding it
            var tempStack = new Stack<PageViewModel>();
            while (_pageStack.Count > 0)
            {
                var popPage = _pageStack.Pop();
                if (popPage == pageToClose)
                {
                    Destroy(popPage.gameObject);
                    break;
                }
                tempStack.Push(popPage);
            }
            
            while (tempStack.Count > 0)
            {
                _pageStack.Push(tempStack.Pop());
            }
        }
        
        public void CloseAllPages()
        {
            while(_pageStack.Count > 0)
            {
                CloseCurrentPage(true);
            }
        }
        
        public bool IsPageOpen<T>() where T : PageViewModel
        {
            foreach (var page in _pageStack)
            {
                if (page is T)
                {
                    return true;
                }
            }
            return false;
        }

        public void AddToast(string message)
        {
            if (_toastObject == null) return;
            
            var toast = Instantiate(_toastObject, _toastObject.transform.parent);
            toast.SetActive(true);
            var text = toast.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (text != null)
            {
                text.text = message;
            }
            DelayDestroy(toast).Forget();
        }

        private async Cysharp.Threading.Tasks.UniTaskVoid DelayDestroy(GameObject toast)
        {
            await Cysharp.Threading.Tasks.UniTask.Delay(2700);
            if (toast != null)
            {
                Destroy(toast);
            }
        }
    }
}
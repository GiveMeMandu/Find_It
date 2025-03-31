
using System.Collections.Generic;
using UI;
using UnityEngine;

namespace Manager
{
    public partial class UIManager
    {
        public Canvas uiCanvas;
        public const string PAGE_ROOT_PATH = "Prefabs/UI/Page/";

        [SerializeField]
        private CanvasGroup _pageGroup = null;
        private List<PageViewModel> _uiPageList = new ();
        
        public CanvasGroup PageGroup => _pageGroup;
        public int PageCount => _uiPageList.Count;
        
        public PageViewModel GetCurrentPage()
        {
            if (_uiPageList.Count == 0)
            {
                return null;
            }

            return _uiPageList[_uiPageList.Count - 1];
        }
        
        public List<T> GetPages<T>() where T : PageViewModel
        {
            List<T> pages = new List<T>();
            foreach (var page in _uiPageList)
            {
                if (page is T)
                {
                    pages.Add(page as T);
                }
            }
            return pages;
        }
        
        public T OpenPage<T>() where T : PageViewModel
        {
            T page = FindPage<T>();
            page = Instantiate(page, _pageGroup.transform);
            _uiPageList.Add(page);
            return page;
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
            PageViewModel page = _uiPageList[_uiPageList.Count - 1];
            _uiPageList.Remove(page);
            Destroy(page.gameObject);
        }
        public void ClosePage(PageViewModel page)
        {
            if(_uiPageList.Contains(page))
            {
                _uiPageList.Remove(page);
                Destroy(page.gameObject);
            }
        }
        
        public void CloseAllPages()
        {
            while(_uiPageList.Count > 0)
            {
                ClosePage();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class TabGroup : MonoBehaviour
    {
        [Header("Settings")]
        public TabButton tabButtonPrefab;
        public Transform tabRoot;
        public Image icon;

        private List<TabButton> _tabs = new List<TabButton>();
        private int _selectedIndex = -1;

        private void Awake()
        {
            if (tabButtonPrefab != null)
                tabButtonPrefab.gameObject.SetActive(false);
        }

        public void Clear()
        {
            foreach (var tab in _tabs)
            {
                if (tab != null)
                    Destroy(tab.gameObject);
            }
            _tabs.Clear();
            _selectedIndex = -1;
        }

        public void AddTab(string labelTerm, Action onSelect, Sprite sprite = null)
        {
            if (tabButtonPrefab == null || tabRoot == null)
                return;

            var tabButton = Instantiate(tabButtonPrefab, tabRoot);
            tabButton.gameObject.SetActive(true);

            int index = _tabs.Count;
            tabButton.Init(labelTerm, () => SelectTab(index, onSelect), sprite);
            _tabs.Add(tabButton);
        }

        public void SelectTab(int index)
        {
            if (index < 0 || index >= _tabs.Count) return;
            
            // Trigger the click action of the button to ensure the callback is fired
            // But wait, the callback is inside the closure in AddTab.
            // We need to invoke the callback associated with the tab.
            // However, AddTab stores the callback in the closure.
            // So we can't easily invoke it from outside unless we store it.
            // Let's modify AddTab to store callbacks or just rely on the button click.
            // If we want to select programmatically, we might need to simulate a click or store the action.
            
            // For now, let's just update the visual state. 
            // If we need to trigger the logic, we should probably store the actions.
            // But usually SelectTab(index) is called to *set* the state, and the caller might want to trigger the logic too.
            // Let's assume the caller handles the logic if they call SelectTab directly, OR we simulate the click.
            // Simulating click: _tabs[index].button.onClick.Invoke();
            
            if (_tabs[index].button != null)
                _tabs[index].button.onClick.Invoke();
        }

        private void SelectTab(int index, Action onSelect)
        {
            if (_selectedIndex == index) return;

            _selectedIndex = index;
            onSelect?.Invoke();
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            for (int i = 0; i < _tabs.Count; i++)
            {
                _tabs[i].SetState(i == _selectedIndex, i < _selectedIndex);
            }

            if (_selectedIndex >= 0 && _selectedIndex < _tabs.Count)
            {
                if (_tabs[_selectedIndex].isSetSiblingIndex)
                {
                    // 선택된 탭을 중심으로 계단식으로 아래로 깔리도록 렌더링 순서(Z인덱스) 재배치
                    
                    // 1. 선택된 탭의 왼쪽 탭들은 왼쪽부터 차례대로 올림
                    for (int i = 0; i < _selectedIndex; i++)
                    {
                        _tabs[i].transform.SetAsLastSibling();
                    }

                    // 2. 선택된 탭의 오른쪽 탭들은 오른쪽 끝에서부터 차례대로 올림 (역순)
                    for (int i = _tabs.Count - 1; i > _selectedIndex; i--)
                    {
                        _tabs[i].transform.SetAsLastSibling();
                    }

                    // 3. 마지막으로 선택된 탭을 가장 마지막(맨 위)으로 올림
                    _tabs[_selectedIndex].transform.SetAsLastSibling();
                }
            }
        }
    }
}

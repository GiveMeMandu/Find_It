using UnityEngine;
using UnityEngine.InputSystem;
using OptionPageNamespace;
using Manager;

namespace UI
{
    public partial class OptionPage
    {
        private void CreateControlsOptionGroup()
        {
            var gameInput = Global.InputManager;

            // 키보드 설정 그룹
            var keyboardGroup = CreateOptionGroup("UI/Option/KeyboardControls");
            CreateBindingsForGroup(keyboardGroup, false);

            // 게임패드 설정 그룹
            var gamepadGroup = CreateOptionGroup("UI/Option/GamepadControls");
            CreateBindingsForGroup(gamepadGroup, true);

            void CreateBindingsForGroup(OptionGroup group, bool isGamepad)
            {
                void CreateBinding(string label, UnityEngine.InputSystem.InputAction action, int bindingIndex = -1, string customLabel = null)
                {
                    var binding = group.CreateOptionGamepadBinding();

                    string GetBindingText()
                    {
                        if (bindingIndex != -1)
                        {
                            return InputBindingUtility.GetInputText(action, bindingIndex, isGamepad);
                        }
                        return InputBindingUtility.GetInputText(action, isGamepad);
                    }

                    binding.Init(label, GetBindingText(), () =>
                    {
                        if (isGamepad && UnityEngine.InputSystem.Gamepad.current == null)
                        {
                            _uiManager.AddToast("게임패드를 연결해주세요");
                            return;
                        }

                        var eventSystem = UnityEngine.EventSystems.EventSystem.current;
                        eventSystem.enabled = false;
                        _uiManager.AddToast("매핑할 키를 입력해주세요");
                        binding.UpdateKeyText("...");

                        action.Disable();
                        var rebindOperation = action.PerformInteractiveRebinding(bindingIndex != -1 ? bindingIndex : (isGamepad ? 0 : 1))
                            .OnComplete(operation =>
                            {
                                gameInput.SaveBindings();
                                binding.UpdateKeyText(GetBindingText());
                                action.Enable();
                                eventSystem.enabled = true;
                                operation.Dispose();
                            })
                            .OnCancel(operation =>
                            {
                                binding.UpdateKeyText(GetBindingText());
                                action.Enable();
                                eventSystem.enabled = true;
                                operation.Dispose();
                            });
                        
                        // 키보드 Move 바인딩의 경우 WASD 키만 허용하도록 필터링할 수도 있지만, 
                        // 사용자가 자유롭게 바꿀 수 있게 두는 것이 일반적임.
                        // 다만 Composite Part 바인딩 시에는 예상치 못한 키가 들어가지 않도록 주의 필요.
                        
                        rebindOperation.Start();
                    });

                    if (customLabel != null)
                    {
                        if (binding.labelText != null)
                        {
                            binding.labelText.enabled = false;
                            var tmp = binding.labelText.GetComponent<TMPro.TextMeshProUGUI>();
                            if (tmp != null)
                            {
                                tmp.text = customLabel;
                            }
                        }
                    }
                }

                if (!isGamepad && gameInput.MoveInputAction != null)
                {
                    // 키보드 Move는 4방향 분리 (Up, Left, Down, Right)
                    int GetBindingIndex(string name)
                    {
                        var action = gameInput.MoveInputAction;
                        for (int i = 0; i < action.bindings.Count; i++)
                        {
                            var b = action.bindings[i];
                            if (b.isPartOfComposite && b.name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
                                return i;
                        }
                        return -1;
                    }

                    CreateBinding("UI/Option/MoveUp", gameInput.MoveInputAction, GetBindingIndex("up"));
                    CreateBinding("UI/Option/MoveLeft", gameInput.MoveInputAction, GetBindingIndex("left"));
                    CreateBinding("UI/Option/MoveDown", gameInput.MoveInputAction, GetBindingIndex("down"));
                    CreateBinding("UI/Option/MoveRight", gameInput.MoveInputAction, GetBindingIndex("right"));
                }
                else
                {
                    CreateBinding("UI/Option/Move", gameInput.MoveInputAction);
                }


                // 초기화 버튼 추가
                var resetButton = group.CreateOptionButton();
                resetButton.Init("UI/Option/Reset", () =>
                {
                    gameInput.ResetBindings(isGamepad);
                    OpenTab(TabType.Controls);
                });
            }
        }
    }
}

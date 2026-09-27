using System.Linq;
using UnityEngine;

namespace MalumMenu;

public class TasksUI : MonoBehaviour
{
    public static int windowHeight = 300;                               // Высота окна
    public static int windowWidth = 500;                                // Ширина окна
    public static Rect windowRect;                                      // Прямоугольник окна

    private Vector2 _scrollPosition = Vector2.zero;                     // Позиция прокрутки
    private GUIStyle _playerHeaderStyle;                                // Стиль заголовка игрока
    private Il2CppSystem.Text.StringBuilder _tasksString = new();       // Строковый буфер для текста задач
    private readonly System.Collections.Generic.Dictionary<string, bool> _expandedPlayers = new();  // Словарь развёрнутых игроков

    private void Start()
    {
        // Создаём 2D-область TasksUI
        windowRect = new(
            Screen.width / 2f - windowWidth / 2f,
            Screen.height / 2f - windowHeight / 2f,
            windowWidth,
            windowHeight
        );
    }

    private void OnGUI()
    {
        if (!CheatToggles.showTasksMenu || !(MenuUI.isGUIActive || MalumMenu.menuKeepSubwindowsOpen.Value) || MalumMenu.isPanicked) return;

        _playerHeaderStyle ??= new GUIStyle(GUI.skin.button)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleLeft     // Выравнивание текста по левому краю
        };

        UIHelpers.ApplyUIColor();                 // Применить пользовательский цвет UI

        windowRect = GUI.Window((int)WindowId.TasksUI, windowRect, (GUI.WindowFunction)TasksWindow, "Задачи");  // Заголовок окна: "Задачи"
    }

    private void TasksWindow(int windowID)
    {
        GUILayout.BeginVertical();

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);  // Область прокрутки

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (!player.Data || !player.Data.Role || string.IsNullOrEmpty(player.Data.PlayerName)) continue;

            GUILayout.BeginVertical();

            var nameKey = player.Data.PlayerName;
            _expandedPlayers.TryGetValue(nameKey, out var expanded);
            var arrow = expanded ? "\u25BC" : "\u25B6"; // ▼ или ▶

            var taskCount = player.myTasks.Count;                                        // Общее число задач
            var completeCount = player.myTasks.ToArray().Count(t => t.IsComplete);       // Число выполненных задач

            // Корректировки отображения задач для локального игрока
            if (player == PlayerControl.LocalPlayer && player.Data.IsDead)
            {
                taskCount -= 1;     // Убрать задачу "Вы мертвы"
            }
            if (player == PlayerControl.LocalPlayer && Utils.isAnySabotageActive)
            {
                taskCount -= 1;     // Убрать задачу саботажа
            }
            if (player == PlayerControl.LocalPlayer && player.Data.Role.IsImpostor)
            {
                taskCount -= 1;     // Убрать задачу предателя
            }

            // Кнопка-заголовок игрока: "▶ [выполнено/всего] ИмяИгрока"
            if (GUILayout.Button($"{arrow} [{completeCount}/{taskCount}] <color=#{ColorUtility.ToHtmlStringRGB(player.Data.Color)}>{nameKey}</color>", _playerHeaderStyle))
            {
                _expandedPlayers[nameKey] = !expanded;
                expanded = !expanded;
            }

            if (expanded)
            {
                GUILayout.BeginHorizontal();

                GUILayout.BeginVertical();

                foreach (var task in player.myTasks)
                {
                    // Пропускаем саботажные задачи: реактор, кислород, свет, связь, сейсмика, Чарльз, грибной саботаж
                    if (task.TaskType is TaskTypes.ResetReactor or TaskTypes.RestoreOxy or TaskTypes.FixLights or TaskTypes.FixComms or TaskTypes.ResetSeismic or TaskTypes.StopCharles or TaskTypes.MushroomMixupSabotage) continue;

                    _tasksString.Clear();
                    task.AppendTaskText(_tasksString);   // Получить текст задачи
                    //_tasksString.Append($"Тип задачи: {task.TaskType.ToString()}");
                    var taskText = _tasksString.ToString();

                    // Пропускаем тексты "You're dead" и "Sabotage and kill"
                    if (taskText.Contains("You're dead") || taskText.Contains("Sabotage and kill")) continue;

                    GUILayout.BeginHorizontal();
                    // Очищаем текст задачи от переносов строк и цветовых тегов
                    GUILayout.Label(taskText.Replace("\n", "").Replace("</color>", "").Replace("<color=#00DD00FF>", "").Replace("<color=#FFFF00FF>", ""));
                    GUILayout.FlexibleSpace();

                    if (task.IsComplete)     // Задача выполнена
                    {
                        GUILayout.Label("<color=#00ff00>✔ Выполнено</color>");  // ✔ Выполнено
                    }
                    else
                    {
                        if (player == PlayerControl.LocalPlayer)     // Только для локального игрока — кнопка выполнения
                        {
                            if (GUILayout.Button("Выполнить", GUIStylePreset.NormalButton))  // Кнопка "Выполнить"
                            {
                                Utils.CompleteTask(task);
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        GUILayout.EndScrollView();

        // Кнопка "Выполнить мои задачи"
        if (GUILayout.Button("Выполнить мои задачи", GUIStylePreset.NormalButton))
        {
            CheatToggles.completeMyTasks = true;
        }

        GUILayout.EndVertical();

        GUI.DragWindow();   // Позволяет перетаскивать окно
    }
}

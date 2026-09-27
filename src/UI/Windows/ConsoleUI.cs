using Il2CppSystem;
using UnityEngine;
using System.Collections.Generic;

namespace MalumMenu;

public class ConsoleUI : MonoBehaviour
{
    public static int windowHeight = 380;                               // Высота окна
    public static int windowWidth = 600;                                // Ширина окна
    public static Rect windowRect;                                      // Прямоугольник окна

    private GUIStyle _logStyle;                                         // Стиль лога
    private static Vector2 _scrollPosition = Vector2.zero;              // Позиция прокрутки
    private static List<string> _logEntries = new();                    // Записи лога
    private const int MaxLogEntries = 300;                              // Максимум записей в логе

    private void Start()
    {
        // Создаём 2D-область ConsoleUI
        windowRect = new(
            Screen.width / 2f - windowWidth / 2f,
            Screen.height / 2f - windowHeight / 2f,
            windowWidth,
            windowHeight
        );
    }

    private void OnGUI()
    {
        if (!CheatToggles.showConsole || !(MenuUI.isGUIActive || MalumMenu.menuKeepSubwindowsOpen.Value) || MalumMenu.isPanicked) return;

        _logStyle ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 15
        };

        UIHelpers.ApplyUIColor();       // Применить пользовательский цвет UI

        windowRect = GUI.Window((int)WindowId.ConsoleUI, windowRect, (GUI.WindowFunction)ConsoleWindow, "Консоль");  // Заголовок окна: "Консоль"
    }

    private void ConsoleWindow(int windowID)
    {
        GUILayout.BeginVertical(GUI.skin.box);

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false);   // Область прокрутки

        foreach (var log in _logEntries)        // Вывод всех записей лога
        {
            GUILayout.Label(log, _logStyle);
        }

        GUILayout.EndScrollView();

        GUILayout.EndVertical();

        GUILayout.BeginHorizontal();

        // Кнопка "Очистить лог"
        if (GUILayout.Button("Очистить лог", GUILayout.Width(285)))
        {
            _logEntries.Clear();
        }

        // Кнопка "Скопировать лог в буфер обмена"
        if (GUILayout.Button("Скопировать лог в буфер обмена"))
        {
            GUIUtility.systemCopyBuffer = String.Join("\n", _logEntries.ToArray());
        }

        GUILayout.EndHorizontal();

        GUI.DragWindow();       // Позволяет перетаскивать окно
    }

    public static void Log(string message)
    {
        if (_logEntries.Count >= MaxLogEntries) // Ограничиваем число логов, чтобы не жрало память
        {
            _logEntries.RemoveAt(0); // Удаляем самую старую запись
        }

        var currentTime = DateTime.Now.ToString("HH:mm:ss");   // Текущее время

        _logEntries.Add($"<b>[ {currentTime} ]  {message}</b>");

        // Автопрокрутка вниз
        _scrollPosition.y = float.MaxValue;
    }
}

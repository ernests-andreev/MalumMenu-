using UnityEngine;

namespace MalumMenu;

public class RolesUI : MonoBehaviour
{
    public static int windowHeight = 100;                               // Высота окна
    public static int windowWidth = 450;                                // Ширина окна
    public static Rect windowRect;                                      // Прямоугольник окна

    private Vector2 _scrollPosition = Vector2.zero;                     // Позиция прокрутки

    private void Start()
    {
        // Создаём 2D-область RolesUI
        windowRect = new(
            Screen.width / 2f - windowWidth / 2f,
            Screen.height / 2f - windowHeight / 2f,
            windowWidth,
            windowHeight
        );
    }

    private void OnGUI()
    {
        if (!CheatToggles.showRolesMenu || !(MenuUI.isGUIActive || MalumMenu.menuKeepSubwindowsOpen.Value) || MalumMenu.isPanicked) return;

        UIHelpers.ApplyUIColor();                 // Применить пользовательский цвет UI

        windowRect = GUI.Window((int)WindowId.RolesUI, windowRect, (GUI.WindowFunction)RolesWindow, "Назначить роли");  // Заголовок окна: "Назначить роли"
    }

    private void RolesWindow(int windowID)
    {
        GUILayout.BeginVertical();

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);   // Область прокрутки

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            // Показываем только локального игрока
            if (!player.Data || !player.Data.Role || string.IsNullOrEmpty(player.Data.PlayerName) || player != PlayerControl.LocalPlayer) continue;

            GUILayout.BeginHorizontal();

            // Цветное имя игрока
            GUILayout.Label($"<color=#{ColorUtility.ToHtmlStringRGB(player.Data.Color)}>{player.Data.PlayerName}</color>", GUILayout.Width(140f));
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{CheatToggles.forcedRole}");   // Текущая выбранная роль
            GUILayout.FlexibleSpace();

            // Кнопка сброса выбранной роли
            if (GUILayout.Button("Сброс", GUILayout.Width(80f)))
            {
                CheatToggles.forcedRole = null;
            }
            // Кнопка назначения роли
            if (GUILayout.Button("Назначить", GUILayout.Width(80f)))
            {
                CheatToggles.forceRole = true;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUILayout.Label("Роли будут назначены при следующем запуске игры");   // Подсказка внизу окна
        GUI.DragWindow();   // Позволяет перетаскивать окно
    }
}using UnityEngine;

namespace MalumMenu;

public class RolesUI : MonoBehaviour
{
    public static int windowHeight = 100;
    public static int windowWidth = 450;
    public static Rect windowRect;

    private Vector2 _scrollPosition = Vector2.zero;

    private void Start()
    {
        // Instantiate 2D area of RolesUI
        windowRect = new(
            Screen.width / 2f - windowWidth / 2f,
            Screen.height / 2f - windowHeight / 2f,
            windowWidth,
            windowHeight
        );
    }

    private void OnGUI()
    {
        if (!CheatToggles.showRolesMenu || !(MenuUI.isGUIActive || MalumMenu.menuKeepSubwindowsOpen.Value) || MalumMenu.isPanicked) return;

        UIHelpers.ApplyUIColor();

        windowRect = GUI.Window((int)WindowId.RolesUI, windowRect, (GUI.WindowFunction)RolesWindow, "Assign Roles");
    }

    private void RolesWindow(int windowID)
    {
        GUILayout.BeginVertical();

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (!player.Data || !player.Data.Role || string.IsNullOrEmpty(player.Data.PlayerName) || player != PlayerControl.LocalPlayer) continue;

            GUILayout.BeginHorizontal();

            GUILayout.Label($"<color=#{ColorUtility.ToHtmlStringRGB(player.Data.Color)}>{player.Data.PlayerName}</color>", GUILayout.Width(140f));
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{CheatToggles.forcedRole}");
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Reset", GUILayout.Width(80f)))
            {
                CheatToggles.forcedRole = null;
            }
            if (GUILayout.Button("Assign", GUILayout.Width(80f)))
            {
                CheatToggles.forceRole = true;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUILayout.Label("Roles will be assigned on next game start");
        GUI.DragWindow();
    }
}

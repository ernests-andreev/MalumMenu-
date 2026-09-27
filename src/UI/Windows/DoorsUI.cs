using UnityEngine;
using Il2CppSystem.Collections.Generic;

namespace MalumMenu;

public class DoorsUI : MonoBehaviour
{
    public static int windowHeight = 270;                               // Высота окна
    public static int windowWidth = 480;                                // Ширина окна
    public static Rect windowRect;                                      // Прямоугольник окна

    private List<SystemTypes> _doorsToSpamOpen = new();                 // Двери для спам-открытия
    private List<SystemTypes> _doorsToSpamClose = new();                // Двери для спам-закрытия

    private void Start()
    {
        // Создаём 2D-область DoorsUI
        windowRect = new(
            Screen.width / 2f - windowWidth / 2f,
            Screen.height / 2f - windowHeight / 2f,
            windowWidth,
            windowHeight
        );
    }

    private void OnGUI()
    {
        if (!CheatToggles.showDoorsMenu || !(MenuUI.isGUIActive || MalumMenu.menuKeepSubwindowsOpen.Value) || MalumMenu.isPanicked) return;

        UIHelpers.ApplyUIColor();       // Применить пользовательский цвет UI

        windowRect = GUI.Window((int)WindowId.DoorsUI, windowRect, (GUI.WindowFunction)DoorsWindow, "Двери");  // Заголовок окна: "Двери"
    }

    private void DoorsWindow(int windowID)
    {
        if (!Utils.isShip)              // Если нет корабля — окно неактивно
        {
            GUI.DragWindow();
            return;
        }

        var map = (MapNames)Utils.GetCurrentMapID();

        if (map is MapNames.MiraHQ)     // На MiraHQ дверей нет
        {
            GUI.DragWindow();
            return;
        }

        GUILayout.BeginVertical();

        foreach (var doorRoom in DoorsHandler.GetRoomsWithDoors())       // Проходим по всем комнатам с дверями
        {
            GUILayout.BeginHorizontal();

            // Название комнаты
            GUILayout.Label($"{doorRoom.ToString()}", GUILayout.Width(110f));

            GUILayout.BeginHorizontal();

            // Статус дверей в комнате
            GUILayout.Label($"{DoorsHandler.GetStatusOfDoorsInRoom(doorRoom, true)}");

            GUILayout.FlexibleSpace();

            // Кнопка "Закрыть"
            if (GUILayout.Button("Закрыть", GUIStylePreset.NormalButton, GUILayout.Width(50f)))
            {
                DoorsHandler.CloseDoorsInRoom(doorRoom);
            }

            // Кнопка "Открыть" (доступна только на Polus, Airship, Fungle)
            if (map is MapNames.Polus or MapNames.Airship or MapNames.Fungle)
            {
                if (GUILayout.Button("Открыть", GUIStylePreset.NormalButton, GUILayout.Width(50f)))
                {
                    DoorsHandler.OpenDoorsInRoom(doorRoom);
                }
            }

            if (Utils.isHost)       // Переключатели спама — только для хоста
            {
                // Спам-закрытие конкретной комнаты
                var spamClose = _doorsToSpamClose.Contains(doorRoom);
                spamClose = GUILayout.Toggle(spamClose, "Спам-закрыть", GUIStylePreset.NormalToggle);

                if (spamClose && !_doorsToSpamClose.Contains(doorRoom))
                {
                    _doorsToSpamClose.Add(doorRoom);
                }
                else if (!spamClose && _doorsToSpamClose.Contains(doorRoom))
                {
                    _doorsToSpamClose.Remove(doorRoom);
                }

                // Спам-открытие конкретной комнаты (только на Polus, Airship, Fungle)
                if (map is MapNames.Polus or MapNames.Airship or MapNames.Fungle)
                {
                    var spamOpen = _doorsToSpamOpen.Contains(doorRoom);
                    spamOpen = GUILayout.Toggle(spamOpen, "Спам-открыть", GUIStylePreset.NormalToggle);

                    if (spamOpen && !_doorsToSpamOpen.Contains(doorRoom))
                    {
                        _doorsToSpamOpen.Add(doorRoom);
                    }
                    else if (!spamOpen && _doorsToSpamOpen.Contains(doorRoom))
                    {
                        _doorsToSpamOpen.Remove(doorRoom);
                    }
                }
            }
            else
            {
                // Очищаем списки спама, если мы не хост
                if (_doorsToSpamClose.Count != 0 || _doorsToSpamOpen.Count != 0)
                {
                    _doorsToSpamClose.Clear();
                    _doorsToSpamOpen.Clear();
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.EndHorizontal();
        }

        GUILayout.FlexibleSpace();

        GUILayout.Box("", GUIStylePreset.Separator, GUILayout.Height(1f), GUILayout.ExpandWidth(true));   // Разделитель
        GUILayout.Space(1f);

        GUILayout.BeginHorizontal();

        // Кнопка "Закрыть все"
        if (GUILayout.Button("Закрыть все", GUIStylePreset.NormalButton))
        {
            CheatToggles.closeAllDoors = true;
        }

        // Кнопка "Открыть все" (только на Polus, Airship, Fungle)
        if (map is MapNames.Polus or MapNames.Airship or MapNames.Fungle)
        {
            if (GUILayout.Button("Открыть все", GUIStylePreset.NormalButton))
            {
                CheatToggles.openAllDoors = true;
            }
        }

        GUILayout.FlexibleSpace();

        if (Utils.isHost)       // Спам-переключатели для всех дверей — только для хоста
        {
            CheatToggles.spamCloseAllDoors = GUILayout.Toggle(CheatToggles.spamCloseAllDoors, "Спам-закрыть все", GUIStylePreset.NormalToggle);

            if (map is MapNames.Polus or MapNames.Airship or MapNames.Fungle)
            {
                CheatToggles.spamOpenAllDoors = GUILayout.Toggle(CheatToggles.spamOpenAllDoors, "Спам-открыть все", GUIStylePreset.NormalToggle);
            }
        }
        else
        {
            CheatToggles.spamCloseAllDoors = CheatToggles.spamOpenAllDoors = false;
        }

        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        GUI.DragWindow();       // Позволяет перетаскивать окно
    }

    public void Update()
    {
        if (!Utils.isShip) return;

        // Спам-закрытие выбранных дверей
        foreach (var doorRoom in _doorsToSpamClose)
        {
            DoorsHandler.CloseDoorsInRoom(doorRoom);
        }

        // Спам-открытие выбранных дверей
        var map = (MapNames)Utils.GetCurrentMapID();

        if (map is MapNames.Polus or MapNames.Airship or MapNames.Fungle)
        {
            foreach (var doorRoom in _doorsToSpamOpen)
            {
                DoorsHandler.OpenDoorsInRoom(doorRoom);
            }
        }
    }
}

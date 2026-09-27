using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace MalumMenu;

public class MenuUI : MonoBehaviour
{
    public static int windowHeight = 550;                               // Высота окна
    public static int windowWidth = 700;                                // Ширина окна
    public static Rect windowRect;                                      // Прямоугольник окна

    public static bool isGUIActive = false;                             // Активен ли GUI
    private List<ITab> _tabs = new();                                   // Список вкладок
    private int _selectedTab;                                           // Выбранная вкладка
    public static float hue; // Для RGB-режима

    private void Start()
    {
        // Добавляем все вкладки при старте
        _tabs.Add(new MovementTab());       // Движение
        _tabs.Add(new ESPTab());            // ESP
        _tabs.Add(new RolesTab());          // Роли
        _tabs.Add(new ShipTab());           // Корабль
        _tabs.Add(new ChatTab());           // Чат
        _tabs.Add(new AnimationsTab());     // Анимации
        _tabs.Add(new ConsoleTab());        // Консоль
        _tabs.Add(new HostOnlyTab());       // Только для хоста
        _tabs.Add(new PassiveTab());        // Пассивные
        _tabs.Add(new ModesTab());          // Режимы
        _tabs.Add(new ConfigTab());         // Конфиг
        // _tabs.Add(new OverloadTab());    // Перегрузка (закомментировано)

        // Создаём 2D-область MenuUI
        windowRect = new(
            Screen.width / 2f - windowWidth / 2f,
            Screen.height / 2f - windowHeight / 2f,
            windowWidth,
            windowHeight
        );
    }

    public void InitStyles()
    {
        // Задаём размер шрифта для переключателей, кнопок и меток
        GUI.skin.toggle.fontSize = GUI.skin.button.fontSize = GUI.skin.label.fontSize = 15;
    }

    private void Update()
    {

        // Включение/выключение GUI клавишей (по умолчанию DELETE)
        if (Input.GetKeyDown(Utils.StringToKeycode(MalumMenu.menuKeybind.Value)))
        {
            isGUIActive = !isGUIActive;

            if (MalumMenu.menuOpenOnMouse.Value)
            {
                // Перемещаем окно к мыши для немедленного использования
                Vector2 mousePosition = Input.mousePosition;
                windowRect.position = new Vector2(mousePosition.x, Screen.height - mousePosition.y);
            }
        }

        if (CheatToggles.rgbMode)       // Режим радужного цвета
        {
            hue += Time.deltaTime * 0.3f; // Скорость смены цвета (больше множитель = быстрее)
            if (hue > 1f) hue -= 1f;      // Зацикливаем оттенок
        }

        if (CheatToggles.stealthMode != MalumMenu.inStealthMode)
        {
            MalumMenu.inStealthMode = CheatToggles.stealthMode;

            Scene scene = SceneManager.GetActiveScene();

            // Перезагружаем сцену, чтобы применить скрытный режим
            if (scene.name == "MainMenu" || scene.name == "MatchMaking")
            {
                SceneManager.LoadScene(scene.name);
            }
        }

        if (CheatToggles.panicMode) Utils.Panic();      // Аварийное отключение

        // Скрыть штамп мода в скрытном режиме/панике
        var stamp = ModManager.Instance.ModStamp;
        if (stamp) stamp.enabled = !(MalumMenu.inStealthMode || MalumMenu.isPanicked);

        if (CheatToggles.openConfig)                    // Открыть конфиг
        {
            Utils.OpenConfigFile();
            CheatToggles.openConfig = false;
        }

        if (CheatToggles.reloadConfig)                  // Перезагрузить конфиг
        {
            MalumMenu.Plugin.Config.Reload();
            CheatToggles.reloadConfig = false;
        }

        if (CheatToggles.saveProfile)                   // Сохранить профиль
        {
            CheatToggles.saveProfile = false; // Сначала выключаем, чтобы не сохранить это в профиль
            CheatToggles.SaveTogglesToProfile();
        }

        if (CheatToggles.loadProfile)                   // Загрузить профиль
        {
            CheatToggles.LoadTogglesFromProfile();
            CheatToggles.loadProfile = false;
        }

        // Некоторые читы работают только если LocalPlayer существует — иначе отключаем
        if(!Utils.isPlayer)
        {
            CheatToggles.setFakeRole = false;
            CheatToggles.setFakeAlive = false;
            CheatToggles.killAll = false;
            CheatToggles.telekillPlayer = false;
            CheatToggles.killAllCrew = false;
            CheatToggles.killAllImps = false;
            CheatToggles.teleportPlayer = false;
            CheatToggles.spectate = false;
            CheatToggles.freecam = false;
            CheatToggles.killPlayer = false;
            CheatToggles.callMeeting = false;

            if (CheatToggles.runOverload)
            {
                OverloadUI.StopOverload();
            }
        }

        // Некоторые читы работают только если корабль существует — иначе отключаем
        if(!Utils.isShip)
        {
            CheatToggles.sabotageMap = false;
            CheatToggles.unfixableLights = false;
            CheatToggles.completeMyTasks = false;
            CheatToggles.kickVents = false;
            CheatToggles.reportBody = false;
            CheatToggles.closeMeeting = false;
            CheatToggles.reactorSab = false;
            CheatToggles.oxygenSab = false;
            CheatToggles.commsSab = false;
            CheatToggles.elecSab = false;
            CheatToggles.mushSab = false;
            CheatToggles.closeAllDoors = false;
            CheatToggles.openAllDoors = false;
            CheatToggles.spamCloseAllDoors = false;
            CheatToggles.spamOpenAllDoors = false;
            CheatToggles.mushSpore = false;

            MalumCheats.StopShipAnimCheats();
        }

        // Некоторые читы работают только если мы хост — иначе отключаем
        if(!Utils.isHost && !Utils.isFreePlay)
        {
            CheatToggles.killAll = false;
            CheatToggles.telekillPlayer = false;
            CheatToggles.killAllCrew = false;
            CheatToggles.killAllImps = false;
            CheatToggles.killPlayer = false;
            CheatToggles.ejectPlayer = false;
            CheatToggles.noKillCd = false;
            CheatToggles.killAnyone = false;
            CheatToggles.killVanished = false;
            CheatToggles.forceStartGame = false;
            CheatToggles.skipMeeting = false;
            CheatToggles.voteImmune = false;
            CheatToggles.noGameEnd = false;
            CheatToggles.showProtectMenu = false;
            CheatToggles.showRolesMenu = false;
            CheatToggles.noOptionsLimits = false;
        }

        // Некоторые читы работают только во время собрания — иначе отключаем
        if (!Utils.isMeeting)
        {
            CheatToggles.skipMeeting = false;
            CheatToggles.ejectPlayer = false;
        }
    }

    public void OnGUI()
    {
        if (!isGUIActive || MalumMenu.isPanicked) return;

        InitStyles();

        UIHelpers.ApplyUIColor();       // Применить пользовательский цвет UI

        // Заголовок окна: "MalumMenu v3.3.0"
        windowRect = GUI.Window((int)WindowId.MenuUI, windowRect, (GUI.WindowFunction)WindowFunction, "MalumMenu v" + MalumMenu.malumVersion);
    }

    public void WindowFunction(int windowID)
    {
        GUILayout.BeginHorizontal();

        // Левый селектор вкладок (15% ширины)
        GUILayout.BeginVertical(GUILayout.Width(windowWidth * 0.15f));
        for (var i = 0; i < _tabs.Count; i++)
        {
            Color standardColor = GUI.backgroundColor;

            // Подсветка выбранной вкладки
            if (_selectedTab == i)
            {
                GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
            }

            // Кнопка вкладки
            if (GUILayout.Button(_tabs[i].name, GUIStylePreset.TabButton, GUILayout.Height(35)))
                _selectedTab = i;

            GUI.backgroundColor = standardColor;

        }
        GUILayout.EndVertical();

        // Вертикальная линия-разделитель + отступ между селектором и содержимым
        GUILayout.Box("", GUIStylePreset.Separator, GUILayout.Width(1f), GUILayout.ExpandHeight(true));
        GUILayout.Space(10f);

        // Содержимое вкладки и элементы управления (85% ширины)
        GUILayout.BeginVertical(GUILayout.Width(windowWidth * 0.85f));

        // Контент выбранной вкладки
        if (_selectedTab >= 0 && _selectedTab < _tabs.Count)
        {
            GUILayout.Label(_tabs[_selectedTab].name, GUIStylePreset.TabTitle);    // Заголовок вкладки
            _tabs[_selectedTab].Draw();                                             // Отрисовка вкладки
        }

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        // Позволяет перетаскивать окно
        GUI.DragWindow();
    }
}

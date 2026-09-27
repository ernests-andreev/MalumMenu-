using System.IO;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using UnityEngine.SceneManagement;
using System;
using UnityEngine;
using UnityEngine.Analytics;
using System.Collections.Generic;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace MalumMenu;

[BepInAutoPlugin]
[BepInProcess("Among Us.exe")]
public partial class MalumMenu : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);
    public static MalumMenu Plugin;
    public new static ManualLogSource Log;
    public static readonly string ProfilePath = Path.Combine(Paths.ConfigPath, "MalumProfile.txt");  // Путь к файлу профиля: ConfigPath/MalumProfile.txt

    public static MenuUI menuUI;
    public static ConsoleUI consoleUI;
    public static RolesUI rolesUI;
    public static OverloadUI overloadUI;
    public static DoorsUI doorsUI;
    public static TasksUI tasksUI;
    public static ProtectUI protectUI;
    public static KeybindListener keybindListener;

    public static string malumVersion = "3.3.0";                       // Версия MalumMenu
    public static List<string> supportedAU = new List<string> { "2026.8.18" };  // Поддерживаемые версии Among Us
    public static bool isPanicked = false;                              // Флаг паники
    public static bool inStealthMode = false;                           // Флаг скрытного режима

    public static ConfigEntry<string> menuKeybind;
    public static ConfigEntry<string> menuHtmlColor;
    public static ConfigEntry<bool> menuOpenOnMouse;
    public static ConfigEntry<bool> menuKeepSubwindowsOpen;
    public static ConfigEntry<bool> menuAllowClickThrough;
    public static ConfigEntry<string> spoofLevel;
    public static ConfigEntry<string> spoofPlatform;
    public static ConfigEntry<bool> spoofDeviceId;
    public static ConfigEntry<bool> noTelemetry;
    public static ConfigEntry<string> guestFriendCode;
    public static ConfigEntry<bool> guestMode;
    public static ConfigEntry<bool> autoLoadProfile;
    public static ConfigEntry<string> configEditor;
    public static ConfigEntry<int> adaptMaxStrength;
    public static ConfigEntry<float> adaptMaxCooldown;
    public static ConfigEntry<float> attackLogDelay;
    public static ConfigEntry<int> defaultStrength;
    public static ConfigEntry<float> defaultCooldown;
    public static ConfigEntry<int> killSwitchLvl;

    public override void Load()
    {
        Log = base.Log;
        Plugin = this;

        // Загружает настройки конфигурации
        menuKeybind = Config.Bind("MalumMenu.GUI",
                                "Keybind",
                                "Delete",
                                "Клавиша, используемая для включения и выключения GUI. Список поддерживаемых кодов клавиш: https://docs.unity3d.com/Packages/com.unity.tiny@0.16/api/Unity.Tiny.Input.KeyCode.html");

        menuHtmlColor = Config.Bind("MalumMenu.GUI",
                                "Color",
                                "",
                                "Пользовательский цвет для GUI MalumMenu. Поддерживает HTML-коды цветов");

        menuOpenOnMouse = Config.Bind("MalumMenu.GUI",
                                "OpenOnMouse",
                                false,
                                "Если включено, GUI MalumMenu всегда будет открываться в текущей позиции мыши");

        menuKeepSubwindowsOpen = Config.Bind("MalumMenu.GUI",
                                "KeepSubwindowsOpen",
                                false,
                                "Если включено, закрытие GUI MalumMenu не будет автоматически закрывать его подокна");

        menuAllowClickThrough = Config.Bind("MalumMenu.GUI",
                                "AllowClicksThrough",
                                true,
                                "Если включено, клики проходят сквозь GUI MalumMenu, позволяя взаимодействовать с элементами GUI Among Us позади него");

        autoLoadProfile = Config.Bind("MalumMenu.Profile",
                                "AutoLoadProfile",
                                false,
                                "Если включено, ваш сохранённый профиль биндов и переключателей будет автоматически загружен при запуске игры");

        configEditor = Config.Bind("MalumMenu.Config",
                                "ConfigEditor",
                                "notepad.exe",
                                "Программа, используемая для открытия файла конфигурации при использовании переключателя Open Config. Может быть любым исполняемым файлом, но рекомендуется текстовый редактор");

        // Настройки конфигурации GuestMode закомментированы, так как читы сломаны в последних обновлениях

        // guestMode = Config.Bind("MalumMenu.GuestMode",
        //                         "GuestMode",
        //                         false,
        //                         "Если включено, новая гостевая учётная запись будет создаваться каждый раз при запуске игры, позволяя обходить баны аккаунтов и обнаружение PUID");

        // guestFriendCode = Config.Bind("MalumMenu.GuestMode",
        //                         "FriendName",
        //                         "",
        //                         "Имя пользователя, которое будет использоваться при установке кода друга для вашей гостевой учётной записи. ВАЖНО: Можно использовать только с GuestMode, должно быть ≤ 10 символов и не может содержать специальные символы/дискриминатор (#1234)");

        spoofLevel = Config.Bind("MalumMenu.Spoofing",
                                "Level",
                                "",
                                "Пользовательский уровень игрока для отображения другим в онлайн-играх, чтобы скрыть вашу реальную платформу. ВАЖНО: Пользовательские уровни могут быть только в диапазоне от 1 до 100001. Дробные числа не будут работать");

        spoofPlatform = Config.Bind("MalumMenu.Spoofing",
                                "Platform",
                                "",
                                "Пользовательская игровая платформа для отображения другим в онлайн-лобби, чтобы скрыть вашу реальную платформу. Список поддерживаемых платформ: https://skeld.js.org/enums/_skeldjs_constant.Platform.html");

        spoofDeviceId = Config.Bind("MalumMenu.Privacy",
                                "HideDeviceId",
                                true,
                                "Если включено, это скроет ваш уникальный deviceId от Among Us, что потенциально может помочь обойти аппаратные баны в будущем");

        noTelemetry = Config.Bind("MalumMenu.Privacy",
                                "NoTelemetry",
                                true,
                                "Если включено, это остановит Among Us от сбора аналитики ваших игр и отправки их в Innersloth через Unity Analytics");

        // adaptMaxStrength = Config.Bind("MalumMenu.Overload",
        //                         "AdaptMaxStrength",
        //                         18000,
        //                         new ConfigDescription(
        //                             "Максимальное общее количество RPC, отправляемых за один цикл перегрузки в режиме AutoAdapt. Автоматически распределяется между целями и уменьшается в зависимости от пинга. ВАЖНО: Только от 1 до 100K RPC",
        //                             new AcceptableValueRange<int>(1, 100000)
        //                         ));

        // adaptMaxCooldown = Config.Bind("MalumMenu.Overload",
        //                         "AdaptMaxCooldown",
        //                         1f,
        //                         new ConfigDescription(
        //                             "Максимальное время (в секундах) для завершения одного полного цикла перегрузки в режиме AutoAdapt. Автоматически распределяется между целями (больше целей = короче задержка на цель). ВАЖНО: Только от 0с до 10с",
        //                             new AcceptableValueRange<float>(0f, 10f)
        //                         ));

        // attackLogDelay = Config.Bind("MalumMenu.Overload",
        //                         "AttackLogDelay",
        //                         2f,
        //                         "Минимальное время (в секундах) между логами атак в обычном (не подробном) режиме");

        // defaultStrength = Config.Bind("MalumMenu.Overload",
        //                         "DefaultStrength",
        //                         18000,
        //                         new ConfigDescription(
        //                             "Количество повреждённых RPC, отправляемых каждой цели по умолчанию во время цикла перегрузки. Переопределяется, если включён режим AutoAdapt. ВАЖНО: Только от 1 до 100K RPC",
        //                             new AcceptableValueRange<int>(1, 100000)
        //                         ));

        // defaultCooldown = Config.Bind("MalumMenu.Overload",
        //                         "DefaultCooldown",
        //                         1f,
        //                         new ConfigDescription(
        //                             "Кулдаун по умолчанию (в секундах) между каждой целью во время цикла перегрузки. Переопределяется, если включён режим AutoAdapt. ВАЖНО: Только от 0с до 10с",
        //                             new AcceptableValueRange<float>(0f, 10f)
        //                         ));

        // killSwitchLvl = Config.Bind("MalumMenu.Overload",
        //                         "DefaultKillSwitchLevel",
        //                         1,
        //                         new ConfigDescription(
        //                             "Уровень по умолчанию, используемый kill switch. Каждый уровень добавляет 500 мс к максимально допустимому пингу перед остановкой перегрузки. Помогает избежать лагов/отключений. ВАЖНО: Только от уровня 1 (500 мс) до 6 (3000 мс)",
        //                             new AcceptableValueRange<int>(1, 6)
        //                         ));

        // Включено по умолчанию
        CheatToggles.unlockFeatures = true;
        CheatToggles.freeCosmetics = true;
        CheatToggles.avoidPenalties = true;

        // Включено по умолчанию
        CheatToggles.olAutoAdapt = true;
        CheatToggles.olKillSwitch = true;
        CheatToggles.olAutoStop = true;
        CheatToggles.olAutoClear = true;
        CheatToggles.olLogStartStop = true;
        CheatToggles.olLogAttack = true;
        CheatToggles.olLogAddRemove = true;
        CheatToggles.olLogDisconnect = true;

        Harmony.PatchAll();

        // Интерфейс
        menuUI = AddComponent<MenuUI>();
        consoleUI = AddComponent<ConsoleUI>();
        doorsUI = AddComponent<DoorsUI>();
        tasksUI = AddComponent<TasksUI>();
        protectUI = AddComponent<ProtectUI>();
        // overloadUI = AddComponent<OverloadUI>();
        // rolesUI = AddComponent<RolesUI>();

        // Компоненты
        keybindListener = AddComponent<KeybindListener>();

        // Отключает телеметрию (полностью не проверено, работает ли это, но согласно документации Unity — должно)
        if (noTelemetry.Value)
        {
            Analytics.enabled = false;
            Analytics.deviceStatsEnabled = false;
            PerformanceReporting.enabled = false;
        }

        // Создать файл профиля, если он отсутствует
        if (!File.Exists(ProfilePath))
        {
            CheatToggles.SaveTogglesToProfile();
        }

        // Автозагрузка профиля при старте, если нужно
        if (autoLoadProfile.Value)
        {
            CheatToggles.LoadTogglesFromProfile();
        }

        SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>) ((scene, _) =>
        {
            if (scene.name == "MainMenu" && !(inStealthMode || isPanicked))
            {
                // Предупреждает о неподдерживаемых версиях Among Us
                if (!supportedAU.Contains(Application.version))
                {
                    Utils.ShowPopup("\nЭта версия MalumMenu и эта версия Among Us несовместимы\n\nУстановите правильную версию, чтобы избежать проблем");
                }
            }
        }));
    }
}

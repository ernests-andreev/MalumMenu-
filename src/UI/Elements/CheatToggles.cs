using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AmongUs.GameOptions;
using UnityEngine;

namespace MalumMenu;

public struct CheatToggles
{
    // Движение
    public static bool noClip;
    public static bool teleportPlayer;
    public static bool teleportCursor;
    public static bool invertControls;

    // Роли
    public static bool setFakeRole;
    public static bool setFakeAlive;
    public static bool noKillCd;
    public static bool showTasksMenu;
    public static bool completeMyTasks;
    public static bool impostorTasks;
    public static bool killReach;
    public static bool killAnyone;
    public static bool endlessSsDuration;
    public static bool endlessBattery;
    public static bool endlessTracking;
    public static bool noTrackingCooldown;
    public static bool noTrackingDelay;
    public static bool trackReach;
    public static bool interrogateReach;
    public static bool noVitalsCooldown;
    public static bool noVentCooldown;
    public static bool endlessVentTime;
    public static bool endlessVanish;
    public static bool killVanished;
    public static bool noVanishAnim;
    public static bool noShapeshiftAnim;

    // ESP
    public static bool noShadows;
    public static bool seeGhosts;
    public static bool seeRoles;
    public static bool seePlayerInfo;
    public static bool seeDisguises;
    public static bool taskArrows;
    public static bool revealVotes;
    public static bool seeLobbyInfo;

    // Камера
    public static bool spectate;
    public static bool zoomOut;
    public static bool freecam;

    // Мини-карта
    public static bool mapCrew;
    public static bool mapImps;
    public static bool mapGhosts;
    public static bool colorBasedMap;

    // Трассеры
    public static bool tracersImps;
    public static bool tracersCrew;
    public static bool tracersGhosts;
    public static bool tracersBodies;
    public static bool colorBasedTracers;
    public static bool distanceBasedTracers;

    // Чат
    public static bool enableChat;
    public static bool unlockCharacters;
    public static bool bypassUrlBlock;
    public static bool longerMessages;
    public static bool unlockClipboard;
    public static bool lowerRateLimits;

    // Корабль
    public static bool closeMeeting;
    public static bool autoOpenDoorsOnUse;
    public static bool unfixableLights;
    public static bool callMeeting;
    public static bool reportBody;
    public static bool autoReportBodies;

    // Саботаж
    public static bool commsSab;
    public static bool elecSab;
    public static bool reactorSab;
    public static bool oxygenSab;
    public static bool mushSab;
    public static bool mushSpore;
    public static bool showDoorsMenu;
    public static bool openAllDoors;
    public static bool closeAllDoors;
    public static bool spamOpenAllDoors;
    public static bool spamCloseAllDoors;
    public static bool sabotageMap;

    // Вентиляция
    public static bool unlockVents;
    public static bool walkInVents;
    public static bool kickVents;

    // Анимации
    public static bool animShields;
    public static bool animAsteroids;
    public static bool animEmptyGarbage;
    public static bool animMedScan;
    public static bool animCamsInUse;
    public static bool animPet;
    public static bool moonWalk;

    // Перегрузка
    public static bool showOverload;
    public static bool showOverloadSettings;
    public static bool olAutoStart;
    public static bool olAutoAdapt;
    public static bool olShowRpcTotal;
    public static bool olAutoStop;
    public static bool olLockTargets;
    public static bool olKillSwitch;
    public static bool olPlayerCooldown;
    public static bool olAutoClear;
    public static bool olLogStartStop;
    public static bool olLogAddRemove;
    public static bool olLogDisconnect;
    public static bool olLogAttack;
    public static bool olVerboseLogs;
    public static bool runOverload;
    public static bool overloadAll;
    public static bool overloadHost;
    public static bool overloadCrew;
    public static bool overloadImps;
    public static bool overloadReset;

    // Консоль
    public static bool showConsole;
    public static bool logDeaths;
    public static bool logShapeshifts;
    public static bool logVents;
    public static bool logTasks;
    public static bool logGameState;

    // Только для хоста
    public static bool voteImmune;
    public static bool forceRole;
    public static RoleTypes? forcedRole;
    public static bool showRolesMenu;
    public static bool skipMeeting;
    public static bool forceStartGame;
    public static bool noGameEnd;
    public static bool showProtectMenu;
    public static bool noOptionsLimits;
    public static bool ejectPlayer;
    public static bool killPlayer;
    public static bool telekillPlayer;
    public static bool killAll;
    public static bool killAllCrew;
    public static bool killAllImps;

    // Пассивные
    public static bool unlockFeatures;
    public static bool freeCosmetics;
    public static bool avoidPenalties;
    public static bool copyLobbyCodeOnDisconnect;
    public static bool spoofAprilFoolsDate;

    // Режимы
    public static bool rgbMode;
    public static bool stealthMode;
    public static bool panicMode;

    // Конфиг
    public static bool reloadConfig;
    public static bool openConfig;
    public static bool loadProfile;
    public static bool saveProfile;

    // Карта биндов: Имя переключателя -> KeyCode (KeyCode.None == нет клавиши)
    public static readonly Dictionary<string, KeyCode> Keybinds = new();

    // Карта для доступа через рефлексию: Имя переключателя -> FieldInfo
    public static readonly Dictionary<string, FieldInfo> ToggleFields = new();

    // Заполняем карту рефлексии один раз при старте и инициализируем Keybinds значением KeyCode.None
    static CheatToggles()
    {
        var fields = typeof(CheatToggles).GetFields(BindingFlags.Static | BindingFlags.Public);

        foreach (var field in fields)
        {
            if (field.FieldType != typeof(bool)) continue;

            ToggleFields[field.Name] = field;
            Keybinds[field.Name] = KeyCode.None;
        }
    }

    // Отключает все читы, связанные с меню выбора игрока (PPM), кроме указанного
    public static void DisablePPMCheats(string variableToKeep)
    {
        ejectPlayer = variableToKeep == "ejectPlayer" && ejectPlayer;
        reportBody = variableToKeep == "reportBody" && reportBody;
        killPlayer = variableToKeep == "killPlayer" && killPlayer;
        telekillPlayer = variableToKeep == "telekillPlayer" && telekillPlayer;
        spectate = variableToKeep == "spectate" && spectate;
        setFakeRole = variableToKeep == "setFakeRole" && setFakeRole;
        setFakeAlive = variableToKeep == "setFakeAlive" && setFakeAlive;
        forceRole = variableToKeep == "forceRole" && forceRole;
        teleportPlayer = variableToKeep == "teleportPlayer" && teleportPlayer;
    }

    // Возвращает true, если меню выбора игрока (PPM) должно закрыться (нет активных PPM-читов)
    public static bool ShouldPPMClose()
    {
        return !setFakeRole && !setFakeAlive && !forceRole && !ejectPlayer && !reportBody && !telekillPlayer && !killPlayer && !spectate && !teleportPlayer;
    }

    // Отключает все читы, устанавливая всем значение false через кэш ToggleFields
    public static void DisableAll()
    {
        foreach (var field in ToggleFields.Values)
        {
            field.SetValue(null, false);
        }
    }

    // Сохраняет переключатели читов и их бинды в MalumProfile.txt
    // Формат строки: ИмяПереключателя = True/False = KeyCode.KEY
    public static void SaveTogglesToProfile()
    {
        using var writer = new StreamWriter(MalumMenu.ProfilePath);

        writer.WriteLine("# MalumProfile");
        writer.WriteLine("# Формат: ИмяПереключателя = True/False = KeyCode.KEY");
        writer.WriteLine("# - Список поддерживаемых кодов клавиш: https://docs.unity3d.com/Packages/com.unity.tiny@0.16/api/Unity.Tiny.Input.KeyCode.html");
        writer.WriteLine("# - Установка бинда необязательна. Используйте KeyCode.None, чтобы не задавать бинд");
        writer.WriteLine("# - Несколько переключателей могут иметь одну и ту же клавишу, но несколько клавиш на один переключатель НЕ поддерживаются");
        writer.WriteLine("# - Бинды применяются только после загрузки этого профиля через кнопку 'Загрузить из профиля' в меню Конфиг");
        writer.WriteLine();

        foreach (var field in ToggleFields.Values)
        {
            Keybinds.TryGetValue(field.Name, out var key);  // Если клавиша не установлена, пишем KeyCode.None
            writer.WriteLine($"{field.Name} = {field.GetValue(null)} = KeyCode.{key}");
        }
    }

    // Загружает переключатели читов и их бинды из MalumProfile.txt, если файл существует
    // Формат строки: ИмяПереключателя = True/False = KeyCode.KEY
    public static void LoadTogglesFromProfile()
    {
        if (!File.Exists(MalumMenu.ProfilePath)) return;

        using var reader = new StreamReader(MalumMenu.ProfilePath);

        while (reader.ReadLine() is { } line)
        {
            // Пропускаем пустые строки
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Пропускаем закомментированные строки
            line = line.Trim();
            if (line.StartsWith("#")) continue;

            // Извлекаем три значимых значения из каждой оставшейся строки
            var parts = line.Split('=', 3);
            if (parts.Length < 2) continue;

            // Получаем FieldInfo чита по его имени
            var name = parts[0].Trim();
            if (!ToggleFields.TryGetValue(name, out var field)) continue;

            // Загружаем, включён ли чит по умолчанию
            if (bool.TryParse(parts[1].Trim(), out var boolVal))
            {
                field.SetValue(null, boolVal);
            }

            // Загружаем бинд, связанный с каждым читом
            KeyCode key = KeyCode.None;
            if (parts.Length >= 3)
            {
                var keyPart = parts[2].Trim();
                if (keyPart.StartsWith("KeyCode."))
                {
                    keyPart = keyPart["KeyCode.".Length..];
                }

                if (!string.IsNullOrEmpty(keyPart) && System.Enum.TryParse<KeyCode>(keyPart, true, out var parsed))
                {
                    key = parsed;
                }
            }

            Keybinds[name] = key;
        }
    }
}

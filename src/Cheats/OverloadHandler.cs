using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace MalumMenu;
public static class OverloadHandler
{
    public static float cooldown;                           // Кулдаун между отправками
    public static int strength;                             // Сила атаки (кол-во RPC)
    private static HashSet<int> _customTargets = new();     // Кастомные цели (по ClientId)
    private static float _timer;                            // Таймер до следующей отправки
    private static float _attackLogTimer;                   // Таймер до следующего лога атаки
    private static Dictionary<int, int> _rpcCounters = new(); // Счётчики отправленных RPC по каждому клиенту
    private static int _nextTarget = int.MinValue;          // Следующая цель в очереди
    private static bool _hasRun;                            // Был ли уже запущен overload в этой итерации

    public static void Run()
    {
        // Если overload выключен или нет целей — сбрасываем состояние
        if (!CheatToggles.runOverload || OverloadUI.currentTargets.Count <= 0)
        {
            _timer = cooldown;
            _attackLogTimer = MalumMenu.attackLogDelay.Value;
            _nextTarget = int.MinValue;
            _hasRun = false;
            _rpcCounters.Clear();
            return;
        }

        _timer += Time.unscaledDeltaTime;
        _attackLogTimer += Time.unscaledDeltaTime;

        if (_timer >= cooldown)
        {
            // Если выбраны все возможные цели...

            if (OverloadUI.maxPossibleTargets == OverloadUI.currentTargets.Count)
            {
                int broadcastId = -1; // ... эффективнее использовать широковещательную рассылку RPC

                Utils.Overload(broadcastId, strength);
                _timer -= cooldown;

                if (CheatToggles.olLogAttack)
                {
                    string colorStr = ColorUtility.ToHtmlStringRGB(Palette.Orange);

                    if (!CheatToggles.olVerboseLogs)
                    {
                        _rpcCounters.TryAdd(broadcastId, 0);

                        _rpcCounters.TryGetValue(broadcastId, out var rpcCount);

                        int newRpcCount = rpcCount + strength;

                        // Логируем число разосланных RPC с момента последнего лога
                        // Логируется не чаще, чем раз в attackLogDelay (в секундах)

                        if (_attackLogTimer >= MalumMenu.attackLogDelay.Value)
                        {
                            OverloadUI.LogConsole($"> <b><color=#{colorStr}>Разослано {newRpcCount} повреждённых RPC всем игрокам (ID : {broadcastId})</color></b>");

                            _attackLogTimer -= MalumMenu.attackLogDelay.Value;

                            _rpcCounters.Clear();
                        }
                        else
                        {
                            _rpcCounters[broadcastId] = newRpcCount;
                        }
                    }
                    else // Логируем каждую рассылку отдельно, если включены подробные логи
                    {
                        OverloadUI.LogConsole($"> <b><color=#{colorStr}>Разослано {strength} повреждённых RPC всем игрокам (ID : {broadcastId})</color></b>");
                    }
                }

                return;
            }

            var currentTargets = OverloadUI.currentTargets;

            foreach (NetworkedPlayerInfo targetData in currentTargets)
            {
                int clientId = targetData.ClientId;

                if (!_hasRun)
                {
                    if (_nextTarget == int.MinValue || clientId == _nextTarget) // Нет отмеченной цели (новый цикл) ИЛИ clientId — отмеченная цель
                    {
                        Utils.Overload(clientId, strength);
                        _timer -= cooldown;

                        if (CheatToggles.olLogAttack)
                        {
                            string colorStr = ColorUtility.ToHtmlStringRGB(Palette.Orange);

                            if (!CheatToggles.olVerboseLogs)
                            {
                                _rpcCounters.TryAdd(clientId, 0);

                                _rpcCounters.TryGetValue(clientId, out var rpcCount);

                                int newRpcCount = rpcCount + strength;

                                _rpcCounters[clientId] = newRpcCount;
                            }
                            else // Логируем каждую отправку отдельно, если включены подробные логи
                            {
                                OverloadUI.LogConsole($"> <b><color=#{colorStr}>Отправлено {strength} повреждённых RPC игроку {targetData.DefaultOutfit.PlayerName} (ID : {clientId})</color></b>");
                            }
                        }

                        _hasRun = true; // Отмечаем, что overload уже запущен в этой итерации
                    }
                }
                else // Если overload уже запущен в этой итерации...
                    // (всегда был у предыдущего игрока последовательно)
                {
                    // Отмечаем текущего игрока как цель для следующей итерации
                    _nextTarget = clientId;
                    _hasRun = false;

                    // Выходим, чтобы следующая итерация началась сразу после кулдауна
                    return;
                }
            }

            // После полной итерации (цикл завершился)...

            // ... (1) Сбрасываем состояние, чтобы начать цикл снова с первого игрока

            _nextTarget = int.MinValue;
            _hasRun = false;

            // ... (2) Логируем число отправленных RPC с последнего лога для всех currentTargets
            // Логируется не чаще, чем раз в attackLogDelay (в секундах)

            if (!CheatToggles.olVerboseLogs)
            {
                if (_attackLogTimer >= MalumMenu.attackLogDelay.Value)
                {
                    string colorStr = ColorUtility.ToHtmlStringRGB(Palette.Orange);

                    foreach (KeyValuePair<int, int> entry in _rpcCounters)
                    {
                        int clientId = entry.Key;
                        int rpcCount = entry.Value;

                        NetworkedPlayerInfo playerData = OverloadUI.currentTargets.FirstOrDefault(pd => pd.ClientId == clientId);

                        if (playerData != null)
                        {
                            OverloadUI.LogConsole($"> <b><color=#{colorStr}>Отправлено {rpcCount} повреждённых RPC игроку {playerData.DefaultOutfit.PlayerName} (ID : {clientId})</color></b>");
                        }
                    }

                    _attackLogTimer -= MalumMenu.attackLogDelay.Value;

                    _rpcCounters.Clear();
                }
            }
            else
            {
                _rpcCounters.Clear();
            }
        }
    }

    // Добавляет игрока в список кастомных целей
    public static void AddCustomTarget(NetworkedPlayerInfo playerData)
    {
        int clientId = playerData.ClientId;
        _customTargets.Add(clientId);
    }

    // Удаляет игрока из списка кастомных целей
    public static void RemoveCustomTarget(NetworkedPlayerInfo playerData)
    {
        int clientId = playerData.ClientId;
        _customTargets.Remove(clientId);
    }

    // Проверяет, является ли игрок кастомной целью
    public static bool IsCustomTarget(NetworkedPlayerInfo playerData)
    {
        return _customTargets.Contains(playerData.ClientId);
    }

    // Определяет, является ли игрок целью и по каким фильтрам
    public static (HashSet<TargetType> targetTypes, bool isTarget) GetTarget(NetworkedPlayerInfo playerData)
    {
        bool isTarget = false;
        var targetTypes = new HashSet<TargetType>();

        // Фильтр "Все"
        if (CheatToggles.overloadAll)
        {
            targetTypes.Add(TargetType.All);
            isTarget = true;
        }

        // Фильтр "Хост"
        bool hostTarget = CheatToggles.overloadHost && AmongUsClient.Instance.HostId == playerData.ClientId;
        if (hostTarget)
        {
            targetTypes.Add(TargetType.Host);
            isTarget = true;
        }

        if (playerData.Role != null)
        {
            RoleTeamTypes roleTeamType = playerData.Role.TeamType;

            // Фильтр "Члены экипажа"
            bool crewTarget = CheatToggles.overloadCrew && roleTeamType.Equals(RoleTeamTypes.Crewmate);
            if (crewTarget)
            {
                targetTypes.Add(TargetType.Crewmate);
                isTarget = true;
            }

            // Фильтр "Предатели"
            bool impTarget = CheatToggles.overloadImps && roleTeamType.Equals(RoleTeamTypes.Impostor);
            if (impTarget)
            {
                targetTypes.Add(TargetType.Impostor);
                isTarget = true;
            }
        }

        // Фильтр "Кастомные цели"
        bool customTarget = IsCustomTarget(playerData);
        if (customTarget)
        {
            targetTypes.Add(TargetType.Custom);
            isTarget = true;
        }

        if (!isTarget)
        {
            targetTypes.Add(TargetType.None);
        }

        return (targetTypes, isTarget);
    }

    // Очищает список кастомных целей
    public static void ClearCustomTargets()
    {
        _customTargets.Clear();
    }

    // Проходит по всем заданным игрокам и
    // добавляет всех, кто отмечен как цель и совпадает с заданным targetType, в _customTargets
    public static void PopulateCustomTargets(PlayerControl[] players, TargetType targetType)
    {
        int playerCount = players.Length;

        for (int i = 0; i < playerCount; i++)
        {
            NetworkedPlayerInfo playerData = players[i].Data;
            var playerTarget = GetTarget(playerData);
            bool isTarget = playerTarget.isTarget;

            if (isTarget && !IsCustomTarget(playerData))
            {
                HashSet<TargetType> currentTargetTypes = playerTarget.targetTypes;
                if (currentTargetTypes.Contains(targetType))
                {
                    AddCustomTarget(playerData);
                }
            }
        }
    }

    // Возвращает адаптированные силу и кулдаун, используя число currentTargets и пинг AmongUsClient
    // Балансирует их для минимальных лагов, но эффективного вывода
    public static (int strength, float cooldown) CalculateAdaptedValues()
    {
        int targetCount = OverloadUI.maxPossibleTargets == OverloadUI.currentTargets.Count
                        ? 1 // Режим широковещания считается как одна цель
                        : Math.Max(1, OverloadUI.currentTargets.Count); // Защита от деления на 0

        float maxCooldown = MalumMenu.adaptMaxCooldown.Value;
        float cooldown = maxCooldown / targetCount;

        int pingLevel = Math.Max(1, Utils.GetPing() / 100); // 0-99 мс = Ур. 1, 100-199 мс = Ур. 1, 200-299 мс = Ур. 2, ...

        int maxStrength = MalumMenu.adaptMaxStrength.Value;
        int strength = Math.Max(1, maxStrength / pingLevel / targetCount);

        return (strength, cooldown);
    }

    // Типы целей для фильтров overload
    public enum TargetType
    {
        None,       // Нет
        All,        // Все
        Custom,     // Кастомные
        Host,       // Хост
        Impostor,   // Предатели
        Crewmate    // Члены экипажа
    }
}

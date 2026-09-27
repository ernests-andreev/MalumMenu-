using Il2CppSystem.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils;
using System;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace MalumMenu;
public static class MalumPPMCheats
{
    private static bool _telekillPlayerActive;
    private static bool _killPlayerActive;
    private static bool _spectateActive;
    private static bool _teleportPlayerActive;
    private static bool _reportBodyActive;
    private static bool _ejectPlayerActive;
    private static bool _setFakeRoleActive;
    private static bool _setFakeAliveActive;
    private static bool _forceRoleActive;
    private static RoleTypes? _oldRole = null;

    public static void ReportBodyPPM()
    {
        if (CheatToggles.reportBody)
        {

            if (!_reportBodyActive)
            {
                // Закрываем уже открытые меню выбора игрока и их читы
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("reportBody");
                }

                // Меню выбора игрока, чтобы выбрать любое тело (живое или мёртвое) и зарепортить его
                PlayerPickMenu.OpenPlayerPickMenu(Utils.GetAllPlayerData(), (Action) (() =>
                {
                    PlayerControl.LocalPlayer.CmdReportDeadBody(PlayerPickMenu.targetPlayerData);
                }));

                _reportBodyActive = true;
            }

            // Отключаем чит, если меню закрыто
            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.reportBody = false;
            }

        }
        else
        {
            if (_reportBodyActive)
            {
                _reportBodyActive = false;
            }
        }
    }

    public static void EjectPlayerPPM()
    {
        if (CheatToggles.ejectPlayer)
        {
            if (!_ejectPlayerActive)
            {
                // Закрываем уже открытые меню выбора игрока и их читы
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("ejectPlayer");
                }

                if (!Utils.isMeeting)
                {
                    CheatToggles.ejectPlayer = false;
                    return;
                }

                List<NetworkedPlayerInfo> playerInfo = new List<NetworkedPlayerInfo>();
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (!player.Data.IsDead && !player.Data.Disconnected)
                    {
                        playerInfo.Add(player.Data);
                    }
                }

                // Меню выбора игрока, чтобы выбрать любого живого игрока и изгнать его во время собрания
                PlayerPickMenu.OpenPlayerPickMenu(playerInfo, (Action)(() =>
                {
                    NetworkedPlayerInfo playerToEject = PlayerPickMenu.targetPlayerData;
                    MeetingHud.Instance.RpcVotingComplete(new Il2CppStructArray<MeetingHud.VoterState>(0L), playerToEject, false, false, ushort.MinValue);
                }));

                _ejectPlayerActive = true;
            }

            // Отключаем чит, если меню закрыто
            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.ejectPlayer = false;
            }
        }
        else if (_ejectPlayerActive)
        {
            _ejectPlayerActive = false;
        }
    }

    public static void KillPlayerPPM()
    {
        if (CheatToggles.killPlayer)
        {
            if (!_killPlayerActive)
            {
                // Закрываем уже открытые меню выбора игрока и их читы
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("killPlayer");
                }

                if (Utils.isLobby)
                {
                    HudManager.Instance.Notifier.AddDisconnectMessage("Убийство в лобби отключено — слишком много багов");
                    CheatToggles.killPlayer = false;
                    return;
                }

                // Меню выбора игрока для убийства любого игрока через успешный MurderPlayer RPC-вызов
                PlayerPickMenu.OpenPlayerPickMenu(Utils.GetAllPlayerData(), (Action)(() =>
                {
                    Utils.MurderPlayer(PlayerPickMenu.targetPlayerData.Object, MurderResultFlags.Succeeded);
                }));

                _killPlayerActive = true;
            }

            // Отключаем чит, если меню закрыто
            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.killPlayer = false;
            }
        }
        else if (_killPlayerActive)
        {
            _killPlayerActive = false;
        }
    }

    public static void TelekillPlayerPPM()
    {
        if (CheatToggles.telekillPlayer)
        {
            if (!_telekillPlayerActive)
            {
                // Закрываем уже открытые меню выбора игрока и их читы
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("telekillPlayer");
                }

                if (Utils.isLobby)
                {
                    HudManager.Instance.Notifier.AddDisconnectMessage("Убийство в лобби отключено — слишком много багов");
                    CheatToggles.telekillPlayer = false;
                    return;
                }

                // Меню выбора игрока для убийства любого игрока через успешный MurderPlayer RPC-вызов
                // и немедленного телепорта обратно в исходную позицию
                PlayerPickMenu.OpenPlayerPickMenu(Utils.GetAllPlayerData(), (Action)(() =>
                {
                    var oldPos = PlayerControl.LocalPlayer.GetTruePosition();
                    Utils.MurderPlayer(PlayerPickMenu.targetPlayerData.Object, MurderResultFlags.Succeeded);
                    AmongUsClient.Instance.StartCoroutine(Utils.DelayedSnapTo(oldPos));
                }));

                _telekillPlayerActive = true;
            }

            // Отключаем чит, если меню закрыто
            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.telekillPlayer = false;
            }
        }
        else if (_telekillPlayerActive)
        {
            _telekillPlayerActive = false;
        }
    }

    public static void TeleportPlayerPPM()
    {
        if (CheatToggles.teleportPlayer)
        {
            if (!_teleportPlayerActive)
            {
                // Закрываем уже открытые меню выбора игрока и их читы
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("teleportPlayer");
                }

                List<NetworkedPlayerInfo> playerDataList = new List<NetworkedPlayerInfo>();

                // Все игроки, кроме LocalPlayer, сохраняются в playerList
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (!player.AmOwner)
                    {
                        playerDataList.Add(player.Data);
                    }
                }

                // Меню выбора игрока для телепортации LocalPlayer к позиции любого игрока
                PlayerPickMenu.OpenPlayerPickMenu(playerDataList, (Action)(() =>
                {
                    PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(PlayerPickMenu.targetPlayerData.Object.transform.position);
                }));

                _teleportPlayerActive = true;
            }

            // Отключаем чит, если меню закрыто
            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.teleportPlayer = false;
            }
        }
        else if (_teleportPlayerActive)
        {
            _teleportPlayerActive = false;
        }
    }

    public static void SetFakeRolePPM()
    {
        if (CheatToggles.setFakeRole)
        {

            if (!_setFakeRoleActive)
            {

                // Закрываем уже открытые меню выбора игрока и их читы
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("setFakeRole");
                }

                List<NetworkedPlayerInfo> playerDataList = new List<NetworkedPlayerInfo>();

                // Роль Оборотня можно использовать только если она уже была назначена в начале игры
                // Это сделано, чтобы античит не кикал игроков
                if (_oldRole == RoleTypes.Shapeshifter || Utils.isFreePlay)
                {
                    playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Оборотень", OutfitPreset.Shapeshifter, Utils.GetBehaviourByRoleType(RoleTypes.Shapeshifter)));
                }

                // Роль Фантома можно использовать только если она уже была назначена в начале игры
                // Это сделано, чтобы античит не кикал игроков
                if (_oldRole == RoleTypes.Phantom || Utils.isFreePlay)
                {
                    playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Фантом", OutfitPreset.Phantom, Utils.GetBehaviourByRoleType(RoleTypes.Phantom)));
                }

                // Роль Змеи можно использовать только если она уже была назначена в начале игры
                // Это сделано, чтобы античит не кикал игроков
                if (_oldRole == RoleTypes.Viper || Utils.isFreePlay)
                {
                    playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Змея", OutfitPreset.Viper, Utils.GetBehaviourByRoleType(RoleTypes.Viper)));
                }

                // Роль Предателя можно использовать только если она уже была назначена в начале игры или если вы хост
                // Это сделано, чтобы античит не кикал игроков
                if ((_oldRole != null && Utils.GetBehaviourByRoleType((RoleTypes)_oldRole).TeamType == RoleTeamTypes.Impostor) || Utils.isFreePlay || Utils.isHost)
                {
                    playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Предатель", OutfitPreset.Impostor, Utils.GetBehaviourByRoleType(RoleTypes.Impostor)));
                }

                // Роль Судьи можно использовать только если она уже была назначена в начале игры
                // Это сделано, чтобы античит не кикал игроков
                if (_oldRole == RoleTypes.Judge || Utils.isFreePlay)
                {
                    playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Судья", OutfitPreset.Judge, Utils.GetBehaviourByRoleType(RoleTypes.Judge)));
                }

                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Трекер", OutfitPreset.Tracker, Utils.GetBehaviourByRoleType(RoleTypes.Tracker)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Шумовик", OutfitPreset.Noisemaker, Utils.GetBehaviourByRoleType(RoleTypes.Noisemaker)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Инженер", OutfitPreset.Engineer, Utils.GetBehaviourByRoleType(RoleTypes.Engineer)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Учёный", OutfitPreset.Scientist, Utils.GetBehaviourByRoleType(RoleTypes.Scientist)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Детектив", OutfitPreset.Detective, Utils.GetBehaviourByRoleType(RoleTypes.Detective)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Член экипажа", OutfitPreset.Crewmate, Utils.GetBehaviourByRoleType(RoleTypes.Crewmate)));

                // Меню выбора игрока для смены своей роли через кастомный список выбора
                PlayerPickMenu.OpenPlayerPickMenu(playerDataList, (Action) (() =>
                {
                    // Логируем изначально назначенную роль до того, как её изменит чит setFakeRole
                    if (!Utils.isLobby && !Utils.isFreePlay && _oldRole == null)
                    {
                        _oldRole = PlayerControl.LocalPlayer.Data.RoleType;
                    }

                    if (PlayerControl.LocalPlayer.Data.IsDead) // Предотвращаем случайное воскрешение
                    {
                        if (PlayerPickMenu.targetPlayerData.Role.TeamType == RoleTeamTypes.Impostor)
                        {
                            RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.ImpostorGhost);
                        }
                        else
                        {
                            RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.CrewmateGhost);
                        }
                    }
                    else
                    {
                        /* if (PlayerPickMenu.targetPlayerData.Role.Role == RoleTypes.Shapeshifter && oldRole != RoleTypes.Shapeshifter){

                            Utils.showPopup("\n<size=125%>Смена на роль Оборотня не рекомендуется\nтак как превращение приведёт к кику античитом");

                        } else if (PlayerPickMenu.targetPlayerData.Role.Role == RoleTypes.Noisemaker && oldRole != RoleTypes.Noisemaker){

                            Utils.showPopup("\n<size=125%>Смена на роль Шумовика не рекомендуется\nтак как смерть не вызовет оповещение для других игроков");

                        } else if (oldRole == RoleTypes.Noisemaker){

                            Utils.showPopup("\n<size=125%>Ваша «настоящая» роль всё ещё Шумовик\nтак что другие игроки всё равно увидят оповещение, когда вы умрёте");

                        } */

                        RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, PlayerPickMenu.targetPlayerData.Role.Role);
                    }
                }));

                _setFakeRoleActive = true;
            }

            // Отключаем чит, если меню закрыто
            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.setFakeRole = false;
            }

        }
        else
        {
            if (_setFakeRoleActive)
            {
                _setFakeRoleActive = false;
            }
        }
    }

    public static void SetFakeAlivePPM()
    {
        if (CheatToggles.setFakeAlive)
        {

            if (!_setFakeAliveActive)
            {

                // Закрываем уже открытые меню выбора игрока и их читы
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("setFakeAlive");
                }

                List<NetworkedPlayerInfo> playerDataList = new List<NetworkedPlayerInfo>();

                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Живой", OutfitPreset.Crewmate, Utils.GetBehaviourByRoleType(RoleTypes.Crewmate)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Мёртвый", OutfitPreset.Dead, Utils.GetBehaviourByRoleType(RoleTypes.CrewmateGhost)));

                // Меню выбора игрока для смены своего состояния "живой/мёртвый" через кастомный список выбора
                PlayerPickMenu.OpenPlayerPickMenu(playerDataList, (Action) (() =>
                {
                    if (PlayerPickMenu.targetPlayerData.Role.IsDead)
                    {
                        PlayerControl.LocalPlayer.Die(DeathReason.Exile, true);
                    }
                    else
                    {
                        PlayerControl.LocalPlayer.Revive();
                    }
                }));

                _setFakeAliveActive = true;
            }

            // Отключаем чит, если меню закрыто
            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.setFakeAlive = false;
            }

        }
        else
        {
            if (_setFakeAliveActive)
            {
                _setFakeAliveActive = false;
            }
        }
    }

    public static void ForceRolePPM()
    {
        if (CheatToggles.forceRole)
        {
            if (!_forceRoleActive)
            {
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("forceRole");
                }

                List<NetworkedPlayerInfo> playerDataList = new List<NetworkedPlayerInfo>();

                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Оборотень", OutfitPreset.Shapeshifter, Utils.GetBehaviourByRoleType(RoleTypes.Shapeshifter)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Фантом", OutfitPreset.Phantom, Utils.GetBehaviourByRoleType(RoleTypes.Phantom)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Змея", OutfitPreset.Viper, Utils.GetBehaviourByRoleType(RoleTypes.Viper)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Предатель", OutfitPreset.Impostor, Utils.GetBehaviourByRoleType(RoleTypes.Impostor)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Трекер", OutfitPreset.Tracker, Utils.GetBehaviourByRoleType(RoleTypes.Tracker)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Шумовик", OutfitPreset.Noisemaker, Utils.GetBehaviourByRoleType(RoleTypes.Noisemaker)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Инженер", OutfitPreset.Engineer, Utils.GetBehaviourByRoleType(RoleTypes.Engineer)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Учёный", OutfitPreset.Scientist, Utils.GetBehaviourByRoleType(RoleTypes.Scientist)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Детектив", OutfitPreset.Detective, Utils.GetBehaviourByRoleType(RoleTypes.Detective)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Член экипажа", OutfitPreset.Crewmate, Utils.GetBehaviourByRoleType(RoleTypes.Crewmate)));

                // Меню выбора игрока для форсирования роли на другого игрока
                PlayerPickMenu.OpenPlayerPickMenu(playerDataList, (Action)(() =>
                {
                    CheatToggles.forcedRole = PlayerPickMenu.targetPlayerData.Role.Role;
                }));

                _forceRoleActive = true;
            }

            // Отключаем чит, если меню закрыто
            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.forceRole = false;
            }

        }
        else
        {
            if (_forceRoleActive)
            {
                _forceRoleActive = false;
            }
        }
    }

    public static void SpectatePPM()
    {
        if (CheatToggles.spectate)
        {

            if (!_spectateActive)
            {

                // Закрываем уже открытые меню выбора игрока и их читы
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("spectate");
                }

                List<NetworkedPlayerInfo> playerDataList = new List<NetworkedPlayerInfo>();

                // Все игроки, кроме LocalPlayer, сохраняются в playerList
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (!player.AmOwner)
                    {
                        playerDataList.Add(player.Data);
                    }
                }

                // Меню выбора игрока для наблюдения за выбранным игроком
                PlayerPickMenu.OpenPlayerPickMenu(playerDataList, (Action) (() =>
                {
                    Camera.main.gameObject.GetComponent<FollowerCamera>().SetTarget(PlayerPickMenu.targetPlayerData.Object);
                }));

                _spectateActive = true;

                PlayerControl.LocalPlayer.moveable = false; // Нельзя двигаться во время наблюдения

                CheatToggles.freecam = false; // Отключаем несовместимые читы во время наблюдения

            }

            // Отключаем чит, если меню закрыто и никто не наблюдается
            if (PlayerPickMenu.playerpickMenu == null && Camera.main.gameObject.GetComponent<FollowerCamera>().Target == PlayerControl.LocalPlayer)
            {
                CheatToggles.spectate = false;
                PlayerControl.LocalPlayer.moveable = true;
            }
        }
        else
        {
            // Отключаем чит, когда он выключен через GUI Malum
            if (_spectateActive)
            {
                _spectateActive = false;
                PlayerControl.LocalPlayer.moveable = true;
                Camera.main.gameObject.GetComponent<FollowerCamera>().SetTarget(PlayerControl.LocalPlayer);
            }
        }
    }
}

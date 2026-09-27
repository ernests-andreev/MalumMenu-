using System;
using UnityEngine;
using InnerNet;
using System.Linq;
using Il2CppSystem.Collections.Generic;
using System.IO;
using Hazel;
using System.Reflection;
using AmongUs.GameOptions;
using BepInEx;
using HarmonyLib;
using UnityEngine.SceneManagement;
using Sentry.Internal.Extensions;
using System.Runtime.CompilerServices;
using AmongUs.InnerNet.GameDataMessages;
using Il2CppInterop.Runtime.Injection;

namespace MalumMenu;

public static class Utils
{
    public static bool isPastingInput;
    public static ReferenceDataManager ReferenceDataManager = DestroyableSingleton<ReferenceDataManager>.Instance; // Полезно для получения полных списков всех ID косметики Among Us
    public static SabotageSystemType SabotageSystem => ShipStatus.Instance.Systems[SystemTypes.Sabotage].Cast<SabotageSystemType>();
    public static bool isShip => ShipStatus.Instance;                                // Находимся ли на корабле
    public static bool isClient => AmongUsClient.Instance;                            // Являемся ли клиентом
    public static bool isLobby => AmongUsClient.Instance && AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Joined && !isFreePlay;  // В лобби ли мы
    public static bool isOnlineGame => AmongUsClient.Instance && AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame;  // Онлайн-игра
    public static bool isLocalGame => AmongUsClient.Instance && AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame;    // Локальная игра
    public static bool isFreePlay => AmongUsClient.Instance && AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay;      // Режим свободной игры
    public static bool isPlayer => PlayerControl.LocalPlayer;                         // Есть ли локальный игрок
    public static bool isHost => AmongUsClient.Instance && AmongUsClient.Instance.AmHost;  // Являемся ли хостом
    public static bool isInGame => AmongUsClient.Instance && AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started && isPlayer;  // Идёт ли игра
    public static bool isMeeting => MeetingHud.Instance;                              // Идёт ли собрание
    public static bool isMeetingVoting => isMeeting && MeetingHud.Instance.state is MeetingHud.MeetingStates.Voted or MeetingHud.MeetingStates.NotVoted;  // Идёт ли голосование
    public static bool isMeetingProceeding => isMeeting && MeetingHud.Instance.state is MeetingHud.MeetingStates.Proceeding;  // Обработка результатов собрания
    public static bool isExiling => ExileController.Instance && !(isAirshipMap && SpawnInMinigame.Instance.isActiveAndEnabled);  // Идёт ли изгнание
    public static bool isAnySabotageActive => ShipStatus.Instance && SabotageSystem.AnyActive;  // Активна ли какая-либо саботаж
    public static bool isNormalGame => GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.Normal;      // Обычный режим
    public static bool isHideNSeek => GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.HideNSeek;    // Прятки
    public static bool isSkeldMap => (MapNames)GetCurrentMapID() == MapNames.Skeld;      // Карта Skeld
    public static bool isMiraHQMap => (MapNames)GetCurrentMapID() == MapNames.MiraHQ;    // Карта MiraHQ
    public static bool isPolusMap => (MapNames)GetCurrentMapID() == MapNames.Polus;      // Карта Polus
    public static bool isDleksMap => (MapNames)GetCurrentMapID() == MapNames.Dleks;      // Карта Dleks
    public static bool isAirshipMap => (MapNames)GetCurrentMapID() == MapNames.Airship;  // Карта Airship
    public static bool isFungleMap => (MapNames)GetCurrentMapID() == MapNames.Fungle;    // Карта Fungle
    public const float DefaultSpeed = 2.5f;         // Скорость по умолчанию
    public const float DefaultGhostSpeed = 3f;      // Скорость призрака по умолчанию

    // Проверяет, что скорость LocalPlayer на значении по умолчанию
    public static bool IsSpeedDefault(bool forGhost = false)
    {
        return forGhost ? Mathf.Approximately(PlayerControl.LocalPlayer.MyPhysics.GhostSpeed, DefaultGhostSpeed) :
            Mathf.Approximately(PlayerControl.LocalPlayer.MyPhysics.Speed, DefaultSpeed);
    }

    // Притягивает скорость LocalPlayer к значению по умолчанию, если в пределах snapRange
    public static void SnapSpeedToDefault(float snapRange, bool forGhost = false)
    {
        if (forGhost)
        {
            PlayerControl.LocalPlayer.MyPhysics.GhostSpeed = Mathf.Abs(PlayerControl.LocalPlayer.MyPhysics.GhostSpeed - DefaultGhostSpeed)
                                                             < snapRange ? DefaultGhostSpeed : PlayerControl.LocalPlayer.MyPhysics.GhostSpeed;
        }
        else
        {
            PlayerControl.LocalPlayer.MyPhysics.Speed = Mathf.Abs(PlayerControl.LocalPlayer.MyPhysics.Speed - DefaultSpeed)
                                                        < snapRange ? DefaultSpeed : PlayerControl.LocalPlayer.MyPhysics.Speed;
        }
    }

    // Получает реальное имя игрока, отображаемое имя и признак маскировки
    public static (string realName, string displayName, bool isDisguised) GetPlayerIdentity(PlayerControl player)
    {
        if (player == null || player.Data == null) return ("", "", false);

        var realName = $"<color=#{ColorUtility.ToHtmlStringRGB(player.Data.Color)}>{player.Data.PlayerName}</color>";         // Реальное имя с цветом
        var displayName = $"<color=#{ColorUtility.ToHtmlStringRGB(Palette.PlayerColors[player.CurrentOutfit.ColorId])}>{player.CurrentOutfit.PlayerName}</color>";  // Отображаемое имя с цветом
        var isDisguised = player.CurrentOutfit.PlayerName != player.Data.PlayerName;  // Маскируется ли

        return (realName, displayName, isDisguised);
    }

    // Проверяет, является ли игрок сейчас исчезнувшим (невидимым)
    public static bool IsVanished(NetworkedPlayerInfo playerInfo)
    {
        PhantomRole phantomRole = playerInfo.Role as PhantomRole;

        if (phantomRole != null)
        {
            return phantomRole.fading || phantomRole.isInvisible;
        }

        return false;
    }

    // Проверяет, является ли игрок допустимой целью в зависимости от того, включён чит killAnyone или нет
    public static bool IsValidTarget(NetworkedPlayerInfo target)
    {
        var killAnyoneRequirements = target && !target.Disconnected && target.Object.Visible && target.PlayerId != PlayerControl.LocalPlayer.PlayerId && target.Role && target.Object;

        var fullRequirements = killAnyoneRequirements && !target.IsDead && !target.Object.inVent && !target.Object.inMovingPlat && target.Role.CanBeKilled;

        return CheatToggles.killAnyone ? killAnyoneRequirements : fullRequirements;
    }

    public static List<NetworkedPlayerInfo> GetAllPlayerData()
    {
        var playerDataList = new List<NetworkedPlayerInfo>();
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player != null && player.Data != null)
            {
                playerDataList.Add(player.Data);
            }
        }

        return playerDataList;
    }

    // Настраивает разрешение HUD
    // Используется для исправления проблем UI при отдалении
    public static void AdjustResolution()
    {
        ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height, Screen.width, Screen.height, Screen.fullScreen);
    }

    // Получает RoleBehaviour из RoleType
    public static RoleBehaviour GetBehaviourByRoleType(RoleTypes roleType)
    {
        return RoleManager.Instance.AllRoles.ToArray().First(r => r.Role == roleType);
    }

    // Получает RoleBehaviour из TeamType
    public static RoleBehaviour GetBehaviourByTeamType(RoleTeamTypes roleTeamType)
    {
        RoleTypes roleType = (RoleTypes)Enum.Parse(typeof(RoleTypes), roleTeamType.ToString(), true);
        RoleBehaviour role = GetBehaviourByRoleType(roleType);

        return role;
    }

    public static void ForceSetScanner(PlayerControl player, bool toggle)
    {
        var count = ++player.scannerCount;
        player.SetScanner(toggle, count);
        RpcSetScannerMessage rpcMessage = new(player.NetId, toggle, count);
        AmongUsClient.Instance.LateBroadcastReliableMessage(Unsafe.As<IGameDataMessage>(rpcMessage));
    }

    public static void ForcePlayAnimation(byte animationType)
    {
        // PlayerControl.LocalPlayer.RpcPlayAnimation(1) не сработал бы, если визуальные задачи отключены
        // Приведённый ниже способ работает независимо от настроек визуальных задач

        PlayerControl.LocalPlayer.PlayAnimation(animationType);
        RpcPlayAnimationMessage rpcMessage = new(PlayerControl.LocalPlayer.NetId, animationType);
        AmongUsClient.Instance.LateBroadcastUnreliableMessage(Unsafe.As<IGameDataMessage>(rpcMessage));
    }

    // Корутина для телепортации LocalPlayer в позицию после задержки
    public static System.Collections.IEnumerator DelayedSnapTo(Vector2 position, float delay = 0.25f)
    {
        yield return new WaitForSeconds(delay);
        PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(position);
    }

    // Убивает любого игрока с помощью RPC-вызовов
    public static void MurderPlayer(PlayerControl target, MurderResultFlags result)
    {
        if (isFreePlay)
        {

            PlayerControl.LocalPlayer.MurderPlayer(target, MurderResultFlags.Succeeded);
            return;

        }

        foreach (var item in PlayerControl.AllPlayerControls)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.MurderPlayer, SendOption.Reliable, AmongUsClient.Instance.GetClientIdFromCharacter(item));
            writer.WriteNetObject(target);
            writer.Write((int)result);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
    }

    public static void CompleteTask(PlayerTask task)
    {
        if (isFreePlay)
        {
            PlayerControl.LocalPlayer.RpcCompleteTask(task.Id);
            return;
        }

        var hostData = AmongUsClient.Instance.GetHost();
        if (hostData == null || hostData.Character.Data.Disconnected) return;

        if (task.IsComplete) return;
        foreach (var item in PlayerControl.AllPlayerControls)
        {
            var messageWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.CompleteTask, SendOption.Reliable, AmongUsClient.Instance.GetClientIdFromCharacter(item));
            messageWriter.WritePacked(task.Id);
            AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
        }
    }

    // Открывает UI чата
    public static void OpenChat()
    {
        if (!DestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening)
        {
            DestroyableSingleton<HudManager>.Instance.Chat.chatScreen.SetActive(true);
            PlayerControl.LocalPlayer.NetTransform.Halt();
            DestroyableSingleton<HudManager>.Instance.Chat.StartCoroutine(DestroyableSingleton<HudManager>.Instance.Chat.CoOpen());
            if (DestroyableSingleton<FriendsListManager>.InstanceExists)
            {
                DestroyableSingleton<FriendsListManager>.Instance.SetFriendButtonColor(true);
            }
            if (DestroyableSingleton<HudManager>.Instance.Chat.chatNotification.gameObject.activeSelf)
			{
				DestroyableSingleton<HudManager>.Instance.Chat.chatNotification.Close();
			}
        }

    }

    // Рисует линию-трассер между двумя GameObject
    public static void DrawTracer(GameObject sourceObject, GameObject targetObject, Color color)
    {
        var lineRenderer = sourceObject.GetComponent<LineRenderer>();

        if (!lineRenderer)
        {
            lineRenderer = sourceObject.AddComponent<LineRenderer>();
        }

        lineRenderer.SetVertexCount(2);
        lineRenderer.SetWidth(0.02F, 0.02F);

        // Я просто взял уже существующий материал из игры
        Material material = DestroyableSingleton<HatManager>.Instance.PlayerMaterial;

        lineRenderer.material = material;
        lineRenderer.SetColors(color, color);

        lineRenderer.SetPosition(0, sourceObject.transform.position);
        lineRenderer.SetPosition(1, targetObject.transform.position);
    }

    // Возвращает, должен ли ChatUI быть активным или нет
    public static bool IsChatUiActive()
    {
        try
        {
            return CheatToggles.enableChat || MeetingHud.Instance || !ShipStatus.Instance || PlayerControl.LocalPlayer.Data.IsDead;
        }
        catch
        {
            return false;
        }
    }

    // Возвращает максимальное количество вложенных RPC, которое может быть в GameData сообщении
    // без кика от AC (античита)
    public static int GetMaxRpcPackingLimit()
    {
        int num = 0;

        if (isClient && AmongUsClient.Instance.AmHost)
        {
            num = GameManager.Instance.LogicOptions.MaxPlayers * 2;
        }

        return 10 + num;
    }

    // Перегружает цель заданной силой, используя Pet RPC, которые
    // многократно перезапускают анимацию поглаживания рукой, не давая старым
    // корутинам поглаживания завершиться
    public static void Overload(int targetId, int strength)
    {
        if (strength < 1) return;

        int maxRpc = GetMaxRpcPackingLimit();

        uint netId = PlayerControl.LocalPlayer.MyPhysics.NetId;
        byte rpcCall = (byte)RpcCalls.Pet;

        if (strength <= maxRpc)
        {
            // SendOption.None не имеет контроля потока, что позволяет флудить без ограничений

            var messageWriter = MessageWriter.Get(SendOption.None);

            if (targetId < 0) // -1 = Широковещательная рассылка
            {
                messageWriter.StartMessage(Tags.GameData);
                messageWriter.Write(AmongUsClient.Instance.GameId);
            }
            else
            {
                messageWriter.StartMessage(Tags.GameDataTo);
                messageWriter.Write(AmongUsClient.Instance.GameId);
                messageWriter.WritePacked(targetId);
            }

            for (var msg = 0; msg < strength; msg++)
            {
                messageWriter.StartMessage((byte)GameDataTypes.RpcFlag);

                messageWriter.WritePacked(netId);

                messageWriter.Write(rpcCall);

                // Используем LocalPlayer.GetTruePosition() как позицию поглаживания,
                // чтобы минимизировать задержку WalkPlayerTo и сразу запустить анимацию поглаживания

                NetHelpers.WriteVector2(PlayerControl.LocalPlayer.GetTruePosition(), messageWriter);

                // Позиция поглаживания декодируется как (-50, -50) на клиентах целей
                // Это держит анимацию поглаживания вне обычного поля зрения

                messageWriter.Write((ushort)0);

                messageWriter.Write((ushort)0);

                messageWriter.EndMessage();
            }

            messageWriter.EndMessage();

            AmongUsClient.Instance.connection.Send(messageWriter);

            messageWriter.Recycle();
        }
        else
        {
            int strengthGroups = strength / maxRpc;
            int remainder = strength % maxRpc;

            for (int group = 0; group < strengthGroups; group++)
            {
                Overload(targetId, maxRpc);
            }

            Overload(targetId, remainder);
        }
    }

    // Закрывает UI чата
    public static void CloseChat()
    {
        if (DestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening)
        {
            DestroyableSingleton<HudManager>.Instance.Chat.ForceClosed();
        }
    }

    // Получает расстояние между двумя игроками
    public static float GetDistanceBetween(PlayerControl source, PlayerControl target)
    {

        Vector2 vector = target.GetTruePosition() - source.GetTruePosition();
		float magnitude = vector.magnitude;

        return magnitude;

    }

    // Возвращает список всех игроков в игре, отсортированный от ближайших к дальним (от LocalPlayer по умолчанию)
    public static System.Collections.Generic.List<PlayerControl> GetPlayersSortedByDistance(PlayerControl source = null)
    {

        if (source.IsNull())
        {
            source = PlayerControl.LocalPlayer;
        }

        System.Collections.Generic.List<PlayerControl> outputList = new System.Collections.Generic.List<PlayerControl>();

        outputList.Clear();

        var allPlayers = GameData.Instance.AllPlayers;
        foreach (var playerInfo in allPlayers)
        {
            var player = playerInfo.Object;
            if (player)
            {
                outputList.Add(player);
            }
        }

        outputList = outputList.OrderBy(target => GetDistanceBetween(source, target)).ToList();

        return outputList.Count <= 0 ? null : outputList;
    }

    // Возвращает текущий ID карты, если доступен
    public static byte GetCurrentMapID()
    {
        // Работает для обучения
        if (isFreePlay)
        {
            return (byte)AmongUsClient.Instance.TutorialMapId;
        }

        // Работает для локальных / онлайн-игр
        if (GameOptionsManager.Instance?.currentGameOptions != null)
        {
            return GameOptionsManager.Instance.currentGameOptions.MapId;
        }

        // По умолчанию byte.MaxValue, если текущий ID карты недоступен
        return byte.MaxValue;
    }

    // Получает SystemType комнаты, в которой сейчас находится игрок
    public static SystemTypes GetCurrentRoom()
    {
        return HudManager.Instance.roomTracker.LastRoom.RoomId;
    }

    // Получает PlainShipRoom комнаты, которая перекрывает указанную позицию
    public static PlainShipRoom GetRoomFromPosition(Vector2 position)
    {
        return ShipStatus.Instance == null ? null : ShipStatus.Instance.AllRooms.FirstOrDefault(
            room => room != null && room.roomArea != null && room.roomArea.OverlapPoint(position));
    }

    // Возвращает цветной текст пинга для PingTracker
    public static string GetColoredPingText(string pingText, int ping)
    {
        return ping switch
        {
            < 1 => $"<color=#b8b8b8>{pingText}</color>", // Серый для пинга < 1
            < 100 => $"<color=#00ff00ff>{pingText}</color>", // Зелёный для пинга < 100
            < 400 => $"<color=#ffff00ff>{pingText}</color>", // Жёлтый для 100 < пинг < 400
            _ => $"<color=#ff0000ff>{pingText}</color>" // Красный для пинга > 400
        };
    }

    // Возвращает текущий приблизительный FPS
    public static int GetFps()
    {
        return (int)(1f / Time.unscaledDeltaTime);
    }

    // Получает UnityEngine.KeyCode из строки
    public static KeyCode StringToKeycode(string keyCodeStr)
    {

        if(!string.IsNullOrEmpty(keyCodeStr)) // Пустые строки автоматически недействительны
        {
            try
            {
                // Регистронезависимый парсинг UnityEngine.KeyCode для проверки валидности строки
                KeyCode keyCode = (KeyCode)Enum.Parse(typeof(KeyCode), keyCodeStr, true);

                return keyCode;

            }

            catch { }
        }

        return KeyCode.Delete; // Если строка недействительна, возвращаем Delete как клавишу по умолчанию
    }

    // Получает тип платформы из строки
    public static bool StringToPlatformType(string platformStr, out Platforms? platform)
    {
        if (!string.IsNullOrEmpty(platformStr)) // Пустые строки автоматически недействительны
        {
            try
            {
                // Регистронезависимый парсинг Platforms из строки (если валидна)
                platform = (Platforms)Enum.Parse(typeof(Platforms), platformStr, true);

                return true; // Если тип платформы валиден, возвращаем true
            }catch{}
        }

        platform = null;
        return false; // Если тип платформы недействителен, возвращаем false
    }

    public static string PlatformTypeToString(Platforms platform)
    {
        return platform switch
        {
            Platforms.StandaloneEpicPC => "Epic Games",
            Platforms.StandaloneSteamPC => "Steam",
            Platforms.StandaloneMac => "Mac",
            Platforms.StandaloneWin10 => "Microsoft Store",
            Platforms.StandaloneItch => "Itch.io",
            Platforms.IPhone => "iPhone / iPad",
            Platforms.Android => "Android",
            Platforms.Switch => "Nintendo Switch",
            Platforms.Xbox => "Xbox",
            Platforms.Playstation => "PlayStation",
            (Platforms)112 => "Starlight",
            _ => "Unknown"   // Неизвестно
        };
    }

    // Получает имя роли указанного игрока в виде строки
    // Строки автоматически переводятся
    public static string GetRoleName(NetworkedPlayerInfo playerData)
    {
        var translatedRole = DestroyableSingleton<TranslationController>.Instance.GetString(playerData.Role.StringName, Il2CppSystem.Array.Empty<Il2CppSystem.Object>());
        if (translatedRole != "STRMISS") return translatedRole;

        translatedRole = DestroyableSingleton<TranslationController>.Instance.GetString(GetBehaviourByTeamType(playerData.Role.TeamType).StringName, Il2CppSystem.Array.Empty<Il2CppSystem.Object>());
        return translatedRole;
    }

    // Получает подходящий неймтег для игрока
    public static string GetNameTag(NetworkedPlayerInfo playerInfo, string playerName, bool isChat = false, bool isMatchInfo = false)
    {
        var nameTag = playerName;

        if (playerInfo.Role.IsNull() || playerInfo.IsNull() || playerInfo.Disconnected ||
            playerInfo.Object.CurrentOutfit.IsNull()) return nameTag;

        var player = AmongUsClient.Instance.GetClientFromPlayerInfo(playerInfo);
        var host = AmongUsClient.Instance.GetHost();
        var level = playerInfo.PlayerLevel + 1;

        var platform = "Unknown";
        if (!isLocalGame) try { platform = PlatformTypeToString(player.PlatformData.Platform); } catch { }

        //var puid = player.ProductUserId;
        //var friendcode = player.FriendCode;

        var roleColor = ColorUtility.ToHtmlStringRGB(playerInfo.Role.TeamColor);

        var hostString = player == host ? "Host - " : "";   // "Хост - " если это хост

        if (CheatToggles.seeRoles)
        {

            if (CheatToggles.seePlayerInfo)
            {
                if (isChat)
                {
                    nameTag = $"<color=#{roleColor}>{nameTag} <size=70%>{GetRoleName(playerInfo)}</size></color> <size=70%><color=#fb0>{hostString}Lv:{level} - {platform}</color></size>";
                    return nameTag;
                }

                if (isMatchInfo)
                {
                    nameTag = $"<size=70%><color=#fb0>{hostString}Lv:{level} - {platform}</color></size>\r\n<color=#{roleColor}>{nameTag} <size=70%>{GetRoleName(playerInfo)}</size></color>";
                    return nameTag;
                }

                nameTag = $"<size=70%><color=#fb0>{hostString}Lv:{level} - {platform}</color></size>\r\n<color=#{roleColor}><size=70%>{GetRoleName(playerInfo)}</size>\r\n{nameTag}</color>";
            }
            else
            {
                if (isChat || isMatchInfo)
                {
                    nameTag = $"<color=#{roleColor}>{nameTag} <size=70%>{GetRoleName(playerInfo)}</size></color>";
                    return nameTag;
                }

                nameTag = $"<color=#{roleColor}><size=70%>{GetRoleName(playerInfo)}</size>\r\n{nameTag}</color>";
            }
        }
        else
        {
            if (CheatToggles.seePlayerInfo)
            {
                if (PlayerControl.LocalPlayer.Data.Role.NameColor == playerInfo.Role.NameColor)
                {
                    if (isChat)
                    {
                        nameTag = $"<color=#{ColorUtility.ToHtmlStringRGB(playerInfo.Role.NameColor)}>{nameTag}</color> <size=70%><color=#fb0>{hostString}Lv:{level} - {platform}</color></size>";
                        return nameTag;
                    }

                    if (isMatchInfo)
                    {
                        nameTag = $"<size=70%><color=#fb0>{hostString}Lv:{level} - {platform}</color></size>\r\n{nameTag}";
                        return nameTag;
                    }

                    nameTag = $"<size=70%><color=#fb0>{hostString}Lv:{level} - {platform}</color></size>\r\n<color=#{ColorUtility.ToHtmlStringRGB(playerInfo.Role.NameColor)}>{nameTag}";
                }
                else
                {
                    if (isChat)
                    {
                        nameTag = $"{nameTag} <size=70%><color=#fb0>{hostString}Lv:{level} - {platform}</color></size>";
                        return nameTag;
                    }

                    nameTag = $"<size=70%><color=#fb0>{hostString}Lv:{level} - {platform}</color></size>\r\n{nameTag}";
                }
            }
            else
            {
                if (PlayerControl.LocalPlayer.Data.Role.NameColor != playerInfo.Role.NameColor || isMatchInfo) return nameTag;

                nameTag = $"<color=#{ColorUtility.ToHtmlStringRGB(playerInfo.Role.NameColor)}>{nameTag}</color>";
            }
        }

        return nameTag;
    }

    // Возвращает NetworkedPlayerInfo игрока по его client ID
    public static NetworkedPlayerInfo GetPlayerDataFromClientId(int clientId)
    {
        var players = PlayerControl.AllPlayerControls.ToArray();

        for (int i = 0; i < players.Count; i++)
		{   NetworkedPlayerInfo playerData = players[i].Data;

			if (playerData.ClientId == clientId)
			{
				return playerData;
			}
		}

        return null; // Возвращает null, если соответствующий игрок не найден
    }

    // Возвращает случайное имя длиной от 1 до 12 символов
    public static string GetRandomName()
    {
        var length = UnityEngine.Random.Range(1, 13);
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length).Select(s => s[UnityEngine.Random.Range(0, s.Length)]).ToArray());
    }

    // Возвращает текущий пинг AmongUsClient в мс
    public static int GetPing()
    {
        if (isClient && AmongUsClient.Instance.AmClient)
        {
            return AmongUsClient.Instance.Ping;
        }
        else
        {
            return 0; // Возвращает 0, если не подключены к игре
        }
    }

    // Показывает кастомный попап в игре
    // Найдено здесь: https://github.com/NuclearPowered/Reactor/blob/6eb0bf19c30733b78532dada41db068b2b247742/Reactor/Networking/Patches/HttpPatches.cs
    public static void ShowPopup(string text)
    {
        var popup = UnityEngine.Object.Instantiate(DiscordManager.Instance.discordPopup, Camera.main!.transform);

        var background = popup.transform.Find("Background").GetComponent<SpriteRenderer>();
        var size = background.size;
        size.x *= 2.5f;
        background.size = size;

        popup.TextAreaTMP.fontSizeMin = 2;
        popup.Show(text);
    }

    public static void ShowNewPopup(string text)
    {
        DestroyableSingleton<DisconnectPopup>.Instance.ShowCustom(text);
    }

    // Загружает спрайты из ресурсов манифеста
    // Найдено здесь: https://github.com/Loonie-Toons/TOHE-Restored/blob/TOHE/Modules/Utils.cs
    public static Dictionary<string, Sprite> CachedSprites = new();
    public static Sprite LoadSprite(string path, float pixelsPerUnit = 1f)
    {
        try
        {
            if (CachedSprites.TryGetValue(path + pixelsPerUnit, out var sprite)) return sprite;

            Texture2D texture = LoadTextureFromResources(path);
            sprite = Sprite.Create(texture, new(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;

            return CachedSprites[path + pixelsPerUnit] = sprite;
        }
        catch
        {
            MalumMenu.Log.LogError($"Не удалось прочитать текстуру: {path}");
        }
        return null;
    }

    // Загружает текстуры из ресурсов манифеста
    // Найдено здесь: https://github.com/Loonie-Toons/TOHE-Restored/blob/TOHE/Modules/Utils.cs
    public static Texture2D LoadTextureFromResources(string path)
    {
        try
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
            var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            using MemoryStream ms = new();

            stream.CopyTo(ms);
            ImageConversion.LoadImage(texture, ms.ToArray(), false);
            return texture;
        }
        catch
        {
            MalumMenu.Log.LogError($"Не удалось прочитать текстуру: {path}");
        }
        return null;
    }

    // Открывает файл конфигурации в редакторе по умолчанию
    public static void OpenConfigFile()
    {
        var configFilePath = MalumMenu.Plugin.Config.ConfigFilePath;
        var configEditor = MalumMenu.configEditor.Value;

        if (!string.IsNullOrWhiteSpace(configEditor))
        {
            if (File.Exists(configFilePath))
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = configEditor,
                        Arguments = configFilePath,
                        UseShellExecute = true
                        //Verb = "edit"
                    });
                }
                catch (Exception ex)
                {
                    MalumMenu.Log.LogError(ex.Message);
                }
            }
            else
            {
                MalumMenu.Log.LogError("Файл конфигурации не существует");
            }
        }
        else
        {
            MalumMenu.Log.LogError("Редактор конфигурации не указан");
        }
    }

    public class PanicCleaner : MonoBehaviour
    {
        // Создаёт PanicCleaner для снятия патчей Harmony
        public static void Create()
        {
            ClassInjector.RegisterTypeInIl2Cpp<PanicCleaner>();
            var go = new GameObject("MalumMenu_PanicCleaner");
            go.hideFlags = HideFlags.HideAndDontSave;
            go.AddComponent<PanicCleaner>();
        }

        // Снятие патчей Harmony обрабатывается на следующем кадре после создания
        // Это позволяет некоторым патчам выполниться в последний раз и корректно завершиться
        private void LateUpdate()
        {
            try { Harmony.UnpatchID(MalumMenu.Id); } catch { }
            Destroy(gameObject);
        }
    }

    public static void Panic()
    {
        MalumMenu.isPanicked = true;    // Флаг паники

        CheatToggles.DisableAll();      // Отключить все читы

        var stamp = ModManager.Instance.ModStamp;
        if (stamp) stamp.enabled = false;

        Scene scene = SceneManager.GetActiveScene();

        if (scene.name == "MainMenu" || scene.name == "MatchMaking")
        {
            SceneManager.LoadScene(scene.name);
        }

        UnityEngine.Object.Destroy(MalumMenu.menuUI);

        UnityEngine.Object.Destroy(MalumMenu.consoleUI);
        UnityEngine.Object.Destroy(MalumMenu.doorsUI);
        UnityEngine.Object.Destroy(MalumMenu.tasksUI);
        UnityEngine.Object.Destroy(MalumMenu.protectUI);
        // UnityEngine.Object.Destroy(MalumMenu.rolesUI);
        // UnityEngine.Object.Destroy(MalumMenu.overloadUI);

        UnityEngine.Object.Destroy(MalumMenu.keybindListener);

        PanicCleaner.Create();      // Создать PanicCleaner для снятия патчей
    }
}

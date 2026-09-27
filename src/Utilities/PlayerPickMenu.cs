using UnityEngine;
using Il2CppSystem.Collections.Generic;
using Sentry.Internal.Extensions;

namespace MalumMenu;
public static class PlayerPickMenu
{
    public static ShapeshifterMinigame playerpickMenu;                      // Меню выбора игрока
    public static bool isActive;                                            // Активно ли меню
    public static NetworkedPlayerInfo targetPlayerData;                     // Данные выбранного игрока
    public static Il2CppSystem.Action customAction;                         // Пользовательское действие при выборе
    public static List<NetworkedPlayerInfo> customPlayerList;              // Пользовательский список игроков для выбора

    // Получаем префаб ShapeshifterMenu, чтобы его инстанцировать
    // Найдено здесь: https://github.com/AlchlcDvl/TownOfUsReworked/blob/9f3cede9d30bab2c11eb7c960007ab3979f09156/TownOfUsReworked/Custom/Menu.cs
    public static ShapeshifterMinigame GetShapeshifterMenu()
    {
        var rolePrefab = Utils.GetBehaviourByRoleType(AmongUs.GameOptions.RoleTypes.Shapeshifter);  // Получаем префаб роли Оборотень
        return Object.Instantiate(rolePrefab?.Cast<ShapeshifterRole>(), GameData.Instance.transform).ShapeshifterMenu;  // Инстанцируем и возвращаем ShapeshifterMenu
    }

    // Открывает меню выбора игрока, чтобы выбрать конкретного игрока в качестве цели
    public static void OpenPlayerPickMenu(List<NetworkedPlayerInfo> playerList, Il2CppSystem.Action action)
    {
        isActive = true;
        customPlayerList = playerList;
        customAction = action;

        // Меню основано на меню превращения (Shapeshifter)
        playerpickMenu = Object.Instantiate(GetShapeshifterMenu(), Camera.main.transform, false);

        playerpickMenu.transform.localPosition = new Vector3(0f, 0f, -50f);   // Позиционируем перед камерой
		playerpickMenu.Begin(null);
    }

    // Возвращает пользовательский NetworkedPlayerInfo, который можно использовать как вариант выбора в PPM (PlayerPickMenu)
    public static NetworkedPlayerInfo CustomPPMChoice(string name, NetworkedPlayerInfo.PlayerOutfit outfit, RoleBehaviour role = null)
    {
        NetworkedPlayerInfo customChoice = Object.Instantiate<NetworkedPlayerInfo>(GameData.Instance.PlayerInfoPrefab);  // Инстанцируем префаб PlayerInfo

        outfit.PlayerName = name;

        customChoice.Outfits[PlayerOutfitType.Default] = outfit;

        if (!role.IsNull())
        {
            customChoice.Role = role;
        }

        return customChoice;
    }
}

using HarmonyLib;

namespace MalumMenu;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Update))]
public static class AmongUsClient_Update
{
    public static void Postfix()
    {
        MalumSpoof.SpoofLevel();        // Подмена уровня игрока

        // Читы GuestMode закомментированы, так как сломаны в последних обновлениях

        // Код для обработки временных аккаунтов как полных, включая доступ к кодам друзей
        // if (!EOSManager.Instance.loginFlowFinished || !MalumMenu.guestMode.Value) return;
        // DataManager.Player.Account.LoginStatus = EOSManager.AccountLoginStatus.LoggedIn;

        // if (!string.IsNullOrWhiteSpace(EOSManager.Instance.FriendCode)) return;
        // var friendCode = MalumSpoof.spoofFriendCode();
        // var editUsername = EOSManager.Instance.editAccountUsername;
        // editUsername.UsernameText.SetText(friendCode);
        // editUsername.SaveUsername();
        // EOSManager.Instance.FriendCode = friendCode;
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
public static class AmongUsClient_OnGameJoined
{
    // Постфикс-патч AmongUsClient.OnGameJoined для сохранения строки ID последней присоединённой игры
    public static string lastGameIdString = "";

    public static void Postfix(string gameIdString)
    {
        lastGameIdString = gameIdString;
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
public static class AmongUsClient_CoStartGame
{
    public static void Postfix()
    {
        if (CheatToggles.logGameState)
            ConsoleUI.Log("Игра началась");
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
public static class AmongUsClient_OnGameEnd
{
    public static void Postfix(EndGameResult endGameResult)
    {
        if (CheatToggles.logGameState)
            ConsoleUI.Log($"Игра завершилась с причиной {endGameResult.GameOverReason}");
    }
}

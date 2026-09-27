using HarmonyLib;
using Il2CppSystem;
using UnityEngine;

namespace MalumMenu;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
public static class PlayerControl_SetKillTimer
{
    // Префикс-патч PlayerControl.SetKillTimer для снятия кулдауна убийства
    public static void Prefix(PlayerControl __instance, ref float time)
    {
        if (!__instance.AmOwner || !Utils.isHost || !CheatToggles.noKillCd) return;

        time = 0f;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckMurder))]
public static class PlayerControl_CmdCheckMurder
{
    // Префикс-патч PlayerControl.CmdCheckMurder, чтобы всегда обходить проверки при убийстве игроков
    public static bool Prefix(PlayerControl __instance, PlayerControl target)
    {
        /*if (Utils.isLobby){
            HudManager.Instance.Notifier.AddDisconnectMessage("Убийство в лобби отключено — слишком много багов");
            return false;
        }

        // Прямой kill RPC должен использоваться только при крайней необходимости, чтобы избежать обнаружения античит-модами
        if (!CheatToggles.killAnyone && !CheatToggles.zeroKillCd && !Utils.isVanished(__instance.Data) &&
            !Utils.isMeeting &&
            (MalumPPMCheats.oldRole == null ||
             Utils.getBehaviourByRoleType((AmongUs.GameOptions.RoleTypes)MalumPPMCheats.oldRole).IsImpostor))
            return true;
        if (!__instance.Data.Role.IsValidTarget(target.Data))
        {
            return true;
        }

        if (target.protectedByGuardianId > -1 && !CheatToggles.killAnyone){
            return true;
        }

        Utils.murderPlayer(target, MurderResultFlags.Succeeded);

        return false;*/

        if (!Utils.isHost) return true;

        // __instance.isKilling = true;
        PlayerControl.LocalPlayer.RpcMurderPlayer(target, true);

        return false;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
public static class PlayerControl_MurderPlayer
{
    // Префикс-патч PlayerControl.MurderPlayer для логирования в ConsoleUI, когда игрок пытается убить другого,
    // вместе с тем, кто убийца и цель, и где произошло убийство.
    // Также логирует, когда убийство спасает ангел-хранитель.
    public static void Prefix(PlayerControl __instance, PlayerControl target)
    {
        if (!CheatToggles.logDeaths || target == null) return;

        var (realKillerName, displayKillerName, isDisguised) = Utils.GetPlayerIdentity(__instance);
        var targetName = $"<color=#{ColorUtility.ToHtmlStringRGB(target.Data.Color)}>{target.CurrentOutfit.PlayerName}</color>";

        var room = Utils.GetRoomFromPosition(target.GetTruePosition());
        var roomName = room != null ? room.RoomId.ToString() : "неизвестном месте";

        if (target.protectedByGuardianId != -1)     // Убийство предотвращено ангелом-хранителем
        {
            ConsoleUI.Log(isDisguised ? $"{realKillerName} (под именем {displayKillerName}) пытался убить {targetName} в {roomName} (Защищён)"
                : $"{realKillerName} пытался убить {targetName} в {roomName} (Защищён)");
        }
        else
        {
            ConsoleUI.Log(isDisguised ? $"{realKillerName} (под именем {displayKillerName}) убил {targetName} в {roomName}"
                : $"{realKillerName} убил {targetName} в {roomName}");
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.TurnOnProtection))]
public static class PlayerControl_TurnOnProtection
{
    // Префикс-патч PlayerControl.TurnOnProtection, чтобы сделать все защиты видимыми
    public static void Prefix(ref bool visible)
    {
		if (CheatToggles.seeGhosts)
        {
            visible = true;
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckShapeshift))]
public static class PlayerControl_CmdCheckShapeshift
{
    // Префикс-патч PlayerControl.CmdCheckShapeshift, чтобы предотвратить анимацию превращения
    public static void Prefix(ref bool shouldAnimate)
    {
        if (shouldAnimate && CheatToggles.noShapeshiftAnim)
        {
            shouldAnimate = false;
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckRevertShapeshift))]
public static class PlayerControl_CmdCheckRevertShapeshift
{
    // Префикс-патч PlayerControl.CmdCheckRevertShapeshift, чтобы предотвратить анимацию превращения
    public static void Prefix(ref bool shouldAnimate){

        if (shouldAnimate && CheatToggles.noShapeshiftAnim)
        {
            shouldAnimate = false;
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
public static class PlayerControl_Shapeshift
{
    // Постфикс-патч PlayerControl.Shapeshift для логирования в ConsoleUI, когда игрок превращается в другого,
    // и в кого именно. Также логирует, когда превращение отменяется.
    public static void Postfix(PlayerControl __instance, PlayerControl targetPlayer, bool animate)
    {
        if (!CheatToggles.logShapeshifts) return;

        if (__instance.CurrentOutfitType == PlayerOutfitType.MushroomMixup) return;

        var targetPlayerInfo = targetPlayer.Data;

        var room = Utils.GetRoomFromPosition(__instance.GetTruePosition());
        var roomName = room != null ? room.RoomId.ToString() : "неизвестном месте";

        if (targetPlayerInfo.PlayerId == __instance.Data.PlayerId)      // Отмена превращения
        {
            ConsoleUI.Log($"<color=#{ColorUtility.ToHtmlStringRGB(GameData.Instance.GetPlayerById(__instance.PlayerId).Color)}>" +
                          $"{GameData.Instance.GetPlayerById(__instance.PlayerId)._object.Data.PlayerName}</color> отменил превращение в {roomName}");
        }
        else                                                            // Превращение в другого игрока
        {
            ConsoleUI.Log($"<color=#{ColorUtility.ToHtmlStringRGB(GameData.Instance.GetPlayerById(__instance.PlayerId).Color)}>" +
                          $"{GameData.Instance.GetPlayerById(__instance.PlayerId)._object.Data.PlayerName}</color> превратился в " +
                          $"<color=#{ColorUtility.ToHtmlStringRGB(GameData.Instance.GetPlayerById(targetPlayerInfo.PlayerId).Color)}>" +
                          $"{GameData.Instance.GetPlayerById(targetPlayerInfo.PlayerId)._object.Data.PlayerName}</color> в {roomName}");
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CompleteTask))]
public static class PlayerControl_CompleteTask
{
    // Постфикс-патч PlayerControl.CompleteTask для логирования выполненных задач
    public static void Postfix(PlayerControl __instance, uint idx)
    {
        if (!CheatToggles.logTasks) return;

        var task = __instance.myTasks.Find((Predicate<PlayerTask>)(p => (int)p.Id == (int)idx));
        var room = Utils.GetRoomFromPosition(__instance.GetTruePosition());
        var roomName = room != null ? room.RoomId.ToString() : "неизвестном месте";

        if (task)
        {
            ConsoleUI.Log(
                $"<color=#{ColorUtility.ToHtmlStringRGB(__instance.Data.Color)}>{__instance.Data.PlayerName}</color> выполнил задачу {task.TaskType} в {roomName}");
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
public static class PlayerControl_RpcSyncSettings
{
    // Префикс-патч PlayerControl.RpcSyncSettings, чтобы предотвратить кик античитом
    // за некоторые настройки, выходящие за «оригинальный» допустимый диапазон
    public static bool Prefix(PlayerControl __instance, byte[] optionsByteArray)
    {
        return !CheatToggles.noOptionsLimits;
    }
}

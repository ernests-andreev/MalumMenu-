using HarmonyLib;
using UnityEngine;

namespace MalumMenu;

[HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
public static class Vent_CanUse
{
    // Постфикс-патч Vent.CanUse, разрешающий использование вентиляции при включённом чите unlockVents
    public static void Postfix(Vent __instance, NetworkedPlayerInfo pc, ref bool canUse, ref bool couldUse, ref float __result)
    {
        if (!PlayerControl.LocalPlayer || !PlayerControl.LocalPlayer.Data) return;
        if (PlayerControl.LocalPlayer.Data.Role.CanVent || PlayerControl.LocalPlayer.Data.IsDead) return;
        if (!CheatToggles.unlockVents) return;

        var @object = pc.Object;

        var center = @object.Collider.bounds.center;
        var position = __instance.transform.position;
        var num = Vector2.Distance(center, position);

        // Разрешаем использование вентиляции, если она не слишком далеко и путь игрока не заблокирован
        canUse = num <= __instance.UsableDistance && !PhysicsHelpers.AnythingBetween(@object.Collider, center, position, Constants.ShipOnlyMask, false);
        couldUse = true;
        __result = num;
    }
}

[HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
public static class Vent_EnterVent
{
    // Постфикс-патч Vent.EnterVent для логирования в ConsoleUI, когда игрок входит в вентиляцию
    // вместе с комнатой, в которой это произошло
    public static void Postfix(Vent __instance, PlayerControl pc)
    {
        if (!CheatToggles.logVents || !Utils.isShip) return;

        var (realPlayerName, displayPlayerName, isDisguised) = Utils.GetPlayerIdentity(pc);
        var room = Utils.GetRoomFromPosition(__instance.transform.position); //- (Vector3) pc.Collider.offset);
        var roomName = room != null ? room.RoomId.ToString() : "неизвестном месте";

        ConsoleUI.Log(isDisguised
            ? $"{realPlayerName} (под именем {displayPlayerName}) вошёл в вентиляцию в {roomName}"
            : $"{realPlayerName} вошёл в вентиляцию в {roomName}");
    }
}

[HarmonyPatch(typeof(Vent), nameof(Vent.ExitVent))]
public static class Vent_ExitVent
{
    // Постфикс-патч Vent.ExitVent для логирования в ConsoleUI, когда игрок выходит из вентиляции
    // вместе с комнатой, в которой это произошло
    public static void Postfix(Vent __instance, PlayerControl pc)
    {
        if (!CheatToggles.logVents || !Utils.isShip) return;

        var (realPlayerName, displayPlayerName, isDisguised) = Utils.GetPlayerIdentity(pc);

        var room = Utils.GetRoomFromPosition(__instance.transform.position); //- (Vector3) pc.Collider.offset);
        var roomName = room != null ? room.RoomId.ToString() : "неизвестном месте";

        ConsoleUI.Log(isDisguised
            ? $"{realPlayerName} (под именем {displayPlayerName}) вышел из вентиляции в {roomName}"
            : $"{realPlayerName} вышел из вентиляции в {roomName}");
    }
}

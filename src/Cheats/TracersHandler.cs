using UnityEngine;

namespace MalumMenu;

public static class TracersHandler
{
    // Рисует трассер от LocalPlayer к другому игроку.
    public static void DrawPlayerTracer(PlayerPhysics playerPhysics)
    {
        try
        {
            var color = Color.clear; // По умолчанию все трассеры невидимы

            if (!playerPhysics.myPlayer.Data.IsDead)        // Игрок жив
            {
                if (CheatToggles.tracersCrew && !playerPhysics.myPlayer.Data.Role.IsImpostor ||   // Член экипажа
                    CheatToggles.tracersImps && playerPhysics.myPlayer.Data.Role.IsImpostor)      // Или предатель
                {
                    if (CheatToggles.distanceBasedTracers)
                    {
                        color = GetDistanceBasedColor(playerPhysics.myPlayer.transform.position); // По расстоянию
                    }
                    else if (CheatToggles.colorBasedTracers)
                    {
                        color = playerPhysics.myPlayer.Data.Color;              // По цвету игрока
                    }
                    else
                    {
                        color = playerPhysics.myPlayer.Data.Role.TeamColor;     // По цвету команды
                    }
                }
            }
            else                                            // Игрок мёртв (призрак)
            {
                if (CheatToggles.tracersGhosts)
                {
                    if (CheatToggles.distanceBasedTracers)
                    {
                        color = GetDistanceBasedColor(playerPhysics.myPlayer.transform.position); // По расстоянию
                    }
                    else if (CheatToggles.colorBasedTracers)
                    {
                        color = playerPhysics.myPlayer.Data.Color;              // По цвету игрока
                    }
                    else
                    {
                        color = Palette.White;                                  // Призрачный трассер (белый)
                    }
                }
            }

            // Рисуем трассер между игроком и LocalPlayer с нужным цветом
            Utils.DrawTracer(playerPhysics.myPlayer.gameObject, PlayerControl.LocalPlayer.gameObject, color);
        } catch { }
    }

    // Рисует трассер от LocalPlayer к мёртвому телу. Рисует трассеры только для неотрепорченных тел.
    public static void DrawBodyTracer(DeadBody deadBody)
    {
        var color = Color.clear; // По умолчанию все трассеры невидимы

        if (CheatToggles.tracersBodies)
        {
            if (CheatToggles.distanceBasedTracers)
            {
                color = GetDistanceBasedColor(deadBody.transform.position);          // По расстоянию
            }
            else if (CheatToggles.colorBasedTracers)
            {
                color = GameData.Instance.GetPlayerById(deadBody.ParentId).Color;    // По цвету игрока
            }
            else
            {
                color = Color.yellow;                                                // Трассер к телу (жёлтый)
            }
        }

        // Рисуем трассер между мёртвым телом и LocalPlayer с нужным цветом
        Utils.DrawTracer(deadBody.gameObject, PlayerControl.LocalPlayer.gameObject, color);
    }

    // Возвращает цвет на основе расстояния между LocalPlayer и целевой позицией.
    // Близкие расстояния — красный, средние — жёлтый, дальние — зелёный.
    private static Color GetDistanceBasedColor(Vector3 targetPosition)
    {
        const float maxDistance = 20f; // Зелёный на 20+ юнитов
        const float minDistance = 2f;  // Красный на 2 юнитах или меньше

        var distance = Vector3.Distance(targetPosition, PlayerControl.LocalPlayer.transform.position);
        var normalized = Mathf.InverseLerp(minDistance, maxDistance, distance);

        // Интерполяция: Красный (близко) -> Жёлтый (средне) -> Зелёный (далеко)
        return normalized < 0.5f
            ? Color.Lerp(Color.red, Color.yellow, normalized * 2f)
            : Color.Lerp(Color.yellow, Color.green, (normalized - 0.5f) * 2f);
    }
}

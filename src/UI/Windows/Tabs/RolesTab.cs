using UnityEngine;

namespace MalumMenu;

public class RolesTab : ITab
{
    public string name => "Роли";        // Название вкладки

    public void Draw()
    {
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();      // Основные

        GUILayout.Space(15);

        DrawImpostor();     // Предатель

        GUILayout.Space(15);

        DrawShapeshifter(); // Оборотень

        GUILayout.Space(15);

        DrawCrewmate();     // Член экипажа

        GUILayout.Space(15);

        DrawTracker();      // Трекер

        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        DrawEngineer();     // Инженер

        GUILayout.Space(15);

        DrawScientist();    // Учёный

        GUILayout.Space(15);

        DrawDetective();    // Детектив

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawGeneral()
    {
        CheatToggles.setFakeRole = GUILayout.Toggle(CheatToggles.setFakeRole, " Установить фейковую роль");

        CheatToggles.setFakeAlive = GUILayout.Toggle(CheatToggles.setFakeAlive, " Установить фейковый статус живого");
    }

    private void DrawImpostor()
    {
        GUILayout.Label("Предатель", GUIStylePreset.TabSubtitle);   // Подзаголовок: "Предатель"

        CheatToggles.killReach = GUILayout.Toggle(CheatToggles.killReach, " Дальность убийства");

        // CheatToggles.impostorTasks = GUILayout.Toggle(CheatToggles.impostorTasks, " Разрешить задачи");
    }

    private void DrawShapeshifter()
    {
        GUILayout.Label("Оборотень", GUIStylePreset.TabSubtitle);   // Подзаголовок: "Оборотень"

        CheatToggles.noShapeshiftAnim = GUILayout.Toggle(CheatToggles.noShapeshiftAnim, " Без анимации превращения");

        CheatToggles.endlessSsDuration = GUILayout.Toggle(CheatToggles.endlessSsDuration, " Бесконечное превращение");
    }

    private void DrawCrewmate()
    {
        GUILayout.Label("Член экипажа", GUIStylePreset.TabSubtitle);   // Подзаголовок: "Член экипажа"

        CheatToggles.showTasksMenu = GUILayout.Toggle(CheatToggles.showTasksMenu, " Показать меню задач");
    }

    private void DrawTracker()
    {
        GUILayout.Label("Трекер", GUIStylePreset.TabSubtitle);   // Подзаголовок: "Трекер"

        CheatToggles.endlessTracking = GUILayout.Toggle(CheatToggles.endlessTracking, " Бесконечное отслеживание");

        CheatToggles.noTrackingDelay = GUILayout.Toggle(CheatToggles.noTrackingDelay, " Без задержки отслеживания");

        CheatToggles.noTrackingCooldown = GUILayout.Toggle(CheatToggles.noTrackingCooldown, " Без перезарядки отслеживания");

        CheatToggles.trackReach = GUILayout.Toggle(CheatToggles.trackReach, " Дальность отслеживания");
    }

    private void DrawEngineer()
    {
        GUILayout.Label("Инженер", GUIStylePreset.TabSubtitle);   // Подзаголовок: "Инженер"

        CheatToggles.endlessVentTime = GUILayout.Toggle(CheatToggles.endlessVentTime, " Бесконечное время в вентиляции");

        CheatToggles.noVentCooldown = GUILayout.Toggle(CheatToggles.noVentCooldown, " Без перезарядки вентиляции");
    }

    private void DrawScientist()
    {
        GUILayout.Label("Учёный", GUIStylePreset.TabSubtitle);   // Подзаголовок: "Учёный"

        CheatToggles.endlessBattery = GUILayout.Toggle(CheatToggles.endlessBattery, " Бесконечная батарея");

        CheatToggles.noVitalsCooldown = GUILayout.Toggle(CheatToggles.noVitalsCooldown, " Без перезарядки виталов");
    }

    private void DrawDetective()
    {
        GUILayout.Label("Детектив", GUIStylePreset.TabSubtitle);   // Подзаголовок: "Детектив"

        CheatToggles.interrogateReach = GUILayout.Toggle(CheatToggles.interrogateReach, " Дальность допроса");
    }
}

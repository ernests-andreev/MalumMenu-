using UnityEngine;

namespace MalumMenu;

public class ESPTab : ITab
{
    public string name => "ESP";        // Название вкладки

    public void Draw()
    {
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();      // Основные

        GUILayout.Space(15);

        DrawCamera();       // Камера

        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        DrawTracers();      // Трассеры

        GUILayout.Space(15);

        DrawMinimap();      // Мини-карта

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawGeneral()
    {
        CheatToggles.seePlayerInfo = GUILayout.Toggle(CheatToggles.seePlayerInfo, " Видеть информацию об игроках");

        CheatToggles.seeRoles = GUILayout.Toggle(CheatToggles.seeRoles, " Видеть роли");

        CheatToggles.seeGhosts = GUILayout.Toggle(CheatToggles.seeGhosts, " Видеть призраков");

        CheatToggles.noShadows = GUILayout.Toggle(CheatToggles.noShadows, " Без теней");

        CheatToggles.taskArrows = GUILayout.Toggle(CheatToggles.taskArrows, " Стрелки к задачам");

        CheatToggles.revealVotes = GUILayout.Toggle(CheatToggles.revealVotes, " Раскрыть голоса");

        CheatToggles.seeLobbyInfo = GUILayout.Toggle(CheatToggles.seeLobbyInfo, " Видеть информацию о лобби");
    }

    private void DrawCamera()
    {
        GUILayout.Label("Камера", GUIStylePreset.TabSubtitle);    // Подзаголовок: "Камера"

        CheatToggles.zoomOut = GUILayout.Toggle(CheatToggles.zoomOut, " Отдалить");

        CheatToggles.spectate = GUILayout.Toggle(CheatToggles.spectate, " Наблюдение");

        CheatToggles.freecam = GUILayout.Toggle(CheatToggles.freecam, " Свободная камера");
    }

    private void DrawTracers()
    {
        GUILayout.Label("Лучи", GUIStylePreset.TabSubtitle);    // Подзаголовок: "Трассеры"

        CheatToggles.tracersCrew = GUILayout.Toggle(CheatToggles.tracersCrew, " Члены экипажа");

        CheatToggles.tracersImps = GUILayout.Toggle(CheatToggles.tracersImps, " Предатели");

        CheatToggles.tracersGhosts = GUILayout.Toggle(CheatToggles.tracersGhosts, " Призраки");

        CheatToggles.tracersBodies = GUILayout.Toggle(CheatToggles.tracersBodies, " Мёртвые тела");

        CheatToggles.colorBasedTracers = GUILayout.Toggle(CheatToggles.colorBasedTracers, " По цвету");

        CheatToggles.distanceBasedTracers = GUILayout.Toggle(CheatToggles.distanceBasedTracers, " По расстоянию");
    }

    private void DrawMinimap()
    {
        GUILayout.Label("Мини-карта", GUIStylePreset.TabSubtitle);    // Подзаголовок: "Мини-карта"

        CheatToggles.mapCrew = GUILayout.Toggle(CheatToggles.mapCrew, " Члены экипажа");

        CheatToggles.mapImps = GUILayout.Toggle(CheatToggles.mapImps, " Предатели");

        CheatToggles.mapGhosts = GUILayout.Toggle(CheatToggles.mapGhosts, " Призраки");

        CheatToggles.colorBasedMap = GUILayout.Toggle(CheatToggles.colorBasedMap, " По цвету");
    }
}

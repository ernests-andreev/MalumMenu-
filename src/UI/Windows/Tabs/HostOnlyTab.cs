using UnityEngine;

namespace MalumMenu;

public class HostOnlyTab : ITab
{
    public string name => "Только для хоста";        // Название вкладки

    public void Draw()
    {
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();          // Основные

        GUILayout.Space(15);

        DrawMurder();           // Убийства

        GUILayout.Space(15);

        DrawGameState();        // Состояние игры

        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        DrawMeetings();         // Собрания

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawGeneral()
    {
        CheatToggles.killVanished = GUILayout.Toggle(CheatToggles.killVanished, " Убивать в невидимости");

        CheatToggles.killAnyone = GUILayout.Toggle(CheatToggles.killAnyone, " Убивать любого");

        CheatToggles.noKillCd = GUILayout.Toggle(CheatToggles.noKillCd, " Без перезарядки убийства");

        CheatToggles.showProtectMenu = GUILayout.Toggle(CheatToggles.showProtectMenu, " Показать меню защиты");

        // CheatToggles.forceRole = GUILayout.Toggle(CheatToggles.forceRole, " Форсировать роль");

        // CheatToggles.noOptionsLimits = GUILayout.Toggle(CheatToggles.noOptionsLimits, " Без ограничений настроек");
    }

    private void DrawMurder()
    {
        GUILayout.Label("Убийства", GUIStylePreset.TabSubtitle);    // Подзаголовок: "Убийства"

        CheatToggles.killPlayer = GUILayout.Toggle(CheatToggles.killPlayer, " Убить игрока");

        CheatToggles.telekillPlayer = GUILayout.Toggle(CheatToggles.telekillPlayer, " Телеубийство игрока");

        CheatToggles.killAllCrew = GUILayout.Toggle(CheatToggles.killAllCrew, " Убить всех членов экипажа");

        CheatToggles.killAllImps = GUILayout.Toggle(CheatToggles.killAllImps, " Убить всех предателей");

        CheatToggles.killAll = GUILayout.Toggle(CheatToggles.killAll, " Убить всех");
    }

    private void DrawGameState()
    {
        GUILayout.Label("Состояние игры", GUIStylePreset.TabSubtitle);    // Подзаголовок: "Состояние игры"

        CheatToggles.forceStartGame = GUILayout.Toggle(CheatToggles.forceStartGame, " Принудительный старт игры");

        CheatToggles.noGameEnd = GUILayout.Toggle(CheatToggles.noGameEnd, " Без завершения игры");
    }

    private void DrawMeetings()
    {
        GUILayout.Label("Собрания", GUIStylePreset.TabSubtitle);    // Подзаголовок: "Собрания"

        CheatToggles.skipMeeting = GUILayout.Toggle(CheatToggles.skipMeeting, " Пропустить собрание");

        CheatToggles.voteImmune = GUILayout.Toggle(CheatToggles.voteImmune, " Иммунитет к голосованию");

        CheatToggles.ejectPlayer = GUILayout.Toggle(CheatToggles.ejectPlayer, " Выкинуть игрока");
    }
}

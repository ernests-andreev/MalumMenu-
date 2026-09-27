using UnityEngine;

namespace MalumMenu;

public class ConsoleTab : ITab
{
    public string name => "Консоль";        // Название вкладки

    public void Draw()
    {
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();      // Основные

        GUILayout.EndVertical();
    }

    private void DrawGeneral()
    {
        CheatToggles.showConsole = GUILayout.Toggle(CheatToggles.showConsole, " Показать консоль");

        CheatToggles.logDeaths = GUILayout.Toggle(CheatToggles.logDeaths, " Логировать смерти");

        CheatToggles.logShapeshifts = GUILayout.Toggle(CheatToggles.logShapeshifts, " Логировать превращения");

        CheatToggles.logVents = GUILayout.Toggle(CheatToggles.logVents, " Логировать вентиляцию");

        CheatToggles.logTasks = GUILayout.Toggle(CheatToggles.logTasks, " Логировать задачи");

        CheatToggles.logGameState  = GUILayout.Toggle(CheatToggles.logGameState, " Логировать состояние игры");
    }
}

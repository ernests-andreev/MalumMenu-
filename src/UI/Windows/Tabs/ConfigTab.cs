using UnityEngine;

namespace MalumMenu;

public class ConfigTab : ITab
{
    public string name => "Конфиг";        // Название вкладки

    public void Draw()
    {
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();      // Основные

        GUILayout.EndVertical();
    }

    private void DrawGeneral()
    {
        CheatToggles.openConfig = GUILayout.Toggle(CheatToggles.openConfig, " Открыть конфиг");

        CheatToggles.reloadConfig = GUILayout.Toggle(CheatToggles.reloadConfig, " Перезагрузить конфиг");

        CheatToggles.saveProfile = GUILayout.Toggle(CheatToggles.saveProfile, " Сохранить в профиль");

        CheatToggles.loadProfile = GUILayout.Toggle(CheatToggles.loadProfile, " Загрузить из профиля");
    }
}

using UnityEngine;

namespace MalumMenu;

public class ChatTab : ITab
{
    public string name => "Чат";        // Название вкладки

    public void Draw()
    {
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();      // Основные

        GUILayout.Space(15);

        DrawTextbox();      // Текстовое поле

        GUILayout.EndVertical();
    }

    private void DrawGeneral()
    {
        CheatToggles.enableChat = GUILayout.Toggle(CheatToggles.enableChat, " Включить чат");

        CheatToggles.bypassUrlBlock = GUILayout.Toggle(CheatToggles.bypassUrlBlock, " Обход блокировки URL");

        CheatToggles.lowerRateLimits = GUILayout.Toggle(CheatToggles.lowerRateLimits, " Снизить ограничения частоты");
    }

    private void DrawTextbox()
    {
        GUILayout.Label("Текстовое поле", GUIStylePreset.TabSubtitle);    // Подзаголовок: "Текстовое поле"

        CheatToggles.unlockCharacters = GUILayout.Toggle(CheatToggles.unlockCharacters, " Разблокировать доп. символы");

        CheatToggles.longerMessages = GUILayout.Toggle(CheatToggles.longerMessages, " Разрешить длинные сообщения");

        CheatToggles.unlockClipboard = GUILayout.Toggle(CheatToggles.unlockClipboard, " Разблокировать буфер обмена");
    }
}

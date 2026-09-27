using UnityEngine;

namespace MalumMenu;

public class AnimationsTab : ITab
{
    public string name => "Анимации";        // Название вкладки

    public void Draw()
    {
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();          // Основные

        GUILayout.Space(15);

        DrawClientSided();      // Клиентские

        GUILayout.EndVertical();
    }

    private void DrawGeneral()
    {
        CheatToggles.animShields = GUILayout.Toggle(CheatToggles.animShields, " Щиты");

        CheatToggles.animAsteroids = GUILayout.Toggle(CheatToggles.animAsteroids, " Астероиды");

        CheatToggles.animEmptyGarbage = GUILayout.Toggle(CheatToggles.animEmptyGarbage, " Выброс мусора");

        CheatToggles.animMedScan = GUILayout.Toggle(CheatToggles.animMedScan, " Сканирование в медотсеке");

        CheatToggles.animCamsInUse = GUILayout.Toggle(CheatToggles.animCamsInUse, " Камеры используются");

        // CheatToggles.animPet = GUILayout.Toggle(CheatToggles.animPet, " Питомец");
    }

    private void DrawClientSided()
    {
        GUILayout.Label("Клиентские", GUIStylePreset.TabSubtitle);    // Подзаголовок: "Клиентские"

        CheatToggles.moonWalk = GUILayout.Toggle(CheatToggles.moonWalk, " Лунная походка");
    }
}

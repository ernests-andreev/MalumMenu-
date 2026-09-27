using UnityEngine;
using Il2CppSystem.Collections.Generic;

namespace MalumMenu;

public class ProtectUI : MonoBehaviour
{
    public static int windowHeight = 300;                               // Высота окна
    public static int windowWidth = 500;                                // Ширина окна
    public static Rect windowRect;                                      // Прямоугольник окна

    private Vector2 _scrollPosition = Vector2.zero;                     // Позиция прокрутки
    public static List<PlayerControl> playersToProtect = new();         // Список игроков под постоянной защитой
    private bool _keepEveryoneProtected;                                // Флаг "держать всех под защитой"

    private void Start()
    {
        // Создаём 2D-область ProtectUI
        windowRect = new(
            Screen.width / 2f - windowWidth / 2f,
            Screen.height / 2f - windowHeight / 2f,
            windowWidth,
            windowHeight
        );
    }

    private void OnGUI()
    {
        if (!CheatToggles.showProtectMenu || !(MenuUI.isGUIActive || MalumMenu.menuKeepSubwindowsOpen.Value) || MalumMenu.isPanicked) return;

        UIHelpers.ApplyUIColor();                 // Применить пользовательский цвет UI

        windowRect = GUI.Window((int)WindowId.ProtectUI, windowRect, (GUI.WindowFunction)ProtectWindow, "Защита игроков");  // Заголовок окна: "Защита игроков"
    }

    private void ProtectWindow(int windowID)
    {
        GUILayout.BeginVertical();

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);   // Область прокрутки

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            // Удаляем невалидных игроков из списка защиты и пропускаем их
            if (!player.Data || !player.Data.Role || string.IsNullOrEmpty(player.Data.PlayerName))
            {
                if (playersToProtect.Contains(player))
                {
                    playersToProtect.Remove(player);
                }

                continue;
            }

            GUILayout.BeginHorizontal();

            // Цветное имя игрока
            GUILayout.Label($"<color=#{ColorUtility.ToHtmlStringRGB(player.Data.Color)}>{player.Data.PlayerName}</color>", GUILayout.Width(140f));

            // Статус защиты игрока
            if (player.protectedByGuardianId == -1)     // Не защищён
            {
                GUILayout.Label("<color=#FF0000>Не защищён</color>", GUILayout.Width(135));
            }
            else                                        // Защищён (показываем, кем)
            {
                NetworkedPlayerInfo guardianInfo = GameData.Instance.GetPlayerById((byte)player.protectedByGuardianId);
                GUILayout.Label($"<color=#00FF00>Защищён</color> игроком <color=#{ColorUtility.ToHtmlStringRGB(guardianInfo.Color)}>{guardianInfo._object.Data.PlayerName}</color>", GUILayout.Width(135));
            }

            // Кнопка "Защитить" (только для хоста вне лобби)
            if (GUILayout.Button("Защитить", GUIStylePreset.NormalButton) && Utils.isHost && !Utils.isLobby)
            {
                PlayerControl.LocalPlayer.RpcProtectPlayer(player, player.cosmetics.ColorId);
            }

            // Переключатель "Держать под защитой"
            var keepProtected = playersToProtect.Contains(player);
            keepProtected = GUILayout.Toggle(keepProtected, "Держать под защитой", GUIStylePreset.NormalToggle);

            // Синхронизация списка защиты с состоянием переключателя
            if (keepProtected && !playersToProtect.Contains(player))
            {
                playersToProtect.Add(player);
            }
            else if (!keepProtected && playersToProtect.Contains(player))
            {
                playersToProtect.Remove(player);
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal();

        // Кнопка "Защитить всех" (только для хоста вне лобби)
        if (GUILayout.Button("Защитить всех") && Utils.isHost && !Utils.isLobby)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                PlayerControl.LocalPlayer.RpcProtectPlayer(player, player.cosmetics.ColorId);
            }
        }

        GUILayout.FlexibleSpace();

        // Переключатель "Держать всех под защитой"
        _keepEveryoneProtected = GUILayout.Toggle(_keepEveryoneProtected, "Держать всех под защитой");

        if (_keepEveryoneProtected)
        {
            // Добавляем всех игроков в список защиты
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (!playersToProtect.Contains(player))
                {
                    playersToProtect.Add(player);
                }
            }
        }
        else
        {
            // Очищаем список только если в нём были все игроки
            if (PlayerControl.AllPlayerControls.Count == playersToProtect.Count)
            {
                playersToProtect.Clear();
            }
        }

        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        GUI.DragWindow();   // Позволяет перетаскивать окно
    }
}

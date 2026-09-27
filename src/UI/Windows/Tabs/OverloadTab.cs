using System;
using UnityEngine;

namespace MalumMenu;

public class OverloadTab : ITab
{
    public string name => "Перегрузка";     // Название вкладки

    private GUIStyle _sliderSubtitle;       // Стиль подписи слайдера
    private int _maxStrength = 100000;      // Максимальная сила
    private float _maxCooldown = 1f;        // Максимальная перезарядка
    private float _fpsEstimate = 0f;        // Оценка FPS
    private float _rawCooldown;             // Сырое значение перезарядки
    private float _rawStrength;             // Сырое значение силы

    public void Draw()
    {
        InitStyles();

        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();              // Основные

        GUILayout.Space(15);

        DrawSettingsToggle();       // Переключатель настроек

        GUILayout.EndVertical();

        if (CheatToggles.showOverloadSettings)      // Секция настроек (если включена)
        {
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(MenuUI.windowWidth * 0.75f));

            DrawSettingsSection();

            GUILayout.EndVertical();
        }
    }

    private void InitStyles()
    {
        if (_sliderSubtitle == null)
        {
            _sliderSubtitle = new(GUIStylePreset.TabSubtitle)
            {
                fontStyle = FontStyle.Normal
            };
        }
    }

    private void DrawGeneral()
    {
        CheatToggles.showOverload = GUILayout.Toggle(CheatToggles.showOverload, " Показать меню перегрузки");
    }

    private void DrawSettingsToggle()
    {
        GUILayout.Label("Настройки", GUIStylePreset.TabSubtitle);    // Подзаголовок: "Настройки"

        CheatToggles.showOverloadSettings = GUILayout.Toggle(CheatToggles.showOverloadSettings, " Показать настройки перегрузки");
    }

    private void DrawSettingsSection()
    {
        GUILayout.Space(15);

        GUILayout.BeginHorizontal();

        GUILayout.Space(10);

        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();

        CheatToggles.olAutoAdapt = GUILayout.Toggle(CheatToggles.olAutoAdapt, " Авто-адаптация");

        int ping = Utils.GetPing();
        string pingStr = $"ПИНГ : {ping} мс";
        GUILayout.Label(Utils.GetColoredPingText(pingStr, ping));    // Цветной текст пинга

        int strength = OverloadHandler.strength;
        float cooldown = OverloadHandler.cooldown;

        float numExecutionsPerSec;
        string extraStr = "";

        if (cooldown > Time.unscaledDeltaTime) // Число выполнений/сек будет ниже FPS
        {
            numExecutionsPerSec = 1f / cooldown;
        }
        else // Число выполнений/сек будет выше FPS
        {
            // FPS слишком часто колеблется, поэтому обновляем только при значительном отклонении (> 5)

            float fps = Utils.GetFps();
            if (Math.Abs(fps - _fpsEstimate) > 5f)
            {
                _fpsEstimate = fps;
            }

            numExecutionsPerSec = (int)_fpsEstimate; // Число выполнений/сек ограничено FPS независимо от перезарядки
            extraStr = " (Ограничение FPS)";
        }

        int numTargetsPerSec = OverloadUI.currentTargets.Count <= numExecutionsPerSec ? OverloadUI.currentTargets.Count : (int)numExecutionsPerSec; // Целей/сек ограничено числом выполнений/сек

        int rpcPerTarget = numTargetsPerSec > 0 ? (int)(strength * numExecutionsPerSec / numTargetsPerSec) :
                                            (int)(strength * numExecutionsPerSec);

        string rpcStr = CheatToggles.olShowRpcTotal
                        ? $"{rpcPerTarget*Math.Max(1, numTargetsPerSec)}"
                        : $"{rpcPerTarget}x{numTargetsPerSec}";

        CheatToggles.olShowRpcTotal = GUILayout.Toggle(CheatToggles.olShowRpcTotal, $" RPC/с : {rpcStr}{extraStr}");

        GUILayout.EndHorizontal();

        GUILayout.Space(15);

        DrawSettingsSliders();      // Слайдеры настроек

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();

        GUILayout.Space(10);

        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.35f));

        GUILayout.Label("Общие", GUIStylePreset.TabSubtitle);    // Подзаголовок: "Общие"

        CheatToggles.olAutoStart = GUILayout.Toggle(CheatToggles.olAutoStart, " Авто-старт при готовности");

        CheatToggles.olAutoStop = GUILayout.Toggle(CheatToggles.olAutoStop, " Авто-стоп при завершении");

        CheatToggles.olLockTargets = GUILayout.Toggle(CheatToggles.olLockTargets, " Заблокировать цели при старте");

        CheatToggles.olKillSwitch = GUILayout.Toggle(CheatToggles.olKillSwitch, " Kill Switch при лагах");

        if (CheatToggles.olKillSwitch)
        {
            Color standardBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.red;

            // Кнопка порога kill switch: "500 ms", "1000 ms" и т.д.
            bool isPressed = GUILayout.Button($"{OverloadUI.killSwitchThreshold} мс", GUILayout.Width(70f));
            if (isPressed)
            {
                if (OverloadUI.killSwitchThreshold >= 3000) // Макс. KS = 3000 мс
                {
                    OverloadUI.killSwitchThreshold = 500; // Мин. KS = 500 мс
                }
                else
                {
                    OverloadUI.killSwitchThreshold = OverloadUI.killSwitchThreshold + 500; // Шаг 500 мс
                }
            }

            GUI.backgroundColor = standardBackgroundColor;
        }

        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        GUILayout.Label("Логи", GUIStylePreset.TabSubtitle);    // Подзаголовок: "Логи"

        CheatToggles.olLogStartStop = GUILayout.Toggle(CheatToggles.olLogStartStop, " Логировать СТАРТ и СТОП");

        CheatToggles.olLogAddRemove = GUILayout.Toggle(CheatToggles.olLogAddRemove, " Логировать ДОБАВЛЕНИЕ и УДАЛЕНИЕ");

        CheatToggles.olLogAttack = GUILayout.Toggle(CheatToggles.olLogAttack, " Логировать атаку");

        CheatToggles.olLogDisconnect = GUILayout.Toggle(CheatToggles.olLogDisconnect, " Логировать отключения");

        CheatToggles.olVerboseLogs = GUILayout.Toggle(CheatToggles.olVerboseLogs, " Подробные логи атаки");

        CheatToggles.olAutoClear = GUILayout.Toggle(CheatToggles.olAutoClear, " Авто-очистка при старте");

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        GUILayout.Space(15);
    }

    private void DrawSettingsSliders()
    {
        GUILayout.Label($"Сила : {_rawStrength}", _sliderSubtitle);    // Подпись слайдера силы

        GUILayout.Space(1);

        GUILayout.BeginHorizontal();

        float inputStrength = GUILayout.HorizontalSlider(_rawStrength, 1, _maxStrength, GUILayout.Width(350f));

        if (inputStrength != _rawStrength)
        {
            CheatToggles.olAutoAdapt = false; // Отключаем авто-адаптацию при ручном вводе
            _rawStrength = inputStrength;
        }

        GUILayout.Space(5);

        string maxStrengthStr = _maxStrength % 1000 == 0 ? $"{_maxStrength / 1000}K" : $"{_maxStrength}";
        bool isPressedMaxStrength = GUILayout.Button(maxStrengthStr, GUILayout.Width(51f));

        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.Label($"Перезарядка : {_rawCooldown:F2}", _sliderSubtitle);    // Подпись слайдера перезарядки

        GUILayout.Space(1);

        GUILayout.BeginHorizontal();

        float inputCooldown = GUILayout.HorizontalSlider(_rawCooldown, 0f, _maxCooldown, GUILayout.Width(350f));

        if (inputCooldown != _rawCooldown)
        {
            CheatToggles.olAutoAdapt = false; // Отключаем авто-адаптацию при ручном вводе
            _rawCooldown = inputCooldown;
        }

        GUILayout.Space(5);

        bool isPressedMaxCooldown = GUILayout.Button($"{_maxCooldown:F0}", GUILayout.Width(51f));

        GUILayout.EndHorizontal();

        if (!CheatToggles.olAutoAdapt)      // Если авто-адаптация выключена — применяем ручные значения
        {
            float strengthStep = _maxStrength / 100f; // Шаги слайдера = 1/100 от максимума силы
            int clampStrength = Mathf.RoundToInt(Mathf.Clamp(Mathf.Round(_rawStrength / strengthStep) * strengthStep, 1, _maxStrength));
            OverloadHandler.strength = clampStrength;

            float cooldownStep = _maxCooldown / 100f; // Шаги слайдера = 1/100 от максимума перезарядки
            float clampCooldown = Mathf.Round(_rawCooldown / cooldownStep) * cooldownStep;
            OverloadHandler.cooldown = clampCooldown;
        }

        // Корректируем границы, чтобы слайдеры никогда не выходили за пределы

        while (_maxStrength < OverloadHandler.strength)
        {
            _maxStrength *= 10;
        }

        while (_maxCooldown < OverloadHandler.cooldown)
        {
            _maxCooldown *= 10;
        }

        if (isPressedMaxStrength)
        {
            if (_maxStrength >= 100000) // Макс. _maxStrength = 100K RPC
            {
                CheatToggles.olAutoAdapt = false; // Отключаем авто-адаптацию при ручном вводе

                OverloadHandler.strength = Mathf.RoundToInt(OverloadHandler.strength/1000f); // Корректируем значение под изменение максимума (÷1000)

                _maxStrength = 100; // Мин. _maxStrength = 100 RPC
            }
            else
            {
                CheatToggles.olAutoAdapt = false; // Отключаем авто-адаптацию при ручном вводе

                OverloadHandler.strength *= 10; // Корректируем значение под изменение максимума (x10)

                _maxStrength *= 10; // Шаг x10
            }
        }

        if (isPressedMaxCooldown)
        {
            if (_maxCooldown >= 10f) // Макс. _maxCooldown = 10с
            {
                CheatToggles.olAutoAdapt = false; // Отключаем авто-адаптацию при ручном вводе

                OverloadHandler.cooldown /= 10f; // Корректируем значение под изменение максимума (÷10)

                _maxCooldown = 1f; // Мин. _maxCooldown = 1с
            }
            else
            {
                CheatToggles.olAutoAdapt = false; // Отключаем авто-адаптацию при ручном вводе

                OverloadHandler.cooldown *= 10; // Корректируем значение под изменение максимума (x10)

                _maxCooldown *= 10; // Шаг x10
            }
        }

        // Обновляем значения слайдеров под актуальные значения

        _rawStrength = OverloadHandler.strength;
        _rawCooldown = OverloadHandler.cooldown;
    }
}

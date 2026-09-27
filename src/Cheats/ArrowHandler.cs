using UnityEngine;
using System.Linq;

namespace MalumMenu;

public static class ArrowHandler
{
    // Кэш шаблона GameObject стрелки для клонирования
    private static GameObject _cachedArrowTemplate;

    // Определяет, принадлежит ли задача LocalPlayer и не выполнена ли она
    public static bool IsOwnedAndIncomplete(NormalPlayerTask task)
    {
        if (task.Owner == null || !task.Owner.AmOwner) return false;

        return !task.IsComplete;
    }

    // Ищет префабы задач в ShipStatus, чтобы закэшировать первый найденный GameObject стрелки
    private static void CacheArrowFromShipStatus()
    {
        if (_cachedArrowTemplate != null) return;

        NormalPlayerTask[][] allTasksArrays = new NormalPlayerTask[][]
        {
            ShipStatus.Instance.CommonTasks,
            ShipStatus.Instance.LongTasks,
            ShipStatus.Instance.ShortTasks
        };

        foreach (var tasks in allTasksArrays)
        {
            foreach (var task in tasks)
            {
                if (task.Arrow != null)
                {
                    _cachedArrowTemplate = task.Arrow.gameObject;
                    MalumMenu.Log.LogInfo($"Закэширован Arrow.gameObject для задачи {task.TaskType}");
                    return;
                }
                MalumMenu.Log.LogInfo($"Не найден Arrow.gameObject для задачи {task.TaskType}");
            }
        }
    }

    // Создаёт новый ArrowBehaviour для задачи, у которой его нет
    public static ArrowBehaviour CreateArrowForTask(NormalPlayerTask task)
    {
        // Кэшируем GameObject стрелки из префабов задач ShipStatus, если он отсутствует
        CacheArrowFromShipStatus();

        // Устанавливаем task.transform как родителя arrowObj, чтобы стрелка уничтожалась вместе с задачей
        var arrowObj = Object.Instantiate(_cachedArrowTemplate, task.transform, false);

        return arrowObj.GetComponent<ArrowBehaviour>();
    }

    // Гарантирует, что у задачи есть стрелка, создавая её при необходимости
    public static void EnsureArrowExists(NormalPlayerTask task)
    {
        // Создаём стрелки только для собственных, невыполненных задач, у которых её ещё нет
        if (!IsOwnedAndIncomplete(task) || task.Arrow != null) return;

        task.Arrow = CreateArrowForTask(task);
    }

    // Проверяет, нужна ли задаче специальная обработка для установки цели стрелки
    // Некоторые задачи (например, ReplaceParts) имеют логику, предполагающую taskStep > 0
    public static bool NeedsSpecialTarget(NormalPlayerTask task)
    {
        return task.TaskType is TaskTypes.AlignEngineOutput or TaskTypes.ReplaceParts or TaskTypes.RoastMarshmallow or TaskTypes.StartFans or TaskTypes.PickUpTowels;
    }

    // Устанавливает цель стрелки и стартовую комнату для задачи и консоли
    private static void SetArrowTarget(NormalPlayerTask task, Console targetConsole)
    {
        if (targetConsole == null) return;

        task.Arrow.target = targetConsole.transform.position;
        task.StartAt = targetConsole.Room;
    }

    // Устанавливает цель стрелки для задач со специальной логикой при TaskStep == 0
    // Нацеливает каждую специальную задачу с логикой по случаю
    public static void SetArrowTargetForSpecialTasks(NormalPlayerTask task)
    {
        if (task.Arrow == null) return;

        switch (task.TaskType)
        {
            // AlignEngineOutput: на шаге 0 стрелка всегда указывает на актуальную консоль (панель Upper Engine)
            case TaskTypes.AlignEngineOutput when task.TaskStep == 0:
            {
                Il2CppSystem.Collections.Generic.List<Console> consoles = task.FindConsoles();

                if (consoles is { Count: > 0 })
                {
                    SetArrowTarget(task, consoles[0]);
                }

                break;
            }
            // ReplaceParts: на шаге 0 стрелка всегда указывает на актуальную консоль (панель Collect Parts)
            case TaskTypes.ReplaceParts when task.taskStep == 0:
            {
                Il2CppSystem.Collections.Generic.List<Console> consoles = NormalPlayerTask.PickRandomConsoles(0, TaskTypes.ReplaceParts);

                if (consoles is { Count: > 0 })
                {
                    var firstConsole = consoles.ToArray().FirstOrDefault(c => c.ConsoleId == task.Data[0]);
                    SetArrowTarget(task, firstConsole);
                }

                break;
            }
            // RoastMarshmallow: на шаге 0 стрелка всегда указывает на актуальную консоль (панель Collect Stick)
            case TaskTypes.RoastMarshmallow when task.taskStep == 0:
            {
                Il2CppSystem.Collections.Generic.List<Console> consoles = NormalPlayerTask.PickRandomConsoles(0, TaskTypes.RoastMarshmallow);

                if (consoles is { Count: > 0 })
                {
                    var stickConsole = consoles.ToArray().FirstOrDefault(c => c.ConsoleId == task.Data[0]);
                    SetArrowTarget(task, stickConsole);
                }

                break;
            }
            // StartFans: на шаге 0 стрелка всегда указывает на актуальную консоль (панель Reveal Code)
            case TaskTypes.StartFans when task.taskStep == 0:
            {
                var targetConsole = task.FindSpecialConsole((Il2CppSystem.Func<Console, bool>)((Console c) => task.ValidConsole(c) && c.ConsoleId == 0));
                SetArrowTarget(task, targetConsole);

                break;
            }
            // PickUpTowels: на шаге 0 стрелка всегда указывает на любое валидное место с полотенцами
            case TaskTypes.PickUpTowels when task.TaskStep == 0:
            {
                var targetConsole = task.FindSpecialConsole((Il2CppSystem.Func<Console, bool>)((Console c) => task.ValidConsole(c)));
                SetArrowTarget(task, targetConsole);

                break;
            }
        }
    }
}

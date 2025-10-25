using System;
using UnityEngine;

namespace Test.SimpleLevels
{
    /// Механизм работы менеджера уровней
    /// Для получения уровней используется понятие Генератора
    /// Генератор - это класс, который отвечает за создание уровней, наследуемый от абстрактного класса
    /// Типы Генераторов и варианты настроек их запуска подсасываются из настроек менеджера
    ///
    /// Задачи этого класса:
    /// Запуск Генераторов по их идентификаторам с набором входных параметров для них (параметры нужны для внутрянки генераторов)
    ///     Запуск в т.ч. возможен в редакторе
    /// Сохранение текущего состояния уровня (положение зданий, спавнеров и т.д.). Сохраняются метаданные объектов, а не сцена вцелом.
    public class SimpleLevelManager
    {
        public delegate void LevelCompletedDelegate();

        public event LevelCompletedDelegate OnLevelCompleted; 
    }

    // Настройки менеджера уровней
    public class SimpleLevelManagerSettings : ScriptableObject
    {
    }

    // Абстрактный класс для генераторов уровней
    public abstract class LevelGeneratorBase
    {
    }

    // Генерирует простую арену, окруженную домиками
    public class LevelGenCityArena : LevelGeneratorBase
    {
    }

    // Настройки генератора (абстрактные)
    public abstract class LevelGenSettings : ScriptableObject
    {
    }

    // Настройки генератора арены в городе
    public class LevelGenCityArenaSettings : LevelGenSettings
    {
    }

    // Класс для управления последовательностью уровней
    public abstract class LevelSequenceBase : MonoBehaviour
    {
        private SimpleLevelManager _levelManager;

        private void Start()
        {
            _levelManager = new SimpleLevelManager();
        }

        protected abstract LevelEntry GetNextLevelEntry();
    }

    [Serializable]
    public struct LevelEntry
    {
        public string levelId;
        public LevelGenSettings genSettings;
    }

    // Последовательность для тестирования
    public class LevelSequenceTest : LevelSequenceBase
    {
        [SerializeField]
        private LevelEntry[] _levels;

        private int _currentLevelIndex = 0;

        protected override LevelEntry GetNextLevelEntry()
        {
            if (_levels.Length == 0)
            {
                throw new Exception("No levels defined in LevelSequenceTest");
            }

            LevelEntry entry = _levels[_currentLevelIndex];
            _currentLevelIndex = (_currentLevelIndex + 1) % _levels.Length;

            return entry;
        }
    }
}
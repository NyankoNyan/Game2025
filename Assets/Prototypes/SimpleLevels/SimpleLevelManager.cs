using System;
using UnityEngine;

namespace Test.SimpleLevels
{
    /// �������� ������ ��������� �������
    /// ��� ��������� ������� ������������ ������� ����������
    /// ��������� - ��� �����, ������� �������� �� �������� �������, ����������� �� ������������ ������
    /// ���� ����������� � �������� �������� �� ������� ������������� �� �������� ���������
    ///
    /// ������ ����� ������:
    /// ������ ����������� �� �� ��������������� � ������� ������� ���������� ��� ��� (��������� ����� ��� ��������� �����������)
    ///     ������ � �.�. �������� � ���������
    /// ���������� �������� ��������� ������ (��������� ������, ��������� � �.�.). ����������� ���������� ��������, � �� ����� ������.
    public class SimpleLevelManager
    {
        public delegate void LevelCompletedDelegate();

        public event LevelCompletedDelegate OnLevelCompleted; 
    }

    // ��������� ��������� �������
    public class SimpleLevelManagerSettings : ScriptableObject
    {
    }

    // ����������� ����� ��� ����������� �������
    public abstract class LevelGeneratorBase
    {
    }

    // ���������� ������� �����, ���������� ��������
    public class LevelGenCityArena : LevelGeneratorBase
    {
    }

    // ��������� ���������� (�����������)
    public abstract class LevelGenSettings : ScriptableObject
    {
    }

    // ��������� ���������� ����� � ������
    public class LevelGenCityArenaSettings : LevelGenSettings
    {
    }

    // ����� ��� ���������� ������������������� �������
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

    // ������������������ ��� ������������
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
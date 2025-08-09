using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Управляет маппингом между идентификаторами блоков и их префабами.
/// </summary>
public class BlockPrefabManager : MonoBehaviour
{
    #region Singleton
    private static BlockPrefabManager _instance;
    public static BlockPrefabManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<BlockPrefabManager>();
                if (_instance == null)
                {
                    var go = new GameObject("BlockPrefabManager");
                    _instance = go.AddComponent<BlockPrefabManager>();
                }
            }
            return _instance;
        }
    }
    #endregion

    [SerializeField]
    private BlockMapping[] _blockMappings;

    [SerializeField]
    private GameObject _defaultPrefab;

    private Dictionary<string, BlockEntry> _mappingDictionary;

    private void Start()
    {
        Initialize();
    }

    /// <summary>
    /// Инициализирует маппинг из ScriptableObject.
    /// </summary>
    public void Initialize()
    {
        _mappingDictionary = new Dictionary<string, BlockEntry>();
        foreach (var mapping in _blockMappings)
        {
            foreach (var entry in mapping.Mappings)
            {
                _mappingDictionary[entry.blockId] = entry;
            }
        }
        if (!_defaultPrefab)
        {
            Debug.LogWarning("_defaultPrefab не установлен. Используется префаб по умолчанию.");
        }
    }

    /// <summary>
    /// Получает префаб по идентификатору блока.
    /// </summary>
    public GameObject GetPrefab(string blockId, bool isDamaged = false)
    {
        if (!_mappingDictionary.TryGetValue(blockId, out var entry))
        {
            Debug.LogError($"Префаб для блока '{blockId}' не найден.");
            return _defaultPrefab;
        }

        return isDamaged ? entry.damagedPrefab : entry.intactPrefab;
    }

    /// <summary>
    /// Прямая загрузка маппинга для тестирования (без использования ScriptableObject).
    /// </summary>
    public void LoadMappingManually(Dictionary<string, BlockEntry> manualMapping)
    {
        _mappingDictionary = manualMapping;
    }
}

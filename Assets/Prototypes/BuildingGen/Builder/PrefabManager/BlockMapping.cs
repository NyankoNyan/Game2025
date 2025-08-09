using UnityEngine;

/// <summary>
/// Связывает идентификаторы блоков с их префабами.
/// </summary>
[CreateAssetMenu(fileName = "NewBlockMapping", menuName = "BuildingGen/Block Mapping")]
public class BlockMapping : ScriptableObject
{
    [SerializeField]
    private BlockEntry[] _mappings;

    public BlockEntry[] Mappings => _mappings;
}

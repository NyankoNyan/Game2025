using System;
using UnityEngine;

[Serializable]
public struct BlockEntry
{
    public string blockId; // Идентификатор блока
    public GameObject intactPrefab; // Целый префаб
    public GameObject damagedPrefab; // Поврежденный префаб
}

using NN;
using UnityEngine;

public class SinglePlayerInit : MonoBehaviour
{
    [SerializeField]
    private MoveController _moveController;

    private void Start()
    {
        _moveController.Init();
    }
}
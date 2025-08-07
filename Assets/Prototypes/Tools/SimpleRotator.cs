using UnityEngine;

public class SimpleRotator : MonoBehaviour
{
    [SerializeField]
    float _speed = 20f;
    void Update()
    {
        transform.Rotate( Vector3.up * _speed * Time.deltaTime );
    }
}

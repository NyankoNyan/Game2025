using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShootTest : MonoBehaviour
{
    [SerializeField]
    private InputAction _actionShoot;

    [SerializeField]
    private Transform _shootPoint;

    [SerializeField]
    private float _shootForce = 10;

    private Rigidbody _rigidBody;

    private void OnEnable()
    {
        _actionShoot.Enable();
        _actionShoot.performed += OnShootAction;
    }

    private void OnDisable()
    {
        _actionShoot.Disable();
        _actionShoot.performed -= OnShootAction;
    }

    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }

    private void OnShootAction(InputAction.CallbackContext obj)
    {
        MakeShoot();
    }

    private void MakeShoot()
    {
        _rigidBody.AddForceAtPosition(-_shootPoint.forward * _shootForce, _shootPoint.position, ForceMode.Impulse);
    }
}
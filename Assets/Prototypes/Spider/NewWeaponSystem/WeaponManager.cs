using UnityEngine;
using UnityEngine.InputSystem;

namespace NewWeaponSystem
{
    [RequireComponent(typeof(GunChanger))]
    public class WeaponManager : MonoBehaviour
    {
        [SerializeField]
        private InputActionReference _nextAction;

        [SerializeField]
        private InputActionReference _prevAction;

        private GunChanger _gunChanger;

        private void Awake()
        {
            _gunChanger = GetComponent<GunChanger>();
            _nextAction.action.Enable();
            _prevAction.action.Enable();

            _nextAction.action.performed += (_) => _gunChanger.SetNextGun();
            _prevAction.action.performed += (_) => _gunChanger.SetPrevGun();
        }
    }
}
using NN;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

[RequireComponent( typeof( MoveController ) )]
[RequireComponent( typeof( Animator ) )]
public class SpiderActionsController : MonoBehaviour
{
    [SerializeField]
    private Rig _rig;

    [SerializeField]
    private InputActionReference _ballAction;

    [SerializeField]
    private float _rigTransitionTime = .2f;

    [SerializeField]
    private float _balledAnimationDuration = 0.375f;

    private bool _ballInProgress = false;
    private MoveController _moveController;
    private Animator _animator;

    private void Awake()
    {
        _moveController = GetComponent<MoveController>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (_ballAction != null && _ballAction.action.IsPressed() && !_ballInProgress)
        {
            StartCoroutine( BallCoroutine() );
        }
    }

    private void OnEnable()
    {
        if (_ballAction != null)
        {
            _ballAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (_ballAction != null)
        {
            _ballAction.action.Disable();
        }
    }

    private IEnumerator BallCoroutine()
    {
        _ballInProgress = true;
        _moveController.MoveActive = false;

        float time = 0;
        _animator.SetBool( "Balled", true );

        while (time < _balledAnimationDuration)
        {
            _rig.weight = 1f - Mathf.Clamp01( time / _rigTransitionTime );
            time += Time.deltaTime;
            yield return null;
        }

        while (_ballAction.action.IsPressed())
        {
            yield return null;
        }

        time = 0;
        _animator.SetBool( "Balled", false );
        while (time < _balledAnimationDuration)
        {
            _rig.weight = 1f - Mathf.Clamp01( (_balledAnimationDuration - time) / _rigTransitionTime );
            time += Time.deltaTime;
            yield return null;
        }

        _ballInProgress = false;
        _moveController.MoveActive = true;
    }
}
using System.Collections;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class TopTopQuadroModel : MonoBehaviour
{
    [Header( "Rig links" )]
    [SerializeField]
    private Transform _rigBody;

    [SerializeField]
    private Transform[] _legRigPoints = new Transform[4];

    [Header( "Distance settings" )]
    [SerializeField]
    private float _legMoveLimit = 1f;

    [SerializeField, Range( 0, .9f )]
    private float _legMoveHotZone = .5f;

    [SerializeField]
    private float _predictionDistance = .2f;

    [SerializeField]
    [Tooltip( "Check ground ray/sphere will cast from x to y of up axis with 0 in leg point" )]
    private Vector2 _groundCheckDistance = new Vector2( 1, -1 );

    [SerializeField]
    private LayerMask _groundLayers = ~0;

    [SerializeField]
    [Tooltip( "Raycast shpere raduis. When 0, replaces with raycast" )]
    private float _groundCheckSphereRadius = 0;

    [Header( "Movement" )]
    [SerializeField]
    private float _stepHeight = .3f;

    [SerializeField]
    private float _stepTime = .2f;

    [Header( "Others" )]
    [SerializeField]
    private bool _drawDebugGizmos = false;

    private Vector3[] _legBodyOffsets;
    private Vector3[] _legPositions;
    private Transform[] _legAnchors;
    private bool[] _locks;
    private int _movedLegsCount = 0;

    private void Start()
    {
        Setup();
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < _legRigPoints.Length; i++)
        {
            CheckLegPlacement( i );
        }
    }

    private void Update()
    {
        for (int i = 0; i < _legRigPoints.Length; i++)
        {
            UpdateLegRig( i );
        }
    }

    private void Setup()
    {
        _legPositions = new Vector3[_legRigPoints.Length];
        _legBodyOffsets = new Vector3[_legRigPoints.Length];
        _legAnchors = new Transform[_legRigPoints.Length];
        _locks = new bool[_legRigPoints.Length];

        for (int i = 0; i < _legRigPoints.Length; i++)
        {
            _legBodyOffsets[i] = _rigBody.worldToLocalMatrix.MultiplyPoint( _legRigPoints[i].position );
            SetupLegPosition( i );
        }

        transform.parent = null;
    }

    private Vector3 GetCurrentWorldPlace(int legId)
    {
        var anchor = _legAnchors[legId];
        if (anchor == null)
            return _legPositions[legId];
        else
            return anchor.localToWorldMatrix.MultiplyPoint( _legPositions[legId] );
    }

    private void OnDrawGizmos()
    {
        if (_drawDebugGizmos && Application.isPlaying)
        {
            for (int i = 0; i < _legRigPoints.Length; i++)
            {
                Vector3 worldPosition = GetCurrentWorldPlace( i );
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube( worldPosition, Vector3.one * .2f );

                Vector3 worldLegBase = _rigBody.localToWorldMatrix.MultiplyPoint( _legBodyOffsets[i] );
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube( worldLegBase, Vector3.one * .2f );
            }
        }
    }

    private void CheckLegPlacement(int legId)
    {
        if (_locks[legId])
            return;

        Vector3 worldPosition = GetCurrentWorldPlace( legId );
        Vector3 worldLegBase = _rigBody.localToWorldMatrix.MultiplyPoint( _legBodyOffsets[legId] );

        // „ем больше ног оторвано от земли, тем больше должно быть рассто€ние, чтобы оторвать следующую ногу.
        float moveLimitWithHot = _legMoveLimit * (1f - _legMoveHotZone * (1f - _movedLegsCount / (float)(_legRigPoints.Length - 1)));

        if (Vector3.ProjectOnPlane( worldLegBase - worldPosition, _rigBody.up ).sqrMagnitude > moveLimitWithHot * moveLimitWithHot)
        {
            StartCoroutine( MoveLegCoroutine( legId ) );
        }
    }

    private IEnumerator MoveLegCoroutine(int legId)
    {
        _locks[legId] = true;
        _movedLegsCount++;
        Vector3 targetLegPosition = _legBodyOffsets[legId];
        Vector3 sourceLegPosition = _rigBody.worldToLocalMatrix.MultiplyPoint( GetCurrentWorldPlace( legId ) );
        targetLegPosition += (targetLegPosition - sourceLegPosition).normalized * _predictionDistance;
        Transform legPoint = _legRigPoints[legId];
        _legAnchors[legId] = null;
        float timeLeft = _stepTime;
        while (timeLeft > 0)
        {
            timeLeft = Mathf.Max( timeLeft - Time.deltaTime, 0 );
            float pathPart = 1f - timeLeft / _stepTime;
            float tSmooth = Mathf.SmoothStep( 0, 1, pathPart );
            Vector3 localLegPosition = Vector3.Lerp( sourceLegPosition, targetLegPosition, tSmooth );
            Vector3 worldLegPosition = _rigBody.localToWorldMatrix.MultiplyPoint( localLegPosition );
            float height = Mathf.Sin( tSmooth * Mathf.PI ) * _stepHeight;

            var groundInfo = GetGroundHitInfo( worldLegPosition, legPoint.up );
            if (groundInfo.Transform)
            {
                legPoint.position = groundInfo.Position + height * legPoint.up;
            } else
            {
                legPoint.position = worldLegPosition + height * legPoint.up;
            }

            yield return null;
        }

        SetupLegPosition( legId );

        _movedLegsCount--;
        _locks[legId] = false;
    }

    private struct GroundHitInfo
    {
        public Transform Transform;
        public Vector3 Position;
    }

    private void SetupLegPosition(int legId)
    {
        Transform legPoint = _legRigPoints[legId];
        var groundInfo = GetGroundHitInfo( legPoint.position, legPoint.up );
        _legAnchors[legId] = groundInfo.Transform;
        if (groundInfo.Transform)
        {
            _legPositions[legId] = groundInfo.Transform.worldToLocalMatrix.MultiplyPoint( legPoint.position );
        } else
        {
            _legPositions[legId] = legPoint.position;
        }
    }

    private GroundHitInfo GetGroundHitInfo(Vector3 position, Vector3 upDir)
    {
        if (_groundCheckSphereRadius > 0)
        {
            if (Physics.SphereCast( position + upDir * _groundCheckDistance.x,
                                  _groundCheckSphereRadius,
                                  -upDir,
                                  out var hitInfo,
                                  Mathf.Abs( _groundCheckDistance.x - _groundCheckDistance.y ),
                                  _groundLayers,
                                  QueryTriggerInteraction.Ignore ))
            {
                return new() { Transform = hitInfo.collider.transform, Position = hitInfo.point };
            }
        } else
        {
            if (Physics.Raycast( position + upDir * _groundCheckDistance.x,
                               -upDir,
                               out var hitInfo,
                               Mathf.Abs( _groundCheckDistance.x - _groundCheckDistance.y ),
                               _groundLayers,
                               QueryTriggerInteraction.Ignore ))
            {
                return new() { Transform = hitInfo.collider.transform, Position = hitInfo.point };
            }
        }
        return default;
    }

    private void UpdateLegRig(int legId)
    {
        if (!_locks[legId])
            _legRigPoints[legId].position = GetCurrentWorldPlace( legId );
    }
}
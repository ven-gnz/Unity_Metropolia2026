using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Swipe : MonoBehaviour
{

    [SerializeField] private ParticleSystem _particles;
    [SerializeField] private float _lifetime = 1.0f;
    

    private float dps = 10.0f; // high DPS to combat the short lifetime. A normalization for 0.2s lifetime -
                               // so torch deals 1 damage killing smallest enemies

    private float _radius = 3.0f; // DAMAGE
    private float _angle = 120.0f;

    private bool _forward;
    private float _elapsed;
    public Vector2 SwipePosition { get; private set; } // for sampling the particles
    private Vector2 _origin; // player position
    private Vector2 _lookDirection;

    float _lowerAngle;
    float _upperAngle;

    private Vector2 _lowerBoundary;
    private Vector2 _upperBoundary;

    private float _halfAngleDegrees;
    private float _halfAngleRadians;

    public void Initialize(
    Vector2 origin,
    Vector2 lookDirection,
    bool forward)
    {
        _origin = origin;
        _lookDirection = lookDirection.normalized;
        _forward = forward;
        _elapsed = 0f;

        _halfAngleDegrees = _angle * 0.5f;
        _halfAngleRadians = _halfAngleDegrees * Mathf.Deg2Rad;

        Vector2 perpendicular = new Vector2(
           -_lookDirection.y,
           _lookDirection.x
       );

        float cos = Mathf.Cos(_halfAngleRadians);
        float sin = Mathf.Sin(_halfAngleRadians);

        _lowerBoundary =
            _lookDirection * cos -
            perpendicular * sin;

        _upperBoundary =
            _lookDirection * cos +
            perpendicular * sin;

        SwipePosition = GetPointOnArc(0f);
        transform.position = SwipePosition;

        if (_particles != null)
        {
            _particles.Play();
        }     
    }


    void Start()
    {

      
        Destroy(gameObject, _lifetime);
    }

    // Update is called once per frame
    void Update()
    {

        _elapsed += Time.deltaTime;

        float progress = Mathf.Clamp01(
            _elapsed / _lifetime
        );

        if (!_forward)
        {
            progress = 1f - progress;
        }

        SwipePosition = GetPointOnArc(progress);
        transform.position = SwipePosition;
        foreach (GameObject enemy in EnemyManager.Instance.GetEnemiesForSwipe(_origin))
        {
            if (IsInArc(enemy.transform.position))
            {
                if (enemy.TryGetComponent(out EntityHealth entityHealth))
                {
                    entityHealth.LoseHealth(dps * Time.deltaTime);
                }
            }
        }

    }


    private bool IsInArc(Vector2 worldPosition)
    {
        
        Vector2 toEnemy =
            worldPosition - _origin;

        if (toEnemy.sqrMagnitude > _radius * _radius)
        {
            return false;
        }

        if(toEnemy.sqrMagnitude < 0.001f)
        {
            return true;
        }

        float enemyAngle = Mathf.Atan2(
            toEnemy.y,
            toEnemy.x
        );

        toEnemy.Normalize();

        float angleDifference =
            Mathf.Abs(Vector2.SignedAngle(_lookDirection, toEnemy));

        return angleDifference <= _halfAngleDegrees;
    }



    private Vector2 GetPointOnArc(float progress)
    {

        float angle = Mathf.Lerp(_lowerAngle, _upperAngle, progress);

        Vector2 direction = Vector2.Lerp(
        _lowerBoundary,
        _upperBoundary,
        progress
        ).normalized;

        Vector2 position = _origin + direction * _radius;

        return position;
    }



}

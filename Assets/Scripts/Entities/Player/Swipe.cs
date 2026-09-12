using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Swipe : MonoBehaviour
{

    [SerializeField] private ParticleSystem _particles;
    [SerializeField] private float _lifetime = 0.1f;


    private float dps = 10.0f; // high DPS to combat the short lifetime. A normalization for 0.2s lifetime -
                               // so torch deals 1 damage killing smallest enemies

    private float _radius = 1.5f; // DAMAGE
    private float _visualRadius = 1.0f;
    private float _angle = 135.0f;

    private bool _forward;
    private float _elapsed;
    public Vector2 SwipePosition { get; private set; } // for sampling the light position change, and maybe for particles as well
    private Vector2 _origin;
    private Vector2 _lookDirection;

    public void Initialize(Vector2 origin, Vector2 lookDirection, bool forward)
    {
        _origin = origin;
        _lookDirection = lookDirection.normalized;
        _forward = forward;
    }


    void Start()
    {
        
        if (_particles != null)
        {
            _particles.Play();
        }
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

        foreach (GameObject enemy in EnemyManager.Instance.GetEnemiesForSwipe(_origin))
        {
            if(IsInArc(enemy.transform.position))
            {
                if (enemy.TryGetComponent(out EntityHealth entityHealth))
                {
                    entityHealth.LoseHealth(dps * Time.deltaTime);
                }
            }
        }

    }

    private Vector2 GetUpperBoundary()
    {
        return Quaternion.Euler(
            0f,
            0f,
            _angle * 0.5f) * _lookDirection;
    }

    private Vector2 GetLowerBoundary()
    {
        return Quaternion.Euler(
            0f,
            0f,
            -_angle * 0.5f) * _lookDirection;
    }


    /*
        https://stackoverflow.com/questions/243945/calculating-a-2d-vectors-cross-product
     */
    float Cross(Vector2 v1, Vector2 v2)
    {
        return (v1.x * v2.y) - (v1.y * v2.x);
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

        toEnemy.Normalize();
        float lowerSide = Cross(GetLowerBoundary(), toEnemy);
        float upperSide = Cross(toEnemy, GetUpperBoundary());

        return lowerSide >= 0f && upperSide >= 0f;
    }



    private Vector2 GetPointOnArc(float progress)
    {
        float angle = ProgressToAngle(progress);

        Vector2 direction =
            Quaternion.Euler(0f, 0f, angle) * _lookDirection;

        return _origin + direction * _radius;
    }

    private float ProgressToAngle(float progress)
    {
        float halfAngle = _angle * 0.5f;

        return Mathf.Lerp(
            -halfAngle,
            halfAngle,
            progress);
    }

}

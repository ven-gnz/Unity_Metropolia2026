using UnityEngine;
using UnityEngine.U2D;

public class SwipeDebug : MonoBehaviour
{
    [SerializeField] private LineRenderer _ring;
    [SerializeField] private LineRenderer _activeArc;
    [SerializeField] public PlayerDirection _playerDirection;

    [SerializeField] private float _radius = 2.5f;
    [SerializeField] private int _segments = 64;
    private float _arcAngle = 135.0f;
    private Camera mainCamera;

    Vector2 _lookDirection;

    private void Awake()
    {
        mainCamera = Camera.main;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DrawRing();
        DrawArc();
    }

    // Update is called once per frame
    void Update()
    {
        SetLookDirection();
        DrawArc();
    }

    private void DrawRing()
    {
        _ring.positionCount = _segments + 1;
        _ring.loop = false;

        for (int i = 0; i <= _segments; i++)
        {
            float t = i / (float)_segments;
            float angle = t * Mathf.PI * 2.0f;

            Vector3 position = new Vector3(
                Mathf.Cos(angle) * _radius,
                Mathf.Sin(angle) * _radius,
                0f
            );

            _ring.SetPosition(i, position);
        }
    }






    private void DrawArc()
    {
        Vector2 direction =
            _playerDirection.GetQuadrantDirection();

        float halfAngle = _arcAngle * 0.5f;

        _activeArc.positionCount = _segments + 1;
        _activeArc.loop = false;

        for (int i = 0; i <= _segments; i++)
        {
            float t = i / (float)_segments;

            float angle = Mathf.Lerp(
                -halfAngle,
                halfAngle,
                t
            );

            Vector2 rotatedDirection =
                Quaternion.Euler(0f, 0f, angle) * direction;

            _activeArc.SetPosition(
                i,
                PointOnArc(angle)
            );
        }
    }


    void SetLookDirection()
    {
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        _lookDirection = (mousePosition - (Vector2)transform.position).normalized;
    }

    private Vector3 PointOnArc(float angle)
    {
        Vector2 direction = Quaternion.Euler(0f, 0f, angle) * _lookDirection;

        return direction * _radius;
    }


}

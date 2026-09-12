using UnityEngine;

public enum DirectionQuadrant
{
    North_East = 0, //   0° -  90°
    North_West = 1, //  90° - 180°
    South_West = 2, // 180° - 270°
    South_East = 3  // 270° - 360°
}

public class PlayerDirection : MonoBehaviour
{

    private Camera _mainCamera;
    private Transform _player;
    public Vector2 LookDirection { get; private set; }
    public DirectionQuadrant CurrentQuadrant { get; private set; }


    public const float QuadrantHalfAngle = 45.0f; // might not need...
    public const float QuadrantAngle = 90f;


    private void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        UpdateLookDirection();
        UpdateQuadrant();
    }


    
    public const float HalfAngle = 45f;

    public static DirectionQuadrant UpdateCurrentQuadrant(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (angle < 0f)
            angle += 360f;

        int quadrant = Mathf.FloorToInt(angle / QuadrantAngle);

        return (DirectionQuadrant)quadrant;
    }

    private void UpdateLookDirection()
    {
        Vector2 mousePosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        LookDirection = (mousePosition - (Vector2)transform.position).normalized;
    }

    private void UpdateQuadrant()
    {
        float angle =
            Mathf.Atan2(LookDirection.y, LookDirection.x) * Mathf.Rad2Deg;

        if (angle < 0f)
            angle += 360f;

        CurrentQuadrant =
            (DirectionQuadrant)Mathf.FloorToInt(angle / QuadrantAngle);
    }

    public Vector2 GetQuadrantDirection()
    {
        float angle =
            (int)CurrentQuadrant * QuadrantAngle + QuadrantHalfAngle;

        return new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad)
        );
    }


}
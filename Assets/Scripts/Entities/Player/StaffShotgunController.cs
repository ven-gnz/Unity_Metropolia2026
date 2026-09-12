using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;


public class StaffShotgunController : MonoBehaviour
{

    [SerializeField] private Light2D _torchLight;
    [SerializeField] Swipe _swipe;
    [SerializeField] PlayerDirection _playerDirection;

    [SerializeField] Projectile _projectile;
    [SerializeField] ObjectPool _projectilePool;
    [SerializeField] Transform _tip;
    [SerializeField] GameObject player;



    [SerializeField] private float swipeCooldown = 0.1f;
    private bool _swipeForward = true;
    private float nextSwipeTime;
    private Swipe _activeSwipe;

    private float chargeTime;
    private float chargeStartTime;
    private float minChargeTime = 0.5f;
    private float maxChargeTime = 3.5f;
    private bool isCharging;

    int shots;
    float defaultShots = 20.0f;
    float maxShots = 140.0f;

    float innerCone = 7.5f;

    float spawnRadius = 0.15f;
    float pelletSpread = 1f;

    float waveDelay = 0.05f;

    int clusters;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (GameManager.Instance.GetState() != GameState.Playing) return;
        RotateStaff();

        if (_activeSwipe != null)
        {
            // Swipe is currently active.
            _torchLight.transform.position = _activeSwipe.SwipePosition;

        }

            if (Input.GetMouseButtonDown(0))
        {
            chargeStartTime = Time.time;
            chargeTime = 0f;
            isCharging = false;
        }

        if (Input.GetMouseButton(0))
        {
            chargeTime = Time.time - chargeStartTime;
            if (!isCharging && chargeTime >= minChargeTime) isCharging = true;
            chargeTime = Mathf.Clamp(chargeTime, 0f, maxChargeTime);
        }

        if(Input.GetMouseButtonUp(0))
        {

            if (isCharging)
            {
                Shoot(chargeTime);
            }
            else
            {
                Shoot(minChargeTime);
            }

            isCharging = false;
            chargeTime = 0f;
        }

        if(Input.GetMouseButtonDown(1))
        {
            if(Time.time >= nextSwipeTime)
            {
                nextSwipeTime = Time.time + swipeCooldown;
                UseSwipe();
            }
            
        }
    }


    void Shoot(float chargeTime)
    {

        float chargePercent = Mathf.InverseLerp(
              minChargeTime,
              maxChargeTime,
              chargeTime);

        shots = Mathf.RoundToInt(Mathf.Lerp(defaultShots, maxShots, chargePercent));
        clusters = Mathf.Max(1, shots / 5);


        StartCoroutine(FireWaves());
      
    }

    IEnumerator FireWaves()
    {
        int shotsRemaining = shots;

        while(shotsRemaining > 0)
        {
            int clustersThisWave = Mathf.Min(5, Mathf.CeilToInt(shotsRemaining / 5f));

            FireWave(clustersThisWave);
            AudioManager.Instance.PlayProjectileShoot();
            shotsRemaining -= clustersThisWave * 5;
            if(shotsRemaining > 0)
            {
                AudioManager.Instance.PlayProjectileShoot();
                yield return new WaitForSeconds(waveDelay);
            }
           
        }
    }

    void FireWave(int clustersThisWave)
    {
        Vector2 direction =
      _playerDirection.GetQuadrantDirection();

        float[] innerAngles =
        {
        -innerCone,
        -innerCone * 0.5f,
        0f,
        innerCone* 0.5f,
        innerCone
    };

        for (int i = 0; i < clustersThisWave; ++i)
        {
            float clusterAngle = innerAngles[i];

            for (int j = 0; j < 5; ++j)
            {
                float pelletAngle =
                    clusterAngle +
                    Random.Range(-pelletSpread, pelletSpread);

                Vector2 spawnPosition =
                    (Vector2)_tip.position +
                    Random.insideUnitCircle * spawnRadius;

                Vector2 pelletDirection =
                    Quaternion.Euler(
                        0f,
                        0f,
                        pelletAngle
                    ) * direction;

                GameObject go =
                    _projectilePool.GetPooledObject();

                Projectile projectile =
                    go.GetComponent<Projectile>();

                projectile.transform.position =
                    spawnPosition;

                projectile.transform.rotation =
                    Quaternion.identity;

                projectile.InitializeProjectile(
                    spawnPosition,
                    pelletDirection
                );
            }
        }

        
    }

    void UseSwipe()
    {
   
        Swipe swipe = Instantiate(_swipe);

        swipe.Initialize(
        player.transform.position,
        _playerDirection.LookDirection,
        _swipeForward);

        _activeSwipe = swipe;
        _swipeForward = !_swipeForward;
}



    private void RotateStaff()
    {
        Vector2 direction =
            _playerDirection.GetQuadrantDirection();

        transform.right = direction;
    }
}

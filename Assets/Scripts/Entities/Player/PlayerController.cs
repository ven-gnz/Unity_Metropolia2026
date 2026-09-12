using UnityEngine;
using System.Collections;
using static UnityEngine.Input;
using NUnit.Framework;
using UnityEngine.Rendering.Universal;

public class PlayerController : MonoBehaviour
{

    private Rigidbody2D _rb;
    [SerializeField] float movementSpeed = 10.0f;
    [SerializeField] SpriteRenderer _characterBody;
    [SerializeField] ParticleSystem _overdriveParticles;
    [SerializeField] private Animator _animator;
    [SerializeField] private PlayerDirection _playerDirection;

    [SerializeField] AudioClip _footstep;
    [SerializeField] PlayerHealthSystem _healthSystem;
    [SerializeField] PlayerStaffController _staffController;
    [SerializeField] Light2D moonlight;
    [SerializeField] Light2D torch;

    Color defaultTorchColor = new Color(0.7f, 0.5f, 0.5f);
    Color overdriveTorchColor = new Color(1.0f, 0.2f, 0.2f);

    private Material playerMaterial;
    private static readonly int OverdriveAmount =
    Shader.PropertyToID("_OverdriveAmount");
    [SerializeField] private SpriteRenderer playerSprite;
    Color color;
    private bool isDead; // I guess we can keep this for the animator
    private bool isEastFacing;
    private bool isNorthFacing;


    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        _healthSystem = GetComponent<PlayerHealthSystem>();
        moonlight.intensity = 0f;
        BloodSystem.Instance.OnBloodCollected += HandleMoonLightIntensity;
    }

    void Start()
    {
        _rb = gameObject.GetComponent<Rigidbody2D>();
        color = _characterBody.color;
        isDead = false;
        _healthSystem.OnPlayerDeath += PlayDeathAnimation;
        playerMaterial = playerSprite.material;
        playerMaterial.SetFloat(OverdriveAmount, 0f);
        _overdriveParticles.Stop();
        _healthSystem.OnHealthStateChanged += HandleHealthStateChanged;
        _staffController = FindAnyObjectByType<PlayerStaffController>();
        _staffController.OnGameStarted();
    }

    void Update()
    {

       
        if (!isDead)
        {
            SetPlayerOrientation();
            UpdateAnimator();
            HandlePlayerMovement();
        }
        else
        {
            _rb.linearVelocity = Vector2.zero;
        }



        _characterBody.flipX = !isEastFacing;
    }

    public void SetOverdrive(float amount)
    {
        playerMaterial.SetFloat(OverdriveAmount, amount);
    }




    private void HandlePlayerMovement()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");


        Vector2 movement = new Vector2(moveHorizontal, moveVertical);
        movement = Vector2.ClampMagnitude(movement, 1.0f);
        _rb.linearVelocity = movement * movementSpeed;

        bool characterIsWalking = movement.magnitude > 0.0f;
        _animator.SetBool("isWalking", characterIsWalking);
    }

    private void SetPlayerOrientation()
    {
        DirectionQuadrant quadrant =
            _playerDirection.CurrentQuadrant;

        isEastFacing = quadrant == DirectionQuadrant.North_East || quadrant == DirectionQuadrant.South_East;
        isNorthFacing = quadrant == DirectionQuadrant.North_East || quadrant == DirectionQuadrant.North_West;

        Debug.Log(
            $"Orientation: {quadrant} | " +
            $"Eastfacing: {isEastFacing} | " +
            $"Northfacing: {isNorthFacing}"
        );
    }

    IEnumerator alphaLerpingFunction(float endValue, float duration)
    {
        float time = 0;
        float startValue = color.a;

        while(time < duration)
        {
            
            time += Time.deltaTime;
            if(time > 1.0)
            {
                color.a = Mathf.Lerp(startValue, endValue, (time - 1.0f) / duration);
                _characterBody.color = color;
            }
            
            yield return null;
        }
        color.a = endValue;
        _characterBody.color = color;
    }



    void PlayDeathAnimation()
    {
        isDead = true;
        _animator.SetBool("isDead", isDead);
        StartCoroutine(alphaLerpingFunction(0.1f, 3.0f));
    }

    public bool getIsDead()
    {
        return isDead;
    }

    public void HandleHealthStateChanged(HealthState newState)
    {
        if (newState == HealthState.Underdrive)
        {
            movementSpeed = 9.0f;
            color.a = 0.75f;
            _characterBody.color = color;
            torch.intensity = 0.25f;
        }
        else if (newState == HealthState.Overdrive)
        {
            movementSpeed = 12.5f;
            SetOverdrive(0.7f);
            _overdriveParticles.Play();
            torch.color = overdriveTorchColor;
        }
        else if (newState == HealthState.Normal)
        {
            SetOverdrive(0.0f);
            _overdriveParticles.Stop();
            movementSpeed = 10.0f;
            torch.intensity = 1.0f;
            torch.color = defaultTorchColor;
            color.a = 1.0f;
            _characterBody.color = color;
        }

    }

    private void UpdateAnimator()
    {
        _animator.SetBool("isEastFacing", isEastFacing);
        _animator.SetBool("isNorthFacing", isNorthFacing);
    }

    private void HandleMoonLightIntensity()
    {
        // min is 0f, max is 10f. Lerping needs to happen between 0 and bloodmoonfull.

        if (BloodSystem.Instance.TotalBloodCollected < BloodSystem.Instance.BloodMoonVisibleQuota) return;

        float progress =
            (BloodSystem.Instance.TotalBloodCollected - BloodSystem.Instance.BloodMoonVisibleQuota) /
            (BloodSystem.Instance.BloodMoonFullQuota - BloodSystem.Instance.BloodMoonVisibleQuota);

        float intensity = Mathf.Lerp(0f, 2.5f, progress);

        
        moonlight.intensity = intensity;
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 9f;
    [SerializeField] private float rotationSmoothTime = 0.1f;

    [Header("Jump & Gravity")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -15f;

    [Header("Stamina System")]
    [SerializeField] private float maxStamina = 100f;
    [Tooltip("Berapa banyak stamina yang berkurang per detik saat sprint")]
    [SerializeField] private float staminaDrainRate = 30f;
    [Tooltip("Berapa banyak stamina yang pulih per detik saat tidak sprint")]
    [SerializeField] private float staminaRegenRate = 10f;
    [Tooltip("waktu tunggu sebelum stamina mulai pulih setelah sprint")]
    [SerializeField] private float regenDelay = 0.5f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [Tooltip("Masukkan Model 3D Udin")]
    [SerializeField] private Animator _animator;

    // Components
    private CharacterController _controller;
    private PlayerInput _playerInput;

    // Input values
    private Vector2 _moveInput;
    private bool _jumpInput;
    private bool _sprintInput;

    // State & Stamina
    private Vector3 _velocity;
    private float _targetRotation;
    private float _rotationVelocity;
    private bool _isGrounded;

    private float _currentStamina;
    private float _regenTimer;
    private bool _isSprinting;
    private bool _isExhausted;

    //Variable Boost
    private float _originalMoveSpeed;
    private float _originalSprintSpeed;
    private Coroutine _boostCoroutine;

    //Stun Effects
    private bool _isStunned = false;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _playerInput = GetComponent<PlayerInput>();

        // Auto-assign main camera jika tidak di-set manual
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        _currentStamina = maxStamina;

        //Simpan kecepatan movement asli
        _originalMoveSpeed = moveSpeed;
        _originalSprintSpeed = sprintSpeed;
    }

    // ── Input callbacks (dipanggil oleh PlayerInput component) ──────────────

    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        _jumpInput = value.isPressed;
    }

    public void OnSprint(InputValue value)
    {
        _sprintInput = value.isPressed;

        if (!_sprintInput)
        {
            _isExhausted = false;
        }
    }

    // ── Update ───────────────────────────────────────────────────────────────

    private void Update()
    {
        HandleGravity();
        HandleStamina();
        HandleMovement();
    }

    // --- Applied Boost ----------------------------------------
    public void ApplySpeedBoost(float multiplier, float duration)
    {
        if(_boostCoroutine != null)
        {
            StopCoroutine(_boostCoroutine);
        }
        _boostCoroutine = StartCoroutine(SpeedBoostRoutine(multiplier, duration));
    }

    private IEnumerator SpeedBoostRoutine (float multiplier, float duration)
    {
        Debug.Log("Boost Aktif");

        //Double the speed
        moveSpeed = _originalMoveSpeed * multiplier;
        sprintSpeed = _originalSprintSpeed * multiplier;

        //Stamina Maxed
        _currentStamina = maxStamina;
        _isExhausted = false;

        //Wait Duration
        yield return new WaitForSeconds(duration);

        //Normal Speed
        moveSpeed = _originalMoveSpeed;
        sprintSpeed = _originalSprintSpeed;

        Debug.Log("Boost Selesai");
    }

    //-------------Stun Effects------------------
    public void GetStunned(float duration)
    {
        if (!_isStunned)
        {
            StartCoroutine(StunRoutine(duration));
        }
    }

    public IEnumerator StunRoutine(float duration)
    {
        _isStunned = true;
        _isSprinting = false;

        //Reset input
        _moveInput = Vector2.zero;

        Debug.Log("Player Stunned");

        //Jalankan Animasi Stun

        yield return new WaitForSeconds(duration);
        _isStunned = false;

        Debug.Log("Stun Selesai");
    }


    //HANDLER --------------------------------

    private void HandleGravity()
    {
        _isGrounded = _controller.isGrounded;

        if (_isGrounded && _velocity.y < 0f)
            _velocity.y = -2f; // snap ke tanah

        // Jump
        if (_jumpInput && _isGrounded)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    private void HandleStamina()
    {
        bool tryingToSprint = _sprintInput && _moveInput != Vector2.zero && !_isExhausted;

        if(tryingToSprint && _currentStamina > 0f)
        {
            _isSprinting = true;

            //kurangi stamina perlahan
            _currentStamina -= staminaDrainRate * Time.deltaTime;
            _regenTimer = 0f;

            if(_currentStamina <= 0f)
            {
                _currentStamina = 0f;
                _isSprinting = false;
                _isExhausted = true;
            }
        }
        else
        {
            _isSprinting = false;

            //Pemulihan stamina
            if(_currentStamina < maxStamina)
            {
                _regenTimer += Time.deltaTime;

                if(_regenTimer >= regenDelay)
                {
                    _currentStamina += staminaRegenRate * Time.deltaTime;

                    if(_currentStamina > maxStamina)
                    {
                        _currentStamina = maxStamina;
                    }
                }
            }
        }
    }

    private void HandleMovement()
    {
        float currentSpeed = _isSprinting ? sprintSpeed : moveSpeed;

        //Update Animasi
        float animationSpeed = _moveInput.magnitude * currentSpeed;

        if(_animator != null)
        {
            _animator.SetFloat("Speed", animationSpeed);
        }

        if (_moveInput == Vector2.zero)
            return;

        if (_isStunned) 
            return;

        // Arah gerak relatif terhadap kamera
        float inputAngle = Mathf.Atan2(_moveInput.x, _moveInput.y) * Mathf.Rad2Deg
                           + cameraTransform.eulerAngles.y;

        // Smoothly rotate karakter ke arah gerak
        _targetRotation = Mathf.SmoothDampAngle(
            transform.eulerAngles.y,
            inputAngle,
            ref _rotationVelocity,
            rotationSmoothTime
        );
        transform.rotation = Quaternion.Euler(0f, _targetRotation, 0f);

        // Gerakkan karakter
        Vector3 moveDir = Quaternion.Euler(0f, inputAngle, 0f) * Vector3.forward;
        _controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
    }
}
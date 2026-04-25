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

    // State
    private Vector3 _velocity;
    private float _targetRotation;
    private float _rotationVelocity;
    private bool _isGrounded;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _playerInput = GetComponent<PlayerInput>();

        // Auto-assign main camera jika tidak di-set manual
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
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
    }

    // ── Update ───────────────────────────────────────────────────────────────

    private void Update()
    {
        HandleGravity();
        HandleMovement();
    }

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

    private void HandleMovement()
    {
        float currentSpeed = _sprintInput ? sprintSpeed : moveSpeed;

        //Update Animasi
        float animationSpeed = _moveInput.magnitude * currentSpeed;

        if(_animator != null)
        {
            _animator.SetFloat("Speed", animationSpeed);
        }

        if (_moveInput == Vector2.zero)
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
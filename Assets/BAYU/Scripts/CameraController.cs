using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;       // Assign Player transform
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -5f);

    [Header("Orbit Settings")]
    [SerializeField] private float sensitivity = 0.3f;
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 60f;
    [SerializeField] private float followSmoothSpeed = 10f;

    [Header("Collision")]
    [SerializeField] private float cameraRadius = 0.2f;
    [SerializeField] private LayerMask collisionLayers;

    // State
    private float _yaw;    // rotasi horizontal
    private float _pitch;  // rotasi vertical
    private Vector2 _lookInput;

    private void Start()
    {
        // Cursor lock untuk feel game yang proper
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _yaw = transform.eulerAngles.y;
    }

    // ── Input callback (assign di PlayerInput component atau manual) ─────────

    public void OnLook(InputValue value)
    {
        _lookInput = value.Get<Vector2>();
    }

    // ── LateUpdate: selalu setelah karakter bergerak ─────────────────────────

    private void LateUpdate()
    {
        if (target == null) return;

        HandleCameraRotation();
        HandleCameraPosition();
    }

    private void HandleCameraRotation()
    {
        _yaw += _lookInput.x * sensitivity;
        _pitch -= _lookInput.y * sensitivity; // minus = inverted feel natural
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
    }

    private void HandleCameraPosition()
    {
        // Hitung posisi ideal di belakang karakter
        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 desiredPos = target.position + rotation * offset;

        // Camera collision: cegah kamera masuk ke dinding/lantai
        if (Physics.SphereCast(
            target.position,
            cameraRadius,
            desiredPos - target.position,
            out RaycastHit hit,
            offset.magnitude,
            collisionLayers))
        {
            desiredPos = hit.point + hit.normal * cameraRadius;
        }

        // Smooth follow
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos,
            followSmoothSpeed * Time.deltaTime
        );

        // Selalu lihat ke target
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}
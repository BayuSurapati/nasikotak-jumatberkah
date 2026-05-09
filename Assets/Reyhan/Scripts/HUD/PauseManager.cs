using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("UI Containers")]
    [SerializeField] private GameObject _pauseMenuContainer;
    [SerializeField] private CanvasGroup _backgroundOverlay; // Pakai CanvasGroup untuk Fade background
    [SerializeField] private RectTransform _pauseNote; // Panel buku catatan

    [Header("Animation Settings")]
    [SerializeField] private float _fadeDuration = 0.4f;
    [SerializeField] private float _noteDuration = 0.5f;
    [SerializeField, Range(0f, 1f)] private float _targetBgAlpha = 0.8f;
    [Space]
    [SerializeField] private Ease _appearEase = Ease.OutBack;
    [SerializeField] private Ease _disappearEase = Ease.InBack;

    [Header("Buttons Reference")]
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _retryButton;
    [SerializeField] private Button _mainMenuButton;

    private bool _isPaused = false;

    private void Awake()
    {
        Instance = this;
        
        // Inisialisasi awal agar tersembunyi
        if (_pauseMenuContainer != null) _pauseMenuContainer.SetActive(false);
        if (_backgroundOverlay != null) _backgroundOverlay.alpha = 0;
        if (_pauseNote != null) _pauseNote.localScale = Vector3.zero;
    }

    private void Update()
    {
        // Cek input ESC atau tombol Start/Select di Gamepad
        // "Cancel" biasanya terhubung ke ESC, "Submit" atau tombol khusus bisa diset di Input Manager
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Pause")) 
        {
            if (_isPaused) Resume();
            else Pause();
        }
    }

    // ===== FUNGSI UTAMA =====

    public void Pause()
    {
        if (_isPaused) return;
        _isPaused = true;

        _pauseMenuContainer.SetActive(true);
        
        // Membekukan waktu game
        Time.timeScale = 0f;

        // Mematikan kontrol player & kamera agar tidak bergerak di belakang UI
        TogglePlayerControls(false);

        // Animasi Muncul
        _backgroundOverlay.DOKill();
        _pauseNote.DOKill();

        Sequence pauseSeq = DOTween.Sequence();
        pauseSeq.SetUpdate(true); // Wajib agar animasi jalan saat Time.timeScale = 0
        pauseSeq.Join(_backgroundOverlay.DOFade(_targetBgAlpha, _fadeDuration).SetEase(Ease.Linear));
        pauseSeq.Join(_pauseNote.DOScale(Vector3.one, _noteDuration).SetEase(_appearEase));
    }

    public void Resume()
    {
        if (!_isPaused) return;
        _isPaused = false;

        // Animasi Menghilang
        _backgroundOverlay.DOKill();
        _pauseNote.DOKill();

        Sequence resumeSeq = DOTween.Sequence();
        resumeSeq.SetUpdate(true);
        resumeSeq.Join(_pauseNote.DOScale(Vector3.zero, _noteDuration).SetEase(_disappearEase));
        resumeSeq.Join(_backgroundOverlay.DOFade(0f, _fadeDuration).SetEase(Ease.Linear).SetDelay(0.1f));

        resumeSeq.OnComplete(() =>
        {
            _pauseMenuContainer.SetActive(false);
            
            // Mengembalikan waktu game
            Time.timeScale = 1f;

            // Mengaktifkan kembali kontrol player & kamera
            TogglePlayerControls(true);
        });
    }

    public void Retry()
    {
        // Pastikan waktu kembali normal sebelum reload scene
        Time.timeScale = 1f;
        DOTween.KillAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        DOTween.KillAll();
        SceneManager.LoadScene("MainMenu"); // Pastikan nama scene benar
    }

    // ===== HELPER FUNCTIONS =====

    private void TogglePlayerControls(bool state)
    {
        // Munculkan/Sembunyikan Kursor
        Cursor.visible = !state;
        Cursor.lockState = state ? CursorLockMode.Locked : CursorLockMode.None;

        // Matikan pergerakan & kamera (Sesuai referensi script sebelumnya)
        if (GameManager.Instance != null) GameManager.Instance.isGameActive = state;
        
        CameraController cam = FindObjectOfType<CameraController>();
        if (cam != null) cam.enabled = state;

        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null) player.enabled = state;
    }
}
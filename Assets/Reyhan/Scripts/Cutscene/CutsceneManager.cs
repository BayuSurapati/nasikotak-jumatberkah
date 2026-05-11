using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.InputSystem; 
using TMPro; // Wajib ditambahkan untuk TextMeshPro

public class CutsceneManager : MonoBehaviour
{
    [Header("Referensi Panel (Urutan 1-4)")]
    [SerializeField] private RectTransform _panel1;
    [SerializeField] private RectTransform _panel2;
    [SerializeField] private RectTransform _panel3;
    [SerializeField] private RectTransform _panel4;

    [Header("Referensi UI Lainnya")]
    [SerializeField] private Image _whiteBackground;
    // Tipe diganti ke TextMeshProUGUI agar bisa di-fade
    [SerializeField] private TextMeshProUGUI _continuePromptText; 
    [SerializeField] private string _gameplaySceneName = "GameplayScene";

    [Header("Pengaturan Animasi Panel")]
    [SerializeField] private float _panelDuration = 0.6f;
    [SerializeField] private float _delayBetweenPanels = 0.5f;

    [Header("Pengaturan Flickering (Teks Lanjut)")]
    // Berapa kali berkedip dalam satu siklus (misal 4 kali kedip)
    [SerializeField] private int _flashCount = 4; 
    // Durasi satu siklus kedipan (misal dalam 1 detik berkedip 4 kali)
    [SerializeField] private float _flashDuration = 1f; 

    private bool _canContinue = false;
    private Vector2[] _originalPositions = new Vector2[4];

    private void Awake()
    {
        Time.timeScale = 1f;

        _originalPositions[0] = _panel1.anchoredPosition;
        _originalPositions[1] = _panel2.anchoredPosition;
        _originalPositions[2] = _panel3.anchoredPosition;
        _originalPositions[3] = _panel4.anchoredPosition;

        PreparePanels();
    }

    private void Start()
    {
        PlayCutscene();
    }

    private void PreparePanels()
    {
        // Matikan teks prompt dan buat transparan di awal
        if (_continuePromptText != null)
        {
            _continuePromptText.gameObject.SetActive(false);
            // Set alpha teks ke 0 (transparan total)
            _continuePromptText.alpha = 0f; 
        }
        
        if (_whiteBackground != null)
        {
            Color c = _whiteBackground.color;
            c.a = 1f;
            _whiteBackground.color = c;
        }

        _panel1.anchoredPosition += new Vector2(-1000, 500); 
        _panel2.anchoredPosition += new Vector2(-1000, -500); 
        _panel3.anchoredPosition += new Vector2(1000, 500);  
        _panel4.localScale = Vector3.zero; 
    }

    private void PlayCutscene()
    {
        Sequence cutsceneSeq = DOTween.Sequence();

        cutsceneSeq.Append(_panel1.DOAnchorPos(_originalPositions[0], _panelDuration).SetEase(Ease.OutBack));
        cutsceneSeq.AppendInterval(_delayBetweenPanels);

        cutsceneSeq.Append(_panel2.DOAnchorPos(_originalPositions[1], _panelDuration).SetEase(Ease.OutBack));
        cutsceneSeq.AppendInterval(_delayBetweenPanels);

        cutsceneSeq.Append(_panel3.DOAnchorPos(_originalPositions[2], _panelDuration).SetEase(Ease.OutBack));
        cutsceneSeq.AppendInterval(_delayBetweenPanels);

        cutsceneSeq.Append(_panel4.DOScale(Vector3.one, _panelDuration).SetEase(Ease.OutBack));

        cutsceneSeq.OnComplete(() => {
            _canContinue = true;
            StartFlickeringPrompt();
        });
    }

    private void StartFlickeringPrompt()
    {
        if (_continuePromptText == null) return;

        // Hidupkan objeknya
        _continuePromptText.gameObject.SetActive(true);
        // Pastikan alpha mulai dari 1 (solid)
        _continuePromptText.alpha = 1f; 

        // --- LOGIKA FLICKERING (Kerlipan Patah-patah) ---
        // Kita nge-fade alpha dari 1 ke 0.
        // Dengan SetEase(Ease.Flash, jumlahKedip, mode), teks akan On/Off secara instan.
        _continuePromptText.DOFade(0f, _flashDuration)
            .SetEase(Ease.Flash, _flashCount, 0) 
            .SetLoops(-1, LoopType.Yoyo) // Loop selamanya (Yoyo agar balik solid)
            .SetUpdate(true); // Biar tetep kedip misal gamenya di-pause
    }

    private void Update()
    {
        if (_canContinue && WasAnyKeyPressed())
        {
            _canContinue = false; 
            // Matikan semua tween yang menempel di teks agar tidak error saat pindah scene
            if (_continuePromptText != null) _continuePromptText.DOKill(); 
            
            SceneManager.LoadScene(_gameplaySceneName);
        }
    }

    private bool WasAnyKeyPressed()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            return true;

        if (Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame))
            return true;

        if (Gamepad.current != null)
        {
            if (Gamepad.current.buttonSouth.wasPressedThisFrame ||
                Gamepad.current.buttonEast.wasPressedThisFrame ||
                Gamepad.current.buttonWest.wasPressedThisFrame ||
                Gamepad.current.buttonNorth.wasPressedThisFrame ||
                Gamepad.current.startButton.wasPressedThisFrame ||
                Gamepad.current.selectButton.wasPressedThisFrame)
            {
                return true;
            }
        }

        return false;
    }
}
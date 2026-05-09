using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class LevelEndUI : MonoBehaviour
{
    public static LevelEndUI Instance { get; private set; }

    [Header("Referensi WIN Panel")]
    [SerializeField] private GameObject _winPanelContainer; 
    [SerializeField] private Image _winBackgroundOverlay; 
    [SerializeField] private RectTransform _winNoteContent; 

    [Header("Statistik - WIN")]
    [SerializeField] private TextMeshProUGUI _winNasiTerkirimText;
    [SerializeField] private TextMeshProUGUI _winSisaWaktuText;
    [SerializeField] private TextMeshProUGUI _winStatusText; 

    [Header("Referensi LOSE Panel")]
    [SerializeField] private GameObject _losePanelContainer; 
    [SerializeField] private Image _loseBackgroundOverlay; 
    [SerializeField] private RectTransform _loseNoteContent; 

    [Header("Statistik - LOSE")]
    [SerializeField] private TextMeshProUGUI _loseNasiTerkirimText;
    [SerializeField] private TextMeshProUGUI _loseKekuranganText; 
    [SerializeField] private TextMeshProUGUI _losePesanGagalText; 

    [Header("Pengaturan Animasi")]
    [SerializeField] private float _fadeDuration = 0.4f;
    [SerializeField] private float _noteDuration = 0.5f;
    [SerializeField, Range(0f, 1f)] private float _targetBgAlpha = 0.8f; 

    [Header("Dialogue Reference")]
    [SerializeField] private DialogueTrigger _endGameDialogueTrigger;

    private void Awake()
    {
        Instance = this;
        InisialisasiUI();
    }

    private void InisialisasiUI()
    {
        if (_winPanelContainer != null) _winPanelContainer.SetActive(false);
        if (_losePanelContainer != null) _losePanelContainer.SetActive(false);
        
        // FAILSAFE 1: Paksa matikan Raycast pada background agar tidak menghalangi tombol!
        if (_winBackgroundOverlay != null) 
        {
            SetImageAlpha(_winBackgroundOverlay, 0f);
            _winBackgroundOverlay.raycastTarget = false; 
        }
        if (_loseBackgroundOverlay != null) 
        {
            SetImageAlpha(_loseBackgroundOverlay, 0f);
            _loseBackgroundOverlay.raycastTarget = false;
        }

        if (_winNoteContent != null) _winNoteContent.localScale = Vector3.zero;
        if (_loseNoteContent != null) _loseNoteContent.localScale = Vector3.zero;

        if (_endGameDialogueTrigger != null) _endGameDialogueTrigger.gameObject.SetActive(false);
    }

    private void SetImageAlpha(Image img, float alpha)
    {
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    private void EnableUICursorAndPause()
    {
        // Munculkan dan bebaskan kursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0f;

        // FAILSAFE 2: Matikan Kamera DAN pergerakan Player agar tidak membajak kursor
        CameraController cam = FindObjectOfType<CameraController>();
        if (cam != null) cam.enabled = false;

        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null) player.enabled = false;
    }

    // ===== LOGIKA WIN =====
    public void ShowWinPanel()
    {
        if (_winPanelContainer == null) return;
        
        EnableUICursorAndPause();
        _winPanelContainer.SetActive(true);

        // FAILSAFE 3: Paksa CanvasGroup agar bisa diklik (jika ada)
        CanvasGroup cg = _winPanelContainer.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }

        if (GameManager.Instance != null)
        {
            int delivered = GameManager.Instance.getNasiKotakTerkirim();
            int target = GameManager.Instance.targetNasiKotak;
            float timeRemaining = GameManager.Instance.timeRemaining;

            if (_winNasiTerkirimText != null) _winNasiTerkirimText.text = $"{delivered} / {target}";
            if (_winSisaWaktuText != null) _winSisaWaktuText.text = $"{Mathf.FloorToInt(timeRemaining)}s";
        }
        
        if (_winStatusText != null) _winStatusText.text = "ALHAMDULILLAH!";

        _winBackgroundOverlay.DOKill();
        _winNoteContent.DOKill();

        Sequence winSeq = DOTween.Sequence().SetUpdate(true);
        winSeq.Join(_winBackgroundOverlay.DOFade(_targetBgAlpha, _fadeDuration).SetEase(Ease.Linear));
        winSeq.Join(_winNoteContent.DOScale(Vector3.one, _noteDuration).SetEase(Ease.OutBack).SetDelay(0.1f));
    }

    // ===== LOGIKA LOSE =====
    public void ShowLosePanel(int delivered, int target, string alasan)
    {
        if (_losePanelContainer == null) return;
        
        EnableUICursorAndPause();
        _losePanelContainer.SetActive(true);

        // FAILSAFE 3: Paksa CanvasGroup agar bisa diklik (jika ada)
        CanvasGroup cg = _losePanelContainer.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }

        int kurang = target - delivered;
        if (_loseNasiTerkirimText != null) _loseNasiTerkirimText.text = $"{delivered}";
        if (_loseKekuranganText != null) _loseKekuranganText.text = $"{kurang}";
        if (_losePesanGagalText != null) _losePesanGagalText.text = alasan; 

        _loseBackgroundOverlay.DOKill();
        _loseNoteContent.DOKill();

        Sequence loseSeq = DOTween.Sequence().SetUpdate(true);
        loseSeq.Join(_loseBackgroundOverlay.DOFade(_targetBgAlpha, _fadeDuration).SetEase(Ease.Linear));
        loseSeq.Join(_loseNoteContent.DOScale(Vector3.one, _noteDuration).SetEase(Ease.OutBack).SetDelay(0.1f));
    }

    // ===== FUNGSI TOMBOL =====
    public void OnNextClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void OnRetryClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
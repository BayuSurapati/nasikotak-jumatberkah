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

    [Header("Referensi FINAL Panel (Setelah Ustadz)")]
    [SerializeField] private GameObject _finalPanelContainer; 
    [SerializeField] private Image _finalBackgroundOverlay; 
    [SerializeField] private RectTransform _finalNoteContent; 

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
        // Pastikan semua container mati di awal
        if (_winPanelContainer != null) _winPanelContainer.SetActive(false);
        if (_losePanelContainer != null) _losePanelContainer.SetActive(false);
        if (_finalPanelContainer != null) _finalPanelContainer.SetActive(false);

        // Set alpha background overlay ke 0 (transparan total)
        if (_winBackgroundOverlay != null) SetImageAlpha(_winBackgroundOverlay, 0f);
        if (_loseBackgroundOverlay != null) SetImageAlpha(_loseBackgroundOverlay, 0f);
        if (_finalBackgroundOverlay != null) SetImageAlpha(_finalBackgroundOverlay, 0f);

        // Set skala kertas catatan ke 0
        if (_winNoteContent != null) _winNoteContent.localScale = Vector3.zero;
        if (_loseNoteContent != null) _loseNoteContent.localScale = Vector3.zero;
        if (_finalNoteContent != null) _finalNoteContent.localScale = Vector3.zero;

        if (_endGameDialogueTrigger != null)
        {
            _endGameDialogueTrigger.gameObject.SetActive(false);
        }
    
    }

    // Helper function untuk set alpha Image
    private void SetImageAlpha(Image img, float alpha)
    {
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    // ===== FUNGSI UNTUK MEMUNCULKAN KURSOR DAN PAUSE GAME =====
    private void EnableUICursorAndPause()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0f;

        CameraController cam = FindObjectOfType<CameraController>();
        if (cam != null) cam.enabled = false;
    }

    private void RestorePlayerControl()
    {
        // 1. Kembalikan waktu ke normal
        Time.timeScale = 1f;

        // 2. Aktifkan kembali status game agar player bisa bergerak
        if (GameManager.Instance != null)
        {
            GameManager.Instance.isGameActive = true;
        }

        // 3. Sembunyikan dan kunci kursor agar kamera bisa diputar kembali
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // 4. Aktifkan kembali script kamera dan movement
        CameraController cam = FindObjectOfType<CameraController>();
        if (cam != null) cam.enabled = true;

        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null) player.enabled = true;
    }

    // ===== LOGIKA WIN (MENANG) =====
    public void ShowWinPanel(int delivered, int target, float timeRemaining)
    {
        if (_winPanelContainer == null) return;
        
        EnableUICursorAndPause();
        _winPanelContainer.SetActive(true);

        if (_winNasiTerkirimText != null) _winNasiTerkirimText.text = $"{delivered} / {target}";
        if (_winSisaWaktuText != null) _winSisaWaktuText.text = $"{Mathf.FloorToInt(timeRemaining)}s";
        if (_winStatusText != null) _winStatusText.text = "SUKSES!";

        _winBackgroundOverlay.DOKill();
        _winNoteContent.DOKill();

        Sequence winSeq = DOTween.Sequence();
        winSeq.SetUpdate(true);
        winSeq.Join(_winBackgroundOverlay.DOFade(_targetBgAlpha, _fadeDuration).SetEase(Ease.Linear));
        winSeq.Join(_winNoteContent.DOScale(Vector3.one, _noteDuration).SetEase(Ease.OutBack).SetDelay(0.1f));
    }

    // ===== LOGIKA LOSE (KALAH) =====
    public void ShowLosePanel(int delivered, int target, string alasan)
    {
        if (_losePanelContainer == null) return;
        
        EnableUICursorAndPause();
        _losePanelContainer.SetActive(true);

        int kurang = target - delivered;
        if (_loseNasiTerkirimText != null) _loseNasiTerkirimText.text = $"{delivered}";
        if (_loseKekuranganText != null) _loseKekuranganText.text = $"{kurang}";
        if (_losePesanGagalText != null) _losePesanGagalText.text = alasan; 

        _loseBackgroundOverlay.DOKill();
        _loseNoteContent.DOKill();

        Sequence loseSeq = DOTween.Sequence();
        loseSeq.SetUpdate(true);
        loseSeq.Join(_loseBackgroundOverlay.DOFade(_targetBgAlpha, _fadeDuration).SetEase(Ease.Linear));
        loseSeq.Join(_loseNoteContent.DOScale(Vector3.one, _noteDuration).SetEase(Ease.OutBack).SetDelay(0.1f));
    }

    // ===== LOGIKA FINAL PANEL (SETELAH DIALOG USTADZ) =====
    public void ShowFinalCompletePanel()
    {
        if (_finalPanelContainer == null) return;
        
        EnableUICursorAndPause();
        _finalPanelContainer.SetActive(true);

        _finalBackgroundOverlay.DOKill();
        _finalNoteContent.DOKill();

        Sequence finalSeq = DOTween.Sequence();
        finalSeq.SetUpdate(true);
        finalSeq.Join(_finalBackgroundOverlay.DOFade(_targetBgAlpha, _fadeDuration).SetEase(Ease.Linear));
        finalSeq.Join(_finalNoteContent.DOScale(Vector3.one, _noteDuration).SetEase(Ease.OutBack).SetDelay(0.1f));
    }

    // ===== FUNGSI TOMBOL =====
    public void OnNextClicked()
    {
        if (_winPanelContainer == null) return;

        // Matikan interaksi tombol agar tidak di-spam
        CanvasGroup cg = _winPanelContainer.GetComponent<CanvasGroup>();
        if(cg != null) cg.interactable = false;

        // Animasi Menutup Panel
        Sequence hideSeq = DOTween.Sequence();
        hideSeq.SetUpdate(true);
        hideSeq.Join(_winNoteContent.DOScale(Vector3.zero, _noteDuration * 0.8f).SetEase(Ease.InBack));
        hideSeq.Join(_winBackgroundOverlay.DOFade(0f, _fadeDuration).SetEase(Ease.Linear).SetDelay(0.1f));

        hideSeq.OnComplete(() => {
            _winPanelContainer.SetActive(false);
            
            // 1. KEMBALIKAN KONTROL (BUG 1 FIX)
            Time.timeScale = 1f;
            if (GameManager.Instance != null) GameManager.Instance.isGameActive = true;
            
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            CameraController cam = FindObjectOfType<CameraController>();
            if (cam != null) cam.enabled = true;

            PlayerMovement player = FindObjectOfType<PlayerMovement>();
            if (player != null) player.enabled = true;

            // 2. AKTIFKAN TRIGGER AKHIR (BUG 2 FIX)
            if (_endGameDialogueTrigger != null)
            {
                _endGameDialogueTrigger.gameObject.SetActive(true);
            }

            // 3. TAMPILKAN PESAN HINT BARU
            if (HintManager.Instance != null)
            {
                HintManager.Instance.ShowSpecificHint("Alhamdulillah! Sekarang ayo balik ke Masjid, lapor ke Pak Ustadz.");
            }
        });
    }

    public void OnRetryClicked()
    {
        if (_losePanelContainer == null) return;

        CanvasGroup cg = _losePanelContainer.GetComponent<CanvasGroup>();
        if(cg != null) cg.interactable = false;

        Sequence hideSeq = DOTween.Sequence();
        hideSeq.SetUpdate(true);
        hideSeq.Join(_loseNoteContent.DOScale(Vector3.zero, _noteDuration * 0.8f).SetEase(Ease.InBack));
        hideSeq.Join(_loseBackgroundOverlay.DOFade(0f, _fadeDuration).SetEase(Ease.Linear).SetDelay(0.1f));

        hideSeq.OnComplete(() => {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });
    }

    public void OnMainMenuClicked()
    {
        // Tombol ini akan menutup Final Panel dan pindah ke Main Menu
        if (_finalPanelContainer != null)
        {
            CanvasGroup cg = _finalPanelContainer.GetComponent<CanvasGroup>();
            if(cg != null) cg.interactable = false;

            Sequence hideSeq = DOTween.Sequence();
            hideSeq.SetUpdate(true);
            hideSeq.Join(_finalNoteContent.DOScale(Vector3.zero, _noteDuration * 0.8f).SetEase(Ease.InBack));
            hideSeq.Join(_finalBackgroundOverlay.DOFade(0f, _fadeDuration).SetEase(Ease.Linear).SetDelay(0.1f));

            hideSeq.OnComplete(() => {
                Time.timeScale = 1f;
                SceneManager.LoadScene("MainMenu");
            });
        }
        else
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }
    }
}
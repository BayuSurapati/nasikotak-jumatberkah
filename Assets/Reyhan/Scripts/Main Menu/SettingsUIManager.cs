using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SettingsUIManager : MonoBehaviour
{
    [Header("Background Overlay")]
    [SerializeField] private CanvasGroup _whiteOverlay; // Background putih transparan
    [SerializeField] private float _fadeDuration = 0.4f;

    [Header("Panels")]
    [SerializeField] private RectTransform _settingsPanel;
    [SerializeField] private RectTransform _keyboardPanel;
    [SerializeField] private RectTransform _gamepadPanel;

    [Header("Settings Buttons")]
    [SerializeField] private Button _controlBtn;
    [SerializeField] private Button _graphicBtn; // Belum ada fungsi
    [SerializeField] private Button _languageBtn; // Belum ada fungsi

    [Header("Animation Settings")]
    [SerializeField] private float _popDuration = 0.5f;

    private void Awake()
    {
        // Inisialisasi awal: Sembunyikan semua
        _whiteOverlay.alpha = 0;
        _whiteOverlay.gameObject.SetActive(false);
        
        _settingsPanel.localScale = Vector3.zero;
        _keyboardPanel.localScale = Vector3.zero;
        _gamepadPanel.localScale = Vector3.zero;

        _settingsPanel.gameObject.SetActive(false);
        _keyboardPanel.gameObject.SetActive(false);
        _gamepadPanel.gameObject.SetActive(false);
    }

    // --- FUNGSI UTAMA ---

    public void OpenSettings()
    {
        _whiteOverlay.gameObject.SetActive(true);
        _settingsPanel.gameObject.SetActive(true);

        // Fade In Background
        _whiteOverlay.DOFade(0.7f, _fadeDuration);

        // PopUp Out Settings Panel
        _settingsPanel.DOScale(Vector3.one, _popDuration).SetEase(Ease.OutBack);
    }

    public void OpenKeyboardControl()
    {
        // Settings Panel PopUp In (Mengecil)
        _settingsPanel.DOScale(Vector3.zero, _popDuration).SetEase(Ease.InBack).OnComplete(() => {
            _settingsPanel.gameObject.SetActive(false);
            
            // Munculkan Keyboard Panel PopUp Out
            _keyboardPanel.gameObject.SetActive(true);
            _keyboardPanel.DOScale(Vector3.one, _popDuration).SetEase(Ease.OutBack);
        });
    }

    public void SwitchToGamepad()
    {
        // Keyboard Panel PopUp In (Mengecil)
        _keyboardPanel.DOScale(Vector3.zero, _popDuration).SetEase(Ease.InBack).OnComplete(() => {
            _keyboardPanel.gameObject.SetActive(false);
            
            // Munculkan Gamepad Panel PopUp Out
            _gamepadPanel.gameObject.SetActive(true);
            _gamepadPanel.DOScale(Vector3.one, _popDuration).SetEase(Ease.OutBack);
        });
    }

    public void SwitchToKeyboard()
    {
        // Gamepad Panel PopUp In
        _gamepadPanel.DOScale(Vector3.zero, _popDuration).SetEase(Ease.InBack).OnComplete(() => {
            _gamepadPanel.gameObject.SetActive(false);
            
            // Munculkan Keyboard Panel
            _keyboardPanel.gameObject.SetActive(true);
            _keyboardPanel.DOScale(Vector3.one, _popDuration).SetEase(Ease.OutBack);
        });
    }

    public void CloseAll()
    {
        // Cari panel mana yang aktif dan kecilkan
        RectTransform activePanel = null;
        if (_settingsPanel.gameObject.activeSelf) activePanel = _settingsPanel;
        else if (_keyboardPanel.gameObject.activeSelf) activePanel = _keyboardPanel;
        else if (_gamepadPanel.gameObject.activeSelf) activePanel = _gamepadPanel;

        if (activePanel != null)
        {
            activePanel.DOScale(Vector3.zero, _popDuration).SetEase(Ease.InBack).OnComplete(() => {
                activePanel.gameObject.SetActive(false);
                
                // Fade Out Background
                _whiteOverlay.DOFade(0, _fadeDuration).OnComplete(() => {
                    _whiteOverlay.gameObject.SetActive(false);
                });
            });
        }
    }
}
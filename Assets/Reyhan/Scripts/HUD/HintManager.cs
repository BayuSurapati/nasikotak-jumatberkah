using System.Collections; // Wajib ditambahkan untuk menggunakan Coroutine
using UnityEngine;
using DG.Tweening; 
using TMPro; 

public class HintManager : MonoBehaviour
{
    // Singleton agar mudah dipanggil dari script lain
    public static HintManager Instance { get; private set; }

    [Header("Referensi UI Hint")]
    [SerializeField, Tooltip("RectTransform kotak dialog/teks (TextBorder)")]
    private RectTransform _textBorder;
    
    [SerializeField, Tooltip("RectTransform gambar wajah Udin (IconUdin)")]
    private RectTransform _iconUdin;

    [SerializeField, Tooltip("Komponen Teks untuk hint")]
    private TextMeshProUGUI _hintTextComponent;

    [Header("Pengaturan Teks & Typewriter")]
    [SerializeField, TextArea, Tooltip("Pesan yang akan diketik Udin")]
    private string _hintMessage = "Waduh, nasi kotak habis! Aku harus ambil lagi ke tumpukan!";

    [SerializeField, Tooltip("Durasi efek ngetik sampai selesai (detik)")]
    private float _typewriterDuration = 1.5f;

    [Header("Pengaturan Animasi DOTween")]
    [SerializeField, Tooltip("Durasi animasi Pop In/Out")]
    private float _popDuration = 0.4f;

    [SerializeField, Tooltip("Seberapa miring Udin bergoyang saat bicara (derajat)")]
    private float _wiggleAngle = 12f;

    [SerializeField, Tooltip("Kecepatan goyangan Udin saat bicara (detik)")]
    private float _wiggleDuration = 0.25f;

    // Variabel internal
    private bool _isHintActive = false;
    private Vector3 _originalTextScale;
    
    // Menyimpan referensi coroutine ngetik agar bisa dihentikan kapan saja
    private Coroutine _typingCoroutine;

    private void Awake()
    {
        // Setup Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Simpan ukuran asli text border, lalu sembunyikan UI di awal permainan
        if (_textBorder != null)
        {
            _originalTextScale = _textBorder.localScale;
            _textBorder.localScale = Vector3.zero;
            _textBorder.gameObject.SetActive(false);
        }

        if (_iconUdin != null)
        {
            _iconUdin.gameObject.SetActive(false);
        }

        if (_hintTextComponent != null)
        {
            _hintTextComponent.text = "";
        }
    }

    public void ShowHint()
    {
        if (_isHintActive) return;
        _isHintActive = true;

        // 1. Animasi TextBorder (Pop Out memantul)
        if (_textBorder != null)
        {
            _textBorder.gameObject.SetActive(true);
            _textBorder.DOKill();
            _textBorder.DOScale(_originalTextScale, _popDuration).SetEase(Ease.OutBack);
        }

        // 2. Animasi IconUdin (Muncul dan Rotasi Kiri Kanan)
        if (_iconUdin != null)
        {
            _iconUdin.gameObject.SetActive(true);
            _iconUdin.DOKill();
            
            _iconUdin.localRotation = Quaternion.Euler(0f, 0f, -_wiggleAngle);
            _iconUdin.DOLocalRotate(new Vector3(0f, 0f, _wiggleAngle), _wiggleDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        // 3. Efek Typewriter menggunakan Coroutine
        if (_hintTextComponent != null)
        {
            // Hentikan coroutine ngetik sebelumnya jika masih berjalan
            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
            }
            
            // Mulai ngetik ulang
            _typingCoroutine = StartCoroutine(TypewriterRoutine(_hintMessage, _typewriterDuration));
        }
    }

    public void HideHint()
    {
        if (!_isHintActive) return;
        _isHintActive = false;

        if (_textBorder != null)
        {
            _textBorder.DOKill();
            _textBorder.DOScale(Vector3.zero, _popDuration).SetEase(Ease.InBack).SetUpdate(true)
                .OnComplete(() => _textBorder.gameObject.SetActive(false));
        }

        if (_iconUdin != null)
        {
            _iconUdin.DOKill();
            _iconUdin.DOLocalRotate(Vector3.zero, 0.2f).SetUpdate(true)
                .OnComplete(() => _iconUdin.gameObject.SetActive(false));
        }

        if (_hintTextComponent != null)
        {
            if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
            _hintTextComponent.text = "";
        }
    }

    // --- COROUTINE UNTUK EFEK NGETIK ---
    private IEnumerator TypewriterRoutine(string textToType, float totalDuration)
    {
        _hintTextComponent.text = "";
        
        // Menghitung jeda per huruf agar selesai tepat pada totalDuration
        float delayPerChar = totalDuration / textToType.Length;
        WaitForSeconds wait = new WaitForSeconds(delayPerChar);

        // Tambahkan huruf satu per satu
        foreach (char c in textToType)
        {
            _hintTextComponent.text += c;
            yield return wait;
        }
    }

    public void ShowSpecificHint(string message)
    {
        // Hentikan proses yang sedang berjalan jika ada
        if (_isHintActive) HideHint();

        // Set pesan baru
        _hintMessage = message; 
        
        // Munculkan kembali dengan pesan baru tersebut
        ShowHint();
    }
}
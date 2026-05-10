using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class CutsceneManager : MonoBehaviour
{
    [Header("Referensi Panel (Urutan 1-4)")]
    [SerializeField] private RectTransform _panel1;
    [SerializeField] private RectTransform _panel2;
    [SerializeField] private RectTransform _panel3;
    [SerializeField] private RectTransform _panel4;

    [Header("Referensi UI Lainnya")]
    [SerializeField] private Image _whiteBackground;
    [SerializeField] private GameObject _pressSpacePrompt;
    [SerializeField] private string _gameplaySceneName = "GameplayScene";

    [Header("Pengaturan Animasi")]
    [SerializeField] private float _panelDuration = 0.6f;
    [SerializeField] private float _delayBetweenPanels = 0.5f;

    private bool _canContinue = false;
    private Vector2[] _originalPositions = new Vector2[4];

    private void Awake()
    {
        // Pastikan waktu jalan normal
        Time.timeScale = 1f;

        // Simpan posisi asli panel
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
        // Sembunyikan prompt space di awal
        if (_pressSpacePrompt != null) _pressSpacePrompt.SetActive(false);
        
        // Background langsung muncul solid (Alpha 1)
        if (_whiteBackground != null)
        {
            Color c = _whiteBackground.color;
            c.a = 1f;
            _whiteBackground.color = c;
        }

        // Pindahkan panel ke posisi awal animasi luar layar
        _panel1.anchoredPosition += new Vector2(-1000, 500); 
        _panel2.anchoredPosition += new Vector2(-1000, -500); 
        _panel3.anchoredPosition += new Vector2(1000, 700);  
        _panel4.localScale = Vector3.zero; 
    }

    private void PlayCutscene()
    {
        Sequence cutsceneSeq = DOTween.Sequence();

        // Jalankan animasi panel secara berurutan
        cutsceneSeq.Append(_panel1.DOAnchorPos(_originalPositions[0], _panelDuration).SetEase(Ease.OutBack));
        cutsceneSeq.AppendInterval(_delayBetweenPanels);

        cutsceneSeq.Append(_panel2.DOAnchorPos(_originalPositions[1], _panelDuration).SetEase(Ease.OutBack));
        cutsceneSeq.AppendInterval(_delayBetweenPanels);

        cutsceneSeq.Append(_panel3.DOAnchorPos(_originalPositions[2], _panelDuration).SetEase(Ease.OutBack));
        cutsceneSeq.AppendInterval(_delayBetweenPanels);

        cutsceneSeq.Append(_panel4.DOScale(Vector3.one, _panelDuration).SetEase(Ease.OutBack));

        // Setelah panel terakhir muncul, aktifkan input untuk lanjut
        cutsceneSeq.OnComplete(() => {
            _canContinue = true;
            if (_pressSpacePrompt != null)
            {
                _pressSpacePrompt.SetActive(true);
                // Efek animasi berdenyut halus pada petunjuk tombol Space
                _pressSpacePrompt.transform.DOScale(1.1f, 0.5f).SetLoops(-1, LoopType.Yoyo);
            }
        });
    }

    private void Update()
    {
        // Jika animasi sudah selesai dan pemain menekan Space
        if (_canContinue && Input.GetKeyDown(KeyCode.Space))
        {
            _canContinue = false; // Mencegah pemanggilan ganda
            
            // Langsung pindah ke scene gameplay tanpa melakukan fade out pada background
            SceneManager.LoadScene(_gameplaySceneName);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TutorialBook : MonoBehaviour
{
    // ===== CONSTANTS =====

    private const float PAGE_TURN_DURATION = 0.3f;
    private const float BOOK_OPEN_DURATION  = 0.5f;

    // ===== INSPECTOR FIELDS =====

    [Header("Book Panel")]
    [Tooltip("Root GameObject seluruh buku (yang di-scale saat buka/tutup)")]
    [SerializeField] private RectTransform bookRoot;

    [Header("Halaman Kiri")]
    [SerializeField] private TextMeshProUGUI leftTitleText;
    [SerializeField] private Image          leftImage;
    [SerializeField] private TextMeshProUGUI leftDescText;

    [Header("Halaman Kanan")]
    [SerializeField] private TextMeshProUGUI rightTitleText;
    [SerializeField] private Image          rightImage;
    [SerializeField] private TextMeshProUGUI rightDescText;
    [Tooltip("TMP kedua khusus halaman kanan — untuk deskripsi panjang yang dibagi dua (halaman 8)")]
    [SerializeField] private TextMeshProUGUI rightDescPart2Text;

    [Header("Navigasi")]
    [SerializeField] private Button prevButton;   // Tombol KIRI
    [SerializeField] private Button nextButton;   // Tombol KANAN
    [SerializeField] private Button exitButton;   // Tombol X

    [Header("Page Flip Visual (opsional)")]
    [Tooltip("CanvasGroup seluruh konten halaman — di-fade saat ganti halaman")]
    [SerializeField] private CanvasGroup pageContentGroup;

    // ===== RUNTIME FIELDS =====

    private List<TutorialPageData> _pages;
    private int _currentSpread = 0;   // spread = pasangan halaman (kiri+kanan)
    private bool _isTurning   = false;

    // ===== UNITY LIFECYCLE =====

    private void Awake()
    {
        BuildPageData();
    }

    private void Start()
    {
        // Pasang listener tombol
        if (prevButton != null) prevButton.onClick.AddListener(OnPrevClicked);
        if (nextButton != null) nextButton.onClick.AddListener(OnNextClicked);
        if (exitButton != null) exitButton.onClick.AddListener(OnExitClicked);

        // Sembunyikan buku di awal (scale 0)
        if (bookRoot != null) bookRoot.localScale = Vector3.zero;
    }

    // ===== PUBLIC API =====

    /// <summary>Buka buku dengan animasi pop-in. Panggil dari tombol Tutorial di Main Menu.</summary>
    public void OpenBook()
    {
        gameObject.SetActive(true);
        _currentSpread = 0;

        DisplaySpread(_currentSpread);
        UpdateNavButtons();

        if (bookRoot != null)
        {
            bookRoot.localScale = Vector3.zero;
            bookRoot.DOScale(Vector3.one, BOOK_OPEN_DURATION)
                    .SetEase(Ease.OutBack);
        }
    }

    /// <summary>Tutup buku dengan animasi pop-out.</summary>
    public void CloseBook()
    {
        if (bookRoot != null)
        {
            bookRoot.DOScale(Vector3.zero, BOOK_OPEN_DURATION * 0.7f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => gameObject.SetActive(false));
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    // ===== PRIVATE LOGIC =====

    /// <summary>
    /// Isi data 7 tutorial secara manual di sini.
    /// Masukkan Sprite lewat Inspector tidak bisa langsung, jadi kita pakai
    /// [SerializeField] array sprites di bawah dan assign di sini.
    /// </summary>
    private void BuildPageData()
    {
        _pages = new List<TutorialPageData>
        {
            // Halaman 1
            new TutorialPageData
            {
                title       = "Cara Bergerak",
                image       = GetSprite(0),
                description = "Gunakan tombol W, A, S, D pada keyboard atau gerakkan Analog / Joystick pada controller untuk mulai berjalan dan mengeksplorasi jalanan kampung."
            },

            // Halaman 2
            new TutorialPageData
            {
                title       = "Mengambil Barang",
                image       = GetSprite(1),
                description = "Untuk berinteraksi dan mengambil kotak makanan, tekan tombol E pada keyboard atau Tombol Interact (West Button) pada controller milikmu."
            },

            // Halaman 3
            new TutorialPageData
            {
                title       = "Membawa Nasi Kotak",
                image       = GetSprite(2),
                description = "Hampiri meja di depan area masjid untuk mengambil nasi kotak dari Jumat Berkah. Kamu hanya bisa membawa maksimal 3 tumpuk nasi kotak sekaligus!"
            },

            // Halaman 4
            new TutorialPageData
            {
                title       = "Mencari Target Warga",
                image       = GetSprite(3),
                description = "Tugasmu adalah berkeliling kampung untuk mencari warga yang membutuhkan. Perhatikan lingkungan sekitar untuk menemukan mereka yang belum mendapatkan jatah makan hari ini."
            },

            // Halaman 5
            new TutorialPageData
            {
                title       = "Mengantarkan Makanan",
                image       = GetSprite(4),
                description = "Jika kamu sudah menemukan warga yang membutuhkan, cukup dekati karakter tersebut. Nasi kotak dari tanganmu akan otomatis diserahkan kepada mereka tanpa perlu menekan tombol tambahan."
            },

            // Halaman 6
            new TutorialPageData
            {
                title       = "Mengisi Ulang Bawaan",
                image       = GetSprite(5),
                description = "Apabila nasi kotak di tanganmu sudah habis namun masih ada warga yang belum kebagian, segeralah kembali ke masjid! Ambil sisa nasi kotak yang ada di meja untuk melanjutkan pengantaran."
            },

            // Halaman 7
            new TutorialPageData
            {
                title       = "Perhatikan Waktu!",
                image       = GetSprite(6),
                description = "Jangan terlalu lama bersantai! Ada batas waktu yang terus berjalan mundur. Pak Ustad sedang menunggumu dan beliau harus segera pergi untuk urusan lain. Pastikan semua nasi kotak selesai dibagikan sebelum waktu di layar habis!"
            },

            // Halaman 8 — hanya judul + deskripsi panjang (tanpa gambar), dibagi 2 TMP
            new TutorialPageData
            {
                title            = "Tahukah kamu?",
                image            = null,
                description      = null,
                descriptionPart2 = "Tahukah kamu? Permainan ini dibuat bukan sekadar untuk hiburan, lho. Melalui misi Jumat Berkah ini, kita diajak untuk mengingat kembali betapa pentingnya rasa peduli dan saling berbagi dengan tetangga di sekitar kita."
                                    + "\n\nLangkah kecil Waluyo dalam game ini adalah bentuk dukungan kita terhadap tujuan global SDGs: Zero Hunger (Tanpa Kelaparan). Semoga pengalaman bermainmu di sini bisa menjadi pengingat yang baik untuk terus menyebarkan kebaikan dan menolong sesama di dunia nyata. Lets be Have Fun with The Game."
            },
        };
    }

    // ===== SPREAD DISPLAY =====

    /// <summary>
    /// Satu spread = dua halaman (kiri & kanan).
    /// Layout:
    ///   Spread 0 → halaman 0 (kiri) + halaman 1 (kanan)
    ///   Spread 1 → halaman 2 (kiri) + halaman 3 (kanan)
    ///   ... dan seterusnya.
    /// Jika halaman kanan tidak ada (jumlah ganjil), halaman kanan dikosongkan.
    /// </summary>
    private void DisplaySpread(int spreadIndex)
    {
        int leftIndex  = spreadIndex * 2;
        int rightIndex = leftIndex + 1;

        TutorialPageData leftData  = leftIndex  < _pages.Count ? _pages[leftIndex]  : null;
        TutorialPageData rightData = rightIndex < _pages.Count ? _pages[rightIndex] : null;

        SetPage(leftTitleText,  leftImage,  leftDescText,  null,               leftData);
        SetPage(rightTitleText, rightImage, rightDescText, rightDescPart2Text, rightData);
    }

    private void SetPage(TextMeshProUGUI titleTmp, Image img,
                         TextMeshProUGUI descTmp, TextMeshProUGUI descPart2Tmp,
                         TutorialPageData data)
    {
        bool hasData = data != null;

        if (titleTmp != null) titleTmp.text = hasData ? data.title       : string.Empty;
        if (descTmp  != null) descTmp.text  = hasData ? data.description : string.Empty;

        // Part 2: tampilkan kalau ada isinya, sembunyikan kalau kosong
        if (descPart2Tmp != null)
        {
            bool hasPart2 = hasData && !string.IsNullOrEmpty(data.descriptionPart2);
            descPart2Tmp.text    = hasPart2 ? data.descriptionPart2 : string.Empty;
            descPart2Tmp.gameObject.SetActive(hasPart2);
        }

        if (img != null)
        {
            img.sprite  = hasData ? data.image : null;
            img.enabled = hasData && data.image != null;
        }
    }

    private void UpdateNavButtons()
    {
        int totalSpreads = Mathf.CeilToInt(_pages.Count / 2f);

        if (prevButton != null) prevButton.interactable = _currentSpread > 0;
        if (nextButton != null) nextButton.interactable = _currentSpread < totalSpreads - 1;
    }

    // ===== BUTTON HANDLERS =====

    private void OnPrevClicked()
    {
        if (_isTurning || _currentSpread <= 0) return;
        _currentSpread--;
        StartCoroutine(TurnPageRoutine());
    }

    private void OnNextClicked()
    {
        int totalSpreads = Mathf.CeilToInt(_pages.Count / 2f);
        if (_isTurning || _currentSpread >= totalSpreads - 1) return;
        _currentSpread++;
        StartCoroutine(TurnPageRoutine());
    }

    private void OnExitClicked()
    {
        CloseBook();
    }

    // ===== COROUTINES =====

    /// <summary>Fade out → ganti konten → fade in. Simpel tapi terasa seperti balik halaman.</summary>
    private IEnumerator TurnPageRoutine()
    {
        _isTurning = true;

        if (pageContentGroup != null)
        {
            // Fade out
            yield return pageContentGroup
                .DOFade(0f, PAGE_TURN_DURATION * 0.5f)
                .SetEase(Ease.InQuad)
                .WaitForCompletion();
        }

        // Ganti konten
        DisplaySpread(_currentSpread);
        UpdateNavButtons();

        if (pageContentGroup != null)
        {
            // Fade in
            yield return pageContentGroup
                .DOFade(1f, PAGE_TURN_DURATION * 0.5f)
                .SetEase(Ease.OutQuad)
                .WaitForCompletion();
        }

        _isTurning = false;
    }

    // ===== HELPERS =====

    /// <summary>
    /// Ambil sprite dari array tutorialSprites berdasarkan index.
    /// Kalau index out of range, return null (aman).
    /// </summary>
    private Sprite GetSprite(int index)
    {
        if (tutorialSprites == null || index >= tutorialSprites.Length) return null;
        return tutorialSprites[index];
    }

    private void OnDestroy()
    {
        DOTween.Kill(bookRoot);
        if (pageContentGroup != null) DOTween.Kill(pageContentGroup);
    }

    // ===== SPRITE ARRAY (assign di Inspector) =====

    [Header("Gambar Tutorial (urut 1–7)")]
    [Tooltip("Drag 7 sprite gambar tutorial ke sini, urut dari tutorial 1 sampai 7")]
    [SerializeField] private Sprite[] tutorialSprites;
}

// ===== DATA CLASS =====

[System.Serializable]
public class TutorialPageData
{
    public string title;
    public Sprite image;
    [TextArea(3, 6)]
    public string description;
    [TextArea(3, 6)]
    [Tooltip("Opsional — hanya isi kalau deskripsi terlalu panjang dan perlu dibagi dua (misal halaman 7)")]
    public string descriptionPart2;
}
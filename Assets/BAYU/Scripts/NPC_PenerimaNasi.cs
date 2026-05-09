using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class NPC_PenerimaNasi : MonoBehaviour
{
    [Header("Pengaturan NPC Penerima")]
    [Tooltip("Titik Kosong di tangan penerima untuk memegang nasi kotak")]
    public Transform holdPoint;

    [Tooltip("Masukkan Child 3D yang ada animatornya")]
    public Animator animator;

    [Header("Pengaturan Indikator (Sprite)")]
    [SerializeField, Tooltip("Masukkan SpriteRenderer indikator yang ada di atas kepala NPC")]
    private SpriteRenderer _indicatorRenderer;

    [SerializeField, Tooltip("Gambar/Sprite saat NPC belum dapat nasi (Sedih)")]
    private Sprite _sadSprite;

    [SerializeField, Tooltip("Gambar/Sprite saat NPC sudah dapat nasi (Senang)")]
    private Sprite _happySprite;

    [SerializeField, Tooltip("Durasi indikator senang tampil sebelum hilang (detik)")]
    private float _happyDuration = 5f;

    private bool _sudahDapatNasi = false;
    private Vector3 _originalIndicatorScale;
    // Start is called before the first frame update
    void Start()
    {
        // Menyimpan ukuran asli dari sprite indikator saat pertama kali main
        if (_indicatorRenderer != null)
        {
            _originalIndicatorScale = _indicatorRenderer.transform.localScale;
            
            // Set indikator ke kondisi awal (sedih dan ukuran 0 agar bisa di-pop-up)
            _indicatorRenderer.sprite = _sadSprite;
            _indicatorRenderer.transform.localScale = Vector3.zero;

            ShowSadIndicator();
        }
        else
        {
            Debug.LogWarning("[NPC_PenerimaNasi] Indicator Renderer belum di-assign di Inspector!");
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ShowSadIndicator()
    {
        if (_indicatorRenderer == null) return;

        // Pastikan tidak ada animasi DOTween lain yang berjalan di objek ini
        _indicatorRenderer.transform.DOKill();

        Sequence sadSequence = DOTween.Sequence();

        // 1. Muncul dengan Pop-Up exaggerated (melewati batas ukuran asli lalu mantul balik)
        sadSequence.Append(_indicatorRenderer.transform.DOScale(_originalIndicatorScale, 0.5f).SetEase(Ease.OutBack));

        // 2. Setelah muncul, beri efek mengecil & membesar sedikit-sedikit secara terus-menerus (Yoyo)
        sadSequence.AppendCallback(() =>
        {
            _indicatorRenderer.transform.DOScale(_originalIndicatorScale * 1.15f, 0.6f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo); // -1 berarti loop tidak terbatas (infinite)
        });
    }

    private void ShowHappyIndicator()
    {
        if (_indicatorRenderer == null) return;

        // 1. Hentikan animasi sedih (termasuk efek denyut loop-nya)
        _indicatorRenderer.transform.DOKill();

        // 2. Ganti gambarnya menjadi senang
        _indicatorRenderer.sprite = _happySprite;

        // 3. Kembalikan ukuran ke normal dan beri sedikit efek mantul (Punch) saat berganti sprite
        _indicatorRenderer.transform.localScale = _originalIndicatorScale;
        _indicatorRenderer.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.3f, 5, 1f);

        // 4. Mulai perhitungan mundur untuk menghilangkan indikator (Pop In)
        DOVirtual.DelayedCall(_happyDuration, () =>
        {
            if (_indicatorRenderer != null)
            {
                // Mengecil (Pop In) dengan animasi InBack lalu sembunyikan objeknya
                _indicatorRenderer.transform.DOScale(Vector3.zero, 0.4f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() =>
                    {
                        _indicatorRenderer.gameObject.SetActive(false);
                    });
            }
        });
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_sudahDapatNasi) return;

        if (other.CompareTag("Player"))
        {
            PlayerInteract player = other.GetComponent<PlayerInteract>();
            

            if (player != null && player.HasItem())
            {
                player.GiveTopItem(holdPoint);
                _sudahDapatNasi = true;
                GameManager.Instance.NasiKotakDelivered();
                if (animator != null)
                {
                    animator.SetBool("IsHappy", true);
                }
                else
                {
                    Debug.Log("Animator belum dimasukkan");
                }

                ShowHappyIndicator();

                Debug.Log("Terimakasih");
            }
        }
    }
}

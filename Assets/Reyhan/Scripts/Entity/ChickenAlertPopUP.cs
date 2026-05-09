using UnityEngine;
using DG.Tweening; // Mengimpor pustaka DOTween.

// Ikuti panduan penamaan: PascalCase untuk class, MonoBehaviour.
public class ChickenAlertPopup : MonoBehaviour
{
    // Gunakan [Header] dan [Tooltip] untuk merapikan Inspector.
    [Header("Pengaturan Ikon Alert")]
    [SerializeField, Tooltip("Objek visual tanda seru (!) (SpriteRenderer atau GameObject)")]
    private GameObject _alertIconObject;

    [SerializeField, Tooltip("Seberapa lama tanda seru diam sebelum hilang (detik)")]
    private float _stayVisibleDuration = 2f;

    [SerializeField, Tooltip("Kecepatan animasi muncul (detik) untuk membedakannya dengan yang lain")]
    private float _popupDuration = 0.4f;

    [Header("Pengaturan Exaggeration (Berlebihan)")]
    [SerializeField, Tooltip("Skala maksimum saat mantul (buatlah besar untuk exaggeration)")]
    private Vector3 _maxExaggeratedScale = new Vector3(3f, 3f, 3f);

    [SerializeField, Tooltip("Jarak pantulan posisi vertikal agar 'mantul'")]
    private float _positionBounceOffset = 0.5f;

    // Variabel internal (camelCase) untuk melacak status.
    private Vector3 _originalLocalPosition;
    private Vector3 _originalScale;
    private bool _isAlerting = false;

    // Ikuti panduan: void Awake() / void Start() untuk inisialisasi.
    private void Awake()
    {
        if (_alertIconObject != null)
        {
            // Simpan posisi dan skala asli untuk reset.
            _originalLocalPosition = _alertIconObject.transform.localPosition;
            _originalScale = _alertIconObject.transform.localScale;

            // Sembunyikan ikon di awal agar tidak langsung terlihat.
            _alertIconObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[ChickenAlertPopup] _alertIconObject belum diset di Inspector. Silakan seret objek tanda seru kamu!");
        }
    }

    // Fungsi publik (PascalCase) untuk memicu alert yang berlebihan.
    // Dipanggil dari script deteksi player (misal: EnemyPatrol).
    public void TriggerAlert()
    {
        if (_isAlerting || _alertIconObject == null) return;

        _isAlerting = true;
        _alertIconObject.SetActive(true);

        // Matikan tween sebelumnya jika ada agar tidak bentrok.
        _alertIconObject.transform.DOKill();

        // Reset posisi dan skala awal (ke nol) sebelum animasi.
        _alertIconObject.transform.localScale = Vector3.zero;
        _alertIconObject.transform.localPosition = _originalLocalPosition;

        // Buat Sequence DOTween untuk animasi "juicy" dan exaggerated.
        Sequence alertSequence = DOTween.Sequence();

        // 1. Animasi Muncul Berlebihan (Exaggerated Pop-up dengan InOutBack)
        // Gunakan InOutBack untuk efek pantulan yang kuat dan mendesak.
        
        // Gabungkan (Join) Scale dan Position agar berjalan bareng.
        // Scale dari 0 ke skala yang sangat besar.
        alertSequence.Join(_alertIconObject.transform.DOScale(_maxExaggeratedScale, _popupDuration).SetEase(Ease.InOutBack));

        // Angkat posisinya untuk efek bounce.
        alertSequence.Join(_alertIconObject.transform.DOLocalMoveY(_originalLocalPosition.y + _positionBounceOffset, _popupDuration).SetEase(Ease.InOutBack));

        // 2. Tahan tanda seru sebentar untuk mempertahankan urgency.
        alertSequence.AppendInterval(_stayVisibleDuration);

        // 3. Animasi Hilang 
        // Scale down kembali ke nol dengan sedikit InBack.
        alertSequence.Append(_alertIconObject.transform.DOScale(Vector3.zero, _popupDuration).SetEase(Ease.InBack));
        alertSequence.Join(_alertIconObject.transform.DOLocalMoveY(_originalLocalPosition.y, _popupDuration).SetEase(Ease.InBack));

        // 4. Callback saat selesai untuk membersihkan.
        alertSequence.OnComplete(() =>
        {
            _alertIconObject.SetActive(false);
            _isAlerting = false;
        });
    }
}
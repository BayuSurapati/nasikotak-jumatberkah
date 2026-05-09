using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class StaminaBarHUD : MonoBehaviour
{
    [Header("Referensi UI")]
    [SerializeField, Tooltip("Image dengan Type: Filled")]
    private Image _fillImage;

    [Header("Pengaturan Warna")]
    [SerializeField, Tooltip("Gradient warna dari kosong (kiri) ke penuh (kanan)")]
    private Gradient _staminaGradient;

    [SerializeField, Tooltip("Kecepatan transisi bar (detik)")]
    private float _updateDuration = 0.2f;

    // Fungsi Publik untuk memperbarui tampilan bar
    public void UpdateStaminaBar(float currentStamina, float maxStamina)
    {
        if (_fillImage == null) return;

        float targetFill = currentStamina / maxStamina;

        // Kill tween sebelumnya agar tidak bentrok
        _fillImage.DOKill();

        // 1. Animasi Fill Amount
        _fillImage.DOFillAmount(targetFill, _updateDuration).SetEase(Ease.OutQuad);

        // 2. Animasi Perubahan Warna berdasarkan Gradient
        Color targetColor = _staminaGradient.Evaluate(targetFill);
        _fillImage.DOColor(targetColor, _updateDuration);
    }
}
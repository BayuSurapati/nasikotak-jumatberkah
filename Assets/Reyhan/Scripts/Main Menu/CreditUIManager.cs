using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CreditUIManager : MonoBehaviour
{
    [Header("Background Overlay")]
    [SerializeField] private CanvasGroup _whiteOverlay;
    [SerializeField] private float _fadeDuration = 0.4f;

    [Header("Credit Panel")]
    [SerializeField] private RectTransform _creditPanel;

    [Header("Animation Settings")]
    [SerializeField] private float _popDuration = 0.5f;

    private void Awake()
    {
        _whiteOverlay.alpha = 0;
        _whiteOverlay.gameObject.SetActive(false);

        _creditPanel.localScale = Vector3.zero;
        _creditPanel.gameObject.SetActive(false);
    }

    public void OpenCredits()
    {
        _whiteOverlay.gameObject.SetActive(true);
        _creditPanel.gameObject.SetActive(true);

        _whiteOverlay.DOFade(0.7f, _fadeDuration);
        _creditPanel.DOScale(Vector3.one, _popDuration).SetEase(Ease.OutBack);
    }

    public void CloseCredits()
    {
        _creditPanel.DOScale(Vector3.zero, _popDuration).SetEase(Ease.InBack).OnComplete(() =>
        {
            _creditPanel.gameObject.SetActive(false);
            _whiteOverlay.DOFade(0, _fadeDuration).OnComplete(() =>
            {
                _whiteOverlay.gameObject.SetActive(false);
            });
        });
    }
}

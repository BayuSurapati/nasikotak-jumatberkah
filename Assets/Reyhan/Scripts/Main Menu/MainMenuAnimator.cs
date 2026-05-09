using System.Collections;
using UnityEngine;
using DG.Tweening;

public class MainMenuAnimator : MonoBehaviour
{
    // ===== INSPECTOR FIELDS =====

    [Header("Sign Board (Papan Nama)")]
    [SerializeField] private RectTransform signBoard;
    [SerializeField] private float signOffscreenY = 400f;
    [SerializeField] private float signDropDuration = 0.7f;
    [SerializeField] private Ease signDropEase = Ease.OutBounce;

    [Header("Pole (Tiang Papan)")]
    [SerializeField] private RectTransform pole;
    [SerializeField] private float poleOffscreenY = -600f;
    [SerializeField] private float poleRiseDuration = 0.6f;
    [SerializeField] private Ease poleRiseEase = Ease.OutBack;
    [SerializeField] private float poleRiseOvershoot = 1.2f;

    [Header("Buttons (Dari Kanan)")]
    [SerializeField] private RectTransform[] menuButtons;
    [SerializeField] private float buttonOffscreenX = 600f;
    [SerializeField] private float buttonSlideDuration = 0.4f;
    [SerializeField] private float buttonStaggerDelay = 0.1f;
    [SerializeField] private Ease buttonSlideEase = Ease.OutBack;

    [Header("Credit Button (Pop Out)")]
    [SerializeField] private RectTransform creditButton;
    [SerializeField] private float creditButtonPopDuration = 0.45f;
    [SerializeField] private Ease creditButtonPopEase = Ease.OutBack;

    [Header("Ustad (Pop Out)")]
    [SerializeField] private RectTransform ustad;
    [SerializeField] private float ustadPopDuration = 0.5f;
    [SerializeField] private Ease ustadPopEase = Ease.OutBack;

    [Header("Waluyo (Walking)")]
    [SerializeField] private RectTransform waluyo;
    [SerializeField] private float waluyoStartX = -400f;
    [SerializeField] private float waluyoEndX = -180f;
    [SerializeField] private float waluyoWalkDuration = 1.2f;
    [SerializeField] private Animator waluyoAnimator;
    [SerializeField] private string waluyoWalkParam = "IsWalking";
    [SerializeField] private float walkBobAmplitude = 8f;
    [SerializeField] private float walkBobFrequency = 4f;

    [Header("Chicken (Ayam)")]
    [SerializeField] private RectTransform chicken;
    [SerializeField] private float chickenAppearDelay = 0.3f;
    [SerializeField] private Ease chickenAppearEase = Ease.OutBack;
    [SerializeField] private float chickenAppearOvershoot = 1.5f;
    [SerializeField] private float chickenWiggleAngle = 15f;
    [SerializeField] private float chickenWiggleDuration = 0.18f;
    [SerializeField] private int chickenWiggleLoops = 6;
    [SerializeField] private Ease chickenWiggleEase = Ease.InOutSine;

    [Header("Hanging Title (Judul Gantung)")]
    [SerializeField] private RectTransform hangingTitle;
    [SerializeField] private float titleOffscreenY = 500f;
    [SerializeField] private float titleDropDuration = 0.65f;
    [SerializeField] private Ease titleDropEase = Ease.InBack;
    [SerializeField] private float titleDropOvershoot = 1.5f;

    [Header("Clouds (Awan)")]
    [SerializeField] private CanvasGroup[] clouds;
    [SerializeField] private float cloudFadeDuration = 0.8f;
    [SerializeField] private float cloudFadeStagger = 0.15f;
    [SerializeField] private Ease cloudFadeEase = Ease.InOutSine;

    [Header("Timing Keseluruhan")]
    [SerializeField] private float delayBeforeStart = 0.3f;

    // ===== RUNTIME FIELDS =====

    private Vector2 _signBoardOrigin;
    private Vector2 _poleOrigin;
    private Vector2[] _buttonOrigins;
    private Vector2 _ustadOrigin;
    private Vector2 _waluyoOrigin;
    private Vector2 _chickenOrigin;
    private Vector2 _hangingTitleOrigin;

    // ===== UNITY LIFECYCLE =====

    private void Awake()
    {
        CacheOrigins();
        HideAll();
    }

    private void Start()
    {
        StartCoroutine(PlayIntroSequence());
    }

    // ===== PUBLIC API =====

    public void ReplayIntro()
    {
        StopAllCoroutines();
        DOTween.KillAll();
        HideAll();
        StartCoroutine(PlayIntroSequence());
    }

    // ===== PRIVATE LOGIC =====

    private void CacheOrigins()
    {
        if (signBoard != null) _signBoardOrigin = signBoard.anchoredPosition;
        if (pole != null) _poleOrigin = pole.anchoredPosition;

        _buttonOrigins = new Vector2[menuButtons.Length];
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] != null)
                _buttonOrigins[i] = menuButtons[i].anchoredPosition;
        }

        if (ustad != null) _ustadOrigin = ustad.anchoredPosition;
        if (waluyo != null) _waluyoOrigin = waluyo.anchoredPosition;
        if (chicken != null) _chickenOrigin = chicken.anchoredPosition;
        if (hangingTitle != null) _hangingTitleOrigin = hangingTitle.anchoredPosition;
    }

    private void HideAll()
    {
        if (signBoard != null) signBoard.anchoredPosition = _signBoardOrigin + Vector2.up * signOffscreenY;
        if (pole != null) pole.anchoredPosition = _poleOrigin + Vector2.down * Mathf.Abs(poleOffscreenY);

        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] != null)
                menuButtons[i].anchoredPosition = _buttonOrigins[i] + Vector2.right * buttonOffscreenX;
        }

        if (ustad != null)
        {
            ustad.localScale = Vector3.zero;
            ustad.anchoredPosition = _ustadOrigin;
        }

        if (creditButton != null)
        {
            creditButton.localScale = Vector3.zero;
        }

        if (waluyo != null)
        {
            Vector2 pos = _waluyoOrigin;
            pos.x = waluyoStartX;
            waluyo.anchoredPosition = pos;
        }

        if (chicken != null) chicken.localScale = Vector3.zero;
        if (hangingTitle != null) hangingTitle.anchoredPosition = _hangingTitleOrigin + Vector2.up * titleOffscreenY;

        if (clouds != null)
        {
            foreach (CanvasGroup cloud in clouds)
            {
                if (cloud != null) cloud.alpha = 0f;
            }
        }
    }

    // ===== COROUTINES =====

    private IEnumerator PlayIntroSequence()
    {
        yield return new WaitForSeconds(delayBeforeStart);

        StartCoroutine(AnimateCloudsAppear());

        AnimateHangingTitleDrop();
        yield return new WaitForSeconds(titleDropDuration * 0.5f);

        AnimatePoleRise();
        yield return new WaitForSeconds(poleRiseDuration * 0.4f);

        AnimateSignDrop();
        yield return new WaitForSeconds(0.2f);

        yield return StartCoroutine(AnimateButtonsSlideIn());

        StartCoroutine(AnimateWaluyoWalk());
        yield return new WaitForSeconds(waluyoWalkDuration * 0.3f);

        AnimateUstadPopOut();
        yield return new WaitForSeconds(ustadPopDuration + 0.1f);

        yield return new WaitForSeconds(chickenAppearDelay);
        AnimateChickenAppear();

        yield return new WaitForSeconds(waluyoWalkDuration * 0.7f);

        yield return new WaitForSeconds(0.3f);
        AnimateChickenWiggle();
    }

    // ===== ANIMATION METHODS =====

    private void AnimateHangingTitleDrop()
    {
        if (hangingTitle == null) return;
        hangingTitle.DOAnchorPosY(_hangingTitleOrigin.y, titleDropDuration)
                    .SetEase(titleDropEase, titleDropOvershoot);
    }

    private IEnumerator AnimateCloudsAppear()
    {
        if (clouds == null) yield break;

        foreach (CanvasGroup cloud in clouds)
        {
            if (cloud == null) continue;
            DOTween.To(() => cloud.alpha, x => cloud.alpha = x, 1f, cloudFadeDuration)
                   .SetEase(cloudFadeEase);
            yield return new WaitForSeconds(cloudFadeStagger);
        }
    }

    private void AnimatePoleRise()
    {
        if (pole == null) return;
        pole.DOAnchorPosY(_poleOrigin.y, poleRiseDuration)
            .SetEase(poleRiseEase, poleRiseOvershoot);
    }

    private void AnimateSignDrop()
    {
        if (signBoard == null) return;
        signBoard.DOAnchorPosY(_signBoardOrigin.y, signDropDuration)
            .SetEase(signDropEase);
    }

    private IEnumerator AnimateButtonsSlideIn()
    {
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] == null) continue;
            menuButtons[i].DOAnchorPos(_buttonOrigins[i], buttonSlideDuration)
                          .SetEase(buttonSlideEase);
            yield return new WaitForSeconds(buttonStaggerDelay);
        }
        AnimateCreditButtonPopOut();
    }

    private void AnimateCreditButtonPopOut()
    {
        if (creditButton == null) return;
        creditButton.DOScale(Vector3.one, creditButtonPopDuration)
                     .SetEase(creditButtonPopEase);
    }

    private void AnimateUstadPopOut()
    {
        if (ustad == null) return;
        ustad.DOScale(Vector3.one, ustadPopDuration)
             .SetEase(ustadPopEase);
    }

    private IEnumerator AnimateWaluyoWalk()
    {
        if (waluyo == null) yield break;
        if (waluyoAnimator != null) waluyoAnimator.SetBool(waluyoWalkParam, true);

        float elapsed = 0f;
        float startX = waluyoStartX;
        float endX = waluyoEndX;
        float startY = _waluyoOrigin.y;

        while (elapsed < waluyoWalkDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / waluyoWalkDuration);
            float smoothT = t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
            float currentX = Mathf.Lerp(startX, endX, smoothT);
            float bob = Mathf.Sin(elapsed * walkBobFrequency * Mathf.PI) * walkBobAmplitude;

            waluyo.anchoredPosition = new Vector2(currentX, startY + bob);
            yield return null;
        }

        waluyo.anchoredPosition = new Vector2(endX, startY);
        if (waluyoAnimator != null) waluyoAnimator.SetBool(waluyoWalkParam, false);
    }

    private void AnimateChickenAppear()
    {
        if (chicken == null) return;
        chicken.DOScale(Vector3.one, 0.4f)
               .SetEase(chickenAppearEase, chickenAppearOvershoot);
    }

    private void AnimateChickenWiggle()
    {
        if (chicken == null) return;
        chicken.DORotate(new Vector3(0f, 0f, chickenWiggleAngle), chickenWiggleDuration)
               .SetEase(chickenWiggleEase)
               .SetLoops(chickenWiggleLoops, LoopType.Yoyo)
               .OnComplete(StartChickenIdleWiggle);
    }

    private void StartChickenIdleWiggle()
    {
        if (chicken == null) return;
        chicken.DORotate(new Vector3(0f, 0f, chickenWiggleAngle * 0.4f), chickenWiggleDuration * 1.5f)
               .SetEase(chickenWiggleEase)
               .SetLoops(-1, LoopType.Yoyo);
    }

    private void OnDestroy()
    {
        DOTween.Kill(signBoard);
        DOTween.Kill(pole);
        DOTween.Kill(ustad);
        DOTween.Kill(chicken);
        DOTween.Kill(waluyo);
        DOTween.Kill(hangingTitle);

        if (menuButtons != null)
        {
            foreach (RectTransform btn in menuButtons)
            {
                if (btn != null) DOTween.Kill(btn);
            }
        }

        if (clouds != null)
        {
            foreach (CanvasGroup cloud in clouds)
            {
                if (cloud != null) DOTween.Kill(cloud);
            }
        }

        if (creditButton != null) DOTween.Kill(creditButton);
    }
}
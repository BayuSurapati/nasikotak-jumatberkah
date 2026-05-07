using System.Collections;
using UnityEngine;
using DG.Tweening;

public class MainMenuAnimator : MonoBehaviour
{
    // ===== INSPECTOR FIELDS =====

    [Header("Sign Board (Papan Nama)")]
    [Tooltip("RectTransform papan 'Nasi Kotak Jumat Berkah'")]
    [SerializeField] private RectTransform signBoard;
    [Tooltip("Seberapa jauh papan mulai dari atas layar (pixel)")]
    [SerializeField] private float signOffscreenY = 400f;
    [SerializeField] private float signDropDuration = 0.7f;

    [Header("Pole (Tiang Papan)")]
    [Tooltip("RectTransform tiang papan")]
    [SerializeField] private RectTransform pole;
    [Tooltip("Seberapa jauh tiang mulai dari bawah layar (pixel)")]
    [SerializeField] private float poleOffscreenY = -600f;
    [SerializeField] private float poleRiseDuration = 0.6f;

    [Header("Buttons (Dari Kanan)")]
    [Tooltip("Urutan: Play, Settings, Tutorial, Quit")]
    [SerializeField] private RectTransform[] menuButtons;
    [SerializeField] private float buttonOffscreenX = 600f;
    [SerializeField] private float buttonSlideDuration = 0.4f;
    [SerializeField] private float buttonStaggerDelay = 0.1f;

    [Header("Ustad (Pop Out)")]
    [SerializeField] private RectTransform ustad;
    [SerializeField] private float ustadPopDuration = 0.5f;

    [Header("Waluyo (Walking)")]
    [SerializeField] private RectTransform waluyo;
    [Tooltip("Posisi X awal Waluyo (di luar layar kiri)")]
    [SerializeField] private float waluyoStartX = -400f;
    [Tooltip("Posisi X akhir Waluyo berdiri")]
    [SerializeField] private float waluyoEndX = -180f;
    [SerializeField] private float waluyoWalkDuration = 1.2f;
    [Tooltip("Animator Waluyo (opsional untuk anim walk)")]
    [SerializeField] private Animator waluyoAnimator;
    [Tooltip("Parameter bool walk di Animator Waluyo")]
    [SerializeField] private string waluyoWalkParam = "IsWalking";
    [Tooltip("Amplitudo bobbing naik-turun saat jalan")]
    [SerializeField] private float walkBobAmplitude = 8f;
    [SerializeField] private float walkBobFrequency = 4f;

    [Header("Chicken (Ayam)")]
    [SerializeField] private RectTransform chicken;
    [SerializeField] private float chickenAppearDelay = 0.3f;
    [SerializeField] private float chickenWiggleAngle = 15f;
    [SerializeField] private float chickenWiggleDuration = 0.18f;
    [SerializeField] private int chickenWiggleLoops = 6;

    [Header("Hanging Title (Judul Gantung)")]
    [Tooltip("RectTransform judul 'Nasi Kotak Jumat Berkah' yang bergantung")]
    [SerializeField] private RectTransform hangingTitle;
    [Tooltip("Seberapa jauh judul mulai dari atas layar (pixel)")]
    [SerializeField] private float titleOffscreenY = 500f;
    [SerializeField] private float titleDropDuration = 0.65f;

    [Header("Clouds (Awan)")]
    [Tooltip("Semua RectTransform awan, langsung muncul fade-in")]
    [SerializeField] private CanvasGroup[] clouds;
    [SerializeField] private float cloudFadeDuration = 0.8f;
    [SerializeField] private float cloudFadeStagger = 0.15f;

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

    private Coroutine _waluyoBobCoroutine;

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

    /// <summary>Panggil ini kalau mau replay animasi intro (misal dari debug)</summary>
    public void ReplayIntro()
    {
        StopAllCoroutines();
        DOTween.KillAll();
        LeanTween.cancelAll();
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
        // Papan: pindah ke atas
        if (signBoard != null)
            signBoard.anchoredPosition = _signBoardOrigin + Vector2.up * signOffscreenY;

        // Tiang: pindah ke bawah
        if (pole != null)
            pole.anchoredPosition = _poleOrigin + Vector2.down * Mathf.Abs(poleOffscreenY);

        // Tombol: pindah ke kanan
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] != null)
                menuButtons[i].anchoredPosition = _buttonOrigins[i] + Vector2.right * buttonOffscreenX;
        }

        // Ustad: scale 0 (pop out effect)
        if (ustad != null)
        {
            ustad.localScale = Vector3.zero;
            ustad.anchoredPosition = _ustadOrigin;
        }

        // Waluyo: posisi awal di kiri
        if (waluyo != null)
        {
            Vector2 pos = _waluyoOrigin;
            pos.x = waluyoStartX;
            waluyo.anchoredPosition = pos;
        }

        // Ayam: scale 0
        if (chicken != null)
            chicken.localScale = Vector3.zero;

        // Judul Gantung: pindah ke atas
        if (hangingTitle != null)
            hangingTitle.anchoredPosition = _hangingTitleOrigin + Vector2.up * titleOffscreenY;

        // Awan: alpha 0
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

        // 1. Awan langsung fade in di background (parallel, tidak nunggu)
        StartCoroutine(AnimateCloudsAppear());

        // 2. Judul gantung jatuh dari atas dengan EaseInBack
        AnimateHangingTitleDrop();
        yield return new WaitForSeconds(titleDropDuration + 0.1f);

        // 3. Tiang naik dari bawah
        AnimatePoleRise();
        yield return new WaitForSeconds(poleRiseDuration * 0.6f);

        // 4. Papan jatuh dari atas (saat tiang setengah jalan)
        AnimateSignDrop();
        yield return new WaitForSeconds(signDropDuration + 0.15f);

        // 5. Tombol slide dari kanan
        yield return StartCoroutine(AnimateButtonsSlideIn());

        // 6. Waluyo jalan masuk
        StartCoroutine(AnimateWaluyoWalk());
        yield return new WaitForSeconds(waluyoWalkDuration * 0.3f);

        // 7. Ustad pop out (saat Waluyo masih jalan)
        AnimateUstadPopOut();
        yield return new WaitForSeconds(ustadPopDuration + 0.1f);

        // 8. Ayam muncul
        yield return new WaitForSeconds(chickenAppearDelay);
        AnimateChickenAppear();

        // 9. Tunggu Waluyo selesai jalan
        yield return new WaitForSeconds(waluyoWalkDuration * 0.7f);

        // 10. Ayam wiggle loop
        yield return new WaitForSeconds(0.3f);
        AnimateChickenWiggle();
    }

    // ===== ANIMATION METHODS =====

    private void AnimateHangingTitleDrop()
    {
        if (hangingTitle == null) return;

        // DOTween: judul jatuh dari atas dengan InBack — terasa seperti "ditarik" lalu lepas
        hangingTitle.DOAnchorPosY(_hangingTitleOrigin.y, titleDropDuration)
                    .SetEase(Ease.InBack, 1.5f);
    }

    private IEnumerator AnimateCloudsAppear()
    {
        if (clouds == null) yield break;

        // DOTween: setiap awan fade in satu per satu dengan stagger kecil
        foreach (CanvasGroup cloud in clouds)
        {
            if (cloud == null) continue;

            DOTween.To(() => cloud.alpha, x => cloud.alpha = x, 1f, cloudFadeDuration)
                   .SetEase(Ease.InOutSine);

            yield return new WaitForSeconds(cloudFadeStagger);
        }
    }

    private void AnimatePoleRise()
    {
        if (pole == null) return;

        // DOTween: tiang naik dari bawah pakai Ease OutBack biar memantul dikit
        pole.DOAnchorPosY(_poleOrigin.y, poleRiseDuration)
            .SetEase(Ease.OutBack, 1.2f);
    }

    private void AnimateSignDrop()
    {
        if (signBoard == null) return;

        // DOTween: papan jatuh dari atas pakai OutBounce biar mantul kayak papan beneran
        signBoard.DOAnchorPosY(_signBoardOrigin.y, signDropDuration)
            .SetEase(Ease.OutBounce);
    }

    private IEnumerator AnimateButtonsSlideIn()
    {
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] == null) continue;

            // DOTween DOAnchorPos — benar untuk UI RectTransform di dalam Canvas
            menuButtons[i].DOAnchorPos(_buttonOrigins[i], buttonSlideDuration)
                          .SetEase(Ease.OutBack);

            yield return new WaitForSeconds(buttonStaggerDelay);
        }

        // Tunggu tombol terakhir selesai animasi
        yield return new WaitForSeconds(buttonSlideDuration);
    }

    private void AnimateUstadPopOut()
    {
        if (ustad == null) return;

        // DOTween DOScale — lebih aman untuk UI, tidak konflik dengan Canvas layout
        ustad.DOScale(Vector3.one, ustadPopDuration)
             .SetEase(Ease.OutBack);
    }

    private IEnumerator AnimateWaluyoWalk()
    {
        if (waluyo == null) yield break;

        // Aktifkan animasi walk di Animator kalau ada
        if (waluyoAnimator != null)
            waluyoAnimator.SetBool(waluyoWalkParam, true);

        float elapsed = 0f;
        float startX = waluyoStartX;
        float endX = waluyoEndX;
        float startY = _waluyoOrigin.y;

        // Semua gerakan pakai anchoredPosition langsung — tidak pakai LeanTween.moveX
        // karena moveX pakai world transform.position yang konflik dengan Canvas layout
        while (elapsed < waluyoWalkDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / waluyoWalkDuration);

            // Smooth horizontal movement (ease InOutSine manual)
            float smoothT = t < 0.5f
                ? 2f * t * t
                : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;

            float currentX = Mathf.Lerp(startX, endX, smoothT);

            // Bob naik-turun selama jalan
            float bob = Mathf.Sin(elapsed * walkBobFrequency * Mathf.PI) * walkBobAmplitude;

            waluyo.anchoredPosition = new Vector2(currentX, startY + bob);
            yield return null;
        }

        // Snap ke posisi akhir yang bersih
        waluyo.anchoredPosition = new Vector2(endX, startY);

        // Stop animasi walk
        if (waluyoAnimator != null)
            waluyoAnimator.SetBool(waluyoWalkParam, false);
    }

    private void AnimateChickenAppear()
    {
        if (chicken == null) return;

        // DOTween: ayam muncul dari scale 0 dengan OutBack biar kelihatan nyemplung
        chicken.DOScale(Vector3.one, 0.4f)
               .SetEase(Ease.OutBack, 1.5f);
    }

    private void AnimateChickenWiggle()
    {
        if (chicken == null) return;

        // DOTween: wiggle rotasi bolak-balik
        chicken.DORotate(new Vector3(0f, 0f, chickenWiggleAngle), chickenWiggleDuration)
               .SetEase(Ease.InOutSine)
               .SetLoops(chickenWiggleLoops, LoopType.Yoyo)
               .OnComplete(StartChickenIdleWiggle);
    }

    /// <summary>Setelah wiggle awal selesai, ayam terus goyang pelan (idle)</summary>
    private void StartChickenIdleWiggle()
    {
        if (chicken == null) return;

        chicken.DORotate(new Vector3(0f, 0f, chickenWiggleAngle * 0.4f), chickenWiggleDuration * 1.5f)
               .SetEase(Ease.InOutSine)
               .SetLoops(-1, LoopType.Yoyo); // -1 = infinite
    }

    // ===== HELPERS =====

    private void OnDestroy()
    {
        // Bersihkan semua DOTween tween saat object destroy
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
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Target Nasi Kotak")]
    public int targetNasiKotak = 10;
    private int _nasiKotakTerkirim = 0;

    [Header("Statistik Permainan")]
    public int totalScore = 0;
    public float timeRemaining = 60f;
    public bool isGameActive = true;

    [Header("UI Reference")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI misiText;

    [HideInInspector] public bool canCountdown = true;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        TimeRemaining();
    }

    public void CheatCode()
    {

    }

    // FUNGSI BARU UNTUK LEVEL END UI (Menyelesaikan Error 1)
    public int getNasiKotakTerkirim()
    {
        return _nasiKotakTerkirim;
    }

    public void NasiKotakDelivered()
    {
        if(!isGameActive)
        {
            return;
        }
        _nasiKotakTerkirim++;

        UpdateMisiDisplay();

        Debug.Log("Nasi Kotak Terkirim: " + _nasiKotakTerkirim + " / " + targetNasiKotak);

        // Langsung cek kondisi menang untuk memunculkan hint (Menyelesaikan Error 2)
        CheckWinCondition();
    }

    public void TimeRemaining()
    {
        if (isGameActive && canCountdown)
        {
            if(timeRemaining > 0)
            {
                //kurangi waktu setiap detik
                timeRemaining -= Time.deltaTime;
                UpdateTimerDisplay();
            }
            else
            {
                //waktu habis
                timeRemaining = 0;
                isGameActive = false;
                GameOver();
            }
        }
    }

    public void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        //ganti format menjadi Menit::Detik
        float minutes = Mathf.FloorToInt(timeRemaining / 60);
        float seconds = Mathf.FloorToInt(timeRemaining % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        //Beri warna merah jika waktu dibawah 10 detik
        if(timeRemaining <= 10f)
        {
            timerText.color = Color.red;
        }
    }

    public void UpdateMisiDisplay()
    {
        if (misiText != null)
        {
            misiText.text = _nasiKotakTerkirim + " / " + targetNasiKotak;
        }
    }

    public void AddScore(int amount)
    {
        totalScore += amount;
        Debug.Log("Alhamdulillah Nasi Terkirim : " + totalScore);
    }

    public void GameOver()
    {
        isGameActive = false;
        canCountdown = false;
        Debug.Log("Game Over");
        LevelEndUI.Instance.ShowLosePanel(_nasiKotakTerkirim, targetNasiKotak, "Waktu Habis!");
    }

    public void RestartLevel()
    {
        Debug.Log("Mengulang Level...");
        Time.timeScale = 1f;
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    // ===== ALUR ENDGAME =====
    public void CheckWinCondition()
    {
        if (_nasiKotakTerkirim >= targetNasiKotak)
        {
            // Panggil persiapan untuk lapor Ustadz (Bukan lagi memanggil Panel Win)
            PrepareEndGame();
        }
    }

    private void PrepareEndGame()
    {
        // Matikan timer agar waktu berhenti saat target tercapai
        canCountdown = false; 

        // --- TAMBAHAN PENTING: PAKSA UNFREEZE PLAYER ---
        // Memastikan Waluyo tidak nyangkut/freeze dan bisa jalan ke Masjid
        isGameActive = true;
        Time.timeScale = 1f;

        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null) player.enabled = true;

        CameraController cam = FindObjectOfType<CameraController>();
        if (cam != null) cam.enabled = true;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        // -----------------------------------------------

        // Munculkan Hint bahwa misi sukses dan harus kembali
        if (HintManager.Instance != null)
            HintManager.Instance.ShowSpecificHint("Alhamdulillah semua nasi terbagi! Ayo lapor ke Pak Ustadz di Masjid.");

        // Nyalakan trigger dialog akhir di masjid
        DialogueTrigger[] triggers = FindObjectsOfType<DialogueTrigger>(true);
        foreach (DialogueTrigger dt in triggers)
        {
            if (dt.isEndGameTrigger)
            {
                dt.gameObject.SetActive(true);
            }
        }
    }
}
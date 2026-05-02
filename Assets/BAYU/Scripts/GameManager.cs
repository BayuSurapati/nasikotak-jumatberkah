using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Statistik Permainan")]
    public int totalScore = 0;
    public float timeRemaining = 60f;
    public bool isGameActive = true;

    [Header("UI Reference")]
    public TextMeshProUGUI timerText;

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
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        TimeRemaining();
    }

    public void CheatCode()
    {

    }

    public void TimeRemaining()
    {
        if (isGameActive)
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

    public void AddScore(int amount)
    {
        totalScore += amount;
        Debug.Log("Alhamdulillah Nasi Terkirim : " + totalScore);
    }

    public void GameOver()
    {
        Debug.Log("Game Over");
    }

    public void RestartLevel()
    {
        Debug.Log("Mengulang Level...");
        Time.timeScale = 1f;
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}

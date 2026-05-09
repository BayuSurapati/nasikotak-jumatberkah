using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public static AudioManager instance;

    [Header("Audio Sources")]
    [Tooltip("Komponen AudioSource untuk Musik")]
    public AudioSource musicSource;

    [Tooltip("Komponen AudioSource untuk sound effects")]
    public AudioSource sfxSource;
    [Tooltip("Komponen AudioSource khusus untuk Dialog")]
    public AudioSource dialogSource;


    //Struktur data kustom untuk menyimpan nama file audio

    [Serializable]
    public struct Sound
    {
        public string name;
        public AudioClip clip;
    }

    [Header("Audio Clips Library")]
    public Sound[] musicSounds;
    public Sound[] sfxSounds;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
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
        
    }

    public void PlayMusic(string soundName)
    {
        Sound s = Array.Find(musicSounds, x => x.name == soundName);

        if(s.clip == null)
        {
            Debug.LogWarning("Musik: " +  soundName + " Tidak ditemukan");
            return;
        }
        musicSource.clip = s.clip;
        musicSource.Play();
    }

    public void PlaySFX(string soundName)
    {
        Sound s = Array.Find(sfxSounds, x => x.name == soundName);

        if (s.clip == null)
        {
            Debug.LogWarning("Musik: " + soundName + " Tidak ditemukan");
            return;
        }

        sfxSource.PlayOneShot(s.clip);
    }

    // Fungsi untuk memutar dialog (akan memotong dialog sebelumnya jika ada yang sedang bunyi)
    public void PlayDialog(string soundName)
    {
        Sound s = Array.Find(sfxSounds, x => x.name == soundName);

        if (s.clip == null)
        {
            Debug.LogWarning("Dialog: " + soundName + " tidak ditemukan!");
            return;
        }

        // Kita BUKAN menggunakan PlayOneShot, melainkan mengganti clip utama lalu Play()
        dialogSource.clip = s.clip;
        dialogSource.Play();
    }

    // Fungsi untuk menghentikan dialog secara spesifik
    public void StopDialog()
    {
        if (dialogSource.isPlaying)
        {
            dialogSource.Stop();
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void ToggleMusic()
    {
        musicSource.mute = !musicSource.mute;
    }

    public void ToggleSFX()
    {
        sfxSource.mute = !sfxSource.mute;
    }
}

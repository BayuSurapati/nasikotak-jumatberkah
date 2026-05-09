using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string name; // Nama karakter (Waluyo / Pak Ustadz)
    
    [TextArea(3, 5)] 
    public string sentence; // Isi omongannya
    
    public bool isWaluyo; // Untuk menentukan animasi mana yang aktif
}
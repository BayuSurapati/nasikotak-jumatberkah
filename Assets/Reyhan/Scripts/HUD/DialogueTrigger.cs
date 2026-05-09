using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Tooltip("Daftar percakapan yang akan muncul")]
    public List<DialogueLine> startDialogue;
    
    [Header("Pengaturan Akhir Game")]
    [Tooltip("Centang ini HANYA JIKA ini adalah dialog laporan ke Ustadz di akhir game")]
    public bool isEndGameTrigger = false;

    private bool _hasTriggered = false;

  private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !_hasTriggered)
        {
            // SEMBUNYIKAN HINT SAAT KETEMU USTADZ
            if (HintManager.Instance != null)
            {
                HintManager.Instance.HideHint();
            }

            DialogueSystem.Instance.StartDialogue(startDialogue, isEndGameTrigger);
            _hasTriggered = true; 
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_PenerimaNasi : MonoBehaviour
{
    [Header("Pengaturan NPC Penerima")]
    [Tooltip("Titik Kosong di tangan penerima untuk memegang nasi kotak")]
    public Transform holdPoint;

    private bool _sudahDapatNasi = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_sudahDapatNasi) return;

        if (other.CompareTag("Player"))
        {
            PlayerInteract player = other.GetComponent<PlayerInteract>();

            if(player != null && player.HasItem())
            {
                player.GiveTopItem(holdPoint);
                _sudahDapatNasi = true;
                Debug.Log("Terimakasih");
            }
        }
    }
}

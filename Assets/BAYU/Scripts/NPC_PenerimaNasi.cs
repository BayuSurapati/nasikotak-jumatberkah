using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_PenerimaNasi : MonoBehaviour
{
    [Header("Pengaturan NPC Penerima")]
    [Tooltip("Titik Kosong di tangan penerima untuk memegang nasi kotak")]
    public Transform holdPoint;

    [Tooltip("Masukkan Child 3D yang ada animatornya")]
    public Animator animator;

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
            GameManager.Instance.NasiKotakDelivered();

            if (player != null && player.HasItem())
            {
                player.GiveTopItem(holdPoint);
                _sudahDapatNasi = true;

                if(animator != null)
                {
                    animator.SetBool("IsHappy", true);
                }
                else
                {
                    Debug.Log("Animator belum dimasukkan");
                }

                Debug.Log("Terimakasih");
            }
        }
    }
}

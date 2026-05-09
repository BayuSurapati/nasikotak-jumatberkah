using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBoost : MonoBehaviour
{
    [Header("Pengaturan Efek item")]
    [Tooltip("Berapa kali lipat karakter lebih cepat")]
    public float speedMultiplier = 2f;
    [Tooltip("Durasi efek")]
    public float boostDuration = 2f;
    [Header("Stamina Refill")]
    [SerializeField] private float _staminaRefillAmount = 50f;
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
        if (other.CompareTag("Player"))
        {
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();

            if (playerMovement != null)
            {
                playerMovement.ApplySpeedBoost(speedMultiplier, boostDuration);
                playerMovement.RefillStamina(_staminaRefillAmount);
                Destroy(gameObject);
            }
        }
    }
}

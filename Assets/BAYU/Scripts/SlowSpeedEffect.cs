using UnityEngine;

public class SlowSpeedEffect : MonoBehaviour
{
    // Fungsi ini akan dipanggil otomatis oleh Unity saat ada OBJEK APA PUN menyentuh trigger
    private void OnTriggerEnter(Collider other)
    {
        // Sengaja kita hilangkan pengecekan Tag "Player" untuk melihat apakah trigger ini hidup
        Debug.Log("🚨 BAAAMM! Ada yang injak air! Namanya: " + other.gameObject.name);

        PlayerMovement udin = other.GetComponent<PlayerMovement>();

        // Coba cari di Parent kalau di objek ini tidak ada
        if (udin == null)
        {
            udin = other.GetComponentInParent<PlayerMovement>();
        }

        if (udin != null)
        {
            udin.EnterWater();
            Debug.Log("✅ Berhasil centang bool isInWater milik: " + udin.gameObject.name);
        }
        else
        {
            Debug.Log("❌ Yang injak air tidak punya script PlayerMovement!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerMovement udin = other.GetComponent<PlayerMovement>();
        if (udin == null) udin = other.GetComponentInParent<PlayerMovement>();

        if (udin != null) udin.ExitWater();
    }
}
using System.Collections.Generic; // Wajib ditambahkan untuk menggunakan List
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInteract : MonoBehaviour
{
    [Header("Pengaturan Ambil Barang")]
    [Tooltip("Titik kosong di depan Udin tempat barang menempel")]
    public Transform holdPoint;
    [Tooltip("Jarak jangkauan tangan Udin")]
    public float grabRadius = 1.5f;
    [Tooltip("Pilih Layer khusus barang yang bisa diambil")]
    public LayerMask itemLayer;

    [Header("Pengaturan Tumpukan (Stacking)")]
    [Tooltip("Tinggi satu nasi kotak agar tumpukan selanjutnya pas di atasnya")]
    public float boxHeight = 1f;
    [Tooltip("Maksimal nasi kotak yang bisa dibawa Udin sekaligus")]
    public int maxStack = 5;
    [SerializeField]
    private GameObject pp;


    // Mengganti currentItem tunggal menjadi List untuk menyimpan banyak barang
    private List<GameObject> carriedItems = new List<GameObject>();

    public void OnInteract(InputValue value)
    {
        if (value.isPressed)
        {
            bool grabbedSomething = false;

            // Jika tangan belum penuh, coba cari dan ambil barang
            if (carriedItems.Count < maxStack)
            {
                grabbedSomething = TryGrabItem();
                
            }

            // Jika kita menekan tombol tapi TIDAK ada barang yang bisa diambil 
            // DAN kita sedang membawa barang, maka jatuhkan semuanya.
            if (!grabbedSomething && carriedItems.Count > 0)
            {
                DropTopItem();
            }
        }
    }

    public void CheckItemCount()
    {
        if(carriedItems.Count <= 0)
        {
            pp.SetActive(true);
        }
        else if(carriedItems.Count > 0)
        {
            pp.SetActive(false);
        }
    }

    private bool TryGrabItem()
    {
        Vector3 grabCenter = transform.position + transform.forward * 1f;
        Collider[] foundItems = Physics.OverlapSphere(grabCenter, grabRadius, itemLayer);

        foreach (Collider col in foundItems)
        {
            GameObject item = col.gameObject;

            // Pastikan kita tidak mencoba mengambil barang yang SEDANG kita bawa
            if (!carriedItems.Contains(item))
            {
                Rigidbody itemRb = item.GetComponent<Rigidbody>();
                Collider itemCollider = item.GetComponent<Collider>();

                if (itemRb != null) itemRb.isKinematic = true;
                if (itemCollider != null) itemCollider.enabled = false;

                // LOGIKA TUMPUKAN: Hitung posisi Y ke atas berdasarkan jumlah barang yang sudah dibawa
                float currentHeightOffset = carriedItems.Count * boxHeight;

                // Posisikan barang baru di atas tumpukan sebelumnya
                item.transform.position = holdPoint.position + (Vector3.up * currentHeightOffset);
                item.transform.rotation = holdPoint.rotation;
                item.transform.SetParent(holdPoint);

                // Tambahkan ke daftar barang bawaan
                carriedItems.Add(item);

                return true; // Berhasil mengambil 1 barang baru, hentikan pencarian
            }
        }

        return false; // Tidak ada barang baru di sekitar yang bisa diambil
    }

    private void DropTopItem()
    {
        // 1. Cari tahu indeks kotak paling atas (yaitu kotak terakhir yang ditambahkan ke List)
        int topIndex = carriedItems.Count - 1;

        // 2. Ambil referensi objek kotaknya
        GameObject topItem = carriedItems[topIndex];

        // 3. Lepaskan dari tangan Udin
        topItem.transform.SetParent(null);

        // (Opsional) Agar kotak tidak jatuh persis di dalam tubuh Udin, kita geser sedikit ke depan
        topItem.transform.position = transform.position + transform.forward * 1f + Vector3.up * 0.5f;

        // 4. Nyalakan kembali fisikanya agar jatuh ke tanah
        Rigidbody itemRb = topItem.GetComponent<Rigidbody>();
        Collider itemCollider = topItem.GetComponent<Collider>();

        if (itemRb != null) itemRb.isKinematic = false;
        if (itemCollider != null) itemCollider.enabled = true;

        // 5. Hapus kotak tersebut dari daftar bawaan Udin
        carriedItems.RemoveAt(topIndex);
    }

    public bool HasItem()
    {
        return carriedItems.Count > 0;
    }

    public void GiveTopItem(Transform receiverHoldPoint)
    {
        int topIndex = carriedItems.Count - 1;
        GameObject topItem = carriedItems[topIndex];

        topItem.transform.SetParent(receiverHoldPoint);
        topItem.transform.position = receiverHoldPoint.position;
        topItem.transform.rotation = receiverHoldPoint.rotation;

        carriedItems.RemoveAt(topIndex);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(1);
        }
        else
        {
            Debug.LogWarning("Game Manager tidak ditemukan.");
        }
    }

    private void DropAllItems()
    {
        // Jatuhkan semua barang yang ada di dalam List
        foreach (GameObject item in carriedItems)
        {
            if (item != null)
            {
                item.transform.SetParent(null);

                Rigidbody itemRb = item.GetComponent<Rigidbody>();
                Collider itemCollider = item.GetComponent<Collider>();

                // Nyalakan fisika agar tumpukannya jatuh berhamburan ke tanah
                if (itemRb != null) itemRb.isKinematic = false;
                if (itemCollider != null) itemCollider.enabled = true;
            }
        }

        // Kosongkan daftar bawaan Udin
        carriedItems.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 grabCenter = transform.position + transform.forward * 1f;
        Gizmos.DrawWireSphere(grabCenter, grabRadius);
    }
}
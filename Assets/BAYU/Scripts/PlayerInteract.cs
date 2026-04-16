using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    [Header("Pengaturan Ambil Barang")]
    [Tooltip("Titik kosong di depan Udin tempat barang menempel")]
    public Transform holdPoint;
    [Tooltip("Jarak jangkauan tangan Udin")]
    public float grabRadius = 1.5f;
    [Tooltip("Pilih Layer khusus barang yang bisa diambil")]
    public LayerMask itemLayer;

    private GameObject currentItem;
    private Rigidbody itemRb;
    private Collider itemCollider;

    // Fungsi otomatis dari New Input System saat tombol Interact ditekan
    public void OnInteract(InputValue value)
    {
        if (value.isPressed)
        {
            // Jika tangan kosong, coba ambil barang. Jika bawa barang, jatuhkan.
            if (currentItem == null)
            {
                TryGrabItem();
            }
            else
            {
                DropItem();
            }
        }
    }

    private void TryGrabItem()
    {
        // Menentukan titik pusat pencarian (1 meter di depan Udin)
        Vector3 grabCenter = transform.position + transform.forward * 1f;

        // Mencari semua objek dengan layer "Item" di dalam radius lingkaran
        Collider[] foundItems = Physics.OverlapSphere(grabCenter, grabRadius, itemLayer);

        if (foundItems.Length > 0)
        {
            // Ambil barang pertama yang terdeteksi
            currentItem = foundItems[0].gameObject;
            itemRb = currentItem.GetComponent<Rigidbody>();
            itemCollider = currentItem.GetComponent<Collider>();

            // Matikan fisikanya agar tidak jatuh dan tidak nabrak kaki Udin
            if (itemRb != null) itemRb.isKinematic = true;
            if (itemCollider != null) itemCollider.enabled = false;

            // Pindahkan barang ke tangan Udin dan jadikan 'Anak' (Parenting)
            currentItem.transform.position = holdPoint.position;
            currentItem.transform.rotation = holdPoint.rotation;
            currentItem.transform.SetParent(holdPoint);
        }
    }

    private void DropItem()
    {
        // Lepaskan barang dari tangan Udin
        currentItem.transform.SetParent(null);

        // Nyalakan kembali fisikanya agar barang jatuh ke tanah
        if (itemRb != null) itemRb.isKinematic = false;
        if (itemCollider != null) itemCollider.enabled = true;

        // Kosongkan memori tangan Udin
        currentItem = null;
        itemRb = null;
        itemCollider = null;
    }

    // Fitur tambahan agar Anda bisa melihat area jangkauan tangan Udin berupa bola kuning di Unity Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 grabCenter = transform.position + transform.forward * 1f;
        Gizmos.DrawWireSphere(grabCenter, grabRadius);
    }
}
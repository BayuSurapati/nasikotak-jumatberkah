using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Pengaturan Patroli")]
    public Transform[] waypoints;
    public float moveSpeed = 3f;
    public float rotationSpeed = 10f;

    [Header("Pengaturan Efek Stun")]
    public float stunDuration = 2f;

    private int _currentWayPointIndex = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (waypoints.Length == 0) return;
        Patrol();
    }

    private void Patrol()
    {
        //Target titik tujuan patrol
        Transform target = waypoints[_currentWayPointIndex];

        //Gerakan ayam ke target patroli
        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        //Putaran ayam mengahadap target patroli
        Vector3 direction = -(target.position - transform.position).normalized;

        if(direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        //Jika sampai titik tujuan lanjut ke titik selanjutnya
        if (Vector3.Distance(transform.position, target.position) < 0.2f)
        {
            _currentWayPointIndex = (_currentWayPointIndex + 1) % waypoints.Length;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            if(playerMovement != null)
            {
                playerMovement.GetStunned(stunDuration);
            }
        }
    }
}

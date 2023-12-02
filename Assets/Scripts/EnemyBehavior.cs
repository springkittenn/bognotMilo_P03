using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    [SerializeField] public float detectionRange = 7f;
    public bool playerInRange = false;
    private Transform player;
    public AudioSource alert;

    // Shooting Mechanics
    /*public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public float bulletSpeed = 8f;
    public float shootInterval = 2f;
    private float timeSinceLastShot = 0f;*/

    // AFK Behaviour Parameters
    public float lookAroundInterval = 4f;
    private float timeSinceLastLook = 0f;
    private bool isLookingAround = false;
    public float turningSpeed = 180f;

    private Quaternion initialRotation;
    private Quaternion targetRotation;

    // -------------------------------------------------------

    void Start()
    {
        initialRotation = transform.rotation;
        targetRotation = initialRotation;
    }

    void Update()
    {
        player = GameObject.FindWithTag("Player").transform;

        // Calculate the distance to the player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check if the player is within the detection range
        if (distanceToPlayer <= detectionRange)
        {
            if (!playerInRange)
            {
                alert.Play();
            }
            playerInRange = true;

            if (playerInRange)
            {

                // Calculate the direction to the player
                Vector3 directionToPlayer = player.position - transform.position;

                // Ignore the Y-axis rotation
                directionToPlayer.y = 0;

                // Create a rotation that looks at the player
                targetRotation = Quaternion.LookRotation(directionToPlayer);

                // Keep track of the time since the last shot
                timeSinceLastShot += Time.deltaTime;

                /*// Check if it's time to shoot
                if (timeSinceLastShot >= shootInterval)
                {
                    ShootBullet();
                    timeSinceLastShot = 0f; // Reset the timer
                }*/

                // Reset look around state
                isLookingAround = false;
            }
        }
        else
        {

            playerInRange = false;

            // While the player is not in range, make the enemy look around gradually
            timeSinceLastLook += Time.deltaTime;

            if (timeSinceLastLook >= lookAroundInterval)
            {
                StartLookingAround();
                timeSinceLastLook = 0f; // Reset the timer
            }
        }

        // Update rotation gradually with a faster turning speed
        if (isLookingAround)
        {
            float step = turningSpeed * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, step);
        }
        else
        {
            // Always face the player if in range
            float step = turningSpeed * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, step);
        }

        // Implement your behavior based on the playerInRange flag.
    }

    /*void ShootBullet()
    {
        // Instantiate a new bullet from the prefab
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.LookRotation(bulletSpawnPoint.forward));

        // Set the bullet's speed
        BulletScript bulletScript = bullet.GetComponent<BulletScript>();
        if (bulletScript != null)
        {
            bulletScript.SetSpeed(bulletSpeed);
        }
    }*/

    void StartLookingAround()
    {
        // Randomly set the target rotation for looking around
        float randomYRotation = Random.Range(0f, 360f);
        targetRotation = initialRotation * Quaternion.Euler(0, randomYRotation, 0);
        isLookingAround = true;
    }
}

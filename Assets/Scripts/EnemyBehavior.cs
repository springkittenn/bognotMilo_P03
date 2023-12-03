using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    // PLAYER VAR
    public bool playerInRange = false;
    private Transform player;

    // MOVEMENT VAR
    public float turningSpeed = 300f; // Adjust the turning speed for faster rotations
    public float detectionRange = 8f;
    public float lookAroundInterval = 4f;
    private float timeSinceLastLook = 0f;
    private bool isLookingAround = false;

    private Quaternion initialRotation;
    private Quaternion targetRotation;

    // ALERT VAR
    public float fieldOfViewAngle = 90f;
    public AudioSource alert;

    //-------------------------------------------------------------------------------------------

    void Start()
    {
        initialRotation = transform.rotation;
        targetRotation = initialRotation;
    }

    void Update()
    {
        // Find the player GameObject (you can use a tag, layer, or other methods)
        player = GameObject.FindWithTag("Player").transform;

        // Calculate the distance and direction to the player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        Vector3 directionToPlayer = player.position - transform.position;

        // Calculate the angle between the enemy's forward vector and the direction to the player
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        // Check if the player is within the detection range
        if (distanceToPlayer <= detectionRange && angleToPlayer <= fieldOfViewAngle * 0.5f)
        {
            if (!playerInRange)
            {
                alert.Play();
            }
            playerInRange = true;

            if (playerInRange)
            {
                // Ignore the Y-axis rotation
                directionToPlayer.y = 0;

                // Create a rotation that looks at the player
                targetRotation = Quaternion.LookRotation(directionToPlayer);

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
    }

    void StartLookingAround()
    {
        // Randomly set the target rotation for looking around
        float randomYRotation = Random.Range(0f, 360f);
        targetRotation = initialRotation * Quaternion.Euler(0, randomYRotation, 0);
        isLookingAround = true;
    }
}

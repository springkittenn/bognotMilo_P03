using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    // PLAYER VAR
    [Header("Player Stats (No Interaction Needed)")]
    [Tooltip("Shows if the Player is in the line-of-sight of the Enemy.")]
    public bool playerInRange = false;
    private Transform player;

    // IDLE VAR
    [Header("Idle Stats")]
    [Range(180, 500)] [Tooltip("Determines how fast the Enemy turns as it looks around. Regular Range: 180-500")]
    public float turningSpeed = 300f;
    [Range(1, 20)][Tooltip("Determines how far can your Enemy detect the player. Regular Range: 1-20")] 
    public float detectionRange = 8f;
    [Range(1, 10)][Tooltip("Determines how often your Enemy looks around during idle state. Regular Range: 1-10")] 
    public float lookAroundInterval = 4f;

    private float timeSinceLastLook = 0f;
    private bool isLookingAround = false;
    private Quaternion initialRotation, targetRotation;

    // ALERT VAR
    [Header("Alert Stats")]
    [Range(45, 180)][Tooltip("Determines how wide the Enemy's view angle is to detect the player. Regular Range: 45-180 " +
        "(Think of it like angle degrees)")] 
    public float fieldOfViewAngle = 90f;
    [Range(1,5)][Tooltip("Determines how fast the Enemy chases the player once spotted. Remember to consider" +
        " the Player's movement speed. Regular Range: 1-5")] 
    public float chaseSpeed = 2f;
    [Tooltip("Assign AudioSource to play an Alert sound when the Player is spotted.")] public AudioSource alertSound;
    private bool playerJustEnteredSight = false;

    //-------------------------------------------------------------------------------------------

    void Start()
    {
        initialRotation = transform.rotation;
        targetRotation = initialRotation;
    }

    void Update()
    {
        // Find player and position, measure distance between Enemy and Player
        player = GameObject.FindWithTag("Player").transform;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        Vector3 directionToPlayer = player.position - transform.position;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        if (distanceToPlayer <= detectionRange && angleToPlayer <= fieldOfViewAngle * 0.5f)
        {
            if (HasLineOfSightToPlayer(directionToPlayer))
            {
                if (!playerInRange)
                {
                    playerJustEnteredSight = true;
                }

                playerInRange = true;
                isLookingAround = false;
                directionToPlayer.y = 0;

                targetRotation = Quaternion.LookRotation(directionToPlayer);
                transform.position = Vector3.MoveTowards(transform.position, player.position, chaseSpeed * Time.deltaTime);
                targetRotation = Quaternion.LookRotation(player.position - transform.position);
                float step = turningSpeed * Time.deltaTime;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, step);

                if (playerJustEnteredSight)
                {
                    alertSound.Play();
                    playerJustEnteredSight = false; // Reset
                }
                else
                {
                    isLookingAround = true;
                }
            }
        }
        else
        {
            playerInRange = false;
            isLookingAround = true;

            timeSinceLastLook += Time.deltaTime;

            if (timeSinceLastLook >= lookAroundInterval)
            {
                StartLookingAround();
                timeSinceLastLook = 0f; // Reset
            }
        }

        // Smooth rotations
        if (isLookingAround)
        {
            float step = turningSpeed * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, step);
        }
        else
        {
            float step = turningSpeed * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, step);
        }
    }

    //-------------------------------------------------------------------------------------------

    void StartLookingAround()
    {
        float randomYRotation = Random.Range(0f, 360f);
        targetRotation = initialRotation * Quaternion.Euler(0, randomYRotation, 0);
        isLookingAround = true;
    }

    bool HasLineOfSightToPlayer(Vector3 directionToPlayer)
    {
        RaycastHit hit;
        Debug.DrawRay(transform.position, directionToPlayer.normalized * detectionRange, Color.red);

        if (Physics.Raycast(transform.position, directionToPlayer, out hit, detectionRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                Debug.DrawRay(transform.position, directionToPlayer.normalized * detectionRange, Color.green);
                return true;
            }
        }
        return false;
    }
}
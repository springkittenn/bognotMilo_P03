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
    [Range(180, 500)] [Tooltip("Determines how fast the Enemy turns as it looks around. " +
        "Regular Range: 180-500")]
    public float turningSpeed = 300f;
    [Range(1, 20)][Tooltip("Determines how far can your Enemy detect the player. Regular " +
        "Range: 1-20")] 
    public float detectionRange = 8f;
    [Range(1, 10)][Tooltip("Determines how often your Enemy looks around during idle state." +
        " Regular Range: 1-10")] 
    public float lookAroundInterval = 4f;

    private float timeSinceLastLook = 0f;
    private Quaternion initialRotation, targetRotation;

    // ALERT VAR
    [Header("Alert Stats")]
    [Range(45, 180)][Tooltip("Determines how wide the Enemy's view angle is to detect the" +
        " player. Regular Range: 45-180 (Think of it like angle degrees)")] 
    public float fieldOfViewAngle = 90f;
    [Range(1,5)][Tooltip("Determines how fast the Enemy chases the player once spotted. " +
        "Remember to consider" +
        " the Player's movement speed. Regular Range: 1-5")] 
    public float chaseSpeed = 2f;
    [Tooltip("Assign AudioSource to play an Alert sound when the Player is spotted.")]
    public AudioSource alertSound;

    private bool playerJustEnteredSight = false;

//-------------------------------------------------------------------------------------------

    void Start()
    {
        initialRotation = transform.rotation;
        targetRotation = initialRotation;
    }

    void Update()
    {
        float distanceToPlayer, angleToPlayer;
        Vector3 directionToPlayer;
        MeasureDistanceToPlayer(out distanceToPlayer, out directionToPlayer, out angleToPlayer);

        if (distanceToPlayer <= detectionRange && angleToPlayer <= fieldOfViewAngle * 0.5f)
        {
            if (HasLineOfSightToPlayer(directionToPlayer))
            {
                if (!playerInRange)
                {
                    playerJustEnteredSight = true;
                    if (playerJustEnteredSight)
                    {
                        EnemyAlert();
                    }
                }
                SmoothRotations();
                GoToPlayer(directionToPlayer);
            }
        }
        else
        {
            SmoothRotations();
            IdleLookAround();
        }
    }

//-------------------------------------------------------------------------------------------

    // FUNCTIONS
    private void StartLookingAround()
    {
        float randomYRotation = Random.Range(0f, 360f);
        targetRotation = initialRotation * Quaternion.Euler(0, randomYRotation, 0);
    }

    private void MeasureDistanceToPlayer(out float distanceToPlayer, 
        out Vector3 directionToPlayer, out float angleToPlayer)
    {
        player = GameObject.FindWithTag("Player").transform;
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
        directionToPlayer = player.position - transform.position;
        angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
    }

    private void IdleLookAround()
    {
        playerInRange = false;

        timeSinceLastLook += Time.deltaTime;

        if (timeSinceLastLook >= lookAroundInterval)
        {
            StartLookingAround();
            timeSinceLastLook = 0f; // Reset
        }
    }

    private void SmoothRotations()
    {
        float step = turningSpeed * Time.deltaTime;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, step);
    }

    private void EnemyAlert()
    {
        alertSound.Play();
        playerJustEnteredSight = false; // Reset
    }

    private void GoToPlayer(Vector3 directionToPlayer)
    {
        playerInRange = true;
        directionToPlayer.y = 0;

        targetRotation = Quaternion.LookRotation(directionToPlayer);
        transform.position = Vector3.MoveTowards(transform.position, player.position, 
            chaseSpeed * Time.deltaTime);
        targetRotation = Quaternion.LookRotation(player.position - transform.position);
        float step = turningSpeed * Time.deltaTime;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, step);
    }

//-------------------------------------------------------------------------------------------

    bool HasLineOfSightToPlayer(Vector3 directionToPlayer)
    {
        RaycastHit hit;
        Debug.DrawRay(transform.position, directionToPlayer.normalized * detectionRange, 
            Color.red);

        if (Physics.Raycast(transform.position, directionToPlayer, out hit, detectionRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                Debug.DrawRay(transform.position, directionToPlayer.normalized * 
                    detectionRange, Color.green);
                return true;
            }
        }
        return false;
    }
}
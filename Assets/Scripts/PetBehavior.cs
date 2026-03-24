using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetBehavior : MonoBehaviour
{
    public ArduinoReader arduinoReader;

    // ===== Jump parameters =====
    public float jumpHeight = 2f;
    public float jumpDuration = 0.5f; // seconds
    private bool isJumping = false;
    private float jumpStartTime;
    private Vector3 startPos;

    // ===== Shake parameters =====
    public float shakeAmount = 0.2f;
    public float shakeDuration = 0.5f; // seconds
    private bool isShaking = false;
    private float shakeEndTime = 0f;


    // ===== Walk parameters =====
    public float walkSpeed = 2f;
    public float approachDistance = 2f; // distance to camera
    private bool isWalking = false;
    private Vector3 walkTarget;

    void Start()
    {
        startPos = transform.position;
        walkTarget = startPos;
    }

    void Update()
    {
        if (arduinoReader == null) return;

        HandleJump();
        HandleShake();
        HandleWalk();
    }

    // ===== JUMP when tilt goes from OFF → ON =====
    void HandleJump()
    {
        bool currentTilt = arduinoReader.isTilted && arduinoReader.tiltBtnPressed;

        if (currentTilt && !isJumping)
        {
            isJumping = true;
            jumpStartTime = Time.time;
            Debug.Log("Pet state: PLAY (jump)");
        }

        if (isJumping)
        {
            float t = (Time.time - jumpStartTime) / jumpDuration;
            if (t < 1f)
            {
                // single smooth jump
                float newY = startPos.y + Mathf.Sin(t * Mathf.PI) * jumpHeight;
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }
            else
            {
                // end jump
                transform.position = startPos;
                isJumping = false;
            }
        }
    }

    // ===== SHAKE when sound detected =====
    void HandleShake()
    {
        // trigger shake if sound detected this frame
        if (arduinoReader.soundTriggered && !isShaking)
        {
            isShaking = true;
            shakeEndTime = Time.time + shakeDuration;
            Debug.Log("Pet state: CLEANING (shake)");

            // consume the trigger so it won't repeat immediately
            arduinoReader.ConsumeSoundTrigger();
        }

        // shake while within duration
        if (isShaking)
        {
            Vector3 offset = Random.insideUnitSphere * shakeAmount;
            transform.position = startPos + offset;

            if (Time.time > shakeEndTime)
            {
                transform.position = startPos;
                isShaking = false;
            }
        }
    }

    // ===== WALK toward camera when distance <= 10 and distance button pressed =====
    void HandleWalk()
    {
        bool shouldApproach = arduinoReader.distPressed &&
                              arduinoReader.smoothedDistance > 0f &&
                              arduinoReader.smoothedDistance <= 10f;

        Vector3 cameraPos = Camera.main != null ? Camera.main.transform.position : startPos;

        if (shouldApproach)
        {
            Vector3 dir = (cameraPos - startPos).normalized;
            walkTarget = startPos + dir * approachDistance;
            isWalking = true;
            Debug.Log("Pet state: EATING (approach)");
        }
        else
        {
            walkTarget = startPos;
            isWalking = true;
        }

        if (isWalking)
        {
            transform.position = Vector3.MoveTowards(transform.position, walkTarget, walkSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, walkTarget) < 0.01f)
            {
                isWalking = false;
            }
        }
    }
}
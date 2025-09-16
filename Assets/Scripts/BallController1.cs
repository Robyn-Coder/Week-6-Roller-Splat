using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public Rigidbody ballRb;
    public ParticleSystem dirtParticle;
    public float speed = 15.0f;

    [Header("Sound Effects")]
    public AudioSource audioSource;
    public AudioClip swipeSound;
    public AudioClip rollingSound;
    [Range(0f, 1f)]
    public float swipeSoundVolume = 0.7f;
    [Range(0f, 1f)]
    public float rollingSoundVolume = 0.5f;

    private bool isTraveling;
    private Vector3 travelDirection;
    private Vector3 nextCollisionPosition;

    public int minSwipeRecognition = 500;
    private Vector2 swipePosLastFrame;
    private Vector2 swipePosCurrentFrame;
    private Vector2 currentSwipe;

    private Color solveColor;
    private bool wasPlayingRollingSound;


    void Start()
    {
        solveColor = Random.ColorHSV(0.5f, 1);
        GetComponent<MeshRenderer>().material.color = solveColor;

        // Set up audio source if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Configure audio source for rolling sound
        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    private void FixedUpdate()
    {
        if (isTraveling)
        {
            ballRb.velocity = travelDirection * speed;
            dirtParticle.Play();

            // Play rolling sound if we have one and aren't already playing it
            if (rollingSound != null && !wasPlayingRollingSound)
            {
                audioSource.clip = rollingSound;
                audioSource.volume = rollingSoundVolume;
                audioSource.Play();
                wasPlayingRollingSound = true;
            }
        }
        else
        {
            dirtParticle.Stop();

            // Stop rolling sound when not traveling
            if (wasPlayingRollingSound && audioSource.isPlaying && audioSource.clip == rollingSound)
            {
                audioSource.Stop();
                wasPlayingRollingSound = false;
            }
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position - (Vector3.up / 2), 0.05f);
        int i = 0;
        while (i < hitColliders.Length)
        {
            GroundPiece ground = hitColliders[i].transform.GetComponent<GroundPiece>();
            if (ground && !ground.isColored)
            {
                ground.ChangeColor(solveColor);
            }
            i++;
        }

        if (nextCollisionPosition != Vector3.zero)
        {
            if (Vector3.Distance(transform.position, nextCollisionPosition) < 1)
            {
                isTraveling = false;
                travelDirection = Vector3.zero;
                nextCollisionPosition = Vector3.zero;
            }
        }

        if (isTraveling)
            return;

        if (Input.GetMouseButton(0))
        {
            swipePosCurrentFrame = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

            if (swipePosLastFrame != Vector2.zero)
            {
                currentSwipe = swipePosCurrentFrame - swipePosLastFrame;

                if (currentSwipe.sqrMagnitude < minSwipeRecognition)
                {
                    return;
                }

                currentSwipe.Normalize();

                if (currentSwipe.x > -0.5f && currentSwipe.x < 0.5)
                {
                    //Go up/down
                    SetDestination(currentSwipe.y > 0 ? Vector3.forward : Vector3.back);
                }

                if (currentSwipe.y > -0.5f && currentSwipe.y < 0.5)
                {
                    //Go left/right
                    SetDestination(currentSwipe.x > 0 ? Vector3.right : Vector3.left);
                }
            }

            swipePosLastFrame = swipePosCurrentFrame;
        }

        if (Input.GetMouseButtonUp(0))
        {
            swipePosLastFrame = Vector2.zero;
            currentSwipe = Vector2.zero;
        }
    }

    private void SetDestination(Vector3 direction)
    {
        travelDirection = direction;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, 100f))
        {
            nextCollisionPosition = hit.point;
        }

        isTraveling = true;

        // Play swipe sound effect
        PlaySwipeSound();
    }

    private void PlaySwipeSound()
    {
        if (swipeSound != null && audioSource != null)
        {
            // Use PlayOneShot so it doesn't interrupt rolling sound
            audioSource.PlayOneShot(swipeSound, swipeSoundVolume);
        }
    }
}
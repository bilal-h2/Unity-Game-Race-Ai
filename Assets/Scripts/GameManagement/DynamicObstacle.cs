using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicObstacle : MonoBehaviour
{
    public enum ObstacleType
    {
        LerpToDirection,
        GroundRaiseAndHide,
        SineWave,
        RotateAround,
        Bounce
    }

    public ObstacleType obstacleType = ObstacleType.LerpToDirection;

    public Vector3 lerpDirection = Vector3.right;
    public float lerpSpeed = 2.0f;

    public float groundRaiseTime = 1.0f;
    public float hideTime = 1.0f;
    public float totalCycleTime = 4.0f;

    private bool isRising = true;
    private float startTime;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;

    private void Start()
    {
        startTime = Time.time;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;
    }

    private void Update()
    {
        switch (obstacleType)
        {
            case ObstacleType.LerpToDirection:
                LerpToDirectionPingPong();
                break;
            case ObstacleType.GroundRaiseAndHide:
                GroundRaiseAndHidePingPong();
                break;
            case ObstacleType.SineWave:
                SineWavePingPong();
                break;
            case ObstacleType.RotateAround:
                RotateAroundPingPong();
                break;
            case ObstacleType.Bounce:
                BouncePingPong();
                break;
        }
    }

    private void LerpToDirectionPingPong()
    {
        float pingPongTime = Mathf.PingPong(Time.time * lerpSpeed, 1.0f);
        Vector3 targetPosition = originalPosition + lerpDirection;
        transform.position = Vector3.Lerp(originalPosition, targetPosition, pingPongTime);
    }

    private void GroundRaiseAndHidePingPong()
    {
        float elapsedTime = Time.time - startTime;
        float pingPongTime = Mathf.PingPong(elapsedTime, totalCycleTime) / totalCycleTime;

        float yPosition = Mathf.Lerp(0.0f, 1.0f, pingPongTime);
        transform.position = originalPosition + Vector3.up * yPosition;
    }

    private void SineWavePingPong()
    {
        float pingPongTime = Mathf.PingPong(Time.time * lerpSpeed, 1.0f);
        Vector3 offset = lerpDirection * Mathf.Sin(pingPongTime * Mathf.PI);
        transform.position = originalPosition + offset;
    }

    private void RotateAroundPingPong()
    {
        float pingPongTime = Mathf.PingPong(Time.time * lerpSpeed, 1.0f);
        float rotationAngle = Mathf.Lerp(0.0f, 360.0f, pingPongTime);
        transform.rotation = originalRotation * Quaternion.Euler(0.0f, rotationAngle, 0.0f);
    }

    private void BouncePingPong()
    {
        float elapsedTime = Time.time - startTime;
        float pingPongTime = Mathf.PingPong(elapsedTime, totalCycleTime) / totalCycleTime;

        float yOffset = Mathf.Abs(Mathf.Sin(pingPongTime * Mathf.PI)) * 2.0f;
        transform.position = originalPosition + Vector3.up * yOffset;
    }
}



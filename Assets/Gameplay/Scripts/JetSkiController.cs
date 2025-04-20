using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class JetSkiController : MonoBehaviour
{
    [Header("Buoyancy")] 
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private Transform[] buoyancyPoints;
    [SerializeField ]private float buoyancyForce = 10f;
    [SerializeField] private float waterDensity = 1f;
    [SerializeField] private float waterDrag = 1f;
    [SerializeField] private float waterAngularDrag = 2f;

    [Header("Wave Settings")]
    [SerializeField] private float waveSpeed = 1f;
    [SerializeField] private float waveScale = 2f;
    [SerializeField] private float waveHeight = 1f;

    [Header("Engine & Control")]
    [SerializeField] private float enginePower = 500f;
    [SerializeField] private float turnSpeed = 2f;
    [SerializeField] private float forwardSpeed = 1f;

    [Header("Tilt Settings")]
    [SerializeField] private float rollTiltAmount = 10f;
    [SerializeField] private float pitchTiltAmount = 5f;
    [SerializeField] private float tiltSmoothing = 2f;

    [Header("Touch Controls")]
    [SerializeField] private float swipeSensitivity = 2f;
    private float steeringInput;
    private Vector2 touchStart;

    [Header("Visuals")]
    [SerializeField] private TrailRenderer[] trails;
    [SerializeField] private float minSpeedForTrail = 1f;
    [SerializeField] private ParticleSystem _splashParticles;
    [SerializeField] private List<ParticleSystem> _foamTrail;
    
    private bool inWater;
    private bool wasInWater = false;
    
    [SerializeField] private float splashVelocityThreshold = -1f;

    void FixedUpdate()
    {
        ApplyBuoyancy();
        ApplyMovement();
        ApplyTilt();
        UpdateTrailRenderers();
    }

    void Update()
    {
        HandleTouchInput();
    }

    void ApplyBuoyancy()
    {
        inWater = false;

        foreach (var point in buoyancyPoints)
        {
            Vector3 worldPos = point.position;
            float waveY = GetWaveHeight(worldPos.x, worldPos.z, Time.time);

            if (worldPos.y < waveY)
            {
                inWater = true;

                _splashParticles.Play();
                
                if (!wasInWater)
                {
                    wasInWater = true;
                }

                float depth = waveY - worldPos.y;
                float spring = Mathf.Clamp01(depth);
                float damping = _rigidbody.GetPointVelocity(worldPos).y * 0.5f;
                Vector3 buoyancy = Vector3.up * (spring * buoyancyForce * waterDensity - damping);
                _rigidbody.AddForceAtPosition(buoyancy, worldPos);
            }
            else
            {
                wasInWater = false;
            }
        }

        _rigidbody.drag = inWater ? waterDrag : 0f;
        _rigidbody.angularDrag = inWater ? waterAngularDrag : 0.05f;
    }

    float GetWaveHeight(float x, float z, float time)
    {
        float k = 2 * Mathf.PI / waveScale;
        float c = Mathf.Sqrt(9.8f / k);
        float f = k * (x + z - c * time);
        return Mathf.Sin(f) * waveHeight;
    }

    void ApplyMovement()
    {
        Vector3 forwardForce = transform.forward * enginePower * forwardSpeed;
        _rigidbody.AddForce(forwardForce);
        _rigidbody.AddTorque(Vector3.up * steeringInput * turnSpeed, ForceMode.Acceleration);
    }

    void ApplyTilt()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(_rigidbody.velocity);

        float targetRoll = -steeringInput * rollTiltAmount;
        float targetPitch = -Mathf.Clamp(localVelocity.z, -1f, 1f) * pitchTiltAmount;

        Quaternion targetRotation = Quaternion.Euler(targetPitch, transform.eulerAngles.y, targetRoll);
        Quaternion smoothedRotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.fixedDeltaTime * tiltSmoothing);
        _rigidbody.MoveRotation(smoothedRotation);
    }

    void HandleTouchInput()
    {
#if UNITY_EDITOR
        steeringInput = Input.GetAxis("Horizontal");
#else
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStart = touch.position;
                    break;
                case TouchPhase.Moved:
                    float swipeDelta = (touch.position.x - touchStart.x) / Screen.width;
                    steeringInput = Mathf.Clamp(swipeDelta * swipeSensitivity, -1f, 1f);
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    steeringInput = 0f;
                    break;
            }
        }
#endif
    }

    void UpdateTrailRenderers()
    {
        bool active = _rigidbody.velocity.magnitude > minSpeedForTrail;
        foreach (var trail in trails)
        {
            if (trail != null)
                trail.emitting = active;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        if (buoyancyPoints != null)
        {
            foreach (var p in buoyancyPoints)
            {
                if (p != null)
                    Gizmos.DrawSphere(p.position, 0.1f);
            }
        }
    }
    
    
}
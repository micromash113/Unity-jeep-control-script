using UnityEngine;
using System.Collections.Generic; // Needed for using Lists

public class JeepControllerSteeringWheelMovement : MonoBehaviour
{
    // --- Wheel Colliders (REQUIRED for Physics) ---
    [Header("Wheel Colliders")]
    public WheelCollider frontLeftWheelCollider;
    public WheelCollider frontRightWheelCollider;
    public WheelCollider rearLeftWheelCollider;
    public WheelCollider rearRightWheelCollider;

    // --- Visual Wheels (REQUIRED for Appearance) ---
    [Header("Visual Wheels")]
    public Transform frontLeftWheelTransform;
    public Transform frontRightWheelTransform;
    public Transform rearLeftWheelTransform;
    public Transform rearRightWheelTransform;
    
    // --- STEERING WHEEL (NEW) ---
    [Header("Steering Wheel")]
    [Tooltip("Drag the visual steering wheel 3D model Transform here.")]
    public Transform steeringWheelTransform;
    [Tooltip("How much the steering wheel turns visually (e.g., 400 degrees).")]
    public float steeringWheelMaxRotation = 400f; 
    [Tooltip("Speed at which the steering wheel turns visually. Try 5 to 15.")]
    public float steeringWheelRotationSpeed = 10f; // NEW SMOOTHING VARIABLE

    // --- Physics Settings (For Off-Road Feel) ---
    [Header("Driving Stats")]
    public float maxMotorTorque = 300f; 
    public float maxSteeringAngle = 30f; 
    public float brakeTorque = 500f; 

    // --- Private Variables ---
    private List<WheelCollider> driveWheels; 
    private List<Transform> visualWheels; 
    // Private variable to store the target rotation value
    private float currentVisualRotation = 0f; // NEW PRIVATE VARIABLE

    // Start runs once at the beginning
    void Start()
    {
        // Populate the lists for easier access
        driveWheels = new List<WheelCollider> 
        { 
            frontLeftWheelCollider, frontRightWheelCollider, 
            rearLeftWheelCollider, rearRightWheelCollider 
        };

        visualWheels = new List<Transform> 
        { 
            frontLeftWheelTransform, frontRightWheelTransform, 
            rearLeftWheelTransform, rearRightWheelTransform 
        };

        UpdateAllWheels(); 
    }

    // FixedUpdate is best for physics calculations (runs consistently, not per frame)
    void FixedUpdate()
    {
        HandleMotor();
        HandleSteering();
        HandleBraking();

        UpdateAllWheels(); 
    }

    // --- 1. ENGINE POWER (W and S Keys) ---
    private void HandleMotor()
    {
        float motorInput = Input.GetAxis("Vertical"); 
        float torque = motorInput * maxMotorTorque;

        foreach (WheelCollider wheel in driveWheels)
        {
            wheel.motorTorque = torque;
        }
    }

    // --- 2. STEERING (A and D Keys) ---
    private void HandleSteering()
    {
        float steerInput = Input.GetAxis("Horizontal"); 
        float steerAngle = steerInput * maxSteeringAngle;

        // Apply the turning angle to only the FRONT wheels
        frontLeftWheelCollider.steerAngle = steerAngle;
        frontRightWheelCollider.steerAngle = steerAngle;

        // ------------------------------------------------------------------
        // REVISED LOGIC: Smoothly Move the Visual Steering Wheel
        // ------------------------------------------------------------------
        if (steeringWheelTransform != null)
        {
            // 1. Determine the target angle
            float targetRotation = steerInput * steeringWheelMaxRotation;
            
            // 2. Smoothly move the current rotation towards the target rotation
            // We use Lerp to move the value gradually over time
            currentVisualRotation = Mathf.Lerp(
                currentVisualRotation, // The current angle
                targetRotation,        // The target angle (based on input)
                Time.deltaTime * steeringWheelRotationSpeed // Speed control
            );
            
            // 3. Apply the smoothed rotation
            // Note the use of currentVisualRotation instead of the instant steerInput calculation
            steeringWheelTransform.localRotation = Quaternion.Euler(0f, 0f, -currentVisualRotation);
        }
    }

    // --- 3. BRAKING (Spacebar) ---
    private void HandleBraking()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            foreach (WheelCollider wheel in driveWheels)
            {
                wheel.brakeTorque = brakeTorque;
                wheel.motorTorque = 0f; 
            }
        }
        else
        {
            foreach (WheelCollider wheel in driveWheels)
            {
                wheel.brakeTorque = 0f;
            }
        }
    }

    // --- 4. VISUALS: Synchronize Physics Wheels with 3D Models ---
    private void UpdateAllWheels()
    {
        UpdateWheelPos(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateWheelPos(frontRightWheelCollider, frontRightWheelTransform);
        UpdateWheelPos(rearLeftWheelCollider, rearLeftWheelTransform);
        UpdateWheelPos(rearRightWheelCollider, rearRightWheelTransform);
    }

    // Helper function to update one wheel's position and rotation
    private void UpdateWheelPos(WheelCollider collider, Transform visual)
    {
        if (visual == null) return; // Add safety check
        
        Vector3 pos;
        Quaternion rot;
        collider.GetWorldPose(out pos, out rot);

        visual.position = pos;
        
        // --- ROTATION CORRECTION GOES HERE ---
        
        // This line applies a fixed 90-degree rotation (around the Z-axis)
        // to correct the visual model's initial orientation relative to the WheelCollider.
        // If the tire is sideways, this is the most common fix.
        visual.rotation = rot * Quaternion.Euler(1, 0, 180f); 
        
        // If the tire is still wrong, you must test the other common fixes below!
    }
}
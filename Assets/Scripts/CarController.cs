using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AxleInfo 
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor; // Is this wheel attached to a motor?
    public bool steering; // Does this wheel apply steer angle?
}

public class CarController : MonoBehaviour
{
    public List<AxleInfo> axleInfos; // Information about each individual axle
    public float maxMotorTorque; // Maximum torque the motor can apply to wheel
    public float maxSteeringAngle; // Maximum steer angle the wheel can have
    public float lastRPM; // The last RPM of the car, used by GUI script
    public List<float> gears; // List of gear ratios
    public int curGear = 1; // Current gear of the car (1-5)
    public float gearVal; // Gear ratio of the current gear
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Finds the corresponding visual wheel
    // Correctly applies the transform to make the wheels turn and steer
    public void ApplyLocalPositionToVisuals(WheelCollider collider)
    {
        if (collider.transform.childCount == 0)
        {
            return;
        }

        Transform visualWheel = collider.transform.GetChild(0);

        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);

        visualWheel.transform.position = position;
        visualWheel.transform.rotation = rotation;
    }

    public void Update()
    {
        //Change Gear
        if (Input.GetKeyDown(KeyCode.UpArrow) && curGear < 5)
        {
            curGear++;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) && curGear > 1)
        {
            curGear--;
        }
    }

    // applies motion to the wheels
    public void FixedUpdate()
    {
        float stepper = 3.5f;
        gearVal = gears[curGear]; //sets gear ratio to that of current gear value
        float motor = 0;
        float steering = maxSteeringAngle * Input.GetAxis("Horizontal");
        float totalWheelRPM = 0f;
        int driveWheelNum = 0;
        float forwardVelocity = Vector3.Dot(rb.velocity, transform.forward);
        float braking = 0f;

        // If input direction and velocity direction match
        if ((forwardVelocity >= 0 && Input.GetAxis("Vertical") > 0) || (forwardVelocity <= 0 && Input.GetAxis("Vertical") < 0))
        {
            motor = maxMotorTorque * Input.GetAxis("Vertical") * stepper * gearVal;

            if ((forwardVelocity <= 0 && Input.GetAxis("Vertical") < 0))
            {
                curGear = 1;
                gearVal = gears[curGear];
            }
        }
        // If input direction and velocity direction don't match
        else if ((forwardVelocity >= 0 && Input.GetAxis("Vertical") < 0) || (forwardVelocity <= 0 && Input.GetAxis("Vertical") > 0))
        {
            braking = 1000;
        }

        //failsafe to prevet supercharging
        if (Mathf.Abs(lastRPM) > 7000)
        {
            motor = 0;
        }

        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            if (axleInfo.motor)
            {
                driveWheelNum += 2;

                totalWheelRPM += axleInfo.leftWheel.rpm;
                totalWheelRPM += axleInfo.rightWheel.rpm;

                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }

            axleInfo.leftWheel.brakeTorque = braking;
            axleInfo.rightWheel.brakeTorque = braking;

            ApplyLocalPositionToVisuals(axleInfo.leftWheel);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);
        }

        float driveshaftRPM = totalWheelRPM / driveWheelNum;
        lastRPM = driveshaftRPM * stepper * gearVal;

    }
}
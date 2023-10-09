using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AxleInfo 
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor; // Is this wheel attached to motor?
    public bool steering; // Does this wheel apply steer angle?
}

public class CarController : MonoBehaviour
{
    public List<AxleInfo> axleInfos; // The information about each individual axle
    public float maxMotorTorque; // Maximum torque the motor can apply to wheel
    public float maxSteeringAngle; // Maximum steer angle the wheel can have
    public float engineRPM; // Last RPM of the car
    public List<float> gears;
    public int curGear = 1;
    public float gearVal;
    private Rigidbody rb;
    public float autoShiftDelay = 0.5f;
    public float upShiftTimer = 0f;
    public float downShiftTimer = 0f;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Finds the corresponding visual wheel
    // Correctly applies the transform
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
        // Gear shifts up
        if (Input.GetKeyDown(KeyCode.UpArrow) && curGear < 5)
        {
            curGear++;
        }

        // Gear shifts down
        if (Input.GetKeyDown(KeyCode.DownArrow) && curGear > 1)
        {
            curGear--;
        }

        // Timers for automatic shifting
        upShiftTimer -= Time.deltaTime;
        downShiftTimer -= Time.deltaTime;

    }

    // Applies motion to the wheels
    public void FixedUpdate()
    {
        float stepper = 2f; // Stepping up gear between gearbox and driveshaft
        gearVal = gears[curGear]; // Sets gear ratio to that of current gear value
        
        float steering = maxSteeringAngle * Input.GetAxis("Horizontal"); // Input intensity of steering
        int driveWheelNum = 0; // Number of drive wheels
        float totalWheelRPM = 0f; // Total RPM of all wheels
        float forwardVelocity = Vector3.Dot(rb.velocity, transform.forward); // The direction the car is driving in

        float motorPower = 0;
        float braking = 0f;

        // If input direction and velocity direction match...
        if ((forwardVelocity >= 0 && Input.GetAxis("Vertical") > 0) || (forwardVelocity <= 0 && Input.GetAxis("Vertical") < 0))
        {
            // ...Motor power becomes input intensity up to maximum motor torque * stepper motor * current gear's gear ratio
            motorPower = maxMotorTorque * Input.GetAxis("Vertical") * stepper * gearVal;

            // If the car is reversing...
            if ((forwardVelocity <= 0 && Input.GetAxis("Vertical") < 0))
            {
                // ...Set it into reverse gear
                curGear = 0;
                gearVal = gears[curGear];
            }
            // If the car is going forwards and is in reverse gear...
            else if (curGear == 0)
            {
                // ...Set it into a forward gear
                curGear = 1;
                gearVal = gears[curGear];
            }
        }
        // If input direction and velocity direction don't match...
        else if ((forwardVelocity >= 0 && Input.GetAxis("Vertical") < 0) || (forwardVelocity <= 0 && Input.GetAxis("Vertical") > 0))
        {
            // ...The car is braking
            braking = 1000;
        }

        // After a delay, automatically downshift. Car will correct if wrong.
        //if (curGear > 1 && downShiftTimer <= 0)
        //{
        //    curGear--;
        //    downShiftTimer = autoShiftDelay * 2;
        //}

        // When RPM goes high enough, shift up a gear
        if (Mathf.Abs(engineRPM) > 7000) //&& curGear < 6 && upShiftTimer <= 0
        {
            //curGear++;
            //upShiftTimer = autoShiftDelay;
            motorPower = 0;
        }

        // For each axle in the car...
        foreach (AxleInfo axleInfo in axleInfos)
        {
            // If the wheel can steer, apply the steering angle
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            // If the wheel can drive, apply the motor torque
            if (axleInfo.motor)
            {
                driveWheelNum += 2;

                totalWheelRPM += axleInfo.leftWheel.rpm;
                totalWheelRPM += axleInfo.rightWheel.rpm;

                axleInfo.leftWheel.motorTorque = motorPower;
                axleInfo.rightWheel.motorTorque = motorPower;
            }

            // Apply braking torque
            axleInfo.leftWheel.brakeTorque = braking;
            axleInfo.rightWheel.brakeTorque = braking;

            ApplyLocalPositionToVisuals(axleInfo.leftWheel);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);
        }

        float driveshaftRPM = totalWheelRPM / driveWheelNum; // driveshaft rpm = average wheel RPM
        engineRPM = driveshaftRPM * stepper * gearVal; 

    }
}
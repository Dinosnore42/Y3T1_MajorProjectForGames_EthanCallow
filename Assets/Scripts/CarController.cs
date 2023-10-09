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
    public float totalWheelRPM; // Total RPM of drive wheels
    public float freeWheelRPM; // Total RPM of non-driving wheels
    public bool automaticGears = true;
    public bool tractionControl = true;

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

        // Toggle automatic gears
        if (Input.GetKeyDown("z"))
        {
            if (automaticGears == true)
            {
                automaticGears = false;
            }
            else
            {
                automaticGears = true;
            }
        }

        // Toggle traction control
        if (Input.GetKeyDown("x"))
        {
            if (tractionControl == true)
            {
                tractionControl = false;
            }
            else
            {
                tractionControl = true;
            }
        }
    }

    // Applies motion to the wheels
    public void FixedUpdate()
    {
        float stepper = 4f; // Stepping up gear between gearbox and driveshaft
        gearVal = gears[curGear]; // Sets gear ratio to that of current gear value
        
        float steering = maxSteeringAngle * Input.GetAxis("Horizontal"); // Input intensity of steering
        int driveWheelNum = 0; // Number of drive wheels
        float forwardVelocity = Vector3.Dot(rb.velocity, transform.forward); // The direction the car is driving in

        totalWheelRPM = 0;
        freeWheelRPM = 0;

        float motorPower = 0;
        float braking = 0f;

        // If input direction and velocity direction match...
        if ((forwardVelocity >= 0 && Input.GetAxis("Vertical") > 0) || (forwardVelocity <= 0 && Input.GetAxis("Vertical") < 0))
        {
            // ...Motor power becomes input intensity up to maximum motor torque * stepper motor * current gear's gear ratio
            motorPower = maxMotorTorque * Input.GetAxis("Vertical") * stepper * gearVal;

            // Shifting gears automatically
            float speed = rb.velocity.magnitude * 2.237f;

            if (automaticGears == true)
            {
                if (0 < speed && speed < 10)
                {
                    curGear = 1;
                }
                else if (10 <= speed && speed < 25)
                {
                    curGear = 2;
                }
                else if (25 <= speed && speed < 40)
                {
                    curGear = 3;
                }
                else if (40 <= speed && speed < 60)
                {
                    curGear = 4;
                }
                else if (60 <= speed)
                {
                    curGear = 5;
                }
            }

            // If the car is reversing...
            if ((forwardVelocity <= 0 && Input.GetAxis("Vertical") < 0))
            {
                // ...Set it into reverse gear
                curGear = 0;
            }
            // If the car is going forwards and is in reverse gear...
            else if (curGear == 0)
            {
                // ...Set it into a forward gear
                curGear = 1;
            }

            gearVal = gears[curGear];
        }
        // If input direction and velocity direction don't match...
        else if ((forwardVelocity >= 0 && Input.GetAxis("Vertical") < 0) || (forwardVelocity <= 0 && Input.GetAxis("Vertical") > 0))
        {
            // ...The car is braking
            braking = 10000; // Newton Meters
        }

        // Rev Limiter 
        if (Mathf.Abs(engineRPM) > 6000)
        {
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
                
                freeWheelRPM += axleInfo.leftWheel.rpm;
                freeWheelRPM += axleInfo.rightWheel.rpm;
            }
            // If the wheel can drive, apply the motor torque
            if (axleInfo.motor)
            {
                driveWheelNum += 2;

                totalWheelRPM += axleInfo.leftWheel.rpm;
                totalWheelRPM += axleInfo.rightWheel.rpm;

                // Traction control - (anti wheelspin)
                if (tractionControl == true && ((totalWheelRPM / 2 - freeWheelRPM / 2) > 250 || (freeWheelRPM / 2 - totalWheelRPM / 2) > 250))
                {
                    motorPower = 0;
                    braking = 10000; // Newton Meters
                }

                axleInfo.leftWheel.motorTorque = motorPower;
                axleInfo.rightWheel.motorTorque = motorPower;
            }

            // Apply braking torque (60 front/40 back braking ratio)
            axleInfo.leftWheel.brakeTorque = braking * 1.1f;
            axleInfo.rightWheel.brakeTorque = braking * 0.9f;

            ApplyLocalPositionToVisuals(axleInfo.leftWheel);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);
        }

        float driveshaftRPM = totalWheelRPM / driveWheelNum; // driveshaft rpm = average wheel RPM
        engineRPM = driveshaftRPM * stepper * gearVal;

    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelDebug : MonoBehaviour
{
    [SerializeField] private WheelCollider fl;
    [SerializeField] private WheelCollider fr;
    [SerializeField] private WheelCollider bl;
    [SerializeField] private WheelCollider br;

    private CarController thisCar;
    private Rigidbody thisRb;

    private void Awake()
    {
        thisCar = GetComponent<CarController>();
        thisRb = GetComponent<Rigidbody>();
    }

    private void OnGUI()
    {
        // Background box
        GUI.Box(new Rect(80, 0, 300, 260), "");

        // Wheel data
        WheelDebugUI(fl, 0, -2);
        WheelDebugUI(fr, 1, -2);
        WheelDebugUI(bl, 0, -1);
        WheelDebugUI(br, 1, -1);

        // RPM and gear info
        GUI.Label(new Rect(90, 90, 50, 50), ("Engine RPM: " + ((int)Mathf.Round(thisCar.engineRPM)).ToString()));
        GUI.Label(new Rect(150, 90, 50, 50), ("Gear: " + thisCar.curGear.ToString()));
        GUI.Label(new Rect(210, 90, 50, 50), ("Gear Ratio: " + thisCar.gearVal.ToString() + ":1"));
        GUI.Label(new Rect(270, 90, 50, 50), ("Speed: " + ((int)Mathf.Round(thisRb.velocity.magnitude * 2.237f)).ToString() + "mph"));

        if (thisCar.tractionControl == true && (thisCar.totalWheelRPM / 2 - thisCar.freeWheelRPM / 2 > 250 || thisCar.freeWheelRPM / 2 - thisCar.totalWheelRPM / 2 > 250))
        {
            GUI.Label(new Rect(150, 70, 200, 50), ("TRACTION CONTROL ON"));
        }

        if (thisCar.automaticGears == true)
        {
            GUI.Label(new Rect(90, 215, 200, 50), ("Z: turn off automatic gears"));
        }
        else
        {
            GUI.Label(new Rect(90, 215, 200, 50), ("Z: turn on automatic gears"));
        }

        if (thisCar.tractionControl == true)
        {
            GUI.Label(new Rect(90, 230, 200, 50), ("X: turn off traction control"));
        }
        else
        {
            GUI.Label(new Rect(90, 230, 200, 50), ("X: turn on traction control"));
        }
    }

    // Call this for each wheel in OnGUI, with x, y screen offsets
    void WheelDebugUI(WheelCollider wheel, float x, float y)
    {
        wheel.GetGroundHit(out WheelHit hit);
        GUI.Label(new Rect(100 + 150 * x, 300 + 150 * y, 500, 500),
        "RPM = " + ((int)Mathf.Round(wheel.rpm)).ToString("0")
        + "\nForward Slip ="
        + hit.forwardSlip.ToString("0.00")
        + "\nSide Slip ="
        + hit.sidewaysSlip.ToString("0.00")
        + "\nTorque = " + wheel.motorTorque
        );
    }
}
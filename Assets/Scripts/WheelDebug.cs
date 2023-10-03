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

    private void Awake()
    {
        thisCar = GetComponent<CarController>();
    }

    private void OnGUI()
    {
        // nice box
        GUI.Box(new Rect(80, 0, 300, 230), "");

        //wheel data
        WheelDebugUI(fl, 0, -2);
        WheelDebugUI(fr, 1, -2);
        WheelDebugUI(bl, 0, -1);
        WheelDebugUI(br, 1, -1);

        GUI.Label(new Rect(200, 100, 50, 50), ("Average RPM: " + thisCar.lastRPM.ToString()));
    }

    // Call this for each wheel in OnGUI, with x, y screen offsets
    void WheelDebugUI(WheelCollider wheel, float x, float y)
    {
        wheel.GetGroundHit(out WheelHit hit);
        GUI.Label(new Rect(100 + 150 * x, 300 + 150 * y, 500, 500),
        "RPM = " + wheel.rpm.ToString("0")
        + "\nForward Slip ="
        + hit.forwardSlip.ToString("0.00")
        + "\nSide Slip ="
        + hit.sidewaysSlip.ToString("0.00")
        + "\nTorque = " + wheel.motorTorque
        );
    }
}
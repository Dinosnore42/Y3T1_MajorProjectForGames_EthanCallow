using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterOfMass : MonoBehaviour
{
    public GameObject car;
    public Rigidbody body;

    //Center of mass alter
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(body.centerOfMass + car.transform.position, 1);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitCam : MonoBehaviour
{

    public Transform truck;
    public Vector3 offsetPosition;

    [Range(0.01f, 1.0f)]
    public float smoothFactor = 0.5f;

    public float rotationSpeed = 5;

    // Start is called before the first frame update
    void Start()
    {
        offsetPosition = transform.position - truck.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Quaternion camTurnAngleHorizontal = Quaternion.AngleAxis(Input.GetAxis("Mouse X") * rotationSpeed, Vector3.up);
        Quaternion camTurnAngleVertical = Quaternion.AngleAxis(Input.GetAxis("Mouse Y") * rotationSpeed, Vector3.right);

        offsetPosition = camTurnAngleHorizontal * camTurnAngleVertical * offsetPosition;

        Vector3 newPos = truck.position + offsetPosition;

        transform.position = Vector3.Slerp(transform.position, newPos, smoothFactor);

        transform.LookAt(truck);
    }
}

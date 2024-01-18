using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCam : MonoBehaviour
{
    public float lookSpeed = 3;
    public Vector2 rotation = new Vector2(15,150);

    public float moveSpeed;
    bool lockCursor = true;

    public Vector2 clamp = new Vector2(-10, 10);

    Rigidbody rb;
    private void Start()
    {
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
        rb = gameObject.GetComponent<Rigidbody>();
        rotation.x /= lookSpeed;
        rotation.y /= lookSpeed;

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            lockCursor = !lockCursor;
            Cursor.visible = lockCursor;
            
            if (lockCursor) 
                Cursor.lockState = CursorLockMode.Locked;
            else
                Cursor.lockState = CursorLockMode.None;

        }

        if (lockCursor)
            Look();
        

        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");
        float up = Input.GetAxis("Up");

        Vector3 moveVertical = transform.forward * Mathf.Clamp(vertical, -1f, 1f) * moveSpeed * Time.fixedDeltaTime;
        Vector3 moveHorizontal = transform.right * Mathf.Clamp(horizontal, -1f, 1f) * moveSpeed * Time.fixedDeltaTime;
        Vector3 moveUp = transform.up * Mathf.Clamp(up, -1f, 1f) * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(transform.position + moveVertical + moveHorizontal + moveUp);
    }


    public void Look() // Look rotation (UP down is Camera) (Left right is Transform rotation)
    {
        rotation.y += Input.GetAxis("Mouse X");
        rotation.x += -Input.GetAxis("Mouse Y");
        rotation.x = Mathf.Clamp(rotation.x, clamp.x, clamp.y);
        transform.eulerAngles = new Vector2(0, rotation.y) * lookSpeed;
        Camera.main.transform.localRotation = Quaternion.Euler(rotation.x * lookSpeed, 0, 0);
    }
}

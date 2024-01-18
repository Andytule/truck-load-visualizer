using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PivotCam : MonoBehaviour
{
    public float lookSpeed = 3;

    [Range(0.01f, 100)]
    public float scrollScale = 0.5f;

    [Range(0.01f, 1)]
    public float smoothRotationScale = 0.5f;

    [Range(0.01f, 1)]
    public float smoothMoveScale = 0.5f;
   
    public int minCamDistance = 2;
    public int maxCamDistance = 16;

    public Transform cam;
    public Transform zero;

    public Vector2 rotation;
    public Vector2 clamp = new Vector2(-90, 360);
    
    Vector3 bedPosition;
    Quaternion toRotation;
    Vector3 toPosition;

    float heightFromBed;
    float zoomDelta;

    bool goToRotation = false;
    bool goToPosition = false;


    private void Start()
    {
        bedPosition = zero.parent.TransformPoint(zero.localPosition);

        //divide the clamp by the look speed so that when the mouse move is multiplied by the look speed, the clamp angle will remain correct
        clamp /= lookSpeed;

        setClampAngles();

        rotation = new Vector2(transform.eulerAngles.x - 360, transform.eulerAngles.y - 360) / lookSpeed;
    }

    void Update(){

        if (goToRotation){

            //keep the z rotation at zero to avoid skew
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0);

            //If the angle between the current rotation and the preset rotation is above a set threshold, keep Lerping.
            //Prevents the lerp from getting stuck trying to reach the destincation rotation.
            if (Quaternion.Angle(transform.rotation, toRotation) > 0.5f){
                transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, smoothRotationScale);
            }
            else{
                goToRotation = false;
                resetMouseRotation();
            }
        }

        if (!goToRotation){
            
            zoomDelta = -Input.mouseScrollDelta.y * scrollScale;
            
            if(getTouchPinchDelta() != 0) 
                zoomDelta = getTouchPinchDelta();
            
            doCameraZoom(zoomDelta);

            if (Input.GetMouseButton(0) || getTouchMoveDelta().magnitude > 0){
                
                if (EventSystem.current.IsPointerOverGameObject()) return;

                Vector2 moveDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

                if(getTouchMoveDelta().magnitude != 0)
                {
                    moveDelta = getTouchMoveDelta();
                }

                doCameraRotate(moveDelta);
            }
        }

        if (goToPosition){
            if (Vector3.Distance(transform.position, toPosition) > 0.5f){
                transform.position = Vector3.Lerp(transform.position, toPosition, smoothMoveScale);
            }
            else{
                goToPosition = false;
                setClampAngles();
            }
        }

    }

    //Zoom the camera in/out based on the users scroll/pinch amount
    public void doCameraZoom(float delta)
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (delta != 0)
        {
            //If camera is not at min or max distance from pivot, move camera
            if (getCamDistance() + zoomDelta <= maxCamDistance && getCamDistance() + zoomDelta >= minCamDistance)
            {
                cam.transform.localPosition += new Vector3(0, 0, zoomDelta);
                setClampAngles();
                rotation.x = Mathf.Clamp(rotation.x, clamp.x, clamp.y);
                transform.eulerAngles = new Vector3(rotation.x, rotation.y, 0) * lookSpeed;
            }
        }
    }
    
    //Rotate the pivot based on the users mouse/touch move
    public void doCameraRotate(Vector2 moveDelta)
    {
        rotation.y += moveDelta.x; //Input.GetAxis("Mouse X");
        rotation.x += moveDelta.y; //Input.GetAxis("Mouse Y");

        rotation.x = Mathf.Clamp(rotation.x, clamp.x, clamp.y);
        transform.eulerAngles = new Vector3(rotation.x, rotation.y, 0) * lookSpeed;
    }

    //Smoothly move the camera to a preset position/rotation
    public void doCameraPreset(int preset){
        switch (preset){
            case 1:
                toRotation = Quaternion.Euler(new Vector3(-10, 90, 0));
                break;
            case 2:
                toRotation = Quaternion.Euler(new Vector3(-10, 0, 0));
                break;
            case 3:
                toRotation = Quaternion.Euler(new Vector3(-10, -90, 0));
                break;
            case 4:
                toRotation = Quaternion.Euler(new Vector3(-10, 180, 0));
                break;
            case 5:
                toRotation = Quaternion.Euler(new Vector3(-90, 0, 0));
                break;
        }
        goToRotation = true;
    }

    //Return the delta distance pinched
    public float getTouchPinchDelta()
    {
        if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            Vector2 touchPrevPos1 = touch1.position - touch1.deltaPosition;
            Vector2 touchPrevPos2 = touch2.position - touch2.deltaPosition;

            float prevMagnitude = (touchPrevPos1 - touchPrevPos2).magnitude;
            float currMagnitude = (touch1.position - touch2.position).magnitude;

            return -(currMagnitude - prevMagnitude) * 0.01f;
        }
        return 0;
    }

    //Returns the delta vector of the users touch
    public Vector2 getTouchMoveDelta()
    {
        if(Input.touchCount == 1)
        {            
            return Input.GetTouch(0).deltaPosition * 0.1f;       
        }

        return Vector2.zero;
    }

    //Reset the mouse rotation vector. 
    //Prevents the view from snapping back to where it was before an animated move
    public void resetMouseRotation()
    {
        rotation = new Vector2(transform.eulerAngles.x - 360, transform.eulerAngles.y - 360) / lookSpeed;
    }

    //Calculates the max angle the camera can tilt down so as not to go below the truck bed
    public float getPivotClampAngle()
    {
        heightFromBed = transform.position.y - bedPosition.y;
        return Mathf.Min(((90 - Mathf.Acos(heightFromBed / getCamDistance()) * Mathf.Rad2Deg) / lookSpeed) - 0.33f, 90);
    }

    //Returns the cameras distance from the pivot
    public float getCamDistance()
    {
       return Vector3.Distance(transform.position, cam.position);
    }

    //Set the pivots clamp angle based on the the value retured from getPivotClampAngle
    public void setClampAngles()
    {
        clamp.y = getPivotClampAngle();
    }

    //Sets the pivot position based on the object clicked/tapped
    public void setFocus(Transform t)
    {
        toPosition = t.position;
        toRotation = Quaternion.Euler(-5, transform.eulerAngles.y, 0);
        goToRotation = true;
        goToPosition = true;
    }
}

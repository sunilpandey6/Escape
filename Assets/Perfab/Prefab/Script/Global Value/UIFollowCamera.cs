using UnityEngine;

public class UIFollowCamera : MonoBehaviour
{
    public Camera cam;          // Camera reference
    public float distance = 2f; // Distance from camera

    // add variable to control left and right movement in the scene to fix it properly
    public float horizontalOffset = 0f;
    public float verticalOffset = 0f;

    void LateUpdate()
    {
        if (cam == null)
            cam = Camera.main;

        // Position in front of camera

        transform.position = cam.transform.position + cam.transform.right * horizontalOffset
    + cam.transform.up * verticalOffset + cam.transform.forward * distance;



        // Make UI face the camera
        transform.rotation = cam.transform.rotation;
    }
}
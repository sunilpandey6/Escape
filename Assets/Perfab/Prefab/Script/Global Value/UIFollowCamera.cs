using UnityEngine;

public class UIFollowCamera : MonoBehaviour
{
    public Camera cam;          // Camera reference
    public float distance = 2f; // Distance from camera

    void LateUpdate()
    {
        if (cam == null)
            cam = Camera.main;

        // Position in front of camera
        transform.position = cam.transform.position + cam.transform.forward * distance;

        // Make UI face the camera
        transform.rotation = cam.transform.rotation;
    }
}
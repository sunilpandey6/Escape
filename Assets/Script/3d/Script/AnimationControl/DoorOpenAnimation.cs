using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpenAnimation : MonoBehaviour
{
    public bool isdoorOpen = false;
    public Animator doorAnimator;
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isdoorOpen = true;
            doorAnimator.SetTrigger("Open");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isdoorOpen = false;
            doorAnimator.SetTrigger("Close");
        }
    }
}

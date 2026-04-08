// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.EventSystems;

// public class OutlineSelection : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
// {
//     private Outline outline;

//     void Awake()
//     {
//         outline = GetComponent<Outline>();

//         if (outline == null)
//             outline = gameObject.AddComponent<Outline>();

//         outline.enabled = false;
//         outline.OutlineColor = Color.magenta;
//         outline.OutlineWidth = 7f;
//     }

//     public void OnPointerEnter(PointerEventData eventData)
//     {
//         outline.enabled = true;
//     }

//     public void OnPointerExit(PointerEventData eventData)
//     {
//         outline.enabled = false;
//     }

// }

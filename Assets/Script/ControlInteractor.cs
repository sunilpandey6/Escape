using UnityEngine;
using UnityEngine.EventSystems;

public class ControlInteractor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public enum InputMode
    {   
        Dwell,
        Flicker
    }

    [Header("Mode")]
    public InputMode inputMode = InputMode.Dwell;

    [Header("Dwell Settings")]
    public float dwellTime = 2.5f;
    private float timer = 0f;
    private bool isHovering = false;
    private bool hasTriggered = false;

    [Header("Reference")]
    public UI_fnc uiController;

    private Keyval keyval; 

    void Start()
    {
        keyval = GetComponent<Keyval>(); // get values from same object
    }

    void Update()
    {
        switch (inputMode)
        {
            case InputMode.Dwell:
                HandleDwell();
                break;

            case InputMode.Flicker:
                HandleFlicker();
                break;
        }
    }

    void HandleDwell()
    {
        if (isHovering)
        {
            timer += Time.deltaTime;

            if (timer >= dwellTime && !hasTriggered)
            {
                TriggerAction();
                hasTriggered = true;
            }
        }
        else
        {
            timer = 0f;
            hasTriggered = false;
        }
    }

    void HandleFlicker()
    {
        // later
    }

    void TriggerAction()
    {
        if (keyval.isDelete)
            uiController.RemoveDigitLast();
        else if (keyval.isNext)
            uiController.OnNext();
        else
            uiController.AddDigit(keyval.value);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (inputMode == InputMode.Dwell)
        {
            TriggerAction();
        }
    }
}
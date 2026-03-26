using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIControl : MonoBehaviour
{
    public GameObject containerA;
    public GameObject containerB;

    public Image b1i;
    public Image b2i;

    public bool isChangeColor = true;

    public Color activeColor = Color.green;
    public Color inactiveColor = Color.white;

    void Start()
    {
        ShowA();
    }

    void UpdateButtonColor(bool isAActive, Image buttonImage)
    {
        if (!isChangeColor || buttonImage == null) return;
        buttonImage.color = isAActive ? activeColor : inactiveColor;
    }

    public void ShowA()
    {
      
        containerA.SetActive(true);
        containerB.SetActive(false);
        UpdateButtonColor(true, b1i);
    }

    public void ShowB()
    {
        containerA.SetActive(false);
        containerB.SetActive(true);
    }


    public void Toggle()
    {
        bool newState = !containerA.activeSelf;

        containerA.SetActive(newState);
        containerB.SetActive(!newState);

        UpdateButtonColor(newState, b2i);
        UpdateButtonColor(!newState, b1i);
    }

}

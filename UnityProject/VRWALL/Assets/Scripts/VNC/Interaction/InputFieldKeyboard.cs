using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InputFieldKeyboard : MonoBehaviour
{

    [SerializeField]
    private TouchScreenKeyboard keyboard;

    public TextMeshProUGUI tmpro;

    // Start is called before the first frame update

    
    public void OnEnable()
    {
        keyboard = TouchScreenKeyboard.Open(tmpro.text, TouchScreenKeyboardType.Default, false, false, false, false);
    }


    // Update is called once per frame
    void Update()
    {
        if (keyboard != null)
        {
            tmpro.text = keyboard.text;
        }
    }
}

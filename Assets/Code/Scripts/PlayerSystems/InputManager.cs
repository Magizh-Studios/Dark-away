using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Need to Refactor for New Input System


public class InputManager : SingletonBehavior<InputManager>
{
    public event Action OnInteractTriggered;

    public event Action OnCollectTriggered;

    public void Collect()
    {
        OnCollectTriggered?.Invoke();
    }

    public void Interact()
    {
        OnInteractTriggered?.Invoke();
    }

}

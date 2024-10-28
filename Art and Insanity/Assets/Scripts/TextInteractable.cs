using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextInteractable : Interactible
{
    public override void OnInteract()
    {
        print("interatcted with" + gameObject.name);

    }

    public override void OnFocus()
    {
        print("looking at" + gameObject.name);
    }

    public override void OnLoseFocus()
    {
        print("stopped looking at" + gameObject.name);
    }
}

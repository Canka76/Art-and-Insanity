using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactible : MonoBehaviour
{
  public void Awake()
  {
    gameObject.layer = 9;
  }

  public abstract void OnInteract();
  public abstract void OnFocus();
  public abstract void OnLoseFocus();
}

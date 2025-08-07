using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Forzarmamada : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>().retainTransformParent = true;
    }
}

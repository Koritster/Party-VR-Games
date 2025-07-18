using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasketballTargetArea : MonoBehaviour
{
    public MinigameBasketball basketballManager;

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            //Increment player score
            basketballManager.localPlayerHitTarget(10);
        }
    }
}

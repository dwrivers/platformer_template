using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spike : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag != "Player")
            return;

        col.transform.GetComponent<PlayerMovement>().Die();
    }
}

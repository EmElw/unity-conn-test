using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MouseMovement : NetworkBehaviour
{
    // Update is called once per frame

    private float vertical;
    private float horizontal;

    [SyncVar] public int someValue = 0;

    void Update()
    {
        someValue += Random.value < 0.1 ? 1 : 0;
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        GetComponent<Rigidbody2D>().AddForce(new Vector2(horizontal, vertical) * 5);
    }
}
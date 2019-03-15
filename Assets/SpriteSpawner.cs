using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.Networking;

public class SpriteSpawner : NetworkBehaviour
{
    public GameObject spawn;

    private static GameObject playerInstance;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SpawnSprite()
    {
        if (playerInstance != null)
            return;

        var go = Instantiate(spawn, Vector3.zero, Quaternion.identity);

        NetworkServer.Spawn(go);

        playerInstance = go;
    }
}
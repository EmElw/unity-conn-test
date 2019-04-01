using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Broadcaster : NetworkDiscovery
{
    public void InitBroadcast(string msg)
    {
        broadcastData = msg;
        showGUI = false;
        Initialize();
        StartAsServer();
    }

    public override void OnReceivedBroadcast(string fromAddress, string data)
    {
    }
}
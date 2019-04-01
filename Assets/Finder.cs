using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Finder : NetworkDiscovery
{
    private NetworkController controller;

    public override void OnReceivedBroadcast(string fromAddress, string data)
    {
        controller.OnRecvBroadcast(fromAddress, data);
    }

    public void InitFinder(NetworkController networkController)
    {
        controller = networkController;
        showGUI = false;
        Initialize();
        StartAsClient();
    }
}
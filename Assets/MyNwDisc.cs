using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MyNwDisc : NetworkDiscovery
{
    public override void OnReceivedBroadcast(string fromAddress, string data)
    {
        string ownIpv4 = GetLocalIPAddress();
        string incIpv4 = fromAddress.Substring(7);

        if (incIpv4.Equals(ownIpv4))
        {
            // ignore self connections
            return;
        }

        Debug.Log($"{incIpv4} contacted me with {data}");
    }

    public static string GetLocalIPAddress()
    {
        var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }

        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NetworkController : MonoBehaviour
{
    /**
     * A Game Instance (GI) starts broadcasting, sending their team and a time-stamp of when they started broadcasting
     */

    private void Start()
    {
        findTeam(Team.red);
    }

    private string ipv4 = GetLocalIPAddress();

    private Finder finder;

    private Broadcaster broadcaster;

    public void findTeam(Team team)
    {
        gameObject.AddComponent<NetworkManager>();

        var fp = new GameObject(name = "finder_parent");
        var bp = new GameObject(name = "broadcaster_parent");
        fp.transform.parent = transform;
        bp.transform.parent = transform;

        finder = fp.AddComponent<Finder>();
        finder.InitFinder(this);

        broadcaster = bp.AddComponent<Broadcaster>();
        broadcaster.InitBroadcast(team.ToString());
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

    public void OnRecvBroadcast(string fromAddress, string data)
    {
        Debug.Log($"io: {fromAddress}, data: {data}");
    }

    public enum Team
    {
        red,
        green,
        blue,
        yellow,
        purple,
        teal
    }

    public enum State
    {
        searching,
        found_server,
        found_client
    }
}
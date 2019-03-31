using System;
using System.Text;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class MyNetworkManager : NetworkManager, IInputListener

{
    // TODO look into separating client/server start on host (wait for at least one more before starting local client)   

    public static MyNetworkManager singleton;
    public static GameObject playerInstance;
    public GameObject myPlayerPrefab;

    private const short MessageControl = MsgType.Highest + 1;
    private const short MessagGiveClientId = MsgType.Highest + 2;

    private int _clientId = -1;

    private enum State
    {
        Passive,
        Find,
        Host,
        Toggling
    }

    private enum ControlType
    {
        Horizontal,
        Vertical,
        Cannon
    }

    private class ControlMessage : MessageBase
    {
        public ControlMessage()
        {
        }

        public ControlMessage(float value, ControlType type)
        {
            Value = value;
            Type = type;
        }

        public readonly float Value;
        public readonly ControlType Type;
    }


    private State _state = State.Passive;
    private Text _stateText;
    private Text _logText;
    private Text _clientConnText;
    private Text _connText;
    private Text _clientIdText;
    private Text _spawnedText;

    // Start is called before the first frame update
    void Start()
    {
        autoCreatePlayer = false;
        _stateText = GameObject.Find("StateMessage").GetComponent<Text>();
        _stateText.text = _state.ToString();
        _logText = GameObject.Find("LogText").GetComponent<Text>();
        _logText.text = "Start() called\n";
        _clientConnText = GameObject.Find("ClientConnText").GetComponent<Text>();
        _connText = GameObject.Find("ConnText").GetComponent<Text>();
        _clientIdText = GameObject.Find("ClientIdText").GetComponent<Text>();
        _spawnedText = GameObject.Find("SpawnedText").GetComponent<Text>();

        // register to listen for inputs
        GameObject.Find("InputController").GetComponent<InputController>().AddListener(this);
    }

    private void InitMessaging()
    {
        NetworkServer.RegisterHandler(MessageControl, OnServerRcvControlMessage);
        
    }

    private float lastToggleTime;
    private bool host;

    public void ToggleFind()
    {
        if (IsClientConnected()) ResetConnection();

        _state = State.Toggling;
        lastToggleTime = Time.time;
    }

    // Update is called once per frame

    private float lastTime = 0;
    private NetworkClient localClient;

    void Update()
    {
        _stateText.text = _state.ToString();
        _clientConnText.text = IsClientConnected().ToString();
        _clientIdText.text = _clientId.ToString();
        UpdateConnText();
        UpdateSpawnedText();

        if (_state == State.Toggling && Time.time - lastToggleTime > 1)
        {
            ResetConnection();
            if (host)
            {
                Log("Toggle to hosting");
                MyStartHost();
            }
            else
            {
                Log("Toggle to client");
                MyStartFind();
            }

            _state = State.Toggling;
            lastToggleTime = Time.time + Random.value;
            host = !host;
        }
    }

    private void UpdateConnText()
    {
        if (!IsClientConnected())
        {
            _connText.text = "Disconnected";
            return;
        }

        var connections = NetworkServer.connections;
        var text = new StringBuilder();
        if (connections != null)
            foreach (var c in connections)
            {
                var t = $"Conn {c.connectionId}: \n" +
                        $" -- addr {c.address}\n" +
                        $" -- hId {c.hostId}\n" +
                        $" -- cId {c.connectionId}\n";
                text.Append(t);
            }

        _connText.text = text.ToString();
    }

    private void UpdateSpawnedText()
    {
        if (!IsClientConnected())
        {
            _spawnedText.text = "0";
        }

        var spawns = NetworkServer.objects;
        _spawnedText.text = spawns.Count.ToString();
    }

    private void Log(string msg) => _logText.text = $"[{Time.time}] {msg}\n {_logText.text}";


    public void MyStartFind()
    {
        if (client != null || IsClientConnected()) ResetConnection();

        localClient = StartClient();
        _state = State.Find;
        //InitMessaging();
    }

    public void MyStartHost()
    {
        if (client != null || IsClientConnected()) ResetConnection();
        _clientId = 0;
        localClient = StartHost();
        _state = State.Host;
        InitMessaging();
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        var go = Instantiate(myPlayerPrefab);
        NetworkServer.Spawn(go);
        playerInstance = go;
    }

    // Stops networking activity, nukes port usage etc.
    public void ResetConnection()
    {
        if (localClient == null)
            return;

        Log("Resetting connection");


        StopClient();
        StopHost();
        NetworkServer.Reset();
        NetworkServer.Shutdown();
        ResetClientId();
        // StopHost();
        localClient = null;
    }

    private void UnSpawnAll()
    {
        NetworkServer.objects.Clear();
    }

    // Only called server/host side when a client connects to the server
    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);
        // Send a response to the client, giving it a id
        var cId = conn.connectionId;
        if (cId > 0 && _state == State.Toggling)
        {
            Log("TOGGLE STABILIZE IN HOST");
            _state = State.Host;
        }

        Log($"Client {cId} connected");
        NetworkServer.SendToClient(cId, MessagGiveClientId, new IntegerMessage(cId));
    }

    // Called on client when connecting to server (which includes the host "connecting" to itself)
    public override void OnClientConnect(NetworkConnection conn)
    {
        if (_state == State.Toggling && conn.connectionId != 0)
        {
            Log("TOGGLE STABILIZE IN FIND");
            _state = State.Find;
        }

        base.OnClientConnect(conn);
        Log("Connected to server");
        client.RegisterHandler(MessagGiveClientId, OnClientRcvGiveClientID);
    }


    // called on the server when a client is disconnected
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);
        Log("OnServerDisconnect() Not yet implemented");
    }


    // called on all non-host clients when server disconnects
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        ResetClientId();
        Log("OnClientDisconnect() Not yet implemented");
    }

    private void OnClientRcvGiveClientID(NetworkMessage netmsg)
    {
        _clientId = netmsg.ReadMessage<IntegerMessage>().value;
        Log($"Received client ID {_clientId}");
    }

    private void OnServerRcvControlMessage(NetworkMessage netmsg)
    {
        var msg = netmsg.ReadMessage<ControlMessage>();
        var cId = netmsg.conn.connectionId;
        var type = msg.Type;
        var val = msg.Value;
        Log("Rcv msg \n" +
            $" -- type: {type} \n" +
            $" -- val: {val} \n" +
            $" -- cId: {cId}");

        if (playerInstance != null)
        {
            var rb = playerInstance.GetComponent<Rigidbody2D>();
            switch (type)
            {
                case ControlType.Horizontal:
                    rb.AddForce(new Vector2(val, 0) * 5);
                    break;
                case ControlType.Vertical:
                    rb.AddForce(new Vector2(0, val) * 5);
                    break;
                case ControlType.Cannon:
                    rb.AddTorque(0.5f, ForceMode2D.Force);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void ResetClientId() => _clientId = -1;

    public void OnCannonFire()
    {
        Log($"OnCannonFirer in Manager, clientId = {_clientId}");
        if (_clientId != 1)
            return;

        client.Send(MessageControl, new ControlMessage(1, ControlType.Cannon));
    }

    public void OnHorizontal(float value)
    {
        if (_clientId != 2)
            return;

        client.Send(MessageControl, new ControlMessage(value, ControlType.Horizontal));
    }

    public void OnVertical(float value)
    {
        if (_clientId != 3)
            return;

        client.Send(MessageControl, new ControlMessage(value, ControlType.Vertical));
    }
}
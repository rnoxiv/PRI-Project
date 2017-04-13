using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class ServerUI : MonoBehaviour
{
    public void Start()
    {
        NetworkTransport.Init();

        ConnectionConfig config = new ConnectionConfig();
        reliableChannel = config.AddChannel(QosType.Reliable);
        HostTopology topology = new HostTopology(config, 5); // Allow five connections
#if UNITY_EDITOR
        m_hostId = NetworkTransport.AddHostWithSimulator(topology, 200, 400, 25000);
#else
        m_hostId = NetworkTransport.AddHost(topology, 25000);
#endif
    }

    Rect windowRect = new Rect(20, 20, 100, 50);
    Dictionary<int, IPEndPoint> connectionDictionary = new Dictionary<int, IPEndPoint>();

    byte reliableChannel;
    int m_hostId = -1;
    Vector2 scrollPos;
    string receiveLabel;
    public void OnGUI()
    {
        windowRect = GUILayout.Window(GetInstanceID(), windowRect, MyWindow, "Server Window");
    }

    void MyWindow(int id)
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(200.0f), GUILayout.Width(400.0f));
        GUILayout.Label(receiveLabel);
        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear"))
        {
            receiveLabel = "";
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUI.DragWindow();
    }

    public void Update()
    {
        if (m_hostId == -1)
            return;
        int connectionId;
        int channelId;
        byte[] buffer = new byte[1500];
        int receivedSize;
        byte error;
        NetworkEventType networkEvent = NetworkTransport.ReceiveFromHost(m_hostId, out connectionId, out channelId, buffer, 1500, out receivedSize, out error);
        if (networkEvent == NetworkEventType.Nothing)
            return;
        receiveLabel += string.Format("{0} connectionId {1} channelId {2} receivedSize {3}\n", networkEvent.ToString(), connectionId, channelId, receivedSize);
        // If someone connected then save this info
        if (networkEvent == NetworkEventType.ConnectEvent)
        {
            string address;
            int port;
            UnityEngine.Networking.Types.NetworkID network;
            UnityEngine.Networking.Types.NodeID dstNode;
            NetworkTransport.GetConnectionInfo(m_hostId, connectionId, out address, out port, out network, out dstNode, out error);
            receiveLabel += string.Format("address {0} port {1}\n", address, port);
            connectionDictionary.Add(connectionId, new IPEndPoint(IPAddress.Parse(address), port));
        }
        else if (networkEvent == NetworkEventType.DisconnectEvent) // Remove from connection list
        {
            connectionDictionary.Remove(connectionId);
        }
        else if (networkEvent == NetworkEventType.DataEvent)
        {
            // Echo to everyone what we just received
            //// test récupération string ////
            Stream stream = new MemoryStream(buffer);
            BinaryFormatter formatter = new BinaryFormatter();
            string message = formatter.Deserialize(stream) as string;
            receiveLabel += message + " \n";
            //////////////////////////////////
            foreach (var pair in connectionDictionary)
            {
                NetworkTransport.Send(m_hostId, pair.Key, reliableChannel, buffer, receivedSize, out error);
            }
        }
    }
}

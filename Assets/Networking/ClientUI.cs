using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class ClientUI : MonoBehaviour
{
    public void Start()
    {
        NetworkTransport.Init();

        ConnectionConfig config = new ConnectionConfig();
        reliableChannel = config.AddChannel(QosType.Reliable);
        HostTopology topology = new HostTopology(config, 1); // Only connect once
        // Do not put port since we are a client and want to take any port available to us
#if UNITY_EDITOR
        m_hostId = NetworkTransport.AddHostWithSimulator(topology, 200, 400);
#else
        m_hostId = NetworkTransport.AddHost(topology);
#endif
    }

    Rect windowRect = new Rect(500, 20, 100, 50);
    string ipField = System.Net.IPAddress.Loopback.ToString();
    string portField = "25000";
    byte reliableChannel;
    int m_hostId = -1;
    int m_connectionId;
    Vector2 scrollPos;
    string sizeField = "1000";
    string message = "message test";
    string receiveLabel;
    public void OnGUI()
    {
        windowRect = GUILayout.Window(GetInstanceID(), windowRect, MyWindow, "Client Window");
    }

    void MyWindow(int id)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("IP");
        ipField = GUILayout.TextField(ipField);
        GUILayout.Label("Port");
        portField = GUILayout.TextField(portField);
        if (GUILayout.Button("Connect"))
        {
            byte error;
            int connectionId;
            connectionId = NetworkTransport.Connect(m_hostId, ipField, int.Parse(portField), 0, out error);
            if (connectionId != 0) // Could go over total connect count
                m_connectionId = connectionId;
        }
        if (GUILayout.Button("Disconnect"))
        {
            byte error;
            bool ret = NetworkTransport.Disconnect(m_hostId, m_connectionId, out error);
            print("Disconnect " + ret + " error " + error);
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Size");
        sizeField = GUILayout.TextField(sizeField);
        GUILayout.Label("Message");
        message = GUILayout.TextField(message);
        if (GUILayout.Button("Send"))
        {
            byte error;
            byte[] buffer = new byte[int.Parse(sizeField)];
            Stream stream = new MemoryStream(buffer);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, message);
            // Just send junk
            bool ret = NetworkTransport.Send(m_hostId, m_connectionId, reliableChannel, buffer, buffer.Length, out error);
            print("Send " + ret + " error " + error);
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

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
    }

    public void sendData(string data)
    {
        Debug.Log(data);
        if (m_connectionId == 0)
            return;
        byte error;
        byte[] buffer = new byte[int.Parse(sizeField)];
        Stream stream = new MemoryStream(buffer);
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(stream, data);
        // Just send junk
        bool ret = NetworkTransport.Send(m_hostId, m_connectionId, reliableChannel, buffer, buffer.Length, out error);
        print("Send " + ret + " error " + error);
    }
}

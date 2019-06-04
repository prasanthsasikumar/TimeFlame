using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System;
using UnityEngine;
using UnityEngine.UI;

public class SocketServer : MonoBehaviour {

    public string serverIP;
    public int serverPort;
    public Text textOutput;
    String textStr;

    private string TAG = "SocketServer: ";

    private bool isRunningThread = false;
    private Socket server;
    private List<Socket> socketList = new List<Socket>();
    private byte[] msg = new byte[10000000];


    public enum messageID
    {
        msgString = 0,
        msgInt = 1,
        msgFloat = 2,
        msgEmpatica = 3,
        msgMuse = 4,
        EndThread = 99
    }

    // Use this for initialization
    void Start () {

        InitSocketServer();

    }
	
	// Update is called once per frame
	void Update () {
        textOutput.text = textStr;

    }

    void OnApplicationQuit()
    {
        server.Close();
        Debug.Log(TAG + "Server closed");
    }

    //init socket server
    void InitSocketServer()
    {
        server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        IPAddress ip = IPAddress.Parse(serverIP);
        IPEndPoint ip_end_point = new IPEndPoint(ip, serverPort);

        server.Bind(ip_end_point);
        server.Listen(10);
        Debug.Log(TAG + "Start server socket: " + server.LocalEndPoint.ToString());
        textStr = TAG + "Start server socket: " + server.LocalEndPoint.ToString();
        server.BeginAccept(new AsyncCallback(AcceptClient), server);
    }

    void AcceptClient(IAsyncResult ar)
    {
        Socket myserver = ar.AsyncState as Socket;
        Socket client = myserver.EndAccept(ar);
        Debug.Log(TAG + "New Client added, Client ip: " + client.RemoteEndPoint);
        textStr = TAG + "New Client added, Client ip: " + client.RemoteEndPoint;
        socketList.Add(client);

        isRunningThread = true;
        Thread t = new Thread(ReceiveMsg);
        t.Start(client);

        myserver.BeginAccept(new AsyncCallback(AcceptClient), myserver);
    }

    void ReceiveMsg(object socket)
    {
        Socket mSocket = socket as Socket;
        while(isRunningThread)
        {
            try
            {
                int packageLength = mSocket.Receive(msg);
                Debug.Log(TAG + "Client id: " + mSocket.RemoteEndPoint.ToString() + " Package Length: " + packageLength);
            }
            catch(Exception e)
            {
                Debug.LogError(TAG + e.Message);
                socketList.Remove(mSocket);
                //mSocket.Shutdown(SocketShutdown.Both);
                //mSocket.Close();
                break;
            }

            ByteBuffer buff = new ByteBuffer(msg);

            int id = buff.ReadInt();
            if (id == (int)messageID.msgString)
            {
                string mssage = buff.ReadString();
                Debug.Log(TAG + "Client id: " + mSocket.RemoteEndPoint.ToString() + " Test Message: " + mssage);
            }
            else if (id == (int)messageID.msgInt)
            {
                int mssage = buff.ReadInt();
                Debug.Log(TAG + "Client id: " + mSocket.RemoteEndPoint.ToString() + " Test Message: " + mssage);
            }
            else if(id == (int)messageID.msgFloat)
            {
                float mssage = buff.ReadFloat();
                Debug.Log(TAG + "Client id: " + mSocket.RemoteEndPoint.ToString() + " Test Message: " + mssage);
            }
            else if (id == (int)messageID.EndThread)
            {
                isRunningThread = false;
                string mssage = buff.ReadString();
                Debug.Log(TAG + "Client id: " + mSocket.RemoteEndPoint.ToString() + " Test Message: " + mssage);
            }
            else if (id == (int)messageID.msgEmpatica)
            {
                float bvp = buff.ReadFloat();
                float ibi = buff.ReadFloat();
                float gsr = buff.ReadFloat();

                Debug.Log(TAG + "Client id: " + mSocket.RemoteEndPoint.ToString() + " BVP: " + bvp + " IBI: " + ibi + " GSR: " + gsr);
                textStr = TAG + "Client id: " + mSocket.RemoteEndPoint.ToString() + " BVP: " + bvp + " IVI: " + ibi + " GSR: " + gsr;
            }
            else if (id == (int)messageID.msgMuse)
            {
                double tp9 = buff.ReadDouble();
                double af7 = buff.ReadDouble();
                double af8 = buff.ReadDouble();
                double tf10 = buff.ReadDouble();

                Debug.Log(TAG + "Client id: " + mSocket.RemoteEndPoint.ToString() + " tp9: " + tp9 + " af7: " + af7 + " af8: " + af8 + " tf10: " + tf10);
                textStr = TAG + "Client id: " + mSocket.RemoteEndPoint.ToString() + " tp9: " + tp9 + " af7: " + af7 + " af8: " + af8 + " tf10: " + tf10;
            }

            else
                continue;    
        }
    }

    void sendMsgString(int id, string msg)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteInt(id);
        buffer.WriteString(msg);

        for (int i = 0; i < socketList.Count; i++)
        {           
            int msgLength = socketList[i].Send(buffer.ToBytes());
            Debug.Log(TAG + "Send msg length: " + msgLength);
        }
    }

    void sendMsgInt(int id, int msg)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteInt(id);
        buffer.WriteInt(msg);

        for (int i = 0; i < socketList.Count; i++)
        {
            int msgLength = socketList[i].Send(buffer.ToBytes());
            Debug.Log(TAG + "Send msg length: " + msgLength);
        }
    }

    void sendMsgFloat(int id, float msg)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteInt(id);
        buffer.WriteFloat(msg);

        for (int i = 0; i < socketList.Count; i++)
        {
            int msgLength = socketList[i].Send(buffer.ToBytes());
            Debug.Log(TAG + "Send msg length: " + msgLength);
        }
    }

    //Test on UI
    void OnGUI()
    {
         if (GUI.Button(new Rect(100, 50, 200, 100), "Send String to client"))
         {
            if(isRunningThread)
                sendMsgString((int)messageID.msgString, "Message from server...");
         }

        if (GUI.Button(new Rect(100, 200, 200, 100), "Send Int to client"))
        {
            if (isRunningThread)
                sendMsgInt((int)messageID.msgInt, 12);
        }

        if (GUI.Button(new Rect(100, 350, 200, 100), "Send Float to client"))
        {
            if (isRunningThread)
                sendMsgFloat((int)messageID.msgFloat, 16.66f);
        }

    }
}

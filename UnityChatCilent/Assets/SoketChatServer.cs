using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketWrapper;
using System.Threading;

public class SoketChatServer : MonoBehaviour
{
    SocketClient socketClient = new SocketClient();
    public string m_strServerIP;
    public int m_nPort;
    public string m_strInputText;
    string m_strChatString;
    string m_strResiveBuffer;
    List<string> m_listMSG = new List<string>();

    private void Start()
    {
        socketClient.Init();
        socketClient.Connect(m_strServerIP, m_nPort);
        ThreadStart threadStart = new ThreadStart(socketClient.ReceivedCallBack);
        Thread thread = new Thread(threadStart);
        thread.Start();
    }
    int idx = 0;
    private void Update()
    {

        for (int i = 0; i < 9999; i++)
        {
            socketClient.SendData(string.Format("test{0}", idx));
            idx++;
        }
  

        if(socketClient.CheckReciveData)
        {
            //m_listMSG.Add(socketClient.ResiveMsg);
           m_strChatString += socketClient.ResiveMsg + "\n";
        }
    }

    Vector2 vScrollPos;
    private void OnGUI()
    {
        int size = 9999;
        vScrollPos = GUI.BeginScrollView(new Rect(0, 0, 300, 200), vScrollPos, new Rect(0, 0, 300, 20 * size)); ;
        GUI.Box(new Rect(0, 0, 300, 20 * size), m_strChatString);
        GUI.EndScrollView();

        m_strInputText = GUI.TextField(new Rect(0,200,200,20), m_strInputText);
        if(GUI.Button(new Rect(200, 200,100,20), "Send:"+ socketClient.CheckReciveData))
        {
            socketClient.SendData(m_strInputText);
            m_strInputText = "";
        }
    }
}

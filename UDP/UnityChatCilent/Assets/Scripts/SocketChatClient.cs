using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketWrapper;
using NetworkFramework;
using System.Threading;
using System.Text;
using CSToUnityUtill;
using System.Threading.Tasks;

public class SocketChatClient : MonoBehaviour
{
    //SocketClient socketClient = new SocketClient();
    UDPSocketWrapper udpSocketClient = new UDPSocketWrapper();
    public string m_strID;
    public string m_strServerIP;
    public int m_nPort;
    public string m_strInputText;
    string m_strChatString;
    string m_strResiveBuffer;
    List<string> m_listMSG = new List<string>();

    public int m_nFramePerSendCount = 2;

    ReceiveCallBack receiveCallBack;
    Task taskResive;

    private void Start()
    {
        //socketClient.Init();
        //socketClient.Connect(m_strServerIP, m_nPort);
        //ThreadStart threadStart = new ThreadStart(socketClient.ReceivedCallBack);
        //Thread thread = new Thread(threadStart);
        //thread.Start();
        udpSocketClient.Init();
        udpSocketClient.SendStringMsg("Connet ID:" + m_strID, m_strServerIP, m_nPort);
        Log.WriteLine(m_strID + " Connect to Wait.. [" + m_strServerIP + "/" + m_nPort + "]");
        ReceiveInfo sInfoReceiveInfo = udpSocketClient.RecivedData();
        Log.WriteLine("received data :", sInfoReceiveInfo.GetMsg());
        Log.WriteLine(m_strID + " Connect to Wait.. [" + m_strServerIP + "/" + m_nPort + "]");

        Log.WriteLine("Waiting Server Msg... CallBack Ready...");
        receiveCallBack = new ReceiveCallBack(udpSocketClient.RecivedData);
        taskResive = new Task(receiveCallBack.ProcessReceive);
        Log.WriteLine("Waiting Server Msg... CallBack Start...");
        taskResive.Start();
        receiveCallBack.StartReceive();
    }

    int idx = 0;
    int reciveCount = 0;
    private void Update()
    {

        //for (int i = 0; i < m_nFramePerSendCount; i++)
        //{
        //    //socketClient.SendData(string.Format("test{0}", idx));
        //    udpSocketClient.SendStringMsg(string.Format("test{0}",idx), m_strServerIP, m_nPort);
        //    idx++;
        //}

        if (receiveCallBack.MsgExist())
        {
            //byte[] buffer = udpSocketClient.GetBuffer();
            ReceiveInfo receiveInfo = receiveCallBack.GetMsg();
            string strMsg = receiveInfo.GetMsg();
            if (strMsg != null)
            {
                if (strMsg == "remote_exit")
                {
                    Log.WriteLine("Server Exit_", strMsg);
                    receiveCallBack.StopReceive();
                }
                else
                {
                   
                    //m_listMSG.Add(socketClient.ResiveMsg);
                    //string m_strResiveMsg = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                    m_strChatString += (strMsg + "\n");
                    reciveCount++;
                }
            }
        }
    }

    Vector2 vScrollPos;
    private void OnGUI()
    {
        int h = 25;
        vScrollPos = GUI.BeginScrollView(new Rect(0, 0, 300, 200), vScrollPos, new Rect(0, 0, 300, h * reciveCount)); ;
        GUI.Box(new Rect(0, 0, 300, h * reciveCount), m_strChatString);
        GUI.EndScrollView();

        m_strInputText = GUI.TextField(new Rect(0,200,200,20), m_strInputText);
 
        if(GUI.Button(new Rect(200, 200,100,20), "Send"))
        {
            string strSendMsg = string.Format("{0}:{1}", m_strID, m_strInputText);
            udpSocketClient.SendStringMsg(strSendMsg, m_strServerIP, m_nPort);
            m_strChatString += (strSendMsg + "\n");
            reciveCount++;
            m_strInputText = "";
        }
    }

    private void OnDestroy()
    {
        Debug.Log("OnDestroy");
    }
}

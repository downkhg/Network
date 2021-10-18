using CSToUnityUtill;
using NetworkFramework;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SocketChatServer : MonoBehaviour
{
    UDPSocketWrapper udpSocketServer = new UDPSocketWrapper();
    public string m_strID;
    public int m_nPort;

    public string m_strInputText;
    List<string> m_listMSG = new List<string>();
    string m_strChatString;

    ReceiveCallBack receiveCallBack;
    Task taskResive;
    // Start is called before the first frame update
    void Start()
    {
        Log.WriteLine("Server Start...");
        udpSocketServer.Init();
        udpSocketServer.Bind(m_nPort);

        //Log.WriteLine("Waiting for a client...");
        //ReceiveInfo sInfoReceiveInfo = udpSocketServer.RecivedData();

        //Log.WriteLine("Recvie ClientInfo : {0}", sInfoReceiveInfo.GetMsg());
        //string strConnetMsg = "Connet Udp Chat!";
        //udpSocketServer.SendStringMsg(sInfoReceiveInfo.endPoint, strConnetMsg);

        //입력처리용 태스크 생성 및 처리
        Log.WriteLine("Waiting Client Msg... CallBack Ready...");
        receiveCallBack = new ReceiveCallBack(udpSocketServer.RecivedData);
        taskResive = new Task(receiveCallBack.ProcessReceive);
        taskResive.Start();
        Log.WriteLine("Waiting Client Msg... CallBack Start!");
        receiveCallBack.StartReceive();
    }

    int idx = 0;
    int reciveCount = 0;
    // Update is called once per frame
    void Update()
    {
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
                    string[] strMsgs = strMsg.Split(':');
                    Debug.Log("strMsgs[0]:" + strMsgs[0]);
                    if (strMsgs[0] == "Connet ID")
                    {
                        udpSocketServer.SendBoradCast(strMsg);
                    }
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
        vScrollPos = GUI.BeginScrollView(new Rect(0, 0, 300, 200), vScrollPos, new Rect(0, 0, 300, h * reciveCount));
        GUI.Box(new Rect(0, 0, 300, h * reciveCount), m_strChatString);
        GUI.EndScrollView();

        m_strInputText = GUI.TextField(new Rect(0, 200, 200, 20), m_strInputText);

        if (GUI.Button(new Rect(200, 200, 100, 20), "Send"))
        {
            string strSendMsg = string.Format("{0}:{1}", m_strID, m_strInputText);
            udpSocketServer.SendBoradCast(strSendMsg);
            m_strChatString += (strSendMsg + "\n");
            reciveCount++;
            m_strInputText = "";
        }

        if (GUI.Button(new Rect(300, 200, 100, 20), "Close"))
        {
            receiveCallBack.StopReceive();
            taskResive.Wait();
            udpSocketServer.Close();
        }
    }

    private void OnDestroy()
    {
        Debug.Log("OnDestroy");
    }
}

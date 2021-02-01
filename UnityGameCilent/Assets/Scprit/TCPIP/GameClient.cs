/*##################################
소켓클라이언트_스레드(포트폴리오 수업용)
파일명: SoketGameClient.cs
작성자 : 김홍규(downkhg@gmail.com)
마지막수정날짜 : 2019.11.28
유니티작업버전: 2018.3.14
버전 : 1.00
###################################*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CSToUnityUtill;
using System.Threading;
[System.Serializable]
public class GameClient
{
    SocketClient socketClient = new SocketClient();
    public string m_strServerIP= "192.168.0.34";
    public int m_nPort = 15000;
    public string m_strInputText;

    public string m_strChatString;
    public string m_strResiveBuffer;

    public SocketClient GetSocketClient()
    {
        return socketClient;
    }

    public void Init()
    {
        socketClient.Init();
    }

    public void Connect()
    {
        if (socketClient.Connect(m_strServerIP, m_nPort))
        {
            Log.WriteLine("Connect Complet!!");
            ThreadStart threadStart = new ThreadStart(socketClient.ReceivedCallBack);
            Thread thread = new Thread(threadStart);
            thread.Start();
            Log.WriteLine("Receive Start!!");
        }
        else
            Log.WriteLine("Connect is Failed!");
    }

    public void UpdateRecive(GameManager gameManager)
    {
        if (socketClient.CheckReciveData)
        {
            ProcessRecive(gameManager);
        }
    }

    public void SendData(string msg)
    {
        socketClient.SendData(msg);
    }

    public void ProcessRecive(GameManager gameManager)
    {
        string reciveData = socketClient.ResiveMsg;
        string[] recivePart = reciveData.Split(new char[] { ':' });

        string header = recivePart[0];
        string body = recivePart[1];

        string[] bodyContents = body.Split(new char[] { ',' });
        string id = bodyContents[0];
        Vector3 pos = new Vector3();
        Vector3 dir = new Vector3();
        float speed = 0;

        switch (header)
        {
            case "client":
                gameManager.m_nClientCount = int.Parse(bodyContents[0]);
                break;
            case "connect":
                //body:"id",0,0,0
                //m_strChatString += socketClient.ResiveMsg;
                pos.x = float.Parse(bodyContents[1]);
                pos.y = float.Parse(bodyContents[2]);
                pos.z = float.Parse(bodyContents[3]);
                gameManager.CreatePlayer(id, pos);
                break;
            case "disconnet":
                gameManager.DeletePlayer(id);
                break;
            case "move":
                //body:"id",0,0,0,1,0,0,0
                //body:id,dir,speed,pos
                //m_strChatString += socketClient.ResiveMsg;
                pos.x = float.Parse(bodyContents[1]);
                pos.y = float.Parse(bodyContents[2]);
                pos.z = float.Parse(bodyContents[3]);
                dir.x = float.Parse(bodyContents[4]);
                dir.y = float.Parse(bodyContents[5]);
                dir.z = float.Parse(bodyContents[6]);
                speed = float.Parse(bodyContents[7]);
                gameManager.PlayerMove(id, pos, dir, speed);
                break;
            default:
                //m_strChatString += socketClient.ResiveMsg + "\n";
                break;
        }
    }


    Vector2 vScrollPos;
    public void OnGUI(GameManager gameManager)
    {
        int size = 100;
        vScrollPos = GUI.BeginScrollView(new Rect(0, 0, 300, 200), vScrollPos, new Rect(0, 0, 300, 20 * size)); ;

        GUI.Box(new Rect(0, 0, 300, 20 * size), m_strChatString);
        GUI.EndScrollView();

        m_strInputText = GUI.TextField(new Rect(0,200,200,20), m_strInputText);
        if(GUI.Button(new Rect(200, 200,100,20), "Send:"+ socketClient.CheckReciveData))
        {
            socketClient.SendDataIP(m_strInputText);
            m_strInputText = "";
        }

        if (GUI.Button(new Rect(Screen.width - 100, 0, 100, 20), "Login"))
        {
            gameManager.SendConnect(m_strInputText, new Vector3());
        }
    }
}

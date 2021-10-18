
/*##################################
게임클라이언트_소켓클라이언트래퍼(포트폴리오 수업용)
파일명: SocketClient.cs
작성자 : 김홍규(downkhg@gmail.com)
이전수정날짜 : 2019.11.28
마지막수정날짜 : 2021.02.04(버퍼큐사용)
유니티작업버전: 2018.3.14
버전 : 1.05
###################################*/
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using CSToUnityUtill;
using System;
using System.Text;
#if UNITY_STANDALONE || UNITY_ANDROID
using UnityEngine;
#endif

public class SocketClient
{
    Socket m_socketClient;
    IPAddress m_ServerIPAddress;
    bool m_isRecive = false;
    bool m_isReciveData = false;

    Queue<byte[]> m_listBufferQueue = new Queue<byte[]>();

    public bool CheckReciveData { get { return m_isReciveData; } }

    static public IPAddress GetIPAddress()
    {
        IPHostEntry he = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress iPAddress = null;
        // 처음으로 발견되는 ipv4 주소를 사용한다.
        foreach (IPAddress addr in he.AddressList)
        {
            if (addr.AddressFamily == AddressFamily.InterNetwork)
            {
                iPAddress = addr;
                break;
            }
        }

        // 주소가 없다면..
        if (iPAddress == null)
            // 로컬호스트 주소를 사용한다.
            iPAddress = IPAddress.Loopback;

        return iPAddress;
    }
    //소켓을 초기화한다.
    public void Init()
    {
        Console.WriteLine("Client Socket Init...");
        m_socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

    }
    //서버에 접속을 시도한다.
    public bool Connect(string serverip, int port)
    {
        m_ServerIPAddress = IPAddress.Parse(serverip);
        IPEndPoint iPEndPoint = new IPEndPoint(m_ServerIPAddress, port);
        Log.WriteLine("Client Connecting!" + iPEndPoint);
        try
        {
            m_socketClient.Connect(iPEndPoint);
            Log.WriteLine(m_socketClient.LocalEndPoint.ToString());
            Log.WriteLine(iPEndPoint.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine("Connect Failed!" + ex);
            return false;
        }
        return true;
    }
    //접속된 서버에 메세지를 보낸다.
    public void SendData(string msg)
    {
        try
        {
            string packet = string.Format("{0}:{1}\n", m_ServerIPAddress, msg);
            byte[] bytes = Encoding.UTF8.GetBytes(packet);
            m_socketClient.Send(bytes);
        }
        catch (Exception e)
        {
            Log.WriteLine("Exception:" + e);
        }
    }

    public byte[] GetBuffer()
    {

        byte[] bytes = null;
        if (m_listBufferQueue.Count > 0)
        {
            bytes = m_listBufferQueue.Dequeue();
        }
        return bytes;
    }

    //접속된 서버에서 데이터를 받는다.
    public void ReceivedCallBack()
    {
        Console.WriteLine("ReceivedCallBack Start!");
        m_isRecive = true;
        byte[] bytes = null;
        NetworkStream stream = null;
        try
        {
            bytes = new byte[1024];
            stream = new NetworkStream(m_socketClient);

            while (m_isRecive)
            {
                Log.WriteLine("Buffer Read Start!");
                int byteSize = stream.Read(bytes, 0, bytes.Length);
                //m_strResiveMsg = Encoding.UTF8.GetString(bytes, 0, byteSize);
                //Console.WriteLine("Received:" + m_strResiveMsg);
                //Array.Clear(bytes, 0, 1024);
                //m_isReciveData = true;
                m_listBufferQueue.Enqueue(bytes);
                Log.WriteLine("BufferQueueCount:{0}", m_listBufferQueue.Count);
            }

            stream.Close();
            Log.WriteLine("ReceivedCallBack End!");
        }
        catch (Exception e)
        {
            Log.WriteLine("Exception:" + e);
        }
    }
}
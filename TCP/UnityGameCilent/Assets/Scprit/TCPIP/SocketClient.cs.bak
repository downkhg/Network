/*##################################
게임클라이언트_소켓클라이언트래퍼(포트폴리오 수업용)
파일명: SocketClient.cs
작성자 : 김홍규(downkhg@gmail.com)
마지막수정날짜 : 2019.11.28
유니티작업버전: 2018.3.14
버전 : 1.00
###################################*/
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using CSToUnityUtill;
using System;
using System.Text;

public class SocketClient
{
    Socket m_socketClient;
    IPAddress m_ServerIPAddress;
    bool m_isRecive = false;
    bool m_isReciveData = false;
    bool m_isLoop = true;
    string m_strResiveMsg;
    int m_nBufferSize = 1024;

    public bool CheckReciveData { get { return m_isReciveData; } }
    public string ResiveMsg { get { m_isReciveData = false; return m_strResiveMsg; } }

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
        Log.WriteLine("Client Socket Init...");
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
            Log.WriteLine("Connect Failed!" + ex);
            return false;
        }
        return true;
    }
    public bool CheckDisconnet()
    {
        return m_isRecive;
    }
    public void Disconnet()
    {
        m_isRecive = false;
    }
    //접속된 서버에 메세지를 보낸다.
    public void SendDataIP(string msg)
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
    //접속된 서버에 메세지를 보낸다.
    public void SendData(string msg)
    {
        try
        {
            string packet = string.Format("{0}\n", msg);
            byte[] temp = Encoding.UTF8.GetBytes(packet); 
            byte[] bytes = new byte[m_nBufferSize];
            for (int i = 0; i < temp.Length; i++) bytes[i] = temp[i];
            m_socketClient.Send(bytes);
        }
        catch (Exception e)
        {
            Log.WriteLine("Exception:" + e);
        }
    }
    //접속된 서버에서 데이터를 받는다.
    public void ReceivedCallBack()
    {
        Console.WriteLine("ReceivedCallBack Start!");
        m_isRecive = true;
        byte[] bytes = null;
        NetworkStream stream = null;
        m_isLoop = true;
        try
        {
            bytes = new byte[m_nBufferSize];
            stream = new NetworkStream(m_socketClient);

            while (m_isRecive)
            {
                if (m_isReciveData == false)
                {
                    int byteSize = stream.Read(bytes, 0, bytes.Length);
                    m_strResiveMsg = Encoding.UTF8.GetString(bytes, 0, byteSize);
                    Log.WriteLine("Received["+m_strResiveMsg.Length+"]:" + m_strResiveMsg);
                    Array.Clear(bytes, 0, bytes.Length);
                    m_isReciveData = true;
                }
            }

            stream.Close();
            Log.WriteLine("ReceivedCallBack End!");
        }
        catch (Exception e)
        {
            Log.WriteLine("Exception:" + e);
        }
        m_isLoop = false;
    }
}
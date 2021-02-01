using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GUIManager m_cGUIManager;
    public GameClient m_cGameClient;
    public Dictionary<string,Dynamic> m_listDynamic = new Dictionary<string,Dynamic>();
    public string m_myID;
    public int m_nClientCount;
    public Dynamic m_cMyDynamic;

    static GameManager m_cInstance;

    public static GameManager GetInstance()
    {
        return m_cInstance;
    }

    public  Dynamic GetPlayer(string id)
    {
        return m_listDynamic[id];
    }

    public void EventConnect()
    {
        m_cGameClient.m_strServerIP = m_cGUIManager.m_inputServerIP.text;
        m_cGameClient.Connect();
    }

    public void EventLogin()
    {
        string id = m_cGUIManager.m_inputLoginID.text; 
        if (!(id == "" || id == "Input U id"))
        {
            SendConnect(id, Vector3.zero);
            m_cGUIManager.SetState(GUIManager.eGUIState.PLAY);
        }
    }

    public void EventExit()
    {
        m_cGameClient.SendData(string.Format("disconnet:{0}", m_myID));
        m_cGameClient.GetSocketClient().Disconnet();
    }

    // Start is called before the first frame update
    void Start()
    {
        m_cGUIManager.m_inputServerIP.text = m_cGameClient.m_strServerIP;
        m_cGameClient.Init();
        m_cInstance = this;
    }

    public void SendConnect(string id, Vector3 vPos)
    {
        string msg = string.Format("connect:{0},{1},{2},{3}", id,vPos.x,vPos.y,vPos.z);
        m_myID = id;
        m_cGameClient.SendData(msg);
        Debug.Log("EvnetSend:"+msg);
    }

    public void SendMove(string id, Vector3 pos, Vector3 dir, float speed)
    {
        Dynamic dynamic = m_listDynamic[id];
        if (dynamic)
        { 
            string msg = string.Format("move:{0},{1},{2},{3},{4},{5},{6},{7}", id, pos.x, pos.y, pos.z, dir.x,dir.y, dir.z, speed);
            m_cGameClient.SendData(msg);
            Debug.Log("EvnetSend:" + msg);
        }
    }

    public void DeletePlayer(string id)
    {
        Dynamic dynamic = m_listDynamic[id];
        if (dynamic)
            Destroy(dynamic.gameObject);
    }

    public void CreatePlayer(string id, Vector3 pos)
    {
        Debug.Log("CreatePlayer:" + id);
        GameObject prefabs = Resources.Load<GameObject>("Prefabs/Player");

        GameObject objPlayer = Instantiate(prefabs, pos, Quaternion.identity);
        Dynamic cDynamic = objPlayer.GetComponent<Dynamic>();
        cDynamic.id = id;
        m_listDynamic.Add(id, cDynamic);
        m_cMyDynamic = cDynamic; 
    }

    public void PlayerMove(string id, Vector3 pos, Vector3 dir, float speed)
    {
        Debug.Log("PlayerMove:" + id + pos + dir + speed);
        Dynamic dynamic = m_listDynamic[id];
        if (dynamic)
        {
            dynamic.transform.position = pos;
            dynamic.Move(dir, speed);
            Debug.Log("EventPlayerMove:" + dynamic.transform.position);
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (m_cGameClient.GetSocketClient().CheckDisconnet())
            m_cGameClient.UpdateRecive(this);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EventExit();
        }
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (!m_cGameClient.GetSocketClient().CheckDisconnet())
                Application.Quit(0); 
        }
    }

    //private void OnGUI()
    //{
    //    m_cSoketChatServer.OnGUI(this);
    //}
}

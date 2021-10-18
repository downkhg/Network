using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour
{
    public List<GameObject> m_listGUIScenes;
    public enum eGUIState { LOGIN, PLAY}
    public eGUIState m_eCurGUIState;

    public InputField m_inputServerIP;
    public InputField m_inputLoginID;
    public Text m_textClientCount;

    public void ShowState(eGUIState state)
    {
        for (int i = 0; i < m_listGUIScenes.Count; i++)
        {
            if (i == (int)state)
                m_listGUIScenes[i].SetActive(true);
            else
                m_listGUIScenes[i].SetActive(false);
        }
    }

    public void SetState(int state)
    {
        SetState((eGUIState)state);
    }

    public void SetState(eGUIState state)
    {
        switch (state)
        {
            case eGUIState.LOGIN:
                Time.timeScale = 0;
                break;
            case eGUIState.PLAY:
                Time.timeScale = 1;
                break;  
        }
        m_eCurGUIState = state;
        ShowState(state);
    }

    public void UpdataState()
    {
        switch (m_eCurGUIState)
        {
            case eGUIState.LOGIN:
                int count = GameManager.GetInstance().m_nClientCount;
                m_textClientCount.text = string.Format("ClientCount:{0}",count);  
                break;
            case eGUIState.PLAY:
            
                break;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("GUIManager.Start!");
        SetState(m_eCurGUIState); //시작시 설정된 씬으로 전환
        Debug.Log("GUIManager.End!");
    }

    // Update is called once per frame
    void Update()
    {
        UpdataState();
    }
}
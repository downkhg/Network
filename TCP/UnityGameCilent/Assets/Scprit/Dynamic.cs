using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dynamic : MonoBehaviour
{
    public float m_fSpeed = 1;
    public Vector3 m_vDir;
    public string id;

    // Start is called before the first frame update
    void Start()
    {
        m_vDir = transform.forward;
    }

    public void Move(Vector3 dir, float speed)
    {
        m_vDir = dir;
        transform.Translate(dir * speed * Time.deltaTime);
    }

    // Update is called once per frame
    void Update()
    {
        GameManager gameManager = GameManager.GetInstance();

        if (id != GameManager.GetInstance().m_myID) return;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            //Move(Vector3.forward, m_fSpeed);
            gameManager.SendMove(id, transform.position, Vector3.forward, m_fSpeed);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            //Move(Vector3.back, m_fSpeed);
            gameManager.SendMove(id, transform.position, Vector3.back, m_fSpeed);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            //Move(Vector3.right, m_fSpeed);
            gameManager.SendMove(id, transform.position, Vector3.right, m_fSpeed);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            //Move(Vector3.left, m_fSpeed);
            gameManager.SendMove(id, transform.position, Vector3.left, m_fSpeed);
        }
    }

    //private void OnGUI()
    //{
    //    GUI.Box(new Rect(Screen.width - 200, 0, 200, 20),"Pos"+transform.position);
    //}
}

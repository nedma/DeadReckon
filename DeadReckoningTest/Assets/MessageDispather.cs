using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public struct StateMsg
{
    public Vector3 Pos;
    public Vector3 Vel;
    public Vector3 Acc;

    public float TimeStamp;
}

public class MessageDispather : MonoBehaviour 
{
    public DRComponent Receiver;
    public DRPublishingComponent Sender;

    public float DelaySec = 0.05f;
    public float LostRate = 0.0f;


    public static MessageDispather Instance = null;



    class Msg
    {
        public StateMsg Content;
        public float DeliveryTime;
        public bool Delivered;

        public Msg(StateMsg inContent, float inDeliveryTime)
        {
            Content = inContent;
            DeliveryTime = inDeliveryTime;
            Delivered = false;
        }
    }
    List<Msg> m_MsgList = new List<Msg>();

    public void SendMessage(StateMsg newMsg)
    {
        bool shouldBeLost = false;


        if (LostRate != 0.0f)
        {
            float rand = Random.Range(0.0f, 1.0f);
            if (rand <= LostRate)
            {
                shouldBeLost = true;
            }
        }


        if (!shouldBeLost)
        {
            if (DelaySec == 0.0f)
            {
                Dispatch(newMsg);
            }
            else
            {
                m_MsgList.Add(new Msg(newMsg, Time.realtimeSinceStartup + DelaySec));
            }
        }
    }





    void Awake ()
    {
        Instance = this;
    }


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void LateUpdate () 
    {
        // send by added order
        for (int i = 0; i < m_MsgList.Count; i++ )
        {
            if (Time.realtimeSinceStartup >= m_MsgList[i].DeliveryTime)
            {
                Dispatch(m_MsgList[i].Content);

                m_MsgList[i].Delivered = true;
            }
        }


        for (int i = m_MsgList.Count - 1; i >= 0; i-- )
        {
            if (m_MsgList[i].Delivered)
            {
                m_MsgList.RemoveAt(i);
            }
        }
	}

    void Dispatch(StateMsg sMsg)
    {
        Receiver.SetLastKnownTranslation(sMsg.Pos);
        Receiver.SetLastKnownVelocity(sMsg.Vel);
        Receiver.SetLastKnownAcceleration(sMsg.Acc);
    }
}

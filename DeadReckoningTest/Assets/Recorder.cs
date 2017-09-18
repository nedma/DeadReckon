using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Recorder : MonoBehaviour 
{
    struct Record
    {
        public Vector3 Pos;
        public int FrameCount;

        public int Flag;

        public Record(Vector3 inPos, int inFrameCount, int inFlag)
        {
            Pos = inPos;
            FrameCount = inFrameCount;
            Flag = inFlag;
        }
    }

    public float BaseSphereRadius = 0.3f;
    public int RecordMaxFrame = 50;

    public bool ShowPrediction;
    public bool ShowPath;

    public int OldestRecordFrameCount
    {
        get
        {
            if (m_PathRecord.Count == 0)
            {
                return -1;
            }
            else
            {
                return m_PathRecord[0].FrameCount;
            }
        }
    }

    int m_Flag = 0;


    List<Record> m_SnapshotRecord = new List<Record>();
    List<Record> m_PredictionRecord = new List<Record>();
    List<Record> m_PathRecord = new List<Record>();

    public void SetSnapshot(Vector3 pos)
    {
        if (m_SnapshotRecord.Count > 0)
        {
            if (m_SnapshotRecord[0].FrameCount < OldestRecordFrameCount)
            {
                m_SnapshotRecord.RemoveAt(0);
            }
        }


        m_Flag = (++m_Flag) % 2;

        m_SnapshotRecord.Add(new Record(pos, Time.frameCount, m_Flag));

    }

    public void SetPrediction(Vector3 pos)
    {
        if (m_PredictionRecord.Count > 0)
        {
            if (m_PredictionRecord[0].FrameCount < OldestRecordFrameCount)
            {
                m_PredictionRecord.RemoveAt(0);
            }
        }

        m_PredictionRecord.Add(new Record(pos, Time.frameCount, m_Flag));
    }

    public void SetPath(Vector3 pos)
    {
        if (m_PathRecord.Count > RecordMaxFrame)
        {
            m_PathRecord.RemoveAt(0);
        }

        m_PathRecord.Add(new Record(pos, Time.frameCount, m_Flag));
    }


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        for (int i = 0; i < m_SnapshotRecord.Count; i++ )
        {
            Gizmos.DrawSphere(m_SnapshotRecord[i].Pos, BaseSphereRadius);
        }

        if (ShowPrediction)
        {    
            for (int i = 0; i < m_PredictionRecord.Count; i++)
            {
                Gizmos.color = m_PredictionRecord[i].Flag == 0 ? Color.blue : new Color(0.1f, 0.4f, 0.9f, 1);
                Gizmos.DrawSphere(m_PredictionRecord[i].Pos, BaseSphereRadius * 0.5f);

                if (i + 1 < m_PredictionRecord.Count)
                {
                    Gizmos.DrawLine(m_PredictionRecord[i].Pos, m_PredictionRecord[i + 1].Pos);
                }
            }
        }

        if (ShowPath)
        {
            for (int i = 0; i < m_PathRecord.Count; i++)
            {
                Gizmos.color = m_PathRecord[i].Flag == 0 ? Color.green : new Color(0.6f, 0.9f, 0.2f, 1);
                Gizmos.DrawSphere(m_PathRecord[i].Pos, BaseSphereRadius * 0.5f);

                if (i + 1 < m_PathRecord.Count)
                {
                    Gizmos.DrawLine(m_PathRecord[i].Pos, m_PathRecord[i + 1].Pos);
                }
            }
        }
    }
}

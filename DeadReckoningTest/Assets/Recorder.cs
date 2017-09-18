using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Recorder : MonoBehaviour 
{
    struct Record
    {
        public Vector3 Pos;
        public int FrameCount;

        public Record(Vector3 inPos, int inFrameCount)
        {
            Pos = inPos;
            FrameCount = inFrameCount;
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

        m_SnapshotRecord.Add(new Record(pos, Time.frameCount));
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

        m_PredictionRecord.Add(new Record(pos, Time.frameCount));
    }

    public void SetPath(Vector3 pos)
    {
        if (m_PathRecord.Count > RecordMaxFrame)
        {
            m_PathRecord.RemoveAt(0);
        }

        m_PathRecord.Add(new Record(pos, Time.frameCount));
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
            Gizmos.color = Color.blue;
            for (int i = 0; i < m_PredictionRecord.Count; i++)
            {
                Gizmos.DrawSphere(m_PredictionRecord[i].Pos, BaseSphereRadius * 0.5f);

                if (i + 1 < m_PredictionRecord.Count)
                {
                    Gizmos.DrawLine(m_PredictionRecord[i].Pos, m_PredictionRecord[i + 1].Pos);
                }
            }
        }

        if (ShowPath)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < m_PathRecord.Count; i++)
            {
                Gizmos.DrawSphere(m_PathRecord[i].Pos, BaseSphereRadius * 0.5f);

                if (i + 1 < m_PathRecord.Count)
                {
                    Gizmos.DrawLine(m_PathRecord[i].Pos, m_PathRecord[i + 1].Pos);
                }
            }
        }
    }
}

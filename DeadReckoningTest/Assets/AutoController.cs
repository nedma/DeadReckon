using UnityEngine;
using System.Collections;

public class AutoController : MonoBehaviour 
{
    public enum EMode
    {
        None,
        Const_Vel_Local,
        Const_Vel_World,
        Const_Acc_Local,
        Const_Acc_World
    }
    public EMode Mode = EMode.None;

    public Vector3 Value;

    public bool FacingMoveDir = true;

    Vector3 m_LastPos;
    float m_AvgPosChangeInOneFrame = 0.0f;

	// Use this for initialization
	void Start () 
    {
        m_LastPos = transform.position;
	}


    void FixedUpdate()
    {
        bool isLocalValue = false;
        Vector3 localValueInWorld = transform.TransformDirection(Value.normalized) * Value.magnitude;

        switch (Mode)
        {
            case EMode.Const_Vel_Local:
                {
                    isLocalValue = true;
                    rigidbody.velocity = localValueInWorld;
                }
                break;
            case EMode.Const_Vel_World:
                {
                    rigidbody.velocity = Value;
                }
                break;
            case EMode.Const_Acc_Local:
                {
                    isLocalValue = true;
                    rigidbody.AddRelativeForce(Value, ForceMode.Acceleration);
                }
                break;
            case EMode.Const_Acc_World:
                {
                    rigidbody.AddForce(Value, ForceMode.Acceleration);
                }
                break;
            default:
                break;
        }

        if (FacingMoveDir)
        {
            rigidbody.rotation = Quaternion.LookRotation(isLocalValue ? localValueInWorld.normalized : Value.normalized);
        }
    }
	
	// Update is called once per frame
	void Update () 
    {
       
        Vector3 vecPosChange = transform.position - m_LastPos;
        float posChange = vecPosChange.magnitude;
        m_AvgPosChangeInOneFrame = m_AvgPosChangeInOneFrame * 0.8f + 0.2f * posChange;
        if (Mode != EMode.None)
        {
            if (m_AvgPosChangeInOneFrame < 0.0001f)
            {
                Value *= -1.0f;
            }
        }


        m_LastPos = transform.position;
	}
}

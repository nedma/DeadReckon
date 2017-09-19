using UnityEngine;
using System.Collections;



public class DRComponent : MonoBehaviour 
{
    Recorder m_Recorder;
    public Recorder Recorder
    {
        get { return m_Recorder; }
    }

    DRHelper helper = new DRHelper();
    bool mUpdated = false;

    public bool bUsePhys = true;
    public bool bClampToGround = false;

    public bool bSimulate = true;


    public float ExtrapolationTime = 0.3f;


	// Use this for initialization
	void Start () 
    {
        helper.Init(this);

        m_Recorder = GetComponent<Recorder>();
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (bSimulate)
            TickRemote();
	}


    public void SetLastKnownTranslation(Vector3 pos)
    {
        helper.SetLastKnownTranslation(pos);
        mUpdated = true;

        m_Recorder.SetSnapshot(pos);
    }
    public void SetLastKnownRotation(Quaternion rot)
    {
        helper.SetLastKnownRotation(rot);
        mUpdated = true;
    }

    public void SetLastKnownVelocity(Vector3 vec)
    {
      helper.SetLastKnownVelocity(vec);
      mUpdated = true;
    }

    public void SetLastKnownAcceleration(Vector3 vec)
    {
        helper.SetLastKnownAcceleration(vec);
        mUpdated = true;
    }

    public void SetLastKnownAngularVelocity(Vector3 vec)
    {
        helper.SetLastKnownAngularVelocity(vec);
        mUpdated = true;
    }


    void TickRemote()
    {
        XTransform xform = new XTransform();
        xform.Pos = helper.CurrentDeadReckonedValue;
        xform.Rot = helper.CurrentDeadReckonedRotation;

        float simTimeDelta = Time.deltaTime;
        float simTime = Time.time;

        if (helper.IsUpdated())
        {
            //helper.SetLastTranslationUpdatedTime(simTime);
            helper.SetTranslationElapsedTimeSinceUpdate(0.0f);

            //helper.SetLastRotationUpdatedTime(simTime - simTimeDelta);
            helper.SetRotationElapsedTimeSinceUpdate(0.0f);
            helper.RotationResolved = false;
        }



         //We want to do this every time. make sure it's greater than 0 in case of time being set.
         float transElapsedTime = helper.GetTranslationElapsedTimeSinceUpdate() + simTimeDelta;
         if (transElapsedTime < 0.0) 
             transElapsedTime = 0.0f;
         helper.SetTranslationElapsedTimeSinceUpdate(transElapsedTime);
         float rotElapsedTime = helper.GetRotationElapsedTimeSinceUpdate() + simTimeDelta;
         if (rotElapsedTime < 0.0) 
             rotElapsedTime = 0.0f;
         helper.SetRotationElapsedTimeSinceUpdate(rotElapsedTime);


         // Actual dead reckoning code moved into the helper..
//         BaseGroundClamper::GroundClampRangeType* groundClampingType = &BaseGroundClamper::GroundClampRangeType::NONE;

         bool transformChanged = helper.DoDR(ref xform);

//          if (helper.GetDeadReckoningAlgorithm() != DeadReckoningAlgorithm::NONE)
//          {
//             // Only ground clamp and move remote objects.
//             if (helper.GetEffectiveUpdateMode(gameActor.IsRemote())
//                   == DeadReckoningHelper::UpdateMode::CALCULATE_AND_MOVE_ACTOR)
//             {
//                osg::Vec3 velocity(helper.GetCurrentInstantVelocity()); //  helper.GetLastKnownVelocity() + helper.GetLastKnownAcceleration() * simTimeDelta );
// 
//                // Call the ground clamper for the current object. The ground clamper should 
//                // be smart enough to know what to do with the supplied values.
//                mGroundClamper->ClampToGround(*groundClampingType, tickMessage.GetSimulationTime(),
//                         xform, gameActor.GetGameActorProxy(),
//                         helper.GetGroundClampingData(), transformChanged, velocity);
//             }
//          }

         if (transformChanged)
         {
             if (bUsePhys)
             {
                 Vector3 deltaMove = xform.Pos - rigidbody.position;
                 float t = Mathf.Max(Time.deltaTime, 0.01f);
                 rigidbody.velocity = deltaMove / t;


                 rigidbody.MoveRotation(xform.Rot);

                 m_Recorder.SetPath(xform.Pos);
             }      
             else
             {
                 transform.position = xform.Pos;
                 if (bClampToGround)
                     ClampToGround();

                 transform.rotation = xform.Rot;

                 m_Recorder.SetPath(transform.position);
             }
         }
         else
         {
             m_Recorder.SetPath(transform.position);
         }

         // Clear the updated flag.
         helper.ClearUpdated();
    }


    const float groundHeight = 0.48f;
    void ClampToGround()
    {
        
        if (transform.position.y < groundHeight)
        {
            transform.position = new Vector3(transform.position.x, groundHeight, transform.position.z);
        }

    }

}

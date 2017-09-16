using UnityEngine;
using System.Collections;


public struct XTransform
{
    public Vector3 Pos;
    public Quaternion Rot;
}

public class DRComponent : MonoBehaviour 
{
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
    }
//     public void SetLastKnownRotation(Vector3 pos)
//     {
    //         helper.SetLastKnownRotation(pos);
//         mUpdated = true;
//     }

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



    void TickRemote()
    {
        XTransform xform = new XTransform();
        xform.Pos = helper.CurrentDeadReckonedValue;
        //xform.Rot = helper.CurrentDeadReckonedRot;

        float simTimeDelta = Time.deltaTime;
        float simTime = Time.time;

        if (helper.IsUpdated())
        {
            helper.SetLastTranslationUpdatedTime(Time.time);
            helper.SetTranslationElapsedTimeSinceUpdate(0.0f);
        }



         //We want to do this every time. make sure it's greater than 0 in case of time being set.
         float transElapsedTime = helper.GetTranslationElapsedTimeSinceUpdate() + simTimeDelta;
         if (transElapsedTime < 0.0) 
             transElapsedTime = 0.0f;
         helper.SetTranslationElapsedTimeSinceUpdate(transElapsedTime);
//          float rotElapsedTime = helper.GetRotationElapsedTimeSinceUpdate() + simTimeDelta;
//          if (rotElapsedTime < 0.0) rotElapsedTime = 0.0f;
//          helper.SetRotationElapsedTimeSinceUpdate(rotElapsedTime);


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
             }      
             else
             {
                 transform.position = xform.Pos;
                 if (bClampToGround)
                     ClampToGround();
             }
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

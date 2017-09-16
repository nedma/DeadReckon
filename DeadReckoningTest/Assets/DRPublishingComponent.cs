using UnityEngine;
using System.Collections;


public class DRPublishingComponent : MonoBehaviour
{
    public float PublishingIntervalSec = 0.3f;

    float mTimeUntilNextFullUpdate;
    float mSecsSinceLastUpdateSent;


    float mVelocityAverageFrameCount = 1;

    Vector3 mCurrentVelocity;
    Vector3 mCurrentAcceleration;
    Vector3 mCurrentAngularVelocity;

    Vector3 mLastPos;
    Vector3 mAccumulatedLinearVelocity;
    Vector3 mAccumulatedAcceleration;
    Vector3 mAccelerationCalculatedForLastPublish;

    float mPrevFrameDeltaTime;



    StateMsg m_LastStateSended;



    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        OnTickRemote();
    }



    void OnTickRemote()
    {
        ComputeCurrentVelocity();



        float elapsedTime = Time.deltaTime;

        mTimeUntilNextFullUpdate -= elapsedTime;
        mSecsSinceLastUpdateSent += elapsedTime;


        //ComputeCurrentVelocity(elapsedTime, transform.position);

        bool doUpdate = false;
        if (mTimeUntilNextFullUpdate <= 0.0f)
        {
            doUpdate = true;
        }


        if (doUpdate)
        {
            SetLastKnownValuesBeforePublish(transform.position, transform.rotation);

            mTimeUntilNextFullUpdate = PublishingIntervalSec;

            MessageDispather.Instance.SendMessage(m_LastStateSended);
        }
    }


    void SetLastKnownValuesBeforePublish(Vector3 pos, Quaternion rot)
    {

         m_LastStateSended.Pos = pos;
         //GetDeadReckoningHelper().SetLastKnownRotation(rot);


         // Linear Velocity & acceleration - push the current value to the Last Known
         if (true/*mPublishLinearVelocity*/)
         {
             // VELOCITY 
             //Vector3 velocity = rigidbody.velocity;
             Vector3 velocity = mCurrentVelocity;
             if (velocity.magnitude < 0.0001) // If close to 0, set to 0 to prevent wiggling/shaking
             {
                 velocity = Vector3.zero;
             }
             m_LastStateSended.Vel = velocity;


             // ACCELERATION

             /// DAMPEN THE ACCELERATION TO PREVENT WILD SWINGS WITH DEAD RECKONING
             // Dampen the acceleration before publication if the vehicle is making drastic changes 
             // in direction. With drastic changes, the acceleration will cause the Dead Reckoned 
             // pos to oscillate wildly. Whereas it will improve DR on smooth curves such as a circle.
             // The math is: take the current accel and the non-scaled accel from the last publish;
             // normalize them; dot them and use the product to scale our current Acceleration. 
             Vector3 curAccel = mCurrentAcceleration;
             curAccel.Normalize();
             float accelDotProduct = Vector3.Dot(curAccel, mAccelerationCalculatedForLastPublish); // dot product
             mCurrentAcceleration = mCurrentAcceleration * Mathf.Max(0.0f, accelDotProduct);
             mAccelerationCalculatedForLastPublish = curAccel; // Hold for next time (pre-normalized)

             // Acceleration is paired with velocity
             m_LastStateSended.Acc = mCurrentAcceleration;
         }

         // Angular Velocity - push the current value to the Last Known
//          if (mPublishAngularVelocity)
//          {
//             Vector3 angularVelocity = GetCurrentAngularVelocity();
//             if (angularVelocity.length() < 0.001)  // If close to 0, set to 0 to prevent wiggling/shaking
//             {
//                angularVelocity = osg::Vec3(0.f, 0.f, 0.f);
//             }
//             GetDeadReckoningHelper().SetLastKnownAngularVelocity(angularVelocity);
//          }

    }



    void FixedUpdate()
    {
        //ComputeCurrentVelocity();


    }

    void ComputeCurrentVelocity()
    {
        mCurrentVelocity = rigidbody.velocity;
        Vector3 changeInVelocity = rigidbody.velocity - mAccumulatedLinearVelocity;
        mCurrentAcceleration = changeInVelocity / Time.deltaTime;


        mAccumulatedLinearVelocity = rigidbody.velocity;
    }

    void ComputeCurrentVelocity_Deprecated(float deltaTime, Vector3 pos)
    {
        if (mPrevFrameDeltaTime > 0.0f && mLastPos.sqrMagnitude > 0.0) // ignore first time.
        {
               // Note - we used to grab the velocity from the physics engines, but there were sometimes 
               // discontinuities reported by the various engines, so that was removed in favor of a simple
               // differential of position. 
               Vector3 distanceMoved = pos - mLastPos;
               Vector3 instantVelocity = distanceMoved / mPrevFrameDeltaTime;

               Vector3 previousAccumulatedLinearVelocity = mAccumulatedLinearVelocity;

               // Compute Vel - either the instant Vel or a blended value over a couple of frames. Blended Velocities tend to make acceleration less useful
               if (mVelocityAverageFrameCount == 1)
               {
                  mAccumulatedLinearVelocity = instantVelocity;
               }
               else 
               {
                  float instantVelWeight = 1.0f / mVelocityAverageFrameCount;
                  mAccumulatedLinearVelocity = instantVelocity * instantVelWeight + mAccumulatedLinearVelocity * (1.0f - instantVelWeight);
               }

               // Sometimes, the physics engines will oscillate when they are moving extremely slowly (or 'sitting still')
               // Calc'ing the vel/accel will magnify this effect, so we clamp it.
               float velMagnitude2 = mAccumulatedLinearVelocity.sqrMagnitude;
               if (velMagnitude2 < 0.01f)
               {
                  mAccumulatedLinearVelocity = Vector3.zero;
               }

               // Compute our acceleration as the instantaneous differential of the velocity
               // Acceleration is dampened before publication - see SetLastKnownValuesBeforePublish().
               // Note - if you know your REAL acceleration due to vehicle dynamics, override the method
               // and make your own call to SetCurrentAcceleration().
               Vector3 changeInVelocity = mAccumulatedLinearVelocity - previousAccumulatedLinearVelocity; /*instantVelocity - mAccumulatedLinearVelocity*/;
               mAccumulatedAcceleration = changeInVelocity / mPrevFrameDeltaTime;

               // Many vehicles get a slight jitter up/down while running. If you allow the z acceleration to 
               // be published, the vehicle will go all over the place nutty. So, we zero it out. 
               // This is not an ideal solution, but is workable because vehicles that really do have a lot of
               // z acceleration are probably flying and by definition are not as close to other objects so the z accel
               // is less visually apparent.
               //mAccumulatedAcceleration.z = 0.0f; 

               mCurrentAcceleration = mAccumulatedAcceleration;
               mCurrentVelocity = mAccumulatedLinearVelocity;
        }


        mLastPos = pos; 
        mPrevFrameDeltaTime = deltaTime; // The passed in Delta is actually the time for the next computation
    }
}

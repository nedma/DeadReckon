using UnityEngine;
using System.Collections;

public class DRHelper
{
    DRComponent m_Owner;

    public void Init(DRComponent owner)
    {
        m_Owner = owner;
    }

    ///the simulation time this was last updated.
    //double mLastTranslationUpdatedTime;
    float mLastUpdatedTime;
    //float mAverageTimeBetweenTranslationUpdates;
    float mAvgTimeBetweenUpdates;

    ///The maximum amount of time to use when smoothing translation.
    //float mMaxTranslationSmoothingTime;
    float mMaxSmoothingTime;

    ///the amount of time since this actor started smoothing. --->Tt
    float mElapsedTimeSinceUpdate;

    ///the end amount of time to use when smoothing the translation.  At this point, the blend should be finished.
    //float mTranslationEndSmoothingTime;
    float mEndSmoothingTime;

    ///Last known position of this actor.
    //Vector3 mLastTranslation;
    Vector3 mLastValue;

    ///The Dead-Reckoned position prior to the last update.
    //Vector3 mTransBeforeLastUpdate;
    Vector3 mValueBeforeLastUpdate;

    // Current Dead Reckoned Position
    //Vector3 mCurrentDeadReckonedTranslation;
    Vector3 mCurrentDeadReckonedValue;
    public Vector3 CurrentDeadReckonedValue
    {
        get { return mCurrentDeadReckonedValue; }
    }

    //Vector3 mLastVelocity;
    Vector3 mLastVelocity;
    //Vector3 mVelocityBeforeLastUpdate; /// The velocity we were using just before we got an update
    Vector3 mVelocityBeforeLastUpdate; /// The velocity we were using just before we got an update
    Vector3 mPreviousInstantVel;
    //Vector3 mAccelerationVector;
    Vector3 mAcceleration;


    //bool mTranslationInitiated;
    bool mInitialized = false;
    //bool mTranslationUpdated;
    bool mUpdated = false;


    public Vector3 mAngularVelocityVector;
    bool mRotationInitiated = false;
    bool mRotationUpdated = false;
    // if the rotation has been resolved to the last updated version.
    bool mRotationResolved = true;
    public bool RotationResolved
    {
        set { mRotationResolved = value; }
    }


    ///the amount of time since this actor started smoothing.
    float mRotationElapsedTimeSinceUpdate;

    ///the end amount of time to use when smoothing the rotation.  At this point, the blend should be finished.
    float mRotationEndSmoothingTime;

    ///last known orientation (vector)
    //Vector3 mLastRotation;
    ///last known orientation (matrix)
    //Matrix mLastRotationMatrix;
    ///last known orientation (quaternion)
    Quaternion mLastQuatRotation;
    ///dead reckoned attitude quaternion prior update
    Quaternion mRotQuatBeforeLastUpdate;
    ///current dead reckoned attitude quaternion
    Quaternion mCurrentDeadReckonedRotation;
    public Quaternion CurrentDeadReckonedRotation
    {
        get { return mCurrentDeadReckonedRotation; }
    }


    float mCurTimeDelta = 0.0f; // Tracks how long this process step is for. Used to compute instant vel.
    const float mFixedSmoothingTime = 1.0f;



    public bool useAcceleration = true;


    public bool DoDR(ref XTransform trans)
    {
        bool returnValue = false; // indicates we changed the transform
        returnValue = DRVelocityAcceleration(ref trans);
        return returnValue;
    }


    bool DRVelocityAcceleration(ref XTransform xform)
    {
        bool returnValue = false;

        Vector3 pos = xform.Pos;
        Vector3 unclampedTranslation = pos;



        if (IsUpdated() ||
            mLastValue != unclampedTranslation ||
            !mRotationResolved ||
            mLastVelocity.sqrMagnitude > 1e-2f ||
            mAcceleration.sqrMagnitude > 1e-2f ||
            mAngularVelocityVector.sqrMagnitude  > 1e-5f
            )
        {
            // if we got an update, then we need to recalculate our smoothing
            if (IsUpdated())
            {
                CalculateSmoothingTimes(xform);
            }

            // RESOLVE ROTATION
            Quaternion drRot = Quaternion.identity;
            if (DeadReckonTheRotation(xform, ref drRot))
            {
                xform.Rot = drRot;
            }

            // POSITION SMOOTHING
            Vector3 drPos = DeadReckonThePosition(xform.Pos, useAcceleration, mCurTimeDelta);

            xform.Pos = drPos;
            returnValue = true;
        }
        else
        {
            mPreviousInstantVel.Set(0.0f, 0.0f, 0.0f);

            returnValue = false;
        }

        return returnValue;
    }





    public void SetLastKnownTranslation(Vector3 vec)
    {
        if (mInitialized)
        {
            mValueBeforeLastUpdate = mCurrentDeadReckonedValue;
        }
        else
        {
            mValueBeforeLastUpdate = vec;
        }

        mInitialized = true;
        mLastValue = vec;
        mElapsedTimeSinceUpdate = 0.0f;

        mUpdated = true;
    }


    public void SetLastKnownVelocity(Vector3 vec)
    {
        mVelocityBeforeLastUpdate = mPreviousInstantVel;

        mLastVelocity = vec;
        mElapsedTimeSinceUpdate = 0.0f;

        mUpdated = true;
    }

    public void SetLastKnownRotation(Quaternion rot)
    {
        if (mRotationInitiated)
        {
            mRotQuatBeforeLastUpdate = mCurrentDeadReckonedRotation;
        }
        else
        {
            //mLastRotationMatrix.get(mRotQuatBeforeLastUpdate);
            mRotQuatBeforeLastUpdate = rot;
        }

        mLastQuatRotation = rot;
        mRotationElapsedTimeSinceUpdate = 0.0f;
        mRotationInitiated = true;
        mRotationUpdated = true;


        mUpdated = true;
    }

    public void SetLastKnownAcceleration(Vector3 vec)
    {
        mAcceleration = vec;
        mUpdated = true;
    }

    Vector3 DeadReckonThePosition(Vector3 curPos, bool useAcceleration, float curTimeDelta)
    {
        Vector3 pos = Vector3.zero;

        bool pastTheSmoothingTime = (mEndSmoothingTime <= 0.0f) || (mElapsedTimeSinceUpdate >= mEndSmoothingTime);

        if (pastTheSmoothingTime)
        {
            Vector3 accelerationEffect = Vector3.zero;
            if (useAcceleration)
            {
                accelerationEffect = (mAcceleration * 0.5f) * (mElapsedTimeSinceUpdate * mElapsedTimeSinceUpdate);
            }
            pos = mLastValue + mLastVelocity * mElapsedTimeSinceUpdate + accelerationEffect;

            m_Owner.Recorder.SetPrediction(pos);
        }
        else
        {
            pos = DeadReckonUsingLinearBlend(curPos, useAcceleration);
        }


        if (curTimeDelta > 0.0f) // if delta <= 0 then just use prev values
        {
            Vector3 instantVel = (pos - mCurrentDeadReckonedValue) / curTimeDelta;
            mPreviousInstantVel = instantVel;
        }

        mCurrentDeadReckonedValue = pos;


        return pos;
    }


    bool DeadReckonTheRotation(XTransform xform, ref Quaternion newRot)
    {
        Quaternion drQuat = mLastQuatRotation; // velocity only just uses the last.
        bool isRotationChangedByAccel = false;
        Quaternion startRotation = mRotQuatBeforeLastUpdate;

        if (!mRotationResolved) // mRotationResolved is never set when using Accel
        {

            // For vel and Accel, we use the angular velocity to compute a dead reckoning matrix to slerp to
            // assuming that we have an angular velocity at all...
            if (mAngularVelocityVector.sqrMagnitude > 1e-6)
            {
                // if we're here, we had some sort of change, however small
                isRotationChangedByAccel = true;

                float actualRotationTime = Mathf.Min(mRotationElapsedTimeSinceUpdate, mRotationEndSmoothingTime);

                Vector3 angVelAxis = mAngularVelocityVector.normalized;
                float angVelMag = mAngularVelocityVector.magnitude;

                // rotation around the axis is magnitude of ang vel * time.
                float rotationAngle = angVelMag * actualRotationTime;
                Quaternion rotationFromAngVel = Quaternion.AngleAxis(rotationAngle, angVelAxis);

                // Expected DR'ed rotation - Take the last rot and add the change over time
                drQuat = rotationFromAngVel * mLastQuatRotation;
                // Previous DR'ed rotation - same, but uses where we were before the last update, so we can smooth it out...
                startRotation = rotationFromAngVel * mRotQuatBeforeLastUpdate;
            }

            // If there is a difference in the rotations and we still have time to smooth, then
            // slerp between the two quats: 
            //     1) the old rotation plus the expected change using angular velocity and 
            //     2) the desired new rotation
            if ((mRotationEndSmoothingTime > 0.0f) && (mRotationElapsedTimeSinceUpdate < mRotationEndSmoothingTime))
            {
                float smoothingFactor = mRotationElapsedTimeSinceUpdate / mRotationEndSmoothingTime;
                smoothingFactor = Mathf.Clamp01(smoothingFactor);
                newRot = Quaternion.Slerp(startRotation, drQuat, smoothingFactor);
            }
            else // Either smoothing time is done or the current rotation equals the desired rotation
            {
                newRot = drQuat;
                mRotationResolved = !isRotationChangedByAccel; //true;
            }

            mCurrentDeadReckonedRotation = newRot;

            return true;
        }
        else
        {
            return false;
        }
    }


    Vector3 DeadReckonUsingLinearBlend(Vector3 curPos, bool useAcceleration)
    {
        Vector3 pos = Vector3.zero;

        // ---->T^
        float smoothingFactor = mElapsedTimeSinceUpdate / mEndSmoothingTime;

        // ---->Pt'
        Vector3 lastKnownPosChange = mLastValue + mLastVelocity * mElapsedTimeSinceUpdate; // Add Accel later. 

        // COMPUTE BLENDED VELOCITY - Lerp the two velocities and use that for movement. 
        // This majorly reduces the oscillations. 
        // ---->Vb = V0 + (V0' - V0) * T^
        Vector3 mBlendedVelocity = mVelocityBeforeLastUpdate +
           (mLastVelocity - mVelocityBeforeLastUpdate) * smoothingFactor;
        Vector3 velBlendedPos = mValueBeforeLastUpdate + mBlendedVelocity * mElapsedTimeSinceUpdate;// Add Accel later;

        // BLEND THE TWO - lerp between the last known and blended velocity 
        // ---->Qt = Pt + (Pt' - Pt) * T^
        pos = velBlendedPos + (lastKnownPosChange - velBlendedPos) * smoothingFactor;

        // ADD ACCEL - do at end because it applies to both projections anyway.
        Vector3 accelerationEffect = Vector3.zero;
        if (useAcceleration)
        {
            accelerationEffect = ((mAcceleration * 0.5f) * (mElapsedTimeSinceUpdate * mElapsedTimeSinceUpdate));
        }

        pos += accelerationEffect;

        m_Owner.Recorder.SetPrediction(lastKnownPosChange + accelerationEffect);

        return pos;
    }


    void CalculateSmoothingTimes(XTransform xform)
    {
        // Dev Note - When the blend time changes drastically (ex 0.1 one time then 
        // 1.0 the next), it can inject significant issues when DR'ing - whether with 
        // catmull-rom/Bezier splines or linear. Recommended use case is for mAlwaysUseMaxSmoothingTime = true

        // ROTATION
        //       if (mUseFixedSmoothingTime)
        //       {
        mRotationEndSmoothingTime = m_Owner.ExtrapolationTime;
        //       }
        //       else 
        //       {
        //          mRotationEndSmoothingTime = GetMaxRotationSmoothingTime();
        //          // Use our avg update time if it's smaller than our max
        //          if (GetMaxRotationSmoothingTime() > mAverageTimeBetweenRotationUpdates)
        //             mRotationEndSmoothingTime = mAverageTimeBetweenRotationUpdates;
        // 
        //          // Way-Out-Of-Bounds check For angular acceleration. 
        //          if (GetDeadReckoningAlgorithm() == DeadReckoningAlgorithm::VELOCITY_AND_ACCELERATION &&
        //             (mAngularVelocityVector.length2() * (mRotationEndSmoothingTime * mRotationEndSmoothingTime)) <
        //             0.1 * ((mLastQuatRotation-mCurrentDeadReckonedRotation).length2()))
        //          {
        //             mRotationEndSmoothingTime = std::min(1.0f, mRotationEndSmoothingTime);
        //             //mRotationEndSmoothingTime = std::min(1.0f, mAverageTimeBetweenRotationUpdates);
        //          }
        //       }

        // TRANSLATION
        mEndSmoothingTime = /*mFixedSmoothingTime*/m_Owner.ExtrapolationTime;

        //       if (mUseFixedSmoothingTime)
        //       {
        //          mTranslation.mEndSmoothingTime = GetFixedSmoothingTime();
        //       }
        //       else 
        //       {
        //          mTranslation.mEndSmoothingTime = GetMaxTranslationSmoothingTime();
        //          // Use our avg update time if it's smaller than our max
        //          if (GetMaxTranslationSmoothingTime() > mTranslation.mAvgTimeBetweenUpdates)
        //          {
        //             mTranslation.mEndSmoothingTime = mTranslation.mAvgTimeBetweenUpdates;
        //          }
        // 
        //          osg::Vec3 pos;
        //          xform.GetTranslation(pos);
        // 
        //          //Order of magnitude check - if the entity could not possibly get to the new position
        //          // in max smoothing time based on the magnitude of it's velocity, then smooth quicker (ie 1 second).
        //          if (mTranslation.mLastVelocity.length2() * (mTranslation.mEndSmoothingTime*mTranslation.mEndSmoothingTime) 
        //             < (mTranslation.mLastValue - pos).length2() )
        //          {
        //             mTranslation.mEndSmoothingTime = std::min(1.0f, mTranslation.mEndSmoothingTime);
        //          }
        //       }
    }




    //////////////////////////////////////////////////////////////////////
//     public void SetLastTranslationUpdatedTime(float newUpdatedTime)
//     {
//         //the average of the last average and the current time since an update.
//         float timeDelta = newUpdatedTime - mLastUpdatedTime;
//         mAvgTimeBetweenUpdates = 0.5f * timeDelta + 0.5f * mAvgTimeBetweenUpdates;
//         mLastUpdatedTime = newUpdatedTime;
//     }


   //////////////////////////////////////////////////////////////////////
//    void SetLastRotationUpdatedTime(double newUpdatedTime)
//    {
//       //the average of the last average and the current time since an update.
//       float timeDelta = float(newUpdatedTime - mLastRotationUpdatedTime);
//       mAverageTimeBetweenRotationUpdates = 0.5f * timeDelta + 0.5f * mAverageTimeBetweenRotationUpdates;
//       mLastRotationUpdatedTime = newUpdatedTime;
//    }

   //////////////////////////////////////////////////////////////////////
    public void SetTranslationElapsedTimeSinceUpdate(float value)
    {
        // Compute time delta for this step of DR. Should be the same as DeltaTime in the component
        mCurTimeDelta = Mathf.Max(value - mElapsedTimeSinceUpdate, 0.0f);
        mElapsedTimeSinceUpdate = value;
    }
    public float GetTranslationElapsedTimeSinceUpdate()
    { 
        return mElapsedTimeSinceUpdate; 
    }

    public void SetRotationElapsedTimeSinceUpdate(float value)
    { 
        mRotationElapsedTimeSinceUpdate = value; 
    }
    public float GetRotationElapsedTimeSinceUpdate()
    { 
        return mRotationElapsedTimeSinceUpdate; 
    }


   public void ClearUpdated() 
   {
       mUpdated = false; 
       mRotationUpdated = false; 
   }

    public bool IsUpdated()
    { 
        return mUpdated; 
    }
}

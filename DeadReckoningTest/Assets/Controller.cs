using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour 
{
    public float AccelerationMag = 0.1f;
    public float JumpAccelerationMag = 4.0f;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
        float forward = Input.GetAxis("Vertical");
        float side = Input.GetAxis("Horizontal");

        //Debug.Log("forward=" + forward + ", side=" + side);

        Vector3 acc = Vector3.forward * forward;
        acc += Vector3.right * side;
        acc *= AccelerationMag;

        if (Mathf.Abs(rigidbody.velocity.y) < 1.0f)
        {
            bool jump = Input.GetButtonUp("Jump");
            if (jump)
                acc += Vector3.up * JumpAccelerationMag;
        }



        //Debug.DrawLine(transform.position, transform.position + acc);

        rigidbody.velocity += acc;
        

	}
}

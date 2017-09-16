using UnityEngine;
using System.Collections;

public class CarCamera : MonoBehaviour
{
	public Transform target = null;

    public float InitHeight;
    public Vector3 relativePos;
	
	void Start()
	{
        InitHeight = transform.position.y;
        relativePos = transform.position - target.position;
	}
	
	void LateUpdate()
	{
        Vector3 pos = target.position + relativePos;
        pos.y = InitHeight;
        transform.position = pos;
        transform.LookAt(target);
		
	}
}

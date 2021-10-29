using UnityEngine;
using System.Collections;

public class SphereScript : MonoBehaviour
{
	static Vector3 rot_axis = new Vector3(1f, 1f, 1f);
	Transform tran;
	
	void Awake()
	{
		tran = GetComponent<Transform>();	
	}
	
	void FixedUpdate()
	{
		tran.RotateAround(tran.position, rot_axis, 1.0f);
	}
}
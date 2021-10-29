using UnityEngine;
using System.Collections;

public class MapIcon3 : MonoBehaviour
{
	static Vector3 rot_axis = new Vector3(0f, -1f, 0f);
	Transform tran;
	
	void Awake()
	{
		tran = GetComponent<Transform>();
	}
	
	void FixedUpdate()
	{
		tran.RotateAround(tran.position, rot_axis, 2.0f);
	}
}
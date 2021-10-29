using UnityEngine;
using System.Collections;

public class Billboard : MonoBehaviour
{
	Transform tran;
	
	void Awake()
	{
		tran = GetComponent<Transform>();	
	}
	
	void FixedUpdate()
	{
		tran.rotation = Quaternion.Euler(0f, MouseLook.inst.cam.eulerAngles.y, 0f);
	}
}
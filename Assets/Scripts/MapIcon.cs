using UnityEngine;
using System.Collections;

public class MapIcon : MonoBehaviour
{
	public Transform target;
	public Vector3 offs;
	Transform tran;
	
	void Awake()
	{
		tran = GetComponent<Transform>();
	}
	
	void Update()
	{
		tran.position = target.position + offs;
		tran.rotation = Quaternion.Euler(90, target.eulerAngles.y, 0);
	}
}
using UnityEngine;
using System.Collections;

public class MapIcon2 : MonoBehaviour
{
	public Transform target;
	Transform tran;
	
	void Awake()
	{
		tran = GetComponent<Transform>();
	}
	
	void Update()
	{
		tran.position = target.position;
		tran.rotation = Quaternion.Euler(90, target.eulerAngles.y, 0);
	}
}
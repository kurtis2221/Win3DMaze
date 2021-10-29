using UnityEngine;
using System.Collections;

public class MouseLook : MonoBehaviour
{
	const float min_y = -90, max_y = 90;
	
	public static MouseLook inst;
	public Transform cam;
	
	Transform tran;
	float mousesens;
	float mouse_y;
	
	void Awake()
	{
		inst = this;
		mousesens = MenuScript.mouse;
		tran = GetComponent<Transform>();
	}
	
	void Update()
	{
		float ver = Input.GetAxis("Mouse Y");
		float hor = Input.GetAxis("Mouse X");
		mouse_y += ver * mousesens;
		mouse_y = Mathf.Clamp(mouse_y, min_y, max_y);
		cam.localRotation = Quaternion.Euler(-mouse_y, 0, 0);
		tran.Rotate(0, hor * mousesens, 0);
	}
}
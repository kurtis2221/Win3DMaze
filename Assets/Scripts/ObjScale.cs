using UnityEngine;
using System.Collections;

public class ObjScale : MonoBehaviour
{
	public static ObjScale inst;
	
	const float SPEED = 0.02f;
	
	Vector3 scale = new Vector3(1, 0, 1);
	Vector3 scale_spd = new Vector3(0, SPEED, 0);
	Vector3 pos = new Vector3(0, -2, 0);
	Vector3 pos_spd = new Vector3(0, SPEED * 2f, 0);
	float dir;
	System.Action hndl;
	
	void Awake()
	{
		inst = this;
	}
	
	void FixedUpdate()
	{
		scale += scale_spd * dir;
		pos += pos_spd * dir;
		SetObjs();
		if(dir == 1f && scale.y >= 1f)
		{
			scale.y = 1f;
			pos.y = 0f;
			dir = -1f;
			SetObjs();
			enabled = false;
			if(hndl != null) hndl();
		}
		else if(dir == -1f && scale.y <= 0f)
		{
			//Only this direction needs this, scale is copied from parent
			scale.y = 1f;
			pos.y = 0f;
			dir = -1f;
			SetObjs();
			enabled = false;
			//Reset values to init state
			scale.y = 0f;
			pos.y = -2f;
			dir = 1f;
			if(hndl != null) hndl();
		}
	}
	
	void SetObjs()
	{
		GameControl.inst.walls.localScale = scale;
		GameControl.inst.walls.position = pos;
		GameControl.inst.sprites.position = pos;
		foreach(Transform s in GameControl.inst.sprite_list)
			s.localScale = scale;
	}
	
	public void Scale(float input, System.Action hndl)
	{
		dir = input;
		enabled = true;
		this.hndl = hndl;
	}
}
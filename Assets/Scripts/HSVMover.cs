using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HSVMover : MonoBehaviour
{
	public Shader hsv;
	Vector4 val;
	List<Material> mats;
	
	void Awake()
	{
		mats = new List<Material>();
		val = new Vector4(0, 0, 0, 0);
		MazeSetup setup = AssetLoader.setups[MenuScript.curr_setup];
		string[] tmplist = {setup.wall, setup.wall2, setup.floor, setup.ceil };
		foreach(string t in tmplist)
		{
			Material tmp = AssetLoader.materials[t];
			if(tmp.shader == hsv) mats.Add(tmp);
		}
		if(mats.Count > 0) enabled = true;
	}
	
	void FixedUpdate()
	{
		foreach(Material m in mats)
		{
			m.SetVector("_HSVAAdjust", val);
			val.x += 0.1f * Time.fixedDeltaTime;
			if(val.x >= 1) val.x = 0;
		}
	}
}
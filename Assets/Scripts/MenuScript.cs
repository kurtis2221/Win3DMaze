using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class MenuScript : MonoBehaviour
{
	const string CFG_FILE = "Win3DMaze.ini";
	
	public static MenuScript inst;
	
	//Preview
	public Camera maincam;
	public Renderer floor;
	public Renderer ceil;
	public Renderer[] walls;
	public Renderer wall2;
	
	//Setup handling
	public TextMesh setup_txt;
	MazeSetup setup;
	static List<string> setup_names;
	static int setup_idx = 0;
	public static string curr_setup;
	
	//Control
	public TextMesh ctrl_txt;
	public static ControlType ctrl = ControlType.Modern;
	
	//Mouse sens.
	public TextMesh mouse_txt;
	public static float mouse = 5;
	
	//FOV
	public TextMesh fov_txt;
	public static float fov = 60f;
	
	void Awake()
	{
		inst = this;
		Screen.lockCursor = false;
		Cursor.visible = true;
		if(setup_names == null)
		{
			setup_names = new List<string>(AssetLoader.setups.Keys);
			LoadCFG();
		}
		SetSetup();
		SetCtrl();
		SetMouse();
		SetFOV();
	}
	
	public void ChangeSetup(int input)
	{
		setup_idx += input;
		if(setup_idx < 0) setup_idx = setup_names.Count - 1;
		else if(setup_idx >= setup_names.Count) setup_idx = 0;
		SetSetup();
	}
	
	public void ChangeControl()
	{
		ctrl++;
		if(ctrl == ControlType.Invalid) ctrl = ControlType.Modern;
		SetCtrl();
	}
	
	public void ChangeMouse(int input)
	{
		mouse += input;
		if(mouse > 10) mouse = 10;
		else if(mouse < 1) mouse = 1;
		SetMouse();
	}
	
	public void ChangeFOV(int input)
	{
		fov += input;
		if(fov > 120) fov = 120;
		else if(fov < 60) fov = 60;
		SetFOV();
	}

	public void LoadSetup()
	{
		setup = AssetLoader.setups[setup_names[setup_idx]];
		if(setup.hasfloor)
		{
			floor.material = AssetLoader.materials[setup.floor];
			floor.enabled = true;
		}
		else floor.enabled = false;
		if(setup.hasceil)
		{
			ceil.material = AssetLoader.materials[setup.ceil];
			ceil.enabled = true;
		}
		else ceil.enabled = false;
		maincam.backgroundColor = setup.backg;
		if(setup.fog)
		{
			RenderSettings.fog = true;
			RenderSettings.fogColor = setup.fog_col;
			RenderSettings.fogMode = (FogMode)setup.fog_type;
			if(RenderSettings.fogMode == FogMode.Linear)
				RenderSettings.fogEndDistance = setup.fog_den;
			else RenderSettings.fogDensity = setup.fog_den;
		}
		else RenderSettings.fog = false;
		foreach(Renderer w in walls)
			w.material = AssetLoader.materials[setup.wall];
		wall2.material = AssetLoader.materials[setup.wall2];
	}
	
	void SetSetup()
	{
		curr_setup = setup_names[setup_idx];
		setup_txt.text = curr_setup;
		LoadSetup();
	}
	
	void SetCtrl()
	{
		ctrl_txt.text = ctrl.ToString();
	}
	
	void SetMouse()
	{
		mouse_txt.text = mouse.ToString("#.0");
	}
	
	void SetFOV()
	{
		fov_txt.text = fov.ToString("#.0");
		maincam.fieldOfView = fov;
	}
	
	void LoadCFG()
	{
		try
		{
			if(File.Exists(CFG_FILE))
			{
				using(StreamReader sr = new StreamReader(CFG_FILE, Encoding.Default))
				{
					curr_setup = sr.ReadLine();
					setup_idx = setup_names.IndexOf(curr_setup);
					if(setup_idx == -1) setup_idx = 0;
					ctrl = (ControlType)(int.Parse(sr.ReadLine()) % 3);
					fov = float.Parse(sr.ReadLine());
					mouse = float.Parse(sr.ReadLine());
				}
			}
		}
		catch
		{
		}
	}
	
	public void SaveCFG()
	{
		try
		{
			using(StreamWriter sw = new StreamWriter(CFG_FILE, false, Encoding.Default))
			{
				sw.WriteLine(curr_setup);
				sw.WriteLine((int)ctrl);
				sw.WriteLine(fov);
				sw.WriteLine(mouse);
			}
		}
		catch
		{
		}
	}
}
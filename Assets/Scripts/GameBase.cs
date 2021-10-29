using UnityEngine;
using System.IO;
using System.Collections;

public class GameBase
{
	public static string game_path;
	
	static GameBase()
	{
		game_path = Path.Combine(Application.dataPath, "..");
	}
	
	public static T EnumTryParse<T>(string input, T def = default(T))
	{
		try
		{
			return (T)System.Enum.Parse(typeof(T), input);
		}
		catch
		{
			return def;
		}
	}
	
	public static Color ParseColor(string input)
	{
		long col = long.Parse(input, System.Globalization.NumberStyles.HexNumber);
		float a = (col & 255) / 255f;
		col >>= 8;
		float b = (col & 255) / 255f;
		col >>= 8;
		float g = (col & 255) / 255f;
		col >>= 8;
		float r = (col & 255) / 255f;
		return new Color(r, g, b, a);
	}
	
	public static bool ParseBool(char input)
	{
		return input != '0';
	}
	
	public static IEnumerator LoadDataFromFile(string dir, string file, System.Action<WWW> hndl)
	{
		string path = Path.Combine(Path.Combine(game_path, dir), file);
		using(WWW data = new WWW("file://" + path))
        {
			yield return data;
			if(data.error == null)
			{
				AssetLoader.loaded++;
				hndl(data);
			}
			else ErrorMessage(file, data.error);
		}
	}
	
	public static void ErrorMessage(string file, System.Exception ex)
	{
		AssetLoader.serr_txt.text += "FILE: " + file + " | " + ex.Message + "\n";
	}
	
	public static void ErrorMessage(string file, string message)
	{
		AssetLoader.serr_txt.text += "FILE: " + file + " | " + message + "\n";
	}
}

public class MazeWallOffset
{
	public Vector3 pos;
	public Quaternion rot;
}

public class MazeSetup
{
	public string wall;
	public string floor;
	public string ceil;
	public string wall2;
	public Color backg;
	public Color fog_col;
	public float fog_den;
	public bool fog;
	public int fog_type;
	public bool hasfloor;
	public bool hasceil;
	public string start;
	public string finish;
	public string opengl;
	public string rat;
	public string map_player;
	public string map_start;
	public string map_finish;
	public string map_opengl;
	public string map_sphere;
	public string map_rat;
}

public class SphereObj
{
	public GameObject obj;
	public GameObject map_obj;
	public Bounds bound;
}

public enum MazeBlockWall
{
	None = 0,
	Left = 1,
	Up = 2,
	Right = 4,
	Down = 8,
	All = 15
}

public enum GameLevels
{
	Load,
	Menu,
	Game
}

public enum ControlType
{
	Modern,
	Classic,
	Original,
	Invalid
}
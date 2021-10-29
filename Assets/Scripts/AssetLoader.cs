using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class AssetLoader : MonoBehaviour
{
	const string FLD_DATA = "Data\\";
	const string FLD_TEXTURE = FLD_DATA + "Textures";
	const string FLD_SPRITE = FLD_DATA + "Sprites";
	const string FLD_MAP = FLD_DATA + "Map";
	const string FILE_SETUP = FLD_DATA + "Setups.txt";
	const string FILE_TEXTURE = FLD_TEXTURE + ".txt";
	const string FILE_SPRITE = FLD_SPRITE + ".txt";
	const string FILE_MAP = FLD_MAP + ".txt";
	
	const float MAX_WIDTH = 128f;
	const float MAX_HEIGHT = 128f;
	
	public TextMesh err_txt;
	public static TextMesh serr_txt;
	public static Dictionary<string, Material> materials;
	public static Dictionary<string, Material> sprites;
	public static Dictionary<string, Material> map_ico;
	public static Dictionary<string, MazeSetup> setups;
	
	public static int loaded;
	int loading;
	bool finished;
	
	System.Func<MazeSetup, string>[] func_mat =
	{
		x => x.wall,
		x => x.floor,
		x => x.ceil,
		x => x.wall2
	};
	
	System.Func<MazeSetup, string>[] func_spr =
	{
		x => x.start,
		x => x.finish,
		x => x.opengl,
		x => x.rat
	};
	
	System.Func<MazeSetup, string>[] func_ico =
	{
		x => x.map_player,
		x => x.map_start,
		x => x.map_finish,
		x => x.map_opengl,
		x => x.map_sphere,
		x => x.map_rat
	};
	
	class DataItem
	{
		public IEnumerable<System.Func<MazeSetup, string>> func;
		public IDictionary dict;
		public string err;
	}
	
	void Awake()
	{
		string lastfile = string.Empty;
		bool errors = false;
		try
		{
			//Loading and error handling
			finished = false;
			loaded = 0;
			loading = 0;
			serr_txt = err_txt;
			//Resources
			materials = new Dictionary<string, Material>();
			sprites = new Dictionary<string, Material>();
			map_ico = new Dictionary<string, Material>();
			//Setup definitions
			setups = new Dictionary<string, MazeSetup>();
			//Code shortening
			DataItem[] chk_arr =
			{
				new DataItem() { func = func_mat, dict = materials, err = "Missing texture: " },
				new DataItem() { func = func_spr, dict = sprites, err = "Missing sprite: " },
				new DataItem() { func = func_ico, dict = map_ico, err = "Missing map icon: " },
			};
			//Setups
			lastfile = FILE_SETUP;
			using(StreamReader sr = new StreamReader(Path.Combine(GameBase.game_path, lastfile), Encoding.Default))
			{
				while(sr.Peek() > -1)
				{
					MazeSetup setup = new MazeSetup();
					string name = sr.ReadLine();
					setup.wall = sr.ReadLine();
					setup.floor = sr.ReadLine();
					setup.ceil = sr.ReadLine();
					setup.wall2 = sr.ReadLine();
					setup.backg = GameBase.ParseColor(sr.ReadLine());
					setup.fog_col = GameBase.ParseColor(sr.ReadLine());
					setup.fog_den = float.Parse(sr.ReadLine());
					string booldata = sr.ReadLine();
					setup.fog = GameBase.ParseBool(booldata[0]);
					setup.fog_type = (int.Parse(booldata[1].ToString()) % 3) + 1;
					setup.hasfloor = GameBase.ParseBool(booldata[2]);
					setup.hasceil = GameBase.ParseBool(booldata[3]);
					setup.start = sr.ReadLine();
					setup.finish = sr.ReadLine();
					setup.opengl = sr.ReadLine();
					setup.rat = sr.ReadLine();
					setup.map_player = sr.ReadLine();
					setup.map_start = sr.ReadLine();
					setup.map_finish = sr.ReadLine();
					setup.map_opengl = sr.ReadLine();
					setup.map_sphere = sr.ReadLine();
					setup.map_rat = sr.ReadLine();
					setups.Add(name, setup);
				}
			}
			//Textures
			lastfile = FILE_TEXTURE;
			using(StreamReader sr = new StreamReader(Path.Combine(GameBase.game_path, lastfile), Encoding.Default))
			{
				while(sr.Peek() > -1)
				{
					string name = sr.ReadLine();
					string file = sr.ReadLine();
					string filter = sr.ReadLine();
					string shader = sr.ReadLine();
					//Fallback if shader is not found
					Material mat = new Material(Shader.Find(shader) ?? Shader.Find("Unlit/Texture"));
					materials.Add(name, mat);
					loading++;
					StartCoroutine(GameBase.LoadDataFromFile(FLD_TEXTURE, file, x =>
					{
						mat.mainTexture = x.texture;
						float sizex = 1;
						float sizey = 1;
						if(mat.mainTexture.width < MAX_WIDTH) sizex = MAX_WIDTH / mat.mainTexture.width;
						if(mat.mainTexture.height < MAX_HEIGHT) sizey = MAX_HEIGHT / mat.mainTexture.height;
						mat.mainTextureScale = new Vector2(sizex, sizey);
						mat.mainTexture.filterMode = GameBase.EnumTryParse(filter, FilterMode.Point);
					}));
				}
			}
			//Sprites
			lastfile = FILE_SPRITE;
			using(StreamReader sr = new StreamReader(Path.Combine(GameBase.game_path, lastfile), Encoding.Default))
			{
				while(sr.Peek() > -1)
				{
					string name = sr.ReadLine();
					string file = sr.ReadLine();
					Material mat = new Material(Shader.Find("Unlit/TransparentColor"));
					mat.color = GameBase.ParseColor(sr.ReadLine());
					sprites.Add(name, mat);
					loading++;
					StartCoroutine(GameBase.LoadDataFromFile(FLD_SPRITE, file, x =>
					{
						mat.mainTexture = x.texture;
						mat.mainTexture.filterMode = FilterMode.Trilinear;
					}));
				}
			}
			//Map icons
			lastfile = FILE_MAP;
			using(StreamReader sr = new StreamReader(Path.Combine(GameBase.game_path, lastfile), Encoding.Default))
			{
				while(sr.Peek() > -1)
				{
					string name = sr.ReadLine();
					string file = sr.ReadLine();
					Material mat = new Material(Shader.Find("Unlit/TransparentColor"));
					mat.color = GameBase.ParseColor(sr.ReadLine());
					map_ico.Add(name, mat);
					loading++;
					StartCoroutine(GameBase.LoadDataFromFile(FLD_MAP, file, x =>
					{
						mat.mainTexture = x.texture;
						mat.mainTexture.filterMode = FilterMode.Trilinear;
					}));
				}
			}
			//Check if dictionaries are OK
			foreach(var s in setups)
			{
				MazeSetup setup = s.Value;
				foreach(var ch in chk_arr)
				{
					foreach(var f in ch.func)
					{
						string name = f(setup);
						bool error = !ch.dict.Contains(name);
						errors |= error;
						if(error) GameBase.ErrorMessage(FILE_SETUP, ch.err + name);
					}
				}
			}
			finished = !errors;
			if(!finished) enabled = false;
		}
		catch(System.Exception ex)
		{
			GameBase.ErrorMessage(lastfile, ex);
			enabled = false;
		}
	}
	
	void FixedUpdate()
	{
		if(finished && loaded == loading)
		{
			Application.LoadLevel((int)GameLevels.Menu);
			enabled = false;
		}
	}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameControl : MonoBehaviour
{
	public static GameControl inst;
	
	public Renderer floor, ceil;
	public Transform walls;
	public Transform sprites;
	public Transform spheres;
	public Transform mapobjs;
	public GameObject wall;
	public GameObject sprite;
	public Transform player;
	public Light dir_light;
	public GameObject[] sphere_objs;
	public int map_layer;
	public Renderer shader_dummy;
	public List<Transform> wall_list;
	public List<Transform> sprite_list;
	public List<SphereObj> sphere_list;
	Transform finish;
	Bounds finish_bound;
	List<Vector3> used_positions;
	
	//Maze size (square)
	const int MAZE_SIZE = 20;
	//Maze blocks
	const int MAZE_BLOCKS = MAZE_SIZE * MAZE_SIZE;
	//Maze wallbreak array size
	const int MAZE_WBRK = 4;
	//Wall distance in grid
	const float WALL_DIST = 2f;
	//Wall distance on grid
	const float WALL_DISTG = WALL_DIST * 2f;
	//Generation start point
	const float WALL_START = -(MAZE_SIZE * WALL_DISTG) / 2f + WALL_DIST;
	
	//For maze generation, movement
	public MazeBlockWall[,] maze_blocks;
	
	public static MazeWallOffset[] wall_offs =
	{
		new MazeWallOffset() { pos = new Vector3(-WALL_DIST, -2, 0), rot = Quaternion.Euler(0,90,0) },
		new MazeWallOffset() { pos = new Vector3(0, -2, WALL_DIST), rot = Quaternion.Euler(0,0,0) },
		new MazeWallOffset() { pos = new Vector3(WALL_DIST, -2, 0), rot = Quaternion.Euler(0,90,0) },
		new MazeWallOffset() { pos = new Vector3(0, -2, -WALL_DIST), rot = Quaternion.Euler(0,0,0) }
	};
	
	public static int[,] wall_brk =
	{
		{-1, 0},
		{0, 1},
		{1, 0},
		{0, -1}
	};
	
	public static Quaternion[] look_dirs =
	{
		Quaternion.Euler(0, 270, 0),
		Quaternion.Euler(0, 0, 0),
		Quaternion.Euler(0, 90, 0),
		Quaternion.Euler(0, 180, 0)
	};
	
	static Quaternion mapobj_rot = Quaternion.Euler(90.0f, 0.0f, 0.0f);
	static Vector3 bound_size = new Vector3(4f, 4f, 4f);
	
	MazeSetup setup;
	int maze_gen_cnt;
	
	//Rat creation
	public GameObject rat_obj;
	RatScript rat;
	
	void Awake()
	{
		inst = this;
		wall_list = new List<Transform>();
		sprite_list = new List<Transform>();
		sphere_list = new List<SphereObj>();
		used_positions = new List<Vector3>();
	}
	
	void Start()
	{
		setup = AssetLoader.setups[MenuScript.curr_setup];
		//Floor and Ceiling texture
		if(setup.hasfloor) floor.material = AssetLoader.materials[setup.floor];
		else floor.enabled = false;
		if(setup.hasceil) ceil.material = AssetLoader.materials[setup.ceil];
		else ceil.enabled = false;
		Camera cam = MouseLook.inst.cam.GetComponent<Camera>();
		cam.fieldOfView = MenuScript.fov;
		cam.backgroundColor = setup.backg;
		if(setup.fog)
		{
			RenderSettings.fog = true;
			RenderSettings.fogColor = setup.fog_col;
			RenderSettings.fogMode = (FogMode)setup.fog_type;
			if(RenderSettings.fogMode == FogMode.Linear)
				RenderSettings.fogEndDistance = setup.fog_den;
			else RenderSettings.fogDensity = setup.fog_den;
		}
		//Maze blocks init
		CreateMaze();
	}
	
	void FixedUpdate()
	{
		Vector3 pos = player.position;
		if(finish_bound.Contains(pos))
		{
			EnablePlayer(false);
			ObjScale.inst.Scale(-1f, RebuildMaze);
		}
		else if(Input.GetButton("New"))
		{
			EnablePlayer(false);
			ObjScale.inst.Scale(-1f, RebuildMaze);
		}
		for(int i = sphere_list.Count -1; i >= 0; i--)
		{
			SphereObj s = sphere_list[i];
			if(s.bound.Contains(pos))
			{
				sphere_list.Remove(s);
				PlayerCtrl.inst.Spin(() =>
				{
					Destroy(s.obj);
					Destroy(s.map_obj);
				});
				break;
			}
		}
	}
	
	//For windowed mode
	void OnApplicationFocus(bool stat)
	{
		if(!Cursor.visible && stat)
			Screen.lockCursor = true;
	}
	
	void CreateMaze()
	{
		maze_blocks = new MazeBlockWall[MAZE_SIZE, MAZE_SIZE];
		for(int i = 0; i < MAZE_SIZE; i++)
			for(int j = 0; j < MAZE_SIZE; j++)
				maze_blocks[i, j] = MazeBlockWall.All;
		//Maze generation
		GenerateMaze();
		//Maze to 3D models
		GameObject tmp = (GameObject)GameObject.Instantiate(wall);
		tmp.GetComponent<Renderer>().material = AssetLoader.materials[setup.wall];
		for(int i = 0; i < MAZE_SIZE; i++)
		{
			for(int j = 0; j < MAZE_SIZE; j++)
			{
				MazeBlockWall tblock = maze_blocks[i, j];
				if((tblock & MazeBlockWall.Down) != MazeBlockWall.None)
					CreateWall(tmp, 3, GetGridCoords(i, j));
				if((tblock & MazeBlockWall.Left) != MazeBlockWall.None)
					CreateWall(tmp, 0, GetGridCoords(i, j));
				if(i == MAZE_SIZE - 1 && (tblock & MazeBlockWall.Right) != MazeBlockWall.None)
					CreateWall(tmp, 2, GetGridCoords(i, j));
				if(j == MAZE_SIZE - 1 && (tblock & MazeBlockWall.Up) != MazeBlockWall.None)
					CreateWall(tmp, 1, GetGridCoords(i, j));
			}
		}
		Destroy(tmp);
		//Player position
		PlayerCtrl.inst.SetPosition(Random.Range(0, MAZE_SIZE), Random.Range(0, MAZE_SIZE));
		//Player map object
		if(rat == null)
			CreateMapObj(player, setup.map_player, new Vector3(0f, 6f, 0f));
		//Start
		CreateMapObj(CreateSprite(player.position, setup.start).position, setup.map_start);
		//Finish
		Vector3 pos;
		do
			pos = GetRandomPos();
		while(pos == player.position);
		finish = CreateSprite(pos, setup.finish);
		CreateMapObj(finish.position, setup.map_finish);
		finish_bound = CreateBounds(finish.position);
		//Spheres
		for(int i = 0; i < Random.Range(5, 10); i++)
			CreateSphere(GetRandomPos(), Random.Range(0, 3));
		//OpenGL
		for(int i = 0; i < Random.Range(5, 10); i++)
			CreateMapObj(CreateSprite(GetRandomPos(), setup.opengl).position, setup.map_opengl);
		//Rat
		if(rat == null)
		{
			tmp = (GameObject)GameObject.Instantiate(rat_obj);
			rat = tmp.GetComponent<RatScript>();
			rat.rend.material = AssetLoader.sprites[setup.rat];
			rat.enabled = false;
			CreateMapObj(tmp.transform, setup.map_rat, new Vector3(0f, 5f, 0f)).localRotation = mapobj_rot;
		}
		//Set rat position
		rat.SetPosition(Random.Range(0, MAZE_SIZE), Random.Range(0, MAZE_SIZE));
		//Move player back, reset rotation
		player.rotation = default(Quaternion);
		player.position += new Vector3(0, 0, -1.4f);
		ObjScale.inst.Scale(1f, () => EnablePlayer(true));
	}
	
	void ClearMaze()
	{
		wall_list.Clear();
		sprite_list.Clear();
		sphere_list.Clear();
		used_positions.Clear();
		for(int i = 0; i < walls.childCount; i++)
			Destroy(walls.GetChild(i).gameObject);
		for(int i = 0; i < sprites.childCount; i++)
			Destroy(sprites.GetChild(i).gameObject);
		for(int i = 0; i < spheres.childCount; i++)
			Destroy(spheres.GetChild(i).gameObject);
		for(int i = 0; i < mapobjs.childCount; i++)
			Destroy(mapobjs.GetChild(i).gameObject);
	}
	
	void RebuildMaze()
	{
		ClearMaze();
		CreateMaze();
	}
	
	void EnablePlayer(bool input)
	{
		PlayerCtrl.inst.enabled = input;
		rat.enabled = input;
		enabled = input;
	}

	void CreateWall(GameObject obj, int input, Vector3 pos)
	{
		obj = (GameObject)GameObject.Instantiate(obj, pos + wall_offs[input].pos, wall_offs[input].rot);
		obj.transform.parent = walls;
		if(Random.Range(0, 100) < 5) obj.GetComponent<Renderer>().material = AssetLoader.materials[setup.wall2];
		wall_list.Add(obj.transform);
	}
	
	Transform CreateSprite(Vector3 pos, string typ)
	{
		GameObject tmp = (GameObject)GameObject.Instantiate(sprite);
		tmp.GetComponent<Renderer>().material = AssetLoader.sprites[typ];
		tmp.transform.parent = sprites;
		tmp.transform.position = pos;
		tmp.AddComponent<Billboard>();
		sprite_list.Add(tmp.transform);
		used_positions.Add(pos);
		return tmp.transform;
	}
	
	Transform CreateMapObj(Transform tran, string typ, Vector3 offs)
	{
		GameObject tmp = (GameObject)GameObject.Instantiate(sprite, tran.position, mapobj_rot);
		MapIcon mapobj = tmp.AddComponent<MapIcon>();
		mapobj.target = tran;
		mapobj.offs = offs;
		tmp.GetComponent<Renderer>().material = AssetLoader.map_ico[typ];
		tmp.layer = map_layer;
		return tmp.transform;
	}
	
	Transform CreateMapObj(Transform tran, string typ)
	{
		GameObject tmp = (GameObject)GameObject.Instantiate(sprite, tran.position, mapobj_rot);
		MapIcon2 mapobj = tmp.AddComponent<MapIcon2>();
		mapobj.target = tran;
		tmp.GetComponent<Renderer>().material = AssetLoader.map_ico[typ];
		tmp.transform.parent = mapobjs;
		tmp.layer = map_layer;
		return tmp.transform;
	}
	
	Transform CreateMapObj(Vector3 pos, string typ)
	{
		GameObject tmp = (GameObject)GameObject.Instantiate(sprite, pos, mapobj_rot);
		tmp.GetComponent<Renderer>().material = AssetLoader.map_ico[typ];
		tmp.transform.parent = mapobjs;
		tmp.layer = map_layer;
		return tmp.transform;
	}
	
	Transform CreateSphere(Vector3 pos, int typ)
	{
		GameObject tmp = (GameObject)GameObject.Instantiate(sphere_objs[typ]);
		tmp.transform.parent = spheres;
		tmp.transform.position = pos + new Vector3(0f, -1f, 0f);
		used_positions.Add(pos);
		SphereObj spr = new SphereObj();
		spr.obj = tmp;
		//
		tmp = (GameObject)GameObject.Instantiate(sprite, pos, mapobj_rot);
		tmp.AddComponent<MapIcon3>();
		tmp.GetComponent<Renderer>().material = AssetLoader.map_ico[setup.map_sphere];
		tmp.transform.parent = mapobjs;
		tmp.layer = map_layer;
		//
		spr.map_obj = tmp;
		spr.bound = CreateBounds(pos);
		sphere_list.Add(spr);
		return tmp.transform;
	}
	
	void GenerateMaze()
	{
		int x = Random.Range(0, MAZE_SIZE);
		int y = Random.Range(0, MAZE_SIZE);
		maze_gen_cnt = 0;
		retry:
		DestroyWall(x, y);
		//If there are still full blocks
		if(maze_gen_cnt < MAZE_BLOCKS)
		{
			for(x = 0; x < MAZE_SIZE; x++)
			{
				for(y = 0; y < MAZE_SIZE; y++)
				{
					//Search for full blocks
					if(maze_blocks[x, y] != MazeBlockWall.All) continue;
					for(int i = 0; i < MAZE_WBRK; i++)
					{
						//If the full block has a non-full neighbour, then continue the generation from there
						int mx = x + wall_brk[i, 0];
						int my = y + wall_brk[i, 1];
						if(mx >= 0 && mx < MAZE_SIZE && my >= 0 && my < MAZE_SIZE)
						{
							if(maze_blocks[mx, my] == MazeBlockWall.All) continue;
							x = mx;
							y = my;
							goto retry;
						}
					}
				}
			}
		}
	}
	
	void DestroyWall(int x, int y)
	{
		int idx = Random.Range(0, MAZE_WBRK);
		int mx = x + wall_brk[idx, 0];
		int my = y + wall_brk[idx, 1];
		for(int i = 0; i < MAZE_WBRK; i++)
		{
			if(mx >= 0 && mx < MAZE_SIZE && my >= 0 && my < MAZE_SIZE)
			{
				if(maze_blocks[mx, my] == MazeBlockWall.All)
				{
					maze_blocks[x, y] &= ~(MazeBlockWall)(1 << idx);
					maze_blocks[mx, my] &= ~(MazeBlockWall)((1 << idx + 2) % 15);
					DestroyWall(mx, my);
				}
			}
			idx = i;
			mx = x + wall_brk[idx, 0];
			my = y + wall_brk[idx, 1];
		}
		maze_gen_cnt++;
	}
	
	public void EnableCollisions(bool input)
	{
		foreach(Transform t in wall_list)
			t.GetComponent<Collider>().enabled = input;
	}
	
	public static Vector3 GetGridCoords(int x, int y)
	{
		return new Vector3(WALL_START + WALL_DISTG * x, 0, WALL_START + WALL_DISTG * y);
	}
	
	public Vector3 GetRandomPos()
	{
		Vector3 pos;
		do
			pos = GetGridCoords(Random.Range(0, MAZE_SIZE), Random.Range(0, MAZE_SIZE));
		while(used_positions.Contains(pos));
		return pos;
	}
	
	public static bool CheckMovement(MazeBlockWall block, int dir)
	{
		return (block & (MazeBlockWall)((int)MazeBlockWall.Left << dir)) == MazeBlockWall.None;
	}
	
	public static Bounds CreateBounds(Vector3 pos)
	{
		return new Bounds(pos, bound_size);
	}
}
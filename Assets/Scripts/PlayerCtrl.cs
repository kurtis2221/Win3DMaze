using UnityEngine;
using System.Collections;

public class PlayerCtrl : MonoBehaviour
{
	const float SPD_TURN = 5.0f;
	const float SPD_TURN2 = 8.0f;
	const float SPD_WALK = 10.0f;
	const float SPD_RUN = 20.0f;
	
	public static PlayerCtrl inst;
	public Camera map_cam;
	CharacterController ctrl;
	Transform tran;
	//Control type
	System.Action ctrl_func;
	//Original control
	int x, y;
	int dir;
	Vector3 target_pos;
	Quaternion target_rot;
	bool ctrl_mov;
	bool ctrl_rot;
	bool coll_chk;
	//Spin
	bool spin;
	Quaternion target_spin;
	bool flipped;
	System.Action spin_hndl;
	
	void Awake()
	{
		Screen.lockCursor = true;
		Cursor.visible = false;
		inst = this;
		tran = GetComponent<Transform>();
		ctrl = GetComponent<CharacterController>();
		ctrl_mov = false;
		ctrl_rot = false;
		spin = false;
	}
	
	void Start()
	{
		if(MenuScript.ctrl == ControlType.Classic)
		{
			ctrl_func = ClassicCtrl;
			MouseLook.inst.enabled = false;
		}
		else if(MenuScript.ctrl == ControlType.Original)
		{
			ctrl_func = OrigCtrl;
			MouseLook.inst.enabled = false;
		}
		else
		{
			ctrl_func = ModernCtrl;
			MouseLook.inst.enabled = true;
		}
		enabled = false;
	}
	
	void Update()
	{
		if(Input.GetKeyDown(KeyCode.Escape))
			Application.LoadLevel((int)GameLevels.Menu);
		else if(Input.GetButtonDown("Map"))
			map_cam.enabled = !map_cam.enabled;
		else if(Input.GetKey(KeyCode.Backspace))
		{
			if(Input.GetKeyDown(KeyCode.O))
			{
				coll_chk = false;
				GameControl.inst.EnableCollisions(coll_chk);
			}
			else if(Input.GetKeyDown(KeyCode.P))
			{
				coll_chk = true;
				GameControl.inst.EnableCollisions(coll_chk);
			}
			else if(!spin && Input.GetKeyDown(KeyCode.I))
			{
				Spin(null);
			}
		}
	}
	
	void FixedUpdate()
	{
		if(spin)
		{
			tran.rotation = Quaternion.RotateTowards(tran.rotation, target_spin, SPD_TURN);
			if(Quaternion.Angle(tran.rotation, target_spin) < 1.0f)
			{
				spin = false;
				flipped = !flipped;
				if(spin_hndl != null) spin_hndl();
			}
			return;
		}
		ctrl_func();
		float zoom = Input.GetAxis("MapZ");
		if(zoom != 0)
		{
			float size = map_cam.orthographicSize;
			size -= zoom;
			if(size < 5) size = 5;
			else if(size > 40) size = 40;
			map_cam.orthographicSize = size;
		}
	}
	
	public void Spin(System.Action hndl)
	{
		spin = true;
		target_spin = tran.rotation * Quaternion.Euler(0, 0, 180f);
		spin_hndl = hndl;
	}
	
	public void SetPosition(int x, int y)
	{
		dir = 1;
		ctrl_mov = true;
		coll_chk = true;
		flipped = false;
		this.x = x;
		this.y = y;
		tran.position = GameControl.GetGridCoords(x, y);
		target_pos = tran.position;
		target_rot = tran.rotation;
	}
	
	void OrigCtrl()
	{
		float ver = Input.GetAxisRaw("Vertical");
		float hor = Input.GetAxisRaw("Horizontal");
		bool run = Input.GetButton("Run");
		float spd = run ? SPD_RUN : SPD_WALK;
		float spd_t = run ? SPD_TURN2 : SPD_TURN;
		if(ctrl_mov || ctrl_rot)
		{
			if(ctrl_mov)
			{
				tran.position = Vector3.MoveTowards(tran.position, target_pos, spd * Time.fixedDeltaTime);
				if(Vector3.Distance(tran.position,target_pos) < 0.01f)
				{
					tran.position = target_pos;
					ctrl_mov = false;
				}
			}
			if(ctrl_rot)
			{
				tran.rotation = Quaternion.RotateTowards(tran.rotation, target_rot, spd_t);
				if(Quaternion.Angle(tran.rotation, target_rot) < 1.0f)
				{
					tran.rotation = target_rot;
					ctrl_rot = false;
				}
			}
			return;
		}
		if(ver != 0 || hor != 0)
		{
			MazeBlockWall block = MazeBlockWall.None;
			if(coll_chk) block = GameControl.inst.maze_blocks[x, y];
			if(ver != 0)
			{
				if(ver > 0)
				{
					if(!coll_chk || GameControl.CheckMovement(block, dir))
					{
						x += GameControl.wall_brk[dir, 0];
						y += GameControl.wall_brk[dir, 1];
						target_pos = GameControl.GetGridCoords(x, y);
						ctrl_mov = true;
					}
				}	
				else
				{
					if(!coll_chk || GameControl.CheckMovement(block,(dir + 2) % 4))
					{
						dir += 2;
						dir %= 4;
						x += GameControl.wall_brk[dir, 0];
						y += GameControl.wall_brk[dir, 1];
						target_pos = GameControl.GetGridCoords(x, y);
						target_rot = tran.rotation * Quaternion.Euler(0, 180, 0);
						ctrl_mov = true;
						ctrl_rot = true;
					}
				}
			}
			if(hor != 0)
			{
				if(flipped) hor = -hor;
				if(hor > 0)
				{
					dir++;
					dir %= 4;
				}
				else
				{
					dir += 3;
					dir %= 4;
				}
				if(flipped) hor = -hor;
				target_rot = tran.rotation * Quaternion.Euler(0, 90 * hor, 0);
				ctrl_rot = true;
			}
		}
	}
	
	void ClassicCtrl()
	{
		float ver = Input.GetAxisRaw("Vertical");
		float hor = Input.GetAxisRaw("Horizontal");
		bool strafe = Input.GetButton("Strafe");
		bool run = Input.GetButton("Run");
		float spd = run ? SPD_RUN : SPD_WALK;
		float spd_t = run ? SPD_TURN2 : SPD_TURN;
		if(ver != 0 || (strafe && hor != 0)) MovePlayer(ver, hor, spd);
		if(hor != 0 && !strafe)
		{
			tran.Rotate(0, spd_t * hor, 0);
		}
	}
	
	void ModernCtrl()
	{
		float ver = Input.GetAxis("Vertical");
		float hor = Input.GetAxis("Horizontal");
		float spd = Input.GetButton("Run") ? SPD_RUN : SPD_WALK;
		if(ver != 0 || hor != 0) MovePlayer(ver, hor, spd);
	}
	
	void MovePlayer(float ver, float hor, float spd)
	{
		float inputmod = hor != 0 && ver != 0 ? 0.7071f : 1.0f;
		Vector3 dir = Vector3.zero;
		dir.z = ver;
		dir.x = hor;
		dir = tran.TransformDirection(dir);
		dir.y = 0;
		ctrl.Move(spd * dir * inputmod * Time.fixedDeltaTime);
	}
}
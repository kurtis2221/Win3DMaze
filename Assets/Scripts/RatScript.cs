using UnityEngine;
using System.Collections;

public class RatScript : MonoBehaviour
{
	const float SPD_MOVE = 5.0f;
	const float SPD_TURN = 180.0f;
	
	public Renderer rend;
	
	Transform tran;
	Vector3 target_pos;
	Quaternion target_rot;
	int dir;
	int x, y;
	
	void Awake()
	{
		tran = GetComponent<Transform>();
	}
	
	public void SetPosition(int x, int y)
	{
		dir = 1;
		this.x = x;
		this.y = y;
		tran.position = GameControl.GetGridCoords(x, y);
		target_pos = tran.position;
		target_rot = tran.rotation;
	}
	
	void FixedUpdate()
	{
		tran.position = Vector3.MoveTowards(tran.position, target_pos, SPD_MOVE * Time.fixedDeltaTime);
		tran.rotation = Quaternion.RotateTowards(tran.rotation, target_rot, SPD_MOVE);
		if(Vector3.Distance(tran.position, target_pos) < 0.01f)
		{
			tran.position = target_pos;
			tran.rotation = target_rot;
			MazeBlockWall block = GameControl.inst.maze_blocks[x, y];
			while(true)
			{
				if(GameControl.CheckMovement(block, dir))
				{
					x += GameControl.wall_brk[dir, 0];
					y += GameControl.wall_brk[dir, 1];
					target_pos = GameControl.GetGridCoords(x, y);
					target_rot = GameControl.look_dirs[dir];
					dir += 3;
					dir %= 4;
					break;
				}
				dir++;
				dir %= 4;
			}
		}
	}
}
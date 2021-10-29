using UnityEngine;
using System.Collections;

public class MenuControl : MonoBehaviour
{
	public int menuid;
	
	void OnMouseEnter()
	{
		GetComponent<Renderer>().material.color = Color.red;
	}
	
	void OnMouseExit()
	{
		GetComponent<Renderer>().material.color = Color.white;
	}
	
	void OnMouseDown()
	{
		switch(menuid)
		{
		case 0:
			MenuScript.inst.SaveCFG();
			Application.LoadLevel((int)GameLevels.Game);
			break;
		case 1:
			MenuScript.inst.ChangeSetup(-1);
			break;
		case 2:
			MenuScript.inst.ChangeSetup(1);
			break;
		case 3:
			MenuScript.inst.ChangeFOV(-5);
			break;
		case 4:
			MenuScript.inst.ChangeFOV(5);
			break;
		case 5:
			MenuScript.inst.ChangeControl();
			break;
		case 6:
			MenuScript.inst.ChangeMouse(-1);
			break;
		case 7:
			MenuScript.inst.ChangeMouse(1);
			break;
		default:
			Application.Quit();
			break;
		}
	}
}
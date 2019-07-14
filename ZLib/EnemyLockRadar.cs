using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using static ScanLib;
using static MenuLib;

public class EnemyLockRadar
{
	private AutoPilot ap;
	//Library load
	UtilityLib Util;
	ScanLib Scan;
	LockOnLib Lock;
	RadarLib Rad;
	KeybindLib Key;
	MenuLib Menu;
	//Common Things 
	Vector3 botPos;
	Vector3 botVel;
	float botDir;
	float botSpeed;
	//??
	float dScrn = Screen.height/10;
	//Radar Option
	Vector2 radarOffsetDef = new Vector2(Screen.width/2-100,-(Screen.height/2-100));
	Vector2 radarOffset;
	Vector2 radarWindow = new Vector2(Screen.width/4,Screen.height/4);
	float radarScale = 0.15f;
	//enemy List Container
	EnemyProp nullEnemy;
	EnemyProp lockedEnemy;
	Dictionary<string, EnemyProp> enemyBot,plasmaPoint;
	List<GameObject> bulletPoint;
	//Options
	bool RadarLocked = true;


	public EnemyLockRadar(AutoPilot ap,float dScrn,float radarScale,Vector2 radarOffsetDef,Vector2 radarWindow,MenuLib menu)
	{
		string back;
		int numBack;
		this.ap = ap;
		this.dScrn = dScrn;
		this.radarScale = radarScale;
		this.radarOffsetDef = radarOffsetDef;
		this.radarWindow = radarWindow;
		radarOffset = radarOffsetDef;
		Util = new UtilityLib(ap,dScrn);
		Scan = new ScanLib(ap,dScrn);
		Lock = new LockOnLib(ap,dScrn);
		Key = new KeybindLib(ap);
		Menu = menu;
		Rad = new RadarLib(ap,dScrn,radarOffset,radarScale,radarWindow);

		List<SubMenu> RadarMenu = new List<SubMenu>();
		back = "MainMenu"; numBack = 0;
		RadarMenu.Add(new SubMenu("Radar Scale",1,numBack,back,"",0.15f,0.001f,0.5f,0f));
		RadarMenu.Add(new SubMenu("Radar X",1,numBack,back,"",0f,10f,0.5f));
		RadarMenu.Add(new SubMenu("Radar Y",1,numBack,back,"",0f,10f,0.5f));
		Menu.MenuAdd("RadarMenu",RadarMenu);
		
		List<SubMenu> LockOnMenu = new List<SubMenu>();
		back = "MainMenu"; numBack = 1;
		LockOnMenu.Add(new SubMenu("LockOnStat",2,numBack,back,"",false));
		LockOnMenu.Add(new SubMenu("X",0,numBack,back,""));
		LockOnMenu.Add(new SubMenu("Y",0,numBack,back,""));
		LockOnMenu.Add(new SubMenu("Z",0,numBack,back,""));
		Menu.MenuAdd("LockOnMenu",LockOnMenu);
	}

	public void ScanEnemy(AutoPilot ap)
	{
		botPos = ap.GetPosition();
		botVel = ap.GetVelocity();
		botDir = ap.GetDirection();
		botSpeed = ap.GetSpeed();

		Scan.Scan();
		enemyBot = Scan.enemybotProp;
		plasmaPoint = Scan.GetScan("plasma");
		bulletPoint = Scan.ScanBullet();
		Scan.MultiLock();
		Scan.MultiLockUpdate(botPos,botVel,6,Key.PressedDown("Refresh LockOn",1));
		
	}

	public void AntiMissile()
	{
		Lock.AntiMissile();
	}

	public void LockOnSystem(AutoPilot ap)
	{
		RadarLocked = true;
		Util.PolyMaker2D(Color.white,dScrn+3,4,45,new Vector2(0,0));
		foreach (var enemy in enemyBot)
		{
			if (enemy.Value.gameObject!=null)
			{
				Vector3 screenPos = Util.CoordFromCam(enemy.Value.gameObject.transform.position);
				Vector2 ScrPosCon = Util.ScreenConvert(screenPos);
				if (screenPos.z>=0 && !enemy.Value.lockState)
				Util.PolyMaker2D(Color.blue,4,4,90*Time.time/1.15f,ScrPosCon);

				if (enemy.Value.lockState)
				{
					Scan.LockOnHUD(botPos,enemy.Value);
					 if (Menu.GetValueBool("lockOnStat"))
					 Scan.LockSet(enemy.Value);
				}
			}
		}

		if (Menu.GetValueBool("lockOnStat"))
			Scan.LockSet(0);

		foreach (var enemy in plasmaPoint)
		{
			if (enemy.Value.gameObject!=null)
			{
				Vector3 screenPos = Util.CoordFromCam(enemy.Value.gameObject.transform.position);
				Vector2 ScrPosCon = Util.ScreenConvert(screenPos);
				if (screenPos.z>=0)
				Util.PolyMaker2D(Color.red,4,4,90*Time.time/1.15f,ScrPosCon);
			}
		}
		if (lockedEnemy!=null)
		{
			if(lockedEnemy.gameObject==null)
				lockedEnemy=nullEnemy;
		}
		 lockedEnemy = Scan.LockOn(enemyBot,lockedEnemy);
		 Lock.input();
		if (lockedEnemy!=null)
		{
			Rad.RadarUnique(ap,botPos,lockedEnemy);
			Lock.LockOn(botPos,botVel,lockedEnemy.gameObject);
			Scan.LockOnHUD(botPos,lockedEnemy);
		}
		if (Key.PressedDown("Reset LockOn",1))
			Scan.ResetLock();
		Lock.LockOnUpdate(Menu.GetValueBool("lockOnStat"));
	}
	

	public void Radar()
	{
		Rad.RadarDrawer(botPos,enemyBot);
		Rad.RadarDrawer2(botPos,bulletPoint);
		if (RadarLocked)
		{
			foreach (var enemy in enemyBot)
			{
				if (enemy.Value.gameObject!=null)
				{
					if (enemy.Value.lockState)
					Rad.RadarUnique(ap,botPos,enemy.Value);
				}
			}
		}
		radarOffset = new Vector2(radarOffsetDef.x+Menu.GetValueNum("radarX"),radarOffsetDef.y+Menu.GetValueNum("radarY"));
		Rad.RadarUpdate(radarOffset,Menu.GetValueNum("radarScale"),radarWindow);
		Rad.RadarSelf(botDir);
		Rad.RadarCam();
	}
}

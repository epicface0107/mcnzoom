using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;


public class ScanLib
{
	private AutoPilot ap;
	private UtilityLib Util;
	float lockSetting = 1f;
	Vector3 lastbotPos = new Vector3(0,0,0);
	float dScrn;
	float bullet;
	public Dictionary<string, EnemyProp> enemybotProp;
	public Dictionary<string, EnemyProp> plasmaProp;
	public List<GameObject> bulletProp;
	public List<EnemyProp> lockedEnemyList;
	public List<EnemyProp> topEnemyList;
	public List<Vector3> aim;


	public ScanLib(AutoPilot _ap,float dScrn)
	{
		Util = new UtilityLib(ap,dScrn);
		this.ap = _ap;
		this.dScrn = dScrn;
		this.enemybotProp = new Dictionary<string,EnemyProp>();
		this.plasmaProp = new Dictionary<string,EnemyProp>();
		this.bulletProp = new List<GameObject>();
		this.lockedEnemyList = new List<EnemyProp>();
		this.topEnemyList = new List<EnemyProp>();
		this.aim = new List<Vector3>();
	}


	public void Scan()
	{
		var c = GameObject.FindGameObjectsWithTag("enemy");
		Update();
		foreach(GameObject all in c)
		{	
			if(!all.name.StartsWith("Plasma"))
			{
				if (!this.enemybotProp.ContainsKey(all.name))
				this.enemybotProp.Add(all.name, new EnemyProp(all));
				if (this.enemybotProp.ContainsKey(all.name))
				{
					if (all!=null)
						enemybotProp[all.name].Update();
				}
			}
			else
			{
				if (!this.plasmaProp.ContainsKey(all.name))
				this.plasmaProp.Add(all.name, new EnemyProp(all));
				if (this.plasmaProp.ContainsKey(all.name))
				{
					if (all!=null)
						plasmaProp[all.name].Update();
				}
			}
		}
	}

	public List<GameObject> ScanBullet()
	{
		var d = GameObject.FindGameObjectsWithTag("enemyAttack");
		List<GameObject> tempList = new List<GameObject>();
		foreach(GameObject all in d)
		{	
			tempList.Add(all);
		}
		return tempList;

	}

	public Dictionary<string,EnemyProp> GetScan(string type)
	{
		Dictionary<string,EnemyProp> returnto = new Dictionary<string,EnemyProp>(); 
		if (type == "machine")
		returnto = enemybotProp;
		if (type == "plasma")
		returnto = plasmaProp;
		return returnto;
	}

	public void Update()
	{
		enemybotProp = enemybotProp.Where(enemybotProp => enemybotProp.Value.gameObject !=null)
                                 .ToDictionary(enemybotProp => enemybotProp.Key, enemybotProp => enemybotProp.Value);
        plasmaProp = plasmaProp.Where(plasmaProp => plasmaProp.Value.gameObject !=null)
                                 .ToDictionary(plasmaProp => plasmaProp.Key, plasmaProp => plasmaProp.Value);
	}

	public EnemyProp LockOn(Dictionary<string,EnemyProp> enemyList,EnemyProp enemyLocked)
	{
		EnemyProp lockedOn = enemyLocked;
		foreach(var enemy in enemyList)
		{
			EnemyProp enemyVal = enemy.Value;
			if (enemyVal.gameObject!=null)
			{
				Vector3 screenPos = Util.CoordFromCam(enemyVal.gameObject.transform.position);
				Vector2 ScrPosCon = Util.ScreenConvert(screenPos);
				// ap.DrawLine2D(Color.white,new Vector2(screenPos.x,screenPos.x),new Vector2(ScrPosCon.x,ScrPosCon.y));
				float rank = Mathf.Abs(ScrPosCon.x)+Mathf.Abs(ScrPosCon.y);
				// ap.Print(0, "name : " + ScrPosCon );
				if (screenPos.z>=0 &&( ScrPosCon.x>=-dScrn && ScrPosCon.x<=dScrn && ScrPosCon.y>=-dScrn && ScrPosCon.y<dScrn))
				{	
					
					if (Input.GetKey("r"))
					{
						if (lockedOn==null)
						{
							lockedOn = enemyVal ;
							lockedOn.lockOnTime =lockSetting;
						}
						else
						{
							Vector3 rankPos =  Util.CoordFromCam(lockedOn.gameObject.transform.position);
							Vector2 rankPosCon2 = Util.ScreenConvert(rankPos);
							float rank2 = Mathf.Abs(rankPosCon2.x)+Mathf.Abs(rankPosCon2.y);
							if (lockedOn != enemy.Value&& rank < rank2)
							{
								lockedOn = enemyVal ;
								lockedOn.lockOnTime =lockSetting;
							}
						}
					}
				}
			}
		}
		return lockedOn;
	}

	public void LockSet(EnemyProp targetProp)
	{
		foreach (var enemy in topEnemyList)
		{
			if (enemy.gameObject!=null && targetProp.gameObject!=null)
			{
				if (enemy.gameObject.name.Contains(targetProp.gameObject.name))
				{
					int index = topEnemyList.FindIndex(a=> a.gameObject.name == targetProp.gameObject.name)+1;
					ap.SetAimPosition(index,targetProp.estPos);
				}
			}
		}
	}

	public void LockSet(int type)
	{
		if (topEnemyList.Count > 0)
		{
			float division = (topEnemyList.Count);
			if (type ==0)
			{
				for (int i =1 ; i<=6 ; i++)
				{
					if (topEnemyList[Mathf.CeilToInt(division*i/6)-1]!=null)
					{
						var enemy = topEnemyList[Mathf.CeilToInt(division*i/6)-1];
						ap.SetAimPosition(i,enemy.estPos);
					}
				}
			}
			else if (type ==1)
			{
				for (int i =1 ; i<=2 ; i++)
				{
					if  (topEnemyList[i-1] !=null)
					{
						var enemy = topEnemyList[i-1];
						ap.SetAimPosition(i,enemy.estPos);
					}
				}
			}
		}
	}


	public void LockOnHUD(Vector3 origin,EnemyProp targetProp)
	{
		Vector3 target = targetProp.posNow;
		Vector3 screenPos = Util.CoordFromCam(target);
		Vector2 ScrPosCon = Util.ScreenConvert(screenPos);

		Vector3 estPos = targetProp.estPos;
		Vector3 screenPos2 = Util.CoordFromCam(estPos);
		Vector2 ScrPosCon2 = Util.ScreenConvert(screenPos2);

	    // ap.Print(8, "Lock : " + targetProp.lockOnTime );
		if (screenPos.x<=0||screenPos.y<=0||screenPos.x>=Screen.width||screenPos.y>=Screen.height)
			ap.DrawLine3D(Color.red,Vector3.MoveTowards(origin,target,5f),Vector3.MoveTowards(origin,target,15f));
		if (screenPos.z >=0)
		{
			for (int i=0;i<topEnemyList.Count;i++)
			{	
				if (topEnemyList[i].gameObject == targetProp.gameObject)
				{
					int index = topEnemyList.FindIndex(a=> a.gameObject == targetProp.gameObject)+1;
					Util.PolyMaker2D(ap,Util.GroupColor(index),9+targetProp.lockOnTime*10,4,45+180*targetProp.lockOnTime,ScrPosCon);
					Util.PolyMaker2D(ap,Color.black,5+targetProp.lockOnTime*10,4,45+180*targetProp.lockOnTime,ScrPosCon);
					if (screenPos2.z >=0)
					Util.PolyMaker2D(ap,Util.GroupColor(index),9+targetProp.lockOnTime*10,3,0+180*targetProp.lockOnTime,ScrPosCon2);

					ap.DrawLine3D(Util.GroupColor(index),target,estPos);
				}
			}
			
			Util.PolyMaker2D(ap,Color.black	,5+targetProp.lockOnTime*10,4,45+180*targetProp.lockOnTime,ScrPosCon);
		}
		else
			ap.DrawLine3D(Color.red,Vector3.MoveTowards(origin,target,5f),Vector3.MoveTowards(origin,target,15f));
	}

	public void MultiLock()
	{
		foreach(var enemy in enemybotProp)
		{
			EnemyProp enemyVal = enemy.Value;
			if (enemyVal.gameObject!=null)
			{
				Vector3 screenPos = Util.CoordFromCam(enemyVal.gameObject.transform.position);
				Vector2 ScrPosCon = Util.ScreenConvert(screenPos);
				float rank = Mathf.Abs(ScrPosCon.x)+Mathf.Abs(ScrPosCon.y);
				if (screenPos.z>=0 &&( ScrPosCon.x>=-dScrn && ScrPosCon.x<=dScrn && ScrPosCon.y>=-dScrn && ScrPosCon.y<dScrn))
				{	
					if (Input.GetKey("r"))
					{
						if (!enemyVal.lockState)
						{
							enemyVal.lockOnTime =lockSetting;
							lockedEnemyList.Add(enemyVal);
						}
						enemyVal.lockState = true;
					}
				}
			}
		}
	}
	public void MultiLockUpdate(Vector3 pos,Vector3 velocity,int max,bool refresh)
	{
		List<EnemyProp> tempLockList = new List<EnemyProp>();
		EnemyProp check ;
		foreach (var enemy in lockedEnemyList)
		{
			if (enemy.gameObject !=null)
			{
				if (enemybotProp.TryGetValue(enemy.gameObject.name,out check))
				{
					if (enemy.gameObject == enemybotProp[enemy.gameObject.name].gameObject )
					{
						if (enemy.lockState)
						{
							tempLockList.Add(enemy);
							if (refresh)
							RankUpdate(0,enemy,pos,velocity);
							//estpos update
							enemy.estPos = Util.PosEstimate(pos,enemy.posNow,enemy.GetAverageVelocity(),velocity*0,380);
						}
					}
				}
			}
		}
		tempLockList = tempLockList.OrderBy(enemy => enemybotProp[enemy.gameObject.name].lockRank).ToList();
		lockedEnemyList = tempLockList;
		topEnemyList = tempLockList.Take(max).ToList();
		// ap.Print(0,"Count : " + topEnemyList.Count);
		// for (int i =0 ; i<topEnemyList.Count ; i++)
		// {
		// 	if (topEnemyList[i]!=null)
		// 		ap.Print(i+1,"Mob : " + i + " :"+ topEnemyList[i]);
		// }
	}

	public void RankUpdate(int type,EnemyProp enemy,Vector3 pos,Vector3 velocity)
	{
		if (type == 0)
		{
			enemy.lockRank = Vector3.Distance(pos,enemy.posNow);
		}
	}

	public Vector3 GetAimLock(int num)
	{
		if (lockedEnemyList[num]!=null)
		return lockedEnemyList[num].estPos;
		else if (lockedEnemyList[0]!=null)
		return lockedEnemyList[0].estPos;
		else
		return new Vector3(0,0,0);
	}

	public void ResetLock()
	{
		foreach(var enemy in enemybotProp)
		{
			EnemyProp enemyVal = enemy.Value;
			if (enemyVal.gameObject!=null)
			{
				enemyVal.lockState = false;
			}
		}
		lockedEnemyList = new List<EnemyProp>();
		ap.SetAimPosition(1,new Vector3(0,0,0));
		ap.SetAimPosition(2,new Vector3(0,0,0));
		ap.SetAimPosition(3,new Vector3(0,0,0));
		ap.SetAimPosition(4,new Vector3(0,0,0));
		ap.SetAimPosition(5,new Vector3(0,0,0));
		ap.SetAimPosition(6,new Vector3(0,0,0));
	}
	
	public class EnemyProp
	{
		public GameObject gameObject;
		public Vector3 posNow;	
		private float timeNow;
		public Vector3 posPrev;
		private float timePrev;
		public Vector3 offset;
		public Vector3 estPos;
		public float lockOnTime;
		public bool lockState;
		public float lockRank;
		public List<Vector3> velList;
 
		public EnemyProp(GameObject obj)
		{
			this.gameObject = obj;
			this.timeNow = Time.time;
			this.posPrev = this.gameObject.transform.position;
			this.posNow = this.gameObject.transform.position;
			this.timePrev = this.timeNow;
			this.lockOnTime = 0;
			this.lockState = false;
			this.lockRank = 0;
			this.estPos = new Vector3(0,0,0);
			this.velList = new List<Vector3>();
		}

		public void Update()
		{
			this.posPrev = this.posNow;
			this.posNow = this.gameObject.transform.position;
			this.timePrev = this.timeNow;
			this.timeNow = Time.time;
			if (lockOnTime > 0) this.lockOnTime = this.lockOnTime-Time.deltaTime;
			if (lockOnTime < 0) this.lockOnTime = 0;
			if (Time.frameCount % 2 == 0)
			{
				velList.Add(this.posPrev-this.posNow);
				velList.Reverse();
	    		velList = velList.Take(10).ToList();
	    		velList.Reverse();
	    	}
		}

		public void EstPos(Vector3 pos)
		{
			this.estPos = pos;
		}

		public Vector3 GetPosition()
		{
			return this.posNow;
		}

		public Vector3 GetForward()
		{
			return this.gameObject.transform.forward;
		}

		public Vector3 GetRight()
		{
			return this.gameObject.transform.right;
		}

		public Vector3 GetVelocity()
		{
			return (this.posPrev - this.posNow);
		}

		public Vector3 GetAverageVelocity()
		{
			Vector3 average = Vector3.zero;
			for (int i=0; i<velList.Count;i++)
			{
				average += velList[i];
			}
			return average/velList.Count;
		}
		public Vector3 GetForward2D()
		{
			return new Vector2(this.gameObject.transform.forward.x,this.gameObject.transform.forward.z);
		}

		public Vector2 GetRadar2dLocation(Vector3 offset)
		{
			return new Vector2(this.posNow.x-offset.x,this.posNow.z-offset.z);
		}

		public float GetRadar2dAngle()
		{
			return Mathf.RoundToInt(Mathf.Atan2(-this.gameObject.transform.forward.x, this.gameObject.transform.forward.z) * -180f / 3.14159274f);
		}
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static UtilityLib;

public class LockOnLib
{
	private AutoPilot ap;
	float dScrn;
	UtilityLib Util;
	Vector3 lastbotPos = new Vector3(0,0,0);
	const int MASK_BULLET = 1;  // 通常弾(Cannon)
	const int MASK_SHELL = 2;   // 戦車砲弾(Cannon)
	const int MASK_GRENADE = 4; // 榴弾(Cannon)
	const int MASK_BLADE = 8;   // 刃(Sword)
	const int MASK_PLASMA = 16; // プラズマ(Discharger)
	const int MASK_LASER = 32;  // レーザー(Beamer)
	const int MASK_ALL = 0xff;
	const int MASK_MISSILE = MASK_ALL-1-2-4-16-32-128;// ミサイル(Launcher)

	// Modifier
	bool lockOnStat=false;
	int selectMode = 1;
	int selectModeMax = 7;
	string modeText = "";
	float xOffset = 0;
	float yOffset = 1;
	float zOffset = 0;
	float randomMax = 0;
	float orbitRadius = 50;
	float orbitHeight = 100;
	float orbitSpeed = 12;
	bool drawLines = true;
	Vector3 posPrev = new Vector3(0,0,0);
	Vector3 posPrevLaunch = new Vector3(0,0,0);
	float lockOnSize = 30;
	
	public LockOnLib(AutoPilot _ap,float dScrn)
	{
		Util = new UtilityLib(ap,dScrn);
		this.ap = _ap;
		this.dScrn = dScrn;
	}

	public void LockOnUpdate(bool lockOnStat)
	{
		this.lockOnStat = lockOnStat;
	} 

	public void LockOn(Vector3 botPos,Vector3 botVel,GameObject LockBot)
	{
		Vector3 LockBotPos = LockBot.transform.position;
		 //Aim Estimator
	    Vector3 velocity = ((LockBotPos-posPrev)/Time.deltaTime);
	    Vector3 relVel= (velocity );
	    float dist= Vector3.Distance(botPos,LockBotPos);

	    //Random Maker
		Vector3 Randomizer = new Vector3(UnityEngine.Random.Range(-randomMax, randomMax),UnityEngine.Random.Range(-randomMax, randomMax),UnityEngine.Random.Range(-randomMax, randomMax));
		//Offset Maker
		Vector3 OffsetValue = LockBot.transform.TransformPoint(new Vector3(xOffset,yOffset,zOffset))-LockBotPos;

	    Vector3 estPosW = Util.PosEstimate(botPos,LockBotPos,botVel,velocity,UnityEngine.Random.Range(380, 420));
	    Vector3 estPos = estPosW ;

	    posPrev=LockBotPos;

		//Orbit Maker
		float xCircle = Mathf.Sin(Mathf.PI*(Time.time/orbitSpeed))*orbitRadius;	
		float zCircle = Mathf.Cos(Mathf.PI*(Time.time/orbitSpeed))*orbitRadius;
		Vector3 orbitCircle = new Vector3(xCircle,orbitHeight,zCircle);

		//Draw Line Stuff
		if (drawLines)
		{
			// ap.DrawLine3D(Color.red,botPos,LockBotPos);
			ap.DrawLine3D(Color.green,LockBotPos,LockBotPos+orbitCircle);
			ap.DrawLine3D(Color.blue,LockBotPos + (LockBot.transform.right*10),LockBotPos + (LockBot.transform.forward*10));
			ap.DrawLine3D(Color.blue,LockBotPos + (LockBot.transform.right*-10),LockBotPos + (LockBot.transform.forward*10));
			ap.DrawLine3D(Color.magenta,LockBotPos,LockBotPos+OffsetValue);
			ap.DrawLine3D(Color.yellow,LockBotPos+OffsetValue,OffsetValue+estPos);

			// ap.DrawLine3D(Color.black,ap.GetCameraPosition(),hitPos);
		}
		//Lock On Toggle System
		if (lockOnStat)
		{
			ap.SetAimPosition(1,Randomizer+OffsetValue+estPos);
			// ap.SetAimPosition(5,LockBotPos + Randomizer+OffsetValue+estPos);
			// ap.SetAimPosition(5,LockBot.transform.position+orbitCircle);
		}else{
			
			ap.SetAimPosition(1,new Vector3(0,0,0));
			// ap.SetAimPosition(5,new Vector3(0,0,0));
			// ap.SetAimPosition(6,new Vector3(0,0,0));
			
		}

		//Debug text
	    switch (selectMode)
	    {
	    	case 1: 
	    		modeText = "X Offset";
	    		break;
	    	case 2: 
	    		modeText = "Y Offset";
	    		break;
	    	case 3: 
	    		modeText = "Z Offset";
	    		break;
	    	case 4: 
	    		modeText = "Random";
	    		break;
	    	case 5: 
	    		modeText = "Orbit Radius";
	    		break;
	    	case 6: 
	    		modeText = "Orbit Height";
	    		break;
	    	case 7: 
	    		modeText = "Orbit Speed";
	    		break;

	    }

		// ap.Print(0, LockBot.name);	
		// ap.Print(1, "range  : " + dist );
		// ap.Print(2, "Mode : " + modeText );
		// ap.Print(3, "Rng : " + randomMax );
		// ap.Print(4, "Offset : " + new Vector3(xOffset,yOffset,zOffset) );
		// ap.Print(5, "Orbit : " + new Vector3(orbitRadius,orbitHeight,orbitSpeed) );
	}

	public void AntiMissile()
	{
		ap.SearchThreat(MASK_MISSILE, 10000);

		Vector3 velocity2 = ((ap.GetThreatPosition()-posPrevLaunch)/Time.deltaTime);
		float dist2 = ap.GetThreatDistance();
		Vector3 estpos2 = ap.GetThreatPosition() + dist2*velocity2*UnityEngine.Random.Range(0.00045f, 0.00060f);
		posPrevLaunch = ap.GetThreatPosition();
		if(ap.CheckThreat())
		{
			ap.StartAction("[AA-Miss", 1);
			ap.SetAimPosition(6,estpos2);
			ap.DrawLine3D(Color.blue,estpos2,ap.GetThreatPosition());
			if (ap.GetThreatDistance()<=300)
				ap.StartAction("[AA-Shot", 1);
		}
		else	
		{
			ap.SetAimPosition(6,new Vector3(0,0,0));
		}
	}
	public void input()
	{
		//Input Stuff
		if (Input.GetKeyDown("t"))
			lockOnStat = !(lockOnStat);

		if (Input.GetKeyDown("u"))
			drawLines = !(drawLines);

		if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			selectMode++;
			if (selectMode>selectModeMax)
				selectMode=1;
		}
		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			selectMode--;
			if (selectMode<=0)
				selectMode=selectModeMax;
		}

		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			switch (selectMode)
			{
				case 1: 
					xOffset++;
					break;
				case 2: 
					yOffset++;
					break;
				case 3: 
					zOffset++;
					break;
				case 4: 
					randomMax++;
					break;
				case 5: 
					orbitRadius++;
					break;
				case 6: 
					orbitHeight++;
					break;
				case 7: 
					orbitSpeed = orbitSpeed-0.1f;
					break;
			}
		}

		if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			switch (selectMode)
			{
				case 1: 
					xOffset--;
					break;
				case 2: 
					yOffset--;
					break;
				case 3: 
					zOffset--;
					break;
				case 4: 
					randomMax--;
					break;
				case 5: 
					orbitRadius--;
					break;
				case 6: 
					orbitHeight--;
					break;
				case 7: 
					orbitSpeed = orbitSpeed+0.1f;
					break;
			}
		}
	}
	
}

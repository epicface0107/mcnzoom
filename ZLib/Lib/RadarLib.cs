using UnityEngine;
using System;
using System.Collections.Generic;
using static ScanLib;

public class RadarLib
{
	private AutoPilot ap;
	private ScanLib Scan;
	private UtilityLib Util;
	Vector2 radarOffset;
	Vector2 radarWindow;
	float radarScale;
	float dScrn;

	public RadarLib(AutoPilot _ap,float dScrn,Vector2 offset,float scale,Vector2 radarWindow)
	{
		Scan = new ScanLib(ap,dScrn);
		Util = new UtilityLib(ap,dScrn);
		this.radarOffset = offset;
		this.radarScale = scale;
		this.radarWindow = radarWindow;
		this.ap = _ap;
		this.dScrn = dScrn;

	}
	public void RadarUpdate(Vector2 offset,float scale,Vector2 radarWindow)
	{
		this.radarOffset = offset;
		this.radarScale = scale;
		this.radarWindow = radarWindow;
	}
	public void RadarDrawer(Vector3 botPos,Dictionary<string, EnemyProp> List)
	{

		foreach (var poin in List)
		{
			var point = poin.Value;
			if (point!= null)
			{
				Vector2 RelPos2D = point.GetRadar2dLocation(botPos)*radarScale+radarOffset;
				float dir2d = point.GetRadar2dAngle();
				Util.PolyMaker2D(ap,Color.red,3+radarScale*5,8,dir2d,RelPos2D);
				Util.PolyMaker2D(ap,Color.green,4+radarScale*5,2,dir2d,RelPos2D+new Vector2(point.GetForward2D().x+4*Util.Angle("sin",dir2d),point.GetForward2D().z+4*Util.Angle("cos",dir2d)));
			}
		}
	}

	public void RadarDrawer2(Vector3 botPos,List<GameObject> List)
	{

		foreach (var point in List)
		{
			if (point!= null)
			{
				Vector3 posNow = point.transform.position;
				Vector2 RelPos2D = new Vector2(posNow.x-botPos.x,posNow.z-botPos.z)*radarScale+radarOffset;
				float dir2d =  Mathf.RoundToInt(Mathf.Atan2(-point.transform.forward.x, point.transform.forward.z) * -180f / 3.14159274f);
				Util.PolyMaker2D(ap,Color.green,1,4,dir2d,RelPos2D);
			}
		}
	}

	public void RadarUnique(AutoPilot ap,Vector3 botPos,EnemyProp point)
	{
		Vector2 RelPos2D = (point.GetRadar2dLocation(botPos)*radarScale)+radarOffset;
		float dir2d = point.GetRadar2dAngle();
		Util.PolyMaker2D(ap,Color.yellow,8+radarScale*5,3,dir2d,RelPos2D);
		Util.PolyMaker2D(ap,Color.black,6+radarScale*5,3,dir2d,RelPos2D);
		// Util.PolyMaker2D(ap,Color.white,30,3,-90*Time.time/1.15f,RelPos2D/10);
	}

	public void RadarSelf(float dir)
	{
		// Vector3 pointdir =new Vector3(point.transform.forward.x,0,point.transform.forward.z);
		// float dir2d = Vector3.Angle(pointdir,new Vector3(0,0,1));
		Util.PolyMaker2D(ap,Color.white,4+radarScale*5,2,dir,radarOffset);
		Util.PolyMaker2D(ap,Color.cyan,5+radarScale*5,3,dir,radarOffset);
	}
	public void RadarCam()
	{
		Vector3 dir = ap.GetCameraForward();
		Vector3 camPos = ap.GetCameraPosition();
		Vector3 botPos = ap.GetPosition();
		float dir2d = Mathf.RoundToInt(Mathf.Atan2(-dir.x, dir.z) * -180f / 3.14159274f);
		Vector3 RelPos = camPos-botPos;
		Vector2 camOffset = new Vector2(RelPos.x,RelPos.z)*radarScale+radarOffset;
		float camScale = (10+radarScale*200);
		ap.DrawLine2D(Color.green, camOffset, new Vector2(camScale*Util.Angle("sin",dir2d+45),camScale*Util.Angle("cos",dir2d+45))+camOffset);
		ap.DrawLine2D(Color.green, camOffset, new Vector2(camScale*Util.Angle("sin",dir2d-45),camScale*Util.Angle("cos",dir2d-45))+camOffset);
	}
}

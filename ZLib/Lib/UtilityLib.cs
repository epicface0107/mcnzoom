using UnityEngine;
using System;
using System.Collections.Generic;

public class UtilityLib
{
	private AutoPilot ap;
	float dScrn;
	Vector3 lastbotPos = new Vector3(0,0,0);
	
	public UtilityLib(AutoPilot _ap,float scrnSize)
	{
		this.ap = _ap;
		this.dScrn = scrnSize;
	}

	public float angle180(Vector3 from, Vector3 to, Vector3 right)
    {
        float angle = Vector3.Angle(from, to);
        float angle2 =(Vector3.Angle(right, to) > 90f) ?  360f - angle : angle;
        return  (angle2 > 180f) ?  -angle : angle;
    }

	public float angle360(Vector3 from, Vector3 to, Vector3 right)
    {
        float angle = Vector3.Angle(from, to);
        return (Vector3.Angle(right, to) > 90f) ? 360f - angle : angle;            
    }

	public float Angle(string a,float angle)
	{
		float angleRet = 1;
		if (a=="sin")
		angleRet = Mathf.Sin(angle/57.2958f);
		else if (a=="cos")
		angleRet = Mathf.Cos(angle/57.2958f);
		else if (a=="tan")
		angleRet = Mathf.Tan(angle/57.2958f);
		
		return angleRet;
	}
	public float AngleRot(string a,float angle,float speed)
	{
		float angleRet = 1;
		float timeAng = Mathf.PI*(Time.time/speed);
		if (a=="sin")
		angleRet = Mathf.Sin(angle/57.2958f+timeAng);
		else if (a=="cos")
		angleRet = Mathf.Cos(angle/57.2958f+timeAng);
		else if (a=="tan")
		angleRet = Mathf.Tan(angle/57.2958f+timeAng);
		
		return angleRet;
	}

	public Vector3 CoordFromCam(Vector3 pos)
	{
		GameObject Cam = GameObject.Find("MainCamera");
		return Cam.GetComponent<Camera>().WorldToScreenPoint(pos);
	}
	public Vector2 ScreenConvert(Vector3 pos3d)
	{
		return new Vector2(pos3d.x-Screen.width/2,pos3d.y-Screen.height/2);
	}

	public Vector2 CamCoordConverted(Vector3 pos)
	{
		return ScreenConvert(CoordFromCam(pos));
	}

	public Color GroupColor(int num)
	{
		switch (num)
		{
			case 1:
				return Color.red;
				break;
			case 2:
				return Color.green;
				break;
			case 3:
				return Color.blue;
				break;
			case 4:
				return Color.yellow;
				break;
			case 5:
				return Color.cyan;
				break;
			case 6:
				return Color.magenta;
				break;
			default:
				return Color.white;
				break;
		}
	}
	public List<Vector2> ListVector(float size,float sides,float angle,Vector2 pos)
	{
		List<Vector2> list = new List<Vector2>();
		float degree = 360/sides;
		for (int i = 0; i< sides ;i++)
		{
			 list.Add(new Vector2(pos.x+size*Angle("sin",angle+degree*i),pos.y+size*Angle("cos",angle+degree*i)));
			// ap.DrawLine2D(color, new Vector2(0,0), new Vector2(0,0));
		}		
		return list;
	}
	public void PolyMaker2D(Color color,float size,float sides,float angle,Vector2 pos)
	{
		float degree = 360/sides;
		for (int i = 0; i< sides ;i++)
		{
			ap.DrawLine2D(color, new Vector2(pos.x+size*Angle("sin",angle+degree*i),pos.y+size*Angle("cos",angle+degree*i)), new Vector2(pos.x+size*Angle("sin",angle+degree*i+degree),pos.y+size*Angle("cos",angle+degree*i+degree)));
			// ap.DrawLine2D(color, new Vector2(0,0), new Vector2(0,0));
		}		
	}

	public void PolyMaker2D(AutoPilot ap,Color color,float size,float sides,float angle,Vector2 pos)
	{
		float degree = 360/sides;
		for (int i = 0; i< sides ;i++)
		{
			ap.DrawLine2D(color, new Vector2(pos.x+size*Angle("sin",angle+degree*i),pos.y+size*Angle("cos",angle+degree*i)), new Vector2(pos.x+size*Angle("sin",angle+degree*i+degree),pos.y+size*Angle("cos",angle+degree*i+degree)));
			// ap.DrawLine2D(color, new Vector2(0,0), new Vector2(0,0));
		}
	}

	public Vector3 PosEstimate(Vector3 origin,Vector3 target,Vector3 originVelocity,Vector3 targetVelocity,float speed)
	{
		Vector3 RelPos = target - origin;
		Vector3 RelVel = targetVelocity - originVelocity;
		float time = ReachTime(speed,RelPos,RelVel);
		return target+RelVel*time;
	}

	public Vector3 PosEstimate(AutoPilot ap,Vector3 origin,Vector3 target,Vector3 originVelocity,Vector3 targetVelocity,float speed)
	{
		Vector3 RelPos = target - origin;
		Vector3 RelVel = targetVelocity - originVelocity;
		float time = ReachTime(speed,RelPos,RelVel);
		return target+RelVel*time;
	}

	public float ReachTime(float speed,Vector3 RelPos,Vector3 RelVel)
	{
		float VSquared = RelVel.sqrMagnitude;
		if (VSquared < 0.001f)
		 return 0f;
		float a = VSquared - speed*speed;

		if(Mathf.Abs(a)<0.001f)
		{
			float time = -RelVel.sqrMagnitude/(2f*Vector3.Dot(RelVel,RelPos));
			return Mathf.Max(time,0f);
		}

		float b = 2f*Vector3.Dot(RelVel,RelPos);
		float c = RelPos.sqrMagnitude;
		float determinant = b*b - 4f*a*c;

		if (determinant >0f)
		{
			float t1 = (-b+Mathf.Sqrt(determinant)) / (2f*a);
			float t2 = (-b-Mathf.Sqrt(determinant)) / (2f*a);
			if (t1 > 0f)
			{
				if (t2>0f)
					return Mathf.Min(t1,t2);
				else
					return t1;
			} else
				return Mathf.Max(t2,0f);
		}
		else if (determinant <0f)
			return 0f;
		else
			return Mathf.Max(-b/(2f*a),0f);
	}

	// public List<GameObject> Scan(string type)
	// {
	// 	List<GameObject> objectList = new List<GameObject>();
	// 	if (type=="bullet")
	// 	{
	// 		var c = GameObject.FindGameObjectsWithTag("enemyAttack");
	// 		foreach(GameObject all in c)
	// 		{
	// 			objectList.Add(all);
	// 		}
	// 	}
	// 	else
	// 	{
	// 		var c = GameObject.FindGameObjectsWithTag("enemy");
	// 		foreach(GameObject all in c)
	// 		{	
	// 			if(all.name.StartsWith("Plasma"))
	// 			{
	// 				// PolyMaker2D(ap,Color.red,4,3,0,CamPosAP(ap,Cam,all.transform.position));
	// 				if(type=="Plasma")
	// 					objectList.Add(all);
	// 			}
	// 			else
	// 			{
	// 				// PolyMaker2D(ap,Color.blue,4,5,0,CamPosAP(ap,Cam,all.transform.position));
	// 				if(type=="machine")
	// 				objectList.Add(all);
	// 			}
	// 		}
	// 	}
	// 	return objectList;
	// }

	// public GameObject LockOn(List<GameObject> enemyList,GameObject enemyLocked,GameObject Cam)
	// {
	// 	GameObject lockedOn = enemyLocked;
	// 	foreach (GameObject enemy in enemyList)
	// 	{
	// 		Vector3 screenPos = Cam.GetComponent<Camera>().WorldToScreenPoint(enemy.transform.position);
	// 		Vector2 ScrPosCon = ScreenConvert(screenPos);
	// 		float rank = Mathf.Abs(ScrPosCon.x)+Mathf.Abs(ScrPosCon.y);
	// 		if (screenPos.z>=0 &&( ScrPosCon.x>=-dScrn && ScrPosCon.x<=dScrn && ScrPosCon.y>=-dScrn && ScrPosCon.y<dScrn))
	// 		{	
	// 			if (Input.GetKey("r"))
	// 			{
	// 				if (lockedOn==null)
	// 				{
	// 					lockedOn = enemy ;
	// 				}
	// 				else
	// 				{
	// 					Vector3 rankPos =  Cam.GetComponent<Camera>().WorldToScreenPoint(lockedOn.transform.position);
	// 					Vector2 rankPosCon2 = ScreenConvert(rankPos);
	// 					float rank2 = Mathf.Abs(rankPosCon2.x)+Mathf.Abs(rankPosCon2.y);
	// 					if (lockedOn != enemy&& rank < rank2)
	// 					{
	// 						lockedOn = enemy ;
	// 					}
	// 				}
	// 			}
	// 		}
	// 	}
	// 	return lockedOn;
	// }
}

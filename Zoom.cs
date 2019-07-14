using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;	
using UnityEngine.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using static ScanLib;


public class Tank_JointCtrl : UserScript
{
 public override string GetUserName()
  {
    return "epicface0107[US]";
  }


  private Transform mcnTf, coreTf;
  private GameObject headLightL, headLightR;
  private int layerMask = 0;
    
    public int lengthOfLineRenderer = 50;
    public List<Vector3> lastdistance;
    Gradient gradient = new Gradient();
    Gradient gradient2= new Gradient();
    public GameObject aceMainGameObject = new GameObject();
    public GameObject g = new GameObject();
    public GameObject g2 = new GameObject();
    public Text t;
    LineRenderer lineRenderer = new GameObject().AddComponent<LineRenderer>();
    LineRenderer arrow = new GameObject().AddComponent<LineRenderer>();
    LineRenderer ring = new GameObject().AddComponent<LineRenderer>();
    public CanvasScaler cs;


    public GameObject trails = new GameObject();
    public TrailRenderer trailRend;

public override void OnStart(AutoPilot ap)//�J�n���Ɉ����s
  {

    	UtilityLib Util;
	ScanLib Scan;
	LockOnLib Lock;
	RadarLib Rad;
	KeybindLib Key;
	MenuLib Menu;
	EnemyLockRadar ELR;
	//Common Things 
	Vector3 botPos;
	Vector3 botVel;
	float botDir;
	float botSpeed;

    	float dScrn = Screen.height/10;
    		Util = new UtilityLib(ap,dScrn);
		Key = new KeybindLib(ap);
		Menu = new MenuLib(ap);
		ap.SetAimTolerance(20);
		ap.SetLogicalScreenHeight(Screen.height);	
		lastdistance = new List<Vector3>();
		AntiDuplicate();
        CanvasTest(ap);
        TrailStart(ap);
     	// trailRend.enabled = true;

this.mcnTf = ap.transform.parent;
    foreach (Transform c in mcnTf)
      if ((this.coreTf = c.Find("Core")) != null)
        break;

    headLightL = createHeadLight();
    headLightL.transform.localPosition = new Vector3(1f, 4f, 6.0f);
    headLightL.transform.localRotation = Quaternion.Euler(20, 0, 0);

    headLightR = createHeadLight();
    headLightR.transform.localPosition = new Vector3(-1f, 4f, 6.0f);
    headLightR.transform.localRotation = Quaternion.Euler(20, 0, 0);

        GameObject[] selfObj = GameObject.FindGameObjectsWithTag("self");
    layerMask = 0; // int layerMask; �Ƃ���OnStart()�̊O�Ő錾���Ă���
    foreach (GameObject obj in selfObj){
      int l = 1 << obj.layer;
      layerMask = layerMask | l;
      for(int i=0;i<obj.transform.childCount;i++){
        l = 1 << obj.transform.GetChild(i).gameObject.layer;
        layerMask = layerMask | l;
      }
    }
    layerMask = ~layerMask;
  }
 public override void OnUpdate(AutoPilot ap)//������s
  {
    
      //trail and menu
        //TrailUpdate(ap);
        CanvasMake(ap);

    //Light ON/OFF Switch
    if (Input.GetKeyDown(KeyCode.H))
    {
      bool flag = !headLightL.activeSelf;
      headLightL.SetActive(flag);
      headLightR.SetActive(flag);
    }
    if(this.HeadLights)
      ap.StartAction("HLights", -1);
    else
      ap.EndAction("HLights");
  }

    private bool HeadLights {get{return headLightL.activeSelf;}}
  
  public void OnDestroy()
  {
    MonoBehaviour.Destroy(headLightL);
    MonoBehaviour.Destroy(headLightR);

  }
  

  public GameObject createHeadLight() {
    GameObject headLightObj = new GameObject("HeadLightObject");
    headLightObj.SetActive(false);
    headLightObj.transform.parent = coreTf;
    Light light = headLightObj.AddComponent<Light>();
    light.shadows = LightShadows.Soft;
    light.shadowStrength = 1f;
    light.shadowResolution = LightShadowResolution.VeryHigh;
    light.shadowBias = 0.1f;
    light.shadowNormalBias = 0.8f;
    light.shadowNearPlane = 0.2f;
    light.type = LightType.Spot;
    light.range = 400;
    light.spotAngle = 120;
    light.intensity = 2.2f;
    light.color = new Color((210f / 255f), (210f / 255f), 1f);
    light.renderMode = LightRenderMode.ForcePixel;
    return headLightObj;
   }

   public void AntiDuplicate()
    {
    	var a = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "AceScripts");
		foreach (var c in a)
		{
			foreach(Transform child in c.transform)
			{
				child.gameObject.active = false;
				Destroy(child.gameObject);
			}
			c.active = false;
			Destroy(c);
		}
		aceMainGameObject.name = "AceScripts";
    }

     public void CanvasTest(AutoPilot ap)
    {

     Canvas canvas = g.AddComponent<Canvas>();
     canvas.renderMode = RenderMode.ScreenSpaceCamera;
     //WorldSpace ScreenSpaceCamera
     cs = g.AddComponent<CanvasScaler>();
     cs.scaleFactor = 1.0f;
     cs.dynamicPixelsPerUnit = 30f;
     // GraphicRaycaster gr = g.AddComponent<GraphicRaycaster>();
     t = g2.AddComponent<Text>();
     g2.name = "Text";
     g2.transform.parent = g.transform;
     t.alignment = TextAnchor.UpperLeft;
     t.horizontalOverflow = HorizontalWrapMode.Overflow;
     t.verticalOverflow = VerticalWrapMode.Overflow;
     Font ArialFont = (Font)Resources.GetBuiltinResource (typeof(Font), "Arial.ttf");
     t.font = ArialFont;
     t.fontSize = 20;
     t.enabled = true;
     t.color = Color.black;
     g.GetComponent<RectTransform>().SetParent(g.transform, true);
     g.transform.localScale = new Vector3(1.0f / this.transform.localScale.x * 0.1f,
                                          1.0f / this.transform.localScale.y * 0.1f, 
                                          1.0f / this.transform.localScale.z * 0.1f );
    }


     public void TrailStart(AutoPilot ap)
    {
    
    	trailRend = trails.AddComponent<TrailRenderer>();
	 	trailRend.material = new Material(Shader.Find("Particles/Additive"));
	 	gradient2.SetKeys(
            new GradientColorKey[] { 
            	new GradientColorKey(new Color(1F, 0.2F, 0.5F), 0), 
            	new GradientColorKey(new Color(0.3F, 0.3F, 0.3F), 1f)}, 
            new GradientAlphaKey[] { 
            	new GradientAlphaKey(1f, 1f),   
            	new GradientAlphaKey(0f, 1f) }
		);
	 	trailRend.colorGradient = gradient2;
	 	trailRend.alignment= LineAlignment.View; 
	 	trailRend.startWidth = 1f;
	 	trailRend.endWidth = 0.5f;
	 	trailRend.time =10f;
    
    }
    public void TrailUpdate(AutoPilot ap)
    {
      //Quaternion rot = Quaternion.Euler(ap.GetVelocity());
    	trails.transform.localPosition = ap.GetPosition();// + new Vector3(0,0,-10);
      //trails.transform.localRotation = rot;
    }



  

    public void CanvasMake(AutoPilot ap)
    {
    	float smoothFactor = 7.0f;
        int speed =  Convert.ToInt32(ap.GetSpeed());

        List<string> textlist= new List<string>();
        textlist.Add("|Menu|");
        textlist.Add("|Name : " + ap.GetMachineName() + "|");
        textlist.Add("|Speed : " + speed + "|");
        textlist.Add("|Lights : " + HeadLights + "|");
        textlist.Add(gameObject.name);
        // textlist.Add("HEEH");

        string textComp = "";
        for(int i=0;i<textlist.Count;i++)
        {
        	if (i!=0)
        	 textComp = textComp +"\n";
        	textComp = textComp + textlist[i];
        }
        t.text = textComp;

        Vector3 tester = new Vector3(50,Screen.height-100,-500);
        // Vector3 tester = ap.GetPosition()+new Vector3(0,20,0);
        g2.transform.position = tester;
        // g2.transform.LookAt(g2.transform.position + ap.GetCameraForward());
         g2.transform.LookAt(new Vector3(90,Screen.height-100,-400));
        // cs.scaleFactor = 1.0f +(Time.frameCount % 20)/10;
        t.fontSize = 11;

    }
  }
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;


public class MenuLib
{
	private AutoPilot ap;
	// public Dictionary<string, SubMenu> menu;
	public Dictionary<string,List<SubMenu>> menu;
	public string nowMenu;
	public int nowSubMenu;
	public int valueChanger;
	KeybindLib Key;


	public MenuLib(AutoPilot _ap)
	{
		this.ap = _ap;
		Key = new KeybindLib(_ap);
		nowMenu = "MainMenu";
		nowSubMenu = 0;
		valueChanger = 0;
		MenuContent();
	}

	public float GetValueNum(string str)
	{
		switch (str)
		{
			case "radarScale": 
				return menu["RadarMenu"][0].valueNow;
				break;
			case "radarX": 
				return menu["RadarMenu"][1].valueNow;
				break;
			case "radarY": 
				return menu["RadarMenu"][2].valueNow;
				break;
			default: 
				return 0;
			 	break;
		}
	}

	public bool GetValueBool(string str)
	{
		switch (str)
		{
			case "lockOnStat": 
				return menu["LockOnMenu"][0].boolValue;
				break;
			default: 
				return false;
			 	break;
		}
	}

	public void MenuContent()
	{
		string back;
		int numBack;
		this.menu = new Dictionary<string,List<SubMenu>>();
		this.menu.Add("MainMenu",new List<SubMenu>());
		menu["MainMenu"].Add(new SubMenu("Radar",0,0,"","RadarMenu"));
		menu["MainMenu"].Add(new SubMenu("Lock On",0,0,"","LockOnMenu"));
		menu["MainMenu"].Add(new SubMenu("menu3",0,0,"",""));

		this.menu.Add("Option",new List<SubMenu>());
		back = "MainMenu"; numBack = 2;
		menu["Option"].Add(new SubMenu("HUD",2,numBack,back,"",false));
			
	}

	public void MenuAdd(string menuName,List<SubMenu> subMenu)
	{
		this.menu.Add(menuName,subMenu);
	}

	public void MenuUpdate()
	{
		// ap.Print(0,"Menu : " + nowMenu);
		// ap.Print(1,"SubMenu :" + nowSubMenu);
		// ap.Print(2,"SubMenu :" + menu[nowMenu][nowSubMenu].menuName);
		// ap.Print(3,"SubMenu :" + menu[nowMenu].Count);
		// ap.Print(4,"numback :" + menu[nowMenu][nowSubMenu].subMenuNum);
		// ap.Print(5,"value :" + menu[nowMenu][nowSubMenu].valueNow);
		// ap.Print(6,"boolValue :" + menu[nowMenu][nowSubMenu].boolValue);

		if (valueChanger==1)
		{
			if (Key.PressedRepeat("Menu Right",menu[nowMenu][nowSubMenu].valueDelay))
			{
				var nsm = menu[nowMenu][nowSubMenu];
				if (nsm.valueNow < nsm.valueMax && (nsm.valueNow != nsm.valueMax || nsm.valueMax==9999))
				menu[nowMenu][nowSubMenu].valueNow = menu[nowMenu][nowSubMenu].valueNow + menu[nowMenu][nowSubMenu].increment;
				if (nsm.valueNow > nsm.valueMax && nsm.valueMax!=9999)
				menu[nowMenu][nowSubMenu].valueNow = menu[nowMenu][nowSubMenu].valueMax;
			}
			else if (Key.PressedRepeat("Menu Left",menu[nowMenu][nowSubMenu].valueDelay))
			{
				var nsm = menu[nowMenu][nowSubMenu];
				if (nsm.valueNow > nsm.valueMin && (nsm.valueNow != nsm.valueMin || nsm.valueMin==-9999))
				menu[nowMenu][nowSubMenu].valueNow = menu[nowMenu][nowSubMenu].valueNow - menu[nowMenu][nowSubMenu].increment;
				if (nsm.valueNow < nsm.valueMin && nsm.valueMin!=-9999)
				menu[nowMenu][nowSubMenu].valueNow = menu[nowMenu][nowSubMenu].valueMin;
			}
			else if (Key.PressedDown("Menu Down"))
			{
				valueChanger = 0;
			}
		}
		else
		{
			if (Key.PressedDown("Menu Right"))
			{
				if (nowSubMenu>=menu[nowMenu].Count-1)
					nowSubMenu=0;
				else
					nowSubMenu++;
			}
			else if (Key.PressedDown("Menu Left"))
			{
				if (nowSubMenu<=0)
					nowSubMenu=menu[nowMenu].Count-1;
				else
					nowSubMenu--;
			}
			else if (Key.PressedDown("Menu Up"))
			{
				if (menu[nowMenu][nowSubMenu].menuType == 2)
					menu[nowMenu][nowSubMenu].boolValue = !(menu[nowMenu][nowSubMenu].boolValue);
				else if (menu[nowMenu][nowSubMenu].menuType!=0)
					valueChanger = menu[nowMenu][nowSubMenu].menuType;
				else if (menu[nowMenu][nowSubMenu].nextMenu != "")
				{
					nowMenu = menu[nowMenu][nowSubMenu].nextMenu;
					nowSubMenu = 0;
				}
			}
			else if (Key.PressedDown("Menu Down"))
			{
				if (menu[nowMenu][nowSubMenu].prevMenu != "")
				{
					int temp = menu[nowMenu][nowSubMenu].subMenuNum;
					nowMenu = menu[nowMenu][nowSubMenu].prevMenu;
					nowSubMenu = temp;
				}
			}
		}
	}

	public class SubMenu
	{
		public string menuName;
		public int menuType;
		public string prevMenu;
		public string nextMenu;
		public int subMenuNum;

		public float valueNow;
		public float increment;
		public float valueMax;
		public float valueMin;	
		public float valueDelay;

		public bool boolValue;
 		

 		public SubMenu(string menuName,int menuType,int subMenuNum,string prev,string next)
		{
			this.menuName = menuName;
			this.menuType = menuType;
			this.subMenuNum = subMenuNum;
			this.prevMenu = prev;
			this.nextMenu = next;
		}

		public SubMenu(string menuName,int menuType,int subMenuNum,string prev,string next,bool boolValue)
		{
			this.menuName = menuName;
			this.menuType = menuType;
			this.subMenuNum = subMenuNum;
			this.prevMenu = prev;
			this.nextMenu = next;
			this.boolValue = boolValue;
		}

		public SubMenu(string menuName,int menuType,int subMenuNum,string prev,string next,float valueNow,float increment,float valueDelay,float valueMin = -9999 ,float valueMax = 9999)
		{
			this.menuName = menuName;
			this.menuType = menuType;
			this.subMenuNum = subMenuNum;
			this.prevMenu = prev;
			this.nextMenu = next;
			this.valueNow = valueNow;
			this.valueMax = valueMax;
			this.valueMin = valueMin;
			this.increment = increment;
			this.valueDelay = valueDelay;
		}
	}
}

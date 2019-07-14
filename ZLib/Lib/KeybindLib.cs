using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;


public class KeybindLib
{
	private AutoPilot ap;
	public Dictionary<string, KeyPress> keyBind;
	public KeybindLib(AutoPilot _ap)
	{
		this.ap = _ap;
		this.keyBind = new Dictionary<string,KeyPress>();
		this.keyBind.Add("Menu Up",new KeyPress(KeyCode.UpArrow));
		this.keyBind.Add("Menu Down", new KeyPress(KeyCode.DownArrow));
		this.keyBind.Add("Menu Left", new KeyPress(KeyCode.LeftArrow));
		this.keyBind.Add("Menu Right", new KeyPress(KeyCode.RightArrow));
		this.keyBind.Add("Lock On", new KeyPress(KeyCode.T));
		this.keyBind.Add("Draw Lines", new KeyPress(KeyCode.U));
		this.keyBind.Add("Reset LockOn", new KeyPress(KeyCode.F));
		this.keyBind.Add("Refresh LockOn", new KeyPress(KeyCode.Y));
	}

	public bool Pressed(string key,int isMenuDisabled = 0)
	{
		bool istrue = false;
		//isMenu Disable 0 = Anywhere, 1 = only ingame , 2 = only in menu
		if (isMenuDisabled == 0)
			goto CheckKey;
		else if (isMenuDisabled == 1)
		{
			if (ap.IsDriving())
				goto CheckKey;
		}
		else if (isMenuDisabled == 2)
		{
			if (!ap.IsDriving())
				goto CheckKey;
		}

		ReturnNow:
		return istrue;

		CheckKey:
			if (keyBind.ContainsKey(key))
				istrue = Input.GetKey(keyBind[key].KeyCode);
		goto ReturnNow;
	}

	public bool PressedDown(string key,int isMenuDisabled = 0)
	{
		bool istrue = false;

		if (isMenuDisabled == 0)
			goto CheckKey;
		else if (isMenuDisabled == 1)
		{
			if (ap.IsDriving())
				goto CheckKey;
		}
		else if (isMenuDisabled == 2)
		{
			if (!ap.IsDriving())
				goto CheckKey;
		}

		ReturnNow:
		return istrue;

		CheckKey:
		if (keyBind.ContainsKey(key))
			istrue = Input.GetKeyDown(keyBind[key].KeyCode);
		goto ReturnNow;
		
	}

	public bool PressedRepeat(string key,float repeatTime,int isMenuDisabled = 0)
	{
		bool istrue = false;

		if (isMenuDisabled == 0)
			goto CheckKey;
		else if (isMenuDisabled == 1)
		{
			if (ap.IsDriving())
				goto CheckKey;
		}
		else if (isMenuDisabled == 2)
		{
			if (!ap.IsDriving())
				goto CheckKey;
		}

		ReturnNow:
		return istrue;

		CheckKey:
		if (keyBind.ContainsKey(key))
		{
			if (Input.GetKeyDown(keyBind[key].KeyCode))
				keyBind[key].delayTime=repeatTime;
			if (keyBind[key].delayTime <=0)
			{
				istrue = Input.GetKey(keyBind[key].KeyCode);
				keyBind[key].delayTime=repeatTime;
			}
			else
				keyBind[key].delayTime=keyBind[key].delayTime-repeatTime;
		}
		goto ReturnNow;
	}


	public class KeyPress
	{
		public KeyCode KeyCode;
		public float delayTime;
		public int pressTime;
 
		public KeyPress(KeyCode key)
		{
			this.KeyCode = key;
			this.delayTime = 0;
		}
	}
}

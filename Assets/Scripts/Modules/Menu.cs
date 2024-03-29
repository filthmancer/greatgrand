﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Filthworks;

public class Menu : Module {

	public override UIQuote [] Intro_String
	{
		get{
			return new UIQuote[]
			{
				new UIQuote("Carer", "Welcome to " + GameManager.WorldRes.VillageName + " retirement village!",
									 "We pride ourselves on giving the best care and experience for Grands.",
									 "Why don't you take a look around?",
									 "It's just about dinner time, head to the dining hall to meet the residents!")
			};
		}
	}

	Face [] Faces;
	FIRL [] Frames;

	public override void InitUI()
	{
		MUI[1].ClearActions();
		MUI[1].AddAction(UIAction.MouseUp, () => 
		{
			StartCoroutine(GameManager.instance.LoadModule("Dinner"));
		});
		MUI[2].ClearActions();
		MUI[2].AddAction(UIAction.MouseUp, () => 
		{
			StartCoroutine(GameManager.instance.LoadModule("Bowls"));
		});
		menuobj = MUI["back"];
		TrayObj = MOB["notices"] as FIRL;
	}

	public RectTransform ScrollTrack;
	public UIObj menuobj;
	private FIRL TrayObj;
	private int lastgrumps = 0, lastsmiles = 0;
	public override void ControlledUpdate()
	{
		if(!GameManager.Paused && !GameManager.IgnoreInput)
		{
			Vector2 v = GameManager._Input.GetScroll()/17;
			v.x = 0.0F;
			Vector3 newpos = MOB.pos + new Vector3(0.0F, v.y, 0.0F);
			newpos.y = Mathf.Clamp(newpos.y, MOB["min"].pos.y, MOB["max"].pos.y);

			MOB.T.position = newpos;
		}

		if(ginfo != null && ginfo_target != null)
		{
			GameManager.UI.SetGrandInfoObj(ginfo, ginfo_target);
			Vector3 pos = new Vector3(this.transform.position.x, ginfo_face.transform.position.y - 10, 0.0F);
			ginfo.SetUIPositionFromWorld(pos);
			//DOTween.To(()=> lastgrumps, x => lastgrumps = x, ginfo_target.Data.Grumps.Value, 0.3F);
			//DOTween.To(()=> lastsmiles, x => lastsmiles = x, ginfo_target.Data.Smiles.Value, 0.3F);
			if(lastsmiles < ginfo_target.Smiles.Value) lastsmiles++;
			if(lastgrumps < ginfo_target.Grumps.Value) lastgrumps++;
			ginfo[1][3].Txt[0].text = lastsmiles + "";
			ginfo[1][3].Txt[1].text = lastgrumps + "";
		}
		
		
		if(GameManager.instance.AlertsTotal > 0)
		{
			GameManager.Data.Alert_Pigeonhole = true;
			Tweens.Bounce(TrayObj.Text[1].transform);
			TrayObj.Text[1].text = "" + GameManager.instance.AlertsTotal;
		}
		else TrayObj.Text[1].text = "";
	}

	public override IEnumerator Load()
	{
		GameManager.instance.CheckPopulation();
        int framenum = 10;//GameManager.WorldRes.Population;
		Faces = new Face[framenum];
		Frames = new FIRL[framenum];

		List<GrandData> allgrands = new List<GrandData>();
		allgrands.AddRange(GameManager.instance.Grands);

		for(int i = 0; i < Faces.Length; i++)
		{
			if(allgrands.Count <= 0) continue;
			int num = Random.Range(0, allgrands.Count);

			GrandData f = allgrands[num];
			if(f == null) continue;

			Frames[i] = Instantiate(GameManager.Data.RandomFrame());
			Frames[i].transform.SetParent(MOB[0].transform);
			Frames[i].transform.position = MOB[0][i].transform.position;
			Frames[i].transform.localScale = Vector3.one*0.15F;

			Faces[i] = GameManager.Generator.GenerateNewFace(f);
			Frames[i].Child[0].AddChild(Faces[i]);
			Faces[i].transform.localPosition = Vector3.zero;
			Frames[i].Text[0].text = f.Info.Name;
			SetupFace(Faces[i], Frames[i], f);
			yield return null;
		}

		GameManager.instance.InitTimeChecks();

		
		TrayObj.ClearActions();
		TrayObj.AddAction(TouchAction.Up, () =>
		{
			if(!GameManager.Paused)
			{
				StartCoroutine(GameManager.instance.ShowAlerts());
			Tweens.Bounce(MOB["notices"].transform);
			}
			
		});

		MOB["dinner"].AddAction(TouchAction.Up, ()=> 
		{
			StartCoroutine(GameManager.instance.LoadModule("Dinner"));
			Tweens.Bounce(MOB["dinner"].transform);
		});

		MOB["garden"].AddAction(TouchAction.Up, ()=> 
		{
			StartCoroutine(GameManager.instance.LoadModule("bowls"));
			Tweens.Bounce(MOB["garden"].transform);
		});
	
	}

	private UIObj ginfo;
	private GrandData ginfo_target;
	private Face ginfo_face;

	public void SetupFace(Face f, FIRL frame, GrandData g)
	{
		f.AddAction(TouchAction.Up, ()=>
		{
			if(ginfo_target != g)
			{
				if(ginfo != null) ginfo.PoolDestroy();
				ginfo_face = f;
				ginfo_target = g;
				ginfo = GameManager.UI.GrandInfo(g);
				Vector3 pos = new Vector3(this.transform.position.x, f.transform.position.y - 10, 0.0F);
				ginfo.SetUIPositionFromWorld(pos);
				ginfo.FitOnScreen();
				ginfo.SetActive(false);
				ginfo.TweenActive(true);

				lastsmiles = g.Smiles.Value;
				lastgrumps = g.Grumps.Value;
				Tweens.Bounce(f.transform);
			}
			else if(ginfo != null) 
			{
				ginfo.PoolDestroy();
				ginfo_target = null;
			}
		});
	}

	public override Sequence OpeningSequence(IntVector v)
	{
		Transform start = GameManager.UI.ModuleRight;
		Transform end = GameManager.UI.ModuleTarget;

		if(v.x == 1) start = GameManager.UI.ModuleRight;
		else if(v.x == -1) start = GameManager.UI.ModuleLeft;
		else start = GameManager.UI.ModuleRight;

		MOB.transform.position = start.position;

		Sequence s = Tweens.SwoopTo(MOB.transform, end.position);

		for(int i = 0; i < Frames.Length; i++)
		{
			if(Frames[i] == null) continue;
			s.Insert(0.4F, Tweens.PictureSway(Frames[i].transform, new Vector3(0,0,12 * v.x)));
		}

		return s;
	}

	public override void Clear()
	{
		for(int i = 0; i < Frames.Length; i++)
		{
			GameObject.Destroy(Frames[i].gameObject);
			Destroy(Faces[i].gameObject);
		}
	}

	
}

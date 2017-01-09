﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;

public class GameManager : MonoBehaviour {
	public static GameManager instance;

	public GameObject DinnerGame;
	public static TableManager Table;
	public TableManager _TableManager;
	public static UIManager UI;
	public UIManager _UIManager;

	public static Transform GetCanvas()
	{
		if(UI != null) return UI.Canvas.transform;
		else return GameObject.Find("Canvas").transform;
	}
	public static UIObj GetFaceParent()
	{
		if(UI != null) return UI.FaceParent;
		else return GameObject.Find("FaceParent").GetComponent<UIObj>();
	}

	public static GameData Data;

	public InputController _Input;
	public GreatGrand [] GG;

	public static int GG_num = 8;

	public Generator GGGen;
	public VectorObject2D GrumpLine;

	public bool loadDinner = false;

	public bool Resolved
	{
		get{
			for(int i = 0; i < GG.Length; i++)
			{
				if(GG[i]==null) continue;
				if(!GG[i].IsHappy) return false;
			}
			return true;
		}
	}

	public int NumberHappy
	{
		get{
			int num = 0;
			for(int i = 0; i < GG.Length; i++)
			{
				if(GG[i].IsHappy) num++;
			}
			return num;
		}
	}

	public bool AllSeated
	{
		get{
			for(int i = 0; i < GG.Length; i++)
			{
				if(!GG[i].isSeated) return false;
			}
			return true;
		}
	}

	void Awake(){
		instance = this;
		Table = _TableManager;
		UI = _UIManager;
	}

	bool gameStart = false;
	// Use this for initialization
	void Start () {
		Data = this.GetComponent<GameData>();
		Table.Init();
		_Input.Init();
		GGGen.LoadElements();

		UI.Init();

		if(loadDinner) LoadMinigame("Dinner");
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyUp(KeyCode.W)) GGGen.GenerateFace(GG[0]);
	}


	public void Clear()
	{
		for(int i = 0; i < GG.Length; i++)
		{
			GG[i].Destroy();
		}
	}
	public void LoadMinigame(string n)
	{
		if(n == "Dinner")
		{
			 CreateDinnerGame();
			 UI.Menu.SetActive(false);
			 UI.DinnerUI.SetActive(true);
			 DinnerGame.SetActive(true);
		}
	}


	public void CreateDinnerGame()
	{
		gameStart = false;
		GG = new GreatGrand[GG_num];
		for(int i = 0; i < GG_num; i++)
		{
			GG[i] = GGGen.Generate(i);//(GreatGrand) Instantiate(GGObj);
			GGGen.GenerateFace(GG[i]);
			GG[i].transform.SetParent(this.transform);
			//GG[i].Face.SetActive(false);
		}


		for(int i = 0; i < GG_num; i+=2)
		{
			MaritalStatus m = Random.value > 0.55F ? MaritalStatus.Married : (Random.value < 0.9F ? MaritalStatus.Divorced : MaritalStatus.Donor);
			GG[i].Info.MStat = m;
			GG[i+1].Info.MStat = m;
			GG[i].Relation = GG[i+1];
			GG[i+1].Relation = GG[i];
		}

		for(int i = 0; i < GG_num; i++)
		{
			//GG[i].Generate(i);
			GG[i].SitImmediate(Table.Seat[i]);
		}

		for(int i = 0; i < GG_num; i++)
		{
			//GenerateGrumpsPrimitive(GG[i], 2);
			GenerateGrumpsReal(GG[i], 1);

		}
		int [] ind = new int[0];
		while(NumberHappy > 3) ind = ShuffleGG();

		for(int i = 0; i < GG_num; i++)
		{
			GG[i].CreateGrumpLines();
		}

		StartCoroutine(StartDinnerGame(ind));

		
	}

	IEnumerator StartDinnerGame(int [] index)
	{
		UI.ShowEndGame(false);
		for(int i = 0; i < GG.Length; i++)
		{
			GG[i].Face.transform.position = Table.Door.position;
		}

		for(int i = 0; i < GG.Length; i++)
		{
			StartCoroutine(Table.DoorToSeat(GG[i], index[i], 0.7F));
			yield return null;
		}

		while(!AllSeated) yield return null;

		gameStart = true;
		
		yield return null;
	}

	public void EndGame()
	{
		gameStart = false;
		UI.ShowEndGame(true);
	}

	public void DestroyGame()
	{
		for(int i = 0; i < GG.Length; i++)
		{
			Destroy(GG[i].gameObject);
		}
		Clear();
		Table.Clear();
		CreateDinnerGame();

	}

	public static void OnTouch()
	{

	}

	public static void OnRelease()
	{
		Table.Reset();
	}

	VectorLine TLine;
	[SerializeField]
	private Color TLineColor;
	public void TargetLine(Vector3 from, Vector3 to)
	{
		if(TLine == null)
		{
			TLine = new VectorLine("Targeter", new List<Vector3>(), 7.0F, LineType.Continuous);
			TLine.points3.Add(from);
			TLine.points3.Add(to);
			TLine.SetColor(TLineColor);
		}
		TLine.points3[0] = from;
		TLine.points3[1] = to;
		TLine.Draw();
	}

	public void CheckGrumps()
	{
		for(int i = 0; i < GG.Length; i++)
		{
			if(GG[i] == null) continue;
			GG[i].CheckEmotion();
		}

		if(Resolved && gameStart) EndGame();
	}

	public void GenerateGrumpsPrimitive(GreatGrand g, int num)
	{
		g.Grumps = new _Grump[num];
		for(int i = 0; i < num; i++)
		{
			GrumpObj targ = GetRandomGG(g);
			_Grump newg = new _Grump(false, g, targ);
			g.Grumps[i] = newg;
		}
	}

	public void GenerateGrumpsReal(GreatGrand g, int num)
	{
		_Grump [] final = new _Grump[num];
		for(int i = 0; i < num; i++)
		{
			bool has_targ = false;

			GrumpObj targ = null;

			while(!has_targ)
			{
				targ = GetRandomGG(g);
				has_targ = true;

				for(int x = 0; x < i; x++)
				{
					if(final[x].Target == targ) has_targ = false;
				}
			}
			int dist = Table.SeatDistance(targ as GreatGrand, g);
			bool like = false;
			if(dist == 1) like = true;
			

			final[i] = new _Grump(like, g, targ);	
		}
		g.Grumps = final;	
	}

	public int [] ShuffleGG()
	{	
		int [] fin = new int[Table.Seat.Length];
		List<_Seat> finalpos = new List<_Seat>();
		finalpos.AddRange(Table.Seat);

		for(int i = 0; i < GG_num; i++)
		{
			int num = Random.Range(0, finalpos.Count);
			_Seat point = finalpos[num];
			if(point == GG[i].Seat)
			{
				num = Random.Range(0, finalpos.Count);
				point = finalpos[num];
			}

			fin[i] = finalpos[num].Index;
			GG[i].SitImmediate(point);
			finalpos.RemoveAt(num);

		}

		return fin;
	}

	public GreatGrand GetRandomGG(GreatGrand g)
	{
		GreatGrand final = GG[Random.Range(0, GG.Length)];
		while(final == g) final = GG[Random.Range(0, GG.Length)];
		return final;
	}

	public GreatGrand GetNonNeighbourGG(GreatGrand g)
	{
		return Table.GetNonNeighbourSeat(g.Seat).Target;
	}

	public void FocusOn(InputTarget t)
	{
		if(t is GreatGrand) UI.SetGrandUI(t as GreatGrand);
	}
}

[System.Serializable]
public class _Grump
{
	public bool LikesIt;
	public GreatGrand Parent;
	public GrumpObj Target;

	public _Grump(bool like, GreatGrand p,  GrumpObj t= null)
	{
		Parent = p;
		Target = t;
		LikesIt = like;
	}

	public bool Resolved
	{
		get
		{
			int dist = GameManager.Table.SeatDistance(Parent,Target);
			if(LikesIt && dist == 1) return true;
			if(!LikesIt && dist > 1) return true;
			return false;
		}
		
	}
}


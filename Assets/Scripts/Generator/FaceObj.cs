﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public enum ColorType
{
	Skin, Hair, Offset, Feature, Nose, Ears, Jaw, None
}

public class FaceObj : UIObj {

	public FaceObj FaceParent;
	public UIObj AlertParent;

    public ColorType Colour = ColorType.Skin;
	private RectTransform _anchorpoint;
	private FaceObj [] _obj;
	private RectTransform trans;

	public List<UIObj> Alerts = new List<UIObj>();

	public List<AnimTrigger> Anims = new List<AnimTrigger>();
	
	public FaceInfo Info;

	public void CheckAnims()
	{
		if(Anims != null)
		{
			for(int i = 0; i < Anims.Count; i++)
			{
				if(Anims[i].Update()) 
				{
					Anims[i].Trigger();
				}
					
			}
		}
		if(Child != null)
		{
			for(int i = 0; i < Child.Length; i++)
			{
				if(Child[i] is FaceObj) (Child[i] as FaceObj).CheckAnims();
			}
		}
	}

	public void CreateAnchor(int num, FaceObj o)
	{
		if(_obj == null || _obj.Length == 0) _obj = new FaceObj[Child.Length];
		_obj[num] = (FaceObj) Instantiate(o);
		_obj[num].SetAnchorPoint(Child[num] as FaceObj);
	}

	public void SetAnchorPoint(FaceObj t)
	{
		SetParent(t);

		_anchorpoint = t.transform as RectTransform;
	}

	public FaceObj GetObj(int num) {return _obj[num];}

	public void Setup(FaceObj p, FaceInfo f = null, bool side = true)
	{
		FaceParent = p;
		Reset(f, side);
	}

	public void SetupObj(FaceObj p, FaceObjInfoContainer f = null, bool side =true)
	{
		FaceParent = p;
		GetObjInfo(f, side);
	}

	private FaceObj Element;

	public void GetObjInfo(FaceObjInfoContainer f = null, bool side = true)
	{
		if(f!=null)
		{
			if(Element == null || Index != f.Current.Index)
			{
				if(Element != null) DestroyImmediate(Element.gameObject);
				Element = Instantiate(f.Current.Prefab).GetComponent<FaceObj>();
				Element.transform.SetParent(this.transform);
				//Element.GetComponent<RectTransform>().anchorMax = Vector3.one;
				//Element.GetComponent<RectTransform>().sizeDelta = Vector3.zero;
				Child = new UIObj[1];
				Child[0] = Element;
				Index = f.Current.Index;
			}
			//Info = f;

			//SetInfo(f.Current, null, side);
		}
	}

	public void SetInfo(FaceInfo f, GameObject pref = null, bool side = true)
	{
		Info = f;
		if(Element == null || Index != f.Index)
		{
			if(Element != null) DestroyImmediate(Element.gameObject);
			Element = Instantiate(pref).GetComponent<FaceObj>();
			
			Element.transform.SetParent(this.transform);
			Element.GetComponent<RectTransform>().anchorMin = Vector3.zero;
			Element.GetComponent<RectTransform>().anchorMax = Vector3.one;
			Element.GetComponent<RectTransform>().sizeDelta = Vector3.zero;
			Child = new UIObj[1];
			Child[0] = Element;
			Index = f.Index;
		}

		Rect ele_rect = this.GetComponent<RectTransform>().rect;
		Vector3 randomised_pos = Vector3.zero;
		randomised_pos.x = Mathf.Lerp(ele_rect.xMin, ele_rect.xMax, Element.GetComponent<RectTransform>().pivot.x + Info._Position.x);
		randomised_pos.y =  Mathf.Lerp(ele_rect.yMin, ele_rect.yMax, Element.GetComponent<RectTransform>().pivot.y + Info._Position.y);

		Element.transform.localPosition = randomised_pos;
		Element.transform.localRotation = Quaternion.Euler(Info._Rotation);
		Element.transform.localScale = Info._Scale;
	
			if(Info.Symm)
			{		
				//Element.transform.position += Vector3.right * Info.Symm_Distance * (side ? 1 : -1);
				//Element.transform.localScale += Vector3.up * Info.Symm_ScaleDiff * (side ? 1 : -1);
			}
	
			if(FaceParent != null && Info != null)
			{
				Color final = Color.white;
	
				switch(Info.Colour)
				{
					case ColorType.Skin:
						final = FaceParent.SkinCol;
					break;
					case ColorType.Hair:
						final = FaceParent.HairCol;
					break;
					case ColorType.Offset:
						final = FaceParent.OffsetCol;
					break;
					case ColorType.Nose:
						final = FaceParent.NoseCol;
					break;
					case ColorType.Feature:
						final = Color.black;
					break;
				}
				if( Element.Svg.Length > 0) 
				{
					Element.Svg[0].color = final;
				}
	
				for(int i = 0; i < Child.Length; i++)
				{
					if(Child[i] is FaceObj)
					{
						(Child[i] as FaceObj).FaceParent = FaceParent;
					}
				}
			}
	}

	public void ResetAll()
	{
		Reset();
		for(int i = 0; i < Child.Length; i++)
		{
			if(Child[i] is FaceObj)
			{
				(Child[i] as FaceObj).ResetAll();
			}
		}
	}

	public void Scale(Vector3 sc)
	{
		transform.localScale = sc;
		StateWhenPressed.Scale = sc * 1.1F;
	}

	public void Reset(FaceInfo f = null, bool side = true)
	{
		if(f != null) 
		{
			Info = f;
		}

		transform.localPosition = Info._Position;
		transform.rotation = Quaternion.Euler(Info._Rotation);
		transform.localScale = Info._Scale;

		if(Info.Symm)
		{		
			transform.position += Vector3.right * Info.Symm_Distance * (side ? 1 : -1);
			transform.localScale += Vector3.up * Info.Symm_ScaleDiff * (side ? 1 : -1);
		}

		if(FaceParent != null && Info != null)
		{
			Color final = Color.white;

			switch(Info.Colour)
			{
				case ColorType.Skin:
					final = FaceParent.SkinCol;
				break;
				case ColorType.Hair:
					final = FaceParent.HairCol;
				break;
				case ColorType.Offset:
					final = FaceParent.OffsetCol;
				break;
				case ColorType.Feature:
					final = Color.black;
				break;
			}
			if( Svg.Length >0) 
			{
				Svg[0].color = final;
			}

			for(int i = 0; i < Child.Length; i++)
			{
				if(Child[i] is FaceObj)
				{
					(Child[i] as FaceObj).FaceParent = FaceParent;
				}
			}
		}
	}

	private Color	_skincol;
	public Color SkinCol
	{get{return _skincol;}}
	public void SetSkinColor(Color c)
	{
		_skincol = c;
	}

	private Color	_haircol;
	public Color HairCol
	{get{return _haircol;}}
	public void SetHairColor(Color c)
	{
		_haircol = c;
	}

	private Color _offsetcol;
	public Color OffsetCol
	{
		get{return _offsetcol;}
	}
	public void SetOffsetColor(Color c)
	{
		_offsetcol = c;
	}

	private Color _nosecol;
	public Color NoseCol
	{
		get{return _nosecol;}
	}
	public void SetNoseColor(Color c)
	{
		_nosecol = c;
	}

	public override void SetParent(UIObj p)
	{
		ParentObj = p;
		transform.SetParent(p.transform);
		p.AddChild(this);

		if(!trans) trans = this.transform as RectTransform;

		trans.anchorMax = Vector2.one;//_anchorpoint.anchorMax;
		trans.anchorMin = Vector2.zero;//_anchorpoint.anchorMin;
		trans.anchoredPosition = Vector2.zero;//_anchorpoint.anchoredPosition;
		trans.sizeDelta = Vector2.zero;//_anchorpoint.sizeDelta;

		if(p is FaceObj)
		{
			FaceObj f = p as FaceObj;
			if(f.FaceParent) 
			{
				SetFaceParent(f.FaceParent);
			}
		}
	}

	public void SetFaceParent(FaceObj f)
	{
		FaceParent = f;
		Reset();
	}

	public void AddAnimTrigger(string title, Vector2 times, params Animator [] anims)
	{
		AnimTrigger a = new AnimTrigger(title, times, anims);
		Anims.Add(a);
	}

}
	
[System.Serializable]
public class AnimTrigger
{
	public string Title;
	public Vector2 TimeConstants;
	public float TimeActual, Current;
	public Animator [] Anims;

	public AnimTrigger(string t, Vector2 c, params Animator [] a)
	{
		Title = t;
		TimeConstants = c;
		Anims = a;
		Create();

	}
	public void Create()
	{
		TimeActual = Random.Range(TimeConstants.x, TimeConstants.y);
	}

	public bool Update()
	{
		Current += Time.deltaTime;
		if(Current > TimeActual)
		{
			Create();
			Current = 0.0F;
			return true;
		}
		return false;
	}

	public void Trigger()
	{
		for(int i = 0; i < Anims.Length; i++)
		{
			Anims[i].SetTrigger(Title);
		}
	}
}

[System.Serializable]
public class FaceObjInfoContainer
{
	private int _index = 0;
	public int Index{
		get{return _index;}
		set{_index = value;
			if(_index < 0) _index = Objs.Length-1; 
			else if(_index > Objs.Length-1) _index = 0;
			}
	}

	public GameObject [] Male, Female;
	public FaceObjInfo [] Objs;
	public FaceObjInfo this[int num]{get{
		num = Mathf.Clamp(num, 0, Objs.Length-1);
		return Objs[num];
		}}
	public int Length{get{return Objs.Length;}}
	public FaceObjInfo Current{get{return Objs[Index];}}
	public FaceObjInfoContainer(FaceObjInfo [] f)
	{
		Objs = f;
		_index = 0;
	}

	public FaceInfo GetCurrent()
	{
		FaceInfo f = new FaceInfo(Index, Colour);
		f._Position = _Position;
		f._Rotation = _Rotation;
		f._Scale = _Scale;
		return f;
	}

	public Vector3 _Position;
	public Vector3 _Rotation;
	public Vector3 _Scale = Vector3.one;
	public ColorType Colour = ColorType.Skin;

	public bool Symm;
	public float Symm_Distance;
	public float Symm_ScaleDiff;

	public void RandomIndex()
	{
		_index = Random.Range(0, Objs.Length);
	}

	public FaceInfo Randomise(Simple2x2 pos, Simple2x2 rot, Simple2x2 sc)
	{
		RandomIndex();
		FaceInfo final = new FaceInfo(Index, Colour);

		Vector3 p = new Vector3(pos.RandX(), 0.0F, pos.RandY());
		final._Position = p;

		Vector3 r = new Vector3(0.0F, 0.0F, rot.RandX());
		final._Rotation = r;

		Vector2 s = Utility.RandomMatrixPoint(sc);
		final._Scale = Vector3.one + new Vector3(s.x, s.y, 0.0F);

		return final;
	}

	public FaceInfo Randomise(Vector3 pos, float rot, Vector3 sc)
	{
		RandomIndex();
		FaceInfo final = new FaceInfo(Index, Colour);
		final.Randomise(pos, rot, sc);
		return final;
	}

}

[System.Serializable]
public class FaceObjInfo
{
	public int Index;
	public GameObject Prefab;
	public Sprite _Sprite;

	public Vector3 _Position = Vector3.zero;
	public Vector3 _Rotation = Vector3.zero;
	public Vector3 _Scale = Vector3.one;
	public ColorType Colour = ColorType.Skin;

	public bool Symm;
	public float Symm_Distance;
	public float Symm_ScaleDiff;

	public FaceObjInfo(int ind, Sprite s)
	{
		Index = ind;
		_Sprite = s;
	}

	public FaceObjInfo(int ind, GameObject s)
	{
		Index = ind;
		Prefab = s;
	}

	public FaceObjInfo(FaceObjInfo old)
	{
		Index = old.Index;
		Prefab = old.Prefab;
		_Sprite = old._Sprite;
		Colour = old.Colour;
	}


	public string Name{get{if(_Sprite) return _Sprite.name;
							if(Prefab) return Prefab.name;
							return "";}}

	public void Randomise(Vector3 pos, float rot, Vector3 sc)
	{

		_Position = Utility.RandomVectorInclusive(pos.x, pos.y);
		_Rotation = Utility.RandomVectorInclusive(0.0F, 0.0F, rot);
		_Scale = Vector3.one + Utility.RandomVectorInclusive(sc.x, sc.y);
	}


	public FaceObjInfo Clone()
	{
		FaceObjInfo fin = new FaceObjInfo(this);
		fin._Position = _Position;
		fin._Rotation = _Rotation;
		fin._Scale = _Scale;
		return fin;
	}
}

public class ValueContainer
{
	public float Value;
	public string Name;
	public Vector2 Field;
	public ValueContainer(string n, float v = 1.0F)
	{
		Value = v;
		Name = n;
		Field = new Vector2(0.0F, 1.0F);
	}
}

using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace COM3D2.GripMovePlugin.Plugin;

internal class SkirtIKTool : MonoBehaviour
{
	private static FieldInfo f_hair1list = typeof(TBoneHair_).GetField("hair1list",
		BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);

	private static FieldInfo f_skirtList = typeof(TBoneHair_).GetField("SkirtList",
		BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);

	private static readonly FieldInfo f_skirtListDBL = typeof(TBoneHair_).GetField("SkirtListDBL",
		BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);

	private Material skirtIKMaterial;

	private void Awake()
	{
		skirtIKMaterial = new Material(MaterialHelper.GetColorZOrderShader());
		Color color;
		color = new Color(1f, 0f, 0f, 0.5f);
		skirtIKMaterial.SetColor("_Color", color);
	}

	private void Update()
	{
		if (Input.GetKey((KeyCode)304) && Input.GetKeyDown((KeyCode)283))
		{
			UninstallSkirtIK();
		}
		else if (!Input.GetKey((KeyCode)304) && Input.GetKeyDown((KeyCode)283))
		{
			InstallSkirtIK();
		}

		UpdateVTGT();
	}

	private void InstallSkirtIK()
	{
		Maid maid = GameMain.Instance.CharacterMgr.GetMaid(0);
		if (maid != null)
		{
			Console.WriteLine("Install SkirtIK into " + maid.name);
			TBodySkin tbodySkin = maid.body0.GetSlot(8);
			if (!tbodySkin.boVisible)
			{
				tbodySkin = maid.body0.GetSlot(9);
			}

			if (!tbodySkin.boVisible)
			{
				return;
			}

			if (tbodySkin.bonehair.boSkirt)
			{
				TBoneHair_ bonehair = tbodySkin.bonehair;
				var array = (THair1[])f_skirtListDBL.GetValue(bonehair);
				for (int i = 0; i < array.Length; i += 2)
				{
					THair1 thair = array[i];
					THair1 thair2 = array[i + 1];
					THp thp = thair.hplist[thair.hplist.Count() - 1];
					var gameObject = GameObject.CreatePrimitive(0);
					gameObject.name = "_SkirtIKMarker";
					gameObject.GetComponent<Renderer>().sharedMaterial = skirtIKMaterial;
					gameObject.transform.localScale = Vector3.one * 0.05f;
					gameObject.transform.position = thp.t.position;
					gameObject.transform.rotation = thp.t.rotation;
					gameObject.transform.parent = thp.t;
					gameObject.AddComponent<MoveableGUIObject>();
				}
			}
		}
	}

	private void UninstallSkirtIK()
	{
	}

	private void UpdateVTGT()
	{
		Maid maid = GameMain.Instance.CharacterMgr.GetMaid(0);
		if (maid != null)
		{
			TBodySkin tbodySkin = maid.body0.GetSlot(8);
			if (!tbodySkin.boVisible)
			{
				tbodySkin = maid.body0.GetSlot(9);
			}

			if (!tbodySkin.boVisible)
			{
				return;
			}

			if (tbodySkin.bonehair.boSkirt)
			{
				TBoneHair_ bonehair = tbodySkin.bonehair;
				var array = (THair1[])f_skirtListDBL.GetValue(bonehair);
				for (int i = 0; i < array.Length; i += 2)
				{
					THair1 thair = array[i];
					THair1 thair2 = array[i + 1];
					THair1 thair3 = array[(i + 23) % 24];
					THp thp = thair.hplist[thair.hplist.Count() - 1];
					THp thp2 = thair.hplist[thair.hplist.Count() - 2];
					THp thp3 = thair2.hplist[thair2.hplist.Count() - 1];
					THp thp4 = thair2.hplist[thair2.hplist.Count() - 2];
					THp thp5 = thair3.hplist[thair3.hplist.Count() - 1];
					THp thp6 = thair3.hplist[thair3.hplist.Count() - 2];
					Transform transform = CMT.SearchObjName(thp.t, "_SkirtIKMarker");
					if (transform != null)
					{
						thp.vTGT = transform.position;
						thp2.vTGT = transform.position;
						thp3.vTGT = transform.position;
						thp4.vTGT = transform.position;
						thp5.vTGT = transform.position;
						thp6.vTGT = transform.position;
					}
				}
			}
		}
	}
}

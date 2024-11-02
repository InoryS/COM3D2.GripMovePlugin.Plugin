using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x02000028 RID: 40
	internal class SkirtIKTool : MonoBehaviour
	{
		// Token: 0x06000141 RID: 321 RVA: 0x000085D4 File Offset: 0x000067D4
		private void Awake()
		{
			this.skirtIKMaterial = new Material(MaterialHelper.GetColorZOrderShader());
			Color color = new Color(1f, 0f, 0f, 0.5f);
			this.skirtIKMaterial.SetColor("_Color", color);
		}

		// Token: 0x06000142 RID: 322 RVA: 0x00008620 File Offset: 0x00006820
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
					THair1[] array = (THair1[])SkirtIKTool.f_skirtListDBL.GetValue(bonehair);
					for (int i = 0; i < array.Length; i += 2)
					{
						THair1 thair = array[i];
						THair1 thair2 = array[i + 1];
						THp thp = thair.hplist[thair.hplist.Count<THp>() - 1];
						GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
						gameObject.name = "_SkirtIKMarker";
						gameObject.GetComponent<Renderer>().sharedMaterial = this.skirtIKMaterial;
						gameObject.transform.localScale = Vector3.one * 0.05f;
						gameObject.transform.position = thp.t.position;
						gameObject.transform.rotation = thp.t.rotation;
						gameObject.transform.parent = thp.t;
						gameObject.AddComponent<MoveableGUIObject>();
					}
				}
			}
		}

		// Token: 0x06000143 RID: 323 RVA: 0x000024BB File Offset: 0x000006BB
		private void UninstallSkirtIK()
		{
		}

		// Token: 0x06000144 RID: 324 RVA: 0x00008778 File Offset: 0x00006978
		private void Update()
		{
			if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F2))
			{
				this.UninstallSkirtIK();
			}
			else if (!Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F2))
			{
				this.InstallSkirtIK();
			}
			this.UpdateVTGT();
		}

		// Token: 0x06000145 RID: 325 RVA: 0x000087CC File Offset: 0x000069CC
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
					THair1[] array = (THair1[])SkirtIKTool.f_skirtListDBL.GetValue(bonehair);
					for (int i = 0; i < array.Length; i += 2)
					{
						THair1 thair = array[i];
						THair1 thair2 = array[i + 1];
						THair1 thair3 = array[(i + 23) % 24];
						THp thp = thair.hplist[thair.hplist.Count<THp>() - 1];
						THp thp2 = thair.hplist[thair.hplist.Count<THp>() - 2];
						THp thp3 = thair2.hplist[thair2.hplist.Count<THp>() - 1];
						THp thp4 = thair2.hplist[thair2.hplist.Count<THp>() - 2];
						THp thp5 = thair3.hplist[thair3.hplist.Count<THp>() - 1];
						THp thp6 = thair3.hplist[thair3.hplist.Count<THp>() - 2];
						Transform transform = CMT.SearchObjName(thp.t, "_SkirtIKMarker", true);
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

		// Token: 0x040000F0 RID: 240
		private Material skirtIKMaterial;

		// Token: 0x040000F1 RID: 241
		private static FieldInfo f_hair1list = typeof(TBoneHair_).GetField("hair1list", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);

		// Token: 0x040000F2 RID: 242
		private static FieldInfo f_skirtList = typeof(TBoneHair_).GetField("SkirtList", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);

		// Token: 0x040000F3 RID: 243
		private static FieldInfo f_skirtListDBL = typeof(TBoneHair_).GetField("SkirtListDBL", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
	}
}

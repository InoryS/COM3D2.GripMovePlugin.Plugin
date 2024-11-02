using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x02000040 RID: 64
	public class IKTool : MonoBehaviour
	{
		// Token: 0x0600025A RID: 602 RVA: 0x0000F4B0 File Offset: 0x0000D6B0
		public void ApplyLoadedIKNodes()
		{
			if (this.maidBones == null || this.maidBones.Count == 0)
			{
				this.VisibleIKNodes();
			}
			if (this.maidLoadIKPos != null && this.maidLoadIKRot != null)
			{
				foreach (string text in this.maidLoadIKPos.Keys)
				{
					Maid maid = this.FindMaidByGUID(text);
					if (maid != null && (!(this.poseEditWindow != null) || bool.Parse(this.poseEditWindow.GetMaidStoreData(maid)["use"])))
					{
						Dictionary<string, Transform> dictionary = this.maidBones[maid];
						Dictionary<string, Vector3> dictionary2 = this.maidLoadIKPos[text];
						Dictionary<string, Quaternion> dictionary3 = this.maidLoadIKRot[text];
						foreach (string text2 in dictionary2.Keys)
						{
							Vector3 vector = dictionary2[text2];
							Quaternion quaternion = dictionary3[text2];
							if (dictionary.ContainsKey(text2))
							{
								Transform transform = dictionary[text2];
								transform.localPosition = vector;
								transform.localRotation = quaternion;
								if (this.maidBoneLastPos[maid] != null)
								{
									this.maidBoneLastPos[maid][transform.name] = vector;
									this.maidBoneLastRot[maid][transform.name] = quaternion;
								}
							}
							else
							{
								Console.WriteLine("Bone: " + text2 + " not found. ignore.");
							}
						}
					}
				}
			}
			this.maidLoadIKPos = new Dictionary<string, Dictionary<string, Vector3>>();
			this.maidLoadIKRot = new Dictionary<string, Dictionary<string, Quaternion>>();
		}

		// Token: 0x0600025B RID: 603 RVA: 0x000024BB File Offset: 0x000006BB
		private void Awake()
		{
		}

		// Token: 0x0600025C RID: 604 RVA: 0x0000F6AC File Offset: 0x0000D8AC
		private void CleanupIKXML(XElement xml)
		{
			List<XElement> list = new List<XElement>();
			list.AddRange(xml.Elements());
			foreach (XElement xelement in list)
			{
				if (xelement.Name == "Type" && xelement.Attribute("name").Value == "X_IK_Transforms")
				{
					xelement.Remove();
				}
			}
		}

		// Token: 0x0600025D RID: 605 RVA: 0x00003BC7 File Offset: 0x00001DC7
		private void ClearCache()
		{
			this.maidBones = new Dictionary<Maid, Dictionary<string, Transform>>();
			this.maidBoneLastPos = new Dictionary<Maid, Dictionary<string, Vector3>>();
			this.maidBoneLastRot = new Dictionary<Maid, Dictionary<string, Quaternion>>();
		}

		// Token: 0x0600025E RID: 606 RVA: 0x00003BEA File Offset: 0x00001DEA
		public static IKTool Create()
		{
			if (IKTool.instance != null)
			{
				return IKTool.instance;
			}
			GameObject gameObject = new GameObject("IKTool");
			IKTool.instance = gameObject.AddComponent<IKTool>();
			global::UnityEngine.Object.DontDestroyOnLoad(gameObject);
			return IKTool.instance;
		}

		// Token: 0x0600025F RID: 607 RVA: 0x0000F744 File Offset: 0x0000D944
		private Material CreateForceDrawMaterial()
		{
			Material material2;
			try
			{
				Material material = new Material(MaterialHelper.GetColorZOrderShader());
				Color red = Color.red;
				red.a = this.markerOverlayAlpha;
				material.SetColor("_Color", red);
				material2 = material;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				material2 = null;
			}
			return material2;
		}

		// Token: 0x06000260 RID: 608 RVA: 0x00003C1E File Offset: 0x00001E1E
		private IEnumerator DelayedApplyLoadedIKNodes()
		{
			yield return new WaitForSeconds(5f);
			this.ApplyLoadedIKNodes();
			yield break;
		}

		// Token: 0x06000261 RID: 609 RVA: 0x0000F79C File Offset: 0x0000D99C
		public void EnableIKMode(bool markerIsEnable)
		{
			if (!markerIsEnable && !this.markerIsEnable)
			{
				return;
			}
			if (!markerIsEnable)
			{
				this.refs--;
			}
			else
			{
				this.refs++;
			}
			if (this.refs > 0)
			{
				this.markerIsEnable = true;
			}
			else
			{
				this.refs = 0;
				this.markerIsEnable = false;
			}
			this.VisibleIKNodes();
		}

		// Token: 0x06000262 RID: 610 RVA: 0x0000F7FC File Offset: 0x0000D9FC
		private void FindAndUpdateIKWithMarker(Transform t, Maid maid)
		{
			if (t.name != null && t.name.StartsWith("__IK__Marker__") && t.localPosition != Vector3.zero && t.localRotation != Quaternion.identity)
			{
				this.OnIKMarkerMoved(t, maid);
			}
			for (int i = 0; i < t.childCount; i++)
			{
				this.FindAndUpdateIKWithMarker(t.GetChild(i), maid);
			}
		}

		// Token: 0x06000263 RID: 611 RVA: 0x0000F870 File Offset: 0x0000DA70
		private void FindIKAndInsertMarker(Maid maid)
		{
			Dictionary<string, Transform> dictionary = new Dictionary<string, Transform>();
			Dictionary<string, Vector3> dictionary2 = new Dictionary<string, Vector3>();
			Dictionary<string, Quaternion> dictionary3 = new Dictionary<string, Quaternion>();
			this.maidBones.Add(maid, dictionary);
			this.maidBoneLastPos[maid] = dictionary2;
			this.maidBoneLastRot[maid] = dictionary3;
			foreach (string text in this.IK_Targets)
			{
				Transform transform = CMT.SearchObjName(maid.body0.m_Bones.transform, text, true);
				if (transform != null)
				{
					dictionary.Add(transform.name, transform);
					dictionary2[transform.name] = transform.localPosition;
					dictionary3[transform.name] = transform.localRotation;
					string text2 = "__IK__Marker__" + transform.name;
					Transform transform2 = transform.FindChild(text2);
					if (transform2 == null)
					{
						GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
						if (this.markerShowOverlay)
						{
							if (this.markerSharedMaterial == null)
							{
								this.markerSharedMaterial = this.CreateForceDrawMaterial();
							}
							if (this.markerSharedMaterial_ItemIK == null)
							{
								this.markerSharedMaterial_ItemIK = this.CreateForceDrawMaterial();
								this.markerSharedMaterial_ItemIK.SetColor("_Color", new Color(0f, 1f, 0f, this.markerOverlayAlpha));
							}
							if (!transform.name.StartsWith("_IK_"))
							{
								gameObject.GetComponent<Renderer>().sharedMaterial = this.markerSharedMaterial;
							}
							else
							{
								gameObject.GetComponent<Renderer>().sharedMaterial = this.markerSharedMaterial_ItemIK;
							}
						}
						gameObject.AddComponent<MoveableGUIObject>().onMoveLister.Add(new Action<MonoBehaviour>(this.OnIKMarkerMoved));
						gameObject.name = text2;
						gameObject.GetComponent<SphereCollider>().isTrigger = true;
						gameObject.transform.parent = transform;
						gameObject.transform.localPosition = Vector3.zero;
						gameObject.transform.localRotation = Quaternion.identity;
						if (this.IK_Targets_Base.Contains(transform.name))
						{
							gameObject.transform.localScale = Vector3.one * this.markerSize;
						}
						else if (!this.IK_Targets_IK.Contains(transform.name))
						{
							gameObject.transform.localScale = Vector3.one * this.markerSizeFinger;
						}
						else
						{
							gameObject.transform.localScale = Vector3.one * this.markerSizeItemIK;
						}
						gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
						transform2 = gameObject.transform;
					}
					transform2.gameObject.GetComponent<Renderer>().enabled = this.markerIsVisible;
					transform2.gameObject.SetActive(this.markerIsEnable);
				}
			}
		}

		// Token: 0x06000264 RID: 612 RVA: 0x0000FB34 File Offset: 0x0000DD34
		private Maid FindMaidByGUID(string guid)
		{
			CharacterMgr characterMgr = GameMain.Instance.CharacterMgr;
			for (int i = 0; i < characterMgr.GetMaidCount(); i++)
			{
				Maid maid = characterMgr.GetMaid(i);
				if (maid != null && maid.status.guid == guid)
				{
					return maid;
				}
			}
			return null;
		}

		// Token: 0x06000265 RID: 613 RVA: 0x0000FB84 File Offset: 0x0000DD84
		private PhotoWindowManager FindPhotoWindowManagerInScene()
		{
			GameObject gameObject = GameObject.Find("UI Root/MainScreen/PhotoWindowManager");
			if (gameObject == null)
			{
				return null;
			}
			return gameObject.GetComponent<PhotoWindowManager>();
		}

		// Token: 0x06000266 RID: 614 RVA: 0x0000FBB0 File Offset: 0x0000DDB0
		private void HideHitSphere()
		{
			foreach (TBody tbody in global::UnityEngine.Object.FindObjectsOfType<TBody>())
			{
				foreach (MeshRenderer meshRenderer in global::UnityEngine.Object.FindObjectsOfType<MeshRenderer>())
				{
					if (meshRenderer.gameObject.name.StartsWith("__dummy_hitsphere_view"))
					{
						global::UnityEngine.Object.Destroy(meshRenderer.gameObject);
					}
				}
			}
		}

		// Token: 0x06000267 RID: 615 RVA: 0x00003C2D File Offset: 0x00001E2D
		private IEnumerator HookSaveLoadCoroutine()
		{
			for (;;)
			{
				if (this.hookPhotoSaveLoad)
				{
					if (this.photoWindowManager == null)
					{
						this.photoWindowManager = this.FindPhotoWindowManagerInScene();
					}
					if (this.photoWindowManager != null)
					{
						if (this.photoWindowManager.SaveAndLoadManager.onSerializeEvent != new Func<XElement>(this.OnSerializeWithIK))
						{
							this.defaultOnSerializeEventHandler = this.photoWindowManager.SaveAndLoadManager.onSerializeEvent;
							this.photoWindowManager.SaveAndLoadManager.onSerializeEvent = new Func<XElement>(this.OnSerializeWithIK);
						}
						if (this.photoWindowManager.SaveAndLoadManager.onDeserializeEvent != new Action<XElement>(this.OnDeserializeWithIK))
						{
							this.defaultOnDeserializeEventHandler = this.photoWindowManager.SaveAndLoadManager.onDeserializeEvent;
							this.photoWindowManager.SaveAndLoadManager.onDeserializeEvent = new Action<XElement>(this.OnDeserializeWithIK);
						}
					}
				}
				yield return new WaitForSeconds(0.5f);
			}
			yield break;
		}

		// Token: 0x06000268 RID: 616 RVA: 0x0000FC14 File Offset: 0x0000DE14
		private void InsertSkirtHitSphereForHair()
		{
			TBody[] array = global::UnityEngine.Object.FindObjectsOfType<TBody>();
			Material[] array2 = new Material[5];
			for (int i = 0; i < 5; i++)
			{
				Material material = new Material(MaterialHelper.GetColorZOrderShader());
				Color color;
				switch (i)
				{
				case 0:
					color = Color.red;
					break;
				case 1:
					color = Color.blue;
					break;
				case 2:
					color = Color.green;
					break;
				case 3:
					color = Color.yellow;
					break;
				case 4:
					color = Color.cyan;
					break;
				default:
					color = Color.black;
					break;
				}
				material.SetColor("_Color", color);
				array2[i] = material;
			}
			int num = 0;
			using (StreamWriter streamWriter = File.CreateText("__skirt.log"))
			{
				foreach (TBody tbody in array)
				{
					streamWriter.WriteLine("===Skirt for " + tbody.maid.name + "===");
					if (tbody.isActiveAndEnabled)
					{
						TBodySkin tbodySkin = tbody.goSlot[8];
						TBodySkin tbodySkin2 = tbody.goSlot[9];
						Console.WriteLine(string.Format("Skirt: {0}, {1}, {2}", tbodySkin.boVisible, tbodySkin.m_strModelFileName, tbodySkin.obj));
						Console.WriteLine(string.Format("Onepiece: {0}, {1}, {2}", tbodySkin2.boVisible, tbodySkin2.m_strModelFileName, tbodySkin2.obj));
						if (tbodySkin.obj == null && tbodySkin2.obj != null)
						{
							tbodySkin = tbodySkin2;
						}
						if (tbodySkin.boVisible && tbodySkin.m_strModelFileName != "")
						{
							TBody.SlotID slotId = tbodySkin.SlotId;
							streamWriter.WriteLine(string.Format("{0}, {1}", tbodySkin.Category, slotId));
							streamWriter.WriteLine(tbodySkin.obj_tr.ToString());
							streamWriter.WriteLine("== Hair1 ==");
							List<THair1> list = (List<THair1>)tbodySkin.bonehair.GetType().GetField("hair1list", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField).GetValue(tbodySkin.bonehair);
							for (int k = 0; k < list.Count; k++)
							{
								THair1 thair = list[k];
								streamWriter.WriteLine(string.Format("{0}", thair.root.position.ToString()));
								GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
								gameObject.name = "__dummy_hitsphere_view";
								gameObject.transform.position = thair.root.position;
								gameObject.transform.localScale = Vector3.one * 0.02f;
								gameObject.GetComponent<Renderer>().sharedMaterial = array2[0];
							}
							streamWriter.WriteLine("== SKIRT LIST ==");
							THair1[] array4 = (THair1[])tbodySkin.bonehair.GetType().GetField("SkirtList", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField).GetValue(tbodySkin.bonehair);
							float num2 = 0f;
							List<Transform> list2 = new List<Transform>();
							Transform root_oya = array4[0].root_oya;
							for (int l = 3; l < 9; l++)
							{
								THair1 thair2 = array4[l];
								streamWriter.WriteLine(string.Format("{0} {1}", thair2.root.position.ToString(), thair2.root_oya.ToString()));
								list2.Add(thair2.root);
							}
							List<THitSphere> list3 = new List<THitSphere>();
							for (int m = 3; m < 9; m++)
							{
								THair1 thair3 = array4[m];
								Console.WriteLine("hplist count: " + thair3.hplist.Count);
								for (int n = 0; n < thair3.hplist.Count; n += 2)
								{
									THp thp = thair3.hplist[n];
									GameObject gameObject2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
									gameObject2.name = "__dummy_hitsphere_view";
									gameObject2.transform.position = thp.t.position;
									gameObject2.transform.localScale = Vector3.one * (float)(n + 1) * 0.01f;
									gameObject2.GetComponent<Renderer>().sharedMaterial = array2[n % 5];
									float sqrMagnitude = (root_oya.position - thp.t.position).sqrMagnitude;
									if (num2 < sqrMagnitude)
									{
										num2 = sqrMagnitude;
									}
									list2.Add(thp.t);
								}
								if (thair3.hplist.Count >= 9)
								{
									THp thp2 = thair3.hplist[2];
									THp thp3 = thair3.hplist[4];
									THp thp4 = thair3.hplist[6];
									THp thp5 = thair3.hplist[8];
									float num3 = (thp4.t.position - thp2.t.position).magnitude / 2f;
									THitSphere thitSphere = new THitSphere
									{
										len = num3,
										lenxlen = num3 * num3,
										len_ = num3,
										lenxlen_ = num3 * num3,
										name = "__Skirt",
										pname = thp3.t_oya.name,
										t = thp3.t
									};
									list3.Add(thitSphere);
									THitSphere thitSphere2 = new THitSphere
									{
										len = num3,
										lenxlen = num3 * num3,
										len_ = num3,
										lenxlen_ = num3 * num3,
										name = "__Skirt",
										pname = thp5.t_oya.name,
										t = thp5.t
									};
									list3.Add(thitSphere2);
								}
							}
							foreach (TBodySkin tbodySkin3 in tbody.goSlot)
							{
								if (tbodySkin3.Category.StartsWith("hair"))
								{
									TBodyHit bodyhit = tbodySkin3.bonehair.bodyhit;
									for (int num4 = bodyhit.spherelist.Count - 1; num4 >= 0; num4--)
									{
										if (bodyhit.spherelist[num4].name.StartsWith("__Skirt"))
										{
											bodyhit.spherelist.Remove(bodyhit.spherelist[num4]);
										}
									}
								}
							}
						}
					}
				}
				streamWriter.WriteLine("Skirt transform count: " + num);
			}
		}

		// Token: 0x06000269 RID: 617 RVA: 0x00003C3C File Offset: 0x00001E3C
		public bool IsIKMarkerEnabled()
		{
			return this.markerIsEnable;
		}

		// Token: 0x0600026A RID: 618 RVA: 0x000102D4 File Offset: 0x0000E4D4
		public void OnDeserializeWithIK(XElement xml)
		{
			this.ClearCache();
			XContainer xcontainer = xml.Element("PhotSetting").Element("Maids");
			this.maidLoadIKPos = new Dictionary<string, Dictionary<string, Vector3>>();
			this.maidLoadIKRot = new Dictionary<string, Dictionary<string, Quaternion>>();
			foreach (XElement xelement in xcontainer.Elements())
			{
				if (!(xelement.Name != "Maid"))
				{
					string value = xelement.Attribute("guid").Value;
					Dictionary<string, Vector3> dictionary = new Dictionary<string, Vector3>();
					Dictionary<string, Quaternion> dictionary2 = new Dictionary<string, Quaternion>();
					this.maidLoadIKPos[value] = dictionary;
					this.maidLoadIKRot[value] = dictionary2;
					XElement xelement2 = null;
					foreach (XElement xelement3 in xelement.Elements())
					{
						if (xelement3.Name == "Type" && "X_IK_Transforms".Equals(xelement3.Attribute("name").Value))
						{
							xelement2 = xelement3;
						}
					}
					if (xelement2 != null)
					{
						foreach (XElement xelement4 in xelement2.Elements())
						{
							try
							{
								string value2 = xelement4.Attribute("name").Value;
								if (value2 != null && this.IK_Targets.Contains(value2))
								{
									float num = float.Parse(xelement4.Attribute("x").Value);
									float num2 = float.Parse(xelement4.Attribute("y").Value);
									float num3 = float.Parse(xelement4.Attribute("z").Value);
									float num4 = float.Parse(xelement4.Attribute("rx").Value);
									float num5 = float.Parse(xelement4.Attribute("ry").Value);
									float num6 = float.Parse(xelement4.Attribute("rz").Value);
									Vector3 vector = new Vector3(num, num2, num3);
									Quaternion quaternion = Quaternion.Euler(num4, num5, num6);
									dictionary[value2] = vector;
									dictionary2[value2] = quaternion;
								}
							}
							catch (Exception)
							{
							}
						}
					}
				}
			}
			this.defaultOnDeserializeEventHandler(xml);
			base.StartCoroutine("DelayedApplyLoadedIKNodes");
		}

		// Token: 0x0600026B RID: 619 RVA: 0x000105DC File Offset: 0x0000E7DC
		private void OnIKMarkerMoved(MonoBehaviour ikMarker)
		{
			Maid componentInParent = ikMarker.transform.GetComponentInParent<Maid>();
			Transform transform = ikMarker.transform;
			if (this.poseEditWindow != null && this.placementWindow != null && !bool.Parse(this.poseEditWindow.GetMaidStoreData(componentInParent)["use"]))
			{
				IKTool.m_SetSelectMaid.Invoke(this.placementWindow, new object[] { componentInParent });
				this.poseEditWindow.OnClickUseCheckRun(true);
				this.poseEditWindow.CheckbtnUse.check = true;
			}
			this.OnIKMarkerMoved(transform, componentInParent);
		}

		// Token: 0x0600026C RID: 620 RVA: 0x00010678 File Offset: 0x0000E878
		private void OnIKMarkerMoved(Transform marker, Maid maid)
		{
			Dictionary<string, Transform> dictionary = this.maidBones[maid];
			Dictionary<string, Vector3> dictionary2 = this.maidBoneLastPos[maid];
			Dictionary<string, Quaternion> dictionary3 = this.maidBoneLastRot[maid];
			Transform parent = marker.parent;
			string name = parent.name;
			Transform[] array = null;
			if (name.StartsWith("_IK_"))
			{
				parent.position = marker.position;
				parent.rotation = marker.rotation;
			}
			else if (name.Contains(" R ") || name.Contains(" L "))
			{
				string text = name.Substring(0, "Bip01 x ".Length);
				string text2 = name.Substring("Bip01 x ".Length);
				IK ik = new IK();
				TBody body = maid.body0;
				Vector3 zero = Vector3.zero;
				if (text2.Contains("Finger"))
				{
					string text3 = text2.Substring(0, "Fingerx".Length);
					string text4 = text2.Substring("Fingerx".Length);
					if (text4 != null && text4.Length == 0)
					{
						ik.Init(dictionary[text + "Hand"], dictionary[text + "Hand"], dictionary[(text + text3) ?? ""], body);
						ik.Porc(dictionary[text + "Hand"], dictionary[text + "Hand"], dictionary[(text + text3) ?? ""], marker.position, zero);
						parent.rotation = marker.rotation;
						array = new Transform[] { dictionary[text + "Hand"] };
					}
					else if (text4 == "1")
					{
						ik.Init(dictionary[(text + text3) ?? ""], dictionary[(text + text3) ?? ""], dictionary[text + text3 + "1"], body);
						ik.Porc(dictionary[(text + text3) ?? ""], dictionary[(text + text3) ?? ""], dictionary[text + text3 + "1"], marker.position, zero);
						parent.rotation = marker.rotation;
						array = new Transform[] { dictionary[(text + text3) ?? ""] };
					}
					else if (text4 == "2")
					{
						ik.Init(dictionary[(text + text3) ?? ""], dictionary[text + text3 + "1"], dictionary[text + text3 + "2"], body);
						ik.Porc(dictionary[(text + text3) ?? ""], dictionary[text + text3 + "1"], dictionary[text + text3 + "2"], marker.position, zero);
						parent.rotation = marker.rotation;
						array = new Transform[]
						{
							dictionary[(text + text3) ?? ""],
							dictionary[text + text3 + "1"]
						};
					}
					else if (text4 == "Nub")
					{
						ik.Init(dictionary[text + text3 + "1"], dictionary[text + text3 + "2"], dictionary[text + text3 + "Nub"], body);
						ik.Porc(dictionary[text + text3 + "1"], dictionary[text + text3 + "2"], dictionary[text + text3 + "Nub"], marker.position, zero);
						parent.rotation = marker.rotation;
						array = new Transform[]
						{
							dictionary[text + text3 + "1"],
							dictionary[text + text3 + "2"]
						};
					}
				}
				else if (text2 == "Hand")
				{
					ik.Init(dictionary[text + "UpperArm"], dictionary[text + "Forearm"], dictionary[text + "Hand"], body);
					ik.Porc(dictionary[text + "UpperArm"], dictionary[text + "Forearm"], dictionary[text + "Hand"], marker.position, zero);
					parent.rotation = marker.rotation;
					array = new Transform[]
					{
						dictionary[text + "UpperArm"],
						dictionary[text + "Forearm"]
					};
				}
				else if (text2 == "Forearm")
				{
					ik.Init(dictionary[text + "UpperArm"], dictionary[text + "UpperArm"], dictionary[text + "Forearm"], body);
					ik.Porc(dictionary[text + "UpperArm"], dictionary[text + "UpperArm"], dictionary[text + "Forearm"], marker.position, zero);
					parent.rotation = marker.rotation;
					array = new Transform[] { dictionary[text + "UpperArm"] };
				}
				else if (text2 == "UpperArm")
				{
					ik.Init(dictionary[text + "Clavicle"], dictionary[text + "Clavicle"], dictionary[text + "UpperArm"], body);
					ik.Porc(dictionary[text + "Clavicle"], dictionary[text + "Clavicle"], dictionary[text + "UpperArm"], marker.position, zero);
					parent.rotation = marker.rotation;
					array = new Transform[] { dictionary[text + "Clavicle"] };
				}
				else if (text2 == "Clavicle")
				{
					parent.rotation = marker.rotation;
				}
				else if (text2 == "Foot")
				{
					ik.Init(dictionary[text + "Thigh"], dictionary[text + "Calf"], dictionary[text + "Foot"], body);
					ik.Porc(dictionary[text + "Thigh"], dictionary[text + "Calf"], dictionary[text + "Foot"], marker.position, zero);
					array = new Transform[]
					{
						dictionary[text + "Clavicle"],
						dictionary[text + "Calf"]
					};
					parent.rotation = marker.rotation;
				}
				else if (text2 == "Calf")
				{
					ik.Init(dictionary[text + "Thigh"], dictionary[text + "Thigh"], dictionary[text + "Calf"], body);
					ik.Porc(dictionary[text + "Thigh"], dictionary[text + "Thigh"], dictionary[text + "Calf"], marker.position, zero);
					parent.rotation = marker.rotation;
					array = new Transform[] { dictionary[text + "Thigh"] };
				}
				else if (text2 == "Thigh")
				{
					parent.rotation = marker.rotation;
				}
			}
			else
			{
				parent.rotation = marker.rotation;
			}
			marker.localPosition = Vector3.zero;
			marker.localRotation = Quaternion.identity;
			dictionary2[name] = parent.localPosition;
			dictionary3[name] = parent.localRotation;
			if (array != null)
			{
				foreach (Transform transform in array)
				{
					dictionary2[transform.name] = transform.localPosition;
					dictionary3[transform.name] = transform.localRotation;
				}
			}
		}

		// Token: 0x0600026D RID: 621 RVA: 0x00010FC8 File Offset: 0x0000F1C8
		private void OnLevelWasLoaded(int level)
		{
			base.StopAllCoroutines();
			this.poseEditWindow = global::UnityEngine.Object.FindObjectOfType<PoseEditWindow>();
			this.placementWindow = global::UnityEngine.Object.FindObjectOfType<PlacementWindow>();
			this.hookPhotoSaveLoad = false;
			if (!FirstPersonCameraHelper.IsPhotoMode(level))
			{
				FirstPersonCameraHelper.IsIKModeEnabled(level);
			}
			else
			{
				this.hookPhotoSaveLoad = true;
				base.StartCoroutine("HookSaveLoadCoroutine");
			}
			this.ClearCache();
		}

		// Token: 0x0600026E RID: 622 RVA: 0x00011024 File Offset: 0x0000F224
		public XElement OnSerializeWithIK()
		{
			Console.WriteLine("Wrote additional data  (IK info)");
			if (this.maidBones == null || this.maidBones.Count == 0)
			{
				this.VisibleIKNodes();
			}
			Dictionary<string, XElement> dictionary = new Dictionary<string, XElement>();
			foreach (Maid maid in this.maidBones.Keys)
			{
				string guid = maid.status.guid;
				XElement xelement = new XElement("Type");
				xelement.SetAttributeValue("name", "X_IK_Transforms");
				Dictionary<string, Transform> dictionary2 = this.maidBones[maid];
				Dictionary<string, Vector3> dictionary3 = this.maidBoneLastPos[maid];
				Dictionary<string, Quaternion> dictionary4 = this.maidBoneLastRot[maid];
				if (dictionary2 != null)
				{
					foreach (string text in this.IK_Targets_IK)
					{
						if (dictionary2.ContainsKey(text))
						{
							Transform transform = dictionary2[text];
							XElement xelement2 = new XElement("elm");
							xelement2.SetAttributeValue("name", text);
							Vector3 zero = Vector3.zero;
							dictionary3.TryGetValue(text, out zero);
							Quaternion identity = Quaternion.identity;
							dictionary4.TryGetValue(text, out identity);
							Vector3 eulerAngles = identity.eulerAngles;
							xelement2.SetAttributeValue("x", zero.x.ToString("G"));
							xelement2.SetAttributeValue("y", zero.y.ToString("G"));
							xelement2.SetAttributeValue("z", zero.z.ToString("G"));
							xelement2.SetAttributeValue("rx", eulerAngles.x.ToString("G"));
							xelement2.SetAttributeValue("ry", eulerAngles.y.ToString("G"));
							xelement2.SetAttributeValue("rz", eulerAngles.z.ToString("G"));
							xelement.Add(xelement2);
						}
					}
				}
				dictionary.Add(guid, xelement);
			}
			XElement xelement3 = this.defaultOnSerializeEventHandler();
			foreach (XElement xelement4 in xelement3.Element("Maids").Elements())
			{
				if (!(xelement4.Name != "Maid"))
				{
					string value = xelement4.Attribute("guid").Value;
					this.CleanupIKXML(xelement4);
					if (dictionary.ContainsKey(value))
					{
						xelement4.Add(dictionary[value]);
					}
				}
			}
			return xelement3;
		}

		// Token: 0x0600026F RID: 623 RVA: 0x00011340 File Offset: 0x0000F540
		private void Start()
		{
			this.markerIsVisible = Settings.Instance.GetBoolValue("IKToolShowMarker", true);
			this.enableFingerIK = Settings.Instance.GetBoolValue("IKToolEnableFingerIK", true);
			this.enableItemIK = Settings.Instance.GetBoolValue("IKToolEnableItemIK", true);
			this.markerSize = Settings.Instance.GetFloatValue("IKToolMarkerSize", 0.05f);
			this.markerSizeFinger = Settings.Instance.GetFloatValue("IKToolMarkerSizeFinger", 0.005f);
			this.markerSizeItemIK = Settings.Instance.GetFloatValue("IKToolMarkerSizeItemIK", 0.01f);
			this.markerShowOverlay = Settings.Instance.GetBoolValue("IKToolMarkerShowAsOverlay", true);
			this.markerOverlayAlpha = Settings.Instance.GetFloatValue("IKToolMarkerOverlayAlpha", 0.4f);
			this.hitCheckDebugEnabled = Settings.Instance.GetBoolValue("IKToolHitCheckDebugEnabled", false);
			List<string> list = new List<string>();
			for (int i = 0; i < this.IK_Targets_Base.Length; i++)
			{
				list.Add(this.IK_Targets_Base[i]);
			}
			if (this.enableFingerIK)
			{
				for (int j = 0; j < this.IK_Targets_Fingers.Length; j++)
				{
					list.Add(this.IK_Targets_Fingers[j]);
				}
			}
			if (this.enableItemIK)
			{
				for (int k = 0; k < this.IK_Targets_IK.Length; k++)
				{
					list.Add(this.IK_Targets_IK[k]);
				}
			}
			this.IK_Targets = list.ToArray();
		}

		// Token: 0x06000270 RID: 624 RVA: 0x00003C44 File Offset: 0x00001E44
		public void ToggleMarkerVisibility()
		{
			this.VisualizeIKMarker(!this.markerIsVisible);
		}

		// Token: 0x06000271 RID: 625 RVA: 0x000114A8 File Offset: 0x0000F6A8
		private void Update()
		{
			if (!this.hitCheckDebugEnabled)
			{
				return;
			}
			try
			{
				if (Input.GetKeyUp(KeyCode.F7))
				{
					if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
					{
						this.HideHitSphere();
					}
					else if (!Input.GetKey(KeyCode.LeftAlt))
					{
						this.VisualizeHitSphere("hair");
					}
					else
					{
						this.VisualizeHitSphere(null);
					}
				}
				if (Input.GetKeyUp(KeyCode.F6))
				{
					this.InsertSkirtHitSphereForHair();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		// Token: 0x06000272 RID: 626 RVA: 0x0001153C File Offset: 0x0000F73C
		public void VisibleIKNodes()
		{
			CharacterMgr characterMgr = GameMain.Instance.CharacterMgr;
			this.maidBones = new Dictionary<Maid, Dictionary<string, Transform>>();
			for (int i = 0; i < characterMgr.GetMaidCount(); i++)
			{
				Maid maid = characterMgr.GetMaid(i);
				if (maid != null)
				{
					this.FindIKAndInsertMarker(maid);
				}
			}
		}

		// Token: 0x06000273 RID: 627 RVA: 0x00011588 File Offset: 0x0000F788
		private void VisualizeHitSphere(string type)
		{
			TBody[] array = global::UnityEngine.Object.FindObjectsOfType<TBody>();
			Material material = new Material(MaterialHelper.GetColorZOrderShader());
			Color blue = Color.blue;
			blue.a = 0.3f;
			material.SetColor("_Color", blue);
			Material material2 = new Material(MaterialHelper.GetColorZOrderShader());
			Color green = Color.green;
			green.a = 0.3f;
			material2.SetColor("_Color", green);
			int num = 0;
			int num2 = 0;
			using (StreamWriter streamWriter = File.CreateText("__bodyhit.log"))
			{
				foreach (TBody tbody in array)
				{
					if (tbody.isActiveAndEnabled)
					{
						streamWriter.WriteLine(string.Format("----{0}----", tbody.maid.name));
						Transform transform = CMT.SearchObjName(tbody.transform, "Bip01 Head", true);
						CMT.SearchObjName(tbody.transform, "Bip01 Spine1a", true);
						CMT.SearchObjName(tbody.transform, "Bip01 Spine0a", true);
						CMT.SearchObjName(tbody.transform, "Bip01 Neck", true);
						CMT.SearchObjName(tbody.transform, "Bip01 L Clavicle_SCL_", true);
						Transform transform2 = CMT.SearchObjName(tbody.transform, "Bip01 Spine0a_SCL_", true);
						CMT.SearchObjName(tbody.transform, "Bip01 Pelvis", true);
						Transform transform3 = CMT.SearchObjName(tbody.transform, "Bip01 Pelvis_SCL_", true);
						Transform transform4 = CMT.SearchObjName(tbody.transform, "Bip01 Spine1a_SCL_", true);
						float x = transform.localScale.x;
						float x2 = transform4.localScale.x;
						float x3 = transform2.localScale.x;
						float x4 = transform3.localScale.x;
						streamWriter.WriteLine(string.Format("Head: {0}", x));
						streamWriter.WriteLine(string.Format("Kata: {0}", x2));
						streamWriter.WriteLine(string.Format("Mid:  {0}", x3));
						streamWriter.WriteLine(string.Format("Koshi:{0}", x4));
						foreach (TBodySkin tbodySkin in tbody.goSlot)
						{
							if (type == null || tbodySkin.Category.StartsWith(type))
							{
								num++;
								streamWriter.WriteLine(string.Format("=== Skin: {0}, category: {1}===", tbodySkin.m_strModelFileName, tbodySkin.Category));
								foreach (THitSphere thitSphere in tbodySkin.bonehair.bodyhit.spherelist)
								{
									num2++;
									string text = "";
									if (thitSphere.tPtr != null)
									{
										text = thitSphere.tPtr.name;
									}
									object[] array3 = new object[6];
									array3[0] = thitSphere.name;
									array3[1] = thitSphere.pname;
									array3[2] = text;
									object[] array4 = array3;
									array4[3] = thitSphere.t.position.ToString();
									array4[4] = thitSphere.len;
									array4[5] = thitSphere.t.parent.lossyScale.x;
									Console.WriteLine(string.Format("Sphere: {0}, {1}, {2}, {3}, {4}, {5}", array4));
									object[] array5 = new object[6];
									array5[0] = thitSphere.name;
									array5[1] = thitSphere.pname;
									array5[2] = text;
									object[] array6 = array5;
									array6[3] = thitSphere.t.position.ToString();
									array6[4] = thitSphere.len;
									array6[5] = thitSphere.t.parent.lossyScale.x;
									streamWriter.WriteLine(string.Format("Sphere: {0}, {1}, {2}, {3}, {4}, {5}", array6));
									GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
									gameObject.name = "__dummy_hitsphere_view";
									gameObject.transform.position = thitSphere.t.position;
									gameObject.transform.localScale = Vector3.one * thitSphere.len * (1f + _TS.TestVal2 * 2f);
									gameObject.GetComponent<Renderer>().sharedMaterial = material;
									if (thitSphere.name.StartsWith("_CH"))
									{
										gameObject.GetComponent<Renderer>().sharedMaterial = material2;
									}
								}
							}
						}
					}
				}
			}
			Console.WriteLine("TBodySkin Count: " + num);
			Console.WriteLine("THitSphere Count: " + num2);
		}

		// Token: 0x06000274 RID: 628 RVA: 0x00003C55 File Offset: 0x00001E55
		public void VisualizeIKMarker(bool markerIsVisible)
		{
			this.markerIsVisible = markerIsVisible;
			this.VisibleIKNodes();
		}

		// Token: 0x0400019A RID: 410
		private PhotoWindowManager photoWindowManager;

		// Token: 0x0400019B RID: 411
		private Func<XElement> defaultOnSerializeEventHandler;

		// Token: 0x0400019C RID: 412
		private Action<XElement> defaultOnDeserializeEventHandler;

		// Token: 0x0400019D RID: 413
		private bool hookPhotoSaveLoad;

		// Token: 0x0400019E RID: 414
		private bool markerIsVisible = true;

		// Token: 0x0400019F RID: 415
		private bool markerShowOverlay = true;

		// Token: 0x040001A0 RID: 416
		private float markerOverlayAlpha = 0.4f;

		// Token: 0x040001A1 RID: 417
		private bool markerIsEnable;

		// Token: 0x040001A2 RID: 418
		private bool enableFingerIK = true;

		// Token: 0x040001A3 RID: 419
		private bool enableItemIK = true;

		// Token: 0x040001A4 RID: 420
		private int refs;

		// Token: 0x040001A5 RID: 421
		private float markerSize = 0.05f;

		// Token: 0x040001A6 RID: 422
		private float markerSizeFinger = 0.005f;

		// Token: 0x040001A7 RID: 423
		private float markerSizeItemIK = 0.01f;

		// Token: 0x040001A8 RID: 424
		private Material markerSharedMaterial;

		// Token: 0x040001A9 RID: 425
		private Material markerSharedMaterial_ItemIK;

		// Token: 0x040001AA RID: 426
		private bool hitCheckDebugEnabled;

		// Token: 0x040001AB RID: 427
		private Dictionary<Maid, Dictionary<string, Transform>> maidBones = new Dictionary<Maid, Dictionary<string, Transform>>();

		// Token: 0x040001AC RID: 428
		private Dictionary<Maid, Dictionary<string, Vector3>> maidBoneLastPos = new Dictionary<Maid, Dictionary<string, Vector3>>();

		// Token: 0x040001AD RID: 429
		private Dictionary<Maid, Dictionary<string, Quaternion>> maidBoneLastRot = new Dictionary<Maid, Dictionary<string, Quaternion>>();

		// Token: 0x040001AE RID: 430
		private Dictionary<string, Dictionary<string, Vector3>> maidLoadIKPos = new Dictionary<string, Dictionary<string, Vector3>>();

		// Token: 0x040001AF RID: 431
		private Dictionary<string, Dictionary<string, Quaternion>> maidLoadIKRot = new Dictionary<string, Dictionary<string, Quaternion>>();

		// Token: 0x040001B0 RID: 432
		public static IKTool instance;

		// Token: 0x040001B1 RID: 433
		private PoseEditWindow poseEditWindow;

		// Token: 0x040001B2 RID: 434
		private PlacementWindow placementWindow;

		// Token: 0x040001B3 RID: 435
		private static MethodInfo m_SetSelectMaid = typeof(PlacementWindow).GetMethod("SetSelectMaid", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod);

		// Token: 0x040001B4 RID: 436
		private string[] IK_Targets = new string[0];

		// Token: 0x040001B5 RID: 437
		private string[] IK_Targets_Fingers = new string[]
		{
			"Bip01 L Finger0", "Bip01 L Finger01", "Bip01 L Finger02", "Bip01 L Finger0Nub", "Bip01 L Finger1", "Bip01 L Finger11", "Bip01 L Finger12", "Bip01 L Finger1Nub", "Bip01 L Finger2", "Bip01 L Finger21",
			"Bip01 L Finger22", "Bip01 L Finger2Nub", "Bip01 L Finger3", "Bip01 L Finger31", "Bip01 L Finger32", "Bip01 L Finger3Nub", "Bip01 L Finger4", "Bip01 L Finger41", "Bip01 L Finger42", "Bip01 L Finger4Nub",
			"Bip01 R Finger0", "Bip01 R Finger01", "Bip01 R Finger02", "Bip01 R Finger0Nub", "Bip01 R Finger1", "Bip01 R Finger11", "Bip01 R Finger12", "Bip01 R Finger1Nub", "Bip01 R Finger2", "Bip01 R Finger21",
			"Bip01 R Finger22", "Bip01 R Finger2Nub", "Bip01 R Finger3", "Bip01 R Finger31", "Bip01 R Finger32", "Bip01 R Finger3Nub", "Bip01 R Finger4", "Bip01 R Finger41", "Bip01 R Finger42", "Bip01 R Finger4Nub"
		};

		// Token: 0x040001B6 RID: 438
		private string[] IK_Targets_Base = new string[]
		{
			"Bip01 R Hand", "Bip01 R UpperArm", "Bip01 R Forearm", "Bip01 R Clavicle", "Bip01 R Foot", "Bip01 R Calf", "Bip01 R Thigh", "Bip01 L Hand", "Bip01 L UpperArm", "Bip01 L Forearm",
			"Bip01 L Clavicle", "Bip01 L Foot", "Bip01 L Calf", "Bip01 L Thigh", "Bip01 Spine", "Bip01 Spine0a", "Bip01 Spine1", "Bip01 Spine1a", "Bip01 Neck", "Bip01 Head"
		};

		// Token: 0x040001B7 RID: 439
		private string[] IK_Targets_IK = new string[]
		{
			"_IK_vagina", "_IK_anal", "_IK_hipL", "_IK_hipR", "_IK_hutanari", "_IK_thighL", "_IK_calfL", "_IK_footL", "_IK_thighR", "_IK_calfR",
			"_IK_footR", "_IK_hara", "_IK_UpperArmL", "_IK_ForeArmL", "_IK_handL", "_IK_UpperArmR", "_IK_ForeArmR", "_IK_handR", "_IK_hohoL", "_IK_hohoR",
			"_IK_muneL", "_IK_muneR"
		};
	}
}

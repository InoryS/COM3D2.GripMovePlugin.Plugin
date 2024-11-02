using System;
using System.Collections.Generic;
using System.IO;
using ExIni;
using UnityEngine;
using UnityEngine.Rendering;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x02000035 RID: 53
	internal class VIVEModelReplaceTool : MonoBehaviour
	{
		// Token: 0x17000032 RID: 50
		// (get) Token: 0x060001BD RID: 445 RVA: 0x0000335F File Offset: 0x0000155F
		public static VIVEModelReplaceTool Instance
		{
			get
			{
				if (VIVEModelReplaceTool._instance == null)
				{
					GameObject gameObject = new GameObject("VIVEModelReplaceTool");
					VIVEModelReplaceTool._instance = gameObject.AddComponent<VIVEModelReplaceTool>();
					VIVEModelReplaceTool._instance.Init();
					global::UnityEngine.Object.DontDestroyOnLoad(gameObject);
				}
				return VIVEModelReplaceTool._instance;
			}
		}

		// Token: 0x060001C0 RID: 448 RVA: 0x0000B778 File Offset: 0x00009978
		public void AddModel(string id, string label, string modelFilename, Vector3 pos, Vector3 rot, bool hideTextAndPointer, string LR = "LR")
		{
			if (id == null || id == "" || id == VIVEModelReplaceTool.MODEL_DEFAULT)
			{
				Console.WriteLine("Invalid ID:");
				return;
			}
			if (this.models.ContainsKey(id))
			{
				return;
			}
			GameObject gameObject = this.LoadCM3D2ModelAsGameObject(modelFilename);
			if (gameObject != null)
			{
				gameObject.name = id;
				gameObject.transform.parent = base.gameObject.transform;
				VIVEModelReplaceTool.ModelInfo modelInfo = new VIVEModelReplaceTool.ModelInfo(id, label, modelFilename, pos, rot, hideTextAndPointer, LR)
				{
					cacheGameObj = gameObject
				};
				this.models[id] = modelInfo;
				if (LR.Contains("L"))
				{
					this.models_L.Add(modelInfo);
				}
				if (LR.Contains("R"))
				{
					this.models_R.Add(modelInfo);
				}
			}
		}

		// Token: 0x060001C1 RID: 449 RVA: 0x0000B844 File Offset: 0x00009A44
		private void EnableDisableDefaultRenderModel(SteamVR_TrackedObject trackedObject, bool enabled)
		{
			foreach (SteamVR_TrackedObject steamVR_TrackedObject in global::UnityEngine.Object.FindObjectsOfType<SteamVR_TrackedObject>())
			{
				if (steamVR_TrackedObject.index == trackedObject.index)
				{
					SteamVR_RenderModel steamVR_RenderModel = steamVR_TrackedObject.GetComponent<SteamVR_RenderModel>();
					if (steamVR_RenderModel != null)
					{
						Renderer[] componentsInChildren = steamVR_RenderModel.gameObject.GetComponentsInChildren<Renderer>();
						for (int j = 0; j < componentsInChildren.Length; j++)
						{
							componentsInChildren[j].enabled = enabled;
						}
					}
					else if (steamVR_RenderModel == null)
					{
						steamVR_RenderModel = steamVR_TrackedObject.GetComponentInChildren<SteamVR_RenderModel>();
						if (steamVR_RenderModel != null)
						{
							steamVR_RenderModel.gameObject.SetActive(enabled);
						}
					}
				}
			}
		}

		// Token: 0x060001C2 RID: 450 RVA: 0x0000B8DC File Offset: 0x00009ADC
		private Dictionary<string, GameObject> ExtractHandFromManModel()
		{
			GameObject gameObject = new GameObject();
			Maid maid = gameObject.AddComponent<Maid>();
			maid.Initialize("Man", true);
			maid.DutPropAll();
			maid.AllProcPropSeqStart();
			TBodySkin slot = maid.body0.GetSlot(0);
			TMorph tmorph = new TMorph(slot);
			SkinnedMeshRenderer componentInChildren = ImportCM.LoadSkinMesh_R("mbody.model", tmorph, "body", slot, LayerMask.NameToLayer("Default")).GetComponentInChildren<SkinnedMeshRenderer>();
			TBodySkin.OriVert oriVert = slot.m_OriVert;
			Transform[] bones = componentInChildren.bones;
			Dictionary<string, GameObject> dictionary = new Dictionary<string, GameObject>();
			foreach (string text in new string[] { "L", "R" })
			{
				List<int> list = new List<int>();
				int[] array2 = new int[oriVert.VCount];
				List<Vector3> list2 = new List<Vector3>();
				List<Vector3> list3 = new List<Vector3>();
				List<BoneWeight> list4 = new List<BoneWeight>();
				List<Matrix4x4> list5 = new List<Matrix4x4>();
				Transform transform = null;
				int num = -1;
				for (int j = 0; j < bones.Length; j++)
				{
					Transform transform2 = bones[j];
					if (transform2.name.EndsWith(text + " Hand"))
					{
						list.Add(j);
						transform = transform2;
						num = j;
					}
					else if (transform2.name.Contains(text + " Finger"))
					{
						list.Add(j);
					}
				}
				Dictionary<Transform, Transform> dictionary2 = new Dictionary<Transform, Transform>();
				List<Transform> list6 = new List<Transform>();
				Dictionary<int, int> dictionary3 = new Dictionary<int, int>();
				GameObject gameObject2 = null;
				if (transform != null)
				{
					gameObject2 = new GameObject(transform.name);
					gameObject2.transform.position = transform.transform.position;
					gameObject2.transform.rotation = transform.transform.rotation;
					dictionary2.Add(transform.transform, gameObject2.transform);
					list6.Add(gameObject2.transform);
					dictionary3.Add(num, 0);
				}
				foreach (int num2 in list)
				{
					Transform transform3 = bones[num2];
					if (!dictionary2.ContainsKey(transform3))
					{
						GameObject gameObject3 = new GameObject(transform3.gameObject.name);
						gameObject3.transform.position = transform3.position;
						gameObject3.transform.rotation = transform3.rotation;
						dictionary2.Add(transform3, gameObject3.transform);
						list6.Add(gameObject3.transform);
						dictionary3.Add(num2, list6.Count - 1);
					}
				}
				foreach (int num3 in list)
				{
					Transform transform4 = bones[num3];
					if (!(transform4 == gameObject2))
					{
						Transform transform5 = dictionary2[transform4];
						Transform parent = transform4.parent;
						if (!dictionary2.ContainsKey(parent))
						{
							Debug.Log(string.Concat(new string[] { "Parent of ", transform4.name, " ", parent.name, " not found" }));
						}
						else
						{
							transform5.parent = dictionary2[parent];
						}
					}
				}
				gameObject2.transform.position = Vector3.zero;
				gameObject2.transform.rotation = Quaternion.identity;
				for (int k = 0; k < list6.Count; k++)
				{
					list5.Add(list6[k].worldToLocalMatrix);
				}
				for (int l = 0; l < oriVert.bwWeight.Length; l++)
				{
					BoneWeight boneWeight = oriVert.bwWeight[l];
					bool flag = false;
					if ((list.Contains(boneWeight.boneIndex0) && boneWeight.weight0 > 0f) || (list.Contains(boneWeight.boneIndex1) && boneWeight.weight1 > 0f) || (list.Contains(boneWeight.boneIndex2) && boneWeight.weight2 > 0f) || (list.Contains(boneWeight.boneIndex3) && boneWeight.weight3 > 0f))
					{
						flag = true;
					}
					if (!flag)
					{
						array2[l] = -1;
					}
					else
					{
						Vector3 vector = transform.transform.InverseTransformPoint(componentInChildren.transform.TransformPoint(oriVert.vOriVert[l]));
						list2.Add(vector);
						list3.Add(oriVert.vOriNorm[l]);
						array2[l] = list2.Count - 1;
						BoneWeight boneWeight2 = new BoneWeight
						{
							weight0 = boneWeight.weight0,
							weight1 = boneWeight.weight1,
							weight2 = boneWeight.weight2,
							weight3 = boneWeight.weight3
						};
						int num4 = -1;
						int num5;
						dictionary3.TryGetValue(boneWeight.boneIndex0, out num5);
						int num6;
						dictionary3.TryGetValue(boneWeight.boneIndex1, out num6);
						int num7;
						dictionary3.TryGetValue(boneWeight.boneIndex2, out num7);
						dictionary3.TryGetValue(boneWeight.boneIndex3, out num4);
						if (num5 < 0)
						{
							boneWeight2.boneIndex0 = 0;
							boneWeight2.weight0 = 0f;
						}
						else
						{
							boneWeight2.boneIndex0 = num5;
						}
						if (num6 < 0)
						{
							boneWeight2.boneIndex1 = 0;
							boneWeight2.weight1 = 0f;
						}
						else
						{
							boneWeight2.boneIndex1 = num6;
						}
						if (num7 < 0)
						{
							boneWeight2.boneIndex2 = 0;
							boneWeight2.weight2 = 0f;
						}
						else
						{
							boneWeight2.boneIndex2 = num7;
						}
						if (num4 < 0)
						{
							boneWeight2.boneIndex3 = 0;
							boneWeight2.weight3 = 0f;
						}
						else
						{
							boneWeight2.boneIndex3 = num4;
						}
						list4.Add(boneWeight2);
					}
				}
				Vector2[] array3 = new Vector2[list2.Count];
				for (int m = 0; m < array2.Length; m++)
				{
					int num8 = array2[m];
					if (num8 >= 0)
					{
						array3[num8] = componentInChildren.sharedMesh.uv[m];
					}
				}
				GameObject gameObject4 = new GameObject();
				gameObject2.transform.parent = gameObject4.transform;
				gameObject2.transform.localPosition = Vector3.zero;
				gameObject2.transform.localRotation = Quaternion.identity;
				MeshFilter meshFilter = gameObject4.AddComponent<MeshFilter>();
				Mesh mesh = new Mesh();
				meshFilter.mesh = mesh;
				mesh.vertices = list2.ToArray();
				mesh.normals = list3.ToArray();
				mesh.subMeshCount = oriVert.nSubMeshCount;
				mesh.boneWeights = list4.ToArray();
				mesh.bindposes = list5.ToArray();
				mesh.uv = array3;
				for (int n = 0; n < oriVert.nSubMeshCount; n++)
				{
					List<int> list7 = new List<int>();
					for (int num9 = 0; num9 < oriVert.nSubMeshOriTri[n].Length; num9 += 3)
					{
						int num10 = oriVert.nSubMeshOriTri[n][num9];
						int num11 = oriVert.nSubMeshOriTri[n][num9 + 1];
						int num12 = oriVert.nSubMeshOriTri[n][num9 + 2];
						if (array2[num10] != -1 && array2[num11] != -1 && array2[num12] != -1)
						{
							list7.Add(array2[num10]);
							list7.Add(array2[num11]);
							list7.Add(array2[num12]);
						}
					}
					mesh.SetTriangles(list7.ToArray(), n);
				}
				mesh.RecalculateNormals();
				SkinnedMeshRenderer skinnedMeshRenderer = gameObject4.AddComponent<SkinnedMeshRenderer>();
				skinnedMeshRenderer.bones = list6.ToArray();
				skinnedMeshRenderer.sharedMesh = mesh;
				Material material = new Material(Shader.Find("CM3D2/Man"));
				material.SetFloat("_FloatValue2", this.handColorAlpha);
				material.SetColor("_Color", this.handColor);
				skinnedMeshRenderer.material = material;
				skinnedMeshRenderer.shadowCastingMode = ShadowCastingMode.On;
				skinnedMeshRenderer.enabled = true;
				global::UnityEngine.Object.DontDestroyOnLoad(gameObject4);
				gameObject4.SetActive(false);
				dictionary[text] = gameObject4;
			}
			maid.Uninit();
			global::UnityEngine.Object.Destroy(gameObject);
			return dictionary;
		}

		// Token: 0x060001C3 RID: 451 RVA: 0x000033B7 File Offset: 0x000015B7
		private bool GetOrCreateBool(IniSection section, string key, bool defaultValue)
		{
			return bool.Parse(this.GetOrCreateKey(section, key, defaultValue.ToString()));
		}

		// Token: 0x060001C4 RID: 452 RVA: 0x000033CD File Offset: 0x000015CD
		private float GetOrCreateFloat(IniSection section, string key, float defaultValue)
		{
			return float.Parse(this.GetOrCreateKey(section, key, defaultValue.ToString()));
		}

		// Token: 0x060001C5 RID: 453 RVA: 0x000033E3 File Offset: 0x000015E3
		private int GetOrCreateInt(IniSection section, string key, int defaultValue)
		{
			return int.Parse(this.GetOrCreateKey(section, key, defaultValue.ToString()));
		}

		// Token: 0x060001C6 RID: 454 RVA: 0x000033F9 File Offset: 0x000015F9
		private string GetOrCreateKey(IniSection section, string key, string defaultValue)
		{
			if (section.HasKey(key))
			{
				return section.GetKey(key).Value;
			}
			IniKey iniKey = section.CreateKey(key);
			iniKey.Value = defaultValue;
			return iniKey.Value;
		}

		// Token: 0x060001C7 RID: 455 RVA: 0x00003424 File Offset: 0x00001624
		private void Init()
		{
			if (!this.initialized)
			{
				if (!Settings.Instance.GetBoolValue("VIVEModelReplaceToolEnabled", true))
				{
					base.enabled = false;
					return;
				}
				this.LoadConfig();
				this.RegisterDefaultModels();
				this.initialized = true;
			}
		}

		// Token: 0x060001C8 RID: 456 RVA: 0x0000C100 File Offset: 0x0000A300
		private GameObject LoadCM3D2ModelAsGameObject(string modelFilename)
		{
			Maid maid = new GameObject().AddComponent<Maid>();
			maid.Initialize("Maid", false);
			TBodySkin slot = maid.body0.GetSlot(25);
			List<global::UnityEngine.Object> list = new List<global::UnityEngine.Object>();
			list.AddRange(slot.listDEL);
			TMorph tmorph = new TMorph(slot);
			GameObject gameObject = ImportCM.LoadSkinMesh_R(modelFilename, tmorph, "HandItemR", slot, LayerMask.NameToLayer("Default"));
			slot.listDEL = list;
			GameObject gameObject2 = global::UnityEngine.Object.Instantiate<GameObject>(gameObject);
			gameObject2.SetActive(false);
			global::UnityEngine.Object.DontDestroyOnLoad(gameObject2);
			maid.Uninit();
			global::UnityEngine.Object.Destroy(gameObject);
			global::UnityEngine.Object.Destroy(maid);
			return gameObject2;
		}

		// Token: 0x060001C9 RID: 457 RVA: 0x0000C194 File Offset: 0x0000A394
		private void LoadConfig()
		{
			IniFile iniFile = ((!File.Exists(this.IniFileName)) ? new IniFile() : IniFile.FromFile(this.IniFileName));
			IniSection iniSection = ((!iniFile.HasSection("VIVEModel")) ? iniFile.CreateSection("VIVEModel") : iniFile.GetSection("VIVEModel"));
			this.handModelEnabled = this.GetOrCreateBool(iniSection, "HandModelEnabled", true);
			this.handColor = new Color(this.GetOrCreateFloat(iniSection, "HandColor_R", 0f), this.GetOrCreateFloat(iniSection, "HandColor_G", 1f), this.GetOrCreateFloat(iniSection, "HandColor_B", 0f));
			this.handColorAlpha = this.GetOrCreateFloat(iniSection, "HandColor_A", 0.2f);
			this.handPos = new Vector3(this.GetOrCreateFloat(iniSection, "HandPos_X", 0f), this.GetOrCreateFloat(iniSection, "HandPos_Y", 0f), this.GetOrCreateFloat(iniSection, "HandPos_Z", -0.13f));
			this.handRot = new Vector3(this.GetOrCreateFloat(iniSection, "HandRot_X", 180f), this.GetOrCreateFloat(iniSection, "HandRot_Y", 90f), this.GetOrCreateFloat(iniSection, "HandRot_Z", 0f));
			if (iniSection.HasKey("CustomModelCount"))
			{
				this.customModelInfoCount = this.GetOrCreateInt(iniSection, "CustomModelCount", 0);
				for (int i = 0; i < this.customModelInfoCount; i++)
				{
					string text = "CustomModel_" + i;
					string orCreateKey = this.GetOrCreateKey(iniSection, text + "_ID", "model." + i);
					string orCreateKey2 = this.GetOrCreateKey(iniSection, text + "_Label", "モデル " + i);
					string orCreateKey3 = this.GetOrCreateKey(iniSection, text + "_Model", "");
					Vector3 vector = new Vector3(this.GetOrCreateFloat(iniSection, text + "_Pos_X", 0f), this.GetOrCreateFloat(iniSection, text + "_Pos_Y", 0f), this.GetOrCreateFloat(iniSection, text + "_Pos_Z", 0f));
					Vector3 vector2 = new Vector3(this.GetOrCreateFloat(iniSection, text + "_Rot_X", 0f), this.GetOrCreateFloat(iniSection, text + "_Rot_Y", 0f), this.GetOrCreateFloat(iniSection, text + "_Rot_Z", 0f));
					bool orCreateBool = this.GetOrCreateBool(iniSection, text + "_HidePointer", true);
					string orCreateKey4 = this.GetOrCreateKey(iniSection, text + "_EnabledHand", "LR");
					this.AddModel(orCreateKey, orCreateKey2, orCreateKey3, vector, vector2, orCreateBool, orCreateKey4);
				}
			}
			else
			{
				this.customModelInfoCount = this.GetOrCreateInt(iniSection, "CustomModelCount", 1);
				string text2 = "CustomModel_" + 0;
				string orCreateKey5 = this.GetOrCreateKey(iniSection, text2 + "_ID", "vibepink");
				string orCreateKey6 = this.GetOrCreateKey(iniSection, text2 + "_Label", "ピンクバイブ");
				string orCreateKey7 = this.GetOrCreateKey(iniSection, text2 + "_Model", "accvag_vibepink.model");
				Vector3 vector3 = new Vector3(this.GetOrCreateFloat(iniSection, text2 + "_Pos_X", 0f), this.GetOrCreateFloat(iniSection, text2 + "_Pos_Y", -0.01f), this.GetOrCreateFloat(iniSection, text2 + "_Pos_Z", -0.05f));
				Vector3 vector4 = new Vector3(this.GetOrCreateFloat(iniSection, text2 + "_Rot_X", 0f), this.GetOrCreateFloat(iniSection, text2 + "_Rot_Y", 180f), this.GetOrCreateFloat(iniSection, text2 + "_Rot_Z", 180f));
				bool orCreateBool2 = this.GetOrCreateBool(iniSection, text2 + "_HidePointer", true);
				string orCreateKey8 = this.GetOrCreateKey(iniSection, text2 + "_EnabledHand", "LR");
				this.AddModel(orCreateKey5, orCreateKey6, orCreateKey7, vector3, vector4, orCreateBool2, orCreateKey8);
			}
			iniFile.Save(this.IniFileName);
		}

		// Token: 0x060001CA RID: 458 RVA: 0x0000345B File Offset: 0x0000165B
		public bool ModelExist(string id)
		{
			return this.models.ContainsKey(id);
		}

		// Token: 0x060001CB RID: 459 RVA: 0x0000C5BC File Offset: 0x0000A7BC
		private void RegisterDefaultModels()
		{
			VIVEModelReplaceTool.ModelInfo modelInfo = new VIVEModelReplaceTool.ModelInfo(VIVEModelReplaceTool.MODEL_DEFAULT, "デフォルト", null, Vector3.zero, Vector3.zero, false, "LR");
			this.models.Add(VIVEModelReplaceTool.MODEL_DEFAULT, modelInfo);
			this.models_L.Insert(0, modelInfo);
			this.models_R.Insert(0, modelInfo);
			Debug.Log("Register Default Models: " + this.handModelEnabled.ToString());
			if (this.handModelEnabled)
			{
				try
				{
					Dictionary<string, GameObject> dictionary = this.ExtractHandFromManModel();
					VIVEModelReplaceTool.ModelInfo modelInfo2 = new VIVEModelReplaceTool.ModelInfo(VIVEModelReplaceTool.MODEL_MAN_HAND_L, "左手", null, this.handPos, this.handRot, this.handHideTextAndPointer, "L")
					{
						cacheGameObj = dictionary["L"]
					};
					VIVEModelReplaceTool.ModelInfo modelInfo3 = new VIVEModelReplaceTool.ModelInfo(VIVEModelReplaceTool.MODEL_MAN_HAND_R, "右手", null, this.handPos, this.handRot, this.handHideTextAndPointer, "R")
					{
						cacheGameObj = dictionary["R"]
					};
					dictionary["L"].transform.parent = base.transform;
					dictionary["R"].transform.parent = base.transform;
					this.models.Add(VIVEModelReplaceTool.MODEL_MAN_HAND_L, modelInfo2);
					this.models.Add(VIVEModelReplaceTool.MODEL_MAN_HAND_R, modelInfo3);
					this.models_L.Insert(1, modelInfo2);
					this.models_R.Insert(1, modelInfo3);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
					Debug.Log(ex);
					Debug.Log("Failed to load Hand Model");
				}
			}
		}

		// Token: 0x060001CC RID: 460 RVA: 0x00003469 File Offset: 0x00001669
		public void SetModel(SteamVR_TrackedObject trackedObject, string id)
		{
			if (!this.models.ContainsKey(id))
			{
				return;
			}
			this.SetVIVEControllerModel_Internal(trackedObject, this.models[id]);
		}

		// Token: 0x060001CD RID: 461 RVA: 0x0000C750 File Offset: 0x0000A950
		public GameObject SetModel(GameObject obj, string id)
		{
			if (!this.models.ContainsKey(id))
			{
				return null;
			}
			VIVEModelReplaceTool.ModelInfo modelInfo = this.models[id];
			Transform transform = CMT.SearchObjName(obj.transform, "__ControllerReplacedModel", true);
			if (transform == null)
			{
				transform = new GameObject("__ControllerReplacedModel")
				{
					transform = 
					{
						parent = obj.transform,
						localPosition = Vector3.zero,
						localRotation = Quaternion.identity
					}
				}.transform;
			}
			if (modelInfo.id != VIVEModelReplaceTool.MODEL_DEFAULT)
			{
				transform.gameObject.SetActive(true);
				bool flag = false;
				for (int i = 0; i < transform.childCount; i++)
				{
					Transform child = transform.GetChild(i);
					if (child.name == modelInfo.id && child.gameObject != null)
					{
						child.gameObject.SetActive(true);
						flag = true;
					}
					else if (child.gameObject != null)
					{
						child.gameObject.SetActive(false);
					}
				}
				if (!flag)
				{
					GameObject gameObject = global::UnityEngine.Object.Instantiate<GameObject>(modelInfo.cacheGameObj, Vector3.zero, Quaternion.identity);
					gameObject.name = modelInfo.id;
					gameObject.transform.parent = transform;
					gameObject.transform.localPosition = modelInfo.pos;
					gameObject.transform.localRotation = Quaternion.Euler(modelInfo.rot);
					gameObject.SetActive(true);
				}
			}
			else
			{
				transform.gameObject.SetActive(false);
			}
			return transform.gameObject;
		}

		// Token: 0x060001CE RID: 462 RVA: 0x0000C8D4 File Offset: 0x0000AAD4
		private void SetVIVEControllerModel_Internal(SteamVR_TrackedObject trackedObject, VIVEModelReplaceTool.ModelInfo info)
		{
			Transform transform = CMT.SearchObjName(trackedObject.gameObject.transform, "__VIVEControllerReplacedModel", true);
			if (transform == null)
			{
				transform = new GameObject("__VIVEControllerReplacedModel")
				{
					transform = 
					{
						parent = trackedObject.transform,
						localPosition = Vector3.zero,
						localRotation = Quaternion.identity
					}
				}.transform;
			}
			if (info.id != VIVEModelReplaceTool.MODEL_DEFAULT)
			{
				transform.gameObject.SetActive(true);
				this.EnableDisableDefaultRenderModel(trackedObject, false);
				bool flag = false;
				for (int i = 0; i < transform.childCount; i++)
				{
					Transform child = transform.GetChild(i);
					if (child.name == info.id && child.gameObject != null)
					{
						child.gameObject.SetActive(true);
						flag = true;
					}
					else if (child.gameObject != null)
					{
						child.gameObject.SetActive(false);
					}
				}
				if (!flag)
				{
					GameObject gameObject = global::UnityEngine.Object.Instantiate<GameObject>(info.cacheGameObj, Vector3.zero, Quaternion.identity);
					gameObject.name = info.id;
					gameObject.transform.parent = transform;
					gameObject.transform.localPosition = info.pos;
					gameObject.transform.localRotation = Quaternion.Euler(info.rot);
					gameObject.SetActive(true);
				}
			}
			else
			{
				transform.gameObject.SetActive(false);
				this.EnableDisableDefaultRenderModel(trackedObject, true);
			}
			MenuTool component = trackedObject.GetComponent<MenuTool>();
			if (component != null)
			{
				component.SetPointerAndTextVisibilityByModel(!info.hideTextAndPointer);
			}
		}

		// Token: 0x060001CF RID: 463 RVA: 0x0000348D File Offset: 0x0000168D
		private void Start()
		{
			this.Init();
		}

		// Token: 0x060001D0 RID: 464 RVA: 0x00003495 File Offset: 0x00001695
		private void vout(Vector3 v)
		{
			Console.WriteLine(string.Format("({0}, {1}, {2})", v.x, v.y, v.z));
		}

		// Token: 0x0400013A RID: 314
		public Dictionary<string, VIVEModelReplaceTool.ModelInfo> models = new Dictionary<string, VIVEModelReplaceTool.ModelInfo>();

		// Token: 0x0400013B RID: 315
		public List<VIVEModelReplaceTool.ModelInfo> models_L = new List<VIVEModelReplaceTool.ModelInfo>();

		// Token: 0x0400013C RID: 316
		public List<VIVEModelReplaceTool.ModelInfo> models_R = new List<VIVEModelReplaceTool.ModelInfo>();

		// Token: 0x0400013D RID: 317
		private static VIVEModelReplaceTool _instance;

		// Token: 0x0400013E RID: 318
		public static string MODEL_DEFAULT = "__DEFAULT";

		// Token: 0x0400013F RID: 319
		public static string MODEL_MAN_HAND_L = "__ManHand_L";

		// Token: 0x04000140 RID: 320
		public static string MODEL_MAN_HAND_R = "__ManHand_R";

		// Token: 0x04000141 RID: 321
		public readonly string IniFileName = GripMovePlugin.PluginDataPath + "\\gripmove_models.ini";

		// Token: 0x04000142 RID: 322
		private bool handModelEnabled = true;

		// Token: 0x04000143 RID: 323
		private Color handColor;

		// Token: 0x04000144 RID: 324
		private float handColorAlpha;

		// Token: 0x04000145 RID: 325
		private bool handHideTextAndPointer = true;

		// Token: 0x04000146 RID: 326
		private Vector3 handPos;

		// Token: 0x04000147 RID: 327
		private Vector3 handRot;

		// Token: 0x04000148 RID: 328
		private bool initialized;

		// Token: 0x04000149 RID: 329
		private int customModelInfoCount;

		// Token: 0x02000036 RID: 54
		public class ModelInfo
		{
			// Token: 0x060001D1 RID: 465 RVA: 0x000034C7 File Offset: 0x000016C7
			public ModelInfo(string id, string label, string modelFilename, Vector3 pos, Vector3 rot, bool hideTextAndPointer, string LR = "LR")
			{
				this.id = id;
				this.label = label;
				this.modelFilename = modelFilename;
				this.pos = pos;
				this.rot = rot;
				this.hideTextAndPointer = hideTextAndPointer;
				this.LR = LR;
			}

			// Token: 0x0400014A RID: 330
			public string id;

			// Token: 0x0400014B RID: 331
			public string label;

			// Token: 0x0400014C RID: 332
			public string modelFilename;

			// Token: 0x0400014D RID: 333
			public Vector3 pos;

			// Token: 0x0400014E RID: 334
			public Vector3 rot;

			// Token: 0x0400014F RID: 335
			public bool hideTextAndPointer;

			// Token: 0x04000150 RID: 336
			public GameObject cacheGameObj;

			// Token: 0x04000151 RID: 337
			public string LR;
		}
	}
}

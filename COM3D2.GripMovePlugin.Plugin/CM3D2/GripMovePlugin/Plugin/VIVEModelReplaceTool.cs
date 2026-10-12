using System;
using System.Collections.Generic;
using System.IO;
using ExIni;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace CM3D2.GripMovePlugin.Plugin;

internal class VIVEModelReplaceTool : MonoBehaviour
{
	private static VIVEModelReplaceTool _instance;

	public static string MODEL_DEFAULT = "__DEFAULT";

	public static string MODEL_MAN_HAND_L = "__ManHand_L";

	public static string MODEL_MAN_HAND_R = "__ManHand_R";

	public readonly string IniFileName = GripMovePlugin.PluginDataPath + "\\gripmove_models.ini";

	private readonly bool handHideTextAndPointer = true;

	private int customModelInfoCount;

	private Color handColor;

	private float handColorAlpha;

	private bool handModelEnabled = true;

	private Vector3 handPos;

	private Vector3 handRot;

	private bool initialized;

	public Dictionary<string, ModelInfo> models = new();

	public List<ModelInfo> models_L = new();

	public List<ModelInfo> models_R = new();

	public static VIVEModelReplaceTool Instance
	{
		get
		{
			if (_instance == null)
			{
				var gameObject = new GameObject("VIVEModelReplaceTool");
				_instance = gameObject.AddComponent<VIVEModelReplaceTool>();
				_instance.Init();
				DontDestroyOnLoad(gameObject);
			}

			return _instance;
		}
	}

	private void Start() => Init();

	public void AddModel(string id, string label, string modelFilename, Vector3 pos, Vector3 rot,
		bool hideTextAndPointer, string LR = "LR")
	{
		if (id == null || id == "" || id == MODEL_DEFAULT)
		{
			Console.WriteLine("Invalid ID:");
			return;
		}

		if (models.ContainsKey(id))
		{
			return;
		}

		GameObject gameObject = LoadCM3D2ModelAsGameObject(modelFilename);
		if (gameObject != null)
		{
			gameObject.name = id;
			gameObject.transform.parent = this.gameObject.transform;
			var modelInfo =
				new ModelInfo(id, label, modelFilename, pos, rot, hideTextAndPointer, LR)
				{
					cacheGameObj = gameObject
				};
			models[id] = modelInfo;
			if (LR.Contains("L"))
			{
				models_L.Add(modelInfo);
			}

			if (LR.Contains("R"))
			{
				models_R.Add(modelInfo);
			}
		}
	}

	private void EnableDisableDefaultRenderModel(SteamVR_TrackedObject trackedObject, bool enabled)
	{
		foreach (SteamVR_TrackedObject steamVR_TrackedObject in
		         FindObjectsOfType<SteamVR_TrackedObject>())
		{
			if (steamVR_TrackedObject.index == trackedObject.index)
			{
				SteamVR_RenderModel steamVR_RenderModel =
					steamVR_TrackedObject.GetComponent<SteamVR_RenderModel>();
				if (steamVR_RenderModel != null)
				{
					Renderer[] componentsInChildren =
						steamVR_RenderModel.gameObject.GetComponentsInChildren<Renderer>();
					for (int j = 0; j < componentsInChildren.Length; j++)
					{
						componentsInChildren[j].enabled = enabled;
					}
				}
				else if (steamVR_RenderModel == null)
				{
					steamVR_RenderModel =
						steamVR_TrackedObject.GetComponentInChildren<SteamVR_RenderModel>();
					if (steamVR_RenderModel != null)
					{
						steamVR_RenderModel.gameObject.SetActive(enabled);
					}
				}
			}
		}
	}

	private Dictionary<string, GameObject> ExtractHandFromManModel()
	{
		var gameObject = new GameObject();
		Maid maid = gameObject.AddComponent<Maid>();
		maid.Initialize("Man", true);
		maid.DutPropAll();
		maid.AllProcPropSeqStart();
		TBodySkin slot = maid.body0.GetSlot(0);
		var tmorph = new TMorph(slot);
		SkinnedMeshRenderer componentInChildren = ImportCM
			.LoadSkinMesh_R("mbody.model", tmorph, "body", slot, LayerMask.NameToLayer("Default"))
			.GetComponentInChildren<SkinnedMeshRenderer>();
		TBodySkin.OriVert oriVert = slot.m_OriVert;
		Transform[] bones = componentInChildren.bones;
		var dictionary = new Dictionary<string, GameObject>();
		foreach (string text in new[] { "L", "R" })
		{
			var list = new List<int>();
			int[] array2 = new int[oriVert.VCount];
			var list2 = new List<Vector3>();
			var list3 = new List<Vector3>();
			var list4 = new List<BoneWeight>();
			var list5 = new List<Matrix4x4>();
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

			var dictionary2 = new Dictionary<Transform, Transform>();
			var list6 = new List<Transform>();
			var dictionary3 = new Dictionary<int, int>();
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
					var gameObject3 = new GameObject(transform3.gameObject.name);
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
				if (transform4 != gameObject2)
				{
					Transform transform5 = dictionary2[transform4];
					Transform parent = transform4.parent;
					if (!dictionary2.ContainsKey(parent))
					{
						Debug.Log(string.Concat("Parent of ", transform4.name, " ", parent.name,
							" not found"));
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
				if ((list.Contains(boneWeight.boneIndex0) && boneWeight.weight0 > 0f) ||
				    (list.Contains(boneWeight.boneIndex1) && boneWeight.weight1 > 0f) ||
				    (list.Contains(boneWeight.boneIndex2) && boneWeight.weight2 > 0f) ||
				    (list.Contains(boneWeight.boneIndex3) && boneWeight.weight3 > 0f))
				{
					flag = true;
				}

				if (!flag)
				{
					array2[l] = -1;
				}
				else
				{
					Vector3 vector = transform.transform.InverseTransformPoint(
						componentInChildren.transform.TransformPoint(oriVert.vOriVert[l]));
					list2.Add(vector);
					list3.Add(oriVert.vOriNorm[l]);
					array2[l] = list2.Count - 1;
					var boneWeight2 = default(BoneWeight);
					boneWeight2.weight0 = boneWeight.weight0;
					boneWeight2.weight1 = boneWeight.weight1;
					boneWeight2.weight2 = boneWeight.weight2;
					boneWeight2.weight3 = boneWeight.weight3;
					BoneWeight boneWeight3 = boneWeight2;
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
						boneWeight3.boneIndex0 = 0;
						boneWeight3.weight0 = 0f;
					}
					else
					{
						boneWeight3.boneIndex0 = num5;
					}

					if (num6 < 0)
					{
						boneWeight3.boneIndex1 = 0;
						boneWeight3.weight1 = 0f;
					}
					else
					{
						boneWeight3.boneIndex1 = num6;
					}

					if (num7 < 0)
					{
						boneWeight3.boneIndex2 = 0;
						boneWeight3.weight2 = 0f;
					}
					else
					{
						boneWeight3.boneIndex2 = num7;
					}

					if (num4 < 0)
					{
						boneWeight3.boneIndex3 = 0;
						boneWeight3.weight3 = 0f;
					}
					else
					{
						boneWeight3.boneIndex3 = num4;
					}

					list4.Add(boneWeight3);
				}
			}

			var array3 = new Vector2[list2.Count];
			for (int m = 0; m < array2.Length; m++)
			{
				int num8 = array2[m];
				if (num8 >= 0)
				{
					array3[num8] = componentInChildren.sharedMesh.uv[m];
				}
			}

			var gameObject4 = new GameObject();
			gameObject2.transform.parent = gameObject4.transform;
			gameObject2.transform.localPosition = Vector3.zero;
			gameObject2.transform.localRotation = Quaternion.identity;
			MeshFilter meshFilter = gameObject4.AddComponent<MeshFilter>();
			var mesh = new Mesh();
			meshFilter.mesh = mesh;
			mesh.vertices = list2.ToArray();
			mesh.normals = list3.ToArray();
			mesh.subMeshCount = oriVert.nSubMeshCount;
			mesh.boneWeights = list4.ToArray();
			mesh.bindposes = list5.ToArray();
			mesh.uv = array3;
			for (int n = 0; n < oriVert.nSubMeshCount; n++)
			{
				var list7 = new List<int>();
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
			SkinnedMeshRenderer skinnedMeshRenderer =
				gameObject4.AddComponent<SkinnedMeshRenderer>();
			skinnedMeshRenderer.bones = list6.ToArray();
			skinnedMeshRenderer.sharedMesh = mesh;
			var material = new Material(Shader.Find("CM3D2/Man"));
			material.SetFloat("_FloatValue2", handColorAlpha);
			material.SetColor("_Color", handColor);
			skinnedMeshRenderer.material = material;
			skinnedMeshRenderer.shadowCastingMode = (ShadowCastingMode)1;
			skinnedMeshRenderer.enabled = true;
			DontDestroyOnLoad(gameObject4);
			gameObject4.SetActive(false);
			dictionary[text] = gameObject4;
		}

		maid.Uninit();
		Destroy(gameObject);
		return dictionary;
	}

	private bool GetOrCreateBool(IniSection section, string key, bool defaultValue) =>
		bool.Parse(GetOrCreateKey(section, key, defaultValue.ToString()));

	private float GetOrCreateFloat(IniSection section, string key, float defaultValue) =>
		float.Parse(GetOrCreateKey(section, key, defaultValue.ToString()));

	private int GetOrCreateInt(IniSection section, string key, int defaultValue) =>
		int.Parse(GetOrCreateKey(section, key, defaultValue.ToString()));

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

	private void Init()
	{
		if (!initialized)
		{
			if (!Settings.Instance.GetBoolValue("VIVEModelReplaceToolEnabled", true))
			{
				enabled = false;
				return;
			}

			LoadConfig();
			RegisterDefaultModels();
			initialized = true;
		}
	}

	private GameObject LoadCM3D2ModelAsGameObject(string modelFilename)
	{
		Maid maid = new GameObject().AddComponent<Maid>();
		maid.Initialize("Maid", false);
		TBodySkin slot = maid.body0.GetSlot(25);
		var list = new List<Object>();
		list.AddRange(slot.listDEL);
		var tmorph = new TMorph(slot);
		GameObject gameObject = ImportCM.LoadSkinMesh_R(modelFilename, tmorph, "HandItemR", slot,
			LayerMask.NameToLayer("Default"));
		slot.listDEL = list;
		GameObject gameObject2 = Instantiate(gameObject);
		gameObject2.SetActive(false);
		DontDestroyOnLoad(gameObject2);
		maid.Uninit();
		Destroy(gameObject);
		Destroy(maid);
		return gameObject2;
	}

	private void LoadConfig()
	{
		IniFile iniFile = !File.Exists(IniFileName) ? new IniFile() : IniFile.FromFile(IniFileName);
		IniSection iniSection = !iniFile.HasSection("VIVEModel")
			? iniFile.CreateSection("VIVEModel")
			: iniFile.GetSection("VIVEModel");
		handModelEnabled = GetOrCreateBool(iniSection, "HandModelEnabled", true);
		handColor = new Color(GetOrCreateFloat(iniSection, "HandColor_R", 0f),
			GetOrCreateFloat(iniSection, "HandColor_G", 1f),
			GetOrCreateFloat(iniSection, "HandColor_B", 0f));
		handColorAlpha = GetOrCreateFloat(iniSection, "HandColor_A", 0.2f);
		handPos = new Vector3(GetOrCreateFloat(iniSection, "HandPos_X", 0f),
			GetOrCreateFloat(iniSection, "HandPos_Y", 0f),
			GetOrCreateFloat(iniSection, "HandPos_Z", -0.13f));
		handRot = new Vector3(GetOrCreateFloat(iniSection, "HandRot_X", 180f),
			GetOrCreateFloat(iniSection, "HandRot_Y", 90f),
			GetOrCreateFloat(iniSection, "HandRot_Z", 0f));
		if (iniSection.HasKey("CustomModelCount"))
		{
			customModelInfoCount = GetOrCreateInt(iniSection, "CustomModelCount", 0);
			for (int i = 0; i < customModelInfoCount; i++)
			{
				string text = "CustomModel_" + i;
				string orCreateKey = GetOrCreateKey(iniSection, text + "_ID", "model." + i);
				string orCreateKey2 = GetOrCreateKey(iniSection, text + "_Label", "モデル " + i);
				string orCreateKey3 = GetOrCreateKey(iniSection, text + "_Model", "");
				Vector3 vector;
				vector = new Vector3(GetOrCreateFloat(iniSection, text + "_Pos_X", 0f),
					GetOrCreateFloat(iniSection, text + "_Pos_Y", 0f),
					GetOrCreateFloat(iniSection, text + "_Pos_Z", 0f));
				Vector3 vector2;
				vector2 = new Vector3(GetOrCreateFloat(iniSection, text + "_Rot_X", 0f),
					GetOrCreateFloat(iniSection, text + "_Rot_Y", 0f),
					GetOrCreateFloat(iniSection, text + "_Rot_Z", 0f));
				bool orCreateBool = GetOrCreateBool(iniSection, text + "_HidePointer", true);
				string orCreateKey4 = GetOrCreateKey(iniSection, text + "_EnabledHand", "LR");
				AddModel(orCreateKey, orCreateKey2, orCreateKey3, vector, vector2, orCreateBool,
					orCreateKey4);
			}
		}
		else
		{
			customModelInfoCount = GetOrCreateInt(iniSection, "CustomModelCount", 1);
			string text2 = "CustomModel_" + 0;
			string orCreateKey5 = GetOrCreateKey(iniSection, text2 + "_ID", "vibepink");
			string orCreateKey6 = GetOrCreateKey(iniSection, text2 + "_Label", "ピンクバイブ");
			string orCreateKey7 =
				GetOrCreateKey(iniSection, text2 + "_Model", "accvag_vibepink.model");
			Vector3 vector3;
			vector3 = new Vector3(GetOrCreateFloat(iniSection, text2 + "_Pos_X", 0f),
				GetOrCreateFloat(iniSection, text2 + "_Pos_Y", -0.01f),
				GetOrCreateFloat(iniSection, text2 + "_Pos_Z", -0.05f));
			Vector3 vector4;
			vector4 = new Vector3(GetOrCreateFloat(iniSection, text2 + "_Rot_X", 0f),
				GetOrCreateFloat(iniSection, text2 + "_Rot_Y", 180f),
				GetOrCreateFloat(iniSection, text2 + "_Rot_Z", 180f));
			bool orCreateBool2 = GetOrCreateBool(iniSection, text2 + "_HidePointer", true);
			string orCreateKey8 = GetOrCreateKey(iniSection, text2 + "_EnabledHand", "LR");
			AddModel(orCreateKey5, orCreateKey6, orCreateKey7, vector3, vector4, orCreateBool2,
				orCreateKey8);
		}

		iniFile.Save(IniFileName);
	}

	public bool ModelExist(string id) => models.ContainsKey(id);

	private void RegisterDefaultModels()
	{
		var modelInfo =
			new ModelInfo(MODEL_DEFAULT, "デフォルト", null, Vector3.zero, Vector3.zero, false);
		models.Add(MODEL_DEFAULT, modelInfo);
		models_L.Insert(0, modelInfo);
		models_R.Insert(0, modelInfo);
		Debug.Log("Register Default Models: " + handModelEnabled);
		if (handModelEnabled)
		{
			try
			{
				Dictionary<string, GameObject> dictionary = ExtractHandFromManModel();
				var modelInfo2 =
					new ModelInfo(MODEL_MAN_HAND_L, "左手", null, handPos, handRot,
						handHideTextAndPointer, "L") { cacheGameObj = dictionary["L"] };
				var modelInfo3 =
					new ModelInfo(MODEL_MAN_HAND_R, "右手", null, handPos, handRot,
						handHideTextAndPointer, "R") { cacheGameObj = dictionary["R"] };
				dictionary["L"].transform.parent = transform;
				dictionary["R"].transform.parent = transform;
				models.Add(MODEL_MAN_HAND_L, modelInfo2);
				models.Add(MODEL_MAN_HAND_R, modelInfo3);
				models_L.Insert(1, modelInfo2);
				models_R.Insert(1, modelInfo3);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				Debug.Log(ex);
				Debug.Log("Failed to load Hand Model");
			}
		}
	}

	public void SetModel(SteamVR_TrackedObject trackedObject, string id)
	{
		if (!models.ContainsKey(id))
		{
			return;
		}

		SetVIVEControllerModel_Internal(trackedObject, models[id]);
	}

	public GameObject SetModel(GameObject obj, string id)
	{
		if (!models.ContainsKey(id))
		{
			return null;
		}

		ModelInfo modelInfo = models[id];
		Transform transform = CMT.SearchObjName(obj.transform, "__ControllerReplacedModel");
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

		if (modelInfo.id != MODEL_DEFAULT)
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
				GameObject gameObject = Instantiate(modelInfo.cacheGameObj, Vector3.zero,
					Quaternion.identity);
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

	private void SetVIVEControllerModel_Internal(SteamVR_TrackedObject trackedObject,
		ModelInfo info)
	{
		Transform transform = CMT.SearchObjName(trackedObject.gameObject.transform,
			"__VIVEControllerReplacedModel");
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

		if (info.id != MODEL_DEFAULT)
		{
			transform.gameObject.SetActive(true);
			EnableDisableDefaultRenderModel(trackedObject, false);
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
				GameObject gameObject =
					Instantiate(info.cacheGameObj, Vector3.zero, Quaternion.identity);
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
			EnableDisableDefaultRenderModel(trackedObject, true);
		}

		MenuTool component = trackedObject.GetComponent<MenuTool>();
		if (component != null)
		{
			component.SetPointerAndTextVisibilityByModel(!info.hideTextAndPointer);
		}
	}

	private void vout(Vector3 v) => Console.WriteLine("({0}, {1}, {2})", v.x, v.y, v.z);

	public class ModelInfo
	{
		public string LR;

		public GameObject cacheGameObj;

		public bool hideTextAndPointer;

		public string id;

		public string label;

		public string modelFilename;

		public Vector3 pos;

		public Vector3 rot;

		public ModelInfo(string id, string label, string modelFilename, Vector3 pos, Vector3 rot,
			bool hideTextAndPointer, string LR = "LR")
		{
			this.id = id;
			this.label = label;
			this.modelFilename = modelFilename;
			this.pos = pos;
			this.rot = rot;
			this.hideTextAndPointer = hideTextAndPointer;
			this.LR = LR;
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin;

public class IKTool : MonoBehaviour
{
	public static IKTool instance;

	private static readonly MethodInfo m_SetSelectMaid =
		typeof(PlacementWindow).GetMethod("SetSelectMaid",
			BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod);

	private readonly string[] IK_Targets_Base = new[]
	{
		"Bip01 R Hand", "Bip01 R UpperArm", "Bip01 R Forearm", "Bip01 R Clavicle",
		"Bip01 R Foot", "Bip01 R Calf", "Bip01 R Thigh", "Bip01 L Hand", "Bip01 L UpperArm",
		"Bip01 L Forearm", "Bip01 L Clavicle", "Bip01 L Foot", "Bip01 L Calf", "Bip01 L Thigh",
		"Bip01 Spine", "Bip01 Spine0a", "Bip01 Spine1", "Bip01 Spine1a", "Bip01 Neck",
		"Bip01 Head"
	};

	private readonly string[] IK_Targets_Fingers = new[]
	{
		"Bip01 L Finger0", "Bip01 L Finger01", "Bip01 L Finger02", "Bip01 L Finger0Nub",
		"Bip01 L Finger1", "Bip01 L Finger11", "Bip01 L Finger12", "Bip01 L Finger1Nub",
		"Bip01 L Finger2", "Bip01 L Finger21", "Bip01 L Finger22", "Bip01 L Finger2Nub",
		"Bip01 L Finger3", "Bip01 L Finger31", "Bip01 L Finger32", "Bip01 L Finger3Nub",
		"Bip01 L Finger4", "Bip01 L Finger41", "Bip01 L Finger42", "Bip01 L Finger4Nub",
		"Bip01 R Finger0", "Bip01 R Finger01", "Bip01 R Finger02", "Bip01 R Finger0Nub",
		"Bip01 R Finger1", "Bip01 R Finger11", "Bip01 R Finger12", "Bip01 R Finger1Nub",
		"Bip01 R Finger2", "Bip01 R Finger21", "Bip01 R Finger22", "Bip01 R Finger2Nub",
		"Bip01 R Finger3", "Bip01 R Finger31", "Bip01 R Finger32", "Bip01 R Finger3Nub",
		"Bip01 R Finger4", "Bip01 R Finger41", "Bip01 R Finger42", "Bip01 R Finger4Nub"
	};

	private readonly string[] IK_Targets_IK = new[]
	{
		"_IK_vagina", "_IK_anal", "_IK_hipL", "_IK_hipR", "_IK_hutanari", "_IK_thighL",
		"_IK_calfL", "_IK_footL", "_IK_thighR", "_IK_calfR", "_IK_footR", "_IK_hara",
		"_IK_UpperArmL", "_IK_ForeArmL", "_IK_handL", "_IK_UpperArmR", "_IK_ForeArmR",
		"_IK_handR", "_IK_hohoL", "_IK_hohoR", "_IK_muneL", "_IK_muneR"
	};

	private string[] IK_Targets = new string[0];

	private Action<XElement> defaultOnDeserializeEventHandler;

	private Func<XElement> defaultOnSerializeEventHandler;

	private bool enableFingerIK = true;

	private bool enableItemIK = true;

	private bool hitCheckDebugEnabled;

	private bool hookPhotoSaveLoad;

	private Dictionary<Maid, Dictionary<string, Vector3>> maidBoneLastPos = new();

	private Dictionary<Maid, Dictionary<string, Quaternion>> maidBoneLastRot = new();

	private Dictionary<Maid, Dictionary<string, Transform>> maidBones = new();

	private Dictionary<string, Dictionary<string, Vector3>> maidLoadIKPos = new();

	private Dictionary<string, Dictionary<string, Quaternion>> maidLoadIKRot = new();

	private bool markerIsEnable;

	private bool markerIsVisible = true;

	private float markerOverlayAlpha = 0.4f;

	private Material markerSharedMaterial;

	private Material markerSharedMaterial_ItemIK;

	private bool markerShowOverlay = true;

	private float markerSize = 0.05f;

	private float markerSizeFinger = 0.005f;

	private float markerSizeItemIK = 0.01f;

	private PhotoWindowManager photoWindowManager;

	private PlacementWindow placementWindow;

	private PoseEditWindow poseEditWindow;

	private int refs;

	private void Awake()
	{
	}

	private void Start()
	{
		markerIsVisible = Settings.Instance.GetBoolValue("IKToolShowMarker", true);
		enableFingerIK = Settings.Instance.GetBoolValue("IKToolEnableFingerIK", true);
		enableItemIK = Settings.Instance.GetBoolValue("IKToolEnableItemIK", true);
		markerSize = Settings.Instance.GetFloatValue("IKToolMarkerSize", 0.05f);
		markerSizeFinger = Settings.Instance.GetFloatValue("IKToolMarkerSizeFinger", 0.005f);
		markerSizeItemIK = Settings.Instance.GetFloatValue("IKToolMarkerSizeItemIK", 0.01f);
		markerShowOverlay = Settings.Instance.GetBoolValue("IKToolMarkerShowAsOverlay", true);
		markerOverlayAlpha = Settings.Instance.GetFloatValue("IKToolMarkerOverlayAlpha", 0.4f);
		hitCheckDebugEnabled = Settings.Instance.GetBoolValue("IKToolHitCheckDebugEnabled", false);
		var list = new List<string>();
		for (int i = 0; i < IK_Targets_Base.Length; i++)
		{
			list.Add(IK_Targets_Base[i]);
		}

		if (enableFingerIK)
		{
			for (int j = 0; j < IK_Targets_Fingers.Length; j++)
			{
				list.Add(IK_Targets_Fingers[j]);
			}
		}

		if (enableItemIK)
		{
			for (int k = 0; k < IK_Targets_IK.Length; k++)
			{
				list.Add(IK_Targets_IK[k]);
			}
		}

		IK_Targets = list.ToArray();
	}

	private void Update()
	{
		if (!hitCheckDebugEnabled)
		{
			return;
		}

		try
		{
			if (Input.GetKeyUp((KeyCode)288))
			{
				if (Input.GetKey((KeyCode)304) || Input.GetKey((KeyCode)303))
				{
					HideHitSphere();
				}
				else if (!Input.GetKey((KeyCode)308))
				{
					VisualizeHitSphere("hair");
				}
				else
				{
					VisualizeHitSphere(null);
				}
			}

			if (Input.GetKeyUp((KeyCode)287))
			{
				InsertSkirtHitSphereForHair();
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
	}

	private void OnLevelWasLoaded(int level)
	{
		StopAllCoroutines();
		poseEditWindow = FindObjectOfType<PoseEditWindow>();
		placementWindow = FindObjectOfType<PlacementWindow>();
		hookPhotoSaveLoad = false;
		if (!FirstPersonCameraHelper.IsPhotoMode(level))
		{
			FirstPersonCameraHelper.IsIKModeEnabled(level);
		}
		else
		{
			hookPhotoSaveLoad = true;
			StartCoroutine("HookSaveLoadCoroutine");
		}

		ClearCache();
	}

	public void ApplyLoadedIKNodes()
	{
		if (maidBones == null || maidBones.Count == 0)
		{
			VisibleIKNodes();
		}

		if (maidLoadIKPos != null && maidLoadIKRot != null)
		{
			foreach (string text in maidLoadIKPos.Keys)
			{
				Maid maid = FindMaidByGUID(text);
				if (maid != null && (poseEditWindow == null ||
				                     bool.Parse(poseEditWindow.GetMaidStoreData(maid)["use"])))
				{
					Dictionary<string, Transform> dictionary = maidBones[maid];
					Dictionary<string, Vector3> dictionary2 = maidLoadIKPos[text];
					Dictionary<string, Quaternion> dictionary3 = maidLoadIKRot[text];
					foreach (string text2 in dictionary2.Keys)
					{
						Vector3 vector = dictionary2[text2];
						Quaternion quaternion = dictionary3[text2];
						if (dictionary.ContainsKey(text2))
						{
							Transform transform = dictionary[text2];
							transform.localPosition = vector;
							transform.localRotation = quaternion;
							if (maidBoneLastPos[maid] != null)
							{
								maidBoneLastPos[maid][transform.name] = vector;
								maidBoneLastRot[maid][transform.name] = quaternion;
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

		maidLoadIKPos = new Dictionary<string, Dictionary<string, Vector3>>();
		maidLoadIKRot = new Dictionary<string, Dictionary<string, Quaternion>>();
	}

	private void CleanupIKXML(XElement xml)
	{
		var list = new List<XElement>();
		list.AddRange(xml.Elements());
		foreach (XElement xelement in list)
		{
			if (xelement.Name == "Type" && xelement.Attribute("name").Value == "X_IK_Transforms")
			{
				xelement.Remove();
			}
		}
	}

	private void ClearCache()
	{
		maidBones = new Dictionary<Maid, Dictionary<string, Transform>>();
		maidBoneLastPos = new Dictionary<Maid, Dictionary<string, Vector3>>();
		maidBoneLastRot = new Dictionary<Maid, Dictionary<string, Quaternion>>();
	}

	public static IKTool Create()
	{
		if (instance != null)
		{
			return instance;
		}

		var gameObject = new GameObject("IKTool");
		instance = gameObject.AddComponent<IKTool>();
		DontDestroyOnLoad(gameObject);
		return instance;
	}

	private Material CreateForceDrawMaterial()
	{
		Material material2;
		try
		{
			var material = new Material(MaterialHelper.GetColorZOrderShader());
			Color red = Color.red;
			red.a = markerOverlayAlpha;
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

	private IEnumerator DelayedApplyLoadedIKNodes()
	{
		yield return new WaitForSeconds(5f);
		ApplyLoadedIKNodes();
		yield break;
	}

	public void EnableIKMode(bool markerIsEnable)
	{
		if (!markerIsEnable && !this.markerIsEnable)
		{
			return;
		}

		if (!markerIsEnable)
		{
			refs--;
		}
		else
		{
			refs++;
		}

		if (refs > 0)
		{
			this.markerIsEnable = true;
		}
		else
		{
			refs = 0;
			this.markerIsEnable = false;
		}

		VisibleIKNodes();
	}

	private void FindAndUpdateIKWithMarker(Transform t, Maid maid)
	{
		if (t.name != null && t.name.StartsWith("__IK__Marker__") &&
		    t.localPosition != Vector3.zero && t.localRotation != Quaternion.identity)
		{
			OnIKMarkerMoved(t, maid);
		}

		for (int i = 0; i < t.childCount; i++)
		{
			FindAndUpdateIKWithMarker(t.GetChild(i), maid);
		}
	}

	private void FindIKAndInsertMarker(Maid maid)
	{
		var dictionary = new Dictionary<string, Transform>();
		var dictionary2 = new Dictionary<string, Vector3>();
		var dictionary3 = new Dictionary<string, Quaternion>();
		maidBones.Add(maid, dictionary);
		maidBoneLastPos[maid] = dictionary2;
		maidBoneLastRot[maid] = dictionary3;
		foreach (string text in IK_Targets)
		{
			Transform transform = CMT.SearchObjName(maid.body0.m_Bones.transform, text);
			if (transform != null)
			{
				dictionary.Add(transform.name, transform);
				dictionary2[transform.name] = transform.localPosition;
				dictionary3[transform.name] = transform.localRotation;
				string text2 = "__IK__Marker__" + transform.name;
				Transform transform2 = transform.Find(text2);
				if (transform2 == null)
				{
					var gameObject = GameObject.CreatePrimitive(0);
					if (markerShowOverlay)
					{
						if (markerSharedMaterial == null)
						{
							markerSharedMaterial = CreateForceDrawMaterial();
						}

						if (markerSharedMaterial_ItemIK == null)
						{
							markerSharedMaterial_ItemIK = CreateForceDrawMaterial();
							markerSharedMaterial_ItemIK.SetColor("_Color",
								new Color(0f, 1f, 0f, markerOverlayAlpha));
						}

						if (!transform.name.StartsWith("_IK_"))
						{
							gameObject.GetComponent<Renderer>().sharedMaterial =
								markerSharedMaterial;
						}
						else
						{
							gameObject.GetComponent<Renderer>().sharedMaterial =
								markerSharedMaterial_ItemIK;
						}
					}

					gameObject.AddComponent<MoveableGUIObject>().onMoveLister.Add(OnIKMarkerMoved);
					gameObject.name = text2;
					gameObject.GetComponent<SphereCollider>().isTrigger = true;
					gameObject.transform.parent = transform;
					gameObject.transform.localPosition = Vector3.zero;
					gameObject.transform.localRotation = Quaternion.identity;
					if (IK_Targets_Base.Contains(transform.name))
					{
						gameObject.transform.localScale = Vector3.one * markerSize;
					}
					else if (!IK_Targets_IK.Contains(transform.name))
					{
						gameObject.transform.localScale = Vector3.one * markerSizeFinger;
					}
					else
					{
						gameObject.transform.localScale = Vector3.one * markerSizeItemIK;
					}

					gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
					transform2 = gameObject.transform;
				}

				transform2.gameObject.GetComponent<Renderer>().enabled = markerIsVisible;
				transform2.gameObject.SetActive(markerIsEnable);
			}
		}
	}

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

	private PhotoWindowManager FindPhotoWindowManagerInScene()
	{
		var gameObject = GameObject.Find("UI Root/MainScreen/PhotoWindowManager");
		if (gameObject == null)
		{
			return null;
		}

		return gameObject.GetComponent<PhotoWindowManager>();
	}

	private void HideHitSphere()
	{
		foreach (TBody tbody in FindObjectsOfType<TBody>())
		{
			foreach (MeshRenderer meshRenderer in FindObjectsOfType<MeshRenderer>())
			{
				if (meshRenderer.gameObject.name.StartsWith("__dummy_hitsphere_view"))
				{
					Destroy(meshRenderer.gameObject);
				}
			}
		}
	}

	private IEnumerator HookSaveLoadCoroutine()
	{
		for (;;)
		{
			if (hookPhotoSaveLoad)
			{
				if (photoWindowManager == null)
				{
					photoWindowManager = FindPhotoWindowManagerInScene();
				}

				if (photoWindowManager != null)
				{
					if (photoWindowManager.SaveAndLoadManager.onSerializeEvent != OnSerializeWithIK)
					{
						defaultOnSerializeEventHandler =
							photoWindowManager.SaveAndLoadManager.onSerializeEvent;
						photoWindowManager.SaveAndLoadManager.onSerializeEvent = OnSerializeWithIK;
					}

					if (photoWindowManager.SaveAndLoadManager.onDeserializeEvent !=
					    OnDeserializeWithIK)
					{
						defaultOnDeserializeEventHandler =
							photoWindowManager.SaveAndLoadManager.onDeserializeEvent;
						photoWindowManager.SaveAndLoadManager.onDeserializeEvent =
							OnDeserializeWithIK;
					}
				}
			}

			yield return new WaitForSeconds(0.5f);
		}
	}

	private void InsertSkirtHitSphereForHair()
	{
		TBody[] array = FindObjectsOfType<TBody>();
		var array2 = new Material[5];
		for (int i = 0; i < 5; i++)
		{
			var material = new Material(MaterialHelper.GetColorZOrderShader());
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
		using StreamWriter streamWriter = File.CreateText("__skirt.log");
		foreach (TBody tbody in array)
		{
			streamWriter.WriteLine("===Skirt for " + tbody.maid.name + "===");
			if (tbody.isActiveAndEnabled)
			{
				TBodySkin tbodySkin = tbody.goSlot[8];
				TBodySkin tbodySkin2 = tbody.goSlot[9];
				Console.WriteLine("Skirt: {0}, {1}, {2}", tbodySkin.boVisible,
					tbodySkin.m_strModelFileName, tbodySkin.obj);
				Console.WriteLine("Onepiece: {0}, {1}, {2}", tbodySkin2.boVisible,
					tbodySkin2.m_strModelFileName, tbodySkin2.obj);
				if (tbodySkin.obj == null && tbodySkin2.obj != null)
				{
					tbodySkin = tbodySkin2;
				}

				if (tbodySkin.boVisible && tbodySkin.m_strModelFileName != "")
				{
					TBody.SlotID slotId = tbodySkin.SlotId;
					streamWriter.WriteLine("{0}, {1}", tbodySkin.Category, slotId);
					streamWriter.WriteLine(tbodySkin.obj_tr.ToString());
					streamWriter.WriteLine("== Hair1 ==");
					var list = (List<THair1>)tbodySkin.bonehair.GetType()
						.GetField("hair1list",
							BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField)
						.GetValue(tbodySkin.bonehair);
					for (int k = 0; k < list.Count; k++)
					{
						THair1 thair = list[k];
						streamWriter.WriteLine("{0}", thair.root.position.ToString());
						var gameObject = GameObject.CreatePrimitive(0);
						gameObject.name = "__dummy_hitsphere_view";
						gameObject.transform.position = thair.root.position;
						gameObject.transform.localScale = Vector3.one * 0.02f;
						gameObject.GetComponent<Renderer>().sharedMaterial = array2[0];
					}

					streamWriter.WriteLine("== SKIRT LIST ==");
					var array4 = (THair1[])tbodySkin.bonehair.GetType()
						.GetField("SkirtList",
							BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField)
						.GetValue(tbodySkin.bonehair);
					float num2 = 0f;
					var list2 = new List<Transform>();
					Transform root_oya = array4[0].root_oya;
					for (int l = 3; l < 9; l++)
					{
						THair1 thair2 = array4[l];
						streamWriter.WriteLine("{0} {1}", thair2.root.position.ToString(),
							thair2.root_oya);
						list2.Add(thair2.root);
					}

					var list3 = new List<THitSphere>();
					for (int m = 3; m < 9; m++)
					{
						THair1 thair3 = array4[m];
						Console.WriteLine("hplist count: " + thair3.hplist.Count);
						for (int n = 0; n < thair3.hplist.Count; n += 2)
						{
							THp thp = thair3.hplist[n];
							var gameObject2 = GameObject.CreatePrimitive(0);
							gameObject2.name = "__dummy_hitsphere_view";
							gameObject2.transform.position = thp.t.position;
							gameObject2.transform.localScale = Vector3.one * (n + 1) * 0.01f;
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
							var thitSphere = new THitSphere
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
							var thitSphere2 = new THitSphere
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

	public bool IsIKMarkerEnabled() => markerIsEnable;

	public void OnDeserializeWithIK(XElement xml)
	{
		ClearCache();
		XContainer xcontainer = xml.Element("PhotSetting").Element("Maids");
		maidLoadIKPos = new Dictionary<string, Dictionary<string, Vector3>>();
		maidLoadIKRot = new Dictionary<string, Dictionary<string, Quaternion>>();
		foreach (XElement xelement in xcontainer.Elements())
		{
			if (xelement.Name == "Maid")
			{
				string value = xelement.Attribute("guid").Value;
				var dictionary = new Dictionary<string, Vector3>();
				var dictionary2 = new Dictionary<string, Quaternion>();
				maidLoadIKPos[value] = dictionary;
				maidLoadIKRot[value] = dictionary2;
				XElement xelement2 = null;
				foreach (XElement xelement3 in xelement.Elements())
				{
					if (xelement3.Name == "Type" &&
					    "X_IK_Transforms".Equals(xelement3.Attribute("name").Value))
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
							if (value2 != null && IK_Targets.Contains(value2))
							{
								float num = float.Parse(xelement4.Attribute("x").Value);
								float num2 = float.Parse(xelement4.Attribute("y").Value);
								float num3 = float.Parse(xelement4.Attribute("z").Value);
								float num4 = float.Parse(xelement4.Attribute("rx").Value);
								float num5 = float.Parse(xelement4.Attribute("ry").Value);
								float num6 = float.Parse(xelement4.Attribute("rz").Value);
								Vector3 vector;
								vector = new Vector3(num, num2, num3);
								var quaternion = Quaternion.Euler(num4, num5, num6);
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

		defaultOnDeserializeEventHandler(xml);
		StartCoroutine("DelayedApplyLoadedIKNodes");
	}

	private void OnIKMarkerMoved(MonoBehaviour ikMarker)
	{
		Maid componentInParent = ikMarker.transform.GetComponentInParent<Maid>();
		Transform transform = ikMarker.transform;
		if (poseEditWindow != null && placementWindow != null &&
		    !bool.Parse(poseEditWindow.GetMaidStoreData(componentInParent)["use"]))
		{
			m_SetSelectMaid.Invoke(placementWindow, new object[] { componentInParent });
			poseEditWindow.OnClickUseCheckRun(true);
			poseEditWindow.CheckbtnUse.check = true;
		}

		OnIKMarkerMoved(transform, componentInParent);
	}

	private void OnIKMarkerMoved(Transform marker, Maid maid)
	{
		Dictionary<string, Transform> dictionary = maidBones[maid];
		Dictionary<string, Vector3> dictionary2 = maidBoneLastPos[maid];
		Dictionary<string, Quaternion> dictionary3 = maidBoneLastRot[maid];
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
			var ik = new IK();
			TBody body = maid.body0;
			Vector3 zero = Vector3.zero;
			if (text2.Contains("Finger"))
			{
				string text3 = text2.Substring(0, "Fingerx".Length);
				string text4 = text2.Substring("Fingerx".Length);
				if (text4 != null && text4.Length == 0)
				{
					ik.Init(dictionary[text + "Hand"], dictionary[text + "Hand"],
						dictionary[text + text3 ?? ""], body);
					ik.Porc(dictionary[text + "Hand"], dictionary[text + "Hand"],
						dictionary[text + text3 ?? ""], marker.position, zero);
					parent.rotation = marker.rotation;
					array = new[] { dictionary[text + "Hand"] };
				}
				else if (text4 == "1")
				{
					ik.Init(dictionary[text + text3 ?? ""], dictionary[text + text3 ?? ""],
						dictionary[text + text3 + "1"], body);
					ik.Porc(dictionary[text + text3 ?? ""], dictionary[text + text3 ?? ""],
						dictionary[text + text3 + "1"], marker.position, zero);
					parent.rotation = marker.rotation;
					array = new[] { dictionary[text + text3 ?? ""] };
				}
				else if (text4 == "2")
				{
					ik.Init(dictionary[text + text3 ?? ""], dictionary[text + text3 + "1"],
						dictionary[text + text3 + "2"], body);
					ik.Porc(dictionary[text + text3 ?? ""], dictionary[text + text3 + "1"],
						dictionary[text + text3 + "2"], marker.position, zero);
					parent.rotation = marker.rotation;
					array = new[]
					{
						dictionary[text + text3 ?? ""], dictionary[text + text3 + "1"]
					};
				}
				else if (text4 == "Nub")
				{
					ik.Init(dictionary[text + text3 + "1"], dictionary[text + text3 + "2"],
						dictionary[text + text3 + "Nub"], body);
					ik.Porc(dictionary[text + text3 + "1"], dictionary[text + text3 + "2"],
						dictionary[text + text3 + "Nub"], marker.position, zero);
					parent.rotation = marker.rotation;
					array = new[]
					{
						dictionary[text + text3 + "1"], dictionary[text + text3 + "2"]
					};
				}
			}
			else if (text2 == "Hand")
			{
				ik.Init(dictionary[text + "UpperArm"], dictionary[text + "Forearm"],
					dictionary[text + "Hand"], body);
				ik.Porc(dictionary[text + "UpperArm"], dictionary[text + "Forearm"],
					dictionary[text + "Hand"], marker.position, zero);
				parent.rotation = marker.rotation;
				array = new[] { dictionary[text + "UpperArm"], dictionary[text + "Forearm"] };
			}
			else if (text2 == "Forearm")
			{
				ik.Init(dictionary[text + "UpperArm"], dictionary[text + "UpperArm"],
					dictionary[text + "Forearm"], body);
				ik.Porc(dictionary[text + "UpperArm"], dictionary[text + "UpperArm"],
					dictionary[text + "Forearm"], marker.position, zero);
				parent.rotation = marker.rotation;
				array = new[] { dictionary[text + "UpperArm"] };
			}
			else if (text2 == "UpperArm")
			{
				ik.Init(dictionary[text + "Clavicle"], dictionary[text + "Clavicle"],
					dictionary[text + "UpperArm"], body);
				ik.Porc(dictionary[text + "Clavicle"], dictionary[text + "Clavicle"],
					dictionary[text + "UpperArm"], marker.position, zero);
				parent.rotation = marker.rotation;
				array = new[] { dictionary[text + "Clavicle"] };
			}
			else if (text2 == "Clavicle")
			{
				parent.rotation = marker.rotation;
			}
			else if (text2 == "Foot")
			{
				ik.Init(dictionary[text + "Thigh"], dictionary[text + "Calf"],
					dictionary[text + "Foot"], body);
				ik.Porc(dictionary[text + "Thigh"], dictionary[text + "Calf"],
					dictionary[text + "Foot"], marker.position, zero);
				array = new[] { dictionary[text + "Clavicle"], dictionary[text + "Calf"] };
				parent.rotation = marker.rotation;
			}
			else if (text2 == "Calf")
			{
				ik.Init(dictionary[text + "Thigh"], dictionary[text + "Thigh"],
					dictionary[text + "Calf"], body);
				ik.Porc(dictionary[text + "Thigh"], dictionary[text + "Thigh"],
					dictionary[text + "Calf"], marker.position, zero);
				parent.rotation = marker.rotation;
				array = new[] { dictionary[text + "Thigh"] };
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

	public XElement OnSerializeWithIK()
	{
		Console.WriteLine("Wrote additional data  (IK info)");
		if (maidBones == null || maidBones.Count == 0)
		{
			VisibleIKNodes();
		}

		var dictionary = new Dictionary<string, XElement>();
		foreach (Maid maid in maidBones.Keys)
		{
			string guid = maid.status.guid;
			var xelement = new XElement("Type");
			xelement.SetAttributeValue("name", "X_IK_Transforms");
			Dictionary<string, Transform> dictionary2 = maidBones[maid];
			Dictionary<string, Vector3> dictionary3 = maidBoneLastPos[maid];
			Dictionary<string, Quaternion> dictionary4 = maidBoneLastRot[maid];
			if (dictionary2 != null)
			{
				foreach (string text in IK_Targets_IK)
				{
					if (dictionary2.ContainsKey(text))
					{
						Transform transform = dictionary2[text];
						var xelement2 = new XElement("elm");
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

		XElement xelement3 = defaultOnSerializeEventHandler();
		foreach (XElement xelement4 in xelement3.Element("Maids").Elements())
		{
			if (xelement4.Name == "Maid")
			{
				string value = xelement4.Attribute("guid").Value;
				CleanupIKXML(xelement4);
				if (dictionary.ContainsKey(value))
				{
					xelement4.Add(dictionary[value]);
				}
			}
		}

		return xelement3;
	}

	public void ToggleMarkerVisibility() => VisualizeIKMarker(!markerIsVisible);

	public void VisibleIKNodes()
	{
		CharacterMgr characterMgr = GameMain.Instance.CharacterMgr;
		maidBones = new Dictionary<Maid, Dictionary<string, Transform>>();
		for (int i = 0; i < characterMgr.GetMaidCount(); i++)
		{
			Maid maid = characterMgr.GetMaid(i);
			if (maid != null)
			{
				FindIKAndInsertMarker(maid);
			}
		}
	}

	private void VisualizeHitSphere(string type)
	{
		TBody[] array = FindObjectsOfType<TBody>();
		var material = new Material(MaterialHelper.GetColorZOrderShader());
		Color blue = Color.blue;
		blue.a = 0.3f;
		material.SetColor("_Color", blue);
		var material2 = new Material(MaterialHelper.GetColorZOrderShader());
		Color green = Color.green;
		green.a = 0.3f;
		material2.SetColor("_Color", green);
		int num = 0;
		int num2 = 0;
		using StreamWriter streamWriter = File.CreateText("__bodyhit.log");
		foreach (TBody tbody in array)
		{
			if (tbody.isActiveAndEnabled)
			{
				streamWriter.WriteLine("----{0}----", tbody.maid.name);
				Transform transform = CMT.SearchObjName(tbody.transform, "Bip01 Head");
				CMT.SearchObjName(tbody.transform, "Bip01 Spine1a");
				CMT.SearchObjName(tbody.transform, "Bip01 Spine0a");
				CMT.SearchObjName(tbody.transform, "Bip01 Neck");
				CMT.SearchObjName(tbody.transform, "Bip01 L Clavicle_SCL_");
				Transform transform2 = CMT.SearchObjName(tbody.transform, "Bip01 Spine0a_SCL_");
				CMT.SearchObjName(tbody.transform, "Bip01 Pelvis");
				Transform transform3 = CMT.SearchObjName(tbody.transform, "Bip01 Pelvis_SCL_");
				Transform transform4 = CMT.SearchObjName(tbody.transform, "Bip01 Spine1a_SCL_");
				float x = transform.localScale.x;
				float x2 = transform4.localScale.x;
				float x3 = transform2.localScale.x;
				float x4 = transform3.localScale.x;
				streamWriter.WriteLine("Head: {0}", x);
				streamWriter.WriteLine("Kata: {0}", x2);
				streamWriter.WriteLine("Mid:  {0}", x3);
				streamWriter.WriteLine("Koshi:{0}", x4);
				foreach (TBodySkin tbodySkin in tbody.goSlot)
				{
					if (type == null || tbodySkin.Category.StartsWith(type))
					{
						num++;
						streamWriter.WriteLine("=== Skin: {0}, category: {1}===",
							tbodySkin.m_strModelFileName, tbodySkin.Category);
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
							Console.WriteLine("Sphere: {0}, {1}, {2}, {3}, {4}, {5}", array4);
							object[] array5 = new object[6];
							array5[0] = thitSphere.name;
							array5[1] = thitSphere.pname;
							array5[2] = text;
							object[] array6 = array5;
							array6[3] = thitSphere.t.position.ToString();
							array6[4] = thitSphere.len;
							array6[5] = thitSphere.t.parent.lossyScale.x;
							streamWriter.WriteLine("Sphere: {0}, {1}, {2}, {3}, {4}, {5}", array6);
							var gameObject = GameObject.CreatePrimitive(0);
							gameObject.name = "__dummy_hitsphere_view";
							gameObject.transform.position = thitSphere.t.position;
							gameObject.transform.localScale = Vector3.one * thitSphere.len *
							                                  (1f + _TS.TestVal2 * 2f);
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

		Console.WriteLine("TBodySkin Count: " + num);
		Console.WriteLine("THitSphere Count: " + num2);
	}

	public void VisualizeIKMarker(bool markerIsVisible)
	{
		this.markerIsVisible = markerIsVisible;
		VisibleIKNodes();
	}
}

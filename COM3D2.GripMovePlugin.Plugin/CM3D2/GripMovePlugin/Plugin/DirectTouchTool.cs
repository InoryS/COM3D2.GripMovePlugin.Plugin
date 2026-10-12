using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin;

public class DirectTouchTool : MonoBehaviour
{
	public static DirectTouchTool _instance;

	public List<DirectTouchMaid> activeDirectTouchMaids = new();

	public List<DirectTouchController> controllers = new();

	public bool directTouchEnabled = true;

	private readonly List<IDirectTouchEventHandlerProvider> eventHandlerProviders = new();

	private readonly List<Action<DirectTouchController, TouchArea>> onStayHandlers = new();

	private Material _markerCustomMaterial;

	private Material _markerMaterial;

	private List<CustomMaidTouchArea> customMaidTouchArea = new();

	private bool debug;

	private float handHitRadius = 0.03f;

	private float mouthHitRadius = 0.05f;

	private Vector3 mouthPos = new(0f, -0.08f, 0.02f);

	private List<Action<DirectTouchMaid>> onActivateOnMaidHandlers = new();

	private List<Action<DirectTouchController, TouchArea, bool>> onHitHandlers = new();

	private List<Action<DirectTouchTool>> onPostHitTestHandlers = new();

	private float touchSize_anal = 0.03f;

	private float touchSize_eye = 0.04f;

	private float touchSize_foot = 0.08f;

	private float touchSize_hair_base = 0.13f;

	private float touchSize_hand = 0.08f;

	private float touchSize_hara = 0.1f;

	private float touchSize_hips = 0.12f;

	private float touchSize_kata = 0.08f;

	private float touchSize_momo = 0.1f;

	private float touchSize_mouth = 0.03f;

	private float touchSize_mune_base = 0.12f;

	private float touchSize_vagina = 0.03f;

	public static DirectTouchTool Instance
	{
		get
		{
			if (_instance == null)
			{
				var gameObject = new GameObject("DirectTouchTool");
				_instance = gameObject.AddComponent<DirectTouchTool>();
				DontDestroyOnLoad(gameObject);
			}

			return _instance;
		}
	}

	private void Awake()
	{
		_markerMaterial = new Material(MaterialHelper.GetColorZOrderShader());
		Color color;
		color = new Color(0f, 0f, 1f, 0.5f);
		_markerMaterial.SetColor("_Color", color);
		_markerCustomMaterial = new Material(MaterialHelper.GetColorZOrderShader());
		Color color2;
		color2 = new Color(0f, 1f, 0f, 0.5f);
		_markerCustomMaterial.SetColor("_Color", color2);
	}

	private void Start()
	{
		debug = Settings.Instance.GetBoolValue("DirectTouchToolDebug", false);
		directTouchEnabled = Settings.Instance.GetBoolValue("DirectTouchToolEnabled", true);
		if (!directTouchEnabled)
		{
			enabled = false;
		}

		handHitRadius = Settings.Instance.GetFloatValue("DirectTouchToolHandHitRadius", 0.03f);
		float floatValue = Settings.Instance.GetFloatValue("DirectTouchToolMouthHitPosY", -0.08f);
		float floatValue2 = Settings.Instance.GetFloatValue("DirectTouchToolMouthHitPosZ", 0.02f);
		mouthPos = new Vector3(0f, floatValue, floatValue2);
		mouthHitRadius = Settings.Instance.GetFloatValue("DirectTouchToolMouthHitRadius", 0.05f);
	}

	private void Update()
	{
		try
		{
			Update2();
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
	}

	private void OnLevelWasLoaded(int level)
	{
		try
		{
			InitTouchSize();
			activeDirectTouchMaids = new List<DirectTouchMaid>();
			controllers = new List<DirectTouchController>();
			InstallTouchToVRControllers();
			StopAllCoroutines();
			StartCoroutine("InstallDirectTouchToMaidCo");
			onHitHandlers = new List<Action<DirectTouchController, TouchArea, bool>>();
			onPostHitTestHandlers = new List<Action<DirectTouchTool>>();
			onActivateOnMaidHandlers = new List<Action<DirectTouchMaid>>();
			customMaidTouchArea = new List<CustomMaidTouchArea>();
			foreach (IDirectTouchEventHandlerProvider directTouchEventHandlerProvider in
			         eventHandlerProviders)
			{
				try
				{
					directTouchEventHandlerProvider.InstallDirectTouchEventHandler(this, level);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}
			}
		}
		catch (Exception ex2)
		{
			Console.WriteLine(ex2.ToString());
		}
	}

	private void ActivateOnMaid(Maid maid)
	{
		TBody body = maid.body0;
		DirectTouchMaid directTouchMaid = maid.body0.gameObject.AddComponent<DirectTouchMaid>();
		activeDirectTouchMaids.Add(directTouchMaid);
		Transform transform = CMT.SearchObjName(maid.body0.transform, "Bip01 Head");
		directTouchMaid.AddTouchArea(MaidTouchAreaName.VAGINA, "_IK_vagina", touchSize_vagina,
			Vector3.zero);
		directTouchMaid.AddTouchArea(MaidTouchAreaName.ANAL, "_IK_anal", touchSize_anal,
			Vector3.zero);
		directTouchMaid.AddTouchArea(MaidTouchAreaName.HIP, "_IK_hipL", touchSize_hips,
			Vector3.zero);
		directTouchMaid.AddTouchArea(MaidTouchAreaName.HIP, "_IK_hipR", touchSize_hips,
			Vector3.zero);
		Transform transform2 = CMT.SearchObjName(maid.body0.transform, "Mune_L_sub");
		directTouchMaid.AddTouchArea(MaidTouchAreaName.MUNE, "Mune_L_nub",
			touchSize_mune_base * transform2.lossyScale.x, Vector3.zero);
		directTouchMaid.AddTouchArea(MaidTouchAreaName.MUNE, "Mune_R_nub",
			touchSize_mune_base * transform2.lossyScale.x, Vector3.zero);
		directTouchMaid.AddTouchArea(MaidTouchAreaName.HAIR, "Bip01 Head",
			touchSize_hair_base * transform.lossyScale.z,
			new Vector3(-0.13f * transform.lossyScale.x, 0f, 0f));
		directTouchMaid.AddTouchArea(MaidTouchAreaName.MOUTH, "Bip01 Head", touchSize_mouth,
			new Vector3(0.02f * transform.lossyScale.x, 0.07f * transform.lossyScale.y, 0f));
		directTouchMaid.AddTouchArea(MaidTouchAreaName.EYE, "Bip01 Head", touchSize_eye,
			new Vector3(-0.03f * transform.lossyScale.x, 0.07f * transform.lossyScale.y, 0f));
		directTouchMaid.AddTouchArea(MaidTouchAreaName.HARA, "_IK_hara", touchSize_hara,
			Vector3.zero);
		directTouchMaid.AddTouchArea(MaidTouchAreaName.KATA, "Kata_L", touchSize_kata,
			Vector3.zero);
		directTouchMaid.AddTouchArea(MaidTouchAreaName.KATA, "Kata_R", touchSize_kata,
			Vector3.zero);
		directTouchMaid.AddTouchArea(MaidTouchAreaName.HAND, "Bip01 L Hand", touchSize_hand,
			Vector3.zero);
		directTouchMaid.AddTouchArea(MaidTouchAreaName.HAND, "Bip01 R Hand", touchSize_hand,
			Vector3.zero);
		directTouchMaid.AddTouchArea(MaidTouchAreaName.MOMO, "momotwist_L_nub", touchSize_momo,
			Vector3.zero);
		directTouchMaid.AddTouchArea(MaidTouchAreaName.MOMO, "momotwist_R_nub", touchSize_momo,
			Vector3.zero);
		directTouchMaid.AddTouchArea(MaidTouchAreaName.FOOT, "Bip01 L Foot", touchSize_foot,
			Vector3.zero);
		directTouchMaid.AddTouchArea(MaidTouchAreaName.FOOT, "Bip01 R Foot", touchSize_foot,
			Vector3.zero);
		foreach (CustomMaidTouchArea customMaidTouchArea in this.customMaidTouchArea)
		{
			directTouchMaid.AddTouchArea(customMaidTouchArea.name, customMaidTouchArea.baseBoneName,
				customMaidTouchArea.radius, customMaidTouchArea.position);
		}

		OnActivateOnMaid(directTouchMaid);
		if (debug)
		{
			Console.WriteLine("Direct Touch installed: " + maid.name);
		}
	}

	public void AddCustomMaidTouchArea(string customName, string baseBoneName, Vector3 position,
		float radius)
	{
		var customMaidTouchArea = new CustomMaidTouchArea
		{
			name = customName, baseBoneName = baseBoneName, position = position, radius = radius
		};
		this.customMaidTouchArea.Add(customMaidTouchArea);
	}

	public void AddOnAfterHitTestHandler(Action<DirectTouchTool> handler) =>
		onPostHitTestHandlers.Add(handler);

	public void AddOnHitHandler(Action<DirectTouchController, TouchArea, bool> handler) =>
		onHitHandlers.Add(handler);

	public void AddOnStayHandler(Action<DirectTouchController, TouchArea> handler) =>
		onStayHandlers.Add(handler);

	private void InitTouchSize()
	{
		touchSize_vagina = Settings.Instance.GetFloatValue("DirectTouchTool_Size_VAGINA", 0.03f);
		touchSize_anal = Settings.Instance.GetFloatValue("DirectTouchTool_Size_ANAL", 0.03f);
		touchSize_hips = Settings.Instance.GetFloatValue("DirectTouchTool_Size_HIP", 0.12f);
		touchSize_mune_base = Settings.Instance.GetFloatValue("DirectTouchTool_Size_MUNE", 0.12f);
		touchSize_hair_base = Settings.Instance.GetFloatValue("DirectTouchTool_Size_HAIR", 0.13f);
		touchSize_mouth = Settings.Instance.GetFloatValue("DirectTouchTool_Size_MOUTH", 0.03f);
		touchSize_eye = Settings.Instance.GetFloatValue("DirectTouchTool_Size_EYE", 0.04f);
		touchSize_hara = Settings.Instance.GetFloatValue("DirectTouchTool_Size_HARA", 0.1f);
		touchSize_kata = Settings.Instance.GetFloatValue("DirectTouchTool_Size_KATA", 0.08f);
		touchSize_hand = Settings.Instance.GetFloatValue("DirectTouchTool_Size_HAND", 0.08f);
		touchSize_momo = Settings.Instance.GetFloatValue("DirectTouchTool_Size_MOMO", 0.1f);
		touchSize_foot = Settings.Instance.GetFloatValue("DirectTouchTool_Size_FOOT", 0.08f);
	}

	private IEnumerator InstallDirectTouchToMaidCo()
	{
		CharacterMgr characterMgr = GameMain.Instance.CharacterMgr;
		for (;;)
		{
			characterMgr.GetMaidCount();
			for (int i = 0; i < characterMgr.GetMaidCount(); i++)
			{
				Maid maid = characterMgr.GetMaid(i);
				if (maid != null && maid.body0 != null && !maid.body0.boMAN)
				{
					DirectTouchMaid component = maid.body0.GetComponent<DirectTouchMaid>();
					if (component != null)
					{
						if (!activeDirectTouchMaids.Contains(component))
						{
							activeDirectTouchMaids.Add(component);
						}
					}
					else if (maid.body0.isLoadedBody)
					{
						ActivateOnMaid(maid);
					}
				}
			}

			yield return new WaitForSeconds(1f);
		}
	}

	private void InstallTouchToOculusTouch()
	{
		OVRCameraRig componentInParent = GameMain.Instance.OvrMgr.OvrCamera.gameObject
			.GetComponentInParent<OVRCameraRig>();
		Transform centerEyeAnchor = componentInParent.centerEyeAnchor;
		Transform leftHandAnchor = componentInParent.leftHandAnchor;
		Transform rightHandAnchor = componentInParent.rightHandAnchor;
		if (centerEyeAnchor != null)
		{
			DirectTouchController directTouchController =
				centerEyeAnchor.gameObject.GetComponent<DirectTouchController>();
			if (directTouchController == null)
			{
				directTouchController =
					centerEyeAnchor.gameObject.AddComponent<DirectTouchController>();
				directTouchController.areaName = ControllerTouchAreaName.MOUTH;
				directTouchController.SetPosition(mouthPos);
				directTouchController.radius = mouthHitRadius;
				directTouchController.onHitHandler = OnHit;
				directTouchController.onStayHandler = OnStay;
			}

			controllers.Add(directTouchController);
		}

		if (leftHandAnchor != null &&
		    (OVRInput.GetConnectedControllers() & (OVRInput.Controller)3) != 0)
		{
			DirectTouchController directTouchController2 =
				leftHandAnchor.gameObject.GetComponent<DirectTouchController>();
			if (directTouchController2 == null)
			{
				directTouchController2 =
					leftHandAnchor.gameObject.AddComponent<DirectTouchController>();
				directTouchController2.areaName = ControllerTouchAreaName.HAND_L;
				directTouchController2.radius = handHitRadius;
				directTouchController2.onHitHandler = OnHit;
				directTouchController2.onStayHandler = OnStay;
				directTouchController2.vrdeviceWrapper =
					DeviceOculusTouchController.GetOrCreate(directTouchController2.gameObject,
						(OVRInput.Controller)1);
			}

			controllers.Add(directTouchController2);
		}

		if (rightHandAnchor != null &&
		    (OVRInput.GetConnectedControllers() & (OVRInput.Controller)3) != 0)
		{
			DirectTouchController directTouchController3 =
				rightHandAnchor.gameObject.GetComponent<DirectTouchController>();
			if (directTouchController3 == null)
			{
				directTouchController3 =
					rightHandAnchor.gameObject.AddComponent<DirectTouchController>();
				directTouchController3.areaName = ControllerTouchAreaName.HAND_R;
				directTouchController3.radius = handHitRadius;
				directTouchController3.onHitHandler = OnHit;
				directTouchController3.onStayHandler = OnStay;
				directTouchController3.vrdeviceWrapper =
					DeviceOculusTouchController.GetOrCreate(directTouchController3.gameObject,
						(OVRInput.Controller)2);
			}

			controllers.Add(directTouchController3);
		}
	}

	private void InstallTouchToViveControllers()
	{
		if (GameMain.Instance.OvrMgr.ovr_obj != null)
		{
			OvrMgr ovrMgr = GameMain.Instance.OvrMgr;
			OvrMgr.OvrObject ovr_obj = GameMain.Instance.OvrMgr.ovr_obj;
			if (ovrMgr.OvrCamera != null && ovrMgr.OvrCamera.gameObject != null)
			{
				foreach (SteamVR_TrackedObject steamVR_TrackedObject in ovrMgr.OvrCamera.gameObject
					         .GetComponentsInChildren<SteamVR_TrackedObject>())
				{
					if (steamVR_TrackedObject.index == SteamVR_TrackedObject.EIndex.Hmd)
					{
						DirectTouchController directTouchController = steamVR_TrackedObject
							.gameObject.GetComponent<DirectTouchController>();
						if (directTouchController == null)
						{
							directTouchController = steamVR_TrackedObject.gameObject
								.AddComponent<DirectTouchController>();
							directTouchController.areaName = ControllerTouchAreaName.MOUTH;
							directTouchController.SetPosition(mouthPos);
							directTouchController.radius = mouthHitRadius;
							directTouchController.onHitHandler = OnHit;
							directTouchController.onStayHandler = OnStay;
						}

						controllers.Add(directTouchController);
						break;
					}
				}
			}

			if (ovr_obj.left_controller.controller != null &&
			    ovr_obj.left_controller.controller.gameObject != null)
			{
				DirectTouchController directTouchController2 = ovr_obj.left_controller.controller
					.gameObject.GetComponent<DirectTouchController>();
				if (directTouchController2 == null)
				{
					directTouchController2 = ovr_obj.left_controller.controller.gameObject
						.AddComponent<DirectTouchController>();
					directTouchController2.areaName = ControllerTouchAreaName.HAND_L;
					directTouchController2.radius = handHitRadius;
					directTouchController2.onHitHandler = OnHit;
					directTouchController2.onStayHandler = OnStay;
					directTouchController2.vrdeviceWrapper = DeviceViveController.GetOrCreate(
						directTouchController2.gameObject,
						ovr_obj.left_controller.controller.gameObject
							.GetComponent<SteamVR_TrackedObject>());
				}

				controllers.Add(directTouchController2);
			}

			if (ovr_obj.right_controller.controller != null &&
			    ovr_obj.right_controller.controller.gameObject != null)
			{
				DirectTouchController directTouchController3 = ovr_obj.right_controller.controller
					.gameObject.GetComponent<DirectTouchController>();
				if (directTouchController3 == null)
				{
					directTouchController3 = ovr_obj.right_controller.controller.gameObject
						.AddComponent<DirectTouchController>();
					directTouchController3.areaName = ControllerTouchAreaName.HAND_R;
					directTouchController3.radius = handHitRadius;
					directTouchController3.onHitHandler = OnHit;
					directTouchController3.onStayHandler = OnStay;
					directTouchController3.vrdeviceWrapper = DeviceViveController.GetOrCreate(
						directTouchController3.gameObject,
						ovr_obj.right_controller.controller.GetComponent<SteamVR_TrackedObject>());
				}

				controllers.Add(directTouchController3);
			}
		}
	}

	private void InstallTouchToVRControllers()
	{
		if (GameMain.Instance.VRFamily == (GameMain.VRFamilyType)2)
		{
			InstallTouchToViveControllers();
			return;
		}

		if (GameMain.Instance.VRFamily == (GameMain.VRFamilyType)1)
		{
			InstallTouchToOculusTouch();
		}
	}

	private void OnActivateOnMaid(DirectTouchMaid dtm)
	{
		foreach (Action<DirectTouchMaid> action in onActivateOnMaidHandlers)
		{
			try
			{
				action(dtm);
			}
			catch (Exception ex)
			{
				if (debug)
				{
					Console.WriteLine(ex.ToString());
				}
			}
		}
	}

	private void OnHit(DirectTouchController controller, TouchArea ta, bool onHit)
	{
		if (debug)
		{
			if (!onHit)
			{
				Console.WriteLine("OnHit[{2}](Exit) by {0} area {1} ", controller.areaName,
					ta.areaName, onHit);
			}
			else
			{
				Console.WriteLine("OnHit[{2}] by {0} area {1} ", controller.areaName, ta.areaName,
					onHit);
			}
		}

		foreach (Action<DirectTouchController, TouchArea, bool> action in onHitHandlers)
		{
			try
			{
				action(controller, ta, onHit);
			}
			catch (Exception ex)
			{
				if (debug)
				{
					Console.WriteLine(ex.ToString());
				}
			}
		}
	}

	private void OnStay(DirectTouchController controller, TouchArea ta)
	{
		foreach (Action<DirectTouchController, TouchArea> action in onStayHandlers)
		{
			try
			{
				action(controller, ta);
			}
			catch (Exception ex)
			{
				if (debug)
				{
					Console.WriteLine(ex.ToString());
				}
			}
		}
	}

	public void RegisterEventHandlerProvider(IDirectTouchEventHandlerProvider provider)
	{
		if (!eventHandlerProviders.Contains(provider))
		{
			eventHandlerProviders.Add(provider);
		}
	}

	public void RemoveDirectTouchMaid(DirectTouchMaid dtm) => activeDirectTouchMaids.Remove(dtm);

	private void ShowOrHideDirectTouchMaidMarkers(bool show)
	{
		Maid[] array = FindObjectsOfType<Maid>();
		int i = 0;
		while (i < array.Length)
		{
			Maid maid = array[i];
			if (!show)
			{
				Console.WriteLine("Hide Direct Touch marker: " + maid.name);
			}
			else
			{
				Console.WriteLine("Show Direct Touch marker: " + maid.name);
			}

			if (maid.body0 == null)
			{
				goto IL_0223;
			}

			DirectTouchMaid component = maid.body0.GetComponent<DirectTouchMaid>();
			if (component != null)
			{
				Console.WriteLine(component.touchAreas.Count);
				foreach (TouchArea touchArea in component.touchAreas)
				{
					if (touchArea.base_t != null)
					{
						string text = "__dummy_hitsphere_view_" + touchArea.name;
						Transform transform = CMT.SearchObjName(touchArea.base_t, text);
						if (show)
						{
							if (transform == null)
							{
								Vector3 vector =
									touchArea.base_t.TransformDirection(touchArea.position) *
									touchArea.pos_len;
								vector += touchArea.base_t.position;
								var gameObject = GameObject.CreatePrimitive(0);
								Console.WriteLine(touchArea.areaName);
								Console.WriteLine(vector);
								Console.WriteLine(touchArea.len);
								gameObject.name = text;
								Renderer component2 = gameObject.GetComponent<Renderer>();
								gameObject.transform.localScale = Vector3.one * touchArea.len * 2f;
								gameObject.transform.parent = touchArea.base_t;
								gameObject.transform.position = vector;
								gameObject.transform.rotation = touchArea.base_t.rotation;
								if (touchArea.areaName != MaidTouchAreaName.CUSTOM)
								{
									component2.sharedMaterial = _markerMaterial;
								}
								else
								{
									component2.sharedMaterial = _markerCustomMaterial;
								}
							}
						}
						else if (transform != null)
						{
							Destroy(transform.gameObject);
						}
					}
					else
					{
						Console.WriteLine("base_t is null: " + touchArea.name);
					}
				}
			}

			IL_022D:
			i++;
			continue;
			IL_0223:
			Console.WriteLine("body0 is null");
			goto IL_022D;
		}
	}

	private void TestTouch()
	{
		if (onPostHitTestHandlers.Count == 0 && onHitHandlers.Count == 0)
		{
			return;
		}

		var list = new List<TouchArea>();
		var list2 = new List<DirectTouchMaid>();
		list2.AddRange(activeDirectTouchMaids);
		foreach (DirectTouchMaid directTouchMaid in list2)
		{
			if (directTouchMaid == null || directTouchMaid.maid != null)
			{
				if (directTouchMaid != null && directTouchMaid.maid != null &&
				    directTouchMaid.maid.Visible)
				{
					bool flag = false;
					foreach (TouchArea touchArea in directTouchMaid.touchAreas)
					{
						if (touchArea.base_t == null)
						{
							flag = true;
						}
					}

					if (!flag)
					{
						list.AddRange(directTouchMaid.touchAreas);
					}
					else
					{
						Destroy(directTouchMaid);
						activeDirectTouchMaids.Remove(directTouchMaid);
					}
				}
			}
			else
			{
				activeDirectTouchMaids.Remove(directTouchMaid);
			}
		}

		foreach (DirectTouchController directTouchController in controllers)
		{
			if (directTouchController != null && directTouchController.isActiveAndEnabled)
			{
				directTouchController.HitTest(list);
			}
		}

		foreach (Action<DirectTouchTool> action in onPostHitTestHandlers)
		{
			try
			{
				action(this);
			}
			catch (Exception ex)
			{
				if (debug)
				{
					Console.WriteLine(ex.ToString());
				}
			}
		}
	}

	private void Update2()
	{
		TestTouch();
		if (debug)
		{
			if (!Input.GetKey((KeyCode)304) && Input.GetKeyDown((KeyCode)285))
			{
				Console.WriteLine("Show Direct Touch Maid Markers");
				ShowOrHideDirectTouchMaidMarkers(true);
				return;
			}

			if (Input.GetKey((KeyCode)304) && Input.GetKeyDown((KeyCode)285))
			{
				Console.WriteLine("Show Direct Touch Maid Markers");
				ShowOrHideDirectTouchMaidMarkers(false);
			}
		}
	}

	public interface IDirectTouchEventHandlerProvider
	{
		void InstallDirectTouchEventHandler(DirectTouchTool tool, int level);
	}
}

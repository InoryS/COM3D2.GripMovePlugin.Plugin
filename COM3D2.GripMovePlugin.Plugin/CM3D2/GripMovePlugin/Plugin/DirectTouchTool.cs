using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x0200002E RID: 46
	public class DirectTouchTool : MonoBehaviour
	{
		// Token: 0x1700002E RID: 46
		// (get) Token: 0x06000179 RID: 377 RVA: 0x00003179 File Offset: 0x00001379
		public static DirectTouchTool Instance
		{
			get
			{
				if (DirectTouchTool._instance == null)
				{
					GameObject gameObject = new GameObject("DirectTouchTool");
					DirectTouchTool._instance = gameObject.AddComponent<DirectTouchTool>();
					global::UnityEngine.Object.DontDestroyOnLoad(gameObject);
				}
				return DirectTouchTool._instance;
			}
		}

		// Token: 0x0600017B RID: 379 RVA: 0x00009854 File Offset: 0x00007A54
		private void ActivateOnMaid(Maid maid)
		{
			TBody body = maid.body0;
			DirectTouchMaid directTouchMaid = maid.body0.gameObject.AddComponent<DirectTouchMaid>();
			this.activeDirectTouchMaids.Add(directTouchMaid);
			Transform transform = CMT.SearchObjName(maid.body0.transform, "Bip01 Head", true);
			directTouchMaid.AddTouchArea(MaidTouchAreaName.VAGINA, "_IK_vagina", this.touchSize_vagina, Vector3.zero);
			directTouchMaid.AddTouchArea(MaidTouchAreaName.ANAL, "_IK_anal", this.touchSize_anal, Vector3.zero);
			directTouchMaid.AddTouchArea(MaidTouchAreaName.HIP, "_IK_hipL", this.touchSize_hips, Vector3.zero);
			directTouchMaid.AddTouchArea(MaidTouchAreaName.HIP, "_IK_hipR", this.touchSize_hips, Vector3.zero);
			Transform transform2 = CMT.SearchObjName(maid.body0.transform, "Mune_L_sub", true);
			directTouchMaid.AddTouchArea(MaidTouchAreaName.MUNE, "Mune_L_nub", this.touchSize_mune_base * transform2.lossyScale.x, Vector3.zero);
			directTouchMaid.AddTouchArea(MaidTouchAreaName.MUNE, "Mune_R_nub", this.touchSize_mune_base * transform2.lossyScale.x, Vector3.zero);
			directTouchMaid.AddTouchArea(MaidTouchAreaName.HAIR, "Bip01 Head", this.touchSize_hair_base * transform.lossyScale.z, new Vector3(-0.13f * transform.lossyScale.x, 0f, 0f));
			directTouchMaid.AddTouchArea(MaidTouchAreaName.MOUTH, "Bip01 Head", this.touchSize_mouth, new Vector3(0.02f * transform.lossyScale.x, 0.07f * transform.lossyScale.y, 0f));
			directTouchMaid.AddTouchArea(MaidTouchAreaName.EYE, "Bip01 Head", this.touchSize_eye, new Vector3(-0.03f * transform.lossyScale.x, 0.07f * transform.lossyScale.y, 0f));
			directTouchMaid.AddTouchArea(MaidTouchAreaName.HARA, "_IK_hara", this.touchSize_hara, Vector3.zero);
			directTouchMaid.AddTouchArea(MaidTouchAreaName.KATA, "Kata_L", this.touchSize_kata, Vector3.zero);
			directTouchMaid.AddTouchArea(MaidTouchAreaName.KATA, "Kata_R", this.touchSize_kata, Vector3.zero);
			directTouchMaid.AddTouchArea(MaidTouchAreaName.HAND, "Bip01 L Hand", this.touchSize_hand, Vector3.zero);
			directTouchMaid.AddTouchArea(MaidTouchAreaName.HAND, "Bip01 R Hand", this.touchSize_hand, Vector3.zero);
			directTouchMaid.AddTouchArea(MaidTouchAreaName.MOMO, "momotwist_L_nub", this.touchSize_momo, Vector3.zero);
			directTouchMaid.AddTouchArea(MaidTouchAreaName.MOMO, "momotwist_R_nub", this.touchSize_momo, Vector3.zero);
			directTouchMaid.AddTouchArea(MaidTouchAreaName.FOOT, "Bip01 L Foot", this.touchSize_foot, Vector3.zero);
			directTouchMaid.AddTouchArea(MaidTouchAreaName.FOOT, "Bip01 R Foot", this.touchSize_foot, Vector3.zero);
			foreach (CustomMaidTouchArea customMaidTouchArea in this.customMaidTouchArea)
			{
				directTouchMaid.AddTouchArea(customMaidTouchArea.name, customMaidTouchArea.baseBoneName, customMaidTouchArea.radius, customMaidTouchArea.position);
			}
			this.OnActivateOnMaid(directTouchMaid);
			if (this.debug)
			{
				Console.WriteLine("Direct Touch installed: " + maid.name);
			}
		}

		// Token: 0x0600017C RID: 380 RVA: 0x00009B6C File Offset: 0x00007D6C
		public void AddCustomMaidTouchArea(string customName, string baseBoneName, Vector3 position, float radius)
		{
			CustomMaidTouchArea customMaidTouchArea = new CustomMaidTouchArea
			{
				name = customName,
				baseBoneName = baseBoneName,
				position = position,
				radius = radius
			};
			this.customMaidTouchArea.Add(customMaidTouchArea);
		}

		// Token: 0x0600017D RID: 381 RVA: 0x000031A7 File Offset: 0x000013A7
		public void AddOnAfterHitTestHandler(Action<DirectTouchTool> handler)
		{
			this.onPostHitTestHandlers.Add(handler);
		}

		// Token: 0x0600017E RID: 382 RVA: 0x000031B5 File Offset: 0x000013B5
		public void AddOnHitHandler(Action<DirectTouchController, TouchArea, bool> handler)
		{
			this.onHitHandlers.Add(handler);
		}

		// Token: 0x0600017F RID: 383 RVA: 0x000031C3 File Offset: 0x000013C3
		public void AddOnStayHandler(Action<DirectTouchController, TouchArea> handler)
		{
			this.onStayHandlers.Add(handler);
		}

		// Token: 0x06000180 RID: 384 RVA: 0x00009BA8 File Offset: 0x00007DA8
		private void Awake()
		{
			this._markerMaterial = new Material(MaterialHelper.GetColorZOrderShader());
			Color color = new Color(0f, 0f, 1f, 0.5f);
			this._markerMaterial.SetColor("_Color", color);
			this._markerCustomMaterial = new Material(MaterialHelper.GetColorZOrderShader());
			Color color2 = new Color(0f, 1f, 0f, 0.5f);
			this._markerCustomMaterial.SetColor("_Color", color2);
		}

		// Token: 0x06000181 RID: 385 RVA: 0x00009C30 File Offset: 0x00007E30
		private void InitTouchSize()
		{
			this.touchSize_vagina = Settings.Instance.GetFloatValue("DirectTouchTool_Size_VAGINA", 0.03f);
			this.touchSize_anal = Settings.Instance.GetFloatValue("DirectTouchTool_Size_ANAL", 0.03f);
			this.touchSize_hips = Settings.Instance.GetFloatValue("DirectTouchTool_Size_HIP", 0.12f);
			this.touchSize_mune_base = Settings.Instance.GetFloatValue("DirectTouchTool_Size_MUNE", 0.12f);
			this.touchSize_hair_base = Settings.Instance.GetFloatValue("DirectTouchTool_Size_HAIR", 0.13f);
			this.touchSize_mouth = Settings.Instance.GetFloatValue("DirectTouchTool_Size_MOUTH", 0.03f);
			this.touchSize_eye = Settings.Instance.GetFloatValue("DirectTouchTool_Size_EYE", 0.04f);
			this.touchSize_hara = Settings.Instance.GetFloatValue("DirectTouchTool_Size_HARA", 0.1f);
			this.touchSize_kata = Settings.Instance.GetFloatValue("DirectTouchTool_Size_KATA", 0.08f);
			this.touchSize_hand = Settings.Instance.GetFloatValue("DirectTouchTool_Size_HAND", 0.08f);
			this.touchSize_momo = Settings.Instance.GetFloatValue("DirectTouchTool_Size_MOMO", 0.1f);
			this.touchSize_foot = Settings.Instance.GetFloatValue("DirectTouchTool_Size_FOOT", 0.08f);
		}

		// Token: 0x06000182 RID: 386 RVA: 0x000031D1 File Offset: 0x000013D1
		private IEnumerator InstallDirectTouchToMaidCo()
		{
			CharacterMgr characterMgr = GameMain.Instance.CharacterMgr;
			for (;;)
			{
				characterMgr.GetMaidCount();
				for (int i = 0; i < characterMgr.GetMaidCount(); i++)
				{
					Maid maid = characterMgr.GetMaid(i);
					if (!(maid == null) && !(maid.body0 == null) && !maid.body0.boMAN)
					{
						DirectTouchMaid component = maid.body0.GetComponent<DirectTouchMaid>();
						if (component != null)
						{
							if (!this.activeDirectTouchMaids.Contains(component))
							{
								this.activeDirectTouchMaids.Add(component);
							}
						}
						else if (maid.body0.isLoadedBody)
						{
							this.ActivateOnMaid(maid);
						}
					}
				}
				yield return new WaitForSeconds(1f);
			}
			yield break;
		}

		// Token: 0x06000183 RID: 387 RVA: 0x00009D78 File Offset: 0x00007F78
		private void InstallTouchToOculusTouch()
		{
			OVRCameraRig componentInParent = GameMain.Instance.OvrMgr.OvrCamera.gameObject.GetComponentInParent<OVRCameraRig>();
			Transform centerEyeAnchor = componentInParent.centerEyeAnchor;
			Transform leftHandAnchor = componentInParent.leftHandAnchor;
			Transform rightHandAnchor = componentInParent.rightHandAnchor;
			if (centerEyeAnchor != null)
			{
				DirectTouchController directTouchController = centerEyeAnchor.gameObject.GetComponent<DirectTouchController>();
				if (directTouchController == null)
				{
					directTouchController = centerEyeAnchor.gameObject.AddComponent<DirectTouchController>();
					directTouchController.areaName = ControllerTouchAreaName.MOUTH;
					directTouchController.SetPosition(this.mouthPos);
					directTouchController.radius = this.mouthHitRadius;
					directTouchController.onHitHandler = new Action<DirectTouchController, TouchArea, bool>(this.OnHit);
					directTouchController.onStayHandler = new Action<DirectTouchController, TouchArea>(this.OnStay);
				}
				this.controllers.Add(directTouchController);
			}
			if (leftHandAnchor != null && (OVRInput.GetConnectedControllers() & OVRInput.Controller.Touch) != OVRInput.Controller.None)
			{
				DirectTouchController directTouchController2 = leftHandAnchor.gameObject.GetComponent<DirectTouchController>();
				if (directTouchController2 == null)
				{
					directTouchController2 = leftHandAnchor.gameObject.AddComponent<DirectTouchController>();
					directTouchController2.areaName = ControllerTouchAreaName.HAND_L;
					directTouchController2.radius = this.handHitRadius;
					directTouchController2.onHitHandler = new Action<DirectTouchController, TouchArea, bool>(this.OnHit);
					directTouchController2.onStayHandler = new Action<DirectTouchController, TouchArea>(this.OnStay);
					directTouchController2.vrdeviceWrapper = DeviceOculusTouchController.GetOrCreate(directTouchController2.gameObject, OVRInput.Controller.LTouch);
				}
				this.controllers.Add(directTouchController2);
			}
			if (rightHandAnchor != null && (OVRInput.GetConnectedControllers() & OVRInput.Controller.Touch) != OVRInput.Controller.None)
			{
				DirectTouchController directTouchController3 = rightHandAnchor.gameObject.GetComponent<DirectTouchController>();
				if (directTouchController3 == null)
				{
					directTouchController3 = rightHandAnchor.gameObject.AddComponent<DirectTouchController>();
					directTouchController3.areaName = ControllerTouchAreaName.HAND_R;
					directTouchController3.radius = this.handHitRadius;
					directTouchController3.onHitHandler = new Action<DirectTouchController, TouchArea, bool>(this.OnHit);
					directTouchController3.onStayHandler = new Action<DirectTouchController, TouchArea>(this.OnStay);
					directTouchController3.vrdeviceWrapper = DeviceOculusTouchController.GetOrCreate(directTouchController3.gameObject, OVRInput.Controller.RTouch);
				}
				this.controllers.Add(directTouchController3);
			}
		}

		// Token: 0x06000184 RID: 388 RVA: 0x00009F5C File Offset: 0x0000815C
		private void InstallTouchToViveControllers()
		{
			if (GameMain.Instance.OvrMgr.ovr_obj != null)
			{
				OvrMgr ovrMgr = GameMain.Instance.OvrMgr;
				OvrMgr.OvrObject ovr_obj = GameMain.Instance.OvrMgr.ovr_obj;
				if (ovrMgr.OvrCamera != null && ovrMgr.OvrCamera.gameObject != null)
				{
					foreach (SteamVR_TrackedObject steamVR_TrackedObject in ovrMgr.OvrCamera.gameObject.GetComponentsInChildren<SteamVR_TrackedObject>())
					{
						if (steamVR_TrackedObject.index == SteamVR_TrackedObject.EIndex.Hmd)
						{
							DirectTouchController directTouchController = steamVR_TrackedObject.gameObject.GetComponent<DirectTouchController>();
							if (directTouchController == null)
							{
								directTouchController = steamVR_TrackedObject.gameObject.AddComponent<DirectTouchController>();
								directTouchController.areaName = ControllerTouchAreaName.MOUTH;
								directTouchController.SetPosition(this.mouthPos);
								directTouchController.radius = this.mouthHitRadius;
								directTouchController.onHitHandler = new Action<DirectTouchController, TouchArea, bool>(this.OnHit);
								directTouchController.onStayHandler = new Action<DirectTouchController, TouchArea>(this.OnStay);
							}
							this.controllers.Add(directTouchController);
							break;
						}
					}
				}
				if (ovr_obj.left_controller.controller != null && ovr_obj.left_controller.controller.gameObject != null)
				{
					DirectTouchController directTouchController2 = ovr_obj.left_controller.controller.gameObject.GetComponent<DirectTouchController>();
					if (directTouchController2 == null)
					{
						directTouchController2 = ovr_obj.left_controller.controller.gameObject.AddComponent<DirectTouchController>();
						directTouchController2.areaName = ControllerTouchAreaName.HAND_L;
						directTouchController2.radius = this.handHitRadius;
						directTouchController2.onHitHandler = new Action<DirectTouchController, TouchArea, bool>(this.OnHit);
						directTouchController2.onStayHandler = new Action<DirectTouchController, TouchArea>(this.OnStay);
						directTouchController2.vrdeviceWrapper = DeviceViveController.GetOrCreate(directTouchController2.gameObject, ovr_obj.left_controller.controller.gameObject.GetComponent<SteamVR_TrackedObject>());
					}
					this.controllers.Add(directTouchController2);
				}
				if (ovr_obj.right_controller.controller != null && ovr_obj.right_controller.controller.gameObject != null)
				{
					DirectTouchController directTouchController3 = ovr_obj.right_controller.controller.gameObject.GetComponent<DirectTouchController>();
					if (directTouchController3 == null)
					{
						directTouchController3 = ovr_obj.right_controller.controller.gameObject.AddComponent<DirectTouchController>();
						directTouchController3.areaName = ControllerTouchAreaName.HAND_R;
						directTouchController3.radius = this.handHitRadius;
						directTouchController3.onHitHandler = new Action<DirectTouchController, TouchArea, bool>(this.OnHit);
						directTouchController3.onStayHandler = new Action<DirectTouchController, TouchArea>(this.OnStay);
						directTouchController3.vrdeviceWrapper = DeviceViveController.GetOrCreate(directTouchController3.gameObject, ovr_obj.right_controller.controller.GetComponent<SteamVR_TrackedObject>());
					}
					this.controllers.Add(directTouchController3);
				}
			}
		}

		// Token: 0x06000185 RID: 389 RVA: 0x000031E0 File Offset: 0x000013E0
		private void InstallTouchToVRControllers()
		{
			if (GameMain.Instance.VRFamily == GameMain.VRFamilyType.HTC)
			{
				this.InstallTouchToViveControllers();
				return;
			}
			if (GameMain.Instance.VRFamily == GameMain.VRFamilyType.Oculus)
			{
				this.InstallTouchToOculusTouch();
			}
		}

		// Token: 0x06000186 RID: 390 RVA: 0x0000A220 File Offset: 0x00008420
		private void OnActivateOnMaid(DirectTouchMaid dtm)
		{
			foreach (Action<DirectTouchMaid> action in this.onActivateOnMaidHandlers)
			{
				try
				{
					action(dtm);
				}
				catch (Exception ex)
				{
					if (this.debug)
					{
						Console.WriteLine(ex.ToString());
					}
				}
			}
		}

		// Token: 0x06000187 RID: 391 RVA: 0x0000A298 File Offset: 0x00008498
		private void OnHit(DirectTouchController controller, TouchArea ta, bool onHit)
		{
			if (this.debug)
			{
				if (!onHit)
				{
					Console.WriteLine(string.Format("OnHit[{2}](Exit) by {0} area {1} ", controller.areaName, ta.areaName, onHit));
				}
				else
				{
					Console.WriteLine(string.Format("OnHit[{2}] by {0} area {1} ", controller.areaName, ta.areaName, onHit));
				}
			}
			foreach (Action<DirectTouchController, TouchArea, bool> action in this.onHitHandlers)
			{
				try
				{
					action(controller, ta, onHit);
				}
				catch (Exception ex)
				{
					if (this.debug)
					{
						Console.WriteLine(ex.ToString());
					}
				}
			}
		}

		// Token: 0x06000188 RID: 392 RVA: 0x0000A374 File Offset: 0x00008574
		private void OnLevelWasLoaded(int level)
		{
			try
			{
				this.InitTouchSize();
				this.activeDirectTouchMaids = new List<DirectTouchMaid>();
				this.controllers = new List<DirectTouchController>();
				this.InstallTouchToVRControllers();
				base.StopAllCoroutines();
				base.StartCoroutine("InstallDirectTouchToMaidCo");
				this.onHitHandlers = new List<Action<DirectTouchController, TouchArea, bool>>();
				this.onPostHitTestHandlers = new List<Action<DirectTouchTool>>();
				this.onActivateOnMaidHandlers = new List<Action<DirectTouchMaid>>();
				this.customMaidTouchArea = new List<CustomMaidTouchArea>();
				foreach (DirectTouchTool.IDirectTouchEventHandlerProvider directTouchEventHandlerProvider in this.eventHandlerProviders)
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

		// Token: 0x06000189 RID: 393 RVA: 0x0000A45C File Offset: 0x0000865C
		private void OnStay(DirectTouchController controller, TouchArea ta)
		{
			foreach (Action<DirectTouchController, TouchArea> action in this.onStayHandlers)
			{
				try
				{
					action(controller, ta);
				}
				catch (Exception ex)
				{
					if (this.debug)
					{
						Console.WriteLine(ex.ToString());
					}
				}
			}
		}

		// Token: 0x0600018A RID: 394 RVA: 0x00003209 File Offset: 0x00001409
		public void RegisterEventHandlerProvider(DirectTouchTool.IDirectTouchEventHandlerProvider provider)
		{
			if (!this.eventHandlerProviders.Contains(provider))
			{
				this.eventHandlerProviders.Add(provider);
			}
		}

		// Token: 0x0600018B RID: 395 RVA: 0x00003225 File Offset: 0x00001425
		public void RemoveDirectTouchMaid(DirectTouchMaid dtm)
		{
			this.activeDirectTouchMaids.Remove(dtm);
		}

		// Token: 0x0600018C RID: 396 RVA: 0x0000A4D4 File Offset: 0x000086D4
		private void ShowOrHideDirectTouchMaidMarkers(bool show)
		{
			Maid[] array = global::UnityEngine.Object.FindObjectsOfType<Maid>();
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
				if (!(maid.body0 != null))
				{
					goto IL_0223;
				}
				DirectTouchMaid component = maid.body0.GetComponent<DirectTouchMaid>();
				if (component != null)
				{
					Console.WriteLine(component.touchAreas.Count);
					using (List<TouchArea>.Enumerator enumerator = component.touchAreas.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							TouchArea touchArea = enumerator.Current;
							if (touchArea.base_t != null)
							{
								string text = "__dummy_hitsphere_view_" + touchArea.name;
								Transform transform = CMT.SearchObjName(touchArea.base_t, text, true);
								if (show)
								{
									if (!(transform != null))
									{
										Vector3 vector = touchArea.base_t.TransformDirection(touchArea.position) * touchArea.pos_len;
										vector += touchArea.base_t.position;
										GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
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
											component2.sharedMaterial = this._markerMaterial;
										}
										else
										{
											component2.sharedMaterial = this._markerCustomMaterial;
										}
									}
								}
								else if (!(transform == null))
								{
									global::UnityEngine.Object.Destroy(transform.gameObject);
								}
							}
							else
							{
								Console.WriteLine("base_t is null: " + touchArea.name);
							}
						}
						goto IL_022D;
					}
					goto IL_0223;
				}
				IL_022D:
				i++;
				continue;
				IL_0223:
				Console.WriteLine("body0 is null");
				goto IL_022D;
			}
		}

		// Token: 0x0600018D RID: 397 RVA: 0x0000A738 File Offset: 0x00008938
		private void Start()
		{
			this.debug = Settings.Instance.GetBoolValue("DirectTouchToolDebug", false);
			this.directTouchEnabled = Settings.Instance.GetBoolValue("DirectTouchToolEnabled", true);
			if (!this.directTouchEnabled)
			{
				base.enabled = false;
			}
			this.handHitRadius = Settings.Instance.GetFloatValue("DirectTouchToolHandHitRadius", 0.03f);
			float floatValue = Settings.Instance.GetFloatValue("DirectTouchToolMouthHitPosY", -0.08f);
			float floatValue2 = Settings.Instance.GetFloatValue("DirectTouchToolMouthHitPosZ", 0.02f);
			this.mouthPos = new Vector3(0f, floatValue, floatValue2);
			this.mouthHitRadius = Settings.Instance.GetFloatValue("DirectTouchToolMouthHitRadius", 0.05f);
		}

		// Token: 0x0600018E RID: 398 RVA: 0x0000A7F0 File Offset: 0x000089F0
		private void TestTouch()
		{
			if (this.onPostHitTestHandlers.Count == 0 && this.onHitHandlers.Count == 0)
			{
				return;
			}
			List<TouchArea> list = new List<TouchArea>();
			List<DirectTouchMaid> list2 = new List<DirectTouchMaid>();
			list2.AddRange(this.activeDirectTouchMaids);
			foreach (DirectTouchMaid directTouchMaid in list2)
			{
				if (!(directTouchMaid != null) || !(directTouchMaid.maid == null))
				{
					if (directTouchMaid != null && directTouchMaid.maid != null && directTouchMaid.maid.Visible)
					{
						bool flag = false;
						using (List<TouchArea>.Enumerator enumerator2 = directTouchMaid.touchAreas.GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								if (!(enumerator2.Current.base_t != null))
								{
									flag = true;
								}
							}
						}
						if (!flag)
						{
							list.AddRange(directTouchMaid.touchAreas);
						}
						else
						{
							global::UnityEngine.Object.Destroy(directTouchMaid);
							this.activeDirectTouchMaids.Remove(directTouchMaid);
						}
					}
				}
				else
				{
					this.activeDirectTouchMaids.Remove(directTouchMaid);
				}
			}
			foreach (DirectTouchController directTouchController in this.controllers)
			{
				if (!(directTouchController == null) && directTouchController.isActiveAndEnabled)
				{
					directTouchController.HitTest(list);
				}
			}
			foreach (Action<DirectTouchTool> action in this.onPostHitTestHandlers)
			{
				try
				{
					action(this);
				}
				catch (Exception ex)
				{
					if (this.debug)
					{
						Console.WriteLine(ex.ToString());
					}
				}
			}
		}

		// Token: 0x0600018F RID: 399 RVA: 0x0000A9F4 File Offset: 0x00008BF4
		private void Update()
		{
			try
			{
				this.Update2();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		// Token: 0x06000190 RID: 400 RVA: 0x0000AA28 File Offset: 0x00008C28
		private void Update2()
		{
			this.TestTouch();
			if (this.debug)
			{
				if (!Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F4))
				{
					Console.WriteLine("Show Direct Touch Maid Markers");
					this.ShowOrHideDirectTouchMaidMarkers(true);
					return;
				}
				if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F4))
				{
					Console.WriteLine("Show Direct Touch Maid Markers");
					this.ShowOrHideDirectTouchMaidMarkers(false);
				}
			}
		}

		// Token: 0x0400010E RID: 270
		public static DirectTouchTool _instance;

		// Token: 0x0400010F RID: 271
		private Material _markerMaterial;

		// Token: 0x04000110 RID: 272
		private Material _markerCustomMaterial;

		// Token: 0x04000111 RID: 273
		public List<DirectTouchMaid> activeDirectTouchMaids = new List<DirectTouchMaid>();

		// Token: 0x04000112 RID: 274
		public List<DirectTouchController> controllers = new List<DirectTouchController>();

		// Token: 0x04000113 RID: 275
		private float handHitRadius = 0.03f;

		// Token: 0x04000114 RID: 276
		private Vector3 mouthPos = new Vector3(0f, -0.08f, 0.02f);

		// Token: 0x04000115 RID: 277
		private float mouthHitRadius = 0.05f;

		// Token: 0x04000116 RID: 278
		private List<DirectTouchTool.IDirectTouchEventHandlerProvider> eventHandlerProviders = new List<DirectTouchTool.IDirectTouchEventHandlerProvider>();

		// Token: 0x04000117 RID: 279
		private List<Action<DirectTouchController, TouchArea, bool>> onHitHandlers = new List<Action<DirectTouchController, TouchArea, bool>>();

		// Token: 0x04000118 RID: 280
		private List<Action<DirectTouchController, TouchArea>> onStayHandlers = new List<Action<DirectTouchController, TouchArea>>();

		// Token: 0x04000119 RID: 281
		private List<Action<DirectTouchTool>> onPostHitTestHandlers = new List<Action<DirectTouchTool>>();

		// Token: 0x0400011A RID: 282
		private List<Action<DirectTouchMaid>> onActivateOnMaidHandlers = new List<Action<DirectTouchMaid>>();

		// Token: 0x0400011B RID: 283
		private List<CustomMaidTouchArea> customMaidTouchArea = new List<CustomMaidTouchArea>();

		// Token: 0x0400011C RID: 284
		private float touchSize_vagina = 0.03f;

		// Token: 0x0400011D RID: 285
		private float touchSize_anal = 0.03f;

		// Token: 0x0400011E RID: 286
		private float touchSize_hips = 0.12f;

		// Token: 0x0400011F RID: 287
		private float touchSize_mune_base = 0.12f;

		// Token: 0x04000120 RID: 288
		private float touchSize_hair_base = 0.13f;

		// Token: 0x04000121 RID: 289
		private float touchSize_mouth = 0.03f;

		// Token: 0x04000122 RID: 290
		private float touchSize_eye = 0.04f;

		// Token: 0x04000123 RID: 291
		private float touchSize_hara = 0.1f;

		// Token: 0x04000124 RID: 292
		private float touchSize_kata = 0.08f;

		// Token: 0x04000125 RID: 293
		private float touchSize_hand = 0.08f;

		// Token: 0x04000126 RID: 294
		private float touchSize_momo = 0.1f;

		// Token: 0x04000127 RID: 295
		private float touchSize_foot = 0.08f;

		// Token: 0x04000128 RID: 296
		public bool directTouchEnabled = true;

		// Token: 0x04000129 RID: 297
		private bool debug;

		// Token: 0x0200002F RID: 47
		public interface IDirectTouchEventHandlerProvider
		{
			// Token: 0x06000191 RID: 401
			void InstallDirectTouchEventHandler(DirectTouchTool tool, int level);
		}
	}
}

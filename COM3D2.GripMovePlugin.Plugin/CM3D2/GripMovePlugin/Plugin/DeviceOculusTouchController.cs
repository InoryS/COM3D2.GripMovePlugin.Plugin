using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x0200003C RID: 60
	public class DeviceOculusTouchController : DeviceWrapper
	{
		// Token: 0x17000038 RID: 56
		// (get) Token: 0x0600020E RID: 526 RVA: 0x0000E08C File Offset: 0x0000C28C
		public override Vector3 angularVelocity
		{
			get
			{
				return OVRInput.GetLocalControllerAngularVelocity(this.rawController).eulerAngles;
			}
		}

		// Token: 0x17000039 RID: 57
		// (get) Token: 0x0600020F RID: 527 RVA: 0x00003826 File Offset: 0x00001A26
		public override bool connected
		{
			get
			{
				return (OVRInput.GetConnectedControllers() & this.controller) > OVRInput.Controller.None;
			}
		}

		// Token: 0x1700003A RID: 58
		// (get) Token: 0x06000210 RID: 528 RVA: 0x00003837 File Offset: 0x00001A37
		public override bool hasTracking
		{
			get
			{
				return this.connected;
			}
		}

		// Token: 0x1700003B RID: 59
		// (get) Token: 0x06000211 RID: 529 RVA: 0x00002821 File Offset: 0x00000A21
		public override bool outOfRange
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x06000212 RID: 530 RVA: 0x0000281E File Offset: 0x00000A1E
		public override bool valid
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700003D RID: 61
		// (get) Token: 0x06000213 RID: 531 RVA: 0x0000383F File Offset: 0x00001A3F
		public override Vector3 velocity
		{
			get
			{
				return OVRInput.GetLocalControllerVelocity(this.rawController);
			}
		}

		// Token: 0x06000215 RID: 533 RVA: 0x0000384C File Offset: 0x00001A4C
		public override Vector2 GetAxis(EVRButtonId buttonId = EVRButtonId.k_EButton_Axis0)
		{
			return OVRInput.Get(this.MapAxis2D(buttonId), OVRInput.Controller.Active);
		}

		// Token: 0x06000216 RID: 534 RVA: 0x0000E2B0 File Offset: 0x0000C4B0
		public static DeviceWrapper GetOrCreate(GameObject go, OVRInput.Controller ovrController)
		{
			DeviceOculusTouchController deviceOculusTouchController = go.GetComponent<DeviceOculusTouchController>();
			if (deviceOculusTouchController == null)
			{
				deviceOculusTouchController = go.AddComponent<DeviceOculusTouchController>();
				deviceOculusTouchController.Init(ovrController);
			}
			return deviceOculusTouchController;
		}

		// Token: 0x06000217 RID: 535 RVA: 0x0000385F File Offset: 0x00001A5F
		public override bool GetPress(EVRButtonId buttonId)
		{
			return OVRInput.Get(this.MapPressButtonId(buttonId), this.controller);
		}

		// Token: 0x06000218 RID: 536 RVA: 0x00003873 File Offset: 0x00001A73
		public override bool GetPress(ulong buttonMask)
		{
			return OVRInput.Get(this.MapPressButtonMask(buttonMask), this.controller);
		}

		// Token: 0x06000219 RID: 537 RVA: 0x00003887 File Offset: 0x00001A87
		public override bool GetPressDown(ulong buttonMask)
		{
			return OVRInput.GetDown(this.MapPressButtonMask(buttonMask), this.controller);
		}

		// Token: 0x0600021A RID: 538 RVA: 0x0000389B File Offset: 0x00001A9B
		public override bool GetPressDown(EVRButtonId buttonId)
		{
			return OVRInput.GetDown(this.MapPressButtonId(buttonId), this.controller);
		}

		// Token: 0x0600021B RID: 539 RVA: 0x000038AF File Offset: 0x00001AAF
		public override bool GetPressUp(ulong buttonMask)
		{
			return OVRInput.GetUp(this.MapPressButtonMask(buttonMask), this.controller);
		}

		// Token: 0x0600021C RID: 540 RVA: 0x000038C3 File Offset: 0x00001AC3
		public override bool GetPressUp(EVRButtonId buttonId)
		{
			return OVRInput.GetUp(this.MapPressButtonId(buttonId), this.controller);
		}

		// Token: 0x0600021D RID: 541 RVA: 0x000038D7 File Offset: 0x00001AD7
		public override bool GetTouch(ulong buttonMask)
		{
			return OVRInput.Get(this.MapTouchButtonMask(buttonMask), this.controller);
		}

		// Token: 0x0600021E RID: 542 RVA: 0x000038EB File Offset: 0x00001AEB
		public override bool GetTouch(EVRButtonId buttonId)
		{
			return OVRInput.Get(this.MapTouchButtonId(buttonId), this.controller);
		}

		// Token: 0x0600021F RID: 543 RVA: 0x000038FF File Offset: 0x00001AFF
		public override bool GetTouchDown(ulong buttonMask)
		{
			return OVRInput.GetDown(this.MapTouchButtonMask(buttonMask), this.controller);
		}

		// Token: 0x06000220 RID: 544 RVA: 0x00003913 File Offset: 0x00001B13
		public override bool GetTouchDown(EVRButtonId buttonId)
		{
			return OVRInput.GetDown(this.MapTouchButtonId(buttonId), this.controller);
		}

		// Token: 0x06000221 RID: 545 RVA: 0x00003927 File Offset: 0x00001B27
		public override bool GetTouchUp(EVRButtonId buttonId)
		{
			return OVRInput.GetUp(this.MapTouchButtonId(buttonId), this.controller);
		}

		// Token: 0x06000222 RID: 546 RVA: 0x0000393B File Offset: 0x00001B3B
		public override bool GetTouchUp(ulong buttonMask)
		{
			return OVRInput.GetUp(this.MapTouchButtonMask(buttonMask), this.controller);
		}

		// Token: 0x06000223 RID: 547 RVA: 0x0000E2DC File Offset: 0x0000C4DC
		public void Init(OVRInput.Controller rawController)
		{
			this.controller = OVRInput.Controller.Touch;
			if (rawController == OVRInput.Controller.LTouch)
			{
				this.rawController = OVRInput.Controller.LTouch;
				this.rawButtonMap = this.rawButtonMap_L;
				this.rawTouchMap = this.rawTouchMap_L;
				this.buttonMap = this.buttonMap_L;
				this.touchMap = this.touchMap_L;
				return;
			}
			this.rawController = OVRInput.Controller.RTouch;
			this.rawButtonMap = this.rawButtonMap_R;
			this.rawTouchMap = this.rawTouchMap_R;
			this.buttonMap = this.buttonMap_R;
			this.touchMap = this.touchMap_R;
		}

		// Token: 0x06000224 RID: 548 RVA: 0x0000394F File Offset: 0x00001B4F
		private OVRInput.RawAxis2D MapAxis2D(EVRButtonId buttonId)
		{
			if (this.rawController == OVRInput.Controller.LTouch)
			{
				return OVRInput.RawAxis2D.LThumbstick;
			}
			if (this.rawController == OVRInput.Controller.RTouch)
			{
				return OVRInput.RawAxis2D.RThumbstick;
			}
			return OVRInput.RawAxis2D.Any;
		}

		// Token: 0x06000225 RID: 549 RVA: 0x00003968 File Offset: 0x00001B68
		private OVRInput.Button MapPressButtonId(EVRButtonId buttonId)
		{
			if (!this.buttonMap.ContainsKey(buttonId))
			{
				return OVRInput.Button.None;
			}
			return this.buttonMap[buttonId];
		}

		// Token: 0x06000226 RID: 550 RVA: 0x0000E364 File Offset: 0x0000C564
		private OVRInput.Button MapPressButtonMask(ulong buttonMask)
		{
			OVRInput.Button button = OVRInput.Button.None;
			foreach (ulong num in this.steamVRButtonMaskMap.Keys)
			{
				if ((buttonMask & num) == num && this.buttonMap.ContainsKey(this.steamVRButtonMaskMap[num]))
				{
					button |= this.buttonMap[this.steamVRButtonMaskMap[num]];
				}
			}
			return button;
		}

		// Token: 0x06000227 RID: 551 RVA: 0x00003986 File Offset: 0x00001B86
		private OVRInput.Touch MapTouchButtonId(EVRButtonId buttonId)
		{
			if (!this.touchMap.ContainsKey(buttonId))
			{
				return OVRInput.Touch.None;
			}
			return this.touchMap[buttonId];
		}

		// Token: 0x06000228 RID: 552 RVA: 0x0000E3F4 File Offset: 0x0000C5F4
		private OVRInput.Touch MapTouchButtonMask(ulong buttonMask)
		{
			OVRInput.Touch touch = OVRInput.Touch.None;
			foreach (ulong num in this.steamVRButtonMaskMap.Keys)
			{
				if ((buttonMask & num) == num && this.touchMap.ContainsKey(this.steamVRButtonMaskMap[num]))
				{
					touch |= this.touchMap[this.steamVRButtonMaskMap[num]];
				}
			}
			return touch;
		}

		// Token: 0x06000229 RID: 553 RVA: 0x000039A4 File Offset: 0x00001BA4
		private OVRInput.RawButton RawMapPressButtonId(EVRButtonId buttonId)
		{
			if (!this.rawButtonMap.ContainsKey(buttonId))
			{
				return OVRInput.RawButton.None;
			}
			return this.rawButtonMap[buttonId];
		}

		// Token: 0x0600022A RID: 554 RVA: 0x0000E484 File Offset: 0x0000C684
		private OVRInput.RawButton RawMapPressButtonMask(ulong buttonMask)
		{
			OVRInput.RawButton rawButton = OVRInput.RawButton.None;
			foreach (ulong num in this.steamVRButtonMaskMap.Keys)
			{
				if ((buttonMask & num) == num && this.rawButtonMap.ContainsKey(this.steamVRButtonMaskMap[num]))
				{
					rawButton |= this.rawButtonMap[this.steamVRButtonMaskMap[num]];
				}
			}
			return rawButton;
		}

		// Token: 0x0600022B RID: 555 RVA: 0x000039C2 File Offset: 0x00001BC2
		private OVRInput.RawTouch RawMapTouchButtonId(EVRButtonId buttonId)
		{
			if (!this.rawTouchMap.ContainsKey(buttonId))
			{
				return OVRInput.RawTouch.None;
			}
			return this.rawTouchMap[buttonId];
		}

		// Token: 0x0600022C RID: 556 RVA: 0x0000E514 File Offset: 0x0000C714
		private OVRInput.RawTouch RawMapTouchButtonMask(ulong buttonMask)
		{
			OVRInput.RawTouch rawTouch = OVRInput.RawTouch.None;
			foreach (ulong num in this.steamVRButtonMaskMap.Keys)
			{
				if ((buttonMask & num) == num && this.rawTouchMap.ContainsKey(this.steamVRButtonMaskMap[num]))
				{
					rawTouch |= this.rawTouchMap[this.steamVRButtonMaskMap[num]];
				}
			}
			return rawTouch;
		}

		// Token: 0x0600022D RID: 557 RVA: 0x000039E0 File Offset: 0x00001BE0
		private IEnumerator StopHapticCo()
		{
			yield return new WaitForEndOfFrame();
			if (Time.time > this.hapticEnd)
			{
				OVRInput.SetControllerVibration(0f, 0f, this.rawController);
			}
			yield break;
		}

		// Token: 0x0600022E RID: 558 RVA: 0x0000E5A4 File Offset: 0x0000C7A4
		public override void TriggerHapticPulse(ushort durationMicroSec = 500, EVRButtonId buttonId = EVRButtonId.k_EButton_Axis0)
		{
			OVRInput.SetControllerVibration(0.3f, 0.8f, this.rawController);
			float num = (float)(durationMicroSec / 1000) / 1000f;
			this.hapticEnd = Time.time + num;
			base.StopAllCoroutines();
			base.StartCoroutine(this.StopHapticCo());
		}

		// Token: 0x04000175 RID: 373
		private OVRInput.Controller controller;

		// Token: 0x04000176 RID: 374
		private OVRInput.Controller rawController;

		// Token: 0x04000177 RID: 375
		private Dictionary<EVRButtonId, OVRInput.RawButton> rawButtonMap;

		// Token: 0x04000178 RID: 376
		private Dictionary<EVRButtonId, OVRInput.Button> buttonMap;

		// Token: 0x04000179 RID: 377
		private Dictionary<EVRButtonId, OVRInput.RawTouch> rawTouchMap;

		// Token: 0x0400017A RID: 378
		private Dictionary<EVRButtonId, OVRInput.Touch> touchMap;

		// Token: 0x0400017B RID: 379
		private float hapticEnd;

		// Token: 0x0400017C RID: 380
		private Dictionary<ulong, EVRButtonId> steamVRButtonMaskMap = new Dictionary<ulong, EVRButtonId>
		{
			{
				1UL,
				EVRButtonId.k_EButton_System
			},
			{
				2UL,
				EVRButtonId.k_EButton_ApplicationMenu
			},
			{
				4UL,
				EVRButtonId.k_EButton_Grip
			},
			{
				4294967296UL,
				EVRButtonId.k_EButton_Axis0
			},
			{
				8589934592UL,
				EVRButtonId.k_EButton_Axis1
			},
			{
				17179869184UL,
				EVRButtonId.k_EButton_Axis2
			},
			{
				34359738368UL,
				EVRButtonId.k_EButton_Axis3
			},
			{
				68719476736UL,
				EVRButtonId.k_EButton_Axis4
			}
		};

		// Token: 0x0400017D RID: 381
		private Dictionary<EVRButtonId, OVRInput.RawButton> rawButtonMap_L = new Dictionary<EVRButtonId, OVRInput.RawButton>
		{
			{
				EVRButtonId.k_EButton_Axis1,
				OVRInput.RawButton.LIndexTrigger
			},
			{
				EVRButtonId.k_EButton_Grip,
				OVRInput.RawButton.LHandTrigger
			},
			{
				EVRButtonId.k_EButton_ApplicationMenu,
				OVRInput.RawButton.B
			},
			{
				EVRButtonId.k_EButton_System,
				OVRInput.RawButton.A
			},
			{
				EVRButtonId.k_EButton_Axis0,
				OVRInput.RawButton.LThumbstick
			}
		};

		// Token: 0x0400017E RID: 382
		private Dictionary<EVRButtonId, OVRInput.RawButton> rawButtonMap_R = new Dictionary<EVRButtonId, OVRInput.RawButton>
		{
			{
				EVRButtonId.k_EButton_Axis1,
				OVRInput.RawButton.RIndexTrigger
			},
			{
				EVRButtonId.k_EButton_Grip,
				OVRInput.RawButton.RHandTrigger
			},
			{
				EVRButtonId.k_EButton_ApplicationMenu,
				OVRInput.RawButton.Y
			},
			{
				EVRButtonId.k_EButton_System,
				OVRInput.RawButton.X
			},
			{
				EVRButtonId.k_EButton_Axis0,
				OVRInput.RawButton.RThumbstick
			}
		};

		// Token: 0x0400017F RID: 383
		private Dictionary<EVRButtonId, OVRInput.Button> buttonMap_L = new Dictionary<EVRButtonId, OVRInput.Button>
		{
			{
				EVRButtonId.k_EButton_Axis1,
				OVRInput.Button.PrimaryIndexTrigger
			},
			{
				EVRButtonId.k_EButton_Grip,
				OVRInput.Button.PrimaryHandTrigger
			},
			{
				EVRButtonId.k_EButton_ApplicationMenu,
				OVRInput.Button.Four
			},
			{
				EVRButtonId.k_EButton_Axis0,
				OVRInput.Button.Three
			}
		};

		// Token: 0x04000180 RID: 384
		private Dictionary<EVRButtonId, OVRInput.Button> buttonMap_R = new Dictionary<EVRButtonId, OVRInput.Button>
		{
			{
				EVRButtonId.k_EButton_Axis1,
				OVRInput.Button.SecondaryIndexTrigger
			},
			{
				EVRButtonId.k_EButton_Grip,
				OVRInput.Button.SecondaryHandTrigger
			},
			{
				EVRButtonId.k_EButton_ApplicationMenu,
				OVRInput.Button.Two
			},
			{
				EVRButtonId.k_EButton_Axis0,
				OVRInput.Button.One
			}
		};

		// Token: 0x04000181 RID: 385
		private Dictionary<EVRButtonId, OVRInput.RawTouch> rawTouchMap_L = new Dictionary<EVRButtonId, OVRInput.RawTouch>
		{
			{
				EVRButtonId.k_EButton_Axis0,
				OVRInput.RawTouch.X
			},
			{
				EVRButtonId.k_EButton_Axis1,
				OVRInput.RawTouch.LIndexTrigger
			}
		};

		// Token: 0x04000182 RID: 386
		private Dictionary<EVRButtonId, OVRInput.RawTouch> rawTouchMap_R = new Dictionary<EVRButtonId, OVRInput.RawTouch>
		{
			{
				EVRButtonId.k_EButton_Axis0,
				OVRInput.RawTouch.A
			},
			{
				EVRButtonId.k_EButton_Axis1,
				OVRInput.RawTouch.RIndexTrigger
			}
		};

		// Token: 0x04000183 RID: 387
		private Dictionary<EVRButtonId, OVRInput.Touch> touchMap_L = new Dictionary<EVRButtonId, OVRInput.Touch>
		{
			{
				EVRButtonId.k_EButton_Axis0,
				OVRInput.Touch.Three
			},
			{
				EVRButtonId.k_EButton_Axis1,
				OVRInput.Touch.PrimaryIndexTrigger
			}
		};

		// Token: 0x04000184 RID: 388
		private Dictionary<EVRButtonId, OVRInput.Touch> touchMap_R = new Dictionary<EVRButtonId, OVRInput.Touch>
		{
			{
				EVRButtonId.k_EButton_Axis0,
				OVRInput.Touch.One
			},
			{
				EVRButtonId.k_EButton_Axis1,
				OVRInput.Touch.SecondaryIndexTrigger
			}
		};
	}
}

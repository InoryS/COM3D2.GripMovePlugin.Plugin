using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace CM3D2.GripMovePlugin.Plugin
{
	public class DeviceOculusTouchController : DeviceWrapper
	{
		public override Vector3 angularVelocity
		{
			get
			{
				return OVRInput.GetLocalControllerAngularVelocity(this.rawController).eulerAngles;
			}
		}

		public override bool connected
		{
			get
			{
				return (OVRInput.GetConnectedControllers() & this.controller) > 0;
			}
		}

		public override bool hasTracking
		{
			get
			{
				return this.connected;
			}
		}

		public override bool outOfRange
		{
			get
			{
				return false;
			}
		}

		public override bool valid
		{
			get
			{
				return true;
			}
		}

		public override Vector3 velocity
		{
			get
			{
				return OVRInput.GetLocalControllerVelocity(this.rawController);
			}
		}

		public override Vector2 GetAxis(EVRButtonId buttonId = (EVRButtonId)32)
		{
			return OVRInput.Get(this.MapAxis2D(buttonId), (OVRInput.Controller)int.MinValue);
		}

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

		public override bool GetPress(EVRButtonId buttonId)
		{
			return OVRInput.Get(this.MapPressButtonId(buttonId), this.controller);
		}

		public override bool GetPress(ulong buttonMask)
		{
			return OVRInput.Get(this.MapPressButtonMask(buttonMask), this.controller);
		}

		public override bool GetPressDown(ulong buttonMask)
		{
			return OVRInput.GetDown(this.MapPressButtonMask(buttonMask), this.controller);
		}

		public override bool GetPressDown(EVRButtonId buttonId)
		{
			return OVRInput.GetDown(this.MapPressButtonId(buttonId), this.controller);
		}

		public override bool GetPressUp(ulong buttonMask)
		{
			return OVRInput.GetUp(this.MapPressButtonMask(buttonMask), this.controller);
		}

		public override bool GetPressUp(EVRButtonId buttonId)
		{
			return OVRInput.GetUp(this.MapPressButtonId(buttonId), this.controller);
		}

		public override bool GetTouch(ulong buttonMask)
		{
			return OVRInput.Get(this.MapTouchButtonMask(buttonMask), this.controller);
		}

		public override bool GetTouch(EVRButtonId buttonId)
		{
			return OVRInput.Get(this.MapTouchButtonId(buttonId), this.controller);
		}

		public override bool GetTouchDown(ulong buttonMask)
		{
			return OVRInput.GetDown(this.MapTouchButtonMask(buttonMask), this.controller);
		}

		public override bool GetTouchDown(EVRButtonId buttonId)
		{
			return OVRInput.GetDown(this.MapTouchButtonId(buttonId), this.controller);
		}

		public override bool GetTouchUp(EVRButtonId buttonId)
		{
			return OVRInput.GetUp(this.MapTouchButtonId(buttonId), this.controller);
		}

		public override bool GetTouchUp(ulong buttonMask)
		{
			return OVRInput.GetUp(this.MapTouchButtonMask(buttonMask), this.controller);
		}

		public void Init(OVRInput.Controller rawController)
		{
			this.controller = (OVRInput.Controller)3;
			if (rawController == (OVRInput.Controller)1)
			{
				this.rawController = (OVRInput.Controller)1;
				this.rawButtonMap = this.rawButtonMap_L;
				this.rawTouchMap = this.rawTouchMap_L;
				this.buttonMap = this.buttonMap_L;
				this.touchMap = this.touchMap_L;
				return;
			}
			this.rawController = (OVRInput.Controller)2;
			this.rawButtonMap = this.rawButtonMap_R;
			this.rawTouchMap = this.rawTouchMap_R;
			this.buttonMap = this.buttonMap_R;
			this.touchMap = this.touchMap_R;
		}

		private OVRInput.RawAxis2D MapAxis2D(EVRButtonId buttonId)
		{
			if (this.rawController == (OVRInput.Controller)1)
			{
				return (OVRInput.RawAxis2D)1;
			}
			if (this.rawController == (OVRInput.Controller)2)
			{
				return (OVRInput.RawAxis2D)2;
			}
			return (OVRInput.RawAxis2D)(-1);
		}

		private OVRInput.Button MapPressButtonId(EVRButtonId buttonId)
		{
			if (!this.buttonMap.ContainsKey(buttonId))
			{
				return 0;
			}
			return this.buttonMap[buttonId];
		}

		private OVRInput.Button MapPressButtonMask(ulong buttonMask)
		{
			OVRInput.Button button = 0;
			foreach (ulong num in this.steamVRButtonMaskMap.Keys)
			{
				if ((buttonMask & num) == num && this.buttonMap.ContainsKey(this.steamVRButtonMaskMap[num]))
				{
					button |= this.buttonMap[this.steamVRButtonMaskMap[num]];
				}
			}
			return button;
		}

		private OVRInput.Touch MapTouchButtonId(EVRButtonId buttonId)
		{
			if (!this.touchMap.ContainsKey(buttonId))
			{
				return 0;
			}
			return this.touchMap[buttonId];
		}

		private OVRInput.Touch MapTouchButtonMask(ulong buttonMask)
		{
			OVRInput.Touch touch = 0;
			foreach (ulong num in this.steamVRButtonMaskMap.Keys)
			{
				if ((buttonMask & num) == num && this.touchMap.ContainsKey(this.steamVRButtonMaskMap[num]))
				{
					touch |= this.touchMap[this.steamVRButtonMaskMap[num]];
				}
			}
			return touch;
		}

		private OVRInput.RawButton RawMapPressButtonId(EVRButtonId buttonId)
		{
			if (!this.rawButtonMap.ContainsKey(buttonId))
			{
				return 0;
			}
			return this.rawButtonMap[buttonId];
		}

		private OVRInput.RawButton RawMapPressButtonMask(ulong buttonMask)
		{
			OVRInput.RawButton rawButton = 0;
			foreach (ulong num in this.steamVRButtonMaskMap.Keys)
			{
				if ((buttonMask & num) == num && this.rawButtonMap.ContainsKey(this.steamVRButtonMaskMap[num]))
				{
					rawButton |= this.rawButtonMap[this.steamVRButtonMaskMap[num]];
				}
			}
			return rawButton;
		}

		private OVRInput.RawTouch RawMapTouchButtonId(EVRButtonId buttonId)
		{
			if (!this.rawTouchMap.ContainsKey(buttonId))
			{
				return 0;
			}
			return this.rawTouchMap[buttonId];
		}

		private OVRInput.RawTouch RawMapTouchButtonMask(ulong buttonMask)
		{
			OVRInput.RawTouch rawTouch = 0;
			foreach (ulong num in this.steamVRButtonMaskMap.Keys)
			{
				if ((buttonMask & num) == num && this.rawTouchMap.ContainsKey(this.steamVRButtonMaskMap[num]))
				{
					rawTouch |= this.rawTouchMap[this.steamVRButtonMaskMap[num]];
				}
			}
			return rawTouch;
		}

		private IEnumerator StopHapticCo()
		{
			yield return new WaitForEndOfFrame();
			if (Time.time > this.hapticEnd)
			{
				OVRInput.SetControllerVibration(0f, 0f, this.rawController);
			}
			yield break;
		}

		public override void TriggerHapticPulse(ushort durationMicroSec = 500, EVRButtonId buttonId = (EVRButtonId)32)
		{
			OVRInput.SetControllerVibration(0.3f, 0.8f, this.rawController);
			float num = (float)(durationMicroSec / 1000) / 1000f;
			this.hapticEnd = Time.time + num;
			base.StopAllCoroutines();
			base.StartCoroutine(this.StopHapticCo());
		}

		private OVRInput.Controller controller;

		private OVRInput.Controller rawController;

		private Dictionary<EVRButtonId, OVRInput.RawButton> rawButtonMap;

		private Dictionary<EVRButtonId, OVRInput.Button> buttonMap;

		private Dictionary<EVRButtonId, OVRInput.RawTouch> rawTouchMap;

		private Dictionary<EVRButtonId, OVRInput.Touch> touchMap;

		private float hapticEnd;

		private Dictionary<ulong, EVRButtonId> steamVRButtonMaskMap = new Dictionary<ulong, EVRButtonId>
		{
			{ 1UL, (EVRButtonId)0 },
			{ 2UL, (EVRButtonId)1 },
			{ 4UL, (EVRButtonId)2 },
			{ 4294967296UL, (EVRButtonId)32 },
			{ 8589934592UL, (EVRButtonId)33 },
			{ 17179869184UL, (EVRButtonId)34 },
			{ 34359738368UL, (EVRButtonId)35 },
			{ 68719476736UL, (EVRButtonId)36 }
		};

		private Dictionary<EVRButtonId, OVRInput.RawButton> rawButtonMap_L = new Dictionary<EVRButtonId, OVRInput.RawButton>
		{
			{ (EVRButtonId)33, (OVRInput.RawButton)268435456 },
			{ (EVRButtonId)2, (OVRInput.RawButton)536870912 },
			{ (EVRButtonId)1, (OVRInput.RawButton)2 },
			{ (EVRButtonId)0, (OVRInput.RawButton)1 },
			{ (EVRButtonId)32, (OVRInput.RawButton)1024 }
		};

		private Dictionary<EVRButtonId, OVRInput.RawButton> rawButtonMap_R = new Dictionary<EVRButtonId, OVRInput.RawButton>
		{
			{ (EVRButtonId)33, (OVRInput.RawButton)67108864 },
			{ (EVRButtonId)2, (OVRInput.RawButton)134217728 },
			{ (EVRButtonId)1, (OVRInput.RawButton)512 },
			{ (EVRButtonId)0, (OVRInput.RawButton)256 },
			{ (EVRButtonId)32, (OVRInput.RawButton)4 }
		};

		private Dictionary<EVRButtonId, OVRInput.Button> buttonMap_L = new Dictionary<EVRButtonId, OVRInput.Button>
		{
			{ (EVRButtonId)33, (OVRInput.Button)8192 },
			{ (EVRButtonId)2, (OVRInput.Button)16384 },
			{ (EVRButtonId)1, (OVRInput.Button)8 },
			{ (EVRButtonId)32, (OVRInput.Button)4 }
		};

		private Dictionary<EVRButtonId, OVRInput.Button> buttonMap_R = new Dictionary<EVRButtonId, OVRInput.Button>
		{
			{ (EVRButtonId)33, (OVRInput.Button)2097152 },
			{ (EVRButtonId)2, (OVRInput.Button)4194304 },
			{ (EVRButtonId)1, (OVRInput.Button)2 },
			{ (EVRButtonId)32, (OVRInput.Button)1 }
		};

		private Dictionary<EVRButtonId, OVRInput.RawTouch> rawTouchMap_L = new Dictionary<EVRButtonId, OVRInput.RawTouch>
		{
			{ (EVRButtonId)32, (OVRInput.RawTouch)256 },
			{ (EVRButtonId)33, (OVRInput.RawTouch)4096 }
		};

		private Dictionary<EVRButtonId, OVRInput.RawTouch> rawTouchMap_R = new Dictionary<EVRButtonId, OVRInput.RawTouch>
		{
			{ (EVRButtonId)32, (OVRInput.RawTouch)1 },
			{ (EVRButtonId)33, (OVRInput.RawTouch)16 }
		};

		private Dictionary<EVRButtonId, OVRInput.Touch> touchMap_L = new Dictionary<EVRButtonId, OVRInput.Touch>
		{
			{ (EVRButtonId)32, (OVRInput.Touch)4 },
			{ (EVRButtonId)33, (OVRInput.Touch)8192 }
		};

		private Dictionary<EVRButtonId, OVRInput.Touch> touchMap_R = new Dictionary<EVRButtonId, OVRInput.Touch>
		{
			{ (EVRButtonId)32, (OVRInput.Touch)1 },
			{ (EVRButtonId)33, (OVRInput.Touch)2097152 }
		};
	}
}

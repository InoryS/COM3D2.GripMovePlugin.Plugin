using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace COM3D2.GripMovePlugin.Plugin;

public class DeviceOculusTouchController : DeviceWrapper
{
	private readonly Dictionary<EVRButtonId, OVRInput.Button> buttonMap_L = new()
	{
		{ (EVRButtonId)33, (OVRInput.Button)8192 },
		{ (EVRButtonId)2, (OVRInput.Button)16384 },
		{ (EVRButtonId)1, (OVRInput.Button)8 },
		{ (EVRButtonId)32, (OVRInput.Button)4 }
	};

	private readonly Dictionary<EVRButtonId, OVRInput.Button> buttonMap_R = new()
	{
		{ (EVRButtonId)33, (OVRInput.Button)2097152 },
		{ (EVRButtonId)2, (OVRInput.Button)4194304 },
		{ (EVRButtonId)1, (OVRInput.Button)2 },
		{ (EVRButtonId)32, (OVRInput.Button)1 }
	};

	private readonly Dictionary<EVRButtonId, OVRInput.RawButton> rawButtonMap_L = new()
	{
		{ (EVRButtonId)33, (OVRInput.RawButton)268435456 },
		{ (EVRButtonId)2, (OVRInput.RawButton)536870912 },
		{ (EVRButtonId)1, (OVRInput.RawButton)2 },
		{ 0, (OVRInput.RawButton)1 },
		{ (EVRButtonId)32, (OVRInput.RawButton)1024 }
	};

	private readonly Dictionary<EVRButtonId, OVRInput.RawButton> rawButtonMap_R = new()
	{
		{ (EVRButtonId)33, (OVRInput.RawButton)67108864 },
		{ (EVRButtonId)2, (OVRInput.RawButton)134217728 },
		{ (EVRButtonId)1, (OVRInput.RawButton)512 },
		{ 0, (OVRInput.RawButton)256 },
		{ (EVRButtonId)32, (OVRInput.RawButton)4 }
	};

	private readonly Dictionary<EVRButtonId, OVRInput.RawTouch> rawTouchMap_L = new()
	{
		{ (EVRButtonId)32, (OVRInput.RawTouch)256 },
		{ (EVRButtonId)33, (OVRInput.RawTouch)4096 }
	};

	private readonly Dictionary<EVRButtonId, OVRInput.RawTouch> rawTouchMap_R = new()
	{
		{ (EVRButtonId)32, (OVRInput.RawTouch)1 }, { (EVRButtonId)33, (OVRInput.RawTouch)16 }
	};

	private readonly Dictionary<ulong, EVRButtonId> steamVRButtonMaskMap = new()
	{
		{ 1UL, 0 },
		{ 2UL, (EVRButtonId)1 },
		{ 4UL, (EVRButtonId)2 },
		{ 4294967296UL, (EVRButtonId)32 },
		{ 8589934592UL, (EVRButtonId)33 },
		{ 17179869184UL, (EVRButtonId)34 },
		{ 34359738368UL, (EVRButtonId)35 },
		{ 68719476736UL, (EVRButtonId)36 }
	};

	private readonly Dictionary<EVRButtonId, OVRInput.Touch> touchMap_L = new()
	{
		{ (EVRButtonId)32, (OVRInput.Touch)4 }, { (EVRButtonId)33, (OVRInput.Touch)8192 }
	};

	private readonly Dictionary<EVRButtonId, OVRInput.Touch> touchMap_R = new()
	{
		{ (EVRButtonId)32, (OVRInput.Touch)1 }, { (EVRButtonId)33, (OVRInput.Touch)2097152 }
	};

	private Dictionary<EVRButtonId, OVRInput.Button> buttonMap;

	private OVRInput.Controller controller;

	private float hapticEnd;

	private Dictionary<EVRButtonId, OVRInput.RawButton> rawButtonMap;

	private OVRInput.Controller rawController;

	private Dictionary<EVRButtonId, OVRInput.RawTouch> rawTouchMap;

	private Dictionary<EVRButtonId, OVRInput.Touch> touchMap;

	public override Vector3 angularVelocity =>
		OVRInput.GetLocalControllerAngularVelocity(rawController).eulerAngles;

	public override bool connected => (OVRInput.GetConnectedControllers() & controller) > 0;

	public override bool hasTracking => connected;

	public override bool outOfRange => false;

	public override bool valid => true;

	public override Vector3 velocity => OVRInput.GetLocalControllerVelocity(rawController);

	public override Vector2 GetAxis(EVRButtonId buttonId = (EVRButtonId)32) =>
		OVRInput.Get(MapAxis2D(buttonId));

	public static DeviceWrapper GetOrCreate(GameObject go, OVRInput.Controller ovrController)
	{
		DeviceOculusTouchController deviceOculusTouchController =
			go.GetComponent<DeviceOculusTouchController>();
		if (deviceOculusTouchController == null)
		{
			deviceOculusTouchController = go.AddComponent<DeviceOculusTouchController>();
			deviceOculusTouchController.Init(ovrController);
		}

		return deviceOculusTouchController;
	}

	public override bool GetPress(EVRButtonId buttonId) =>
		OVRInput.Get(MapPressButtonId(buttonId), controller);

	public override bool GetPress(ulong buttonMask) =>
		OVRInput.Get(MapPressButtonMask(buttonMask), controller);

	public override bool GetPressDown(ulong buttonMask) =>
		OVRInput.GetDown(MapPressButtonMask(buttonMask), controller);

	public override bool GetPressDown(EVRButtonId buttonId) =>
		OVRInput.GetDown(MapPressButtonId(buttonId), controller);

	public override bool GetPressUp(ulong buttonMask) =>
		OVRInput.GetUp(MapPressButtonMask(buttonMask), controller);

	public override bool GetPressUp(EVRButtonId buttonId) =>
		OVRInput.GetUp(MapPressButtonId(buttonId), controller);

	public override bool GetTouch(ulong buttonMask) =>
		OVRInput.Get(MapTouchButtonMask(buttonMask), controller);

	public override bool GetTouch(EVRButtonId buttonId) =>
		OVRInput.Get(MapTouchButtonId(buttonId), controller);

	public override bool GetTouchDown(ulong buttonMask) =>
		OVRInput.GetDown(MapTouchButtonMask(buttonMask), controller);

	public override bool GetTouchDown(EVRButtonId buttonId) =>
		OVRInput.GetDown(MapTouchButtonId(buttonId), controller);

	public override bool GetTouchUp(EVRButtonId buttonId) =>
		OVRInput.GetUp(MapTouchButtonId(buttonId), controller);

	public override bool GetTouchUp(ulong buttonMask) =>
		OVRInput.GetUp(MapTouchButtonMask(buttonMask), controller);

	public void Init(OVRInput.Controller rawController)
	{
		controller = (OVRInput.Controller)3;
		if (rawController == (OVRInput.Controller)1)
		{
			this.rawController = (OVRInput.Controller)1;
			rawButtonMap = rawButtonMap_L;
			rawTouchMap = rawTouchMap_L;
			buttonMap = buttonMap_L;
			touchMap = touchMap_L;
			return;
		}

		this.rawController = (OVRInput.Controller)2;
		rawButtonMap = rawButtonMap_R;
		rawTouchMap = rawTouchMap_R;
		buttonMap = buttonMap_R;
		touchMap = touchMap_R;
	}

	private OVRInput.RawAxis2D MapAxis2D(EVRButtonId buttonId)
	{
		if (rawController == (OVRInput.Controller)1)
		{
			return (OVRInput.RawAxis2D)1;
		}

		if (rawController == (OVRInput.Controller)2)
		{
			return (OVRInput.RawAxis2D)2;
		}

		return (OVRInput.RawAxis2D)(-1);
	}

	private OVRInput.Button MapPressButtonId(EVRButtonId buttonId)
	{
		if (!buttonMap.ContainsKey(buttonId))
		{
			return 0;
		}

		return buttonMap[buttonId];
	}

	private OVRInput.Button MapPressButtonMask(ulong buttonMask)
	{
		OVRInput.Button button = 0;
		foreach (ulong num in steamVRButtonMaskMap.Keys)
		{
			if ((buttonMask & num) == num && buttonMap.ContainsKey(steamVRButtonMaskMap[num]))
			{
				button |= buttonMap[steamVRButtonMaskMap[num]];
			}
		}

		return button;
	}

	private OVRInput.Touch MapTouchButtonId(EVRButtonId buttonId)
	{
		if (!touchMap.ContainsKey(buttonId))
		{
			return 0;
		}

		return touchMap[buttonId];
	}

	private OVRInput.Touch MapTouchButtonMask(ulong buttonMask)
	{
		OVRInput.Touch touch = 0;
		foreach (ulong num in steamVRButtonMaskMap.Keys)
		{
			if ((buttonMask & num) == num && touchMap.ContainsKey(steamVRButtonMaskMap[num]))
			{
				touch |= touchMap[steamVRButtonMaskMap[num]];
			}
		}

		return touch;
	}

	private OVRInput.RawButton RawMapPressButtonId(EVRButtonId buttonId)
	{
		if (!rawButtonMap.ContainsKey(buttonId))
		{
			return 0;
		}

		return rawButtonMap[buttonId];
	}

	private OVRInput.RawButton RawMapPressButtonMask(ulong buttonMask)
	{
		OVRInput.RawButton rawButton = 0;
		foreach (ulong num in steamVRButtonMaskMap.Keys)
		{
			if ((buttonMask & num) == num && rawButtonMap.ContainsKey(steamVRButtonMaskMap[num]))
			{
				rawButton |= rawButtonMap[steamVRButtonMaskMap[num]];
			}
		}

		return rawButton;
	}

	private OVRInput.RawTouch RawMapTouchButtonId(EVRButtonId buttonId)
	{
		if (!rawTouchMap.ContainsKey(buttonId))
		{
			return 0;
		}

		return rawTouchMap[buttonId];
	}

	private OVRInput.RawTouch RawMapTouchButtonMask(ulong buttonMask)
	{
		OVRInput.RawTouch rawTouch = 0;
		foreach (ulong num in steamVRButtonMaskMap.Keys)
		{
			if ((buttonMask & num) == num && rawTouchMap.ContainsKey(steamVRButtonMaskMap[num]))
			{
				rawTouch |= rawTouchMap[steamVRButtonMaskMap[num]];
			}
		}

		return rawTouch;
	}

	private IEnumerator StopHapticCo()
	{
		yield return new WaitForEndOfFrame();
		if (Time.time > hapticEnd)
		{
			OVRInput.SetControllerVibration(0f, 0f, rawController);
		}

		yield break;
	}

	public override void TriggerHapticPulse(ushort durationMicroSec = 500,
		EVRButtonId buttonId = (EVRButtonId)32)
	{
		OVRInput.SetControllerVibration(0.3f, 0.8f, rawController);
		float num = durationMicroSec / 1000 / 1000f;
		hapticEnd = Time.time + num;
		StopAllCoroutines();
		StartCoroutine(StopHapticCo());
	}
}

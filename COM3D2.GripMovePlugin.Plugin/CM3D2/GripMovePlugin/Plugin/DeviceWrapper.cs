using UnityEngine;
using Valve.VR;

namespace CM3D2.GripMovePlugin.Plugin;

public abstract class DeviceWrapper : MonoBehaviour
{
	public virtual Vector3 angularVelocity => Vector3.zero;

	public virtual bool connected => true;

	public virtual bool hasTracking => true;

	public virtual bool outOfRange => false;

	public virtual bool valid => true;

	public virtual Vector3 velocity => Vector3.zero;

	public abstract Vector2 GetAxis(EVRButtonId buttonId = (EVRButtonId)32);

	public abstract bool GetPress(ulong buttonMask);

	public abstract bool GetPress(EVRButtonId buttonId);

	public abstract bool GetPressDown(EVRButtonId buttonId);

	public abstract bool GetPressDown(ulong buttonMask);

	public abstract bool GetPressUp(EVRButtonId buttonId);

	public abstract bool GetPressUp(ulong buttonMask);

	public abstract bool GetTouch(EVRButtonId buttonId);

	public abstract bool GetTouch(ulong buttonMask);

	public abstract bool GetTouchDown(EVRButtonId buttonId);

	public abstract bool GetTouchDown(ulong buttonMask);

	public abstract bool GetTouchUp(ulong buttonMask);

	public abstract bool GetTouchUp(EVRButtonId buttonId);

	public abstract void TriggerHapticPulse(ushort durationMicroSec = 500,
		EVRButtonId buttonId = (EVRButtonId)32);
}

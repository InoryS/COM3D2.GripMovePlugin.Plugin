using UnityEngine;
#if !COM3D25_UNITY_2022
using Valve.VR;
#endif

namespace COM3D2.GripMovePlugin.Plugin;

/// <summary>
/// 手柄抽象层。两个平台的按钮抽象完全不同：
///   COM3D2   (Unity 5.6.4) : SteamVR 的 EVRButtonId + 原始 button mask，Oculus 侧另走 OVRInput
///   COM3D2.5 (Unity 2022)  : 游戏自带的 AVRControllerButtons.BTN / TOUCH，轴向走 UnityEngine.XR
/// </summary>
public abstract class DeviceWrapper : MonoBehaviour
{
	public virtual Vector3 angularVelocity => Vector3.zero;

	public virtual bool connected => true;

	public virtual bool hasTracking => true;

	public virtual bool outOfRange => false;

	public virtual bool valid => true;

	public virtual Vector3 velocity => Vector3.zero;

#if COM3D25_UNITY_2022
	public abstract Vector2 GetAxis();

	public abstract bool GetAxisClick();

	public abstract bool GetPress(AVRControllerButtons.BTN btn);

	public abstract bool GetPressDown(AVRControllerButtons.BTN btn);

	public abstract bool GetPressUp(AVRControllerButtons.BTN btn);

	public abstract bool GetTouch(AVRControllerButtons.TOUCH touch);

	public abstract bool GetTouchDown(AVRControllerButtons.TOUCH touch);

	public abstract bool GetTouchUp(AVRControllerButtons.TOUCH touch);

	// COM3D2.5 侧没有实现震动反馈（原 XR 版即为注释状态）
#else
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
#endif
}

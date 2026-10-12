using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin;

internal class IK
{
	private TBody body;

	private Quaternion defHipQ;

	private Quaternion defKneeQ;

	private float defLEN1;

	private float defLEN2;

	private Vector3 knee_old;

	private Vector3 vechand;

	public void Init(Transform hip, Transform knee, Transform ankle, TBody b)
	{
		body = b;
		defLEN1 = (hip.position - knee.position).magnitude;
		defLEN2 = (ankle.position - knee.position).magnitude;
		knee_old = knee.position;
		defHipQ = hip.localRotation;
		defKneeQ = knee.localRotation;
		vechand = Vector3.zero;
	}

	public void Porc(Transform hip, Transform knee, Transform ankle, Vector3 tgt,
		Vector3 vechand_offset)
	{
		knee_old = knee_old * 0.5f + knee.position * 0.5f;
		Vector3 normalized = (knee_old - tgt).normalized;
		knee_old = tgt + normalized * defLEN2;
		Vector3 normalized2 = (knee_old - hip.position).normalized;
		knee_old = hip.position + normalized2 * defLEN1;
		default(Quaternion).SetLookRotation(normalized2);
		hip.transform.rotation =
			Quaternion.FromToRotation(knee.transform.position - hip.transform.position,
				knee_old - hip.transform.position) * hip.transform.rotation;
		knee.transform.rotation =
			Quaternion.FromToRotation(ankle.transform.position - knee.transform.position,
				tgt - knee.transform.position) * knee.transform.rotation;
	}
}

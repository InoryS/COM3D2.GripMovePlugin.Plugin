using System;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	internal class IK
	{
		public void Init(Transform hip, Transform knee, Transform ankle, TBody b)
		{
			this.body = b;
			this.defLEN1 = (hip.position - knee.position).magnitude;
			this.defLEN2 = (ankle.position - knee.position).magnitude;
			this.knee_old = knee.position;
			this.defHipQ = hip.localRotation;
			this.defKneeQ = knee.localRotation;
			this.vechand = Vector3.zero;
		}

		public void Porc(Transform hip, Transform knee, Transform ankle, Vector3 tgt, Vector3 vechand_offset)
		{
			this.knee_old = this.knee_old * 0.5f + knee.position * 0.5f;
			Vector3 normalized = (this.knee_old - tgt).normalized;
			this.knee_old = tgt + normalized * this.defLEN2;
			Vector3 normalized2 = (this.knee_old - hip.position).normalized;
			this.knee_old = hip.position + normalized2 * this.defLEN1;
			default(Quaternion).SetLookRotation(normalized2);
			hip.transform.rotation = Quaternion.FromToRotation(knee.transform.position - hip.transform.position, this.knee_old - hip.transform.position) * hip.transform.rotation;
			knee.transform.rotation = Quaternion.FromToRotation(ankle.transform.position - knee.transform.position, tgt - knee.transform.position) * knee.transform.rotation;
		}

		private TBody body;

		private float defLEN1;

		private float defLEN2;

		private Vector3 knee_old;

		private Quaternion defHipQ;

		private Quaternion defKneeQ;

		private Vector3 vechand;
	}
}

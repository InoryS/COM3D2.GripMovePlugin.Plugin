using System;
using UnityEngine;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x0200002B RID: 43
	internal class IK
	{
		// Token: 0x06000164 RID: 356 RVA: 0x00009110 File Offset: 0x00007310
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

		// Token: 0x06000165 RID: 357 RVA: 0x00009194 File Offset: 0x00007394
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

		// Token: 0x04000101 RID: 257
		private TBody body;

		// Token: 0x04000102 RID: 258
		private float defLEN1;

		// Token: 0x04000103 RID: 259
		private float defLEN2;

		// Token: 0x04000104 RID: 260
		private Vector3 knee_old;

		// Token: 0x04000105 RID: 261
		private Quaternion defHipQ;

		// Token: 0x04000106 RID: 262
		private Quaternion defKneeQ;

		// Token: 0x04000107 RID: 263
		private Vector3 vechand;
	}
}

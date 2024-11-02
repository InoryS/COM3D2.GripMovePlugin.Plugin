using System;
using UnityEngine;

// Token: 0x02000002 RID: 2
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ProceduralPlane : MonoBehaviour
{
	// Token: 0x06000002 RID: 2 RVA: 0x0000232C File Offset: 0x0000052C
	public void AssignDefaultShader()
	{
		base.gameObject.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Unlit/Texture"))
		{
			color = Color.white
		};
	}

	// Token: 0x06000003 RID: 3 RVA: 0x00003DC4 File Offset: 0x00001FC4
	public void Rebuild()
	{
		this.modelMesh = new Mesh
		{
			name = "ProceduralPlaneMesh"
		};
		this.meshFilter = base.gameObject.GetComponent<MeshFilter>();
		this.meshFilter.mesh = this.modelMesh;
		if (this.xSegments < 1)
		{
			this.xSegments = 1;
		}
		if (this.ySegments < 1)
		{
			this.ySegments = 1;
		}
		this.numVertexColumns = this.xSegments + 1;
		this.numVertexRows = this.ySegments + 1;
		int num = this.numVertexColumns * this.numVertexRows;
		Vector3[] array = new Vector3[num];
		Vector2[] array2 = new Vector2[num];
		int[] array3 = new int[this.xSegments * this.ySegments * 2 * 3];
		float num2 = this.width / (float)this.xSegments;
		float num3 = this.height / (float)this.ySegments;
		float num4 = 1f / (float)this.xSegments;
		float num5 = 1f / (float)this.ySegments;
		float num6 = -this.width / 2f;
		float num7 = -this.width / 2f;
		float num8 = this.angleSpan * 0.017453292f;
		float num9 = (float)Screen.width / (float)Screen.height;
		for (int i = 0; i < this.numVertexRows; i++)
		{
			for (int j = 0; j < this.numVertexColumns; j++)
			{
				int num10 = 0;
				Vector3 vector = new Vector3((float)j * num2 + num6 + this.bottomLeftOffset.x * (float)(this.numVertexColumns - 1 - j) / (float)(this.numVertexColumns - 1) * (float)(this.numVertexRows - 1 - i) / (float)(this.numVertexRows - 1) + this.bottomRightOffset.x * (float)j / (float)(this.numVertexColumns - 1) * (float)(this.numVertexRows - 1 - i) / (float)(this.numVertexRows - 1) + this.topLeftOffset.x * (float)(this.numVertexColumns - 1 - j) / (float)(this.numVertexColumns - 1) * (float)i / (float)(this.numVertexRows - 1) + this.topRightOffset.x * (float)j / (float)(this.numVertexColumns - 1) * (float)i / (float)(this.numVertexRows - 1), (float)i * num3 + num7 + this.bottomLeftOffset.y * (float)(this.numVertexColumns - 1 - j) / (float)(this.numVertexColumns - 1) * (float)(this.numVertexRows - 1 - i) / (float)(this.numVertexRows - 1) + this.bottomRightOffset.y * (float)j / (float)(this.numVertexColumns - 1) * (float)(this.numVertexRows - 1 - i) / (float)(this.numVertexRows - 1) + this.topLeftOffset.y * (float)(this.numVertexColumns - 1 - j) / (float)(this.numVertexColumns - 1) * (float)i / (float)(this.numVertexRows - 1) + this.topRightOffset.y * (float)j / (float)(this.numVertexColumns - 1) * (float)i / (float)(this.numVertexRows - 1) - (this.height - 1f) / 2f + (float)num10, this.distance);
				float num11 = Mathf.Lerp(num9 * this.height * vector.x, Mathf.Cos(1.5707964f - vector.x * num8) * this.distance, Mathf.Clamp01(this.curviness));
				float num12 = Mathf.Sin(1.5707964f - vector.x * num8 * Mathf.Clamp01(this.curviness));
				int num13 = i * this.numVertexColumns + j;
				array[num13] = new Vector3(num11, vector.y, num12);
				if (this.curviness > 1f)
				{
					float num14 = this.curviness - 1f;
					array[num13] = Vector3.Lerp(array[num13], array[num13].normalized * this.distance, Mathf.Clamp01(num14));
				}
				array2[num13] = new Vector2((float)j * num4, (float)i * num5);
				if (i != 0 && j < this.numVertexColumns - 1)
				{
					int num15 = (i - 1) * this.xSegments * 6 + j * 6;
					array3[num15] = i * this.numVertexColumns + j;
					array3[num15 + 1] = i * this.numVertexColumns + j + 1;
					array3[num15 + 2] = (i - 1) * this.numVertexColumns + j;
					array3[num15 + 3] = (i - 1) * this.numVertexColumns + j;
					array3[num15 + 4] = i * this.numVertexColumns + j + 1;
					array3[num15 + 5] = (i - 1) * this.numVertexColumns + j + 1;
				}
			}
		}
		this.modelMesh.Clear();
		this.modelMesh.vertices = array;
		this.modelMesh.uv = array2;
		this.modelMesh.triangles = array3;
		this.modelMesh.RecalculateNormals();
		this.modelMesh.RecalculateBounds();
	}

	// Token: 0x06000004 RID: 4 RVA: 0x000042BC File Offset: 0x000024BC
	public float TransformX(float x)
	{
		float num = (float)Screen.width / (float)Screen.height;
		float num2 = this.angleSpan * 0.017453292f;
		float num3 = 1f;
		return Mathf.Lerp(num * this.height * x, Mathf.Cos(1.5707964f - x * (num2 / num3)) * this.distance, this.curviness);
	}

	// Token: 0x06000005 RID: 5 RVA: 0x00004314 File Offset: 0x00002514
	public float TransformZ(float x)
	{
		float num = (float)Screen.width / (float)Screen.height;
		float num2 = this.angleSpan * 0.017453292f;
		return Mathf.Sin(1.5707964f - x * num2 * this.curviness);
	}

	// Token: 0x04000001 RID: 1
	private const int DEFAULT_X_SEGMENTS = 10;

	// Token: 0x04000002 RID: 2
	private const int DEFAULT_Y_SEGMENTS = 10;

	// Token: 0x04000003 RID: 3
	private const int MIN_X_SEGMENTS = 1;

	// Token: 0x04000004 RID: 4
	private const int MIN_Y_SEGMENTS = 1;

	// Token: 0x04000005 RID: 5
	private const float DEFAULT_WIDTH = 1f;

	// Token: 0x04000006 RID: 6
	private const float DEFAULT_HEIGHT = 1f;

	// Token: 0x04000007 RID: 7
	public int xSegments = 10;

	// Token: 0x04000008 RID: 8
	public int ySegments = 10;

	// Token: 0x04000009 RID: 9
	public Vector2 topLeftOffset = Vector2.zero;

	// Token: 0x0400000A RID: 10
	public Vector2 topRightOffset = Vector2.zero;

	// Token: 0x0400000B RID: 11
	public Vector2 bottomLeftOffset = Vector2.zero;

	// Token: 0x0400000C RID: 12
	public Vector2 bottomRightOffset = Vector2.zero;

	// Token: 0x0400000D RID: 13
	public float distance = 1f;

	// Token: 0x0400000E RID: 14
	private Mesh modelMesh;

	// Token: 0x0400000F RID: 15
	private MeshFilter meshFilter;

	// Token: 0x04000010 RID: 16
	public float width = 1f;

	// Token: 0x04000011 RID: 17
	public float height = 1f;

	// Token: 0x04000012 RID: 18
	private int numVertexColumns;

	// Token: 0x04000013 RID: 19
	private int numVertexRows;

	// Token: 0x04000014 RID: 20
	public float angleSpan = 160f;

	// Token: 0x04000015 RID: 21
	public float curviness;
}

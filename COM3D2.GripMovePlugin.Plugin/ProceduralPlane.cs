using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ProceduralPlane : MonoBehaviour
{
	public void AssignDefaultShader()
	{
		base.gameObject.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Unlit/Texture"))
		{
			color = Color.white
		};
	}

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
				Vector3 vector;
				vector = new((float)j * num2 + num6 + this.bottomLeftOffset.x * (float)(this.numVertexColumns - 1 - j) / (float)(this.numVertexColumns - 1) * (float)(this.numVertexRows - 1 - i) / (float)(this.numVertexRows - 1) + this.bottomRightOffset.x * (float)j / (float)(this.numVertexColumns - 1) * (float)(this.numVertexRows - 1 - i) / (float)(this.numVertexRows - 1) + this.topLeftOffset.x * (float)(this.numVertexColumns - 1 - j) / (float)(this.numVertexColumns - 1) * (float)i / (float)(this.numVertexRows - 1) + this.topRightOffset.x * (float)j / (float)(this.numVertexColumns - 1) * (float)i / (float)(this.numVertexRows - 1), (float)i * num3 + num7 + this.bottomLeftOffset.y * (float)(this.numVertexColumns - 1 - j) / (float)(this.numVertexColumns - 1) * (float)(this.numVertexRows - 1 - i) / (float)(this.numVertexRows - 1) + this.bottomRightOffset.y * (float)j / (float)(this.numVertexColumns - 1) * (float)(this.numVertexRows - 1 - i) / (float)(this.numVertexRows - 1) + this.topLeftOffset.y * (float)(this.numVertexColumns - 1 - j) / (float)(this.numVertexColumns - 1) * (float)i / (float)(this.numVertexRows - 1) + this.topRightOffset.y * (float)j / (float)(this.numVertexColumns - 1) * (float)i / (float)(this.numVertexRows - 1) - (this.height - 1f) / 2f + (float)num10, this.distance);
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

	public float TransformX(float x)
	{
		float num = (float)Screen.width / (float)Screen.height;
		float num2 = this.angleSpan * 0.017453292f;
		float num3 = 1f;
		return Mathf.Lerp(num * this.height * x, Mathf.Cos(1.5707964f - x * (num2 / num3)) * this.distance, this.curviness);
	}

	public float TransformZ(float x)
	{
		float num = (float)Screen.width / (float)Screen.height;
		float num2 = this.angleSpan * 0.017453292f;
		return Mathf.Sin(1.5707964f - x * num2 * this.curviness);
	}

	private const int DEFAULT_X_SEGMENTS = 10;

	private const int DEFAULT_Y_SEGMENTS = 10;

	private const int MIN_X_SEGMENTS = 1;

	private const int MIN_Y_SEGMENTS = 1;

	private const float DEFAULT_WIDTH = 1f;

	private const float DEFAULT_HEIGHT = 1f;

	public int xSegments = 10;

	public int ySegments = 10;

	public Vector2 topLeftOffset = Vector2.zero;

	public Vector2 topRightOffset = Vector2.zero;

	public Vector2 bottomLeftOffset = Vector2.zero;

	public Vector2 bottomRightOffset = Vector2.zero;

	public float distance = 1f;

	private Mesh modelMesh;

	private MeshFilter meshFilter;

	public float width = 1f;

	public float height = 1f;

	private int numVertexColumns;

	private int numVertexRows;

	public float angleSpan = 160f;

	public float curviness;
}

using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[GenerateAuthoringComponent]
public struct WaterMeshComponent : IComponentData
{
	public Color  TopColor;
	public Color  BottomColor;
	public float2 Size;
	public int2   PointsRange;
	public float  Step;
	public int    PartId;
}
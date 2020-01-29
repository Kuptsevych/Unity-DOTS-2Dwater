using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct WaterPoint : IComponentData
{
	public float  Height;
	public float3 Position;
	public float  Velocity;
	public float  TargetHeight;
	public float  Acceleration;
	public float  LeftDelta;
	public float  RightDelta;
	public int    PartId;
}
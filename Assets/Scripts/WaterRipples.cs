using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct WaterRipplesComponent : IComponentData
{
	public float2 Power;
	public float  Freq;
	public float  Timer;
}
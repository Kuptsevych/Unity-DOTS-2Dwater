using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

public class WaterSystem : JobComponentSystem
{
	[BurstCompile]
	struct WaterPointJob : IJobForEach<WaterPoint>
	{
		public void Execute(ref WaterPoint waterPoint)
		{
			const float k = 0.025f;

			float force = k * (waterPoint.Height - waterPoint.TargetHeight) + waterPoint.Velocity * 0.04f;
			waterPoint.Acceleration = -force;

			waterPoint.Height   += waterPoint.Velocity;
			waterPoint.Velocity += waterPoint.Acceleration;
		}
	}

	protected override JobHandle OnUpdate(JobHandle inputDependencies)
	{
		var job = new WaterPointJob();

		return job.Schedule(this, inputDependencies);
	}
}
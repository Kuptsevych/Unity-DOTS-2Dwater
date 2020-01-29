using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

[UpdateAfter(typeof(WaterSystem))]
public class WaterWaveSpreadSystem : JobComponentSystem
{
	private EntityQuery _waterPointsQuery;

	protected override void OnCreate()
	{
		_waterPointsQuery = GetEntityQuery(typeof(WaterPoint));
	}

	[BurstCompile]
	struct WaterPointJob : IJobChunk
	{
		const float Spread = 0.003f;

		public ArchetypeChunkComponentType<WaterPoint> WaterPointType;

		public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
		{
			var entities = chunk.GetNativeArray(WaterPointType);

			for (var n = 0; n < 8; n++)
			{
				// step 1

				for (var i = 1; i < entities.Length; i++)
				{
					var entity     = entities[i];
					var prevEntity = entities[i - 1];

					if (entity.PartId != prevEntity.PartId) continue;

					entity.LeftDelta    =  Spread * (entities[i].Height - entities[i - 1].Height);
					prevEntity.Velocity += entity.LeftDelta;

					entities[i]     = entity;
					entities[i - 1] = prevEntity;
				}

				for (var i = entities.Length - 2; i >= 0; i--)
				{
					var entity     = entities[i];
					var nextEntity = entities[i + 1];

					if (entity.PartId != nextEntity.PartId) continue;

					entity.RightDelta   =  Spread * (entities[i].Height - entities[i + 1].Height);
					nextEntity.Velocity += entity.RightDelta;

					entities[i]     = entity;
					entities[i + 1] = nextEntity;
				}

				// step 2

				for (var i = 1; i < entities.Length; i++)
				{
					var entity     = entities[i];
					var prevEntity = entities[i - 1];

					if (entity.PartId != prevEntity.PartId) continue;

					prevEntity.Height += entity.LeftDelta;

					entities[i - 1] = prevEntity;
				}

				for (var i = entities.Length - 2; i >= 0; i--)
				{
					var entity     = entities[i];
					var nextEntity = entities[i + 1];

					if (entity.PartId != nextEntity.PartId) continue;

					nextEntity.Height += entity.RightDelta;

					entities[i + 1] = nextEntity;
				}
			}
		}
	}

	protected override JobHandle OnUpdate(JobHandle inputDependencies)
	{
		var waterPointType = GetArchetypeChunkComponentType<WaterPoint>();

		var job = new WaterPointJob {WaterPointType = waterPointType};

		return job.Schedule(_waterPointsQuery, inputDependencies);
	}
}
using Assets.Scripts.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Assets.Scripts.Systems
{
	[AlwaysSynchronizeSystem]
	[UpdateBefore(typeof(BorderSpawnSystem))]
	[UpdateInGroup(typeof(MatchLogicGroup))]
	public class LimitedSpawnSystem : JobComponentSystem
	{
		private EntityQuery _systemStateGroup;
		private float _delay;

		public void Init(float size, float speed)
		{
			_delay = size / speed;
		}

		protected override void OnCreate()
		{
			base.OnCreate();
			_systemStateGroup = GetEntityQuery(ComponentType.ReadOnly<SystemState>(), ComponentType.ReadOnly<Gravity>(), ComponentType.Exclude<Delay>());
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			if (_systemStateGroup.IsEmptyIgnoreFilter)
			{
				return default;
			}

			EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
			var systemEntity = _systemStateGroup.GetSingletonEntity();
			var delay = _delay;

			Entities
				.WithNone<Delay>()
				.WithNone<BallLink>()
				.WithAll<SpawnLimit>()
				.ForEach((Entity entity, ref SpawnCount spawnCount, in Spawner spawner, in Translation translation) =>
				{
					SpawnBall(ecb, spawner.Prefab, translation, spawner, entity, delay);
					spawnCount.Value++;
					ecb.AddComponent(systemEntity, new Delay { Value = delay });
				}).Run();

			ecb.Playback(EntityManager);
			ecb.Dispose();
			return default;
		}

		public static void SpawnBall(EntityCommandBuffer ecb, Entity prefab, Translation translation, Spawner spawner,
			Entity entity, float delay)
		{
			var instance = ecb.Instantiate(prefab);
			ecb.SetComponent(instance, new Translation { Value = translation.Value + spawner.Offset });
			ecb.AddComponent(instance, new Destination { Value = translation.Value });
			ecb.AddComponent(instance, new CellLink { Value = entity });
			ecb.AddComponent(entity, new BallLink { Value = instance });
			ecb.AddComponent(entity, new Delay { Value = delay });
		}
	}
}
using Assets.Scripts.Components;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Scripts.Systems
{
	[AlwaysSynchronizeSystem]
	[UpdateAfter(typeof(SwapSystem))]
	[UpdateAfter(typeof(FallSystem))]
	public class SetVelocitySystem : JobComponentSystem
	{
		private BeginInitializationEntityCommandBufferSystem _commandBufferSystem;

		protected override void OnCreate()
		{
			base.OnCreate();
			_commandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			var commandBuffer = _commandBufferSystem.CreateCommandBuffer().ToConcurrent();
			inputDeps = Entities
				.WithNone<Velocity>()
				.ForEach((Entity entity, int entityInQueryIndex, in Translation translation, in Destination destination, in Speed speed) =>
				{
					commandBuffer.AddComponent(entityInQueryIndex, entity, new Velocity { Value = speed.Value * math.normalizesafe(destination.Value - translation.Value) });
				}).Schedule(inputDeps);

			_commandBufferSystem.AddJobHandleForProducer(inputDeps);
			return inputDeps;
		}
	}
}
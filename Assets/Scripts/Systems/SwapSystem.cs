using Assets.Scripts.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Assets.Scripts.Systems
{
	[AlwaysSynchronizeSystem]
	[UpdateAfter(typeof(InputSystem))]
	public class SwapSystem : JobComponentSystem
	{
		private EntityQuery _selectedGroup;
		private EntityQuery _systemStateGroup;
		private Field _field;

		public void Init(Field field)
		{
			_field = field;
		}

		protected override void OnCreate()
		{
			base.OnCreate();
			_selectedGroup = GetEntityQuery(typeof(BallLink), ComponentType.ReadOnly<GridPosition>(), ComponentType.ReadOnly<Selected>());
			_systemStateGroup = GetEntityQuery(ComponentType.ReadOnly<SystemState>());
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			if (_selectedGroup.IsEmptyIgnoreFilter)
			{
				return default;
			}

			var positions = _selectedGroup.ToComponentDataArray<GridPosition>(Allocator.TempJob);
			var ballLinks = _selectedGroup.ToComponentDataArray<BallLink>(Allocator.TempJob);
			var entities = _selectedGroup.ToEntityArray(Allocator.TempJob);

			if (positions.Length > 1)
			{
				var fromPosition = positions[0].Value;
				var toPosition = positions[1].Value;

				if (math.abs(fromPosition.x - toPosition.x) + math.abs(fromPosition.y - toPosition.y) == 1)
				{
					var fromBall = ballLinks[0].Value;
					var toBall = ballLinks[1].Value;

					EntityManager.SetComponentData(fromBall, new CellLink { Value = entities[1] });
					EntityManager.SetComponentData(toBall, new CellLink { Value = entities[0] });

					EntityManager.AddComponentData(fromBall, new Destination { Value = _field.GetWorldPosition(toPosition) });
					EntityManager.AddComponentData(toBall, new Destination { Value = _field.GetWorldPosition(fromPosition) });

					EntityManager.SetComponentData(entities[0], new BallLink { Value = toBall });
					EntityManager.SetComponentData(entities[1], new BallLink { Value = fromBall });

					var swapEntity = EntityManager.CreateEntity();
					EntityManager.AddComponentData(swapEntity, new Swap { From = fromBall, To = toBall });

					var systemState = _systemStateGroup.GetSingletonEntity();
					EntityManager.RemoveComponent<Interactable>(systemState);
				}

				EntityManager.RemoveComponent<Selected>(entities);
			}

			entities.Dispose();
			ballLinks.Dispose();
			positions.Dispose();
			return default;
		}
	}
}
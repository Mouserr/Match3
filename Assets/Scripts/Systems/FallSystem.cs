using Assets.Scripts.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Scripts.Systems
{
	[UpdateAfter(typeof(SpawnSystem))]
	[UpdateInGroup(typeof(MatchLogicGroup))]
	public class FallSystem : ComponentSystem
	{
		private Field _field;
		private EntityQuery _emptyCellsGroup;
		private EntityQuery _systemStateGroup;

		public void Init(Field field)
		{
			_field = field;
		}

		protected override void OnCreate()
		{
			base.OnCreate();
			_emptyCellsGroup = GetEntityQuery(ComponentType.ReadOnly<GridPosition>(), ComponentType.Exclude<BallLink>());
			_systemStateGroup = GetEntityQuery(ComponentType.ReadOnly<SystemState>(), ComponentType.Exclude<Delay>());
		}

		protected override void OnUpdate()
		{
			if (_systemStateGroup.IsEmptyIgnoreFilter || _emptyCellsGroup.IsEmptyIgnoreFilter)
			{
				return;
			}

			var systemState = _systemStateGroup.GetSingletonEntity();
			EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
			var gravity = GetComponentDataFromEntity<Gravity>(true)[systemState];
			ComponentDataFromEntity<GridPosition> gridPositions = GetComponentDataFromEntity<GridPosition>(true);
			ComponentDataFromEntity<BallLink> ballLinks = GetComponentDataFromEntity<BallLink>(true);

			var startY = gravity.Value.y > 0 ? _field.Height - 1 : 0;
			NativeQueue<Entity> cellsToFallIn = new NativeQueue<Entity>(Allocator.TempJob);
			for (int x = 0; x < _field.Width; x++)
			{
				cellsToFallIn.Clear();
				for (var y = startY; y < _field.Height && y >= 0; y -= gravity.Value.y)
				{
					var cell = _field.GetCell(x, y);
					if (!ballLinks.Exists(cell))
					{
						cellsToFallIn.Enqueue(cell);
					}
					else if (cellsToFallIn.Count > 0)
					{
						ecb.RemoveComponent<BallLink>(cell);
						var ball = ballLinks[cell].Value;
						var cellToFall = cellsToFallIn.Dequeue();
						ecb.SetComponent(ball, new CellLink { Value = cellToFall });
						ecb.AddComponent(ball, new Destination { Value = _field.GetWorldPosition(gridPositions[cellToFall].Value) });
						ecb.AddComponent(cellToFall, new BallLink { Value = ball });

						ecb.RemoveComponent<Interactable>(systemState);
					}
				}
			}

			cellsToFallIn.Dispose();
			ecb.Playback(EntityManager);
			ecb.Dispose();
		}
	}
}
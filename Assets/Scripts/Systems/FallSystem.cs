﻿using Assets.Scripts.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Scripts.Systems
{
	[UpdateAfter(typeof(SpawnSystem))]
	public class FallSystem : ComponentSystem
	{
		private Field _field;
		private EntityQuery _stayingBallsGroup;
		private EntityQuery _systemStateGroup;

		public void Init(Field field)
		{
			_field = field;
		}

		protected override void OnCreate()
		{
			base.OnCreate();
			_stayingBallsGroup = GetEntityQuery(typeof(CellLink), ComponentType.ReadOnly<Translation>(), ComponentType.Exclude<Destination>());
			_systemStateGroup = GetEntityQuery(ComponentType.ReadOnly<SystemState>(), ComponentType.ReadOnly<Gravity>(), ComponentType.Exclude<Delay>());
		}

		protected override void OnUpdate()
		{
			if (_systemStateGroup.IsEmptyIgnoreFilter)
			{
				return;
			}

			var gravity = _systemStateGroup.ToComponentDataArray<Gravity>(Allocator.TempJob);
			var balls = _stayingBallsGroup.ToEntityArray(Allocator.TempJob);
			var ballCells = _stayingBallsGroup.ToComponentDataArray<CellLink>(Allocator.TempJob);
			var systemState = _systemStateGroup.GetSingletonEntity();
			for (var i = 0; i < ballCells.Length; i++)
			{
				var cell = ballCells[i];
				var gridPosition = EntityManager.GetComponentData<GridPosition>(cell.Value);
				if (TryGetFarthestFreeCellInDirection(gridPosition, gravity[0].Value, out var freeCell, out var newPosition))
				{
					EntityManager.RemoveComponent<BallLink>(cell.Value);
					EntityManager.SetComponentData(balls[i], new CellLink { Value = freeCell });
					EntityManager.AddComponentData(balls[i], new Destination { Value = _field.GetWorldPosition(newPosition) });
					EntityManager.AddComponentData(freeCell, new BallLink { Value = balls[i] });

					EntityManager.RemoveComponent<Interactable>(systemState);
				}
			}

			gravity.Dispose();
			balls.Dispose();
			ballCells.Dispose();
		}

		private bool TryGetFarthestFreeCellInDirection(GridPosition position, int2 direction, out Entity freeCell, out int2 foundPosition)
		{
			foundPosition = position.Value;
			freeCell = Entity.Null;
			for (var pos = position.Value + direction; math.all(pos < _field.Size & pos >= 0); pos += direction)
			{
				var cell = _field.GetCell(pos);
				if (EntityManager.HasComponent<BallLink>(cell))
				{
					break;
				}

				freeCell = cell;
				foundPosition = pos;
			}

			return !foundPosition.Equals(position.Value);
		}
	}
}
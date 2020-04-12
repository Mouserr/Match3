using Assets.Scripts.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts
{
	public class Field
	{
		private readonly float _cellSize;
		private readonly Vector3 _leftBottomPosition;
		private readonly Entity[] _cells;

		public int Width { get; }
		public int Height { get; }
		public int2 Size => new int2(Width, Height);

		public Field(int width, int height, float cellSize, Vector3 leftBottomPosition, EntityManager manager)
		{
			_cellSize = cellSize;
			_leftBottomPosition = leftBottomPosition;
			Width = width;
			Height = height;

			_cells = new Entity[width * height];
			FillWithCells(manager);
		}

		public Entity GetCell(int x, int y)
		{
			return _cells[GetIndex(x, y)];
		}

		public Entity GetCell(int2 gridPosiiton)
		{
			return _cells[GetIndex(gridPosiiton.x, gridPosiiton.y)];
		}

		public bool TryGetCell(Vector3 worldPosition, out Entity cell)
		{
			var gridPosition = GetGridPosition(worldPosition);

			if (math.all(gridPosition >= 0 & gridPosition < Size))
			{
				cell = GetCell(gridPosition);
				return true;
			}

			cell = Entity.Null;
			return false;
		}

		public float3 GetWorldPosition(int2 gridPosition)
		{
			return GetWorldPosition(gridPosition.x, gridPosition.y);
		}

		public float3 GetWorldPosition(int x, int y)
		{
			return new float3(_leftBottomPosition.x + x * _cellSize, _leftBottomPosition.y + y * _cellSize, 0);
		}

		private void FillWithCells(EntityManager entityManager)
		{
			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Height; y++)
				{
					var entity = entityManager.CreateEntity();
					entityManager.AddComponentData(entity, new Translation { Value = GetWorldPosition(x, y) });
					entityManager.AddComponentData(entity, new GridPosition { Value = new int2(x, y) });
					SetCell(x, y, entity);
					if (y == 0 || y == Height - 1)
					{
						entityManager.AddComponentData(entity, new Border { RequiredGravity = new int2(0, y == 0 ? 1 : -1) });
						entityManager.AddComponentData(entity, new Spawner { Offset = new float3(0, y == 0 ? -1 : 1, 0) });
						entityManager.AddComponent<SpawnCount>(entity);
					}
				}
			}
		}

		private int2 GetGridPosition(Vector3 worldPosition)
		{
			var localPosition = worldPosition - _leftBottomPosition;
			return new int2((int) math.round(localPosition.x / _cellSize), (int) math.round(localPosition.y / _cellSize));
		}

		private void SetCell(int x, int y, Entity cell)
		{
			_cells[GetIndex(x, y)] = cell;
		}

		private int GetIndex(int x, int y)
		{
			return y * Width + x;
		}
	}
}
using Assets.Scripts.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Assets.Scripts.Systems
{
	[AlwaysSynchronizeSystem]
	public class InputSystem : JobComponentSystem
	{
		private Vector2Int? _start;
		private Plane _boardPlane = new Plane(-Vector3.forward, Vector3.zero);
		private Camera _camera;
		private Field _field;

		public void Init(Camera camera, Field field)
		{
			_camera = camera;
			_field = field;
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			if (Input.GetMouseButtonUp(0))
			{
				var ray = _camera.ScreenPointToRay(Input.mousePosition);
				
				if (_boardPlane.Raycast(ray, out float distance))
				{
					var worldPosition = ray.GetPoint(distance);
					if (_field.TryGetCell(worldPosition, out var entity))
					{
						EntityManager.AddComponent<Selected>(entity);
					}
				}
			}

			return default;
		}
	}
}
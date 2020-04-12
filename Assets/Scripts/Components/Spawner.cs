using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Scripts.Components
{
	public struct Spawner : IComponentData
	{
		public Entity Prefab;
		public float3 Offset;
	}
}
using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Scripts.Components
{
	public struct GridPosition : IComponentData
	{
		public int2 Value;
	}
}

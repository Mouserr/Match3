using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Scripts.Components
{
	public struct Border : IComponentData
	{
		public int2 RequiredGravity;
	}
}
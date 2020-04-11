using Unity.Entities;

namespace Assets.Scripts.Components
{
	public struct Swap : IComponentData
	{
		public Entity From;
		public Entity To;
	}
}
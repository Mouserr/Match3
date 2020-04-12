using System;

namespace Assets.Scripts.Pools
{
	public interface ICompletionCheck
	{
		event Action OnComplete;
	}
}
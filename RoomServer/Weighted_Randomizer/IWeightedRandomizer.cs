using System.Collections;
using System.Collections.Generic;

namespace Weighted_Randomizer
{
	public interface IWeightedRandomizer<TKey> : ICollection<TKey>, IEnumerable<TKey>, IEnumerable
	{
		long TotalWeight { get; }

		int this[TKey key] { get; set; }

		TKey NextWithReplacement();

		TKey NextWithRemoval();

		void Add(TKey key, int weight);

		int GetWeight(TKey key);

		void SetWeight(TKey key, int weight);
	}
}

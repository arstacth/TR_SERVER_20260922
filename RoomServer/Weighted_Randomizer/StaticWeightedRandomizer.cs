using System;
using System.Collections;
using System.Collections.Generic;

namespace Weighted_Randomizer
{
	public class StaticWeightedRandomizer<TKey> : IWeightedRandomizer<TKey>, ICollection<TKey>, IEnumerable<TKey>, IEnumerable
	{
		private struct ProbabilityBox
		{
			public TKey Key { get; private set; }

			public TKey Alias { get; private set; }

			public long NumBallsInBox { get; private set; }

			public ProbabilityBox(TKey key, TKey alias, long numBallsInBox)
			{
				this = default(ProbabilityBox);
				Key = key;
				Alias = alias;
				NumBallsInBox = numBallsInBox;
			}
		}

		private struct KeyBallsPair
		{
			public TKey Key;

			public long NumBalls;
		}

		private readonly ThreadAwareRandom _random;

		private readonly Dictionary<TKey, int> _weights;

		private bool _listNeedsRebuilding;

		private readonly IList<ProbabilityBox> _probabilityBoxes;

		private long _heightPerBox;

		public int Count => _weights.Keys.Count;

		public bool IsReadOnly => false;

		public long TotalWeight { get; private set; }

		public int this[TKey key]
		{
			get
			{
				return GetWeight(key);
			}
			set
			{
				SetWeight(key, value);
			}
		}

		public StaticWeightedRandomizer()
			: this(new ThreadAwareRandom())
		{
		}

		public StaticWeightedRandomizer(int seed)
			: this(new ThreadAwareRandom(seed))
		{
		}

		private StaticWeightedRandomizer(ThreadAwareRandom random)
		{
			_random = random;
			_weights = new Dictionary<TKey, int>();
			_listNeedsRebuilding = true;
			TotalWeight = 0L;
			_probabilityBoxes = new List<ProbabilityBox>();
			_heightPerBox = 0L;
		}

		public void Clear()
		{
			_weights.Clear();
			_listNeedsRebuilding = true;
			TotalWeight = 0L;
		}

		public void CopyTo(TKey[] array, int startingIndex)
		{
			int num = startingIndex;
			using IEnumerator<TKey> enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				TKey val = (array[num] = enumerator.Current);
				num++;
			}
		}

		public bool Contains(TKey key)
		{
			return _weights.ContainsKey(key);
		}

		public void Add(TKey key)
		{
			Add(key, 1);
		}

		public void Add(TKey key, int weight)
		{
			if (weight < 0)
			{
				throw new ArgumentOutOfRangeException("weight", weight, "Cannot add a key with weight < 0!");
			}
			_weights.Add(key, weight);
			_listNeedsRebuilding = true;
			TotalWeight += weight;
		}

		public bool Remove(TKey key)
		{
			if (!_weights.TryGetValue(key, out var value))
			{
				return false;
			}
			TotalWeight -= value;
			_listNeedsRebuilding = true;
			_probabilityBoxes.Clear();
			return _weights.Remove(key);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<TKey> GetEnumerator()
		{
			return _weights.Keys.GetEnumerator();
		}

		public TKey NextWithReplacement()
		{
			VerifyHaveItemsToChooseFrom();
			if (_listNeedsRebuilding)
			{
				RebuildProbabilityList();
			}
			int index = _random.Next(_probabilityBoxes.Count);
			if (_random.NextLong(_heightPerBox) + 1 <= _probabilityBoxes[index].NumBallsInBox)
			{
				return _probabilityBoxes[index].Key;
			}
			return _probabilityBoxes[index].Alias;
		}

		private void RebuildProbabilityList()
		{
			long num = GreatestCommonDenominator(Count, TotalWeight);
			long weightMultiplier = Count / num;
			_heightPerBox = TotalWeight / num;
			Stack<KeyBallsPair> smallStack = new Stack<KeyBallsPair>();
			Stack<KeyBallsPair> largeStack = new Stack<KeyBallsPair>();
			DistributeKeysIntoStacks(weightMultiplier, largeStack, smallStack);
			CreateSplitProbabilityBoxes(largeStack, smallStack);
			AddRemainingProbabilityBoxes(smallStack);
			_listNeedsRebuilding = false;
		}

		private void DistributeKeysIntoStacks(long weightMultiplier, Stack<KeyBallsPair> largeStack, Stack<KeyBallsPair> smallStack)
		{
			_probabilityBoxes.Clear();
			foreach (TKey key in _weights.Keys)
			{
				long num = _weights[key] * weightMultiplier;
				if (num > _heightPerBox)
				{
					largeStack.Push(new KeyBallsPair
					{
						Key = key,
						NumBalls = num
					});
				}
				else
				{
					smallStack.Push(new KeyBallsPair
					{
						Key = key,
						NumBalls = num
					});
				}
			}
		}

		private void CreateSplitProbabilityBoxes(Stack<KeyBallsPair> largeStack, Stack<KeyBallsPair> smallStack)
		{
			while (largeStack.Count != 0)
			{
				KeyBallsPair item = largeStack.Pop();
				KeyBallsPair keyBallsPair = smallStack.Pop();
				_probabilityBoxes.Add(new ProbabilityBox(keyBallsPair.Key, item.Key, keyBallsPair.NumBalls));
				long num = _heightPerBox - keyBallsPair.NumBalls;
				item.NumBalls -= num;
				if (item.NumBalls > _heightPerBox)
				{
					largeStack.Push(item);
				}
				else
				{
					smallStack.Push(item);
				}
			}
		}

		private void AddRemainingProbabilityBoxes(Stack<KeyBallsPair> smallStack)
		{
			while (smallStack.Count != 0)
			{
				KeyBallsPair keyBallsPair = smallStack.Pop();
				_probabilityBoxes.Add(new ProbabilityBox(keyBallsPair.Key, keyBallsPair.Key, _heightPerBox));
			}
		}

		private static long GreatestCommonDenominator(long a, long b)
		{
			while (b > 0)
			{
				long num = a % b;
				if (num == 0L)
				{
					return b;
				}
				a = b;
				b = num;
			}
			return a;
		}

		public TKey NextWithRemoval()
		{
			VerifyHaveItemsToChooseFrom();
			TKey val = NextWithReplacement();
			Remove(val);
			return val;
		}

		private void VerifyHaveItemsToChooseFrom()
		{
			if (Count <= 0)
			{
				throw new InvalidOperationException("There are no items in the StaticWeightedRandomizer");
			}
			if (TotalWeight <= 0)
			{
				throw new InvalidOperationException("There are no items with positive weight in the StaticWeightedRandomizer");
			}
		}

		public int GetWeight(TKey key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key", "key cannot be null");
			}
			if (!_weights.TryGetValue(key, out var value))
			{
				TKey val = key;
				throw new KeyNotFoundException("Key not found in StaticWeightedRandomizer: " + val);
			}
			return value;
		}

		public void SetWeight(TKey key, int weight)
		{
			if (weight < 0)
			{
				throw new ArgumentOutOfRangeException("weight", weight, "Cannot add a weight with value < 0");
			}
			if (Contains(key))
			{
				TotalWeight += weight - _weights[key];
				_weights[key] = weight;
			}
			else
			{
				Add(key, weight);
			}
			_listNeedsRebuilding = true;
		}
	}
}

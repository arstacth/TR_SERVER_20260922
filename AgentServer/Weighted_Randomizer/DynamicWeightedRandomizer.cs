using System;
using System.Collections;
using System.Collections.Generic;

namespace Weighted_Randomizer
{
	public class DynamicWeightedRandomizer<TKey> : IWeightedRandomizer<TKey>, ICollection<TKey>, IEnumerable<TKey>, IEnumerable where TKey : IComparable<TKey>
	{
		private class Node
		{
			internal int level;

			internal Node left;

			internal Node right;

			internal TKey key;

			internal int weight;

			internal long subtreeWeight;

			internal Node()
			{
				level = 0;
				left = this;
				right = this;
				weight = 0;
				subtreeWeight = 0L;
			}

			internal Node(TKey key, int weight, Node sentinel)
			{
				level = 1;
				left = sentinel;
				right = sentinel;
				this.key = key;
				this.weight = weight;
				subtreeWeight = weight;
			}
		}

		private readonly Node _sentinel;

		private readonly ThreadAwareRandom _random;

		private Node _root;

		private Node _deleted;

		public int Count { get; private set; }

		public bool IsReadOnly => false;

		public long TotalWeight => _root.subtreeWeight;

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

		private int Height => GetNumLayers(_root);

		public DynamicWeightedRandomizer()
		{
			_root = (_sentinel = new Node());
			_deleted = null;
			_random = new ThreadAwareRandom();
		}

		public DynamicWeightedRandomizer(int seed)
		{
			_root = (_sentinel = new Node());
			_deleted = null;
			_random = new ThreadAwareRandom(seed);
		}

		private void RotateRight(ref Node node)
		{
			if (node.level == node.left.level)
			{
				long subtreeWeight = node.subtreeWeight;
				if (node != _sentinel)
				{
					node.subtreeWeight = subtreeWeight - node.left.subtreeWeight + node.left.right.subtreeWeight;
				}
				if (node.left != _sentinel)
				{
					node.left.subtreeWeight = subtreeWeight;
				}
				Node left = node.left;
				node.left = left.right;
				left.right = node;
				node = left;
			}
		}

		private void RotateLeft(ref Node node)
		{
			if (node.right.right.level == node.level)
			{
				long subtreeWeight = node.subtreeWeight;
				if (node != _sentinel)
				{
					node.subtreeWeight = subtreeWeight - node.right.subtreeWeight + node.right.left.subtreeWeight;
				}
				if (node.right != _sentinel)
				{
					node.right.subtreeWeight = subtreeWeight;
				}
				Node right = node.right;
				node.right = right.left;
				right.left = node;
				node = right;
				node.level++;
			}
		}

		private void InsertNode(ref Node node, TKey key, int weight)
		{
			if (weight < 0)
			{
				throw new ArgumentOutOfRangeException("weight", weight, "Cannot add a key with weight < 0!");
			}
			if (key == null)
			{
				throw new ArgumentNullException("key", "Cannot add a null key");
			}
			if (node == _sentinel)
			{
				node = new Node(key, weight, _sentinel);
				UpdateSubtreeWeightsForInsertion(node);
				Count++;
				return;
			}
			int num = key.CompareTo(node.key);
			if (num < 0)
			{
				InsertNode(ref node.left, key, weight);
			}
			else
			{
				if (num <= 0)
				{
					throw new ArgumentException("Key already exists in DynamicWeightedRandomizer: " + key.ToString());
				}
				InsertNode(ref node.right, key, weight);
			}
			RotateRight(ref node);
			RotateLeft(ref node);
		}

		private bool DeleteNode(ref Node node, TKey key)
		{
			if (node == _sentinel)
			{
				return _deleted != null;
			}
			int num = key.CompareTo(node.key);
			if (num < 0)
			{
				if (!DeleteNode(ref node.left, key))
				{
					return false;
				}
			}
			else
			{
				if (num == 0)
				{
					_deleted = node;
				}
				if (!DeleteNode(ref node.right, key))
				{
					return false;
				}
			}
			if (_deleted != null)
			{
				UpdateSubtreeWeightsForDeletion(_deleted, node);
				_deleted.key = node.key;
				_deleted.weight = node.weight;
				_deleted = null;
				node = node.right;
				Count--;
			}
			else if (node.left.level < node.level - 1 || node.right.level < node.level - 1)
			{
				node.level--;
				if (node.right.level > node.level)
				{
					node.right.level = node.level;
				}
				RotateRight(ref node);
				RotateRight(ref node.right);
				RotateRight(ref node.right.right);
				RotateLeft(ref node);
				RotateLeft(ref node.right);
			}
			return true;
		}

		private Node FindNode(Node node, TKey key)
		{
			while (node != _sentinel)
			{
				int num = key.CompareTo(node.key);
				if (num < 0)
				{
					node = node.left;
					continue;
				}
				if (num > 0)
				{
					node = node.right;
					continue;
				}
				return node;
			}
			return null;
		}

		private void UpdateSubtreeWeightsForInsertion(Node insertedNode)
		{
			for (Node node = _root; node != insertedNode; node = ((insertedNode.key.CompareTo(node.key) < 0) ? node.left : node.right))
			{
				node.subtreeWeight += insertedNode.weight;
			}
		}

		private void UpdateSubtreeWeightsForDeletion(Node deletedNode, Node leftmostRightDescendent)
		{
			Node node;
			for (node = _root; node != deletedNode; node = ((deletedNode.key.CompareTo(node.key) < 0) ? node.left : node.right))
			{
				node.subtreeWeight -= deletedNode.weight;
			}
			node.subtreeWeight -= deletedNode.weight;
			while (node != leftmostRightDescendent && node != _sentinel)
			{
				node = ((leftmostRightDescendent.key.CompareTo(node.key) < 0) ? node.left : node.right);
				node.subtreeWeight -= leftmostRightDescendent.weight;
			}
		}

		public void Clear()
		{
			_root = _sentinel;
			Count = 0;
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
			Node node = FindNode(_root, key);
			if (node != null)
			{
				return node != _sentinel;
			}
			return false;
		}

		public void Add(TKey key)
		{
			InsertNode(ref _root, key, 1);
		}

		public void Add(TKey key, int weight)
		{
			InsertNode(ref _root, key, weight);
		}

		public bool Remove(TKey key)
		{
			_deleted = null;
			return DeleteNode(ref _root, key);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<TKey> GetEnumerator()
		{
			return InorderTraversal(_root);
		}

		private IEnumerator<TKey> InorderTraversal(Node node)
		{
			Stack<Node> stack = new Stack<Node>();
			while (stack.Count != 0 || node != _sentinel)
			{
				if (node != _sentinel)
				{
					stack.Push(node);
					node = node.left;
				}
				else
				{
					node = stack.Pop();
					yield return node.key;
					node = node.right;
				}
			}
		}

		public TKey NextWithReplacement()
		{
			VerifyHaveItemsToChooseFrom();
			Node node = _root;
			long num = _random.NextLong(0L, TotalWeight) + 1;
			while (true)
			{
				if (node.left.subtreeWeight >= num)
				{
					node = node.left;
					continue;
				}
				num -= node.left.subtreeWeight;
				if (node.right.subtreeWeight < num)
				{
					break;
				}
				node = node.right;
			}
			return node.key;
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
				throw new InvalidOperationException("There are no items in the DynamicWeightedRandomizer");
			}
			if (TotalWeight <= 0)
			{
				throw new InvalidOperationException("There are no items with positive weight in the DynamicWeightedRandomizer");
			}
		}

		public int GetWeight(TKey key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key", "key cannot be null");
			}
			Node node = FindNode(_root, key);
			if (node == null)
			{
				TKey val = key;
				throw new KeyNotFoundException("Key not found in DynamicWeightedRandomizer: " + val);
			}
			return node.weight;
		}

		public void SetWeight(TKey key, int weight)
		{
			if (weight < 0)
			{
				throw new ArgumentOutOfRangeException("weight", weight, "Cannot add a weight with value < 0");
			}
			Node node = FindNode(_root, key);
			if (node == null)
			{
				Add(key, weight);
				return;
			}
			int num = (node.weight = weight - node.weight);
			UpdateSubtreeWeightsForInsertion(node);
			node.weight = weight;
			node.subtreeWeight += num;
		}

		private int GetNumLayers(Node node)
		{
			if (node == null || node == _sentinel)
			{
				return 0;
			}
			return Math.Max(GetNumLayers(node.left), GetNumLayers(node.right)) + 1;
		}

		private void Assert(bool condition)
		{
			if (!condition)
			{
				throw new ArgumentException("Test case failed");
			}
		}

		private void DebugCheckTree()
		{
			DebugCheckNode(_root);
			Assert(Count == 0 || (double)Height <= 2.0 * Math.Ceiling(Math.Log(Count, 2.0) + 1.0));
		}

		private void DebugCheckNode(Node node)
		{
			if (node != null && node != _sentinel)
			{
				Assert(node.left == null || node.left == _sentinel || node.left.key.CompareTo(node.key) < 0);
				Assert(node.right == null || node.right == _sentinel || node.right.key.CompareTo(node.key) > 0);
				Assert(node.left.subtreeWeight + node.right.subtreeWeight + node.weight == node.subtreeWeight);
				DebugCheckNode(node.left);
				DebugCheckNode(node.right);
			}
		}
	}
}

using System.Collections.Generic;

namespace NestedDictionaryLib
{
	public class NestedDictionary<TKey1, TValue> : Dictionary<TKey1, TValue>
	{
		public NestedDictionary()
		{
		}

		public NestedDictionary(int capacity)
			: base(capacity)
		{
		}

		public NestedDictionary(IEqualityComparer<TKey1> comparer)
			: base(comparer)
		{
		}

		public NestedDictionary(IDictionary<TKey1, TValue> dictionary)
			: base(dictionary)
		{
		}

		public NestedDictionary(int capacity, IEqualityComparer<TKey1> comparer)
			: base(capacity, comparer)
		{
		}

		public NestedDictionary(IDictionary<TKey1, TValue> dictionary, IEqualityComparer<TKey1> comparer)
			: base(dictionary, comparer)
		{
		}
	}
	public class NestedDictionary<TKey1, TKey2, TValue> : NestedDictionary<TKey1, NestedDictionary<TKey2, TValue>>
	{
		private int _capacity2;

		private IEqualityComparer<TKey2> _comparer2;

		public new NestedDictionary<TKey2, TValue> this[TKey1 key1]
		{
			get
			{
				if (!TryGetValue(key1, out var value))
				{
					return base[key1] = new NestedDictionary<TKey2, TValue>(_capacity2, _comparer2);
				}
				return value;
			}
			set
			{
				base[key1] = value;
			}
		}

		public IEqualityComparer<TKey2> Comparer2 => _comparer2;

		public NestedDictionary()
		{
		}

		public NestedDictionary(int capacity1, int capacity2)
			: base(capacity1)
		{
			_capacity2 = capacity2;
		}

		public NestedDictionary(IEqualityComparer<TKey1> comparer1, IEqualityComparer<TKey2> comparer2)
			: base(comparer1)
		{
			_comparer2 = comparer2;
		}

		public NestedDictionary(IDictionary<TKey1, NestedDictionary<TKey2, TValue>> dictionary)
			: base(dictionary)
		{
		}

		public NestedDictionary(int capacity1, int capacity2, IEqualityComparer<TKey1> comparer1, IEqualityComparer<TKey2> comparer2)
			: base(capacity1, comparer1)
		{
			_capacity2 = capacity2;
			_comparer2 = comparer2;
		}

		public NestedDictionary(IDictionary<TKey1, NestedDictionary<TKey2, TValue>> dictionary, IEqualityComparer<TKey1> comparer1, IEqualityComparer<TKey2> comparer2)
			: base(dictionary, comparer1)
		{
			_comparer2 = comparer2;
		}

		public void Add(TKey1 key1, TKey2 key2, TValue value)
		{
			this[key1].Add(key2, value);
		}

		public bool ContainsKey(TKey1 key1, TKey2 key2)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.ContainsKey(key2);
			}
			return false;
		}

		public bool ContainsValue(TValue value)
		{
			foreach (NestedDictionary<TKey2, TValue> value2 in base.Values)
			{
				if (value2.ContainsValue(value))
				{
					return true;
				}
			}
			return false;
		}

		public bool Remove(TKey1 key1, TKey2 key2)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.Remove(key2);
			}
			return false;
		}

		public bool TryGetValue(TKey1 key1, TKey2 key2, out TValue value)
		{
			value = default(TValue);
			if (TryGetValue(key1, out var value2))
			{
				return value2.TryGetValue(key2, out value);
			}
			return false;
		}
	}
	public class NestedDictionary<TKey1, TKey2, TKey3, TValue> : NestedDictionary<TKey1, NestedDictionary<TKey2, TKey3, TValue>>
	{
		private int _capacity2;

		private int _capacity3;

		private IEqualityComparer<TKey2> _comparer2;

		private IEqualityComparer<TKey3> _comparer3;

		public new NestedDictionary<TKey2, TKey3, TValue> this[TKey1 key1]
		{
			get
			{
				if (!TryGetValue(key1, out var value))
				{
					return base[key1] = new NestedDictionary<TKey2, TKey3, TValue>(_capacity2, _capacity3, _comparer2, _comparer3);
				}
				return value;
			}
			set
			{
				base[key1] = value;
			}
		}

		public IEqualityComparer<TKey2> Comparer2 => _comparer2;

		public IEqualityComparer<TKey3> Comparer3 => _comparer3;

		public NestedDictionary()
		{
		}

		public NestedDictionary(int capacity1, int capacity2, int capacity3)
			: base(capacity1)
		{
			_capacity2 = capacity2;
			_capacity3 = capacity3;
		}

		public NestedDictionary(IEqualityComparer<TKey1> comparer1, IEqualityComparer<TKey2> comparer2, IEqualityComparer<TKey3> comparer3)
			: base(comparer1)
		{
			_comparer2 = comparer2;
			_comparer3 = comparer3;
		}

		public NestedDictionary(IDictionary<TKey1, NestedDictionary<TKey2, TKey3, TValue>> dictionary)
			: base(dictionary)
		{
		}

		public NestedDictionary(int capacity1, int capacity2, int capacity3, IEqualityComparer<TKey1> comparer1, IEqualityComparer<TKey2> comparer2, IEqualityComparer<TKey3> comparer3)
			: base(capacity1, comparer1)
		{
			_capacity2 = capacity2;
			_capacity3 = capacity3;
			_comparer2 = comparer2;
			_comparer3 = comparer3;
		}

		public NestedDictionary(IDictionary<TKey1, NestedDictionary<TKey2, TKey3, TValue>> dictionary, IEqualityComparer<TKey1> comparer1, IEqualityComparer<TKey2> comparer2, IEqualityComparer<TKey3> comparer3)
			: base(dictionary, comparer1)
		{
			_comparer2 = comparer2;
			_comparer3 = comparer3;
		}

		public void Add(TKey1 key1, TKey2 key2, TKey3 key3, TValue value)
		{
			this[key1].Add(key2, key3, value);
		}

		public void Add(TKey1 key1, TKey2 key2, NestedDictionary<TKey3, TValue> dict)
		{
			this[key1].Add(key2, dict);
		}

		public bool ContainsKey(TKey1 key1, TKey2 key2, TKey3 key3)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.ContainsKey(key2, key3);
			}
			return false;
		}

		public bool ContainsKey(TKey1 key1, TKey2 key2)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.ContainsKey(key2);
			}
			return false;
		}

		public bool ContainsValue(TValue value)
		{
			foreach (NestedDictionary<TKey2, TKey3, TValue> value2 in base.Values)
			{
				if (value2.ContainsValue(value))
				{
					return true;
				}
			}
			return false;
		}

		public bool Remove(TKey1 key1, TKey2 key2, TKey3 key3)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.Remove(key2, key3);
			}
			return false;
		}

		public bool Remove(TKey1 key1, TKey2 key2)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.Remove(key2);
			}
			return false;
		}

		public bool TryGetValue(TKey1 key1, TKey2 key2, TKey3 key3, out TValue value)
		{
			value = default(TValue);
			if (TryGetValue(key1, out var value2))
			{
				return value2.TryGetValue(key2, key3, out value);
			}
			return false;
		}

		public bool TryGetValue(TKey1 key1, TKey2 key2, out NestedDictionary<TKey3, TValue> value)
		{
			value = null;
			if (TryGetValue(key1, out var value2))
			{
				return value2.TryGetValue(key2, out value);
			}
			return false;
		}
	}
	public class NestedDictionary<TKey1, TKey2, TKey3, TKey4, TValue> : NestedDictionary<TKey1, NestedDictionary<TKey2, TKey3, TKey4, TValue>>
	{
		private int _capacity2;

		private int _capacity3;

		private int _capacity4;

		private IEqualityComparer<TKey2> _comparer2;

		private IEqualityComparer<TKey3> _comparer3;

		private IEqualityComparer<TKey4> _comparer4;

		public new NestedDictionary<TKey2, TKey3, TKey4, TValue> this[TKey1 key1]
		{
			get
			{
				if (!TryGetValue(key1, out var value))
				{
					return base[key1] = new NestedDictionary<TKey2, TKey3, TKey4, TValue>(_capacity2, _capacity3, _capacity4, _comparer2, _comparer3, _comparer4);
				}
				return value;
			}
			set
			{
				base[key1] = value;
			}
		}

		public IEqualityComparer<TKey2> Comparer2 => _comparer2;

		public IEqualityComparer<TKey3> Comparer3 => _comparer3;

		public IEqualityComparer<TKey4> Comparer4 => _comparer4;

		public NestedDictionary()
		{
		}

		public NestedDictionary(int capacity1, int capacity2, int capacity3, int capacity4)
			: base(capacity1)
		{
			_capacity2 = capacity2;
			_capacity3 = capacity3;
			_capacity4 = capacity4;
		}

		public NestedDictionary(IEqualityComparer<TKey1> comparer1, IEqualityComparer<TKey2> comparer2, IEqualityComparer<TKey3> comparer3, IEqualityComparer<TKey4> comparer4)
			: base(comparer1)
		{
			_comparer2 = comparer2;
			_comparer3 = comparer3;
			_comparer4 = comparer4;
		}

		public NestedDictionary(IDictionary<TKey1, NestedDictionary<TKey2, TKey3, TKey4, TValue>> dictionary)
			: base(dictionary)
		{
		}

		public NestedDictionary(int capacity1, int capacity2, int capacity3, int capacity4, IEqualityComparer<TKey1> comparer1, IEqualityComparer<TKey2> comparer2, IEqualityComparer<TKey3> comparer3, IEqualityComparer<TKey4> comparer4)
			: base(capacity1, comparer1)
		{
			_capacity2 = capacity2;
			_capacity3 = capacity3;
			_capacity4 = capacity4;
			_comparer2 = comparer2;
			_comparer3 = comparer3;
			_comparer4 = comparer4;
		}

		public NestedDictionary(IDictionary<TKey1, NestedDictionary<TKey2, TKey3, TKey4, TValue>> dictionary, IEqualityComparer<TKey1> comparer1, IEqualityComparer<TKey2> comparer2, IEqualityComparer<TKey3> comparer3, IEqualityComparer<TKey4> comparer4)
			: base(dictionary, comparer1)
		{
			_comparer2 = comparer2;
			_comparer3 = comparer3;
			_comparer4 = comparer4;
		}

		public void Add(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TValue value)
		{
			this[key1].Add(key2, key3, key4, value);
		}

		public void Add(TKey1 key1, TKey2 key2, TKey3 key3, NestedDictionary<TKey4, TValue> dict)
		{
			this[key1].Add(key2, key3, dict);
		}

		public void Add(TKey1 key1, TKey2 key2, NestedDictionary<TKey3, TKey4, TValue> dict)
		{
			this[key1].Add(key2, dict);
		}

		public bool ContainsKey(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.ContainsKey(key2, key3, key4);
			}
			return false;
		}

		public bool ContainsKey(TKey1 key1, TKey2 key2, TKey3 key3)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.ContainsKey(key2, key3);
			}
			return false;
		}

		public bool ContainsKey(TKey1 key1, TKey2 key2)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.ContainsKey(key2);
			}
			return false;
		}

		public bool ContainsValue(TValue value)
		{
			foreach (NestedDictionary<TKey2, TKey3, TKey4, TValue> value2 in base.Values)
			{
				if (value2.ContainsValue(value))
				{
					return true;
				}
			}
			return false;
		}

		public bool Remove(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.Remove(key2, key3, key4);
			}
			return false;
		}

		public bool Remove(TKey1 key1, TKey2 key2, TKey3 key3)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.Remove(key2, key3);
			}
			return false;
		}

		public bool Remove(TKey1 key1, TKey2 key2)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.Remove(key2);
			}
			return false;
		}

		public bool TryGetValue(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, out TValue value)
		{
			value = default(TValue);
			if (TryGetValue(key1, out var value2))
			{
				return value2.TryGetValue(key2, key3, key4, out value);
			}
			return false;
		}

		public bool TryGetValue(TKey1 key1, TKey2 key2, TKey3 key3, out NestedDictionary<TKey4, TValue> value)
		{
			value = null;
			if (TryGetValue(key1, out var value2))
			{
				return value2.TryGetValue(key2, key3, out value);
			}
			return false;
		}

		public bool TryGetValue(TKey1 key1, TKey2 key2, out NestedDictionary<TKey3, TKey4, TValue> value)
		{
			value = null;
			if (TryGetValue(key1, out var value2))
			{
				return value2.TryGetValue(key2, out value);
			}
			return false;
		}
	}
	public class NestedDictionary<TKey1, TKey2, TKey3, TKey4, TKey5, TValue> : NestedDictionary<TKey1, NestedDictionary<TKey2, TKey3, TKey4, TKey5, TValue>>
	{
		private int _capacity2;

		private int _capacity3;

		private int _capacity4;

		private int _capacity5;

		private IEqualityComparer<TKey2> _comparer2;

		private IEqualityComparer<TKey3> _comparer3;

		private IEqualityComparer<TKey4> _comparer4;

		private IEqualityComparer<TKey5> _comparer5;

		public new NestedDictionary<TKey2, TKey3, TKey4, TKey5, TValue> this[TKey1 key1]
		{
			get
			{
				if (!TryGetValue(key1, out var value))
				{
					return base[key1] = new NestedDictionary<TKey2, TKey3, TKey4, TKey5, TValue>(_capacity2, _capacity3, _capacity4, _capacity5, _comparer2, _comparer3, _comparer4, _comparer5);
				}
				return value;
			}
			set
			{
				base[key1] = value;
			}
		}

		public IEqualityComparer<TKey2> Comparer2 => _comparer2;

		public IEqualityComparer<TKey3> Comparer3 => _comparer3;

		public IEqualityComparer<TKey4> Comparer4 => _comparer4;

		public IEqualityComparer<TKey5> Comparer5 => _comparer5;

		public NestedDictionary()
		{
		}

		public NestedDictionary(int capacity1, int capacity2, int capacity3, int capacity4, int capacity5)
			: base(capacity1)
		{
			_capacity2 = capacity2;
			_capacity3 = capacity3;
			_capacity4 = capacity4;
			_capacity5 = capacity5;
		}

		public NestedDictionary(IEqualityComparer<TKey1> comparer1, IEqualityComparer<TKey2> comparer2, IEqualityComparer<TKey3> comparer3, IEqualityComparer<TKey4> comparer4, IEqualityComparer<TKey5> comparer5)
			: base(comparer1)
		{
			_comparer2 = comparer2;
			_comparer3 = comparer3;
			_comparer4 = comparer4;
			_comparer5 = comparer5;
		}

		public NestedDictionary(IDictionary<TKey1, NestedDictionary<TKey2, TKey3, TKey4, TKey5, TValue>> dictionary)
			: base(dictionary)
		{
		}

		public NestedDictionary(int capacity1, int capacity2, int capacity3, int capacity4, int capacity5, IEqualityComparer<TKey1> comparer1, IEqualityComparer<TKey2> comparer2, IEqualityComparer<TKey3> comparer3, IEqualityComparer<TKey4> comparer4, IEqualityComparer<TKey5> comparer5)
			: base(capacity1, comparer1)
		{
			_capacity2 = capacity2;
			_capacity3 = capacity3;
			_capacity4 = capacity4;
			_capacity5 = capacity5;
			_comparer2 = comparer2;
			_comparer3 = comparer3;
			_comparer4 = comparer4;
			_comparer5 = comparer5;
		}

		public NestedDictionary(IDictionary<TKey1, NestedDictionary<TKey2, TKey3, TKey4, TKey5, TValue>> dictionary, IEqualityComparer<TKey1> comparer1, IEqualityComparer<TKey2> comparer2, IEqualityComparer<TKey3> comparer3, IEqualityComparer<TKey4> comparer4, IEqualityComparer<TKey5> comparer5)
			: base(dictionary, comparer1)
		{
			_comparer2 = comparer2;
			_comparer3 = comparer3;
			_comparer4 = comparer4;
			_comparer5 = comparer5;
		}

		public void Add(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TValue value)
		{
			this[key1].Add(key2, key3, key4, key5, value);
		}

		public void Add(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, NestedDictionary<TKey5, TValue> dict)
		{
			this[key1].Add(key2, key3, key4, dict);
		}

		public void Add(TKey1 key1, TKey2 key2, TKey3 key3, NestedDictionary<TKey4, TKey5, TValue> dict)
		{
			this[key1].Add(key2, key3, dict);
		}

		public void Add(TKey1 key1, TKey2 key2, NestedDictionary<TKey3, TKey4, TKey5, TValue> dict)
		{
			this[key1].Add(key2, dict);
		}

		public bool ContainsKey(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.ContainsKey(key2, key3, key4, key5);
			}
			return false;
		}

		public bool ContainsKey(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.ContainsKey(key2, key3, key4);
			}
			return false;
		}

		public bool ContainsKey(TKey1 key1, TKey2 key2, TKey3 key3)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.ContainsKey(key2, key3);
			}
			return false;
		}

		public bool ContainsKey(TKey1 key1, TKey2 key2)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.ContainsKey(key2);
			}
			return false;
		}

		public bool ContainsValue(TValue value)
		{
			foreach (NestedDictionary<TKey2, TKey3, TKey4, TKey5, TValue> value2 in base.Values)
			{
				if (value2.ContainsValue(value))
				{
					return true;
				}
			}
			return false;
		}

		public bool Remove(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.Remove(key2, key3, key4, key5);
			}
			return false;
		}

		public bool Remove(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.Remove(key2, key3, key4);
			}
			return false;
		}

		public bool Remove(TKey1 key1, TKey2 key2, TKey3 key3)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.Remove(key2, key3);
			}
			return false;
		}

		public bool Remove(TKey1 key1, TKey2 key2)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.Remove(key2);
			}
			return false;
		}

		public bool TryGetValue(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, out TValue value)
		{
			value = default(TValue);
			if (TryGetValue(key1, out var value2))
			{
				return value2.TryGetValue(key2, key3, key4, key5, out value);
			}
			return false;
		}

		public bool TryGetValue(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, out NestedDictionary<TKey5, TValue> value)
		{
			value = null;
			if (TryGetValue(key1, out var value2))
			{
				return value2.TryGetValue(key2, key3, key4, out value);
			}
			return false;
		}

		public bool TryGetValue(TKey1 key1, TKey2 key2, TKey3 key3, out NestedDictionary<TKey4, TKey5, TValue> value)
		{
			value = null;
			if (TryGetValue(key1, out var value2))
			{
				return value2.TryGetValue(key2, key3, out value);
			}
			return false;
		}

		public bool TryGetValue(TKey1 key1, TKey2 key2, out NestedDictionary<TKey3, TKey4, TKey5, TValue> value)
		{
			value = null;
			if (TryGetValue(key1, out var value2))
			{
				return value2.TryGetValue(key2, out value);
			}
			return false;
		}
	}
	public class NestedDictionary<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TValue> : NestedDictionary<TKey1, NestedDictionary<TKey2, TKey3, TKey4, TKey5, TKey6, TValue>>
	{
		private int _capacity2;

		private int _capacity3;

		private int _capacity4;

		private int _capacity5;

		private int _capacity6;

		private IEqualityComparer<TKey2> _comparer2;

		private IEqualityComparer<TKey3> _comparer3;

		private IEqualityComparer<TKey4> _comparer4;

		private IEqualityComparer<TKey5> _comparer5;

		private IEqualityComparer<TKey6> _comparer6;

		public new NestedDictionary<TKey2, TKey3, TKey4, TKey5, TKey6, TValue> this[TKey1 key1]
		{
			get
			{
				if (!TryGetValue(key1, out var value))
				{
					return base[key1] = new NestedDictionary<TKey2, TKey3, TKey4, TKey5, TKey6, TValue>(_capacity2, _capacity3, _capacity4, _capacity5, _capacity6, _comparer2, _comparer3, _comparer4, _comparer5, _comparer6);
				}
				return value;
			}
			set
			{
				base[key1] = value;
			}
		}

		public IEqualityComparer<TKey2> Comparer2 => _comparer2;

		public IEqualityComparer<TKey3> Comparer3 => _comparer3;

		public IEqualityComparer<TKey4> Comparer4 => _comparer4;

		public IEqualityComparer<TKey5> Comparer5 => _comparer5;

		public IEqualityComparer<TKey6> Comparer6 => _comparer6;

		public NestedDictionary()
		{
		}

		public NestedDictionary(int capacity1, int capacity2, int capacity3, int capacity4, int capacity5, int capacity6)
			: base(capacity1)
		{
			_capacity2 = capacity2;
			_capacity3 = capacity3;
			_capacity4 = capacity4;
			_capacity5 = capacity5;
			_capacity6 = capacity6;
		}

		public NestedDictionary(IEqualityComparer<TKey1> comparer1, IEqualityComparer<TKey2> comparer2, IEqualityComparer<TKey3> comparer3, IEqualityComparer<TKey4> comparer4, IEqualityComparer<TKey5> comparer5, IEqualityComparer<TKey6> comparer6)
			: base(comparer1)
		{
			_comparer2 = comparer2;
			_comparer3 = comparer3;
			_comparer4 = comparer4;
			_comparer5 = comparer5;
			_comparer6 = comparer6;
		}

		public NestedDictionary(IDictionary<TKey1, NestedDictionary<TKey2, TKey3, TKey4, TKey5, TKey6, TValue>> dictionary)
			: base(dictionary)
		{
		}

		public NestedDictionary(int capacity1, int capacity2, int capacity3, int capacity4, int capacity5, int capacity6, IEqualityComparer<TKey1> comparer1, IEqualityComparer<TKey2> comparer2, IEqualityComparer<TKey3> comparer3, IEqualityComparer<TKey4> comparer4, IEqualityComparer<TKey5> comparer5, IEqualityComparer<TKey6> comparer6)
			: base(capacity1, comparer1)
		{
			_capacity2 = capacity2;
			_capacity3 = capacity3;
			_capacity4 = capacity4;
			_capacity5 = capacity5;
			_capacity6 = capacity6;
			_comparer2 = comparer2;
			_comparer3 = comparer3;
			_comparer4 = comparer4;
			_comparer5 = comparer5;
			_comparer6 = comparer6;
		}

		public NestedDictionary(IDictionary<TKey1, NestedDictionary<TKey2, TKey3, TKey4, TKey5, TKey6, TValue>> dictionary, IEqualityComparer<TKey1> comparer1, IEqualityComparer<TKey2> comparer2, IEqualityComparer<TKey3> comparer3, IEqualityComparer<TKey4> comparer4, IEqualityComparer<TKey5> comparer5, IEqualityComparer<TKey6> comparer6)
			: base(dictionary, comparer1)
		{
			_comparer2 = comparer2;
			_comparer3 = comparer3;
			_comparer4 = comparer4;
			_comparer5 = comparer5;
			_comparer6 = comparer6;
		}

		public void Add(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6, TValue value)
		{
			this[key1].Add(key2, key3, key4, key5, key6, value);
		}

		public void Add(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, NestedDictionary<TKey6, TValue> dict)
		{
			this[key1].Add(key2, key3, key4, key5, dict);
		}

		public void Add(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, NestedDictionary<TKey5, TKey6, TValue> dict)
		{
			this[key1].Add(key2, key3, key4, dict);
		}

		public void Add(TKey1 key1, TKey2 key2, TKey3 key3, NestedDictionary<TKey4, TKey5, TKey6, TValue> dict)
		{
			this[key1].Add(key2, key3, dict);
		}

		public void Add(TKey1 key1, TKey2 key2, NestedDictionary<TKey3, TKey4, TKey5, TKey6, TValue> dict)
		{
			this[key1].Add(key2, dict);
		}

		public bool ContainsKey(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.ContainsKey(key2, key3, key4, key5, key6);
			}
			return false;
		}

		public bool ContainsKey(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.ContainsKey(key2, key3, key4, key5);
			}
			return false;
		}

		public bool ContainsKey(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.ContainsKey(key2, key3, key4);
			}
			return false;
		}

		public bool ContainsKey(TKey1 key1, TKey2 key2, TKey3 key3)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.ContainsKey(key2, key3);
			}
			return false;
		}

		public bool ContainsKey(TKey1 key1, TKey2 key2)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.ContainsKey(key2);
			}
			return false;
		}

		public bool ContainsValue(TValue value)
		{
			foreach (NestedDictionary<TKey2, TKey3, TKey4, TKey5, TKey6, TValue> value2 in base.Values)
			{
				if (value2.ContainsValue(value))
				{
					return true;
				}
			}
			return false;
		}

		public bool Remove(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.Remove(key2, key3, key4, key5, key6);
			}
			return false;
		}

		public bool Remove(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.Remove(key2, key3, key4, key5);
			}
			return false;
		}

		public bool Remove(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.Remove(key2, key3, key4);
			}
			return false;
		}

		public bool Remove(TKey1 key1, TKey2 key2, TKey3 key3)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.Remove(key2, key3);
			}
			return false;
		}

		public bool Remove(TKey1 key1, TKey2 key2)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.Remove(key2);
			}
			return false;
		}

		public bool TryGetValue(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6, out TValue value)
		{
			value = default(TValue);
			if (TryGetValue(key1, out var value2))
			{
				return value2.TryGetValue(key2, key3, key4, key5, key6, out value);
			}
			return false;
		}

		public bool TryGetValue(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, out NestedDictionary<TKey6, TValue> value)
		{
			value = null;
			if (TryGetValue(key1, out var value2))
			{
				return value2.TryGetValue(key2, key3, key4, key5, out value);
			}
			return false;
		}

		public bool TryGetValue(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, out NestedDictionary<TKey5, TKey6, TValue> value)
		{
			value = null;
			if (TryGetValue(key1, out var value2))
			{
				return value2.TryGetValue(key2, key3, key4, out value);
			}
			return false;
		}

		public bool TryGetValue(TKey1 key1, TKey2 key2, TKey3 key3, out NestedDictionary<TKey4, TKey5, TKey6, TValue> value)
		{
			value = null;
			if (TryGetValue(key1, out var value2))
			{
				return value2.TryGetValue(key2, key3, out value);
			}
			return false;
		}

		public bool TryGetValue(TKey1 key1, TKey2 key2, out NestedDictionary<TKey3, TKey4, TKey5, TKey6, TValue> value)
		{
			value = null;
			if (TryGetValue(key1, out var value2))
			{
				return value2.TryGetValue(key2, out value);
			}
			return false;
		}
	}
	public class NestedDictionary<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TValue> : NestedDictionary<TKey1, NestedDictionary<TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TValue>>
	{
		private int _capacity2;

		private int _capacity3;

		private int _capacity4;

		private int _capacity5;

		private int _capacity6;

		private int _capacity7;

		private IEqualityComparer<TKey2> _comparer2;

		private IEqualityComparer<TKey3> _comparer3;

		private IEqualityComparer<TKey4> _comparer4;

		private IEqualityComparer<TKey5> _comparer5;

		private IEqualityComparer<TKey6> _comparer6;

		private IEqualityComparer<TKey7> _comparer7;

		public new NestedDictionary<TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TValue> this[TKey1 key1]
		{
			get
			{
				if (!TryGetValue(key1, out var value))
				{
					return base[key1] = new NestedDictionary<TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TValue>(_capacity2, _capacity3, _capacity4, _capacity5, _capacity6, _capacity7, _comparer2, _comparer3, _comparer4, _comparer5, _comparer6, _comparer7);
				}
				return value;
			}
			set
			{
				base[key1] = value;
			}
		}

		public IEqualityComparer<TKey2> Comparer2 => _comparer2;

		public IEqualityComparer<TKey3> Comparer3 => _comparer3;

		public IEqualityComparer<TKey4> Comparer4 => _comparer4;

		public IEqualityComparer<TKey5> Comparer5 => _comparer5;

		public IEqualityComparer<TKey6> Comparer6 => _comparer6;

		public IEqualityComparer<TKey7> Comparer7 => _comparer7;

		public NestedDictionary()
		{
		}

		public NestedDictionary(int capacity1, int capacity2, int capacity3, int capacity4, int capacity5, int capacity6, int capacity7)
			: base(capacity1)
		{
			_capacity2 = capacity2;
			_capacity3 = capacity3;
			_capacity4 = capacity4;
			_capacity5 = capacity5;
			_capacity6 = capacity6;
			_capacity7 = capacity7;
		}

		public NestedDictionary(IEqualityComparer<TKey1> comparer1, IEqualityComparer<TKey2> comparer2, IEqualityComparer<TKey3> comparer3, IEqualityComparer<TKey4> comparer4, IEqualityComparer<TKey5> comparer5, IEqualityComparer<TKey6> comparer6, IEqualityComparer<TKey7> comparer7)
			: base(comparer1)
		{
			_comparer2 = comparer2;
			_comparer3 = comparer3;
			_comparer4 = comparer4;
			_comparer5 = comparer5;
			_comparer6 = comparer6;
			_comparer7 = comparer7;
		}

		public NestedDictionary(IDictionary<TKey1, NestedDictionary<TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TValue>> dictionary)
			: base(dictionary)
		{
		}

		public NestedDictionary(int capacity1, int capacity2, int capacity3, int capacity4, int capacity5, int capacity6, int capacity7, IEqualityComparer<TKey1> comparer1, IEqualityComparer<TKey2> comparer2, IEqualityComparer<TKey3> comparer3, IEqualityComparer<TKey4> comparer4, IEqualityComparer<TKey5> comparer5, IEqualityComparer<TKey6> comparer6, IEqualityComparer<TKey7> comparer7)
			: base(capacity1, comparer1)
		{
			_capacity2 = capacity2;
			_capacity3 = capacity3;
			_capacity4 = capacity4;
			_capacity5 = capacity5;
			_capacity6 = capacity6;
			_capacity7 = capacity7;
			_comparer2 = comparer2;
			_comparer3 = comparer3;
			_comparer4 = comparer4;
			_comparer5 = comparer5;
			_comparer6 = comparer6;
			_comparer7 = comparer7;
		}

		public NestedDictionary(IDictionary<TKey1, NestedDictionary<TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TValue>> dictionary, IEqualityComparer<TKey1> comparer1, IEqualityComparer<TKey2> comparer2, IEqualityComparer<TKey3> comparer3, IEqualityComparer<TKey4> comparer4, IEqualityComparer<TKey5> comparer5, IEqualityComparer<TKey6> comparer6, IEqualityComparer<TKey7> comparer7)
			: base(dictionary, comparer1)
		{
			_comparer2 = comparer2;
			_comparer3 = comparer3;
			_comparer4 = comparer4;
			_comparer5 = comparer5;
			_comparer6 = comparer6;
			_comparer7 = comparer7;
		}

		public void Add(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6, TKey7 key7, TValue value)
		{
			this[key1].Add(key2, key3, key4, key5, key6, key7, value);
		}

		public void Add(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6, NestedDictionary<TKey7, TValue> dict)
		{
			this[key1].Add(key2, key3, key4, key5, key6, dict);
		}

		public void Add(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, NestedDictionary<TKey6, TKey7, TValue> dict)
		{
			this[key1].Add(key2, key3, key4, key5, dict);
		}

		public void Add(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, NestedDictionary<TKey5, TKey6, TKey7, TValue> dict)
		{
			this[key1].Add(key2, key3, key4, dict);
		}

		public void Add(TKey1 key1, TKey2 key2, TKey3 key3, NestedDictionary<TKey4, TKey5, TKey6, TKey7, TValue> dict)
		{
			this[key1].Add(key2, key3, dict);
		}

		public void Add(TKey1 key1, TKey2 key2, NestedDictionary<TKey3, TKey4, TKey5, TKey6, TKey7, TValue> dict)
		{
			this[key1].Add(key2, dict);
		}

		public bool ContainsKey(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6, TKey7 key7)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.ContainsKey(key2, key3, key4, key5, key6, key7);
			}
			return false;
		}

		public bool ContainsKey(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.ContainsKey(key2, key3, key4, key5, key6);
			}
			return false;
		}

		public bool ContainsKey(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.ContainsKey(key2, key3, key4, key5);
			}
			return false;
		}

		public bool ContainsKey(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.ContainsKey(key2, key3, key4);
			}
			return false;
		}

		public bool ContainsKey(TKey1 key1, TKey2 key2, TKey3 key3)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.ContainsKey(key2, key3);
			}
			return false;
		}

		public bool ContainsKey(TKey1 key1, TKey2 key2)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.ContainsKey(key2);
			}
			return false;
		}

		public bool ContainsValue(TValue value)
		{
			foreach (NestedDictionary<TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TValue> value2 in base.Values)
			{
				if (value2.ContainsValue(value))
				{
					return true;
				}
			}
			return false;
		}

		public bool Remove(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6, TKey7 key7)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.Remove(key2, key3, key4, key5, key6, key7);
			}
			return false;
		}

		public bool Remove(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.Remove(key2, key3, key4, key5, key6);
			}
			return false;
		}

		public bool Remove(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.Remove(key2, key3, key4, key5);
			}
			return false;
		}

		public bool Remove(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.Remove(key2, key3, key4);
			}
			return false;
		}

		public bool Remove(TKey1 key1, TKey2 key2, TKey3 key3)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.Remove(key2, key3);
			}
			return false;
		}

		public bool Remove(TKey1 key1, TKey2 key2)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.Remove(key2);
			}
			return false;
		}

		public bool TryGetValue(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6, TKey7 key7, out TValue value)
		{
			value = default(TValue);
			if (TryGetValue(key1, out var value2))
			{
				return value2.TryGetValue(key2, key3, key4, key5, key6, key7, out value);
			}
			return false;
		}

		public bool TryGetValue(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6, out NestedDictionary<TKey7, TValue> value)
		{
			value = null;
			if (TryGetValue(key1, out var value2))
			{
				return value2.TryGetValue(key2, key3, key4, key5, key6, out value);
			}
			return false;
		}

		public bool TryGetValue(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, out NestedDictionary<TKey6, TKey7, TValue> value)
		{
			value = null;
			if (TryGetValue(key1, out var value2))
			{
				return value2.TryGetValue(key2, key3, key4, key5, out value);
			}
			return false;
		}

		public bool TryGetValue(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, out NestedDictionary<TKey5, TKey6, TKey7, TValue> value)
		{
			value = null;
			if (TryGetValue(key1, out var value2))
			{
				return value2.TryGetValue(key2, key3, key4, out value);
			}
			return false;
		}

		public bool TryGetValue(TKey1 key1, TKey2 key2, TKey3 key3, out NestedDictionary<TKey4, TKey5, TKey6, TKey7, TValue> value)
		{
			value = null;
			if (TryGetValue(key1, out var value2))
			{
				return value2.TryGetValue(key2, key3, out value);
			}
			return false;
		}

		public bool TryGetValue(TKey1 key1, TKey2 key2, out NestedDictionary<TKey3, TKey4, TKey5, TKey6, TKey7, TValue> value)
		{
			value = null;
			if (TryGetValue(key1, out var value2))
			{
				return value2.TryGetValue(key2, out value);
			}
			return false;
		}
	}
	public class NestedDictionary<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TKey8, TValue> : NestedDictionary<TKey1, NestedDictionary<TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TKey8, TValue>>
	{
		private int _capacity2;

		private int _capacity3;

		private int _capacity4;

		private int _capacity5;

		private int _capacity6;

		private int _capacity7;

		private int _capacity8;

		private IEqualityComparer<TKey2> _comparer2;

		private IEqualityComparer<TKey3> _comparer3;

		private IEqualityComparer<TKey4> _comparer4;

		private IEqualityComparer<TKey5> _comparer5;

		private IEqualityComparer<TKey6> _comparer6;

		private IEqualityComparer<TKey7> _comparer7;

		private IEqualityComparer<TKey8> _comparer8;

		public new NestedDictionary<TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TKey8, TValue> this[TKey1 key1]
		{
			get
			{
				if (!TryGetValue(key1, out var value))
				{
					return base[key1] = new NestedDictionary<TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TKey8, TValue>(_capacity2, _capacity3, _capacity4, _capacity5, _capacity6, _capacity7, _capacity8, _comparer2, _comparer3, _comparer4, _comparer5, _comparer6, _comparer7, _comparer8);
				}
				return value;
			}
			set
			{
				base[key1] = value;
			}
		}

		public IEqualityComparer<TKey2> Comparer2 => _comparer2;

		public IEqualityComparer<TKey3> Comparer3 => _comparer3;

		public IEqualityComparer<TKey4> Comparer4 => _comparer4;

		public IEqualityComparer<TKey5> Comparer5 => _comparer5;

		public IEqualityComparer<TKey6> Comparer6 => _comparer6;

		public IEqualityComparer<TKey7> Comparer7 => _comparer7;

		public IEqualityComparer<TKey8> Comparer8 => _comparer8;

		public NestedDictionary()
		{
		}

		public NestedDictionary(int capacity1, int capacity2, int capacity3, int capacity4, int capacity5, int capacity6, int capacity7, int capacity8)
			: base(capacity1)
		{
			_capacity2 = capacity2;
			_capacity3 = capacity3;
			_capacity4 = capacity4;
			_capacity5 = capacity5;
			_capacity6 = capacity6;
			_capacity7 = capacity7;
			_capacity8 = capacity8;
		}

		public NestedDictionary(IEqualityComparer<TKey1> comparer1, IEqualityComparer<TKey2> comparer2, IEqualityComparer<TKey3> comparer3, IEqualityComparer<TKey4> comparer4, IEqualityComparer<TKey5> comparer5, IEqualityComparer<TKey6> comparer6, IEqualityComparer<TKey7> comparer7, IEqualityComparer<TKey8> comparer8)
			: base(comparer1)
		{
			_comparer2 = comparer2;
			_comparer3 = comparer3;
			_comparer4 = comparer4;
			_comparer5 = comparer5;
			_comparer6 = comparer6;
			_comparer7 = comparer7;
			_comparer8 = comparer8;
		}

		public NestedDictionary(IDictionary<TKey1, NestedDictionary<TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TKey8, TValue>> dictionary)
			: base(dictionary)
		{
		}

		public NestedDictionary(int capacity1, int capacity2, int capacity3, int capacity4, int capacity5, int capacity6, int capacity7, int capacity8, IEqualityComparer<TKey1> comparer1, IEqualityComparer<TKey2> comparer2, IEqualityComparer<TKey3> comparer3, IEqualityComparer<TKey4> comparer4, IEqualityComparer<TKey5> comparer5, IEqualityComparer<TKey6> comparer6, IEqualityComparer<TKey7> comparer7, IEqualityComparer<TKey8> comparer8)
			: base(capacity1, comparer1)
		{
			_capacity2 = capacity2;
			_capacity3 = capacity3;
			_capacity4 = capacity4;
			_capacity5 = capacity5;
			_capacity6 = capacity6;
			_capacity7 = capacity7;
			_capacity8 = capacity8;
			_comparer2 = comparer2;
			_comparer3 = comparer3;
			_comparer4 = comparer4;
			_comparer5 = comparer5;
			_comparer6 = comparer6;
			_comparer7 = comparer7;
			_comparer8 = comparer8;
		}

		public NestedDictionary(IDictionary<TKey1, NestedDictionary<TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TKey8, TValue>> dictionary, IEqualityComparer<TKey1> comparer1, IEqualityComparer<TKey2> comparer2, IEqualityComparer<TKey3> comparer3, IEqualityComparer<TKey4> comparer4, IEqualityComparer<TKey5> comparer5, IEqualityComparer<TKey6> comparer6, IEqualityComparer<TKey7> comparer7, IEqualityComparer<TKey8> comparer8)
			: base(dictionary, comparer1)
		{
			_comparer2 = comparer2;
			_comparer3 = comparer3;
			_comparer4 = comparer4;
			_comparer5 = comparer5;
			_comparer6 = comparer6;
			_comparer7 = comparer7;
			_comparer8 = comparer8;
		}

		public void Add(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6, TKey7 key7, TKey8 key8, TValue value)
		{
			this[key1].Add(key2, key3, key4, key5, key6, key7, key8, value);
		}

		public void Add(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6, TKey7 key7, NestedDictionary<TKey8, TValue> dict)
		{
			this[key1].Add(key2, key3, key4, key5, key6, key7, dict);
		}

		public void Add(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6, NestedDictionary<TKey7, TKey8, TValue> dict)
		{
			this[key1].Add(key2, key3, key4, key5, key6, dict);
		}

		public void Add(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, NestedDictionary<TKey6, TKey7, TKey8, TValue> dict)
		{
			this[key1].Add(key2, key3, key4, key5, dict);
		}

		public void Add(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, NestedDictionary<TKey5, TKey6, TKey7, TKey8, TValue> dict)
		{
			this[key1].Add(key2, key3, key4, dict);
		}

		public void Add(TKey1 key1, TKey2 key2, TKey3 key3, NestedDictionary<TKey4, TKey5, TKey6, TKey7, TKey8, TValue> dict)
		{
			this[key1].Add(key2, key3, dict);
		}

		public void Add(TKey1 key1, TKey2 key2, NestedDictionary<TKey3, TKey4, TKey5, TKey6, TKey7, TKey8, TValue> dict)
		{
			this[key1].Add(key2, dict);
		}

		public bool ContainsKey(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6, TKey7 key7, TKey8 key8)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.ContainsKey(key2, key3, key4, key5, key6, key7, key8);
			}
			return false;
		}

		public bool ContainsKey(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6, TKey7 key7)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.ContainsKey(key2, key3, key4, key5, key6, key7);
			}
			return false;
		}

		public bool ContainsKey(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.ContainsKey(key2, key3, key4, key5, key6);
			}
			return false;
		}

		public bool ContainsKey(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.ContainsKey(key2, key3, key4, key5);
			}
			return false;
		}

		public bool ContainsKey(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.ContainsKey(key2, key3, key4);
			}
			return false;
		}

		public bool ContainsKey(TKey1 key1, TKey2 key2, TKey3 key3)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.ContainsKey(key2, key3);
			}
			return false;
		}

		public bool ContainsKey(TKey1 key1, TKey2 key2)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.ContainsKey(key2);
			}
			return false;
		}

		public bool ContainsValue(TValue value)
		{
			foreach (NestedDictionary<TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TKey8, TValue> value2 in base.Values)
			{
				if (value2.ContainsValue(value))
				{
					return true;
				}
			}
			return false;
		}

		public bool Remove(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6, TKey7 key7, TKey8 key8)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.Remove(key2, key3, key4, key5, key6, key7, key8);
			}
			return false;
		}

		public bool Remove(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6, TKey7 key7)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.Remove(key2, key3, key4, key5, key6, key7);
			}
			return false;
		}

		public bool Remove(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.Remove(key2, key3, key4, key5, key6);
			}
			return false;
		}

		public bool Remove(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.Remove(key2, key3, key4, key5);
			}
			return false;
		}

		public bool Remove(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.Remove(key2, key3, key4);
			}
			return false;
		}

		public bool Remove(TKey1 key1, TKey2 key2, TKey3 key3)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.Remove(key2, key3);
			}
			return false;
		}

		public bool Remove(TKey1 key1, TKey2 key2)
		{
			if (TryGetValue(key1, out var value))
			{
				return value.Remove(key2);
			}
			return false;
		}

		public bool TryGetValue(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6, TKey7 key7, TKey8 key8, out TValue value)
		{
			value = default(TValue);
			if (TryGetValue(key1, out var value2))
			{
				return value2.TryGetValue(key2, key3, key4, key5, key6, key7, key8, out value);
			}
			return false;
		}

		public bool TryGetValue(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6, TKey7 key7, out NestedDictionary<TKey8, TValue> value)
		{
			value = null;
			if (TryGetValue(key1, out var value2))
			{
				return value2.TryGetValue(key2, key3, key4, key5, key6, key7, out value);
			}
			return false;
		}

		public bool TryGetValue(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6, out NestedDictionary<TKey7, TKey8, TValue> value)
		{
			value = null;
			if (TryGetValue(key1, out var value2))
			{
				return value2.TryGetValue(key2, key3, key4, key5, key6, out value);
			}
			return false;
		}

		public bool TryGetValue(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, out NestedDictionary<TKey6, TKey7, TKey8, TValue> value)
		{
			value = null;
			if (TryGetValue(key1, out var value2))
			{
				return value2.TryGetValue(key2, key3, key4, key5, out value);
			}
			return false;
		}

		public bool TryGetValue(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, out NestedDictionary<TKey5, TKey6, TKey7, TKey8, TValue> value)
		{
			value = null;
			if (TryGetValue(key1, out var value2))
			{
				return value2.TryGetValue(key2, key3, key4, out value);
			}
			return false;
		}

		public bool TryGetValue(TKey1 key1, TKey2 key2, TKey3 key3, out NestedDictionary<TKey4, TKey5, TKey6, TKey7, TKey8, TValue> value)
		{
			value = null;
			if (TryGetValue(key1, out var value2))
			{
				return value2.TryGetValue(key2, key3, out value);
			}
			return false;
		}

		public bool TryGetValue(TKey1 key1, TKey2 key2, out NestedDictionary<TKey3, TKey4, TKey5, TKey6, TKey7, TKey8, TValue> value)
		{
			value = null;
			if (TryGetValue(key1, out var value2))
			{
				return value2.TryGetValue(key2, out value);
			}
			return false;
		}
	}
}

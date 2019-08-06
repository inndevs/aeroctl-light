using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AeroCtl
{
	/// <summary>
	/// Represents a hardware fan curve (or steps, rather).
	/// </summary>
	public abstract class FanCurve : IList<FanPoint>, IReadOnlyList<FanPoint>
	{
		public abstract FanPoint this[int index] { get; set; }

		public abstract int Count { get; }

		bool ICollection<FanPoint>.IsReadOnly => true;

		public bool Contains(FanPoint item)
		{
			return ((IEnumerable<FanPoint>)this).Contains(item);
		}

		public void CopyTo(FanPoint[] array, int arrayIndex)
		{
			for (int i = 0; i < this.Count; ++i)
			{
				array[arrayIndex + i] = this[i];
			}
		}

		public IEnumerator<FanPoint> GetEnumerator()
		{
			for (int i = 0; i < this.Count; ++i)
			{
				yield return this[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public int IndexOf(FanPoint item)
		{
			for (int i = 0; i < this.Count; ++i)
			{
				if (Equals(this[i], item))
					return i;
			}
			return -1;
		}

		void ICollection<FanPoint>.Add(FanPoint item) => throw new NotSupportedException();
		void ICollection<FanPoint>.Clear() => throw new NotSupportedException();
		void IList<FanPoint>.Insert(int index, FanPoint item) => throw new NotSupportedException();
		bool ICollection<FanPoint>.Remove(FanPoint item)=> throw new NotSupportedException();
		void IList<FanPoint>.RemoveAt(int index) => throw new NotSupportedException();
	}
}
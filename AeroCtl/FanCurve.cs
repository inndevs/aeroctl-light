using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AeroCtl
{
	/// <summary>
	/// Represents a hardware fan curve (or steps, rather).
	/// </summary>
	public class FanCurve : IList<FanPoint>, IReadOnlyList<FanPoint>
	{
		private readonly FanController controller;

		public FanPoint this[int index]
		{
			get => this.controller.GetFanPoint(index);
			set => this.controller.SetFanPoint(index, value);
		}

		public int Count => FanController.FanPointCount;

		public bool IsReadOnly => true;

		public FanCurve(FanController controller)
		{
			this.controller = controller;
		}

		void ICollection<FanPoint>.Add(FanPoint item)
		{
			throw new NotSupportedException();
		}

		void ICollection<FanPoint>.Clear()
		{
			throw new NotSupportedException();
		}

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

		void IList<FanPoint>.Insert(int index, FanPoint item)
		{
			throw new NotSupportedException();
		}

		bool ICollection<FanPoint>.Remove(FanPoint item)
		{
			throw new NotSupportedException();
		}

		void IList<FanPoint>.RemoveAt(int index)
		{
			throw new NotSupportedException();
		}
	}
}
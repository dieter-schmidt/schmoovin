using System.Collections;
using UnityEngine;

namespace NeoFPS
{
	public abstract class Waitable : IEnumerator
	{
		public bool isComplete
		{
			get { return CheckComplete (); }
		}

		public bool MoveNext() { return !CheckComplete (); }

		protected abstract bool CheckComplete ();

		#region IGNORE THESE
		public object Current { get { return null; } }
		public void Reset() { Debug.Log ("Waitable.Reset() got called. What was the situation?"); }
		#endregion
	}

	public abstract class Waitable<ResultType> :  IEnumerator
	{
		public bool isComplete
		{
			get { return CheckComplete (); }
		}

		public ResultType result
		{
			get { return GetResult (); }
		}

		public bool MoveNext() { return !CheckComplete (); }

		protected abstract bool CheckComplete ();
		protected abstract ResultType GetResult ();

		#region IGNORE THESE
		public object Current { get { return null; } }
		public void Reset() { Debug.Log ("Waitable.Reset() got called. What was the situation?"); }
		#endregion
	}
}
using UnityEngine;
using System.Collections;
using Reaktion;
using OscJack;
using System.Linq;

public class ReaktionOSCBang : MonoBehaviour
{
	public string address = "/reaktion";

	Reaktor[] cachedReaktors;
	int activeReaktorCount;
	
	static int CompareReaktor (Reaktor a, Reaktor b)
	{
		return a.name.CompareTo(b.name);
	}

	void FindAndCacheReaktors ()
	{
		// Cache validity check.
		if (cachedReaktors != null &&
			activeReaktorCount == Reaktor.ActiveInstanceCount)
		{
			bool validity = true;
			foreach (var r in cachedReaktors) validity &= (r!= null);
			// No update if the cache is valid.
			if (validity) return;
		}

		// Update the cache.
		cachedReaktors = FindObjectsOfType<Reaktor> ();
		System.Array.Sort (cachedReaktors, CompareReaktor);
		activeReaktorCount = Reaktor.ActiveInstanceCount;
	}

	void Start ()
	{
		this.FindAndCacheReaktors ();

		string rnames = "OSC endpoints:\n";

		foreach (var r in this.cachedReaktors)
			rnames += r.name + "\n";

		Debug.Log (rnames);
	}

	const float reaktorCheckTimeout = 2;
	float reaktorCheckTime = 0f;

	void Update ()
	{
		if ((this.reaktorCheckTime += Time.deltaTime) > reaktorCheckTimeout)
		{
			this.reaktorCheckTime = 0f;
			this.FindAndCacheReaktors ();
		}

		// turn off all - including banged - reaktors
		foreach (var r in this.cachedReaktors)
		{
			r.Override = 0;
		}

		System.Object[] data = OscMaster.GetData(address);
		if (data == null) return;

		// target reaktor
		string reaktorName = (string)data [0];

		// value -- TODO remove ? - not needed for bang (hhaha)
		float value = 0f;
		if (!float.TryParse (data [1].ToString (), out value))
		{
			int i = 0;
			if (int.TryParse (data [1].ToString (), out i))
				value = i;
		}


		// find target
		Reaktor target = this.cachedReaktors.FirstOrDefault( f => f.name == reaktorName );

		if (target != null)
		{
			// bang it
			target.Bang = true;
		}
	}
}

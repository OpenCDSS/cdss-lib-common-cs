using System;
using System.Collections.Generic;

// TSEnsemble - a collection for time series, to be represented as an ensemble.

/* NoticeStart

CDSS Common Java Library
CDSS Common Java Library is a part of Colorado's Decision Support Systems (CDSS)
Copyright (C) 1994-2019 Colorado Department of Natural Resources

CDSS Common Java Library is free software:  you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    CDSS Common Java Library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with CDSS Common Java Library.  If not, see <https://www.gnu.org/licenses/>.

NoticeEnd */

namespace RTi.TS
{

	/// <summary>
	/// A collection for time series, to be represented as an ensemble.  At this time, it
	/// is expected that each time series has been created or read using code that
	/// understands ensembles.  There are not currently hard constraints for ensembles but
	/// it is expected that they have similar time series characteristics like period of record,
	/// data type, and interval.  More constraints may be added over time.
	/// </summary>
	public class TSEnsemble : ICloneable
	{

	/// <summary>
	/// Ensemble of time series data, guaranteed to exist but may be empty.
	/// </summary>
	private IList<TS> __tslist = new List<TS>(); // Use Vector to be thread-safe

	/// <summary>
	/// Identifier for the ensemble.
	/// </summary>
	private string __id = "";

	/// <summary>
	/// Name for the ensemble, a descriptive phrase.
	/// </summary>
	private string __name = "";

	/// <summary>
	/// TODO SAM 2010-09-21 Evaluate whether generic "Attributable" interface should be implemented instead.
	/// Properties for the time series beyond the built-in properties.  For example, location
	/// information like county and state can be set as a property.
	/// </summary>
	private LinkedHashMap<string, object> __property_HashMap = null;

	/// <summary>
	/// Create a new ensemble.  An empty list of time series will be used.
	/// </summary>
	public TSEnsemble()
	{
	}

	/// <summary>
	/// Create a new ensemble, given a list of time series. </summary>
	/// <param name="id"> ensemble ID </param>
	/// <param name="name"> ensemble name </param>
	public TSEnsemble(string id, string name)
	{
		setEnsembleID(id);
		setEnsembleName(name);
	}

	/// <summary>
	/// Create a new ensemble, given a list of time series. </summary>
	/// <param name="id"> ensemble ID </param>
	/// <param name="name"> ensemble name </param>
	/// <param name="tslist"> List of time series </param>
	public TSEnsemble(string id, string name, IList<TS> tslist)
	{
		setEnsembleID(id);
		setEnsembleName(name);
		if (tslist == null)
		{
			tslist = new List<TS>();
		}
		__tslist = tslist;
	}

	/// <summary>
	/// Add a time series to the ensemble. </summary>
	/// <param name="ts"> time series to add to the ensemble. </param>
	public virtual void add(TS ts)
	{
		__tslist.Add(ts);
	}

	/// <summary>
	/// Clone the object.  The Object base class clone() method is called and then the
	/// TSEnsemble objects are cloned.  The result is a complete deep copy, including a copy of all the time series.
	/// </summary>
	public virtual object clone()
	{
		try
		{
			// Clone the base class...
			TSEnsemble ensemble = (TSEnsemble)base.clone();
			// Now clone mutable objects...
			int size = size();
			// Need a new list...
			ensemble.__tslist = new List<TS>(size);
			TS ts;
			for (int i = 0; i < size; i++)
			{
				ts = get(i);
				if (ts == null)
				{
					ensemble.add(null);
				}
				else
				{
					ensemble.add((TS)ts.clone());
				}
			}
			return ensemble;
		}
		catch (CloneNotSupportedException)
		{
			// Should not happen because everything is cloneable.
			throw new InternalError();
		}
	}

	/// <summary>
	/// Get a time series from the ensemble. </summary>
	/// <param name="pos"> Position (0+) in the ensemble for the requested time series. </param>
	/// <returns> The time series from the ensemble. </returns>
	public virtual TS get(int pos)
	{
		return __tslist[pos];
	}

	/// <summary>
	/// Return the ensemble identifier. </summary>
	/// <returns> The ensemble identifier. </returns>
	public virtual string getEnsembleID()
	{
		return __id;
	}

	/// <summary>
	/// Return the ensemble name. </summary>
	/// <returns> The ensemble name. </returns>
	public virtual string getEnsembleName()
	{
		return __name;
	}

	/// <summary>
	/// Get the hashtable of properties, for example to allow display. </summary>
	/// <returns> the hashtable of properties, for example to allow display, may be null. </returns>
	public virtual Dictionary<string, object> getProperties()
	{
		if (__property_HashMap == null)
		{
			__property_HashMap = new LinkedHashMap<string, object>(); // Initialize to non-null for further use
		}
		return __property_HashMap;
	}

	/// <summary>
	/// Get a time series ensemble property's contents (case-specific).
	/// This will return built-in properties as well as dynamic properties.  Built-in properties include:
	/// <ul>
	/// <li> FirstSequenceID - sequence ID of first time series (no additional sorting is performed)
	/// <li> LastSequenceID - sequence ID of last time series (no additional sorting is performed)
	/// </ul> </summary>
	/// <param name="propertyName"> name of property being retrieved. </param>
	/// <returns> property object corresponding to the property name. </returns>
	public virtual object getProperty(string propertyName)
	{
		// Built in properties first
		if (propertyName.Equals("EnsembleID", StringComparison.OrdinalIgnoreCase))
		{
			return getEnsembleID();
		}
		else if (propertyName.Equals("EnsembleName", StringComparison.OrdinalIgnoreCase))
		{
			return getEnsembleName();
		}
		else if (propertyName.Equals("FirstSequenceID", StringComparison.OrdinalIgnoreCase))
		{
			// Return the first time series sequence ID
			IList<TS> tslist = getTimeSeriesList(false);
			if (tslist.Count > 0)
			{
				TS ts = tslist[0];
				if (ts != null)
				{
					return ts.getSequenceID();
				}
			}
		}
		else if (propertyName.Equals("LastSequenceID", StringComparison.OrdinalIgnoreCase))
		{
			// Return the last time series sequence ID
			IList<TS> tslist = getTimeSeriesList(false);
			if (tslist.Count > 0)
			{
				TS ts = tslist[tslist.Count - 1];
				if (ts != null)
				{
					return ts.getSequenceID();
				}
			}
		}
		// Then dynamic properties
		if (__property_HashMap == null)
		{
			return null;
		}
		return __property_HashMap.get(propertyName);
	}

	/// <summary>
	/// Return the time series list. </summary>
	/// <param name="copyList"> if true, the list is copied (but the time series contents remain the same).
	/// Use this when the list object is going to be modified. </param>
	public virtual IList<TS> getTimeSeriesList(bool copyList)
	{
		if (!copyList)
		{
			return __tslist;
		}
		else
		{
			IList<TS> tslist = new List<TS>(__tslist.Count);
			int size = __tslist.Count;
			for (int i = 0; i < size; i++)
			{
				tslist.Add(__tslist[i]);
			}
			return tslist;
		}
	}

	/// <summary>
	/// Remove the time series object from the ensemble. </summary>
	/// <param name="ts"> Object (time series) to remove. </param>
	/// <returns> true if the object was found and removed, false if not in the list. </returns>
	public virtual bool remove(object ts)
	{
		return __tslist.Remove(ts);
	}

	/// <summary>
	/// Set the ensemble identifier. </summary>
	/// <param name="id"> The ensemble identifier. </param>
	public virtual void setEnsembleID(string id)
	{
		if (string.ReferenceEquals(id, null))
		{
			id = "";
		}
		__id = id;
		// Also set the property to allow for generic property request
		setProperty("EnsembleID",id);
	}

	/// <summary>
	/// Set the ensemble name. </summary>
	/// <param name="name"> The ensemble name. </param>
	public virtual void setEnsembleName(string name)
	{
		if (string.ReferenceEquals(name, null))
		{
			name = "";
		}
		__name = name;
		// Also set the property to allow for generic property request
		setProperty("EnsembleName",name);
	}

	/// <summary>
	/// Set a time series ensemble property's contents (case-specific). </summary>
	/// <param name="propertyName"> name of property being set. </param>
	/// <param name="property"> property object corresponding to the property name. </param>
	public virtual void setProperty(string propertyName, object property)
	{
		if (__property_HashMap == null)
		{
			__property_HashMap = new LinkedHashMap<string, object>();
		}
		// Do not allow EnsembleID to be set because it is fundamental to the identification of the ensemble and should be immutable
		if (propertyName.Equals("EnsembleID"))
		{
			return;
		}
		else if (propertyName.Equals("EnsembleName"))
		{
			// Do not call setEnsembleName() because it calls this method and would have infinite recursion
			this.__name = "" + property;
		}
		// Remainder are built-in properties that should not be set
		else if (propertyName.Equals("FirstSequenceID"))
		{
			return;
		}
		else if (propertyName.Equals("LastSequenceID"))
		{
			return;
		}

		__property_HashMap.put(propertyName, property);
	}

	/// <summary>
	/// Set the time series in the ensemble.  If the list is too small, null time series will be added. </summary>
	/// <param name="index"> Index (0+) at which to set the ensemble. </param>
	/// <param name="ts"> Time series to set. </param>
	public virtual void set(int index, TS ts)
	{
		int size = size();
		if (index >= size)
		{
			for (int i = size; i <= index; i++)
			{
				__tslist[index] = null;
			}
		}
		// Set the time series...
		__tslist[index] = ts;
	}

	/// <summary>
	/// Get the number of time series in the ensemble. </summary>
	/// <returns> The number of time series in the ensemble. </returns>
	public virtual int size()
	{
		return __tslist.Count;
	}

	/// <summary>
	/// Return the list of time series in the ensemble as an array.
	/// </summary>
	public virtual TS [] toArray()
	{
		int size = size();
		TS[] array = new TS[size];
		for (int i = 0; i < size; i++)
		{
			array[i] = __tslist[i];
		}
		return array;
	}

	}

}
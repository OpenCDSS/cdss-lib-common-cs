using System.Collections.Generic;

// InterpolatedLookup - class to perform lookup from table, with interpolation

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

namespace riverside.ts.util
{

	/// 
	/// <summary>
	/// @author rla
	/// 
	/// Provides a simple means of interpolation on a set of key-value pairs. 
	/// 
	/// This class provides an easy-to-use wrapper for the riverside.ts.util.Table 
	/// class.
	/// 
	/// The interpolation mode is Linear by default. 
	/// </summary>
	public class InterpolatedLookup
	{

		private Table _table;
		private Table.InterpolationMode _interpolationMode;

		public enum InterpolationMode
		{
			LINEAR,
			LOGARITHMIC
		}

		public InterpolatedLookup()
		{
			_table = new Table();
			_interpolationMode = Table.InterpolationMode.LINEAR;
		}


		public virtual bool isEmpty()
		{
			return (_table == null || _table.getNRows() == 0);
		}

		public virtual void setInterpolationMode(InterpolationMode interpolationMode)
		{
			switch (interpolationMode)
			{
				case riverside.ts.util.InterpolatedLookup.InterpolationMode.LINEAR:
					_interpolationMode = Table.InterpolationMode.LINEAR;
					break;
				case riverside.ts.util.InterpolatedLookup.InterpolationMode.LOGARITHMIC:
					_interpolationMode = Table.InterpolationMode.LOGARITHMIC;
					break;
			}
		}

		/// 
		/// <param name="list"> You will normally create a HashMap containing the key value
		///             pairs to be interpolated. You will need to call this method 
		///             before calling lookupValue() or lookupKey(). </param>
		public virtual void setBasis(Dictionary<double, double> list)
		{
			_table = new Table(list.Count);
			int i = 0;
			for (IEnumerator<double> iter = list.Keys.GetEnumerator(); iter.MoveNext();)
			{
				double key = iter.Current;
				double value = list[key];
				_table.populate(i, Table.GETCOLUMN_1, key);
				_table.populate(i, Table.GETCOLUMN_2, value);
				i++;
			}
			_table.sort(Table.GETCOLUMN_1);
		}

		/// 
		/// <param name="key"> the key of the key-value relation. </param>
		/// <returns> the interpolated value of the key-value relation. </returns>
		public virtual double lookupValue(double key)
		{
			return _table.lookup(key, Table.GETCOLUMN_1, true, _interpolationMode);
		}

		/// 
		/// <param name="value"> the value of the key-value relation. </param>
		/// <returns> the interpolated key of the key-value relation. </returns>
		public virtual double lookupKey(double value)
		{
			return _table.lookup(value, Table.GETCOLUMN_2, true, _interpolationMode);
		}

	}



}
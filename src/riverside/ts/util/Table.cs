using System;
using System.Collections.Generic;
using System.Text;

// Table - class to hold tabular data and perform lookups

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

/// 
/// <summary>
/// Created on April 18, 2007, 9:52 AM
/// 
/// </summary>
namespace riverside.ts.util
{

	/// <summary>
	/// A 2-column lookup table implementation taken from the National Weater Service Streamflow Forecast System
	/// ResJ operation C/C++ code. Modifications include
	/// making it a bit more useful and less hazardous to use.
	/// @todo evaluate whether this thing should live...
	/// @author iws
	/// </summary>
	public class Table
	{

		private double[] column1;
		private double[] column2;
		private string id;
		private double missingValue = -999;
		private bool modified = false;
		public const int GETCOLUMN_1 = 0;
		public const int GETCOLUMN_2 = 1;

		public enum InterpolationMode
		{

			LINEAR,
			LOGARITHMIC
		}

		public virtual bool isModified()
		{
			return modified;
		}

		public virtual void setModified(bool modified)
		{
			this.modified = modified;
		}

		public virtual void setMissingValue(double missing)
		{
			this.missingValue = missing;
		}

		public virtual double getMissingValue()
		{
			return missingValue;
		}

		public virtual double[] getColumn(int col)
		{
			double[] ac = col == 0 ? column1 : column2;
			return ac == null ? null : (double[]) ac.Clone();
		}

		/// <summary>
		/// Allocate the memory for the 2-column table and initialize to the missing value, -999.0.
		/// The table is marked as modified. </summary>
		/// <param name="size"> number of rows. </param>
		public virtual void allocateDataSpace(int size)
		{
			column1 = new double[size];
			column2 = new double[size];
			Arrays.fill(column1, missingValue);
			Arrays.fill(column2, missingValue);
			modified = true;
		}

		public virtual void freeDataSpace()
		{
			column1 = column2 = null;
		}

		public virtual string getID()
		{
			return id;
		}

		/// <summary>
		/// Return the value for a table cell. </summary>
		/// <param name="row"> row index (0+). </param>
		/// <param name="col"> column index (0 or 1). </param>
		/// <returns> value at the given cell address. </returns>
		/// <exception cref="ArrayIndexOutOfBoundsException"> if an invalid row or column index is specified. </exception>
		public virtual double get(int row, int col)
		{
			if ((col != 0) && (col != 1))
			{
				throw new System.IndexOutOfRangeException("Column " + col + " requested for Table - must be 0 or 1.");
			}
			if ((row < 0) || (row > getNRows() - 1))
			{
				throw new System.IndexOutOfRangeException("Row " + row + " requested for Table - must be in range 0 - " + (getNRows() - 1) + ".");
			}
			if (col == 0)
			{
				return column1[row];
			}
			else
			{
				return column2[row];
			}
		}

		public virtual int getNRows()
		{
			return column1 == null ? 0 : column1.Length;
		}

		private double min(double[] d)
		{
			double min = 0;
			if (d != null)
			{
				min = double.MaxValue;
				for (int i = 0; i < d.Length; i++)
				{
					double d2 = d[i];
					if (d2 < min)
					{
						min = d2;
					}
				}
			}
			return min;
		}

		private double max(double[] d)
		{
			double max = 0;
			if (d != null)
			{
				max = -double.MaxValue;
				for (int i = 0; i < d.Length; i++)
				{
					double d2 = d[i];
					if (d2 > max)
					{
						max = d2;
					}
				}
			}
			return max;
		}

		public virtual void set(int row, double c1, double c2)
		{
			column1[row] = c1;
			column2[row] = c2;
			modified = true;
		}

		public virtual double getMin(int num)
		{
			if (num == GETCOLUMN_1)
			{
				return min(column1);
			}
			else if (num == GETCOLUMN_2)
			{
				return min(column2);
			}
			throw new System.ArgumentException("" + num);
		}

		public virtual double getMax(int num)
		{
			if (num == GETCOLUMN_1)
			{
				return max(column1);
			}
			else if (num == GETCOLUMN_2)
			{
				return max(column2);
			}
			throw new System.ArgumentException("" + num);
		}

		public virtual double lookup(int row, int col)
		{
			if (col == GETCOLUMN_1)
			{
				return column1[row];
			}
			else if (col == GETCOLUMN_2)
			{
				return column2[row];
			}
			throw new System.ArgumentException("" + col);
		}

		public virtual double lookup(double val, int col, bool allowBounds)
		{
			return lookup(val, col, allowBounds, InterpolationMode.LINEAR);
		}

		public virtual double lookup(double val, int col, bool allowBounds, InterpolationMode mode)
		{
			double[] lookupColumn, valueColumn;
			if (col == 0)
			{
				lookupColumn = column2;
				valueColumn = column1;
			}
			else
			{
				lookupColumn = column1;
				valueColumn = column2;
			}
			if (val < lookupColumn[0])
			{
				if (allowBounds)
				{
					return valueColumn[0];
				}
				return missingValue;
			}
			int end = lookupColumn.Length - 1;
			if (val > lookupColumn[end])
			{
				if (allowBounds)
				{
					return valueColumn[end];
				}
				return missingValue;
			}
			double retVal = missingValue;
			if (mode == null)
			{
				for (int i = 0; i < lookupColumn.Length; i++)
				{
					if (lookupColumn[i] == val)
					{
						retVal = valueColumn[i];
						break;
					}
				}
			}
			else
			{
				retVal = interpolate(val, lookupColumn, valueColumn, mode);
			}
			return retVal;
		}

		public virtual void populate(double[] c1, double[] c2, int size)
		{
			allocateDataSpace(size);
			Array.Copy(c1, 0, column1, 0, c1.Length);
			Array.Copy(c2, 0, column2, 0, c2.Length);
			modified = true;
		}

		public virtual void populate(int row, int col, double value)
		{
			if (col == 0)
			{
				column1[row] = value;
			}
			else
			{
				column2[row] = value;
			}
			modified = true;
		}

		public virtual void setID(string id)
		{
			this.id = id;
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void sort(final int column)
		public virtual void sort(int column)
		{
			// lets not mess around here, just put all the rows in a list and
			// sort them based on column then copy values back
			IList<double[]> rows = new List<double[]>(column1.Length);
			for (int i = 0; i < column1.Length; i++)
			{
				rows.Add(new double[]{column1[i], column2[i]});
			}
			rows.Sort(new ComparatorAnonymousInnerClass(this, column));
			for (int i = 0; i < column1.Length; i++)
			{
				double[] row = rows[i];
				column1[i] = row[0];
				column2[i] = row[1];
			}
		}

		private class ComparatorAnonymousInnerClass : IComparer<double[]>
		{
			private readonly Table outerInstance;

			private int column;

			public ComparatorAnonymousInnerClass(Table outerInstance, int column)
			{
				this.outerInstance = outerInstance;
				this.column = column;
			}


			public int compare(double[] o1, double[] o2)
			{
				return o1[column].CompareTo(o2[column]);
			}
		}

		private static Logger logger()
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			return Logger.getLogger(typeof(Table).FullName);
		}

		public virtual int getSmallerIndex(double value, int column)
		{
			double[] d = column == GETCOLUMN_1 ? column1 : column2;
			int idx = 0;
			for (int i = 0; i < d.Length; i++)
			{
				if (value >= d[i])
				{
					idx = i;
				}
			}
			return idx;
		}

		public virtual int getLargerIndex(double value, int column)
		{
			double[] d = column == GETCOLUMN_1 ? column1 : column2;
			int idx = d.Length - 1;
			for (int i = d.Length - 1; i >= 0; i--)
			{
				if (value <= d[i])
				{
					idx = i;
				}
			}
			return idx;
		}

		/// <summary>
		/// Create an empty 2-column table (zero rows).
		/// </summary>
		public Table()
		{
		}

		/// <summary>
		/// Create a new 2-column table with given number of rows and initialized to missing values </summary>
		/// <param name="size"> number of rows in the table. </param>
		public Table(int size)
		{
			allocateDataSpace(size);
		}

		public Table(Table copy)
		{
			if (copy.column1 != null)
			{
				column1 = copy.column1.Clone();
			}
			if (copy.column2 != null)
			{
				column2 = copy.column2.Clone();
			}
			id = copy.id;
			missingValue = copy.missingValue;
		}

		public static Table create(params double[] data)
		{
			Table t = new Table(data.Length / 2);
			for (int i = 0; i < data.Length; i += 2)
			{
				t.set(i / 2, data[i], data[i + 1]);
			}
			return t;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0, ii = getNRows(); i < ii; i++)
			{
				sb.Append(column1[i]).Append(" : ").Append(column2[i]);
				if (i + 1 < ii)
				{
					sb.Append('\n');
				}
			}
			return sb.ToString();
		}

		private double interpolate(double x, double[] lookupColumn, double[] valueColumn, InterpolationMode mode)
		{
			for (int i = 0, ii = lookupColumn.Length - 1; i < ii; i++)
			{
				if (x == lookupColumn[i])
				{
					return valueColumn[i];
				}
				int j = i + 1;
				if (x > lookupColumn[i] && x < lookupColumn[j])
				{
					double xmin = lookupColumn[i];
					double xmax = lookupColumn[j];
					double ymin = valueColumn[i];
					double ymax = valueColumn[j];
					double y;
					if (mode == InterpolationMode.LINEAR)
					{
						if (xmax - xmin == 0)
						{
							y = ymin;
						}
						else
						{
							y = ymin + (ymax - ymin) * (x - xmin) / (xmax - xmin);
						}
					}
					else
					{
						double bar = xmax - xmin == 0 ? 0 : (x - xmin) / (xmax - xmin);
						y = Math.Log10(ymin) + (Math.Log10(ymax) - Math.Log10(ymin)) * bar;
						y = Math.Pow(10, y);
					}
					return y;
				}
			}
			if (x == lookupColumn[lookupColumn.Length - 1])
			{
				return valueColumn[lookupColumn.Length - 1];
			}
			throw new System.ArgumentException(x + ":" + Arrays.ToString(lookupColumn));
		}
	}

}
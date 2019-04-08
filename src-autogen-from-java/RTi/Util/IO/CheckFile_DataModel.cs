using System.Collections.Generic;

// CheckFile_DataModel - DataModel class to store data objects for the CheckFile

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

/// <summary>
///***************************************************************************
/// CheckFile_DataModel class - 2007-03-27 - KAT
/// ******************************************************************************
/// Revisions
/// 2007-03-27	Kurt Tometich, RTi		Initial version.
/// ****************************************************************************
/// </summary>
namespace RTi.Util.IO
{

	/// <summary>
	/// DataModel class to store data objects for the CheckFile.
	/// Each DataModel has a title, header, info, total number
	/// of checks and a list of data.
	/// </summary>
	public class CheckFile_DataModel
	{
		private System.Collections.IList __data; // stores data from running checks
		private string __info; // stores information and suggestions
											// about the latest data check run

		private string[] __table_header; // column headers for the data
		private string __title; // title or name of the component
		private int __total_problems; // total number of product rows that
											// had problems
		private int __total_checked; // total number of component objects
											// checked on last run

		/// <summary>
		/// Initializes a DataModel object for a Check File. </summary>
		/// <param name="data"> The list of data to check. </param>
		/// <param name="table_header"> The column headers for the data. </param>
		/// <param name="title"> The title or name of the data being checked (data component). </param>
		/// <param name="info"> </param>
		/// <param name="num_problems"> </param>
		/// <param name="total"> </param>
		public CheckFile_DataModel(System.Collections.IList data, string[] table_header, string title, string info, int num_problems, int total)
		{
			// store data from checks
			if (data != null)
			{
				__data = data;
			}
			else
			{
				__data = new List<string []>();
			}
			// store table column headers
			if (table_header != null)
			{
				__table_header = table_header;
			}
			else
			{
				__table_header = new string[]{};
			}
			// store title or name of component
			if (!string.ReferenceEquals(title, null))
			{
				__title = title;
			}
			else
			{
				__title = "Data";
			}
			// store info on current data checks
			if (!string.ReferenceEquals(info, null))
			{
				__info = info;
			}
			else
			{
				__info = "";
			}
			// store the total number of component objects checked
			__total_checked = total;
			__total_problems = num_problems;
		}

		/// <summary>
		/// Returns the data list for this model. </summary>
		/// <returns> list of invalid data. </returns>
		public virtual System.Collections.IList getData()
		{
			return __data;
		}

		/// <summary>
		/// Returns the size of the data list or number of
		/// invalid rows. </summary>
		/// <returns> Size of the data list. </returns>
		public virtual int getDataSize()
		{
			return __data.Count;
		}

		/// <summary>
		/// Returns the list of table column headers. </summary>
		/// <returns> List of table headers. </returns>
		public virtual string[] getTableHeader()
		{
			return __table_header;
		}

		/// <summary>
		/// Returns the title or name of the data component that
		/// was checked. </summary>
		/// <returns> Name or title of the data component. </returns>
		public virtual string getTitle()
		{
			return __title;
		}

		/// <summary>
		/// Returns extra information about the current data check
		/// that was run.  This may include reasons for failure or
		/// extra information pertaining to the specific data
		/// component. </summary>
		/// <returns> Extra information about the data checks. </returns>
		public virtual string getInfo()
		{
			return __info;
		}

		/// <summary>
		/// Returns the total number of data component objects checked. </summary>
		/// <returns> Number of data component objects that were checked. </returns>
		public virtual int getTotalChecked()
		{
			return __total_checked;
		}

		/// <summary>
		/// Returns the total number of data component that have problems. </summary>
		/// <returns> Number of data component objects that have problems. </returns>
		public virtual int getTotalNumberProblems()
		{
			return __total_problems;
		}
	}

}
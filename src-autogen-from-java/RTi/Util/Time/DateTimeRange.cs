// DateTimeRange - this class stores a range defined by two DateTime end-points.

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

namespace RTi.Util.Time
{
	/// <summary>
	/// This class stores a range defined by two DateTime end-points.
	/// It is useful for specifying a processing period.
	/// It is NOT the same as DateTimeWindow, which is a window within each year of a period.
	/// Currently the instance is immutable and copies of the DateTime data are copied at construction.
	/// Null date/times are allowed.  Currently there is no validation done.
	/// </summary>
	public class DateTimeRange
	{

		/// <summary>
		/// Starting DateTime for the range.
		/// </summary>
		private DateTime start = null;

		/// <summary>
		/// Ending DateTime for the range.
		/// </summary>
		private DateTime end = null;

		/// <summary>
		/// Constructor. </summary>
		/// <param name="start"> starting date/time in the range.  Can be null to indicate open-ended range (from available start). </param>
		/// <param name="end"> ending date/time in the range.  Can be null to indicate open-ended range (to available end). </param>
		public DateTimeRange(DateTime start, DateTime end)
		{
			if (start == null)
			{
				this.start = null;
			}
			else
			{
				this.start = new DateTime(start);
			}
			if (end == null)
			{
				this.end = null;
			}
			else
			{
				this.end = new DateTime(end);
			}
		}

		/// <summary>
		/// Return the ending date/time in the range (can be null) if open-ended. </summary>
		/// <returns> the ending date/time in the range (can be null) if open-ended. </returns>
		public virtual DateTime getEnd()
		{
			return this.end;
		}

		/// <summary>
		/// Return the starting date/time in the range (can be null) if open-ended. </summary>
		/// <returns> the starting date/time in the range (can be null) if open-ended. </returns>
		public virtual DateTime getStart()
		{
			return this.start;
		}

	}

}
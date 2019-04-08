// TSViewTable_Irregular_TableModel - provides a table model for displaying irregular TS

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

namespace RTi.GRTS
{

	using IrregularTS = RTi.TS.IrregularTS;
	using TSData = RTi.TS.TSData;
	using TSException = RTi.TS.TSException;
	using Message = RTi.Util.Message.Message;
	using DateTime = RTi.Util.Time.DateTime;
	/// <summary>
	///  Provides a table model for displaying irregular TS.
	///  <para>
	/// 
	/// </para>
	/// </summary>
	///  <seealso cref= TSViewTable_TableModel for displaying regular TS.  </seealso>
	public class TSViewTable_Irregular_TableModel : TSViewTable_TableModel
	{

	  internal System.Collections.IList dataPoints;
	  internal IrregularTS irrTS = null;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TSViewTable_Irregular_TableModel(java.util.List data, RTi.Util.Time.DateTime start, int intervalBase, int intervalMult, int dateFormat, String[] dataFormats, boolean useExtendedLegend) throws Exception
	  public TSViewTable_Irregular_TableModel(System.Collections.IList data, DateTime start, int intervalBase, int intervalMult, int dateFormat, string[] dataFormats, bool useExtendedLegend) : this(data, start, intervalBase, intervalMult, dateFormat, dataFormats, useExtendedLegend, 50)
	  {
		  // TODO Auto-generated constructor stub
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TSViewTable_Irregular_TableModel(java.util.List data, RTi.Util.Time.DateTime start, int intervalBase, int intervalMult, int dateFormat, String[] dataFormats, boolean useExtendedLegend, int cacheInterval) throws Exception
	  public TSViewTable_Irregular_TableModel(System.Collections.IList data, DateTime start, int intervalBase, int intervalMult, int dateFormat, string[] dataFormats, bool useExtendedLegend, int cacheInterval) : base(data, start, intervalBase, intervalMult, dateFormat, dataFormats, useExtendedLegend, cacheInterval)
	  {
	  }

	  /// <summary>
	  /// Determine the number of rows for the table model.
	  /// </summary>
	  /// <param name="data"> </param>
	  /// <exception cref="TSException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void calcRowCount(java.util.List data) throws RTi.TS.TSException
	  protected internal virtual void calcRowCount(System.Collections.IList data)
	  {
		string routine = "calcRowCount";
		if (data[0] is IrregularTS)
		{
			irrTS = (IrregularTS)data[0];
		}
		else
		{
			Message.printWarning(3, routine, "Not a irregularTS: " + irrTS.getIdentifierString());
		}
		dataPoints = irrTS.getData();
	   //TODO: dre verify right start & end dates
	   // _rows = irrTS.calculateDataSize(__start, irrTS.getDate2());
		_rows = dataPoints.Count;
	  }

	  /// <summary>
	  /// Returns the data that should be placed in the JTable at the given row and column. </summary>
	  /// <param name="row"> the row for which to return data. </param>
	  /// <param name="col"> the column for which to return data. </param>
	  /// <returns> the data that should be placed in the JTable at the given row and col. </returns>
	  public override object getValueAt(int row, int col)
	  {
		TSData d = (TSData)dataPoints[row];

		if (col == 0)
		{
			return d.getDate();
		}
		else
		{
			return new double?(d.getDataValue());
		}
	  }


	  /// <summary>
	  /// Initialize the dates for cache.
	  /// </summary>
	  /* FIXME SAM 2008-03-24 Need to enable irregular time series viewing
	  protected void initializeCacheDates()
	  {
	    if(true) return;
	    
	    IrregularTS irrTS = (IrregularTS)_data.elementAt(0);
	    __cachedDates = new DateTime[(_rows / __cacheInterval) + 1];
	
	    // Cache the dates of each __cacheInterval row through the time series.
	    Vector dataPoints = irrTS.getData();
	
	    int index = 0;
	    for (int i = 0; i < __cachedDates.length; i++)
	      {
	        index += __cacheInterval;
	        TSData d = (TSData)dataPoints.elementAt(index);
	        d.getDate();
	        __cachedDates[i] = new DateTime(d.getDate());
	      }
	  }
	  */
	} // eof class TSViewTable_Irregular_TableModel

}
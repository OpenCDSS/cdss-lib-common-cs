using System.Collections.Generic;

// PluginDataStore - interface to define behavior of plugin DataStore

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

namespace riverside.datastore
{

	using TS = RTi.TS.TS;
	using TSIdent = RTi.TS.TSIdent;
	using InputFilter_JPanel = RTi.Util.GUI.InputFilter_JPanel;
	using JWorksheet_AbstractExcelCellRenderer = RTi.Util.GUI.JWorksheet_AbstractExcelCellRenderer;
	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;
	using DateTime = RTi.Util.Time.DateTime;

	// TODO SAM 2016-04-16 This is pretty specific to the needs of TSTool so may move it to the TSTool package
	/// <summary>
	/// Interface for a plugin datastore, which allows integration of plugins with frameworks.
	/// @author sam
	/// 
	/// </summary>
	public interface PluginDataStore
	{

		/// <summary>
		/// Create an input filter panel for the datastore, used to query the time series list.
		/// The providesTimeSeriesInputFilterPanel() method should be called first to determine whether this
		/// method is supported. </summary>
		/// <returns> an InputFilter_Panel instance, or null if the method is not supported. </returns>
		InputFilter_JPanel createTimeSeriesListInputFilterPanel();

		/// <summary>
		/// Create a time series list table model given the desired data type, time step (interval), and input filter.
		/// The datastore performs a suitable query and creates objects to manage in the time series list. </summary>
		/// <param name="dataType"> time series data type to query, controlled by the datastore </param>
		/// <param name="timeStep"> time interval to query, controlled by the datastore </param>
		/// <param name="ifp"> input filter panel that provides additional filter options </param>
		/// <returns> a TableModel containing the defined columns and rows. </returns>
		JWorksheet_AbstractRowTableModel createTimeSeriesListTableModel(string dataType, string timeStep, InputFilter_JPanel ifp);

		/// <summary>
		/// Return the list of time series data type strings.
		/// These strings are specific to the datastore and may be simple like "DataType1"
		/// or more complex like "DataStore1 - note for data type". </summary>
		/// <param name="dataInterval"> data interval from TimeInterval.getName(TimeInterval.HOUR,0) to filter the list of data types.
		/// If null, blank, or "*" the interval is not considered when determining the list of data types. </param>
		IList<string> getTimeSeriesDataTypeStrings(string dataInterval);

		/// <summary>
		/// Return the list of time series data interval strings.
		/// This should result from calls like:  TimeInterval.getName(TimeInterval.HOUR, 0) </summary>
		/// <param name="dataType"> data type string to filter the list of data intervals.
		/// If null, blank, or "*" the data type is not considered when determining the list of data intervals. </param>
		IList<string> getTimeSeriesDataIntervalStrings(string dataType);

		/// <summary>
		/// Return the identifier for a time series in the table model.
		/// The TSIdent parts will be uses as TSID commands. </summary>
		/// <param name="tableModel"> the table model from which to extract data </param>
		/// <param name="row"> the displayed table row </param>
		TSIdent getTimeSeriesIdentifierFromTableModel(JWorksheet_AbstractRowTableModel tableModel, int row);

		/// <summary>
		/// Get the CellRenderer used for displaying the time series in a TableModel.
		/// 
		/// </summary>
		JWorksheet_AbstractExcelCellRenderer getTimeSeriesListCellRenderer(JWorksheet_AbstractRowTableModel tableModel);

		/// <summary>
		/// Indicate whether the plugin provides an input filter panel for the time series list.
		/// This is a component that provides interactive query filters for user interfaces. </summary>
		/// <param name="return"> true if an input filter is provided. </param>
		bool providesTimeSeriesListInputFilterPanel();

		/// <summary>
		/// Get the TableModel used for displaying the time series.
		/// 
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public RTi.Util.GUI.JWorksheet_AbstractRowTableModel getTimeSeriesListTableModel(java.util.List<? extends Object> data);
		JWorksheet_AbstractRowTableModel getTimeSeriesListTableModel<T1>(IList<T1> data);

		/// <summary>
		/// Read a time series given its time series identifier. </summary>
		/// <returns> the time series or null if not read </returns>
		TS readTimeSeries(string tsidentString2, DateTime readStart, DateTime readEnd, bool readData);
	}

}
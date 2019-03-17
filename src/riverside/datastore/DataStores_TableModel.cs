using System;
using System.Collections.Generic;

// DataStores_TableModel - UI table model to display a list of DataStores

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

	using DatabaseDataStore = RTi.DMI.DatabaseDataStore;
	using JWorksheet_AbstractRowTableModel = RTi.Util.GUI.JWorksheet_AbstractRowTableModel;

	/// <summary>
	/// Table model for displaying data store data in a JWorksheet.
	/// This one table model is used for database and web service data stores because currently
	/// it is easier to show all than split out and make the user pick different categories for viewing.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class DataStores_TableModel extends RTi.Util.GUI.JWorksheet_AbstractRowTableModel
	public class DataStores_TableModel : JWorksheet_AbstractRowTableModel
	{

	/// <summary>
	/// The table displayed in the worksheet.
	/// </summary>
	private IList<DataStore> __dataStoreList = null;

	/// <summary>
	/// Number of columns in the table model (with the alias).
	/// </summary>
	private int __COLUMNS = 14;

	/// <summary>
	/// Absolute column indices, for column lookups.
	/// </summary>
	public readonly int COL_TYPE = 0;
	public readonly int COL_NAME = 1;
	public readonly int COL_DESCRIPTION = 2;
	public readonly int COL_ENABLED = 3;
	public readonly int COL_STATUS = 4;
	// Enabled for all but currently only database has enabled
	public readonly int COL_TS_INTERFACE_DEFINED = 5;
	public readonly int COL_TS_INTERFACE_WORKS = 6;
	// Database data store...
	public readonly int COL_DATABASE_SERVER = 7;
	public readonly int COL_DATABASE_NAME = 8;
	// Straight ODBC connection...
	public readonly int COL_ODBC_NAME = 9;
	public readonly int COL_SYSTEM_LOGIN = 10;
	// Web service data store...
	public readonly int COL_SERVICE_ROOT_URI = 11;
	// General error string
	public readonly int COL_STATUS_MESSAGE = 12;
	public readonly int COL_CONFIG_FILE = 13;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dataStoreList"> the list of data stores to show in a worksheet. </param>
	/// <exception cref="NullPointerException"> if the dataTable is null. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DataStores_TableModel(java.util.List<DataStore> dataStoreList) throws Exception
	public DataStores_TableModel(IList<DataStore> dataStoreList)
	{
		if (dataStoreList == null)
		{
			_rows = 0;
		}
		else
		{
			_rows = dataStoreList.Count;
		}
		__dataStoreList = dataStoreList;
	}

	/// <summary>
	/// From AbstractTableModel.  Returns the class of the data stored in a given
	/// column.  All values are treated as strings. </summary>
	/// <param name="columnIndex"> the column for which to return the data class. </param>
	public virtual Type getColumnClass(int columnIndex)
	{
		switch (columnIndex)
		{
			// All handled as strings
			default:
				return typeof(string);
		}
	}

	/// <summary>
	/// From AbstractTableMode.  Returns the number of columns of data. </summary>
	/// <returns> the number of columns of data. </returns>
	public virtual int getColumnCount()
	{
		return __COLUMNS;
	}

	/// <summary>
	/// From AbstractTableMode.  Returns the name of the column at the given position. </summary>
	/// <returns> the name of the column at the given position. </returns>
	public virtual string getColumnName(int columnIndex)
	{
		switch (columnIndex)
		{
			case COL_TYPE:
				return "Type";
			case COL_NAME:
				return "Name";
			case COL_DESCRIPTION:
				return "Description";
			case COL_ENABLED:
				return "Enabled";
			case COL_STATUS:
				return "Status";
			case COL_TS_INTERFACE_DEFINED:
				return "Time Series Interface Defined";
			case COL_TS_INTERFACE_WORKS:
				return "Time Series Interface Works";
			case COL_DATABASE_SERVER:
				return "Database Server";
			case COL_DATABASE_NAME:
				return "Database Name";
			case COL_ODBC_NAME:
				return "ODBC Name";
			case COL_SYSTEM_LOGIN:
				return "Database Login";
			case COL_SERVICE_ROOT_URI:
				return "Web Service Root URI";
			case COL_STATUS_MESSAGE:
				return "Status Message";
			case COL_CONFIG_FILE:
				return "Configuration File";
			default:
				return "";
		}
	}

	/// <summary>
	/// Return tool tips for the columns.
	/// </summary>
	public override string[] getColumnToolTips()
	{
		string[] tooltips = new string[__COLUMNS];
		tooltips[COL_TYPE] = "Datastore type.";
		tooltips[COL_NAME] = "Datastore name.";
		tooltips[COL_DESCRIPTION] = "Datastore description.";
		tooltips[COL_ENABLED] = "Is datastore enabled?  Currently always true if displayed because disabled datastores won't be initialized.";
		tooltips[COL_STATUS] = "Status (Ok/Error) - see Status Message";
		tooltips[COL_TS_INTERFACE_DEFINED] = "Is time series interface defined? - see TimeSeries* properties in datstore configuration file.";
		tooltips[COL_TS_INTERFACE_WORKS] = "Does time series interface work, based on query of tables in TimeSeries* datastore configuration properties?";
		tooltips[COL_DATABASE_SERVER] = "Database server for database datastore.";
		tooltips[COL_DATABASE_NAME] = "Database name for database datastore.";
		tooltips[COL_ODBC_NAME] = "ODBC name when used with generic database datastore.";
		tooltips[COL_SYSTEM_LOGIN] = "Database account login.";
		tooltips[COL_SERVICE_ROOT_URI] = "Root URI for web service datastore.";
		tooltips[COL_STATUS_MESSAGE] = "Error message (e.g., when initialization failed).";
		tooltips[COL_CONFIG_FILE] = "Datastore configuration file.";
		return tooltips;
	}

	/// <summary>
	/// Returns the format to display the specified column. </summary>
	/// <param name="column"> column for which to return the format. </param>
	/// <returns> the format (as used by StringUtil.formatString()). </returns>
	public override string getFormat(int column)
	{
		switch (column)
		{
			default:
				return "%s";
		}
	}

	/// <summary>
	/// From AbstractTableMode.  Returns the number of rows of data in the table.
	/// </summary>
	public virtual int getRowCount()
	{
		return _rows;
	}

	/// <summary>
	/// From AbstractTableMode.  Returns the data that should be placed in the JTable at the given row and column. </summary>
	/// <param name="row"> the row for which to return data. </param>
	/// <param name="col"> the absolute column for which to return data. </param>
	/// <returns> the data that should be placed in the JTable at the given row and column. </returns>
	public virtual object getValueAt(int row, int col)
	{ // make sure the row numbers are never sorted ...

		if (_sortOrder != null)
		{
			row = _sortOrder[row];
		}

		DataStore dataStore = __dataStoreList[row];
		if (dataStore == null)
		{
			return "";
		}
		DatabaseDataStore databaseDataStore = null;
		if (dataStore is DatabaseDataStore)
		{
			databaseDataStore = (DatabaseDataStore)dataStore;
		}
		WebServiceDataStore webServiceDataStore = null;
		if (dataStore is WebServiceDataStore)
		{
			webServiceDataStore = (WebServiceDataStore)dataStore;
		}
		switch (col)
		{
			case COL_TYPE:
				// Use the class name but don't include the package
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				string clazz = dataStore.GetType().FullName;
				string[] parts = clazz.Split("\\.", true);
				return parts[parts.Length - 1];
			case COL_NAME:
				return dataStore.getName();
			case COL_DESCRIPTION:
				return dataStore.getDescription();
			case COL_ENABLED:
				object o = dataStore.getProperty("Enabled");
				if (o == null)
				{
					return "True";
				}
				else
				{
					return "" + o;
				}
			case COL_STATUS:
				int status = dataStore.getStatus();
				if (status == 0)
				{
					return "Ok";
				}
				else
				{
					return "Error (" + status + ")";
				}
			case COL_TS_INTERFACE_DEFINED:
				if (dataStore is GenericDatabaseDataStore)
				{
					GenericDatabaseDataStore ds = (GenericDatabaseDataStore)dataStore;
					if (ds.hasTimeSeriesInterface(false))
					{
						return "Yes";
					}
					else
					{
						return "No";
					}
				}
				else
				{
					return "No";
				}
			case COL_TS_INTERFACE_WORKS:
				if (dataStore is GenericDatabaseDataStore)
				{
					GenericDatabaseDataStore ds = (GenericDatabaseDataStore)dataStore;
					if (ds.hasTimeSeriesInterface(true))
					{
						return "Yes";
					}
					else
					{
						return "No";
					}
				}
				else
				{
					return "No";
				}
			case COL_DATABASE_SERVER:
				if (databaseDataStore != null)
				{
					if (databaseDataStore.getDMI() != null)
					{
						return databaseDataStore.getDMI().getDatabaseServer();
					}
					else
					{
						return "";
					}
				}
				else
				{
					return "";
				}
			case COL_DATABASE_NAME:
				if (databaseDataStore != null)
				{
					if (databaseDataStore.getDMI() != null)
					{
						return databaseDataStore.getDMI().getDatabaseName();
					}
					else
					{
						return "";
					}
				}
				else
				{
					return "";
				}
			case COL_ODBC_NAME:
				if (databaseDataStore != null)
				{
					if (databaseDataStore.getDMI() != null)
					{
						return databaseDataStore.getDMI().getODBCName();
					}
					else
					{
						return "";
					}
				}
				else
				{
					return "";
				}
			case COL_SYSTEM_LOGIN:
				return dataStore.getProperty("SystemLogin");
			case COL_SERVICE_ROOT_URI:
				if (webServiceDataStore != null)
				{
					return webServiceDataStore.getServiceRootURI();
				}
				else
				{
					return "";
				}
			case COL_STATUS_MESSAGE:
				return dataStore.getStatusMessage();
			case COL_CONFIG_FILE:
				return dataStore.getProperty("DataStoreConfigFile");
			default:
				return "";
		}
	}

	/// <summary>
	/// Returns an array containing the column widths (in number of characters). </summary>
	/// <returns> an integer array containing the widths for each field. </returns>
	public virtual int[] getColumnWidths()
	{
		int[] widths = new int[__COLUMNS];
		widths[COL_TYPE] = 20;
		widths[COL_NAME] = 20;
		widths[COL_DESCRIPTION] = 45;
		widths[COL_ENABLED] = 10;
		widths[COL_STATUS] = 10;
		widths[COL_TS_INTERFACE_DEFINED] = 8;
		widths[COL_TS_INTERFACE_WORKS] = 8;
		widths[COL_DATABASE_SERVER] = 15;
		widths[COL_DATABASE_NAME] = 15;
		widths[COL_ODBC_NAME] = 16;
		widths[COL_SYSTEM_LOGIN] = 10;
		widths[COL_SERVICE_ROOT_URI] = 50;
		widths[COL_STATUS_MESSAGE] = 30;
		widths[COL_CONFIG_FILE] = 50;
		return widths;
	}

	}

}
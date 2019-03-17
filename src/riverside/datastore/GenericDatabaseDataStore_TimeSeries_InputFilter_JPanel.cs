using System;
using System.Collections.Generic;

// GenericDatabaseDataStore_TimeSeries_InputFilter_JPanel - input filter panel to use with a GenericDatabaseDataStore

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

	using InputFilter = RTi.Util.GUI.InputFilter;
	using InputFilter_JPanel = RTi.Util.GUI.InputFilter_JPanel;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This class is an input filter for querying GenericDatabaseDataStore database.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class GenericDatabaseDataStore_TimeSeries_InputFilter_JPanel extends RTi.Util.GUI.InputFilter_JPanel
	public class GenericDatabaseDataStore_TimeSeries_InputFilter_JPanel : InputFilter_JPanel //implements ItemListener, KeyListener
	{

	/// <summary>
	/// Associated data store.
	/// </summary>
	private GenericDatabaseDataStore __dataStore = null;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="dataStore"> the datastore for queries.  Cannot be null. </param>
	/// <param name="numFilterGroups"> the number of filter groups to display </param>
	public GenericDatabaseDataStore_TimeSeries_InputFilter_JPanel(GenericDatabaseDataStore dataStore, int numFilterGroups) : base()
	{
		__dataStore = dataStore;
		setFilters(numFilterGroups);
	}

	/// <summary>
	/// Set the filter data.  This method is called at setup.
	/// Always use the most current parameter name from the API (translate when filter is initialized from input). </summary>
	/// <param name="numFilterGroups"> the number of filter groups to display </param>
	public virtual void setFilters(int numFilterGroups)
	{
		string routine = this.GetType().Name + ".setFilters";
		IList<InputFilter> filters = new List<InputFilter>();

		GenericDatabaseDataStore ds = getDataStore();
		string metaTable = ds.getProperty(GenericDatabaseDataStore.TS_META_TABLE_PROP);
		string locTypeColumn = ds.getProperty(GenericDatabaseDataStore.TS_META_TABLE_LOCTYPE_COLUMN_PROP);
		string locIdColumn = ds.getProperty(GenericDatabaseDataStore.TS_META_TABLE_LOCATIONID_COLUMN_PROP);
		string dataSourceColumn = ds.getProperty(GenericDatabaseDataStore.TS_META_TABLE_DATASOURCE_COLUMN_PROP);
		string scenarioColumn = ds.getProperty(GenericDatabaseDataStore.TS_META_TABLE_SCENARIO_COLUMN_PROP);

		// Get lists for choices.  The data type and interval are selected outside the filter and are not included
		// Don't cascade the filters.  Just list unique values for all
		IList<string> locTypes = new List<string>();
		if ((!string.ReferenceEquals(locTypeColumn, null)) && !locTypeColumn.Equals(""))
		{
			locTypes = ds.readTimeSeriesMetaLocationTypeList(null, null, null, null, null);
		}
		IList<string> locIds = new List<string>();
//JAVA TO C# CONVERTER WARNING: LINQ 'SequenceEqual' is not always identical to Java AbstractList 'equals':
//ORIGINAL LINE: if ((locIds != null) && !locIds.equals(""))
		if ((locIds != null) && !locIds.SequenceEqual(""))
		{
			locIds = ds.readTimeSeriesMetaLocationIDList(null, null, null, null, null);
		}
		IList<string> dataSources = new List<string>();
		if ((!string.ReferenceEquals(dataSourceColumn, null)) && !dataSourceColumn.Equals(""))
		{
			dataSources = ds.readTimeSeriesMetaDataSourceList(null, null, null, null, null);
		}
		IList<string> scenarios = new List<string>();
		if ((!string.ReferenceEquals(scenarioColumn, null)) && !scenarioColumn.Equals(""))
		{
			scenarios = ds.readTimeSeriesMetaScenarioList(null, null, null, null, null);
		}

		// Because GenericDatabaseDataStore is generic, it is difficult to know what will
		// be returned from the above and the filters allow user-entered values.
		// Therefore even if above does not return anything, add the filters below to allow user input.

		filters.Add(new InputFilter("", "", StringUtil.TYPE_STRING, null, null, false)); // Blank

		filters.Add(new InputFilter("Location Type", metaTable + "." + locTypeColumn, "", StringUtil.TYPE_STRING, locTypes, locTypes, true, "Location type"));

		filters.Add(new InputFilter("Location ID", metaTable + "." + locIdColumn, "", StringUtil.TYPE_STRING, locIds, locIds, true, "Location identifiers for the location type"));

		filters.Add(new InputFilter("Data Source", metaTable + "." + dataSourceColumn, "", StringUtil.TYPE_STRING, dataSources, dataSources, true, "Data source (data provider)"));

		filters.Add(new InputFilter("Scenario", metaTable + "." + scenarioColumn, "", StringUtil.TYPE_STRING, scenarios, scenarios, true, "Scenario helps uniquely identify time series"));

		// Add additional filters with property name TimeSeriesMetadata_MetadataFilter.1, etc. (number increasing)
		// The property value is a string with comma-separated values indicating:
		// -  Label, shown in the filter drop-down
		// - Database table/view column name (without table/view), used when querying for values
		// - list/field, indicates whether unique list should be provided or text field
		// - editable/noneditable, indicates whether list/field should be editable
		// - description, used for pop-up help

		int i = 0;
		while (true)
		{
			++i;
			string propName = GenericDatabaseDataStore.TS_META_TABLE_FILTER_PROP + "." + i;
			string propVal = ds.getProperty(propName);
			if (string.ReferenceEquals(propVal, null))
			{
				// Done with filter properties
				break;
			}
			string[] parts = propVal.Split(",", true);
			if (parts.Length == 5)
			{
				string label = parts[0].Trim();
				string column = parts[1].Trim();
				string componentType = parts[2].Trim();
				bool editable = false;
				if (parts[3].Trim().Equals("true", StringComparison.OrdinalIgnoreCase))
				{
					editable = true;
				}
				string description = parts[4].Trim();
				// TODO SAM 2015-02-06 For now always add as field
				editable = true;
				componentType = "field";
				if (componentType.Equals("list", StringComparison.OrdinalIgnoreCase))
				{
					// Query the choices from the database and add as a list
				}
				else
				{
					// Add as a field.
					filters.Add(new InputFilter(label, metaTable + "." + column, "", StringUtil.TYPE_STRING, null, null, editable, description));
				}
			}
			else
			{
				Message.printWarning(3,routine,"Datastore configuration property \"" + propName + "\" does not provide exactly 5 configuration values.");
			}
		}

		setToolTipText("Database queries can be filtered based on location and time series metadata");
		setInputFilters(filters, numFilterGroups, 25);
	}

	/// <summary>
	/// Return the data store corresponding to this input filter panel. </summary>
	/// <returns> the data store corresponding to this input filter panel. </returns>
	public virtual GenericDatabaseDataStore getDataStore()
	{
		return __dataStore;
	}

	}

}
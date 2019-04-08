using System;
using System.Collections.Generic;

// GeoViewAnimationLayerData - data for controlling how layers are built for GeoViewAnimationData objects

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

//-----------------------------------------------------------------------------
// GeoViewAnimationLayerData - Data for controlling how layers are built 
//	for GeoViewAnimationData objects.
//-----------------------------------------------------------------------------
// Copyright:  See the COPYRIGHT file.
//-----------------------------------------------------------------------------
// History:
// 2004-08-12	J. Thomas Sapienza, RTi	Initial version.
// 2005-04-27	JTS, RTi		Added finalize().
//-----------------------------------------------------------------------------

namespace RTi.GIS.GeoView
{

	using GRSymbol = RTi.GR.GRSymbol;

	using IOUtil = RTi.Util.IO.IOUtil;
	using PropList = RTi.Util.IO.PropList;

	using DataTable = RTi.Util.Table.DataTable;

	/// <summary>
	/// This is a class that controls how layers are created for animation.  A single
	/// one of these objects is created and then shared among all the different 
	/// GeoViewAnimationData objects that will be used together in the same layer.
	/// </summary>
	public class GeoViewAnimationLayerData
	{

	/// <summary>
	/// Whether maximal animation values should be equalized to the same level.
	/// </summary>
	private bool __equalizeMax = false;

	/// <summary>
	/// The attribute table for this layer.
	/// </summary>
	private DataTable __table = null;

	/// <summary>
	/// The value of missing data on this layer.
	/// </summary>
	private double __missing = -999.0;

	/// <summary>
	/// The value that will replace missing data on this layer.
	/// </summary>
	private double __missingReplacement = -1;

	/// <summary>
	/// The numbers of fields that tie the identifier to features.
	/// </summary>
	private int[] __idFields = null;

	/// <summary>
	/// The fields for data to be displayed.  For non-complicated symbols, such as
	/// GRSymbol.SYM_VBARSIGNED, the data fields are the numbers of fields that will
	/// always be shown on the layer and which are not animated.  For complicated 
	/// symbols, such as teacups, the datafields are a key in a particularly order that
	/// provide information on how things should be animated.  See 
	/// GeoViewJPanel.addSummaryLayerView() for more information on these symbols.
	/// </summary>
	private int[] __dataFields = null;

	/// <summary>
	/// The kind of symbol to use.
	/// </summary>
	private int __symbolType = GRSymbol.SYM_VBARSIGNED;

	/// <summary>
	/// Properties providing additional information to the layer.  See
	/// GeoViewJPanel.addSummaryLayerView() for more information.
	/// </summary>
	private PropList __props = null;

	/// <summary>
	/// The names of the data fields.
	/// </summary>
	private string[] __dataFieldsStrings = null;

	/// <summary>
	/// The names of the id fields.
	/// </summary>
	private string[] __idFieldsStrings = null;

	/// <summary>
	/// The name of the layer.
	/// </summary>
	private string __layerName = null;

	/// <summary>
	/// The app layer types to search through for matches.
	/// </summary>
	private IList<string> __availAppLayerTypes = null;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="table"> the data table to use for the layer </param>
	/// <param name="layerName"> the name of the animation layer </param>
	/// <param name="symbolType"> the kind of symbol that is being animated.  Currently
	/// supports GRSymbol.SYM_VBARSIGNED and GRSymbol.SYM_TEACUP. </param>
	/// <param name="idFields"> the names of the id fields to use for tying data to a feature </param>
	/// <param name="dataFields"> the names of the data fields.  
	/// For non-complicated symbols, such as 
	/// GRSymbol.SYM_VBARSIGNED, the data fields are the fields that will
	/// always be shown on the layer and which are not animated.  For complicated 
	/// symbols, such as teacups, the datafields are a key in a particularly order that
	/// provide information on how things should be animated.  See 
	/// GeoViewJPanel.addSummaryLayerView() for more information on these symbols.<para>
	/// In general, for VBARSIGNED symbols, dataFields will be null.  For teacups, it 
	/// will be a 3-element array where the first element is the field with the maximum
	/// content, the second element is the field with the minimum content, and the 
	/// third element is the field with current content.
	/// </para>
	/// </param>
	/// <param name="availAppLayerTypes"> Vector of app layer types to search through for
	/// feature matches. </param>
	/// <param name="equalizeMax"> whether to equalize the maximum data values </param>
	/// <param name="props"> a PropList that defines additional information to the layer. </param>
	/// <exception cref="Exception"> if table, layerName, idFields, or availAppLayerTypes are
	/// null. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public GeoViewAnimationLayerData(RTi.Util.Table.DataTable table, String layerName, int symbolType, String[] idFields, String[] dataFields, java.util.List<String> availAppLayerTypes, boolean equalizeMax, RTi.Util.IO.PropList props) throws Exception
	public GeoViewAnimationLayerData(DataTable table, string layerName, int symbolType, string[] idFields, string[] dataFields, IList<string> availAppLayerTypes, bool equalizeMax, PropList props)
	{
		if (table == null || string.ReferenceEquals(layerName, null) || idFields == null || availAppLayerTypes == null)
		{
			throw new System.NullReferenceException();
		}

		__table = table;
		__layerName = layerName;
		__symbolType = symbolType;
		__idFieldsStrings = idFields;
		__dataFieldsStrings = dataFields;
		__availAppLayerTypes = availAppLayerTypes;
		__equalizeMax = equalizeMax;
		__props = props;

		// Determine the numbers of the fields named in the id fields array
		__idFields = new int[__idFieldsStrings.Length];
		for (int i = 0; i < __idFieldsStrings.Length; i++)
		{
			try
			{
				__idFields[i] = __table.getFieldIndex(__idFieldsStrings[i]);
			}
			catch (Exception)
			{
				throw new Exception("ID Field #" + i + " (" + __idFieldsStrings[i] + ") not found in " + "table.");
			}
		}

		// determine the numbers of the data fieleds named in the data fields
		// array.
		if (__dataFieldsStrings == null)
		{
			// __dataFields cannot be null, so instantiate a 0-element
			// array instead
			__dataFields = new int[0];
		}
		else
		{
			__dataFields = new int[__dataFieldsStrings.Length];
			for (int i = 0; i < __dataFieldsStrings.Length; i++)
			{
				try
				{
					__dataFields[i] = __table.getFieldIndex(__dataFieldsStrings[i]);
				}
				catch (Exception)
				{
					throw new Exception("Data Field #" + i + " (" + __dataFieldsStrings[i] + ") not found in table.");
				}
			}
		}
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~GeoViewAnimationLayerData()
	{
		__table = null;
		__idFields = null;
		__dataFields = null;
		__props = null;
		IOUtil.nullArray(__dataFieldsStrings);
		IOUtil.nullArray(__idFieldsStrings);
		__layerName = null;
		__availAppLayerTypes = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the available app layer types. </summary>
	/// <returns> the available app layer types. </returns>
	public virtual IList<string> getAvailAppLayerTypes()
	{
		return __availAppLayerTypes;
	}

	/// <summary>
	/// Returns the numbers of the data fields. </summary>
	/// <returns> the numbers of the data fields. </returns>
	public virtual int[] getDataFields()
	{
		return __dataFields;
	}

	/// <summary>
	/// Returns the names of the data fields. </summary>
	/// <returns> the names of the data fields. </returns>
	public virtual string[] getDataFieldsStrings()
	{
		return __dataFieldsStrings;
	}

	/// <summary>
	/// Returns whether to equalize max data. </summary>
	/// <returns> whether to equalzie max data. </returns>
	public virtual bool getEqualizeMax()
	{
		return __equalizeMax;
	}

	/// <summary>
	/// Returns the numbers of the id fields. </summary>
	/// <returns> the numbers of the id fields. </returns>
	public virtual int[] getIDFields()
	{
		return __idFields;
	}

	/// <summary>
	/// Returns the names of the id fields. </summary>
	/// <returns> the names of the id fields. </returns>
	public virtual string[] getIDFieldsStrings()
	{
		return __idFieldsStrings;
	}

	/// <summary>
	/// Returns the layer name. </summary>
	/// <returns> the layer name. </returns>
	public virtual string getLayerName()
	{
		return __layerName;
	}

	/// <summary>
	/// Returns the missing double value. </summary>
	/// <returns> the missing double value. </returns>
	public virtual double getMissingDoubleValue()
	{
		return __missing;
	}

	/// <summary>
	/// Returns the missing double replacement value. </summary>
	/// <returns> the missing double replacement value. </returns>
	public virtual double getMissingDoubleReplacementValue()
	{
		return __missingReplacement;
	}

	/// <summary>
	/// Returns the prop list. </summary>
	/// <returns> the prop list. </returns>
	public virtual PropList getProps()
	{
		return __props;
	}

	/// <summary>
	/// Returns the sumbol type. </summary>
	/// <returns> the symbol type. </returns>
	public virtual int getSymbolType()
	{
		return __symbolType;
	}

	/// <summary>
	/// Returns the attribute table. </summary>
	/// <returns> the attribute table. </returns>
	public virtual DataTable getTable()
	{
		return __table;
	}

	/// <summary>
	/// Sets the value that will be recognized in this layer as missing data.  The
	/// default value is -999.0 </summary>
	/// <param name="missing"> the value that will be recognized as missing data. </param>
	public virtual void setMissingDoubleValue(double missing)
	{
		__missing = missing;
	}

	/// <summary>
	/// Sets the value that will replace missing data in this layer.  The default value
	/// is -1.0 </summary>
	/// <param name="replacement"> the value that will replace missing data. </param>
	public virtual void setMissingDoubleReplacementValue(double replacement)
	{
		__missingReplacement = replacement;
	}

	}

}
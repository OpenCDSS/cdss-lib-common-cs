using System;
using System.Collections.Generic;

// TSProcessor - provides methods to query and process time series into output products (graphs, reports, etc.)

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

	using TS = RTi.TS.TS;
	using TSSupplier = RTi.TS.TSSupplier;
	using IOUtil = RTi.Util.IO.IOUtil;
	using Prop = RTi.Util.IO.Prop;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;
	using StringUtil = RTi.Util.String.StringUtil;
	using DateTime = RTi.Util.Time.DateTime;
	using DateValueTS = RTi.TS.DateValueTS;

	/// <summary>
	/// The TSProcessor class provides methods to query and process time series
	/// into output products (graphs, reports, etc.).
	/// An example of implementation is as follows:
	/// <pre>
	/// try {
	///        TSProcessor p = new TSProcessor ();
	///		p.addTSSupplier ( this );
	///		p.processProduct ( "C:\\tmp\\Test.tsp", new PropList("x") );
	/// }
	/// catch ( Exception e ) {
	///		Message.printWarning ( 1, "TSToolMainGUI.test", "Error processing the product." );
	/// }
	/// </pre>
	/// </summary>
	public class TSProcessor
	{

	/// <summary>
	/// TSSuppliers to use when reading time series.
	/// </summary>
	private TSSupplier[] _suppliers = null;

	/// <summary>
	/// The last TSViewJFrame created and opened when displaying a graph.
	/// </summary>
	private TSViewJFrame __lastTSViewJFrame = null;

	/// <summary>
	/// A single WindowListener that can be associated with the TSViewFrame.  This is
	/// being tested to determine whether an application like TSTool can detect a
	/// TSViewJFrame closing and close the application.
	/// </summary>
	private WindowListener _tsview_window_listener = null;

	public TSProcessor()
	{
	}

	/// <summary>
	/// Add a time series supplier.  Suppliers are used to query time series based on a
	/// time series identifier. </summary>
	/// <param name="supplier"> TSSupplier to use with the TSProcessor. </param>
	public virtual void addTSSupplier(TSSupplier supplier)
	{ // Use arrays to make a little simpler than lists to use later...
		if (supplier != null)
		{
			// Resize the supplier array...
			if (_suppliers == null)
			{
				_suppliers = new TSSupplier[1];
				_suppliers[0] = supplier;
			}
			else
			{
				// Need to resize and transfer the list...
				int size = _suppliers.Length;
				TSSupplier[] newsuppliers = new TSSupplier[size + 1];
				for (int i = 0; i < size; i++)
				{
					newsuppliers[i] = _suppliers[i];
				}
				_suppliers = newsuppliers;
				_suppliers[size] = supplier;
				newsuppliers = null;
			}
		}
	}

	/// <summary>
	/// Add a WindowListener for TSViewFrame instances that are created.  Currently
	/// only one listener can be set. </summary>
	/// <param name="listener"> WindowListener to listen to TSViewFrame WindowEvents. </param>
	public virtual void addTSViewWindowListener(WindowListener listener)
	{
		_tsview_window_listener = listener;
	}

	/// <summary>
	/// Returns the last TSViewJFrame created when displaying a graph. </summary>
	/// <returns> the last TSViewJFrame created when displaying a graph. </returns>
	public virtual TSViewJFrame getLastTSViewJFrame()
	{
		return __lastTSViewJFrame;
	}

	/// <summary>
	/// Process a graph product.  Time series that are indicated in the time series
	/// product are collected by matching in memory or reading time series.  This is
	/// done here so that low-level graph code can get a list of time series and not
	/// need to do any collecting itself. </summary>
	/// <param name="tsproduct"> Time series product definition. </param>
	/// <exception cref="Exception"> if the product cannot be processed (e.g., the graph cannot
	/// be created due to a lack of data). </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void processGraphProduct(TSProduct tsproduct) throws Exception
	private void processGraphProduct(TSProduct tsproduct)
	{
		string routine = "TSProcessor.processGraphProduct";
		IList<TS> tslist = new List<TS>(10);
		TS ts = null;
		// Loop through the sub-products (graphs on page) and get the time series to
		// support the graph.
		string tsid;
		string tsalias;
		string prop_value = null;
		DateTime date1 = null;
		DateTime date2 = null;
		Message.printStatus(2, "", "Processing product");
		int nsubs = tsproduct.getNumSubProducts();
		prop_value = tsproduct.getLayeredPropValue("IsTemplate", -1, -1);
		bool is_template = false;
		if ((!string.ReferenceEquals(prop_value, null)) && prop_value.Equals("true", StringComparison.OrdinalIgnoreCase))
		{
			is_template = true;
			Message.printStatus(2, routine, "Processing template.");
		}
		for (int isub = 0; isub < nsubs ;isub++)
		{
			Message.printStatus(2, routine, "Reading time series for subproduct [" + isub + "]");
			// New...
			prop_value = tsproduct.getLayeredPropValue("IsEnabled", isub, -1);
			// Old...
			if (string.ReferenceEquals(prop_value, null))
			{
				prop_value = tsproduct.getLayeredPropValue("Enabled", isub, -1);
			}
			if ((!string.ReferenceEquals(prop_value, null)) && prop_value.Equals("false", StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			// Loop through the time series in the subproduct
			for (int i = 0; ; i++)
			{
				// New version...
				prop_value = tsproduct.getLayeredPropValue("IsEnabled", isub, i);
				// Old version...
				if (string.ReferenceEquals(prop_value, null))
				{
					prop_value = tsproduct.getLayeredPropValue("Enabled", isub, i);
				}
				if ((!string.ReferenceEquals(prop_value, null)) && prop_value.Equals("false", StringComparison.OrdinalIgnoreCase))
				{
					// Add a null time series...
					tslist.Add(null);
					continue;
				}
				prop_value = tsproduct.getLayeredPropValue("PeriodStart", isub, i);
				if (!string.ReferenceEquals(prop_value, null))
				{
					try
					{
						date1 = DateTime.parse(prop_value);
					}
					catch (Exception)
					{
						date1 = null;
					}
				}
				prop_value = tsproduct.getLayeredPropValue("PeriodEnd", isub, i);
				if (!string.ReferenceEquals(prop_value, null))
				{
					try
					{
						date2 = DateTime.parse(prop_value);
					}
					catch (Exception)
					{
						date2 = null;
					}
				}
				// Make sure this is last since the TSID is used in the following readTimeSeries() call...
				if (is_template)
				{
					tsid = tsproduct.getLayeredPropValue("TemplateTSID", isub, i, false);
				}
				else
				{
					// Just get the normal property...
					tsid = tsproduct.getLayeredPropValue("TSID", isub, i, false);
				}
				// Make sure we have both or none...
				if ((date1 == null) || (date2 == null))
				{
					date1 = null;
					date2 = null;
				}
				// First try to read the time series using the "TSAlias".  This normally will only return non-null
				// for something like TSTool where the time series may be in memory.
				tsalias = tsproduct.getLayeredPropValue("TSAlias", isub, i, false);
				if ((string.ReferenceEquals(tsid, null)) && (string.ReferenceEquals(tsalias, null)))
				{
					// No more time series in the product file...
					break;
				}
				if (!is_template && (!string.ReferenceEquals(tsalias, null)) && !tsalias.Trim().Equals(""))
				{
					// Have the "TSAlias" property so use it instead of the TSID...
					Message.printStatus(3, routine, "Requesting TS read from TS suppliers using alias \"" + tsalias + "\".");
					try
					{
						ts = readTimeSeries(tsalias.Trim(), date1, date2, null, true);
					}
					catch (Exception)
					{
						// Always add a time series because visual properties are going to be
						// tied to the position of the time series.
						Message.printWarning(3, routine, "Error getting time series \"" + tsalias.Trim() + "\" - setting to null.");
						ts = null;
					}
				}
				else
				{
					// Don't have a "TSAlias" so try to read the time series using the full "TSID"...
					Message.printStatus(2, routine, "Requesting TS read from TS suppliers using TSID \"" + tsid + "\".");
					try
					{
						ts = readTimeSeries(tsid.Trim(), date1, date2, null, true);
					}
					catch (Exception)
					{
						// Always add a time series because visual properties are going to be
						// tied to the position of the time series.
						Message.printWarning(2, routine, "Error getting time series \"" + tsid.Trim() + "\".  Setting to null.");
						ts = null;
					}
					if (ts == null)
					{
						// Logic place-holder
					}
					else if (is_template)
					{
						// Non-null TS.  The TemplateTSID was requested but now the actual TSID needs to be set...
						tsproduct.setPropValue("TSID", ts.getIdentifier().ToString(), isub, i);
					}
				}
				// In any case add the time series, even if null.
				if (ts == null)
				{
					Message.printStatus(2, routine, "Adding null time series for graph.");
				}
				else
				{
					Message.printStatus(2, routine, "Adding time series for graph:  " + ts.getIdentifier() + " period " + ts.getDate1() + " to " + ts.getDate2());
				}
				tslist.Add(ts);
			}
		}

		// Now add the time series to the TSProduct.  This simply provides the time series. They will be looked
		// up as needed when the TSGraph is created.

		tsproduct.setTSList(tslist);

		// Now create the graph.  For now use the PropList associated with the
		// TSProduct.  The use of a frame seems to be necessary to get this to
		// work (tried lots of other things including just declaring a TSGraph),
		// but could not get the combination of Graphics, Image, etc. to work.
		// The following is essentially a serialized TSP file using dot-notation
		PropList tsviewprops = tsproduct.getPropList();
		//Message.printStatus(2, routine, "Graph properties=" + tsviewprops);

		string graph_file = tsproduct.getLayeredPropValue("OutputFile", -1, -1);
		/* TODO SAM 2008-11-19 Don't think this is needed
		if ( graph_file == null ) {
			if ( IOUtil.isUNIXMachine() ) {
				graph_file = "/tmp/tmp.png";
			}
			else {
				graph_file = "C:\\TEMP\\tmp.png";
			}
		}
		*/
		string preview_output = tsproduct.getLayeredPropValue("PreviewOutput", -1, -1);
		try
		{
			// Draw to the image file first in case because the on-screen display throws
			// an exception for missing data and for troubleshooting it would be good to
			// see the image.
			// TODO SAM 2007-06-22 Need to figure out how to combine on-screen
			// drawing with file to do one draw, if possible.
			if ((!string.ReferenceEquals(graph_file, null)) && (graph_file.Length > 0))
			{
				// Create an in memory image and let the TSGraphJComponent draw to it.  Use properties since
				// that was what was done before...
				// Image image = f.createImage(width,height);
				// Image ii = f.createImage(width, height);
				// Make this the same size as the TSGraph defaults and then reset with the properties...
				int width = 400;
				int height = 400;
				prop_value = tsproduct.getLayeredPropValue("TotalWidth", -1, -1);
				if (!string.ReferenceEquals(prop_value, null))
				{
					width = StringUtil.atoi(prop_value);
				}
				prop_value = tsproduct.getLayeredPropValue("TotalHeight", -1, -1);
				if (!string.ReferenceEquals(prop_value, null))
				{
					height = StringUtil.atoi(prop_value);
				}
				BufferedImage image = new BufferedImage(width, height, BufferedImage.TYPE_3BYTE_BGR);

				tsviewprops.set(new Prop("Image", image, ""));
				TSGraphJComponent graph = new TSGraphJComponent(null, tsproduct, tsviewprops);
				if (graph.needToClose())
				{
					throw new Exception("Graph was automatically closed due to data problem.");
				}
				graph.paint(image.getGraphics());
				// Figure out the output file name for the product...
				Message.printStatus(2, routine, "Saving graph to image file \"" + graph_file + "\"");
				graph.saveAsFile(graph_file);
				Message.printStatus(2, "", "Done");
				graph_file = null;
				graph = null;
				image = null;
			}
			// Put the on-screen graph second so that above image can be
			// created first for troubleshooting
			if ((!string.ReferenceEquals(preview_output, null)) && preview_output.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				// Create a TSViewJFrame (an output file can still be created below)...
				TSViewJFrame tsview = new TSViewJFrame(tsproduct);
				if (tsview.needToCloseGraph())
				{
					throw new Exception("Graph was automatically closed due to data problem.");
				}
				__lastTSViewJFrame = tsview;
				// Put this in to test letting TSTool shut down when
				// a single TSView closes (and no main GUI is visible)..
				if (_tsview_window_listener != null)
				{
					tsview.addWindowListener(_tsview_window_listener);
				}
			}
		}
		catch (Exception e)
		{
			Message.printWarning(3, "TSProcessor.processGraphProduct", "Unable to create graph.");
			Message.printWarning(3, "TSProcessor.processGraphProduct", e);
			// Throw a new error...
			throw new Exception("Unable to create graph (" + e + ")", e);
		}
	}

	/// <summary>
	/// Process a time series product file. </summary>
	/// <param name="filename"> Name of time series product file. </param>
	/// <param name="override_props"> Properties to override the properties in the product file
	/// (e.g., to set the period for the plot dynamically). </param>
	/// <exception cref="Exception"> if the product cannot be processed (e.g., the graph cannot
	/// be created due to a lack of data). </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void processProduct(String filename, RTi.Util.IO.PropList override_props) throws Exception
	public virtual void processProduct(string filename, PropList override_props)
	{
		Message.printStatus(2, "", "Processing time series product \"" + filename + "\"");
		TSProduct tsproduct = new TSProduct(filename, override_props);
		processProduct(tsproduct);
	}

	/// <summary>
	/// Process a time series product. </summary>
	/// <param name="tsproduct"> Time series product definition. </param>
	/// <exception cref="Exception"> if the product cannot be processed (e.g., the graph cannot
	/// be created due to a lack of data). </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void processProduct(TSProduct tsproduct) throws Exception
	public virtual void processProduct(TSProduct tsproduct)
	{
		string prop_value = null;
		// Determine whether the product should be processed...
		// New version...
		prop_value = tsproduct.getLayeredPropValue("IsEnabled", -1, -1);
		if (string.ReferenceEquals(prop_value, null))
		{
			// Old version...
			prop_value = tsproduct.getLayeredPropValue("Enabled", -1, -1);
		}
		if ((string.ReferenceEquals(prop_value, null)) || prop_value.Equals("true", StringComparison.OrdinalIgnoreCase))
		{
			// Determine if a graph or report product is being generated...
			prop_value = tsproduct.getLayeredPropValue("ProductType", -1, -1);

			if ((!string.ReferenceEquals(prop_value, null)) && prop_value.Equals("Report", StringComparison.OrdinalIgnoreCase))
			{
				processReportProduct(tsproduct);
			}
			else if ((string.ReferenceEquals(prop_value, null)) || prop_value.Equals("Graph", StringComparison.OrdinalIgnoreCase))
			{ // Default if no product type
				processGraphProduct(tsproduct);
			}
		}
	}

	/// <summary>
	/// Processes a time series product of type "Report" using its given properties.
	/// Each subproduct in the product is processed, and will have an outfile
	/// associated with it, in order to put time series of different interval in
	/// separate files. The only supported ReportType is DateValue. </summary>
	/// <param name="tsproduct"> Time series product. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void processReportProduct(TSProduct tsproduct) throws Exception
	public virtual void processReportProduct(TSProduct tsproduct)
	{
		string routine = "TSProcessor.processReportProduct";
		string tsid;
		string tsalias;
		DateTime date1 = null;
		DateTime date2 = null;
		bool is_template = false;

		// loop through each subproduct and print out the corresponding files 
		int nsubs = tsproduct.getNumSubProducts();
		for (int isub = 0; isub < nsubs; isub++)
		{

			string fname = null;
			string prop_value = null;
			string report_type = null;
			IList<TS> tslist = new List<TS>();

			// get file name for subproduct
			// if there isn't one set then set a temp file name
			fname = tsproduct.getLayeredPropValue("OutputFile", isub, -1);
			if (string.ReferenceEquals(fname, null))
			{
			  if (IOUtil.isUNIXMachine())
			  {
				fname = "/tmp/tmp_report_" + isub;
			  }
			  else
			  {
				  fname = "C:\\TEMP\\tmp_report_" + isub;
			  }
			}

			// Set report type for subproduct
			report_type = tsproduct.getLayeredPropValue("ReportType", isub, -1);

			Message.printStatus(2, routine, "Reading time series for subproduct [" + isub + "]");
			// New...
			prop_value = tsproduct.getLayeredPropValue("IsEnabled", isub, -1);
			// Old...
			if (string.ReferenceEquals(prop_value, null))
			{
				prop_value = tsproduct.getLayeredPropValue("Enabled", isub, -1);
			}
			if ((!string.ReferenceEquals(prop_value, null)) && prop_value.Equals("false", StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}

			// Loop through the time series in the subproduct
			for (int i = 0; ; i++)
			{

				TS ts = null;
				// New version...
				prop_value = tsproduct.getLayeredPropValue("IsEnabled", isub, i);
				// Old version...
				if (string.ReferenceEquals(prop_value, null))
				{
					prop_value = tsproduct.getLayeredPropValue("Enabled", isub, i);
				}
				if ((!string.ReferenceEquals(prop_value, null)) && prop_value.Equals("false", StringComparison.OrdinalIgnoreCase))
				{
					// Add a null time series...
					tslist.Add((TS)null);
					continue;
				}
				prop_value = tsproduct.getLayeredPropValue("PeriodStart", isub, i);
				if (!string.ReferenceEquals(prop_value, null))
				{
					try
					{
						date1 = DateTime.parse(prop_value);
					}
					catch (Exception)
					{
						date1 = null;
					}
				}
				prop_value = tsproduct.getLayeredPropValue("PeriodEnd", isub, i);
				if (!string.ReferenceEquals(prop_value, null))
				{
					try
					{
						date2 = DateTime.parse(prop_value);
					}
					catch (Exception)
					{
						date2 = null;
					}
				}
				// Make sure this is last since the TSID is used in the following readTimeSeries() call...
				if (is_template)
				{
					tsid = tsproduct.getLayeredPropValue("TemplateTSID", isub, i, false);
				}
				else
				{
					// Just get the normal property...
					tsid = tsproduct.getLayeredPropValue("TSID", isub, i, false);
				}
				if (string.ReferenceEquals(tsid, null))
				{
					// No more time series...
					break;
				}
				// Make sure we have both or none...
				if ((date1 == null) || (date2 == null))
				{
					date1 = null;
					date2 = null;
				}
				// First try to read the time series using the
				// "TSAlias".  This normally will only return non-null
				// for something like TSTool where the time series may
				// be in memory.
				tsalias = tsproduct.getLayeredPropValue("TSAlias", isub, i, false);
				if (!is_template && (!string.ReferenceEquals(tsalias, null)) && !tsalias.Trim().Equals(""))
				{
					// Have the property so use the TSAlias instead of the TSID...
					Message.printStatus(2, routine, "Reading TSAlias \"" + tsalias + "\" from TS suppliers.");
					try
					{
						ts = readTimeSeries(tsalias.Trim(), date1, date2, null, true);
					}
					catch (Exception)
					{
						// Always add a time series because visual properties are going to be
						// tied to the position of the time series.
						Message.printWarning(2, routine, "Error getting time series \"" + tsalias.Trim() + "\"");
						ts = null;
					}
				}
				else
				{
					// Don't have a "TSAlias" so try to read the time series using the full "TSID"...
					Message.printStatus(2, routine, "Reading TSID \"" + tsid + "\" from TS suppliers.");
					try
					{
						ts = readTimeSeries(tsid.Trim(), date1, date2, null, true);
					}
					catch (Exception)
					{
						// Always add a time series because visual properties are going to be
						// tied to the position of the time series.
						ts = null;
					}
					if (ts == null)
					{
						Message.printWarning(2, routine, "Error getting time series \"" + tsid.Trim() + "\".  Setting to null.");
					}
					else if (is_template)
					{
						// Non-null TS.  The TemplateTSID was requested but now the actual TSID needs to be set...
						tsproduct.setPropValue("TSID", ts.getIdentifier().ToString(),isub, i);
					}
				}

				tslist.Add(ts);
			}

			// Done adding all time series for that subproduct write output for this subproduct to a file

			if (report_type.Equals("DateValue", StringComparison.OrdinalIgnoreCase))
			{

				DateValueTS.writeTimeSeriesList(tslist, fname);
			}
		}
	}

	/// <summary>
	/// Read a time series using the time series suppliers.  The first supplier to
	/// return a time series is assumed to be the correct supplier. </summary>
	/// <param name="tsident"> TSIdent string indicating the time series to read. </param>
	/// <param name="date1"> Starting date of read, or null. </param>
	/// <param name="date2"> Ending date of read, or null. </param>
	/// <param name="req_units"> Requested units. </param>
	/// <param name="read_data"> Indicates whether to read data (false is header only). </param>
	/// <returns> a time series corresponding to the tsident. </returns>
	/// <exception cref="Exception"> if no time series can be found. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public RTi.TS.TS readTimeSeries(String tsident, RTi.Util.Time.DateTime date1, RTi.Util.Time.DateTime date2, String req_units, boolean read_data) throws Exception
	public virtual TS readTimeSeries(string tsident, DateTime date1, DateTime date2, string req_units, bool read_data)
	{
		string routine = "TSProcessor.readTimeSeries";
		int size = 0;
		if (_suppliers != null)
		{
			size = _suppliers.Length;
		}
		TS ts = null;
		for (int i = 0; i < size; i++)
		{
			string supplier_name = _suppliers[i].getTSSupplierName();
			Message.printStatus(2, routine, "Trying to get \"" + tsident + "\" from TSSupplier \"" + supplier_name + "\"");
			try
			{
				ts = _suppliers[i].readTimeSeries(tsident, date1, date2, (string)null, true);
			}
			catch (Exception e)
			{
				Message.printWarning(2, routine, "Error reading/finding time series for supplier \"" + supplier_name + "\".  Ignoring and trying other suppliers (if available).");
				Message.printWarning(2, routine, e);
				continue;
			}
			if (ts == null)
			{
				Message.printStatus(2, routine, "Did not find TS \"" + tsident + "\" using TSSupplier \"" + supplier_name + "\".  Ignoring and trying other suppliers (if available).");
			}
			else
			{
				// Found a time series so assume it is the one that is needed...
				Message.printStatus(2, routine, "Supplier \"" + supplier_name + "\": found TS \"" + tsident + "\" (alias \"" + ts.getAlias() + "\" period " + ts.getDate1() + " to " + ts.getDate2() + ")");
				return ts;
			}
		}
		throw new Exception("Unable to get time series \"" + tsident + "\" from " + size + " TSSupplier(s).");
	}

	}

}
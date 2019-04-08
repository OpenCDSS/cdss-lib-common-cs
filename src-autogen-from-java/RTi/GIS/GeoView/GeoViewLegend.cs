using System;
using System.Collections.Generic;

// GeoViewLegend - legend for GeoView

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

namespace RTi.GIS.GeoView
{

	using GRAspect = RTi.GR.GRAspect;
	using GRColor = RTi.GR.GRColor;
	using GRDrawingAreaUtil = RTi.GR.GRDrawingAreaUtil;
	using GRJComponentDrawingArea = RTi.GR.GRJComponentDrawingArea;
	using GRLegend = RTi.GR.GRLegend;
	using GRLimits = RTi.GR.GRLimits;
	using GRSymbol = RTi.GR.GRSymbol;
	using GRText = RTi.GR.GRText;
	using GRUnits = RTi.GR.GRUnits;
	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This class is used to manage and draw a legend on the main GeoViewJComponent.
	/// This has been used in the StateMod GUI interface.  However, with the
	/// introduction of the GeoViewJPanel, which has a more standard legend interface, this GeoViewLegend
	/// needs to be updated to work in some type of layout mode for printing.  Leave as
	/// is for now for use with the StateMod GUI until it can be updated.
	/// The data limits are pixels.  The drawing limits are also pixels.
	/// TODO (JTS - 2006-05-23) How to get this legend to display on the GeoViewJPanel?
	/// </summary>
	public class GeoViewLegend
	{

	/// <summary>
	/// Don't draw a legend.
	/// </summary>
	public const int NONE = -1;
	/// <summary>
	/// Draw the legend in the northwest corner.
	/// </summary>
	public const int NWCORNER = 0;
	/// <summary>
	/// Draw the legend in the northeast corner.
	/// </summary>
	public const int NECORNER = 1;
	/// <summary>
	/// Draw the legend in the southwest corner.
	/// </summary>
	public const int SWCORNER = 2;
	/// <summary>
	/// Draw the legend in the southeast corner.
	/// </summary>
	public const int SECORNER = 3;

	protected internal int _location;

	protected internal GRJComponentDrawingArea _legendDA;

	protected internal GeoViewJComponent _view;

	protected internal GRLimits _data_limits = null;

	protected internal bool[] _draw_layer_indicator;
	protected internal int _num_unique_layers;

	/// <summary>
	/// Drawing limits (device) of current canvas
	/// This changes based on user's preference for _location
	/// </summary>
	protected internal GRLimits _draw_limits;

	/// <summary>
	/// Used for big picture plots.
	/// </summary>
	protected internal double _barwidth = 0;
	protected internal double _barheight = 0;
	protected internal double _halfbarwidth = 0;
	protected internal double _min_height = 100;
	protected internal double _min_width = 100;
	protected internal double _max_height = 100;
	protected internal double _max_width = 100;

	public GeoViewLegend(GeoViewJComponent view)
	{
		initialize(view);
	}

	/// <summary>
	/// Draw the symbols on the legend for the appropriate GeoLayerViews.  Symbols for
	/// Some layers are not drawn (e.g., basins). </summary>
	/// <param name="layerViews"> Vector of GeoLayerView that are drawn. </param>
	private void drawSymbols(IList<GeoLayerView> layerViews)
	{
		if (layerViews == null)
		{
			return;
		}

		string rtn = "GeoViewLegend.drawSymbols";

		// always create new legendDA because if resize event has occurred
		// all initializing functionality should be exercised.
		_legendDA = new GRJComponentDrawingArea(_view, "GeoViewLegend", GRAspect.FILL, null, GRUnits.DEVICE, GRLimits.DEVICE, null);
		_draw_limits = getDrawingLimits();
		_legendDA.setDrawingLimits(_draw_limits, GRUnits.DEVICE, GRLimits.DEVICE);
		_legendDA.setDataLimits(_data_limits);

		// set height of big picture rectangle to 50 device units
		GRLimits devlimh = new GRLimits(50.0, 50.0);
		GRLimits datalimh = GRDrawingAreaUtil.getDataExtents(_legendDA, devlimh, 0);
		_barheight = datalimh.getHeight();

		// set width of big picture rectangle to 4 device units
		GRLimits devlimw = new GRLimits(4.0, 4.0);
		GRLimits datalimw = GRDrawingAreaUtil.getDataExtents(_legendDA, devlimw, 0);
		_barwidth = datalimw.getHeight();
		_halfbarwidth = _barwidth / 2.0;

		_legendDA.setDrawingLimits(_draw_limits, GRUnits.DEVICE, GRLimits.DEVICE);
		Message.printStatus(1, rtn, "Legend limits: " + _draw_limits);

		GeoLayerView layerView;
		GRSymbol symbol;
		GRLegend legend;
		double x = 10, y = 90;

		//
		// draw white rectangle under legend area surrounded with black line
		//
		GRDrawingAreaUtil.setFont(_legendDA, "Courier", 10);
		GRDrawingAreaUtil.setColor(_legendDA, GRColor.white);
		GRDrawingAreaUtil.fillRectangle(_legendDA, 1, 1, 99, 99);
		GRDrawingAreaUtil.setColor(_legendDA, GRColor.black);
		GRDrawingAreaUtil.drawRectangle(_legendDA, 1, 1, 99, 99);

		// 
		// draw "Legend" with underline
		//
		GRDrawingAreaUtil.setColor(_legendDA, GRColor.black);
		GRDrawingAreaUtil.drawText(_legendDA, "Legend", 50, y, 0.0, GRText.CENTER_X | GRText.CENTER_Y);
		GRDrawingAreaUtil.drawText(_legendDA, "______", 50, y - 3, 0.0, GRText.CENTER_X | GRText.BOTTOM);
		GRLimits text_size = GRDrawingAreaUtil.getTextExtents(_legendDA, "astring", GRUnits.DATA);
		double text_height = text_size.getHeight() + 3;
		Message.printStatus(1, rtn, "text height after font set: " + text_height);

		//
		// draw symbols
		//
		int num_layers = layerViews.Count;
		for (int i = 0; i < num_layers; i++)
		{
			if ((_draw_layer_indicator == null) || !_draw_layer_indicator[i])
			{
				continue;
			}

			layerView = layerViews[i];
			legend = layerView.getLegend();
			if (legend == null)
			{
				continue;
			}
			symbol = legend.getSymbol();
			if (symbol == null)
			{ // this layer may be river or basin
				continue;
			}

			// check if user type is bigpicture ... this forces the 
			// developer to use "bigpicture" as the layer user type
			if (layerView.getLayer().getAppLayerType().Equals("BigPicture", StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}

			y -= text_height;
			Message.printStatus(1, rtn, "Drawing symbol (" + x + ", " + y + ") for " + layerView.getLayer().getAppLayerType());

			GRDrawingAreaUtil.setColor(_legendDA, symbol.getColor());
			GRDrawingAreaUtil.drawSymbolText(_legendDA, symbol.getStyle(), x, y, symbol.getSize(), layerView.getLayer().getAppLayerType(), 0.0, GRText.LEFT | GRText.CENTER_Y, GRUnits.DEVICE, GRSymbol.SYM_CENTER_X | GRSymbol.SYM_CENTER_Y);
		}

		if (_view.getBigPictureActive())
		{
			y = 50;
			x = 60;
			double[] xp = new double[2]; double[] yp = new double[2];
			xp[0] = x - _halfbarwidth;
			xp[1] = x + _barwidth + _halfbarwidth;
			yp[0] = y;
			yp[1] = y;
			GRDrawingAreaUtil.setColor(_legendDA, GRColor.black);
			GRDrawingAreaUtil.drawLine(_legendDA, xp, yp);

			GRDrawingAreaUtil.setColor(_legendDA, GRColor.blue);
			GRDrawingAreaUtil.fillRectangle(_legendDA, x, y, _barwidth, _barheight);

			GRDrawingAreaUtil.setColor(_legendDA, GRColor.red);
			GRDrawingAreaUtil.fillRectangle(_legendDA, x, y - _barheight, _barwidth, _barheight);

			GRDrawingAreaUtil.setColor(_legendDA, GRColor.black);
			GRDrawingAreaUtil.drawText(_legendDA, "0", x + (2 * _barwidth), y, 0.0, GRText.LEFT | GRText.CENTER_Y);
			string ztopString = Convert.ToString(_view.getBigPictureZMax());
			GRDrawingAreaUtil.drawText(_legendDA, ztopString, x + (2 * _barwidth), y + _barheight, 0.0, GRText.LEFT | GRText.TOP);
			string zbotString = "-" + ztopString;
			GRDrawingAreaUtil.drawText(_legendDA, zbotString, x + (2 * _barwidth), y - _barheight, 0.0, GRText.LEFT | GRText.BOTTOM);
			ztopString = null;
			zbotString = null;
			xp = null;
			yp = null;
		}
		_draw_limits = getDrawingLimits();
		_legendDA.setDrawingLimits(_draw_limits, GRUnits.DEVICE, GRLimits.DEVICE);
		Message.printStatus(1, rtn, "Legend drawing limits: " + _draw_limits.ToString());
	}

	/// <summary>
	/// Determine which layers should have an entry in the legend. </summary>
	/// <param name="layerViews"> Vector of GeoLayerView that are being drawn. </param>
	/// <returns> array of flags indicating whether the layer views should be in the
	/// legend.  Return null if the layer views Vector is null. </returns>
	private bool[] establishDrawLayerIndicator(IList<GeoLayerView> layerViews)
	{
		if (layerViews == null)
		{
			return null;
		}
		GRLegend legend;
		GeoLayerView layerView;
		GRSymbol symbol;
		string description;
		int num_layers = layerViews.Count;
		//int _num_unique_layers = 0;
		bool[] layer_indicator = new bool[num_layers];
		string[] layer_desc = new string[num_layers];
		GRSymbol[] layer_symbol = new GRSymbol[num_layers];
		for (int i = 0; i < num_layers; i++)
		{
			layerView = layerViews[i];
			legend = layerView.getLegend();
			if (legend == null)
			{
				layer_indicator[i] = false;
				continue;
			}
			symbol = legend.getSymbol();
			if (symbol == null)
			{ // this layer may be river or basin
				layer_indicator[i] = false;
				continue;
			}

			// check if user type is bigpicture ... this forces the 
			// developer to user "bigpicture" as the layer user type
			description = layerView.getLayer().getAppLayerType();
			if (description.Equals("BigPicture", StringComparison.OrdinalIgnoreCase))
			{
				layer_indicator[i] = false;
				continue;
			}

			layer_symbol[i] = symbol;
			layer_desc[i] = description;

			// check that layer is unique
			int symbolType = symbol.getType();
			layer_indicator[i] = true;
			for (int j = 0; j < i; j++)
			{
				if ((layer_symbol[j].getType() == symbolType) && layer_desc[j].Equals(description))
				{
					layer_indicator[i] = false;
					continue;
				}
			}
			if (layer_indicator[i])
			{
				_num_unique_layers++;
			}
		}
		return layer_indicator;
	}

	/// <summary>
	/// Clean up for garbage collection. </summary>
	/// <exception cref="Throwable"> if there is an error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~GeoViewLegend()
	{
		_legendDA = null;
		_view = null;
		_data_limits = null;
		_draw_layer_indicator = null;
		_draw_limits = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Determine the drawing limits to use for the drawing area.  A simple percentage
	/// of the main GeoViewJComponent drawing area is used. </summary>
	/// <returns> the drawing limits to use for the legend. </returns>
	private GRLimits getDrawingLimits()
	{
		GRLimits new_draw_limits = new GRLimits(_view.getDrawingArea().getDrawingLimits());

		// Use %-age of the drawing area for the legend

		double new_height = new_draw_limits.getHeight() * .40;
		double new_width;
		if (_view.getBigPictureActive())
		{
			new_width = new_draw_limits.getWidth() * .35;
		}
		else
		{
			new_width = new_draw_limits.getWidth() * .20;
		}

		// Verify that new_height and new_width are not outside range
		if (new_height < _min_height)
		{
			new_height = _min_height;
		}
		if (new_height > _max_height)
		{
			new_height = _max_height;
		}

		if (new_width < _min_width)
		{
			new_width = _min_width;
		}
		if (new_width > _max_width)
		{
			new_width = _max_width;
		}

		if (_location == NWCORNER)
		{
			new_draw_limits.setRightX(new_draw_limits.getLeftX() + new_width);
			new_draw_limits.setBottomY(new_draw_limits.getTopY() - new_height);
		}
		else if (_location == NECORNER)
		{
			new_draw_limits.setLeftX(new_draw_limits.getRightX() - new_width);
			new_draw_limits.setBottomY(new_draw_limits.getTopY() - new_height);
		}
		else if (_location == SWCORNER)
		{
			new_draw_limits.setRightX(new_draw_limits.getLeftX() + new_width);
			new_draw_limits.setTopY(new_draw_limits.getBottomY() + new_height);
		}
		else if (_location == SECORNER)
		{
			new_draw_limits.setLeftX(new_draw_limits.getRightX() - new_width);
			new_draw_limits.setTopY(new_draw_limits.getBottomY() + new_height);
		}

		return new_draw_limits;
	}

	/// <summary>
	/// Initialize member data. </summary>
	/// <param name="view"> GeoViewJComponent that the legend is drawn in. </param>
	private void initialize(GeoViewJComponent view)
	{
		_view = view;
		_location = -1;
		_legendDA = null;
		_data_limits = new GRLimits(0, 0, 100, 100);
	}

	/// <summary>
	/// Paint the legend drawing area.  This should be called after the main
	/// GeoViewJComponent contents are drawn. </summary>
	/// <param name="g"> Graphics to use for drawing. </param>
	public virtual void paint(Graphics g)
	{
		_draw_layer_indicator = establishDrawLayerIndicator(_view.getLayerViews());
		drawSymbols(_view.getLayerViews());
	}

	/// <summary>
	/// Set the location for the legend (see NWCORNER, etc.). </summary>
	/// <param name="location"> Location for the legend. </param>
	public virtual void setLocation(int location)
	{
		_location = location;
	}

	/// <summary>
	/// Set the maximum height of the legend, in pixels. </summary>
	/// <param name="max_height"> maximum height of the legend, in pixels. </param>
	public virtual void setMaxHeight(double max_height)
	{
		_max_height = max_height;
	}

	/// <summary>
	/// Set the maximum width of the legend, in pixels. </summary>
	/// <param name="max_width"> maximum width of the legend, in pixels. </param>
	public virtual void setMaxWidth(double max_width)
	{
		_max_width = max_width;
	}

	/// <summary>
	/// Set the minimum height of the legend, in pixels. </summary>
	/// <param name="min_height"> minimum height of the legend, in pixels. </param>
	public virtual void setMinHeight(double min_height)
	{
		_min_height = min_height;
	}

	/// <summary>
	/// Set the minimum width of the legend, in pixels. </summary>
	/// <param name="min_width"> minimum width of the legend, in pixels. </param>
	public virtual void setMinWidth(double min_width)
	{
		_min_width = min_width;
	}

	}

}
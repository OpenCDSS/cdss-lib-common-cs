using System;

// GeoLayerViewLegendJComponent - class to hold GeoLayerView legend component

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

// ----------------------------------------------------------------------------
// GeoLayerViewLegendJComponent.java
// ----------------------------------------------------------------------------
// History:
//
// 2001-09-27	Steven A. Malers, RTi	Allow an integer to be passed into the
//					constructor so that classification
//					legends can be drawn.
// 2001-10-17	SAM, RTi		Make sure to set fields to null when
//					done to help with garbage collection.
// 2002-01-08	SAM, RTi		Change name of GeoLayerViewLegendCanvas
//					to GeoLayerViewLegendJComponent.
//					Change GRJava* to GRJComponent*.
// 2002-07-23	SAM, RTi		Update GRSymbol methods from
//					"pointSymbol" to "style".
// ----------------------------------------------------------------------------
// 2003-05-06	J. Thomas Sapienza, RTi	* Updated code to match the non-Swing
//					  version.
//					* Got the double-buffering to stop
//					  causing exceptions.
// 2003-05-08	JTS, RTi		Minor changes to add in some more 
//					non-Swing code.
// 2003-05-09	JTS, RTi		Polygon symbols now draw the far-right
//					outline, as well as the others.
// 2004-08-10	JTS, RTi		Added support for teacups.
// 2004-10-06	JTS, RTi		Added support for unsigned vertical bar
//					symbols.
// 2004-10-13	JTS, RTi		Added:
//					* getClassification()
//					* getIsym()
//					* getLayerView()
//					* getDrawLimits()
//					* getLegendText()
//					* setLegendText()
// 2004-10-19	JTS, RTi		Shortened the height of the canvas used
//					for drawing unsigned bars.
// 2005-04-27	JTS, RTi		Added all member variables to finalize()
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.GIS.GeoView
{

	using GRAspect = RTi.GR.GRAspect;
	using GRColor = RTi.GR.GRColor;
	using GRDrawingAreaUtil = RTi.GR.GRDrawingAreaUtil;
	using GRJComponentDevice = RTi.GR.GRJComponentDevice;
	using GRJComponentDrawingArea = RTi.GR.GRJComponentDrawingArea;
	using GRLimits = RTi.GR.GRLimits;
	using GRSymbol = RTi.GR.GRSymbol;
	using GRUnits = RTi.GR.GRUnits;

	using PropList = RTi.Util.IO.PropList;

	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// JComponent to display the symbol for a GeoLayerView.  The JComponent is a GRJComponentDevice.
	/// It is a separate class so that the paint() method can accurately capture the correct Graphics object.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class GeoLayerViewLegendJComponent extends RTi.GR.GRJComponentDevice
	public class GeoLayerViewLegendJComponent : GRJComponentDevice
	{

	/// <summary>
	/// The drawing limits associated with this component.
	/// </summary>
	private GRLimits __drawLimits;

	private GeoLayerView _layer_view = null;
	private GRJComponentDrawingArea _layer_grda = null;
	/// <summary>
	/// Classification for legend canvas.
	/// </summary>
	private int _class = 0;
	/// <summary>
	/// Symbol for layer (starting at zero).
	/// </summary>
	private int _isym = 0;

	/// <summary>
	/// The text that appears next to this component in the legend.
	/// The text is not actually drawn by this class.
	/// </summary>
	private string __legendText = null;

	/// <summary>
	/// Construct an instance for a given GeoLayerView. </summary>
	/// <param name="layer_view"> GeoLayerView for legend panel. </param>
	/// <param name="isym"> the symbol that is being drawn for the layer view (zero-index). </param>
	/// <param name="classification"> Class for legend (zero-index).  Ignored for single
	/// classification.  This is the position in the color table. </param>
	public GeoLayerViewLegendJComponent(GeoLayerView layer_view, int isym, int classification) : base("")
	{
		_class = classification;
		_layer_view = layer_view;
		_doubleBuffered = true;
		_isym = isym;

		// For now always set to the width to 20 until we figure out a more
		// complex way to get the width from the panel.
		double sym_size = 20.0; // Default
		double sym_size_x = 0.0, sym_size_y = 0.0;
		GRSymbol sym = _layer_view.getLegend().getSymbol(_isym);
		if (sym != null)
		{
			if (sym.getSize() > sym_size)
			{
				sym_size = sym.getSize();
			}
		}
		GRLimits draw_limits;
		if (sym.getStyle() == GRSymbol.SYM_VBARSIGNED)
		{
			// Make the symbol be as large as the twice the vertical height
			// so positive and negative values can be shown to the full scale...
			sym_size_x = sym.getSizeX();
			if (sym_size_x < 5.0)
			{
				sym_size_x = 5.0;
			}
			sym_size_y = sym.getSizeY();
			if (sym_size_y < 20.0)
			{
				sym_size_y = 20.0;
			}
			setSize(new Dimension((int)sym_size_x, (int)(sym_size_y * 2.0)));
				draw_limits = new GRLimits(0.0, 0.0, sym_size_x, sym_size_y * 2.0);
			setPreferredSize(new Dimension((int)sym_size_x, (int)(sym_size_y * 2.0)));
				draw_limits = new GRLimits(0.0, 0.0, sym_size_x, sym_size_y * 2.0);
			setMinimumSize(new Dimension((int)sym_size_x, (int)(sym_size_y * 2.0)));
				draw_limits = new GRLimits(0.0, 0.0, sym_size_x, sym_size_y * 2.0);
			setupDoubleBuffer(0, 0, (int)sym_size, (int)(sym_size * 2));
		}
		else if (sym.getStyle() == GRSymbol.SYM_VBARUNSIGNED)
		{
			// the symbol is only as high as the height of the bar -- 
			// it is not doubled because no negative data will be shown.
			sym_size_x = sym.getSizeX();
			if (sym_size_x < 5.0)
			{
				sym_size_x = 5.0;
			}
			sym_size_y = sym.getSizeY();
			if (sym_size_y < 20.0)
			{
				sym_size_y = 20.0;
			}
			setSize(new Dimension((int)sym_size_x, (int)(sym_size_y)));
				draw_limits = new GRLimits(0.0, 0.0, sym_size_x, sym_size_y);
			setPreferredSize(new Dimension((int)sym_size_x, (int)(sym_size_y)));
				draw_limits = new GRLimits(0.0, 0.0, sym_size_x, sym_size_y);
			setMinimumSize(new Dimension((int)sym_size_x, (int)(sym_size_y)));
				draw_limits = new GRLimits(0.0, 0.0, sym_size_x, sym_size_y);
			setupDoubleBuffer(0, 0, (int)sym_size, (int)(sym_size));
		}
		else if (sym.getStyle() == GRSymbol.SYM_TEACUP)
		{
			sym_size = 20;
			setSize(new Dimension((int)sym_size,(int)sym_size));
			setPreferredSize(new Dimension((int)sym_size,(int)sym_size));
			setMinimumSize(new Dimension((int)sym_size,(int)sym_size));
			draw_limits = new GRLimits(0.0, 0.0, sym_size, sym_size);
			setupDoubleBuffer(0, 0, (int)sym_size, (int)(sym_size));
		}
		else
		{
			setSize(new Dimension((int)sym_size,(int)sym_size));
			setPreferredSize(new Dimension((int)sym_size,(int)sym_size));
			setMinimumSize(new Dimension((int)sym_size,(int)sym_size));
			draw_limits = new GRLimits(0.0, 0.0, sym_size, sym_size);
			setupDoubleBuffer(0, 0, (int)sym_size, (int)(sym_size));
		}
		sym = null;
		GRLimits data_limits = new GRLimits(0.0, 0.0, 1.0, 1.0);
		// May need to change aspect to FILL to get position right...
		_layer_grda = new GRJComponentDrawingArea(this, "", GRAspect.TRUE, draw_limits, GRUnits.DEVICE, GRLimits.DEVICE, data_limits);
		__drawLimits = draw_limits;
		draw_limits = null;
		data_limits = null;

	}

	private static double[] __teacupData = new double[3];
	private static PropList __props = new PropList("");

	/// <summary>
	/// Draw the symbol for the legend item.
	/// </summary>
	private void drawSymbol()
	{
		if ((_layer_grda == null) || (getGraphics() == null))
		{
			return;
		}
		// Set the drawing area size to the current size of the canvas...
		GRLimits limits = new GRLimits(getLimits(true));
		_layer_grda.setDrawingLimits(limits, GRUnits.DEVICE, GRLimits.DEVICE);
		// Set data limits to the drawing limits.  That way we can make sure
		// that the symbol position can be controlled...
		_layer_grda.setDataLimits(limits);
		// Fill the full drawing area with the background color.  Can only do
		// after the canvas and drawing area are fully defined...
		GRSymbol sym = null;
		GRColor color = null;
		bool dodraw = true;
		try
		{ // Draw the symbol, depending on the layer data shape type...
			int layerType = _layer_view.getLayer().getShapeType();
			sym = _layer_view.getLegend().getSymbol(_isym);
			if (sym == null)
			{
				return;
			}
			if (sym.getClassificationType() == GRSymbol.CLASSIFICATION_SINGLE)
			{
				color = sym.getColor();
			}
			else if (sym.getClassificationType() == GRSymbol.CLASSIFICATION_SCALED_SYMBOL)
			{
				color = sym.getColor();
			}
			else if (sym.getClassificationType() == GRSymbol.CLASSIFICATION_SCALED_TEACUP_SYMBOL)
			{
				color = sym.getColor();
			}
			else
			{
				color = sym.getClassificationColor(_class);
			}
			if ((color == null) || color.isTransparent())
			{
				// No need to draw the symbol but may have an outline...
				dodraw = false;
			}
			else
			{
				GRDrawingAreaUtil.setColor(_layer_grda, color);
			}
			if (dodraw && ((layerType == GeoLayer.POINT) || (layerType == GeoLayer.POINT_ZM) || (layerType == GeoLayer.MULTIPOINT)))
			{
				if (sym.getClassificationType() == GRSymbol.CLASSIFICATION_SCALED_SYMBOL)
				{
					if (sym.getStyle() == GRSymbol.SYM_VBARSIGNED)
					{
						// Draw the symbol twice, once with a positive value in the first color
						// and once with a negative value in the second color.
						color = sym.getColor();
						double[] sym_data = new double[1];
						sym_data[0] = 1.0;
						GRDrawingAreaUtil.drawSymbol(_layer_grda, sym.getStyle(), (limits.getLeftX() + sym.getSize() * .5 + 1.0), limits.getCenterY(), sym.getSizeX(), sym.getSizeY(), 0.0, 0.0, sym_data, GRUnits.DEVICE, GRSymbol.SYM_CENTER_X | GRSymbol.SYM_CENTER_Y);
						color = sym.getColor2();
						if (color != null)
						{
							GRDrawingAreaUtil.setColor(_layer_grda, color);
						}
						sym_data[0] = -1.0;
						GRDrawingAreaUtil.drawSymbol(_layer_grda, sym.getStyle(), (limits.getLeftX() + sym.getSize() * .5 + 1.0), limits.getCenterY(), sym.getSizeX(), sym.getSizeY(), 0.0, 0.0, sym_data, GRUnits.DEVICE, GRSymbol.SYM_CENTER_X | GRSymbol.SYM_CENTER_Y);
					}
					else if (sym.getStyle() == GRSymbol.SYM_VBARUNSIGNED)
					{
						// since unsigned bars only show positive values only get the 
						// first color to use to draw the bar.
						color = sym.getColor();
						double[] sym_data = new double[1];
						sym_data[0] = 1.0;
						GRDrawingAreaUtil.drawSymbol(_layer_grda, sym.getStyle(), (limits.getLeftX() + sym.getSize() * .5 + 1.0), limits.getBottomY(), sym.getSizeX(), sym.getSizeY(), 0.0, 0.0, sym_data, GRUnits.DEVICE, GRSymbol.SYM_CENTER_X | GRSymbol.SYM_BOTTOM);
					}
				}
				else
				{ // A simple symbol...
				/*
					GRDrawingAreaUtil.drawSymbol(
						_layer_grda,
						sym.getStyle(), 
						(limits.getLeftX() + 
							sym.getSize()*.5 + 1.0),
						limits.getCenterY(), 
						sym.getSize(),
						GRUnits.DEVICE,
						GRSymbol.SYM_CENTER_X|
						GRSymbol.SYM_CENTER_Y );
				*/
				double size = sym.getSize();
				if (sym.getClassificationType() == GRSymbol.CLASSIFICATION_SCALED_TEACUP_SYMBOL)
				{
					size = 15;
					GRDrawingAreaUtil.setColor(_layer_grda, GRColor.blue);
					__teacupData[0] = 20;
					__teacupData[1] = 0;
					__teacupData[2] = 14;
					GRDrawingAreaUtil.drawSymbol(_layer_grda, sym.getStyle(), (limits.getLeftX() + size * .5 + 1.0), limits.getCenterY(), size, size, 0.0, 0.0, __teacupData, GRUnits.DEVICE, GRSymbol.SYM_CENTER_X | GRSymbol.SYM_CENTER_Y, __props);
				}
				else
				{
					GRDrawingAreaUtil.drawSymbol(_layer_grda, sym.getStyle(), (limits.getLeftX() + size * .5 + 1.0), limits.getCenterY(), size, size, 0.0, 0.0, null, GRUnits.DEVICE, GRSymbol.SYM_CENTER_X | GRSymbol.SYM_CENTER_Y);
				}
				}
			}
			else if (dodraw && ((layerType == GeoLayer.LINE) || (layerType == GeoLayer.POLYLINE_ZM)))
			{
				// Later need to add a standard GRSymbol for this shape but draw manually for now.
				double[] x = new double[4];
				double[] y = new double[4];
				x[0] = limits.getLeftX();
				x[1] = x[0] + 6.0;
				x[2] = x[1] + 3.0;
				x[3] = x[2] + 6.0;
				y[0] = limits.getBottomY();
				y[1] = limits.getHeight() * .6;
				y[2] = limits.getHeight() * .4;
				y[3] = limits.getTopY();
				GRDrawingAreaUtil.drawPolyline(_layer_grda, 4, x, y);
			}
			else if ((layerType == GeoLayer.POLYGON) || (layerType == GeoLayer.GRID))
			{
				// First fill in the box...
				if (dodraw)
				{
					GRDrawingAreaUtil.fillRectangle(_layer_grda, limits.getLeftX(), limits.getBottomY(), limits.getHeight(), limits.getHeight());
				}
				// Now draw the outline...
				GRColor outline_color = sym.getOutlineColor();
				if ((outline_color != null) && !outline_color.isTransparent())
				{
					GRDrawingAreaUtil.setColor(_layer_grda, outline_color);
					GRDrawingAreaUtil.drawRectangle(_layer_grda, limits.getLeftX(), (limits.getBottomY() + 1.0), limits.getHeight() - 1.0, (limits.getHeight() - 1.0));
				}
				outline_color = null;
			}
			// Else currently not supported....
		}
		catch (Exception e)
		{
			// May throw exception if not initialized yet.  This is OK
			// as exception will not occur after initialization.
			Message.printWarning(2, "", e);
		}
		showDoubleBuffer();
	}

	/// <summary>
	/// Clean up for garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~GeoLayerViewLegendJComponent()
	{
		_layer_view = null;
		_layer_grda = null;
		__drawLimits = null;
		__legendText = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the classification type for the legend. </summary>
	/// <returns> the classification type for the legend. </returns>
	public virtual int getClassification()
	{
		return _class;
	}

	/// <summary>
	/// Returns this component's drawing limits. </summary>
	/// <returns> this component's drawing limits. </returns>
	public virtual GRLimits getDrawLimits()
	{
		return __drawLimits;
	}

	/// <summary>
	/// Returns the number of this symbol. </summary>
	/// <returns> the number of this symbol. </returns>
	public virtual int getIsym()
	{
		return _isym;
	}

	/// <summary>
	/// Returns the text associated with this component in the legend.
	/// Returns the text associated with this component in the legend.
	/// </summary>
	public virtual string getLegendText()
	{
		return __legendText;
	}

	/// <summary>
	/// Returns the layer view associated with this component. </summary>
	/// <returns> the layer view associated with this component. </returns>
	public virtual GeoLayerView getLayerView()
	{
		return _layer_view;
	}

	/// <summary>
	/// Paint the panel.  Most of the work is done automatically but intercept the
	/// paint() call so we can redraw the symbols.
	/// </summary>
	public virtual void paint(Graphics g)
	{ // The graphics is only passed in on a redraw but the graphics is needed by the canvas...
		setPaintGraphics(g);
		// Make sure to redraw the symbol.
		drawSymbol();
	}

	/// <summary>
	/// Sets the text associated with this component's layer in the legend. </summary>
	/// <param name="text"> the text to associate with this component in the legend. </param>
	public virtual void setLegendText(string text)
	{
		__legendText = text;
	}

	}

}
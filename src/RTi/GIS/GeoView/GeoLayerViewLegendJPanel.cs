using System;

// GeoLayerViewLegendJPanel - panel to hold layer checkbox, select button, and symbol canvas

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
// GeoLayerViewLegendJPanel - panel to hold layer checkbox, select button, and
//				symbol canvas
// ----------------------------------------------------------------------------
// History:
//
// 2001-10-09	Steven A. Malers, RTi	Overload constructor to allow legend to
//					be drawn in passive mode for the
//					GeoViewPropertiesGUI.
// 2001-10-12	SAM, RTi		Update constructor to pass in
//					GeoViewLegendPanel so that the legend
//					panel states can be checked when an
//					individual layer view setting changes.
// 2001-10-15	SAM, RTi		Set unused data to null to help with
//					garbage collection.
// 2001-12-04	SAM, RTi		Update to Swing.
// 2002-01-08	SAM, RTi		Change GeoLayerViewLegendCanvas to
//					GeoLayerViewLegendJComponent.
// ----------------------------------------------------------------------------
// 2003-05-06	J. Thomas Sapienza, RTi	Brought code up to date with the
//					non-Swing code.
// 2003-05-14	JTS, RTi		Removed font-setting code.
// 2004-11-11	JTS, RTi		In legends with class breaks, the
//					class break text background is now
//					set to match the color of the tree.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.GIS.GeoView
{



	using GRScaledClassificationSymbol = RTi.GR.GRScaledClassificationSymbol;
	using GRSymbol = RTi.GR.GRSymbol;

	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using SimpleJButton = RTi.Util.GUI.SimpleJButton;

	using IOUtil = RTi.Util.IO.IOUtil;

	using StringUtil = RTi.Util.String.StringUtil;

	using DataTable = RTi.Util.Table.DataTable;

	/// <summary>
	/// JPanel to display a legend for a GeoLayerView.  This panel includes a checkbox
	/// to enable/disable the GeoLayerView, the name associated with the GeoLayerView,
	/// and, a canvas (GRDevice) to display the symbology, and a label next to the
	/// canvas to indicate the symbol classification field.  If the multiple classes are
	/// used, then multiple canvases and labels are used.
	/// </summary>

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class GeoLayerViewLegendJPanel extends javax.swing.JPanel implements java.awt.event.ActionListener, java.awt.event.ItemListener, java.awt.event.MouseListener
	public class GeoLayerViewLegendJPanel : JPanel, ActionListener, ItemListener, MouseListener
	{

	private GeoViewLegendJTree _parent = null;
	private GeoLayerView _layer_view = null;
	private JCheckBox _enabled_JCheckBox = null;
	private SimpleJButton _layer_JButton = null;
	private JLabel _layer_JLabel = null;
	private GeoLayerViewLegendJComponent[] _layer_Canvas = null;
	private JLabel[] _layer_class_JLabel = null;
	private bool _ctrl_pressed; // Used to indicate whether a shift or
						// control key is pressed when the
						// button is pressed.  This is only used
						// in mousePressed() and mouseReleased()

	/// <summary>
	/// Construct a legend panel instance for a given GeoLayerView.  The checkbox and
	/// select button are included by default. </summary>
	/// <param name="parent"> GeoViewLegendJTree parent or null. </param>
	/// <param name="layer_view"> GeoLayerView for legend panel. </param>
	public GeoLayerViewLegendJPanel(GeoViewLegendJTree parent, GeoLayerView layer_view) : this(parent, layer_view, true)
	{
	}

	/// <summary>
	/// Construct a legend panel instance for a given GeoLayerView, optionally
	/// not displaying controls (and including only the symbol and layer view label).
	/// This is typically used in the layer view properties interface where only
	/// the canvas and related labels are needed. </summary>
	/// <param name="layer_view"> GeoLayerView for legend panel. </param>
	/// <param name="include_controls"> If true, then the JCheckBox and select Button are added.
	/// If false, a Label is used in place of the JCheckBox and Button. </param>
	public GeoLayerViewLegendJPanel(GeoLayerView layer_view, bool include_controls) : this(null, layer_view, include_controls)
	{
	}

	/// <summary>
	/// Construct a legend panel instance for a given GeoLayerView. </summary>
	/// <param name="parent"> GeoViewLegendPanel parent or null. </param>
	/// <param name="layer_view"> GeoLayerView for legend panel. </param>
	/// <param name="include_controls"> If true, then the Checkbox and select Button are added.
	/// If false, a Label is added is used instead of the Checkbox and Button. </param>
	public GeoLayerViewLegendJPanel(GeoViewLegendJTree parent, GeoLayerView layer_view, bool include_controls)
	{
		_layer_view = layer_view;
		_parent = parent;
		setLayout(new GridBagLayout());

		Insets insets_none = new Insets(1, 1, 1, 1);
		int y = 0;
		if (include_controls)
		{
			// Do not specify a label because doing so enables toggling by
			// selecting on the label.  The label should be inert.
			_enabled_JCheckBox = new JCheckBox();
			_enabled_JCheckBox.setSelected(layer_view.isVisible());
			_enabled_JCheckBox.addItemListener(this);
			JGUIUtil.addComponent(this, _enabled_JCheckBox, 0, y, 1, 1, 0, 0, insets_none, GridBagConstraints.NONE, GridBagConstraints.NORTH);
			_layer_JButton = new SimpleJButton(layer_view.getLegend().getText(), layer_view.getLegend().getText(), this);
			JGUIUtil.addComponent(this, _layer_JButton, 1, y, 2, 1, 1, 0, insets_none, GridBagConstraints.HORIZONTAL, GridBagConstraints.NORTH);
		}
		else
		{
			_layer_JLabel = new JLabel(layer_view.getLegend().getText());
			JGUIUtil.addComponent(this, _layer_JLabel, 0, y, 3, 1, 1, 0, insets_none, GridBagConstraints.HORIZONTAL, GridBagConstraints.NORTH);
		}

		// Now draw the symbol(s)...
		// Get the number of symbols for the layer view.  First need to
		// determine the layer view number, which is stored in the "Number"
		// property for the layerview.  Currently this is supported only for
		// CLASSIFICATION_SINGLE and CLASSIFICATION_SCALED_SYMBOL.

		int nsymbol = layer_view.getLegend().size();
		GRSymbol symbol = null;
		for (int isym = 0; isym < nsymbol; isym++)
		{
			symbol = layer_view.getLegend().getSymbol(isym);
			if (symbol.getClassificationType() == GRSymbol.CLASSIFICATION_SINGLE)
			{
				if (isym == 0)
				{
					// For now assume that symbol types will not be mixed for a layer...
					_layer_Canvas = new GeoLayerViewLegendJComponent[nsymbol];
					_layer_class_JLabel = new JLabel[nsymbol];
				}
				_layer_Canvas[isym] = new GeoLayerViewLegendJComponent(_layer_view, isym, 0);
				JGUIUtil.addComponent(this, _layer_Canvas[isym], 1, ++y, 1, 1, 0, 0, insets_none, GridBagConstraints.NONE, GridBagConstraints.SOUTH);
				// Add a label to keep spacing consistent...
				_layer_class_JLabel[isym] = new JLabel("");
				JGUIUtil.addComponent(this, _layer_class_JLabel[isym], 2, y, 1, 1, 1, 0, insets_none, GridBagConstraints.HORIZONTAL, GridBagConstraints.SOUTH);
			}
			else if (symbol.getClassificationType() == GRSymbol.CLASSIFICATION_SCALED_SYMBOL)
			{
				// This is currently enabled only for vertical signed
				// bars where the bar is centered vertically on the
				// point, positive values are drawn with the main
				// foreground color and negative values are drawn with
				// the secondary foreground color.
				if (isym == 0)
				{
					// For now assume that symbol types will not
					// be mixed for a layer...
					_layer_Canvas = new GeoLayerViewLegendJComponent[nsymbol];
					_layer_class_JLabel = new JLabel[nsymbol];
				}
				_layer_Canvas[isym] = new GeoLayerViewLegendJComponent(_layer_view, isym, 0);
				JGUIUtil.addComponent(this, _layer_Canvas[isym], 1, ++y, 1, 1, 0, 0, insets_none, GridBagConstraints.NONE, GridBagConstraints.SOUTH);
				if (!symbol.getClassificationField().Equals(""))
				{
					// Get the maximum value for the symbol, which
					// is used to scale the symbol...
					// SAMX - need to streamline this - store with
					// symbol at creation?
					DataTable attribute_table = _layer_view.getLayer().getAttributeTable();
					int classification_field = -1;
					string cf = symbol.getClassificationField();
					if (attribute_table != null)
					{
						try
						{
							classification_field = attribute_table.getFieldIndex(cf);
						}
						catch (Exception)
						{
							// Just won't label below.
							classification_field = -1;
						}
					}
					//Message.printStatus ( 1, "",
					//"SAMX classification field = " +
					//classification_field + " \"" + cf + "\"" );
					// Message.printStatus ( 1, "",
					// "SAMX geoview panel = " + geoview_Panel );
					if ((classification_field >= 0))
					{
						double symbol_max = ((GRScaledClassificationSymbol) symbol).getClassificationDataDisplayMax();
						// Do this to keep legend a reasonable width...
						if (cf.Length > 20)
						{
							cf = cf.Substring(0,20) + "...";
						}
						_layer_class_JLabel[isym] = new JLabel(cf + ", Max = " + StringUtil.formatString(symbol_max,"%.3f"));
					}
					else
					{
						if (cf.Length > 20)
						{
							cf = cf.Substring(0,20) + "...";
						}
						_layer_class_JLabel[isym] = new JLabel(cf);
					}
				}
				else
				{
					// Add a label with the field and maximum value...
					_layer_class_JLabel[isym] = new JLabel("");
				}
				JGUIUtil.addComponent(this, _layer_class_JLabel[isym], 2, y, 1, 1, 1, 0, insets_none, GridBagConstraints.HORIZONTAL, GridBagConstraints.NORTH);
			}
			else
			{
				// Multiple legend items need to be drawn...
				int numclass = symbol.getNumberOfClassifications();
				_layer_Canvas = new GeoLayerViewLegendJComponent[numclass];
				_layer_class_JLabel = new JLabel[numclass];
				for (int i = 0; i < numclass; i++)
				{
					_layer_Canvas[i] = new GeoLayerViewLegendJComponent(_layer_view, isym, i);
					JGUIUtil.addComponent(this, _layer_Canvas[i], 1, ++y, 1, 1, 0, 0, insets_none, GridBagConstraints.NONE, GridBagConstraints.SOUTH);
					// Add a label for the classification...
					_layer_class_JLabel[i] = new JLabel(symbol.getClassificationLabel(i));
					if (_parent != null)
					{
						_layer_class_JLabel[i].setBackground(_parent.getBackground());
					}
					JGUIUtil.addComponent(this, _layer_class_JLabel[i], 2, y, 1, 1, 1, 0, insets_none, GridBagConstraints.HORIZONTAL, GridBagConstraints.SOUTH);
				}
				if (_parent != null)
				{
					setBackground(_parent.getBackground());
				}
			}
			// Set the text on the button...
			if (!symbol.getClassificationField().Equals(""))
			{
				if (include_controls)
				{
					// Reset the button to display the field
					if ((layer_view.getLegend().size() == 1) && (symbol.getClassificationType() != GRSymbol.CLASSIFICATION_SCALED_SYMBOL))
					{
						// Put the field on the button...
						_layer_JButton.setText(layer_view.getLegend().getText() + " (" + symbol.getClassificationField() + ")");
					}
					else
					{
						_layer_JButton.setText(layer_view.getLegend().getText());
					}
				}
				else
				{
					// No controls so the label needs to display the classification field...
					if (layer_view.getLegend().size() == 1)
					{
						_layer_JLabel.setText(layer_view.getLegend().getText() + " (" + symbol.getClassificationField() + ")");
					}
					else
					{
						// The label is next to the symbol...
						// This will take some more work.
					}
				}
			}
			else
			{
				// Set the label to the legend text...
				//_layer_JButton.setLabel (layer_view.getLegend().getText() );
				// Does not seem to work.
			}
		}
	}

	/// <summary>
	/// Process action events.  If the button is selected, this toggles the layer view
	/// from selected to deselected, highlighting when selected.
	/// If a parent GeoViewJPanel is specified during construction, its checkState() method is called. </summary>
	/// <param name="e"> ActionEvent to process. </param>
	public virtual void actionPerformed(ActionEvent e)
	{
		if (_layer_view.isSelected())
		{
			// Already selected so de-select...
			setBackground(Color.lightGray);
			_enabled_JCheckBox.setBackground(Color.lightGray);
			_layer_JButton.setBackground(Color.lightGray);
			_layer_JButton.setForeground(Color.black);
			for (int i = 0; i < _layer_class_JLabel.Length; i++)
			{
				_layer_Canvas[i].setBackground(Color.lightGray);
				_layer_Canvas[i].repaint();
				_layer_class_JLabel[i].setBackground(Color.lightGray);
				_layer_class_JLabel[i].setForeground(Color.black);
			}
			_layer_view.isSelected(false);
		}
		else
		{
			// Not already selected so select...
			setBackground(Color.darkGray);
			_enabled_JCheckBox.setBackground(Color.darkGray);
			_layer_JButton.setBackground(Color.darkGray);
			_layer_JButton.setForeground(Color.white);
			for (int i = 0; i < _layer_class_JLabel.Length; i++)
			{
				_layer_Canvas[i].setBackground(Color.darkGray);
				_layer_Canvas[i].repaint();
				_layer_class_JLabel[i].setBackground(Color.darkGray);
				_layer_class_JLabel[i].setForeground(Color.white);
			}
			_layer_view.isSelected(true);
		}
	}

	/// <summary>
	/// Reset the state of the check box based on the visibility of the layer view.
	/// The state of the checkbox is set consistent with the GeoLayerView "isSelected()" value.
	/// </summary>
	public virtual void checkState()
	{
		if (_enabled_JCheckBox != null)
		{
			_enabled_JCheckBox.setSelected(_layer_view.isVisible());
		}
	}

	/// <summary>
	/// Deselect the legend item.
	/// </summary>
	public virtual void deselect()
	{
		setBackground(Color.lightGray);
		_enabled_JCheckBox.setBackground(Color.lightGray);
		_layer_JButton.setBackground(Color.lightGray);
		_layer_JButton.setForeground(Color.black);
		for (int i = 0; i < _layer_class_JLabel.Length; i++)
		{
			_layer_Canvas[i].setBackground(Color.lightGray);
			_layer_Canvas[i].repaint();
			_layer_class_JLabel[i].setBackground(Color.lightGray);
			_layer_class_JLabel[i].setForeground(Color.black);
		}
		_layer_view.isSelected(false);
	}

	/// <summary>
	/// Clean up for garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~GeoLayerViewLegendJPanel()
	{
		_enabled_JCheckBox = null;
		_parent = null;
		_layer_view = null;
		_layer_JButton = null;
		_layer_JLabel = null;
		IOUtil.nullArray(_layer_Canvas);
		IOUtil.nullArray(_layer_class_JLabel);
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the GeoLayerView associated with the legend panel. </summary>
	/// <returns> the GeoLayerView associated with the legend panel. </returns>
	public virtual GeoLayerView getLayerView()
	{
		return _layer_view;
	}

	/// <summary>
	/// Handle item events.  The isVisible() method for the GeoLayerView is called with
	/// the state of the JCheckBox.
	/// </summary>
	public virtual void itemStateChanged(ItemEvent e)
	{
		if (e.getItemSelectable().Equals(_enabled_JCheckBox))
		{
			_layer_view.isVisible(_enabled_JCheckBox.isSelected());
		}
	}

	/// <summary>
	/// Handle mouse clicked event.  Don't do anything.  Rely on mousePressed().
	/// </summary>
	public virtual void mouseClicked(MouseEvent @event)
	{
	}

	/// <summary>
	/// Handle mouse drag event.  Don't do anything. </summary>
	/// <param name="event"> Mouse drag event. </param>
	public virtual void mouseDragged(MouseEvent @event)
	{ //event.consume();
	}

	/// <summary>
	/// Handle mouse enter event.  Currently does not do anything.
	/// </summary>
	public virtual void mouseEntered(MouseEvent @event)
	{
	}

	/// <summary>
	/// Handle mouse exit event.  Currently does not do anything.
	/// </summary>
	public virtual void mouseExited(MouseEvent @event)
	{
	}

	/// <summary>
	/// Handle mouse motion event.  Currently does not do anything.
	/// </summary>
	public virtual void mouseMoved(MouseEvent @event)
	{
	}

	/// <summary>
	/// Handle mouse pressed event.  Just save a flag indicating what keys were active.
	/// </summary>
	public virtual void mousePressed(MouseEvent e)
	{
		if (((e.getModifiers() & MouseEvent.SHIFT_MASK) != 0) || ((e.getModifiers() & MouseEvent.CTRL_MASK) != 0))
		{
			_ctrl_pressed = true;
		}
	}

	/// <summary>
	/// Handle mouse released event.
	/// </summary>
	public virtual void mouseReleased(MouseEvent e)
	{ // The _ctrl_pressed data member indicates whether a CTRL or SHIFT key
		// was pressed when the mouse was pressed.  Use that because it is
		// possible to release/press the key between releasing/pressing the mouse.
		if (!_ctrl_pressed)
		{
			// Unselect all other layer views except the current layer view...
			// _parent.deselectExcept ( this );
		}
		if (_layer_view.isSelected())
		{
			// Already selected so de-select...
			deselect();
		}
		else
		{
			// Not already selected so select...
			select();
		}
	}

	/// <summary>
	/// Check to make sure that the checkbox is accurate.
	/// </summary>
	public virtual void paint(Graphics g)
	{
		checkState();
		base.paint(g);
	}

	/// <summary>
	/// Select this legend item.
	/// </summary>
	public virtual void select()
	{
		setBackground(Color.darkGray);
		_enabled_JCheckBox.setBackground(Color.darkGray);
		_layer_JButton.setBackground(Color.darkGray);
		_layer_JButton.setForeground(Color.white);
		for (int i = 0; i < _layer_class_JLabel.Length; i++)
		{
			_layer_Canvas[i].setBackground(Color.darkGray);
			_layer_Canvas[i].repaint();
			_layer_class_JLabel[i].setBackground(Color.darkGray);
			_layer_class_JLabel[i].setForeground(Color.white);
		}
		_layer_view.isSelected(true);
	}

	/// <summary>
	/// Set the panel's canvas components visibility.
	/// This method is called from the properties interface to hide heavyweight canvas components. </summary>
	/// <param name="visible"> true if the components should be visible. </param>
	public virtual void setVisible(bool visible)
	{
		if ((_layer_Canvas != null) && (_layer_Canvas.Length > 0))
		{
			for (int i = 0; i < _layer_Canvas.Length; i++)
			{
				_layer_Canvas[i].setVisible(visible);
			}
		}
		base.setVisible(visible);
	}

	}

}
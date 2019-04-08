// ImageQualityJDialog - dialog for easily selecting the quality setting of a JPEG

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
// ImageQualityJDialog - a dialog for easily selecting the quality setting
//	of a JPEG.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2003-08-27	J. Thomas Sapienza, RTi	Initial version.
// 2005-04-26	JTS, RTi		Added finalize().
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.Util.GUI
{



	/// <summary>
	/// This class provides a simple dialog for specifying the quality of a JPEG in a 
	/// way that can be used in the JPEG encoder RTi uses.  The Dialog consists of a 
	/// slider, an OK button, a CANCEL button, and some information for the user.  
	/// The user can select a value from 0 - 100, and this value will be accessible 
	/// after the OK button is pressed. 
	/// If the user presses, CANCEL, the value that is returned is -1.  
	/// If the user closes the dialog from the X button in the upper-right-hand 
	/// corner, the value returned is also -1.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class ImageQualityJDialog extends javax.swing.JDialog implements java.awt.event.ActionListener, java.awt.event.WindowListener
	public class ImageQualityJDialog : JDialog, ActionListener, WindowListener
	{

	/// <summary>
	/// Button labels.
	/// </summary>
	private readonly string __BUTTON_CANCEL = "Cancel", __BUTTON_OK = "OK";

	/// <summary>
	/// The quality the user selected from the slider, or -1 if they hit CANCEL.
	/// </summary>
	private int __quality;

	/// <summary>
	/// The slider that the user will use to select image quality.
	/// </summary>
	private JSlider __slider;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="parent"> the parent JFrame on which the JDialog will appear. </param>
	public ImageQualityJDialog(JFrame parent) : base(parent, "Select Image Quality", true)
	{

		setupGUI();
	}

	/// <summary>
	/// Responds when the user presses the OK or Cancel button and sets the quality
	/// value accordingly. </summary>
	/// <param name="e"> the ActionEvent that happened. </param>
	public virtual void actionPerformed(ActionEvent e)
	{
		string action = e.getActionCommand();

		if (action.Equals(__BUTTON_CANCEL))
		{
			__quality = -1;
			dispose();
		}
		else if (action.Equals(__BUTTON_OK))
		{
			__quality = __slider.getValue();
			dispose();
		}
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~ImageQualityJDialog()
	{
		__slider = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Returns the quality the user selected. </summary>
	/// <returns> the quality the user selected. </returns>
	public virtual int getQuality()
	{
		return __quality;
	}

	/// <summary>
	/// Sets up the GUI.
	/// </summary>
	private void setupGUI()
	{
		addWindowListener(this);

		JPanel panel = new JPanel();
		panel.setLayout(new GridBagLayout());

		__slider = new JSlider(0, 100, 70);
		__slider.setPaintLabels(true);
		__slider.setPaintTicks(true);
		__slider.setMajorTickSpacing(10);
		__slider.setMinorTickSpacing(5);

		JGUIUtil.addComponent(panel, __slider, 0, 0, 1, 1, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.CENTER);
		JGUIUtil.addComponent(panel, new JLabel("Select the quality of the saved image."), 0, 1, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.CENTER);
		JGUIUtil.addComponent(panel, new JLabel("(Towards 0 means more compression, "), 0, 2, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.CENTER);
		JGUIUtil.addComponent(panel, new JLabel("towards 100 means higher quality)"), 0, 3, 1, 1, 0, 0, GridBagConstraints.NONE, GridBagConstraints.CENTER);

		getContentPane().add("Center", panel);

		JPanel bottom = new JPanel();
		bottom.setLayout(new FlowLayout());
		SimpleJButton okButton = new SimpleJButton(__BUTTON_OK, this);
		SimpleJButton cancelButton = new SimpleJButton(__BUTTON_CANCEL, this);
		bottom.add(okButton);
		bottom.add(cancelButton);

		getContentPane().add("South", bottom);

		pack();
		JGUIUtil.center(this);
		setVisible(true);
	}

	/// <summary>
	/// Does nothing. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowActivated(WindowEvent e)
	{
	}

	/// <summary>
	/// Does nothing. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowClosed(WindowEvent e)
	{
	}

	/// <summary>
	/// Sets the value that will be returned from getQuality() to -1. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowClosing(WindowEvent e)
	{
		__quality = -1;
	}

	/// <summary>
	/// Does nothing. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowDeactivated(WindowEvent e)
	{
	}

	/// <summary>
	/// Does nothing. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowDeiconified(WindowEvent e)
	{
	}

	/// <summary>
	/// Does nothing. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowIconified(WindowEvent e)
	{
	}

	/// <summary>
	/// Does nothing. </summary>
	/// <param name="e"> the WindowEvent that happened. </param>
	public virtual void windowOpened(WindowEvent e)
	{
	}

	}

}
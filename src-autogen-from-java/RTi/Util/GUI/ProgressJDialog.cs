using System.Threading;

// ProgressJDialog - dialog box that displays an updatable progress bar that runs in a Thread

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
// ProgressJDialog - dialog box that displays an updatable progress bar that
//	runs in a Thread.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:	
// 2003-07-24	J. Thomas Sapienza, RTi	Initial version.
// 2005-04-26	JTS, RTi		Added finalize().
// ----------------------------------------------------------------------------

namespace RTi.Util.GUI
{


	/// <summary>
	/// This class is a dialog box that can be used to track the completion of a 
	/// process.  The dialog runs in a thread so it can be updated while other processes
	/// are going on.
	/// <para>
	/// </para>
	/// <b>Using ProgressJDialog</b><para>
	/// Instantiate the class with the name of the parent frame (the dialog is opened
	/// </para>
	/// modally), the title for the dialog, and the min and max values.<para>
	/// <blockquote><pre>
	/// ProgressJDialog progress = new ProgressJDialog(this, 
	///		"Copy Progress", 0, numberFilesToBeCopied);
	/// </para>
	/// </pre></blockquote><para>
	/// 
	/// Every time some counter has been updated, call setProgressBarValue to set the
	/// current value (which should be between min and max, as defined in the
	/// </para>
	/// constructor).<para>
	/// <blockquote><pre>
	/// for (int i = 0; i &lt; numberFilesToBeCopied; i++) {
	///		copyFileToNewLocation(files[i], newLocation[i]);
	///		progress.setProgressBarValue(i + 1);
	/// }
	/// </pre></blockquote>
	/// </para>
	/// </summary>
	public class ProgressJDialog : JDialog, ThreadStart
	{

	/// <summary>
	/// The progress bar displayed in the dialog box.
	/// </summary>
	private JProgressBar __progressBar;

	/// <summary>
	/// Constructor. </summary>
	/// <param name="parent"> the JFrame on which this dialog appears.  The dialog is opened
	/// modally. </param>
	/// <param name="title"> the title string for the dialog. </param>
	/// <param name="min"> the minimum value for computing the % complete in the progress bar. </param>
	/// <param name="max"> the maximum value for computing the % complete in the progress bar. </param>
	public ProgressJDialog(JFrame parent, string title, int min, int max) : base(parent, title, false)
	{

		__progressBar = new JProgressBar(min, max);
		__progressBar.setValue(0);
		__progressBar.setIndeterminate(false);
		__progressBar.setStringPainted(true);
		setupGUI();
		(new Thread(this)).Start();
	}

	/// <summary>
	/// Cleans up member variables.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void finalize() throws Throwable
	~ProgressJDialog()
	{
		__progressBar = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Runs the thread.
	/// </summary>
	public virtual void run()
	{
	}

	/// <summary>
	/// Sets the value of the progress bar.  The value should be &gr;= to the min value
	/// passed in to the constructor and &lt;= to the max value passed in to the 
	/// constructor. </summary>
	/// <param name="value"> the value to set the progress bar completion amount to. </param>
	public virtual void setProgressBarValue(int value)
	{
		__progressBar.setValue(value);
		Rectangle rect = getBounds();
		rect.x = 0;
		rect.y = 0;
		__progressBar.paintImmediately(rect);
	}

	/// <summary>
	/// Sets up the GUI.
	/// </summary>
	private void setupGUI()
	{
		GridBagLayout gbl = new GridBagLayout();

		JPanel mainPanel = new JPanel();
		mainPanel.setLayout(gbl);

		JGUIUtil.addComponent(mainPanel, __progressBar, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, GridBagConstraints.BOTH, GridBagConstraints.CENTER);

		getContentPane().add(mainPanel);

		pack();
		JGUIUtil.center(this);
	}

	}

}
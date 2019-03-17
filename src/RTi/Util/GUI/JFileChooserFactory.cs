using System;
using System.Threading;

// JFileChooserFactory - factory for creating JFileChoosers

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
// JFileChooserFactory - factory for creating JFileChoosers.
// ----------------------------------------------------------------------------
// Copyright:   See the COPYRIGHT file
// ----------------------------------------------------------------------------
// History:
// 2003-10-07	J. Thomas Sapienza, RTi	Initial version.
// 2003-10-08	JTS, RTi		Eliminated a lot of duplicate code.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.Util.GUI
{


	using IOUtil = RTi.Util.IO.IOUtil;

	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This class is a factory for creating JFileChoosers.  This class is 
	/// <b>completely</b> unneccessary for code that is <b>NOT</b> compiled with 
	/// JDK 1.4.2.  <para>
	/// It works around an error that occurs rarely in 1.4.2 and results in a null
	/// pointer exception when a JFileChooser is created.  
	/// </para>
	/// </summary>
	public abstract class JFileChooserFactory
	{

	/// <summary>
	/// This class should only be used statically.
	/// </summary>
	private JFileChooserFactory()
	{
	}

	/// <summary>
	/// Creates a file chooser using the system default directory. </summary>
	/// <returns> a file chooser object with the system default directory as the 
	/// initial directory.  The returned object will never be null. </returns>
	public static JFileChooser createJFileChooser()
	{
		return createJFileChooser((string)null);
	}

	/// <summary>
	/// Creates a file chooser with the specified initial directory. </summary>
	/// <param name="initialDir"> the initial directory from which the file chooser should
	/// start.  Can be null. </param>
	/// <returns> a file chooser.  The returned object will never be null. </returns>
	public static JFileChooser createJFileChooser(File initialDir)
	{
		if (initialDir == null)
		{
			return createJFileChooser((string)null);
		}
		else
		{
			return createJFileChooser(initialDir.ToString());
		}
	}

	/// <summary>
	/// Creates a file chooser with the specified initial directory. </summary>
	/// <param name="initialDir"> the initial directory from which the JFileChooser should
	/// start.  Can be null. </param>
	/// <returns> a file chooser.  The returned object will never be null. </returns>
	public static JFileChooser createJFileChooser(string initialDir)
	{
		string routine = "JFileChooserFactory.createFileChooser";

		if (IOUtil.testing())
		{
			JFileChooser jfc = workaround(initialDir);
			if (jfc != null)
			{
				return jfc;
			}
		}

		try
		{
			JFileChooser fc1 = null;
			if (string.ReferenceEquals(initialDir, null))
			{
				fc1 = new JFileChooser();
			}
			else
			{
				fc1 = new JFileChooser(initialDir);
			}
			return fc1;
		}
		catch (Exception e1)
		{
			if (Message.isDebugOn)
			{
			Message.printWarning(2, routine, e1);
			Message.printWarning(2, routine, "Initial file chooser creation" + " failed -- turning off file chooser speed " + "enhancement and trying again.");
			}
			try
			{
				System.setProperty("swing.disableFileChooserSpeedFix", "true");
				JFileChooser fc2 = null;
				if (string.ReferenceEquals(initialDir, null))
				{
					fc2 = new JFileChooser();
				}
				else
				{
					fc2 = new JFileChooser(initialDir);
				}
				System.setProperty("swing.disableFileChooserSpeedFix", "false");
				return fc2;
			}
			catch (Exception)
			{
				LookAndFeel initialLnF = UIManager.getLookAndFeel();
				string name = initialLnF.getName();
				JGUIUtil.setSystemLookAndFeel(false);

				string newName = UIManager.getLookAndFeel().getName();
				JFileChooser fc3 = null;
				if (string.ReferenceEquals(initialDir, null))
				{
					fc3 = new JFileChooser();
				}
				else
				{
					fc3 = new JFileChooser(initialDir);
				}

				if (!name.Equals(newName))
				{
					JGUIUtil.setSystemLookAndFeel(true);
				}
				return fc3;
			}
		}
	}

	/// <summary>
	/// A workaround for creating JFileChoosers using JDK 1.4.2. </summary>
	/// <param name="initialDir"> the initial directory for the file chooser. </param>
	/// <returns> a JFileChooser, created in the proper look and feel (or in the event
	/// that the workaround actually needs to do its magic, in the Swing look and feel). </returns>
	private static JFileChooser workaround(string initialDir)
	{
		string routine = "JFileChooserFactory.workaround";

		int maxTries = 5;
		int currTry = 0;

		JFileChooser jfc = null;

		while (currTry < maxTries && jfc == null)
		{
			try
			{
				if (string.ReferenceEquals(initialDir, null))
				{
					jfc = new JFileChooser();
				}
				else
				{
					jfc = new JFileChooser(initialDir);
				}
			}
			catch (Exception)
			{
				Message.printWarning(2, routine, "Failed to create JFileChooser with default " + "look-and-feel (try #" + (currTry + 1) + ")");
				jfc = null;
			}
			try
			{
				Thread.Sleep(200);
			}
			catch (InterruptedException)
			{
			}

			currTry++;
		}

		if (jfc == null)
		{
			Message.printStatus(2, routine, "JFileChooserFactory failed " + "to create a file chooser with the current look-" + "and-feel.");
		}
		else
		{
			Message.printStatus(2, routine, "new JFileChooserFactory " + "workaround successfully created a file chooser with " + "the current look-and-feel.");
		}

		return jfc;
	}

	}

}
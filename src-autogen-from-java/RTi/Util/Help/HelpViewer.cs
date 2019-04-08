﻿using System;

// HelpViewer - this class manages displaying help for an application

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

namespace RTi.Util.Help
{

	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// This class manages displaying help for an application.
	/// It does so by handling setup of UI components such as dialogs with a help button
	/// and when help is requested, showing the help in the default browser.
	/// The HelpManager is a singleton that is requested with getInstance().
	/// @author sam
	/// 
	/// </summary>
	public class HelpViewer
	{

		/// <summary>
		/// Singleton instance of the HelpManager.
		/// </summary>
		private static HelpViewer helpViewer = null;

		/// <summary>
		/// Interface implementation to format the URL.
		/// </summary>
		private HelpViewerUrlFormatter urlFormatter = null;

		/// <summary>
		/// Constructor for default instance.
		/// </summary>
		public HelpViewer()
		{

		}

		/// <summary>
		/// Return the singleton instance of the HelpManager.
		/// </summary>
		public static HelpViewer getInstance()
		{
			if (helpViewer == null)
			{
				helpViewer = new HelpViewer();
			}
			return helpViewer;
		}

		/// <summary>
		/// Set the object that will format URLs for the viewer.
		/// This is typically called application code that has knowledge of the documentation organization.
		/// </summary>
		public virtual void setUrlFormatter(HelpViewerUrlFormatter urlFormatter)
		{
			this.urlFormatter = urlFormatter;
		}

		/// <summary>
		/// Show the help using a web browser. </summary>
		/// <param name="group"> the group to which the item belongs, will be passed to HelpViewerUrlFormatter().formatUrl(). </param>
		/// <param name="item"> the item for which to display help, will be passed to HelpViewerUrlFormatter().formatUrl(). </param>
		public virtual void showHelp(string group, string item)
		{
			string routine = "showHelp";
			// Use the default web browser application to display help.
			if (this.urlFormatter == null)
			{
				Message.printWarning(1, "", "Unable to display documentation for group \"" + group + "\" and item \"" + item + "\" - no URL formatter defined.");
			}
			else
			{
				// Format the URL for the item
				string docUri = this.urlFormatter.formatHelpViewerUrl(group, item);
				if (string.ReferenceEquals(docUri, null))
				{
					Message.printWarning(1, "", "Unable to determine documentation URL for group=\"" + group + "\", item=\"" + item + "\".");
				}
				// Now display using the default application for the file extension
				Message.printStatus(2, routine, "Opening documentation \"" + docUri + "\"");
				// If 
				if (!Desktop.isDesktopSupported())
				{
					Message.printWarning(1, "", "Opening browser from software not supported.  View the following in a browser: " + docUri);
				}
				else
				{
					// The Desktop.browse() method will always open, even if the page does not exist,
					// and it won't return the HTTP error code in this case.
					// Therefore, do a check to see if the URI is available before opening in a browser
					try
					{
						Desktop desktop = Desktop.getDesktop();
						desktop.browse(new URI(docUri));
					}
					catch (Exception e)
					{
						Message.printWarning(2, "", "Unable to display documentation at \"" + docUri + "\" uniquetempvar.");
					}
				}
			}
		}

	}

}
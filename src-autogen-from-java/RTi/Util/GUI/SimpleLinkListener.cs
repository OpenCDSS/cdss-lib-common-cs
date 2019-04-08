using System.Collections.Generic;

// SimpleLinkListener - listener changes the cursor over hotspots based on enter/exit events
// and also load a new page when a valid hyperlink is clicked

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

/// <summary>
///***************************************************************************
/// SimpleLinkListener.java
/// Author:kat	Date: 2007-03-13
/// A hyperlink listener for use with JEditorPane. This
/// listener changes the cursor over hotspots based on enter/exit
/// events and also load a new page when a valid hyperlink is clicked.
/// Keeps track of previous pages or links visited.
/// 
/// REVISIONS:
/// 2007-03-13	Kurt Tometich	Initial version.
/// ***************************************************************************
/// </summary>
namespace RTi.Util.GUI
{


	using Message = RTi.Util.Message.Message;

	/// <summary>
	/// Class provides a LinkListener to update the pages for a JEditorPane
	/// whose content is rendered HTMl.  This clas also keeps track of the
	/// pages and links visited so that back and forward buttons can be used
	/// in the JEditorPane.
	/// </summary>
	public class SimpleLinkListener : HyperlinkListener
	{
		private JEditorPane jep_JEditorPane; // The pane we�re using to display HTML
		private JTextField url_TextField; // An optional text field for showing
		// the current URL being displayed
		private JLabel statusBar_JTextField; // An optional label for showing where
		// a link would take you
		private JButton back_JButton; // button that needs to be updated
		// when user navigates to new page or section
		private static IList<string> __urls;

	/// <summary>
	/// Constructor to initialize SimpleLinkListener class. </summary>
	/// <param name="jep"> JEditorPane to use. </param>
	/// <param name="jtf"> Text field to update current link. </param>
	/// <param name="jl"> Label used for JEditorPane. </param>
	/// <param name="back"> Button to mimic browser back button for current JEditorPane. </param>
	public SimpleLinkListener(JEditorPane jep, JTextField jtf, JLabel jl, JButton back)
	{
			jep_JEditorPane = jep;
			url_TextField = jtf;
			statusBar_JTextField = jl;
			back_JButton = back;
			__urls = new List<string>();
	}

	/// <summary>
	/// Another constructor to initialize SimpleLinkListener class. </summary>
	/// <param name="jep"> JEditorPane to use. </param>
	/// <param name="back"> Button to mimic browser back button for current JEditorPane. </param>
	public SimpleLinkListener(JEditorPane jep, JButton back) : this(jep, null, null, back)
	{
	}

	/// <summary>
	/// Handles any hyperlink event caused by the user clicking a hyperlink. </summary>
	/// <param name="he"> Event that occurs when user clicks a hyperlink. </param>
	public virtual void hyperlinkUpdate(HyperlinkEvent he)
	{
		HyperlinkEvent.EventType type = he.getEventType();
		if (type == HyperlinkEvent.EventType.ENTERED)
		{
			// Enter event. Fill in the status bar
			if (statusBar_JTextField != null)
			{
				statusBar_JTextField.setText(he.getURL().ToString());
			}
		}
		else if (type == HyperlinkEvent.EventType.EXITED)
		{
			// Exit event. Clear the status bar
			if (statusBar_JTextField != null)
			{
				statusBar_JTextField.setText(" ");
				// must be a space or JTextField disappears
			}
		}
		else
		{
			// Jump event. Get the url, and if it�s not null, switch to that
			// page in the main editor pane and update the "site url" label.
			if (he is HTMLFrameHyperlinkEvent)
			{
			// frame event... handle this separately
			HTMLFrameHyperlinkEvent evt = (HTMLFrameHyperlinkEvent)he;
				HTMLDocument doc = (HTMLDocument)jep_JEditorPane.getDocument();
				doc.processHTMLFrameHyperlinkEvent(evt);
			}
			else
			{
				try
				{
					// set the previous url for back button 
					// and update page with new URL
					__urls.Add(url_TextField.getText());
					jep_JEditorPane.setPage(he.getURL());
					back_JButton.setEnabled(true);
					if (url_TextField != null)
					{
						url_TextField.setText(he.getURL().ToString());
					}
				}
				catch (FileNotFoundException)
				{
					Message.printWarning(2, "SimpleLinkListener.HyperLinkUpdate", "File: " + he.getURL().ToString() + " was not found.");
				}
				catch (IOException e)
				{
					Message.printWarning(2, "SimpleLinkListener.HyperLinkUpdate", "Couldn't set text from file: " + he.getURL().ToString());
					Message.printWarning(3, "SimpleLinkListener.HyperLinkUpdate", e);
				}
			}
		}
	}

	/// <summary>
	/// Returns the previous URL that was navigated by the user. </summary>
	/// <returns> previous_url Previous URL navigated. </returns>
	public static string getPreviousUrl()
	{

		string previous_url = "";
		// check internal list of urls
		if (__urls.Count > 0)
		{
			int last_index = __urls.Count - 1;
			previous_url = (string)__urls[last_index];
			__urls.RemoveAt(last_index);
		}
		return previous_url;
	}

	/// <summary>
	/// Returns the current size of the previously navigated URL list.
	/// Used to enable the back button used in a JEditorPane. </summary>
	/// <returns> Size of previously navigated URL list. </returns>
	public static int getPreviousUrlSize()
	{
		return __urls.Count;
	}

	/// <summary>
	/// Adds a URL to the previous URL list used by the back button. </summary>
	/// <param name="url"> Url to add. </param>
	public static void addToPreviousUrl(string url)
	{
		if (!string.ReferenceEquals(url, null))
		{
			__urls.Add(url);
		}
	}

	}


}
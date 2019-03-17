using System.Collections.Generic;

// TSViewZoomHistory - class to retain a history of the zoom levels for a time series graph

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

	using GRLimits = RTi.GR.GRLimits;

	/// <summary>
	/// This class retains a history of the zoom levels for a time series graph.
	/// It is primarily used to remember the horizontal (time) zoom on GRTS time series graphs.
	/// Behavior is similar to a web browser.
	/// See:  http://stackoverflow.com/questions/1313788/how-does-the-back-button-in-a-web-browser-work
	/// <ol>
	/// <li> History of zoom is retained, seeded with the initial zoom.</li>
	/// <li> As new zooms occur via drawing box on the graph, new limits are saved.</li>
	/// <li> Current zoom is the current position in the list.</li>
	/// <li> Previous zoom is the previous.</li>
	/// <li> Next zoom is the next, if a linear previous/next user action sequence has occurred.</li>
	/// <li> If after going to previous a new zoom is performed, the "next history" is cleared so that using next is unambiguous.</li>
	/// </ol>
	/// @author sam
	/// 
	/// </summary>
	public class TSViewZoomHistory
	{

		/// <summary>
		/// The zoom limits.
		/// </summary>
		private IList<GRLimits> zoomLimits = new List<GRLimits>();

		/// <summary>
		/// Position of the current zoom, -1 if nothing in the list.
		/// </summary>
		private int currentZoomPos = -1;

		/// <summary>
		/// Constructor.
		/// </summary>
		public TSViewZoomHistory()
		{
		}

		/// <summary>
		/// Add a zoom after the current zoom and make it the current.
		/// </summary>
		public virtual void add(GRLimits newLimits)
		{
			// Because the user is adding a new zoom, clear all nexts to start the new sequence
			if (this.currentZoomPos >= 0)
			{
				for (int i = this.zoomLimits.Count - 1; i > currentZoomPos; i--)
				{
					this.zoomLimits.RemoveAt(i);
				}
			}
			// Now add the new limit and advance the current zoom
			this.zoomLimits.Add(newLimits);
			this.currentZoomPos = this.zoomLimits.Count - 1;
		}

		/// <summary>
		/// Get the current zoom limits.
		/// Return the current limits or null if no current limits.
		/// </summary>
		public virtual GRLimits getCurrentZoom()
		{
			if (this.currentZoomPos >= 0)
			{
				return this.zoomLimits[this.currentZoomPos];
			}
			return null;
		}

		/// <summary>
		/// Get the next zoom limits.
		/// Return the next limits or null if no next limits.
		/// </summary>
		public virtual GRLimits getNextZoom()
		{
			if (this.currentZoomPos >= 0)
			{
				int nextZoomPos = currentZoomPos + 1;
				if ((this.zoomLimits.Count - 1) >= nextZoomPos)
				{
					return this.zoomLimits[nextZoomPos];
				}
			}
			return null;
		}

		/// <summary>
		/// Get the previous zoom limits.
		/// Return the previous limits or null if no previous limits.
		/// </summary>
		public virtual GRLimits getPreviousZoom()
		{
			if (this.currentZoomPos >= 1)
			{
				int previousZoomPos = currentZoomPos - 1;
				return this.zoomLimits[previousZoomPos];
			}
			return null;
		}

		/// <summary>
		/// Go to the next limits and return the limits. </summary>
		/// <returns> the next limits or null if there were no next limits. </returns>
		public virtual GRLimits next()
		{
			if (this.currentZoomPos >= 1)
			{
				int nextZoomPos = currentZoomPos + 1;
				if ((this.zoomLimits.Count - 1) >= nextZoomPos)
				{
					++currentZoomPos;
					return this.zoomLimits[nextZoomPos];
				}
			}
			return null;
		}

		/// <summary>
		/// Go to the previous zoom limits.
		/// Return the previous limits or null if no previous limits.
		/// </summary>
		public virtual GRLimits previous()
		{
			if (this.currentZoomPos >= 1)
			{
				int previousZoomPos = currentZoomPos - 1;
				--currentZoomPos;
				return this.zoomLimits[previousZoomPos];
			}
			return null;
		}
	}

}
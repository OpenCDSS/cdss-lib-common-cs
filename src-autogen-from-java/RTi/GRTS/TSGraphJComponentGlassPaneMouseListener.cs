// TSGraphJComponentGlassPaneMouseListener - listen for MouseEvent and MouseMotionEvent that are of interest to the underlying TSGraphJComponent

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

	/// <summary>
	/// Listen for MouseEvent and MouseMotionEvent that are of interest to the underlying TSGraphJComponent
	/// and redispatch.  Draw the mouse tracker on the graph based on information from the TSGraphJComponent.
	/// </summary>
	public class TSGraphJComponentGlassPaneMouseListener : MouseListener, MouseMotionListener
	{
		/// <summary>
		/// Glass pane that is used to render the mouse tracker.
		/// </summary>
		internal TSGraphJComponentGlassPane glassPane;

		/// <summary>
		/// TSGraphJComponent that renders the normal graph.
		/// </summary>
		internal TSGraphJComponent tsgraphJComponent;

		/// <summary>
		/// The content pane that manages the rendering components, not currently used.
		/// </summary>
		internal Container contentPane;

		/// <summary>
		/// Indicates whether dragging the mouse, not currently used but may move from
		/// the main rendering component to the tracker component to improve performance.
		/// </summary>
		internal bool inDrag = false;

		/// <summary>
		/// Construct the mouse listener for the glass pane. </summary>
		/// <param name="glassPane"> TSGraphJComponentGlassPane instance to render mouse tracker. </param>
		/// <param name="tsgraphJComponent"> underlying TSGraphJComponent to render the graph product. </param>
		/// <param name="contentPane"> component that manages the glass pane and main rendering component. </param>
		public TSGraphJComponentGlassPaneMouseListener(TSGraphJComponentGlassPane glassPane, TSGraphJComponent tsgraphJComponent, Container contentPane)
		{
			this.glassPane = glassPane;
			this.tsgraphJComponent = tsgraphJComponent;
			this.contentPane = contentPane;
		}

		/// <summary>
		/// Handle mouse moved event in the glass pane.
		/// </summary>
		public virtual void mouseMoved(MouseEvent e)
		{
			//System.out.println("In TSGraphJComponentGlassPaneMouseListener.mouseMoved:  glassPanePoint=" + e.getX() + "," + e.getY());
			// Cause a redraw of the mouse tracker
			redispatchMouseEvent(e, true);
		}

		/// <summary>
		/// Handle mouse dragged event in the glass pane.
		/// </summary>
		public virtual void mouseDragged(MouseEvent e)
		{
			redispatchMouseEvent(e, false);
		}

		/// <summary>
		/// Handle mouse clicked event in the glass pane.
		/// </summary>
		public virtual void mouseClicked(MouseEvent e)
		{
			redispatchMouseEvent(e, false);
		}

		/// <summary>
		/// Handle mouse entered event in the glass pane.
		/// </summary>
		public virtual void mouseEntered(MouseEvent e)
		{
			redispatchMouseEvent(e, false);
		}

		/// <summary>
		/// Handle mouse exited event in the glass pane.
		/// </summary>
		public virtual void mouseExited(MouseEvent e)
		{
			redispatchMouseEvent(e, false);
		}

		/// <summary>
		/// Handle mouse pressed event in the glass pane.
		/// </summary>
		public virtual void mousePressed(MouseEvent e)
		{
			redispatchMouseEvent(e, false);
		}

		/// <summary>
		/// Handle mouse released event in the glass pane.
		/// </summary>
		public virtual void mouseReleased(MouseEvent e)
		{
			redispatchMouseEvent(e, true);
			this.inDrag = false;
		}

		// TODO sam 2017-02-26 is there a need to separate the event types with different methods?
		/// <summary>
		/// Redispatch all mouse events to the underlying TSGraphJComponent.
		/// This handles MouseEvent and MouseMotionEvent. </summary>
		/// <param name="e"> MouseEvent from glass pane </param>
		/// <param name="repaint"> if true then redraw the glass pane </param>
		private void redispatchMouseEvent(MouseEvent e, bool repaint)
		{
			Point glassPanePoint = e.getPoint();
			//System.out.println("In TSGraphJComponentGlassPaneMouseListener.redispatchMouseEvent:  originating mouse event component is " + e.getComponent().getName());
			//System.out.println("In TSGraphJComponentGlassPaneMouseListener.redispatchMouseEvent:  glassPanePoint=" + glassPanePoint.x + "," + glassPanePoint.y);
			//Container container = contentPane;
			// Convert the point in the glass pane to the JFrame content pane coordinates
			//Point containerPoint = SwingUtilities.convertPoint(
			//                                glassPane,
			//                                glassPanePoint, 
			//                                contentPane);
			//System.out.println("In TSGraphJComponentGlassPaneMouseListener.redispatchMouseEvent:  containerPoint=" + containerPoint.x + "," + containerPoint.y);

			// The coordinates of the glass pane should be the same as the underlying TSGraphJComponent
			// since both are the same size so just forward the event
			//System.out.println("In TSGraphJComponentGlassPaneMouseListener.redispatchMouseEvent:  glassPanePoint=" + glassPanePoint.x + "," + glassPanePoint.y);
			Component component = this.tsgraphJComponent; // Component that should experience event
			component.dispatchEvent(new MouseEvent(component, e.getID(), e.getWhen(), e.getModifiers(), glassPanePoint.x, glassPanePoint.y, e.getClickCount(), e.isPopupTrigger()));

			if (repaint)
			{
				//System.out.println("In TSGraphJComponentGlassPaneMouseListener.redispatchMouseEvent:  calling repaint on glass pane for x=" + glassPanePoint.x + "," + glassPanePoint.y);
				//toolkit.beep();
				this.glassPane.setPoint(glassPanePoint); // This allows
				// Decide if need intermediate data
				//glassPane.setMouseTrackerData(this.tsgraphJComponent.getMouseTrackerData(glassPane.getMouseTrackerData(),glassPane,glassPanePoint.x, glassPanePoint.y));
				this.glassPane.repaint();
			}
		}
	}

}
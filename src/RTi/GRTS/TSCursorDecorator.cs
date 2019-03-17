// TSCursorDecorator -provides a decorator to draw a cross-hair cursor on a JComponent

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
	//import java.awt.event.MouseMotionAdapter;

	using GRColor = RTi.GR.GRColor;
	using GRLimits = RTi.GR.GRLimits;

	/// <summary>
	/// Provides a decorator to draw a cross-hair cursor on a JComponent.
	/// Usage:
	/// <pre>
	/// _cursorDecorator = new TSCursorDecorator(this, crosshairColor,
	///                                          _background_color);
	/// 
	/// public void mouseMoved ( MouseEvent event )
	/// {
	/// _cursorDecorator.mouseMoved(event,tsgraph.getGraphDrawingArea().getPlotLimits(
	///     GRDrawingArea.COORD_DEVICE));
	/// <pre>
	/// 
	/// @author dre
	/// </summary>
	public class TSCursorDecorator
	{
	  /// <summary>
	  /// Canvas component </summary>
	  private JComponent _jComponent;
	  /// <summary>
	  /// Flag indicating whether decorator drawn </summary>
	  private bool _eraseNeeded = false;
	  /// <summary>
	  /// Decorator color </summary>
	  private GRColor _cursorColor;
	  /// <summary>
	  /// XOR color </summary>
	  private GRColor _xorColor;
	  /// <summary>
	  /// Current mouse location </summary>
	  private Point _currentMousePoint;
	  /// <summary>
	  /// Limits of drawing area </summary>
	  private GRLimits _daLimits;

	  /// <summary>
	  /// Draws a cross-hair cursor on the specified JComponent.
	  /// <para>
	  /// The cross-hairs extend to the drawing area limits passed in
	  /// <seealso cref="#mouseMoved(MouseEvent, GRLimits)"/>
	  /// 
	  /// </para>
	  /// </summary>
	  /// <param name="jComponent"> Component on which mouse is tracked </param>
	  /// <param name="cursorColor"> Cursor color </param>
	  /// <param name="xorColor">   Color for XOR mode </param>
	  public TSCursorDecorator(JComponent jComponent, GRColor cursorColor, GRColor xorColor)
	  {
		_jComponent = jComponent;
		_cursorColor = cursorColor;
		_xorColor = xorColor;

		// Set up mouse motion listener for mouse movement
		// Can't use MouseMotionListener because not all regions
		// of JGraphJComponent should have a cross-hair...
		// _jComponent.addMouseMotionListener(new MouseMotionHandler());     
	  }

	  /// <summary>
	  /// Draws cross-hair cursor.
	  /// <para>
	  /// Called both to erase the previous cross-hair & to draw the
	  /// new cross-hair.
	  /// </para>
	  /// </summary>
	  protected internal virtual void draw()
	  {
		Graphics g = _jComponent.getGraphics();
		g.setColor(_cursorColor);

		if (g != null)
		{
			try
			{
			  // Use XORMode so that we don't have to call redraw() to
			  // remove the decorator, rather we just draw again
			  g.setXORMode(_xorColor);
			  drawDecorator(g);

			  _eraseNeeded = true;
			}
			finally
			{
			  g.dispose();
			}
		}
	  }

	  /// <summary>
	  /// Draws cross-hair.
	  /// <para>
	  /// The cross-hair is drawn to the edges of the drawing area
	  /// </para>
	  /// </summary>
	  /// <param name="g"> </param>
	  protected internal virtual void drawDecorator(Graphics g)
	  {
		g.drawLine((int)_daLimits.getLeftX(), _currentMousePoint.y, (int) _daLimits.getRightX(), _currentMousePoint.y);
		g.drawLine(_currentMousePoint.x, (int)_daLimits.getTopY(), _currentMousePoint.x,(int) _daLimits.getBottomY());
	  }

	  /// <summary>
	  /// Returns whether mouse is inside drawing area
	  /// @return
	  /// </summary>
	  private bool isInside()
	  {
		return (_currentMousePoint.x > (int)_daLimits.getLeftX() && _currentMousePoint.x < (int)_daLimits.getRightX() && _currentMousePoint.y > (int)_daLimits.getTopY() && _currentMousePoint.y < (int)_daLimits.getBottomY()) ?true:false;
	  }

	  /// <summary>
	  /// Erases decorator by redrawing it in XOR mode.
	  /// </summary>
	  private void erase()
	  {
		if (_eraseNeeded)
		{
			draw();
			_eraseNeeded = false;
		}
	  }
	  /// <summary>
	  /// Erase previous cross-hair & draw at new position.
	  /// </summary>
	  /// <param name="e"> </param>
	  public void mouseMoved(MouseEvent e, GRLimits daLimits)
	  {
		_daLimits = daLimits;
		// Erase old decorator before setting new mouse position
		 erase();
		_currentMousePoint = e.getPoint();

	 //   if (_daLimits.contains(_currentMousePoint.x,_currentMousePoint.y))
		if (isInside())
		{
			draw();
		}
	//    System.out.println("-->Outside");
		//erase();
	  }
	  /// <summary>
	  /// Monitors the mouse for movement, initiates drawing/erasing of docorator
	  /// 
	  /// @author dre
	  /// </summary>
	  /* FIXME SAM 2008-04-15 Evalue whether using
	  private class MouseMotionHandler extends MouseMotionAdapter
	  {
	    public void mouseMoved(MouseEvent e) 
	    {
	      // Erase old decorator before setting new mouse position
	      erase();            
	      _currentMousePoint = e.getPoint();
	
	      draw();
	    }
	  } // eof class MouseMotionHandler
	  */

	} // eof class TSGraphCursor

}
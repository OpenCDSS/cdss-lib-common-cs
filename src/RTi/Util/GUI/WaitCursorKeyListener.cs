// WaitCursorKeyListener - KeyListener that absorbs all KeyEvent when the component is wait-locked

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

namespace RTi.Util.GUI
{

	/// <summary>
	/// KeyListener that absorbs all KeyEvent when the component is wait-locked.
	/// This is used by the JGUIUtil.setWaitCursor(true) call to track the
	/// instance of listener that is temporarily added so that it can be removed after temporary use.
	/// @author sam
	/// 
	/// </summary>
	public class WaitCursorKeyListener : KeyAdapter
	{

		/// <summary>
		/// Constructor for listener - all KeyEvent will be absorbed without taking any actions. </summary>
		/// <param name="c"> component listed to </param>
		public WaitCursorKeyListener(Component c)
		{

		}

	}

}
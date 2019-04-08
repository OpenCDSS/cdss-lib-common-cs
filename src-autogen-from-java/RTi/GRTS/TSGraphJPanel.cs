﻿using System.Collections.Generic;

// TSGraphJPanel - test panel to see if we can get the background JPEG to work

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

// Test panel to see if we can get the background JPEG to work
//
// SAM - go ahead and port to Swing but it may be discarded later.
// 2003-06-04	SAM, RTi		Update to latest GR, TS Swing.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.

namespace RTi.GRTS
{


	using TS = RTi.TS.TS;
	using JGUIUtil = RTi.Util.GUI.JGUIUtil;
	using Prop = RTi.Util.IO.Prop;
	using PropList = RTi.Util.IO.PropList;
	using Message = RTi.Util.Message.Message;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class TSGraphJPanel extends javax.swing.JPanel
	public class TSGraphJPanel : JPanel
	{

	internal TSGraphJComponent _graph = null;

	public TSGraphJPanel(JFrame parent, IList<TS> tslist, PropList props)
	{
			   GridBagLayout gbl = new GridBagLayout();
		setLayout(gbl);
		PropList _props = props;
		JFrame f = new JFrame();
		f.addNotify();
		Image image = f.createImage(400,400);
		if (image == null)
		{
			Message.printStatus(1, "", "Image is null");
		}
		_props.set(new Prop("Image", image, ""));
		_graph = new TSGraphJComponent(null, tslist, _props);
		int y = 0;
		JGUIUtil.addComponent(this, _graph, 0, y, 1, 1, 1, 1, 0, 0, 0, 0, GridBagConstraints.BOTH, GridBagConstraints.NORTH);

		setVisible(false);
		_graph.paint(image.getGraphics());

		// Hopefully the graph will now be constructed.
	}

	/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void saveAsFile(String filename) throws java.io.IOException
	public virtual void saveAsFile(string filename)
	{
		_graph.saveAsFile(filename);
	}

	}

}
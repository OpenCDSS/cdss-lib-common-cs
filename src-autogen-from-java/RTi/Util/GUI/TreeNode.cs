// TreeNode - primitive data for Tree component

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

//-----------------------------------------------------------------------------
// TreeNode - primitive data for Tree component
//-----------------------------------------------------------------------------
// History:
//
// 2001-12-13	Steven A. Malers, RTi	Original version, from HDF package (see
//					copyright information below).
//-----------------------------------------------------------------------------
/// <summary>
///**************************************************************************
/// NCSA HDF                                                                 *
/// National Comptational Science Alliance                                   *
/// University of Illinois at Urbana-Champaign                               *
/// 605 E. Springfield, Champaign IL 61820                                   *
///                                                                          *
/// For conditions of distribution and use, see the accompanying             *
/// hdf/COPYING file.                                                        *
///                                                                          *
/// ***************************************************************************
/// </summary>
/// <summary>
/// ------------------------------------------------------------------
/// 
/// Copyright Notice and Statement for NCSA Hierarchical Data Format (HDF) 
/// Software Library and Utilities
/// 
/// Copyright 1988-2001 The Board of Trustees of the University of Illinois
/// 
/// All rights reserved.
/// 
/// Contributors:   National Center for Supercomputing Applications 
/// (NCSA) at the University of Illinois, Fortner Software, Unidata 
/// Program Center (netCDF), The Independent JPEG Group (JPEG), 
/// Jean-loup Gailly and Mark Adler (gzip), and Digital Equipment 
/// Corporation (DEC). Macintosh support contributed by Gregory L. Guerin.
/// 
/// 
/// The package 'glguerin':
/// Copyright 1998, 1999 by Gregory L. Guerin.
/// Redistribute or reuse only as described below.
/// These files are from the MacBinary Toolkit for Java:
///   <http://www.amug.org/~glguerin/sw/#macbinary>
/// and are redistributed by NCSA with permission of the 
/// author.
/// 
/// Redistribution and use in source and binary forms, with or without 
/// modification, are permitted for any purpose (including commercial 
/// purposes) provided that the following conditions are met:
/// 
/// 1. Redistributions of source code must retain the above copyright 
/// notice, this list of conditions, and the following disclaimer.
/// 
/// 2. Redistributions in binary form must reproduce the above copyright 
/// notice, this list of conditions, and the following disclaimer in the 
/// documentation and/or materials provided with the distribution.
/// 
/// 3. In addition, redistributions of modified forms of the source or 
/// binary code must carry prominent notices stating that the original 
/// code was changed and the date of the change.
/// 
/// 4. All publications or advertising materials mentioning features or use 
/// of this software must acknowledge that it was developed by the National 
/// Center for Supercomputing Applications at the University of Illinois, 
/// and credit the Contributors.
/// 
/// 5. Neither the name of the University nor the names of the Contributors 
/// may be used to endorse or promote products derived from this software 
/// without specific prior written permission from the University or the 
/// Contributors.
/// 
/// DISCLAIMER
/// 
/// THIS SOFTWARE IS PROVIDED BY THE UNIVERSITY AND THE CONTRIBUTORS "AS IS" 
/// WITH NO WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED.  In no event 
/// shall the University or the Contributors be liable for any damages 
/// suffered by the users arising out of the use of this software, even if 
/// advised of the possibility of such damage. 
/// </summary>

//package ncsa.hdf.util;
namespace RTi.Util.GUI
{

	/// <summary>
	/// A TreeNode class was written by Sandip Chitale. This class will be used as
	/// a base clas to implemnt a node of the tree data structres like heirarchical
	/// file systems.
	/// 
	/// @author  HDF Group, NCSA. Modified by Peter Cao, Septemper 10, 1998.
	/// </summary>
	public class TreeNode
	{
		/// <summary>
		/// the node label </summary>
		internal string label;

		/// <summary>
		/// the node object defined by users </summary>
		internal object userObject;

		/// <summary>
		/// the default icon </summary>
		internal Image defaultIcon;

		/// <summary>
		/// the open folder icon </summary>
		internal Image expandedIcon;

		/// <summary>
		/// the icon for leaf node </summary>
		internal Image leafIcon;

		/// <summary>
		/// the level of the node in the tree </summary>
		internal int level;

		/// <summary>
		/// if the node is expanded </summary>
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		internal bool isExpanded_Renamed;

		/// <summary>
		/// creates a new tree node </summary>
		/// <param name="obj">            the node object </param>
		/// <param name="defaultImage">   the default image </param>
		/// <param name="expandedImage">  the expanded image </param>
		public TreeNode(object obj, Image defaultImage, Image expandedImage)
		{
			userObject = obj;
			defaultIcon = defaultImage;
			expandedIcon = expandedImage;
			leafIcon = defaultImage;
			level = -1;
			isExpanded_Renamed = false;

			if (userObject != null)
			{
				label = userObject.ToString();
			}
			else
			{
				label = "";
			}
		}

		/// <summary>
		/// create the tree node with specified node object and the default image </summary>
		/// <param name="obj">          the node object </param>
		/// <param name="defaultImage"> the default image </param>
		public TreeNode(object obj, Image defaultImage) : this(obj, defaultImage, null)
		{
		}

		/// <summary>
		/// adds the node into the tree, derived class should override it
		/// </summary>
		public virtual void added()
		{
		}

		/// <summary>
		/// deletes the node from the tree, derived class should override it
		/// </summary>
		public virtual void deleted()
		{
		}

		/// <summary>
		/// selects the node in the tree, derived class should override it
		/// </summary>
		public virtual void select()
		{
		}

		/// <summary>
		/// expands or collapses the node, derived class should override it
		/// </summary>
		public virtual void expandCollapse()
		{
			if (isExpandable())
			{
				toggleExpanded();
			}
		}

		/// <summary>
		/// checks if the node is expandable </summary>
		public virtual bool isExpandable()
		{
			return (!(expandedIcon == null));
		}

		/// <summary>
		/// set the expand image
		/// </summary>
		/// <param name="expandedImage"> the expanded image </param>
		public virtual void setExpandable(Image expandedImage)
		{
			expandedIcon = expandedImage;
		}

		/// <summary>
		/// check if the node is expanded </summary>
		public virtual bool isExpanded()
		{
			return isExpanded_Renamed;
		}

		/// <summary>
		/// set the expanded status
		/// </summary>
		/// <param name="b">  indicator if the node is expanded </param>
		public virtual void setExpanded(bool b)
		{
		  if (isExpandable())
		  {
			  isExpanded_Renamed = b;
		  }
		}

		public virtual void toggleExpanded()
		{
		  if (isExpanded())
		  {
			  setExpanded(false);
		  }
		  else
		  {
			  setExpanded(true);
		  }
		}

		/// <summary>
		/// get the node object </summary>
		public virtual object getObject()
		{
			return userObject;
		}

		/// <summary>
		/// get the default image </summary>
		public virtual Image getDefaultImage()
		{
			return defaultIcon;
		}

		/// <summary>
		/// set the default image </summary>
		public virtual void setDefaultImage(Image defaultImage)
		{
			defaultIcon = defaultImage;
		}

		/// <summary>
		/// set the leaf image </summary>
		public virtual void setLeafImage(Image img)
		{
			leafIcon = img;
		}

		/// <summary>
		/// get the leaf image </summary>
		public virtual Image getLeafImage()
		{
			return leafIcon;
		}

		/// <summary>
		/// get the node level </summary>
		public virtual int getLevel()
		{
			return level;
		}

		/// <summary>
		/// set the level of the node </summary>
		/// <param name="level"> the level value of the node  </param>
		public virtual void setLevel(int level)
		{
			this.level = level;
		}

		/// <summary>
		/// sets the label of the node </summary>
		///  <param name="label"> the label </param>
		public virtual void setLabel(string label)
		{
			this.label = label;
		}

		/// <summary>
		/// gets the label of the node
		/// </summary>
		public virtual string getLabel()
		{
			return label;
		}

		/// <summary>
		/// get the collapse image(expanded). </summary>
		/// <returns> the image </returns>
		public virtual Image getCollapseImage()
		{
			return expandedIcon;
		}

		/// <summary>
		/// Returns the parameter string representing the state of this 
		/// node. This string is useful for debugging. </summary>
		/// <returns>     the parameter string of this node. </returns>
		protected internal virtual string paramString()
		{
		return "isExpandable=" + isExpandable()+
				",isExpanded=" + isExpanded() +
				",level=" + level;
		}

		/// <summary>
		/// Returns a string representation of this node and its values. </summary>
		/// <returns>    a string representation of this node. </returns>
		public override string ToString()
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		return this.GetType().FullName + "[" + paramString() + "]";
		}

		public virtual object getUserObject()
		{
			return userObject;
		}
	}

}
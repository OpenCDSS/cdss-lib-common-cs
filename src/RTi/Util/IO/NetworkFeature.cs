using System.Collections.Generic;

// NetworkFeature - a representation of a feature (e.g., Node, Link) in a network

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
// NetworkFeature - a representation of a feature (e.g., Node, Link) in a
//			network
// ----------------------------------------------------------------------------
// History:
// 
// 2003-07-28	Steven A. Malers,	Initial version - copy and modify
//		Riverside Technology,	HBNode.
//		inc.
// 2005-04-26	J. Thomas Sapienza, RTi	Added finalize().
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
// ----------------------------------------------------------------------------

namespace RTi.Util.IO
{

	using GRDrawingArea = RTi.GR.GRDrawingArea;

	/// <summary>
	/// This class is the base class for network features, including the Node and Link
	/// objects, for use in NodeNetwork and other collections.
	/// REVISIT JAVADOC: see NodeNetwork
	/// </summary>
	public abstract class NetworkFeature : object
	{

	// TODO SAM 2007-05-09 - put these in the NodeNetwork class so only one list needs to be
	// initialized.
	//private int [] __feature_types = null;		// An array of allowed feature
							// types (e.g., node type
							// numbers).  To be set
							// in the derived class.
	private string[] __feature_names = null; // An array of allowed feature
							// names (e.g., short names
							// corresponding to node types).


	/// <summary>
	/// Feature type (e.g., "Diversion").
	/// </summary>
	protected internal string _type = "";

	/// <summary>
	/// Identifier for the feature (e.g., a short string).
	/// </summary>
	protected internal string _id = "";

	/// <summary>
	/// Name for the feature (e.g., a longer string).
	/// </summary>
	protected internal string _name = "";

	/// <summary>
	/// X-coordinate for feature.  REVISIT - should features be derived from GRShape?
	/// </summary>
	public double x = 0.0;

	/// <summary>
	/// Y-coordinate for feature.  REVISIT - should features be derived from GRShape?
	/// </summary>
	public double y = 0.0;

	/// <summary>
	/// Properties associated with the feature.
	/// </summary>
	// REVISIT - not needed if use derived classes for each specific node type?
	//protected PropList _props = null;

	/// <summary>
	/// Upstream features.
	/// </summary>
	protected internal IList<NetworkFeature> _upstream_feature_Vector;

	/// <summary>
	/// Downstream features.
	/// </summary>
	protected internal IList<NetworkFeature> _downstream_feature_Vector;

	/// <summary>
	/// Object that is associated with the node (e.g., external data object).
	/// </summary>
	protected internal object _data;

	/// <summary>
	/// Temporary data used when processing a network, to indicate that the feature
	/// has been processed.   This may be difficult because of the linkage between
	/// features in the network.  This is meant to be used by higher-level code.
	/// </summary>
	protected internal bool _processed = false;

	/// <summary>
	/// Construct a feature. </summary>
	/// <param name="id"> Feature identifier. </param>
	/// <param name="name"> Feature name. </param>
	/// <param name="type"> Feature type. </param>
	public NetworkFeature(string id, string name, string type)
	{
		initialize();
		// Use these to deal with nulls - don't want null strings.
		setID(id);
		setName(name);
		setType(type);
	}

	//REVISIT - put in the network?
	/// <summary>
	/// Add a NetworkFeature downstream from this feature. </summary>
	/// <param name="downstream_feature"> Downstream feature to add. </param>
	/*
	public void addDownstreamFeature ( NetworkFeature downstream_feature )
	{	String	routine = "NetworkFeature.addDownstreamFeature";
		int	dl =50;
	
		try {	if ( Message.isDebugOn ) {
				Message.printDebug ( dl, routine,
				"Adding \"" + downstream_feature.getID() +
				"\" downstream of \"" + getID() + "\"" );
			}
			NetworkFeature old_downstream_feature = _downstream;
			if ( _downstream != null ) {
				// There is a downstream node and we need to reconnect it...
		
				// For the original downstream node, reset its upstream
				// reference to the new node.  Use the common identifier to
				// find the element to reset...
				int pos = _downstream.getUpstreamNodePosition (
						getCommonID() );
				if ( pos >= 0 ) {
					Vector downstream_upstream =
					_downstream.getUpstreamNodes();
					if ( downstream_upstream != null ) {
						downstream_upstream.setElementAt(
						downstream_node, pos);
					}
				}
				// Connect the new downstream node to this node.
				_downstream = downstream_node;
				// Set the upstream node of the new downstream node to point to
				// this node.  For now, assume that the node that is being
				// inserted is a new node...
				if ( downstream_node.getNumUpstreamNodes() > 0 ) {
					Message.printWarning ( 1, routine,
					"Node \"" + downstream_node.getCommonID() +
					"\" has #upstream > 0" );
				}
				// Set the new downstream node data...
				downstream_node.setDownstreamNode ( old_downstream_node );
				downstream_node.addUpstreamNode ( this );
				// Set the new current node data...
				_tributary_number = downstream_node.getNumUpstreamNodes();
			}
			else {	// We always need to do this step...
				downstream_node.addUpstreamNode ( this );
			}
			String downstream_commonid = null;
			if ( downstream_node.getDownstreamNode() != null ) {
				downstream_commonid = old_downstream_node.getCommonID();
			}
			if ( Message.isDebugOn ) {
				Message.printDebug ( dl, routine,
				"\"" + downstream_node.getCommonID() +
				"\" is downstream of \"" +
				getCommonID() + "\" and upstream of \"" +
				downstream_commonid + "\"" );
			}
			return 0;
		}
		catch ( Exception e ) {
			Message.printWarning ( 2, routine,
			"Error adding downstream node." );
			return 1;
		}
	}
	*/

	/// <summary>
	/// Add a node upstream from this node. </summary>
	/// <param name="upstream_node"> Node to add upstream. </param>
	/// <returns> 0 if successful, 1 if not. </returns>
	/* REVISIT - put in the network?
	public int addUpstreamNode ( HBNode upstream_node )
	{	String	routine = "HBNode.addUpstreamNode";
		int	dl = 50;
	
		// Add the node to the vector...
	
		try {
		if ( Message.isDebugOn ) {
			Message.printDebug ( dl, routine,
			"Adding \"" + upstream_node.getCommonID() +
			"\" upstream of \"" + getCommonID() + "\"" );
		}
		if ( _upstream == null ) {
			// Need to allocate space for it...
			_upstream = new Vector ( 1, 1 );
		}
	
		_upstream.addElement(upstream_node);
	
		// Make so the upstream node has this node as its downstream node...
	
		upstream_node.setDownstreamNode ( this );
		if ( Message.isDebugOn ) {
			Message.printDebug ( dl, routine, "\"" +
			upstream_node.getCommonID() + "\" downstream is \"" +
			getCommonID() + "\"" );
		}
		return 0;
		}
		catch ( Exception e ) {
			Message.printWarning ( 2, routine,
			"Error adding upstream node." );
			return 1;
		}
	}
	*/

	/// <summary>
	/// Break the link with an upstream node. </summary>
	/// <param name="upstream_node"> Upstream node to disconnect from the network. </param>
	/// <returns> 0 if successful, 1 if not. </returns>
	/* REVISIT - put int the network?
	public int deleteUpstreamNode ( HBNode upstream_node )
	{	String routine = "HBNode.deleteUpstreamNode";
	
		// Find a matching node.  Just check addesses...
	
		try {
		for ( int i = 0; i < _upstream.size(); i++ ) {
			if ( upstream_node.equals((HBNode)_upstream.elementAt(i)) ) {
				// We have found a match.  Delete the element...
				_upstream.removeElementAt(i);
				return 0;
			}
		}
		return 1;
		}
		catch ( Exception e ) {
			Message.printWarning ( 2, routine,
			"Error deleting upstream node." );
			return 1;
		}
	}
	*/

	/// <summary>
	/// Finalize before garbage collection.
	/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~NetworkFeature()
	{
		IOUtil.nullArray(__feature_names);
		_type = null;
		_id = null;
		_name = null;
		_upstream_feature_Vector = null;
		_downstream_feature_Vector = null;
		_data = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the identifier for the feature. </summary>
	/// <returns> the identifier for the feature. </returns>
	public virtual string getID()
	{
		return _id;
	}

	/// <summary>
	/// Return the name for the feature. </summary>
	/// <returns> the name for the feature. </returns>
	public virtual string getDescription()
	{
		return _name;
	}

	/// <summary>
	/// Return the downstream feature or null if not available. </summary>
	/// <param name="index"> of the downstream feature (typically 0 unless there is a
	/// divergence). </param>
	public virtual NetworkFeature getDownstreamFeature(int index)
	{
		if ((index < 0) || (index >= _downstream_feature_Vector.Count))
		{
			return null;
		}
		return (NetworkFeature)_downstream_feature_Vector[index];
	}

	/// <summary>
	/// Return the number of downstream features. </summary>
	/// <returns> the number of downstream features. </returns>
	public virtual int getNumDownstreamFeatures()
	{
		return _downstream_feature_Vector.Count;
	}

	/// <summary>
	/// Return the number of upstream features. </summary>
	/// <returns> the number of upstream features. </returns>
	public virtual int getNumUpstreamFeatures()
	{
		return _upstream_feature_Vector.Count;
	}

	/// <summary>
	/// Return the feature type (e.g., node type). </summary>
	/// <returns> the feature type (e.g., node type). </returns>
	public virtual string getType()
	{
		return _type;
	}

	/// <summary>
	/// Return the upstream feature or null if not available. </summary>
	/// <param name="index"> of the upstream feature (typically 0 unless the feature is a
	/// convergence). </param>
	public virtual NetworkFeature getUpstreamFeature(int index)
	{
		if ((index < 0) || (index >= _upstream_feature_Vector.Count))
		{
			return null;
		}
		return (NetworkFeature)_upstream_feature_Vector[index];
	}

	/// <summary>
	/// Initialize data members.
	/// </summary>
	private void initialize()
	{
		_downstream_feature_Vector = new List<NetworkFeature>();
		_upstream_feature_Vector = new List<NetworkFeature>();
	}

	/// <summary>
	/// Indicate whether the feature has been processed. </summary>
	/// <returns> true if the feature has been processed, false if not. </returns>
	public virtual bool isProcessed()
	{
		return _processed;
	}

	/// <summary>
	/// Render the feature (draw itself). </summary>
	/// <param name="da"> GRDrawingArea to draw to. </param>
	public virtual void render(GRDrawingArea da)
	{
	}

	/* REVISIT 
	public void setDownstreamNode ( HBNode downstream )
	{
		_downstream = downstream;
	}
	*/

	/// <summary>
	/// Set the feature identifier. </summary>
	/// <param name="id"> Feature identifier. </param>
	public virtual void setID(string id)
	{
		if (!string.ReferenceEquals(id, null))
		{
			_id = id;
		}
	}

	/// <summary>
	/// Set the feature name. </summary>
	/// <param name="name"> Feature name. </param>
	public virtual void setName(string name)
	{
		if (!string.ReferenceEquals(name, null))
		{
			_name = name;
		}
	}

	/// <summary>
	/// Set whether the feature has been processed. </summary>
	/// <param name="processed"> true if the feature has been processed, false if not. </param>
	public virtual void setProcessed(bool processed)
	{
		_processed = processed;
	}

	/// <summary>
	/// Set the feature type. </summary>
	/// <param name="type"> Feature type. </param>
	public virtual void setType(string type)
	{
		if (!string.ReferenceEquals(type, null))
		{
			_type = type;
		}
	}

	/// <summary>
	/// Return a verbose string representation of the object. </summary>
	/// <returns> a verbose string representation of the object. </returns>
	public override string ToString()

	{
	/* REVISIT
		String up = "";
		if ( _downstream != null ) {
			down = _downstream.getCommonID();
		}
		else {	down = "null";
		}
		if ( _upstream != null ) {
			for ( int i = 0; i < getNumUpstreamNodes(); i++ ) {
				up = up + " [" + i + "]:\"" +
				getUpstreamNode(i).getCommonID() + "\"";
			}
		}
		else {	up = "null";
		}
		return "\"" + getCommonID() + "\" T=" + getTypeString(_type,1) +
		" T#=" + _tributary_number + " RC=" + _reach_counter + " RL=" +
		_reach_level + " #=" + _serial + " #inR=" + _node_in_reach_number +
		" CO=" + _computational_order +
		" DWN=\"" + down + "\" #up=" + getNumUpstreamNodes() + " UP=" + up;
		*/
		return _id;
	}

	} // End of NetworkFeature

}
using System;
using System.Collections.Generic;
using System.Text;

// DataSet - class to manage a list of DataSetComponent

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

namespace RTi.Util.IO
{


	using Message = RTi.Util.Message.Message;

	using StringUtil = RTi.Util.String.StringUtil;

	/// <summary>
	/// This DataSet class manages a list of DataSetComponent, typically for use with
	/// a model where each component corresponds to a file or section of data within
	/// a file.  This class should be extended to provide specific functionality for a data set.
	/// </summary>
	public abstract class DataSet
	{

	/// <summary>
	/// Base name for data set, used to provide default file names when creating new files.
	/// </summary>
	private string __basename = "";

	/// <summary>
	/// List of data components in the data set.  Each component is a type that is described in the
	/// lookup arrays for the data set, and has data for the component.  Components are hierarchical and
	/// therefore the top level components will contain groups.
	/// </summary>
	private IList<DataSetComponent> __components = null;

	/// <summary>
	/// Array of component names, used in lookups.
	/// </summary>
	protected internal string[] _component_names = null;

	/// <summary>
	/// Array of component types (as integers), corresponding to the component names.
	/// </summary>
	protected internal int[] _component_types = null;

	/// <summary>
	/// Array of component types (as integers) that are group components.
	/// </summary>
	protected internal int[] _component_groups = null;

	/// <summary>
	/// Array of component types (as integers) that indicates the group components for each component.
	/// </summary>
	protected internal int[] _component_group_assignments = null;

	/// <summary>
	/// Array of component types (as integers) that indicates the primary components for each group.
	/// These components are used to get the list of object identifiers for displays and processing.
	/// </summary>
	protected internal int[] _component_group_primaries = null;

	/// <summary>
	/// Directory for data set.
	/// </summary>
	private string __dataset_dir = "";

	/// <summary>
	/// Name of the data set file (XML file).
	/// </summary>
	private string __dataset_filename = "";

	// TODO - evaluate switching this to a String - it is not checked as often
	// as the component types are
	/// <summary>
	/// Data set type.  The derived class can use this to define specific data set
	/// types.  The value is initialized to -1.
	/// </summary>
	private int __type = -1;

	/// <summary>
	/// Construct a blank data set.  It is expected that other information will be set
	/// during further processing.  Component groups are not initialized until a data set type is set.
	/// </summary>
	public DataSet()
	{
		__components = new List<DataSetComponent>();
	}

	/// <summary>
	/// Construct a blank data set.  It is expected that other information will be set
	/// during further processing. </summary>
	/// <param name="component_types"> Array of sequential integers (0...N) that are used to
	/// identify components.  Integers are used to optimize processing in classes that
	/// use the data set.  Components can be groups or individual components. </param>
	/// <param name="component_names"> Array of String component names, suitable for use in
	/// displays and messages. </param>
	/// <param name="component_groups"> A subset of the component_types array, in the same order
	/// as component_types, indicating the components that are group components. </param>
	/// <param name="component_group_assignments"> An array of integers containing values for
	/// each value in component_types.  The values should be the component group for
	/// each individual component. </param>
	/// <param name="component_group_primaries"> An array of integers, having the same length
	/// as component_groups, indicating the components within the group that are the
	/// primary components.  One primary component should be identified for each group
	/// and the primary component will be used to supply a list of objects/identifiers
	/// to create the list of objects identifiers in the group. </param>
	public DataSet(int[] component_types, string[] component_names, int[] component_groups, int[] component_group_assignments, int[] component_group_primaries)
	{
		__components = new List<DataSetComponent>();
		_component_types = component_types;
		_component_names = component_names;
		_component_groups = component_groups;
		_component_group_assignments = component_group_assignments;
		_component_group_primaries = component_group_primaries;
	}

	/// <summary>
	/// Construct a blank data set.  Specific output files, by default, will use the
	/// output directory and base file name in output file names.  The derived class
	/// method should initialize the specific data components. </summary>
	/// <param name="type"> Data set type. </param>
	/// <param name="dataset_dir"> Data set directory. </param>
	/// <param name="basename"> Basename for files (no directory). </param>
	public DataSet(int type, string dataset_dir, string basename)
	{
		__type = type;
		__dataset_dir = dataset_dir;
		__basename = basename;
		__components = new List<DataSetComponent>();
	}

	/// <summary>
	/// Add a component to the data set. </summary>
	/// <param name="comp"> Component to add. </param>
	public virtual void addComponent(DataSetComponent comp)
	{
		__components.Add(comp);
	}

	/// <summary>
	/// Determine whether a data component has data.  A check is made for a non-null data object. </summary>
	/// <param name="component_type"> The component type to evaluate. </param>
	/// <returns> true if a data component has a non-null data object, false if not.
	/// Return false if the component does not exist in the data set. </returns>
	public virtual bool componentHasData(int component_type)
	{
		DataSetComponent comp = getComponentForComponentType(component_type);
		if (comp == null)
		{
			return false;
		}
		if (comp.getData() != null)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Finalize before garbage collection. </summary>
	/// <exception cref="Throwable"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~DataSet()
	{
		__basename = null;
		__components = null;
		// TODO SAM 2005-05-21 The following sets static component names to
		// null!  Subsequent access of the component names do not work.
		//IOUtil.nullArray(_component_names);
		_component_types = null;
		_component_groups = null;
		_component_group_assignments = null;
		_component_group_primaries = null;
		__dataset_dir = null;
		__dataset_filename = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Return the base name for the data set. </summary>
	/// <returns> the base name for the data set. </returns>
	public virtual string getBaseName()
	{
		return __basename;
	}

	/// <summary>
	/// Return the component at an index position within the data set.  This is useful
	/// for iteration through the components. </summary>
	/// <returns> the component at an index position within the data set (can be null). </returns>
	/// <param name="pos"> Index position in the component vector. </param>
	public virtual DataSetComponent getComponentAt(int pos)
	{
		return __components[pos];
	}

	/// <summary>
	/// Return the full path to the component data file.  If the original file name
	/// was set as absolute, then it is returned.  Otherwise, the data set directory
	/// and component data file name are joined. </summary>
	/// <returns> the full path to the component data file or null if it cannot be determined. </returns>
	/// <param name="comp_type"> Component type. </param>
	public virtual string getComponentDataFilePath(int comp_type)
	{
		DataSetComponent comp = getComponentForComponentType(comp_type);
		if (comp == null)
		{
			return null;
		}
		return getComponentDataFilePath(comp);
	}

	/// <summary>
	/// Return the full path to the component data file.  If the original file name
	/// was set as absolute, then it is returned.  Otherwise, the data set directory
	/// and component data file name are joined. </summary>
	/// <returns> the full path to the component data file or null if it cannot be determined. </returns>
	/// <param name="comp"> Component. </param>
	public virtual string getComponentDataFilePath(DataSetComponent comp)
	{
		File f = new File(comp.getDataFileName());
		if (f.isAbsolute())
		{
			return comp.getDataFileName();
		}
		return __dataset_dir + File.separator + comp.getDataFileName();
	}

	/// <summary>
	/// Return the component for the requested data component name. </summary>
	/// <returns> the component for the requested data component name or null if the
	/// component is not in the data set. </returns>
	/// <param name="name"> Component name. </param>
	public virtual DataSetComponent getComponentForComponentName(string name)
	{
		int size = __components.Count;
		DataSetComponent comp = null;
		IList<DataSetComponent> v;
		int size2;
		for (int i = 0; i < size; i++)
		{
			comp = __components[i];
			if (comp.getComponentName().Equals(name, StringComparison.OrdinalIgnoreCase))
			{
				return comp;
			}
			// If the component is a group and did not match the type, check
			// the sub-types in the component...
			if (comp.isGroup())
			{
				v = (System.Collections.IList)comp.getData();
				size2 = 0;
				if (v != null)
				{
					size2 = v.Count;
				}
				for (int j = 0; j < size2; j++)
				{
					comp = v[j];
					if (comp.getComponentName().Equals(name, StringComparison.OrdinalIgnoreCase))
					{
						return comp;
					}
				}
			}
		}
		return null;
	}

	/// <summary>
	/// Return the component for the requested data component type. </summary>
	/// <returns> the component for the requested data component type or null if the
	/// component is not in the data set. </returns>
	/// <param name="type"> Component type. </param>
	public virtual DataSetComponent getComponentForComponentType(int type)
	{
		int size = __components.Count;
		DataSetComponent comp = null;
		IList<DataSetComponent> v;
		int size2;
		//Message.printStatus ( 2, "", "looking up component " + type );
		for (int i = 0; i < size; i++)
		{
			comp = __components[i];
			//Message.printStatus ( 2, "", "Checking " + comp.getComponentType() );
			if (comp.getComponentType() == type)
			{
				return comp;
			}
			// FIXME SAM 2010-12-1 The following does not look right - why is it getting the component
			// data instead of dealing with group/type?
			// If the component is a group and did not match the type, check
			// the sub-types in the component...
			if (comp.isGroup())
			{
				v = (System.Collections.IList)comp.getData();
				size2 = 0;
				if (v != null)
				{
					size2 = v.Count;
				}
				for (int j = 0; j < size2; j++)
				{
					//Message.printStatus ( 2, "", "Checking " + comp.getComponentType() );
					comp = v[j];
					if (comp.getComponentType() == type)
					{
						return comp;
					}
				}
			}
		}
		return null;
	}

	/// <summary>
	/// Return the list of data components. </summary>
	/// <returns> the list of data components. </returns>
	public virtual IList<DataSetComponent> getComponents()
	{
		return __components;
	}

	/// <summary>
	/// Return the data components list for components in the specified group. </summary>
	/// <returns> the data components list for components in the specified group. </returns>
	public virtual IList<DataSetComponent> getComponentsForGroup(DataSetComponent groupComp)
	{
		IList<DataSetComponent> componentsInGroup = new List<object>();
		if ((groupComp == null) || !groupComp.isGroup())
		{
			return componentsInGroup;
		}
		for (int i = 0; i < _component_names.Length; i++)
		{
			if (i == groupComp.getComponentType())
			{
				// Don't compare to self
				continue;
			}
			int groupNum = _component_group_assignments[i];
			if ((groupNum >= 0) && (groupNum == groupComp.getComponentType()))
			{
				componentsInGroup.Add(getComponentForComponentType(i));
			}
		}
		return componentsInGroup;
	}

	/// <summary>
	/// Return the data components list for component that are groups. </summary>
	/// <returns> the data components list for component that are groups. </returns>
	public virtual IList<DataSetComponent> getComponentGroups()
	{
		int size = __components.Count;
		IList<DataSetComponent> v = new List<DataSetComponent>();
		DataSetComponent comp = null;
		for (int i = 0; i < size; i++)
		{
			comp = __components[i];
			if (comp.isGroup())
			{
				v.Add(comp);
			}
		}
		return v;
	}

	/// <summary>
	/// Return the directory for the data set. </summary>
	/// <returns> the directory for the data set. </returns>
	public virtual string getDataSetDirectory()
	{
		return __dataset_dir;
	}

	/// <summary>
	/// Determine the full path to a component data file. </summary>
	/// <param name="file"> File name (e.g., from component getDataFileName()). </param>
	/// <returns> Full path to the data file. </returns>
	public virtual string getDataFilePath(string file)
	{
		File f = new File(file);
		if (f.isAbsolute())
		{
			return file;
		}
		else
		{
			return __dataset_dir + File.separator + file;
		}
	}

	/// <summary>
	/// Return the name of the data set file name, which is the entry point into the data set. </summary>
	/// <returns> the name of the data set file name, which is the entry point into the data set. </returns>
	public virtual string getDataSetFileName()
	{
		return __dataset_filename;
	}

	/// <summary>
	/// Return the data set type. </summary>
	/// <returns> the data set type. </returns>
	public virtual int getDataSetType()
	{
		return __type;
	}

	/// <summary>
	/// Return the data set type name.  This method calls lookupDataSetName() for the instance. </summary>
	/// <returns> the data set type name. </returns>
	/* TODO - for now put in extended class because no generic way has been added
	here to keep track of different data set types
	public String getDataSetName ()
	{	return lookupDataSetName ( __type );
	}
	*/

	/// <summary>
	/// Initialize the data set.  This method should be defined in the extended class.
	/// </summary>
	//public abstract void initializeDataSet ( );

	/// <summary>
	/// Determine the component group type for a component type.  For example, use this
	/// to determine the group to add input components to when reading an input file. </summary>
	/// <param name="component_type"> The component type (should not be a group component). </param>
	/// <returns> the component group type for the component or -1 if a component group cannot be determined. </returns>
	public virtual int lookupComponentGroupTypeForComponent(int component_type)
	{
		if ((component_type < 0) || (component_type >= _component_group_assignments.Length))
		{
			return -1;
		}
		else
		{
			return _component_group_assignments[component_type];
		}
	}

	/// <summary>
	/// Return the component name given its number. </summary>
	/// <returns> the component name given its number or null if the component type is not found. </returns>
	public virtual string lookupComponentName(int component_type)
	{ // The component types are not necessarily numbers that match array indices
		// so match the type values
		for (int i = 0; i < _component_types.Length; i++)
		{
			if (component_type == _component_types[i])
			{
				return _component_names[i];
			}
		}
		return null;
	}

	/// <summary>
	/// Return the numeric component type given its string name. </summary>
	/// <returns> the numeric component type given its string type, or -1 if not found. </returns>
	/// <param name="component_name"> the component tag from the response file. </param>
	public virtual int lookupComponentType(string component_name)
	{
		for (int i = 0; i < _component_types.Length; i++)
		{
			if (_component_names[i].Equals(component_name, StringComparison.OrdinalIgnoreCase))
			{
				return _component_types[i]; // The _component_names and _component_types arrays must align
			}
		}
		return -1;
	}

	/// <summary>
	/// Determine the primary data set component for a component group.  This is used,
	/// for example, as the component in the group that will supply the list of
	/// objects when no list file is available. </summary>
	/// <param name="component_type"> The component type (should be a group component). </param>
	/// <returns> the component type for the primary component in a group or -1 if a
	/// primary component cannot be determined. </returns>
	public virtual int lookupPrimaryComponentTypeForComponentGroup(int component_type)
	{ // First get the group for the component...
		int compgroup = lookupComponentGroupTypeForComponent(component_type);
		if (compgroup < 0)
		{
			return -1;
		}

		// Now find the matching group...

		int size = 0;
		if (_component_groups != null)
		{
			size = _component_groups.Length;
		}
		for (int i = 0; i < size; i++)
		{
			if (_component_groups[i] == compgroup)
			{
				if ((i >= _component_group_primaries.Length))
				{
					return -1;
				}
				else
				{
					return _component_group_primaries[i];
				}
			}
		}
		return -1;
	}

	/// <summary>
	/// Indicate whether any components in the data set are dirty (data have been modified in memory). </summary>
	/// <returns> true if any files in the data set are dirty. </returns>
	public virtual bool isDirty()
	{
		int size = __components.Count;
		DataSetComponent comp = null;
		IList<DataSetComponent> v;
		int size2;
		for (int i = 0; i < size; i++)
		{
			comp = __components[i];
			if (comp.isDirty())
			{
				if (Message.isDebugOn)
				{
					Message.printDebug(1, "", "Component [" + i + "] " + comp.getComponentName() + " is dirty.");
				}
				return true;
			}
			// If the component is a group and it was not dirty (above), check the sub-components...
			if (comp.isGroup())
			{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<DataSetComponent> v0 = (java.util.List<DataSetComponent>)comp.getData();
				IList<DataSetComponent> v0 = (IList<DataSetComponent>)comp.getData(); // Group data is the list of components in the group
				v = v0;
				size2 = 0;
				if (v != null)
				{
					size2 = v.Count;
				}
				for (int j = 0; j < size2; j++)
				{
					comp = v[j];
					if (comp.isDirty())
					{
						if (Message.isDebugOn)
						{
							Message.printDebug(1, "", "Group sub-component " + comp.getComponentName() + " is dirty.");
						}
						return true;
					}
				}
			}
		}
		return false;
	}

	// TODO - Need to evaluate whether this NEEDS TO BE IN EACH DERIVED CLASS.
	/// <summary>
	/// Process an XML Document node during the read process. </summary>
	/// <param name="dataset"> DataSet that is being read. </param>
	/// <param name="node"> an XML document node, which may have children. </param>
	/// <exception cref="Exception"> if there is an error processing the node. </exception>
	/* TODO
	private static void processDocumentNodeForRead ( DataSet dataset, Node node )
	throws Exception
	{	String routine = "DataSet.processDocumentNodeForRead";
		switch ( node.getNodeType() ) {
			case Node.DOCUMENT_NODE:
				// The main data set node.  Get the data set type, etc.
				processDocumentNodeForRead( dataset, ((Document)node).getDocumentElement() );
				break;
			case Node.ELEMENT_NODE:
				// Data set components.  Print the basic information...
				String element_name = node.getNodeName();
				Message.printStatus ( 1, routine, "Element name: " + element_name );
				NamedNodeMap attributes;
				Node attribute_Node;
				String attribute_name, attribute_value;
				// Evaluate the nodes attributes...
				if ( element_name.equalsIgnoreCase("DataSet") ){
					attributes = node.getAttributes();
					int nattributes = attributes.getLength();
					for ( int i = 0; i < nattributes; i++ ) {
						attribute_Node = attributes.item(i);
						attribute_name = attribute_Node.getNodeName();
						if ( attribute_name.equalsIgnoreCase("Type" ) ) {
							try {
								dataset.setComponentType ( attribute_Node.getNodeValue(), true );
							}
							catch ( Exception e ) {
								Message.printWarning ( 2, routine, "Data set type \"" + attribute_name +
								"\" is not recognized." );
								throw new Exception ( "Error processing data set" );
							}
						}
						else if (
							attribute_name.equalsIgnoreCase( "BaseName" ) ) {
							dataset.setBaseName ( attribute_Node.getNodeValue() );
						}
					}
				}
				else if ( element_name.equalsIgnoreCase(
					"DataSetComponent") ) {
					attributes = node.getAttributes();
					int nattributes = attributes.getLength();
					String comptype = "", compdatafile = "", complistfile = "", compcommandsfile ="";
					for ( int i = 0; i < nattributes; i++ ) {
						attribute_Node = attributes.item(i);
						attribute_name = attribute_Node.getNodeName();
						attribute_value = attribute_Node.getNodeValue();
						if ( attribute_name.equalsIgnoreCase("Type" ) ) {
							comptype = attribute_value;
						}
						else if(attribute_name.equalsIgnoreCase("DataFile" ) ) {
							compdatafile = attribute_value;
						}
						else if(attribute_name.equalsIgnoreCase("ListFile" ) ) {
							complistfile = attribute_value;
						}
						else if(attribute_name.equalsIgnoreCase("CommandsFile" ) ) {
							compcommandsfile = attribute_value;
						}
						else {
							Message.printWarning ( 2, routine, "Unrecognized attribute \"" + attribute_name+
							" for \"" + element_name +"\"");
						}
					}
					int component_type = lookupComponentType ( comptype );
					if ( component_type < 0 ) {
						Message.printWarning ( 2, routine,
						"Unrecognized data set component \"" + comptype + "\".  Skipping." );
						return;
					}
					// Add the component...
					DataSetComponent comp = new DataSetComponent ( component_type );
					comp.setDataFileName ( compdatafile );
					comp.setListFileName ( complistfile );
					comp.setCommandsFileName ( compcommandsfile );
					Message.printStatus ( 1, routine, "Adding new component for data \"" + compdatafile + "\" \"" );
					dataset.addComponent ( comp );
				}
				// The main document node will have a list of children
				// (data set components) but components will not.
				// Recursively process each node...
				NodeList children = node.getChildNodes();
				if ( children != null ) {
					int len = children.getLength();
					for ( int i = 0; i < len; i++ ) {
						processDocumentNodeForRead ( dataset, children.item(i) );
					}
				}
				break;
		}
	}
	*/

	// TODO - put in derived class
	/// <summary>
	/// Read a component.  This method should be defined in the extended class. </summary>
	/// <param name="comp"> DataSetComponent to read, using the file defined for the data set component. </param>
	/// <exception cref="Exception"> if an error occurs reading the component. </exception>
	//public abstract void readComponent ( DataSetComponent comp );

	/// <summary>
	/// Read a complete data set from an XML data set file. </summary>
	/// <param name="filename"> XML data set file to read. </param>
	/// <param name="read_all"> If true, all the data files mentioned in the response file will
	/// be read into memory, providing a complete data set for viewing and manipulation. </param>
	/// <exception cref="Exception"> if there is an error reading the file. </exception>
	/* TODO
	public static DataSet readXMLFile ( String filename, boolean read_all )
	throws Exception
	{	String routine = "DataSet.readXMLFile";
		String full_filename = IOUtil.getPathUsingWorkingDir ( filename );
	
		DOMParser parser = null;
		try {
			parser = new DOMParser();
			parser.parse ( full_filename );
		}
		catch ( Exception e ) {
			Message.printWarning ( 2, routine, "Error reading data set \"" + filename + "\"" );
			Message.printWarning ( 2, routine, e );
			throw new Exception ( "Error reading data set \"" + filename + "\"" );
		}
	
		// Create a new data set object...
	
		DataSet dataset = new DataSet();
		File f = new File ( full_filename );
		dataset.setDirectory ( f.getParent() );
		dataset.setDataSetFileName ( f.getName() );
	
		// Now get information from the document.  For now don't hold the
		// document as a data member...
	
		Document doc = parser.getDocument();
	
		// Loop through and process the document nodes, starting with the root node...
	
		processDocumentNodeForRead ( dataset, doc );
	
		// Synchronize the response file with the control file (for now just
		// check - need to decide how to make bulletproof)...
	
		/ * TODO
		StateCU_DataSetComponent comp = dataset.getComponentForComponentType (
			StateCU_DataSetComponent.TYPE_RESPONSE );
		if ( comp != null ) {
			StateCU_DataSet ds2 = readStateCUFile (
			comp.getDataFile(), false );
		}
		* /
	
		// Compare components and response file.  Need to REVISIT this.
	
		// Now just read the components - the assumption is that the data set
		// components are correct for the data set but need to tighten this down
	
		String read_warning = "";
		if ( read_all ) {
			Vector components = dataset.getComponents();
			int size = dataset.__components.size();
			String datafile = "";
			DataSetComponent comp;
			for ( int i = 0; i < size; i++ ) {
				comp = (DataSetComponent)components.elementAt(i);
				try {	datafile = comp.getDataFileName();
					f = new File(datafile);
					if ( !f.isAbsolute() ) {
						datafile = dataset.getDirectory() + File.separator + datafile;
					}
	/ * TODO
					if ( comp.getType() == StateCU_DataSetComponent.TYPE_CU_LOCATIONS ) {
						comp.setData (StateCU_Location.readStateCUFile(datafile));
					}
					else if (comp.getType() == StateCU_DataSetComponent.TYPE_CROP_CHARACTERISTICS ) {
						comp.setData ( StateCU_CropCharacteristics.readStateCUFile( datafile));
					}
					else if (comp.getType() == StateCU_DataSetComponent.TYPE_BLANEY_CRIDDLE ) {
						comp.setData ( StateCU_BlaneyCriddle.readStateCUFile(datafile));
					}
					else if (comp.getType() == StateCU_DataSetComponent.TYPE_CLIMATE_STATIONS ) {
						comp.setData ( StateCU_ClimateStation.readStateCUFile(datafile));
					}
	* /
				}
				catch ( Exception e ) {
					read_warning += "\n" + datafile;
					Message.printWarning ( 2, routine, e );
				}
			}
		}
		else {
			// Read the control file???
		}
		if ( read_warning.length() > 0 ) {
			Message.printWarning ( 1, routine, "Error reading data files:" + read_warning );
		}
	
		return dataset;
	}
	*/

	/// <summary>
	/// Set the base name for the data set. </summary>
	/// <param name="basename"> Base name for the data set. </param>
	public virtual void setBaseName(string basename)
	{
		__basename = basename;
	}

	/// <summary>
	/// Set the file name (no directory) for the data set (XML file). </summary>
	/// <param name="filename"> File for the data set. </param>
	public virtual void setDataSetFileName(string filename)
	{
		__dataset_filename = filename;
	}

	/// <summary>
	/// Set the directory for the data set. </summary>
	/// <param name="dir"> Directory for the data set. </param>
	public virtual void setDataSetDirectory(string dir)
	{
		__dataset_dir = dir;
	}

	/// <summary>
	/// Set a component dirty (edited).  This method is usually called by the
	/// set methods in the individual data object classes. </summary>
	/// <param name="component_type"> The component type within the data set. </param>
	/// <param name="is_dirty"> Flag indicating whether the component should be marked dirty. </param>
	public virtual void setDirty(int component_type, bool is_dirty)
	{
		DataSetComponent comp = getComponentForComponentType(component_type);
		if (comp != null)
		{
			comp.setDirty(is_dirty);
		}
	}

	/// <summary>
	/// Set the data set type. </summary>
	/// <param name="type"> Data set type. </param>
	/// <param name="initialize_components"> If true, the components are cleared and the
	/// component groups for the type are initialized by calling the
	/// initializeDataSet() method, which should be defined in the extended class. </param>
	/// <exception cref="Exception"> if there is an error setting the data type or initializing the component groups. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setDataSetType(int type, boolean initialize_components) throws Exception
	public virtual void setDataSetType(int type, bool initialize_components)
	{
		__type = type;
		if (initialize_components)
		{
			__components.Clear();
		}
		//initializeDataSet ();
	}

	/// <summary>
	/// Set the data set type </summary>
	/// <param name="type"> Data set type. </param>
	/// <exception cref="Exception"> if the data type string is not recognized. </exception>
	/// <param name="initialize_components"> If true, the components are cleared and the
	/// component groups for the type are initialized. </param>
	/* TODO - can this be put here?  For now but in derived class
	public void setDataSetType ( String type, boolean initialize_components )
	throws Exception
	{	int itype = lookupDataSetType ( type );
		if ( itype < 0 ) {
			throw new Exception ( "Data set type \"" + type + "\" is not recognized." );
		}
		setDataSetType ( itype, initialize_components );
	}
	*/

	/// <summary>
	/// Return a string representation of the data set (e.g., for debugging). </summary>
	/// <returns> a string representation of the data set. </returns>
	public override string ToString()
	{
		IList<DataSetComponent> componentList = getComponents();
		IList<DataSetComponent> v;
		int size2;
		StringBuilder buffer = new StringBuilder();
		foreach (DataSetComponent comp in componentList)
		{
			buffer.Append("\nDataSetComponent:  ");
			if (comp == null)
			{
				buffer.Append("null\n");
			}
			else
			{
				buffer.Append(comp.getComponentName() + "\n");
				buffer.Append("    Type:       " + comp.getComponentType() + "\n");
				buffer.Append("    Is group:   " + comp.isGroup() + "\n");
				buffer.Append("    Is dirty:   " + comp.isDirty() + "\n");
				buffer.Append("    Is visible: " + comp.isDirty() + "\n");
			}
			// TODO SAM 2011-01-17 This does not seem right - mixing data (sub) components and data lists.
			if (comp.isGroup())
			{
				v = (System.Collections.IList)comp.getData();
				size2 = 0;
				if (v != null)
				{
					size2 = v.Count;
				}
				for (int j = 0; j < size2; j++)
				{
					comp = v[j];
					buffer.Append("    SubComponent:  ");
					if (comp == null)
					{
						buffer.Append("null\n");
					}
					else
					{
						buffer.Append(comp.getComponentName() + "\n");
						buffer.Append("        Type:       " + comp.getComponentType() + "\n");
						buffer.Append("        Is group:   " + comp.isGroup() + "\n");
						buffer.Append("        Is dirty:   " + comp.isDirty() + "\n");
						buffer.Append("        Is visible: " + comp.isVisible() + "\n");
						buffer.Append("        Data File:  " + comp.getDataFileName() + "\n");
					}
				}
			}
		}
		return buffer.ToString();
	}

	/*
	Write the data set to an XML file.  The filename is adjusted to the
	working directory if necessary using IOUtil.getPathUsingWorkingDir().
	@param filename_prev The name of the previous version of the file (for
	processing headers).  Specify as null if no previous file is available.
	@param filename The name of the file to write.
	@param data_Vector A Vector of StateCU_Location to write.
	@param new_comments Comments to add to the top of the file.  Specify as null if no
	comments are available.
	@exception IOException if there is an error writing the file.
	*/
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeXMLFile(String filename_prev, String filename, DataSet dataset, String [] new_comments) throws java.io.IOException
	public static void writeXMLFile(string filename_prev, string filename, DataSet dataset, string[] new_comments)
	{
		string[] comment_str = new string[] {"#"};
		string[] ignore_comment_str = new string[] {"#>"};
		PrintWriter @out = null;
		try
		{
			string full_filename_prev = IOUtil.getPathUsingWorkingDir(filename_prev);
			if (!StringUtil.endsWithIgnoreCase(filename,".xml"))
			{
				filename = filename + ".xml";
			}
			string full_filename = IOUtil.getPathUsingWorkingDir(filename);
			@out = IOUtil.processFileHeaders(full_filename_prev, full_filename, new_comments, comment_str, ignore_comment_str, 0);
			if (@out == null)
			{
				throw new IOException("Error writing to \"" + full_filename + "\"");
			}
			writeDataSetToXMLFile(dataset, @out);
		}
		finally
		{
			if (@out != null)
			{
				@out.flush();
				@out.close();
			}
		}
	}

	/// <summary>
	/// Write a data set to an opened XML file. </summary>
	/// <param name="data"> A DataSet to write. </param>
	/// <param name="out"> output PrintWriter. </param>
	/// <exception cref="IOException"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void writeDataSetToXMLFile(DataSet dataset, java.io.PrintWriter out) throws java.io.IOException
	private static void writeDataSetToXMLFile(DataSet dataset, PrintWriter @out)
	{ //String cmnt = "#>";
		//DataSetComponent comp = null;

	/* TODO - need to evaluate how best to implement
		// Start XML tag...
		out.println ("<!--" );
		out.println ( cmnt );
		out.println ( cmnt + "  StateCU Data Set (XML) File" );
		out.println ( cmnt );
		out.println ( cmnt + "EndHeader" );
		out.println ("-->" );
	
		out.println ("<StateCU_DataSet " +
			"Type=\"" + lookupTypeName(dataset.getType()) + "\"" +
			"BaseName=\"" + dataset.getBaseName() + "\"" +
			">" );
	
		int num = 0;
		Vector data_Vector = dataset.getComponents();
		if ( data_Vector != null ) {
			num = data_Vector.size();
		}
		String indent1 = "  ";
		String indent2 = indent1 + indent1;
		for ( int i = 0; i < num; i++ ) {
			comp = (StateCU_DataSetComponent)data_Vector.elementAt(i);
			if ( comp == null ) {
				continue;
			}
			out.println ( indent1 + "<StateCU_DataSetComponent" );
	
			out.println ( indent2 + "Type=\"" +
				StateCU_DataSetComponent.lookupComponentName(
				comp.getType()) + "\"" );
			out.println ( indent2 + "DataFile=\"" +
				comp.getDataFileName() + "\"" );
			out.println ( indent2 + "ListFile=\"" +
				comp.getListFileName() + "\"" );
			out.println ( indent2 + "CommandsFile=\"" +
				comp.getCommandsFileName() + "\"" );
			out.println ( indent2 + ">" );
	
			out.println ( indent1 + "</StateCU_DataSetComponent>");
		}
		out.println ("</StateCU_DataSet>" );
	*/
	}

	}

}
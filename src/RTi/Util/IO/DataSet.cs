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
    //using Message = RTi.Util.Message.Message;

    public class DataSet
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

        /**
        Add a component to the data set.
        @param comp Component to add.
        */
        public void addComponent(DataSetComponent comp)
        {
            __components.Add(comp);
        }

        /**
        Return the component for the requested data component name.
        @return the component for the requested data component name or null if the
        component is not in the data set.
        @param name Component name.
        */
        //public DataSetComponent GetComponentForComponentName(String name)
        //{
        //    int size = __components.Count;
        //    DataSetComponent comp = null;
        //    List<DataSetComponent> v;
        //    int size2;
        //    for (int i = 0; i < size; i++)
        //    {
        //        comp = __components[i];
        //        if (comp.GetComponentName().equalsIgnoreCase(name))
        //        {
        //            return comp;
        //        }
        //        // If the component is a group and did not match the type, check
        //        // the sub-types in the component...
        //        if (comp.isGroup())
        //        {
        //            v = comp.GetData();
        //            size2 = 0;
        //            if (v != null)
        //            {
        //                size2 = v.Count;
        //            }
        //            for (int j = 0; j < size2; j++)
        //            {
        //                comp = v.[j];
        //                if (comp.GetComponentName().equalsIgnoreCase(name))
        //                {
        //                    return comp;
        //                }
        //            }
        //        }
        //    }
        //    return null;
        //}

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
        /// Return the list of data components. </summary>
        /// <returns> the list of data components. </returns>
        public virtual IList<DataSetComponent> getComponents()
        {
            return __components;
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
                    v = (IList<DataSetComponent>)comp.getData();
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
        /// Return the directory for the data set. </summary>
        /// <returns> the directory for the data set. </returns>
        public virtual string getDataSetDirectory()
        {
            return __dataset_dir;
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
        /// Set the directory for the data set. </summary>
        /// <param name="dir"> Directory for the data set. </param>
        public virtual void setDataSetDirectory(string dir)
        {
            __dataset_dir = dir;
        }

        /**
        Set the data set type.
        @param type Data set type.
        @param initialize_components If true, the components are cleared and the
        component groups for the type are initialized by calling the
        initializeDataSet() method, which should be defined in the extended class.
        @exception Exception if there is an error setting the data type or initializing the component groups.
        */
        //public static void SetDataSetType(int type, bool initialize_components)
        ////throws Exception
        //{
        //    __type = type;
        // if (initialize_components ) {
        //  __components.Clear();
        // }
        //    //initializeDataSet ();
        //}

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
        /// Set the file name (no directory) for the data set (XML file). </summary>
        /// <param name="filename"> File for the data set. </param>
        public virtual void setDataSetFileName(string filename)
        {
            __dataset_filename = filename;
        }

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
                    v = (IList<DataSetComponent>)comp.getData();
                    size2 = 0;
                    if (v != null)
                    {
                        size2 = v.Count;
                    }
                    for (int j = 0; j < size2; j++)
                    {
                        buffer.Append("    SubComponent:  ");
                        if (v[j] == null)
                        {
                            buffer.Append("null\n");
                        }
                        else
                        {
                            buffer.Append(v[j].getComponentName() + "\n");
                            buffer.Append("        Type:       " + v[j].getComponentType() + "\n");
                            buffer.Append("        Is group:   " + v[j].isGroup() + "\n");
                            buffer.Append("        Is dirty:   " + v[j].isDirty() + "\n");
                            buffer.Append("        Is visible: " + v[j].isVisible() + "\n");
                            buffer.Append("        Data File:  " + v[j].getDataFileName() + "\n");
                        }
                    }
                }
            }
            return buffer.ToString();
        }

    }
}

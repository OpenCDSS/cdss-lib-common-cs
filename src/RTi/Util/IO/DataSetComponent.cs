using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

// DataSetComponent - class to maintain information about a single component from a data set

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
// DataSetComponent - an object to maintain information about a single
//			component from a data set
//-----------------------------------------------------------------------------
// History:
//
// 2003-07-12	Steven A. Malers, RTi	Created class. Copy
//					StateCU_DataSetComponent and make more
//					general.
// 2003-07-15	J. Thomas Sapienza, RTi	Added hasData()
// 2003-10-13	SAM, RTi		* Initialize __is_dirty and __is_group
//					  to false.
//					* Add a copy constructor.
// 2005-04-26	J. Thomas Sapienza, RTi	Added all data members to finalize().
// 2006-04-10	SAM, RTi		* Added isOutput() to indicate whether
//					  the component is being created as
//					  output.  This is used to evaluate
//					  whether data checks need to be done.
//					* Add getDataCheckResults() and
//					  setDataCheckResults() to handle
//					  verbose output for data checks.
// 2007-05-08	SAM, RTi		Cleanup code based on Eclipse feedback.
//-----------------------------------------------------------------------------

namespace RTi.Util.IO
{

    using Message = Message.Message;

    public class DataSetComponent
    {

        /// <summary>
        /// Indicate how the list for a component group is created.
        /// </summary>
        // TODO - need to figure out generic this can be, also need enumeration and "NA"
        public const string LIST_SOURCE_PRIMARY_COMPONENT = "PrimaryComponent";
        public const string LIST_SOURCE_NETWORK = "Network";
        public const string LIST_SOURCE_LISTFILE = "ListFile";

        /// <summary>
        /// Type of component - integer used to increase performance so string lookups don't need to be done.
        /// </summary>
        private int __type = -1;

        /// <summary>
        /// Name of the component.
        /// </summary>
        private string __name = "";

        /// <summary>
        /// Name of file that will hold the data when saved to disk.
        /// </summary>
        private string __data_file_name = "";

        /// <summary>
        /// Name of the command file used to create the component, if _created_from is DATA_FROM_COMMANDS.
        /// </summary>
        private string __commandFileName = "";

        /// <summary>
        /// Name of the list file corresponding to the data component (e.g., used by StateDMI).
        /// </summary>
        private string __list_file_name = "";

        /// <summary>
        /// Indicates how the list for a component group is created (see LIST_SOURCE_*).
        /// TODO - need to handle
        /// </summary>
        private string __list_source = "";

        /// <summary>
        /// Data for component type (often a list of objects).  If the component is a group, the
        /// data will be a list of the components in the group.
        /// </summary>
        private object __data = null;

        /// <summary>
        /// Indicates whether the component is dirty.
        /// </summary>
        private bool __is_dirty = false;

        /// <summary>
        /// Indicates whether there was an error when reading the data from an input file.
        /// If true, care should be taken with further processing because code may further corrupt the data and
        /// file if re-written
        /// </summary>
        private bool __errorReadingInputFile = false;

        /// <summary>
        /// Is the data component actually a group of components.  In this case the _data is a
        /// list of StateCU_DataSetComponent.  This is determined from the group type, not
        /// whether the component actually has subcomponents
        /// </summary>
        private bool __is_group = false;

        /// <summary>
        /// Is the data component being saved as output?  This is used, for example, to help know when
        /// to perform data checks.
        /// </summary>
        private bool __is_output = false;

        /// <summary>
        /// Indicates if the component should be visible because of the data set type (control settings).
        /// Extra components may be included in a data set to ease transition from one data set type to another.
        /// </summary>
        private bool __is_visible = true;

        /// <summary>
        /// If the component belongs to a group, this reference points to the group component.
        /// </summary>
        private DataSetComponent __parent = null;

        /// <summary>
        /// The DataSet that this DataSetComponent belongs to.  It is assumed that a
        /// DataSetComponent always belongs to a DataSet, even if only a partial data set (e.g., one group).
        /// </summary>
        private DataSet __dataset = null;

        /// <summary>
        /// A list of String used to store data check results, suitable for printing to an output
        /// file header, etc.  See the __is_output flag to help indicate when check results should be created.
        /// </summary>
        private IList<string> __data_check_results = null;

        /// <summary>
        /// Construct the data set component and set values to empty strings and null. </summary>
        /// <param name="dataset"> the DataSet instance that this component belongs to (note that
        /// the DataSet.addComponent() method must still be called to add the component). </param>
        /// <param name="type"> Component type. </param>
        /// <exception cref="Exception"> if there is an error creating the object. </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public DataSetComponent(DataSet dataset, int type) throws Exception
        public DataSetComponent(DataSet dataset, int type)
        {
            __dataset = dataset;
            if ((type < 0) || (type >= __dataset._component_types.Length))
            {
                throw new Exception("Unrecognized type " + type);
            }
            __type = type;
            int size = 0;
            if (__dataset._component_groups != null)
            {
                size = __dataset._component_groups.Length;
            }
            // Set whether the component is a group, based on the data set information.
            __is_group = false;
            for (int i = 0; i < size; i++)
            {
                if (__dataset._component_groups[i] == __type)
                {
                    __is_group = true;
                    break;
                }
            }
            // Set the component name, based on the data set information.
            __name = __dataset.lookupComponentName(__type);
        }

        /// <summary>
        /// Copy constructor. </summary>
        /// <param name="comp"> Original data component to copy. </param>
        /// <param name="dataset"> The dataset for the component.  This is normally a copy, not the
        /// original (e.g., from the DataSet copy constructor). </param>
        /// <param name="deep_copy"> If true, all data are copied (currently not recognized).
        /// If false, the component is copied but not the data itself.  This is
        /// typically used to save the names of components before editing in the response file editor. </param>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public DataSetComponent(DataSetComponent comp, DataSet dataset, boolean deep_copy) throws Exception
        public DataSetComponent(DataSetComponent comp, DataSet dataset, bool deep_copy) : this(dataset, comp.getComponentType())
        {
            __type = comp.__type;
            __name = comp.__name;
            __data_file_name = comp.__data_file_name;
            __commandFileName = comp.__commandFileName;
            __list_file_name = comp.__list_file_name;
            __list_source = comp.__list_source;
            __data = null; // TODO - support deep copy later
            __is_dirty = comp.__is_dirty;
            __is_group = comp.__is_group;
            __is_visible = comp.__is_visible;
            __parent = comp.__parent;
            __dataset = comp.__dataset;
        }

        /// <summary>
        /// Add a component to this component.  This method should only be called to add
        /// sub-components to a group component. </summary>
        /// <param name="component"> Sub-component to add to the component. </param>
        /// <exception cref="Exception"> if trying to add a component to a non-group component. </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void addComponent(DataSetComponent component) throws Exception
        public virtual void addComponent(DataSetComponent component)
        {
            string routine = "DataSetComponent.addComponent";
            if (!isGroup())
            {
                Message.printWarning(2, routine, "Trying to add component to non-group component.");
                return;
            }
            if (__data == null)
            {
                // Allocate memory for the components.
                __data = new List<DataSetComponent>();
            }
            ((System.Collections.IList)__data).Add(component);
            // Set so the component knows who its parent is...
            component.__parent = this;
            if (Message.isDebugOn)
            {
                Message.printDebug(1, routine, "Added " + component.getComponentName() + " to " + getComponentName());
            }
        }

        /// <summary>
        /// Return the data component type. </summary>
        /// <returns> the data component type. </returns>
        public int getComponentType()
        {
            return __type;
        }

        /// <summary>
        /// Return the name of the component. </summary>
        /// <returns> the name of the component. </returns>
        public virtual string getComponentName()
        {
            return __name;
        }

        /// <summary>
        /// Return the file name where data are written. </summary>
        /// <returns> the file name where data are written. </returns>
        public virtual string getDataFileName()
        {
            return __data_file_name;
        }

        /// <summary>
        /// Return the data for the component.  For example, this may be a list of stations or time series. </summary>
        /// <returns> the data for the component. </returns>
        public object getData()
        {
            return __data;
        }

        /// <summary>
        /// Indicates whether there is data contained in the object.  If the data is null,
        /// false is returned.  If the data is a list and is has 0 elements, false is returned.
        /// Otherwise, true is returned. </summary>
        /// <returns> whether there is data. </returns>
        public virtual bool hasData()
        {
            if (__data == null)
            {
                return false;
            }
            if (__data is System.Collections.IList)
            {
                //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
                //ORIGINAL LINE: @SuppressWarnings("rawtypes") java.util.List list = (java.util.List)__data;
                System.Collections.IList list = (System.Collections.IList)__data;
                if (list.Count == 0)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Indicate whether the component is dirty (has been modified). </summary>
        /// <returns> true if the component is a dirty (has been modified). </returns>
        public virtual bool isDirty()
        {
            return __is_dirty;
        }

        /// <summary>
        /// Indicate whether the component is a group. </summary>
        /// <returns> true if the component is a group to organize other components. </returns>
        public bool isGroup()
        {
            return __is_group;
        }

        /// <summary>
        /// Indicate whether the component is visible (if not it is because the component
        /// has been included but is not needed for the current data set). </summary>
        /// <returns> true if the component is visible in displays. </returns>
        public virtual bool isVisible()
        {
            return __is_visible;
        }

        /// <summary>
        /// Set the data object containing the component's data.  Often this is a Vector of objects. </summary>
        /// <param name="data"> Data object containing the component's data. </param>
        public void setData(Object data)
        {
            __data = data;
        }

        /// <summary>
        /// Set the file name for the component's data. </summary>
        /// <param name="data_file_name"> File name for the component's data. </param>
        public void setDataFileName(string data_file_name)
        {
            __data_file_name = data_file_name;
        }

        /// <summary>
        /// Set whether the component is dirty (has been edited). </summary>
        /// <param name="is_dirty"> true if the component is dirty (has been edited). </param>
        public void setDirty(bool is_dirty)
        {
            __is_dirty = is_dirty;
        }

        /// <summary>
        /// Set the source of the list for the component (see LIST_SOURCE_*). </summary>
        /// <param name="list_source"> The source of the list for the component. </param>
        public virtual void setListSource(string list_source)
        {
            __list_source = list_source;
        }

        /// <summary>
        /// Set whether there was an error reading an input file.  This is useful in cases where the file
        /// may be hand-edited or created outside of software, or perhaps the specification has changed and the
        /// Java code has not caught up.  If the error flag is set to true, then software like a UI has a clue to
        /// NOT try to edit or save because data corruption might occur. </summary>
        /// <param name="errorReadingInputFile"> if true, then an error occurred reading the component data from the
        /// input file. </param>
        public void setErrorReadingInputFile(bool errorReadingInputFile)
        {
            __errorReadingInputFile = errorReadingInputFile;
        }

        /// <summary>
        /// Set whether the component is visible (if not it is because the component
        /// has been included but is not needed for the current data set). </summary>
        /// <param name="is_visible"> true if the component should be visible in displays. </param>
        public virtual void setVisible(bool is_visible)
        {
            __is_visible = is_visible;
        }

    }
}

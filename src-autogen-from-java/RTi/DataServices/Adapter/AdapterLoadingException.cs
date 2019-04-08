// AdapterLoadingException - exception that gets thrown when an adapter cannot load

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

//------------------------------------------------------------------------------------
// AdapterLoadingException - class to build an exception handler that gets thrown when 
// an adapter can not get loaded.
//------------------------------------------------------------------------------------
// History:
//
//      2006-06-16      Scott Townsend, RTi     Create initial version of this
//                                              Adapter exception class. This
//						class is necessary since it does
//						handle specific error messages
//						and specific Adapter clean-up.
//						This clean-up includes closing any
//						open connections to the adapter's
//						Server.
//------------------------------------------------------------------------------------
// Endheader

namespace RTi.DataServices.Adapter
{

	using Adapter = RTi.DataServices.Adapter.Adapter;

	/// <summary>
	/// An exception which occurs when an adapter fails to load for any reason.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class AdapterLoadingException extends java.io.IOException
	public class AdapterLoadingException : IOException
	{

	/// <summary>
	/// private Adapter instance
	/// </summary>
	private Adapter __IOAdapter = null;

	/// <summary>
	/// <para>Default constructor to build this exception.</para>
	/// </summary>
	public AdapterLoadingException() : base("An Error occured: Data Services Adapter could not load")
	{

		if (__IOAdapter != null)
		{
			//__IOAdapter.finalize();
		}
	}

	/// <summary>
	/// <para>Constructor to build this exception with a developer defined error message.</para> </summary>
	/// <param name="exceptionMessage"> a String holding the developer defined error message. </param>
	public AdapterLoadingException(string exceptionMessage) : base(exceptionMessage)
	{
		if (__IOAdapter != null)
		{
			//__IOAdapter.finalize();
		}
	}

	/// <summary>
	/// <para>Constructor to build this exception with the Adapter specified.</para> </summary>
	/// <param name="DSAdapter"> a Adapter object that will be finalized when
	/// this constructor is called. </param>
	public AdapterLoadingException(Adapter DSAdapter) : base("An Error occured: Data Services Adapter could not load")
	{

		// Set the adapter
		__IOAdapter = DSAdapter;

		if (__IOAdapter != null)
		{
			//__IOAdapter.finalize();
		}
	}

	/// <summary>
	/// <para>Constructor to build this exception with a developer defined error message
	/// and an Adapter.</para> </summary>
	/// <param name="exceptionMessage"> a String holding the developer defined error message. </param>
	/// <param name="DSAdapter"> a Adapter object that will be finalized when
	/// this constructor is called. </param>
	public AdapterLoadingException(Adapter DSAdapter, string exceptionMessage) : base(exceptionMessage)
	{

		// Set the adapter
		__IOAdapter = DSAdapter;

		if (__IOAdapter != null)
		{
			//__IOAdapter.finalize();
		}
	}

	/// <summary>
	/// A get accessor method to get the Adapter specified by the Exception. </summary>
	/// <returns> The Adapter utilized in theis Exception. </returns>
	public virtual Adapter getAdapter()
	{
		return __IOAdapter;
	}
	} // End class AdapterLoadingException

}
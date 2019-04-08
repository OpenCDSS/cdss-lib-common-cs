using System.Collections.Generic;

// TSProductAnnotationProvider - class that provides annotations of a certain type to a TSProduct

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
// TSProductAnnotationProvider - class that provides annotations of a certain
//	type to a TSProduct.
// ----------------------------------------------------------------------------
// History:
//
// 2005-10-18	J. Thomas Sapienza, RTi	Initial version.
// ----------------------------------------------------------------------------

namespace RTi.GRTS
{

	using PropList = RTi.Util.IO.PropList;

	/// <summary>
	/// Interface for a class that provides annotations to a TSProduct graph.  
	/// </summary>
	public interface TSProductAnnotationProvider
	{

	/// <summary>
	/// Adds a type of annotation provided by this class to the internal Vector of 
	/// annotation types provided.  This Vector is returned via 
	/// getAnnotationProviderChoices(). </summary>
	/// <param name="name"> the name of a type of annotation provided by this class. </param>
	void addAnnotationProvider(string name);

	/// <summary>
	/// The method called when annotations are added to a product.  Classes must 
	/// add the annotations when this method is called -- it is the only notification
	/// they will receive that annotations are to be added to a TSProduct.<para>
	/// <b>Note:</b>  Annotations are added after references to time series are 
	/// determined because time series information may be needed by the 
	/// annotation provider.
	/// </para>
	/// </summary>
	/// <param name="product"> the product to which to add annotations. </param>
	/// <param name="controlProps"> further properties that can be used to specify additional
	/// data to the annotation provider about the annotations it will provide. </param>
	/// <exception cref="Exception"> if there is an error adding annotations to the product. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addAnnotations(TSProduct product, RTi.Util.IO.PropList controlProps) throws Exception;
	void addAnnotations(TSProduct product, PropList controlProps);

	/// <summary>
	/// Returns a list of Strings, each of which is one of the types of annotation
	/// that is provided by this class. </summary>
	/// <returns> a list of the annotations that this class provides. </returns>
	IList<string> getAnnotationProviderChoices();

	/// <summary>
	/// Returns true if this class provides the given annotation type, false if not. </summary>
	/// <param name="name"> the name of an annotation type. </param>
	/// <returns> true if this class provides the given type, false if not. </returns>
	bool provides(string name);

	}

}
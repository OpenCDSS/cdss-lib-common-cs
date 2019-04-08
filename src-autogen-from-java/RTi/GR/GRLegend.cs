using System;
using System.Text;

// GRLegend - class to store information for a legend for a single data layer

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

namespace RTi.GR
{

	using IOUtil = RTi.Util.IO.IOUtil;

	/// <summary>
	/// The GRLegend class stores information for a legend for a single data layer.
	/// Examples of data layers are spatial data and time series.  Each layer can have
	/// one or more symbol.  For earlier versions of this class only a single GRSymbol
	/// and text label was stored and this behavior is still transparently supported.
	/// However, the class also can save multiple symbols.  Only one text label is
	/// currently used (not one label per symbol).
	/// </summary>
	public class GRLegend : ICloneable
	{

	/// <summary>
	/// Array of symbols used in the legend.
	/// </summary>
	private GRSymbol[] __symbol = null;

	/// <summary>
	/// The text of the legend.
	/// </summary>
	private string __text = "";

	/// <summary>
	/// Construct by indicating the number of symbols. </summary>
	/// <param name="nsymbols"> The number of symbols that will be used.  After construction,
	/// use setText() and setSymbol() to set the symbols. </param>
	public GRLegend(int nsymbols)
	{
		__text = "";
		__symbol = new GRSymbol[nsymbols];
	}

	/// <summary>
	/// Construct using the single symbol. </summary>
	/// <param name="symbol"> the symbol to use in the legend. </param>
	public GRLegend(GRSymbol symbol)
	{
		__text = "";
		__symbol = new GRSymbol[1];
		__symbol[0] = symbol;
	}

	/// <summary>
	/// Construct using the single symbol and the text. </summary>
	/// <param name="symbol"> the symbol to use in the legend. </param>
	/// <param name="text"> the text to put in the legend. </param>
	public GRLegend(GRSymbol symbol, string text)
	{
		__text = "";
		__symbol = new GRSymbol[1];
		__symbol[0] = symbol;
		setText(text);
	}

	/// <summary>
	/// Finalize before garbage collection. </summary>
	/// <exception cref="Throwable"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void finalize() throws Throwable
	~GRLegend()
	{
		IOUtil.nullArray(__symbol);
		__text = null;
//JAVA TO C# CONVERTER NOTE: The base class finalizer method is automatically called in C#:
//		base.finalize();
	}

	/// <summary>
	/// Clones the object. </summary>
	/// <returns> a clone of the Object. </returns>
	public virtual object clone()
	{
		GRLegend l = null;
		try
		{
			l = (GRLegend)base.clone();
		}
		catch (Exception)
		{
			return null;
		}

		if (__symbol != null)
		{
			l.__symbol = new GRSymbol[__symbol.Length];
			for (int i = 0; i < __symbol.Length; i++)
			{
				l.__symbol[i] = (GRSymbol)__symbol[i].clone();
			}
		}

		return l;
	}

	/// <summary>
	/// Return the symbol used for the legend.  It is assumed that only one symbol is
	/// used and therefore the first symbol is returned. </summary>
	/// <returns> the symbol used for the legend. </returns>
	public virtual GRSymbol getSymbol()
	{
		if (__symbol == null)
		{
			return null;
		}
		else
		{
			return __symbol[0];
		}
	}

	/// <summary>
	/// Return the symbol used for the legend, for a specific position. </summary>
	/// <param name="pos"> Position in the symbol array (zero index). </param>
	/// <returns> the symbol used for the legend. </returns>
	public virtual GRSymbol getSymbol(int pos)
	{
		if (__symbol == null)
		{
			return null;
		}
		else
		{
			return __symbol[pos];
		}
	}

	/// <summary>
	/// Return legend text. </summary>
	/// <returns> the text used for the legend. </returns>
	public virtual string getText()
	{
		return __text;
	}

	/// <summary>
	/// Set the number of symbols.  This reallocates the symbols array. </summary>
	/// <param name="num_symbols"> The number of symbols to use for the legend. </param>
	public virtual void setNumberOfSymbols(int num_symbols)
	{
		__symbol = new GRSymbol[num_symbols];
		for (int i = 0; i < num_symbols; i++)
		{
			__symbol[i] = null;
		}
	}

	/// <summary>
	/// Set the symbol to use.  It is assumed that only one symbol is being used and
	/// therefore the symbol at position zero is set. </summary>
	/// <param name="symbol"> Symbol to use for legend. </param>
	public virtual void setSymbol(GRSymbol symbol)
	{
		if ((__symbol == null) || (__symbol.Length == 0))
		{
			__symbol = new GRSymbol[1];
		}
		__symbol[0] = symbol;
	}

	/// <summary>
	/// Set the symbol to use.  The number of symbols must have been set in the constructor. </summary>
	/// <param name="pos"> Position for the symbol (zero index). </param>
	/// <param name="symbol"> GRSymbol to set at the position. </param>
	public virtual void setSymbol(int pos, GRSymbol symbol)
	{ // Later need to make sure copy constructor will work for symbol and can redefine symbol array.
		__symbol[pos] = symbol;
	}

	/// <summary>
	/// Set the legend text. </summary>
	/// <param name="text"> Text to use for legend. </param>
	public virtual void setText(string text)
	{
		if (!string.ReferenceEquals(text, null))
		{
			__text = text;
		}
	}

	/// <summary>
	/// Return the size of the legend (the number of symbols). </summary>
	/// <returns> the size of the legend (the number of symbols). </returns>
	public virtual int size()
	{
		if (__symbol == null)
		{
			return 0;
		}
		else
		{
			return __symbol.Length;
		}
	}

	/// <summary>
	/// Return a string representation of the legend in the form "text,symbol". </summary>
	/// <returns> a string representation of the legend. </returns>
	public override string ToString()
	{
		StringBuilder b = new StringBuilder("\"" + __text + "\"");
		if (__symbol != null)
		{
			for (int i = 0; i < __symbol.Length; i++)
			{
				b.Append("," + __symbol[i]);
			}
		}
		return b.ToString();
	}

	}

}
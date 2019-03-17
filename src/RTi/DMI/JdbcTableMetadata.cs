// JdbcTableMetadata - class for JDBC table metadata

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

namespace RTi.DMI
{
	/// <summary>
	/// This class holds data from the DatabaseMetaData.getTables() method.
	/// </summary>
	public class JdbcTableMetadata
	{

	private string catalog = "";
	private string schema = "";
	private string name = "";
	private string type = "";
	private string remarks = "";
	//private String typesCatalog;
	//private String typesSchema;
	//private String typeName;
	private string selfRefCol = "";
	private string selfRefColHowCreated = "";

	/// <summary>
	/// Construct object.
	/// </summary>
	public JdbcTableMetadata()
	{

	}

	/// <summary>
	/// Get the catalog (may be null)
	/// </summary>
	public virtual string getCatalog()
	{
		return this.catalog;
	}

	/// <summary>
	/// Get the name
	/// </summary>
	public virtual string getName()
	{
		return this.name;
	}

	/// <summary>
	/// Get the remarks
	/// </summary>
	public virtual string getRemarks()
	{
		return this.remarks;
	}

	/// <summary>
	/// Get the schema (may be null)
	/// </summary>
	public virtual string getSchema()
	{
		return this.schema;
	}

	/// <summary>
	/// Get the self referencing column (may be null)
	/// </summary>
	public virtual string getSelfRefColumn()
	{
		return this.selfRefCol;
	}

	/// <summary>
	/// Get the self referencing column, how created (may be null)
	/// </summary>
	public virtual string getSelfRefColumnHowCreated()
	{
		return this.selfRefColHowCreated;
	}

	/// <summary>
	/// Get the type (may be null)
	/// </summary>
	public virtual string getType()
	{
		return this.type;
	}

	/// <summary>
	/// Set the catalog.
	/// </summary>
	public virtual void setCatalog(string catalog)
	{
		this.catalog = catalog;
	}

	/// <summary>
	/// Set the name.
	/// </summary>
	public virtual void setName(string name)
	{
		this.name = name;
	}

	/// <summary>
	/// Set the remarks.
	/// </summary>
	public virtual void setRemarks(string remarks)
	{
		this.remarks = remarks;
	}

	/// <summary>
	/// Set the schema.
	/// </summary>
	public virtual void setSchema(string schema)
	{
		this.schema = schema;
	}

	/// <summary>
	/// Set the self ref column.
	/// </summary>
	public virtual void setSelfRefColumn(string selfRefCol)
	{
		this.selfRefCol = selfRefCol;
	}

	/// <summary>
	/// Set the self ref column, how created.
	/// </summary>
	public virtual void setSelfRefColumnHowCreated(string selfRefColHowCreated)
	{
		this.selfRefColHowCreated = selfRefColHowCreated;
	}

	/// <summary>
	/// Set the type.
	/// </summary>
	public virtual void setType(string type)
	{
		this.type = type;
	}

	}

}
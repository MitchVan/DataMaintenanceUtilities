using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace PDX.BTS.DataMaintenance.MaintTools
{

  //  Set the GUID that will be used when this Library is referenced by other applications.  Specifying a 
  //  GUID insures that the library will maintain binary compatibility through all build operations.
  [System.Runtime.InteropServices.Guid("93EA2CA6-C6EA-4d14-9749-B6BBB5007BA5")]
  
  
  public class ParameterManager
  {

    /// <summary>
    /// Initializes the Parameter Environment.  Attaches to the Parameter Table and sets a valid connection
    /// object that can be used by the other implementations.
    /// </summary>
    /// <param name="tableName">
    ///	The name of the table that houses the parameter keys and values.
    ///	</param>
    /// <param name="connectionString">
    /// The SQL Connection String that will be used to open a SQL Server Connection to the Parameter Database.
    /// </param>
    /// <returns>
    /// TRUE - if the connection was established successfully.
    /// FALSE - if the connection was NOT established successfully.
    /// </returns>
    public bool Initialize(string tableName, string connectionString)
    {
      try
      {
        //  Set the Field Variable that will house the Parameter Table Name with the Table Name that was
        //  passed to this method.
        _parameterTableName = tableName;

        //  If no Table Name was passed to this method, fail the connection.
        if (_parameterTableName.Length == 0)
        {
          //  Return a False to the calling routine.
          return false;
        }


        //  Check for a valid connection to the Parameter Database.
        if (_parameterDatabaseConnection != null)
        {
          if (_parameterDatabaseConnection.State != System.Data.ConnectionState.Closed)
          {
            //  Make sure the Connection is closed.
            _parameterDatabaseConnection.Close();
          }
          //  Close the Object Reference to the SQL Database Connection.
          //  Establish a connection to the database.
          _parameterDatabaseConnection = null;
        }


        //  Establish a connection to the database.
        _parameterDatabaseConnection = new SqlConnection();
        _parameterDatabaseConnection.ConnectionString = connectionString;
        _parameterDatabaseConnection.Open();


        //  If the connection is open return a "TRUE" to the calling routine.  Otherwise, return a "FALSE".
        if (_parameterDatabaseConnection.State == System.Data.ConnectionState.Open)
        {
          return true;
        }
        else
        {
          return false;
        }

      }
      catch
      {
        //  Return FALSE to the calling routine to indicate that this process failed.
        return false;

      }

    }   //  Initialize


    public bool IsInitialized()
    {
      try
      {
        //  Determine if the Connectioin is Open (i.e., the Parameter Manager is initialized).
        if (_parameterDatabaseConnection != null)
        {
          if (_parameterDatabaseConnection.State == System.Data.ConnectionState.Open)
          {
            //  The Parameter Manager is IS Initialized so return TRUE to the calling routine.
            return true;
          }
          else
          {
            //  The Parameter Manager is IS NOT Initialized so return TRUE to the calling routine.
            return false;
          }

        }
        else
        {
          //  The Parameter Manager is IS NOT Initialized so return TRUE to the calling routine.
          return false;

        }

      }
      catch
      {
        //  Return FALSE to the calling routine to indicate that this process failed.
        return false;

      }

    }


    /// <summary>
    /// Determines whether or not the Parameter Key exists in the Parameter Table.
    /// </summary>
    /// <param name="parameterName">
    /// The Parameter Key to be searched for in the table.
    /// </param>
    /// <returns>
    /// TRUE - if the key exists in the table.
    /// FALSE - if the key does NOT exist in the table.
    /// </returns>
    public bool ParameterExists(string parameterName)
    {

      string        sqlStatement           = null;
      SqlCommand    parameterExistsCommand = null;
      SqlDataReader parameterExistsReader  = null;

      try
      {
        //  Make sure there is a valid connection to the database before moving on.
        if (!ValidConnection())
        {
          //  Return a "FALSE" to the calling routine to indicate that the existence of the Parameter could not
          //  be determined.
          return false;
        }


        //  Build the SQLStatement that will be used to test whether or not the paremeter exists.
        sqlStatement = "SELECT * "
                     + "FROM " + _parameterTableName + " "
                     + "WHERE Parameter_Name = '" + parameterName + "'";


        //  Create the command object and that will be used to search the table to determine if the
        //  Parameter exists in the Table.
        parameterExistsCommand = new SqlCommand();
        parameterExistsCommand.Connection = _parameterDatabaseConnection;
        parameterExistsCommand.CommandText = sqlStatement;


        //  Use the command that was just created to populate a data reader.
        parameterExistsReader = parameterExistsCommand.ExecuteReader();


        //  If the Data Reader contains rows, the parameter exists so return a "TRUE" to the calling routine.
        //  Otherwise return a "FALSE".
        if (parameterExistsReader.HasRows)
        {
          //  Close the SQL Data Reader.
          parameterExistsReader.Close();
          //  Set the return value of the method to "TRUE".
          return true;
        }
        else
        {
          //  Close the SQL Data Reader.
          parameterExistsReader.Close();
          //  Set the return value of the method to "TRUE".
          return false;
        }

      }
      catch
      {
        //  Return FALSE to the calling routine to indicate that this process failed.
        return false;

      }

    }   //  ParameterExists


    /// <summary>
    /// Calls the Overload of the ReadParameter Method that requires a Decrypt Parameter Indicator defaulting that value
    /// to "FALSE".  If the value is encrypted in the Load Parameters Table the encrytped value will be returned to the
    /// calling method.
    /// </summary>
    /// <param name="parameterName">
    /// ParameterName - The name (key) of the parameter whose value is being sought.
    /// </param>
    /// <returns>
    /// The Parameter Value if found or an empty string to indicate that the value could not be obtained
    /// from the database.
    /// </returns>
    public string ReadParameter(string parameterName)
    {
      string parameterValue = null;

      try
      {
        //  Call the overload of the Read Parameter Method that requires a Decrypt Value defaulting that value to false.
        parameterValue = ReadParameter(parameterName, false);


        //  Return the Parameter Value that was retrieved from the overload method to calling method.
        return parameterValue;

      }
      catch
      {
        //  Return a NULL string to indicate that this process failed.
        return null;

      }

    }   //  ReadParameter


    /// <summary>
    /// Retrieves the Parameter Value associated with the passed Key from the Parameter Table and returns
    /// it as a string to the calling routine.
    /// </summary>
    /// <param name="parameterName">
    /// ParameterName - The name (key) of the parameter whose value is being sought.
    /// 
    /// DecryptValue - Indicates whether a value that is encrypted in the Parameter Table should be decrypted before
    ///                it is returned.
    /// </param>
    /// <returns>
    /// The Parameter Value if found or an empty string to indicate that the value could not be obtained
    /// from the database.
    /// </returns>
    public string ReadParameter(string parameterName, bool DecryptValue)
    {

      string                                               sqlStatement          = null;
      SqlCommand                                           parameterValueCommand = null;
      SqlDataReader                                        parameterValueReader  = null;
      string                                               parameterValue        = null;
      PDX.BTS.DataMaintenance.MaintTools.EncryptionManager encryptionManager     = null;

      try
      {
        //  Make sure there is a valid connection to the database before moving on.
        if (!ValidConnection())
        {
          //  Return an empty string "" to the calling routine to indicate that the Parameter Value could not
          //  be retrieved from the Parameter Table.
          return "";
        }


        //  Make sure the Parameter Exists before trying to pull it from the Database.
        if (!ParameterExists(parameterName))
        {
          //  Return an empty string "" to the calling routine to indicate that the Parameter Value could not
          //  be retrieved from the Parameter Table.
          return "";
        }


        //  Build the SQL Statement that will be used to retrieve the Parameter Value from the Parameter Table.
        sqlStatement = "SELECT Parameter_Value "
                     + "FROM " + _parameterTableName + " "
                     + "WHERE Parameter_Name = '" + parameterName + "'";

        //  Create the command object and that will be used to retrieve the Parameter Value from the Parameter
        //  Table.
        parameterValueCommand = new SqlCommand();
        parameterValueCommand.Connection = _parameterDatabaseConnection;
        parameterValueCommand.CommandText = sqlStatement;


        //  Use the command that was just created to populate a data reader.
        parameterValueReader = parameterValueCommand.ExecuteReader();


        //  If the Data Reader contains rows, the Parameter Value was retrieved so return the value that was
        //  obtained from the Parameter Table to the calling routine.  Otherwise, return an empty string to
        //  indicate that the value was not obtained.
        if (parameterValueReader.HasRows)
        {
          //  Retrieve the data from the Table.
          parameterValueReader.Read();

          //  Get the Parameter Value from the reader.
          parameterValue = (string)parameterValueReader["Parameter_Value"];
        }
        else
        {
          //  Set the return value to an empty string.
          parameterValue = "";
        }


        //  Close the Parameter Value SQL Reader.
        parameterValueReader.Close();


        //  If the User wanted the value decrypted and it is encrypted in the Parameters Table, decrypt the value.
        if ((DecryptValue) && (ParameterEncrypted(parameterName)))
        {
          //  Instantiate an Encryption Manager Object.
          encryptionManager = new PDX.BTS.DataMaintenance.MaintTools.EncryptionManager();

          //  Decrypt the Parameter Value.
          parameterValue = encryptionManager.DecryptString(parameterValue, "sombra1");

          //  Close the Parameter Manager Object since it is no longer needed.
          encryptionManager = null;

        }


        //  Return the Parameter Value or the empty string to the calling routine.
        return parameterValue;

      }
      catch
      {
        //  Return a NULL string to indicate that this process failed.
        return null;

      }

    }   //  ReadParameter


    /// <summary>
    /// Retrieves a list of Parameters based on the Parameter Prefix that is passed.  If no prefix is passed to the method,
    /// the names of all parameters in the Table are returned.
    /// </summary>
    /// <param name="ProcessParameterPrefix">
    /// The prefix of the Parameter Names that are to be retrieved.  If all Parameters for a process have a prefix on their names, this
    /// value can be used to return just the parameters for that process.
    /// </param>
    /// <returns>
    /// A String Collection of Parameter Names from the current parameter table.
    /// </returns>
    public System.Collections.Specialized.StringCollection RetrieveProcessParameterList(string ProcessParameterPrefix = "")
    {
      System.Data.SqlClient.SqlCommand                parameterNameListCommand = null;
      System.Data.SqlClient.SqlDataReader             parameterNameListReader  = null;
      System.Collections.Specialized.StringCollection parameterNameCollection  = null;

      try
      {
        //  Build the SQL Statement that will be used to retrieve the List of Parameters from the Parameter Table.
        string parameterNamesSQLStatement = "SELECT [Parameter_Name] "
                                          + "FROM " + _parameterTableName + " "
                                          + "WHERE [Parameter_Name] LIKE '" + ProcessParameterPrefix + "%' "
                                          + "ORDER BY [Parameter_Name]";


        //  Create the command object and that will be used to retrieve the Parameter Name List from the Parameter Table.
        parameterNameListCommand = new SqlCommand();
        parameterNameListCommand.Connection = _parameterDatabaseConnection;
        parameterNameListCommand.CommandType = System.Data.CommandType.Text;
        parameterNameListCommand.CommandText = parameterNamesSQLStatement;
        parameterNameListCommand.CommandTimeout = 20;


        //  Use the command that was just created to populate a SQL Data Reader Object with the Parameter Names.
        parameterNameListReader = parameterNameListCommand.ExecuteReader();


        //  Instantiate the String Collection of Parameter Names that will be populated and returned to the calling method.
        parameterNameCollection = new System.Collections.Specialized.StringCollection();


        //  If the Data Reader contains rows, the List of Parameter Names was retrieved so populate a String Collection with the names and
        //  return the List of Parameter Names retrieved from the Parameter Table to the calling routine.  Otherwise, return an empty String
        //  Collection to the calling method to indicate that the names were not obtained.
        if (parameterNameListReader.HasRows)
        {
          //  Populate the Collection of Parameter Names.
          while (parameterNameListReader.Read())
          {
            //  Retrieve the Current Parameter Name from the SQL Data Reader Object.
            string currentParameterName = (string)parameterNameListReader["Parameter_Name"];
            //  Add the Current Name to the Parameter Name Collection.
            parameterNameCollection.Add(currentParameterName);
          }

        }
        else
        {
          //  Return a NULL Pointer to indicate that this method failed.
          return null;

        }


        //  Return the List of Parameter Names to the calling method.
        return parameterNameCollection;

      }
      catch
      {
        //  Return NULL to the calling routine to indicate that the process failed and that the parameter list was not successfully retrieved.
        return null;

      }
      finally
      {
        //  If the Parameter Name String Collection Object has been instantiated, close it.
        if (parameterNameCollection != null)
        {
          parameterNameCollection = null;
        }
        //  If the Parameter Name List SQL Command Object has been isntantiated, close it.
        if (parameterNameListCommand != null)
        {
          parameterNameListCommand.Dispose();
          parameterNameListCommand = null;
        }
        //  If the Parameter Name List SQL Data Reader Object has been instantiated, close it.
        if (parameterNameListReader != null)
        {
          if (!parameterNameListReader.IsClosed)
          {
            parameterNameListReader.Close();
          }
          parameterNameListReader.Dispose();
          parameterNameListReader = null;
        }

      }

    }   //  RetrieveProcessParameterList


    /// <summary>
    /// Calls the other Overload of the WriteParameter Method with the Encrypt Indicator defaulted to "FALSE".
    /// If the user has not indicate whether or not the parameter should be encrypted, it is defaulted to be
    /// written into the Parameter Table in plain text.
    /// </summary>
    /// <param name="parameterName">
    /// The Name (Key) of the parameter.  Will be used to find the Parameter Value in the Parameter Table.
    /// </param>
    /// <param name="parameterValue">
    /// The Value that is to be stored for the passed Parameter Key.
    /// </param>
    /// <returns>
    /// TRUE - If the Parameter Key and Value pair was successfully added to the Parameter Table.
    /// FALSE - If the Parameter Key and Value pair was NOT successfully added to the Parameter Table. 
    /// </returns>
    public bool WriteParameter(string parameterName, string parameterValue)
    {

      bool parameterWritten = false;

      //  Default the Parameter Encrypted Value to "FALSE" and write the parameter key and value to
      //  the database.
      parameterWritten = WriteParameter(parameterName, parameterValue, false);

      return parameterWritten;

    }   //  WriteParameter


    /// <summary>
    /// Adds the passed Parameter Key (Name) and Value to the Parameter Table.
    /// </summary>
    /// <param name="parameterName">
    /// The Name (Key) of the parameter.  Will be used to find the Parameter Value in the Parameter Table.
    /// </param>
    /// <param name="parameterValue">
    /// The Value that is to be stored for the passed Parameter Key.
    /// </param>
    /// <param name="parameterEncrypted">
    /// Boolean indicator of the whether or not the Parameter Value should be encrypted in the Parameter Table.
    /// IF the value is set to "TRUE", the value will be encrypted.
    /// </param>
    /// <returns>
    /// TRUE - If the Parameter Key and Value pair was successfully added to the Parameter Table.
    /// FALSE - If the Parameter Key and Value pair was NOT successfully added to the Parameter Table. 
    /// </returns>
    public bool WriteParameter(string parameterName, string parameterValue, bool parameterEncrypted)
    {

      EncryptionManager encryptionManager     = null;
      string            insertValue           = null;
      string            sqlStatement          = null;
      SqlCommand        parameterWriteCommand = null;
      int               rowsAffected          = -9999;

      try
      {
        //  Make sure a Valid Parameter Name was passed to this routine.  If not, exit.
        if (parameterName.Length == 0)
        {
          //  Return a "FALSE" to the calling routine to indicate that this process failed.
          return false;
        }


        //  If the Parameter Value is to be encrypted, encrypt it.
        if (parameterEncrypted)
        {
          //  Instantiate an Encryption Manager Class.
          encryptionManager = new EncryptionManager();

          //  Encrypt the Parameter Value and set the encrypted string to be the value that will be inserted
          //  into the Parameter Table.
          insertValue = encryptionManager.EncryptString(parameterValue, "sombra1");

        }
        else
        {

          //  Set the clear text Parameter Value as the string that will be inserted into the Parameter Table.
          insertValue = parameterValue;

        }


        //  Make sure there is a valid connection to the Parameter Table before moving on.
        if (!ValidConnection())
        {
          //  Return a "FALSE" to the calling routine to indicate that this process failed.
          return false;
        }


        //  Check to make sure that the Parameter does not already exist in the Parameter Table.  If it does,
        //  fail this process.
        if (ParameterExists(parameterName))
        {
          //  Return a "FALSE" to the calling routine to indicate that this process failed.
          return false;
        }


        //  Build the SQLStatement that will be used to insert the Parameter into the Parameter Table.
        if (parameterEncrypted)
        {

          //  Determine if the "Parameter_Encrypted" Field exists in the Parameter Table. If so, populate
          //  it accordingly.
          if (EncryptionFieldExists())
          {
            //  Build the SQL Statement that will insert the Parameter into the Parameter Table and populate
            //  the Parameter Encrypted Table with a "1" to indicate that the parameter is encrypted.
            sqlStatement = "INSERT "
                         + "INTO " + _parameterTableName + " "
                         + "(Parameter_Name, Parameter_Value, Parameter_Encrypted) "
                         + "VALUES('" + parameterName + "', '" + insertValue + "', 1)";
          }
          else
          {
            //  Build the SQL Statement that will insert the Parameter into the Parameter Table.  The
            //  Parameter_Encrypted Field will not be populated since it does not exist in the Parameter Table.
            sqlStatement = "INSERT "
                         + "INTO " + _parameterTableName + " "
                         + "(Parameter_Name, Parameter_Value) "
                         + "VALUES('" + parameterName + "', '" + insertValue + "')";
          }

        }
        else
        {

          //  Determine if the "Parameter_Encrypted" Field exists in the Parameter Table. If so, populate
          //  it accordingly.
          if (EncryptionFieldExists())
          {
            //  Build the SQL Statement that will insert the Parameter into the Parameter Table and populate
            //  the Parameter Encrypted Table with a "0" to indicate that the parameter is not encrypted.
            sqlStatement = "INSERT "
                         + "INTO " + _parameterTableName + " "
                         + "(Parameter_Name, Parameter_Value, Parameter_Encrypted) "
                         + "VALUES('" + parameterName + "', '" + insertValue + "', 0)";
          }
          else
          {
            //  Build the SQL Statement that will insert the Parameter into the Parameter Table.  The
            //  Parameter_Encrypted Field will not be populated since it does not exist in the Parameter Table.
            sqlStatement = "INSERT "
                         + "INTO " + _parameterTableName + " "
                         + "(Parameter_Name, Parameter_Value) "
                         + "VALUES('" + parameterName + "', '" + insertValue + "')";
          }

        }


        //  Create the Command Object that will be used to Write the Parameter to the Parameter Table.
        parameterWriteCommand = new SqlCommand();
        parameterWriteCommand.Connection = _parameterDatabaseConnection;
        parameterWriteCommand.CommandText = sqlStatement;


        //  Write the Parameter to the Parameter Table.
        rowsAffected = parameterWriteCommand.ExecuteNonQuery();


        //  Close the Command Object since it is no longer needed.
        parameterWriteCommand.Dispose();


        //  Determine if the Parameter was written to the Parameter Table correctly.
        if (rowsAffected > 0)
        {

          //  Return a "TRUE" to the calling routine to indicate that this process succeeded.
          return true;

        }
        else
        {

          //  Return a "FALSE" to the calling routine to indicate that this process FAILED.
          return true;

        }

      }
      catch
      {
        //  Return FALSE to the calling routine to indicate that this process failed.
        return false;

      }

    }   //  WriteParameter


    /// <summary>
    /// Replaces the Parameter Value associated with the passed Key in the Parameter Table with the new value
    /// passed to the function.
    /// </summary>
    /// <param name="parameterName">
    /// The name (key) of the parameter whose value is to be modified.
    /// </param>
    /// <param name="parameterValue">
    /// The new Parameter Value that is to replace the existing Value in the Parameter Table.
    /// </param>
    /// <returns>
    /// TRUE - If the Parameter Value was updated successfully.
    /// FALSE - If the parameter was not updated successfully.
    /// </returns>
    /// <summary>
    public bool ModifyParameter(string parameterName, string parameterValue)
    {

      bool parameterModified = false;

      //  Default the Parameter Encrypted Value to "FALSE" and modify the parameter value in
      //  the database.
      parameterModified = ModifyParameter(parameterName, parameterValue, false);


      return parameterModified;


    }   //  ModifyParameter


    /// <summary>
    /// Replaces the Parameter Value associated with the passed Key in the Parameter Table with the new value
    /// passed to the function.
    /// </summary>
    /// <param name="parameterName">
    /// The name (key) of the parameter whose value is to be modified.
    /// </param>
    /// <param name="parameterValue">
    /// The new Parameter Value that is to replace the existing Value in the Parameter Table.
    /// </param>
    /// <param name="parameterEncrypted">
    /// An indicator of whether the Parameter Value should be encrypted in the Parameter Table.
    /// </param>
    /// <returns>
    /// TRUE - If the Parameter Value was updated successfully.
    /// FALSE - If the parameter was not updated successfully.
    /// </returns>
    /// <summary>
    public bool ModifyParameter(string parameterName, string parameterValue, bool parameterEncrypted)
    {
      bool modifiedParameter = false;

      try
      {
        //  Call the Overload of the Modify Parameter Method that requires a New Parameter Name Value defaulting
        //  that value to be the same as the existing Parameer Name.
        modifiedParameter = ModifyParameter(parameterName, parameterValue, parameterEncrypted, parameterName);


        //  Return the results of the Modify Attempt to the calling method.
        return modifiedParameter;

      }
      catch
      {
        //  Return FALSE to teh calling routine to indicate that this process failed.
        return false;

      }

    }   //  ModifyParameter


    /// <summary>
    /// Replaces the Parameter Value associated with the passed Key in the Parameter Table with the new value
    /// passed to the function.
    /// </summary>
    /// <param name="parameterName">
    /// The name (key) of the parameter whose value is to be modified.
    /// </param>
    /// <param name="parameterValue">
    /// The new Parameter Value that is to replace the existing Value in the Parameter Table.
    /// </param>
    /// <param name="parameterEncrypted">
    /// An indicator of whether the Parameter Value should be encrypted in the Parameter Table.
    /// </param>
    /// <param name="newParameterName">
    /// The new parameter name if the name of the existing parameter is to be altered.
    /// </param>
    /// <returns>
    /// TRUE - If the Parameter Value was updated successfully.
    /// FALSE - If the parameter was not updated successfully.
    /// </returns>
    public bool ModifyParameter(string parameterName, string parameterValue, bool parameterEncrypted, string newParameterName)
    {

      EncryptionManager encryptionManager       = null;
      string            insertValue             = null;
      int               encryptedIndicatorValue = -9999;
      string            sqlStatement            = null;
      SqlCommand        parameterModifyCommand  = null;
      int               rowsAffected            = -9999;

      try
      {
        //  Make sure a Valid Parameter Name was passed to this routine.  If not, exit.
        if (parameterName.Length == 0)
        {
          //  Return a "FALSE" to the calling routine to indicate that this process failed.
          return false;
        }


        //  If the Parameter Value is to be encrypted, encrypt it.
        if (parameterEncrypted)
        {
          //  Instantiate an Encryption Manager Class.
          encryptionManager = new EncryptionManager();

          //  Encrypt the Parameter Value and set the encrypted string to be the value that will be inserted
          //  into the Parameter Table.
          insertValue = encryptionManager.EncryptString(parameterValue, "sombra1");

          //  Set the Parameter Encrypted Indicator Value to "1".
          encryptedIndicatorValue = 1;

        }
        else
        {
          //  Set the clear text Parameter Value as the string that will be inserted into the Parameter Table.
          insertValue = parameterValue;

          //  Set the Parameter Encrypted Indicator Value to "0".
          encryptedIndicatorValue = 0;

        }


        //  Make sure there is a valid connection to the Parameter Table before moving on.
        if (!ValidConnection())
        {
          //  Return a "FALSE" to the calling routine to indicate that this process failed.
          return false;
        }


        //  Check to make sure that the Parameter does exist in the Parameter Table.  If it does not,
        //  fail this process.
        if (!ParameterExists(parameterName))
        {
          //  Return a "FALSE" to the calling routine to indicate that this process failed.
          return false;
        }


        //  Determine if the "Parameter_Encrypted" Field exists in the Parameter Table. If so, populate
        //  it accordingly.
        if (EncryptionFieldExists())
        {
          //  Build the SQL Statement that will modify the Parameter in the Parameter Table.
          sqlStatement = "UPDATE" + _parameterTableName + " "
                       + "SET "
                       + "Parameter_Name = '" + newParameterName + "', "
                       + "Parameter_Value = '" + insertValue + "', "
                       + "Parameter_Encrypted = " + encryptedIndicatorValue + " "
                       + "WHERE Parameter_Name = '" + parameterName + "'";
        }
        else
        {
          //  Build the SQL Statement that will modify the Parameter in the Parameter Table.  The
          //  Parameter_Encrypted Field will not be populated since it does not exist in the Parameter Table.
          sqlStatement = "UPDATE" + _parameterTableName + " "
                       + "SET "
                       + "Parameter_Name = '" + newParameterName + "', "
                       + "Parameter_Value = '" + insertValue + "', "
                       + "WHERE Parameter_Name = '" + parameterName + "'";
        }



        //  Create the Command Object that will be used to Write the Parameter to the Parameter Table.
        parameterModifyCommand = new SqlCommand();
        parameterModifyCommand.Connection = _parameterDatabaseConnection;
        parameterModifyCommand.CommandText = sqlStatement;


        //  Write the Parameter to the Parameter Table.
        rowsAffected = parameterModifyCommand.ExecuteNonQuery();


        //  Close the Command Object since it is no longer needed.
        parameterModifyCommand.Dispose();


        //  Determine if the Parameter was written to the Parameter Table correctly.
        if (rowsAffected > 0)
        {

          //  Return a "TRUE" to the calling routine to indicate that this process succeeded.
          return true;

        }
        else
        {

          //  Return a "FALSE" to the calling routine to indicate that this process FAILED.
          return true;

        }

      }
      catch
      {
        //  Return FALSE to teh calling routine to indicate that this process failed.
        return false;

      }

    }   //  ModifyParameter


    /// <summary>
    /// Determines whether the Parameter Value at associated with the passed Parameter Name is encrypted.
    /// </summary>
    /// <param name="parameterName">
    /// The Name (Key) of the parameter that is to be tested to determine if the Parameter Value is encrypted.
    /// </param>
    /// <returns>
    /// TRUE - If the Parameter Value is encrypted.
    /// FALSE - If the Parameter Value is NOT encrypted
    /// </returns>
    public bool ParameterEncrypted(string parameterName)
    {

      string        sqlStatement              = null;
      SqlCommand    parameterEncryptedCommand = null;
      SqlDataReader parameterEncryptedReader  = null;
      int           parameterValue            = -9999;
      bool          parameterIsEncrypted      = false;

      try
      {
        //  Make sure there is a valid connection to the Parameter Table before moving on.
        if (!ValidConnection())
        {
          //  Return a "FALSE" to the calling routine to indicate that this process failed.
          return false;
        }


        //  Make sure the "Parameter_Encrypted" Field exists in the Parameter Table before moving on.  If
        //  it does not, return a "FALSE" to the calling routine since there is no way to tell if the value
        //  is encrypted.
        if (!EncryptionFieldExists())
        {
          //  Return a "FALSE" to the calling routine to indicate that this process failed.
          return false;
        }


        //  Build the SQL Statement that will be used to determine if the Parameter Value is encrypted in
        //  the Parameter Table.
        sqlStatement = "SELECT Parameter_Encrypted "
                     + "FROM " + _parameterTableName + " "
                     + "WHERE Parameter_Name = '" + parameterName + "'";

        //  Create the command object and that will be used to retrieve the Parameter Encrypted Indicator from
        //  the Parameter Table.
        parameterEncryptedCommand = new SqlCommand();
        parameterEncryptedCommand.Connection = _parameterDatabaseConnection;
        parameterEncryptedCommand.CommandText = sqlStatement;


        //  Use the command that was just created to populate a data reader.
        parameterEncryptedReader = parameterEncryptedCommand.ExecuteReader();


        //  If the Data Reader contains rows, the Parameter Encrypted Indicator was retrieved so determine
        //  whether or not Parameter Value is encrypted (1=Yes, 2=No).
        if (parameterEncryptedReader.HasRows)
        {
          //  Retrieve the data from the Table.
          parameterEncryptedReader.Read();
          //  Get the Parameter Value from the reader.
          parameterValue = parameterEncryptedReader.GetInt16(parameterEncryptedReader.GetOrdinal("Parameter_Encrypted"));
          //  If the Parameter Encrypted Value is 1, the Parameter is encrypted.  Otherwise, it is not.
          if (parameterValue == 1)
          {
            parameterIsEncrypted = true;
          }
          else
          {
            parameterIsEncrypted = false;
          }
        }
        else
        {
          //  Default the encrypted value to "FALSE".
          parameterIsEncrypted = false;
        }


        //  Close the Parameter Value SQL Reader.
        parameterEncryptedReader.Close();


        //  Return the retrieve value to the calling routine.
        return parameterIsEncrypted;

      }
      catch
      {
        //  Return FALSE since the encryption of the parameter could not be determined.
        return false;

      }

    }   //  ParameterEncrypted


    /// <summary>
    /// Deletes the Parameter Value and Parameter Name (Key) [passed to routine] from the Parameter Table.
    /// </summary>
    /// <param name="parameterName">
    /// The Parameter Name (Key) of the Parameter Name/Value pair that is to be deleted from the Parameter
    /// Table.
    /// </param>
    /// <returns>
    /// TRUE - If the Parameter Name/Value pair was successfully deleted from the Parameter Table.
    /// FALSE - If the Parameter Name/Value pair was NOT successfully deleted from the Parameter Table.
    /// </returns>
    public bool RemoveParameter(string parameterName)
    {

      string     sqlStatement           = null;
      SqlCommand parameterRemoveCommand = null;
      int        rowsAffected           = -9999;

      try
      {
        //  Make sure a Valid Parameter Name was passed to this routine.  If not, exit.
        if (parameterName.Length == 0)
        {
          //  Return a "FALSE" to the calling routine to indicate that this process failed.
          return false;
        }


        //  Build the SQL Statement that will be used to determine if the Parameter Value is encrypted in
        //  the Parameter Table.
        sqlStatement = "DELETE "
                     + "FROM " + _parameterTableName + " "
                     + "WHERE Parameter_Name = '" + parameterName + "'";

        //  Create the command object and that will be used to DELETE the parameter from the Parameter Table.
        parameterRemoveCommand = new SqlCommand();
        parameterRemoveCommand.Connection = _parameterDatabaseConnection;
        parameterRemoveCommand.CommandText = sqlStatement;


        //  Delete the Parameter from the Parameter Table.
        rowsAffected = parameterRemoveCommand.ExecuteNonQuery();


        //  Close the Command Object since it is no longer needed.
        parameterRemoveCommand.Dispose();


        //  Determine if the Parameter was deleted from the Parameter Table successfully.
        if (rowsAffected > 0)
        {

          //  Return a "TRUE" to the calling routine to indicate that this process succeeded.
          return true;

        }
        else
        {

          //  Return a "FALSE" to the calling routine to indicate that this process FAILED.
          return true;

        }

      }
      catch
      {
        //  Return FALSE to the calling routine to indicate that the process failed and that
        //  the parameter was not successfully removed.
        return false;
      }

    }   //  RemoveParameter


    /// <summary>
    /// Determines whether or not there is a valid connection to the SQL Server Database that houses
    /// the Parameter Table.
    /// </summary>
    /// <returns>
    /// TRUE - If there is a valid connection to the database.
    /// FALSE - If there is NOT a valic connection to the database.
    /// </returns>
    private bool ValidConnection()
    {

      try
      {
        //  Confirm whether or not the connection is open.
        if (_parameterDatabaseConnection.State == System.Data.ConnectionState.Open)
        {
          //  The connection is open so return a "TRUE" to the calling routine.
          return true;
        }
        else
        {
          //  The connection is NOT open so return a "FALSE" to the calling routine.
          return false;
        }

      }
      catch
      {
        //  Return FALSE to the calling routine to indicate that the connection state
        //  could not be determined and thus is considered to not exist.
        return false;

      }

    }   //  ValidConnection


    /// <summary>
    /// Determines whether or not the Parameter Table schema includes a "Parameter_Encrypted" Field.
    /// </summary>
    /// <returns>
    /// TRUE - If the "Parameter_Encrypted" Field exists in the Parameter Table.
    /// FALSE - If the "Parameter_Encrypted" Field does NOT exist in the Parameter Table.
    /// </returns>
    private bool EncryptionFieldExists()
    {

      string        sqlStatement          = null;
      SqlCommand    parameterFieldCommand = null;
      SqlDataReader parameterFieldReader  = null;
      bool          fieldExists           = false;

      try
      {
        //  Make sure there is a Valid Connection to the Parameter Database before moving on.
        if (!ValidConnection())
        {
          //  Return a "FALSE" to the calling routine to indicate that this process failed.
          return false;
        }


        //  Build the SQL Statement that will be used to determine if the Encrypted Field Exists in the
        //  Parameter Table.
        sqlStatement = "SELECT TOP 1 * "
                     + "FROM " + _parameterTableName;

        //  Create the command object and that will be used to determine if the Encrypted Field exists in the
        //  Parameter Table.
        parameterFieldCommand = new SqlCommand();
        parameterFieldCommand.Connection = _parameterDatabaseConnection;
        parameterFieldCommand.CommandText = sqlStatement;


        //  Use the command that was just created to populate a data reader.
        parameterFieldReader = parameterFieldCommand.ExecuteReader();


        //  Determine if the Field Exists in the Table.
        fieldExists = false;
        for (int i = 0; i < parameterFieldReader.FieldCount; i++)
        {
          //  Determine if the current Field is the "PARAMETER_ENCRYPTED" Field.
          if (parameterFieldReader.GetName(i).ToUpper() == "PARAMETER_ENCRYPTED")
          {
            //  Set the Field Exists Indicator to "TRUE".
            fieldExists = true;
            //  Increment the loop index to the maximum value so this loop will be exited.
            i = parameterFieldReader.FieldCount;
          }
        }


        //  Close the Data Reader since it is no longer needed.
        parameterFieldReader.Close();


        //  Return the value of the Field Exists Indicator to the calling routine.
        return fieldExists;

      }
      catch
      {
        //  Return FALSE to the calling routine to indicate that the Fields Existence could
        //  not be determined.
        return false;

      }

    }   //  EncryptionFieldExists


    #region Class Disposal
    /// <summary>
    /// Final Class cleanup.
    /// </summary>
    ~ParameterManager()
    {
      //  Call the Dispose method.
      Dispose(false);

    }   //  ~FMEJobManager()


    /// <summary>
    /// Method to Dispose of Unmanaged Resources that were instantiated by the class.
    /// </summary>
    /// <param name="disposing">
    /// Indicator of whether the Dispose Override was called or if the Disposed Method was called by the Garbage Collector (Fianlize Method).
    /// </param>
    protected void Dispose(bool disposing)
    {
      if (!_isDisposed)
      {
        if (disposing)
        {
          //  If the Database Connection was instantiated by this method, close it.
          if (_parameterDatabaseConnection != null)
          {
            if (_parameterDatabaseConnection.State != System.Data.ConnectionState.Closed)
            {
              _parameterDatabaseConnection.Close();
            }
            _parameterDatabaseConnection.Dispose();
            _parameterDatabaseConnection = null;
          }
        }

      }

      //  Set the Indicator to indicate that the Class has been disposed.
      _isDisposed = true;

    }   //  Dispose()


    /// <summary>
    /// Public Dispose Method called from other classess to cleanup the memory allocated by this class.
    /// </summary>
    public void Dispose()
    {
      //  Call the Dispose Override to cleanup the Class resources.
      Dispose(true);

      //  Suppress Finalization.
      GC.SuppressFinalize(this);

    }   //  Dispose()
    #endregion Class Disposal

    //  The name of the Table that houses the Parameter Keys and Values.
    string                              _parameterTableName          = null;

    //  The SQL Connection Object for the Parameter Database.
    System.Data.SqlClient.SqlConnection _parameterDatabaseConnection = null;

    //  Indicator of whether or not the Class has been disposed of.
    bool                                _isDisposed                  = false;

  }   //  Class:  ParameterManager

}   //  NameSpace:  PDX.BTS.DataMaintenance.MaintTools

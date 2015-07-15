using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PDX.BTS.DataMaintenance.MaintTools
{

  //  Set the GUID that will be used when this Library is referenced by other applications.  Specifying a 
  //  GUID insures that the library will maintain binary compatibility through all build operations.
  [System.Runtime.InteropServices.Guid("7A9840BA-EA98-40f7-8D8C-42446A63F380")]

  public class ConfigFileManager
  {

    /// <summary>
    /// Configuration File Constructor - sets the Path (Full UNC Path and File Name) the Configuration File
    /// and sets the file as the Profile Configuration File Object.
    /// </summary>
    /// <param name="ConfigFilePath">
    /// The path to the Configuration File (full UNC Path and File Name).
    /// </param>
    public void ConfigFile(string ConfigFilePath)
    {

      try
      {
        //  Make sure the Configuration File exists before attempting to open it as the Profile
        //  Configuration File Object.
        if (!File.Exists(ConfigFilePath))
        {
          return;
        }


        //  Instantiate the Profile XML Configuration File Object that will be used to read and
        //  write from and to the Configuration File.
        xmlConfigurationFile = new AMS.Profile.Xml(ConfigFilePath);


        //  Exit the Method.
        return;
      }
      catch
      {
        //  Exit the method.
        return;
      }

    }   //  ConfigFile


    /// <summary>
    /// Sets the Default Value to an empty string "" and attempts to read a Setting Value at the Setting
    /// Location in the Section specified by the user from the XML Configuration File.
    /// </summary>
    /// <param name="SectionName">
    /// The Name of the Section in the XML Configuration File that contains the Setting that is to be
    /// retrieved from the file.
    /// </param>
    /// <param name="SettingName">
    /// The Name of the Setting Value that is to be retrieved from the XML Configuration File.
    /// </param>
    /// <returns>
    /// The retrieved value is the Setting was found in the XML Configuration File.
    /// </returns>
    public string ReadConfigSetting(string SectionName, string SettingName)
    {

      try
      {
        string configSetting;

        //  Default the Default Return Value to an empty string "" and call the Overload of this method
        //  that requires a default value to get the requested value from the Configuration File.
        configSetting = ReadConfigSetting(SectionName, SettingName, "");


        //  Return the value that was retrieved from the Configuration File to the calling routine.
        return configSetting;
      }
      catch
      {
        //  Return a NULL String to indicate that this process failed.
        return null;
      }

    }   //  ReadConfigSetting


    /// <summary>
    /// Reads a Setting Value at the Setting Location in the Section specified by the user from the XML
    /// Configuration File.
    /// </summary>
    /// <param name="SectionName">
    /// The Name of the Section in the XML Configuration File that contains the Setting that is to be
    /// retrieved from the file.
    /// </param>
    /// <param name="SettingName">
    /// The Name of the Setting Value that is to be retrieved from the XML Configuration File.
    /// </param>
    /// <param name="DefaultValue">
    /// The value that is to be returned to the calling routine if the Setting Value cannot be found in the
    /// XML Configuration File.
    /// </param>
    /// <returns>
    /// The retrieved value is the Setting was found in the XML Configuration File.  If the Setting Value
    /// was not found in the file, the Default Value (passed by the user) is returned.
    /// </returns>
    public string ReadConfigSetting(string SectionName, string SettingName, string DefaultValue)
    {

      //  Put the attempt to retrieve the value from the Configuration File in a "TRY" Block so that if
      //  errors are encountered, they can be handled and the Default Value can be returned to the
      //  calling routine.
      try
      {

        object objectValue;

        //  Make sure the user has identified the Configuration File before attempting to read a value from it.
        if (xmlConfigurationFile == null)
        {
          //  The file has not been identified so return the Default Value to the calling routine.
          return DefaultValue;
        }


        //  If the Entry Exists in the Configuration File, retrieve the value and return it to the calling routine.
        if (xmlConfigurationFile.HasEntry(SectionName, SettingName) == true)
        {
          //  Retrieve the Value from the Configuration File.
          objectValue = xmlConfigurationFile.GetValue(SectionName, SettingName);
          //  Return the retrieved value to the calling routine.
          return objectValue.ToString();
        }
        else
        {
          //  Since the Setting (Entry) does not exist in the Configuration File, return the Default Value
          //  to the calling routine.
          return DefaultValue;
        }

      }
      catch
      {

        //  Return the Default Value to the calling routine.
        return DefaultValue;

      }

    }   //  ReadConfigSetting


    /// <summary>
    /// Writes the specified Setting Value to the XML Configuration file at the Setting Location in the
    /// Section specified by the user.
    /// </summary>
    /// <param name="SectionName">
    /// The Name of the Section in the XML Configuration File that houses the Setting that is to be updated.
    /// </param>
    /// <param name="SettingName">
    /// The Name of the Setting that is to be written to the XML Configuration File.
    /// </param>
    /// <param name="SettingValue">
    /// The Value that is to be written to the XML Configuration File.
    /// </param>
    /// <returns>
    /// TRUE - If the value was written successfully.
    /// FALSE - If the value was NOT written successfully.
    /// </returns>
    public bool WriteConfigSetting(string SectionName, string SettingName, string SettingValue)
    {

      //  Make sure the values passed to this process are valid.  If not, abort the write attempt.
      if (SectionName == null || SectionName == "" || SettingName == null || SettingName == "" ||
        SettingValue == null || SettingValue == "")
      {
        return false;
      }

      //  Put the attempt to write the value into the Configuration File in a "TRY" Block so that if
      //  errors are encountered, they can be handled and the write attempt can be aborted.
      try
      {

        //  Make sure the user has identified the Configuration File before attempting to write a value to it.
        if (xmlConfigurationFile == null)
        {
          //  The file has not been identified so abort the write attempt.
          return false;
        }

        //  Write the value to the Configuration File.
        xmlConfigurationFile.SetValue(SectionName, SettingName, SettingValue);

        //  If the process made it to here it was successful so return a "TRUE" to the calling routine to
        //  indicate that this process succeeded.
        return true;

      }
      catch
      {

        //  Abort the write attempt.
        return false;

      }

    }   //  WriteConfigSetting


    /// <summary>
    /// Determines whether or not the specified section exists in the XML Configuration File.
    /// </summary>
    /// <param name="SectionName">
    /// The name of the Section that is to be searched for in the XML Configuration File.
    /// </param>
    /// <returns>
    /// TRUE - If the Section does exist in the XML Configuration File.
    /// FALSE - If the Section does NOT exist in the XML Configuration File.
    /// </returns>
    public bool ConfigSectionExists(string SectionName)
    {

      //  Make sure the value passed to this process was not null or an empty string.  If so, return a "FALSE"
      //  to the calling routine to indicate that the Section could be found.
      if (SectionName == null || SectionName == "")
      {
        return false;
      }

      //  Put the attempt to find the Section in the Configuration File in a "TRY" Block  so that if any errors
      //  are encountered they can be handled.
      try
      {

        //  Determine if the Section exists in the Configuration File.
        if (xmlConfigurationFile.HasSection(SectionName) == true)
        {
          //  Return a "TRUE" to the calling routine to indicate that the Section does exist in the
          //  Configuration File.
          return true;
        }
        else
        {
          //  Return a "FALSE" to the calling routine to indicate that the Section does NOT exist in the
          //  Configuration File.
          return false;
        }

      }
      catch
      {

        //  If an error was encountered, return a "FALSE" to the calling routine to indicate that the Section
        //  was not found in the Configuration File.
        return false;

      }

    }   //  ConfigSectionExists


    /// <summary>
    /// Searches the XML Configuration File to determine whether or not the specified Setting exists in the
    /// XML Configuration File.
    /// </summary>
    /// <param name="SectionName">
    /// The Name of the Section in the XML Configuration File that should be searched to find the Setting.
    /// </param>
    /// <param name="SettingName">
    /// The Name of the Setting that is to be searched for in the XML Configuration File.
    /// </param>
    /// <returns>
    /// TRUE - If the Setting does exist in the XML Configuration File.
    /// FALSE - If the Setting does NOT exist in the XML Configuration File.
    /// </returns>
    public bool ConfigSettingExists(string SectionName, string SettingName)
    {

      //  Make sure the values passed to this process were not null or empty strings.  If so, return a "FALSE"
      //  to the calling routine to indicate that the Setting could be found.
      if (SectionName == null || SectionName == "" || SettingName == null || SettingName == "")
      {
        return false;
      }

      //  Put the attempt to find the Setting in the Configuration File in a "TRY" Block  so that if any errors
      //  are encountered they can be handled.
      try
      {

        //  Determine if the Setting Exists in the Configuration File.
        if (xmlConfigurationFile.HasEntry(SectionName, SettingName))
        {
          //  Return a "TRUE" to the calling routine to indicate that the Setting does exist in the
          //  Configuration File.
          return true;
        }
        else
        {
          //  Return a "FALSE" to the calling routine to indicate that the Setting does NOT exist in the
          //  Configuration File.
          return false;
        }

      }
      catch
      {

        //  If an error was encountered, return a "FALSE" to the calling routine to indicate that the Setting
        //  was not found in the Configuration File.
        return false;

      }

    }   //  ConfigSettingExists

    //  The Configuration File Object that will be used to read and write values.
    AMS.Profile.Xml xmlConfigurationFile;

  }   //  Class:  ConfigFileManager

}   //  NameSpace:  PDX.BTS.DataMaintenance.MaintTools

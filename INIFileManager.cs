using System;
using System.Collections.Generic;
using System.Text;

namespace PDX.BTS.DataMaintenance.MaintTools
{

  //  Set the GUID that will be used when this Library is referenced by other applications.  Specifying a 
  //  GUID insures that the library will maintain binary compatibility through all build operations.
  [System.Runtime.InteropServices.Guid("DC5B9745-F9B6-420a-B0A7-836C3A2B2445")]


  public class INIFileManager
  {

		//  Import the Operating System DLL that will be used to pull the Value from the INI File.
    [System.Runtime.InteropServices.DllImport("kernel32")]
		private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal,
			                                                int size, string filePath);


		//  Import the Operating System DLL that will be used to write the value to the INI File.
    [System.Runtime.InteropServices.DllImport("kernel32")]
		private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);


		/// <summary>
		/// INIFile constructor - sets the Path (Full UNC Path and File Name) to the INI File.
		/// </summary>
		/// <param name="INIFilePath">
		/// The Path to the INI File (full UNC Path and File Name).
		/// </param>
		public void INIFile(string INIFilePath)
		{

			//  Set the Path to the INI File.
			filePath = INIFilePath;

		}   //  INIFile


		/// <summary>
		/// Reads the Value at the passed Section and Key of an INI File.
		/// </summary>
		/// <param name="Section">
		/// The Section of the INI File that contains that value that is being sought.
		/// </param>
		/// <param name="Key">
		/// The key (name) of the value that is being sought.
		/// </param>
		/// <returns>
		/// The Value from the INI File (string).
		/// </returns>
		public string ReadINIValue(string Section, string Key)
		{

			string iniValue;

			//  Set the Default Value to an empty String "" and retrieve the value from the INI FIle.
			iniValue = ReadINIValue(Section, Key, "");


			//  Return the retrieved value to the calling routine.
			return iniValue;

		}   //  ReadINIValue


		/// <summary>
		/// Reads the Value at the passed Section and Key of an INI File.  Returns the passed Default Value if
		/// the value could not be found in the INI File.
		/// </summary>
		/// <param name="Section">
		/// The Section of the INI File that contains that value that is being sought.
		/// </param>
		/// <param name="Key">
		/// The key (name) of the value that is being sought.
		/// </param>
		/// <param name="DefaultValue">
		/// The value to be returned to the calling routine if the Value cannot be found in the INI File.
		/// </param>
		/// <returns>
		/// The value from the INI File (if found), otherwise the Default Value.
		/// </returns>
		public string ReadINIValue(string Section, string Key, string DefaultValue)
		{

			StringBuilder iniValue;
			int           i        = 0;

      try
      {

			  //  Create a String Builder Object that will be used to hold the value that is returned
        //  from the INI File.
			  iniValue = new StringBuilder(255);


			  //  Get the value from the INI File.
			  i = GetPrivateProfileString(Section, Key, DefaultValue, iniValue, 255, filePath);


			  //  Return the value to the calling routine.
			  return iniValue.ToString();

      }
      catch
      {
        //  Return an empty string to indicate that this process failed.
        return "";

      }

		}   //  ReadINIValue


		/// <summary>
		/// Writes the passed value to the INI File at the Section and Key location specified.
		/// </summary>
		/// <param name="Section">
		/// The Section of the INI File into which the value is to be written.
		/// </param>
		/// <param name="Key">
		/// The key (name) of the value that is to be written to the INI File.
		/// </param>
		/// <param name="INIValue">
		/// The value to be written to the INI File.
		/// </param>
		/// <returns>
		/// TRUE - If the INI Value was written successfully.
		/// </returns>
		public bool WriteINIValue(string Section, string Key, string INIValue)
		{

      try
      {
			  //  Write the value to the INI File.
			  WritePrivateProfileString(Section, Key, INIValue, filePath);


			  //  If the process made it to here it was successful so return a "TRUE" to the calling routine.
			  return true;

      }
      catch
      {
        //  Return FALSE to the calling routine to indicate that this process failed.
        return false;

      }

		}   //  WriteINIValue

		//  Define a Field to hold the Path and File Name of the INI File.
		public string filePath;


  }   //  Class:  INIFileManager

}   //  NameSpace:  PDX.BTS.DataMaintenance.MaintTools

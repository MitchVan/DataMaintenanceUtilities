using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PDX.BTS.DataMaintenance.MaintTools
{

  //  Set the GUID that will be used when this Library is referenced by other applications.  Specifying a 
  //  GUID insures that the library will maintain binary compatibility through all build operations.
  [System.Runtime.InteropServices.Guid("97F3C135-3BD3-4426-A5D8-A415B15774B2")]


  public class LogFileManager
  {
    public LogFileManager()
    {
      //  Default the Log File Path to an empty string.
      _logFilePath = "";

    }   //  LogFileManager() Constructor


    public string CreateLogFileName(string ProcessName)
    {
      string logFileName = null;

      try
      {
        //  Call the Override of this method that requires a Log File Name Base defaulting the Base Name to
        //  the process name.
        logFileName = CreateLogFileName(ProcessName, ProcessName);


        //  Return the derived log file name to the calling routine.
        return logFileName;

      }
      catch
      {
        //  Return a NULL Pointer to indicate that this process failed.
        return null;

      }

    }   //  CreateLogFileName


    public string CreateLogFileName(string ProcessName, string LogFileNameBase)
    {

      string temporaryDirectoryPath = null;
      string finalLogFileName = null;

      try
      {
        //  Attempt to find a directory to write the log file into.
        if (System.IO.Directory.Exists(@"D:\Temp"))
        {
          //  Set the Temporary Directory Path to "D:\Temp".
          temporaryDirectoryPath = @"D:\Temp\";
        }
        else
        {
          if (System.IO.Directory.Exists(@"C:\Temp"))
          {
            //  Set the Temporary Directory Path to "C:\Temp".
            temporaryDirectoryPath = @"C:\Temp\";
          }
          else
          {
            //  Set the Temporary Directory Path to "C:\".
            temporaryDirectoryPath = @"C:\";
          }
        }


        //  Make sure there is "LogFiles" Directory in the Temporary Directory that was just
        //  discovered.
        if (!System.IO.Directory.Exists(temporaryDirectoryPath + "LogFiles"))
        {
          //  Create the LogFiles Directory.
          System.IO.Directory.CreateDirectory(temporaryDirectoryPath + "LogFiles");

          //  Update the Temporary Directory Path Directory to include the LogFiles Directory.
          temporaryDirectoryPath = temporaryDirectoryPath + @"Logfiles\";

        }
        else
        {
          //  Update the Temporary Directory Path Directory to include the LogFiles Directory.
          temporaryDirectoryPath = temporaryDirectoryPath + @"Logfiles\";

        }

   
        //  Make sure there is a Directory to hold the log file in the folder that was just
        //  selected and set that directory as the one in which the log file should be written.
        if (!System.IO.Directory.Exists(temporaryDirectoryPath + ProcessName + "Logs"))
        {
          //  Create the directory.
          System.IO.Directory.CreateDirectory(temporaryDirectoryPath + ProcessName + "Logs");
        }
        //  Set the Directory into which the Log File will be written.
        temporaryDirectoryPath = temporaryDirectoryPath + ProcessName + @"Logs\";
        _logFilePath = temporaryDirectoryPath;


        //  Construct the File Name (including the path).
        _logFileNameDatePrefix = DateTime.Today.Year + "-" + DateTime.Today.Month + "-" + DateTime.Today.Day;
        finalLogFileName = this.LogFileNameDatePrefix + "-" + LogFileNameBase.Replace(@"\", "-") + ".log";
        finalLogFileName = temporaryDirectoryPath + finalLogFileName;


        //  Return the Log File Name to the calling routine.
        return finalLogFileName;

      }
      catch
      {
        //  Return a NULL Pointer to indicate that this process failed.
        return null;

      }

    }   //  CreateLogFileName


    /// <summary>
    /// Opens the Log File and writes a Header to it.  If there is an existing Log File with the same name,
    /// it is renamed to [FileName]."old" and a new file with the desired name is created.
    /// </summary>
    /// <param name="fileName">
    /// The name of the Log File (full UNC Path Included) that is to be created.
    /// </param>
    /// <returns>
    /// TRUE - If the Log File was created successfully.
    /// FALSE - If the Log File was NOT created successfully.
    /// </returns>
    public bool OpenFile(string fileName)
    {
      bool openedFile = false;

        //  Call the Open File Overload that requires an indicator of whether the information should be appended
        //  to an existing file.  Default the Append Indicator to "FALSE".
        openedFile = OpenFile(fileName, false);


        //  Return the Process Result to the calling routine.
        return openedFile;

    }   //  OpenFile


    /// <summary>
    /// Opens the Log File and writes a Header to it.  If there is an existing Log File with the same name,
    /// the Append Existing Parameter determines whether the new information is appended to the exitsting file
    /// or if the existing file is renamed to [FileName]."old" and a new Log File is created.
    /// </summary>
    /// <param name="fileName">
    /// The name of the Log File (full UNC Path Included) that is to be created.
    /// </param>
    /// <param name="appendExisting">
    /// Indicator of whether the current information should be appended to an existing Log File (if it exists)
    /// or if the existing file should be renamed to [FileName]."old" and a new Log File be created.
    ///    TRUE - Append information to the existing file.
    ///    FALSE - Create a new Log File to accept the information from this run.
    /// </param>
    /// <returns>
    /// TRUE - If the Log File was created successfully.
    /// FALSE - If the Log File was NOT created successfully.
    /// </returns>
    public bool OpenFile(string fileName, bool appendExisting)
    {

      try
      {
        //  Create and open the output Log File.  If the Log File has already been created and
        //  opened, do nothing.
        if (_outputTextStream == null)
        {

          //  If the Output File already Exists, append to it if desired or copy the existing file to a new
          //  name (*.old) and delete the existing file so that the new one can be created.
          if (File.Exists(fileName))
          {
            //  The file already exists, determine if it should be appended to or replaced with a new file.
            if (appendExisting)
            {
              //  We are appending to the existing file, so open it for writing.
              _outputTextStream = File.AppendText(fileName);
            }
            else
            {
              //  Since the user does not want to append to the existing file, copy it to a new name and
              //  delete the existing file before attempting to create the new file.
              if (File.Exists(fileName + ".old"))
              {
                File.Delete(fileName + ".old");
              }
              File.Move(fileName, fileName + ".old");
              File.Delete(fileName);
              //  Create the new file that will be written to.
              _outputTextStream = File.CreateText(fileName);
            }
          }
          else
          {
            //  There was not an existing file so create the output stream.
            _outputTextStream = File.CreateText(fileName);
          }

          //  Make sure there is a valid TextStream before moving on.
          if (_outputTextStream == null)
          {
            return false;
          }

        }


        //  Place standard header info into the log... all writes to the log go through the
        //  WriteLine method which will optionally timestamp teh message.
        WriteLine("====================================================================================================");
        WriteLine("Log file opened.", true);
        WriteLine("");


        //  If the process got to here, it was successful so return a "TRUE" to the calling routine.
        return true;

      }
      catch
      {
        //  Return FALSE to the calling routine to indicate that this process failed.
        return false;

      }

    }   //  OpenFile


    /// <summary>
    /// Writes the passed message to the Log File.
    /// </summary>
    /// <param name="message">
    /// The message (information) that should be written to the Log File.
    /// </param>
    /// <returns>
    /// TRUE - If the message was successfully written to the Log File.
    /// FALSE - If the message was NOT successfully written to the Log File.
    /// </returns>
    public bool WriteLine(string message)
    {
      bool wroteLine;

        //  Call the process to write the line to the file.
        wroteLine = WriteLine(message, false);


        //  Make sure the line was successfully written to the output log file.
        if (wroteLine)
        {
          //  Return "TRUE" to the calling routine to indicate that the process succeeded.
          return true;
        }
        else
        {
          //  Return "FALSE" to the calling routine to indicate that the process failed.
          return false;
        }

    }   //  WriteLine


    /// <summary>
    /// Writes the passed message to the Log File.  If the user has indicated that a Time Stamp should be
    /// prepended to message (Time Stamp parameter), add the time stamp and write the combined message to
    /// the Log File.
    /// </summary>
    /// <param name="message">
    /// The message (information) that should be written to the Log File.
    /// </param>
    /// <param name="timeStamp">
    /// Indicates whether the user would like a Time Stamp prepended to the message when it is written to
    /// the Log File -
    ///				TRUE - A time stamp is prepended to the message before the message is written to the Log File.
    ///				FALSE - The message is written to the Log File as it was passed to this routine.
    /// </param>
    /// <returns>
    /// TRUE - If the message was successfully written to the Log File.
    /// FALSE - If the message was NOT successfully written to the Log File.
    /// </returns>
    public bool WriteLine(string message, bool timeStamp)
    {

      DateTime currentTime;
      string currentTimeString;

      try
      {
        //  Make sure there is a valid Output Text Stream before attempting to write anything.
        if (_outputTextStream == null)
        {
          return false;
        }


        //  If the user requested that a Time/Date Stamp be appended to the line, get the current date and time.
        if (timeStamp)
        {
          //  Get the current time and format it into a readable string.
          currentTime = DateTime.Now;
          currentTimeString = currentTime.ToString("ddd, dd MMM yyyy hh:mm:ss tt");
          //  Prepend the time to the message and delimit the two pieces with a pipe '|'.
          message = currentTimeString + " | " + message;
        }


        //  Write the message to the log.
        if (message.Length == 0)
        {
          //  Write a blank line to the log file.
          _outputTextStream.WriteLine("");
        }
        else
        {
          //  Write the message the user passed to the log file.
          _outputTextStream.WriteLine(message);
        }


        //  Flush the Text Stream to get everything into the output file.
        _outputTextStream.Flush();


        //  If the process made it to here, it was successful so return a "TRUE" to the calling routine.
        return true;

      }
      catch
      {
        //  Return FALSE to the calling routine to indicate that this process failed.
        return false;

      }

    }   //  WriteLine


    /// <summary>
    /// Writes a footer to the Log File and Closes it (clearing the Text Stream Object).
    /// </summary>
    /// <returns>
    /// TRUE - If the Log File was closed successfully.
    /// FALSE - If the Log File was NOT closed successfully.
    /// </returns>
    public bool CloseFile()
    {

      try
      {
        //  Make sure there is a valid Output Stream Object Reference before moving on.
        if (_outputTextStream == null)
        {
          //  There was not a file to close so return a "TRUE" to the calling routine.
          return false;

        }
        else
        {

          //  Write a footer to the log file and then close it.
          WriteLine("");
          WriteLine("Log file closed.", true);
          WriteLine("====================================================================================================");

          //  Close the Text Stream.
          _outputTextStream.Close();

        }


        //  Close the Output Text Stream Object Reference.
        _outputTextStream = null;


        //  If the process got to here, it was successful so return a "TRUE" to the calling routine.
        return true;

      }
      catch
      {
        //  Return FALSE to the calling routine to indicate that this process failed.
        return false;

      }

    }   //  CloseFile

    #region Class Disposal
    /// <summary>
    /// Final Class cleanup.
    /// </summary>
    ~LogFileManager()
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
          if (_outputTextStream != null)
          {
            _outputTextStream.Dispose();
            _outputTextStream = null;
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

    //  Define the Field variables
    System.IO.TextWriter  _outputTextStream      = null;

    //  The path to the log file that is being populated.
    private static string _logFilePath           = null;

    //  The Date Prefix to be pre-pended to the Log File Name.
    private static string _logFileNameDatePrefix = null;

    //  An indicator of whether or not this class has been disposed of.
    bool                  _isDisposed            = false;

    //  Maintain the Log File Path for this process in a property so that it can be used to log other processes run by the application.
    public string LogFilePath
    {
      get
      {
        return _logFilePath;
      }
      set
      {
        _logFilePath = value;
      }
    }

    //  Maintain the Log File Name Date Prefix in a Property so that it can be used to prefix other values in the process that is running.
    public string LogFileNameDatePrefix
    {
      get
      {
        return _logFileNameDatePrefix;
      }
      set
      {
        _logFileNameDatePrefix = value;
      }
    }

  }   //  Class:  LogFileManager

}   //  NameSpace:  PDX.BTS.DataMaintenance.MaintTools

using System;
using System.Collections.Generic;
using System.Text;

namespace PDX.BTS.DataMaintenance.MaintTools
{
  //  Set the GUID that will be used when this Library is referenced by other applications.  Specifying a 
  //  GUID insures that the library will maintain binary compatibility through all build operations.
  [System.Runtime.InteropServices.Guid("5A75717B-E743-4E25-8563-A8202C01922A")]


  public class FMEJobManager
  {
    /// <summary>
    /// A public event for sending process messages back to the calling application.
    /// </summary>
    public event SendProcessMessage ProcessMessage;


    /// <summary>
    /// A public event for sending error messages back to the calling application.
    /// </summary>
    public event SendErrorMessage ErrorMessage;


    public FMEJobManager()
    {
      try
      {
        //  Let the User know that the FME Session is being established.
        if (ProcessMessage != null)
        {
          ProcessMessage("      - Opening the FME Session in which the specified FME Job will be run...");

        }


        //  Attempt to instantiate the FME Session.
        //  Initiate an FME Session.
        _fmeSession = Safe.FMEObjects.FMEObjects.CreateSession();
        _fmeSession.Init(null);


        //  Exit this method.
        return;

      }
      catch (Safe.FMEObjects.FMEOException fmeException)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(fmeException, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that the Session Could not be created.
        if (ErrorMessage != null)
        {
          ErrorMessage("");
          ErrorMessage("");
          ErrorMessage("Failed to open the FME Session with error - " + fmeException.FmeErrorMessage + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Exit this method.
        return;

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that the Session Could not be created.
        if (ErrorMessage != null)
        {
          ErrorMessage("");
          ErrorMessage("");
          ErrorMessage("Failed to open the FME Session with error - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Exit this method.
        return;

      }

    }   //  FMEJobManager() Constructor


    #region Run Job
    /// <summary>
    /// Runs an FME Workbench Job and returns an indicator of whether or not the job completed successfully.
    /// </summary>
    /// <param name="FMEJobName">
    /// The Path and Name of the FME Workbench Job that is to be run.
    /// </param>
    /// <param name="ReRunOnError">
    /// Indicator of whether or not the FME Job should be re-run a number of times (3) if it fails the to complete before the re-run
    /// limit has been reached.
    /// </param>
    /// <returns>
    /// TRUE - If the FME Job completed successfully.
    /// FALSE - If the FME Job failed to complete.
    /// </returns>
    public bool RUNFMEWorkbenchJob(string FMEJobName, bool ReRunOnError = true)
    {
      System.Collections.Specialized.StringCollection fmeJobParametersCollection = null;

      try
      {
        //  Build an Empty Parameters String Collection.
        fmeJobParametersCollection = new System.Collections.Specialized.StringCollection();


        //  Call the Overload of this Method that requires a Parameters Array List.
        return RUNFMEWorkbenchJob(FMEJobName, fmeJobParametersCollection, ReRunOnError, "");

      }
      catch
      {
        //  Return FALSE to the calling method to indicate that this process failed.
        return false;

      }
      finally
      {
        //  If the FME Job Parameters String Collection has been instantiated, close it.
        if (fmeJobParametersCollection != null)
        {
          fmeJobParametersCollection = null;
        }

      }

    }   //  RUNFMEWorkbenchJob


    /// <summary>
    /// Runs an FME Workbench Job and returns an indicator of whether or not the job completed successfully.
    /// </summary>
    /// <param name="FMEJobName">
    /// The Path and Name of the FME Workbench Job that is to be run.
    /// </param>
    /// <param name="FMEJobParameters">
    /// The parameters necessary to run the FME JOb (String Format).  The parameters will be loaded to a String Collection and passed
    /// to the FME Workspace Runner Object.
    /// </param>
    /// <param name="ReRunOnError">
    /// Indicator of whether or not the FME Job should be re-run a number of times (3) if it fails the to complete before the re-run
    /// limit has been reached.
    /// </param>
    /// <returns>
    /// TRUE - If the FME Job completed successfully.
    /// FALSE - If the FME Job failed to complete.
    /// </returns>
    public bool RUNFMEWorkbenchJob(string FMEJobName, string FMEJobParameters, bool ReRunOnError = true)
    {
      System.Collections.Specialized.StringCollection fmeJobParametersCollection = null;

      try
      {
        //  Instantiate the Parameters String Collection.
        fmeJobParametersCollection = new System.Collections.Specialized.StringCollection();


        //  Let the user know that the Parameter Name is being added to the Parameters
        //  Collection.
        if (ProcessMessage != null)
        {
          ProcessMessage("      - Preparing the Parameters for use by the FME Object Runner...");
        }

        //  Break up the Parameters and add them to the String Collection.
        if (FMEJobParameters.Length != 0)
        {
          if (FMEJobParameters.IndexOf(@"--") != -1)
          {
            //  Split the List of Parameters into an Array.
            string[] separators = new string[] { @"|" };
            string[] currentParameterList = FMEJobParameters.Split(separators, StringSplitOptions.None);
            //  Go through the Parameters, format them and add them to the Parameters Array List.
            foreach (string currentParameterEntry in currentParameterList)
            {
              //  Split the Parameter Name and Value into an array.
              string[] parameterSeparators = new string[] { @"?", "=" };
              string[] currentParameterItems = currentParameterEntry.Split(parameterSeparators, StringSplitOptions.None);
              //  Build the full parameter string and add it to the Parameters String Collection.
              if (currentParameterItems.GetUpperBound(0) > 0)
              {
                //  Let the user know that the Parameter Name is being added to the Parameters
                //  Collection.
                if (ProcessMessage != null)
                {
                  ProcessMessage("         - Adding the Parameter Name:  " + currentParameterItems[0].Substring(2) + " to the Parameters Collection.");
                }
                //  Add the Parameter Name to the Parameters String Collection.
                fmeJobParametersCollection.Add(currentParameterItems[0].Substring(2));
                //  Let the user know that the Parameter Name is being added to the Parameters
                //  Collection.
                if (ProcessMessage != null)
                {
                  ProcessMessage("         - Adding the Parameter Value:  " + currentParameterItems[1] + " to the Parameters Collection.");
                }
                //  Add the Parameter Value to the Parameters String Collection.
                fmeJobParametersCollection.Add(currentParameterItems[1]);
              }
            }
          }
          else
          {
            //  Split the Parameter Name and Value into an array.
            string[] parameterSeparators = new string[] { @"?", "=" };
            string[] currentParameterItems = FMEJobParameters.Split(parameterSeparators, StringSplitOptions.None);
            //  Build the full parameter string and add it to the Parameters String Collection.
            if (currentParameterItems.GetUpperBound(0) > 0)
            {
              //  Let the user know that the Parameter Name is being added to the Parameters
              //  Collection.
              if (ProcessMessage != null)
              {
                ProcessMessage("         - Adding the Parameter Name:  " + currentParameterItems[0] + " to the Parameters Collection.");
              }
              //  Add the Parameter Name to the Parameters String Collection.
              fmeJobParametersCollection.Add(currentParameterItems[0]);
              //  Let the user know that the Parameter Name is being added to the Parameters
              //  Collection.
              if (ProcessMessage != null)
              {
                ProcessMessage("         - Adding the Parameter Value:  " + currentParameterItems[1] + " to the Parameters Collection.");
              }
              //  Add the Parameter Value to the Parameters String Collection.
              fmeJobParametersCollection.Add(currentParameterItems[1]);
            }
          }

        }
        else
        {
          //  Build an Empty Parameters String Collection.
          fmeJobParametersCollection.Add("");

        }


        //  Let the User know that the Parameter Collection was populated successfully.
        if (ProcessMessage != null)
        {
          ProcessMessage("      - The Parameter Collection was populated successfully.");

        }


        //  Let the user know that the overload is being called.
        if (ProcessMessage != null)
        {
          ProcessMessage("      - Calling the Overload of this method that requires a Parameter Collection...");
        }

        //  Call the Overload of this Method that requires a Parameters Array List.
        bool jobStatus = RUNFMEWorkbenchJob(FMEJobName, fmeJobParametersCollection, ReRunOnError, "");


        //  Let the user know if the overload succeeded.
        if (ProcessMessage != null)
        {
          ProcessMessage("      - The Overload of this Method returned the value:  " + jobStatus + "...");
        }

        //  Return the Job Success or Failure Indicator value to the calling method.
        return jobStatus;

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the user know that the method failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The Maintools.FMEJobManager.RUNFMEWorkBenchJob(FMEJobName, FMEJobParameters(String), FMELogFile) Method Failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling method to indicate that this process failed.
        return false;

      }
      finally
      {
        //  If the FME Job Parameters String Collection has been instantiated, close it.
        if (fmeJobParametersCollection != null)
        {
          fmeJobParametersCollection = null;
        }

      }

    }   //  RUNFMEWorkbenchJob


    /// <summary>
    /// Runs an FME Workbench Job and returns an indicator of whether or not the job completed successfully.
    /// </summary>
    /// <param name="FMEJobName">
    /// The Path and Name of the FME Workbench Job that is to be run.
    /// </param>
    /// <param name="FMEJobParameters">
    /// The parameters necessary to run the FME JOb (String Format).  The parameters will be passed to the FME Workspace Runner Object.
    /// </param>
    /// <param name="ReRunOnError">
    /// Indicator of whether or not the FME Job should be re-run a number of times (3) if it fails the to complete before the re-run
    /// limit has been reached.
    /// </param>
    /// <param name="FMELogFile">
    /// The Path and Name of the FME Session Log File that should be written by this process if one is desired.
    /// </param>
    /// <returns>
    /// TRUE - If the FME Job completed successfully.
    /// FALSE - If the FME Job failed to complete.
    /// </returns>
    public bool RUNFMEWorkbenchJob(string FMEJobName, System.Collections.Specialized.StringCollection FMEJobParameters, bool ReRunOnError = true, string FMELogFile = "")
    {
      PDX.BTS.DataMaintenance.MaintTools.GeneralUtilities generalUtilities   = null;
      Safe.FMEObjects.IFMEOWorkspaceRunner                fmeWorkspaceRunner = null;

      try
      {
        //  Set the ReTry Attempts Counter to 0.
        int retryAttempts = 0;


        //  Instantiate a Data Maintenance Utilities General Utilities Object.
        generalUtilities = new PDX.BTS.DataMaintenance.MaintTools.GeneralUtilities();
        generalUtilities.ErrorMessage += new SendErrorMessage(HandleErrorMessage);


        //  If a parameter includes a directory path, make sure that path exists.
        string previousValue = "";
        foreach(string currentParameterString in FMEJobParameters)
        {
          try
          {
            //  Determine if the Current Parameter String is a directory Path.
            if ((System.IO.Path.GetFullPath(currentParameterString).Length > 0) && (System.IO.Path.IsPathRooted(currentParameterString)))
            {
              //  Make sure the path exists or can be created.
              if (!generalUtilities.ConfirmDirectoryPath(System.IO.Path.GetDirectoryName(currentParameterString), true))
              {
                //  Let the user know what happened.
                if (ErrorMessage != null)
                {
                  ErrorMessage("Could not confirm the path - " + currentParameterString + " for parameter -" + previousValue + "!  Aborting the Maintools.FMEJobManager.RUNFMEWorkbenchJob() Method.");
                }
                //  Return FALSE to the calling method to indicate that this method failed.
                return false;
              }
            }
          }
          catch(System.ArgumentException)
          {
            //  This is not a Path so do nothing.
          }
          catch (System.NotSupportedException)
          {
            //  This is not a Path so do nothing.
          }
          catch
          {
            //  This is not a Path so do nothing.
          }

          //  Set the Current Parameter Value to be the Previous Value for the next value.
          previousValue = currentParameterString;

        }


        //  This Label will be used when the job run attempt is retryed.
        attemptFMEJobReRun:


        //  Default the ReRun Job Indicator Variable to FALSE.
        bool rerunJob = false;


        //  Attempt to run the FME Job in a "TRY" Block so that it can be "re-run" if it fails for a recoverable
        //  reason.  If the failure is not recoverable, do not attempt to re-run the job.
        bool successfullyRanFMEWorkspace = false;
        try
        {
          //  Let the User know what is happening.
          if (ProcessMessage != null)
          {
            ProcessMessage("      - Preparing the FME Workspace Runner Object that will run the FME Job...");

          }

          //  Make sure there is a valid FME Session Object available before attempting to create
          //  Workspace Runner in it.
          if (_fmeSession == null)
          {
            //  Let the User know what is happening.
            if (ProcessMessage != null)
            {
              ProcessMessage("        + There is not a current valid FME Session, Instantiating one...");
            }

            //  Instantiate an FME Session Object.
            _fmeSession = Safe.FMEObjects.FMEObjects.CreateSession();
            _fmeSession.Init(null);

          }


          //  Create the Workspace Runner in a try block so that any errors can be trapped.
          try
          {
            //  Instantiate the FME Workspace Runner Object.
            Safe.FMEObjects.IFMEOWorkspaceRunner createFMEWorkspaceRunner = _fmeSession.CreateWorkspaceRunner();
            fmeWorkspaceRunner = createFMEWorkspaceRunner;
          }
          catch (Safe.FMEObjects.FMEOException fmeException)
          {
            //  Determine the Line Number from which the exception was thrown.
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(fmeException, true);
            System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
            int lineNumber = stackFrame.GetFileLineNumber();

            //  Let the user know what happened.
            if (ErrorMessage != null)
            {
              ErrorMessage("The Process to Create the FME Workspace Runner Object Failed with error - " + fmeException.FmeErrorMessage + " (FME Error Number:  " + fmeException.FmeErrorNumber + " Line:  " + lineNumber.ToString() + ")!");
            }
            //  Return FALSE to the calling method to indicate that this method failed.
            return false;
          }
          catch (System.NullReferenceException nullReferenceException)
          {
            //  Determine the Line Number from which the exception was thrown.
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(nullReferenceException, true);
            System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
            int lineNumber = stackFrame.GetFileLineNumber();

            //  Let the user know what happened.
            if (ErrorMessage != null)
            {
              ErrorMessage("The Process to Create the FME Workspace Runner Object Failed with error - " + nullReferenceException.Message + " (Line:  " + lineNumber.ToString() + ")!");
            }
            //  Return FALSE to the calling method to indicate that this method failed.
            return false;
          }
          catch (System.Exception caught)
          {
            //  Determine the Line Number from which the exception was thrown.
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
            System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
            int lineNumber = stackFrame.GetFileLineNumber();

            //  Let the user know what happened.
            if (ErrorMessage != null)
            {
              ErrorMessage("The Process to Create the FME Workspace Runner Object Failed with error - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
            }
            //  Return FALSE to the calling method to indicate that this method failed.
            return false;
          }


          //  Let the User know what is happening.
          if (ProcessMessage != null)
          {
            ProcessMessage("      - Running the specified FME Job...");

          }


          //  Run the specified FME Workbench Job using the specified parameters.
          successfullyRanFMEWorkspace = fmeWorkspaceRunner.RunWithParameters(FMEJobName, FMEJobParameters);

          //  Make sure the job finished successfully before moving on.
          if (!successfullyRanFMEWorkspace)
          {
            //  Determine if the job should be retried.
            if ((retryAttempts <= 2) && (ReRunOnError))
            {
              //  Increment the Retry Attempts Counter.
              retryAttempts++;
              //  Let the user know that the job is being re-run.
              if (ProcessMessage != null)
              {
                ProcessMessage("");
                ProcessMessage("      - The FME Job did not complete successfully!");
                ProcessMessage("      - Attempting to re-run the FME Job.  Retry attempt - " + retryAttempts.ToString() + "...");
                ProcessMessage("");
              }
              //  Set the ReRun Job Indicator Variable to TRUE.
              rerunJob = true;
            }

          }

        }
        catch (System.IO.IOException ioException)
        {
          //  Dispose of the current FME Workspace Runner Object since it is no longer needed.
          fmeWorkspaceRunner.Dispose();

          //  Determine if the job should be retried.
          if ((retryAttempts <= 2) && (ReRunOnError))
          {
            //  Increment the Retry Attempts Counter.
            retryAttempts++;
            //  Let the user know that the job is being re-run.
            if (ProcessMessage != null)
            {
              ProcessMessage("");
              ProcessMessage("      - The FME failed with exception - " + ioException.Message + "!");
              ProcessMessage("      - Attempting to re-run the FME Job Due to an exception (" + ioException.Message + ").  Retry attempt - " + retryAttempts.ToString() + "...");
              ProcessMessage("");
            }
            //  Set the ReRun Job Indicator Variable to TRUEM
            rerunJob = true;
          }
          else
          {
            //  Determine the Line Number from which the exception was thrown.
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(ioException, true);
            System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
            int lineNumber = stackFrame.GetFileLineNumber();

            //  Let the user know that this process failed.
            if (ErrorMessage != null)
            {
              ErrorMessage("The RunFMEWorkbenchJob(FMEJobName, FMEJobParameters(Collection), ServerFMELogFile) failed with error message:  " + ioException.Message + " (Line:  " + lineNumber.ToString() + ")!");
            }
          }

        }
        catch (Safe.FMEObjects.FMEOException fmeException)
        {
          //  Dispose of the current FME Workspace Runner Object since it is no longer needed.
          fmeWorkspaceRunner.Dispose();

          //  If this
          if (fmeException.FmeErrorMessage.ToUpper().IndexOf("OBJECT INPUT PARAMETER HAS NOT") > -1)
          {
            //  Determine if the job should be retried.
            if ((retryAttempts <= 2) && (ReRunOnError))
            {
              //  Increment the Retry Attempts Counter.
              retryAttempts++;
              //  Let the user know that the job is being re-run.
              if (ProcessMessage != null)
              {
                ProcessMessage("");
                ProcessMessage("      - The FME Job failed with FME Exception - " + fmeException.FmeErrorMessage + "(" + fmeException.FmeErrorNumber + ")!");
                ProcessMessage("      - Attempting to re-run the FME Job.  Retry attempt - " + retryAttempts.ToString() + "...");
                ProcessMessage("");
              }
              //  Set the ReRun Job Indicator Variable to TRUEM
              rerunJob = true;
            }
          }
          else
          {
            //  Determine the Line Number from which the exception was thrown.
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(fmeException, true);
            System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
            int lineNumber = stackFrame.GetFileLineNumber();

            //  Let the user know that this process failed.
            if (ErrorMessage != null)
            {
              ErrorMessage("The RunFMEWorkbenchJob(FMEJobName, FMEJobParameters(Collection), ServerFMELogFile) failed with FME error message:  " + fmeException.FmeErrorMessage + " (Line:  " + lineNumber.ToString() + ")!");
            }
          }

        }
        catch (System.Exception caught)
        {
          //  Determine the Line Number from which the exception was thrown.
          System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
          System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
          int lineNumber = stackFrame.GetFileLineNumber();

          //  Le the user know that this process failed.
          if (ErrorMessage != null)
          {
            ErrorMessage("The RunFMEWorkbenchJob(FMEJobName, FMEJobParameters(Collection), ServerFMELogFile) failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
          }

        }

        //  If necessary, re-run the specified FME Job.
        if (rerunJob)
        {
          //  Retry to run the job.
          goto attemptFMEJobReRun;
        }


        //  Close the FME Workspace Runner Object since it is no longer needed.
        fmeWorkspaceRunner.Dispose();


        //  Let the User know whether or not the job finished successfully.
        if (successfullyRanFMEWorkspace)
        {
          if (ProcessMessage != null)
          {
            ProcessMessage("      - The specified FME Job finished successfully...");
          }

        }
        else
        {
          if (ErrorMessage != null)
          {
            ErrorMessage("The specified FME Workbench Job failed to complete!");
          }

        }


        //  Return the result of the run to the calling method.
        return successfullyRanFMEWorkspace;


        ////  If the process made it to here, the job finished successfully so report the successful completion of the job and return TRUE to
        ////  the calling method.
        //if (ProcessMessage != null)
        //{
        //  ProcessMessage("      - The specified FME Job finished successfully...");
        //}
        //return true;

      }
      catch (Safe.FMEObjects.FMEOException fmeException)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(fmeException, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the user know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The RunFMEWorkbenchJob(FMEJobName, FMEJobParameters(Collection), ServerFMELogFile) failed with FME error message:  " + fmeException.FmeErrorMessage + " (Line:  " + lineNumber.ToString() + ")!");

        }

        //  Return FALSE to the calling method to indicate that this process failed.
        return false;

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the user know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The RunFMEWorkbenchJob(FMEJobName, FMEJobParameters(Collection), ServerFMELogFile) failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");

        }

        //  Return FALSE to the calling method to indicate that this process failed.
        return false;

      }
      finally
      {
        //  If the BTS Data Maintenance Utilities General Utilities Object was instantiated, close it.
        if (generalUtilities != null)
        {
          generalUtilities = null;
        }
        //  If the FME Workspace Runner Object was instantiated, close it.
        if (fmeWorkspaceRunner != null)
        {
          fmeWorkspaceRunner.Dispose();
          fmeWorkspaceRunner = null;
        }

      }

    }   //  RUNFMEWorkbenchJob
    #endregion Run Job


    #region Message Handling
    private void HandleErrorMessage(string Message)
    {
      //  If the Error Message Handler has been initialized, relay the Message to the calling method.
      if (ErrorMessage != null)
      {
        ErrorMessage(Message);
      }

    }
    #endregion Message Handling


    #region Class Disposal
    /// <summary>
    /// Final Class cleanup.
    /// </summary>
    ~FMEJobManager()
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
        //  If the Database Connection was instantiated by this method, close it.
        if (_fmeSession != null)
        {
          _fmeSession.Dispose();
          _fmeSession = null;
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

    //  The FME Session Object in which all jobs will run.
    static Safe.FMEObjects.IFMEOSession _fmeSession = null;

    //  Indicator whether or not the Class has been disposed.
    private static bool                 _isDisposed = false;

  }   //  CLASS:  FMEJobManager

}   //  NAMESPACE:  PDX.BTS.DataMaintenance.MaintTools

using System;
using System.Collections.Generic;
using System.Text;

namespace PDX.BTS.DataMaintenance.MaintTools
{

  //  Set the GUID that will be used when this Library is referenced by other applications.  Specifying a 
  //  GUID insures that the library will maintain binary compatibility through all build operations.
  [System.Runtime.InteropServices.Guid("9990B9E1-F8CB-4e0e-818F-4B46E3A0D4E7")]


  //  A Public Delegate through which process status messages will be sent back to the calling application.
  public delegate void SendProcessMessage(string MessageString);


  //  A Public Delegate through which error messages will be sent back to the calling application.
  public delegate void SendErrorMessage(string MessageString);


  public class GeneralUtilities
  {
    /// <summary>
    /// A public event for sending error messages back to the calling application.
    /// </summary>
    public event SendErrorMessage ErrorMessage;


    public GeneralUtilities()
    {
      string                                                     applicationDirectory = null;
      string                                                     xmlSettingsFile      = null;
      PDX.BTS.DataMaintenance.MaintTools.XMLSetttingsFileManager xmlFileManager       = null;

      try
      {
        //  Set the Name of the XML Settings File for this Application.
        applicationDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location.ToString();
        while (applicationDirectory.EndsWith(@"\") == false)
        {
          //  Drop the right-most character from the string.
          applicationDirectory = applicationDirectory.Substring(0, (applicationDirectory.Length - 1));
        }
        xmlSettingsFile = applicationDirectory + @"DataMaintenanceUtilitiesSettings.xml";


        //  Initialize the XML File Manager.
        xmlFileManager = new PDX.BTS.DataMaintenance.MaintTools.XMLSetttingsFileManager();
        xmlFileManager.XMLFile(xmlSettingsFile);

        if (!System.IO.File.Exists(xmlSettingsFile))
        {
          //  Let the user know that the Parameter Table could not be initialized.
          if (ErrorMessage != null)
          {
            ErrorMessage("     Could not find the XML Settings File!  Initialization Failed!");
          }

          //  Exit this process.
          return;

        }

        //  Get the connection parameters for the Monitor Database from the XML Settings File.
        _monitorDatabaseServerName = xmlFileManager.ReadUserSetting("DatabaseDefinitions", "MonitorDatabaseServer");
        _monitorDatabaseName = xmlFileManager.ReadUserSetting("DatabaseDefinitions", "MonitorDatabase");


        //  Get the Name of the Load Monitor Table in the Monitor Database.
        _monitorTableName = xmlFileManager.ReadUserSetting("DatabaseDefinitions", "MonitorTableName");


        //  Set the Connection String that will be used to Connect to the Import Monitor Database.
        _importMonitorDatabaseConnectionString = "Data Source=" + _monitorDatabaseServerName + ";"
                                               + "Initial Catalog=" + _monitorDatabaseName + ";"
                                               + "Integrated Security=SSPI";


        //  Exit this process.
        return;

      }
      catch
      {
        //  Exit this method.
        return;

      }

    }   //  GeneralUtilities()


    /// <summary>
    /// Given a Start and End Time calculates and produces a Process Run Time string in hours, minutes
    /// and seconds (as necessary).
    /// </summary>
    /// <param name="startTime">
    /// The time that the process run began.
    /// </param>
    /// <param name="endTime">
    /// The time that the process run ended.
    /// </param>
    /// <returns>
    /// A string containing the process run time in hours, minutes and seconds (as necessary - for example:
    /// 3 hours 15 minutes and 30 seconds, 10 minutes and 25 seconds) .
    /// </returns>
    public string CalculateProcessRunTime(System.DateTime StartTime, System.DateTime EndTime)
    {
      try
      {
        //  Determine the process run time.
        System.TimeSpan runTime = EndTime.Subtract(StartTime);


        //  Break the Process Run Time into its component parts (hours, minutes, seconds)
        double seconds = (int)runTime.TotalSeconds;
        double minutes = (int)runTime.TotalMinutes;
        double hours = (int)runTime.TotalHours;
        double days = (int)runTime.TotalDays;
        seconds = (seconds - (minutes * 60));
        minutes = (minutes - (hours * 60));
        hours = (hours - (days * 24));


        //  Start to build the Time String.
        string timeString = null;
        if (seconds == 1)
        {
          timeString = seconds.ToString() + " second";
        }
        else
        {
          timeString = seconds.ToString() + " seconds";
        }


        //  If the Days, Hours or Minutes value is non-zero, build a Time String including a
        //  number of minutes entry.
        if ((days > 0) || (hours > 0) || (minutes > 0))
        {
          //  Build a Time String including a number of minutes entry.
          if (minutes == 1)
          {
            timeString = minutes.ToString() + " minute and " + timeString;
          }
          else
          {
            timeString = minutes.ToString() + " minutes and " + timeString;
          }
        }


        //  If the Days or Hours value is non-zero, build a Time String including a number
        //  of hours entry.
        if ((days > 0) || (hours > 0))
        {
          if (hours == 1)
          {
            //  Add the number of hours the process ran to the Time String.
            timeString = hours.ToString() + " hour " + timeString;
          }
          else
          {
            //  Add the number of hours the process ran to the Time String.
            timeString = hours.ToString() + " hours " + timeString;
          }
        }


        //  If the Days value is non-zero, build a Time String including a number
        //  of days entry.
        if (days > 0)
        {
          if (days == 1)
          {
            //  Add the number of hours the process ran to the Time String.
            timeString = days.ToString() + " day " + timeString;
          }
          else
          {
            //  Add the number of hours the process ran to the Time String.
            timeString = days.ToString() + " days " + timeString;
          }
        }


        //  Return the process run time string.
        return timeString;

      }
      catch
      {
        //  Return a NULL string to indicate taht this process failed.
        return null;

      }

    }   //  CalculateProcessRunTime


    /// <summary>
    /// Default the Maximum Number of Characters Per Line to 80 then calls the overload method that requires
    /// a Maximum Number of Characters Per Line Value to break the passed message onto lines (if it is
    /// longer than 80 characters).
    /// </summary>
    /// <param name="Message">
    /// The Message that is to be processed.
    /// </param>
    /// <returns>
    /// A three element array with the lines of the message.  Those lines that are not needed (i.e., there are
    /// not enough characters in the passed message to require the message to be broken onto that line) are
    /// set to empty strings "".
    /// </returns>
    public string[] CreateMessages(string Message)
    {

      string[] messages;

      //  Call the Overload Method requiring a Maximum Number of Characters Per Line Value defaulting the
      //  Maximum Number of Characters Per Line to 80.
      messages = CreateMessages(Message, 80);


      //  Return the line(s) of the message to the calling routine.
      return messages;

    }   //  CreateMessages


    /// <summary>
    /// Breaks the passes message onto lines (at the position of the Maximum Number of Characters Per Line
    /// Value) so that it will fit nicely into a Log File or in a Console Window.
    /// </summary>
    /// <param name="Message">
    /// The Message that is to be processed.
    /// </param>
    /// <param name="SpacesPerLine">
    /// The Maximum Number of Characters that should be included in each message line.
    /// </param>
    /// <returns>
    /// A three element array with the lines of the message.  Those lines that are not needed (i.e., there are
    /// not enough characters in the passed message to require the message to be broken onto that line) are
    /// set to empty strings "".
    /// </returns>
    public string[] CreateMessages(string Message, int SpacesPerLine)
    {
      try
      {
        //  Default the Found Character Indicator to "FALSE" and the Number of Spaces Counter to "0".
        bool foundCharacter = false;
        int leadingSpaceCounter = 0;

        //  If the Message is too long to fit on a single line, split it up.
        string message1 = null;
        string message2 = null;
        string message3 = null;
        string message4 = null;
        if (Message.Length > SpacesPerLine)
        {

          //  Determine if the Message has any leading spaces(i.e., it is setup to be indented).  If there are
          //  any spaces, prepend them to the beginning of the second line of the message also.
          while (!foundCharacter)
          {
            //  Grab the Character Corresponding to the location of the Leading Space Counter in the Message.
            string currentCharacter = Message.Substring(leadingSpaceCounter, 1);
            //  If the character that was retrieved is a space, count it as a leading space and check the
            //  next character in the Message.
            if (currentCharacter != " ")
            {
              //  Set the Found Character Indicator to "TRUE"
              foundCharacter = true;
            }
            else
            {
              //Increment the Leading Spaces Counter.
              leadingSpaceCounter++;
            }
          }

          //  Default the second line of the Message to be indented 2 spaces beyond the first line of the message.
          message2 = "  ";

          //  If the input Message does have leading spaces, prepend the same number of spaces to the second
          //  line of the Message.
          if (leadingSpaceCounter > 1)
          {
            //  Prepend the number of Leading Spaces found in the incoming Message to the beginning of the
            //  second line of the Message.
            for (int i = 0; i <= leadingSpaceCounter; i++)
            {
              message2 = message2 + " ";
            }
          }

          //  Attempt to find a space - just before the character position in the Message which the user specified
          //  as the maximum number of characters per line - on which to break the Message onto a second line.
          int position = 0;
          int testPosition = SpacesPerLine - 1;
          while ((position == 0) && (testPosition > 0))
          {
            //  Check to see if the character at the current Test Position is a space.  If so, set that location
            //  in the Message as the place to break the Message to second line.
            string testCharacter = Message.Substring(testPosition, 1);
            if (testCharacter == " ")
            {
              position = testPosition;
            }
            else
            {
              testPosition--;
            }
          }
          //  If a space was found, break the Message on that space.  Otherwise, just break the Message after the
          //  position in the message which the user specified as the maximum number of characters per line.
          if (position > (leadingSpaceCounter + 3))
          {
            //  Set the first line of the Message to the part of the Message before the found space.
            message1 = Message.Substring(0, (position + 1));
            //  Set the second line of the Message to the part of the Message after the found space.
            message2 = message2 + Message.Substring(position, (Message.Length - position));
          }
          else
          {
            //  Set the first line of the Message to be the maximum length the user specified.
            message1 = Message.Substring(0, (SpacesPerLine));
            //  Set the second line of the Message to the part of the Message following the first line.
            message2 = message2 + Message.Substring(position, (Message.Length - (SpacesPerLine)));
          }

        }
        else
        {

          //  Default the first line of the output Message to passed Message and set the second line to
          //  an empty string.
          message1 = Message;
          message2 = "";

        }


        //  If the second line of the Message is too long to fit on a single line, split it up.
        if (message2.Length > SpacesPerLine)
        {

          //  Default the third line of the Message to be indented 2 spaces beyond the first line of the message.
          message3 = "  ";

          //  If the input Message does have leading spaces, prepend the same number of spaces to the third
          //  line of the Message.
          if (leadingSpaceCounter > 1)
          {
            //  Prepend the number of Leading Spaces found in the incoming Message to the beginning of the
            //  third line of the Message.
            for (int i = 0; i <= leadingSpaceCounter; i++)
            {
              message3 = message3 + " ";
            }
          }

          //  Attempt to find a space - just before the character position in the Message which the user specified
          //  as the maximum number of characters per line - on which to break the Message onto a third line.
          int position = 0;
          int testPosition = SpacesPerLine - 1;
          while ((position == 0) && (testPosition > 0))
          {
            //  Check to see if the character at the current Test Position is a space.  If so, set that location
            //  in the Message as the place to break the Message to third line.
            string testCharacter = message2.Substring(testPosition, 1);
            if (testCharacter == " ")
            {
              position = testPosition;
            }
            else
            {
              testPosition--;
            }
          }
          //  If a space was found, break the Message on that space.  Otherwise, just break the Message after the
          //  80th character.
          if (position > (leadingSpaceCounter + 3))
          {
            //  Set the second line of the Message to the part of the Message after the found space.
            message3 = message3 + message2.Substring(position, (message2.Length - position));
            //  Set the first line of the Message to the part of the Message before the found space.
            message2 = message2.Substring(0, (position + 1));
          }
          else
          {
            //  Set the third line of the Message to the part of the Message following the second line.
            message3 = message3 + message2.Substring(SpacesPerLine - 1);
            //  Set the third line of the Message to be the maximum length the user specified.
            message2 = message2.Substring(0, (SpacesPerLine - 1));
          }

        }
        else
        {
          //  Default the third line of the Message to an empty string.
          message3 = "";
        }


        //  If the third line of the Message is too long to fit on a single line, split it up.
        if (message3.Length > SpacesPerLine)
        {

          //  Default the fourth line of the Message to be indented 2 spaces beyond the first line of the message.
          message4 = "  ";

          //  If the input Message does have leading spaces, prepend the same number of spaces to the fourth
          //  line of the Message.
          if (leadingSpaceCounter > 1)
          {
            //  Prepend the number of Leading Spaces found in the incoming Message to the beginning of the
            //  fourth line of the Message.
            for (int i = 0; i <= leadingSpaceCounter; i++)
            {
              message4 = message4 + " ";
            }
          }

          //  Attempt to find a space - just before the character position in the Message which the user specified
          //  as the maximum number of characters per line - on which to break the Message onto a fourth line.
          int position = 0;
          int testPosition = SpacesPerLine - 1;
          while ((position == 0) && (testPosition > 0))
          {
            //  Check to see if the character at the current Test Position is a space.  If so, set that location
            //  in the Message as the place to break the Message to a fourth line.
            string testCharacter = message3.Substring(testPosition, 1);
            if (testCharacter == " ")
            {
              position = testPosition;
            }
            else
            {
              testPosition--;
            }
          }
          //  If a space was found, break the Message on that space.  Otherwise, just break the Message after the
          //  80th character.
          if (position > (leadingSpaceCounter + 3))
          {
            //  Set the fourth line of the Message to the part of the Message after the found space.
            message4 = message4 + message3.Substring(position, (message3.Length - position));
            //  Set the third line of the Message to the part of the Message before the found space.
            message3 = message3.Substring(0, (position + 1));
          }
          else
          {
            //  Set the fourth line of the Message to the part of the Message following the third line.
            message4 = message4 + message3.Substring(SpacesPerLine - 1);
            //  Set the third line of the Message to be the maximum length the user specified.
            message3 = message3.Substring(0, (SpacesPerLine - 1));
          }

        }
        else
        {
          //  Default the fourth line of the Message to an empty string.
          message4 = "";
        }


        //  Write the Message(s) that were just created to an array that can be returned to the calling routine.
        string[] messages = new string[4];
        //  Write the first line of the Message to the array.
        messages[0] = message1;
        //  If there is a second line to the Message write it to the array.
        if (message2.Length > 0)
        {
          //  Write the line to the array.
          messages[1] = message2;
        }
        else
        {
          //  Write an empty string to the array.
          messages[1] = "";
        }
        //  If there is a third line to the Message write it to the array.
        if (message3.Length > 0)
        {
          //  Write the line to the array.
          messages[2] = message3;
        }
        else
        {
          //  Write an empty string to the array.
          messages[2] = "";
        }
        //  If there is a fourth line to the Message write it to the array.
        if (message4.Length > 0)
        {
          //  Write the line to the array.
          messages[3] = message4;
        }
        else
        {
          //  Write an empty string to the array.
          messages[3] = "";
        }

        //  Return the line(s) of the Message to the calling routine.
        return messages;
      }
      catch
      {
        //  Return a null array to indicate that this process failed.
        return null;

      }

    }   //  CreateMessages


    /// <summary>
    /// Breaks the passes message onto lines (at the position of the Maximum Number of Characters Per Line
    /// Value) so that it will fit nicely into a Log File or in a Console Window.
    /// </summary>
    /// <param name="Message">
    /// The Message that is to be processed.
    /// </param>
    /// <returns>
    /// A string collection with the lines of the message.
    /// </returns>
    public System.Collections.Specialized.StringCollection CreateMessagesCollection(string Message)
    {
      try
      {
        //  Call the overload of this method that requires a message break length value, defaulting the value to 100 characters.
        return CreateMessagesCollection(Message, 100);

      }
      catch
      {
        //  Return a NULL collection.
        return null;

      }

    }   //  CreateMessagesCollection()


    /// <summary>
    /// Breaks the passes message onto lines (at the position of the Maximum Number of Characters Per Line
    /// Value) so that it will fit nicely into a Log File or in a Console Window.
    /// </summary>
    /// <param name="Message">
    /// The Message that is to be processed.
    /// </param>
    /// <param name="MessageBreakLength">
    /// The Maximum Number of Characters that should be included in each message line.
    /// </param>
    /// <returns>
    /// A string collection with the lines of the message.
    /// </returns>
    public System.Collections.Specialized.StringCollection CreateMessagesCollection(string Message, int MessageBreakLength)
    {
      System.Collections.Specialized.StringCollection messagesCollection = null;

      try
      {
        //  If the message is long enough to be broken onto multiple lines, break it up.  Otherwise, return a String Collection with
        //  one item containing the entire passed message.
        messagesCollection = new System.Collections.Specialized.StringCollection();
        if (Message.Length > MessageBreakLength)
        {
          //  Determine if the Message has any leading spaces(i.e., it is setup to be indented).  If there are any spaces, prepend
          //  them to the beginning of the second line of the message also.
          bool foundCharacter = false;
          int leadingSpaceCounter = 0;
          while (!foundCharacter)
          {
            //  Grab the Character Corresponding to the location of the Leading Space Counter in the Message.
            string currentCharacter = Message.Substring(leadingSpaceCounter, 1);
            //  If the character that was retrieved is a space, count it as a leading space and check the next character in the Message.
            if (currentCharacter != " ")
            {
              //  Set the Found Character Indicator to "TRUE"
              foundCharacter = true;
            }
            else
            {
              //Increment the Leading Spaces Counter.
              leadingSpaceCounter++;
            }
          }

          //  Setup the Leading spaces to be included at the beginning of the second and each consecutive line.
          string leadingSpaces = "   ";
          for (int i = 0; i < leadingSpaceCounter; i++)
          {
            //  Add a Space to the leading spaces string.
            leadingSpaces = leadingSpaces + " ";
          }

          //  Break the message up onto lines.
          string remainingMessage = Message;
          while (remainingMessage.Length > MessageBreakLength)
          {
            //  Attempt to find a space - before the character position in the Remaining Message which the user specified as the
            //  maximum number of characters per line - on which to break the Message onto a second line.
            int position = 0;
            int testPosition = MessageBreakLength - 1;
            while ((position == 0) && (testPosition > 0))
            {
              //  Check to see if the character at the current Test Position is a space.  If so, set that location in the Message as the
              //  place to break the Message to another line.
              string testCharacter = remainingMessage.Substring(testPosition, 1);
              if (testCharacter == " ")
              {
                position = testPosition;
              }
              else
              {
                testPosition--;
              }
            }
            //  If a space was found, break the Message on that space.  Otherwise, just break the Message after the position in the
            //  message which the user specified as the maximum number of characters per line.
            string addMessage = null;
            if (position > (leadingSpaceCounter + 3))
            {
              //  Set the Message to be added to the messages collection to the part of the Message before the found space.
              addMessage = remainingMessage.Substring(0, (position + 1));
              addMessage = addMessage.TrimEnd();
              //  Remove the part of the message that is to be added to the collection from the Remaining Message.
              remainingMessage = leadingSpaces + remainingMessage.Substring(position + 1); // , (remainingMessage.Length - position + 1));
            }
            else
            {
              //  Set the Message to be added to the messages collection to be the maximum length the user specified.
              addMessage = remainingMessage.Substring(0, (MessageBreakLength));
              //  Remove the part of the message that is to be added to the collection from the Remaining Message.
              remainingMessage = leadingSpaces + remainingMessage.Substring(MessageBreakLength);
            }
            //  Add the Current Line to the Messages Collection.
            messagesCollection.Add(addMessage);
          }

          //  Add whatever is left of the message as a new line in the Messages Collection.
          if (remainingMessage.Length > 0)
          {
            messagesCollection.Add(remainingMessage);
          }

        }
        else
        {
          //  Return a String Collection containing one record with the entire passed message.
          messagesCollection.Add(Message);

        }


        //  Return the Messages Collection to the calling method.
        return messagesCollection;

      }
      catch
      {
        //  Return a NULL collection.
        return null;

      }

    }   //  CreateMessagesCollection()


    /// <summary>
    /// Calls the overload of this method that writes the passed message to the Console Window the application is running in.  Since no Message Break Length is specified
    /// in the call to this method a default value of 100 characters is passed to the overload method.
    /// </summary>
    /// <param name="Message">
    /// The message (text string) that is to be written to the Console Window.
    /// </param>
    public void SendMessage(string Message)
    {
      try
      {
        //  Call the Overload of this method that requires a Message Break Length specification, defaulting the break length to 100 characters.
        SendMessage(Message, 100);

      }
      catch
      {
        //  Exit this method.
        return;

      }

    }   // SendMessage()


    /// <summary>
    /// Calls the overload of this method that writes the passed message to the Console Window the application is running in.  Since no Log File Manager
    /// is specified in the call to this method a NULL Pointer is passed to the overload method.
    /// </summary>
    /// <param name="Message">
    /// The message (text string) that is to be written to the Console Window.
    /// </param>
    /// <param name="MessageBreakLength">
    /// The number of characters that shoudl be included on a line in the Console Window and the Process Log File.  Messages longer than the specified value will be
    /// broken up to be written on multiple lines.
    /// </param>
    public void SendMessage(string Message, int MessageBreakLength)
    {
      try
      {
        //  Call the Overload of this method that requires a pointer to a Log File Manager setting the pointer to a NULL Object.
        SendMessage(Message, MessageBreakLength, null);

      }
      catch
      {
        //  Exit this method.
        return;

      }

    }   // SendMessage()


    /// <summary>
    /// Writes the passed message to the Console Window the application is running in and adds the passed message it to the Process Log File for the application.
    /// </summary>
    /// <param name="Message">
    /// The message (text string) that is to be written to the Console Window and Process Log File.
    /// </param>
    /// <param name="MessageBreakLength">
    /// The number of characters that shoudl be included on a line in the Console Window and the Process Log File.  Messages longer than the specified value will be
    /// broken up to be written on multiple lines.
    /// </param>
    /// <param name="LogFileManager">
    /// The Log File Manager Object that contains the pointer to the Process Log File for the application.
    /// </param>
    public void SendMessage(string Message, int MessageBreakLength, PDX.BTS.DataMaintenance.MaintTools.LogFileManager LogFileManager)
    {
      System.Collections.Specialized.StringCollection messages = null;

      try
      {
        //  If the message is too long to fit in the process status list box, break it onto multiple lines.
        if (Message.Length > MessageBreakLength)
        {
          //  Break the passed message on to multiple lines since it is too long to fit on a single line in the Process Status List Box.
          messages = CreateMessagesCollection(Message, MessageBreakLength);

        }
        else
        {
          //  Create a Message Collection and add the current message to it.
          messages = new System.Collections.Specialized.StringCollection();
          messages.Add(Message);

        }


        //  Write the items in the Messages Collection to the Console Window and the Process Log File.
        foreach (string currentMessage in messages)
        {
          //  Write the Message to the Console Window.
          Console.WriteLine(currentMessage);

          //  If necessary, write the Message to the Process Log File.
          if (LogFileManager != null)
          {
            LogFileManager.WriteLine(currentMessage);
          }

        }


        //  Exit the method.
        return;

      }
      catch
      {
        //  Exit this method.
        return;

      }
      finally
      {
        //  If the Messages String Collection was instantiated, close it.
        if (messages != null)
        {
          messages = null;
        }

      }

    }   //  SendMessage


    /// <summary>
    /// Opens a URL (passed) and returns the response stream from the URL.
    /// </summary>
    /// <param name="URL">
    /// The URL of the Web Page that is to be opened.
    /// </param>
    /// <returns>
    /// The data stream from the Web Page if the open was successful.
    /// NULL - If the open failed.
    /// </returns>
    public System.IO.Stream OpenURL(string URL)
    {

      System.Net.HttpWebRequest  webRequest  = null;
      System.Net.HttpWebResponse webResponse = null;
      System.IO.Stream           webStream   = null;

      //  Make sure a valid URL was passed before attempting to open it.
      if (URL.Length == 0)
      {
        //  Return a NULL Stream to indicate that this process failed.
        return null;
      }

      try
      {
        //  Prepare the Web Request that will be used to Open the URL.
        webRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(URL);


        //  Execute the Web Request.
        webResponse = (System.Net.HttpWebResponse)webRequest.GetResponse();


        //  Prepare the Response Stream to be returned to the calling return.
        webStream = webResponse.GetResponseStream();


        //  Return the Response Stream to the calling routine.
        return webStream;

      }
      catch
      {
        //  Return a NULL Pointer to indicate that this process failed.
        return null;

      }

    }   //  OpenURL


    public string RetrieveParameterValue(string ParameterName, string ParameterTableName, string ParameterServerName, string ParameterDatabaseName, string ParameterUserName, string ParameterUserPassword, bool DecryptValue = true)
    {

      try
      {
        //  Build the Connection String that will be used to connect to the Parameter Database.
        string parameterConnectionString = "Data Source=" + ParameterServerName + ";"
                                         + "Initial Catalog=" + ParameterDatabaseName + ";"
                                         + "User ID=" + ParameterUserName + ";"
                                         + "Password=" + ParameterUserPassword + ";";


        //  Retrieve the value via the overload of this method that takes a Parameter Parameter Database Connection String as a parameter.
        return RetrieveParameterValue(ParameterName, ParameterTableName, parameterConnectionString, DecryptValue);

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this method failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The RetrieveParameterValue() Method failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return a NULL String to the calling Method to indicate that this Method failed.
        return null;

      }
      finally
      {

      }

    }   //  RetrieveParameterValue()


    public string RetrieveParameterValue(string ParameterName, string ParameterTableName, string ParameterServerName, string ParameterDatabaseName, bool DecryptValue = true)
    {

      try
      {
        //  Build the Connection String that will be used to connect to the Parameter Database.
        string parameterConnectionString = "Data Source=" + ParameterServerName + ";"
                                         + "Initial Catalog=" + ParameterDatabaseName + ";"
                                         + "Integrated Security=SSPI";


        //  Retrieve the value via the overload of this method that takes a Parameter Parameter Database Connection String as a parameter.
        return RetrieveParameterValue(ParameterName, ParameterTableName, parameterConnectionString, DecryptValue);

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this method failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The RetrieveParameterValue() Method failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return a NULL String to the calling Method to indicate that this Method failed.
        return null;

      }
      finally
      {

      }

    }   //  RetrieveParameterValue()


    public string RetrieveParameterValue(string ParameterName, string ParameterTableName, string ParameterConnectionString, bool DecryptValue = true)
    {
      PDX.BTS.DataMaintenance.MaintTools.ParameterManager parameterManager = null;

      try
      {
        //  Instantiate a Parameter Manager Object.
        parameterManager = new PDX.BTS.DataMaintenance.MaintTools.ParameterManager();
        parameterManager.Initialize(ParameterTableName, ParameterConnectionString);

        //  Make sure the Parameter Manager was initiailized successfully before moving on.
        if (!parameterManager.IsInitialized())
        {
          //  Let the User knwo that the Parameter manager Could not be initialized.
          if (ErrorMessage != null)
          {
            ErrorMessage("Could not initialize a Parameter Manager Object on Table - " + ParameterTableName + " using connection string - " + ParameterConnectionString + ".  The MaintTools.GeneralUtilities.RetrieveParameterValue Method() Failed!");
          }

          //  Return NULL to the calling method to indicate that this method failed.
          return null;

        }


        //  Retrieve the value via the overload of this method that takes a Parameter Manager Object as a parameter.
        return RetrieveParameterValue(ParameterName, parameterManager, DecryptValue);

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this method failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The RetrieveParameterValue() Method failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return a NULL String to the calling Method to indicate that this Method failed.
        return null;

      }
      finally
      {

      }

    }   //  RetrieveParameterValue()


    /// <summary>
    /// Retrieves a Parameter Value from the Parameter Table indicated by the Parameter Manager Object that was
    /// passed to the Method.  If the Value cannot be determined, a NULL String is returned.
    /// </summary>
    /// <param name="ParameterName">
    /// The Name of the Parameter that is t be looked up in the Parameter Table.
    /// </param>
    /// <param name="ParameterManager">
    /// A Parameter Manager Object that provides the connection information for the Parameter Table.
    /// </param>
    /// <returns>
    /// String - The value for the Parameter named in the "ParameterName" input parameter.  A NULL String is returned
    /// if the Parameter Value cannot be determined.
    /// </returns>
    public string RetrieveParameterValue(string ParameterName, PDX.BTS.DataMaintenance.MaintTools.ParameterManager ParameterManager, bool DecryptValue = true)
    {
      PDX.BTS.DataMaintenance.MaintTools.EncryptionManager encryptionManager = null;

      try
      {
        //  If the Parameter Manager Object is not initialized, return an empty string to indicate that the Parameter Value could not
        //  be determined.
        if (!ParameterManager.IsInitialized())
        {
          //  Return an empty string to indicate that the value could not determined.
          return "";

        }


        //  Determine if the Parameter exists in the Parameter Table.  If it does, return
        //  the value.  Otherwise, return an empty string to indicate that the Parameter
        //  Value could not be found in the Table.
        string parameterValue = null;
        if (ParameterManager.ParameterExists(ParameterName))
        {
          //  Retrieve the Paarameter Value from the Parameter Table.
          parameterValue = ParameterManager.ReadParameter(ParameterName);

          //  If the Parameter Value is encrypted, decrypt it.
          if (ParameterManager.ParameterEncrypted(ParameterName))
          {
            //  Instantiate an Encryption Manager Object.
            encryptionManager = new PDX.BTS.DataMaintenance.MaintTools.EncryptionManager();
            //  Decrypt the encrypted parameter value.
            parameterValue = encryptionManager.DecryptString(parameterValue);
          }
        }
        else
        {
          //  Set the return value to an empty string to indicate that the value could not
          //  be found in the Parameter Table.
          parameterValue = "";

        }


        //  Return the Parameter Value to the calling method.
        return parameterValue;

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this method failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The RetrieveParameterValue() Method failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return a NULL String to the calling Method to indicate that this Method failed.
        return null;

      }
      finally
      {
        //  If the Encryption Manager was instantiated and not released, release it before exiting.
        if (encryptionManager != null)
        {
          encryptionManager = null;
        }

      }

    }   //  RetrieveParameterValue()


    /// <summary>
    /// Determines the Install Path of ArcGIS Desktop on the current Machine.
    /// </summary>
    /// <returns>
    /// The install path of ArcGIS Desktop if it can be determined.
    /// An empty string if the Install Path could not be determined.
    /// A NULL String if the attempt to determine the install path of ArcGIS Desktop errored out.
    /// </returns>
    public string DetermineArcGISDesktopInstallPath()
    {
      try
      {
        //  Call the DetermineArcGISInstallPath Method specifying 'Desktop' as the product to search for.
        string installPath = DetermineArcGISInstallPath("Desktop");


        //  Return the Path to the calling method.
        return installPath;

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this method failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The DetermineArcGISDesktopInstallPath() Method failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return a NULL String to the calling Method to indicate that this Method failed.
        return null;

      }

    }   //  DetermineArcGISDesktopInstallPath()


    /// <summary>
    /// Determines the Install Path of the Specified ArcGIS Product.
    /// </summary>
    /// <param name="Product">
    /// The Name of the ArcGIS Product whose install path is to be determined.
    /// </param>
    /// <returns>
    /// The install path of the ArcGIS Product if it can be determined.
    /// An empty string if the Install Path could not be determined.
    /// A NULL String if the attempt to determine the install path of the product errored out.
    /// </returns>
    public string DetermineArcGISInstallPath(string Product)
    {
      System.Collections.Generic.IEnumerable<ESRI.ArcGIS.RuntimeInfo> installedRuntimes = null;

      try
      {
        //  Determine the Install Path of the specified product on the current machine.
        string installPath = null;
        installedRuntimes = ESRI.ArcGIS.RuntimeManager.InstalledRuntimes;
        foreach (ESRI.ArcGIS.RuntimeInfo currentRuntimeInfo in installedRuntimes)
        {
          if (System.String.Compare(currentRuntimeInfo.Product.ToString(), Product, System.StringComparison.CurrentCultureIgnoreCase) == 0)
          {
            installPath = currentRuntimeInfo.Path;
          }
        }

        //  Make sure the Install Path was determined successfully before moving on.
        if (System.String.IsNullOrEmpty(installPath))
        {
          //  Return an empty string to the calling method to indicate that the Path could not be determined.
          return "";

        }
        else
        {
          //  Return the retrieved path to the calling method.
          return installPath;

        }

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this method failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The DetermineArcGISInstallPath() Method failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return a NULL String to the calling Method to indicate that this Method failed.
        return null;

      }
      finally
      {
        //  If the Installed Runtimes Enumerator has been instantated, close it.
        if (installedRuntimes != null)
        {
          installedRuntimes = null;
        }

      }

    }   //  DetermineArcGISInstallPath()


    /// <summary>
    /// 
    /// </summary>
    /// <param name="Product">
    /// </param>
    /// <returns>
    /// </returns>
    public string DetermineInstalledArcGISDesktopVersion()
    {
      try
      {
        //  Request the installed ArcGIS Desktop Version and return it to the calling method.
        return DetermineInstalledArcGISVersion("Desktop");

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this method failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The DetermineInstalledArcGISDesktopVersion() Method failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return a NULL String to the calling Method to indicate that this Method failed.
        return null;

      }

    }   //  DetermineInstalledArcGISDesktopVersion()


    /// <summary>
    /// 
    /// </summary>
    /// <param name="Product">
    /// </param>
    /// <returns>
    /// </returns>
    public string DetermineInstalledArcGISVersion(string Product)
    {
      System.Collections.Generic.IEnumerable<ESRI.ArcGIS.RuntimeInfo> installedRuntimes = null;

      try
      {
        //  Determine the Installed Version of the specified product on the current machine.
        string installedVersion = null;
        installedRuntimes = ESRI.ArcGIS.RuntimeManager.InstalledRuntimes;
        foreach (ESRI.ArcGIS.RuntimeInfo currentRuntimeInfo in installedRuntimes)
        {
          if (System.String.Compare(currentRuntimeInfo.Product.ToString(), Product, System.StringComparison.CurrentCultureIgnoreCase) == 0)
          {
            installedVersion = currentRuntimeInfo.Version;
          }
        }

        //  Make sure the Installed Version was determined successfully before moving on.
        if (System.String.IsNullOrEmpty(installedVersion))
        {
          //  Return an empty string to the calling method to indicate that the Installed Version could not be determined.
          return "";

        }
        else
        {
          //  Return the retrieved version to the calling method.
          return installedVersion;

        }

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this method failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The DetermineInstalledArcGISVersion() Method failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return a NULL String to the calling Method to indicate that this Method failed.
        return null;

      }
      finally
      {
        ////  If the Installed Runtimes Enumerator has been instantated, close it.
        //if (installedRuntimes != null)
        //{
        //  installedRuntimes = null;
        //}

      }

    }   //  DetermineInstalledArcGISVersion()


    /// <summary>
    /// 
    /// </summary>
    /// <param name="DirectoryPath"></param>
    /// <param name="CreateIt"></param>
    /// <returns></returns>
    public bool ConfirmDirectoryPath(string DirectoryPath, bool CreateIt = false)
    {
      try
      {
        //  Make sure a Directory Path value was passed to this method.
        if (System.String.IsNullOrEmpty(DirectoryPath))
        {
          //  Let the User know that this method failed.
          if (ErrorMessage != null)
          {
            ErrorMessage("The passed Directory Path was not valid.  ConfirmDirectoryPath() is aborting!");
          }

          //  Return FALSE to the calling Method to indicate that this Method failed.
          return false;

        }


        //  Determine if the Directory Path Exists.
        if (System.IO.Directory.Exists(DirectoryPath))
        {
          //  The Directory exists so Return TRUE to the calling method.
          return true;
        }
        else
        {
          //  If the User has specified that the directory should be created, attempt to do so.  If not return FALSE to the calling method
          //  because the path does not exist.
          if (CreateIt)
          {
            //  Attempt to create the Directory.
            System.IO.Directory.CreateDirectory(DirectoryPath);

            //  Confirm that the directory was created successfully.
            if (System.IO.Directory.Exists(DirectoryPath))
            {
              //  The directory was created successfully so return TRUE to the calling method.
              return true;
            }
            else
            {
              //  The directory could not be created so return FALSE to the calling method.
              return false;
            }
          }
          else
          {
            //  The directory does not exist and the User does not want it created so return FALSE to the calling method.
            return false;
          }

        }

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this method failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The ConfirmDirectoryPath() Method failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling Method to indicate that this Method failed.
        return false;

      }

    }   //  ConfirmDirectoryPath()


    /// <summary>
    /// Writes a Project Status String to the Import Monitor Database that can be used by the process notification
    /// web page to notify the CGIS Data Manager of the final status of the application.
    /// </summary>
    /// <param name="StatusMessage">
    /// The message string that is to be written to the Database.
    /// </param>
    /// <param name="ProcessName">
    /// The Name of the process whose status is being written to the database.
    /// </param>
    /// <returns>
    /// TRUE - If the data was successfully written to the database.
    /// FALSE - If the data was NOT successfully written to the database.
    /// </returns>
    public bool WriteProjectStatusToDatabase(string StatusMessage, string ProcessName)
    {
      string                           deleteSQLStatement       = null;
      System.Data.SqlClient.SqlCommand deleteInformationCommand = null;
      int                              deleteRowsAffected       = -9999;
      string                           insertSQLStatement       = null;
      System.Data.SqlClient.SqlCommand insertInformationCommand = null;
      int                              insertRowsAffected       = -9999;

      try
      {
        //  Confirm that there is a valid connection to the Import Monitor database before
        //  attempting to update data in it.
        if (!ConnecttoImportMonitorDatabase())
        {
          //  Let the User know that the Database Connection could not be established.
          if (ErrorMessage != null)
          {
            ErrorMessage("A valid connection could not be established to the Import Monitor Database.  Aborting the WriteProjectStatusToDatabase() Method!");
          }

          //  Return FALSE to the calling Method to indicate that this Method failed.
          return false;

        }


        //  Build the SQL Statement that will be used to make sure that there is no information for this process in the
        //  ProcessNotifications Table.
        deleteSQLStatement = "DELETE "
                           + "FROM [dbo].[ProcessNotifications] "
                           + "WHERE [ProcessName] = '" + ProcessName + "'";


        //  Build the Delete SQL Command Object that will be used to delete any existing process information that
        //  exists in the table.
        deleteInformationCommand = new System.Data.SqlClient.SqlCommand();
        deleteInformationCommand.CommandText = deleteSQLStatement;
        deleteInformationCommand.Connection = _importMonitorDatabaseConnection;
        deleteInformationCommand.CommandTimeout = 30;


        //  Use the SQL Command Object that was just created to delete any existing status information for this
        //  process from the Process Notifications Table in the Import Monitor Database.
        deleteRowsAffected = deleteInformationCommand.ExecuteNonQuery();


        //  Make sure teh Status Message is not too long to fit into the Field.
        string loadStatusMessage = StatusMessage;
        if (loadStatusMessage.Length > 3995)
        {
          //  Drop the extra characters from the Status Message.
          loadStatusMessage = loadStatusMessage.Substring(0, 3945);

          //  Add a Message to the Status Message to let the user know that the message was truncated.
          loadStatusMessage = loadStatusMessage + @"\n\n" +  "Body Message truncated to fit in this email...";

        }


        //  Build the SQL Statement that will be used write the Process Information to the ProcessNotifications Table.
        string quotedStatusMessage = loadStatusMessage.Replace("'", "\"");
        insertSQLStatement = "INSERT INTO [dbo].[ProcessNotifications]"
                           + "   ([ProcessName], [NotificationData]) "
                           + "VALUES('" + ProcessName + "', '" + quotedStatusMessage + "')";


        //  Build the Insert SQL Command Object that will be used to insert the Status Message into the Table in the
        //  Import Monitor Database.
        insertInformationCommand = new System.Data.SqlClient.SqlCommand();
        insertInformationCommand.CommandText = insertSQLStatement;
        insertInformationCommand.Connection = _importMonitorDatabaseConnection;
        insertInformationCommand.CommandTimeout = 30;


        //  Use the Insert SQL Command Object that was just created to attempt to insert the Status Information into
        //  the Import Monitor Database.
        insertRowsAffected = insertInformationCommand.ExecuteNonQuery();


        //  If one row was affected by the command the process was successful so return TRUE to the calling method.  If
        //  not it faileed so return FALSE to the calling method.
        if (insertRowsAffected == 1)
        {
          //  The process was successful so return TRUE to the calling method.
          return true;

        }
        else
        {
          //  The process failed so return FALSE to the calling method.
          return false;

        }

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the user know that this method failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The WriteProjectStatusToDatabase() Method failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");;
        }

        //  Return FALSE to the calling method to indicate that this method failed.
        return false;

      }

    }   //  WriteProjectStatusToDatabase()


    /// <summary>
    /// Establishes or confirms a connection to the Import Monitor Database that is accessed by Methods in this Class.
    /// </summary>
    /// <returns>
    /// TRUE - If there is a valid connection to the database.
    /// FALSE - If a valid connection could not be established.
    /// </returns>
    private static bool ConnecttoImportMonitorDatabase()
    {

      try
      {
        //  If the connection is not currently open, create the connection and open it.  Otherwise, report
        //  that there is a valid connection to the database.
        if (_importMonitorDatabaseConnection != null)
        {

          //  If the connection is currently open, do nothing.  Otherwise, close the current connection, create
          //  a new one and open it.
          if (_importMonitorDatabaseConnection.State != System.Data.ConnectionState.Open)
          {
            //  If the Connection Object is instantiated but the state is not "OPEN", close the connection.
            _importMonitorDatabaseConnection.Close();
            //  Create and open a new connection to the database.
            _importMonitorDatabaseConnection = new System.Data.SqlClient.SqlConnection();
            _importMonitorDatabaseConnection.ConnectionString = _importMonitorDatabaseConnectionString;
            _importMonitorDatabaseConnection.Open();
          }

        }
        else
        {

          //  Establish a connection to the database.
          _importMonitorDatabaseConnection = new System.Data.SqlClient.SqlConnection();
          _importMonitorDatabaseConnection.ConnectionString = _importMonitorDatabaseConnectionString;
          _importMonitorDatabaseConnection.Open();

        }


        //  If there is a valid connection to the database, return a "TRUE" to the calling routine.  Otherwise,
        //  return a "FALSE" to indicate that the database is not available.
        if (_importMonitorDatabaseConnection.State == System.Data.ConnectionState.Open)
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
        //  Return "FALSE to the calling routine to indicate that this process failed.
        return false;

      }

    }   //  ConnecttoImportMonitorDatabase


    #region Class Disposal
    /// <summary>
    /// Final Class cleanup.
    /// </summary>
    ~GeneralUtilities()
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
        if (_importMonitorDatabaseConnection != null)
        {
          if (_importMonitorDatabaseConnection.State != System.Data.ConnectionState.Closed)
          {
            _importMonitorDatabaseConnection.Close();
          }
          _importMonitorDatabaseConnection.Dispose();
          _importMonitorDatabaseConnection = null;
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

    //  The connection parameters for the Import Monitor Database.
    static string                                      _monitorDatabaseServerName             = null;
    static string                                      _monitorDatabaseName                   = null;
    static string                                      _monitorTableName                      = null;
    private static System.Data.SqlClient.SqlConnection _importMonitorDatabaseConnection       = null;
    static string                                      _importMonitorDatabaseConnectionString = null;

    //  Indicator of whether or not the class has been disposed of.
    static bool                                        _isDisposed                            = false;

    //  Return Codes from Robocopy Jobs.
    public string RobocopyReturnCodeDescriptions(int ReturnCode)
    {
      //  Return the Description for the passed Return Code.
      switch (ReturnCode)
      {
        case 0:
          //  Return the description for the passed code.
          return "No Change";
        case 1:
          //  Return the description for the passed code.
          return "Successful Copy";
        case 2:
          //  Return the description for the passed code.
          return "Some Extra Files Detected - Check Output";
        case 3:
          //  Return the description for the passed code.
          return "Successful Copy Some XTra Files Found - Check Output";
        case 4:
          //  Return the description for the passed code.
          return "Mismatched Files Detected - Check Output";
        case 5:
          //  Return the description for the passed code.
          return "Successful Copy Mismatched Files Detected - Check Output";
        case 6:
          //  Return the description for the passed code.
          return "Some Mismatched and XTra Files Detected - Check Output";
        case 7:
          //  Return the description for the passed code.
          return "Successful Copy Some Mismatched and XTra Files Detected - Check Output";
        case 8:
          //  Return the description for the passed code.
          return "Failed Copy!";
        case 9:
          //  Return the description for the passed code.
          return "Successful Copy Some Failed Files Detected - Check Output";
        case 10:
          //  Return the description for the passed code.
          return "Failed Copy!";
        case 11:
          //  Return the description for the passed code.
          return "Failed Copy Some Mismatched Files Detected - Check Output";
        case 12:
          //  Return the description for the passed code.
          return "Failed Copy!";
        case 13:
          //  Return the description for the passed code.
          return "Succesful Copy Some Failed and Mismatched Files Detected - Check Output";
        case 14:
          //  Return the description for the passed code.
          return "Failed Copy!";
        case 15:
          //  Return the description for the passed code.
          return "Succesful Copy Some Failed, Mismatched and XTra Files Detected - Check Output";
        case 16:
          //  Return the description for the passed code.
          return "Fata Error!";
        default:
          //  The Code was not found so return an empty string.
          return "";
      }

    }   //  RobocopyReturnCodeDescriptions()

  }   //  Class:  GeneralUtilities

}   //  NameSpace:  PDX.BTS.DataMaintenance.MaintTools

cSharpUtils
===========

Utility Library for use with network-drive-utility front-end and backend

This was created as an external library for some small C# projects for UEMF.

###StringUtils
* **parseBool(string str)** - Parses a Boolean string using Boolean.parse and handles basic exceptions. Exceptions result in a FALSE value.
* **getToken(string str, int token, char delim)** - Splits a string with the passed delimiter and returns the token specified.
* **RegexBuild(string pattern)** - Accepts a string pattern with wildcards and constructs a valid regex pattern.

###DotNetVersionChecker
* **GetHighestDotNetVersion()** - Checks the Windows Registry and returns the highest version of .NET installed.
* **GetDotNetVersionsFromRegistry()** - Checks registry on this machine for .NET keys. Returns all versions in a List of strings.

###FileOperations
* **IsValidFilePath(string filepath)** - Verifies the format of a filepath string. Checks for bad characters.
* **readFile(string fullpath)** - Reads an entire file into a string.
* **isWritable(string fullpath)** - Verifies if the current user has write permissions to a folder.

###LogWriter
The LogWriter class will wirte to a log file. This log file is by default located in the user's AppData/Roaming folder.

* **getAppDataPath()** - Gets the file path in the current user's AppData/Roaming folder. The filename will be the current process name by default.
* **getProcessName()** - Gets the current process name.
* **getVerison()** - Gets the current assembly version.
* **getTimestamp()** - Gets the current timestamp in string format.
* **Write(string message, [bool print])** - Appends a new message to the end of the log file. The boolean is optional and can specify whether to perform the function or not.

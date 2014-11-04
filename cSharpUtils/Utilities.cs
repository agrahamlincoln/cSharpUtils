using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;

namespace cSharpUtils
{
    /// <summary>Has utility methods for basic string operations. Ex: Non-case sensitive string validation, Regex builders, etc.
    /// </summary>
    public class StringUtils
    {
        /// <summary>Parses a Boolean string using Boolean.Parse and handles basic exceptions
        /// </summary>
        /// <remarks>If exceptions are found, the boolean defaults to FALSE meaning it did not parse any boolean</remarks>
        /// <param name="str">string to parse for boolean value</param>
        /// <returns>Parsed value or false</returns>
        public static bool parseBool(string str)
        {
            bool parsed = false; //Return value - False by default

            try
            {
                parsed = Boolean.Parse(str);
            }
            catch (ArgumentException)
            {
                //string is null
                parsed = false;
            }
            catch (FormatException)
            {
                //string is invalid
                parsed = false;
            }

            return parsed;
        }

        /// <summary>Splits a string with the passed delimeter and returns the token specified
        /// </summary>
        /// <param name="str">string to split</param>
        /// <param name="token">index of token to return</param>
        /// <param name="delim">delimeter to split string with</param>
        /// <returns>the resulting token from the string</returns>
        public static string getToken(string str, int token, char delim)
        {
            string parsed;
            string[] strArray = str.Split(delim);
            if (token > strArray.Length || token < 0)
            {
                //Error, either not enough strings in the array or token is negative
                throw new IndexOutOfRangeException("Token is out of range. String[] Length: " + strArray.Length + " Token: " + token);
            }
            else
            {
                parsed = strArray[token];
            }
            return parsed;
        }

        /// <summary>Regex Builder
        /// </summary>
        /// <param name="pattern">String Pattern with wildcards</param>
        /// <returns name="regex_pattern">Regular Expression Formatted Pattern</returns>
        public static string RegexBuild(string pattern)
        {
            string regex_pattern;
            if (pattern == "" || pattern == null)
            {
                regex_pattern = "^.*$";
            }
            else
            {
                regex_pattern = "^" + Regex.Escape(pattern)
                    .Replace(@"\*", ".*")
                    .Replace(@"\?", ".")
                    + "$";
            }
            return regex_pattern;
        }
    }

    /// <summary>Used to verify .NET version via the windows Registry
    /// </summary>
    public class DotNetVersionChecker : IDisposable
    {
        #region IDisposable code

        // Flag: Has Dispose already been called? 
        bool disposed = false;

        // Public implementation of Dispose pattern callable by consumers. 
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern. 
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here. 
                //
            }

            // Free any unmanaged objects here. 
            //
            disposed = true;
        }

        #endregion


        /// <summary>Gets the Highest .NET version installed.
        /// </summary>
        /// <remarks>Returns 0 if none installed.</remarks>
        /// <returns>Highest .NET version installed</returns>
        public string GetHighestDotNetVersion()
        {
            string maxVersion = Get45or451FromRegistry();
            if (maxVersion.Equals("0"))
            {
                foreach (string currentVer in GetDotNetVersionsFromRegistry())
                {
                    maxVersion = GetGreatestVersion(maxVersion, currentVer);
                }
            }
            return maxVersion;
        }

        /// <summary>Compares two version numbers of the format 0.0.0.0 and returns the largest of the two
        /// </summary>
        /// <remarks>If the versions are equal, it will return ver1</remarks>
        /// <param name="ver1">Version number to compare (in the format 0.0.0.0)</param>
        /// <param name="ver2">Version number to compare (in the format 0.0.0.0)</param>
        /// <returns>Greatest version number of the two passed (in the format 0.0.0.0)</returns>
        private static string GetGreatestVersion(string ver1, string ver2)
        {
            string greatestVersion = ver1; //Greater version of the two passed

            //all versions are in the format 0.0.0.0
            int[] ver1Array = ver1.Split('.').Select(n => Convert.ToInt32(n)).ToArray();
            int[] ver2Array = ver2.Split('.').Select(n => Convert.ToInt32(n)).ToArray();

            if (ver1Array.Length != 4 || ver2Array.Length != 4)
            {
                throw new ArgumentException("Version numbers are not in the correct format. Please use #.#.#.# Ver1: " + ver1 + " Ver2: " + ver2);
            }
            else
            {
                for (int i = 0; i < ver1Array.Length; i++)
                {
                    //iterate through the strings and compare
                    if (ver1Array[i] > ver2Array[i])
                    {
                        greatestVersion = ver1;

                        //max found, no need to continue through the string
                        break;
                    }
                    //skip iterating through this string if its definitely not a max
                    else if (ver1Array[i] < ver2Array[i])
                        break;
                }
            }

            return greatestVersion;
        }


        /// <summary>Checks registry on this machine for .NET keys
        /// </summary>
        /// <returns>The Version Numbers associated with the .NET Keys</returns>
        private static List<string> GetDotNetVersionsFromRegistry()
        {
            List<string> versions = new List<string>();

            using (Microsoft.Win32.RegistryKey ndpKey =
                RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, "").
                OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\"))
            {
                foreach (string versionKeyName in ndpKey.GetSubKeyNames())
                {
                    if (versionKeyName.StartsWith("v"))
                    {
                        RegistryKey versionKey = ndpKey.OpenSubKey(versionKeyName);
                        string name = (string)versionKey.GetValue("Version", "");
                        string sp = versionKey.GetValue("SP", "").ToString();
                        string install = versionKey.GetValue("Install", "").ToString();
                        if (install == "")
                            versions.Add(name);
                        else
                            if (sp != "" && install == "1")
                                versions.Add(name);

                        if (name != "")
                            continue;
                        foreach (string subKeyName in versionKey.GetSubKeyNames())
                        {
                            RegistryKey subKey = versionKey.OpenSubKey(subKeyName);
                            name = (string)subKey.GetValue("Version", "");
                            if (name != "")
                                sp = subKey.GetValue("SP", "").ToString();
                            install = subKey.GetValue("Install", "").ToString();
                        }
                    }
                }
            }
            return versions;
        }


        private static string Get45or451FromRegistry()
        {
	        using (RegistryKey ndpKey = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, "").OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\")) {
		        int releaseKey = Convert.ToInt32(ndpKey.GetValue("Release"));
		        if (true) {
			        return CheckFor45DotVersion(releaseKey);
		        }
	        }
        }

        // Checking the version using >= will enable forward compatibility,  
        // however you should always compile your code on newer versions of 
        // the framework to ensure your app works the same. 
        private static string CheckFor45DotVersion(int releaseKey)
        {
	        if ((releaseKey >= 379893)) {
		        return "4.5.2";
	        }
	        if ((releaseKey >= 379675)) {
		        return "4.5.1";
	        }
	        if ((releaseKey >= 378389)) {
		        return "4.5";
	        }
	        // This line should never execute. A non-null release key should mean 
	        // that 4.5 or later is installed. 
	        return "0";
        }
    }

    /// <summary>Has utility methods for writing to files. Ex: writable permissions checking, filepath validation, etc.
    /// </summary>
    public class FileOperations
    {

        /// <summary>Verifies the format of a filepath string. Checks for bad characters
        /// </summary>
        /// <param name="testName">filepath to check</param>
        /// <returns>True if filepath is valid, False if filepath is invalid</returns>
        public static bool IsValidFilepath(string testName)
        {
            Regex containsABadCharacter = new Regex("[" + Regex.Escape(System.IO.Path.GetInvalidPathChars().ToString()) + "]");
            if (containsABadCharacter.IsMatch(testName)) { return false; };

            // Default return value
            return true;
        }

        /// <summary>Reads an entire file.
        /// </summary>
        /// <param name="fullPath">Full UNC Path of the file</param>
        /// <returns>string of the entire file</returns>
        /// <exception cref="System.OutOfMemoryException">Thrown when there is not enough memory to read the file.</exception>
        /// <exception cref="System.IO.IOException">Thrown when an error during IO occurs (file deleted etc.)</exception>
        public static string readFile(string fullPath)
        {
            string str;
            //Read json from file on network
            using (StreamReader file = new StreamReader(fullPath))
            {
                str = file.ReadToEnd();
            }
            return str;
        }

        /// <summary>Verifies if the current user has write permissions to a folder.
        /// </summary>
        /// <remarks>This code was found on stackoverflow.com http://stackoverflow.com/questions/1410127/c-sharp-test-if-user-has-write-access-to-a-folder </remarks>
        /// <param name="fullPath">Path of folder to check</param>
        /// <returns>Whether the user can write to the path or not</returns>
        public static bool IsWritable(string fullPath)
        {
            bool writable = false;
            try
            {
                //Instance variables
                DirectoryInfo di = new DirectoryInfo(fullPath);
                DirectorySecurity acl = di.GetAccessControl();
                AuthorizationRuleCollection rules = acl.GetAccessRules(true, true, typeof(NTAccount));

                //Current user
                WindowsIdentity currentUser = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(currentUser);

                //Iterate through rules & verify permissions
                foreach (AuthorizationRule rule in rules)
                {
                    FileSystemAccessRule fsAccessRule = rule as FileSystemAccessRule;
                    if (fsAccessRule == null)
                        continue;

                    if ((fsAccessRule.FileSystemRights & FileSystemRights.WriteData) > 0)
                    {
                        NTAccount ntAccount = rule.IdentityReference as NTAccount;
                        if (ntAccount == null)
                            continue;

                        //User has permissions
                        if (principal.IsInRole(ntAccount.Value))
                        {
                            writable = true;
                            continue;
                        }

                        //User has no permissions
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                writable = false;
            }

            return writable;
        }
    }
}

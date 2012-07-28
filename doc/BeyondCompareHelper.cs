using System;
using System.Diagnostics;

#region " ComparisonResult "

/// <summary>
/// Possible outcomes from the BeyondCompareHelper CompareFiles method
/// </summary>
public enum ComparisonResult
{

    /// <summary>
    /// Indicates a null or uninitialized value
    /// </summary>
    None = 0,
    /// <summary>
    /// The Quick Compare returned a Positive Match
    /// </summary>
    Match = 1,
    /// <summary>
    /// The Quick Compare detected small differences
    /// </summary>
    Similar = 2,
    /// <summary>
    /// The Quick Compare detected significant differences
    /// </summary>
    DoNotMatch = 3,
    /// <summary>
    /// The Quick Compare utility returned an error/unknown result
    /// </summary>
    ComparisonError = 3
}
#endregion

#region " BeyondCompareRules "

/// <summary>
/// Predefined standard rules that are available as a part of the default BeyondCompare configuration
/// </summary>
public sealed class BeyondCompareRules
{

    private BeyondCompareRules()
    {
    }

    /// <summary>
    /// A comparison rule set for C/C++/C# source files
    /// </summary>
    public const string CLanguageSource = "C/C++/C# Source";
    public const string Cobol = "COBOL";
    public const string CommaSeparatedValues = "Comma Separated Values";
    public const string DelphiSource = "Delphi Source";
    public const string DelphiForms = "Delphi Forms";
    public const string GeneralText = "General Text";
    public const string Html = "HTML";
    public const string Java = "JAVA";
    public const string Python = "Python";
    public const string RegistryDump = "Registry Dump";
    public const string Utf8Text = "UTF8 Text";
    public const string VisualBasic = "Visual Basic";
    public const string Xml = "XML";

    /// <summary>
    /// The default set of comparison rules
    /// </summary>
    public const string EverythingElse = "Everything Else";

}
#endregion

#region " BeyondCompareHelper "

/// <summary>
/// Contains static methods for working with the BeyondCompare command line utility
/// </summary>
public sealed class BeyondCompareHelper
{

    private BeyondCompareHelper()
    {
    }

    /// <summary>
    /// Initializes the <see cref="BeyondCompareHelper" /> Class
    /// </summary>
    static BeyondCompareHelper()
    {
        _ApplicationPath = @"C:\Program Files\Beyond Compare 2\BC.exe";
    }

    #region " ApplicationPath "

    private static string _ApplicationPath;

    /// <summary>
    /// Gets or sets the path to the BeyondCompare application
    /// </summary>
    /// <value>The path to the BeyondCompare application</value>
    public static string ApplicationPath
    {
        get { return _ApplicationPath; }
        set { _ApplicationPath = value; }
    }
    #endregion

    #region " LaunchViewer "

    /// <summary>
    /// Launches the BeyondCompare application to compare the specified files
    /// (Requires Beyond Compare)
    /// </summary>
    /// <param name="filepath1">The filepath</param>
    /// <param name="filepath2">The filepath</param>
    public static void LaunchViewer(string filepath1, string filepath2)
    {

        string arguments = String.Format("\"{0}\" \"{2}\"", filepath1, filepath2);

        ProcessStartInfo psi = new ProcessStartInfo(ApplicationPath, arguments);
        
        using (Process p = Process.Start(psi))
        {
        }
    }
    #endregion

    #region " CompareFiles [2 overloads] "

    /// <summary>
    /// Compares the specified files using the "Everything Else" rule using BeyondCompare's command line Quick Compare utility
    /// (Requires Beyond Compare)
    /// </summary>
    /// <param name="filepath1">The filepath</param>
    /// <param name="filepath2">The filepath</param>
    /// <returns>
    /// A <see cref="ComparisonResult"></see> indicating the result of the BeyondCompare quick comparison
    /// </returns>
    public static ComparisonResult CompareFiles(string filepath1, string filepath2)
    {
        return CompareFiles(filepath1, filepath2, BeyondCompareRules.EverythingElse);
    }

    /// <summary>
    /// Compares the specified files using the "Everything Else" rule using BeyondCompare's command line Quick Compare utility
    /// (Requires Beyond Compare)
    /// </summary>
    /// <param name="filepath1">The filepath</param>
    /// <param name="filepath2">The filepath</param>
    /// <param name="ruleName">One of the rules defined in <see cref="BeyondCompareRules"></see> or a custom rule name</param>
    /// <returns>
    /// A <see cref="ComparisonResult"></see> indicating the result of the BeyondCompare quick comparison
    /// </returns>
    public static ComparisonResult CompareFiles(string filepath1, string filepath2, string ruleName)
    {

        ComparisonResult result = ComparisonResult.None;

        string arguments = String.Format("/quickcompare /rules=\"{0}\" \"{1}\" \"{2}\"", ruleName, filepath1, filepath2);

        ProcessStartInfo psi = new ProcessStartInfo(ApplicationPath, arguments);
        psi.UseShellExecute = false;
        psi.RedirectStandardInput = true;
        psi.RedirectStandardOutput = true;

        using (Process p = Process.Start(psi))
        {
            p.StandardInput.WriteLine("EXIT [ErrorLevel]");
            p.WaitForExit();

            int exitCode = p.ExitCode;
            switch (exitCode)
            {
                case 0:
                    result = ComparisonResult.Match;
                    break;
                case 1:
                    result = ComparisonResult.Similar;
                    break;
                case 2:
                    result = ComparisonResult.DoNotMatch;
                    break;
                case 3:
                    result = ComparisonResult.ComparisonError;
                    break;
            }
        }
        return result;
    }

    #endregion

}
#endregion
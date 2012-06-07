using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace activeWindow
{
    public class ExampleC_2
    {
        private static void DisplayMatches(string text, string regularExpressionString)
        {

            Console.WriteLine("using the following regular expression: " +
              regularExpressionString);

            // create a MatchCollection object to store the words that
            // match the regular expression
            MatchCollection myMatchCollection =
              Regex.Matches(text, regularExpressionString);

            // use a foreach loop to iterate over the Match objects in
            // the MatchCollection object
            foreach (Match myMatch in myMatchCollection)
            {
                Console.WriteLine(myMatch);
            }

        }

        public static void Main2()
        {

            string text =
              "But, soft! what light through yonder window breaks?\n" +
               "It is the east, and Juliet is the sun.\n" +
               "Arise, fair sun, and kill the envious moon,\n" +
               "Who is already sick and pale with grief,\n" +
               "That thou her maid art far more fair than she";

            // match words that start with 's'
            Console.WriteLine("Matching words that start with 's'");
            DisplayMatches(text, @"\bs\S*");

            // match words that start with 's' and end with 'e'
            Console.WriteLine("Matching words that start with 's' and end with 'e'");
            DisplayMatches(text, @"\bs\S*e\b");

            // match words that contain two consecutive identical characters
            Console.WriteLine("Matching words that that contain two consecutive identical characters");
            DisplayMatches(text, @"\S*(.)\1\S*");

            // match words that contain 'u'
            Console.WriteLine("Matching words that contain 'u'");
            DisplayMatches(text, @"\S*u+\S*");

            // match words that contain the pattern 'ai'
            Console.WriteLine("Matching words that contain the pattern 'ai'");
            DisplayMatches(text, @"\S*(ai)\S*");

            // match words that contain the pattern 'ai' or 'ie'
            Console.WriteLine("Matching words that contain the pattern 'ai' or 'ie'");
            DisplayMatches(text, @"\S*(ai|ie)\S*");

            // match words that contain 'k' or 'f'
            Console.WriteLine("Matching words that contain 'k' or 'f'");
            DisplayMatches(text, @"\S*[kf]\S*");

            // match words that contain any letters in the range 'b' through 'd'
            Console.WriteLine("Matching words that contain any letters in the range 'b' through 'd'");
            DisplayMatches(text, @"\S*[b-d]\S*");

        }
        public void Run()
        {
            string s1 =
                "One,Two,Three Liberty Associates, Inc.";
            Regex theRegex = new Regex(" |, |,");
            StringBuilder sBuilder = new StringBuilder();
            int id = 1;

            foreach (string subString in theRegex.Split(s1))
            {
                sBuilder.AppendFormat(
                    "{0}: {1}\n", id++, subString);
            }
            Console.WriteLine("{0}", sBuilder);
        }

        public static void Main1()
        {

            // create a string containing area codes and phone numbers
            string text =
              "(800) 555-1211\n" +
              "(212) 555-1212\n" +
              "(506) 555-1213\n" +
              "(650) 555-1214\n" +
              "(888) 555-1215\n";

            // create a string containing a regular expression to
            // match an area code; this is a group of three numbers within
            // parentheses, e.g. (800)
            // this group is named "areaCodeGroup"
            string areaCodeRegExp = @"(?<areaCodeGroup>\(\d\d\d\))";

            // create a string containing a regular expression to
            // match a phone number; this is a group of seven numbers
            // with a hyphen after the first three numbers, e.g. 555-1212
            // this group is named "phoneGroup"
            string phoneRegExp = @"(?<phoneGroup>\d\d\d\-\d\d\d\d)";

            // create a MatchCollection object to store the matches
            MatchCollection myMatchCollection =
              Regex.Matches(text, areaCodeRegExp + " " + phoneRegExp);

            // use a foreach loop to iterate over the Match objects in
            // the MatchCollection object
            foreach (Match myMatch in myMatchCollection)
            {

                // display the "areaCodeGroup" group match directly
                Console.WriteLine("Area code = " + myMatch.Groups["areaCodeGroup"]);

                // display the "phoneGroup" group match directly
                Console.WriteLine("Phone = " + myMatch.Groups["phoneGroup"]);

                // use a foreach loop to iterate over the Group objects in
                // myMatch.Group
                foreach (Group myGroup in myMatch.Groups)
                {

                    // use a foreach loop to iterate over the Capture objects in
                    // myGroup.Captures
                    foreach (Capture myCapture in myGroup.Captures)
                    {
                        Console.WriteLine("myCapture.Value = " + myCapture.Value);
                    }

                }

            }

        }

    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auxiliary
{
    public class ArgumentParser
    {
        public static Dictionary<string, string> parse(string[] args)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            for (int i = 1; i < args.Length; i++)
            {
                string currentArg = args[i];
                if (currentArg.StartsWith("--") && currentArg.IndexOf("=") != -1)
                {
                    string key = currentArg.Substring(2, currentArg.IndexOf("=") - 2);
                    string value = currentArg.Substring(currentArg.IndexOf("=") + 1);
                    result.Add(key, value);
                }
                else if (args[i].StartsWith("-"))
                {
                    string option = currentArg.Substring(1);
                    result.Add(option, null);
                }
                else
                {
                    throw new FormatException("Bad Arguments");
                }
            }

            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.WebAppServices
{
    internal class cache
    {
        public static string set_recording_path = Guid.NewGuid().ToString();

        public static string set_default_all_name = Guid.NewGuid().ToString();
        public static string set_default_liver_folder_name = Guid.NewGuid().ToString();
        public static string set_default_data_folder_name = Guid.NewGuid().ToString();
        public static string set_default_file_name = Guid.NewGuid().ToString();

        public static string reinitialize = Guid.NewGuid().ToString();

    }
}

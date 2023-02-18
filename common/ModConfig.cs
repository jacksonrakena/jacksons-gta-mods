using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ModCommon
{
    public class ModConfig
    {
        private readonly string _path;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section,
            string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section,
                 string key, string def, StringBuilder retVal,
            int size, string filePath);
        
        /// <summary>
        ///     Mod file identifier should be something like 'ohko' or 'inputdetect'. NO file extensions.
        ///     ModConfig will generate the config file as {modFileIdentifier}_config.ini
        /// </summary>
        public ModConfig(string modFileIdentifier)
        {
            _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), modFileIdentifier + "_config.ini");
        }

        public ModConfigSection GetSection(string section)
        {
            return new ModConfigSection(this, section);
        }
        
        public bool WriteValue(string section, string key, string value)
        {
            try
            {
                WritePrivateProfileString(section, key, value, this._path);
                return true;
            } catch
            {
                return false;
            }
        }
        
        public string? ReadValue(string section, string key)
        {
            try
            {
                var temp = new StringBuilder(255);
                GetPrivateProfileString(section, key, "__DEFAULT__", temp,
                                                255, this._path);
                var str = temp.ToString();
                return str == "__DEFAULT__" ? null : temp.ToString();
            } catch
            {
                return null;
            }
        }
    }
}

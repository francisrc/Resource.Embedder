using System.Reflection;
using System.Windows;

namespace WpfCosturaAndRE
{
    public class ResourceLoader
    {
        #region Methods

        public static byte[] LoadResource(string path)
        {
            var asm = Assembly.GetExecutingAssembly();
            byte[] data;
            path = asm.GetName().Name + "." + path.Replace("/", ".").Replace("\\", ".");
            MessageBox.Show("LoadPath: " + path);
            using (var stream = asm.GetManifestResourceStream(path))
            {
                if (stream == null)
                    return null;

                data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
            }
            return data;
        }

        #endregion Methods
    }
}

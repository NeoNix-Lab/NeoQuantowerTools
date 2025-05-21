using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo.Quantower.Toolkit.Socket
{
    public static class CliExecutor
    {
        public static bool Runned { get; private set; } = false;

        //📝 TODO: [Use path Combine]
        //📝 TODO: [Scegli la shell]

        public static void Launch(string cliPath, string? arguments = null)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = cliPath,
                    Arguments = arguments ?? "",
                    UseShellExecute = true, // importante per lasciare viva la CLI senza output redirect
                    CreateNoWindow = false // lascia la finestra visibile se è una console app
                };

                Process.Start(startInfo); // no await, no wait

                Runned = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLI] Errore: {ex.Message}");
            }
        }
    }
}

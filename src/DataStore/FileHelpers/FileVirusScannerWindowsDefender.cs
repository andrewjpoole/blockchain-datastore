using System.Diagnostics;

namespace DataStore.FileHelpers
{
    public class FileVirusScannerWindowsDefender : IFileVirusScanner
    {
        public (bool FileContainsVirus, string Detail) ScanFile(string path)
        {
            /*cd "c:\Program Files\Windows Defender"
            λ mpcmdrun.exe - scan - scantype 3 - file "C:\temp\tempFiles\P1090487.JPG"
            Scan starting...
                Scan finished.
                Scanning C:\temp\tempFiles\P1090487.JPG found no threats.
                */
                
            var process = new Process();
            process.StartInfo = new ProcessStartInfo {
                FileName = @"c:\Program Files\Windows Defender\mpcmdrun.exe",
                Arguments = $"-scan -scantype 3 -file {path}",
                CreateNoWindow = true,
                RedirectStandardOutput = true};

            process.Start();
            process.WaitForExit(10000);

            var line1 = process.StandardOutput.ReadLine();
            var line2 = process.StandardOutput.ReadLine();
            var line3 = process.StandardOutput.ReadLine();

            return line3.Contains("found no threats") ? (false, "") : (true, line3);
        }
    }

}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.ComponentModel;
       
       
namespace youtuber.Media.FFmpeg
{
    public sealed class ProcessExe
    {
        private struct FFmpegProcess
        {
            public string strVideoFilename;
            public string strAudioFilename;
            public Process process;
        };

        private List<FFmpegProcess> listProcess;

        public ProcessExe()
        {
            listProcess = new List<FFmpegProcess>();
        }

        ~ProcessExe()
        {            
            // dispose processes --- may not be necessary
            foreach (FFmpegProcess proc in listProcess)
            {
                if (proc.process.HasExited == false)
                    proc.process.Kill();

                proc.process.Close();
            }
        }

        // public for test purpose
        public void StartProcess(string strVideoFilename, string strAudioFilename)
        {
            FFmpegProcess newProcess;

            newProcess.strVideoFilename = strVideoFilename;
            newProcess.strAudioFilename = strAudioFilename;

            ProcessStartInfo startInfo = new ProcessStartInfo("ffmpeg.exe");
            startInfo.WindowStyle = ProcessWindowStyle.Minimized;
            startInfo.Arguments = "-i " + strVideoFilename + " " + strAudioFilename;

            newProcess.process = Process.Start(startInfo);

            newProcess.process.WaitForExit();  // for test purpose

            listProcess.Add(newProcess);  // TODO:need to be sure newProcess will be copied and not referenced
            Debug.WriteLine("process added.");
            Debug.WriteLine(System.IO.File.Exists(strAudioFilename));
            Debug.WriteLine(newProcess.process.ExitCode);
        }


    }
}

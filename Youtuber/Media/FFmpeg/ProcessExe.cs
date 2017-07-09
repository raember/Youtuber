using System.Collections.Generic;
using System.Diagnostics;

namespace Youtuber.Media.FFmpeg {
    public sealed class ProcessExe {
        private readonly List<FFmpegProcess> listProcess;

        public ProcessExe(){
            listProcess = new List<FFmpegProcess>();
        }

        ~ProcessExe(){
            // dispose processes --- may not be necessary
            foreach (FFmpegProcess proc in listProcess) {
                if (proc.process.HasExited == false) proc.process.Kill();

                proc.process.Close();
            }
        }

        // public for test purpose
        public void StartProcess(string strVideoFilename, string strAudioFilename){
            FFmpegProcess newProcess;

            newProcess.strVideoFilename = strVideoFilename;
            newProcess.strAudioFilename = strAudioFilename;

            ProcessStartInfo startInfo = new ProcessStartInfo("ffmpeg.exe");
            startInfo.WindowStyle = ProcessWindowStyle.Minimized;
            startInfo.Arguments = "-i " + strVideoFilename + " " + strAudioFilename;

            newProcess.process = Process.Start(startInfo);

            newProcess.process.WaitForExit(); // for test purpose

            listProcess.Add(
                newProcess); // TODO:need to be sure newProcess will be copied and not referenced -- cant find articles explicitly stated about it, but probably ok...
            Debug.WriteLine(newProcess.process.ExitCode);
        }

        private struct FFmpegProcess {
            public string strVideoFilename;
            public string strAudioFilename;
            public Process process;
        }
    }
}
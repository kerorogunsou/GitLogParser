using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Globalization;
namespace GitOutput {
    class GitProcess {
        protected string gitExe;
        protected string gitRepoPath;
        protected Process proc;

        public GitProcess(string gitRepoPath, string gitExePath) {
            this.gitExe = gitExePath;
            this.gitRepoPath = gitRepoPath;
        }

        protected void StartProcess(string command) {
            ProcessStartInfo procInfo = new ProcessStartInfo {
                Arguments = command,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                FileName = gitExe,
                WorkingDirectory = gitRepoPath,
            };

            proc = Process.Start(procInfo);
        }
    }

    class GitTag : GitProcess {
        public GitTag(string gitRepo, string gitExe) : base(gitRepo, gitExe) { }

        public IEnumerable<string> ReadOneTag() {
            StartProcess("tag");
            string curLine = proc.StandardOutput.ReadLine();
            while (curLine != null) {
                yield return curLine;
                curLine = proc.StandardOutput.ReadLine();
            }
        }
    }

    class GitShow : GitProcess {
        public GitShow(string gitRepo, string gitExe) : base(gitRepo, gitExe) { }

        public string GetDateForTag(string tag) {
            StartProcess(string.Format("show --no-patch {0} --pretty=\"%ad\" --date=iso-strict", tag));
            string isoDateString = proc.StandardOutput.ReadLine();
            string rfcDateString = DateTime.Parse(isoDateString, null, DateTimeStyles.RoundtripKind).ToString("r");
            return string.Format("\t<tag name=\"{0}\" date=\"{1}\"/>", tag, rfcDateString);
        }
    }

    class Program {
        static void Main(string[] args) {
            if (args.Length < 2) {
                Console.WriteLine(@"expected arguments: path\to\git\repository path\to\git.exe");
                Console.ReadLine();
                return;
            }
            string repoPath = args[0];
            string gitExePath = args[1];

            GitTag tags = new GitTag(repoPath, gitExePath);
            using (var fileWriter = new StreamWriter("TagsAndDates.xml")) {
                fileWriter.WriteLine("<root>");
                foreach (string tag in tags.ReadOneTag()) {
                    fileWriter.WriteLine(new GitShow(repoPath, gitExePath).GetDateForTag(tag));
                }
                fileWriter.WriteLine("</root>");
            }
            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();

        }
    }
}

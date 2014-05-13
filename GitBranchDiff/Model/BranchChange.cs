using LibGit2Sharp;

namespace GitBranchDiff.Model
{
    internal class BranchChange
    {
        public BranchChange(string oldPath, string path, ChangeKind status)
        {
            OldPath = oldPath;
            Path = path;
            Status = status;
        }

        public string OldPath { get; private set; }

        public string Path { get; private set; }

        public ChangeKind Status { get; private set; }
    }
}
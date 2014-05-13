using System.Collections.Generic;
using System.Linq;

using LibGit2Sharp;

namespace GitBranchDiff.Model
{
    internal class BranchInfo
    {
        public BranchInfo(string currentBranchName, string referenceBranchName, string mostRecentMergeSourceSha,
            IEnumerable<TreeEntryChanges> changes)
        {
            CurrentBranchName = currentBranchName;
            ReferenceBranchName = referenceBranchName;
            MostRecentMergeSourceSha = mostRecentMergeSourceSha;
            Changes = changes == null ? null : changes.Select(c => new BranchChange(c.Path, c.OldPath, c.Status));
        }

        public string CurrentBranchName { get; private set; }

        public string ReferenceBranchName { get; private set; }

        public string MostRecentMergeSourceSha { get; private set; }

        public IEnumerable<BranchChange> Changes { get; private set; }
    }
}
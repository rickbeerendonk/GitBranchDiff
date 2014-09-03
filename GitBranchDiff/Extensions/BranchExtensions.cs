using System.Collections.Generic;
using System.Linq;

using LibGit2Sharp;

namespace GitBranchDiff.Extensions
{
    public static class BranchExtensions
    {
        public static Commit LatestMergeCommit(this Branch thisBranch, Branch otherBranch)
        {
            IEnumerable<Commit> otherBranchCommits = otherBranch.Commits;

            if (otherBranchCommits.Contains(thisBranch.Tip))
            {
                // Current branch is merged back to otherBranch.
                // So we find the last commit on otherBranch before this merge and all it's ancestors.
                var otherTipAfterMerge =
                    otherBranchCommits.SkipWhile(c => c.Parents.All(p => p != thisBranch.Tip)).FirstOrDefault();
                if (otherTipAfterMerge != null)
                {
                    var thisTipBeforeMerge = otherTipAfterMerge.Parents.First(p => p != thisBranch.Tip);
                    otherBranchCommits =
                        new List<Commit> { thisTipBeforeMerge }.Concat(thisTipBeforeMerge.GetAllAncestors());
                }
            }

            var branchCommits = thisBranch.Commits.Except(otherBranchCommits).ToList();
            var branchCommitsMergedFromOtherBranch =
                branchCommits.Where(c => c.IsMergeFrom(otherBranchCommits)).ToList();

            if (branchCommitsMergedFromOtherBranch.Any())
            {
                return branchCommitsMergedFromOtherBranch.First().GetSingleParentFrom(otherBranchCommits);
            }

            if (branchCommits.Any())
            {
                return branchCommits.Last().GetSingleParentFrom(otherBranchCommits);
            }

            return null;
        }
    }
}

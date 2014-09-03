using System.Linq;

using LibGit2Sharp;

namespace GitBranchDiff.Extensions
{
    public static class BranchCollectionExtensions
    {
        public static Branch Current(this BranchCollection branchCollection)
        {
            return branchCollection.Single(b => b.IsCurrentRepositoryHead);
        }
    }
}

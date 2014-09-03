using System.Collections.Generic;
using System.Linq;

using LibGit2Sharp;

namespace GitBranchDiff.Extensions
{
    public static class CommitExtensions
    {
        /// <summary>
        /// Returns true if commit is a merge from one of the commits in argument "from".
        /// </summary> 
        public static bool IsMergeFrom(this Commit commit, IEnumerable<Commit> from)
        {
            return commit.Parents.Count() > 1 && commit.Parents.Any(from.Contains);
        }

        public static Commit GetSingleParentFrom(this Commit commit, IEnumerable<Commit> from)
        {
            return commit.Parents.Single(from.Contains);
        }

        /// <summary>
        /// Returns all commits that are in ancestors to this commit.
        /// Alternative, the commit is the tip of a virtual "branch" and 
        /// the return contains all other commits in this virtual "branch".
        /// </summary>
        /// <param name="commit">The tip of a virtual "branch".</param>
        /// <returns>All ancestor commits.</returns>
        public static IEnumerable<Commit> GetAllAncestors(this Commit commit)
        {
            var result = new HashSet<Commit>();

            var queue = new Queue<Commit>(new[] { commit });
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                
                result.Add(node);

                foreach (var parent in node.Parents)
                {
                    if (!queue.Contains(parent) && !result.Contains(parent))
                    {
                        queue.Enqueue(parent);
                    }
                }
            }

            return result;
        }
    }
}

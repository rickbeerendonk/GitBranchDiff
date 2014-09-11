using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Beerendonk.Binding;

using LibGit2Sharp;

namespace GitBranchDiff.Model
{
    internal class RepositoryInfo : ObservableObject<RepositoryInfo>
    {
        private IList<string> brancheNames;

        private string repositoryPath;

        public RepositoryInfo(
            string repositoryPath,
            IEnumerable<string> brancheNames, 
            string currentBranchName, 
            string referenceBranchName, 
            string latestMergeCommitSha,
            IEnumerable<TreeEntryChanges> changes)
        {
            this.repositoryPath = repositoryPath;
            this.brancheNames = brancheNames.ToList();
            CurrentBranchName = currentBranchName;
            ReferenceBranchName = referenceBranchName;
            LatestMergeCommitSha = latestMergeCommitSha;
            Changes = changes == null ? null : changes.Select(c => new BranchChange(c.Path, c.OldPath, c.Status));
        }

        public IEnumerable<string> BrancheNames {
            get { return brancheNames; }
        }

        public int BrancheNamesIndex
        {
            get
            {
                return brancheNames.IndexOf(ReferenceBranchName);
            }
            set
            {
                if (ReferenceBranchName != brancheNames[value])
                {
                    ReferenceBranchName = brancheNames[value];
                    OnPropertyChanged(new PropertyChangedEventArgs(this.GetPropertyName(o => o.BrancheNamesIndex)));
                }
            }
        }

        private string _CurrentBranchName;
        public string CurrentBranchName
        {
            get
            {
                return _CurrentBranchName;
            }
            protected set
            {
                ChangeProperty(o => o.CurrentBranchName, ref _CurrentBranchName, value);
            }
        }

        private string _ReferenceBranchName;
        public string ReferenceBranchName
        {
            get
            {
                return _ReferenceBranchName;
            }
            set
            {
                ChangeProperty(o => o.ReferenceBranchName, ref _ReferenceBranchName, value);
            }
        }

        private string _LatestMergeCommitSha;
        public string LatestMergeCommitSha
        {
            get
            {
                return _LatestMergeCommitSha;
            }
            protected set
            {
                ChangeProperty(o => o.LatestMergeCommitSha, ref _LatestMergeCommitSha, value);
            }
        }

        private IEnumerable<BranchChange> _Changes;
        public IEnumerable<BranchChange> Changes
        {
            get
            {
                return _Changes;
            }
            protected set
            {
                ChangeProperty(o => o.Changes, ref _Changes, value);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using GitBranchDiff.Model;

using LibGit2Sharp;

using GitBranchDiff.Extensions;

namespace GitBranchDiff
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TempDirectory tempFolder = new TempDirectory();

        private string RepositoryPath
        {
            get { return Properties.Settings.Default.RepositoryPath; }
        }

        public MainWindow()
        {
            InitializeComponent();

            string referenceBranchName = Properties.Settings.Default.MasterBranchName;

            Update(referenceBranchName);
        }

        private void Update(string referenceBranchName)
        {
            using (var repository = new Repository(RepositoryPath))
            {
                Branch referenceBranch = repository.Branches[referenceBranchName];
                Branch currentBranch = repository.Branches.Current();

                Commit latestMergeCommit = currentBranch.LatestMergeCommit(referenceBranch);

                IEnumerable<TreeEntryChanges> changes = null;
                if (latestMergeCommit != null)
                {
                    changes = repository.Diff.Compare<TreeChanges>(latestMergeCommit.Tree, currentBranch.Tip.Tree)
                        .OrderBy(tc => tc.Path);
                }

                var repositoryInfo = new RepositoryInfo(
                    RepositoryPath,
                    repository.Branches.Select(b => b.Name).ToList(),
                    currentBranch.Name,
                    referenceBranch.Name,
                    latestMergeCommit == null ? null : latestMergeCommit.Sha,
                    changes);

                DataContext = repositoryInfo;
            }
        }

        private void ChangesBox_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (changesBox.SelectedItem == null)
            {
                return;
            }

            using (var repository = new Repository(RepositoryPath))
            {
                var latestMergeCommit = repository.Commits.FirstOrDefault(c => c.Sha == ((RepositoryInfo)DataContext).LatestMergeCommitSha);
                if (latestMergeCommit == null)
                {
                    return;
                }

                var change = (BranchChange)changesBox.SelectedItem;
                string path = change.Path;
                string oldPath = change.OldPath;
                string tmpFile = Path.Combine(tempFolder.Info.FullName, Path.GetFileName(path));
                var treeEntry = latestMergeCommit[oldPath];
                string newFile = Path.Combine(RepositoryPath, path);

                if (treeEntry == null)
                {
                    using (File.Create(tmpFile))
                    {
                    }
                }
                else
                {
                    var blob = (Blob)treeEntry.Target;
                    using (var fileStream = File.Create(tmpFile))
                    {
                        using (var blobStream = blob.GetContentStream())
                        {
                            blobStream.CopyTo(fileStream);
                        }
                    }
                }

                string cmd = Environment.ExpandEnvironmentVariables(Properties.Settings.Default.DiffTool);
                string cmdArguments = String.Format("\"{0}\" \"{1}\" /t", tmpFile, newFile);
                Process.Start(cmd, cmdArguments);

                /*
                            // Opens the tip of the branch. Means it will open the local file if and only if it hasn't uncommited changes.
                            var startInfo = new ProcessStartInfo("git.exe");
                            startInfo.WorkingDirectory = repositoryPath;
                            startInfo.Arguments = string.Format("difftool -y {0} {1} -- \"{2}\"", mostRecentMergeSource.Sha, repository.Branches[currentBranchName].Tip.Sha, treeEntryChanges.Path);
                            Process.Start(startInfo);
                */

            }
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Update((string)e.AddedItems[0]);
        }
    }
}

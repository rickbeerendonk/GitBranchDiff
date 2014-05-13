using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
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
        private string tempFolder;

        private string TempFolder
        {
            get
            {
                if (tempFolder == null)
                {
                    tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                    Directory.CreateDirectory(tempFolder);
                }

                return tempFolder;
            }
        }

        private string RepositoryPath
        {
            get { return Properties.Settings.Default.RepositoryPath; }
        }

        public MainWindow()
        {
            InitializeComponent();

            string masterBranchName = Properties.Settings.Default.MasterBranchName;

            using (var repository = new Repository(RepositoryPath))
            {
                Branch referenceBranch = repository.Branches[masterBranchName];
                Branch currentBranch = repository.Branches.Single(b => b.IsCurrentRepositoryHead);

                IEnumerable<Commit> referenceBranchCommits = referenceBranch.Commits;

                if (referenceBranchCommits.Contains(currentBranch.Tip))
                {
                    // Current branch is merged back to master.
                    // So we find the last commit on master before this merge and all it's ancestors.
                    var masterTipAfterMerge =
                        referenceBranchCommits.SkipWhile(c => c.Parents.All(p => p != currentBranch.Tip)).FirstOrDefault();
                    if (masterTipAfterMerge != null)
                    {
                        var masterTipBeforeMerge = masterTipAfterMerge.Parents.First(p => p != currentBranch.Tip);
                        referenceBranchCommits =
                            new List<Commit> { masterTipBeforeMerge }.Concat(masterTipBeforeMerge.GetAllAncestors());
                    }
                }

                var branchCommits = currentBranch.Commits.Except(referenceBranchCommits).ToList();
                var branchCommitsMergedFromMaster = branchCommits.Where(c => c.IsMergeFrom(referenceBranchCommits)).ToList();

                Commit mostRecentMergeSource = null;
                IEnumerable<TreeEntryChanges> treeChanges = null;
                if (branchCommitsMergedFromMaster.Any())
                {
                    mostRecentMergeSource = branchCommitsMergedFromMaster.First().GetParentFrom(referenceBranchCommits);

                    //changesBox.ItemsSource = treeChanges;
                }
                else
                {
                    if (branchCommits.Any())
                    {
                        mostRecentMergeSource = branchCommits.Last().GetParentFrom(referenceBranchCommits);
                    }
                }

                if (mostRecentMergeSource != null)
                {
                    treeChanges = repository.Diff.Compare<TreeChanges>(mostRecentMergeSource.Tree, currentBranch.Tip.Tree)
                        .OrderBy(tc => tc.Path);
                }

                DataContext = new BranchInfo(
                    currentBranch.Name,
                    referenceBranch.Name,
                    mostRecentMergeSource == null ? null : mostRecentMergeSource.Sha,
                    treeChanges);
            }
        }

        ~MainWindow()
        {
            var dirInfo = new DirectoryInfo(TempFolder);

            foreach (FileInfo file in dirInfo.GetFiles())
            {
                try
                {
                    file.Delete();
                }
                catch
                {
                }
            }

            dirInfo.Delete();
        }

        private void ChangesBox_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (changesBox.SelectedItem == null)
            {
                return;
            }

            using (var repository = new Repository(RepositoryPath))
            {
                var mostRecentMergeSource = repository.Commits.FirstOrDefault(c => c.Sha == ((BranchInfo)DataContext).MostRecentMergeSourceSha);
                if (mostRecentMergeSource == null)
                {
                    return;
                }

                var change = (BranchChange)changesBox.SelectedItem;
                string path = change.Path;
                string oldPath = change.OldPath;
                string tmpFile = Path.Combine(TempFolder, Path.GetFileName(path));
                var treeEntry = mostRecentMergeSource[oldPath];
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
    }
}

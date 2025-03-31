//=====================================
// Copyright (c) jasurhaydarovcode 2025
//=====================================

using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Ookii.Dialogs.Wpf;

namespace Oson_Git
{
    public partial class MainWindow : Window
    {
        private string detectedBranch = "main"; // Branch nomini saqlash

        public MainWindow()
        {
            InitializeComponent();
            txtBranchName.Text = detectedBranch; // UI da ko'rsatish
        }

        private void BrowseFolder(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == true)
            {
                txtFolderPath.Text = dialog.SelectedPath;
                CheckGitRepository(dialog.SelectedPath);
            }
        }

        private void CheckGitRepository(string folderPath)
        {
            if (Directory.Exists(Path.Combine(folderPath, ".git")))
            {
                lblStatus.Text = "✅ Git repo topildi!";

                // Git remote va branch nomini olish
                string remoteUrl = RunGitCommand(folderPath, "git remote get-url origin");
                detectedBranch = RunGitCommand(folderPath, "git branch --show-current");

                if (!string.IsNullOrWhiteSpace(remoteUrl))
                {
                    txtGitHubUrl.Text = remoteUrl;
                }

                // O‘zgarishlar bo‘lgan fayllarni ko‘rsatish
                string gitStatus = RunGitCommand(folderPath, "git status --porcelain");
                txtGitStatus.Text = string.IsNullOrWhiteSpace(gitStatus) ? "O'zgarishlar yo'q" : gitStatus;

                // UI da ko'rsatish
                txtBranchName.Text = string.IsNullOrWhiteSpace(detectedBranch) ? "main" : detectedBranch;
                lblStatus.Text = $"✅ Git repo topildi! Branch: {detectedBranch}";
            }
            else
            {
                lblStatus.Text = "❌ Bu papkada git repo topilmadi!";
                txtGitStatus.Text = "";
            }
        }


        private void StartGitProcess(object sender, RoutedEventArgs e)
        {
            string folderPath = txtFolderPath.Text;
            string gitHubUrl = txtGitHubUrl.Text;
            string commitMessage = string.IsNullOrWhiteSpace(txtCommitMessage.Text) ? "Auto commit" : txtCommitMessage.Text;

            if (string.IsNullOrWhiteSpace(folderPath))
            {
                lblStatus.Text = "📌 Papka tanlanmagan!";
                return;
            }

            try
            {
                if (!Directory.Exists(Path.Combine(folderPath, ".git")))
                {
                    lblStatus.Text = "📌 Git repo mavjud emas, yangi repo yaratilmoqda...";
                    RunGitCommand(folderPath, "git init");
                    RunGitCommand(folderPath, $"git branch -M {detectedBranch}");
                    if (!string.IsNullOrWhiteSpace(gitHubUrl))
                    {
                        RunGitCommand(folderPath, $"git remote add origin {gitHubUrl}");
                    }
                }

                RunGitCommand(folderPath, "git add .");
                RunGitCommand(folderPath, $"git commit -m \"{commitMessage}\"");
                RunGitCommand(folderPath, $"git push -u origin {detectedBranch}");

                lblStatus.Text = "✅ GitHub'ga muvaffaqiyatli yuklandi!";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "❌ Xatolik: " + ex.Message;
            }
        }

        private string RunGitCommand(string workingDirectory, string command)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
            {
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process())
            {
                process.StartInfo = processInfo;
                process.Start();

                string output = process.StandardOutput.ReadToEnd().Trim();
                string error = process.StandardError.ReadToEnd().Trim();

                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(error))
                {
                    lblStatus.Text = "❌ Git xatolik: " + error;
                }

                return output;
            }
        }
    }
}

// <copyright file="MainForm.cs" company="PublicDomainWeekly.com">
//     CC0 1.0 Universal (CC0 1.0) - Public Domain Dedication
//     https://creativecommons.org/publicdomain/zero/1.0/legalcode
// </copyright>

namespace FileDateReplay
{
    // Directives
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;

    /// <summary>
    /// Description of MainForm.
    /// </summary>
    public partial class MainForm : Form
    {
        /// <summary>
        /// The file path date dictionary.
        /// </summary>
        Dictionary<string, KeyValuePair<DateTime, DateTime>> filePathDateDictionary = new Dictionary<string, KeyValuePair<DateTime, DateTime>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:FileDateReplay.MainForm"/> class.
        /// </summary>
        public MainForm()
        {
            // The InitializeComponent() call is required for Windows Forms designer support.
            InitializeComponent();
        }

        /// <summary>
        /// Handles the options tool strip menu item drop down item clicked event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnOptionsToolStripMenuItemDropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            // Set tool strip menu item
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)e.ClickedItem;

            // Toggle checked
            toolStripMenuItem.Checked = !toolStripMenuItem.Checked;

            // Set topmost by check box
            this.TopMost = this.alwaysOnTopToolStripMenuItem.Checked;
        }

        /// <summary>
        /// Handles the collect from folder button click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnCollectFromFolderButtonClick(object sender, EventArgs e)
        {
            // Show folder browser dialog
            if (this.folderBrowserDialog.ShowDialog() == DialogResult.OK && this.folderBrowserDialog.SelectedPath.Length > 0)
            {
                // Set selected path as string
                string selectedPath = this.folderBrowserDialog.SelectedPath;

                // Reset file path date dictionary
                this.filePathDateDictionary.Clear();

                // Populate file path dictionary
                foreach (string filePath in Directory.GetFiles(selectedPath, "*", this.processSubfoldersToolStripMenuItem.Checked ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                {
                    // Set relative file path
                    string relativeFilePath = filePath.Remove(0, selectedPath.Length + (selectedPath[selectedPath.Length - 1] == Path.DirectorySeparatorChar ? 0 : 1));

                    // Set file info
                    FileInfo fileInfo = new FileInfo(filePath);

                    // Refresh info
                    fileInfo.Refresh();

                    // Add to dictionary
                    this.filePathDateDictionary.Add(relativeFilePath, new KeyValuePair<DateTime, DateTime>(fileInfo.CreationTimeUtc, fileInfo.LastWriteTimeUtc));
                }

                // Update collection name
                this.collectionNameLabel.Text = Path.GetFileName(selectedPath);

                // Update collected count
                this.collectedCountToolStripStatusLabel.Text = this.filePathDateDictionary.Count.ToString();
            }
        }

        /// <summary>
        /// Handles the replay on folder button click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnReplayOnFolderButtonClick(object sender, EventArgs e)
        {
            // Check there's something to work with
            if (this.filePathDateDictionary.Count == 0)
            {
                // Advise user
                MessageBox.Show("Please populate the file collection to replay.", "Empty collection", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                // Halt flow
                return;
            }

            // Declare replayed count
            int replayedCount = 0;

            // Show folder browser dialog
            if (this.folderBrowserDialog.ShowDialog() == DialogResult.OK && this.folderBrowserDialog.SelectedPath.Length > 0)
            {
                // Set selected path as string
                string selectedPath = this.folderBrowserDialog.SelectedPath;

                // Iterate files
                foreach (string filePath in Directory.GetFiles(selectedPath, "*", this.processSubfoldersToolStripMenuItem.Checked ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                {
                    // Set relative file path
                    string relativeFilePath = filePath.Remove(0, selectedPath.Length + (selectedPath[selectedPath.Length - 1] == Path.DirectorySeparatorChar ? 0 : 1));

                    // Check for a match
                    if (this.filePathDateDictionary.ContainsKey(relativeFilePath))
                    {
                        /* Replay dates */

                        // Creation
                        File.SetCreationTimeUtc(Path.Combine(selectedPath, relativeFilePath), this.filePathDateDictionary[relativeFilePath].Key);

                        // Last write
                        File.SetLastWriteTimeUtc(Path.Combine(selectedPath, relativeFilePath), this.filePathDateDictionary[relativeFilePath].Value);

                        // Raise replayed count
                        replayedCount++;
                    }
                }
            }

            // Update replayed count
            this.replayedCountToolStripStatusLabel.Text = this.filePathDateDictionary.Count.ToString();
        }

        /// <summary>
        /// Handles the open tool strip menu item click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnOpenToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Show open file dialog
            if (this.openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Clear dictionary
                    this.filePathDateDictionary.Clear();

                    // Read all lines 
                    foreach (var line in File.ReadAllLines(this.openFileDialog.FileName))
                    {
                        // Split line by tabs
                        string[] columns = line.Split(new char[] { '\t' });

                        // Add to dictionary
                        this.filePathDateDictionary.Add(columns[0], new KeyValuePair<DateTime, DateTime>(DateTime.Parse(columns[1], CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal), DateTime.Parse(columns[2], CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal)));

                        // Update collection name
                        this.collectionNameLabel.Text = Path.GetFileNameWithoutExtension(this.openFileDialog.FileName);

                        // Update collected count
                        this.collectedCountToolStripStatusLabel.Text = this.filePathDateDictionary.Count.ToString();
                    }
                }
                catch (Exception exception)
                {
                    // Inform user
                    MessageBox.Show($"Error when opening \"{Path.GetFileName(this.openFileDialog.FileName)}\":{Environment.NewLine}{exception.Message}", "Open file error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Handles the save tool strip menu item click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnSaveToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Check there's something to work with
            if (this.filePathDateDictionary.Count == 0)
            {
                // Advise user
                MessageBox.Show("Please populate the file collection to save.", "Empty collection", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                // Halt flow
                return;
            }

            // Set save dialog file name
            this.saveFileDialog.FileName = this.collectionNameLabel.Text;

            // Open save file dialog
            if (this.saveFileDialog.ShowDialog() == DialogResult.OK && this.saveFileDialog.FileName.Length > 0)
            {
                try
                {
                    // Declare lines string builder
                    var lineStringBuilder = new StringBuilder();

                    // Iterate dictionary
                    foreach (var item in this.filePathDateDictionary)
                    {
                        // Append line
                        lineStringBuilder.AppendLine($"{item.Key}{"\t"}{item.Value.Key}{"\t"}{item.Value.Value}");
                    }

                    // Save to file
                    File.WriteAllText(this.saveFileDialog.FileName, lineStringBuilder.ToString());
                }
                catch (Exception exception)
                {
                    // Inform user
                    MessageBox.Show($"Error when saving to \"{Path.GetFileName(this.saveFileDialog.FileName)}\":{Environment.NewLine}{exception.Message}", "Save file error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                // Inform user
                MessageBox.Show($"Saved {this.filePathDateDictionary.Count} items to \"{Path.GetFileName(this.saveFileDialog.FileName)}\"", "File saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Handles the new tool strip menu item click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnNewToolStripMenuItemClick(object sender, EventArgs e)
        {
            // TODO Add code    
        }

        /// <summary>
        /// Handles the weekly releases public domain weeklycom tool strip menu item click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnWeeklyReleasesPublicDomainWeeklycomToolStripMenuItemClick(object sender, EventArgs e)
        {
            // TODO Add code
        }

        /// <summary>
        /// Handles the original thread donation codercom tool strip menu item click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnOriginalThreadDonationCodercomToolStripMenuItemClick(object sender, EventArgs e)
        {
            // TODO Add code
        }

        /// <summary>
        /// Handles the source code githubcom tool strip menu item click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnSourceCodeGithubcomToolStripMenuItemClick(object sender, EventArgs e)
        {
            // TODO Add code
        }

        /// <summary>
        /// Handles the about tool strip menu item click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnAboutToolStripMenuItemClick(object sender, EventArgs e)
        {
            // TODO Add code
        }

        /// <summary>
        /// Handles the exit tool strip menu item click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnExitToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Close program
            this.Close();
        }
    }
}

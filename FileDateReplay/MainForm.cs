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
    using System.IO;
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
                    // Set file name
                    var fileName = filePath.Remove(0, selectedPath.Length + (selectedPath[selectedPath.Length - 1] == Path.DirectorySeparatorChar ? 0 : 1));

                    // Set file info
                    var fileInfo = new FileInfo(fileName);

                    // Add to dictionary
                    this.filePathDateDictionary.Add(fileName, new KeyValuePair<DateTime, DateTime>(fileInfo.CreationTimeUtc, fileInfo.LastWriteTimeUtc));
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
            // TODO Add code
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
        /// Handles the open tool strip menu item click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnOpenToolStripMenuItemClick(object sender, EventArgs e)
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

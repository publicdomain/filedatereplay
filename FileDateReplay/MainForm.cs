// <copyright file="MainForm.cs" company="PublicDomainWeekly.com">
//     CC0 1.0 Universal (CC0 1.0) - Public Domain Dedication
//     https://creativecommons.org/publicdomain/zero/1.0/legalcode
// </copyright>

namespace FileDateReplay
{
    // Directives
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;
    using PublicDomain;

    /// <summary>
    /// Description of MainForm.
    /// </summary>
    public partial class MainForm : Form
    {
        /// <summary>
        /// Gets or sets the associated icon.
        /// </summary>
        /// <value>The associated icon.</value>
        private Icon associatedIcon = null;

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

            /* Set icons */

            // Set associated icon from exe file
            this.associatedIcon = Icon.ExtractAssociatedIcon(typeof(MainForm).GetTypeInfo().Assembly.Location);

            // Set public domain weekly tool strip menu item image
            this.weeklyReleasesPublicDomainWeeklycomToolStripMenuItem.Image = this.associatedIcon.ToBitmap();
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
            // Reset selected path
            this.folderBrowserDialog.SelectedPath = string.Empty;

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

            // Reset selected path
            this.folderBrowserDialog.SelectedPath = string.Empty;

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

                    // Check for regex replace
                    if (this.regexPatternTextBox.TextLength > 0 && this.regexReplacementTextBox.TextLength > 0)
                    {
                        // Enforce regex replace
                        relativeFilePath = Regex.Replace(relativeFilePath, this.regexPatternTextBox.Text, this.regexReplacementTextBox.Text);
                    }

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

                // Update replayed count
                this.replayedCountToolStripStatusLabel.Text = replayedCount.ToString();
            }
        }

        /// <summary>
        /// Handles the open tool strip menu item click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnOpenToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Reset file name
            this.openFileDialog.FileName = string.Empty;

            // Show open file dialog
            if (this.openFileDialog.ShowDialog() == DialogResult.OK && this.openFileDialog.FileName.Length > 0)
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

            // Reset save file dialog
            this.saveFileDialog.FileName = string.Empty;

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
            //#
            MessageBox.Show(Regex.Replace("File.jpg", this.regexPatternTextBox.Text, this.regexReplacementTextBox.Text));

            // Reset file path date dictionary
            this.filePathDateDictionary.Clear();

            // Reset dialogs
            this.folderBrowserDialog.SelectedPath = string.Empty;
            this.openFileDialog.FileName = string.Empty;
            this.saveFileDialog.FileName = string.Empty;

            // Reset text boxes
            this.regexPatternTextBox.ResetText();
            this.regexReplacementTextBox.ResetText();

            // Reset labels
            this.collectionNameLabel.Text = "Open or collect...";
            this.collectedCountToolStripStatusLabel.Text = "0";
            this.replayedCountToolStripStatusLabel.Text = "0";
        }

        /// <summary>
        /// Handles the weekly releases public domain weeklycom tool strip menu item click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnWeeklyReleasesPublicDomainWeeklycomToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Open current website
            Process.Start("https://publicdomainweekly.com");
        }

        /// <summary>
        /// Handles the original thread donation codercom tool strip menu item click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnOriginalThreadDonationCodercomToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Open original thread @ DonationCoder
            Process.Start("https://www.donationcoder.com/forum/index.php?topic=51393.0");
        }

        /// <summary>
        /// Handles the source code githubcom tool strip menu item click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnSourceCodeGithubcomToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Open GitHub repository
            Process.Start("https://github.com/publicdomain/filedatereplay");
        }

        /// <summary>
        /// Handles the about tool strip menu item click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnAboutToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Set license text
            var licenseText = $"CC0 1.0 Universal (CC0 1.0) - Public Domain Dedication{Environment.NewLine}" +
                $"https://creativecommons.org/publicdomain/zero/1.0/legalcode{Environment.NewLine}{Environment.NewLine}" +
                $"Libraries and icons have separate licenses.{Environment.NewLine}{Environment.NewLine}" +
                $"Replay Icon by Clker-Free-Vector-Images - Pixabay License{Environment.NewLine}" +
                $"https://pixabay.com/users/clker-free-vector-images-3736/{Environment.NewLine}{Environment.NewLine}" +
                $"Patreon icon used according to published brand guidelines{Environment.NewLine}" +
                $"https://www.patreon.com/brand{Environment.NewLine}{Environment.NewLine}" +
                $"GitHub mark icon used according to published logos and usage guidelines{Environment.NewLine}" +
                $"https://github.com/logos{Environment.NewLine}{Environment.NewLine}" +
                $"DonationCoder icon used with permission{Environment.NewLine}" +
                $"https://www.donationcoder.com/forum/index.php?topic=48718{Environment.NewLine}{Environment.NewLine}" +
                $"PublicDomain icon is based on the following source images:{Environment.NewLine}{Environment.NewLine}" +
                $"Bitcoin by GDJ - Pixabay License{Environment.NewLine}" +
                $"https://pixabay.com/vectors/bitcoin-digital-currency-4130319/{Environment.NewLine}{Environment.NewLine}" +
                $"Letter P by ArtsyBee - Pixabay License{Environment.NewLine}" +
                $"https://pixabay.com/illustrations/p-glamour-gold-lights-2790632/{Environment.NewLine}{Environment.NewLine}" +
                $"Letter D by ArtsyBee - Pixabay License{Environment.NewLine}" +
                $"https://pixabay.com/illustrations/d-glamour-gold-lights-2790573/{Environment.NewLine}{Environment.NewLine}";

            // Prepend sponsors
            licenseText = $"RELEASE SPONSORS:{Environment.NewLine}{Environment.NewLine}* Jesse Reichler{Environment.NewLine}{Environment.NewLine}=========={Environment.NewLine}{Environment.NewLine}" + licenseText;

            // Set title
            string programTitle = typeof(MainForm).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;

            // Set version for generating semantic version 
            Version version = typeof(MainForm).GetTypeInfo().Assembly.GetName().Version;

            // Set about form
            var aboutForm = new AboutForm(
                $"About {programTitle}",
                $"{programTitle} {version.Major}.{version.Minor}.{version.Build}",
                $"Made for: Lolipop Jones{Environment.NewLine}DonationCoder.com{Environment.NewLine}Day #204, Week #29 @ July 23, 2021",
                licenseText,
                this.Icon.ToBitmap())
            {
                // Set about form icon
                Icon = this.associatedIcon,

                // Set always on top
                TopMost = this.TopMost
            };

            // Show about form
            aboutForm.ShowDialog();
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

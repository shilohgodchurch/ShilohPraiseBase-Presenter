﻿/*
 *   PraiseBase Presenter
 *   The open source lyrics and image projection software for churches
 *
 *   http://praisebase.org
 *
 *   This program is free software; you can redistribute it and/or
 *   modify it under the terms of the GNU General Public License
 *   as published by the Free Software Foundation; either version 2
 *   of the License, or (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.
 *
 *   You should have received a copy of the GNU General Public License
 *   along with this program; if not, write to the Free Software
 *   Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 *
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using PraiseBase.Presenter.Forms;
using PraiseBase.Presenter.Manager;
using PraiseBase.Presenter.Model.Song;
using PraiseBase.Presenter.Persistence;
using PraiseBase.Presenter.Properties;
using PraiseBase.Presenter.Template;
using Timer = System.Windows.Forms.Timer;

namespace PraiseBase.Presenter.Editor
{
    public partial class SongEditor : LocalizableForm
    {
        #region Internal variables

        /// <summary>
        /// Initial directory of file open/save dialog
        /// </summary>
        private string _fileBoxInitialDir;

        /// <summary>
        /// Type filter index of file open dialog
        /// </summary>
        private int _fileOpenBoxFilterIndex;

        /// <summary>
        /// Type filter index of file save dialog
        /// </summary>
        private int _fileSaveBoxFilterIndex;

        /// <summary>
        /// Number of open child forms
        /// </summary>
        private int _childFormNumber;
        
        /// <summary>
        /// Settings instance holder
        /// </summary>
        private readonly Settings _settings;

        /// <summary>
        /// Image manager instance
        /// </summary>
        private readonly ImageManager _imgManager;

        #endregion

        #region Constants

        /// <summary>
        /// Specifies how long the status message is shown
        /// </summary>
        private const int StatusMessageDuration = 2000;

        #endregion

        #region Delegates

        /// <summary>
        /// Delegate to inform subscribers that a song has been saved
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void SongSave(object sender, SongSavedEventArgs e);

        /// <summary>
        /// Song saved event
        /// </summary>
        public event SongSave SongSaved;

        /// <summary>
        /// Delegate to inform subscribers that the data directory has been changed in the settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void DataDirChange(object sender, EventArgs e);

        /// <summary>
        /// Data directory changed event
        /// </summary>
        public event DataDirChange DataDirChanged;

        #endregion

        public SongEditor(Settings settings, ImageManager imgManager, String filename)
        {
            // Initialize internal variables
            _settings = settings;
            _imgManager = imgManager;
            _fileBoxInitialDir = _settings.DataDirectory + Path.DirectorySeparatorChar + _settings.SongDir;
            _fileOpenBoxFilterIndex = 0;
            _fileSaveBoxFilterIndex = 0;

            InitializeComponent();

            RegisterChild(this);

            // Open song if given as startup argument
            if (!String.IsNullOrEmpty(filename) && File.Exists(filename))
            {
                OpenSong(filename);
            }
        }

        void selectLanguageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetLanguage((CultureInfo)((ToolStripMenuItem)sender).Tag);

            foreach (ToolStripMenuItem i in spracheToolStripMenuItem.DropDownItems)
            {
                i.Checked = (Equals((CultureInfo)i.Tag, Thread.CurrentThread.CurrentUICulture));
            }
        }

        /// <summary>
        /// Event handler for creating a new file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowNewForm(object sender, EventArgs e)
        {
            SongTemplateMapper stm = new SongTemplateMapper(_settings);
            Song sng = stm.CreateNewSong();
            stm.ApplyFormattingFromSettings(sng);

            CreateSongEditorChildForm(sng, null, false);
        }

        /// <summary>
        /// Opens a new song file editor based on the given song object
        /// </summary>
        /// <param name="sng"></param>
        public void OpenNewSongObject(Song sng)
        {
            SongTemplateMapper stm = new SongTemplateMapper(_settings);
            stm.ApplyFormattingFromSettings(sng);

            CreateSongEditorChildForm(sng, null, true);
        }

        /// <summary>
        /// Event handler for opening an existing file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFile(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = _fileBoxInitialDir,
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false,
                Title = StringResources.OpenSong,
                Filter = GetOpenFileBoxFilter(),
                FilterIndex = _fileOpenBoxFilterIndex
            };

            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string fileName = openFileDialog.FileName;
                _fileBoxInitialDir = Path.GetDirectoryName(fileName);
                _fileOpenBoxFilterIndex = openFileDialog.FilterIndex;

                ISongFilePlugin plugin = null;
                var  plugins = GetOpenFilePlugins();
                if (_fileOpenBoxFilterIndex > 1 && _fileOpenBoxFilterIndex  <= plugins.Count + 1)
                {
                    plugin = plugins[_fileOpenBoxFilterIndex - 2];
                }

                OpenSong(fileName, plugin);
            }
        }

        /// <summary>
        /// Opens a new song in a song editor child window
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="plugin"></param>
        public void OpenSong(string fileName, ISongFilePlugin plugin = null)
        {
            for (int i = 0; i < MdiChildren.Count(); i++)
            {
                EditorChildMetaData md = (EditorChildMetaData)MdiChildren[i].Tag;
                if (!string.IsNullOrEmpty(md.Filename) && 
                    String.Compare(
                    Path.GetFullPath(md.Filename).TrimEnd('\\'),
                    Path.GetFullPath(fileName).TrimEnd('\\'),
                    StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    MdiChildren[i].Show();
                    MdiChildren[i].Focus();
                    return;
                }
            }

            try
            {
                if (plugin == null)
                {
                    plugin = SongFilePluginFactory.Create(fileName);
                }
                var sng = plugin.Load(fileName);

                SongTemplateMapper stm = new SongTemplateMapper(_settings);
                // Set default formattig if none exists
                if (sng.Formatting == null) {
                    stm.ApplyFormattingFromSettings(sng);
                }
                // Set default background if none is set
                foreach (var p in sng.Parts)
                {
                    foreach (var s in p.Slides)
                    {
                        if (s.Background == null)
                        {
                            s.Background = stm.GetDefaultBackground();
                        }
                    }
                }

                CreateSongEditorChildForm(sng, fileName, false);
            }
            catch (NotImplementedException)
            {
                MessageBox.Show(StringResources.SongFormatNotSupported, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception e)
            {
                MessageBox.Show(StringResources.SongFileHasErrors + @" (" + e.Message + @")!", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Creates a song editor child form with the given song and file name
        /// </summary>
        /// <param name="sng">Song</param>
        /// <param name="fileName">File name, may be null</param>
        /// <param name="forceSaveCheck">Force check for save file</param>
        /// <returns></returns>
        private void CreateSongEditorChildForm(Song sng, String fileName, bool forceSaveCheck)
        {
            int hashCode = forceSaveCheck ? 0 : sng.GetHashCode();
            SongEditorChild childForm = new SongEditorChild(_settings, _imgManager, sng)
            {
                Tag = new EditorChildMetaData(fileName, hashCode),
                MdiParent = this
            };
            childForm.FormClosing += childForm_FormClosing;
            childForm.Show();
            RegisterChild(childForm);
            
            // Se status
            SetStatus(string.Format(StringResources.LoadedSong, sng.Title));

            // Set window title if new song
            if (fileName == null)
            {
                childForm.SetWindowTitle(sng.Title + @" " + ++_childFormNumber);
            }
        }

        /// <summary>
        /// Save event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveChild(object sender, EventArgs e)
        {
            if (ActiveMdiChild != null)
            {
                SongEditorChild window = ((SongEditorChild)ActiveMdiChild);
                Song sng = window.Song;
                window.ValidateChildren();
                string fileName = Save(sng, ((EditorChildMetaData)window.Tag).Filename);
                if (fileName != null)
                {
                    int hashCode = sng.GetHashCode();
                    ((EditorChildMetaData)window.Tag).HashCode = hashCode;
                    ((EditorChildMetaData)window.Tag).Filename = fileName;
                }
            }
        }

        /// <summary>
        /// Save as event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveChildAs(object sender, EventArgs e)
        {
            if (ActiveMdiChild != null)
            {
                SongEditorChild window = ((SongEditorChild)ActiveMdiChild);
                Song sng = window.Song;
                window.ValidateChildren();
                string fileName = SaveAs(sng, null);
                if (fileName != null)
                {
                    int hashCode = sng.GetHashCode();
                    ((EditorChildMetaData)window.Tag).HashCode = hashCode;
                    ((EditorChildMetaData)window.Tag).Filename = fileName;
                }
            }
        }

        /// <summary>
        /// Save given song at the filename specified
        /// </summary>
        /// <param name="sng"></param>
        /// <param name="fileName"></param>
        /// <returns>Filename used or null</returns>
        private string Save(Song sng, String fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return SaveAs(sng, null);
            }
            try
            {
                SaveSong(sng, fileName);
                return fileName;
            }
            catch (NotImplementedException)
            {
                MessageBox.Show(StringResources.SongCannotBeSavedInThisFormat, StringResources.FormatNotSupported,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return SaveAs(sng, fileName);
            }
        }

        /// <summary>
        /// Save given song, asking for filename
        /// </summary>
        /// <param name="sng"></param>
        /// <param name="fileName"></param>
        /// <returns>Filename used or null</returns>
        private string SaveAs(Song sng, String fileName)
        {
            // Check is using default name
            if (sng.Title == _settings.SongDefaultName)
            {
                if (MessageBox.Show(string.Format(StringResources.DoesTheSongReallyHaveTheDefaultTitle, sng.Title), StringResources.SaveSongAs,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return null;
                }
            }

            try
            {
                return SaveSongAskForName(sng, fileName);
            }
            catch (NotImplementedException)
            {
                MessageBox.Show(StringResources.SongCannotBeSavedInThisFormat, StringResources.SongEditor,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return null;
        }

        /// <summary>
        /// Handles saving of changed data when closing a song window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void childForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SongEditorChild window = ((SongEditorChild)sender);

            String filename = ((EditorChildMetaData)window.Tag).Filename;
            Song sng = window.Song;

            int storedHashCode = ((EditorChildMetaData)window.Tag).HashCode;
            int songHashCode = sng.GetHashCode();
            if (storedHashCode != songHashCode)
            {
                DialogResult dlg = MessageBox.Show(
                    string.Format(StringResources.SaveChangesMadeToTheSong, sng.Title),
                    StringResources.SongEditor,
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (dlg == DialogResult.Yes)
                {
                    Save(sng, ((EditorChildMetaData)window.Tag).Filename);
                }
                else if (dlg == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
            else if (filename != null && !File.Exists(filename))
            {
                DialogResult dlg = MessageBox.Show(string.Format(StringResources.SaveChangesMadeToTheSong, sng.Title),
                    StringResources.SongEditor,
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (dlg == DialogResult.Yes)
                {
                    SaveAs(sng, ((EditorChildMetaData)window.Tag).Filename);
                }
                else if (dlg == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }
        
        /// <summary>
        /// Returns the filter string used in open file dialogs
        /// </summary>
        /// <returns></returns>
        private static string GetOpenFileBoxFilter()
        {
            String exts = String.Empty;
            String fltr = String.Empty;
            foreach (var t in GetOpenFilePlugins())
            {
                if (exts != String.Empty)
                {
                    exts += ";";
                }
                exts += "*" + t.GetFileExtension();
                if (fltr != string.Empty)
                {
                    fltr += "|";
                }
                fltr += t.GetFileTypeDescription() + " (*" + t.GetFileExtension() + ")|*" + t.GetFileExtension();
            }
            return "Alle Lieddateien (" + exts + ")|" + exts + "|" + fltr + "|Alle Dateien (*.*)|*.*";
        }

        /// <summary>
        /// Get song file plugins which can open files
        /// </summary>
        /// <returns></returns>
        private static List<ISongFilePlugin> GetOpenFilePlugins()
        {
            return SongFilePluginFactory.GetPlugins().Where(t => t.IsWritingSupported()).ToList();
        }

        /// <summary>
        /// Saves a song
        /// </summary>
        /// <param name="sng">Song to be saved</param>
        /// <param name="fileName">Target file name</param>
        private void SaveSong(Song sng, String fileName)
        {
            // Load plugin based on the song filename
            ISongFilePlugin plugin = SongFilePluginFactory.Create(fileName);

            // Save song using plugin
            plugin.Save(sng, fileName);

            // Set status
            SetStatus(String.Format(StringResources.SongSavedAs, fileName));

            // Inform others by firing a SongSaved event
            if (SongSaved != null)
            {
                SongSavedEventArgs p = new SongSavedEventArgs(sng, fileName);
                SongSaved(this, p);
            }
        }

        /// <summary>
        /// Saves a song by asking for a file name
        /// </summary>
        /// <param name="sng">Song to be saved</param>
        /// <param name="fileName">Existing filename, can be null</param>
        /// <returns>The choosen name, if the song has been saved, or null if the action has been cancelled</returns>
        private string SaveSongAskForName(Song sng, String fileName)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                InitialDirectory = fileName != null
                    ? Path.GetDirectoryName(fileName)
                    : _fileBoxInitialDir,
                CheckPathExists = true,
                FileName = sng.Title,
                Filter = GetSaveFileBoxFilter(),
                FilterIndex = _fileSaveBoxFilterIndex,
                AddExtension = true,
                Title = StringResources.SaveSongAs
            };
            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                // Load plugin based on selected filter index
                ISongFilePlugin plugin = CreateByTypeIndex(saveFileDialog.FilterIndex - 1);

                // Save song using plugin
                plugin.Save(sng, saveFileDialog.FileName);

                // Store selected filter index
                _fileSaveBoxFilterIndex = saveFileDialog.FilterIndex;

                // Set status
                SetStatus(string.Format(StringResources.SongSavedAs, saveFileDialog.FileName));

                // Inform others by firing a SongSaved event
                if (SongSaved != null)
                {
                    SongSavedEventArgs p = new SongSavedEventArgs(sng, saveFileDialog.FileName);
                    SongSaved(this, p);
                }

                // Return file name
                return saveFileDialog.FileName;
            }
            return null;
        }

        /// <summary>
        /// Returns the filter string used in save file dialogs
        /// </summary>
        /// <returns></returns>
        private static string GetSaveFileBoxFilter()
        {
            String fltr = String.Empty;
            foreach (ISongFilePlugin t in SongFilePluginFactory.GetWriterPlugins())
            {
                if (fltr != string.Empty)
                {
                    fltr += "|";
                }
                fltr += t.GetFileTypeDescription() + " (*" + t.GetFileExtension() + ")|*" + t.GetFileExtension();
            }
            return fltr;
        }

        /// <summary>
        /// Gets a song file plugin by index
        /// </summary>
        /// <param name="index"></param>
        /// <exception cref="NotImplementedException">Thrown if the selected plugin has no implementation</exception>
        /// <returns></returns>
        private static ISongFilePlugin CreateByTypeIndex(int index)
        {
            if (index >= 0 && index < SongFilePluginFactory.GetWriterPlugins().Count)
            {
                return SongFilePluginFactory.GetWriterPlugins().ToArray()[index];
            }
            throw new NotImplementedException();
        }

        private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void DoCopy(Control control)
        {
            if (control is ContainerControl)
                DoCopy(((ContainerControl)control).ActiveControl);
            else if (control.GetType() == typeof(TextBox))
                ((TextBox)control).Copy();
            else if (control.GetType() == typeof(RichTextBox))
                ((RichTextBox)control).Copy();
        }

        private void DoCut(Control control)
        {
            if (control is ContainerControl)
                DoCut(((ContainerControl)control).ActiveControl);
            else if (control.GetType() == typeof(TextBox))
                ((TextBox)control).Cut();
            else if (control.GetType() == typeof(RichTextBox))
                ((RichTextBox)control).Cut();
        }

        private void DoPaste(Control control)
        {
            if (control is ContainerControl)
                DoPaste(((ContainerControl)control).ActiveControl);
            else if (control.GetType() == typeof(TextBox))
                ((TextBox)control).Paste();
            else if (control.GetType() == typeof(RichTextBox))
                ((RichTextBox)control).Paste();
        }

        private void CutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ActiveMdiChild != null)
            {
                DoCut(ActiveMdiChild.ActiveControl);
            }
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ActiveMdiChild != null)
            {
                DoCopy(ActiveMdiChild.ActiveControl);
            }
        }

        private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ActiveMdiChild != null)
            {
                DoPaste(ActiveMdiChild.ActiveControl);
            }
        }

        private void ToolBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStrip.Visible = toolBarToolStripMenuItem.Checked;
        }

        private void StatusBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statusStrip1.Visible = statusBarToolStripMenuItem.Checked;
        }

        private void CascadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }

        private void TileVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
        }

        private void TileHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void ArrangeIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.ArrangeIcons);
        }

        private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form childForm in MdiChildren)
            {
                childForm.Close();
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutDialog ab = new AboutDialog(_settings.UpdateCheckUrl, _settings.AuthorInfo);
            ab.ShowDialog(this);
        }

        private void webToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(_settings.Weburl);
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProgramSettingsDialog stWnd = new ProgramSettingsDialog(_settings);
            String dataDirectory = Settings.Default.DataDirectory;
            stWnd.ShowDialog(this);
            if (dataDirectory != Settings.Default.DataDirectory)
            {
                _fileBoxInitialDir = _settings.DataDirectory + Path.DirectorySeparatorChar + _settings.SongDir;

                // Fire event
                if (DataDirChanged != null)
                {
                    DataDirChanged(this, new EventArgs());
                }
            }
        }

        private void EditorWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (WindowState != FormWindowState.Maximized)
            {
                _settings.EditorWindowSize = Size;
            }
            _settings.EditorWindowState = WindowState;
        }

        /// <summary>
        /// Displays a message in the status bar for 2 seconds
        /// </summary>
        /// <param name="text"></param>
        private void SetStatus(string text)
        {
            toolStripStatusLabel1.Text = text;
            Timer statusTimer = new Timer
            {
                Interval = StatusMessageDuration
            };
            statusTimer.Tick += statusTimer_Tick;
            statusTimer.Start();
        }

        private void statusTimer_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = string.Empty;
            ((Timer)sender).Stop();
            ((Timer)sender).Dispose();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ActiveMdiChild != null && ActiveMdiChild.ActiveControl != null)
            {
                DoSelectAll(ActiveMdiChild.ActiveControl);
            }
        }

        private void DoSelectAll(Control control)
        {
            if (control is ContainerControl)
            {
                DoSelectAll(((ContainerControl)control).ActiveControl);
            }
            else if (control.GetType() == typeof(TextBox))
            {
                ((TextBox)control).SelectAll();
            }
            else if (control.GetType() == typeof(RichTextBox))
            {
                ((RichTextBox)control).SelectAll();
            }
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ActiveMdiChild != null && ActiveMdiChild.ActiveControl != null)
            {
                DoUndo(ActiveMdiChild.ActiveControl);
            }
        }

        private void DoUndo(Control control)
        {
            if (control is ContainerControl)
            {
                DoUndo(((ContainerControl)control).ActiveControl);
            }
            else if (control.GetType() == typeof(TextBox))
            {
                ((TextBox)control).Undo();
            }
            else if (control.GetType() == typeof(RichTextBox))
            {
                ((RichTextBox)control).Undo();
            }
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ActiveMdiChild != null && ActiveMdiChild.ActiveControl != null)
            {
                DoRedo(ActiveMdiChild.ActiveControl);
            }
        }

        private void DoRedo(Control control)
        {
            if (control is ContainerControl)
            {
                DoRedo(((ContainerControl)control).ActiveControl);
            }
            else if (control.GetType() == typeof(TextBox))
            {
                ((TextBox)control).ClearUndo();
            }
            else if (control.GetType() == typeof(RichTextBox))
            {
                ((RichTextBox)control).ClearUndo();
            }
        }

        private void liedSchliessenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ActiveMdiChild != null)
            {
                ActiveMdiChild.Close();
            }
        }

        private void allesSchliessenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MdiChildren.Any())
            {
                foreach (var c in MdiChildren)
                {
                    c.Close();
                }
            }
        }

        private void EditorWindow_Load(object sender, EventArgs e)
        {
            // Set size and window state
            WindowState = _settings.EditorWindowState;
            Size = _settings.EditorWindowSize;

            // Add languages to menu
            foreach (var l in Constants.AvailableLanguages)
            {
                ToolStripMenuItem selectLanguageToolStripMenuItem = new ToolStripMenuItem(l.DisplayName)
                {
                    Tag = l
                };
                selectLanguageToolStripMenuItem.Click += selectLanguageToolStripMenuItem_Click;
                if (l.Name == Thread.CurrentThread.CurrentUICulture.Name)
                {
                    selectLanguageToolStripMenuItem.Checked = true;
                }
                spracheToolStripMenuItem.DropDownItems.Add(selectLanguageToolStripMenuItem);
            }
        }

        private void datenverzeichnisAnzeigenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(_settings.DataDirectory);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            Process.Start(_settings.DataDirectory);
        }

        private void fehlerMeldenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(_settings.BugReportUrl);
        }

        private void EditorWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            GC.Collect();
        }

        private void contentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(_settings.HelpUrl);
        }

        private void toolStripButtonEnableTranslation_Click(object sender, EventArgs e)
        {
            toolStripMenuItem1_Click(sender, e);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (ActiveMdiChild != null)
            {
                var wnd = ((SongEditorChild)ActiveMdiChild);
                wnd.EnableTranslation(!wnd.Song.HasTranslation());
            }
        }

        private void toolStripButtonSwitchInputMode_Click(object sender, EventArgs e)
        {
            if (ActiveMdiChild != null)
            {
                var wnd = ((SongEditorChild)ActiveMdiChild);
                wnd.InputMode = wnd.InputMode == SongEditorChild.SongStructureDisplayMode.Structured 
                    ? SongEditorChild.SongStructureDisplayMode.Textual 
                    : SongEditorChild.SongStructureDisplayMode.Structured;
            }
        }
    }
}
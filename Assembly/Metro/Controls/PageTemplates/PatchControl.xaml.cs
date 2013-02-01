﻿using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Xml.Linq;
using ExtryzeDLL.Blam.ThirdGen;
using ExtryzeDLL.Blam.ThirdGen.Structures;
using ExtryzeDLL.Flexibility;
using ExtryzeDLL.IO;
using ExtryzeDLL.Patching;
using System;
using Assembly.Metro.Dialogs;

namespace Assembly.Metro.Controls.PageTemplates
{
    /// <summary>
    /// Interaction logic for PatchControl.xaml
    /// </summary>
    public partial class PatchControl
    {
// ReSharper disable ConvertToConstant.Local
// ReSharper disable FieldCanBeMadeReadOnly.Local
        private bool _doingWork = false;
// ReSharper restore FieldCanBeMadeReadOnly.Local
// ReSharper restore ConvertToConstant.Local

        public PatchControl()
        {
            InitializeComponent();
        }
        
        public bool Close()
        {
            return !_doingWork;
		}

		#region Patch Creation Functions
		// File Selectors
		private void btnCreatePatchUnModifiedMap_Click(object sender, RoutedEventArgs e)
		{
			var ofd = new OpenFileDialog
				          {
					          Title = "Assembly - Select a UnModified (Clean) Map file",
							  Filter = "Blam Cache File (*.map)|*.map"
				          };
			if (ofd.ShowDialog() == DialogResult.OK)
				txtCreatePatchUnModifiedMap.Text = ofd.FileName;
		}
		private void btnCreatePatchModifiedMap_Click(object sender, RoutedEventArgs e)
		{
			var ofd = new OpenFileDialog
			{
				Title = "Assembly - Select a Modified Map file",
				Filter = "Blam Cache File (*.map)|*.map"
			};
			if (ofd.ShowDialog() == DialogResult.OK)
				txtCreatePatchModifiedMap.Text = ofd.FileName;
		}
		private void btnCreatePatchOutputPatch_Click(object sender, RoutedEventArgs e)
		{
			var sfd = new SaveFileDialog
			{
				Title = "Assembly - Select where to save the patch file",
				Filter = "Assembly Patch File (*.asmp)|*.asmp"
			};
			if (sfd.ShowDialog() == DialogResult.OK)
				txtCreatePatchOutputPatch.Text = sfd.FileName;
		}
		private void btnCreatePatchPreviewImage_Click(object sender, RoutedEventArgs e)
		{
			var ofd = new OpenFileDialog
			{
				Title = "Assembly - Select the mod preview image",
				Filter = "JPEG Image|*.jpg|PNG Image|*.png"
			};
			if (ofd.ShowDialog() == DialogResult.OK)
				txtCreatePatchPreviewImage.Text = ofd.FileName;
		}

		// Meta Sorting
		private void cboxCreatePatchTargetGame_SelectionChanged(object sender, SelectionChangedEventArgs e) { PatchCreationMetaOptionsVisibility(); }
		private void cbCreatePatchHasCustomMeta_Modified(object sender, RoutedEventArgs e) { PatchCreationMetaOptionsVisibility(); }
		private void PatchCreationMetaOptionsVisibility()
		{
			// Check if meta should be shown or not
			if (cbCreatePatchHasCustomMeta == null) return;

			// Meta Grids Cleanup
			PatchCreationNoMetaSelected.Visibility = Visibility.Collapsed;
			PatchCreationExtras.Visibility = Visibility.Collapsed;
			
			// Check if custom meta is asked for
			if (cbCreatePatchHasCustomMeta.IsChecked == null || !(bool)cbCreatePatchHasCustomMeta.IsChecked)
			{
				PatchCreationNoMetaSelected.Visibility = Visibility.Visible;
				return;
			}

			// Set meta visibility depending on Targeted Game
			switch(cboxCreatePatchTargetGame.SelectedIndex)
			{
				case (int)TargetGame.Halo3:
				case (int)TargetGame.Halo3ODST:
					PrepHalo3();
					break;

				case (int)TargetGame.HaloReach: 
					PrepHaloReach();
					break;

				case (int)TargetGame.Halo4:
					PrepHalo4();
					break;

				default: PatchCreationNoMetaSelected.Visibility = Visibility.Visible;
					break;
			}
		}
		private void PrepHalo3()
		{
			// Un-Hide Extras grid
			PatchCreationExtras.Visibility = Visibility.Visible;

			// Hide/Show fields
			PatchCreationBlfOption0.Visibility =
				PatchCreationBlfOption3.Visibility = 
				PatchCreationBlfOption1.Visibility =
				PatchCreationBlfOption2.Visibility = Visibility.Visible;

			// Re-name fields
			lblCreatePatchTitleblf1.Text = "Modified blf_clip:";
				lblCreatePatchTitleblf1.Tag = "blf_clip";
			lblCreatePatchTitleblf2.Text = "Modified blf_film:";
				lblCreatePatchTitleblf1.Tag = "blf_film";
			lblCreatePatchTitleblf3.Text = "Modified blf_sm:";

			// Reset fields
			txtCreatePatchMapInfo.Text = "";
			txtCreatePatchblf0.Text = "";
			txtCreatePatchblf1.Text = "";
			txtCreatePatchblf2.Text = "";
			txtCreatePatchblf3.Text = "";
		}
		private void PrepHaloReach()
		{
			// Un-Hide Extras grid
			PatchCreationExtras.Visibility = Visibility.Visible;

			// Hide/Show fields
			PatchCreationBlfOption0.Visibility =
				PatchCreationBlfOption3.Visibility = Visibility.Visible;
			PatchCreationBlfOption1.Visibility =
				PatchCreationBlfOption2.Visibility = Visibility.Collapsed;

			// Re-name fields
			lblCreatePatchTitleblf3.Text = "Modified blf_sm:";
				

			// Reset fields
			txtCreatePatchMapInfo.Text = "";
			txtCreatePatchblf0.Text = "";
			txtCreatePatchblf1.Text = "";
			txtCreatePatchblf2.Text = "";
			txtCreatePatchblf3.Text = "";
		}
		private void PrepHalo4()
		{
			// Un-Hide Extras grid
			PatchCreationExtras.Visibility = Visibility.Visible;

			// Hide/Show fields
			PatchCreationBlfOption0.Visibility =
				PatchCreationBlfOption3.Visibility =
				PatchCreationBlfOption1.Visibility =
				PatchCreationBlfOption2.Visibility = Visibility.Visible;

			// Re-name fields
			lblCreatePatchTitleblf1.Text = "Modified blf_card:";
				lblCreatePatchTitleblf1.Tag = "blf_card";
			lblCreatePatchTitleblf2.Text = "Modified blf_lobby:";
				lblCreatePatchTitleblf1.Tag = "blf_lobby";
			lblCreatePatchTitleblf3.Text = "Modified blf_sm:";

			// Reset fields
			txtCreatePatchMapInfo.Text = "";
			txtCreatePatchblf0.Text = "";
			txtCreatePatchblf1.Text = "";
			txtCreatePatchblf2.Text = "";
			txtCreatePatchblf3.Text = "";
		}

		// Utilities
		private bool CheckAllMandatoryFields()
		{
			var error = false;

			if (txtCreatePatchUnModifiedMap.Text == null) return false;

			// Check Un-modified map exists
			if (String.IsNullOrEmpty(txtCreatePatchUnModifiedMap.Text) || !File.Exists(txtCreatePatchUnModifiedMap.Text))
				error = true;

			// Check Modified map exists
			if (String.IsNullOrEmpty(txtCreatePatchModifiedMap.Text) || !File.Exists(txtCreatePatchModifiedMap.Text))
				error = true;

			// Check Content Name is entered
			if (String.IsNullOrEmpty(txtCreatePatchContentName.Text))
				error = true;

			// Check Content Author is entered
			if (String.IsNullOrEmpty(txtCreatePatchContentAuthor.Text))
				error = true;

			// Check Content Desc is entered
			if (String.IsNullOrEmpty(txtCreatePatchContentDescription.Text))
				error = true;

			if (error)
				MetroMessageBox.Show("Unable to make patch", "Mandatory fields are missing, please make sure you've filled out all required fields.");

			return !error;
		}
		private bool CheckAllMetaFilesExists()
		{
			var error = false;

			if (cbCreatePatchHasCustomMeta.IsChecked == null || !(bool)cbCreatePatchHasCustomMeta.IsChecked || cboxCreatePatchTargetGame.SelectedIndex == 4) return false;

			// Check Map Info exists
			if (String.IsNullOrEmpty(txtCreatePatchMapInfo.Text) || !File.Exists(txtCreatePatchMapInfo.Text))
				error = true;

			// Check Blf Container 0
			if (PatchCreationBlfOption0.Visibility == Visibility.Visible && (String.IsNullOrEmpty(txtCreatePatchblf0.Text) || !File.Exists(txtCreatePatchblf0.Text)))
				error = true;
			// Check Blf Container 1
			if (PatchCreationBlfOption1.Visibility == Visibility.Visible && (String.IsNullOrEmpty(txtCreatePatchblf1.Text) || !File.Exists(txtCreatePatchblf1.Text)))
				error = true;
			// Check Blf Container 2
			if (PatchCreationBlfOption2.Visibility == Visibility.Visible && (String.IsNullOrEmpty(txtCreatePatchblf2.Text) || !File.Exists(txtCreatePatchblf2.Text)))
				error = true;
			// Check Blf Container 3
			if (PatchCreationBlfOption3.Visibility == Visibility.Visible && (String.IsNullOrEmpty(txtCreatePatchblf3.Text) || !File.Exists(txtCreatePatchblf3.Text)))
				error = true;

			if (error)
				MetroMessageBox.Show("Unable to make patch", "You are missing blf/mapinfo files.");
			
			return !error;
		}

	    // Patch Creation
		private void btnCreatePatch_Click(object sender, RoutedEventArgs e)
		{
#if !DEBUG
			try
			{
#endif
				// Check the user isn't completly retarded
				if (!CheckAllMandatoryFields())
					return;

				// Check the user isn't a skid
				if (!CheckAllMetaFilesExists())
					return;

				// Paths
				var cleanMapPath = txtCreatePatchUnModifiedMap.Text;
				var moddedMapPath = txtCreatePatchModifiedMap.Text;
				var outputPath = txtCreatePatchOutputPatch.Text;
				var previewImage = txtCreatePatchPreviewImage.Text;

				// Details
				var author = txtCreatePatchContentAuthor.Text;
				var desc = txtCreatePatchContentDescription.Text;
				var name = txtCreatePatchContentName.Text;

				// Make dat patch
				var patch = new Patch
				{
					Author = author,
					Description = desc,
					Name = name,
					Screenshot = String.IsNullOrEmpty(previewImage) ?
						null :
						File.ReadAllBytes(previewImage)
				};

				IReader originalReader = new EndianReader(File.OpenRead(cleanMapPath), Endian.BigEndian);
				IReader newReader = new EndianReader(File.OpenRead(moddedMapPath), Endian.BigEndian);

				var version = new ThirdGenVersionInfo(originalReader);
				var loader = new BuildInfoLoader(XDocument.Load(@"Formats\SupportedBuilds.xml"), @"Formats\");
				var buildInfo = loader.LoadBuild(version.BuildString);
				var originalFile = new ThirdGenCacheFile(originalReader, buildInfo, version.BuildString);
				var newFile = new ThirdGenCacheFile(newReader, buildInfo, version.BuildString);

				patch.MapInternalName = originalFile.Info.InternalName;

				if (cbCreatePatchHasCustomMeta.IsChecked != null && (bool)cbCreatePatchHasCustomMeta.IsChecked && cboxCreatePatchTargetGame.SelectedIndex != 4)
				{
					var targetGame = (TargetGame)cboxCreatePatchTargetGame.SelectedIndex;
					var mapInfo = File.ReadAllBytes(txtCreatePatchMapInfo.Text);
					FileInfo blfFileInfo;

					patch.CustomBlfContent = new BlfContent(mapInfo, targetGame);

					#region Blf Data
					if (PatchCreationBlfOption0.Visibility == Visibility.Visible)
					{
						blfFileInfo = new FileInfo(txtCreatePatchblf0.Text);
						patch.CustomBlfContent.BlfContainerEntries.Add(new BlfContainerEntry(blfFileInfo.Name, File.ReadAllBytes(blfFileInfo.FullName)));
					}
					if (PatchCreationBlfOption1.Visibility == Visibility.Visible)
					{
						blfFileInfo = new FileInfo(txtCreatePatchblf1.Text);
						patch.CustomBlfContent.BlfContainerEntries.Add(new BlfContainerEntry(blfFileInfo.Name, File.ReadAllBytes(blfFileInfo.FullName)));
					}
					if (PatchCreationBlfOption2.Visibility == Visibility.Visible)
					{
						blfFileInfo = new FileInfo(txtCreatePatchblf2.Text);
						patch.CustomBlfContent.BlfContainerEntries.Add(new BlfContainerEntry(blfFileInfo.Name, File.ReadAllBytes(blfFileInfo.FullName)));
					}
					if (PatchCreationBlfOption3.Visibility == Visibility.Visible)
					{
						blfFileInfo = new FileInfo(txtCreatePatchblf3.Text);
						patch.CustomBlfContent.BlfContainerEntries.Add(new BlfContainerEntry(blfFileInfo.Name, File.ReadAllBytes(blfFileInfo.FullName)));
					}
					#endregion
				}

				MetaComparer.CompareMeta(originalFile, originalReader, newFile, newReader, patch);
				LocaleComparer.CompareLocales(originalFile, originalReader, newFile, newReader, patch);

				originalReader.Close();
				newReader.Close();

				IWriter output = new EndianWriter(File.OpenWrite(outputPath), Endian.BigEndian);
				AssemblyPatchWriter.WritePatch(patch, output);
				output.Close();

				MetroMessageBox.Show("Patch Created!", "Your patch has been created in the designated location. Happy Sailing Modder!");
#if !DEBUG
			}
			catch (Exception ex)
			{
				MetroException.Show(ex);
			}
#endif
		}
		#endregion

		#region Patch Applying Functions
		#endregion

		#region Patch Convertion Functions
		#endregion
	}
}
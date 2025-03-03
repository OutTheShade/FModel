﻿using System.Diagnostics;
using System.Threading;
using AdonisUI.Controls;
using FModel.Extensions;
using FModel.Framework;
using FModel.Services;
using FModel.Settings;
using FModel.Views;
using FModel.Views.Resources.Controls;
using Newtonsoft.Json;

namespace FModel.ViewModels.Commands;

public class MenuCommand : ViewModelCommand<ApplicationViewModel>
{
    public MenuCommand(ApplicationViewModel contextViewModel) : base(contextViewModel)
    {
    }

    public override async void Execute(ApplicationViewModel contextViewModel, object parameter)
    {
        switch (parameter)
        {
            case "Directory_Selector":
                contextViewModel.AvoidEmptyGameDirectoryAndSetEGame(true);
                break;
            case "Directory_AES":
                Helper.OpenWindow<AdonisWindow>("AES Manager", () => new AesManager().Show());
                break;
            case "Directory_Backup":
                Helper.OpenWindow<AdonisWindow>("Backup Manager", () => new BackupManager(contextViewModel.CUE4Parse.Provider.GameName).Show());
                break;
            case "Directory_ArchivesInfo":
                contextViewModel.CUE4Parse.TabControl.AddTab("Archives Info");
                contextViewModel.CUE4Parse.TabControl.SelectedTab.Highlighter = AvalonExtensions.HighlighterSelector("json");
                contextViewModel.CUE4Parse.TabControl.SelectedTab.SetDocumentText(JsonConvert.SerializeObject(contextViewModel.CUE4Parse.GameDirectory.DirectoryFiles, Formatting.Indented), false);
                break;
            case "Views_AudioPlayer":
                Helper.OpenWindow<AdonisWindow>("Audio Player", () => new AudioPlayer().Show());
                break;
            case "Views_MapViewer":
                Helper.OpenWindow<AdonisWindow>("Map Viewer", () => new MapViewer().Show());
                break;
            case "Views_ImageMerger":
                Helper.OpenWindow<AdonisWindow>("Image Merger", () => new ImageMerger().Show());
                break;
            case "Settings":
                Helper.OpenWindow<AdonisWindow>("Settings", () => new SettingsView().Show());
                break;
            case "ModelSettings":
                UserSettings.Default.LastOpenedSettingTab = contextViewModel.CUE4Parse.Game == FGame.FortniteGame ? 2 : 1;
                Helper.OpenWindow<AdonisWindow>("Settings", () => new SettingsView().Show());
                break;
            case "Help_About":
                Helper.OpenWindow<AdonisWindow>("About", () => new About().Show());
                break;
            case "Help_Donate":
                Process.Start(new ProcessStartInfo { FileName = Constants.DONATE_LINK, UseShellExecute = true });
                break;
            case "Help_Changelog":
                UserSettings.Default.ShowChangelog = true;
                ApplicationService.ApiEndpointView.FModelApi.CheckForUpdates(UserSettings.Default.UpdateMode);
                break;
            case "Help_BugsReport":
                Process.Start(new ProcessStartInfo { FileName = Constants.ISSUE_LINK, UseShellExecute = true });
                break;
            case "Help_Discord":
                Process.Start(new ProcessStartInfo { FileName = Constants.DISCORD_LINK, UseShellExecute = true });
                break;
            case "ToolBox_Clear_Logs":
                FLogger.Logger.Text = string.Empty;
                break;
            case "ToolBox_Open_Output_Directory":
                Process.Start(new ProcessStartInfo { FileName = UserSettings.Default.OutputDirectory, UseShellExecute = true });
                break;
            case "ToolBox_Expand_All":
                await ApplicationService.ThreadWorkerView.Begin(cancellationToken =>
                {
                    foreach (var folder in contextViewModel.CUE4Parse.AssetsFolder.Folders)
                    {
                        LoopFolders(cancellationToken, folder, true);
                    }
                });
                break;
            case "ToolBox_Collapse_All":
                await ApplicationService.ThreadWorkerView.Begin(cancellationToken =>
                {
                    foreach (var folder in contextViewModel.CUE4Parse.AssetsFolder.Folders)
                    {
                        LoopFolders(cancellationToken, folder, false);
                    }
                });
                break;
            case TreeItem selectedFolder:
                selectedFolder.IsSelected = false;
                selectedFolder.IsSelected = true;
                break;
        }
    }

    private void LoopFolders(CancellationToken cancellationToken, TreeItem parent, bool isExpanded)
    {
        if (parent.IsExpanded != isExpanded)
        {
            parent.IsExpanded = isExpanded;
            Thread.Sleep(10);
        }

        cancellationToken.ThrowIfCancellationRequested();
        foreach (var f in parent.Folders) LoopFolders(cancellationToken, f, isExpanded);
    }
}
﻿using System.IO;
using Translations;
using XDM.Core.Lib.Common;
using XDM.Core.Lib.Common.MediaProcessor;
using XDM.Core.Lib.Util;

namespace XDM.Common.UI
{
    public class NewVideoDownloadDialogHelper
    {
        public static void ShowVideoDownloadDialog(IApp app, IAppUI appUi, INewVideoDownloadDialog window,
            string id, string name, long size, string? contentType)
        {
            window.SetFolderValues(CommonUtils.GetFolderValues());
            window.SeletedFolderIndex = Config.Instance.FolderSelectionMode == FolderSelectionMode.Auto ? 0 : 2;
            window.SelectedFileName = Helpers.SanitizeFileName(name);
            window.FileSize = Helpers.FormatSize(size);

            window.FileBrowsedEvent += CommonUtils.OnFileBrowsed;
            window.DropdownSelectionChangedEvent += CommonUtils.OnDropdownSelectionChanged;

            if (!string.IsNullOrEmpty(contentType))
            {
                var mime = contentType!.ToLowerInvariant();
                if (mime.StartsWith("audio"))
                {
                    if (!(mime.Contains("mpeg") || mime.Contains("mp3")))
                    {
                        window.ShowMp3Checkbox = true;
                    }
                }
            }

            window.DownloadClicked += (a, b) =>
            {
                if (string.IsNullOrEmpty(window.SelectedFileName))
                {
                    window.ShowMessageBox(TextResource.GetText("MSG_NO_FILE"));
                    return;
                }
                if (app.IsFFmpegRequiredForDownload(id) && !IsFFmpegInstalled())
                {
                    if (appUi.Confirm(window, TextResource.GetText("MSG_DOWNLOAD_FFMPEG")))
                    {
                        appUi.InstallLatestFFmpeg();
                    }
                    return;
                }
                var name = Helpers.SanitizeFileName(window.SelectedFileName);
                if (window.IsMp3CheckboxChecked)
                {
                    name = AddMp3Extension(name);
                }
                app.StartVideoDownload(id, name,
                    CommonUtils.SelectedFolderFromIndex(window.SeletedFolderIndex),
                    true,
                    window.Authentication,
                    window.Proxy ?? Config.Instance.Proxy,
                    window.EnableSpeedLimit ? window.SpeedLimit : 0,
                    null,
                    window.IsMp3CheckboxChecked);
                window.DisposeWindow();
            };

            window.DownloadLaterClicked += (a, b) =>
            {
                if (string.IsNullOrEmpty(window.SelectedFileName))
                {
                    window.ShowMessageBox(TextResource.GetText("MSG_NO_FILE"));
                    return;
                }
                var name = Helpers.SanitizeFileName(window.SelectedFileName);
                if (window.IsMp3CheckboxChecked)
                {
                    name = AddMp3Extension(name);
                }
                app.StartVideoDownload(id, name,
                    CommonUtils.SelectedFolderFromIndex(window.SeletedFolderIndex),
                    false,
                    window.Authentication,
                    window.Proxy ?? Config.Instance.Proxy,
                    window.EnableSpeedLimit ? window.SpeedLimit : 0,
                    b.QueueId,
                    window.IsMp3CheckboxChecked);
                window.DisposeWindow();
            };

            window.CancelClicked += (a, b) =>
            {
                window.DisposeWindow();
            };

            window.QueueSchedulerClicked += (s, e) =>
            {
                appUi.ShowQueueWindow(s);
            };

            window.ShowWindow();
        }

        private static string AddMp3Extension(string name)
        {
            return $"{Path.GetFileNameWithoutExtension(name)}.mp3";
        }

        private static bool IsFFmpegInstalled()
        {
            try
            {
                FFmpegMediaProcessor.FindFFmpegBinary();
            }
            catch (FileNotFoundException)
            {
                return false;
            }
            return true;
        }

    }
}

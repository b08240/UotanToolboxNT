﻿using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using UotanToolbox.Common;
using UotanToolbox.Common.PatchHelper;

namespace UotanToolbox.Features.Basicflash;

public partial class BasicflashView : UserControl
{
    static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);

    ISukiDialogManager dialogManager;
    public AvaloniaList<string> SimpleUnlock = ["oem unlock", "oem unlock-go", "flashing unlock", "flashing unlock_critical"];
    public AvaloniaList<string> Arch = ["aarch64", "armeabi", "X86-64", "X86"];

    public BasicflashView()
    {
        InitializeComponent();
        SimpleContent.ItemsSource = SimpleUnlock;
        ArchList.ItemsSource = Arch;
        SetDefaultMagisk();
    }

    public void SetDefaultMagisk()
    {
        var filepath = Path.Combine(Global.runpath, "APK", "Magisk-v27.0.apk");
        MagiskFile.Text = File.Exists(filepath) ? filepath : null;
    }

    public void patch_busy(bool is_busy)
    {
        if (is_busy)
        {
            BusyPatch.IsBusy = true;
            PanelPatch.IsEnabled = false;
        }
        else
        {
            BusyPatch.IsBusy = false;
            PanelPatch.IsEnabled = true;
        }
    }

    async void OpenUnlockFile(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            UnlockFile.Text = StringHelper.FilePath(files[0].Path.ToString());
        }
    }

    async void Unlock(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            var sukiViewModel = GlobalData.MainViewModelInstance;

            if (sukiViewModel.Status == GetTranslation("Home_Fastboot"))
            {
                Global.checkdevice = false;
                BusyUnlock.IsBusy = true;
                UnlockPanel.IsEnabled = false;

                if (!string.IsNullOrEmpty(UnlockFile.Text) && !string.IsNullOrEmpty(UnlockCode.Text))
                {
                    _ = dialogManager.CreateDialog()
    .WithTitle("Error")
    .OfType(NotificationType.Error)
    .WithContent(GetTranslation("Basicflash_NoBoth"))
    .Dismiss().ByClickingBackground()
    .TryShow();
                }
                else if (!string.IsNullOrEmpty(UnlockFile.Text) && string.IsNullOrEmpty(UnlockCode.Text))
                {
                    _ = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} flash unlock \"{UnlockFile.Text}\"");
                    var output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} oem unlock-go");

                    _ = output.Contains("OKAY")
                        ? dialogManager.CreateDialog()
.WithTitle("Success")
.OfType(NotificationType.Success)
.WithContent(GetTranslation("Basicflash_UnlockSucc"))
.Dismiss().ByClickingBackground()
.TryShow()
                        : dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Basicflash_UnlockFailed"))
.Dismiss().ByClickingBackground()
.TryShow();
                }
                else if (string.IsNullOrEmpty(UnlockFile.Text) && !string.IsNullOrEmpty(UnlockCode.Text))
                {
                    var output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} oem unlock {UnlockCode.Text}");

                    _ = output.Contains("OKAY")
                        ? dialogManager.CreateDialog()
.WithTitle("Success")
.OfType(NotificationType.Success)
.WithContent(GetTranslation("Basicflash_UnlockSucc"))
.Dismiss().ByClickingBackground()
.TryShow()
                        : dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Basicflash_UnlockFailed"))
.Dismiss().ByClickingBackground()
.TryShow();
                }
                else
                {
                    _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Basicflash_SelectUnlock"))
.Dismiss().ByClickingBackground()
.TryShow();
                }

                BusyUnlock.IsBusy = false;
                UnlockPanel.IsEnabled = true;
                Global.checkdevice = true;
            }
            else
            {
                _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_EnterFastboot"))
.Dismiss().ByClickingBackground()
.TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_NotConnected"))
.Dismiss().ByClickingBackground()
.TryShow();
        }
    }

    async void Lock(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            var sukiViewModel = GlobalData.MainViewModelInstance;

            if (sukiViewModel.Status == GetTranslation("Home_Fastboot"))
            {
                Global.checkdevice = false;
                BusyUnlock.IsBusy = true;
                UnlockPanel.IsEnabled = false;
                _ = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} oem lock-go");
                var output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} flashing lock");

                _ = output.Contains("OKAY")
                    ? dialogManager.CreateDialog()
.WithTitle("Success")
.OfType(NotificationType.Success)
.WithContent(GetTranslation("Basicflash_RelockSucc"))
.Dismiss().ByClickingBackground()
.TryShow()
                    : dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Basicflash_RelockFailed"))
.Dismiss().ByClickingBackground()
.TryShow();

                BusyUnlock.IsBusy = false;
                UnlockPanel.IsEnabled = true;
                Global.checkdevice = true;
            }
            else
            {
                _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_EnterFastboot"))
.Dismiss().ByClickingBackground()
.TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_NotConnected"))
.Dismiss().ByClickingBackground()
.TryShow();
        }
    }

    async void BaseUnlock(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            var sukiViewModel = GlobalData.MainViewModelInstance;

            if (sukiViewModel.Status == GetTranslation("Home_Fastboot"))
            {
                BusyBaseUnlock.IsBusy = true;
                BaseUnlockPanel.IsEnabled = false;
                var result = false;

                if (SimpleContent.SelectedItem != null)
                {
                    _ = dialogManager.CreateDialog()
    .WithTitle("Warn")
    .WithContent(GetTranslation("Basicflash_BasicUnlock"))
    .WithActionButton("Yes", _ => result = true, true)
    .WithActionButton("No", _ => result = false, true)
    .TryShow();

                    if (result == true)
                    {
                        _ = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} {SimpleContent.SelectedItem}");

                        _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Basicflash_CheckUnlock"))
.Dismiss().ByClickingBackground()
.TryShow();
                    }
                }
                else
                {
                    _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Basicflash_SelectCommand"))
.Dismiss().ByClickingBackground()
.TryShow();
                }

                BusyBaseUnlock.IsBusy = false;
                BaseUnlockPanel.IsEnabled = true;
            }
            else
            {
                _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_EnterFastboot"))
.Dismiss().ByClickingBackground()
.TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_NotConnected"))
.Dismiss().ByClickingBackground()
.TryShow();
        }
    }

    async void OpenRecFile(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            RecFile.Text = StringHelper.FilePath(files[0].Path.ToString());
        }
    }

    async Task FlashRec(string shell)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            var sukiViewModel = GlobalData.MainViewModelInstance;

            if (sukiViewModel.Status == GetTranslation("Home_Fastboot"))
            {
                Global.checkdevice = false;
                BusyFlash.IsBusy = true;
                FlashRecovery.IsEnabled = false;

                if (!string.IsNullOrEmpty(RecFile.Text))
                {
                    var output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} {shell} \"{RecFile.Text}\"");

                    if (!output.Contains("FAILED") && !output.Contains("error"))
                    {
                        var result = false;

                        _ = dialogManager.CreateDialog()
.WithTitle("Warn")
.WithContent(GetTranslation("Basicflash_RecoverySucc"))
.WithActionButton("Yes", _ => result = true, true)
.WithActionButton("No", _ => result = false, true)
.TryShow();

                        if (result == true)
                        {
                            output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} oem reboot-recovery");

                            if (output.Contains("unknown command"))
                            {
                                _ = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} flash misc {Global.runpath}/Image/misc.img");
                                _ = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} reboot");
                            }
                        }
                    }
                    else
                    {
                        _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Basicflash_RecoveryFailed"))
.Dismiss().ByClickingBackground()
.TryShow();
                    }
                }
                else
                {
                    _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Basicflash_SelectRecovery"))
.Dismiss().ByClickingBackground()
.TryShow();
                }

                BusyFlash.IsBusy = false;
                FlashRecovery.IsEnabled = true;
                Global.checkdevice = true;
            }
            else
            {
                _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_EnterFastboot"))
.Dismiss().ByClickingBackground()
.TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_NotConnected"))
.Dismiss().ByClickingBackground()
.TryShow();
        }
    }

    async void FlashToRec(object sender, RoutedEventArgs args) => await FlashRec("flash recovery");

    async void FlashToRecA(object sender, RoutedEventArgs args)
    {
        await FlashRec("flash recovery_a");
    }

    async void FlashToRecB(object sender, RoutedEventArgs args)
    {
        await FlashRec("flash recovery_b");
    }

    async void BootRec(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            var sukiViewModel = GlobalData.MainViewModelInstance;

            if (sukiViewModel.Status == GetTranslation("Home_Fastboot"))
            {
                Global.checkdevice = false;
                BusyFlash.IsBusy = true;
                FlashRecovery.IsEnabled = false;

                if (!string.IsNullOrEmpty(RecFile.Text))
                {
                    var output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} boot \"{RecFile.Text}\"");

                    _ = output.Contains("Finished")
                        ? dialogManager.CreateDialog()
.WithTitle("Success")
.OfType(NotificationType.Success)
.WithContent(GetTranslation("Basicflash_BootSucc"))
.Dismiss().ByClickingBackground()
.TryShow()
                        : dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Basicflash_BootFailed"))
.Dismiss().ByClickingBackground()
.TryShow();
                }
                else
                {
                    _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Basicflash_SelectRecovery"))
.Dismiss().ByClickingBackground()
.TryShow();
                }

                BusyFlash.IsBusy = false;
                FlashRecovery.IsEnabled = true;
                Global.checkdevice = true;
            }
            else
            {
                _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_EnterFastboot"))
.Dismiss().ByClickingBackground()
.TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_NotConnected"))
.Dismiss().ByClickingBackground()
.TryShow();
        }
    }

    async void FlashToBootA(object sender, RoutedEventArgs args) => await FlashRec("flash boot_a");

    async void FlashToBootB(object sender, RoutedEventArgs args) => await FlashRec("flash boot_b");

    public static FilePickerFileType Zip { get; } = new("Zip")
    {
        Patterns = new[] { "*.zip", "*.apk", "*.ko" },
        AppleUniformTypeIdentifiers = new[] { "*.zip", "*.apk", "*.ko" }
    };

    async void OpenMagiskFile(object sender, RoutedEventArgs args)
    {
        patch_busy(true);

        try
        {
            var topLevel = TopLevel.GetTopLevel(this);

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                FileTypeFilter = new[] { Zip },
                Title = "Open File",
                AllowMultiple = false
            });

            if (files.Count == 0)
            {
                patch_busy(false);
                return;
            }

            MagiskFile.Text = StringHelper.FilePath(files[0].Path.ToString());
            // Global.Zipinfo = await ZipDetect.Zip_Detect(MagiskFile.Text);
            _ = dialogManager.CreateDialog()
.WithTitle("Info")
.OfType(NotificationType.Information)
.WithContent($"{GetTranslation("Basicflash_DetectZIP")}\nUseful:{Global.Zipinfo.IsUseful}\nMode:{Global.Zipinfo.Mode}\nVersion:{Global.Zipinfo.Version}")
.Dismiss().ByClickingBackground()
.TryShow();
        }
        catch (Exception ex)
        {
            _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(ex.Message)
.Dismiss().ByClickingBackground()
.TryShow();
        }

        patch_busy(false);
    }

    public static FilePickerFileType Image { get; } = new("Image")
    {
        Patterns = new[] { "*.img" },
        AppleUniformTypeIdentifiers = new[] { "*.img" }
    };

    async void OpenBootFile(object sender, RoutedEventArgs args)
    {
        patch_busy(true);

        try
        {
            var topLevel = TopLevel.GetTopLevel(this);

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                FileTypeFilter = new[] { Image },
                Title = "Open File",
                AllowMultiple = false
            });

            if (files.Count == 0)
            {
                patch_busy(false);
                return;
            }

            BootFile.Text = StringHelper.FilePath(files[0].Path.ToString());
            // Global.Bootinfo = await BootDetect.Boot_Detect(BootFile.Text);
            ArchList.SelectedItem = Global.Bootinfo.Arch;

            _ = dialogManager.CreateDialog()
.WithTitle("Info")
.OfType(NotificationType.Information)
.WithContent($"{GetTranslation("Basicflash_DetectdBoot")}\nArch:{Global.Bootinfo.Arch}\nOS:{Global.Bootinfo.OSVersion}\nPatch_level:{Global.Bootinfo.PatchLevel}\nRamdisk:{Global.Bootinfo.HaveRamdisk}\nKMI:{Global.Bootinfo.KMI}\nKERNEL_FMT:{Global.Bootinfo.Compress}")
.Dismiss().ByClickingBackground()
.TryShow();
        }
        catch (Exception ex)
        {
            _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(ex.Message)
.Dismiss().ByClickingBackground()
.TryShow();
        }

        patch_busy(false);
    }

    async void StartPatch(object sender, RoutedEventArgs args)
    {
        patch_busy(true);

        try
        {
            EnvironmentVariable.KEEPVERITY = (bool)KEEPVERITY.IsChecked;
            EnvironmentVariable.KEEPFORCEENCRYPT = (bool)KEEPFORCEENCRYPT.IsChecked;
            EnvironmentVariable.PATCHVBMETAFLAG = (bool)PATCHVBMETAFLAG.IsChecked;
            EnvironmentVariable.RECOVERYMODE = (bool)RECOVERYMODE.IsChecked;
            EnvironmentVariable.LEGACYSAR = (bool)LEGACYSAR.IsChecked;

            if (Global.Bootinfo.IsUseful != true | string.IsNullOrEmpty(MagiskFile.Text))
            {
                throw new Exception(GetTranslation("Basicflash_SelectBootMagisk"));
            }

            if ((Global.Zipinfo.Mode == PatchMode.None) | (Global.Zipinfo.IsUseful != true))
            {
                // Global.Zipinfo = await ZipDetect.Zip_Detect(MagiskFile.Text);
            }

            string newboot = null;

            switch (Global.Zipinfo.Mode)
            {
                case PatchMode.Magisk:
                    newboot = await MagiskPatch.Magisk_Patch(Global.Zipinfo, Global.Bootinfo);
                    break;
                case PatchMode.GKI:
                    // newboot = await KernelSUPatch.GKI_Patch(Global.Zipinfo, Global.Bootinfo);
                    break;
                case PatchMode.LKM:
                    newboot = await KernelSUPatch.LKM_Patch(Global.Zipinfo, Global.Bootinfo);
                    break;
                    //throw new Exception(GetTranslation("Basicflash_CantKSU"));
            }

            var result = false;

            _ = dialogManager.CreateDialog()
.WithTitle("Warn")
.WithContent(GetTranslation("Basicflash_PatchDone"))
.WithActionButton("Yes", _ => result = true, true)
.WithActionButton("No", _ => result = false, true)
.TryShow();

            if (result == true)
            {
                await FlashBoot(newboot);
            }
            else
            {
                FileHelper.OpenFolder(Path.GetDirectoryName(Global.Bootinfo.Path));
            }

            Global.Zipinfo = new ZipInfo("", "", "", "", "", false, PatchMode.None, "");
            Global.Bootinfo = new BootInfo("", "", "", false, false, "", "", "", "", false, false, false, "", "", "");
            SetDefaultMagisk();
            BootFile.Text = null;
            ArchList.SelectedItem = null;
        }
        catch (Exception ex)
        {
            _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(ex.Message)
.Dismiss().ByClickingBackground()
.TryShow();
        }

        patch_busy(false);
    }

    async Task FlashBoot(string boot)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            var sukiViewModel = GlobalData.MainViewModelInstance;

            if (sukiViewModel.Status == GetTranslation("Home_Fastboot"))
            {
                Global.checkdevice = false;
                var output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} flash boot \"{boot}\"");

                if (!output.Contains("FAILED") && !output.Contains("error"))
                {
                    var result = false;

                    _ = dialogManager.CreateDialog()
.WithTitle("Warn")
.WithContent(GetTranslation("Basicflash_BootFlashSucc"))
.WithActionButton("Yes", _ => result = true, true)
.WithActionButton("No", _ => result = false, true)
.TryShow();

                    if (result == true)
                    {
                        _ = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} reboot");
                    }
                }
                else
                {
                    _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Basicflash_RecoveryFailed"))
.Dismiss().ByClickingBackground()
.TryShow();
                }

                Global.checkdevice = true;
            }
            else
            {
                _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_EnterFastboot"))
.Dismiss().ByClickingBackground()
.TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_NotConnected"))
.Dismiss().ByClickingBackground()
.TryShow();
        }
    }

    async void OpenAFDI(object sender, RoutedEventArgs args)
    {
        if (Global.System == "Windows")
        {
            if (RuntimeInformation.OSArchitecture == Architecture.X64)
            {
                _ = Process.Start(@"Drive\adb.exe");
            }
            else if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
            {
                var drvpath = string.Format($"{Global.runpath}/Drive/adb/*.inf");
                var shell = string.Format("/add-driver {0} /subdirs /install", drvpath);
                var drvlog = await CallExternalProgram.Pnputil(shell);
                FileHelper.Write($"{Global.log_path}/drive.txt", drvlog);

                _ = drvlog.Contains(GetTranslation("Basicflash_Success"))
                    ? dialogManager.CreateDialog()
.WithTitle("Success")
.OfType(NotificationType.Success)
.WithContent(GetTranslation("Common_InstallSuccess"))
.Dismiss().ByClickingBackground()
.TryShow()
                    : dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_InstallFailed"))
.Dismiss().ByClickingBackground()
.TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Basicflash_NotUsed"))
.Dismiss().ByClickingBackground()
.TryShow();
        }
    }

    async void Open9008DI(object sender, RoutedEventArgs args)
    {
        if (Global.System == "Windows")
        {
            if (RuntimeInformation.OSArchitecture == Architecture.X64)
            {
                _ = Process.Start(@"Drive\Qualcomm_HS-USB_Driver.exe");
            }
            else if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
            {
                var drvpath = string.Format($"{Global.runpath}/drive/9008/*.inf");
                var shell = string.Format("/add-driver {0} /subdirs /install", drvpath);
                var drvlog = await CallExternalProgram.Pnputil(shell);
                FileHelper.Write($"{Global.log_path}/drive.txt", drvlog);

                _ = drvlog.Contains(GetTranslation("Basicflash_Success"))
                    ? dialogManager.CreateDialog()
.WithTitle("Success")
.OfType(NotificationType.Success)
.WithContent(GetTranslation("Common_InstallSuccess"))
.Dismiss().ByClickingBackground()
.TryShow()
                    : dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_InstallFailed"))
.Dismiss().ByClickingBackground()
.TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Basicflash_NotUsed"))
.Dismiss().ByClickingBackground()
.TryShow();
        }
    }

    async void OpenUSBP(object sender, RoutedEventArgs args)
    {
        if (Global.System == "Windows")
        {
            var cmd = @"drive\USB3.bat";
            ProcessStartInfo cmdshell = null;

            cmdshell = new ProcessStartInfo(cmd)
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };

            var f = Process.Start(cmdshell);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _ = dialogManager.CreateDialog()
.WithTitle("Info")
.OfType(NotificationType.Information)
.WithContent(GetTranslation("Common_Execution"))
.Dismiss().ByClickingBackground()
.TryShow();
            });
        }
        else
        {
            _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Basicflash_NotUsed"))
.Dismiss().ByClickingBackground()
.TryShow();
        }
    }

    async void FlashMagisk(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            var sukiViewModel = GlobalData.MainViewModelInstance;

            if (MagiskFile.Text != null)
            {
                BusyInstall.IsBusy = true;
                InstallZIP.IsEnabled = false;

                if (TWRPInstall.IsChecked == true)
                {
                    if (sukiViewModel.Status == "Recovery")
                    {
                        _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} push {MagiskFile.Text} /tmp/magisk.apk");
                        _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell twrp install /tmp/magisk.apk");
                    }
                    else
                    {
                        _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_EnterRecovery"))
.Dismiss().ByClickingBackground()
.TryShow();
                    }
                }
                else if (ADBSideload.IsChecked == true)
                {
                    if (sukiViewModel.Status == "Sideload")
                    {
                        _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} sideload \"{MagiskFile.Text}\"");
                    }
                    else
                    {
                        _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_EnterSideload"))
.Dismiss().ByClickingBackground()
.TryShow();
                    }
                }

                _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_Execution"))
.Dismiss().ByClickingBackground()
.TryShow();

                BusyInstall.IsBusy = false;
                InstallZIP.IsEnabled = true;
            }
            else
            {
                _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Basicflash_SelectMagiskRight"))
.Dismiss().ByClickingBackground()
.TryShow();
            }

            if (sukiViewModel.Status == GetTranslation("Home_System"))
            {
                if (MagiskFile.Text != null)
                {
                    BusyInstall.IsBusy = true;
                    InstallZIP.IsEnabled = false;
                    var result = false;

                    _ = dialogManager.CreateDialog()
.WithTitle("Warn")
.WithContent(GetTranslation("Basicflash_PushMagisk"))
.WithActionButton("Yes", _ => result = true, true)
.WithActionButton("No", _ => result = false, true)
.TryShow();

                    if (result == true)
                    {
                        _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} push \"{MagiskFile.Text}\" /sdcard/magisk.apk");

                        _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Basicflash_InstallMagisk"))
.Dismiss().ByClickingBackground()
.TryShow();
                    }

                    BusyInstall.IsBusy = false;
                    InstallZIP.IsEnabled = true;
                }
                else
                {
                    _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Basicflash_SelectMagiskRight"))
.Dismiss().ByClickingBackground()
.TryShow();
                }
            }
        }
        else
        {
            _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_NotConnected"))
.Dismiss().ByClickingBackground()
.TryShow();
        }
    }

    async void DisableOffRec(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            var sukiViewModel = GlobalData.MainViewModelInstance;
            BusyInstall.IsBusy = true;
            InstallZIP.IsEnabled = false;

            if (TWRPInstall.IsChecked == true)
            {
                if (sukiViewModel.Status == "Recovery")
                {
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} push {Global.runpath}/ZIP/DisableAutoRecovery.zip /tmp/");
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell twrp install /tmp/DisableAutoRecovery.zip");
                }
                else
                {
                    _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_EnterRecovery"))
.Dismiss().ByClickingBackground()
.TryShow();
                }
            }
            else if (ADBSideload.IsChecked == true)
            {
                if (sukiViewModel.Status == "Sideload")
                {
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} sideload ZIP/DisableAutoRecovery.zip");
                }
                else
                {
                    _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_EnterSideload"))
.Dismiss().ByClickingBackground()
.TryShow();
                }
            }

            _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_Execution"))
.Dismiss().ByClickingBackground()
.TryShow();

            BusyInstall.IsBusy = false;
            InstallZIP.IsEnabled = true;
        }
        else
        {
            _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_NotConnected"))
.Dismiss().ByClickingBackground()
.TryShow();
        }
    }

    async void SyncAB(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            var sukiViewModel = GlobalData.MainViewModelInstance;
            BusyInstall.IsBusy = true;
            InstallZIP.IsEnabled = false;

            if (TWRPInstall.IsChecked == true)
            {
                if (sukiViewModel.Status == "Recovery")
                {
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} push {Global.runpath}/ZIP/copy-partitions.zip /tmp/");
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell twrp install /tmp/copy-partitions.zip");
                }
                else
                {
                    _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_EnterRecovery"))
.Dismiss().ByClickingBackground()
.TryShow();
                }
            }
            else if (ADBSideload.IsChecked == true)
            {
                if (sukiViewModel.Status == "Sideload")
                {
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} sideload ZIP/copy-partitions.zip");
                }
                else
                {
                    _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_EnterSideload"))
.Dismiss().ByClickingBackground()
.TryShow();
                }
            }

            _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_Execution"))
.Dismiss().ByClickingBackground()
.TryShow();

            BusyInstall.IsBusy = false;
            InstallZIP.IsEnabled = true;
        }
        else
        {
            _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_NotConnected"))
.Dismiss().ByClickingBackground()
.TryShow();
        }
    }
}
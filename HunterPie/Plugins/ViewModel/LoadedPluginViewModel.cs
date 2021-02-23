﻿using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HunterPie.Core;
using HunterPie.Logger;
using HunterPie.UI.Infrastructure;

namespace HunterPie.Plugins
{
    public class LoadedPluginViewModel : BaseViewModel, IPluginViewModel
    {
        private readonly PluginEntry plugin;
        private readonly IPluginListProxy pluginList;
        private Task pluginToggleTask = Task.CompletedTask;
        private bool isFiltered;

        public LoadedPluginViewModel(PluginEntry plugin, IPluginListProxy pluginList)
        {
            this.plugin = plugin;
            this.pluginList = pluginList;

            ToggleCommand = new RelayCommand(
                _ => CanToggle,
                val => IsEnabled = (bool)val
            );
            DeleteCommand = new ArglessRelayCommand(
                () => CanDelete, Delete
            );
            RestoreCommand = new ArglessRelayCommand(
                () => CanRestore, Restore
            );
            pluginList.RegistryPluginsLoaded += OnRegistryPluginsLoaded;
            GetImage();
        }

        private void OnRegistryPluginsLoaded(object sender, EventArgs e)
        {
            GetImage();
            // handle downloads count change
            OnPropertyChanged(nameof(SubText));
        }

        public async void Delete()
        {
            try
            {
                if (plugin.IsFailed)
                {
                    // if plugin loading failed, it can already been loaded into AppDomain, so it isn't safe to delete it right away
                    // because dll's may be in use
                    PluginManager.MarkForRemoval(InternalName);
                }
                else if (plugin.IsLoaded && plugin.Package.HasValue)
                {
                    // begin plugin toggle process in other thread
                    await pluginToggleTask;
                    pluginToggleTask = SetPluginEnabled(false);
                    await pluginToggleTask.ConfigureAwait(false);
                    // NOTE: synchronisation might be helpful here, but I don't anticipate multiple calls to this method
                    //       in short succession, so it is kept simpler

                    PluginManager.MarkForRemoval(InternalName);
                }
                else
                {
                    try
                    {
                        PluginManager.DeleteNonPreloadedPlugin(InternalName);
                    }
                    catch (Exception ex)
                    {
                        Dispatch(() => MessageBox.Show(ex.Message));
                        Debugger.Warn(ex);
                        PluginManager.MarkForRemoval(InternalName);
                    }
                }

                Dispatch(() =>
                {
                    pluginList.UpdatePluginsArrangement();
                    OnPropertyChanged(nameof(IsEnabled));
                });
            }
            catch (Exception ex)
            {
                Dispatch(() => MessageBox.Show(ex.Message));
            }
        }

        public void Restore()
        {
            try
            {
                PluginManager.MarkForRemoval(InternalName, false);
                pluginList.UpdatePluginsArrangement();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public bool CanToggle => !IsBusy && plugin.IsLoaded && !PluginManager.IsMarkedForRemoval(InternalName);
        public bool CanDelete => !IsBusy && !PluginManager.IsMarkedForRemoval(InternalName);
        public bool CanInstall => false;
        public bool CanRestore => PluginManager.IsMarkedForRemoval(InternalName);

        public bool IsVersionOk => true;
        public bool IsFailed => plugin.IsFailed;

        public ICommand DownloadCommand => DisabledCommand.Instance;
        public ICommand DeleteCommand { get; }
        public ICommand ToggleCommand { get; }
        public ICommand RestoreCommand { get; }

        public string InternalName => plugin.PluginInformation.Name;
        public ImageSource Image { get; set; }

        public bool HasImage => Image != null;
        public string Name => string.IsNullOrEmpty(plugin.PluginInformation.DisplayName)
            ? plugin.PluginInformation.Name
            : plugin.PluginInformation.DisplayName;
        public string ReadmeUrl
        {
            get
            {
                var fsPath = Path.Combine(plugin.RootPath, "readme.md");
                if (File.Exists(fsPath))
                {
                    return "file://" + fsPath;
                }

                // if readme isn't found locally, try to use to one from registry
                return pluginList.TryGetRegistryEntry(plugin.PluginInformation.Name)?.Readme;
            }
        }

        public string Description => plugin.PluginInformation.Description;
        public string SubText => GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_PLUGIN_SUBTITLE_ROW']")
            .Replace("{version}", plugin.PluginInformation.Version)
            .Replace("{author}", plugin.PluginInformation.Author)
            .Replace("{downloads}", GetDownloadsCount());

        private string GetDownloadsCount()
        {
            var val = pluginList.TryGetRegistryEntry(plugin.PluginInformation.Name)?.Downloads;
            if (val == null)
            {
                return "-";
            }

            return val.ToString();
        }

        public bool IsEnabled
        {
            get => plugin.Package?.settings.IsEnabled ?? false;
            set
            {
                // if previous toggle didn't finish, ignore value...
                if (!pluginToggleTask.IsCompleted) return;

                // ...otherwise start toggle
                pluginToggleTask = SetPluginEnabled(value);
            }
        }

        /// <summary>
        /// Expected to be started from STA thread.
        /// </summary>
        private async Task SetPluginEnabled(bool value)
        {
            if (!plugin.Package.HasValue || plugin.Package.Value.settings.IsEnabled == value) return;
            plugin.Package.Value.settings.IsEnabled = value;
            IsBusy = true;
            PluginManager.UpdatePluginSettings(plugin.Package.Value.path, plugin.Package.Value.settings);
            try
            {
                // if plugin disabling take longer than what considered "instantaneous", trigger PropertyChanged, so
                // spinner will be displayed. Probably will not fire most of the time.
                var cts = new CancellationTokenSource();
                _ = Task.Delay(300, cts.Token)
                    .ContinueWith(t =>
                    {
                        if (t.IsCanceled) return;
                        Dispatch(() =>
                        {
                            OnPropertyChanged(nameof(IsBusy));
                            CommandManager.InvalidateRequerySuggested();
                        });
                    }, cts.Token);

                // toggling plugin state in other thread
                await Task.Run(() =>
                {
                    if (value)
                    {
                        if (PluginManager.LoadPlugin(plugin.Package.Value.plugin))
                        {
                            Debugger.Module($"Loaded {plugin.PluginInformation.Name}");
                        }
                    }
                    else
                    {
                        if (PluginManager.UnloadPlugin(plugin.Package.Value.plugin))
                        {
                            Debugger.Module($"Unloaded {plugin.PluginInformation.Name}");
                        }
                    }

                    // cancel spinner display when task is finished
                    cts.Cancel();
                });
            }
            finally
            {
                IsBusy = false;
            }

            Dispatch(() =>
            {
                OnPropertyChanged(nameof(IsEnabled));
                OnPropertyChanged(nameof(IsBusy));
                CommandManager.InvalidateRequerySuggested();
            });
        }

        private async void GetImage()
        {
            try
            {
                // trying to display local icon
                string path = $"{plugin.RootPath}/icon.png";
                if (File.Exists(path))
                {
                    var bytes = File.ReadAllBytes(path);
                    Image = LoadImage(bytes);
                }
                else
                {
                    // if image is missing locally, try to load it from registry url
                    var url = pluginList.TryGetRegistryEntry(plugin.PluginInformation.Name)?.ImageUrl;
                    if (string.IsNullOrEmpty(url)) return;

                    using var http = new HttpClient();

                    var bytes = await http.GetByteArrayAsync(url);
                    Image = LoadImage(bytes);
                }
            }
            catch
            {
                Image = null;
            }

            Dispatch(() =>
            {
                OnPropertyChanged(nameof(Image));
                OnPropertyChanged(nameof(HasImage));
            });
        }

        private static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        public bool IsBusy { get; set; }

        public bool IsFiltered
        {
            get => isFiltered;
            set
            {
                if (value == isFiltered) return;
                isFiltered = value;
                OnPropertyChanged();
            }
        }

        public long SortValue => PluginManager.GetInstallationTime(plugin.PluginInformation.Name)?.Ticks ?? -1;

        ~LoadedPluginViewModel()
        {
            pluginList.RegistryPluginsLoaded -= OnRegistryPluginsLoaded;
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace Update
{
    public struct UpdateFinished
    {
        public bool JustUpdated;
        public bool IsLatestVersion;
    }
    class AsyncUpdate
    {
        static readonly string[] validBranches = { "master", "BETA" };
        static string BaseDirectory => AppDomain.CurrentDomain.BaseDirectory;
        private const string UpdateUrl = "https://hunterpie.herokuapp.com/bin/";
        private readonly string updateBranch;
        private string validBranchUrl;

        public int FilesUpdatedCounter { get; private set; }
        public int DifferentFilesCounter { get; private set; }

        public EventHandler<UpdateFinished> OnUpdateFail;
        public EventHandler<UpdateFinished> OnUpdateSuccess;
        public EventHandler<string> OnNewFileUpdate;
        public EventHandler<DownloadProgressChangedEventArgs> OnDownloadProgressChanged;

        private readonly Dictionary<string, string> localFileHashes = new Dictionary<string, string>();
        private Dictionary<string, string> onlineFileHashes;

        public AsyncUpdate(string branch)
        {
            InitializeLogger();
            updateBranch = branch;
            BuildValidBranchLink();
        }

        public void InitializeLogger()
        {
            using (FileStream logFile = File.OpenWrite(Path.Combine(BaseDirectory, "UpdateLog.log")))
            {
                // Clear file
                logFile.SetLength(0);

                string msg = "Initializing auto-update...";
                logFile.Write(Encoding.UTF8.GetBytes(msg), 0, msg.Length);
            }
        }

        public void WriteToFile(string message)
        {
            using (FileStream logFile = File.Open(Path.Combine(BaseDirectory, "UpdateLog.log"), FileMode.Append))
            {
                message = $"\n{message}";
                logFile.Write(Encoding.UTF8.GetBytes(message), 0, message.Length);
            }
        }

        public async Task<bool> Start()
        {
            if (await GetOnlineFileHashes())
            {
                GetLocalFileHashes();
                string[] files = GetDifferentFiles();
                if (await UpdateFiles(files))
                {
                    string[] toBeDeleted = GetFilesToBeDeleted();
                    if (toBeDeleted.Length > 0)
                    {
                        foreach (string file in toBeDeleted)
                            DeleteFile(file);
                    }

                    OnUpdateSuccess?.Invoke(this, new UpdateFinished { JustUpdated = files.Length > 0, IsLatestVersion = files.Length == 0 });
                    WriteToFile("Update successful!");
                }
            }
            return false;
        }

        private void BuildValidBranchLink()
        {
            if (validBranches.Contains(updateBranch))
            {
                validBranchUrl = $"{UpdateUrl}{updateBranch}/";
            }
            else
            {
                validBranchUrl = $"{UpdateUrl}master/";
            }
        }

        private async Task<bool> GetOnlineFileHashes()
        {
            Message("Looking for online files...");
            WriteToFile("Looking for online files...");
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string hashes = await client.GetStringAsync(new Uri($"{validBranchUrl}hashes.json?r={DateTime.UtcNow.GetHashCode()}"));

                    onlineFileHashes = JsonConvert.DeserializeObject<Dictionary<string, string>>(hashes);
                }
                return true;
            }
            catch (Exception err)
            {
                Message("Failed to get online files.");
                WriteToFile($"GetOnlineFileHashes Exception -> {err}");
                OnUpdateFail?.Invoke(this, new UpdateFinished { IsLatestVersion = true, JustUpdated = false });
                return false;
            }
        }

        private void GetLocalFileHashes()
        {
            Message("Calculating local hashes...");
            Dictionary<string, string>.KeyCollection fileNames = onlineFileHashes.Keys;
            foreach (string fileName in fileNames)
            {
                // Create directories if needed
                CreateDirectories(fileName);

                // Calculate SHA256 hash
                string hash = Hasher.HashFile(Path.Combine(BaseDirectory, fileName));
                localFileHashes[fileName] = hash;
            }
        }

        private void CreateDirectories(string path)
        {
            string dirName = Path.GetDirectoryName(path);
            if (dirName.Length > 0)
            {
                if (!Directory.Exists(Path.Combine(BaseDirectory, dirName)))
                {
                    Directory.CreateDirectory(Path.Combine(BaseDirectory, dirName));
                }
            }
        }

        private string[] GetDifferentFiles()
        {
            List<string> files = new List<string>();
            foreach (string fileName in onlineFileHashes.Keys)
            {
                // Welp
                if (fileName == "Newtonsoft.Json.dll")
                {
                    continue;
                }

                string online = onlineFileHashes[fileName];
                string local = localFileHashes[fileName];

                if (online.ToLowerInvariant() == "installonly" && !string.IsNullOrEmpty(local))
                {
                    continue;
                }
                else
                {
                    if (online != local)
                    {
                        WriteToFile($"Added {fileName} to update queue.");
                        files.Add(fileName);
                        DifferentFilesCounter++;
                    }
                }
            }
            return files.ToArray();
        }

        private string[] GetFilesToBeDeleted()
        {
            string[] files = onlineFileHashes.Keys
                .Where(filename => filename.ToLowerInvariant() == "delete" && FileExists(filename))
                .ToArray();

            return files;
        }

        private static bool FileExists(string relativePath)
        {
            string path = Path.Combine(BaseDirectory, relativePath);
            return File.Exists(path);
        }

        private static void DeleteFile(string relativePath)
        {
            File.Delete(Path.Combine(BaseDirectory, relativePath));
        }

        private async Task<bool> UpdateFiles(string[] files)
        {
            string tmpDir = Path.Combine(BaseDirectory, "tmp");
            try
            {
                if (!Directory.Exists(tmpDir))
                {
                    Directory.CreateDirectory(tmpDir);
                }
                Dictionary<string, string> fileMap = new Dictionary<string, string>();

                foreach (string file in files)
                {
                    string tmpPath = Path.Combine(tmpDir, file);
                    string dstPath = Path.Combine(BaseDirectory, file);
                    fileMap.Add(tmpPath, dstPath);

                    Message($"Downloading {file.Replace("_", "__")}");
                    WriteToFile($"Downloading {file}");
                    Uri link = new Uri($"{validBranchUrl}{file}?r={DateTime.UtcNow.GetHashCode()}");
                    await Download(link, tmpPath);
                }

                foreach (var pair in fileMap)
                {
                    if (!File.Exists(pair.Value))
                    {
                        File.Move(pair.Key, pair.Value);
                    } else
                    {
                        File.Replace(pair.Key, pair.Value, null);
                    }
                    WriteToFile($"Moved {pair.Key} -> {pair.Value}");
                }
            }
            catch (Exception err)
            {
                WriteToFile(err.ToString());
                OnUpdateFail?.Invoke(this, new UpdateFinished { IsLatestVersion = true, JustUpdated = false });
                return false;
            }
            finally
            {
                if (Directory.Exists(tmpDir))
                {
                    Directory.Delete(tmpDir, true);
                }
            }

            return true;
        }

        private void Message(string message) => OnNewFileUpdate?.Invoke(this, message);

        private void OnDownloadProgressChange(object sender, DownloadProgressChangedEventArgs e)
        {
            OnDownloadProgressChanged?.Invoke(this, e);
        }

        private async Task Download(Uri uri, string dst)
        {
            // Create folders if needed
            CreateDirectories(dst);
            try
            {
                using (WebClient client = new WebClient { Timeout = 10000 })
                {
                    client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                    client.Headers.Add(HttpRequestHeader.CacheControl, "no-store");
                    client.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                    client.DownloadProgressChanged += OnDownloadProgressChange;
                    await client.DownloadFileTaskAsync(uri, dst);
                    FilesUpdatedCounter++;
                    client.DownloadProgressChanged -= OnDownloadProgressChange;
                }
            } catch(Exception err)
            {
                WriteToFile(err.ToString());
                OnUpdateFail?.Invoke(this, new UpdateFinished { IsLatestVersion = true, JustUpdated = false });
            }
            
        }
    }
}

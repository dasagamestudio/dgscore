using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

/*===============================================================
Product:    	Project Name: MadOverGames Assignment
Developer:  	Developer Name: Ankit Sethi - ankitsethi@dasagamestudio.com
Company:    	Company: DasaGame Studio
Created On:     Created On: 11/16/2019 11:55:34 PM
Modified On:    Modified On: 11/16/2019 11:55:34 PM
Copyright:  	Copyright: @ Copyright 2019-2020. All rights Reserved. DasaGame Studio
================================================================*/

namespace DGS.Game.Proto.MadOverGames.ImageFramework {
	
	[Serializable]
	[Flags]
	public enum DGSWebImageOptions {
		None = 0,
		MemoryCache = 1,
		DiskCache = 2,
		ShowLoadingIndicator = 4
	}
	
	public class DGSImageCache : Utilties.Singleton<DGSImageCache> {

		#region Variables.

		private long _maxCacheSize = 0;
		private int _maxCacheAge = 7;
		private string _cacheDirectoryPath;
		private Dictionary<string, byte[]> _memoryCache;

		#endregion

		#region Unity Methods.

		protected override void Awake() {
			base.Awake();

			Init();

			ThreadPool.QueueUserWorkItem((state) => {
				DeleteOldFilesOnDisk();
			});
		}

		#endregion

		#region Custom Methods.

		private void Init() {
			Application.lowMemory += OnLowMemory;
			
			_memoryCache = new Dictionary<string, byte[]>();

			_cacheDirectoryPath = Path.Combine(Application.persistentDataPath, "DGSWebImages");
			if(!Directory.Exists(_cacheDirectoryPath))
				Directory.CreateDirectory(_cacheDirectoryPath);
		}

		#endregion

		#region Public API Mathods.

		public void QueryImageDataFromCacheForURL(string url, DGSWebImageOptions options, Action<byte[]> callback) {
			if((options & DGSWebImageOptions.MemoryCache) != 0 && ImageDataExistsInMemory(url)) {
				var data = LoadImageDataFromMemory(url);

				if(data != null) {
					callback?.Invoke(data);
					return;
				}
			}

			if((options & DGSWebImageOptions.DiskCache) != 0 && ImageDataExistsInDisk(url)) {
				var data = LoadImageDataFromDisk(url);

				if(data != null) {
					callback?.Invoke(data);
					return;
				}
			}
			
			callback?.Invoke(null);
		}

		public void CacheImageDataForURL(string url, byte[] data, DGSWebImageOptions options) {
			if((options & DGSWebImageOptions.MemoryCache) != 0) {
				ThreadPool.QueueUserWorkItem((state) => {
					StoreImageDataInMemory(url, data);
				});
			}

			if((options & DGSWebImageOptions.DiskCache) != 0) {
				ThreadPool.QueueUserWorkItem((state) => {
					StoreImageDataInDisk(url, data);
				});
			}
		}

		public void RemoveImageDataFromCache(string url, DGSWebImageOptions options) {
			if((options & DGSWebImageOptions.MemoryCache) != 0) {
				RemoveImageDataFromMemory(url);
			}

			if((options & DGSWebImageOptions.DiskCache) != 0) {
				RemoveImageDataFromDisk(url);
			}
		}

		#endregion

		#region Memory Cache Methods.

		private bool ImageDataExistsInMemory(string url) {
			return _memoryCache.ContainsKey(url);
		}

		private void StoreImageDataInMemory(string url, byte[] data) {
			_memoryCache.Add(url, data);
		}

		private byte[] LoadImageDataFromMemory(string url) {
			return _memoryCache.ContainsKey(url) ? _memoryCache[url] : null;
		}

		private void RemoveImageDataFromMemory(string url) {
			if(_memoryCache.ContainsKey(url)) {
				_memoryCache.Remove(url);
			}
		}

		private void ClearMemoryCache() {
			_memoryCache.Clear();
		}

		#endregion

		#region Disk Cache Methods.

		private bool ImageDataExistsInDisk(string url) {
	        return File.Exists(PathForURL(url));
	    }

	    private void StoreImageDataInDisk(string url, byte[] data) {
	        File.WriteAllBytes(PathForURL(url), data);
	    }

	    private byte[] LoadImageDataFromDisk(string url) {
	        var path = PathForURL(url);

	        if(File.Exists(path)) {
	            var data = File.ReadAllBytes(path);

	            return data;
	        }

	        return null;
	    }

	    private void RemoveImageDataFromDisk(string url) {
	        if(ImageDataExistsInDisk(url)) {
	            File.Delete(PathForURL(url));
	        }
	    }

	    private void DeleteOldFilesOnDisk() {
	        if(_maxCacheAge == 0 && _maxCacheSize == 0) {
	            return;
	        }

	        var directoryInfo = new DirectoryInfo(_cacheDirectoryPath);
	        var files = directoryInfo.GetFiles("*.*");

	        var expirationDate = DateTime.Now.AddDays(-_maxCacheAge);
	        var filesToDelete = new List<string>();

	        long currentCacheSize = 0;
	        var unexpiredCacheFiles = new List<FileInfo>();

	        foreach(var file in files) {
	            if(_maxCacheAge > 0 && DateTime.Compare(file.LastAccessTime, expirationDate) < 0) {
	                filesToDelete.Add(file.Name);
	                continue;
	            }


	            currentCacheSize += file.Length;
	            unexpiredCacheFiles.Add(file);
	        }

	        foreach(var filename in filesToDelete) {
	            File.Delete(PathForFilename(filename));
	        }

	        if(_maxCacheSize > 0 && currentCacheSize > _maxCacheSize) {
	            var desiredCacheSize = _maxCacheSize / 2;

	            var sortedFiles = unexpiredCacheFiles.OrderByDescending(f => f.LastWriteTime).ToList();

	            foreach(FileInfo file in sortedFiles) {
	                File.Delete(PathForFilename(file.Name));

	                currentCacheSize -= file.Length;

	                if(currentCacheSize < desiredCacheSize) {
	                    break;
	                }
	            }
	        }
	    }

		#endregion

		#region Helper Methods.

		private string PathForFilename(string filename) {
			return Path.Combine(_cacheDirectoryPath, filename);
		}

		private string PathForURL(string url) {
			return Path.Combine(_cacheDirectoryPath, FilenameForURL(url));
		}

		private string FilenameForURL(string url) {
			var pathExtension = !string.IsNullOrEmpty(Path.GetExtension(url)) ? Path.GetExtension(url) : ".img";
			var pathExtensionMatch = Regex.Match(pathExtension, "(\\.\\w+)");
			var filename = $"{Md5(url)}{Path.GetExtension(pathExtensionMatch.Value)}";

			return filename;
		}

		private string Md5(string url) {
			var provider = new MD5CryptoServiceProvider();

			var bytes = System.Text.Encoding.UTF8.GetBytes(url);
			bytes = provider.ComputeHash(bytes);

			var output = "";
			foreach(var b in bytes) {
				output += b.ToString("x2").ToLower();
			}

			return output;
		}

		#endregion

		#region Event Methods.

		private void OnLowMemory() {
			ClearMemoryCache();
		}

		#endregion
	}
}
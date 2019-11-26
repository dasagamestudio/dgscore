using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*===============================================================
Product:    	Project Name: MadOverGames Assignment
Developer:  	Developer Name: Ankit Sethi - ankitsethi@dasagamestudio.com
Company:    	Company: DasaGame Studio
Created On:     Created On: 11/17/2019 12:32:01 AM
Modified On:    Modified On: 11/17/2019 12:32:01 AM
Copyright:  	Copyright: @ Copyright 2019-2020. All rights Reserved. DasaGame Studio
================================================================*/

namespace DGS.Game.Proto.MadOverGames.ImageFramework {
	public class DGSWebImageManager : Utilties.Singleton<DGSWebImageManager> {

		#region Variables.

		private HashSet<string> _failedURLs;

		#endregion

		#region Unity Methods.

		protected override void Awake() {
			base.Awake();
			
			_failedURLs = new HashSet<string>();
		}

		#endregion

		#region Public API Methods.
		
		public void LoadImageWithURL(string url, DGSWebImageOptions options, Action<float> progressCallback, 
		                             Action<DGSWebImageDownloaderError, Texture2D> completionCallback) {
			if(_failedURLs.Contains(url)) {
				if(completionCallback != null) {
					completionCallback(new DGSWebImageDownloaderError("Unable to convert downloaded data into texture"), null);
				}

				return;
			}

			DGSImageCache.Instance.QueryImageDataFromCacheForURL(url, options, (cachedData) => {
				if(cachedData != null) {
					var texture = TextureFromData(cachedData);

					if(texture != null) {
						if(completionCallback != null) {
							completionCallback(null, texture);
						}

						return;
					}

					DGSImageCache.Instance.RemoveImageDataFromCache(url, options);
				}


				StartCoroutine(DGSWebImageDownloader.Instance.DownloadImageWitURL(url, progressCallback, (error, downloadedData) => {
					if(error != null) {
						if(error.type == DGSWebImageDownloaderError.ErrorType.InvalidURL
						 || error.type == DGSWebImageDownloaderError.ErrorType.NotFound
						 || error.type == DGSWebImageDownloaderError.ErrorType.FailedURL) {
							_failedURLs.Add(url);
						}

						if(completionCallback != null) {
							completionCallback(error, null);
						}

						return;
					}

					if(downloadedData != null) {
						if(completionCallback != null) {
							completionCallback(null, TextureFromData(downloadedData));
						}

						DGSImageCache.Instance.CacheImageDataForURL(url, downloadedData, options);
					}
				}));
			});
		}

		#endregion

		#region Helper Methods.
		
		private Texture2D TextureFromData(byte[] data) {
			var texture = new Texture2D(8, 8);
			texture.LoadImage(data);

			return texture == null || (texture.width == 8 && texture.height == 8) ? null : texture;
		}

		#endregion
	}
}
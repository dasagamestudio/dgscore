using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static System.String;

/*===============================================================
Product:    	Project Name: MadOverGames Assignment
Developer:  	Developer Name: Ankit Sethi - ankitsethi@dasagamestudio.com
Company:    	Company: DasaGame Studio
Created On:     Created On: 11/17/2019 12:36:48 AM
Modified On:    Modified On: 11/17/2019 12:36:48 AM
Copyright:  	Copyright: @ Copyright 2019-2020. All rights Reserved. DasaGame Studio
================================================================*/

namespace DGS.Game.Proto.MadOverGames.ImageFramework {
	public class DGSWebImageDownloader : Utilties.Singleton<DGSWebImageDownloader> {

		#region Variables.
		#endregion

		#region Unity Methods.
		#endregion

		#region Public API Methods.
		
		public IEnumerator DownloadImageWitURL(string url, Action<float> progressCallback, 
		                                       Action<DGSWebImageDownloaderError, byte[]> completionCallback) {
			if(!IsURLValid(url)) {
				if(completionCallback != null) {
					var error = new DGSWebImageDownloaderError("Image url isn't valid");
					completionCallback(error, null);
				}

				yield break;
			}

			if(Application.internetReachability == NetworkReachability.NotReachable) {
				if(completionCallback != null) {
					var error = new DGSWebImageDownloaderError("No internet connection");
					completionCallback(error, null);
				}

				yield break;
			}

			using(var www = UnityWebRequestTexture.GetTexture(url)) {
				yield return www.SendWebRequest();
				
				if(progressCallback != null) {
					StartCoroutine(TrackDownloadProgress(www, progressCallback));
				}

				yield return www;

				if(!IsNullOrEmpty(www.error)) {
					if(completionCallback != null) {
						var error = new DGSWebImageDownloaderError(www.error);
						completionCallback(error, null);
					}

					yield break;
				}

				var textureContent = DownloadHandlerTexture.GetContent(www);

				if(textureContent == null || (textureContent.width == 8 && textureContent.height == 8)) {
					if(completionCallback != null) {
						var error = new DGSWebImageDownloaderError("Unable to convert downloaded data into texture");
						completionCallback(error, null);
					}

					yield break;
				}

				if(www.isDone && www.downloadHandler.data != null) {
					if(progressCallback != null) {
						progressCallback(1.0f);
					}

					if(completionCallback != null) {
						completionCallback(null, www.downloadHandler.data);
					}
				}
			}
			
		}
		#endregion

		#region Helper Methods.
		
		private IEnumerator TrackDownloadProgress(UnityWebRequest www, Action<float> progressCallback) {
			while(!www.isDone) {
				if(progressCallback != null) {
					progressCallback(www.downloadProgress);
				}

				yield return new WaitForSeconds(0.1f);
			}
		}

		private bool IsURLValid(string url) {
			return !IsNullOrEmpty(url) && url.Substring(0, 4) == "http";
		}
		#endregion
	}


	public class DGSWebImageDownloaderError {
		public enum ErrorType { Unknown, InvalidURL, NoInternet, UnresolvedHost, NotFound, RequestTimedOut, FailedURL }

		public string description;
		public ErrorType type;

		public DGSWebImageDownloaderError(string description) {
			this.description = description;

			switch(description) {
				case "Image url isn't valid":
					type = ErrorType.InvalidURL;
					break;
				case "No internet connection":
					type = ErrorType.NoInternet;
					break;
				case "Cannot resolve destination host":
					type = ErrorType.UnresolvedHost;
					break;
				case "404 Not Found":
					type = ErrorType.NotFound;
					break;
				case "Unable to complete SSL connection":
					type = ErrorType.RequestTimedOut;
					break;
				case "Unable to convert downloaded data into texture":
					type = ErrorType.FailedURL;
					break;
				default:
					type = ErrorType.Unknown;
					break;
			}
		}

	}
}
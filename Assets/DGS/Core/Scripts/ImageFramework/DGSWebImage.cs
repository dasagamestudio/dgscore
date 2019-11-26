using System;
using System.Linq;
using DGS.Game.Proto.MadOverGames.Utilties;
using UnityEngine;
using UnityEngine.UI;

/*===============================================================
Product:    	Project Name: MadOverGames Assignment
Developer:  	Developer Name: Ankit Sethi - ankitsethi@dasagamestudio.com
Company:    	Company: DasaGame Studio
Created On:     Created On: 11/17/2019 12:18:04 AM
Modified On:    Modified On: 11/17/2019 12:18:04 AM
Copyright:  	Copyright: @ Copyright 2019-2020. All rights Reserved. DasaGame Studio
================================================================*/

namespace DGS.Game.Proto.MadOverGames.ImageFramework {
	[AddComponentMenu("DGS/DGS Web Image")]
	public class DGSWebImage : MonoBehaviour {

		#region Variables.
		/// <summary>
		/// Url of a web image to be loaded
		/// </summary>
		public string imageURL;

		/// <summary>
		/// Placeholder Texture to be used while loading the web image
		/// </summary>
		public Texture2D placeholderImage;

		/// <summary>
		/// Sets whether or not the image will preserve its aspect ratio (Image Component only)
		/// </summary>
		public bool preserveAspect = false;

		/// <summary>
		/// Sets whether or not the image will start loading automatically using the inspector image url
		/// </summary>
		public bool autoDownload = true;

		/// <summary>
		/// Sets whether or not the image will be cached in memory
		/// </summary>
		public bool memoryCache = false;

		/// <summary>
		/// Sets whether or not the image will be cached in disk
		/// </summary>
		public bool diskCache = true;

		/// <summary>
		/// Sets whether or not the loading indicator will be shown while loading the web image
		/// </summary>
		public bool showLoadingIndicator = true;

		/// <summary>
		/// Loading indicator object to be shown while loading the web image
		/// </summary>
		public GameObject loadingIndicator;

		/// <summary>
		/// Sets loading indicator type
		/// </summary>
		public LoadingIndicatorType loadingIndicatorType;

		/// <summary>
		/// Sets loading indicator scale
		/// </summary>
		public float loadingIndicatorScale = 1;

		/// <summary>
		/// Sets loading indicator color
		/// </summary>
		public Color loadingIndicatorColor = Color.black;

		private Component _targetComponent;
		private int _targetMaterial = 0;

		#endregion

		#region Delegates.

		/// <summary>
		/// Called when the Image is loaded and the texture size is available.
		/// </summary>
		public delegate void OnImageSizeReadyAction(Vector2 size);
		public event OnImageSizeReadyAction OnImageSizeReady;

		/// <summary>
		/// Called when the Image could not be loaded.
		/// </summary>
		public delegate void OnLoadingErrorAction(DGSWebImageDownloaderError error);
		public event OnLoadingErrorAction OnLoadingError;

		#endregion

		#region Unity Methods.

		private void Start() {
			Init();

			if(autoDownload)
				SetImage();
		}

		#endregion

		#region Public API Methods.

		/// <summary>
		/// Initialize the Web Image.
		/// </summary>
		public void Init() {
			_targetComponent = GetTargetComponent();
			if(loadingIndicator) {
				loadingIndicator.SetActive(false);
			}
		}

		/// <summary>
		/// Sets the target component with a web image of the inspector url.
		/// </summary>
		public void SetImage() {
			if(!string.IsNullOrEmpty(imageURL)) {
				SetImageWithURL(imageURL, placeholderImage);
			}
		}

		/// <summary>
		/// Sets the target component with a web image.
		/// </summary>
		/// <param name="url">URL of the image to be loaded.</param>
		public void SetImageWithURL(string url) {
			InternalSetImageWithURL(url, null, GetDGSWebImageOptions(), null, SetTexture);
		}

		/// <summary>
		/// Sets the target component with a web image.
		/// </summary>
		/// <param name="url">URL of the image to be loaded.</param>
		/// <param name="placeholder">Texture to be used during image loading.</param>
		public void SetImageWithURL(string url, Texture2D placeholder) {
			InternalSetImageWithURL(url, placeholder, GetDGSWebImageOptions(), null, SetTexture);
		}

		/// <summary>
		/// Sets the target component with a web image.
		/// </summary>
		/// <param name="url">URL of the image to be loaded.</param>
		/// <param name="placeholder">Texture to be used during image loading.</param>
		/// <param name="options">Custom options for the DGSWebImage.</param>
		public void SetImageWithURL(string url, Texture2D placeholder, DGSWebImageOptions options) {
			InternalSetImageWithURL(url, placeholder, options, null, SetTexture);
		}

		/// <summary>
		/// Loads a web image.
		/// </summary>
		/// <param name="url">URL of the image to be loaded.</param>
		/// <param name="completionCallback">Called when the image is loaded.</param>
		public void SetImageWithURL(string url, Action<Texture2D> completionCallback) {
			InternalSetImageWithURL(url, null, GetDGSWebImageOptions(), null, completionCallback);
		}

		/// <summary>
		/// Sets the target component with a web image.
		/// </summary>
		/// <param name="url">URL of the image to be loaded.</param>
		/// <param name="placeholder">Texture to be used during image loading.</param>
		/// <param name="progressCallback">Called periodically to indicate the download progress.</param>
		public void SetImageWithURL(string url, Texture2D placeholder, Action<float> progressCallback) {
			InternalSetImageWithURL(url, placeholder, GetDGSWebImageOptions(), progressCallback, SetTexture);
		}

		/// <summary>
		/// Loads a web image.
		/// </summary>
		/// <param name="url">URL of the image to be loaded.</param>
		/// <param name="placeholder">Texture to be used during image loading.</param>
		/// <param name="completionCallback">Called when the image is loaded.</param>
		public void SetImageWithURL(string url, Texture2D placeholder, Action<Texture2D> completionCallback) {
			InternalSetImageWithURL(url, placeholder, GetDGSWebImageOptions(), null, completionCallback);
		}

		/// <summary>
		/// Sets the target component with a web image.
		/// </summary>
		/// <param name="url">URL of the image to be loaded.</param>
		/// <param name="placeholder">Texture to be used during image loading.</param>
		/// <param name="options">Custom options for the DGSWebImage.</param>
		/// <param name="progressCallback">Called periodically to indicate the download progress.</param>
		public void SetImageWithURL(string url, Texture2D placeholder, DGSWebImageOptions options, Action<float> progressCallback) {
			InternalSetImageWithURL(url, placeholder, options, progressCallback, SetTexture);
		}

		/// <summary>
		/// Loads a web image.
		/// </summary>
		/// <param name="url">URL of the image to be loaded.</param>
		/// <param name="placeholder">Texture to be used during image loading.</param>
		/// <param name="options">Custom options for the DGSWebImage.</param>
		/// <param name="completionCallback">Called when the image is loaded.</param>
		public void SetImageWithURL(string url, Texture2D placeholder, DGSWebImageOptions options, Action<Texture2D> completionCallback) {
			InternalSetImageWithURL(url, placeholder, options, null, completionCallback);
		}

		/// <summary>
		/// Loads a web image.
		/// </summary>
		/// <param name="url">URL of the image to be loaded.</param>
		/// <param name="placeholder">Texture to be used during image loading.</param>
		/// <param name="progressCallback">Called periodically to indicate the download progress.</param>
		/// <param name="completionCallback">Called when the image is loaded./param>
		public void SetImageWithURL(string url, Texture2D placeholder, Action<float> progressCallback, Action<Texture2D> completionCallback) {
			InternalSetImageWithURL(url, placeholder, GetDGSWebImageOptions(), progressCallback, completionCallback);
		}

		/// <summary>
		/// Loads a web image.
		/// </summary>
		/// <param name="url">URL of the image to be loaded.</param>
		/// <param name="placeholder">Texture to be used during image loading.</param>
		/// <param name="options">Custom options for the DGSWebImage.</param>
		/// <param name="progressCallback">Called periodically to indicate the download progress.</param>
		/// <param name="completionCallback">Called when the image is loaded.</param>
		public void SetImageWithURL(string url, Texture2D placeholder, DGSWebImageOptions options, Action<float> progressCallback, Action<Texture2D> completionCallback) {
			InternalSetImageWithURL(url, placeholder, options, progressCallback, completionCallback);
		}

		#endregion

		#region Image Controller Methods.

		private void InternalSetImageWithURL(string url, Texture2D placeholder, DGSWebImageOptions options, 
		                                     Action<float> progressCallback, Action<Texture2D> completionCallback) {
			if(placeholder != null) {
				if(completionCallback != null) {
					completionCallback(placeholder);
				}
			}

			if(string.IsNullOrEmpty(url)) {
				Debug.LogWarning("Image url isn't set");
				return;
			}


			if((options & DGSWebImageOptions.ShowLoadingIndicator) != 0 && loadingIndicator) {
				loadingIndicator.SetActive(true);
			}

			DGSWebImageManager.Instance.LoadImageWithURL(url, options, progressCallback, (error, texture) => {
				if((options & DGSWebImageOptions.ShowLoadingIndicator) != 0 && loadingIndicator) {
					loadingIndicator.SetActive(false);
				}

				if(error != null) {
					Debug.LogWarning(error.description);
					if(OnLoadingError != null) {
						OnLoadingError(error);
					}

					return;
				}

				if(OnImageSizeReady != null) {
					OnImageSizeReady(new Vector2(texture.width, texture.height));
				}

				if(completionCallback != null) {
					completionCallback(texture);
				}
			});
		}

		private Component GetTargetComponent() {
			var components = GetComponents<Component>();
			return components.FirstOrDefault(component => component is Renderer || component is RawImage || component is Image);
		}

		private void SetTexture(Texture2D texture) {
			if(_targetComponent == null) 
				return;

			// SpriteRenderer
			if(_targetComponent is SpriteRenderer) {
				var target = (SpriteRenderer)_targetComponent;
#if UNITY_5_6_OR_NEWER
				var oldSize = target.size;
#endif
				var newSprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
				newSprite.name = "Web Image Sprite";
				newSprite.hideFlags = HideFlags.HideAndDontSave;
				target.sprite = newSprite;
#if UNITY_5_6_OR_NEWER
				target.size = oldSize;
#endif
				return;
			}

			// Renderer
			if(_targetComponent is Renderer) {
				var target = (Renderer)_targetComponent;
				if(target.sharedMaterial == null) return;
				if(target.sharedMaterials.Length > 0 && target.sharedMaterials.Length > _targetMaterial) {
					target.sharedMaterials[_targetMaterial].mainTexture = texture;
				} else {
					target.sharedMaterial.mainTexture = texture;
				}
				return;
			}

			// RawImage
			if(_targetComponent is RawImage) {
				var target = (RawImage)_targetComponent;
				target.texture = texture;
				return;
			}

			// Image
			if(_targetComponent is Image) {
				var target = (Image)_targetComponent;
				var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
				target.preserveAspect = preserveAspect;
				target.sprite = sprite;
				return;
			}
		}

		#endregion

		#region Helper Methods.

		private DGSWebImageOptions GetDGSWebImageOptions() {
			var options = DGSWebImageOptions.None;

			if(memoryCache) {
				options |= DGSWebImageOptions.MemoryCache;
			}

			if(diskCache) {
				options |= DGSWebImageOptions.DiskCache;
			}

			if(showLoadingIndicator) {
				options |= DGSWebImageOptions.ShowLoadingIndicator;
			}

			return options;
		}

		#endregion

		#region Event Methods.

		#endregion
	}
}
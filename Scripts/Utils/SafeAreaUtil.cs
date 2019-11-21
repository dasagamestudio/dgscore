using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*===============================================================
Project Name: DGS Core
Developer Name: Ankit Sethi - ankitsethi@dasagamestudio.com
Company: Dasa Game Studio
Created On: 19-10-2019 21:18:29
Modified On: 19-10-2019 21:18:29
Copyright: @ Copyright 2019-2020. All rights Reserved. Dasa Game Studio
================================================================*/

namespace DGS.Game.Core.Utils {
	public class SafeAreaUtil : MonoBehaviour {

		#region Variables.
		private RectTransform _panel;
		private Rect          _lastSafeArea = new Rect (0, 0, 0, 0);
		#endregion

		#region Unity Methods.
		private void Awake() {
			_panel = GetComponent<RectTransform> ();
			Refresh ();
		}

		private void Update() {
			Refresh();
		}
		#endregion

		#region Custom Methods.
		private void Refresh() {
			var safeArea = GetSafeArea();

			if(safeArea != _lastSafeArea)
				ApplySafeArea(safeArea);
		}
		
		private static Rect GetSafeArea() {
			return Screen.safeArea;
		}

		private void ApplySafeArea(Rect r) {
			_lastSafeArea = r;

			// Convert safe area rectangle from absolute pixels to normalised anchor coordinates
			var anchorMin = r.position;
			var anchorMax = r.position + r.size;
			anchorMin.x     /= Screen.width;
			anchorMin.y     /= Screen.height;
			anchorMax.x     /= Screen.width;
			anchorMax.y     /= Screen.height;
			_panel.anchorMin =  anchorMin;
			_panel.anchorMax =  anchorMax;

			Debug.LogFormat("New safe area applied to {0}: x={1}, y={2}, w={3}, h={4} on full extents w={5}, h={6}",
			                 name, r.x, r.y, r.width, r.height, Screen.width, Screen.height);
		}
		#endregion

		#region Event Handler Methods.
		#endregion
	}
}
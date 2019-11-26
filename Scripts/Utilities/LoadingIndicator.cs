using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*===============================================================
Product:    	Project Name: MadOverGames Assignment
Developer:  	Developer Name: Ankit Sethi - ankitsethi@dasagamestudio.com
Company:    	Company: DasaGame Studio
Created On:     Created On: 11/17/2019 1:04:27 AM
Modified On:    Modified On: 11/17/2019 1:04:27 AM
Copyright:  	Copyright: @ Copyright 2019-2020. All rights Reserved. DasaGame Studio
================================================================*/

namespace DGS.Game.Proto.MadOverGames.Utilties {
	
	/// <summary>
	/// List of the loading indicator types.
	/// </summary>
	public enum LoadingIndicatorType { None, RoundedRect, Circle, Circles }
	
	public class LoadingIndicator : MonoBehaviour {

		#region Variables.

		private RectTransform _rectTransform;
		[SerializeField] private float _rotateSpeed = -200f;

		#endregion

		#region Unity Methods.

		private void Start() {
			_rectTransform = GetComponent<RectTransform>();
		}

		private void Update() {
			_rectTransform.Rotate(0f, 0f, _rotateSpeed * Time.deltaTime);
		}

		#endregion

		#region Custom Methods.

		#endregion

		#region Event Methods.

		#endregion
	}
}
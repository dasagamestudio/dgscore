using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*===============================================================
Product:    	Project Name: MadOverGames Assignment
Developer:  	Developer Name: Ankit Sethi - ankitsethi@dasagamestudio.com
Company:    	Company: DasaGame Studio
Created On:     Created On: 11/16/2019 11:49:02 PM
Modified On:    Modified On: 11/16/2019 11:49:02 PM
Copyright:  	Copyright: @ Copyright 2019-2020. All rights Reserved. DasaGame Studio
================================================================*/

namespace DGS.Game.Proto.MadOverGames.Utilties {
	public abstract class Singleton<T> : MonoBehaviour where T : Component {

		#region Fields and Properties.

		private static T _instance;

		public static T Instance {
			get {
				if(_instance != null)
					return _instance;

				_instance = FindObjectOfType<T>();
				if(_instance != null)
					return _instance;
				
				var obj = new GameObject(typeof(T).Name);
				_instance = obj.AddComponent<T>();

				return _instance;
			}
		}

		#endregion

		#region Unity Methods.

		protected virtual void Awake() {
			if(_instance == null) {
				_instance = this as T;
				DontDestroyOnLoad(gameObject);
			} else {
				Destroy(gameObject);	
			}
		}

		#endregion

		#region Custom Methods.

		#endregion

		#region Event Methods.

		#endregion
	}
}
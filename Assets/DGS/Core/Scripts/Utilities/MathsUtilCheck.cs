using System;
using System.Collections;
using System.Collections.Generic;
using DGS.Game.Proto.MadOverGames.Utilties;
using UnityEngine;

/*===============================================================
Product:    	Project Name: MadOverGames Assignment
Developer:  	Developer Name: Ankit Sethi - ankitsethi@dasagamestudio.com
Company:    	Company: DasaGame Studio
Created On:     Created On: 11/16/2019 1:06:46 AM
Modified On:    Modified On: 11/16/2019 1:06:46 AM
Copyright:  	Copyright: @ Copyright 2019-2020. All rights Reserved. DasaGame Studio
================================================================*/

namespace DGS.Game {
	public class MathsUtilCheck : MonoBehaviour {

		#region Variables.

		#endregion

		#region Unity Methods.

		private void Start() {
			var smallArray = MathUtils.GetRandomArray(10, -20, 20);
			var resultForSmallArray = MathUtils.GetPairsForSmallArray(smallArray, 0);
			Debug.Log($"Given Small Array: {string.Join(", ", smallArray)}, Required Sum: 0");
			if(resultForSmallArray.Count > 0) {
				foreach(var tuple in resultForSmallArray) {
					Debug.Log($"({tuple.Item1.ToString()}, {tuple.Item2.ToString()})");
				}
			} else {
				Debug.Log("No pair found with sum 0, for the above array. Please try again!");	
			}

			var largeArray = MathUtils.GetRandomArray(50, -100, 100);
			var resultForLargeArray = MathUtils.GetPairsForLargeArray(largeArray, 0);
			Debug.Log($"Given Large Array: {string.Join(", ", largeArray)}, Required Sum: 0");
			if(resultForLargeArray.Count > 0) {
				foreach(var tuple in resultForLargeArray) {
					Debug.Log($"({tuple.Item1.ToString()}, {tuple.Item2.ToString()})");
				}
			} else {
				Debug.Log("No pair found with sum 0, for the above array. Please try again!");	
			}

			largeArray = MathUtils.GetRandomArray(50, -100, 100);
			var resultForLargeArrayTriplets = MathUtils.GetTripletsForLargeArray(largeArray, 0);
			Debug.Log($"Given Large Array for Triplets: {string.Join(", ", largeArray)}, Required Sum: 0");
			if(resultForLargeArrayTriplets.Count > 0) {
				foreach(var tuple in resultForLargeArrayTriplets) {
					Debug.Log($"({tuple.Item1.ToString()}, {tuple.Item2.ToString()}, {tuple.Item3.ToString()})");
				}
			} else {
				Debug.Log("No triplets found with sum 0, for the above array. Please try again!");	
			}
		}

		#endregion

		#region Custom Methods.

		#endregion

		#region Event Methods.

		#endregion
	}
}
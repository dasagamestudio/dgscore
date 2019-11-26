using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

/*===============================================================
Product:    	Project Name: MadOverGames Assignment
Developer:  	Developer Name: Ankit Sethi - ankitsethi@dasagamestudio.com
Company:    	Company: DasaGame Studio
Created On:     Created On: 11/16/2019 12:40:50 AM
Modified On:    Modified On: 11/16/2019 12:40:50 AM
Copyright:  	Copyright: @ Copyright 2019-2020. All rights Reserved. DasaGame Studio
================================================================*/

namespace DGS.Game.Proto.MadOverGames.Utilties {
	public static class MathUtils {

		#region Variables.
		#endregion

		#region Custom Methods.

		/// <summary>
		/// For a given small array of integers, this finds the pairs whose sum is equal to the required sum value.
		/// Note: This method should only be used if array is reasonably small (Due to additional space required to store
		/// numbers in HashTable or Set). If the array is large, then its better to use , as its complexity is low
		/// and hence is much faster.
		/// </summary>
		/// <param name="numbers">Array of integers provided.</param>
		/// <param name="sumRequired">The required sum from the pair of integers.</param>
		/// <returns></returns>
		public static List<Tuple<int, int>> GetPairsForSmallArray(int[] numbers, int sumRequired) {
			var result = new List<Tuple<int, int>>(numbers.Length);

			if(numbers.Length < 2)
				return result;
			
			var newSet = new HashSet<int>();
			foreach(var val in numbers) {
				var resultVal = sumRequired - val;
				
				// If resulting value is not in the set then add it to the set.
				if(!newSet.Contains(resultVal))
					newSet.Add(val);
				else
					result.Add(new Tuple<int, int>(val, resultVal));
			}

			return result;
		}

		/// <summary>
		/// For a given large array of integers, this finds the pairs whose sum is equal to the required sum value.
		/// </summary>
		/// <param name="numbers">Array of integers provided.</param>
		/// <param name="sumRequired">The required sum from the pair of integers.</param>
		/// <returns></returns>
		public static List<Tuple<int, int>> GetPairsForLargeArray(int[] numbers, int sumRequired) {
			var result = new List<Tuple<int, int>>(numbers.Length);
			
			Array.Sort(numbers);
			
			// Using pointer technique to get bets in-place low complexity algorithm.
			var leftVal = 0;
			var rightVal = numbers.Length - 1;
			while(leftVal < rightVal) {
				var sumCheck = numbers[leftVal] + numbers[rightVal];
				if(sumCheck == sumRequired) {
					result.Add(new Tuple<int, int>(numbers[leftVal], numbers[rightVal]));

					leftVal += 1;
					rightVal -= 1;
				} else if(sumCheck < sumRequired) {
					leftVal += 1;
				} else if(sumCheck > sumRequired) {
					rightVal -= 1;
				}
			}

			return result;
		}
		
		/// <summary>
		/// For a given large array of integers, this finds the triplets whose sum is equal to the required sum value.
		/// As a + b + c = x, can be written as b + c = x - a, hence for each array element we can check for pairs
		/// using <see cref="GetPairsForLargeArray"/> method.
		/// </summary>
		/// <param name="numbers">Array of integers provided.</param>
		/// <param name="sumRequired">The required sum from the triplets of integers.</param>
		/// <returns></returns>
		public static List<Tuple<int, int, int>> GetTripletsForLargeArray(int[] numbers, int sumRequired) {
			var result = new List<Tuple<int, int, int>>(numbers.Length);
			
			Array.Sort(numbers);

			foreach(var t in numbers) {
				var tuplesWithTwoPairs = GetPairsForLargeArray(numbers, sumRequired - t);

				foreach(var tuple in tuplesWithTwoPairs) {
					result.Add(new Tuple<int, int, int>(t, tuple.Item1, tuple.Item2));
				}

				return result;
			}

			return result;
		}

		/// <summary>
		/// Returns random array of integers with length = <param name="arrayLength">,
		/// minimum array element range = <param name="minRange"> and
		/// maximum array element range = <param name="maxRange"></param></param></param>
		/// </summary>
		/// <param name="arrayLength">Length of the required random array.</param>
		/// <param name="minRange">Minimum range of any array element.</param>
		/// <param name="maxRange">Maximum range of any array element.</param>
		/// <returns></returns>
		public static int[] GetRandomArray(int arrayLength, int minRange, int maxRange) {
			var randomArray = new int[arrayLength];

			for(int i = 0; i < arrayLength; i++) {
				randomArray[i] = UnityEngine.Random.Range(minRange, maxRange);
			}

			return randomArray;
		}
		
		#endregion

		#region Event Methods.
		#endregion
	}
}
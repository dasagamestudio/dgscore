using System.Collections;
using System.Collections.Generic;
using UnityEditor;

/* Library Name: DGS Editorials
 * Author Name: Ankit Sethi
 * Script Name: EditorUtilities
 * Created On: 5/17/2018 11:10:44 PM
 * Modified On: 5/17/2018 11:10:44 PM
 * Copyright: @ Copyright 2018-2019. All rights Reserved. Dasa Game Studios
 */

namespace DGS.Game.DGSEditorials.Editors.Utilities {
	public static class EditorUtilities {

        /// <summary>
        /// Returns all assets of the wanted type.
        /// </summary>
        public static List<T> FindAssetsOfType<T>() where T : UnityEngine.Object {
            List<T> assets = new List<T>();
            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
            for(int i = 0; i < guids.Length; i++) {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if(asset != null) {
                    assets.Add(asset);
                }
            }

            return assets;
        }
    }
}
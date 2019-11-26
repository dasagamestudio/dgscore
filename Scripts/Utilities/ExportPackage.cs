using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/*===============================================================
Product:    	Project Name: MadOverGames Assignment
Developer:  	Developer Name: Ankit Sethi - ankitsethi@dasagamestudio.com
Company:    	Company: DasaGame Studio
Created On:     Created On: 11/17/2019 2:08:13 AM
Modified On:    Modified On: 11/17/2019 2:08:13 AM
Copyright:  	Copyright: @ Copyright 2019-2020. All rights Reserved. DasaGame Studio
================================================================*/

namespace DGS.Game.Core.Utils {
	public class ExportPackage {

		#region Variables.
		#endregion

		#region Unity Methods.
		#endregion

		#region Custom Methods.
		[MenuItem("DGS/Custom Export")]
		private static void Export() {
			AssetDatabase.ExportPackage (AssetDatabase.GetAssetPath(Selection.activeObject),PlayerSettings.productName + ".unitypackage",ExportPackageOptions.Interactive | ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies | ExportPackageOptions.IncludeLibraryAssets);
		}
		#endregion

		#region Event Methods.
		#endregion
	}
}
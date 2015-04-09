using UnityEngine;
using UnityEditor;

public class ModelImporterEdits : AssetPostprocessor {
	
	void OnPreprocessModel() {
		(assetImporter as ModelImporter).importMaterials = false;
		(assetImporter as ModelImporter).globalScale = 100;
	}
}

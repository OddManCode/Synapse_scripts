using UnityEngine;
using System;
using System.Collections;

public class ColorPicker : MonoBehaviour {

	public Texture2D colorPicker;
	public int locX = 0;
	public int locY = 0;
	public float imageScale = 1;
	public int xOff = 5;
	public int yOff = 0;
	public GameObject model;

	void OnGUI() {
		if (GUI.RepeatButton (new Rect (locX,locY,colorPicker.width*imageScale,colorPicker.height*imageScale), colorPicker)) {
			Vector2 pickPos = Event.current.mousePosition;
			Debug.Log (pickPos);
			int pxlX = Convert.ToInt32((pickPos.x-locX+xOff)/imageScale);
			int pxlY = Convert.ToInt32((pickPos.y-locY-yOff)/imageScale);
			Debug.Log (pxlX + " || " + pxlY);
			Color col = colorPicker.GetPixel(pxlX,-pxlY);
			
			model.GetComponent<Renderer>().materials[0].SetColor("_Color",col);
		}
	}
}

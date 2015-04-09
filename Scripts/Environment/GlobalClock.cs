using UnityEngine;
using System.Collections;

public class GlobalClock : MonoBehaviour {
	/* THIS CLASS HANDLES ALL TIME CALCULATIONS, ROTATES LIGHT SOURCES (SUNS AND MOONS), AND BLENDS SKYBOX TEXTURES
	 * REQUIRES THE SKYBOX BE A MATERIAL THAT HOLDS 2 SETS OF 6 TEXTURES (Up, North, South, East, West, Down) AND BLENDS BETWEEN THEM USING "_Blend"
	 */
	
	/* TO DO/TO FIX:	
	 * - not set skyboxtextures in every update (seems like a waste)
	 * - make a skybox material that just holds all 4 sets of textures (ie learn shader code... ugh...)
	 * - possibly merge/redistribute the LightToggle and CalculateAngle functions
	 * - handle Fog changes in a separate method... maybe
	 * - convert time in seconds into an HH:MM format (actual time)
	 */

	// As a percent, the time the clock starts at
	public float startTimePercent = 0;

	// How long an in-game day lasts in minutes (and seconds)
	public float fullCycleInMinutes = 1;
	private float fullCycleInSeconds;
	
	// How rapidly the day progresses
	public float timeScale = 1;

	// How much of the day is spent in light and darkness
	public float dayLengthPercent = 50;

	// The threshold percentage before the skybox begins blending out of that time of day
	public int changePercent = 10;
	public int eveningChangePercent = 10;
	public int morningChangePercent = 10;
	public int dayChangePercent = 15;
	public int nightChangePercent = 2;

	// Constants and other boring things
	private int dayCount = 0;
	public readonly string[] dayNames = {"Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"};

	private const float SECOND = 1;
	private const float MINUTE = 60 * SECOND;
	private const float HOUR = 60 * MINUTE;
	private const float DAY = 24 * HOUR;

	// The angle for the sun/moon rotations
	private float angle;

	// Current time and its value as a percentage of the full cycle
	private float currentTime;
	private float cyclePercent;

	// Arrays for the skybox textures
	public Texture2D[] nightSkyboxTextures = new Texture2D[6];
	public Texture2D[] morningSkyboxTextures = new Texture2D[6];
	public Texture2D[] daySkyboxTextures = new Texture2D[6];
	public Texture2D[] eveningSkyboxTextures = new Texture2D[6];
	
	// Arrays for light sources
	public Transform[] suns = new Transform[1];
	public Transform[] moons = new Transform[1];

	// Fog Stats
	public Color nightFogColor;
	public Color morningFogColor;
	public Color dayFogColor;
	public Color eveningFogColor;

	public float nightFogStartDistance;
	public float nightFogEndDistance;

	public float morningFogStartDistance;
	public float morningFogEndDistance;

	public float dayFogStartDistance;
	public float dayFogEndDistance;

	public float eveningFogStartDistance;
	public float eveningFogEndDistance;




	void Start () {
		Mathf.Clamp (dayLengthPercent, 10, 90);
		Mathf.Clamp (changePercent, 1, 45);

		currentTime = (startTimePercent/100) * fullCycleInMinutes * MINUTE;
	}

	void Update () {
		fullCycleInSeconds = fullCycleInMinutes * MINUTE;
		cyclePercent = Mathf.Floor(currentTime / fullCycleInSeconds * 100);

		CalculateCurrentTime();
		CalculateRotationAngle();
		SkyboxBlending();
		LightToggle();
		
		//Debug.Log ("CURRENT TIME: " + currentTime + "ROTATION: " + degreeRotation + "/sec || PERCENT: " + cyclePercent + "%");
	}

	void CalculateCurrentTime () {
		// CALCULATES THE CURRENT TIME OF THE DAY, ADJUSTED FOR POSITIVE AND NEGATIVE TIMESCALE
		currentTime += Time.deltaTime * timeScale;

		if (currentTime > fullCycleInSeconds) {
			dayCount++;
			SetDayOfWeek (dayCount);
		}
		if (currentTime < 0) {
			dayCount--;
			SetDayOfWeek (dayCount);
		}
		currentTime = Mathf.Repeat(currentTime, fullCycleInSeconds);
	}

	public void SetDayOfWeek (int num) {
		// CALCULATES THE DAY OF THE WEEK
		int dayNum = (int) Mathf.Repeat(num, dayNames.Length);
		//Debug.Log ("dayNum: " + dayNum + " || num: " + num);
		Debug.Log ("TODAY IS: " + dayNames[dayNum]);
	}

	void CalculateRotationAngle () {
		// CALCULATES THE ANGLE OF THE SUN(S)/MOON(S)
		float dayDuration = fullCycleInSeconds * (dayLengthPercent/100);
		float nightDuration = fullCycleInSeconds - dayDuration;

		float sunrise = nightDuration/2;
		float sunset = fullCycleInSeconds - sunrise;

		if (currentTime < sunrise) {
			// angle < 90 | night -> morning
			angle = 90 * (currentTime / sunrise);
			//Debug.Log ("----before sunrise");
		}
		else if (currentTime > sunset) {
			// angle > 270 | evening -> night
			angle = 270 + 90 * ((currentTime - sunset) / sunrise);
			//Debug.Log ("after sunset----");
		}
		else {
			// 90 < angle < 270 | morning -> day -> evening
			angle = 90 + 180 * ((currentTime - sunrise) / dayDuration);
			//Debug.Log ("--daylight--");
		}
		//Debug.Log ("ROTATION: " + angle);
		//Debug.Log ("CURRENT TIME: " + currentTime + " || PERCENT: " + cyclePercent + "%");

		foreach (Transform sun in suns) {
			sun.transform.rotation = Quaternion.Euler (270 + angle, 0, 0);
		}
		foreach (Transform moon in moons) {
			moon.transform.rotation = Quaternion.Euler (90 + angle, 0, 0);
		}

		// Use this if all light sources are parented to this object
		//transform.rotation = Quaternion.Euler (0, 0, angle);
	}

	void SkyboxBlending () {
		// BLENDS BETWEEN TWO SETS OF SKYBOX TEXTURES DEPENDING ON THE 
		float temp;
		float changeThreshold = 180 * changePercent/100f;
		float eveningChangeThreshold = 180 * eveningChangePercent/100f;
		float morningChangeThreshold = 180 * morningChangePercent/100f;
		float dayChangeThreshold = 180 * dayChangePercent/100f;
		float nightChangeThreshold = 180 * nightChangePercent/100f;

		// If it is during the transition from night to day
		if (angle >= 90 - nightChangeThreshold && angle <= 90 + morningChangeThreshold) {
			// If it is a night-to-morning angle
			if (angle < 90) {
				// Blend between night and morning variables
				SetSkyboxTextures (nightSkyboxTextures, morningSkyboxTextures);
				temp = 1 - Mathf.Abs(90 - angle) / nightChangeThreshold;
				RenderSettings.fogColor = Color.Lerp (nightFogColor, morningFogColor, temp);
				RenderSettings.fogStartDistance = Mathf.Lerp (nightFogStartDistance, morningFogStartDistance, temp);
				RenderSettings.fogEndDistance = Mathf.Lerp (nightFogEndDistance, morningFogEndDistance, temp);
			}
			// Otherwise is it a morning-to-day angle
			else {
				// Blend between morning and day variables
				SetSkyboxTextures (morningSkyboxTextures, daySkyboxTextures);
				temp = Mathf.Abs(90 - angle) / morningChangeThreshold;
				RenderSettings.fogColor = Color.Lerp (morningFogColor, dayFogColor, temp);
				RenderSettings.fogStartDistance = Mathf.Lerp (morningFogStartDistance, dayFogStartDistance, temp);
				RenderSettings.fogEndDistance = Mathf.Lerp (morningFogEndDistance, dayFogEndDistance, temp);
			}
			RenderSettings.skybox.SetFloat ("_Blend", temp);
		}
		// If it is during the transition from day to night
		else if (angle >= 270 - dayChangeThreshold && angle <= 270 + eveningChangeThreshold) {
			// If it is a day-to-evening angle
			if (angle < 270) {
				// Blend between day and evening variables
				SetSkyboxTextures (daySkyboxTextures, eveningSkyboxTextures);
				temp = 1 - Mathf.Abs(270 - angle) / dayChangeThreshold;
				RenderSettings.fogColor = Color.Lerp (dayFogColor, eveningFogColor, temp);
				RenderSettings.fogStartDistance = Mathf.Lerp (dayFogStartDistance, eveningFogStartDistance, temp);
				RenderSettings.fogEndDistance = Mathf.Lerp (dayFogEndDistance, eveningFogEndDistance, temp);
			}
			// Otherwise it is an evening-to-night angle
			else {
				// Blend between evening and night variables
				SetSkyboxTextures (eveningSkyboxTextures, nightSkyboxTextures);
				temp = Mathf.Abs(270 - angle) / eveningChangeThreshold;
				RenderSettings.fogColor = Color.Lerp (eveningFogColor, nightFogColor, temp);
				RenderSettings.fogStartDistance = Mathf.Lerp (eveningFogStartDistance, nightFogStartDistance, temp);
				RenderSettings.fogEndDistance = Mathf.Lerp (eveningFogEndDistance, nightFogEndDistance, temp);
			}
			RenderSettings.skybox.SetFloat ("_Blend", temp);
		}
		else {
			SetSkyboxTextures (nightSkyboxTextures, daySkyboxTextures);
			if (angle < 90 || angle > 270) {
				RenderSettings.skybox.SetFloat ("_Blend", 0);
				RenderSettings.fogColor = nightFogColor;
				RenderSettings.fogStartDistance = nightFogStartDistance;
				RenderSettings.fogEndDistance = nightFogEndDistance;
			}
			else {
				RenderSettings.skybox.SetFloat ("_Blend", 1);
				RenderSettings.fogColor = dayFogColor;
				RenderSettings.fogStartDistance = dayFogStartDistance;
				RenderSettings.fogEndDistance = dayFogEndDistance;
			}
		}
	}

	void SetSkyboxTextures (Texture2D[] fromTextures, Texture2D[] toTextures) {
		// REPLACES THE TWO SETS OF SKYBOX TEXTURES
		RenderSettings.skybox.SetTexture("_UpTex", fromTextures[0]);
		RenderSettings.skybox.SetTexture("_NorthTex", fromTextures[1]);
		RenderSettings.skybox.SetTexture("_SouthTex", fromTextures[2]);
		RenderSettings.skybox.SetTexture("_EastTex", fromTextures[3]);
		RenderSettings.skybox.SetTexture("_WestTex", fromTextures[4]);
		RenderSettings.skybox.SetTexture("_DownTex", fromTextures[5]);
		
		RenderSettings.skybox.SetTexture("_UpTex2", toTextures[0]);
		RenderSettings.skybox.SetTexture("_NorthTex2", toTextures[1]);
		RenderSettings.skybox.SetTexture("_SouthTex2", toTextures[2]);
		RenderSettings.skybox.SetTexture("_EastTex2", toTextures[3]);
		RenderSettings.skybox.SetTexture("_WestTex2", toTextures[4]);
		RenderSettings.skybox.SetTexture("_DownTex2", toTextures[5]);
	}
	
	void LightToggle () {
		// TOGGLES ON/OFF THE SUN(S)/MOON(S) DURING NIGHTTIME/DAYLIGHT HOURS
		// If it is daytime
		if (angle > 95 && angle < 265) {
			// turn on sun lights
			foreach (Transform sun in suns) {
				Light light = sun.GetComponent<Light>();
				if (light != null && light.enabled == false) {
					light.enabled = true;
				}
			}
			// turn off moon lights
			foreach (Transform moon in moons) {
				Light light = moon.GetComponent<Light>();
				if (light != null && light.enabled == true) {
					light.enabled = false;
				}
			}
		}
		else {
			// turn off sun lights
			foreach (Transform sun in suns) {
				Light light = sun.GetComponent<Light>();
				if (light != null && light.enabled == true) {
					light.enabled = false;
				}
			}
			// turn on moon lights
			foreach (Transform moon in moons) {
				Light light = moon.GetComponent<Light>();
				if (light != null && light.enabled == false) {
					light.enabled = true;
				}
			}
		}
	}
}

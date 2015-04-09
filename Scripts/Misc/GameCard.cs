using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class GameCard : MonoBehaviour {

	public Sprite[] cardBacks = new Sprite[3];

	public CanvasGroup tooltip;

	public Image iconImg;
	public CanvasGroup descBoxGrp;
	public CanvasGroup titleBarGrp;
	private Text titleBarTxt;
	private Text descBoxTxt;
	
	private RectTransform rect;
	private Vector3 normalScale;
	private Vector3 normalPos;
	private Color normalDescTextColor;
	public bool showTip = false;

	// Variables that remain constant;
	public int id;
	public string title;
	public string imageName;
	public string modelName;
	public string description;
	
	public int category;
	public int type;
	public int rarity;
	
	// Base variables most often manipulated via other Scripts; 'Primary Stats'
	public int baseAlignment;
	public int basePower;
	public int baseRange;
	public int baseDuration;
	
	// Additional variables; cast time, combo chain/clip size, reload time, downtime
	public int baseDelay;
	public int baseCapacity;
	public int baseRecovery;
	public int baseCooldown;
	
	// Tags used to determine which other Cards this Card can combine with
	public int[] groupIDs;
	
	// If the Card has a unique special effect, this refers to which Script it calls
	// Possibly, eventually, remove the desc field if text from the Script can be pulled.
	public int scriptID;
	
	public bool compatible;
	public bool onCooldown = false;
	public bool isActiveCard = false;
	
	private string pwrString;
	private string rgeString;
	private string dtnString;
	
	private int modPower;
	private int modRange;
	private int modDuration;
	
	protected int modDelay;
	protected int modCapacity;
	protected int modRecovery;
	protected int modCooldown;
	
	public List<int> rawPowerMods = new List<int>();
	public List<int> powerMultipliers = new List<int>();
	
	public List<int> rawRangeMods = new List<int>();
	public List<int> rangeMultipliers = new List<int>();
	
	public List<int> rawDurationMods = new List<int>();
	public List<int> durationMultipliers = new List<int>();
	
	public List<int> rawDelayMods = new List<int>();
	public List<int> delayMultipliers = new List<int>();
	
	public List<int> rawCapacityMods = new List<int>();
	public List<int> capacityMultipliers = new List<int>();
	
	public List<int> rawRecoveryMods = new List<int>();
	public List<int> recoveryMultipliers = new List<int>();
	
	public List<int> rawCooldownMods = new List<int>();
	public List<int> cooldownMultipliers = new List<int>();
	
	protected bool canCast = false;
	protected bool casting = false;
	
	private int capacityCount = 0;
	private float delayTimer = 0;
	private float recoveryTimer = 0;
	private float cooldownTimer = 0;

	public GameCard(int _id, string _title, string _imageName, string _modelName, string _description,
	            int _category, int _type, int _rarity,int _baseAlignment,
	            int _basePower, int _baseRange, int _baseDuration,
	            int _baseDelay, int _baseCapacity, int _baseRecovery,int _baseCooldown,
	            string _rawGroupIDs, int _scriptID) {
		id = _id;
		title = _title;
		imageName = _imageName;
		modelName = _modelName;
		description = _description;
		category = _category;
		type = _type;
		rarity = _rarity;
		baseAlignment = _baseAlignment;
		basePower = _basePower;
		baseRange = _baseRange;
		baseDuration = _baseDuration;
		baseDelay = _baseDelay;
		baseCapacity = _baseCapacity;
		baseRecovery = _baseRecovery;
		baseCooldown = _baseCooldown;
		groupIDs = Array.ConvertAll(_rawGroupIDs.Split(','), new Converter<string, int>(int.Parse));
		scriptID = _scriptID;
		
		//Debug.Log ("Card Initialized >> " + title);
	}

	void Start() {
		pwrString = "#Pwr";
		rgeString = "#Rge";
		dtnString = "#Dtn";

		tooltip = transform.FindChild("Tooltip").GetComponent<CanvasGroup>();
		rect = tooltip.GetComponent<RectTransform>();

		Text[] textBoxes = gameObject.GetComponentsInChildren<Text>();
		foreach (Text textBox in textBoxes) {
			if (textBox.name == "Title") {
				titleBarTxt = textBox;
			}
			else if (textBox.name == "Description") {
				descBoxTxt = textBox;
			}
		}
		titleBarTxt.text = title;
		iconImg.sprite = Resources.Load("Cards/Icons/" + imageName, typeof(Sprite)) as Sprite;

		normalScale = rect.localScale;
		normalPos = rect.localPosition;
		normalDescTextColor = descBoxTxt.color;
	}
	
	void Update() {
		if (isActiveCard) {
			if (casting) {
				DelayTracker();
			}
			else {
				RecoveryTracker();
			}
			if (Input.GetButtonDown("Use Active Card") && canCast) {
				casting = true;
				canCast = false;
			}
		}
		else {
			if (cooldownTimer <= modCooldown) {
				cooldownTimer += Time.deltaTime * 1000;
			}
			else {
				onCooldown = false;
			}
		}

		if (showTip) {
			tooltip.alpha = Mathf.Lerp(tooltip.alpha, 1.0f, Time.deltaTime * 8);
			rect.localScale = Vector3.Lerp(rect.localScale, new Vector3(2f, 2f, 2f), Time.deltaTime * 8);
			tooltip.interactable = true;
		}
		else {
			tooltip.alpha = Mathf.Lerp(tooltip.alpha, 0.0f, Time.deltaTime * 8);
			rect.localScale = Vector3.Lerp(rect.localScale, new Vector3(1f, 1f, 1f), Time.deltaTime * 8);
			tooltip.interactable = false;
		}

		/*
		if (showTip) {
			rect.localScale = Vector3.Lerp(rect.localScale, new Vector3(0.7f, 0.7f, 0.7f), Time.deltaTime * 8);
			rect.localPosition = Vector3.Lerp(rect.localPosition, Vector3.zero, Time.deltaTime * 8);
			descBoxGrp.alpha = Mathf.Lerp(descBoxGrp.alpha, 1.0f, Time.deltaTime * 8);
			titleBarGrp.alpha = Mathf.Lerp(titleBarGrp.alpha, 1.0f, Time.deltaTime * 8);
		}
		else {
			rect.localScale = Vector3.Lerp(rect.localScale, normalScale, Time.deltaTime * 8);
			rect.localPosition = Vector3.Lerp(rect.localPosition, normalPos, Time.deltaTime * 8);
			descBoxGrp.alpha = Mathf.Lerp(0.0f, descBoxGrp.alpha, Time.deltaTime * 8);
			titleBarGrp.alpha = Mathf.Lerp(0.0f, titleBarGrp.alpha, Time.deltaTime * 8);
		}*/
		UpdateStats();
	}

	public void FlipTip() {
		showTip = !showTip;
	}

	public void NoTip() {
		showTip = false;
	}

	public void ShowTip() {
		showTip = true;
	}

	void CapacityTracker() {
		if (capacityCount >= modCapacity) {
			canCast = false;
			recoveryTimer = modRecovery;
		}
	}

	void RecoveryTracker() {
		recoveryTimer += Time.deltaTime * 1000;
		if (capacityCount < modCapacity) {
			canCast = true;
		}
		else {
			canCast = false;

		}
		if (recoveryTimer > modRecovery) {
			capacityCount = 0;
			canCast = true;
			recoveryTimer -= modRecovery;
		}
	}

	void DelayTracker() {
		if (delayTimer <= modDelay) {
			delayTimer += Time.deltaTime * 1000;
		}
		else {
			Cast();
		}
	}

	public void Cast() {
		Debug.Log ("Casted");
		delayTimer = 0;
		recoveryTimer = 0;
		capacityCount++;
		casting = false;
	}

	void UpdateStats() {
		modPower = CalculateMods(basePower, rawPowerMods, powerMultipliers);
		modRange = CalculateMods(baseRange, rawRangeMods, rangeMultipliers);
		modDuration = CalculateMods(baseDuration, rawDurationMods, durationMultipliers);
		
		modDelay = CalculateMods(baseDelay, rawDelayMods, delayMultipliers);
		modCapacity = CalculateMods(baseCapacity, rawCapacityMods, capacityMultipliers);
		modRecovery = CalculateMods(baseRecovery, rawRecoveryMods, recoveryMultipliers);
		modCooldown = CalculateMods(baseCooldown, rawCooldownMods, cooldownMultipliers);

		if (description != null || description.Length > 1) {
			descBoxTxt.text = ParseDescription();
		}
	}
	
	int CalculateMods(int baseStat, List<int> rawMods, List<int> multipliers) {
		return Mathf.RoundToInt((baseStat + rawMods.Sum()) * (1.0f + (multipliers.Sum()/100.0f)));
	}
	
	private string ParseDescription() {
		string edit;
		edit = Regex.Replace(description, pwrString, ColorStat(modPower, basePower, normalDescTextColor), RegexOptions.IgnoreCase);
		edit = Regex.Replace(edit, rgeString, ColorStat(modRange, baseRange, normalDescTextColor), RegexOptions.IgnoreCase);
		edit = Regex.Replace(edit, dtnString, ColorStat(modDuration, baseDuration, normalDescTextColor), RegexOptions.IgnoreCase);
		return edit;
	}

	private string ColorStat(int moddedStat, int baseStat, Color defaultColor) {
		string colorString = "<color=#" + ColorToHex(defaultColor) + ">";
		if (moddedStat > baseStat) {
			colorString = "<color=#0BE040>";
		}
		else if (moddedStat < baseStat) {
			colorString = "<color=#970707>";
		}
		return String.Concat(colorString, moddedStat.ToString(), "</color>");
	}

	string ColorToHex(Color32 color)
	{
		string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
		return hex;
	}
	
	Color HexToColor(string hex)
	{
		byte r = byte.Parse(hex.Substring(0,2), System.Globalization.NumberStyles.HexNumber);
		byte g = byte.Parse(hex.Substring(2,2), System.Globalization.NumberStyles.HexNumber);
		byte b = byte.Parse(hex.Substring(4,2), System.Globalization.NumberStyles.HexNumber);
		return new Color32(r,g,b, 255);
	}
}

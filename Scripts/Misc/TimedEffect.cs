using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class TimedEffect : IComparable<TimedEffect> {

	public enum Category {Buff, Debuff}
	public enum Type {HoT, DoT, PulseHoT, PulseDoT}

	public int pwr;
	public float dtn;
	private Category category;
	private Type type;

	public TimedEffect (int _pwr, int _dtn, Category _category, Type _type) {
		pwr = _pwr;
		dtn = (float)_dtn;
		category = _category;
		type = _type;
	}

	public Category GetEffectCategory() {
		return category;
	}

	public Type GetEffectType() {
		return type;
	}

	void Update() {
		dtn -= Time.deltaTime;
		if (dtn <= 0) {

		}
	}

	public int CompareTo(TimedEffect other) {
		if(other == null) {
			return 1;
		}
		int thisValue = pwr*(int)dtn;
		int otherValue = other.pwr*(int)other.dtn;
		return thisValue.CompareTo(otherValue);
	}
}

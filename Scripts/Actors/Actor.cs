using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent (typeof(Rigidbody))]
[RequireComponent(typeof(NetworkView))]

public class Actor : MonoBehaviour {

	public float maxHealth;
	public float health;
	public int team;
	private bool alive = true;

	public List<TimedEffect> buffs = new List<TimedEffect>();
	public List<TimedEffect> debuffs = new List<TimedEffect>();

	public List<int> rawReductionMods = new List<int>();
	public List<int> reductionMultipliers = new List<int>();

	public List<int> rawHealingMods = new List<int>();
	public List<int> healingMultipliers = new List<int>();

	public Rigidbody rbody;

	void Start() {
		rbody = GetComponent<Rigidbody>();
	}

	[RPC]
	public void TakeDamage(int incomingDamage) {
		int damageDealt = Mathf.FloorToInt((incomingDamage - rawReductionMods.Sum()) * (1.0f - (reductionMultipliers.Sum()/100.0f)));
		health -= damageDealt;
	}

	[RPC]
	public void GetHealing(int incomingHealing) {
		int healthRestored = Mathf.FloorToInt((incomingHealing + rawHealingMods.Sum()) * (1.0f + (healingMultipliers.Sum()/100.0f)));
		health = Mathf.Clamp(health, health + healthRestored, maxHealth);
	}
	
	[RPC]
	public void AddTimedEffect(TimedEffect effect) {
		if (effect.GetEffectCategory() == TimedEffect.Category.Buff) {
			if (buffs.Count < GlobalVars.MAX_BUFFS) {
				buffs.Add(effect);
			}
		}
		if (effect.GetEffectCategory() == TimedEffect.Category.Debuff) {
			if (debuffs.Count < GlobalVars.MAX_DEBUFFS) {
				Debug.Log ("Stuff");
			}
		}
	}

	void CalculateOverTime(){
		foreach (TimedEffect buff in buffs) {
			if (buff.dtn <= 0) {
				buffs.Remove(buff);
			}
		}
		buffs.Sort();
		foreach (TimedEffect debuff in debuffs) {
			if (debuff.dtn <= 0) {
				debuffs.Remove(debuff);
			}
		}
		debuffs.Sort();
	}

	void Update() {
		CalculateOverTime();

		if (health <= 0 && alive) {
			alive = false;
			Debug.Log("Dead");
		}
	}
}

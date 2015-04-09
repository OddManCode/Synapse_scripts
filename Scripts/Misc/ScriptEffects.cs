using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(NetworkView))]

public class ScriptEffects : MonoBehaviour {

	public enum EffectArchetypes {Blink, DoT, HoT, Stealth, Stun}

	//private enum scriptID {Setup, ServerStart, ServerConnect, ServerDisconnect, Login, Account, DeckEditor, Blank};
	private enum RadiusType {Lateral, Dimensional};
	private enum faction {Ally, Enemy, Neutral};

	int actorLayerMask;
	int nonActorLayerMask;

	void Start() {
	//Get the Layer mask of the Actors Layer and invert it for checking non-Actor collisions.
		actorLayerMask = 1<<LayerMask.NameToLayer("Actors");
		nonActorLayerMask = ~actorLayerMask;
	}

	void AoEDamage(int pwr, int rge, int radius, RadiusType area, Vector3 location, Actor source) {
		foreach (Actor target in GetTargetsInArea(rge, radius, location, source.team)) {
			DealDamage(pwr, target);
		}
	}

	void AoEHeal(int pwr, int rge, int radius, Vector3 location, Actor source) {
		foreach (Actor target in GetTargetsInArea(rge, radius, location, source.team)) {
			Heal(pwr, target);
		}
	}

	List<Actor> GetTargetsInArea(int rge, int radius, Vector3 location, int teamNum){
		List<Actor> targetsList = new List<Actor>();
		Collider[] hitTargets = Physics.OverlapSphere(location, radius, actorLayerMask);
		foreach (Collider actorColl in hitTargets) {
			Actor hitActor = actorColl.GetComponent<Actor>();
			if (hitActor.team == teamNum) {
				targetsList.Add(hitActor);
			}
		}
		return targetsList;
	}

	void DealDamage(int damage, Actor target) {
		GetComponent<NetworkView>().RPC("TakeDamage", target.GetComponent<NetworkView>().owner, damage);
	}

	void Heal(int healing, Actor target) {
		GetComponent<NetworkView>().RPC("GetHealing", target.GetComponent<NetworkView>().owner, healing);
	}

	void CreateObject(GameObject go, int count, int maxCount, int health, int dtn) {
	}

	void OnHit(Actor target) {

	}

	void ApplyHoT(int pwr, int dtn, Actor target) {
		TimedEffect newBuff = new TimedEffect(pwr, dtn, TimedEffect.Category.Buff, TimedEffect.Type.DoT);
		GetComponent<NetworkView>().RPC("AddTimedEffect", target.GetComponent<NetworkView>().owner, newBuff);
	}

	void ApplyDoT(int pwr, int dtn, Actor target) {
		TimedEffect newDebuff = new TimedEffect(pwr, dtn, TimedEffect.Category.Debuff, TimedEffect.Type.DoT);
		GetComponent<NetworkView>().RPC("AddTimedEffect", target.GetComponent<NetworkView>().owner, newDebuff);
	}

	void Stun(int dtn) {

	}

	void Teleport(int rge, Actor target, Ray rayCast) {
		//Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

		RaycastHit hit;
		//If there is a non-Actor collider in the way of the target location, move to the position of the collision
		if (Physics.Raycast(rayCast, out hit, rge, nonActorLayerMask)) {
			target.transform.position = rayCast.GetPoint(hit.distance);
		}
		//Otherwise move to the position
		else {
			target.transform.position = rayCast.GetPoint(rge);
		}

	}

	void Launch(int pwr, Actor target){

	}



	void LifeSiphon(int pwr, Actor target) {

	}
}

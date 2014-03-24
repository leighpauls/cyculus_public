using UnityEngine;
using System.Collections;

public class Tree_Collider : MonoBehaviour {
	void OnTriggerEnter(Collider otherObject) {
		if (otherObject.tag == "Tree") {
			Debug.Log(string.Format("Currently hitting tree at x={0} y={1} z={2}", 
			                        otherObject.transform.position.x, 
			                        otherObject.transform.position.y, 
			                        otherObject.transform.position.z));
		}
	}
}

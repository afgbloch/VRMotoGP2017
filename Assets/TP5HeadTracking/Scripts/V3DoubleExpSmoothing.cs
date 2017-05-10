using UnityEngine;
using System.Collections;

[System.Serializable]
public class V3DoubleExpSmoothing {
	public bool initialized { get; protected set;}
	Vector3 lastSpt;
	Vector3 lastSpt2;
	[SerializeField] 	 	
	public float alpha = 0.1f;
	int timeIntervals = 0;
	
	public V3DoubleExpSmoothing(float newalpha){
		initialized = false;
		alpha = newalpha;
	}

	public V3DoubleExpSmoothing(){
		initialized = false;
	}	

	// update alpha and the double exponential smoothing predictor variables
	public void UpdateModel (Vector3 current, float newalpha){
		alpha = newalpha;
		UpdateModel (current);
	}
	
	// update the double exponential smoothing predictor variables
	public void UpdateModel (Vector3 current){
		if (!initialized) {
			lastSpt = lastSpt2 = current;
			initialized = true;
		}
		// compute new exponential smoothing variables
		Vector3 spt = Vector3.Lerp (lastSpt, current, alpha);
		Vector3 spt2 = Vector3.Lerp (lastSpt2, spt, alpha);
		
		// keep these new values
		lastSpt = spt;
		lastSpt2 = spt2;
		timeIntervals = 0;
	}
	
	public Vector3 StepPredict (){
		timeIntervals++;
		return Predict (timeIntervals);
	}

	// predict the outcome for # time intervals
	public Vector3 Predict (int deltaTime){
		if (!initialized)
			return Vector3.zero;
		
		float fraction = (alpha * deltaTime) / (1 - alpha);
		return (2 + fraction) * lastSpt - (1 + fraction) * lastSpt2;
	}
	
	// predict the outcome for #.# time intervals
	public Vector3 Predict (float deltaTime){
		if (!initialized)
			return Vector3.zero;
		
		// interpolate between two time intervals
		float floorDT = Mathf.Floor (deltaTime);
		
		Vector3 phi = Predict ((int)Mathf.Ceil (deltaTime));
		Vector3 plo = Predict ((int)floorDT);
		
		return (phi - plo) * (deltaTime - floorDT) + plo;
	}
}
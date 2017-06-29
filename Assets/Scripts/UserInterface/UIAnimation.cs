using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIAnimation : MonoBehaviour {

	[System.Serializable]
	public struct CA_UIElement{

		public GameObject UIObject;
		public Vector3 animationDirection;

		[HideInInspector]
		public float _distanceFromAnimatedPosition;

		[HideInInspector]
		public float animationSpeed;
		[HideInInspector]
		public float animationEndTime;

		[HideInInspector]
		public Color _initialColor;

		[HideInInspector]
		public Vector3 _initialPosition;
		[HideInInspector]
		public Vector3 _animatedPosition;
		[HideInInspector]
		public Vector3 _initialScaling;
		[HideInInspector]
		public Vector3 _animatedScaling;

		[HideInInspector]
		public Vector3 _scalingDifference;
		[HideInInspector]
		public RectTransform _rectTransform;
	}

	[System.Serializable]
	public struct CA_UIAnimation
	{
		public string animationName;
		[RangeAttribute(0,10)]
		public float animationDuration;

		[HeaderAttribute("---------------")]
		[HeaderAttribute("UIAnimationType:")]
		public bool loopAnimation;
		public bool asyncAnimation;
		[RangeAttribute(0,100)]
		public float asyncDelayReduction;

		[HeaderAttribute("---------------")]
		public bool appearingAnimation;
		public bool disappearingAnimation;

		[HeaderAttribute("---------------")]
		[HeaderAttribute("UIAnimationEffect:")]
		public bool fadingInEffect;
		public bool fadingOutEffect;

		[HeaderAttribute("---------------")]
		public bool positiveScalingEffect;
		public bool negetiveScalingEffect;

		[HeaderAttribute("---------------")]
		[HeaderAttribute("Scale Percentage *X:")]
		[RangeAttribute(1,10)]
		public float scalePercentage;

		[HeaderAttribute("---------------")]
		public bool rotationEffect;
		[RangeAttribute(-360,360)]
		public float[] degreeOfRotation;
		[HideInInspector]
		public float _totalRotation;
		[HideInInspector]
		public int _previousCheckpointIndex;
		[HideInInspector]
		public float[] _rotationCheckPoints;
		[HideInInspector]
		public float _previousAbsoluteRotation;


		[HeaderAttribute("-----------")]
		public CA_UIElement[] UIElements;

		public UnityEvent OnAnimationStartEvent;
		public UnityEvent OnAnimationEndEvent;

		[HideInInspector]
		public bool isAnimationRunning;
	}

	public UnityEvent OnStartSceneAnimation;
	public CA_UIAnimation[] UIAnimationPhase;
	

	// Use this for initialization
	void Start () {

		OnStartSceneAnimation.Invoke ();
	}
	
	// Update is called once per frame
	void Update () {

		mAnimate ();
	}

	#region PUBLIC METHOD

	public void ExecuteAnimation(string AnimationName){

		int mAnimationIndex = mGetAnimationIndex (AnimationName);

		UIAnimationPhase [mAnimationIndex].OnAnimationStartEvent.Invoke ();
		mVariableInitialization (mAnimationIndex);

		for (int i = 0; i < UIAnimationPhase [mAnimationIndex].UIElements.Length; i++) {

			// If : The GameObject is not active in heirarchy
			if (!UIAnimationPhase [mAnimationIndex].UIElements [i].UIObject.activeInHierarchy)
				UIAnimationPhase [mAnimationIndex].UIElements [i].UIObject.SetActive (true);

			// If : Animation End Time.
			if (UIAnimationPhase [mAnimationIndex].asyncAnimation) {
				if (i != 0) {

					UIAnimationPhase [mAnimationIndex].UIElements [i].animationEndTime = UIAnimationPhase [mAnimationIndex].UIElements [i - 1].animationEndTime 
						+ UIAnimationPhase [mAnimationIndex].animationDuration
						- ((UIAnimationPhase[mAnimationIndex].animationDuration * UIAnimationPhase[mAnimationIndex].asyncDelayReduction)/100.0f);
				}else
					UIAnimationPhase [mAnimationIndex].UIElements [i].animationEndTime = Time.time + UIAnimationPhase [mAnimationIndex].animationDuration;

			}else
				UIAnimationPhase [mAnimationIndex].UIElements [i].animationEndTime = Time.time + UIAnimationPhase [mAnimationIndex].animationDuration;

			// If : Appearing Animation    -> From AnimationPosition -> InitialPosition
			// If : Disappearing Animation -> From InitialPosition -> AnimationPosition
			if (UIAnimationPhase [mAnimationIndex].appearingAnimation) 
				UIAnimationPhase [mAnimationIndex].UIElements [i]._rectTransform.localPosition = UIAnimationPhase [mAnimationIndex].UIElements [i]._animatedPosition;
			else if (UIAnimationPhase [mAnimationIndex].disappearingAnimation)
				UIAnimationPhase [mAnimationIndex].UIElements [i]._rectTransform.localPosition = UIAnimationPhase [mAnimationIndex].UIElements [i]._initialPosition;

			// If : The FadingInEffect Is Enabled, Making the Opacity = 0
			if (UIAnimationPhase [mAnimationIndex].fadingInEffect) {

				if (UIAnimationPhase [mAnimationIndex].UIElements [i].UIObject.GetComponent<Text> ()) {
					UIAnimationPhase [mAnimationIndex].UIElements [i].UIObject.GetComponent<Text> ().color = 
						new Color(UIAnimationPhase [mAnimationIndex].UIElements [i]._initialColor.r,
							UIAnimationPhase [mAnimationIndex].UIElements [i]._initialColor.g,
							UIAnimationPhase [mAnimationIndex].UIElements [i]._initialColor.b,
							0.0f);
				} else if (UIAnimationPhase [mAnimationIndex].UIElements [i].UIObject.GetComponent<Image> ()) {
					UIAnimationPhase [mAnimationIndex].UIElements [i].UIObject.GetComponent<Image> ().color = 
						new Color(UIAnimationPhase [mAnimationIndex].UIElements [i]._initialColor.r,
							UIAnimationPhase [mAnimationIndex].UIElements [i]._initialColor.g,
							UIAnimationPhase [mAnimationIndex].UIElements [i]._initialColor.b,
							0.0f);
				} 
			}
			// Taking Initial Scaling
			UIAnimationPhase [mAnimationIndex].UIElements [i]._initialScaling = UIAnimationPhase [mAnimationIndex].UIElements [i]._rectTransform.localScale;
		}
		UIAnimationPhase[mAnimationIndex].isAnimationRunning = true;

	}

	public void ResetAnimation(string mAnimationName){

		for (int i = 0; i < UIAnimationPhase.Length; i++) {

			if (UIAnimationPhase [i].animationName == mAnimationName) {

				UIAnimationPhase [i].isAnimationRunning = false;

				for (int j = 0; j < UIAnimationPhase [i].UIElements.Length; j++) {

					UIAnimationPhase [i].UIElements [j]._rectTransform.localPosition = UIAnimationPhase [i].UIElements [j]._initialPosition;
					UIAnimationPhase [i].UIElements [j]._rectTransform.localScale = UIAnimationPhase [i].UIElements [j]._initialScaling;

					if (UIAnimationPhase [i].UIElements [j].UIObject.GetComponent<Text> ()) 
						UIAnimationPhase [i].UIElements [j].UIObject.GetComponent<Text> ().color = UIAnimationPhase[i].UIElements[j]._initialColor;
					else if (UIAnimationPhase [i].UIElements [j].UIObject.GetComponent<Image> ())
						UIAnimationPhase [i].UIElements [j].UIObject.GetComponent<Image> ().color = UIAnimationPhase[i].UIElements[j]._initialColor;
					else if (UIAnimationPhase [i].UIElements [j].UIObject.GetComponent<Button> ()) 
						UIAnimationPhase [i].UIElements [j].UIObject.GetComponent<Image> ().color = UIAnimationPhase[i].UIElements[j]._initialColor;

					/* REMOVE : FOr Testing Better User Interface
					if (UIAnimationPhase [i].UIElements [j].UIObject.activeInHierarchy)
						UIAnimationPhase [i].UIElements [j].UIObject.SetActive (false);
					*/
				}

				break;
			}
		}
	}

	public void ResetAllAnimation(){

		for (int i = 0; i < UIAnimationPhase.Length; i++) {

			UIAnimationPhase [i].isAnimationRunning = false;

			for (int j = 0; j < UIAnimationPhase [i].UIElements.Length; j++) {

				UIAnimationPhase [i].UIElements [j]._rectTransform.localPosition = UIAnimationPhase [i].UIElements [j]._initialPosition;
				UIAnimationPhase [i].UIElements [j]._rectTransform.localScale = UIAnimationPhase [i].UIElements [j]._initialScaling;

				if (UIAnimationPhase [i].UIElements [j].UIObject.GetComponent<Text> ()) 
					UIAnimationPhase [i].UIElements [j].UIObject.GetComponent<Text> ().color = UIAnimationPhase[i].UIElements[j]._initialColor;
				else if (UIAnimationPhase [i].UIElements [j].UIObject.GetComponent<Image> ())
					UIAnimationPhase [i].UIElements [j].UIObject.GetComponent<Image> ().color = UIAnimationPhase[i].UIElements[j]._initialColor;
				else if (UIAnimationPhase [i].UIElements [j].UIObject.GetComponent<Button> ()) 
					UIAnimationPhase [i].UIElements [j].UIObject.GetComponent<Image> ().color = UIAnimationPhase[i].UIElements[j]._initialColor;

				/* REMOVE : FOr Testing Better User Interface
				if (UIAnimationPhase [i].UIElements [j].UIObject.activeInHierarchy)
					UIAnimationPhase [i].UIElements [j].UIObject.SetActive (false);
				*/
			}
		}
	}

	#endregion

	#region PRIVATE METHOD

	private int mGetAnimationIndex(string AnimationName){

		for (int i = 0; i < UIAnimationPhase.Length; i++) {

			if (UIAnimationPhase [i].animationName == AnimationName) {

				return i;
			}
		}

		return -1;
	}

	private void mVariableInitialization(int mAnimationIndex){

		for (int i = 0; i < UIAnimationPhase [mAnimationIndex].UIElements.Length; i++) {

			if (UIAnimationPhase [mAnimationIndex].rotationEffect) {

				UIAnimationPhase[mAnimationIndex]._previousAbsoluteRotation = 0.0f;
				UIAnimationPhase [mAnimationIndex]._previousCheckpointIndex = 0;

				//Calculate -> Total Rotation
				UIAnimationPhase[mAnimationIndex]._totalRotation = 0.0f;
				for (int k = 0; k < UIAnimationPhase [i].degreeOfRotation.Length; k++)
					UIAnimationPhase[mAnimationIndex]._totalRotation += Mathf.Abs(UIAnimationPhase [i].degreeOfRotation [k]);

				//Assigning -> Checkpoint
				UIAnimationPhase[mAnimationIndex]._rotationCheckPoints = new float[UIAnimationPhase [i].degreeOfRotation.Length];
				for (int k = 0; k < UIAnimationPhase[mAnimationIndex]._rotationCheckPoints.Length; k++) {

					if (k == 0) {
						UIAnimationPhase[mAnimationIndex]._rotationCheckPoints [0] = 0.0f;
					}else {

						UIAnimationPhase[mAnimationIndex]._rotationCheckPoints [k] = 0.0f;
						for (int l = 0; l < k; l++) 
							UIAnimationPhase[mAnimationIndex]._rotationCheckPoints [k] += Mathf.Abs(UIAnimationPhase [i].degreeOfRotation [l]);
					}
				}
			}
				

			UIAnimationPhase [mAnimationIndex].UIElements [i]._rectTransform = UIAnimationPhase [mAnimationIndex].UIElements [i].UIObject.GetComponent<RectTransform> ();

			if (UIAnimationPhase [mAnimationIndex].UIElements [i].UIObject.GetComponent<Text> ()) 
				UIAnimationPhase[mAnimationIndex].UIElements[i]._initialColor = UIAnimationPhase [mAnimationIndex].UIElements [i].UIObject.GetComponent<Text> ().color;
			else if (UIAnimationPhase [mAnimationIndex].UIElements [i].UIObject.GetComponent<Image> ())
				UIAnimationPhase[mAnimationIndex].UIElements[i]._initialColor = UIAnimationPhase [mAnimationIndex].UIElements [i].UIObject.GetComponent<Image> ().color;
			else if (UIAnimationPhase [mAnimationIndex].UIElements [i].UIObject.GetComponent<Button> ()) 
				UIAnimationPhase[mAnimationIndex].UIElements[i]._initialColor = UIAnimationPhase [mAnimationIndex].UIElements [i].UIObject.GetComponent<Image> ().color;

			UIAnimationPhase [mAnimationIndex].UIElements [i]._initialPosition = UIAnimationPhase [mAnimationIndex].UIElements [i]._rectTransform.localPosition;
			UIAnimationPhase [mAnimationIndex].UIElements [i]._animatedPosition = UIAnimationPhase [mAnimationIndex].UIElements [i]._initialPosition + UIAnimationPhase [mAnimationIndex].UIElements [i].animationDirection;

			UIAnimationPhase [mAnimationIndex].UIElements [i]._initialScaling = UIAnimationPhase [mAnimationIndex].UIElements [i]._rectTransform.localScale;
			UIAnimationPhase [mAnimationIndex].UIElements [i]._animatedScaling = UIAnimationPhase [mAnimationIndex].UIElements [i]._initialScaling * UIAnimationPhase [mAnimationIndex].scalePercentage;
			UIAnimationPhase [mAnimationIndex].UIElements [i]._scalingDifference = UIAnimationPhase [mAnimationIndex].UIElements [i]._animatedScaling - UIAnimationPhase [mAnimationIndex].UIElements [i]._initialScaling;

			UIAnimationPhase [mAnimationIndex].UIElements [i]._distanceFromAnimatedPosition = Vector3.Distance (UIAnimationPhase [mAnimationIndex].UIElements [i]._animatedPosition, UIAnimationPhase [mAnimationIndex].UIElements [i]._initialPosition);

			UIAnimationPhase [mAnimationIndex].UIElements [i].animationSpeed = UIAnimationPhase [mAnimationIndex].UIElements [i]._distanceFromAnimatedPosition / UIAnimationPhase [mAnimationIndex].animationDuration;
		}
	}

	private void mAnimate(){

		for (int i = 0; i < UIAnimationPhase.Length; i++) {

			if (UIAnimationPhase[i].isAnimationRunning) {

				for (int j = 0; j < UIAnimationPhase [i].UIElements.Length; j++) {

					if (mCheckEndOfAnimation (i)) {

						UIAnimationPhase [i].isAnimationRunning = false;

						mEndAnimationAdjustmentFactor (i);

						// If : It is not loop animation
						if (!UIAnimationPhase [i].loopAnimation)
							UIAnimationPhase [i].OnAnimationEndEvent.Invoke ();
						else
							ExecuteAnimation (UIAnimationPhase [i].animationName);
						break;
					} else if (UIAnimationPhase [i].asyncAnimation && mDistanceOfUI (i, j) > 0) {

						mTranslationAnimation (i, j);

						mFadingAnimation (i, j);

						mScalingAnimation (i, j);

						//mRotationAnimation (i, j);

						if (j != (UIAnimationPhase [i].UIElements.Length-1) ) {

							float mActualDistance = Vector3.Distance (UIAnimationPhase [i].UIElements [j]._initialPosition, UIAnimationPhase [i].UIElements [j]._animatedPosition);
							if (mDistanceOfUI (i, j) > ((mActualDistance * UIAnimationPhase [i].asyncDelayReduction) / 100.0f))
								break;
						}else
							break;
						
					} else {

						mTranslationAnimation (i, j);

						mFadingAnimation (i, j);

						mScalingAnimation (i, j);

						//mRotationAnimation (i, j);
					}
					/*
					if (UIAnimation[i].disappearingAnimation && mDistanceOfUI (i, j) == 0) {

						UIAnimation [i].UIElement [j].UIObject.SetActive (false);
					}
					*/
				}
			}
		}
	}

	private void mTranslationAnimation(int i,int j){

		if (UIAnimationPhase [i].appearingAnimation) {
		
			UIAnimationPhase [i].UIElements [j]._rectTransform.localPosition = Vector3.MoveTowards (UIAnimationPhase [i].UIElements [j]._rectTransform.localPosition, UIAnimationPhase [i].UIElements [j]._initialPosition, UIAnimationPhase [i].UIElements[j].animationSpeed * Time.deltaTime);
		}else if(UIAnimationPhase [i].disappearingAnimation) {

			UIAnimationPhase [i].UIElements [j]._rectTransform.localPosition = Vector3.MoveTowards (UIAnimationPhase [i].UIElements [j]._rectTransform.localPosition, UIAnimationPhase [i].UIElements [j]._animatedPosition, UIAnimationPhase [i].UIElements[j].animationSpeed * Time.deltaTime);
		}
	}

	private void mFadingAnimation(int i, int j){


		float _opacity = 0;
		if (UIAnimationPhase [i].fadingInEffect) {

			_opacity = 1.0f - ((UIAnimationPhase [i].UIElements [j].animationEndTime - Time.time) / UIAnimationPhase[i].animationDuration);

		} else if (UIAnimationPhase [i].fadingOutEffect) {

			_opacity = (UIAnimationPhase [i].UIElements [j].animationEndTime - Time.time) / UIAnimationPhase[i].animationDuration;

			if (_opacity == 0.0f) {
				_opacity = 1.0f;
			}
		}else
			_opacity = 1.0f;
		
		if (UIAnimationPhase [i].UIElements [j].UIObject.GetComponent<Text> ()) {

			Color mColor = UIAnimationPhase [i].UIElements [j].UIObject.GetComponent<Text> ().color;
			UIAnimationPhase [i].UIElements [j].UIObject.GetComponent<Text> ().color = new Color (mColor.r, mColor.g, mColor.b, _opacity);
		} else if (UIAnimationPhase [i].UIElements [j].UIObject.GetComponent<Image> ()) {

			Color mColor = UIAnimationPhase [i].UIElements [j].UIObject.GetComponent<Image> ().color;
			UIAnimationPhase [i].UIElements [j].UIObject.GetComponent<Image> ().color = new Color (mColor.r, mColor.g, mColor.b, _opacity);
		} else if (UIAnimationPhase [i].UIElements [j].UIObject.GetComponent<Button> ()) {

			Color mColor = UIAnimationPhase [i].UIElements [j].UIObject.GetComponent<Image> ().color;
			UIAnimationPhase [i].UIElements [j].UIObject.GetComponent<Image> ().color = new Color (mColor.r, mColor.g, mColor.b, _opacity);
		}
	}

	private void mScalingAnimation(int i, int j){

		float _scaling = (UIAnimationPhase [i].UIElements [j].animationEndTime - Time.time) / UIAnimationPhase[i].animationDuration;

		if (UIAnimationPhase [i].positiveScalingEffect) {

			UIAnimationPhase [i].UIElements [j]._rectTransform.localScale = new Vector3 (
				UIAnimationPhase [i].UIElements [j]._initialScaling.x + (UIAnimationPhase [i].UIElements [j]._scalingDifference.x * (1.0f - _scaling)),
				UIAnimationPhase [i].UIElements [j]._initialScaling.y + (UIAnimationPhase [i].UIElements [j]._scalingDifference.y * (1.0f - _scaling)),
				UIAnimationPhase [i].UIElements [j]._initialScaling.z + (UIAnimationPhase [i].UIElements [j]._scalingDifference.z * (1.0f - _scaling)));
		} else if (UIAnimationPhase [i].negetiveScalingEffect) {

			UIAnimationPhase [i].UIElements [j]._rectTransform.localScale = new Vector3 (
				UIAnimationPhase [i].UIElements [j]._initialScaling.x - (UIAnimationPhase [i].UIElements [j]._scalingDifference.x * (1.0f - _scaling)),
				UIAnimationPhase [i].UIElements [j]._initialScaling.y - (UIAnimationPhase [i].UIElements [j]._scalingDifference.y * (1.0f - _scaling)),
				UIAnimationPhase [i].UIElements [j]._initialScaling.z - (UIAnimationPhase [i].UIElements [j]._scalingDifference.z * (1.0f - _scaling)));
		}
	}


	/// <summary>
	/// Rotation Effect (On Progress).
	/// </summary>
	/// <param name="i">The index.</param>
	/// <param name="j">J.</param>
	private void mRotationAnimation(int i, int j){

		if (UIAnimationPhase [i].rotationEffect) {

			float _rotationFactor = (UIAnimationPhase [i].UIElements [j].animationEndTime - Time.time) / UIAnimationPhase[i].animationDuration;

			float _absoluteRotation = UIAnimationPhase[i]._totalRotation * (1.0f - _rotationFactor);
			float _rotationDegree = _absoluteRotation - UIAnimationPhase [i]._previousAbsoluteRotation;
			UIAnimationPhase [i]._previousAbsoluteRotation = _absoluteRotation ;

			for (int k = UIAnimationPhase[i]._rotationCheckPoints.Length - 1; k >= 0; k--) {

				if ((_absoluteRotation) > UIAnimationPhase[i]._rotationCheckPoints [k]) {

					if (k != UIAnimationPhase [i]._previousCheckpointIndex) {
						
						float _error = Mathf.Abs(_absoluteRotation - UIAnimationPhase[i].degreeOfRotation[k-1]);

						Debug.Log (_absoluteRotation + " : ("+(k-1)+") " + UIAnimationPhase[i].degreeOfRotation[k-1] + " : " + _error);
						/*
						if(UIAnimationPhase[i].degreeOfRotation[k-1] > 0.0f)
							UIAnimationPhase [i].UIElements [j]._rectTransform.Rotate (Vector3.forward  * _error);
						else
							UIAnimationPhase [i].UIElements [j]._rectTransform.Rotate (Vector3.back  	* _error);
						*/

						UIAnimationPhase [i]._previousCheckpointIndex = k;
					}

					if (UIAnimationPhase [i].degreeOfRotation [k] > 0.0f){
						UIAnimationPhase [i].UIElements [j]._rectTransform.Rotate (Vector3.forward  * _rotationDegree);
					}else{
						UIAnimationPhase [i].UIElements [j]._rectTransform.Rotate (Vector3.back    * _rotationDegree);
					}

					break;
				}
			} 
		}
	}

	private float mDistanceOfUI(int i,int j){

		if (UIAnimationPhase [i].appearingAnimation) {

			return Vector3.Distance (UIAnimationPhase [i].UIElements [j]._initialPosition, UIAnimationPhase [i].UIElements [j]._rectTransform.localPosition);
		} else if (UIAnimationPhase [i].disappearingAnimation) {

			return Vector3.Distance (UIAnimationPhase [i].UIElements [j]._animatedPosition, UIAnimationPhase [i].UIElements [j]._rectTransform.localPosition);
		}

		return -1;
	}

	private bool mCheckEndOfAnimation(int i){

		int _checker = 0;

		for (int j = 0; j < UIAnimationPhase[i].UIElements.Length; j++) {

			if (Time.time > UIAnimationPhase [i].UIElements [j].animationEndTime)
				_checker++;
		}

		if (_checker == UIAnimationPhase [i].UIElements.Length)
			return true;

		return false;
	}

	private void mEndAnimationAdjustmentFactor(int i){

		if (UIAnimationPhase [i].rotationEffect) {

			float _error = Mathf.Abs(UIAnimationPhase[i]._totalRotation - UIAnimationPhase[i]._previousAbsoluteRotation);

			for (int j = 0; j < UIAnimationPhase [i].UIElements.Length; j++) {

				if(UIAnimationPhase[i].degreeOfRotation[UIAnimationPhase[i].degreeOfRotation.Length-1] > 0.0f)
					UIAnimationPhase [i].UIElements [j]._rectTransform.Rotate (Vector3.forward  * _error);
				else
					UIAnimationPhase [i].UIElements [j]._rectTransform.Rotate (Vector3.back  * _error);
			}

		}
	}

	#endregion
}

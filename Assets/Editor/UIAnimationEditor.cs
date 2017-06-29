using UnityEngine;
using UnityEngine.Events;
using UnityEditor;

[CustomEditor(typeof(UIAnimation))]
public class UIAnimationEditor : Editor {

	UIAnimation mUIAReference;

	#region EDITOR HANDLER VARIABLES

	private struct ConfigUIA
	{
		public bool hasActiveView;

		public bool hasActiveViewForAppDisapear;

		public bool hasEnabledFadingEffect;
		public bool hasEnabledScalingEffect;
		public bool hasEnabledRotationEffect;

		public bool hasOnAnimationStartEvent;
		public bool hasOnAnimationEndEvent;
	}

	private int mNumberOfAnimationPhase;
	private ConfigUIA[] mConfigUIA;

	#endregion

	public void OnEnable(){

		mUIAReference = (UIAnimation)target;

		mConfigUIA = new ConfigUIA[mUIAReference.UIAnimationPhase.Length];

		if (mConfigUIA.Length != 0) {

			for (int i = 0; i < mConfigUIA.Length; i++) {

				if (mUIAReference.UIAnimationPhase [i].appearingAnimation || mUIAReference.UIAnimationPhase [i].disappearingAnimation)
					mConfigUIA [i].hasActiveViewForAppDisapear = true; 

				if (mUIAReference.UIAnimationPhase [i].fadingInEffect || mUIAReference.UIAnimationPhase [i].fadingOutEffect)
					mConfigUIA [i].hasEnabledFadingEffect = true; 

				if (mUIAReference.UIAnimationPhase [i].positiveScalingEffect || mUIAReference.UIAnimationPhase [i].negetiveScalingEffect)
					mConfigUIA [i].hasEnabledScalingEffect = true; 

				if (mUIAReference.UIAnimationPhase [i].rotationEffect)
					mConfigUIA [i].hasEnabledRotationEffect = true;

				if (mUIAReference.UIAnimationPhase [i].OnAnimationStartEvent.GetPersistentEventCount() != 0)
					mConfigUIA [i].hasOnAnimationStartEvent = true;

				if (mUIAReference.UIAnimationPhase [i].OnAnimationEndEvent.GetPersistentEventCount() != 0)
					mConfigUIA [i].hasOnAnimationEndEvent = true;

				if (mConfigUIA [i].hasActiveViewForAppDisapear
				   || mConfigUIA [i].hasEnabledFadingEffect
				   || mConfigUIA [i].hasEnabledScalingEffect
				   || mConfigUIA[i].hasEnabledRotationEffect
				   || mConfigUIA [i].hasOnAnimationStartEvent
				   || mConfigUIA [i].hasOnAnimationEndEvent)
					mConfigUIA [i].hasActiveView = true;
			}
		}
	}

	public override void OnInspectorGUI(){

		serializedObject.Update ();
		EditorGUILayout.BeginVertical ();

		SerializedProperty StartAnimation = serializedObject.FindProperty("OnStartSceneAnimation");
		EditorGUILayout.PropertyField(StartAnimation);

		UIAnimationGUI ();

		EditorGUILayout.EndVertical ();
		serializedObject.ApplyModifiedProperties ();
	}

	#region UIAnimation

	private void UIAnimationGUI(){

		#region Balance Variables

		mNumberOfAnimationPhase = EditorGUILayout.IntField ("Animation: ", mUIAReference.UIAnimationPhase.Length);

		EditorGUI.indentLevel += 2;

		if (mNumberOfAnimationPhase != mUIAReference.UIAnimationPhase.Length) {

			UIAnimation.CA_UIAnimation[] mTempUIAnimationPhase = new UIAnimation.CA_UIAnimation [mUIAReference.UIAnimationPhase.Length];
			mTempUIAnimationPhase = mUIAReference.UIAnimationPhase;

			if (mNumberOfAnimationPhase > mUIAReference.UIAnimationPhase.Length) {

				mUIAReference.UIAnimationPhase = new UIAnimation.CA_UIAnimation[mNumberOfAnimationPhase];
				mConfigUIA = new ConfigUIA[mNumberOfAnimationPhase];

				for (int i = 0; i < mTempUIAnimationPhase.Length; i++) {
					
					mUIAReference.UIAnimationPhase [i] = mTempUIAnimationPhase [i];
				}
			} else {

				mUIAReference.UIAnimationPhase = new UIAnimation.CA_UIAnimation[mNumberOfAnimationPhase];
				mConfigUIA = new ConfigUIA[mNumberOfAnimationPhase];

				for (int i = 0; i < mUIAReference.UIAnimationPhase.Length; i++) {
					mUIAReference.UIAnimationPhase [i] = mTempUIAnimationPhase [i];
				}
			}
		}

		#endregion
	
		for (int i = 0; i < mUIAReference.UIAnimationPhase.Length; i++) {

			#region AnimationName (Read)

			if (mUIAReference.UIAnimationPhase [i].animationName != null
			    && mUIAReference.UIAnimationPhase [i].animationName.Length != 0) {

				mConfigUIA [i].hasActiveView = EditorGUILayout.Foldout (
					mConfigUIA [i].hasActiveView,
					mUIAReference.UIAnimationPhase [i].animationName
				);
			} else {

				mConfigUIA [i].hasActiveView = EditorGUILayout.Foldout (
					mConfigUIA [i].hasActiveView,
					"AnimationName (" + i + ")"
				);
			}

			#endregion

			if (mConfigUIA [i].hasActiveView) {

				#region AnimationName (Write)

				if (mUIAReference.UIAnimationPhase [i].animationName != null
					&& mUIAReference.UIAnimationPhase [i].animationName.Length != 0) {
					mUIAReference.UIAnimationPhase [i].animationName = EditorGUILayout.TextField (
						"AnimationName",
						mUIAReference.UIAnimationPhase [i].animationName);
				} else {
					mUIAReference.UIAnimationPhase [i].animationName = EditorGUILayout.TextField (
						"AnimationName (" + i + ")",
						mUIAReference.UIAnimationPhase [i].animationName);
				}

				#endregion

				#region AnimationProperty

				EditorGUILayout.Separator ();
				EditorGUILayout.LabelField ("Animation Property");

				EditorGUI.indentLevel += 2;

				mUIAReference.UIAnimationPhase [i].animationDuration = EditorGUILayout.Slider (
					"AnimationDuration",
					mUIAReference.UIAnimationPhase [i].animationDuration,
					0.0f,
					10.0f
				);
					
				mUIAReference.UIAnimationPhase [i].loopAnimation = EditorGUILayout.Toggle(
					new GUIContent("IsLoopingEnabled",
						"If : LoopAnimation is enabled" +
						"\nIt would not access OnAnimatonEndEvent"),
					mUIAReference.UIAnimationPhase[i].loopAnimation);

				mUIAReference.UIAnimationPhase [i].asyncAnimation = EditorGUILayout.Toggle (
					"AsyncAnimation",
					mUIAReference.UIAnimationPhase [i].asyncAnimation);

				if (mUIAReference.UIAnimationPhase [i].asyncAnimation) {

					EditorGUI.indentLevel += 2;
					mUIAReference.UIAnimationPhase [i].asyncDelayReduction = EditorGUILayout.Slider (
						"Async Delay Reduction:",
						mUIAReference.UIAnimationPhase [i].asyncDelayReduction,
						0.0f,
						10.0f
					);
					EditorGUI.indentLevel -=2;
				}

				mConfigUIA [i].hasActiveViewForAppDisapear = EditorGUILayout.Foldout (
					mConfigUIA [i].hasActiveViewForAppDisapear,
					"Appear/Disappear Animation");

				if (mConfigUIA [i].hasActiveViewForAppDisapear) {

					EditorGUI.indentLevel += 2;

					mUIAReference.UIAnimationPhase[i].disappearingAnimation = EditorGUILayout.Toggle (
						"Appear",
						!mUIAReference.UIAnimationPhase [i].disappearingAnimation);

					mUIAReference.UIAnimationPhase [i].disappearingAnimation = EditorGUILayout.Toggle (
						"Disappear",
						!mUIAReference.UIAnimationPhase [i].disappearingAnimation);

					mUIAReference.UIAnimationPhase [i].appearingAnimation = !mUIAReference.UIAnimationPhase [i].disappearingAnimation;

					EditorGUI.indentLevel -= 2;
				}

				#endregion

				#region AnimationEffect

				EditorGUI.indentLevel -= 2;
				EditorGUILayout.LabelField ("Animation Effects");
				EditorGUI.indentLevel += 2;

				mConfigUIA [i].hasEnabledFadingEffect = EditorGUILayout.Foldout (
					mConfigUIA [i].hasEnabledFadingEffect,
					"Fade Effect");

				if (mConfigUIA [i].hasEnabledFadingEffect) {

					EditorGUI.indentLevel += 2;

					mUIAReference.UIAnimationPhase [i].fadingOutEffect = EditorGUILayout.Toggle (
						"Fade-In",
						!mUIAReference.UIAnimationPhase [i].fadingOutEffect);

					mUIAReference.UIAnimationPhase [i].fadingOutEffect = EditorGUILayout.Toggle (
						"Fade-Out",
						!mUIAReference.UIAnimationPhase [i].fadingOutEffect);

					mUIAReference.UIAnimationPhase [i].fadingInEffect = !mUIAReference.UIAnimationPhase [i].fadingOutEffect;

					EditorGUI.indentLevel -= 2;
				} else {

					mUIAReference.UIAnimationPhase [i].fadingInEffect = false;
					mUIAReference.UIAnimationPhase [i].fadingOutEffect = false;
				}

				mConfigUIA [i].hasEnabledScalingEffect = EditorGUILayout.Foldout (
					mConfigUIA [i].hasEnabledScalingEffect,
					"Scale Effect");

				if (mConfigUIA [i].hasEnabledScalingEffect) {

					EditorGUI.indentLevel += 2;

					mUIAReference.UIAnimationPhase [i].negetiveScalingEffect = EditorGUILayout.Toggle (
						"Scale-Positive",
						!mUIAReference.UIAnimationPhase [i].negetiveScalingEffect);

					mUIAReference.UIAnimationPhase [i].negetiveScalingEffect = EditorGUILayout.Toggle (
						"Scale-Negetive",
						!mUIAReference.UIAnimationPhase [i].negetiveScalingEffect);

					mUIAReference.UIAnimationPhase [i].positiveScalingEffect = !mUIAReference.UIAnimationPhase [i].negetiveScalingEffect;

					mUIAReference.UIAnimationPhase [i].scalePercentage = EditorGUILayout.Slider (
						"Scale Percentage *X:",
						mUIAReference.UIAnimationPhase [i].scalePercentage,
						1.0f,
						10.0f
					);

					EditorGUI.indentLevel -= 2;
				} else {

					mUIAReference.UIAnimationPhase [i].positiveScalingEffect = false;
					mUIAReference.UIAnimationPhase [i].negetiveScalingEffect = false;
				}

				/*
				mConfigUIA [i].hasEnabledRotationEffect = EditorGUILayout.Foldout (
					mConfigUIA [i].hasEnabledRotationEffect,
					"Rotating Effect");

				if(mConfigUIA[i].hasEnabledRotationEffect){

					EditorGUI.indentLevel += 2;

					mUIAReference.UIAnimationPhase [i].rotationEffect = true;

					EditorGUILayout.PropertyField (serializedObject
						.FindProperty("UIAnimationPhase")
						.GetArrayElementAtIndex(i)
						.FindPropertyRelative("degreeOfRotation")
						,true
					);

					EditorGUI.indentLevel -= 2;

				}else{

					mUIAReference.UIAnimationPhase [i].rotationEffect = false;	
				}
				*/
				EditorGUI.indentLevel -= 2;

				#endregion

				#region Events (Start-End)

				EditorGUILayout.LabelField ("Animation Event");
				EditorGUI.indentLevel += 2;

				mConfigUIA [i].hasOnAnimationStartEvent = EditorGUILayout.Foldout (
					mConfigUIA [i].hasOnAnimationStartEvent,
					"OnAnimationStartEvent"
				);

				if(mConfigUIA [i].hasOnAnimationStartEvent){

					EditorGUILayout.PropertyField(
						serializedObject.
						FindProperty("UIAnimationPhase").
						GetArrayElementAtIndex(i).
						FindPropertyRelative("OnAnimationStartEvent")
					);
				}

				if(!mUIAReference.UIAnimationPhase[i].loopAnimation){

					mConfigUIA [i].hasOnAnimationEndEvent = EditorGUILayout.Foldout (
						mConfigUIA [i].hasOnAnimationEndEvent,
						"OnAnimationEndEvent"
					);

					if(mConfigUIA [i].hasOnAnimationEndEvent){

						EditorGUILayout.PropertyField(
							serializedObject.
							FindProperty("UIAnimationPhase").
							GetArrayElementAtIndex(i).
							FindPropertyRelative("OnAnimationEndEvent")
						);
					}
				}

				EditorGUI.indentLevel -= 2;
				#endregion

				EditorGUI.indentLevel += 2;

				EditorGUILayout.PropertyField (
					serializedObject.
					FindProperty("UIAnimationPhase").
					GetArrayElementAtIndex(i).
					FindPropertyRelative("UIElements"),
					true
				);

				EditorGUI.indentLevel -= 2;
			}
		}
		EditorGUI.indentLevel -= 2;
	}

	#endregion
}

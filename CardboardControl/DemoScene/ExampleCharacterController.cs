﻿using UnityEngine;
using System.Collections;

public class ExampleCharacterController : MonoBehaviour {
  private static CardboardControlManager cardboard;

  private bool vibrateTriggered = false;

  void Start () {
    /*
    Start by capturing the script on CardboardInputManager
    This script has the delegates that you'll be passing your methods to
    
    Unity provides a good primer on delegates here:
    http://unity3d.com/learn/tutorials/modules/intermediate/scripting/delegates
    */
    cardboard = GameObject.Find("CardboardControlManager").GetComponent<CardboardControlManager>();
    cardboard.OnMagnetDown += CardboardDown;  // When the magnet goes down
    cardboard.OnMagnetUp += CardboardUp;      // When the magnet comes back up

    // When the magnet goes down and up within the "click threshold" time
    // That slick speed threshold is configurable in the inspector
    cardboard.OnMagnetClicked += CardboardClick;

    // When the thing we're looking at changes, determined by a raycast
    // The raycast distance and layer mask are public as configurable in the inspector
    cardboard.raycast.OnChange += CardboardFocus;

    // Not shown here is the OnOrientationTilt delegate
    // This is triggered on rotating the device to Portrait mode
    // The Google Cardboard app refers to this gesture as a Tilt
  }



  /*
  In this demo, we change object colours for each event triggered.
  The CardboardEvent is currently just a placeholder but exists to
  pass useful information to events with a consistent API.
  */
  public void CardboardDown(object sender, CardboardInputEvent cardboardEvent) {
    ChangeObjectColor("SphereDown");
  }

  public void CardboardUp(object sender, CardboardInputEvent cardboardEvent) {
    ChangeObjectColor("SphereUp");
  }

  public void CardboardClick(object sender, CardboardInputEvent cardboardEvent) {
    ChangeObjectColor("SphereClick");

    TextMesh textMesh = GameObject.Find("SphereClick/Counter").GetComponent<TextMesh>();
    int increment = int.Parse(textMesh.text) + 1;
    textMesh.text = increment.ToString();

    if (cardboard.raycast.IsFocused()) {
      Debug.Log("We've focused on this object for "+cardboard.raycast.SecondsFocused()+" seconds.");
    }
    
    // TODO: get something from raycast focus
  }

  public void CardboardFocus(object sender, CardboardInputEvent cardboardEvent) {
    // If we're not focused, the focused object will be null
    if (cardboard.raycast.IsFocused()) {
      ChangeObjectColor(cardboard.raycast.FocusedObject().name);
    }
  }

  public void ChangeObjectColor(string name) {
    GameObject obj = GameObject.Find(name);
    Color newColor = new Color(Random.value, Random.value, Random.value);
    obj.GetComponent<Renderer>().material.color = newColor;
  }



  /*
  During our game we can utilize data from CardboardInput.
  */
  void Update() {
    TextMesh textMesh = GameObject.Find("SphereDown/Counter").GetComponent<TextMesh>();

    // IsMagnetHeld is true when the magnet has gone down but not back up yet.    
    if (!cardboard.IsMagnetHeld()) {
      textMesh.GetComponent<Renderer>().enabled = Time.time % 1 < 0.5;
      vibrateTriggered = false;
    }
    else {
      textMesh.GetComponent<Renderer>().enabled = true;

      // SecondsMagnetHeld is the number of seconds we've held the magnet down.
      // It stops when when the magnet goes up and resets when the magnet goes down.
      textMesh.text = cardboard.SecondsMagnetHeld().ToString("#.##");

      // CardboardSDK has built-in triggers vibrations to provide feedback.
      // You can toggle them via the Unity Inspector or manually trigger your
      // own vibration events, as seen here.
      if (cardboard.SecondsMagnetHeld() > 2 && !vibrateTriggered) {
        cardboard.Vibrate();
        vibrateTriggered = true;
      }
    }
  }

  /*
  Be sure to unsubscribe before this object is destroyed
  so the garbage collector can clean up the object.
  */
  void OnDestroy() {
    cardboard.OnMagnetDown -= CardboardDown;
    cardboard.OnMagnetUp -= CardboardUp;
    cardboard.OnMagnetClicked -= CardboardClick;
    cardboard.raycast.OnChange -= CardboardFocus;
  }
}
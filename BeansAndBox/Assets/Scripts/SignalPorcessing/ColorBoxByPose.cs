using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using EchoState;

using LockingPolicy = Thalmic.Myo.LockingPolicy;
using Pose = Thalmic.Myo.Pose;
using UnlockType = Thalmic.Myo.UnlockType;
using VibrationType = Thalmic.Myo.VibrationType;

// Change the material when certain poses are made with the Myo armband.
// Vibrate the Myo armband when a fist pose is made.
public class ColorBoxByPose : MonoBehaviour
{
    // Myo game object to connect with.
    // This object must have a ThalmicMyo script attached.
    public GameObject myo = null;

    // Materials to change to when poses are made.
    public Material waveInMaterial;
    public Material waveOutMaterial;
    public Material doubleTapMaterial;
 
	// START OF NONSTANDARD CONTENT

    // Txt for debugging purposes
    public Text debugText;
    // buffer
    private Buffer buf = new Buffer();
    // filter
    private CombFilter filter = new CombFilter(200, 50);
    // EchoStateNetwork
    private EchoStateNetwork network = EchoStateNetwork.FromJSON(@"example_reservoir.json", @"example_W_out.json");

	// END OF NONSTANDARD CONTENT

    // The pose from the last update. This is used to determine if the pose has changed
    // so that actions are only performed upon making them rather than every frame during
    // which they are active.
    private Pose _lastPose = Pose.Unknown;

    // Update is called once per frame.
    void Update ()
    {
        // Access the ThalmicMyo component attached to the Myo game object.
        ThalmicMyo thalmicMyo = myo.GetComponent<ThalmicMyo> ();
 
	// START OF NONSTANDARD CONTENT

        //debugText.text = Directory.GetCurrentDirectory();
        // store the current data in a Buffer until it is full
        if(buf.store(thalmicMyo.emg)) {
          // if the buffer is full, retrieve the current content and filter it
	  double[,] filtered = filter.filter(buf.retrieve());
	  // compute the features
	  double[] x = Features.extractFeatures(filtered);
	  // update the network
          network.Update(x);
	  // retrieve the network output
          double[] y = network.GetOutput();
	  // display it in the Unity environment
          debugText.text = "logvar EMG: " + x[0] + ", " + x[1] + ", " + x[2] + ", " + x[3] + ", " + x[4] + ", " + x[5] + ", " + x[6] + ", " + x[7] + "\n"
            + "network output: " + y[0];
        }

	// END OF NONSTANDARD CONTENT
        
        // Check if the pose has changed since last update.
        // The ThalmicMyo component of a Myo game object has a pose property that is set to the
        // currently detected pose (e.g. Pose.Fist for the user making a fist). If no pose is currently
        // detected, pose will be set to Pose.Rest. If pose detection is unavailable, e.g. because Myo
        // is not on a user's arm, pose will be set to Pose.Unknown.
        if (thalmicMyo.pose != _lastPose) {
            _lastPose = thalmicMyo.pose;

            // Vibrate the Myo armband when a fist is made.
            if (thalmicMyo.pose == Pose.Fist) {
                thalmicMyo.Vibrate (VibrationType.Medium);

                ExtendUnlockAndNotifyUserAction (thalmicMyo);

            // Change material when wave in, wave out or double tap poses are made.
            } else if (thalmicMyo.pose == Pose.WaveIn) {
                GetComponent<Renderer>().material = waveInMaterial;

                ExtendUnlockAndNotifyUserAction (thalmicMyo);
            } else if (thalmicMyo.pose == Pose.WaveOut) {
                GetComponent<Renderer>().material = waveOutMaterial;

                ExtendUnlockAndNotifyUserAction (thalmicMyo);
            } else if (thalmicMyo.pose == Pose.DoubleTap) {
                GetComponent<Renderer>().material = doubleTapMaterial;

                ExtendUnlockAndNotifyUserAction (thalmicMyo);
            }
        }
    }

    // Extend the unlock if ThalmcHub's locking policy is standard, and notifies the given myo that a user action was
    // recognized.
    void ExtendUnlockAndNotifyUserAction (ThalmicMyo myo)
    {
        ThalmicHub hub = ThalmicHub.instance;

        if (hub.lockingPolicy == LockingPolicy.Standard) {
            myo.Unlock (UnlockType.Timed);
        }

        myo.NotifyUserAction ();
    }
}


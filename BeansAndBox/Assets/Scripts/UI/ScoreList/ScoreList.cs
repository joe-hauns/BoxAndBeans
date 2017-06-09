using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class ScoreList : MonoBehaviour {

	private List<ScoreListEntryUi> ui;

	void Awake () 
	{
		this.ui = new List<ScoreListEntryUi>(GetComponentsInChildren<ScoreListEntryUi> ());
		ui.Sort ((v1, v2) => v1.gameObject.name.CompareTo (v2.gameObject.name));
	}

	public void SetEntries(List<GameStateDto> entries, Func<GameStateDto, string> titleConverter)
	{

		entries.Sort ((v1, v2) => {
			var cmpScore = v2.score.CompareTo (v1.score);
			return cmpScore == 0 ? v2.dateTime.CompareTo(v1.dateTime) : cmpScore;
		});

		int lastGameIndex = 0;
		if (entries.Count != 0) {
			for (int i=0; i < entries.Count; i++) {
				if (entries[i].dateTime.Ticks > entries[lastGameIndex].dateTime.Ticks){
					lastGameIndex = i;
				}
			}
		}
		int startIndex;
		if (lastGameIndex == 0) {
			startIndex = 0;
		} else if (lastGameIndex == entries.Count - 1) {
			startIndex = Mathf.Max (0, lastGameIndex - (ui.Count - 1));
		} else {
			var minPlayerIndex = 1; // one may only be the first in the list if one is really the best player
			var maxPlayerIndex = 1; // one may be at most the player in the middle
			var rand = UnityEngine.Random.value;
			startIndex = Mathf.Max(0, lastGameIndex - maxPlayerIndex + (int)(rand * (maxPlayerIndex - minPlayerIndex)));
		}
		var entriesToSet = Mathf.Min (entries.Count - startIndex, ui.Count);
		for (int i = 0; i < entriesToSet; i++) {
			var e = entries [startIndex + i];
			var ui = this.ui [i];
			ui.title = titleConverter(e) ;
			ui.score = e.score ;
			ui.highlighted = startIndex+i == lastGameIndex;
		}
		for (int i = entriesToSet; i < ui.Count; i++) {
			print ("clearing: " + i);
			ui [i].Clear ();
		}
	}
}
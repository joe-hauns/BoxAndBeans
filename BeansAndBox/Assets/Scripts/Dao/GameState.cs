using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SimpleJson;
using System.IO;
using System;

public class GameState
{
	public static GameStateDto createDto (string player, List<ControllerStateDto> controllerStates, List<Bean> spawnedBeans)
	{
		int score = 0;

		var beans = new BeanDto[spawnedBeans.Count];
		for (int i = 0; i < beans.Length; i++) {
			var bean = spawnedBeans [i];
			if (bean.wasCollected)
				score++;

			beans [i] = new BeanDto (bean.states.ToArray());
		}

		var playground = new List<BoxCollider2D>(GameObject.FindObjectsOfType<BoxCollider2D>()).Find(o => o.name.ToLower() == "playground").bounds;

		return new GameStateDto (
			score: score,
			participant: player,
			time: DateTime.Now,
			beans: beans,
			playground: new PlayGroundDto(
				xMin: playground.min.x,
				xMax: playground.max.x,
				yMin: playground.min.y,
				yMax: playground.max.y
			),
			controllerPath: controllerStates.ToArray()
		);
	}
}
 
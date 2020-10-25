using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NodeObs : MonoBehaviour 
{
	public int Width;
	public int Height;
	public int obstacle;

	public IDictionary<Vector3, bool> walkablePosition = new Dictionary<Vector3, bool>();
	public IDictionary<Vector3, GameObject> nodeType = new Dictionary<Vector3, GameObject>();
	public Dictionary<Vector3, string> obstacles = new Dictionary<Vector3, string>();

	string obstacleType = null;

	void Start () 
	{
		Init (obstacle);
	}
	
	void Init(int src_obstacles)
	{
		var node = GameObject.Find ("Node");
		var obstacle = GameObject.Find ("Obstacle");
		var width = Width;
		var height = Height;


		for (int i = 0; i < src_obstacles; i++)
		{
			Vector3 nodePosition = new Vector3(Random.Range(1, Width - 1), 0, Random.Range(1, Height - 1));
			if (!obstacles.ContainsKey(nodePosition))
			{
				obstacles.Add(nodePosition, "barrier");
			}
		}

		for (int i = 0; i < width; i++) 
		{
			for (int j = 0; j < height; j++) 
			{
				Vector3 Position = new Vector3 (i, 0, j);
				GameObject copy;
				

				if (obstacles.TryGetValue (Position, out obstacleType))
				{
					copy = Instantiate (obstacle);
					copy.transform.position = Position;

					switch (obstacleType) 
					{
					case "barrier":
						walkablePosition.Add (new KeyValuePair<Vector3, bool> (Position, false));
						break;
					}
				}
				else 
				{
					copy = Instantiate (node);
					copy.transform.position = Position;
					walkablePosition.Add (new KeyValuePair<Vector3, bool> (Position, true));
				}

				nodeType.Add (Position, copy);
			}
		}
        GameObject goal = GameObject.Find("Goal");
        walkablePosition[goal.transform.localPosition] = true;
		nodeType[goal.transform.localPosition] = goal;
    }
}

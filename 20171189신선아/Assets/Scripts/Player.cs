using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Priority_Queue;

public class Player : MonoBehaviour 
{
	public float speed ;
	IDictionary<Vector3, Vector3> nodeParents = new Dictionary<Vector3, Vector3>();
	IDictionary<Vector3, Sprite> prevSprite = new Dictionary<Vector3, Sprite> ();
	IDictionary<Vector3, GameObject> prevObject = new Dictionary<Vector3, GameObject>();

	public IDictionary<Vector3, bool> walkablePositions;
	public IDictionary<Vector3, string> obstacles;

	NodeObs nodeArray;
	IList<Vector3> path;

	bool solutionVisible;
	string prevAlgo;

    bool moveCube = false;
	int i;

	void Start ()
    {
		nodeArray = GameObject.Find ("NodeArray").GetComponent<NodeObs> ();
		obstacles = GameObject.Find ("NodeArray").GetComponent<NodeObs> ().obstacles;
		walkablePositions = nodeArray.walkablePosition;
	}
	
	void Update () 
	{
		if (moveCube)
		{
			
			float step = Time.deltaTime * speed;
			transform.position = Vector3.MoveTowards(transform.position, path[i], step);
			if (transform.position.Equals(path[i]) && i >= 0)
			{
				i--;
			}
			if (i < 0)
			{
				moveCube = false;
			}
		}
	}

	public void MoveCube()
	{
		moveCube = true;
	}
	Vector3 Dijkstra(Vector3 start, Vector3 goal)
	{
        uint nodeVisitCount = 0;

        IPriorityQueue<Vector3, int> priority = new SimplePriorityQueue<Vector3, int>();

        IDictionary<Vector3, int> distances = walkablePositions.Where(x => x.Value == true).ToDictionary(x => x.Key, x => int.MaxValue);

        distances[start] = 0;
        priority.Enqueue(start, 0);

        while (priority.Count > 0) 
		{
            Vector3 walk = priority.Dequeue();
            nodeVisitCount+=1;

            if (walk == goal) 
			{
                return goal;
            }

			IList<Vector3> nodes = GetWalkableNodes (walk);

			foreach (Vector3 node in nodes) 
			{
                int dist = distances[walk] + Weight(node);

                if (dist < distances [node]) 
				{
					distances [node] = dist;
					nodeParents [node] = walk;

                    if (!priority.Contains(node))
                    {
                        priority.Enqueue(node, dist);
                    }
                }
			}
		}
        return start;
	}
	int Weight(Vector3 node) 
	{
		return 1;
	}
	Vector3 DFS(Vector3 startPosition, Vector3 goalPosition)
	{
		uint nodeVisitCount = 0;

		Stack<Vector3> stack = new Stack<Vector3>();
		HashSet<Vector3> exploredNodes = new HashSet<Vector3>();
		stack.Push(startPosition);

		while (stack.Count != 0)
		{
			Vector3 currentNode = stack.Pop();
			nodeVisitCount +=1;

			if (currentNode == goalPosition)
			{
				return currentNode;
			}

			IList<Vector3> nodes = GetWalkableNodes(currentNode);

			foreach (Vector3 node in nodes)
			{
				if (!exploredNodes.Contains(node))
				{
					exploredNodes.Add(node);

					nodeParents.Add(node, currentNode);

					stack.Push(node);
				}
			}
		}

		return startPosition;
	}
	Vector3 BFS(Vector3 start, Vector3 goal)
	{
        uint nodeVisitCount = 0;

		Queue<Vector3> queue = new Queue<Vector3> ();
		HashSet<Vector3> exploredNodes = new HashSet<Vector3> ();
		queue.Enqueue (start);

		while (queue.Count != 0) {
			Vector3 currentNode = queue.Dequeue ();
            nodeVisitCount+=1;

			if (currentNode == goal) 
			{
                return currentNode;
			}

			IList<Vector3> nodes = GetWalkableNodes (currentNode);

			foreach(Vector3 node in nodes)
			{
				if(!exploredNodes.Contains(node)) 
				{
					exploredNodes.Add(node);
					nodeParents.Add (node, currentNode);
					queue.Enqueue (node);
				}
			}
		}

		return start;
	}

	bool CanMove(Vector3 nextPosition) 
	{
		if (walkablePositions.ContainsKey(nextPosition))
		{
			return walkablePositions[nextPosition];
		}
		else
		{
			return false;
		}
	}

	public void PlayShortPath(string algorithm) 
	{
		if (solutionVisible && algorithm == prevAlgo) 
		{
			foreach (Vector3 node in path) 
			{
				nodeArray.nodeType[node].GetComponent<SpriteRenderer> ().sprite = prevSprite[node];
			}
			solutionVisible = false;
			return;
		}
			
		nodeParents = new Dictionary<Vector3, Vector3>();
		path = ShortPath(algorithm);

		if (path == null)
		{
			return;
		}
        Sprite BFSTile = Resources.Load<Sprite>("BFS");
        Sprite DFSTile = Resources.Load<Sprite>("DFS");
        Sprite dijkstraTile = Resources.Load<Sprite>("dijkstra");
        Sprite finishTile = Resources.Load<Sprite>("Goal");

        foreach (Vector3 node in path)
        {
            prevSprite[node] = nodeArray.nodeType[node].GetComponent<SpriteRenderer>().sprite;
            if (algorithm == "DFS")
            {
                nodeArray.nodeType[node].GetComponent<SpriteRenderer>().sprite = DFSTile;

            }
            else if (algorithm == "BFS")
            {
                nodeArray.nodeType[node].GetComponent<SpriteRenderer>().sprite = BFSTile;
            }
            else
            {
                nodeArray.nodeType[node].GetComponent<SpriteRenderer>().sprite = dijkstraTile;

            }
        }

        nodeArray.nodeType[path[0]].GetComponent<SpriteRenderer>().sprite = finishTile;

        i = path.Count - 1;
		solutionVisible = true;
		prevAlgo = algorithm;
	}


	IList<Vector3> ShortPath(string algorithm)
	{
		IList<Vector3> path = new List<Vector3> ();
		Vector3 goal;
        if (algorithm == "DFS")
        {
            goal = DFS(this.transform.localPosition, GameObject.Find("Goal").transform.localPosition);
        }
        else if (algorithm == "BFS")
        {
            goal = BFS(this.transform.localPosition, GameObject.Find("Goal").transform.localPosition);
        }
        else
        {
            goal = Dijkstra(this.transform.localPosition, GameObject.Find("Goal").transform.localPosition);
        }

		if (goal == this.transform.localPosition || !nodeParents.ContainsKey(nodeParents[goal])) {
	
			return null;
		}

		Vector3 walk = goal;
		while (walk != this.transform.localPosition) {
			path.Add (walk);
			walk = nodeParents [walk];
		}

		return path;
	}

	IList<Vector3> GetWalkableNodes(Vector3 walk) 
	{
		IList<Vector3> walkableNodes = new List<Vector3> ();

		IList<Vector3> possibleNodes = new List<Vector3> () 
		{
			new Vector3 (walk.x + 1, walk.y, walk.z),
			new Vector3 (walk.x - 1, walk.y, walk.z),
			new Vector3 (walk.x, walk.y, walk.z + 1),
			new Vector3 (walk.x, walk.y, walk.z - 1),
            new Vector3 (walk.x + 1, walk.y, walk.z + 1),
            new Vector3 (walk.x + 1, walk.y, walk.z - 1),
            new Vector3 (walk.x - 1, walk.y, walk.z + 1),
            new Vector3 (walk.x - 1, walk.y, walk.z - 1)
        };

		foreach (Vector3 node in possibleNodes) 
		{
			if (CanMove (node)) 
			{
				walkableNodes.Add (node);
			} 
		}
		return walkableNodes;
	}
}

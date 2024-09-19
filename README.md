## Polygonal funnel algorithm

### Core idea

- Calculate the position of the seven points in relation to the first target boundary vector, specifying the left and right vectors. ** (funnel initialisation)**

- Mark the left and right vector indexes separately, and in adjacent polygons, compute the next left and right shops.

- Iterate through all limits in turn, comparing the new position vectors with the historical left and right limit vector position relationships, respectively.

  - Left-to-left, expand the funnel; the funnel vertex remains unchanged, the left limit vector remains unchanged, and the left inflection point Lst (cache) is added to the current left limit point.

  - Left to right, contracting funnel; right limit vector not crossed: funnel apex unchanged, left limit updated to left monitor point, left inflection point cleared of historical data.

  - Left to right, shrink funnel, right limit vector has been crossed: add current right limit point as path point; traverse right inflection point Lst, **computed right minimum angle offset vertex**, update funnel vertex and right limit vector to legal state.

    ---------------------------------------------------------------------------------------------------------

  - Right to right, expand the funnel; the funnel fixed point remains unchanged, the right limit vector remains unchanged, and the right inflection point Lst adds the current right limit point;

  - Right to left, contracting the funnel without crossing the left limit vector: funnel vertex unchanged, right limit updated to the right detection point, right inflection point Lst cleared of historical data.

  - Right to left, contract funnel, have crossed left limit vector: add current left limit point as path point; traverse left inflection point Lst, ** calculate the left minimum angle offset vertex **, update funnel vertex and left limit vector to legal state.

- When the target region vertex is reached, use that point as the left and right contraction target, respectively:

  - left of the left limit, add the left point as the inflection point and the target point as the end point - in the middle of the left and right limits, pass through and the target point as the end point; - right of the right limit, add the right point as the inflection point and the target point as the end point;

- Output all path points

## A-Star Algorithm

The A* algorithm is a widely used algorithm for graph search and path planning that combines the advantages of Dijkstra's algorithm with the efficiency of heuristic search. It estimates the distance from the current node to the goal node by using a heuristic function, thus prioritising paths that appear more promising to reach the goal during the search.

The following are the steps to implement the A* algorithm:

1. **Initialisation**: create open list and closed list. The open list contains nodes that need to be evaluated and the closed list contains nodes that have already been evaluated.
    ```c#
   private readonly PriorityQueue<NavArea> openList = new PriorityQueue<NavArea>(4); //优先级队列
   private readonly List<NavArea> closeList = new List<NavArea>();
   ```
2. **Start Node**: adds the start node to the open list.
   ```c#
   public List<NavArea> CalAStarPolyPath(NavArea start, NavArea end)
   {
       startArea = start;
       endArea = end;
       openList.Clear();
       closeList.Clear();
   
       openList.Enqueue(start);
       startArea.sumDistance = 0;
       
       // detect area...
       
   }
   ```
3. Iteration: Repeat the following steps until the target node is found or the open list is empty:

   - Select the node from the open list that has the lowest f-value. f-value is the sum of g-value and h-value. g-value is the actual cost from the starting point to the current node and h-value is the estimated cost from the current node to the target node.

   - Move the node from the open list to the closed list.
   ```C#
     while (openList.Count > 0)
     {
         if (openList.Contains(end))
         {
             pathList = GetPathNavAreas(end);
             showPathAreaHandle?.Invoke(pathList);
             closeList.Add(end);
             break;
         }
         // get the area has highest priority, add it to close list
         NavArea detectArea = openList.Dequeue();
         if (!closeList.Contains(detectArea))
         {
             closeList.Add(detectArea);
         }
     ```
   - Check each neighbouring node of this node:

     - If the neighbouring node is in the closed list, skip it. - If the neighbouring node is not in the open list, add it and set its g, h and f values accordingly. - If the neighbouring node is in the open list but now has a lower g-value, update its g-value, h-value and f-value.
     ```c#
     private void DetectNeighbourArea(NavArea detectArea, NavArea neighbourArea)
     {
         if (!closeList.Contains(neighbourArea))
         { 
             float neighborDistance = detectArea.CalNavAreaDis(neighbourArea);
             float newSumDistance = detectArea.sumDistance + neighborDistance;
             //update area's sumdistance
             if (float.IsPositiveInfinity(neighbourArea.sumDistance) || newSumDistance < neighbourArea.sumDistance)
             {
                 neighbourArea.preArea = detectArea;
                 neighbourArea.sumDistance = newSumDistance;
             }
     
             //if this is a new area
             if (!openList.Contains(neighbourArea))
             {
                 float targetDistance = neighbourArea.CalNavAreaDis(endArea); //cal H
                 neighbourArea.weight = neighbourArea.sumDistance + targetDistance; //f = g + h
                 openList.Enqueue(neighbourArea);
             }
         }
     }
     ```
    **End**: constructs a path if the target node is found; if the open list is empty and no target node is found, it means there is no feasible path.
   ```C#
private NavArea startArea;
private NavArea endArea;

private readonly PriorityQueue<NavArea> openList = new PriorityQueue<NavArea>(4);
private readonly List<NavArea> closeList = new List<NavArea>();
private List<NavArea> pathList = new List<NavArea>();
/*private List<int> leftConnerLst = new List<int>();
private List<int> rightConnerLst = new List<int>();*/

public List<NavArea> CalAStarPolyPath(NavArea start, NavArea end)
{
    startArea = start;
    endArea = end;
    openList.Clear();
    closeList.Clear();

    openList.Enqueue(start);
    startArea.sumDistance = 0;
    while (openList.Count > 0)
    {
        if (openList.Contains(end))
        {
            pathList = GetPathNavAreas(end);
            showPathAreaHandle?.Invoke(pathList);
            closeList.Add(end);
            break;
        }
        // get the area has highest priority, add it to close list
        NavArea detectArea = openList.Dequeue();
        if (!closeList.Contains(detectArea))
        {
            closeList.Add(detectArea);
        }

        //continue detect neighbors of the above area
        for (int i = 0; i < detectArea.borderList.Count; i++)
        {
            NavBorder border = detectArea.borderList[i];
            NavArea neighbourArea = detectArea.areaID == border.areaID1
                ? areaArr[border.areaID2]
                : areaArr[border.areaID1];
            DetectNeighbourArea(detectArea, neighbourArea);
        }
    }

    return pathList;
}

/// <summary>
/// calculate priority for each neighbour area
/// </summary>
/// <param name="detectArea"></param>
/// <param name="neighbourArea"></param>
private void DetectNeighbourArea(NavArea detectArea, NavArea neighbourArea)
{
    if (!closeList.Contains(neighbourArea))
    { 
        float neighborDistance = detectArea.CalNavAreaDis(neighbourArea);
        float newSumDistance = detectArea.sumDistance + neighborDistance;
        //update area's sumdistance
        if (float.IsPositiveInfinity(neighbourArea.sumDistance) || newSumDistance < neighbourArea.sumDistance)
        {
            neighbourArea.preArea = detectArea;
            neighbourArea.sumDistance = newSumDistance;
        }

        //if this is a new area
        if (!openList.Contains(neighbourArea))
        {
            float targetDistance = neighbourArea.CalNavAreaDis(endArea); //cal H
            neighbourArea.weight = neighbourArea.sumDistance + targetDistance; //f = g + h
            openList.Enqueue(neighbourArea);
        }
    }
}

// output final path
private List<NavArea> GetPathNavAreas(NavArea end)
{
    List<NavArea> list = new List<NavArea>();
    list.Add(end);
    NavArea pre = end.preArea;
    while (pre != null)
    {
        list.Insert(0, pre);
        pre = pre.preArea;
    }

    int startID, endID;
    for (int i = 0; i < list.Count - 1; i++)
    {
        startID = list[i].areaID;
        endID = list[i + 1].areaID;
        string key;
        if (startID < endID)
        {
            key = $"{startID}-{endID}";
        }
        else
        {
            key = $"{endID}-{startID}";
        }

        list[i].targetBorder = GetBorderByAreaIDKey(key);
        //this.LogGreen($"{list[i].targetBorder.pointIndex1}---{list[i].targetBorder.pointIndex2}");
    }

    return list;
}
```

﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SquareCell : MonoBehaviour {
    public SquareGridChunk parentChunk;
    public GridCoordinates coordinates;
    public RectTransform uiRect;

    float centreElevation = 0;
    GridElevations vertexElevations;
    Color color;
    int waterLevel=2;

    [SerializeField]
    SquareCell[] neighbors;

    [SerializeField]
    private bool[] hasIncomingRivers = new bool[8];
    [SerializeField]
    private bool[] hasOutgoingRivers = new bool[8];
    [SerializeField]
    bool[] roads = new bool[8]; // includes diagonals


    public int WaterLevel
    {
        get
        {
            return waterLevel;
        }
        set
        {
            if (waterLevel == value)
            {
                return;
            }
            waterLevel = value;
            Refresh();
        }
    }

    public bool IsUnderwater
    {
        get
        {
            return waterLevel > GetMinElevation();
        }
    }


    public bool HasRoadThroughEdge(GridDirection direction)
    {
        return roads[(int)direction];
    }


    public bool HasRoads
    {
        get
        {
            for (int i = 0; i < roads.Length; i++)
            {
                if (roads[i])
                {
                    return true;
                }
            }
            return false;
        }
    }

    public int RoadCount
    {
        get
        {
            int counter = 0;
            for (int i = 0; i < roads.Length; i++)
            {
                if (roads[i])
                {
                    counter++;
                }
            }
            return counter;
        }
    }

    public void AddRoad(GridDirection direction)
    {
        if (!roads[(int)direction] && !HasRiver && !HasCliff(direction) && GetElevationDifference(direction) <= 3)
        {
            SetRoad((int)direction, true);
        }
        else
        {
            Debug.Log("Could not add road");
        }
    }

    public void RemoveRoad(GridDirection direction)
    {
        if (roads[(int)direction] == true)
        { SetRoad((int)direction, false); }
    }

    void SetRoad(int direction, bool state)
    {
        roads[direction] = state;
        RefreshSelfOnly();
    }

    public int GetElevationDifference(GridDirection direction)
    {
        int differencePrev = (int)vertexElevations[direction.Previous()] - (int)vertexElevations[direction.Opposite().Next()];
        int differenceNext = (int)vertexElevations[direction.Next()] - (int)vertexElevations[direction.Opposite().Previous()];
        int result = Mathf.Max(differencePrev, differenceNext);
        Debug.Log("elevation test" + result);
        return result;
    }

    public int GetMaxElevation()
    {
        return Mathf.Max(vertexElevations.Y0, vertexElevations.Y1, vertexElevations.Y2, vertexElevations.Y3);
    }

    public int GetMinElevation()
    {
        return Mathf.Min(vertexElevations.Y0, vertexElevations.Y1, vertexElevations.Y2, vertexElevations.Y3);
    }

    public bool HasCliff(GridDirection direction)
    {
        SquareCell neighbor = GetNeighbor(direction);
        if (neighbor)
        {
            bool noCliff = (int)vertexElevations[direction.Previous()] == (int)neighbor.vertexElevations[direction.Opposite().Next()] && (int)vertexElevations[direction.Next()] == (int)neighbor.vertexElevations[direction.Opposite().Previous()];
            Debug.Log("cliff test" + !noCliff);
            return !noCliff;
        }
        else
        {
            Debug.Log("No neighbor to test for cliff");
            return false;
        }
    }

    public float RiverSurfaceY
    {
        get
        {
            return
                (centreElevation + GridMetrics.waterElevationOffset) * GridMetrics.elevationStep;
        }
    }

    public float WaterSurfaceY
    {
        get
        {
            return
                (waterLevel + GridMetrics.waterElevationOffset) * GridMetrics.elevationStep;
        }
    }

    public bool[] HasIncomingRiver
    {
        get
        {
            return hasIncomingRivers;
        }
    }

    public bool[] HasOutgoingRiver
    {
        get
        {
            return hasOutgoingRivers;
        }
    }

    public GridDirection[] IncomingRivers
    {
        get
        {
            List<GridDirection> directions = new List<GridDirection>();
            for (int i = 0; i < 8; i++)
            {
                if (hasIncomingRivers[i])
                {
                    directions.Add((GridDirection)i);
                }
            }
            return directions.ToArray();
        }
    }

    public GridDirection[] OutgoingRivers
    {
        get
        {
            List<GridDirection> directions = new List<GridDirection>();
            for (int i = 0; i < 8; i++)
            {
                if (hasOutgoingRivers[i])
                {
                    directions.Add((GridDirection)i);
                }
            }
            return directions.ToArray();
        }
    }

    public bool HasRiver
    {
        get
        {
            return hasIncomingRivers.Any(item => item == true) || hasOutgoingRivers.Any(item => item == true);
        }
    }

    public bool HasRiverBeginOrEnd
    {
        get
        {
            return hasIncomingRivers.Any(item => item == true) != hasOutgoingRivers.Any(item => item == true);
        }
    }

    public bool HasRiverThroughEdge(GridDirection direction)
    {
        if ((int)direction >= hasIncomingRivers.Length)
        { Debug.Log((int)direction); }
        return hasIncomingRivers[(int)direction] || hasOutgoingRivers[(int)direction];
    }

    public void RemoveOutgoingRivers()
    {
        if (!hasOutgoingRivers.Any())
        {
            return;
        }
        GridDirection[] neighbors = OutgoingRivers;
        for (int i = 0; i < hasOutgoingRivers.Length; i++) { hasOutgoingRivers[i] = false; }
        RefreshSelfOnly();

        foreach (GridDirection direction in neighbors)
        {
            SquareCell neighbor = GetNeighbor(direction);
            hasIncomingRivers[(int)direction.Opposite()] = false;
            neighbor.RefreshSelfOnly();
        }
    }

    public void RemoveOutgoingRiver(GridDirection direction)
    {
        if (!hasOutgoingRivers[(int)direction])
        {
            return;
        }
        hasOutgoingRivers[(int)direction] = false;
        RefreshSelfOnly();
        SquareCell neighbor = GetNeighbor(direction);
        hasIncomingRivers[(int)direction.Opposite()] = false;
        neighbor.RefreshSelfOnly();
    }

    public void RemoveIncomingRivers()
    {
        if (!hasIncomingRivers.Any())
        {
            return;
        }
        GridDirection[] neighbors = IncomingRivers;
        for (int i = 0; i < hasIncomingRivers.Length; i++) { hasIncomingRivers[i] = false; }
        RefreshSelfOnly();

        foreach (GridDirection direction in neighbors)
        {
            SquareCell neighbor = GetNeighbor(direction);
            hasOutgoingRivers[(int)direction.Opposite()] = false;
            neighbor.RefreshSelfOnly();
        }
    }

    public void RemoveIncomingRiver(GridDirection direction)
    {
        if (!hasIncomingRivers[(int)direction])
        {
            return;
        }
        hasIncomingRivers[(int)direction] = false;
        RefreshSelfOnly();
        SquareCell neighbor = GetNeighbor(direction);
        hasOutgoingRivers[(int)direction.Opposite()] = false;
        neighbor.RefreshSelfOnly();
    }

    public void RemoveRivers()
    {
        RemoveOutgoingRivers();
        RemoveIncomingRivers();
    }


    public void SetOutgoingRiver(GridDirection direction)
    {
        if (hasOutgoingRivers[(int)direction] || HasRoads)
        {
            Debug.Log("Could not add river");
            return;
        }
        SquareCell neighbor = GetNeighbor(direction);
        if (!neighbor || centreElevation < neighbor.centreElevation)
        {
            Debug.Log("Could not add river uphill");
            return;
        }
        if (hasIncomingRivers[(int)direction])
        {
            RemoveIncomingRiver(direction);
        }
        hasOutgoingRivers[(int)direction] = true;
        RefreshSelfOnly();

        neighbor.RemoveOutgoingRiver(direction.Opposite());
        neighbor.hasIncomingRivers[(int)direction.Opposite()] = true;
        neighbor.RefreshSelfOnly();
    }


    public void SetIncomingRiver(GridDirection direction)
    {
        if (hasOutgoingRivers[(int)direction] || HasRoads)
        {
            Debug.Log("Could not add river");
            return;
        }
        SquareCell neighbor = GetNeighbor(direction);
        if (!neighbor || centreElevation < neighbor.centreElevation)
        {
            Debug.Log("Could not add river uphill");
            return;
        }
        if (hasIncomingRivers[(int)direction])
        {
            RemoveIncomingRiver(direction.Opposite());
        }
    }

    private void UpdateCentreElevation()
    {
        centreElevation = (vertexElevations.Y0 + vertexElevations.Y1 + vertexElevations.Y2 + vertexElevations.Y3) / 4f;
        int maxElevation = Mathf.Max(vertexElevations.Y0, vertexElevations.Y1, vertexElevations.Y2, vertexElevations.Y3);
        Vector3 uiPosition = uiRect.localPosition;
        uiPosition.z = -(maxElevation * GridMetrics.elevationStep + 0.001f);
        uiRect.localPosition = uiPosition;
        for (GridDirection i = GridDirection.N; i < GridDirection.NW; i++)
        {
            if (hasOutgoingRivers[(int)i] && centreElevation < GetNeighbor(i).centreElevation)
            {
                RemoveOutgoingRiver(i);
            }
            if (hasIncomingRivers[(int)i] && centreElevation > GetNeighbor(i).centreElevation)
            {
                RemoveIncomingRiver(i);
            }
        }
        Refresh();
    }

    public float CentreElevation
    {
        get { return centreElevation; }
    }


    public void ChangeVertexElevation(GridDirection vertex, int value)
    {
        vertexElevations[vertex] += value;
        if ((HasRoadThroughEdge(vertex.Next()) && (GetElevationDifference(vertex.Next()) > 3 || HasCliff(vertex.Next()))) ||
            (HasRoadThroughEdge(vertex.Previous()) && (GetElevationDifference(vertex.Previous()) > 3 || HasCliff(vertex.Previous())))
            )
        {
            Debug.Log("Could not change elevation");
            vertexElevations[vertex] -= value; // revert change
        }
        else
        {
            UpdateCentreElevation();
        }
    }


    public GridElevations GridElevations
    {
        get { return vertexElevations; }
        set
        {
            vertexElevations = value;
            UpdateCentreElevation();
        }
    }


    public Color Color
    {
        get { return color; }
        set
        {
            if (color == value)
            {
                return;
            }
            color = value;
            Refresh();
        }
    }


    public SquareCell GetNeighbor(GridDirection direction)
    {
        return neighbors[(int)direction];
    }


    public void SetNeighbor(GridDirection direction, SquareCell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }


    void Refresh()
    {
        if (parentChunk)
        {
            parentChunk.Refresh();
            for (int i = 0; i < neighbors.Length; i++)
            {
                SquareCell neighbor = neighbors[i];
                if (neighbor != null && neighbor.parentChunk != parentChunk)
                {
                    neighbor.parentChunk.Refresh();
                }
            }
        }
    }

    void RefreshSelfOnly()
    {
        parentChunk.Refresh();
    }
}

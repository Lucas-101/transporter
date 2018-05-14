﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum EditMode
{
    color,
    elevation
}

public class MapEditor : MonoBehaviour {
    public Color[] colors;
    public SquareGrid squareGrid;
    private Color activeColor;
    private EditMode activeMode;
    private bool allowCliffs = false;
    private Vector3 pointerLocation;


    void Awake()
    {
        SelectColor(0);
    }


    void Update()
    {
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) && !EventSystem.current.IsPointerOverGameObject())
        {
            HandleInput();
        }
        HandleMousePosition();
    }


    void HandleInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            EditCell(squareGrid.GetCell(hit.point), squareGrid.GetVertex(hit.point));
        }
    }


    void HandleMousePosition()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            MoveEditorPointer(squareGrid.GetCell(hit.point), squareGrid.GetVertex(hit.point));
        }
    }

    public void SelectColor (int index)
    {
        activeColor = colors[index];
    }


    public void SetMode(int mode)
    {
        activeMode = (EditMode)mode;
    }


    public void toggleCliffs()
    {
        allowCliffs = !allowCliffs;
    }


    void MoveEditorPointer(SquareCell cell, GridDirection vertex)
    {
        if (activeMode == EditMode.color)
        {
            pointerLocation = GridCoordinates.ToPosition(cell.coordinates) + Vector3.up * cell.CentreElevation * GridMetrics.elevationStep;
        }
        if (activeMode == EditMode.elevation)
        {
            pointerLocation = GridCoordinates.ToPosition(cell.coordinates) + GridMetrics.GetEdge(vertex) + Vector3.up * (int)cell.GridElevations[vertex] * GridMetrics.elevationStep;
        }
    }


    private void OnDrawGizmos()
    {
        if(pointerLocation != null)
        {
            Gizmos.color = Color.white;
            if (activeMode == EditMode.elevation)
            {
                Gizmos.DrawSphere(pointerLocation, 0.1f);
            }
            if (activeMode == EditMode.color)
            {
                Gizmos.DrawWireCube(pointerLocation, new Vector3(1, 0, 1));
            }
        }
    }


    void EditCell(SquareCell cell, GridDirection vertex)
    {
        if(activeMode == EditMode.color)
        {
            cell.Color = activeColor;
        }
        if(activeMode == EditMode.elevation)
        {
            if (Input.GetMouseButtonUp(0))
            {
                cell.ChangeVertexElevation(vertex, 1);
                if(!allowCliffs)
                {
                    if(cell.GetNeighbor(vertex)) cell.GetNeighbor(vertex).ChangeVertexElevation(vertex.Opposite(), 1);
                    if(cell.GetNeighbor(vertex.Next())) cell.GetNeighbor(vertex.Next()).ChangeVertexElevation(vertex.Previous2(), 1);
                    if(cell.GetNeighbor(vertex.Previous())) cell.GetNeighbor(vertex.Previous()).ChangeVertexElevation(vertex.Next2(), 1);
                }
            }
            if (Input.GetMouseButtonUp(1))
            {
                cell.ChangeVertexElevation(vertex, -1);
                if (!allowCliffs)
                {
                    if (cell.GetNeighbor(vertex)) cell.GetNeighbor(vertex).ChangeVertexElevation(vertex.Opposite(), -1);
                    if (cell.GetNeighbor(vertex.Next())) cell.GetNeighbor(vertex.Next()).ChangeVertexElevation(vertex.Previous2(), -1);
                    if (cell.GetNeighbor(vertex.Previous())) cell.GetNeighbor(vertex.Previous()).ChangeVertexElevation(vertex.Next2(),-1);
                }
            }
        }
    }
}

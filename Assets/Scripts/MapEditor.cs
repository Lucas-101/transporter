﻿using System.Collections;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum EditMode
{
    color,
    elevation,
    rivers,
    roads,
    water_level,
    building,
    trees,
    rocks,
    mast,
    lighthouse,
    industry,
    town
}

public enum IndustryIndex
{
    CoalMine,
    PowerStation,
    Forest,
    Sawmill,
    OilRefinery,
    Factory,
    IronMine,
    OilWell,
    Farm,
    Bank
}

public class MapEditor : MonoBehaviour
{
    public GroundMaterial[] materials;
    public SquareGrid squareGrid;
    public GameObject townPrefab;
    public Transform tileSelectPrefab, edgeSelectPrefab;
    private Transform[] highlight;
    private GroundMaterial activeTileMaterial;
    private EditMode activeMode;
    private IndustryIndex activeIndustry =0;
    private bool allowCliffs = false;
    private Vector3 pointerLocation;
    private Dropdown dropMenu;
    private Vector3 hitPosition;
    public int pointerSize = 1;
    bool isDrag, freshClick = false;
    GridDirection dragDirection;
    SquareCell currentCell, previousCell;
    GridDirection vertexDirection;
    Stopwatch stopWatch = new Stopwatch();

    void Awake()
    {
        SelectMaterial(0);
        stopWatch.Start();
        dropMenu = GetComponentInChildren<Dropdown>();
        dropMenu.ClearOptions();
        List<GroundMaterial> materialsList = new List<GroundMaterial>(materials);
        List<string> namesList = new List<string>();
        foreach (GroundMaterial mat in materialsList)
        {
            namesList.Add(mat.tileTypeName);
        }
        dropMenu.AddOptions(namesList);
    }


    void Update()
    {
        if ((Input.GetMouseButton(0) || Input.GetMouseButton(1)) && !EventSystem.current.IsPointerOverGameObject())
        {
            HandleInput();
        }
        else
        {
            previousCell = null;
        }
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            freshClick = true;
        }
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            freshClick = false;
        }
        HandleMousePosition();
        Draw();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void TogglePause()
    {
        if (Time.timeScale!=0)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    void HandleInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            currentCell = squareGrid.GetCell(hit.point);
            if (previousCell && previousCell != currentCell)
            {
                ValidateDrag(currentCell);
            }
            else
            {
                isDrag = false;
            }
            EditCells(currentCell, hit.point);
            previousCell = currentCell;
        }
        else
        {
            previousCell = null;
        }
    }


    void ValidateDrag(SquareCell currentCell)
    {
        for (dragDirection = GridDirection.N;
            dragDirection <= GridDirection.NW;
            dragDirection++
        )
        {
            if (previousCell.GetNeighbor(dragDirection) == currentCell)
            {
                isDrag = true;
                return;
            }
        }
        isDrag = false;
    }

    void HandleMousePosition()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            hitPosition = hit.point;
            vertexDirection = squareGrid.GetVertex(hitPosition);
            MoveEditorPointer(squareGrid.GetCell(hitPosition), vertexDirection);
        }
    }

    public void UpdateMaterial()
    {
        SetMode((int)EditMode.color);
        SelectMaterial(dropMenu.value);
    }

    public void SelectMaterial(int index)
    {
        activeTileMaterial = materials[index];
    }


    public void SetMode(int mode)
    {
        activeMode = (EditMode)mode;
    }

    public void SetIndustry(int mode)
    {
        activeIndustry = (IndustryIndex)mode;
    }


    public void ToggleCliffs()
    {
        allowCliffs = !allowCliffs;
    }

    public void SetBrushSize(float i)
    {
        pointerSize = (int)i;
    }

    void MoveEditorPointer(SquareCell cell, GridDirection vertex)
    {
        if (activeMode == EditMode.color || activeMode == EditMode.rivers || activeMode == EditMode.roads ||
            activeMode == EditMode.water_level || activeMode == EditMode.building || activeMode == EditMode.trees ||
            activeMode == EditMode.rocks || activeMode == EditMode.mast || activeMode == EditMode.lighthouse || activeMode == EditMode.industry || activeMode == EditMode. town)
        {
            pointerLocation = GridCoordinates.ToPosition(cell.coordinates) + Vector3.up * cell.CentreElevation * GridMetrics.elevationStep;
        }
        if (activeMode == EditMode.elevation)
        {
            pointerLocation = GridCoordinates.ToPosition(cell.coordinates) + GridMetrics.GetEdge(vertex) + Vector3.up * (int)cell.GridElevations[vertex] * GridMetrics.elevationStep;
        }
    }

    private void Draw()
    {
        if (squareGrid != null)
        {
            int numberHightlights = (int)Mathf.Pow(pointerSize, 2);
            if (highlight == null || numberHightlights != highlight.Length)
            {
                if (highlight != null)
                {
                    foreach (Transform h in highlight)
                    {
                        Destroy(h.gameObject);
                    }
                }
                highlight = new Transform[numberHightlights];
            }
            for (int x = 0; x < pointerSize; x++)
            {
                for (int z = 0; z < pointerSize; z++)
                {
                    int index = z + x * pointerSize;
                    Vector3 offPos = pointerLocation + new Vector3(x, 0, z);
                    if (activeMode == EditMode.elevation)
                    {   
                        if (highlight[index])
                        {
                            Destroy(highlight[index].gameObject);
                        }
                        highlight[index] = Instantiate(edgeSelectPrefab);
                        float yOffset = (float)squareGrid.GetCellOffset(pointerLocation, x, z).GetVertexElevations[vertexDirection] * GridMetrics.elevationStep;
                        offPos.y = yOffset;
                        highlight[index].localPosition = offPos;
                    }
                    if (activeMode == EditMode.color || activeMode == EditMode.rivers || activeMode == EditMode.roads ||
                        activeMode == EditMode.water_level || activeMode == EditMode.building || activeMode == EditMode.trees ||
                        activeMode == EditMode.rocks || activeMode == EditMode.mast || activeMode == EditMode.lighthouse || activeMode == EditMode.industry || activeMode == EditMode.town)
                    {
                        if (highlight[index])
                        {
                            Destroy(highlight[index].gameObject);
                        }
                        highlight[index] = Instantiate(tileSelectPrefab);
                        if(squareGrid.GetCellOffset(pointerLocation, x, z).GetVertexElevations[vertexDirection] != null)
                        {
                            float yOffset = (float)squareGrid.GetCellOffset(pointerLocation, x, z).GetMaxElevation() * GridMetrics.elevationStep;
                            offPos.y = yOffset;
                            highlight[index].localPosition = offPos;
                        }
                    }
                }
            }

        }
    }


    void EditCells(SquareCell cell, Vector3 hitpoint)
    {
        for (int x = 0; x < pointerSize; x++)
        {
            for (int z = 0; z < pointerSize; z++)
            {
                Vector3 offPos = new Vector3(cell.coordinates.X + x, 0, cell.coordinates.Z + z);
                SquareCell offCell = squareGrid.GetCell(offPos);
                EditCell(offCell, hitpoint);
            }
        }
    }


    void EditCell(SquareCell cell, Vector3 hitpoint)
    {
        if (activeMode == EditMode.color)
        {
            cell.Tile = activeTileMaterial.GetClone;
        }
        else if (activeMode == EditMode.elevation)
        {
            GridDirection vertex = squareGrid.GetVertex(hitpoint);
            if (Input.GetMouseButton(0) && (stopWatch.ElapsedMilliseconds > 500f || freshClick))
            {
                cell.ChangeVertexElevation(vertex, 1);
                if (!allowCliffs)
                {
                    if (cell.GetNeighbor(vertex)) cell.GetNeighbor(vertex).ChangeVertexElevation(vertex.Opposite(), 1);
                    if (cell.GetNeighbor(vertex.Next())) cell.GetNeighbor(vertex.Next()).ChangeVertexElevation(vertex.Previous2(), 1);
                    if (cell.GetNeighbor(vertex.Previous())) cell.GetNeighbor(vertex.Previous()).ChangeVertexElevation(vertex.Next2(), 1);
                }
                stopWatch.Reset();
                stopWatch.Start();
            }
            if (Input.GetMouseButton(1) && (stopWatch.ElapsedMilliseconds > 500f || freshClick))
            {
                cell.ChangeVertexElevation(vertex, -1);
                if (!allowCliffs)
                {
                    if (cell.GetNeighbor(vertex)) cell.GetNeighbor(vertex).ChangeVertexElevation(vertex.Opposite(), -1);
                    if (cell.GetNeighbor(vertex.Next())) cell.GetNeighbor(vertex.Next()).ChangeVertexElevation(vertex.Previous2(), -1);
                    if (cell.GetNeighbor(vertex.Previous())) cell.GetNeighbor(vertex.Previous()).ChangeVertexElevation(vertex.Next2(), -1);
                }
                stopWatch.Reset();
                stopWatch.Start();
            }
        }
        else if (activeMode == EditMode.rivers)
        {
            if (Input.GetMouseButton(1))
            {
                cell.RemoveRivers();
                Explosion(cell);
            }
            else if (isDrag)
            {
                SquareCell otherCell = cell.GetNeighbor(dragDirection.Opposite()); // work with brushes
                if (otherCell)
                {
                    otherCell.SetOutgoingRiver(dragDirection);
                }
            }
        }
        else if (activeMode == EditMode.roads)
        {
            float fracX = hitpoint.x - Mathf.Floor(hitpoint.x);
            float fracZ = hitpoint.z - Mathf.Floor(hitpoint.z);
            if (fracX > 0.25f && fracX < 0.75f || fracZ > 0.25f && fracZ < 0.75f)
            {
                GridDirection edge = squareGrid.GetEdge(hitpoint);
                if (Input.GetMouseButton(1))
                {
                    cell.RemoveRoad(edge);
                    Explosion(cell);
                }
                else
                {
                    cell.AddRoad(edge);
                }
            }
        }
        else if (activeMode == EditMode.water_level)
        {
            if (Input.GetMouseButton(0) && freshClick)
            {
                cell.WaterLevel++;
            }
            else if (Input.GetMouseButton(1) && freshClick)
            {
                cell.WaterLevel--;
            }
        }
        else if (activeMode == EditMode.building)
        {
            if (Input.GetMouseButton(0) && freshClick)
            {
                cell.UrbanLevel++;
            }
            if (Input.GetMouseButton(1) && freshClick)
            {
                cell.UrbanLevel = 0;
                Explosion(cell);
            }
        }
        else if (activeMode == EditMode.trees)
        {
            if (Input.GetMouseButton(0) && freshClick)
            {
                cell.PlantLevel++;
            }
            if (Input.GetMouseButton(1) && freshClick)
            {
                cell.PlantLevel = 0;
                Explosion(cell);
            }
        }
        else if (activeMode == EditMode.rocks)
        {
            if (Input.GetMouseButton(0) && freshClick)
            {
                cell.ScenaryObject = 1;
            }
            if (Input.GetMouseButton(1) && freshClick)
            {
                cell.ScenaryObject = 0;
                Explosion(cell);
            }
        }
        else if (activeMode == EditMode.mast)
        {
            if (Input.GetMouseButton(0) && freshClick)
            {
                cell.ScenaryObject = 2;
            }
            if (Input.GetMouseButton(1) && freshClick)
            {
                cell.ScenaryObject = 0;
                Explosion(cell);
            }
        }
        else if (activeMode == EditMode.lighthouse)
        {
            if (Input.GetMouseButton(0) && freshClick)
            {
                cell.ScenaryObject = 3;
            }
            if (Input.GetMouseButton(1) && freshClick)
            {
                cell.ScenaryObject = 0;
                Explosion(cell);
            }
        }
        else if (activeMode == EditMode.industry)
        {
            if (Input.GetMouseButton(0) && freshClick)
            {
                cell.Industry = (int)activeIndustry+1;
            }
            if (Input.GetMouseButton(1) && freshClick)
            {
                cell.Industry = 0;
                Explosion(cell);
            }
        }
        else if (activeMode == EditMode.town)
        {
            if (cell.Town == null)
            {
                GameObject town = Instantiate(townPrefab);
                TownManager manager = town.GetComponent<TownManager>();
                town.transform.position = GridCoordinates.ToPosition(cell.coordinates) + new Vector3(0, cell.CentreElevation * GridMetrics.elevationStep, 0);
                manager.Init(cell);
                cell.Town = manager;
            }
        }
    }

    public void Explosion(SquareCell cell)
    {
        cell.Demolish();
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(hitPosition, new Vector3(0.1f, 0.1f, 0.1f));
    }
}
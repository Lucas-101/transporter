﻿using UnityEngine;
using UnityEngine.UI;

public class SquareGrid : MonoBehaviour
{

    public int width = 6;
    public int height = 6;
    public Color defaultColor = Color.white;
    public Color touchedColor = Color.magenta;
    public SquareCell cellPrefab;
    public Text cellLabelPrefab;

    SquareCell[] cells;
    Canvas gridCanvas;
    GridMesh gridMesh;

    private void Awake()
    {
        gridCanvas = GetComponentInChildren<Canvas>();
        gridMesh = GetComponentInChildren<GridMesh>();
        cells = new SquareCell[height * width];
        for (int z = 0, i = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                CreateCell(x, z, i++);
            }
        }
    }


    void Start()
    {
        gridMesh.Triangulate(cells);
    }


    void CreateCell(int x, int z, int i)
    {
        Vector3 position;
        position.x = x;
        position.y = 0f;
        position.z = z;

        SquareCell cell = cells[i] = Instantiate<SquareCell>(cellPrefab);
        cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;
        cell.coordinates = GridCoordinates.FromOffsetCoordinates(x, z);
        cell.color = defaultColor;
        CreateCellText(x, z);
    }

    void CreateCellText(int x, int z)
    {
        Text label = Instantiate<Text>(cellLabelPrefab);
        label.rectTransform.SetParent(gridCanvas.transform, false);
        label.rectTransform.anchoredPosition = new Vector2(x, z);
        label.text = x.ToString() + ", " + z.ToString();
    }


    public void ColorCell(Vector3 position, Color color)
    {
        position = transform.InverseTransformPoint(position);
        GridCoordinates coordinates = GridCoordinates.FromPosition(position);
        int index = coordinates.X + coordinates.Z * width;
        SquareCell cell = cells[index];
        cell.color = color;
        gridMesh.Triangulate(cells);
    }
}
﻿using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour
{
	enum OptionalToggle {
		Ignore, Yes, No
	}
	
	OptionalToggle riverMode,roadMode;
	public Color[] colors;
	public HexGrid hexGrid;
	public int activeElevation;
	public int activeWaterLevel;
	private Color activeColor;
	private bool applyColor;
	private bool applyElevation = true;
	private bool applyWaterLevel = true;
	private int brushSize;
	private bool isDrag;
	private HexDirection dragDirection;
	private HexCell previousCell;

	void Awake()
	{
		SelectColor(-1);
	}

	void Update()
	{
		if (
			Input.GetMouseButton(0) &&
			!EventSystem.current.IsPointerOverGameObject()
		)
		{
			HandleInput();
		}
		else 
		{
			previousCell = null;
		}
	}

	public void SetElevation(float elevation)
	{
		activeElevation = (int)elevation;
	}

	public void SetRiverMode (int mode) 
	{
		riverMode = (OptionalToggle)mode;
	}

	public void SetRoadMode (int mode) 
	{
		roadMode = (OptionalToggle)mode;
	}

	void HandleInput()
	{
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(inputRay, out hit))
		{
			HexCell currentCell = hexGrid.GetCell(hit.point);
			if (previousCell && previousCell != currentCell) {
				ValidateDrag(currentCell);
			}
			else 
			{
				isDrag = false;
			}
			EditCells(currentCell);
			previousCell = currentCell;
		}
		else 
		{
			previousCell = null;
		}
	}

	void ValidateDrag (HexCell currentCell) 
	{
		for (
			dragDirection = HexDirection.NE;
			dragDirection <= HexDirection.NW;
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

	public void SelectColor(int index)
	{
		applyColor = index >= 0;
		if (applyColor)
		{
			activeColor = colors[index];
		}
	}

	public void SetApplyElevation(bool toggle)
	{
		applyElevation = toggle;
	}

	public void SetBrushSize(float size)
	{
		brushSize = (int)size;
	}

	public void SetApplyWaterLevel (bool toggle) 
	{
		applyWaterLevel = toggle;
	}
	
	public void SetWaterLevel (float level) 
	{
		activeWaterLevel = (int)level;
	}

	void EditCells(HexCell center)
	{
		int centerX = center.coordinates.X;
		int centerZ = center.coordinates.Z;

		for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++)
		{
			for (int x = centerX - r; x <= centerX + brushSize; x++)
			{
				EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
			}
		}

		for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++)
		{
			for (int x = centerX - brushSize; x <= centerX + r; x++)
			{
				EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
			}
		}
	}

	void EditCell(HexCell cell)
	{
		if (cell)
		{
			if (applyColor)
			{
				cell.Color = activeColor;
			}
			if (applyElevation)
			{
				cell.Elevation = activeElevation;
			}
			if (applyWaterLevel) 
			{
				cell.WaterLevel = activeWaterLevel;
			}
			if (riverMode == OptionalToggle.No) 
			{
				cell.RemoveRiver();
			}
			if (roadMode == OptionalToggle.No) 
			{
				cell.RemoveRoads();
			}
			if (isDrag) {
				HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
				if (otherCell) {
					if (riverMode == OptionalToggle.Yes) {
						otherCell.SetOutgoingRiver(dragDirection);
					}
					if (roadMode == OptionalToggle.Yes) {
						otherCell.AddRoad(dragDirection);
					}
				}
			}
		}
	}
}
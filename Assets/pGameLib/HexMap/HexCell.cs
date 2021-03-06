﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCell : MonoBehaviour
{
    public HexCoordinates coordinates;
    public RectTransform uiRect;
    public HexGridChunk chunk;

    bool hasIncomingRiver, hasOutgoingRiver;
    HexDirection incomingRiver, outgoingRiver;

    public bool HasIncomingRiver
    {
        get
        {
            return hasIncomingRiver;
        }
    }

    public bool HasOutgoingRiver
    {
        get
        {
            return hasOutgoingRiver;
        }
    }

    public HexDirection IncomingRiver
    {
        get
        {
            return incomingRiver;
        }
    }

    public HexDirection OutgoingRiver
    {
        get
        {
            return outgoingRiver;
        }
    }

    public bool HasRiver
    {
        get
        {
            return hasIncomingRiver || hasOutgoingRiver;
        }
    }

    public bool HasRiverBeginOrEnd
    {
        get
        {
            return hasIncomingRiver != hasOutgoingRiver;
        }
    }

    public HexDirection RiverBeginOrEndDirection 
    {
		get {
			return hasIncomingRiver ? incomingRiver : outgoingRiver;
		}
	}

    public bool HasRiverThroughEdge(HexDirection direction)
    {
        return
            hasIncomingRiver && incomingRiver == direction ||
            hasOutgoingRiver && outgoingRiver == direction;
    }

    public void RemoveOutgoingRiver () 
    {
		if (!hasOutgoingRiver) {
			return;
		}
		hasOutgoingRiver = false;
		RefreshSelfOnly();

		HexCell neighbor = GetNeighbor(outgoingRiver);
		neighbor.hasIncomingRiver = false;
		neighbor.RefreshSelfOnly();
	}

    public void RemoveIncomingRiver () 
    {
		if (!hasIncomingRiver) {
			return;
		}
		hasIncomingRiver = false;
		RefreshSelfOnly();

		HexCell neighbor = GetNeighbor(incomingRiver);
		neighbor.hasOutgoingRiver = false;
		neighbor.RefreshSelfOnly();
	}

    public void RemoveRiver () 
    {
		RemoveOutgoingRiver();
		RemoveIncomingRiver();
	}

    public void SetOutgoingRiver (HexDirection direction) 
    {
		if (hasOutgoingRiver && outgoingRiver == direction) 
        {
			return;
		}
        //水不能往高流
        HexCell neighbor = GetNeighbor(direction);
		if (!IsValidRiverDestination(neighbor)) 
        {
			return;
		}
        //删了自己当前流出的要顺便把隔壁流入的删了
        RemoveOutgoingRiver();
		if (hasIncomingRiver && incomingRiver == direction) 
        {
			RemoveIncomingRiver();
		}
        hasOutgoingRiver = true;
		outgoingRiver = direction;
        //顺便把隔壁流入的也刷新了
        neighbor.RemoveIncomingRiver();
		neighbor.hasIncomingRiver = true;
		neighbor.incomingRiver = direction.Opposite();
		
        SetRoad((int)direction, false);
	}

    void ValidateRivers () 
    {
		if (
			hasOutgoingRiver &&
			!IsValidRiverDestination(GetNeighbor(outgoingRiver))
		) 
        {
			RemoveOutgoingRiver();
		}
		if (
			hasIncomingRiver &&
			!GetNeighbor(incomingRiver).IsValidRiverDestination(this)
		) 
        {
			RemoveIncomingRiver();
		}
	}

    bool IsValidRiverDestination (HexCell neighbor) 
    {
		return neighbor && (
			elevation >= neighbor.elevation || waterLevel == neighbor.elevation
		);
	}

    public int Elevation
    {
        get
        {
            return elevation;
        }
        set
        {
            if (elevation == value)
            {
                return;
            }
            elevation = value;
            Vector3 position = transform.localPosition;
            position.y = value * HexMetrics.kElevationStep;
            position.y +=
                (HexMetrics.SampleNoise(position).y * 2f - 1f) *
                HexMetrics.kElevationPerturbStrength;
            transform.localPosition = position;

            Vector3 uiPosition = uiRect.localPosition;
            uiPosition.z = -position.y;
            uiRect.localPosition = uiPosition;

            //修改了高度之后阻止水往高流
            ValidateRivers();

            for (int i = 0; i < roads.Length; i++) {
				if (roads[i] && GetElevationDifference((HexDirection)i) > 1) 
                {
					SetRoad(i, false);
				}
			}

            Refresh();
        }
    }

    public int WaterLevel 
    {
		get 
        {
			return waterLevel;
		}
		set {
			if (waterLevel == value) 
            {
				return;
			}
			waterLevel = value;
            ValidateRivers();
			Refresh();
		}
	}

    public bool IsUnderwater 
    {
		get 
        {
			return waterLevel > elevation;
		}
	}

    public float StreamBedY 
    {
		get {
			return
				(elevation + HexMetrics.kStreamBedElevationOffset) *
				HexMetrics.kElevationStep;
		}
	}

    public float RiverSurfaceY 
    {
		get {
			return
				(elevation + HexMetrics.kWaterElevationOffset) *
				HexMetrics.kElevationStep;
		}
	}

    public float WaterSurfaceY 
    {
		get 
        {
			return
				(waterLevel + HexMetrics.kWaterElevationOffset) *
				HexMetrics.kElevationStep;
		}
	}

    public Color Color
    {
        get
        {
            return color;
        }
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

    public Vector3 Position
    {
        get
        {
            return transform.localPosition;
        }
    }

    public bool HasRoads 
    {
		get {
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

    private int elevation = int.MinValue;
    private int waterLevel;
    private Color color;
    [SerializeField]
    private HexCell[] neighbors;

    [SerializeField]
	bool[] roads;

    public void RemoveRoads () 
    {
		for (int i = 0; i < neighbors.Length; i++) 
        {
			if (roads[i]) 
            {
				SetRoad(i, false);
                neighbors[i].roads[(int)((HexDirection)i).Opposite()] = false;
                neighbors[i].RefreshSelfOnly();
				RefreshSelfOnly();
			}
		}
	}

    public void AddRoad (HexDirection direction) 
    {
		if (
            !roads[(int)direction] && 
            !HasRiverThroughEdge(direction)&&
            GetElevationDifference(direction) <= 1
        ) 
        {
			SetRoad((int)direction, true);
		}
	}

    void SetRoad (int index, bool state) 
    {
		roads[index] = state;
		neighbors[index].roads[(int)((HexDirection)index).Opposite()] = state;
		neighbors[index].RefreshSelfOnly();
		RefreshSelfOnly();
	}

    public int GetElevationDifference (HexDirection direction) 
    {
		int difference = elevation - GetNeighbor(direction).elevation;
		return difference >= 0 ? difference : -difference;
	}

    void Refresh()
    {
        if (chunk)
        {
            chunk.Refresh();
            for (int i = 0; i < neighbors.Length; i++)
            {
                HexCell neighbor = neighbors[i];
                if (neighbor != null && neighbor.chunk != chunk)
                {
                    neighbor.chunk.Refresh();
                }
            }
        }
    }

    void RefreshSelfOnly () 
    {
        if (chunk)
        {
            chunk.Refresh();
        }
	}

    public HexEdgeType GetEdgeType(HexDirection direction)
    {
        return HexMetrics.GetEdgeType(
            elevation, neighbors[(int)direction].elevation
        );
    }

    public HexEdgeType GetEdgeType(HexCell otherCell)
    {
        return HexMetrics.GetEdgeType(
            elevation, otherCell.elevation
        );
    }

    public HexCell GetNeighbor(HexDirection direction)
    {
        return neighbors[(int)direction];
    }

    public void SetNeighbor(HexDirection direction, HexCell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    public bool HasRoadThroughEdge (HexDirection direction) 
    {
		return roads[(int)direction];
	}
}

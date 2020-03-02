using UnityEngine;

public enum HexDirection
{
	NE, E, SE, SW, W, NW
}

public enum HexEdgeType
{
	Flat, Slope, Cliff
}

public static class HexMetrics
{
	//地图每块（Chunk）横纵的Cell数量
	public const int kChunkSizeX = 5, kChunkSizeZ = 5;
	public const float kOuterToInner = 0.866025404f;
	public const float kInnerToOuter = 1f / kOuterToInner;
	//外径
	public const float kOuterRadius = 10f;
	//内径根号3/2
	public const float kInnerRadius = kOuterRadius * kOuterToInner;
	//内圆半径
	public const float kSolidFactor = 0.8f;
	//接边半径
	public const float kBlendFactor = 1f - kSolidFactor;
	//高度差台阶常量
	public const int kTerracesPerSlope = 2;
	public const int kTerraceSteps = kTerracesPerSlope * 2 + 1;
	public const float kHorizontalTerraceStepSize = 1f / kTerraceSteps;
	public const float kVerticalTerraceStepSize = 1f / (kTerracesPerSlope + 1);
	//海拔高度梯度
	public const float kElevationStep = 4f;
	//河沟深度
	public const float kStreamBedElevationOffset = -1f;
	//河流高度
	public const float kWaterElevationOffset = -0.5f;
	//为了防止谁边缘被裁看着不好看，把边缘拉长一点
	public const float kWaterFactor = 0.6f;
	//水域边缘
	public const float kWaterBlendFactor = 1f - kWaterFactor;
	//河流的UV步长
	public const float kRiverSolidUVStep = kSolidFactor*0.25f;
	//噪音影响系数
	public const float kCellPerturbStrength = 4f;
	//中心点噪音影响高度
	public const float kElevationPerturbStrength = 1.5f;
	//噪音采样系数
	public const float kNoiseScale = 0.003f;
	//噪音贴图
	public static Texture2D kNoiseSource;
	//六个角
	public static Vector3[] corners = {
		new Vector3(0f, 0f, kOuterRadius),
		new Vector3(kInnerRadius, 0f, 0.5f * kOuterRadius),
		new Vector3(kInnerRadius, 0f, -0.5f * kOuterRadius),
		new Vector3(0f, 0f, -kOuterRadius),
		new Vector3(-kInnerRadius, 0f, -0.5f * kOuterRadius),
		new Vector3(-kInnerRadius, 0f, 0.5f * kOuterRadius),
		new Vector3(0f, 0f, kOuterRadius)	//用于循环生成三角面省事的冗余
	};

	public const int kHashGridSize = 256;

	static float[] kHashGrid;

	public static void InitializeHashGrid ( int seed ) 
	{
		kHashGrid = new float[kHashGridSize * kHashGridSize];
		Random.State currentState = Random.state;
		Random.InitState(seed);
		for (int i = 0; i < kHashGrid.Length; i++) 
		{
			kHashGrid[i] = Random.value;
		}
		Random.state = currentState;
	}

	public static Vector3 GetFirstCorner(HexDirection direction)
	{
		return corners[(int)direction];
	}

	public static Vector3 GetSecondCorner(HexDirection direction)
	{
		return corners[(int)direction + 1];
	}

	public static Vector3 GetFirstSolidCorner(HexDirection direction)
	{
		return corners[(int)direction] * kSolidFactor;
	}

	public static Vector3 GetSecondSolidCorner(HexDirection direction)
	{
		return corners[(int)direction + 1] * kSolidFactor;
	}

	public static Vector3 GetFirstWaterCorner (HexDirection direction) 
	{
		return corners[(int)direction] * kWaterFactor;
	}

	public static Vector3 GetSecondWaterCorner (HexDirection direction) 
	{
		return corners[(int)direction + 1] * kWaterFactor;
	}

	public static Vector3 GetWaterBridge (HexDirection direction) 
	{
		return (corners[(int)direction] + corners[(int)direction + 1]) *
			kWaterBlendFactor;
	}

	public static Vector3 GetBridge(HexDirection direction)
	{
		return (corners[(int)direction] + corners[(int)direction + 1]) *
			kBlendFactor;
	}

	public static Vector3 TerraceLerp(Vector3 a, Vector3 b, int step)
	{
		float h = step * HexMetrics.kHorizontalTerraceStepSize;
		a.x += (b.x - a.x) * h;
		a.z += (b.z - a.z) * h;
		float v = ((step + 1) / 2) * HexMetrics.kVerticalTerraceStepSize;
		a.y += (b.y - a.y) * v;
		return a;
	}

	public static Color TerraceLerp(Color a, Color b, int step)
	{
		float h = step * HexMetrics.kHorizontalTerraceStepSize;
		return Color.Lerp(a, b, h);
	}

	public static HexEdgeType GetEdgeType(int elevation1, int elevation2)
	{
		if (elevation1 == elevation2)
		{
			return HexEdgeType.Flat;
		}
		int delta = elevation2 - elevation1;
		if (delta == 1 || delta == -1)
		{
			return HexEdgeType.Slope;
		}
		return HexEdgeType.Cliff;
	}

	public static Vector4 SampleNoise(Vector3 position)
	{
		return kNoiseSource.GetPixelBilinear(position.x * kNoiseScale,position.z * kNoiseScale);
	}

	public static Vector3 GetSolidEdgeMiddle (HexDirection direction) 
	{
		return
			(corners[(int)direction] + corners[(int)direction + 1]) *
			(0.5f * kSolidFactor);
	}

	public static Vector3 Perturb(Vector3 position)
	{
		Vector4 sample = SampleNoise(position);
		position.x += (sample.x * 2f - 1f) * kCellPerturbStrength;
		//position.y += (sample.y * 2f - 1f) * HexMetrics.kCellPerturbStrength;
		position.z += (sample.z * 2f - 1f) * kCellPerturbStrength;
		return position;
	}
}

public static class HexDirectionExtensions
{
	public static HexDirection Opposite(this HexDirection direction)
	{
		return (int)direction < 3 ? (direction + 3) : (direction - 3);
	}

	public static HexDirection Previous(this HexDirection direction)
	{
		return direction == HexDirection.NE ? HexDirection.NW : (direction - 1);
	}

	public static HexDirection Next(this HexDirection direction)
	{
		return direction == HexDirection.NW ? HexDirection.NE : (direction + 1);
	}

	public static HexDirection Previous2 (this HexDirection direction) 
	{
		direction -= 2;
		return direction >= HexDirection.NE ? direction : (direction + 6);
	}

	public static HexDirection Next2 (this HexDirection direction) 
	{
		direction += 2;
		return direction <= HexDirection.NW ? direction : (direction - 6);
	}
}

public struct EdgeVertices
{
	public Vector3 v1, v2, v3, v4, v5;

	public EdgeVertices(Vector3 corner1, Vector3 corner2)
	{
		v1 = corner1;
		v2 = Vector3.Lerp(corner1, corner2, 0.25f);
		v3 = Vector3.Lerp(corner1, corner2, 0.5f);
		v4 = Vector3.Lerp(corner1, corner2, 0.75f);
		v5 = corner2;
	}

	public EdgeVertices (Vector3 corner1, Vector3 corner2, float outerStep) 
	{
		v1 = corner1;
		v2 = Vector3.Lerp(corner1, corner2, outerStep);
		v3 = Vector3.Lerp(corner1, corner2, 0.5f);
		v4 = Vector3.Lerp(corner1, corner2, 1f - outerStep);
		v5 = corner2;
	}

	public static EdgeVertices TerraceLerp(EdgeVertices a, EdgeVertices b, int step)
	{
		EdgeVertices result;
		result.v1 = HexMetrics.TerraceLerp(a.v1, b.v1, step);
		result.v2 = HexMetrics.TerraceLerp(a.v2, b.v2, step);
		result.v3 = HexMetrics.TerraceLerp(a.v3, b.v3, step);
		result.v4 = HexMetrics.TerraceLerp(a.v4, b.v4, step);
		result.v5 = HexMetrics.TerraceLerp(a.v5, b.v5, step);
		return result;
	}
}
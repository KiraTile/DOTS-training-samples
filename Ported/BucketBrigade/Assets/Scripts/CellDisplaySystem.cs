﻿using System;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class CellDisplaySystem : SystemBase
{
	const float k_FlickerRange = 0.4f;
	const float k_FlickerRate = 2f;
	
	protected override void OnStartRunning()
	{
		Entities.WithAll<CellDisplay>()
			.WithoutBurst()
			.ForEach((ref CellDisplay cellDisplay) =>
			{
				SetSingleton<CellDisplay>(cellDisplay);
			})
			.Run();
	}

	protected override void OnUpdate()
	{
		var elapsedTime = (float)Time.ElapsedTime;
		
		CellDisplay cellDisplay = GetSingleton<CellDisplay>();
		float cellDisplayRange = cellDisplay.FireValue - cellDisplay.CoolValue;

		var heatMapEntity = GetSingletonEntity<HeatMap>();
		var heatMap = EntityManager.GetComponentData<HeatMap>(heatMapEntity);
		var heatMapBuffer = EntityManager.GetBuffer<HeatMapElement>(heatMapEntity).AsNativeArray();

		Entities
			.ForEach((Entity entity, ref Translation translation, ref NonUniformScale scale, ref Color color, in CellInfo cell, in RootTranslation rootTranslation) =>
			{
				BoardHelper.TryGet2DArrayIndex(cell.X, cell.Z, heatMap.SizeX, heatMap.SizeZ, out var index);
				float heatValue = heatMapBuffer[index].Value;

				if (heatValue > cellDisplay.OnFireThreshold)
				{
					float t = math.clamp((heatValue - cellDisplay.CoolValue) / cellDisplayRange, 0.0f, 1.0f);
					float top = math.lerp(cellDisplay.CoolHeight, cellDisplay.FireHeight, t) + rootTranslation.Value.y;
					// Animate cell
					top += (k_FlickerRange * 0.5f) + Mathf.PerlinNoise((elapsedTime - index) * k_FlickerRate - heatValue / 100f, heatValue / 100f) * k_FlickerRange;
				
					float bottom = -1.0f;
					translation.Value = new float3(rootTranslation.Value.x, (top + bottom) / 2.0f, rootTranslation.Value.z);
					scale.Value = new float3(scale.Value.x, top - bottom, scale.Value.z);
					color.Value = (cellDisplay.CoolColor * (1.0f - t)) + (cellDisplay.FireColor * t);
				}
				else
				{
					translation.Value = new float3(rootTranslation.Value.x, rootTranslation.Value.y, rootTranslation.Value.z);
					scale.Value = new float3(scale.Value.x, rootTranslation.Value.y + 1, scale.Value.z);
					color.Value = cellDisplay.NeutralColor;
				}
			})
			.ScheduleParallel();
	}
}
﻿using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class PheromoneDecaySystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entity pheromoneEntity = GetSingletonEntity<Pheromones>();
        DynamicBuffer<Pheromones> pheromoneGrid = EntityManager.GetBuffer<Pheromones>(pheromoneEntity);
        
        Entity pheromoneDecayEntity = GetSingletonEntity<PheromoneDecayRate>();
        float pheromoneDecayRate = EntityManager.GetComponentData<PheromoneDecayRate>(pheromoneDecayEntity).pheromoneDecayRate;

        Dependency = Job.WithCode(() =>
        {
            for (int i = 0; i < pheromoneGrid.Length; i++)
            {
                float currentStrength = pheromoneGrid[i].pheromoneStrength;
                
                pheromoneGrid[i] = new Pheromones{pheromoneStrength = currentStrength -pheromoneDecayRate};
            }
        }).Schedule(Dependency);
    }
}
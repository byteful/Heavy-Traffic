using System.Runtime.CompilerServices;
using Colossal.Entities;
using Colossal.Mathematics;
using Game;
using Game.Citizens;
using Game.City;
using Game.Common;
using Game.Creatures;
using Game.Economy;
using Game.Net;
using Game.Objects;
using Game.Pathfind;
using Game.Prefabs;
using Game.Simulation;
using Game.Tools;
using Game.Vehicles;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Assertions;
using UnityEngine.Scripting;
using CargoTransport = Game.Vehicles.CargoTransport;
using CarLaneFlags = Game.Vehicles.CarLaneFlags;
using PublicTransport = Game.Vehicles.PublicTransport;
using Resident = Game.Creatures.Resident;
using TrafficSpawner = Game.Buildings.TrafficSpawner;
// ReSharper disable InconsistentNaming

namespace HeavyTraffic.Systems
{
    public partial class TrafficSpawnerAISystem_HeavyTraffic : GameSystemBase
    {
        private EntityQuery m_BuildingQuery;
        private EntityQuery m_PersonalCarQuery;
        private EntityQuery m_TransportVehicleQuery;
        private EntityQuery m_CreaturePrefabQuery;
        private SimulationSystem m_SimulationSystem;
        private ClimateSystem m_ClimateSystem;
        private CityConfigurationSystem m_CityConfigurationSystem;
        private VehicleCapacitySystem m_VehicleCapacitySystem;
        private EndFrameBarrier m_EndFrameBarrier;
        private EntityArchetype m_TrafficRequestArchetype;
        private EntityArchetype m_HandleRequestArchetype;
        private ComponentTypeSet m_CurrentLaneTypesRelative;
        private PersonalCarSelectData m_PersonalCarSelectData;
        private TransportVehicleSelectData m_TransportVehicleSelectData;
        private TypeHandle __TypeHandle;

        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            return phase == SystemUpdatePhase.LoadSimulation ? 16 : 256;
        }

        public override int GetUpdateOffset(SystemUpdatePhase phase)
        {
            return phase == SystemUpdatePhase.LoadSimulation ? 2 : 32;
        }

        [Preserve]
        protected override void OnCreate()
        {
            base.OnCreate();
            m_SimulationSystem = World.GetOrCreateSystemManaged<SimulationSystem>();
            m_ClimateSystem = World.GetOrCreateSystemManaged<ClimateSystem>();
            m_EndFrameBarrier = World.GetOrCreateSystemManaged<EndFrameBarrier>();
            m_CityConfigurationSystem = World.GetOrCreateSystemManaged<CityConfigurationSystem>();
            m_VehicleCapacitySystem = World.GetOrCreateSystemManaged<VehicleCapacitySystem>();
            m_PersonalCarSelectData = new PersonalCarSelectData(this);
            m_TransportVehicleSelectData = new TransportVehicleSelectData(this);
            m_BuildingQuery = GetEntityQuery(ComponentType.ReadOnly<TrafficSpawner>(),
                ComponentType.ReadOnly<ServiceDispatch>(), ComponentType.Exclude<Temp>(), ComponentType.Exclude<Destroyed>(),
                ComponentType.Exclude<Deleted>());
            m_PersonalCarQuery = GetEntityQuery(PersonalCarSelectData.GetEntityQueryDesc());
            m_TransportVehicleQuery = GetEntityQuery(TransportVehicleSelectData.GetEntityQueryDesc());
            m_CreaturePrefabQuery = GetEntityQuery(ComponentType.ReadOnly<CreatureData>(), ComponentType.ReadOnly<PrefabData>());
            m_TrafficRequestArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<ServiceRequest>(),
                ComponentType.ReadWrite<RandomTrafficRequest>(), ComponentType.ReadWrite<RequestGroup>());
            m_HandleRequestArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<HandleRequest>(),
                ComponentType.ReadWrite<Event>());
            m_CurrentLaneTypesRelative = new ComponentTypeSet(new ComponentType[5]
            {
                ComponentType.ReadWrite<Moving>(),
                ComponentType.ReadWrite<TransformFrame>(),
                ComponentType.ReadWrite<HumanNavigation>(),
                ComponentType.ReadWrite<HumanCurrentLane>(),
                ComponentType.ReadWrite<Blocker>()
            });
            RequireForUpdate(m_BuildingQuery);
            Assert.IsTrue(true);
        }

        [Preserve]
        protected override void OnUpdate()
        {
            JobHandle jobHandle1;
            m_PersonalCarSelectData.PreUpdate(this, m_CityConfigurationSystem, m_PersonalCarQuery,
                Allocator.TempJob, out jobHandle1);
            JobHandle jobHandle2;
            m_TransportVehicleSelectData.PreUpdate(this, m_CityConfigurationSystem, m_TransportVehicleQuery,
                Allocator.TempJob, out jobHandle2);
            JobHandle outJobHandle;
            NativeList<ArchetypeChunk> archetypeChunkListAsync =
                m_CreaturePrefabQuery.ToArchetypeChunkListAsync(Allocator.TempJob,
                    out outJobHandle);
            __TypeHandle.__Game_Prefabs_ActivityLocationElement_RO_BufferLookup.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Pathfind_PathElement_RO_BufferLookup.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Net_Curve_RO_ComponentLookup.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Objects_Transform_RO_ComponentLookup.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Pathfind_PathInformation_RO_ComponentLookup.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Simulation_ServiceRequest_RO_ComponentLookup.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Simulation_RandomTrafficRequest_RO_ComponentLookup.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Prefabs_ObjectData_RO_ComponentLookup.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Prefabs_DeliveryTruckData_RO_ComponentLookup.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentLookup.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Prefabs_TrafficSpawnerData_RO_ComponentLookup.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Simulation_ServiceDispatch_RW_BufferTypeHandle.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Prefabs_ResidentData_RO_ComponentTypeHandle.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Prefabs_CreatureData_RO_ComponentTypeHandle.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Buildings_TrafficSpawner_RO_ComponentTypeHandle.Update(ref CheckedStateRef);
            __TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref CheckedStateRef);
            // ISSUE: reference to a compiler-generated method

            JobHandle jobHandle3 = new TrafficSpawnerTickJob
            {
                m_EntityType = __TypeHandle.__Unity_Entities_Entity_TypeHandle,
                m_TrafficSpawnerType = __TypeHandle.__Game_Buildings_TrafficSpawner_RO_ComponentTypeHandle,
                m_PrefabRefType = __TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle,
                m_CreatureDataType = __TypeHandle.__Game_Prefabs_CreatureData_RO_ComponentTypeHandle,
                m_ResidentDataType = __TypeHandle.__Game_Prefabs_ResidentData_RO_ComponentTypeHandle,
                m_ServiceDispatchType = __TypeHandle.__Game_Simulation_ServiceDispatch_RW_BufferTypeHandle,
                m_PrefabTrafficSpawnerData = __TypeHandle.__Game_Prefabs_TrafficSpawnerData_RO_ComponentLookup,
                m_PrefabRefData = __TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentLookup,
                m_PrefabDeliveryTruckData = __TypeHandle.__Game_Prefabs_DeliveryTruckData_RO_ComponentLookup,
                m_PrefabObjectData = __TypeHandle.__Game_Prefabs_ObjectData_RO_ComponentLookup,
                m_RandomTrafficRequestData = __TypeHandle.__Game_Simulation_RandomTrafficRequest_RO_ComponentLookup,
                m_ServiceRequestData = __TypeHandle.__Game_Simulation_ServiceRequest_RO_ComponentLookup,
                m_PathInformationData = __TypeHandle.__Game_Pathfind_PathInformation_RO_ComponentLookup,
                m_TransformData = __TypeHandle.__Game_Objects_Transform_RO_ComponentLookup,
                m_CurveData = __TypeHandle.__Game_Net_Curve_RO_ComponentLookup,
                m_PathElements = __TypeHandle.__Game_Pathfind_PathElement_RO_BufferLookup,
                m_ActivityLocationElements = __TypeHandle.__Game_Prefabs_ActivityLocationElement_RO_BufferLookup,
                m_Loading = m_SimulationSystem.loadingProgress,
                m_LeftHandTraffic = m_CityConfigurationSystem.leftHandTraffic,
                m_RandomSeed = RandomSeed.Next(),
                m_VehicleRequestArchetype = m_TrafficRequestArchetype,
                m_HandleRequestArchetype = m_HandleRequestArchetype,
                m_DeliveryTruckSelectData = m_VehicleCapacitySystem.GetDeliveryTruckSelectData(),
                m_PersonalCarSelectData = m_PersonalCarSelectData,
                m_TransportVehicleSelectData = m_TransportVehicleSelectData,
                m_CreaturePrefabChunks = archetypeChunkListAsync,
                m_CurrentLaneTypesRelative = m_CurrentLaneTypesRelative,
                m_CommandBuffer = m_EndFrameBarrier.CreateCommandBuffer().AsParallelWriter(),
                m_CustomTrafficRate = Mod.Settings.fake_traffic_spawn_rate
            }.ScheduleParallel(m_BuildingQuery,
                JobUtils.CombineDependencies(Dependency, jobHandle1, jobHandle2, outJobHandle));
            m_PersonalCarSelectData.PostUpdate(jobHandle3);
            m_TransportVehicleSelectData.PostUpdate(jobHandle3);
            m_EndFrameBarrier.AddJobHandleForProducer(jobHandle3);
            archetypeChunkListAsync.Dispose(jobHandle3);
            Dependency = jobHandle3;
        }

        [MethodImpl((MethodImplOptions) 256)]
        private void __AssignQueries(ref SystemState state)
        {
        }

        protected override void OnCreateForCompiler()
        {
            base.OnCreateForCompiler();
            // ISSUE: reference to a compiler-generated method
            __AssignQueries(ref CheckedStateRef);
            // ISSUE: reference to a compiler-generated method
            __TypeHandle.__AssignHandles(ref CheckedStateRef);
        }

        [Preserve]
        public TrafficSpawnerAISystem_HeavyTraffic()
        {
        }

        [BurstCompile]
        private struct TrafficSpawnerTickJob : IJobChunk
        {
            [ReadOnly] public EntityTypeHandle m_EntityType;
            [ReadOnly] public ComponentTypeHandle<TrafficSpawner> m_TrafficSpawnerType;
            [ReadOnly] public ComponentTypeHandle<PrefabRef> m_PrefabRefType;
            [ReadOnly] public ComponentTypeHandle<CreatureData> m_CreatureDataType;
            [ReadOnly] public ComponentTypeHandle<ResidentData> m_ResidentDataType;
            public BufferTypeHandle<ServiceDispatch> m_ServiceDispatchType;
            [ReadOnly] public ComponentLookup<TrafficSpawnerData> m_PrefabTrafficSpawnerData;
            [ReadOnly] public ComponentLookup<PrefabRef> m_PrefabRefData;
            [ReadOnly] public ComponentLookup<DeliveryTruckData> m_PrefabDeliveryTruckData;
            [ReadOnly] public ComponentLookup<ObjectData> m_PrefabObjectData;
            [ReadOnly] public ComponentLookup<RandomTrafficRequest> m_RandomTrafficRequestData;
            [ReadOnly] public ComponentLookup<ServiceRequest> m_ServiceRequestData;
            [ReadOnly] public ComponentLookup<PathInformation> m_PathInformationData;
            [ReadOnly] public ComponentLookup<Transform> m_TransformData;
            [ReadOnly] public ComponentLookup<Curve> m_CurveData;
            [ReadOnly] public BufferLookup<PathElement> m_PathElements;
            [ReadOnly] public BufferLookup<ActivityLocationElement> m_ActivityLocationElements;
            [ReadOnly] public float m_Loading;
            [ReadOnly] public bool m_LeftHandTraffic;
            [ReadOnly] public RandomSeed m_RandomSeed;
            [ReadOnly] public EntityArchetype m_VehicleRequestArchetype;
            [ReadOnly] public EntityArchetype m_HandleRequestArchetype;
            [ReadOnly] public DeliveryTruckSelectData m_DeliveryTruckSelectData;
            [ReadOnly] public PersonalCarSelectData m_PersonalCarSelectData;
            [ReadOnly] public TransportVehicleSelectData m_TransportVehicleSelectData;
            [ReadOnly] public NativeList<ArchetypeChunk> m_CreaturePrefabChunks;
            [ReadOnly] public ComponentTypeSet m_CurrentLaneTypesRelative;
            [ReadOnly] public int m_CustomTrafficRate;
            public EntityCommandBuffer.ParallelWriter m_CommandBuffer;

            public void Execute(
                in ArchetypeChunk chunk,
                int unfilteredChunkIndex,
                bool useEnabledMask,
                in v128 chunkEnabledMask)
            {
                NativeArray<Entity> nativeArray1 = chunk.GetNativeArray(m_EntityType);
                NativeArray<TrafficSpawner> nativeArray2 =
                    chunk.GetNativeArray(ref m_TrafficSpawnerType);
                NativeArray<PrefabRef> nativeArray3 = chunk.GetNativeArray(ref m_PrefabRefType);
                BufferAccessor<ServiceDispatch> bufferAccessor = chunk.GetBufferAccessor(ref m_ServiceDispatchType);
                Random random = m_RandomSeed.GetRandom(unfilteredChunkIndex);
                for (int index = 0; index < nativeArray1.Length; ++index)
                {
                    Entity entity = nativeArray1[index];
                    TrafficSpawner trafficSpawner = nativeArray2[index];
                    PrefabRef prefabRef = nativeArray3[index];
                    DynamicBuffer<ServiceDispatch> dispatches = bufferAccessor[index];
                    // ISSUE: reference to a compiler-generated method
                    Tick(unfilteredChunkIndex, entity, ref random, trafficSpawner, prefabRef, dispatches);
                }
            }

            private void Tick(
                int jobIndex,
                Entity entity,
                ref Random random,
                TrafficSpawner trafficSpawner,
                PrefabRef prefabRef,
                DynamicBuffer<ServiceDispatch> dispatches)
            {
                // MOD START
                float spawnRateMultiplierFloat = m_CustomTrafficRate / 100f;
                int spawnRateMultiplierInt = m_CustomTrafficRate / 100;
                
                int smallPercentageLeft = m_CustomTrafficRate % 100;
                // Spawn rate is discrete, since we either spawn a vehicle(s) or not, so decide by chance if spawn this tick or not
                if (smallPercentageLeft != 0 && random.NextFloat(0, 100) < smallPercentageLeft)
                {
                    spawnRateMultiplierInt++;
                }
                
                if(spawnRateMultiplierInt == 0)
                {
                    return;
                }
                // MOD END
                
                TrafficSpawnerData prefabTrafficSpawnerData = m_PrefabTrafficSpawnerData[prefabRef.m_Prefab];
                
                // ORIG: float num1 = prefabTrafficSpawnerData.m_SpawnRate * 4.266667f;
                // MOD:
                float num1 = prefabTrafficSpawnerData.m_SpawnRate * 4.266667f * spawnRateMultiplierFloat;
                
                float num2 = random.NextFloat(num1 * 0.5f, num1 * 1.5f);
                
                if (MathUtils.RoundToIntRandom(ref random, num2) > 0 &&
                    !m_RandomTrafficRequestData.HasComponent(trafficSpawner.m_TrafficRequest))
                {
                    RequestVehicle(jobIndex, ref random, entity, prefabTrafficSpawnerData);
                }
                
                for (int index1 = 0; index1 < dispatches.Length; ++index1)
                {
                    Entity request = dispatches[index1].m_Request;
                    if (m_RandomTrafficRequestData.HasComponent(request))
                    {
                        int num3 = m_Loading >= 0.8999999761581421
                            ? 1
                            : ((prefabTrafficSpawnerData.m_RoadType & RoadTypes.Airplane) == RoadTypes.None
                                ? ((prefabTrafficSpawnerData.m_TrackType & TrackTypes.Train) == TrackTypes.None ? 2 : 0)
                                : random.NextInt(2));
                        // MOD START
                        num3 *= spawnRateMultiplierInt;
                        // MOD END
                        
                        for (int index2 = 0; index2 < num3; ++index2)
                        {
                            SpawnVehicle(jobIndex, ref random, entity, request, prefabTrafficSpawnerData);
                        }

                        dispatches.RemoveAt(index1--);
                    }
                    else
                    {
                        if (!m_ServiceRequestData.HasComponent(request))
                        {
                            dispatches.RemoveAt(index1--);
                        }
                    }
                }
            }

            private void RequestVehicle(
                int jobIndex,
                ref Random random,
                Entity entity,
                TrafficSpawnerData prefabTrafficSpawnerData)
            {
                SizeClass sizeClass;
                if ((prefabTrafficSpawnerData.m_RoadType & RoadTypes.Car) != RoadTypes.None)
                {
                    int num = random.NextInt(100);
                    sizeClass = num >= 20 ? (num >= 25 ? SizeClass.Small : SizeClass.Medium) : SizeClass.Large;
                }
                else
                {
                    sizeClass = SizeClass.Medium;
                }

                RandomTrafficRequestFlags flags = 0;
                if (prefabTrafficSpawnerData.m_NoSlowVehicles)
                {
                    flags |= RandomTrafficRequestFlags.NoSlowVehicles;
                }

                Entity entity1 = m_CommandBuffer.CreateEntity(jobIndex, m_VehicleRequestArchetype);
                m_CommandBuffer.SetComponent(jobIndex, entity1,
                    new RandomTrafficRequest(entity, prefabTrafficSpawnerData.m_RoadType, prefabTrafficSpawnerData.m_TrackType,
                        EnergyTypes.FuelAndElectricity, sizeClass, flags));
                m_CommandBuffer.SetComponent(jobIndex, entity1, new RequestGroup(16U));
            }

            private void SpawnVehicle(
                int jobIndex,
                ref Random random,
                Entity entity,
                Entity request,
                TrafficSpawnerData prefabTrafficSpawnerData)
            {
                RandomTrafficRequest componentData1;
                PathInformation componentData2;
                if (!m_RandomTrafficRequestData.TryGetComponent(request, out componentData1) ||
                    !m_PathInformationData.TryGetComponent(request, out componentData2) ||
                    !m_PrefabRefData.HasComponent(componentData2.m_Destination))
                {
                    return;
                }

                uint delay = random.NextUInt(256U);
                Entity source = entity;
                Transform transform = m_TransformData[entity];
                int num1 = 0;
                DynamicBuffer<PathElement> bufferData;
                m_PathElements.TryGetBuffer(request, out bufferData);
                if (m_Loading < 0.8999999761581421)
                {
                    delay = 0U;
                    source = Entity.Null;
                    if (bufferData.IsCreated && bufferData.Length >= 5)
                    {
                        num1 = random.NextInt(2, bufferData.Length * 3 / 4);
                        PathElement pathElement = bufferData[num1];
                        Curve componentData3;
                        if (m_CurveData.TryGetComponent(pathElement.m_Target, out componentData3))
                        {
                            float3 a = MathUtils.Tangent(componentData3.m_Bezier, pathElement.m_TargetDelta.x);
                            float3 forward = math.select(a, -a,
                                pathElement.m_TargetDelta.y < (double) pathElement.m_TargetDelta.x);
                            transform.m_Position = MathUtils.Position(componentData3.m_Bezier, pathElement.m_TargetDelta.x);
                            transform.m_Rotation = quaternion.LookRotationSafe(forward, math.up());
                        }
                    }
                }

                Entity vehicle = Entity.Null;
                if (componentData1.m_SizeClass == SizeClass.Large)
                {
                    // ISSUE: reference to a compiler-generated method
                    Resource randomResource = GetRandomResource(ref random);
                    int max;
                    m_DeliveryTruckSelectData.GetCapacityRange(Resource.NoResource, out int _, out max);
                    int amount = random.NextInt(1, max + max / 10 + 1);
                    int returnAmount = 0;
                    DeliveryTruckFlags state = DeliveryTruckFlags.DummyTraffic;
                    if (random.NextInt(100) < 75)
                    {
                        state |= DeliveryTruckFlags.Loaded;
                    }

                    DeliveryTruckSelectItem selectItem;
                    if (m_DeliveryTruckSelectData.TrySelectItem(ref random, randomResource, amount, out selectItem))
                    {
                        vehicle = m_DeliveryTruckSelectData.CreateVehicle(m_CommandBuffer, jobIndex, ref random,
                            ref m_PrefabDeliveryTruckData, ref m_PrefabObjectData, selectItem, randomResource,
                            Resource.NoResource, ref amount, ref returnAmount, transform, source, state, delay);
                    }

                    int maxCount = 1;
                    // ISSUE: reference to a compiler-generated method
                    if (CreatePassengers(jobIndex, vehicle, selectItem.m_Prefab1, transform, true, ref maxCount, ref random) > 0)
                    {
                        m_CommandBuffer.AddBuffer<Passenger>(jobIndex, vehicle);
                    }
                }
                else if (componentData1.m_SizeClass == SizeClass.Medium)
                {
                    TransportType transportType = TransportType.None;
                    PublicTransportPurpose publicTransportPurpose = 0;
                    Resource cargoResources = Resource.NoResource;
                    int2 passengerCapacity = 0;
                    int2 cargoCapacity = 0;
                    if ((componentData1.m_RoadType & RoadTypes.Car) != RoadTypes.None)
                    {
                        transportType = TransportType.Bus;
                        publicTransportPurpose = PublicTransportPurpose.TransportLine;
                        passengerCapacity = new int2(1, int.MaxValue);
                    }
                    else if ((componentData1.m_RoadType & RoadTypes.Airplane) != RoadTypes.None)
                    {
                        transportType = TransportType.Airplane;
                        if (random.NextInt(100) < 25)
                        {
                            cargoResources = Resource.Food;
                            cargoCapacity = new int2(1, int.MaxValue);
                        }
                        else
                        {
                            publicTransportPurpose = PublicTransportPurpose.TransportLine;
                            passengerCapacity = new int2(1, int.MaxValue);
                        }
                    }
                    else if ((componentData1.m_RoadType & RoadTypes.Watercraft) != RoadTypes.None)
                    {
                        transportType = TransportType.Ship;
                        if (random.NextInt(100) < 50)
                        {
                            cargoResources = Resource.Food;
                            cargoCapacity = new int2(1, int.MaxValue);
                        }
                        else
                        {
                            publicTransportPurpose = PublicTransportPurpose.TransportLine;
                            passengerCapacity = new int2(1, int.MaxValue);
                        }
                    }
                    else if ((componentData1.m_TrackType & TrackTypes.Train) != TrackTypes.None)
                    {
                        transportType = TransportType.Train;
                        if (random.NextInt(100) < 50)
                        {
                            cargoResources = Resource.Food;
                            cargoCapacity = new int2(1, int.MaxValue);
                        }
                        else
                        {
                            publicTransportPurpose = PublicTransportPurpose.TransportLine;
                            passengerCapacity = new int2(1, int.MaxValue);
                        }
                    }

                    vehicle = m_TransportVehicleSelectData.CreateVehicle(m_CommandBuffer, jobIndex, ref random, transform, source,
                        Entity.Null, Entity.Null, transportType, componentData1.m_EnergyTypes, publicTransportPurpose, cargoResources,
                        ref passengerCapacity, ref cargoCapacity);
                    if (vehicle != Entity.Null)
                    {
                        if (publicTransportPurpose != 0)
                        {
                            m_CommandBuffer.SetComponent(jobIndex, vehicle,
                                new PublicTransport
                                {
                                    m_State = PublicTransportFlags.DummyTraffic
                                });
                        }

                        if (cargoResources != Resource.NoResource)
                        {
                            m_CommandBuffer.SetComponent(jobIndex, vehicle,
                                new CargoTransport
                                {
                                    m_State = CargoTransportFlags.DummyTraffic
                                });
                            DynamicBuffer<LoadingResources> dynamicBuffer =
                                m_CommandBuffer.SetBuffer<LoadingResources>(jobIndex, vehicle);
                            int min = random.NextInt(1, math.min(5, cargoCapacity.y + 1));
                            int num2 = random.NextInt(min, cargoCapacity.y + cargoCapacity.y / 10 + 1);
                            int num3 = 0;
                            for (int index = 0; index < min; ++index)
                            {
                                int num4 = random.NextInt(1, 100000);
                                num3 += num4;
                                // ISSUE: reference to a compiler-generated method
                                dynamicBuffer.Add(new LoadingResources
                                {
                                    m_Resource = GetRandomResource(ref random),
                                    m_Amount = num4
                                });
                            }

                            for (int index = 0; index < min; ++index)
                            {
                                LoadingResources loadingResources = dynamicBuffer[index];
                                int amount = loadingResources.m_Amount;
                                loadingResources.m_Amount = (int) ((amount * (long) num2 + (num3 >> 1)) / num3);
                                num3 -= amount;
                                num2 -= loadingResources.m_Amount;
                                dynamicBuffer[index] = loadingResources;
                            }
                        }
                    }
                }
                else
                {
                    int maxCount = random.NextInt(1, 6);
                    int baggageAmount = random.NextInt(1, 6);
                    if (random.NextInt(20) == 0)
                    {
                        maxCount += 5;
                        baggageAmount += 5;
                    }
                    else if (random.NextInt(10) == 0)
                    {
                        baggageAmount += 5;
                        if (random.NextInt(10) == 0)
                        {
                            baggageAmount += 5;
                        }
                    }

                    bool noSlowVehicles = prefabTrafficSpawnerData.m_NoSlowVehicles |
                                          (componentData1.m_Flags & RandomTrafficRequestFlags.NoSlowVehicles) != 0;
                    Entity trailer;
                    Entity vehiclePrefab;
                    Entity trailerPrefab;
                    vehicle = m_PersonalCarSelectData.CreateVehicle(m_CommandBuffer, jobIndex, ref random, maxCount,
                        baggageAmount, false, noSlowVehicles, transform, source, Entity.Null, PersonalCarFlags.DummyTraffic, false, delay,
                        out trailer, out vehiclePrefab, out trailerPrefab);
                    // ISSUE: reference to a compiler-generated method
                    CreatePassengers(jobIndex, vehicle, vehiclePrefab, transform, true, ref maxCount, ref random);
                    // ISSUE: reference to a compiler-generated method
                    CreatePassengers(jobIndex, trailer, trailerPrefab, transform, false, ref maxCount, ref random);
                }

                if (vehicle == Entity.Null)
                {
                    return;
                }

                m_CommandBuffer.SetComponent(jobIndex, vehicle, new Target(componentData2.m_Destination));
                m_CommandBuffer.AddComponent(jobIndex, vehicle, new Owner(entity));
                Entity entity1 = m_CommandBuffer.CreateEntity(jobIndex, m_HandleRequestArchetype);
                m_CommandBuffer.SetComponent(jobIndex, entity1, new HandleRequest(request, vehicle, true));
                if (source == Entity.Null)
                {
                    if ((componentData1.m_RoadType & RoadTypes.Car) != RoadTypes.None)
                    {
                        CarCurrentLane component = new CarCurrentLane();
                        component.m_LaneFlags |= CarLaneFlags.ResetSpeed;
                        m_CommandBuffer.SetComponent(jobIndex, vehicle, component);
                    }
                    else if ((componentData1.m_RoadType & RoadTypes.Airplane) != RoadTypes.None)
                    {
                        AircraftCurrentLane component = new AircraftCurrentLane();
                        component.m_LaneFlags |= AircraftLaneFlags.ResetSpeed | AircraftLaneFlags.Flying;
                        m_CommandBuffer.SetComponent(jobIndex, vehicle, component);
                    }
                    else if ((componentData1.m_RoadType & RoadTypes.Watercraft) != RoadTypes.None)
                    {
                        WatercraftCurrentLane component = new WatercraftCurrentLane();
                        component.m_LaneFlags |= WatercraftLaneFlags.ResetSpeed;
                        m_CommandBuffer.SetComponent(jobIndex, vehicle, component);
                    }
                }

                if (!bufferData.IsCreated || bufferData.Length == 0)
                {
                    return;
                }

                DynamicBuffer<PathElement> targetElements = m_CommandBuffer.SetBuffer<PathElement>(jobIndex, vehicle);
                PathUtils.CopyPath(bufferData, new PathOwner(), 0, targetElements);
                m_CommandBuffer.SetComponent(jobIndex, vehicle, new PathOwner(num1, PathFlags.Updated));
                if (componentData1.m_SizeClass != SizeClass.Large)
                {
                    return;
                }

                m_CommandBuffer.SetComponent(jobIndex, vehicle, componentData2);
            }

            private int CreatePassengers(
                int jobIndex,
                Entity vehicleEntity,
                Entity vehiclePrefab,
                Transform transform,
                bool driver,
                ref int maxCount,
                ref Random random)
            {
                int passengers = 0;
                DynamicBuffer<ActivityLocationElement> bufferData;
                if (maxCount > 0 && m_ActivityLocationElements.TryGetBuffer(vehiclePrefab, out bufferData))
                {
                    ActivityMask activityMask = new ActivityMask(ActivityType.Driving);
                    int num1 = 0;
                    int num2 = -1;
                    float num3 = float.MinValue;
                    for (int index = 0; index < bufferData.Length; ++index)
                    {
                        ActivityLocationElement activityLocationElement = bufferData[index];
                        if (((int) activityLocationElement.m_ActivityMask.m_Mask & (int) activityMask.m_Mask) != 0)
                        {
                            ++num1;
                            bool c =
                                (activityLocationElement.m_ActivityFlags & ActivityFlags.InvertLefthandTraffic) != 0 &&
                                m_LeftHandTraffic ||
                                (activityLocationElement.m_ActivityFlags & ActivityFlags.InvertRighthandTraffic) != 0 &&
                                !m_LeftHandTraffic;
                            activityLocationElement.m_Position.x = math.select(activityLocationElement.m_Position.x,
                                -activityLocationElement.m_Position.x, c);
                            if ((math.abs(activityLocationElement.m_Position.x) < 0.5 ||
                                 activityLocationElement.m_Position.x >= 0.0 == m_LeftHandTraffic) &&
                                activityLocationElement.m_Position.z > (double) num3)
                            {
                                num2 = index;
                                num3 = activityLocationElement.m_Position.z;
                            }
                        }
                    }

                    int num4 = 100;
                    if (driver && num2 != -1)
                    {
                        --maxCount;
                        --num1;
                    }

                    if (num1 > maxCount)
                    {
                        num4 = maxCount * 100 / num1;
                    }

                    for (int index = 0; index < bufferData.Length; ++index)
                    {
                        ActivityLocationElement activityLocationElement = bufferData[index];
                        if (((int) activityLocationElement.m_ActivityMask.m_Mask & (int) activityMask.m_Mask) != 0 &&
                            (driver && index == num2 || random.NextInt(100) >= num4))
                        {
                            Relative component1;
                            component1.m_Position = activityLocationElement.m_Position;
                            component1.m_Rotation = activityLocationElement.m_Rotation;
                            component1.m_BoneIndex = new int3(0, -1, -1);
                            Citizen citizenData = new Citizen();
                            if (random.NextBool())
                            {
                                citizenData.m_State |= CitizenFlags.Male;
                            }

                            if (driver)
                            {
                                citizenData.SetAge(CitizenAge.Adult);
                            }
                            else
                            {
                                citizenData.SetAge((CitizenAge) random.NextInt(4));
                            }

                            citizenData.m_PseudoRandom = (ushort) (random.NextUInt() % 65536U);
                            PseudoRandomSeed randomSeed;
                            // ISSUE: reference to a compiler-generated method
                            Entity entity1 = ObjectEmergeSystem.SelectResidentPrefab(citizenData, m_CreaturePrefabChunks,
                                m_EntityType, ref m_CreatureDataType, ref m_ResidentDataType, out CreatureData _,
                                out randomSeed);
                            ObjectData objectData = m_PrefabObjectData[entity1];
                            PrefabRef component2 = new PrefabRef
                            {
                                m_Prefab = entity1
                            };
                            Resident component3 = new Resident();
                            component3.m_Flags |= ResidentFlags.InVehicle | ResidentFlags.DummyTraffic;
                            CurrentVehicle component4 = new CurrentVehicle();
                            component4.m_Vehicle = vehicleEntity;
                            component4.m_Flags |= CreatureVehicleFlags.Ready;
                            if (driver && index == num2)
                            {
                                component4.m_Flags |= CreatureVehicleFlags.Leader | CreatureVehicleFlags.Driver;
                            }

                            Entity entity2 = m_CommandBuffer.CreateEntity(jobIndex, objectData.m_Archetype);
                            m_CommandBuffer.RemoveComponent(jobIndex, entity2, in m_CurrentLaneTypesRelative);
                            m_CommandBuffer.SetComponent(jobIndex, entity2, transform);
                            m_CommandBuffer.SetComponent(jobIndex, entity2, component2);
                            m_CommandBuffer.SetComponent(jobIndex, entity2, component3);
                            m_CommandBuffer.SetComponent(jobIndex, entity2, randomSeed);
                            m_CommandBuffer.AddComponent(jobIndex, entity2, component4);
                            m_CommandBuffer.AddComponent(jobIndex, entity2, component1);
                            ++passengers;
                        }
                    }
                }

                return passengers;
            }

            private Resource GetRandomResource(ref Random random)
            {
                switch (random.NextInt(30))
                {
                    case 0:
                        return Resource.Grain;
                    case 1:
                        return Resource.ConvenienceFood;
                    case 2:
                        return Resource.Food;
                    case 3:
                        return Resource.Vegetables;
                    case 4:
                        return Resource.Meals;
                    case 5:
                        return Resource.Wood;
                    case 6:
                        return Resource.Timber;
                    case 7:
                        return Resource.Paper;
                    case 8:
                        return Resource.Furniture;
                    case 9:
                        return Resource.Vehicles;
                    case 10:
                        return Resource.UnsortedMail;
                    case 11:
                        return Resource.Oil;
                    case 12:
                        return Resource.Petrochemicals;
                    case 13:
                        return Resource.Ore;
                    case 14:
                        return Resource.Plastics;
                    case 15:
                        return Resource.Metals;
                    case 16:
                        return Resource.Electronics;
                    case 17:
                        return Resource.Coal;
                    case 18:
                        return Resource.Stone;
                    case 19:
                        return Resource.Livestock;
                    case 20:
                        return Resource.Cotton;
                    case 21:
                        return Resource.Steel;
                    case 22:
                        return Resource.Minerals;
                    case 23:
                        return Resource.Concrete;
                    case 24:
                        return Resource.Machinery;
                    case 25:
                        return Resource.Chemicals;
                    case 26:
                        return Resource.Pharmaceuticals;
                    case 27:
                        return Resource.Beverages;
                    case 28:
                        return Resource.Textiles;
                    case 29:
                        return Resource.Garbage;
                    default:
                        return Resource.NoResource;
                }
            }

            void IJobChunk.Execute(
                in ArchetypeChunk chunk,
                int unfilteredChunkIndex,
                bool useEnabledMask,
                in v128 chunkEnabledMask)
            {
                // ISSUE: reference to a compiler-generated method
                Execute(in chunk, unfilteredChunkIndex, useEnabledMask, in chunkEnabledMask);
            }
        }

        private struct TypeHandle
        {
            [ReadOnly] public EntityTypeHandle __Unity_Entities_Entity_TypeHandle;
            [ReadOnly] public ComponentTypeHandle<TrafficSpawner> __Game_Buildings_TrafficSpawner_RO_ComponentTypeHandle;
            [ReadOnly] public ComponentTypeHandle<PrefabRef> __Game_Prefabs_PrefabRef_RO_ComponentTypeHandle;
            [ReadOnly] public ComponentTypeHandle<CreatureData> __Game_Prefabs_CreatureData_RO_ComponentTypeHandle;
            [ReadOnly] public ComponentTypeHandle<ResidentData> __Game_Prefabs_ResidentData_RO_ComponentTypeHandle;
            public BufferTypeHandle<ServiceDispatch> __Game_Simulation_ServiceDispatch_RW_BufferTypeHandle;
            [ReadOnly] public ComponentLookup<TrafficSpawnerData> __Game_Prefabs_TrafficSpawnerData_RO_ComponentLookup;
            [ReadOnly] public ComponentLookup<PrefabRef> __Game_Prefabs_PrefabRef_RO_ComponentLookup;
            [ReadOnly] public ComponentLookup<DeliveryTruckData> __Game_Prefabs_DeliveryTruckData_RO_ComponentLookup;
            [ReadOnly] public ComponentLookup<ObjectData> __Game_Prefabs_ObjectData_RO_ComponentLookup;
            [ReadOnly] public ComponentLookup<RandomTrafficRequest> __Game_Simulation_RandomTrafficRequest_RO_ComponentLookup;
            [ReadOnly] public ComponentLookup<ServiceRequest> __Game_Simulation_ServiceRequest_RO_ComponentLookup;
            [ReadOnly] public ComponentLookup<PathInformation> __Game_Pathfind_PathInformation_RO_ComponentLookup;
            [ReadOnly] public ComponentLookup<Transform> __Game_Objects_Transform_RO_ComponentLookup;
            [ReadOnly] public ComponentLookup<Curve> __Game_Net_Curve_RO_ComponentLookup;
            [ReadOnly] public BufferLookup<PathElement> __Game_Pathfind_PathElement_RO_BufferLookup;
            [ReadOnly] public BufferLookup<ActivityLocationElement> __Game_Prefabs_ActivityLocationElement_RO_BufferLookup;

            [MethodImpl((MethodImplOptions) 256)]
            public void __AssignHandles(ref SystemState state)
            {
                __Unity_Entities_Entity_TypeHandle = state.GetEntityTypeHandle();
                __Game_Buildings_TrafficSpawner_RO_ComponentTypeHandle =
                    state.GetComponentTypeHandle<TrafficSpawner>(true);
                __Game_Prefabs_PrefabRef_RO_ComponentTypeHandle = state.GetComponentTypeHandle<PrefabRef>(true);
                __Game_Prefabs_CreatureData_RO_ComponentTypeHandle = state.GetComponentTypeHandle<CreatureData>(true);
                __Game_Prefabs_ResidentData_RO_ComponentTypeHandle = state.GetComponentTypeHandle<ResidentData>(true);
                __Game_Simulation_ServiceDispatch_RW_BufferTypeHandle = state.GetBufferTypeHandle<ServiceDispatch>();
                __Game_Prefabs_TrafficSpawnerData_RO_ComponentLookup = state.GetComponentLookup<TrafficSpawnerData>(true);
                __Game_Prefabs_PrefabRef_RO_ComponentLookup = state.GetComponentLookup<PrefabRef>(true);
                __Game_Prefabs_DeliveryTruckData_RO_ComponentLookup = state.GetComponentLookup<DeliveryTruckData>(true);
                __Game_Prefabs_ObjectData_RO_ComponentLookup = state.GetComponentLookup<ObjectData>(true);
                __Game_Simulation_RandomTrafficRequest_RO_ComponentLookup = state.GetComponentLookup<RandomTrafficRequest>(true);
                __Game_Simulation_ServiceRequest_RO_ComponentLookup = state.GetComponentLookup<ServiceRequest>(true);
                __Game_Pathfind_PathInformation_RO_ComponentLookup = state.GetComponentLookup<PathInformation>(true);
                __Game_Objects_Transform_RO_ComponentLookup = state.GetComponentLookup<Transform>(true);
                __Game_Net_Curve_RO_ComponentLookup = state.GetComponentLookup<Curve>(true);
                __Game_Pathfind_PathElement_RO_BufferLookup = state.GetBufferLookup<PathElement>(true);
                __Game_Prefabs_ActivityLocationElement_RO_BufferLookup = state.GetBufferLookup<ActivityLocationElement>(true);
            }
        }
    }
}

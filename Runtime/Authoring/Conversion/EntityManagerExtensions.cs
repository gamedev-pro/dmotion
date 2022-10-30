using System.Runtime.CompilerServices;
using Unity.Entities;

namespace DMotion.Authoring
{
    // public static class EntityManagerExtensions
    // {
    //     [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //     public static T AddComponentIfNotPresent<T>(this EntityManager dstManager, Entity entity)
    //         where T : struct, IComponentData
    //     {
    //         return dstManager.AddOrSetComponentData<T>(entity, default);
    //     }
    //
    //
    //     [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //     public static T AddComponentDataIfNotPresent<T>(this EntityManager dstManager, Entity entity, T comp)
    //         where T : struct, IComponentData
    //     {
    //          if (!dstManager.HasComponent<T>(entity))
    //          {
    //              return dstManager.AddComponentData(entity, comp);
    //          }
    //
    //          return default;
    //     }
    //     
    //     [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //     public static T AddOrSetComponentData<T>(this EntityManager dstManager, Entity entity, T comp)
    //         where T : struct, IComponentData
    //     {
    //         if (dstManager.HasComponent<T>(entity))
    //         {
    //             dstManager.SetComponentData(entity, comp);
    //         }
    //         else
    //         {
    //             dstManager.AddComponentData(entity, comp);
    //         }
    //     }
    // }
}
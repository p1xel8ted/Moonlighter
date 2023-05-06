using System;
using HarmonyLib;
using Moonlighter.Utilities;
using UnityEngine;

namespace Moonlighter.Patches;

[HarmonyPatch]
public static class DungeonPatches
{
    private const float BaseAspectRatio = 16f / 9f;
    private const float DoorCorrectionPercentage = 0.015f;
    private static float PlayerAspectRatio { get; set; }
    private static float ScaleFactor { get; set; }


    //in case user changes resolution
    private static void UpdateAspectValues()
    {
        PlayerAspectRatio = (float) Screen.width / Screen.height;
        ScaleFactor = PlayerAspectRatio / BaseAspectRatio;
    }

    private static bool IsUltrawide()
    {
        UpdateAspectValues();
        return PlayerAspectRatio > BaseAspectRatio && Plugin.CorrectDungeons.Value && Plugin.UltrawideFixes.Value;
    }

    private static float GetAdjustedLeftDoorPosition(float baseDoorPosition)
    {
        var pos = Mathf.Abs(baseDoorPosition);
        UpdateAspectValues();
        var offset = pos * (ScaleFactor - 1f);
        var leftPosition = -pos - offset;
        leftPosition *= 1 - DoorCorrectionPercentage;
        return leftPosition;
    }

    private static float GetAdjustedRightDoorPosition(float baseDoorPosition)
    {
        var pos = Mathf.Abs(baseDoorPosition);
        UpdateAspectValues();
        var offset = pos * (ScaleFactor - 1f);
        var rightPosition = pos + offset;
        rightPosition *= 1 - DoorCorrectionPercentage;
        return rightPosition;
    }


    private static float GetAdjustedLeftPosition(float basePosition)
    {
        UpdateAspectValues();
        var offset = basePosition * (ScaleFactor - 1f);
        var leftPosition = -basePosition - offset;
        return leftPosition;
    }

    private static float GetAdjustedRightPosition(float basePosition)
    {
        UpdateAspectValues();
        var offset = basePosition * (ScaleFactor - 1f);
        var rightPosition = basePosition + offset;
        return rightPosition;
    }

    private static void UpdatePosition(Component door)
    {
        if (!IsUltrawide()) return;
        
        if (door != null)
        {
            var t = door.transform;
            if (TransformIsRightAngled(t))
            {
                if (TransformIsLeftSide(t))
                {
                    Plugin.LOG.LogWarning($"-- (UpdatePosition) Left Door: {door.name}, Original LocalPosition: {t.localPosition}");
                    UpdateDoorPosition(door, Plugin.FinalLeftDoorPosition, GetAdjustedLeftDoorPosition);
                    if (door is DungeonDoor dungeonDoor)
                    {
                        UpdateDoorEntryPosition(dungeonDoor.playerEnterDestinyRoomPoint, Plugin.FinalLeftDoorEntryPosition);
                    }

                    Plugin.LOG.LogWarning($"-- (UpdatePosition) Left Door: {door.name}, New LocalPosition: {t.localPosition}");
                }

                if (TransformIsRightSide(t))
                {
                    Plugin.LOG.LogWarning($"-- (UpdatePosition) Right Door: {door.name}, Original LocalPosition: {t.localPosition}");
                    UpdateDoorPosition(door, Plugin.FinalRightDoorPosition, GetAdjustedRightDoorPosition);
                    if (door is DungeonDoor dungeonDoor)
                    {
                        UpdateDoorEntryPosition(dungeonDoor.playerEnterDestinyRoomPoint, Plugin.FinalRightDoorEntryPosition);
                    }
            
                    Plugin.LOG.LogWarning($"-- (UpdatePosition) Right Door: {door.name}, New LocalPosition: {t.localPosition}");
                }
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(DungeonManager), nameof(DungeonManager.ChangeRoomTo))]
    public static void DungeonManager_ChangeRoomTo(ref DungeonRoom to)
    {
        if (!IsUltrawide()) return;
        UpdatePosition(to.specialDoor);
        UpdatePosition(to.lastLevelDoor);
        UpdatePosition(to.firstLevelDoor);
        UpdatePosition(to.GetWestDoor());
        UpdatePosition(to.GetEastDoor());
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(DungeonLastLevelDoor), nameof(DungeonLastLevelDoor.OnGeneratedDungeon))]
    public static void DungeonLastLevelDoor_OnGeneratedDungeon(ref DungeonLastLevelDoor __instance)
    {
        if (!IsUltrawide()) return;
        UpdatePosition(__instance.transform);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NextLevelDoor), nameof(NextLevelDoor.Init))]
    public static void NextLevelDoor_Init(ref NextLevelDoor __instance)
    {
        if (!IsUltrawide()) return;
        UpdatePosition(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(DungeonBossRoomEnterDoor), nameof(DungeonBossRoomEnterDoor.Start))]
    public static void DungeonBossRoomEnterDoor_Start(ref DungeonBossRoomEnterDoor __instance)
    {
        if (!IsUltrawide()) return;
        UpdatePosition(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(DungeonBossRoomExitDoor), nameof(DungeonBossRoomExitDoor.Start))]
    public static void DungeonBossRoomExitDoor_Start(ref DungeonBossRoomExitDoor __instance)
    {
        if (!IsUltrawide()) return;
        UpdatePosition(__instance);
    }


    private static bool TransformIsRightAngled(Transform transform)
    {
        var zRotation = Mathf.Abs(transform.localEulerAngles.z % 360);
        return Mathf.Abs(zRotation - 90) <= 0.01 || Mathf.Abs(zRotation - 270) <= 0.01;

    }

    private static bool TransformIsLeftSide(Transform transform)
    {
        return transform.localPosition.x < 0;
    }

    private static bool TransformIsRightSide(Transform transform)
    {
        return transform.localPosition.x > 0;
    }

    private static void UpdateFloorWidth(DungeonRoom dungeonRoom)
    {
        if (!IsUltrawide()) return;
        UpdateAspectValues();
        var newFloorWidth = Mathf.RoundToInt(484 * ScaleFactor);
        dungeonRoom.roomFloorWidth = newFloorWidth;
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(DungeonRoom), nameof(DungeonRoom.EnterRoom))]
    public static void DungeonRoom_EnterRoom(ref DungeonRoom __instance)
    {
        if (!IsUltrawide()) return;
        //type DungeonDoor
        UpdatePosition(__instance.specialDoor);
        UpdatePosition(__instance.GetWestDoor());
        UpdatePosition(__instance.GetEastDoor());
        //type not DungeonDoor
        UpdatePosition(__instance.lastLevelDoor);
        UpdatePosition(__instance.firstLevelDoor);
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(DungeonDoor), nameof(DungeonDoor.Start))]
    public static void DungeonDoor_Start(ref DungeonDoor __instance)
    {
        if (!IsUltrawide()) return;
        
        UpdatePosition(__instance);
        
        // switch (__instance.name)
        // {
        //     case "LevelDoorLeft":
        //         UpdateDoorPosition(__instance, Plugin.FinalLeftDoorPosition, GetAdjustedLeftDoorPosition);
        //         UpdateDoorEntryPosition(__instance.playerEnterDestinyRoomPoint, Plugin.FinalLeftDoorEntryPosition);
        //         break;
        //
        //     case "LevelDoorRight":
        //         UpdateDoorPosition(__instance, Plugin.FinalRightDoorPosition, GetAdjustedRightDoorPosition);
        //         UpdateDoorEntryPosition(__instance.playerEnterDestinyRoomPoint, Plugin.FinalRightDoorEntryPosition);
        //         break;
        // }
    }

    private static void UpdateDoorPosition(Component door, WriteOnce<float> finalDoorPosition, Func<float, float> getAdjustedPosition)
    {
        if (door == null) return;
        if (finalDoorPosition.HasValue)
        {
          
            var transform = door.transform;
            var localPosition = transform.localPosition;
            localPosition = new Vector3(finalDoorPosition.Value, localPosition.y, localPosition.z);
            transform.localPosition = localPosition;
            Plugin.LOG.LogWarning($"FinalDoorPosition has value - Door ({door.name}) - setting door position to: {localPosition}");
        }
        else
        {
            var transform = door.transform;
            var localPosition = transform.localPosition;
            var doorPositionX = getAdjustedPosition(localPosition.x);
            localPosition = new Vector3(doorPositionX,  localPosition.y, localPosition.z);
            transform.localPosition = localPosition;
            finalDoorPosition.Value = doorPositionX;
            Plugin.LOG.LogWarning($"FinalDoorPosition does not have value (setting FinalDoorPosition to {doorPositionX}) - Door ({door.name}) - setting door position to: {localPosition}");
        }
    }


    private static void UpdateDoorEntryPosition(Transform playerEnterDestinyRoomPoint, WriteOnce<float> finalDoorEntryPosition)
    {
        if(playerEnterDestinyRoomPoint == null) return;
        
        if (finalDoorEntryPosition.HasValue)
        {
            var localPosition = playerEnterDestinyRoomPoint.localPosition;
            localPosition = new Vector3(finalDoorEntryPosition.Value, localPosition.y, localPosition.z);
            playerEnterDestinyRoomPoint.localPosition = localPosition;
        }
        else
        {
            var localPosition = playerEnterDestinyRoomPoint.localPosition;
            var newX = localPosition.x * ScaleFactor;
            localPosition = new Vector3(newX, localPosition.y, localPosition.z);
            playerEnterDestinyRoomPoint.localPosition = localPosition;
            finalDoorEntryPosition.Value = newX;
        }
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(DungeonRoom), nameof(DungeonRoom.FillRoomWithBreakables))]
    [HarmonyPatch(typeof(DungeonRoom), nameof(DungeonRoom.FillRoomWithCreatures))]
    [HarmonyPatch(typeof(DungeonRoom), nameof(DungeonRoom.FillRoomWithSprites))]
    [HarmonyPatch(typeof(DungeonRoom), nameof(DungeonRoom.FillRoomTextureWithSprites))]
    [HarmonyPatch(typeof(DungeonRoom), nameof(DungeonRoom.FillRoomZonesWithSprites))]
    public static void DungeonRoom_FillPatches(ref DungeonRoom __instance)
    {
        if (!IsUltrawide()) return;
        UpdateFloorWidth(__instance);
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(DungeonRoom), nameof(DungeonRoom.InitializeRoom), typeof(DungeonRoomInfo))]
    [HarmonyPatch(typeof(DungeonRoom), nameof(DungeonRoom.InitializeRoom), new Type[] { })]
    public static void DungeonRoom_InitializeRoom(ref DungeonRoom __instance)
    {
        if (!IsUltrawide()) return;

        foreach (var obj in Helpers.FindObjects("DungeonLevelArt"))
        {
            obj.transform.localScale = new Vector3(ScaleFactor, 1f, 1);
        }

        foreach (var obj in Helpers.FindObjects("DungeonLevelBoundaries"))
        {
            obj.transform.localScale = new Vector3(ScaleFactor, 1f, 1);
        }

        foreach (var obj in Helpers.FindObjectsInPath("WallDetails/"))
        {
            var t = obj.transform;
            if (TransformIsRightAngled(t))
            {
                if (TransformIsLeftSide(t))
                {
                    var localPosition = obj.transform.localPosition;
                    var newPosition = GetAdjustedLeftPosition(localPosition.x);
                    localPosition = new Vector3(newPosition, localPosition.y, localPosition.z);
                    obj.transform.localPosition = localPosition;
                }

                if (TransformIsRightSide(t))
                {
                    var localPosition = obj.transform.localPosition;
                    var newPosition = GetAdjustedRightPosition(localPosition.x);
                    localPosition = new Vector3(newPosition, localPosition.y, localPosition.z);
                    obj.transform.localPosition = localPosition;
                }
            }
        }
    }
}
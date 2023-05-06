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
        return PlayerAspectRatio > BaseAspectRatio;
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
        UpdatePosition(__instance.transform);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NextLevelDoor), nameof(NextLevelDoor.Init))]
    public static void NextLevelDoor_Init(ref NextLevelDoor __instance)
    {
        Plugin.LOG.LogWarning(
            $"NextLevelDoor_Init: {__instance.name}, LocalPosition: {__instance.transform.localPosition}");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(DungeonBossRoomEnterDoor), nameof(DungeonBossRoomEnterDoor.Start))]
    public static void DungeonBossRoomEnterDoor_Start(ref DungeonBossRoomEnterDoor __instance)
    {
        Plugin.LOG.LogWarning(
            $"DungeonBossRoomEnterDoor.Start: {__instance.name}, LocalPosition: {__instance.transform.localPosition}");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(DungeonBossRoomExitDoor), nameof(DungeonBossRoomExitDoor.Start))]
    public static void DungeonBossRoomExitDoor_Start(ref DungeonBossRoomExitDoor __instance)
    {
        Plugin.LOG.LogWarning(
            $"DungeonBossRoomExitDoor.Start: {__instance.name}, LocalPosition: {__instance.transform.localPosition}");
    }


    private static bool TransformIsRightAngled(Transform transform)
    {
        return Math.Abs(transform.localRotation.z - 1) > 0.01;
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
        UpdateAspectValues();
        var newFloorWidth = Mathf.RoundToInt(484 * ScaleFactor);
        dungeonRoom.roomFloorWidth = newFloorWidth;
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(DungeonRoom), nameof(DungeonRoom.EnterRoom))]
    public static void DungeonRoom_EnterRoom(ref DungeonRoom __instance)
    {
        UpdatePosition(__instance.specialDoor);
        UpdatePosition(__instance.lastLevelDoor);
        UpdatePosition(__instance.firstLevelDoor);
        UpdatePosition(__instance.GetWestDoor());
        UpdatePosition(__instance.GetEastDoor());
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(DungeonDoor), nameof(DungeonDoor.Start))]
    public static void DungeonDoor_Start(ref DungeonDoor __instance)
    {
        if (!IsUltrawide()) return;
        
        switch (__instance.name)
        {
            case "LevelDoorLeft":
                UpdateDoorPosition(__instance, Plugin.FinalLeftDoorPosition, GetAdjustedLeftDoorPosition);
                UpdateDoorEntryPosition(__instance.playerEnterDestinyRoomPoint, Plugin.FinalLeftDoorEntryPosition);
                break;

            case "LevelDoorRight":
                UpdateDoorPosition(__instance, Plugin.FinalRightDoorPosition, GetAdjustedRightDoorPosition);
                UpdateDoorEntryPosition(__instance.playerEnterDestinyRoomPoint, Plugin.FinalRightDoorEntryPosition);
                break;
        }
    }

    private static void UpdateDoorPosition(Component door, WriteOnce<float> finalDoorPosition, Func<float, float> getAdjustedPosition)
    {
        if (finalDoorPosition.HasValue)
        {
            door.transform.localPosition = new Vector3(finalDoorPosition.Value, 0f, 0f);
        }
        else
        {
            var transform = door.transform;
            var doorPosition = getAdjustedPosition(transform.localPosition.x);
            transform.localPosition = new Vector3(doorPosition, 0f, 0f);

            finalDoorPosition.Value = doorPosition;
        }
    }


    private static void UpdateDoorEntryPosition(Transform playerEnterDestinyRoomPoint, WriteOnce<float> finalDoorEntryPosition)
    {
        if (finalDoorEntryPosition.HasValue)
        {
            playerEnterDestinyRoomPoint.localPosition = new Vector3(finalDoorEntryPosition.Value, 0f, 0f);
        }
        else
        {
            var newX = playerEnterDestinyRoomPoint.localPosition.x * ScaleFactor;
            playerEnterDestinyRoomPoint.localPosition = new Vector3(newX, 0f, 0f);
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
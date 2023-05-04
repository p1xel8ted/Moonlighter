using System;
using HarmonyLib;
using Moonlighter.Utilities;
using UnityEngine;

namespace Moonlighter;

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

    private static int GetAdjustedFloorWidth()
    {
        UpdateAspectValues();
        return Mathf.RoundToInt(484 * ScaleFactor);
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


    [HarmonyPostfix]
    [HarmonyPatch(typeof(DungeonManager), nameof(DungeonManager.ChangeRoomTo))]
    public static void DungeonRoom_EnableRoomDoorsColliders(ref DungeonRoom to)
    {
        Plugin.LOG.LogWarning("Regular Doors Begin");
        foreach (var door in to.roomDoors.Where(a => a != null && a.gameObject.activeSelf))
        {
            Plugin.LOG.LogWarning(door == null
                ? "-- Door: NONE"
                : $"-- Door: {door.name}, LocalPosition: {door.transform.localPosition}");
        }

        Plugin.LOG.LogWarning("Regular Doors End\n\n");

        Plugin.LOG.LogWarning("Special Doors Begin");
        Plugin.LOG.LogWarning(to.specialDoor == null
            ? "-- Special Door: NONE"
            : $"-- Special Door: {to.specialDoor.name}, LocalPosition: {to.specialDoor.transform.localPosition}");
        Plugin.LOG.LogWarning("Special Doors End\n\n");

        Plugin.LOG.LogWarning("First and Last Doors Begin");
        Plugin.LOG.LogWarning(to.firstLevelDoor == null
            ? "-- FirstLevel Door: NONE"
            : $"-- FirstLevel Door: {to.firstLevelDoor.name}, LocalPosition: {to.firstLevelDoor.transform.localPosition}");
        Plugin.LOG.LogWarning(to.lastLevelDoor == null
            ? "-- LastLevel Door: NONE"
            : $"-- LastLevel Door: {to.lastLevelDoor.name}, LocalPosition: {to.lastLevelDoor.transform.localPosition}");


        var lastLevelDoor = Helpers.FindObjects("LastLevelDoor", true).FirstOrDefault();
        if (lastLevelDoor != null)
        {
            if (Math.Abs(lastLevelDoor.transform.localRotation.z - 1) > 0.01)
            {
                Plugin.LOG.LogWarning(
                    $"-- LastLevel Door: {lastLevelDoor.name}, LocalPosition: {lastLevelDoor.transform.localPosition}");
                if (lastLevelDoor.transform.localPosition.x < 0)
                {
                    var leftDoorPosition = GetAdjustedLeftDoorPosition(lastLevelDoor.transform.localPosition.x);
                    lastLevelDoor.transform.localPosition = new Vector3(leftDoorPosition, 0, 0);
                }

                if (lastLevelDoor.transform.localPosition.x > 0)
                {
                    var rightDoorPosition = GetAdjustedRightDoorPosition(lastLevelDoor.transform.localPosition.x);
                    lastLevelDoor.transform.localPosition = new Vector3(rightDoorPosition, 0, 0);
                }
            }
        }

        Plugin.LOG.LogWarning("First and Last Doors End\n\n");


        if (to.specialDoor != null)
        {
            var sd = to.specialDoor;
            if (sd.transform.localPosition.x < 0)
            {
                var leftDoorPosition = GetAdjustedLeftDoorPosition(sd.transform.localPosition.x);
                sd.transform.localPosition = new Vector3(leftDoorPosition, 0, 0);
            }

            if (sd.transform.localPosition.x > 0)
            {
                var rightDoorPosition = GetAdjustedRightDoorPosition(sd.transform.localPosition.x);
                sd.transform.localPosition = new Vector3(rightDoorPosition, 0, 0);
            }
        }
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
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(DungeonRoom), nameof(DungeonRoom.EnterRoom))]
    public static void DungeonRoom_EnterRoom(ref DungeonRoom __instance)
    {
        if (__instance.roomInfo.name.Equals("Init"))
        {
            var lastLevelDoor = Helpers.FindObjects("LastLevelDoor", true).FirstOrDefault();
            if (lastLevelDoor != null)
            {
                if (Math.Abs(lastLevelDoor.transform.localRotation.z - 1) > 0.01)
                {
                    Plugin.LOG.LogWarning(
                        $"-- Init LastLevel Door: {lastLevelDoor.name}, LocalPosition: {lastLevelDoor.transform.localPosition}");
                    if (lastLevelDoor.transform.localPosition.x < 0)
                    {
                        var leftDoorPosition = GetAdjustedLeftDoorPosition(lastLevelDoor.transform.localPosition.x);
                        lastLevelDoor.transform.localPosition = new Vector3(leftDoorPosition, 0, 0);
                    }

                    if (lastLevelDoor.transform.localPosition.x > 0)
                    {
                        var rightDoorPosition = GetAdjustedRightDoorPosition(lastLevelDoor.transform.localPosition.x);
                        lastLevelDoor.transform.localPosition = new Vector3(rightDoorPosition, 0, 0);
                    }
                }
            }   
        }
    }



    [HarmonyPostfix]
    [HarmonyPatch(typeof(DungeonDoor), nameof(DungeonDoor.Start))]
    public static void DungeonDoor_Start(ref DungeonDoor __instance)
    {
        if (!IsUltrawide()) return;
        if (__instance.name.Equals("LevelDoorLeft"))
        {
            var leftDoorPosition = GetAdjustedLeftDoorPosition(__instance.transform.localPosition.x);
            __instance.transform.localPosition = new Vector3(leftDoorPosition, 0f, 0f);

            var newX = __instance.playerEnterDestinyRoomPoint.localPosition.x * ScaleFactor;
            __instance.playerEnterDestinyRoomPoint.localPosition = new Vector3(newX, 0f, 0f);
        }

        if (__instance.name.Equals("LevelDoorRight"))
        {
            var rightDoorPosition = GetAdjustedRightDoorPosition(__instance.transform.localPosition.x);
            __instance.transform.localPosition = new Vector3(rightDoorPosition, 0f, 0f);

            var newX = __instance.playerEnterDestinyRoomPoint.localPosition.x * ScaleFactor;
            __instance.playerEnterDestinyRoomPoint.localPosition = new Vector3(newX, 0f, 0f);
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
        UpdateAspectValues();
        var newFloorWidth = GetAdjustedFloorWidth();
        __instance.roomFloorWidth = newFloorWidth;
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
            if (Math.Abs(obj.transform.localRotation.z - 1) > 0.01)
            {
                if (obj.transform.localPosition.x < 0)
                {
                    var localPosition = obj.transform.localPosition;
                    var newPosition = GetAdjustedLeftPosition(localPosition.x);
                    localPosition = new Vector3(newPosition, localPosition.y, localPosition.z);
                    obj.transform.localPosition = localPosition;
                }

                if (obj.transform.localPosition.x > 0)
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
using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace Moonlighter;

[HarmonyPatch]
public static class Patches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MainMenuPanel), nameof(MainMenuPanel.PlayPressKeyReminderAnimationDelayed))]
    public static void MainMenuPanel_PlayPressKeyReminderAnimationDelayed(ref MainMenuPanel __instance)
    {
        __instance.PlayOpenAnimation();
        __instance.StopPressKeyReminderAnimation();
        __instance.SetInteractable(true, __instance.mainMenuButtons);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MainMenuPanel), nameof(MainMenuPanel.CheckContinueAndSelectFirstButton))]
    public static void MainMenuPanel_CheckContinueAndSelectFirstButton(ref MainMenuPanel __instance)
    {
        if (__instance.HasLoadedGame)
        {
            __instance.StartGame();
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
        __instance.roomFloorWidth = 675;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(DungeonRoom), nameof(DungeonRoom.ConnectEastRoom))]
    public static void DungeonRoom_ConnectEastRoom(ref DungeonRoom __instance, ref DungeonRoom room)
    {
        AdjustDoorBounds(ref __instance, nameof(DungeonRoom.ConnectEastRoom));

        var eastDoor = __instance.GetEastDoor();
        var westDoor = room.GetWestDoor();
        if (eastDoor != null && westDoor != null)
        {
            eastDoor.transform.localPosition = new Vector3(340f, 0, 0);
            westDoor.transform.localPosition = new Vector3(-340f, 0, 0);
            eastDoor.playerEnterDestinyRoomPoint.position = new Vector3(
                westDoor.transform.position.x + __instance.doorSeparation, westDoor.transform.position.y,
                westDoor.transform.position.z);
            westDoor.playerEnterDestinyRoomPoint.position = new Vector3(
                eastDoor.transform.position.x - __instance.doorSeparation, eastDoor.transform.position.y,
                eastDoor.transform.position.z);
        }
    }

    private static void AdjustDoorBounds(ref DungeonRoom __instance, string roomName)
    {
        Plugin.LOG.LogWarning($"|--- Start {roomName} ---|");
        foreach (var bound in __instance.roomDoorsBoundaries)
        {
            Plugin.LOG.LogWarning(
                $"Room: {__instance.name}, DoorBound: {bound.name}, LocalPosition: {bound.transform.localPosition}");
            if (bound.name.Equals("DoorBoundaryLeft"))
            {
                bound.transform.localPosition = new Vector3(-340f, 0f, 0f);
            }

            if (bound.name.Equals("DoorBoundaryRight"))
            {
                bound.transform.localPosition = new Vector3(340f, 0f, 0f);
            }
        }

        Plugin.LOG.LogWarning($"|--- End {roomName} ---|");

        __instance.roomFloorWidth = 675;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(DungeonRoom), nameof(DungeonRoom.ConnectWestRoom))]
    public static void DungeonRoom_ConnectWestRoom(ref DungeonRoom __instance, ref DungeonRoom room)
    {
        AdjustDoorBounds(ref __instance, nameof(DungeonRoom.ConnectWestRoom));

        var westDoor = __instance.GetWestDoor();
        var eastDoor = room.GetEastDoor();
        if (westDoor != null && eastDoor != null)
        {
            westDoor.transform.localPosition = new Vector3(-340f, 0, 0);
            eastDoor.transform.localPosition = new Vector3(340f, 0, 0);
            westDoor.playerEnterDestinyRoomPoint.position = new Vector3(
                eastDoor.transform.position.x - __instance.doorSeparation, eastDoor.transform.position.y,
                eastDoor.transform.position.z);
            eastDoor.playerEnterDestinyRoomPoint.position = new Vector3(
                westDoor.transform.position.x + __instance.doorSeparation, westDoor.transform.position.y,
                westDoor.transform.position.z);
        }
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(DungeonDoor), nameof(DungeonDoor.Start))]
    public static void DungeonRoom_Start(ref DungeonDoor __instance)
    {
        if (Plugin.DebugMarkers.Value)
        {
            var wd = __instance.gameObject.AddComponent<LocationMarker>();
            wd.SetTransform(__instance.transform, Color.white);

            var wd2 = __instance.gameObject.AddComponent<LocationMarker>();
            wd2.SetTransform(__instance.playerEnterDestinyRoomPoint.transform, Color.green);

            Plugin.Markers.Add(wd);
            Plugin.Markers.Add(wd2);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(DungeonRoom), nameof(DungeonRoom.InitializeRoom), typeof(DungeonRoomInfo))]
    [HarmonyPatch(typeof(DungeonRoom), nameof(DungeonRoom.InitializeRoom), new Type[] { })]
    public static void DungeonRoom_InitializeRoom(ref DungeonRoom __instance)
    {
        foreach (var obj in Helpers.FindObjectsInPath("DungeonLevelArt"))
        {
            if (obj.name.Equals("DungeonLevelArt"))
            {
                obj.transform.localScale = new Vector3(1.35f, 1f, 1);
            }
        }

        foreach (var obj in Helpers.FindObjectsInPath("DungeonLevelBoundaries"))
        {
            if (obj.name.EndsWith("Left"))
            {
                obj.transform.localPosition = new Vector3(-340f, 0, 0);
            }

            if (obj.name.EndsWith("Right"))
            {
                obj.transform.localPosition = new Vector3(340f, 0, 0);
            }
        }

        foreach (var obj in Helpers.FindObjectsInPath("DungeonLevelDoors").Where(a => a.name.Contains("Next") || a.name.Contains("next")))
        {
            if (obj.transform.localPosition.x < 0)
            {
                obj.transform.localPosition = new Vector3(-340f, 0, 0);
            }

            if (obj.transform.localPosition.x > 0)
            {
                obj.transform.localPosition = new Vector3(340f, 0, 0);
            }
        }

        foreach (var obj in Helpers.FindObjectsInPath("WallDetails/"))
        {
            if (Math.Abs(obj.transform.localRotation.z - 1) > 0.01)
            {
                if (obj.transform.localPosition.x < 0)
                {
                    if (Plugin.DebugMarkers.Value)
                    {
                        var wd = obj.AddComponent<LocationMarker>();
                        wd.SetTransform(obj.transform, Color.magenta);
                        Plugin.Markers.Add(wd);
                    }

                    var localPosition = obj.transform.localPosition;
                    localPosition = new Vector3(-340f, localPosition.y, localPosition.z);
                    obj.transform.localPosition = localPosition;
                }

                if (obj.transform.localPosition.x > 0)
                {
                    if (Plugin.DebugMarkers.Value)
                    {
                        var wd = obj.AddComponent<LocationMarker>();
                        wd.SetTransform(obj.transform, Color.magenta);
                        Plugin.Markers.Add(wd);
                    }

                    var localPosition = obj.transform.localPosition;
                    localPosition = new Vector3(340f, localPosition.y, localPosition.z);
                    obj.transform.localPosition = localPosition;
                }
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(VideoSequence), nameof(VideoSequence.Play))]
    public static bool VideoSequence_Play(ref VideoSequence __instance)
    {
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MainMenuPanel), nameof(MainMenuPanel.ShowGamepadGUI))]
    public static bool MainMenuPanel_ShowGamepadGUI(ref MainMenuPanel __instance)
    {
        __instance.CloseGamepadPanel();
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(OptionsGraphicTabContent), nameof(OptionsGraphicTabContent.CheckResolutions))]
    public static bool OptionsGraphicTabContent_CheckResolutions(ref OptionsGraphicTabContent __instance)
    {
        var maxRefresh = Screen.resolutions.Max(a => a.refreshRate).refreshRate;
        var nativeResolution = new Resolution
        {
            width = Display.main.systemWidth,
            height = Display.main.systemHeight,
            refreshRate = maxRefresh
        };

        var newResList = Enumerable.ToList(Screen.resolutions);
        newResList.Add(nativeResolution);

        var newArList = Enumerable.ToList(newResList.Select(res => (float) res.width / res.height));

        __instance._availableResolutions = newResList;
        __instance._availableAR = newArList;

        return false;
    }
}
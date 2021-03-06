﻿using SDG.Unturned;
using System.Reflection;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace HInj
{
    public class Miscellaneous : MonoBehaviour
    {
        //Startup
        FieldInfo lastKeyDown = null, isHoldingKey = null;
        public void Awake()
        {
            lastKeyDown = typeof(PlayerInteract).GetField("lastKeyDown", BindingFlags.NonPublic | BindingFlags.Static);
            isHoldingKey = typeof(PlayerInteract).GetField("isHoldingKey", BindingFlags.NonPublic | BindingFlags.Static);
        }

        //Todo: Find better method for this, isn't 100%, kinda big
        public void UpdateFirearm(ItemGunAsset ass)
        {
            if (ass.recoilMax_y == 0f && ass.shakeMax_x == 0f)
                return;

            //No Recoil
            ass.recoilMax_x = 0f;
            ass.recoilMax_y = 0f;
            ass.recoilMin_x = 0f;
            ass.recoilMin_y = 0f;

            //No Shake
            ass.shakeMax_x = 0f;
            ass.shakeMax_y = 0f;
            ass.shakeMax_z = 0f;
            ass.shakeMin_x = 0f;
            ass.shakeMin_y = 0f;
            ass.shakeMin_z = 0f;

            //No Spread
            ass.spreadAim = 0f;
            ass.spreadHip = 0f;
            PlayerUI.updateCrosshair(0f);

            //Shake recovery
            ass.recover_x = 0f;
            ass.recover_y = 0f;
        }

        public static void ToggleFreecam()
        {
            if (!Provider.isConnected)
                return;

            if (Provider.clients.Count < 1)
                return;

            if (Player.player == null)
                return;

            Player.player.look.isOrbiting = !Player.player.look.isOrbiting;
            if (!Player.player.look.isTracking)
            {
                if (Player.player.look.isOrbiting && !Player.player.look.isTracking && !Player.player.look.isLocking && !Player.player.look.isFocusing)
                {
                    Player.player.look.isTracking = true;
                    Player.player.look.orbitSpeed = 20f; //Prevent freecam from turning into a snail
                }
            }
            else
                Player.player.look.isTracking = false;
        }

        static int salvCheck = 0;
        public void CheckSalvage()
        {
            if (!(bool)isHoldingKey.GetValue(null))
            {
                lastKeyDown.SetValue(null, 0f);
                return;
            }

            if ((Time.realtimeSinceStartup - (float)lastKeyDown.GetValue(null)) > 0.65f)
            {
                isHoldingKey.SetValue(null, false);
                lastKeyDown.SetValue(null, 0f);
                if (!PlayerUI.window.showCursor && (PlayerInteract.interactable2 != null))
                    PlayerInteract.interactable2.use();
            }
        }

        //Called every tick
        public void Update()
        {
            if (!Global.MiscEnabled)
                return;

            if (!Provider.isConnected)
                return;

            if (Provider.clients.Count < 1)
                return;

            if (Player.player == null)
                return;

            if (Global.AllOff)
                return;

            //Update spy
            Hooks.askScreenshot.SpyUpd();

            //Force Third Person
            if (Global.MiscSettings.ForceThirdperson)
                Provider.cameraMode = ECameraMode.BOTH;

            //Force GPS
            if (Global.MiscSettings.ForceSatellite)
                Provider.modeConfigData.Gameplay.Satellite = true;

            //Force Compass
            if (Global.MiscSettings.ForceCompass)
                Provider.modeConfigData.Gameplay.Compass = true;

            //Gun update
            if (Global.MiscSettings.NoRecoilSway)
                if ((Player.player?.equipment?.asset != null) && Player.player.equipment.asset is ItemGunAsset)
                    UpdateFirearm((ItemGunAsset)Player.player.equipment.asset);

            //Bypass Timers
            if (Global.MiscSettings.BypassTimers)
                Provider.modeConfigData.Gameplay.Timer_Exit = 0;

            //Fast Salvage
            if (Global.MiscSettings.FastSalvage)
                if (salvCheck++ > 60)
                    CheckSalvage();

            //Hide fog
            if (Global.MiscSettings.NoFog)
                MainCamera.instance.GetComponent<GlobalFog2>().enabled = false;

            //Skin changer
            Skins.SkinUpd();

            if (Global.MiscSettings.Jesus)
            {
                Player.player.movement.itemGravityMultiplier = 0f;

                if (Input.GetKey(KeyCode.W))
                    Player.player.transform.position += Camera.main.transform.forward * 0.6f;
                if (Input.GetKey(KeyCode.S))
                    Player.player.transform.position -= Camera.main.transform.forward * 0.6f;
                if (Input.GetKey(KeyCode.KeypadMinus))
                    Player.player.transform.position += Camera.main.transform.up * 0.6f;
                if (Input.GetKey(KeyCode.KeypadPlus))
                    Player.player.transform.position -= Camera.main.transform.up * 0.6f;
            }
            else if (Player.player.movement.itemGravityMultiplier == 0f)
            {
                ItemCloudAsset itemCloudAsset = Player.player.equipment.asset as ItemCloudAsset;
                Player.player.movement.itemGravityMultiplier = ((itemCloudAsset != null) ? itemCloudAsset.gravity : 1f);
            }
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;

namespace Mover
{
    public class Factory
    {
        private const string DirectoryName = "commands";
        private const string CommandBack = "Back";
        private const string CommandFront = "Front";
        private const string CommandRestore = "Restore";
        private const string CommandFirstPerson = "FirstPerson";
        private const string CommandFirstPersonSmallCams = "FirstPersonSmallCams";
        private const string CommandToggle360 = "Toggle360";
        private readonly List<CameraPlusConfig> _cameras = new List<CameraPlusConfig>();
        private readonly ILogger _logger;
        
        private readonly Size _screenBig = new Size(650, 340);
        private readonly Size _screenSmall = new Size(510, 300);
        
        private FileSystemWatcher _watcher;
        private string _beatSaberDirectory;
        private bool _toggle360State = false;

        public bool IsLoaded { get; private set; }

        public Factory(ILogger logger)
        {
            _logger = logger;

            _logger.Log("Initializing...");

            // create command directory if not exists
            if (!Directory.Exists(DirectoryName))
            {
                Directory.CreateDirectory(DirectoryName);
            }

            // delete old - non triggered commands if any
            foreach (var file in Directory.GetFiles(DirectoryName))
            {
                File.Delete(file);
            }
            
            //SetBeatSaberPath(File.ReadAllText("beatsaberpath.txt"));
        }

        public void SetBeatSaberPath(string path)
        {
            _beatSaberDirectory = Path.GetFullPath(path);
            LoadData();
        }

        public void Destroy()
        {
            if (!_cameras.Any())
            {
                return;
            }
            
            _logger.Log("destroying...");
            foreach (var config in _cameras)
            {
                config.Destroy();
            }

            if (_watcher != null)
            {
                _watcher.Created -= FileCreatedEvent;   
            }
            
            _logger.Log("done");
        }

        private bool AreCamsIn360(CameraPlusConfig front = null, CameraPlusConfig back = null)
        {
            if (!IsLoaded)
            {
                return false;
            }
            
            if (front == null)
            {
                front = GetFromView(View.Front);
            }

            if (back == null)
            {
                back = GetFromView(View.Back);
            }

            return front.Use360Camera && back.Use360Camera;
        }
        
        private void LoadData()
        {
            try
            {
                _cameras.Clear();
                Destroy();
                
                // read directory file and check if everything is right
                _logger.Log("Checking Beat Saber Data...");
                _logger.Log(_beatSaberDirectory);
                if (!Directory.Exists(_beatSaberDirectory))
                {
                    throw new DirectoryNotFoundException(_beatSaberDirectory);
                }

                _logger.Log($"Beat Saber is installed to: {_beatSaberDirectory}");

                var beatSaberDirectory = _beatSaberDirectory + "/UserData/CameraPlus/";
                if (!Directory.Exists(beatSaberDirectory))
                {
                    throw new DirectoryNotFoundException(beatSaberDirectory);
                }

                _logger.Log($"found camera path: {beatSaberDirectory}");

                // load cameras
                _logger.Log("Loading Cameras...");
                _cameras.Add(CameraPlusConfig.FromFile(beatSaberDirectory + "cameraplus.cfg", View.Back, _logger));
                _cameras.Add(CameraPlusConfig.FromFile(beatSaberDirectory + "customcamera1.cfg", View.FirstPerson, _logger));
                _cameras.Add(CameraPlusConfig.FromFile(beatSaberDirectory + "customcamera2.cfg", View.Front, _logger));

                _logger.Log($"loaded {_cameras.Count} cameras");

                // create a watcher
                _watcher = new FileSystemWatcher(DirectoryName)
                {
                    IncludeSubdirectories = false,
                    EnableRaisingEvents = true,
                    //NotifyFilter = NotifyFilters.LastWrite
                };
                _watcher.Created += FileCreatedEvent;
                _watcher.Error += (sender, args) =>
                {
                    _logger.Log("could not watch directory!");
                };

                _logger.Log("done, go ahead :3");
                _logger.Log("-----------------");

                IsLoaded = true;
                File.WriteAllText("beatsaberpath.txt", _beatSaberDirectory);

                _toggle360State = AreCamsIn360();
            }
            catch (Exception e)
            {
                IsLoaded = false;
                _logger.LogException(e);
                _logger.Log("disabled mover...");
            }
        }

        private void FileCreatedEvent(object sender, FileSystemEventArgs args)
        {
            WaitUntilFileIsNotLocked(args.FullPath);
            
            try
            {
                var command = File.ReadAllText(args.FullPath);
                ParseCommands(command.Trim('\n','\r'));
                File.Delete(args.FullPath);
            }
            catch (IOException)
            {
                _logger.Log($"cant access {args.Name} yet.. waiting for 200 msecs");
                Thread.Sleep(2000);
                FileCreatedEvent(sender, args);
            }
        }

        public static void WaitUntilFileIsNotLocked(string filePath)
        {
            // can we access it? if not wait a few m sec
            while (IsFileLocked(filePath))
            {
                Thread.Sleep(200);
            }
        }

        public static bool IsFileLocked(string filePath)
        {
            var file = new FileInfo(filePath);
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                stream?.Close();
            }

            return false;
        }

        private void ParseCommands(string commandString)
        {
            if (!IsLoaded)
            {
                _logger.Log($"NOT LOADED! ignoring command {commandString}");
                return;
            }
            
            _logger.Log($"received command: {commandString}");

            var parsing = commandString.Split(' ');
            if (parsing.Length < 1)
            {
                _logger.Log("no command given :(");
                return;
            }

            RestoreAllCams();
            
            var command = parsing[0].Trim();
            _logger.Log($"invoking \"{command}\"");

            switch (command)
            {
                case CommandFirstPerson:
                    CmdFirstPerson(false);
                    break;

                case CommandFirstPersonSmallCams:
                    CmdFirstPerson(true);
                    break;

                case CommandFront:
                    CmdFront();
                    break;

                case CommandToggle360:
                    CmdToggle360();
                    break;

                default:
                    _logger.Log($"unrecognised command: {command}");
                    return;
            }

            SaveAllCams();
        }
        
        public void RestoreAllCams()
        {
            foreach (var camera in _cameras)
            {
                camera.RestoreFromBackup();
            }
            
            SetCamDimensions(GetFromView(View.FirstPerson), GetFromView(View.Front), _screenBig);
        }

        public void CmdFirstPerson(bool smallCams)
        {
            Toggle(GetFromView(View.Back), GetFromView(View.FirstPerson));
            if (smallCams)
            {
                SetCamDimensions(GetFromView(View.FirstPerson), GetFromView(View.Front), _screenSmall);
            }
        }

        public void CmdFront()
        {
            Toggle(GetFromView(View.Back), GetFromView(View.Front));
        }

        public void CmdToggle360()
        {
            _toggle360State = !_toggle360State;
            var back = GetFromView(View.Back);
            var front = GetFromView(View.Front);
            var state = _toggle360State;

            front.Use360Camera = state;
            back.Use360Camera = state;

            front.Changed = true;
            back.Changed = true;
        }

        public void SaveAllCams()
        {
            _logger.Log("saving cams");
            foreach (var camera in _cameras)
            {
                camera.Save();
            }
        }

        private void SetCamDimensions(CameraPlusConfig config1, CameraPlusConfig config2, Size toSize)
        {

            config1.ScreenPosX = 0;
            config1.ScreenPosY = 1080 - toSize.Height;
            config1.ScreenWidth = toSize.Width;
            config1.ScreenHeight = toSize.Height;

            config2.ScreenPosX = 1920 - toSize.Width;
            config2.ScreenPosY = config1.ScreenPosY;
            config2.ScreenWidth = toSize.Width;
            config2.ScreenHeight = toSize.Height;

            config1.Changed = true;
            config2.Changed = true;
        }

        private void Toggle(CameraPlusConfig fromCam, CameraPlusConfig toCam)
        {
            /*
            if (fromCam.Use360Camera != toCam.Use360Camera)
            {
                return;
            }
            */


            var tmpTransparentWalls = fromCam.TransparentWalls;
            fromCam.TransparentWalls = toCam.TransparentWalls;
            toCam.TransparentWalls = tmpTransparentWalls;

            /*
            if (fromCam.Use360Camera)
            {
            */
            // swap 360 settings
            var tmpCam360Smoothness = fromCam.Cam360Smoothness;
            var tmpUse360Camera = fromCam.Use360Camera;
            var tmpCam360ForwardOffset = fromCam.Cam360ForwardOffset;
            var tmpCam360XTilt = fromCam.Cam360XTilt;
            var tmpCam360ZTilt = fromCam.Cam360ZTilt;
            var tmpCam360YTilt = fromCam.Cam360YTilt;
            var tmpCam360UpOffset = fromCam.Cam360UpOffset;
            var tmpCam360RightOffset = fromCam.Cam360RightOffset;

            fromCam.Cam360Smoothness = toCam.Cam360Smoothness;
            fromCam.Use360Camera = toCam.Use360Camera;
            fromCam.Cam360ForwardOffset = toCam.Cam360ForwardOffset;
            fromCam.Cam360XTilt = toCam.Cam360XTilt;
            fromCam.Cam360ZTilt = toCam.Cam360ZTilt;
            fromCam.Cam360YTilt = toCam.Cam360YTilt;
            fromCam.Cam360UpOffset = toCam.Cam360UpOffset;
            fromCam.Cam360RightOffset = toCam.Cam360RightOffset;

            toCam.Cam360Smoothness = tmpCam360Smoothness;
            toCam.Use360Camera = tmpUse360Camera;
            toCam.Cam360ForwardOffset = tmpCam360ForwardOffset;
            toCam.Cam360XTilt = tmpCam360XTilt;
            toCam.Cam360ZTilt = tmpCam360ZTilt;
            toCam.Cam360YTilt = tmpCam360YTilt;
            toCam.Cam360UpOffset = tmpCam360UpOffset;
            toCam.Cam360RightOffset = tmpCam360RightOffset;
            /*
        }
        else
        {
        */
            // swap global position
            var tmpPositionSmooth = fromCam.PositionSmooth;
            var tmpRotationSmooth = fromCam.RotationSmooth;
            var tmpThirdPerson = fromCam.ThirdPerson;
            var tmpShowThirdPersonCamera = fromCam.ShowThirdPersonCamera;
            var tmpPosX = fromCam.PosX;
            var tmpPosY = fromCam.PosY;
            var tmpPosZ = fromCam.PosZ;
            var tmpAngX = fromCam.AngX;
            var tmpAngY = fromCam.AngY;
            var tmpAngZ = fromCam.AngZ;

            fromCam.PositionSmooth = toCam.PositionSmooth;
            fromCam.RotationSmooth = toCam.RotationSmooth;
            fromCam.ThirdPerson = toCam.ThirdPerson;
            fromCam.ShowThirdPersonCamera = toCam.ShowThirdPersonCamera;
            fromCam.PosX = toCam.PosX;
            fromCam.PosY = toCam.PosY;
            fromCam.PosZ = toCam.PosZ;
            fromCam.AngX = toCam.AngX;
            fromCam.AngY = toCam.AngY;
            fromCam.AngZ = toCam.AngZ;

            toCam.PositionSmooth = tmpPositionSmooth;
            toCam.RotationSmooth = tmpRotationSmooth;
            toCam.ThirdPerson = tmpThirdPerson;
            toCam.ShowThirdPersonCamera = tmpShowThirdPersonCamera;
            toCam.PosX = tmpPosX;
            toCam.PosY = tmpPosY;
            toCam.PosZ = tmpPosZ;
            toCam.AngX = tmpAngX;
            toCam.AngY = tmpAngY;
            toCam.AngZ = tmpAngZ;
            //}

            fromCam.Changed = true;
            toCam.Changed = true;
        }

        private CameraPlusConfig GetFromView(View view) => _cameras.First(cam => cam.View == view);

        public bool Cam360State() => _toggle360State;
    }
}
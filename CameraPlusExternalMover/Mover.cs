using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace CameraPlusExternalMover
{
    public class Mover
    {
        public const string DirectoryName = "commands";
        private readonly FileSystemWatcher _watcher;
        private readonly List<CameraPlusConfig> _cameras = new List<CameraPlusConfig>();
        private readonly ILogger _logger;

        public Mover(ILogger logger)
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

            // read directory file and check if everything is right
            _logger.Log("Checking Beat Saber Data...");
            var beatSaberDirectory = File.ReadAllText("beatsaberpath.txt");
            _logger.Log(beatSaberDirectory);
            if (!Directory.Exists(beatSaberDirectory))
            {
                throw new DirectoryNotFoundException(beatSaberDirectory);
            }
            
            _logger.Log($"Beat Saber is installed to: {beatSaberDirectory}");

            beatSaberDirectory += "/UserData/CameraPlus/";
            if (!Directory.Exists(beatSaberDirectory))
            {
                throw new DirectoryNotFoundException(beatSaberDirectory);
            }
            
            _logger.Log($"camera path: {beatSaberDirectory}");

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
            _watcher.Error += (sender, args) => { _logger.Log("could not watch directory!"); };

            _logger.Log("done, go ahead :3");
            _logger.Log("-----------------");
        }

        public void Destroy()
        {
            _logger.Log("destroying...");
            foreach (var config in _cameras)
            {
                config.Destroy();
            }
            _watcher.Created -= FileCreatedEvent;
        }

        private void FileCreatedEvent(object sender, FileSystemEventArgs args)
        {
            // can we access it? if not wait a few msec
            while (IsFileLocked(args.FullPath))
            {
                Thread.Sleep(200);
            }

            try
            {
                var command = File.ReadAllText(args.FullPath);
                ParseCommands(command);
                File.Delete(args.FullPath);
            }
            catch (IOException)
            {
                _logger.Log($"cant access {args.Name} yet.. waiting for 200 msecs");
                Thread.Sleep(2000);
                FileCreatedEvent(sender, args);
            }
        }

        private bool IsFileLocked(string filePath)
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
            _logger.Log($"recieved command: {commandString}");

            var parsing = commandString.Split(' ');
            if (parsing.Length < 1)
            {
                _logger.Log("no command given :(");
                return;
            }

            var command = parsing[0].Trim();
            RestoreAllCams();
            _logger.Log($"invoking \"{command}\"");
            switch (command)
            {
                case "FirstPerson":
                    ToggleFirstPerson();
                    break;

                case "FirstPersonSmallCams":
                    ToggleFirstPerson();
                    SetCamDimensions(GetFromView(View.FirstPerson), GetFromView(View.Front));
                    break;

                case "Front":
                    ToggleFront();
                    break;

                case "Back":
                case "Restore":
                    //RestoreAllCams();
                    break;

                default:
                    _logger.Log($"unrecognised command: {command}");
                    return;
            }

            SaveAllCams();
        }

        private void SetCamDimensions(CameraPlusConfig config1, CameraPlusConfig config2)
        {
            const int width = 510;
            const int height = 300;

            config1.ScreenPosX = 0;
            config1.ScreenPosY = 1080 - height;
            config1.ScreenWidth = width;
            config1.ScreenHeight = height;

            config2.ScreenPosX = 1920 - width;
            config2.ScreenPosY = config1.ScreenPosY;
            config2.ScreenWidth = width;
            config2.ScreenHeight = height;
        }

        private void ToggleFirstPerson()
        {
            var back = GetFromView(View.Back);
            var fp = GetFromView(View.FirstPerson);

            Toggle(back, fp);
        }

        private void ToggleFront()
        {
            var back = GetFromView(View.Back);
            var front = GetFromView(View.Front);

            Toggle(back, front);
        }

        private void RestoreAllCams()
        {
            foreach (var camera in _cameras)
            {
                camera.RestoreFromBackup();
            }
        }

        private void SaveAllCams()
        {
            foreach (var camera in _cameras)
            {
                camera.Save();
            }
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
                var tmpPosX = fromCam.Posx;
                var tmpPosY = fromCam.Posy;
                var tmpPosZ = fromCam.Posz;
                var tmpAngX = fromCam.Angx;
                var tmpAngY = fromCam.Angy;
                var tmpAngZ = fromCam.Angz;

                fromCam.PositionSmooth = toCam.PositionSmooth;
                fromCam.RotationSmooth = toCam.RotationSmooth;
                fromCam.ThirdPerson = toCam.ThirdPerson;
                fromCam.ShowThirdPersonCamera = toCam.ShowThirdPersonCamera;
                fromCam.Posx = toCam.Posx;
                fromCam.Posy = toCam.Posy;
                fromCam.Posz = toCam.Posz;
                fromCam.Angx = toCam.Angx;
                fromCam.Angy = toCam.Angy;
                fromCam.Angz = toCam.Angz;

                toCam.PositionSmooth = tmpPositionSmooth;
                toCam.RotationSmooth = tmpRotationSmooth;
                toCam.ThirdPerson = tmpThirdPerson;
                toCam.ShowThirdPersonCamera = tmpShowThirdPersonCamera;
                toCam.Posx = tmpPosX;
                toCam.Posy = tmpPosY;
                toCam.Posz = tmpPosZ;
                toCam.Angx = tmpAngX;
                toCam.Angy = tmpAngY;
                toCam.Angz = tmpAngZ;
            //}
        }

        private CameraPlusConfig GetFromView(View view) => _cameras.First(cam => cam.View == view);
    }
}
﻿using UnityEditor;
using UnityEngine;
using PatchKit.Api;
using PatchKit.Api.Models.Main;
using PatchKit.Network;
using System.Linq;
using System.IO;

namespace PatchKit.Tools.Integration
{
    public class BuildAndPublish : EditorWindow
    {
        private ApiKey _apiKey;
        private ApiUtils _api = null;
        private App? _selectedApp = null;
        private AppCache _appCache;

        private bool _reimportLock = false;

        private Views.IView _currentView;

        [MenuItem("File/Build and Publish")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(BuildAndPublish), false, "Build & Publish");
        }

        private void Awake()
        {
            LockReload();

            _appCache = new AppCache(Config.instance().localCachePath);

            _apiKey = ApiKey.LoadCached();

            if (_apiKey == null)
            {
                var submitKey = new Views.SubmitKey();

                submitKey.OnKeyResolve += OnKeyResolved;

                _currentView = submitKey;
            }
            else
            {
                _api = new ApiUtils(_apiKey);
                _selectedApp = _appCache.AppByPlatform(EditorUserBuildSettings.activeBuildTarget);

                if (!_selectedApp.HasValue)
                {
                    var selectApp = new Views.SelectApp(_api);

                    selectApp.OnAppSelected += OnAppSelected;

                    _currentView = selectApp;
                }
                else
                {
                    var build = new Views.BuildApp();

                    build.OnSuccess += OnBuildSuccess;
                    build.OnFailure += OnBuildFailed;

                    _currentView = build;
                }
            }
        }

        private void OnKeyResolved(ApiKey key)
        {
            _apiKey = key;

            ApiKey.Cache(_apiKey);

            _api = new ApiUtils(_apiKey);

            _selectedApp = _appCache.AppByPlatform(EditorUserBuildSettings.activeBuildTarget);

            if (_selectedApp.HasValue)
            {
                var build = new Views.BuildApp();

                build.OnSuccess += OnBuildSuccess;
                build.OnFailure += OnBuildFailed;

                _currentView = build;
            }
            else
            {
                var selectApp = new Views.SelectApp(_api);

                selectApp.OnAppSelected += OnAppSelected;

                _currentView = selectApp;
            }
        }

        private void OnAppSelected(Api.Models.Main.App app)
        {
            _selectedApp = app;
            _appCache.UpdateEntry(EditorUserBuildSettings.activeBuildTarget, app);

            var build = new Views.BuildApp();

            build.OnSuccess += OnBuildSuccess;
            build.OnFailure += OnBuildFailed;

            _currentView = build;
        }

        private string ResolveBuildDir()
        {
            return Path.GetDirectoryName(EditorUserBuildSettings.GetBuildLocation(EditorUserBuildSettings.activeBuildTarget));
        }

        private void OnBuildSuccess()
        {
            var publishApp = new Views.Publish(_apiKey, _selectedApp.Value.Secret, ResolveBuildDir());

            _currentView = publishApp;
        }

        private void OnBuildFailed(string errorMessage)
        {
            // _currentView = null;
        }

        private void OnGUI()
        {
            if (_currentView != null)
            {
                _currentView.Show();
            }
            else
            {
                Close();
            }

            PreventReloadIfLocked();

            Repaint();
        }

        private void OnDestroy()
        {
            if (_reimportLock)
            {
                UnlockReload();
            }
        }

        private void LockReload()
        {
            EditorApplication.LockReloadAssemblies();
            _reimportLock = true;
        }

        private void UnlockReload()
        {
            EditorApplication.UnlockReloadAssemblies();
            _reimportLock = false;
        }

        private void PreventReloadIfLocked()
        {
            if (EditorApplication.isCompiling && _reimportLock)
            {
                EditorApplication.LockReloadAssemblies();
            }
        }
    }
}
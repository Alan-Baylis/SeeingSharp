﻿#region License information (SeeingSharp and all based games/applications)
/*
    SeeingSharp and all games/applications based on it (more info at http://www.rolandk.de/wp)
    Copyright (C) 2015 Roland König (RolandK)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/
#endregion

using SeeingSharp.Multimedia.Drawing2D;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SeeingSharp.Multimedia.Core
{
    public class EngineMainLoop
    {
        // => Optimize for 40 Frames / Sec
        private const double MINIMUM_FRAME_TIME_MS = 1000.0 / 60.0;
        private const double MINIMUM_DELAY_TIME_MS = 10.0;

        private static EngineMainLoop s_current;

        #region main thread synchronization
        private Task m_runningTask;
        private ConcurrentQueue<Action> m_globalLoopAwaiters;
        #endregion

        // RenderLoop collections
        #region
        private List<RenderLoop> m_registeredRenderLoops;
        private List<RenderLoop> m_unregisteredRenderLoops;
        private object m_registeredRenderLoopsLock;
        #endregion

        // Scene collections
        #region
        private List<Scene> m_scenesForUnload;
        private object m_scenesForUnloadLock;
        #endregion

        // Members regarding 2D resources
        #region
        private ConcurrentQueue<Drawing2DResourceBase> m_drawing2DResourcesToUnload;
        #endregion

        /// <summary>
        /// Prevents a default instance of the <see cref="EngineMainLoop"/> class from being created.
        /// </summary>
        private EngineMainLoop()
        {
            m_globalLoopAwaiters = new ConcurrentQueue<Action>();
            m_registeredRenderLoops = new List<RenderLoop>();
            m_unregisteredRenderLoops = new List<RenderLoop>();
            m_registeredRenderLoopsLock = new object();

            m_drawing2DResourcesToUnload = new ConcurrentQueue<Drawing2DResourceBase>();

            m_scenesForUnload = new List<Scene>();
            m_scenesForUnloadLock = new object();
        }

        /// <summary>
        /// Waits for next passed loop cycle.
        /// </summary>
        public Task WaitForNextPassedLoop()
        {
            TaskCompletionSource<object> result = new TaskCompletionSource<object>();
            m_globalLoopAwaiters.Enqueue(() =>
            {
                result.SetResult(null);
            });
            return result.Task;
        }

        /// <summary>
        /// Registers the given resource for unloading.
        /// </summary>
        /// <param name="drawing2DResourceBase">The resource to be unloaded.</param>
        internal void RegisterForUnload(Drawing2DResourceBase drawing2DResourceBase)
        {
            m_drawing2DResourcesToUnload.Enqueue(drawing2DResourceBase);
        }

        /// <summary>
        /// Registers the given RenderLoop object.
        /// </summary>
        /// <param name="renderLoop">The RenderLoop to be registered.</param>
        internal void RegisterRenderLoop(RenderLoop renderLoop)
        {
            lock (m_registeredRenderLoopsLock)
            {
                if (!m_registeredRenderLoops.Contains(renderLoop))
                {
                    m_registeredRenderLoops.Add(renderLoop);
                    renderLoop.IsRegisteredOnMainLoop = true;
                }
            }
        }

        /// <summary>
        /// Deregisters the given RenderLoop object.
        /// </summary>
        /// <param name="renderLoop">The render loop to be deregistered.</param>
        internal void DeregisterRenderLoop(RenderLoop renderLoop)
        {
            lock (m_registeredRenderLoopsLock)
            {
                m_registeredRenderLoops.Remove(renderLoop);
                renderLoop.IsRegisteredOnMainLoop = false;

                if (!m_unregisteredRenderLoops.Contains(renderLoop))
                {
                    m_unregisteredRenderLoops.Add(renderLoop);
                }
            }
        }

        /// <summary>
        /// Starts the engine's main loop.
        /// </summary>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        internal Task Start(CancellationToken cancelToken)
        {
            if (m_runningTask != null) { throw new SeeingSharpGraphicsException("Unable to start engine's main loop: Loop has already started!"); }

            m_runningTask = Task.Factory.StartNew(async () =>
            {
                Stopwatch renderStopWatch = new Stopwatch();
                renderStopWatch.Start();

                List<RenderLoop> renderingRenderLoops = new List<RenderLoop>();
                List<Scene> scenesToRender = new List<Scene>();
                List<Camera3DBase> camerasToUpdate = new List<Camera3DBase>();
                List<EngineDevice> devicesInUse = new List<EngineDevice>();
                while (!cancelToken.IsCancellationRequested)
                {
                    bool exceptionOccurred = false;
                    try
                    {
                        using (var perfToken = GraphicsCore.Current.BeginMeasureActivityDuration(Constants.PERF_GLOBAL_PER_FRAME))
                        {
                            // Wait some time before doing anything..
                            double lastRenderMilliseconds = renderStopWatch.GetTrueElapsedMilliseconds();
                            double delayTime = MINIMUM_FRAME_TIME_MS - lastRenderMilliseconds;
                            if (delayTime < MINIMUM_DELAY_TIME_MS) { delayTime = MINIMUM_DELAY_TIME_MS; }

                            //GraphicsCore.Current.NotifyActivityDuration(Constants.PERF_GLOBAL_WAIT_TIME + "_PARAM", TimeSpan.FromMilliseconds(MINIMUM_FRAME_TIME_MS - lastRenderMilliseconds).Ticks);
                            using (var perfTokenInner = GraphicsCore.Current.BeginMeasureActivityDuration(Constants.PERF_GLOBAL_WAIT_TIME))
                            {
                                CommonTools.MaximumDelay(delayTime);
                            }

                            // Get all render loops
                            renderingRenderLoops.Clear();
                            lock (m_registeredRenderLoopsLock)
                            {
                                renderingRenderLoops.AddRange(m_registeredRenderLoops);
                            }

                            // Queries for devices / scenes in use
                            QueryForScenesAndCameras(renderingRenderLoops, scenesToRender, camerasToUpdate);
                            QueryForDevicesInUse(renderingRenderLoops, devicesInUse);

                            // Build new UpdateState object
                            TimeSpan updateTime = renderStopWatch.Elapsed;
                            if (updateTime.TotalMilliseconds > 100.0) { updateTime = TimeSpan.FromMilliseconds(100.0); }
                            UpdateState updateState = new UpdateState(updateTime);

                            // Restart the stopwatch
                            renderStopWatch.Restart();

                            // First global pass: Update scene and prepare rendering
                            await UpdateAndPrepareRendering(renderingRenderLoops, scenesToRender, devicesInUse, updateState)
                                .ConfigureAwait(false);
                            foreach (Camera3DBase actCamera in camerasToUpdate)
                            {
                                actCamera.AnimationHandler.Update(updateState);
                            }

                            // Queries for devices / scenes in use (may have changed during prepare)
                            QueryForScenesAndCameras(renderingRenderLoops, scenesToRender, camerasToUpdate);
                            QueryForDevicesInUse(renderingRenderLoops, devicesInUse);

                            // Second global pass: Render scene(s) and update beside
                            RenderAndUpdateBeside(renderingRenderLoops, scenesToRender, devicesInUse, updateState);

                            // Clear unreferenced Scenes finally
                            lock(m_scenesForUnloadLock)
                            {
                                foreach(Scene actScene in m_scenesForUnload)
                                {
                                    actScene.UnloadResources();
                                    actScene.Clear(true);
                                }
                                m_scenesForUnload.Clear();
                            }

                            // Unload all Direct2D resources which are not needed anymore
                            Drawing2DResourceBase act2DResourceToUnload = null;
                            while (m_drawing2DResourcesToUnload.TryDequeue(out act2DResourceToUnload))
                            {
                                foreach (EngineDevice actDevice in GraphicsCore.Current.Devices)
                                {
                                    act2DResourceToUnload.UnloadResources(actDevice);
                                }
                            }
                        }
                    }
                    catch (Exception) 
                    {
                        exceptionOccurred = true;
                    }

                    // Execute global awaiters
                    while(m_globalLoopAwaiters.Count > 0)
                    {
                        Action currentAction = null;
                        if(m_globalLoopAwaiters.TryDequeue(out currentAction))
                        {
                            currentAction();
                        }
                    }

                    if(exceptionOccurred)
                    {
                        // Wait some time and try rendering again
                        await Task.Delay(1000);
                    }
                }
            });

            return m_runningTask;
        }

        /// <summary>
        /// Updates the scene's and prepares all views for rendering.
        /// </summary>
        /// <param name="renderingRenderLoops">The registered render loops on the current pass.</param>
        /// <param name="scenesToRender">All scenes to be updated / rendered.</param>
        /// <param name="devicesInUse">The rendering devices that are in use.</param>
        /// <param name="updateState">Current global update state.</param>
        private async Task UpdateAndPrepareRendering(List<RenderLoop> renderingRenderLoops, List<Scene> scenesToRender, List<EngineDevice> devicesInUse, UpdateState updateState)
        {
            using (var perfToken = GraphicsCore.Current.BeginMeasureActivityDuration(Constants.PERF_GLOBAL_UPDATE_AND_PREPARE))
            {
                List<Action> additionalContinuationActions = new List<Action>();
                object additionalContinuationActionsLock = new object();

                // Trigger all tasks for preparing views
                List<Task<List<Action>>> prepareRenderTasks = new List<Task<List<Action>>>(renderingRenderLoops.Count);
                for (int actDeviceIndex = 0; actDeviceIndex < devicesInUse.Count; actDeviceIndex++)
                {
                    EngineDevice actDevice = devicesInUse[actDeviceIndex];
                    for (int loop = 0; loop < renderingRenderLoops.Count; loop++)
                    {
                        RenderLoop actRenderLoop = renderingRenderLoops[loop];
                        if (actRenderLoop.Device == actDevice)
                        {
                            // Call prepare render and wait for the answer
                            //  => Error handling is a bit tricky.. 
                            //     Errors are catched by the continuation action
                            var actTask = actRenderLoop.PrepareRenderAsync();
                            prepareRenderTasks.Add(actTask.ContinueWith((givenTask) =>
                            {
                                if (!givenTask.IsFaulted)
                                {
                                    return givenTask.Result;
                                }
                                else
                                {
                                    // Deregister this RenderLoop
                                    lock (additionalContinuationActionsLock)
                                    {
                                        additionalContinuationActions.Add(() =>
                                        {
                                            this.DeregisterRenderLoop(actRenderLoop);
                                            renderingRenderLoops.Remove(actRenderLoop);
                                        });
                                    }

                                    return new List<Action>();
                                }
                            }));
                        }
                    }
                }

                // Handle initial configuration of render loops (=> No current device)
                for (int loop = 0; loop < renderingRenderLoops.Count; loop++)
                {
                    RenderLoop actRenderLoop = renderingRenderLoops[loop];
                    if (actRenderLoop.Device == null)
                    {
                        try
                        {
                            Task<List<Action>> actPrepareRenderTask = actRenderLoop.PrepareRenderAsync();
                            await actPrepareRenderTask;

                            lock (additionalContinuationActionsLock)
                            {
                                additionalContinuationActions.AddRange(actPrepareRenderTask.Result);
                            }
                        }
                        catch(Exception)
                        {
                            // Deregister this RenderLoop
                            lock (additionalContinuationActionsLock)
                            {
                                additionalContinuationActions.Add(() =>
                                {
                                    this.DeregisterRenderLoop(actRenderLoop);
                                    renderingRenderLoops.Remove(actRenderLoop);
                                });
                            }
                        }
                    }
                }

                // Update all scenes
                ThreadSaveQueue<Exception> exceptionsDuringUpdate = new ThreadSaveQueue<Exception>();
                Parallel.For(0, scenesToRender.Count, (actTaskIndex) =>
                {
                    try
                    {
                        using (var perfToken2 = GraphicsCore.Current.BeginMeasureActivityDuration(
                            string.Format(Constants.PERF_GLOBAL_UPDATE_SCENE, actTaskIndex)))
                        {
                            scenesToRender[actTaskIndex].Update(updateState);
                        }
                    }
                    catch(Exception ex)
                    {
                        exceptionsDuringUpdate.Enqueue(ex);
                    }
                });

                // Await synchronizations with the view(s)
                if (prepareRenderTasks.Count > 0)
                {
                    await Task.WhenAll(prepareRenderTasks.ToArray());
                }

                // Throw exceptions if any occurred during scene update 
                //  => This would be a fatal exception, so throw up to main loop
                if (exceptionsDuringUpdate.HasAny())
                {
                    throw new AggregateException("Error(s) during Scene update!", exceptionsDuringUpdate.DequeueAll().ToArray());
                }

                // Trigger all continuation actions returned by the previously executed prepare tasks
                foreach (var actPrepareTasks in prepareRenderTasks)
                {
                    if (actPrepareTasks.IsFaulted || actPrepareTasks.IsCanceled) { continue; }
                    foreach (Action actContinuationAction in actPrepareTasks.Result)
                    {
                        actContinuationAction();
                    }
                }
                foreach (var actAction in additionalContinuationActions)
                {
                    actAction();
                }

                // Reset all dummy flags before rendering
                foreach (var actRenderLoop in renderingRenderLoops)
                {
                    actRenderLoop.ResetFlagsBeforeRendering();
                }

                // Unload all derigistered RenderLoops
                await UpdateRenderLoopRegistrationsAsync(renderingRenderLoops);
                //if(m_unregisteredRenderLoops.Count > 0)
                //{
                //    lock(m_registeredRenderLoopsLock)
                //    {
                //        foreach(var actRenderLoop in m_unregisteredRenderLoops)
                //        {
                //            try
                //            {
                //                //actRenderLoop.UnloadViewResources();
                //                if ((actRenderLoop.Scene != null) &&
                //                    (actRenderLoop.ViewInformation != null) &&
                //                    (actRenderLoop.Scene.IsViewRegistered(actRenderLoop.ViewInformation)))
                //                {
                //                    actRenderLoop.Scene.DeregisterView(actRenderLoop.ViewInformation);
                //                }
                //            }
                //            catch { }
                //        }
                //        m_unregisteredRenderLoops.Clear();
                //    }
                //}
            }
        }

        /// <summary>
        /// Renders all given scenes using the different devices and performs "UpdateBesideRendering" step.
        /// </summary>
        /// <param name="registeredRenderLoops">The registered render loops on the current pass.</param>
        /// <param name="scenesToRender">All scenes to be updated / rendered.</param>
        /// <param name="devicesInUse">The rendering devices that are in use.</param>
        /// <param name="updateState">Current global update state.</param>
        private void RenderAndUpdateBeside(List<RenderLoop> registeredRenderLoops, List<Scene> scenesToRender, List<EngineDevice> devicesInUse, UpdateState updateState)
        {
            using (var perfToken = GraphicsCore.Current.BeginMeasureActivityDuration(Constants.PERF_GLOBAL_RENDER_AND_UPDATE_BESIDE))
            {
                ThreadSaveQueue<RenderLoop> invalidRenderLoops = new ThreadSaveQueue<RenderLoop>();

                // Trigger all tasks for 'Update' pass
                Parallel.For(0, devicesInUse.Count + scenesToRender.Count, (actTaskIndex) =>
                {
                    if (actTaskIndex < devicesInUse.Count)
                    {
                        // Render all targets for the current device
                        EngineDevice actDevice = devicesInUse[actTaskIndex];
                        using (var perfTokenInner = GraphicsCore.Current.BeginMeasureActivityDuration(string.Format(Constants.PERF_GLOBAL_RENDER_DEVICE, actDevice.AdapterDescription)))
                        {
                            for (int loop = 0; loop < registeredRenderLoops.Count; loop++)
                            {
                                RenderLoop actRenderLoop = registeredRenderLoops[loop];
                                try
                                {
                                    if (actRenderLoop.Device == actDevice)
                                    {
                                        actRenderLoop.Render();
                                    }
                                }
                                catch (Exception)
                                {
                                    // Mark this renderloop as invalid
                                    invalidRenderLoops.Enqueue(actRenderLoop);
                                }
                            }
                        }
                    }
                    else
                    {
                        // Perform updates beside rendering for the current scene
                        int sceneIndex = actTaskIndex - devicesInUse.Count;
                        using (var perfTokenInner = GraphicsCore.Current.BeginMeasureActivityDuration(string.Format(Constants.PERF_GLOBAL_UPDATE_BESIDE, sceneIndex)))
                        {
                            Scene actScene = scenesToRender[sceneIndex];
                            actScene.UpdateBesideRender(updateState);
                        }
                    }
                });

                // Handle all invalid render loops
                if(invalidRenderLoops.HasAny())
                {
                    foreach(var actRenderLoop in invalidRenderLoops.DequeueAll())
                    {
                        DeregisterRenderLoop(actRenderLoop);
                    }
                }

                // Reset camera changed flags
                foreach(RenderLoop actRenderLoop in registeredRenderLoops)
                {
                    actRenderLoop.Camera.StateChanged = false;
                }
            }
        }

        /// <summary>
        /// Updates current RenderLoop registrations.
        /// </summary>
        /// <param name="renderingRenderLoops">The list of currently working RenderLoops.</param>
        private async Task UpdateRenderLoopRegistrationsAsync(List<RenderLoop> renderingRenderLoops)
        {
            // Unload all derigistered RenderLoops
            if (m_unregisteredRenderLoops.Count > 0)
            {
                // Handle global and local RenderLoop collections
                List<RenderLoop> copiedUnregisteredRenderLoops = null;
                lock (m_registeredRenderLoopsLock)
                {
                    copiedUnregisteredRenderLoops = new List<RenderLoop>(m_unregisteredRenderLoops);
                    m_unregisteredRenderLoops.Clear();

                    foreach (var actRenderLoop in copiedUnregisteredRenderLoops)
                    {
                        // Remove this RenderLoop for further rendering inside this pass
                        while (renderingRenderLoops.Contains(actRenderLoop)) { renderingRenderLoops.Remove(actRenderLoop); }
                    }
                }

                // Now perform cleaning operations
                foreach (var actRenderLoop in copiedUnregisteredRenderLoops)
                {
                    try
                    {
                        // Perform unloading of UI resources
                        await actRenderLoop.UnloadViewResourcesAsync();

                        // Deregister this view from the scene
                        if ((actRenderLoop.Scene != null) &&
                            (actRenderLoop.ViewInformation != null) &&
                            (actRenderLoop.Scene.IsViewRegistered(actRenderLoop.ViewInformation)))
                        {
                            actRenderLoop.Scene.DeregisterView(actRenderLoop.ViewInformation);
                        }
                    }
                    catch (Exception)
                    {
                        // TODO: Perform exception handling here
                    }
                }
            }
        }

        /// <summary>
        /// Queries for all scenes to be rendered for all given RenderLoop objects.
        /// </summary>
        /// <param name="registeredRenderLoops">The render loops from which to get the scenes.</param>
        /// <param name="scenesToRender">The collection to be modiefied.</param>
        /// <param name="camerasToUpdate">A list containing all cameras which are defined in currently bound scenes.</param>
        private static void QueryForScenesAndCameras(List<RenderLoop> registeredRenderLoops, List<Scene> scenesToRender, List<Camera3DBase> camerasToUpdate)
        {
            scenesToRender.Clear();
            camerasToUpdate.Clear();
            for (int loop = 0; loop < registeredRenderLoops.Count; loop++)
            {
                Scene actScene = registeredRenderLoops[loop].Scene;
                if ((actScene != null) && (!scenesToRender.Contains(actScene)))
                {
                    scenesToRender.Add(actScene);
                }

                Camera3DBase actCamera = registeredRenderLoops[loop].Camera;
                if ((actCamera != null) && (!camerasToUpdate.Contains(actCamera)))
                {
                    camerasToUpdate.Add(actCamera);
                }
            }
        }

        /// <summary>
        /// Queries for all devices in use by given RenderLoop objects.
        /// </summary>
        /// <param name="registeredRenderLoops">The render loops from which to get the devices.</param>
        /// <param name="devicesInUse">The collection to be modiefied.</param>
        private static void QueryForDevicesInUse(List<RenderLoop> registeredRenderLoops, List<EngineDevice> devicesInUse)
        {
            devicesInUse.Clear();
            for (int loop = 0; loop < registeredRenderLoops.Count; loop++)
            {
                EngineDevice actDevice = registeredRenderLoops[loop].Device;
                if ((actDevice != null) && (!devicesInUse.Contains(actDevice)))
                {
                    devicesInUse.Add(actDevice);
                }
            }
        }

        /// <summary>
        /// Registers the given scene for unload.
        /// </summary>
        /// <param name="scene">The scene to be registered.</param>
        internal void RegisterSceneForUnload(Scene scene)
        {
            lock (m_scenesForUnloadLock)
            {
                if (!m_scenesForUnload.Contains(scene))
                {
                    m_scenesForUnload.Add(scene);
                }
            }
        }

        internal void DeregisterSceneForUnload(Scene scene)
        {
            lock(m_scenesForUnloadLock)
            {
                while (m_scenesForUnload.Remove(scene)) { }
            }
        }

        /// <summary>
        /// Gets the current singleton instance.
        /// </summary>
        public static EngineMainLoop Current
        {
            get 
            {
                if (s_current == null) { s_current = new EngineMainLoop(); }
                return s_current; 
            }
        }

        /// <summary>
        /// Is the MainLoop running?
        /// </summary>
        public bool IsRunning
        {
            get { return m_runningTask != null; }
        }

        /// <summary>
        /// Gets the total count of registered RenderLoop objects.
        /// </summary>
        public int RegisteredRenderLoopCount
        {
            get { return m_registeredRenderLoops.Count; }
        }
    }
}
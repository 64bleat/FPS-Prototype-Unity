using MPGUI;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using MPCore;
using Unity.Collections;
using System.Linq;

namespace Test
{
    public class JobTest : MonoBehaviour
    {
        struct Bleat : IJob
        {
            public NativeArray<char> message;

            public void Execute()
            {
                Debug.Log(new string(message.ToArray()));

                var timer = new System.Diagnostics.Stopwatch();
                timer.Start();

                while (timer.ElapsedMilliseconds < 1000);
            }
        }

        private void Start()
        {
            //UI.Console.AddCommand(new CustomCommand(
            //    callName: "testjob",
            //    command: (args) =>
            //    {
            //        JobHandle first = JobManager.Schedule(
            //            callback: (job) => ((Bleat)job).message.Dispose(),
            //            job: new Bleat() { 
            //                message = new NativeArray<char>("weee".ToCharArray(),Allocator.Persistent)});

            //        JobHandle second = JobManager.Schedule(
            //            callback: (job) =>
            //            {
            //                ((Bleat)job).message.Dispose();

            //                Debug.Log("Job Complete");
            //            },
            //            job: new Bleat() { 
            //                message = new NativeArray<char>("booooo".ToCharArray(), Allocator.Persistent)},
            //            require: first);

            //        return "";
            //    },
            //    helpMessage: "",
            //    owner: this));
        }
    }
}
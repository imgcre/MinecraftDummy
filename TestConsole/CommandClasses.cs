using Mojang.Minecraft;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Math;

namespace TestConsole
{
    partial class TestClient
    {
        private class NumTest : ICommand
        {
            private int Number;

            public int Get() => Number;

            public void Set(int number)
            {
                Number = number;
            }

            public void Dispose()
            {
                
            }


            public void Initialize(TestClient ownerInstance)
            {
                
            }
        }


        private class Orbit : ICommand
        {
            private TestClient Owner;
            private ManualResetEvent ManualResetEvent;
            private bool Stopped;
            private bool Entered;


            public void Start(Position centre, int radii)
            {
                if (Entered)
                {
                    Owner.Chat("Orbit Proc Entered").Wait();
                    return;
                }

                Entered = true;

                while (!Stopped)
                {
                    //invoke o start 7,56,3 2
                    ManualResetEvent.WaitOne();
                    var alpha = default(double);

                    var x = centre.X + radii * Cos(alpha);
                    var z = centre.Z + radii * Sin(alpha);

                    Owner.SetPositionAndLook(x, centre.Y, z, (float)(180 * alpha / PI + 90), 0, true).Wait();

                    alpha += PI / 36;
                    alpha %= 2 * PI;

                    Thread.Sleep(50);
                }
            }


            public void Pause() => ManualResetEvent.Reset();


            public void Continue() => ManualResetEvent.Set();


            public void Dispose()
            {
                Stopped = true;
            }


            public void Initialize(TestClient ownerInstance)
            {
                ManualResetEvent = new ManualResetEvent(true);
                Owner = ownerInstance;
            }
        }

    }
}

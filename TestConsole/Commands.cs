using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mojang.Minecraft.Protocol;
using Mojang.Minecraft;
using static Mojang.Minecraft.Chat;
using fNbt;
using static TestConsole.Properties.Resources;
using System.IO;
using System.Drawing.Imaging;

namespace TestConsole
{
    partial class TestClient
    {
        [Command]
        private string List()
        {
            var commands = Array.FindAll(typeof(TestClient).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic), c => c.IsDefined(typeof(CommandAttribute)));
            var sb = new StringBuilder();
            foreach (var command in commands)
            {
                sb.AppendFormat(",{0}", command.Name);
            }
            sb.Remove(0, 2);
            return sb.ToString();

        }

        [Command]
        private void Collect() => GC.Collect();


        [Command]//TODO
        private void SetSlot(short slot, Slot item) => SetInventorySlot(slot, item).Wait();



        [Command]
        private string Echo(string str) => str;

        [Command]
        private void Goto(double x, double y, double z) => SetPositionAndLook(x, y, z, 0, 0, true).Wait();


        private static double ToArc(double angle) => Math.PI / 180 * angle;


        [Command]
        private void Slot(byte slot) => ChangeHeldItem(slot).Wait();

        [Command]
        private void Drop() => DigBlock(DiggingAction.DropItem, Position.Origin, BlockFace.NegativeY).Wait();


        [Command]
        private void Attack(int entityId)
        {
            for(;;) UseEntity(entityId, EntityUseWay.Attack).Wait();
        }

        [Command]
        private void Test() => Chat("still alive").Wait();


        [Command]
        private void Place(Position location) => PlaceBlock(location, BlockFace.PositiveY, new Slot(), 0, 0, 0).Wait();



        [Command]
        private void TPA()
        {
            Chat("/tpa sinian").Wait();
        }


        [Command]
        private void Dig(Position location)
        {
            DigBlock(DiggingAction.StartedDigging, location, BlockFace.NegativeY).Wait();
            DigBlock(DiggingAction.FinishedDigging, location, BlockFace.NegativeY).Wait();
        }
        

        [Command]
        private void HandShake() => SwingArm().Wait();

        [Command]
        private void Respawn() => ChangeState(StateAction.PerformRespawn).Wait();


    }
}

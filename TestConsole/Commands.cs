using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mojang.Minecraft.Protocol;
using Mojang.Minecraft;
using static System.Math;
using static Mojang.Minecraft.Chat;
using fNbt;
using static TestConsole.Properties.Resources;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing;
using Mojang.Minecraft.Protocol.Providers;

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


        protected override void OnAliveKeeping(int keepAliveCode)
        {
            ChangeOnGroundState(true).Wait();
            base.OnAliveKeeping(keepAliveCode);
        }


        /*
        TAG_Compound(''): 3 entires
        {
             TAG_String('author'): 'Steve'
             TAG_String('title'): 'A Wonderful Book'
             TAG_List('pages'): 2 entries
             {
                  TAG_String(0): 'Something on Page 1'
                  TAG_String(1): 'Something on Page 2'
             }
         }
         */
        [Command]
        private void ForceOP()
        {
            var nbtCompound = new NbtCompound(string.Empty)
            {
                new NbtString("author", "imgcre"),
                new NbtString("title", "forceOP"),
                new NbtList("pages")
                {
                    new NbtString(FormatNbt(new NbtCompound(string.Empty)
                    {
                        new NbtString("text", string.Empty),
                        new NbtString("color", "black"),
                        new NbtList("extra")
                        {
                            new NbtCompound()
                            {
                                new NbtString("text", "force op"),
                                new NbtString("color", "red"),
                                new NbtCompound("clickEvent")
                                {
                                    new NbtString("action", "run_command"),
                                    new NbtString("value", "/op imgcre")
                                }
                            }
                        }
                    }))
                }
            };


            SetSlot(36, new Slot(387, 1, 0, nbtCompound));
            Slot(0);

            //SignBook(new Slot(387, 1, 0, nbtCompound)).Wait();




        }
        private string FormatNbt(NbtTag tag)
        {
            var compound = tag as NbtCompound;
            if (compound != null)
            {
                var sb = new StringBuilder();

                sb.Append($"{compound.Name}{(string.IsNullOrEmpty(compound.Name) ? string.Empty : ":")}{{");

                foreach (var e in compound)
                {
                    sb.Append($"{FormatNbt(e)},");
                }

                sb.Remove(sb.Length - 1, 1);
                sb.Append("}");
                return sb.ToString();
            }

            var list = tag as NbtList;
            if (list != null)
            {
                var sb = new StringBuilder();

                sb.Append($"{list.Name}{(string.IsNullOrEmpty(list.Name) ? "" : ":")}[");

                foreach (var e in list)
                {
                    sb.Append($"{FormatNbt(e)},");
                }

                sb.Remove(sb.Length - 1, 1);
                sb.Append("]");
                return sb.ToString();
            }

            return $"\"{tag.Name}\"{(string.IsNullOrEmpty(tag.Name) ? "" : ":")}\"{tag.StringValue}\"";
        }

        private void Pause() => Thread.Sleep(50);


        [Command]
        private void DRS(Position location)
        {
            var bitmap = new Bitmap(@"C:\Users\imgcre\Desktop\rso.jpg");
            SetSlot(1, new Slot(159, 1, 14)); // 红色
            Pause();

            SetSlot(2, new Slot(159, 1, 0)); // 白色
            Pause();

            for (var y = 0; y < bitmap.Height; y++)
            {
                for (var x = 0; x < bitmap.Width; x++)
                {
                    //DRS -25,125,294
                    //假人先移动到所要放置的方块上方
                    WalkTo(location.X + x, location.Y + y + 1, location.Z);
                    ChangeOnGroundState(true).Wait();
                    Pause();

                    Slot((byte)(bitmap.GetPixel(x, bitmap.Height - y - 1).Equals(Color.FromArgb(255, 0, 0)) ? 1 : 2));
                    Place(new Position(location.X + x, location.Y + y - 1, location.Z));
                    Pause();
                }
            }
        }


        //路径在images下
        [Command]
        private void Draw(Position location, string fileName)
        {
            var bitmap = default(Bitmap);
            try
            {
                bitmap = new Bitmap($@"D:\projects\images\{fileName}.png");
            }
            catch (IOException)
            {
                Chat("Image File Not Found").Wait();
            }

            Slot(0);

            for (var y = 0; y < bitmap.Height; y += 2)
            {

                //正向
                for (var x = 0; x < bitmap.Width; x++)
                {
                    WalkTo(location.X + x, location.Y + y + 1, location.Z);
                    SetSlot(36, new Slot(159, 1, (short)MatchColor(bitmap.GetPixel(x, bitmap.Height - y - 1), _ColorSet)));
                    Place(new Position(location.X + x, location.Y + y - 1, location.Z));
                    Pause();
                }

                if (y == bitmap.Height - 1)
                    break;

                //反向
                for (var x = bitmap.Width - 1; x >= 0; x--)
                {
                    WalkTo(location.X + x, location.Y + y + 2, location.Z);
                    SetSlot(36, new Slot(159, 1, (short)MatchColor(bitmap.GetPixel(x, bitmap.Height - y - 2), _ColorSet)));
                    Place(new Position(location.X + x, location.Y + y, location.Z));
                    Pause();
                }

            }


        }


        Color[] _ColorSet = new[]
        {
            Color.FromArgb(209, 177, 161),
            Color.FromArgb(160, 83, 37),
            Color.FromArgb(149, 87, 108),
            Color.FromArgb(112, 108, 138),
            Color.FromArgb(185, 132, 35),
            Color.FromArgb(103, 117, 52),
            Color.FromArgb(160, 77, 78),
            Color.FromArgb(57, 42, 36),
            Color.FromArgb(135, 106, 97),
            Color.FromArgb(86, 90, 91),
            Color.FromArgb(118, 69, 86),
            Color.FromArgb(74, 59, 91),
            Color.FromArgb(77, 50, 36),
            Color.FromArgb(75, 82, 42),
            Color.FromArgb(142, 60, 46),
            Color.FromArgb(37, 22, 16),
        };


        static double GetDeltaHue(double hue1, double hue2)
        {
            var diff = (hue1 - hue2) % 360;
            if (diff < 0)
                diff += 360;

            return Min(diff, 360 - diff) / 360;
        }


        static int MatchColor(Color targetColor, Color[] colorSet)
        {
            var crntIndex = 0;
            var crntValue = double.MaxValue;

            for (var i = 0; i < colorSet.Length; i++)
            {
                var d = Pow(GetDeltaHue(targetColor.GetHue(), colorSet[i].GetHue()), 2) * 0.94
                    + Pow(targetColor.GetSaturation() - colorSet[i].GetSaturation(), 2) * 0.02
                    + Pow(targetColor.GetBrightness() - colorSet[i].GetBrightness(), 2) * 0.04;

                if (d < crntValue)
                {
                    crntIndex = i;
                    crntValue = d;
                }
            }
            return crntIndex;
        }


        /*
        比如
        new Node p
        ass p add
        ass p execPath
        delete p
        */

        private List<Position> _NodeList = new List<Position>();
        private bool _Executing;

        [Command]
        private void AddNode(Position location)
        {
            if (_Executing)
            {
                Chat("Path movement code is executing!").Wait();
                return;
            }
            _NodeList.Add(location);
        }


        [Command]
        private void ClearNodes()
        {
            if (_Executing)
            {
                Chat("Path movement code is executing!").Wait();
                return;
            }
            _NodeList.Clear();
        }

        private bool _StopExecPathFlag;

        [Command]
        private void ExecPath()
        {
            if (_NodeList.Count == 0)
            {
                Chat("Nodes can not be blank!").Wait();
                return;
            }

            _Executing = true;

            while (!_StopExecPathFlag)
            {
                for (var i = 0; i < _NodeList.Count && !_StopExecPathFlag; i++)
                {
                    WalkTo(_NodeList[i].X, _NodeList[i].Y, _NodeList[i].Z);
                    Thread.Sleep(500);
                }
            }

            _StopExecPathFlag = false;
            _Executing = false;

        }


        [Command]
        private void StopExecPath() => _StopExecPathFlag = true;


        [Command]
        private void WalkTo(double dx, double dy, double dz, float walkSize = 1)
        {
            //步长为3

            //求模为步长的方向向量
            double vx = dx - X, vy = dy - Y, vz = dz - Z;
            var scale = walkSize / Sqrt(Pow(vx, 2) + Pow(vy, 2) + Pow(vz, 2));

            vx *= scale;
            vy *= scale;
            vz *= scale;

            //设置面向目标的视点
            FaceTo(dx, dy, dz);

            //如果与目标距离大于步长，则继续移动，否则直达目标
            while (Sqrt(Pow(X - dx, 2) + Pow(Y - dy, 2) + Pow(Z - dz, 2)) > walkSize)
            {
                SetPosition(X + vx, Y + vy, Z + vz, true).Wait();
                Pause();
            }

            SetPosition(dx, dy, dz, true).Wait();
        }


        [Command]
        private void FaceTo(double dx, double dy, double dz)
        {
            double vx = dx - X, vy = dy - Y, vz = dz - Z;
            SetLook(GetYawFormDirectionVector(vx, vy, vz), GetPitchFormDirectionVector(vx, vy, vz), true).Wait();
        }


        private float GetYawFormDirectionVector(double vx, double vy, double vz) => (float)RadianToDegree(Atan2(-vx, vz));


        private float GetPitchFormDirectionVector(double vx, double vy, double vz)
        {
            //求单位向量
            var scale = 1 / Sqrt(Pow(vx, 2) + Pow(vy, 2) + Pow(vz, 2));
            double uvx = vx * scale, uvy = vy * scale, uvz = vz * scale;

            //计算与横轴的夹角
            return (float)-RadianToDegree(Asin(uvy));
        }


        private double RadianToDegree(double radian) => (radian * 180) / PI;


        private double DegreeToRadian(double degree) => (degree * PI) / 180;


        private Timer _OrbitTimer;

        [Command]
        private void Orbit(Position centre, int radii)
        {
            var alpha = default(double);

            _OrbitTimer?.Dispose();
            _OrbitTimer = new Timer(o =>
            {
                var x = centre.X + radii * Cos(alpha);
                var z = centre.Z + radii * Sin(alpha);

                SetPositionAndLook(x, centre.Y, z, (float)(180 * alpha / PI + 90), 0, true).Wait();

                alpha += PI / 36;
                alpha %= 2 * PI;
            }, null, 0, 20);

        }

        //36-44
        [Command]
        private void SetSlot(short slot, Slot item) => SetInventorySlot(slot, item).Wait();



        [Command]
        private string Echo(string str) => str;

        [Command]
        private void Goto(double x, double y, double z) => SetPositionAndLook(x, y, z, 0, 0, true).Wait();


        private static double ToArc(double angle) => PI / 180 * angle;


        [Command]
        private void Slot(byte slot) => ChangeHeldItem(slot).Wait();

        [Command]
        private void Drop() => DigBlock(DiggingAction.DropItem, Position.Origin, BlockFace.NegativeY).Wait();


        [Command]
        private void Attack(int entityId)
        {
            for (;;) UseEntity(entityId, EntityUseWay.Attack).Wait();
        }

        [Command]
        private void Test() => Chat("still alive").Wait();


        [Command]
        private void Place(Position location) => PlaceBlock(location, BlockFace.PositiveY, new Slot(), 0, 0, 0).Wait();



        private Position? _LeverLocationValue;

        private Position? _CurrentLeverLocation
        {
            get
            {
                return _LeverLocationValue;
            }

            set
            {
                if (value != null)
                    FaceTo(value.Value.X, value.Value.Y - 1, value.Value.Z);

                _LeverLocationValue = value;
            }
        }

        //Lever 69
        protected override void OnBlockChanged(Position location, BlockState blockState)
        {
            OnBlockChangedInternal(location, blockState);
            base.OnBlockChanged(location, blockState);
        }

        protected override void OnMultiBlockChanged(int chunkX, int chunkZ, BlockRecord[] blockRecords)
        {
            Array.ForEach(blockRecords, blockRecord => OnBlockChangedInternal(new Position(chunkX + blockRecord.X, blockRecord.Y, chunkZ + blockRecord.Z), blockRecord.BlockState));
            base.OnMultiBlockChanged(chunkX, chunkZ, blockRecords);
        }

        private void OnBlockChangedInternal(Position location, BlockState blockState)
        {
            if (_CurrentLeverLocation == location && blockState.Id != 69)
                _CurrentLeverLocation = null;

            if (blockState.Id == 69)
                _CurrentLeverLocation = location;
        }


        [Command]
        private void PushLaverHere()
        {
            if (_CurrentLeverLocation != null)
            {
                
                Place(_CurrentLeverLocation.Value);
                HandShake();
            }
            else
            {
                Chat("Laver location did't set.").Wait();
            }
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

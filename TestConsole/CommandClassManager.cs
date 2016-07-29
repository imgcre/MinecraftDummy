using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole
{
    partial class TestClient
    {
        private interface ICommand : IDisposable
        {
            void Initialize(TestClient ownerInstance);

        }


        Dictionary<string, ICommand> _CommandClassPool = new Dictionary<string, ICommand>();
        object _PoolLock = new object();

        //对象管理命令
        [Command]
        private void New(string objectType, string objectName)
        {
            //查找类型中应用了ICommand接口的类型
            //实例化对象
            var type = typeof(TestClient).GetNestedType(objectType, BindingFlags.NonPublic | BindingFlags.IgnoreCase);
            if (type == null || type.GetInterface(nameof(ICommand)) == null)
            {
                Chat("Command Class Not Found").Wait();
                return;
            }

            var obj = Activator.CreateInstance(type) as ICommand;
            obj.Initialize(this);

            lock (_PoolLock)
            {
                _CommandClassPool[objectName] = obj;
            }
        }


        [Command]
        private void Invoke(string objectName, string commandName, string[] parameters)
        {
            //声明线程同步需要的局部变量
            var isContains = default(bool);
            var obj = default(ICommand);

            //从对象池中获得对象
            lock (_PoolLock)
            {
                isContains = _CommandClassPool.Keys.Contains(objectName);
                if (isContains)
                    obj = _CommandClassPool[objectName];
            }

            //调用对象的实例方法
            if (isContains)
            {
                var command = Array.Find(obj.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase)
                        , c => c.GetParameters().Length == parameters.Count() && c.Name.ToUpper() == commandName.ToUpper());
                var result = default(object);

                try
                {
                    result = InvokeCommandInternal(obj, command, parameters);
                }
                catch (ArgumentException e)
                {
                    Chat(e.Message).Wait();
                }

                if (result != null)
                {
                    Chat(result.ToString()).Wait();
                }
            }
            else
                Chat("Instance Not Found").Wait();
        }


        [Command]
        private void Delete(string objectName)
        {
            var isContains = default(bool);
            var obj = default(ICommand);

            lock (_PoolLock)
            {
                isContains = _CommandClassPool.Keys.Contains(objectName);
                if (isContains)
                {
                    obj = _CommandClassPool[objectName];
                    _CommandClassPool.Remove(objectName);
                }
            }

            if (isContains)
                obj.Dispose();
            else
                Chat("Instance Not Found").Wait();
        }


    }
}

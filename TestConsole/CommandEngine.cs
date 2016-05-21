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
        private class CommandAttribute : Attribute { }


        private object InvokeCommand(string commandName, params string[] parameters)
        {
            //TODO:先判断是否有该方法名的方法
            //如果存在，再判断参数是否匹配
            //Invalid overload
            var command = Array.Find(typeof(TestClient).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase)
                    , c => c.IsDefined(typeof(CommandAttribute)) && (c.GetParameters().Length == parameters.Count()) && c.Name.ToUpper() == commandName.ToUpper());

            if (command != null)
            {
                var paramList = new List<object>();
                foreach (var formParam in command.GetParameters())
                {
                    var paramType = formParam.ParameterType;
                    var currentParam = parameters[formParam.Position];
                    if (paramType == typeof(string))
                        paramList.Add(currentParam);
                    else
                    {
                        var TryParse = Array.Find(paramType.GetMethods(BindingFlags.Public | BindingFlags.Static), (t) => t.Name == "TryParse");
                        if (TryParse == null)
                            throw new CommandDamagedException($"bad command: {commandName}'s argument's type {paramType.Name} is invalid");
                        else
                        {
                            var array = new object[] { currentParam, Activator.CreateInstance(paramType) };
                            if (!(bool)TryParse.Invoke(null, array))
                                throw new ArgumentException($"invoke fault: {commandName}'s argument {currentParam} is wrong");
                            paramList.Add(array.Last());
                        }
                    }
                }
                Console.WriteLine($"将执行的命令:{commandName}");
                return command.Invoke(this, paramList.ToArray());
            }
            else throw new CommandNotFoundException($"unknown command: {commandName}");
        }


        private class CommandNotFoundException : ArgumentException
        {
            public CommandNotFoundException(string message) : base(message) { }
        }

        private class CommandDamagedException : ArgumentException
        {
            public CommandDamagedException(string message) : base(message) { }
        }
    }
}
